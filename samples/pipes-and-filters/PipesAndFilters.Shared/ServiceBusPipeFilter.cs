// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
namespace PipesAndFilters.Shared
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    public class ServiceBusPipeFilter
    {
        private readonly string connectionString;
        private readonly string inQueuePath;
        private readonly string outQueuePath;

        // Create a reset event to pause processing before shutting down and create the event signaled to allow processing
        private readonly ManualResetEvent pauseProcessingEvent = new ManualResetEvent(true);

        private QueueClient inQueue;
        private QueueClient outQueue;

        public ServiceBusPipeFilter(string connectionString, string inQueuePath, string outQueuePath = null)
        {
            this.connectionString = connectionString;
            this.inQueuePath = inQueuePath;
            this.outQueuePath = outQueuePath;
        }

        private void OptionsOnExceptionReceived(object sender, ExceptionReceivedEventArgs exceptionReceivedEventArgs) =>
            Trace.TraceError($"Exception in QueueClient.ExceptionReceived: {exceptionReceivedEventArgs?.Exception?.Message}");

        public async Task StartAsync()
        {
            // Create the inbound filter queue if it does not exist
            await ServiceBusUtilities.CreateQueueIfNotExistsAsync(this.connectionString, this.inQueuePath)
                .ConfigureAwait(false);
            this.inQueue = QueueClient.CreateFromConnectionString(this.connectionString, this.inQueuePath);

            // Create the outbound filter queue if it does not exist
            if (!string.IsNullOrWhiteSpace(outQueuePath))
            {
                await ServiceBusUtilities.CreateQueueIfNotExistsAsync(this.connectionString, this.outQueuePath)
                    .ConfigureAwait(false);

                this.outQueue = QueueClient.CreateFromConnectionString(this.connectionString, this.outQueuePath);
            }
        }

        public void OnPipeFilterMessageAsync(Func<BrokeredMessage, Task<BrokeredMessage>> asyncFilterTask,
            CancellationToken cancellationToken, int maxConcurrentCalls = 1)
        {
            var options = new OnMessageOptions()
            {
                AutoComplete = true,
                MaxConcurrentCalls = maxConcurrentCalls
            };

            options.ExceptionReceived += this.OptionsOnExceptionReceived;

            this.inQueue.OnMessageAsync(
                async (msg) =>
            {
                if (!cancellationToken.IsCancellationRequested)
                {

                    // Perform a simple check to dead letter potential poison messages.
                    //  If we have dequeued the message more than the max count we can assume the message is poison and deadletter it.
                    if (msg.DeliveryCount > Constants.MaxServiceBusDeliveryCount)
                    {
                        await msg.DeadLetterAsync()
                            .ConfigureAwait(false);

                        Trace.TraceWarning($"Maximum Message Count Exceeded: {Constants.MaxServiceBusDeliveryCount} for MessageID: {msg.MessageId}");

                        return;
                    }

                    // Process the filter and send the output to the next queue in the pipeline
                    var outMessage = await asyncFilterTask(msg)
                        .ConfigureAwait(false);

                    // Send the message from the filter processor to the next queue in the pipeline
                    if (outQueue != null)
                    {
                        await outQueue.SendAsync(outMessage)
                            .ConfigureAwait(false);
                    }

                    //// Note: There is a chance we could send the same message twice or that a message may be processed by an upstream or downstream filter at the same time.
                    ////       This would happen in a situation where we completed processing of a message, sent it to the next pipe/queue, and then failed to Complete it when using PeakLock
                    ////       Idempotent message processing and concurrency should be considered in the implementation.
                }
            },
            options);
        }

        public async Task CloseAsync()
        {
            await this.inQueue.CloseAsync()
                .ConfigureAwait(false);

            // Cleanup resources.
            await ServiceBusUtilities.DeleteQueueIfExistsAsync(Settings.ServiceBusConnectionString, this.inQueue.Path)
                .ConfigureAwait(false);
        }
    }
}

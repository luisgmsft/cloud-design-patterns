// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
namespace PipesAndFilters.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;

    public class QueueManager
    {
        private readonly string queueName;
        private readonly string connectionString;
        private QueueClient client;

        public QueueManager(string queueName, string connectionString)
        {
            this.queueName = queueName;
            this.connectionString = connectionString;
        }

        public async Task SendMessageAsync(BrokeredMessage message) => await this.client.SendAsync(message).ConfigureAwait(false);

        public async Task StartAsync()
        {
            await ServiceBusUtilities.CreateQueueIfNotExistsAsync(connectionString,
                new QueueDescription(this.queueName)
                {
                    MaxDeliveryCount = 3
                })
                .ConfigureAwait(false);

            // Create the queue client. By default, the PeekLock method is used.
            this.client = QueueClient.CreateFromConnectionString(this.connectionString, this.queueName);
        }
        
        public async Task StopAsync()
        {
            await this.client.CloseAsync()
                .ConfigureAwait(false);

            await ServiceBusUtilities.DeleteQueueIfExistsAsync(this.connectionString, this.queueName)
                .ConfigureAwait(false);
        }
    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
namespace PipesAndFilters.Shared
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;

    public static class ServiceBusUtilities
    {
        public static async Task CreateQueueIfNotExistsAsync(string connectionString, string path)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException($"{nameof(connectionString)} cannot be null, empty, or only whitespace");
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException($"{nameof(path)} cannot be null, empty, or only whitespace");
            }

            await CreateQueueIfNotExistsAsync(connectionString, new QueueDescription(path))
                .ConfigureAwait(false);
        }

        public static async Task CreateQueueIfNotExistsAsync(string connectionString, QueueDescription queueDescription)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException($"{nameof(connectionString)} cannot be null, empty, or only whitespace");
            }

            if (queueDescription == null)
            {
                throw new ArgumentNullException(nameof(queueDescription));
            }

            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            if (!await namespaceManager.QueueExistsAsync(queueDescription.Path)
                .ConfigureAwait(false))
            {
                try
                {
                    await namespaceManager.CreateQueueAsync(queueDescription)
                        .ConfigureAwait(false);
                }
                catch (MessagingEntityAlreadyExistsException)
                {
                    Trace.TraceWarning(
                        $"MessagingEntityAlreadyExistsException Creating Queue - Queue likely already exists for path: {queueDescription.Path}");
                }
                catch (MessagingException ex) when (((ex.InnerException as WebException)?.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.Conflict)
                {
                    Trace.TraceWarning(
                        $"MessagingException HttpStatusCode.Conflict - Queue likely already exists or is being created or deleted for path: {queueDescription.Path}");
                }
            }
        }

        public static async Task DeleteQueueIfExistsAsync(string connectionString, string path)
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            if (await namespaceManager.QueueExistsAsync(path)
                .ConfigureAwait(false))
            {
                try
                {
                    await namespaceManager.DeleteQueueAsync(path)
                        .ConfigureAwait(false);
                }
                catch (MessagingEntityNotFoundException)
                {
                    Trace.TraceWarning(
                        $"MessagingEntityNotFoundException Deleting Queue - Queue does not exist at path: {path}");
                }
            }
        }
    }
}

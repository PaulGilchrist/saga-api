using System.Diagnostics;
using System.Transactions;
using API.Models;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace API.Services {
    // Example for using multiple different backend message services through a common interface
    public class MessageServiceAzureServiceBus: IMessageService, IDisposable {
        private readonly ApplicationSettings _applicationSettings;
        private ServiceBusClient _client;

        private bool disposedValue;

        public MessageServiceAzureServiceBus(ApplicationSettings applicationSettings) {
            _applicationSettings = applicationSettings;
            // Create a ServiceBusClient that will authenticate using a connection string
            string connectionString = _applicationSettings.QueueConnectionString;
            // since ServiceBusClient implements IAsyncDisposable we create it with "await using"
            var options = new ServiceBusClientOptions { EnableCrossEntityTransactions = true };
            _client = new ServiceBusClient(connectionString,options);
            // Create the sender and receiver
        }

        public void Send(string queueName, string type, object? jsonSerializableData, Type? dataSerializableType) {
            var sender = _client.CreateSender(queueName);
            // type examples: created, updated, deleted, etc.
            var cloudEvent = new Azure.Messaging.CloudEvent("contacts-api", type, jsonSerializableData, dataSerializableType);
            cloudEvent.Id = new Guid().ToString();
            cloudEvent.Time = DateTime.Now;
            var json = JsonConvert.SerializeObject(cloudEvent, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            // cloudEvent.data is not converting properly to JSON but the original jsonSerializableData does
            var dataJson = JsonConvert.SerializeObject(jsonSerializableData, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            json = json.Replace("\"data\":{}", $"\"data\":{dataJson}");
            // send the message
            sender.SendMessageAsync(new ServiceBusMessage(json)).GetAwaiter();
            var activityTagsCollection = new ActivityTagsCollection();
            activityTagsCollection.Add("message",json);
            Activity.Current?.AddEvent(new ActivityEvent("AzureServiceBus.Message.Sent",default,activityTagsCollection));
            sender.CloseAsync().GetAwaiter();
            sender.DisposeAsync().GetAwaiter();
        }

        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if(!disposedValue) {
                if(disposing) {
                    // TODO: dispose managed state (managed objects)


                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~MessageServiceAzureServiceBus()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

    }
}

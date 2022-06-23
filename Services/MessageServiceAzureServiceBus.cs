using System.Diagnostics;
using System.Transactions;
using API.Models;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace API.Services {
    // Example for using multiple different backend message services through a common interface
    public class MessageServiceAzureServiceBus: MessageService {
        private readonly ApplicationSettings _applicationSettings;
        private ServiceBusClient _client;

        public MessageServiceAzureServiceBus(ApplicationSettings applicationSettings) : base(applicationSettings) {
            _applicationSettings = applicationSettings;
            // Create a ServiceBusClient that will authenticate using a connection string
            string connectionString = _applicationSettings.QueueConnectionString;
            // since ServiceBusClient implements IAsyncDisposable we create it with "await using"
            var options = new ServiceBusClientOptions { EnableCrossEntityTransactions = true };
            _client = new ServiceBusClient(connectionString,options);
            // Create the sender and receiver
        }

        public override void Send(string queueName,string type,object? jsonSerializableData,Type? dataSerializableType, bool delaySend = false) {
            if(delaySend) {
                _eventMessages.Add(new EventMessage(queueName, type, jsonSerializableData, dataSerializableType));
            } else {
                var sender = _client.CreateSender(queueName);
                // type examples: created, updated, deleted, etc.
                var cloudEvent = new Azure.Messaging.CloudEvent("contacts-api",type,jsonSerializableData,dataSerializableType);
                cloudEvent.Id = new Guid().ToString();
                cloudEvent.Time = DateTime.Now;
                var json = JsonConvert.SerializeObject(cloudEvent,new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                // cloudEvent.data is not converting properly to JSON but the original jsonSerializableData does
                var dataJson = JsonConvert.SerializeObject(jsonSerializableData,new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                json = json.Replace("\"data\":{}",$"\"data\":{dataJson}");
                // send the message
                sender.SendMessageAsync(new ServiceBusMessage(json)).GetAwaiter();
                var activityTagsCollection = new ActivityTagsCollection();
                activityTagsCollection.Add("message",json);
                Activity.Current?.AddEvent(new ActivityEvent("AzureServiceBus.Message.Sent",default,activityTagsCollection));
                sender.CloseAsync().GetAwaiter();
                sender.DisposeAsync().GetAwaiter();
            }
        }

    }
}

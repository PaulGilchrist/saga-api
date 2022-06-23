using System.Diagnostics;
using API.Models;
using Azure;
using Azure.Messaging;
using Azure.Messaging.EventGrid;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace API.Services {
    // Example for using multiple different backend message services through a common interface
    public class MessageServiceAzureEventGrid: MessageService {
        private readonly ApplicationSettings _applicationSettings;

        public MessageServiceAzureEventGrid(ApplicationSettings applicationSettings) : base(applicationSettings) {
            _applicationSettings = applicationSettings;
        }

        public override void Send(string queueName,string type,object? jsonSerializableData,Type? dataSerializableType, bool delaySend = false) {
            if(delaySend) {
                _eventMessages.Add(new EventMessage(queueName, type, jsonSerializableData, dataSerializableType));
            } else {
                var client = new EventGridPublisherClient(
                    new Uri(queueName), // Store the full URL name not just the short topic name
                    new AzureKeyCredential(_applicationSettings.QueueConnectionString));
                // type examples: created, updated, deleted, etc.
                var cloudEvent = new Azure.Messaging.CloudEvent("contacts-api",type,jsonSerializableData,dataSerializableType);
                cloudEvent.Id = new Guid().ToString();
                cloudEvent.Time = DateTime.Now;
                var json = JsonConvert.SerializeObject(cloudEvent,new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                // cloudEvent.data is not converting properly to JSON but the original jsonSerializableData does
                var dataJson = JsonConvert.SerializeObject(jsonSerializableData,new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                json = json.Replace("\"data\":{}",$"\"data\":{dataJson}");
                // send the message
                client.SendEventAsync(cloudEvent).GetAwaiter();
                var activityTagsCollection = new ActivityTagsCollection();
                activityTagsCollection.Add("message",json);
                Activity.Current?.AddEvent(new ActivityEvent("AzureEventGrid.Message.Sent",default,activityTagsCollection));
            }
        }

    }
}

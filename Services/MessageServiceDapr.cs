/*
 * Make sure Dapr is running before debugging this class
 * dapr run --app-id contacts-api --app-port 80 --dapr-http-port 3500 --components-path ./dapr-components/local-rabbitmq
*/
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using API.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace API.Services {
    public class MessageServiceDapr: MessageService {

        protected readonly HttpClient _httpClient;

        public MessageServiceDapr(ApplicationSettings applicationSettings, HttpClient httpClient) : base(applicationSettings) {
            _httpClient = httpClient;
        }

        public override void Send(string queueName, string type, object? jsonSerializableData, Type? dataSerializableType, Guid? changeSetId = null) {
           if(changeSetId != null) {
                // Do not send the message until the entire $batch ChangeSet has completed successfully
                _eventMessages.Add(new EventMessage(changeSetId, queueName, type, jsonSerializableData, dataSerializableType));
            } else {
                // queueName examples: contact, email, phone, address, etc.
                var cloudEvent = new Azure.Messaging.CloudEvent("contacts-api", type, jsonSerializableData, dataSerializableType);
                //cloudEvent.Type = examples: // created, updated, deleted, etc.
                cloudEvent.Id = Guid.NewGuid().ToString();
                cloudEvent.Time = DateTime.Now;
                var json = JsonConvert.SerializeObject(cloudEvent, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                // cloudEvent.data is not converting properly to JSON but the original jsonSerializableData does
                var dataJson = JsonConvert.SerializeObject(jsonSerializableData, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                json = json.Replace("\"data\":{}", $"\"data\":{dataJson}");
                var url = "http://localhost:3500/v1.0/publish/contacts-api-pubsub/" + queueName;
                var message = new StringContent(json,Encoding.UTF8,"application/cloudevents+json");
                var result = _httpClient.PostAsync(url,message).GetAwaiter().GetResult();
                var activityTagsCollection = new ActivityTagsCollection();
                activityTagsCollection.Add("message",json);
                Activity.Current?.AddEvent(new ActivityEvent("Dapr.Message.Sent",default,activityTagsCollection));
            }
        }

    }

}

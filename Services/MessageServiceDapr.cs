/*
 * Make sure Dapr is running before debugging this class
 * dapr run --app-id contacts-api --app-port 80 --dapr-http-port 3500 --components-path ./dapr-components/local
*/
using System.Diagnostics;
using System.Text;
using API.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace API.Services {
    public class MessageServiceDapr: IMessageService {
        private readonly ApplicationSettings _applicationSettings;
        private readonly HttpClient _httpClient;
        private bool disposedValue;

        public MessageServiceDapr(ApplicationSettings applicationSettings, HttpClient httpClient) {
            _applicationSettings = applicationSettings;
            _httpClient = httpClient;
        }

        public void Send(string type, string subject, object? jsonSerializableData, Type? dataSerializableType) {
            // type examples: contact, email, phone, address, etc.
            var cloudEvent = new Azure.Messaging.CloudEvent("contacts-api", type, jsonSerializableData, dataSerializableType);
            cloudEvent.Subject = subject; // created, updated, deleted, etc.
            cloudEvent.Id = Guid.NewGuid().ToString();
            cloudEvent.Time = DateTime.Now;
            var json = JsonConvert.SerializeObject(cloudEvent, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            // cloudEvent.data is not converting properly to JSON but the original jsonSerializableData does
            var dataJson = JsonConvert.SerializeObject(jsonSerializableData, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            json = json.Replace("\"data\":{}", $"\"data\":{dataJson}");
            var url = "http://localhost:3500/v1.0/publish/contacts-pubsub/" + _applicationSettings.QueueName;
            var message = new StringContent(json,Encoding.UTF8,"application/cloudevents+json");
            var result = _httpClient.PostAsync(url,message).GetAwaiter().GetResult();
            var activityTagsCollection = new ActivityTagsCollection();
            activityTagsCollection.Add("message",json);
            Activity.Current?.AddEvent(new ActivityEvent("Dapr.Message.Sent",default,activityTagsCollection));
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
        // ~MessageServiceDapr()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

    }


}

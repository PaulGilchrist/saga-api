using System.Diagnostics;
using API.Models;
using Azure;
using Azure.Messaging;
using Azure.Messaging.EventGrid;

namespace API.Services {
    // Example for using multiple different backend message services through a common interface
    public class MessageServiceAzureEventGrid: IMessageService, IDisposable {
        private readonly ApplicationSettings _applicationSettings;
        private EventGridPublisherClient _client;

        private bool disposedValue;

        public MessageServiceAzureEventGrid(ApplicationSettings applicationSettings) {
            _applicationSettings = applicationSettings;
            _client = new EventGridPublisherClient(
                new Uri(applicationSettings.QueueName), // Store the full URL name not just the short topic name
                new AzureKeyCredential(applicationSettings.QueueConnectionString));
        }

        public void Send(string message) {
            // send the message
            var cloudEvent = new CloudEvent("Contacts API", "Data Change", message);
            _client.SendEventAsync(cloudEvent).GetAwaiter();
            var activityTagsCollection = new ActivityTagsCollection();
            activityTagsCollection.Add("message",message);
            Activity.Current?.AddEvent(new ActivityEvent("AzureEventGrid.Message.Sent",default,activityTagsCollection));
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

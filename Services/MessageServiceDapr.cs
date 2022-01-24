using System.Diagnostics;
using System.Text;
using API.Models;

namespace API.Services {
    public class MessageServiceDapr: IMessageService {
        private readonly ApplicationSettings _applicationSettings;
        private bool disposedValue;

        public MessageServiceDapr(ApplicationSettings applicationSettings) {
            _applicationSettings = applicationSettings;
        }

        public void Send(string message) {
            var url = "http://localhost:3500/v1.0/publish/pubsub/" + _applicationSettings.QueueName;
            using var client = new HttpClient();
            var data = new StringContent(message,Encoding.UTF8,"application/json");
            var result = client.PostAsync(url,data).GetAwaiter().GetResult();
            var activityTagsCollection = new ActivityTagsCollection();
            activityTagsCollection.Add("message",message);
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

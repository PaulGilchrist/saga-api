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
    public abstract class MessageService: IMessageService {
        protected readonly ApplicationSettings _applicationSettings;
        protected List<EventMessage> _eventMessages;
        protected bool disposedValue;

        public MessageService(ApplicationSettings applicationSettings) {
            _applicationSettings = applicationSettings;
            _eventMessages = new List<EventMessage>();
        }

        // Send message now or save the message to send later (delaySend)
        public abstract void Send(string queueName, string type, object? jsonSerializableData, Type? dataSerializableType, bool delaySend = false);

        public void ClearDelayed() {
            // Clear all delayed event messages
            _eventMessages = new List<EventMessage>();
        }

        // Send all delayed event messages in FIFO order
        //public abstract void SendDelayed();
        public void SendDelayed() {
            // Send all delayed event messages in FIFO order
            try {
                _eventMessages.ForEach(eventMessage => {
                    Send(eventMessage.QueueName, eventMessage.Type, eventMessage.JsonSerializableData, eventMessage.DataSerializableType);
                });
                ClearDelayed();
            } catch(Exception ex) {
                Activity.Current?.AddTag("exception",ex);
                throw;
            }
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

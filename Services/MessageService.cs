using System.Diagnostics;
using API.Models;

namespace API.Services {
    public abstract class MessageService: IMessageService {
        protected readonly ApplicationSettings _applicationSettings;
        protected List<EventMessage> _eventMessages;
        protected bool disposedValue;

        public MessageService(ApplicationSettings applicationSettings) {
            _applicationSettings = applicationSettings;
            _eventMessages = new List<EventMessage>();
        }

        public void ClearDelayed(Guid? changeSetId) {
            // Remove messages with the matching ChangeSetId
            _eventMessages.RemoveAll(message => message.ChangeSetId == changeSetId);
       }

        // Send message now or save the message to send later (delaySend)
        public abstract void Send(string queueName, string type, object? jsonSerializableData, Type? dataSerializableType, Guid? changeSetId = null);

        public void SendDelayed(Guid? changeSetId) {
            // Send delayed event messages with the matching ChangeSetId (send in FIFO order)
            try {
                _eventMessages.FindAll(message => message.ChangeSetId == changeSetId).ForEach(message => {
                    Send(message.QueueName, message.Type, message.JsonSerializableData, message.DataSerializableType, null);
                });
                ClearDelayed(changeSetId);
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

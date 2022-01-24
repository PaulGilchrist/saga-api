using System.Diagnostics;
using System.Text;
using API.Models;
using RabbitMQ.Client;

namespace API.Services {
    public class MessageServiceRabbitMQ: IMessageService, IDisposable {
        private readonly ApplicationSettings _applicationSettings;
        private ConnectionFactory _factory;
        private IConnection _connection;
        private IModel _channel;
        private bool disposedValue;

        public MessageServiceRabbitMQ(ApplicationSettings applicationSettings) {
            _applicationSettings = applicationSettings;
            _factory = new ConnectionFactory();
            //_factory.UserName = _applicationSettings.QueueUser;
            //_factory.Password = _applicationSettings.QueuePassword;
            //_factory.VirtualHost = _applicationSettings.QueueVHost;
            _factory.HostName = _applicationSettings.QueueConnectionString;
            //_factory.Port = _applicationSettings.QueuePort;
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
            // var args = new Dictionary<string, object>();
            // args.Add("x-message-ttl", 3600000); // 60 minutes
            // model.QueueDeclare("api", false, false, false, args);
            _channel.QueueDeclare(queue: _applicationSettings.QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        public void Send(string message) {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "", routingKey: _applicationSettings.QueueName, basicProperties: null, body: body);
            var activityTagsCollection = new ActivityTagsCollection();
            activityTagsCollection.Add("message",message);
            Activity.Current?.AddEvent(new ActivityEvent("RabbitMQ.Message.Sent",default,activityTagsCollection));
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
                    _channel.Close();
                    _connection.Close();
                }
                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~MessageServiceRabbitMQ() {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

    }


}

using System.Diagnostics;
using System.Text;
using API.Models;
using RabbitMQ.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace API.Services {
    public class MessageServiceRabbitMQ: MessageService {
        private ConnectionFactory _factory;
        private IConnection _connection;
        private IModel _channel;

        public MessageServiceRabbitMQ(ApplicationSettings applicationSettings) : base(applicationSettings) {
            _factory = new ConnectionFactory();
            //_factory.UserName = _applicationSettings.QueueUser;
            //_factory.Password = _applicationSettings.QueuePassword;
            //_factory.VirtualHost = _applicationSettings.QueueVHost;
            _factory.HostName = applicationSettings.QueueConnectionString; // localhost
            //_factory.Port = _applicationSettings.QueuePort;
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
            // var args = new Dictionary<string, object>();
            // args.Add("x-message-ttl", 3600000); // 60 minutes
            // model.QueueDeclare("api", false, false, false, args);
        }

        public override void Send(string queueName,string type,object? jsonSerializableData,Type? dataSerializableType, bool delaySend = false) {
            if(delaySend) {
                _eventMessages.Add(new EventMessage(queueName, type, jsonSerializableData, dataSerializableType));
            } else {
                _channel.QueueDeclare(queue: queueName,durable: false,exclusive: false,autoDelete: false,arguments: null);
                // type examples: created, updated, deleted, etc.
                var cloudEvent = new Azure.Messaging.CloudEvent("contacts-api",type,jsonSerializableData,dataSerializableType);
                cloudEvent.Id = new Guid().ToString();
                cloudEvent.Time = DateTime.Now;
                var json = JsonConvert.SerializeObject(cloudEvent,new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                // cloudEvent.data is not converting properly to JSON but the original jsonSerializableData does
                var dataJson = JsonConvert.SerializeObject(jsonSerializableData,new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                json = json.Replace("\"data\":{}",$"\"data\":{dataJson}");
                var body = Encoding.UTF8.GetBytes(json);
                _channel.BasicPublish(exchange: queueName,routingKey: queueName,basicProperties: null,body: body);
                var activityTagsCollection = new ActivityTagsCollection();
                activityTagsCollection.Add("message",json);
                Activity.Current?.AddEvent(new ActivityEvent("RabbitMQ.Message.Sent",default,activityTagsCollection));
            }
        }

    }

}

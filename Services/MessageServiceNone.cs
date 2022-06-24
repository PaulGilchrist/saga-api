using System.Diagnostics;
using API.Models;

namespace API.Services {
    public class MessageServiceNone: MessageService {

        public MessageServiceNone(ApplicationSettings applicationSettings) : base(applicationSettings) { }

        public override void Send(string queueName,string type,object? jsonSerializableData,Type? dataSerializableType, Guid? changeSetId = null) {
            // Will not send event message to any queue
        }

    }

}

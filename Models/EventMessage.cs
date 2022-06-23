using System;
namespace API.Models {
    public class EventMessage {

        public string QueueName { get; }
        public string Type { get; }
        public object? JsonSerializableData { get; }
        public Type? DataSerializableType { get; }

        public EventMessage(string queueName, string type, object? jsonSerializableData, Type? dataSerializableType) {
            QueueName = queueName;
            Type = type;
            JsonSerializableData = jsonSerializableData;
            DataSerializableType = dataSerializableType;
        }

    }
}


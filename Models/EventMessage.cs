using System;
namespace API.Models {
    public class EventMessage {
        public Guid? ChangeSetId { get; } // OData $batch can request multiple changes to all succeed or fail together.  Null if not part of a ChangeSet
        public string QueueName { get; } // The queue topic (also object name) - examples: contact, email, phone, address, etc.
        public string Type { get; } // The action - examples: // created, updated, deleted, etc.
        public object? JsonSerializableData { get; }
        public Type? DataSerializableType { get; }

        public EventMessage(Guid? changeSetId, string queueName, string type, object? jsonSerializableData, Type? dataSerializableType) {
            ChangeSetId = changeSetId;
            QueueName = queueName;
            Type = type;
            JsonSerializableData = jsonSerializableData;
            DataSerializableType = dataSerializableType;
        }

    }
}


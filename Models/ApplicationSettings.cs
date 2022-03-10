using System;

namespace API.Models {
    public class ApplicationSettings {
        public string BasePath { get; set; }
        public string ContactsCollectionName { get; set; }
        public string DatabaseConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string QueueConnectionString { get; set; }
        public string QueueName { get; set; }
        public string QueueType { get; set; }
        public string TelemetryConnectionString { get; set; }
        public string TelemetryType { get; set; }
        public ApplicationSettings() {
#pragma warning disable CS8601
            BasePath = Environment.GetEnvironmentVariable("BasePath") ?? "/"; // Change if using layer 7 routing like when using an ingress-gateway or reverse-proxy
            ContactsCollectionName = Environment.GetEnvironmentVariable("ContactsCollectionName");
            DatabaseConnectionString = Environment.GetEnvironmentVariable("DatabaseConnectionString");
            DatabaseName = Environment.GetEnvironmentVariable("DatabaseName");
            QueueConnectionString = Environment.GetEnvironmentVariable("QueueConnectionString"); // Same as HostName for QueueType="RabbitMQ"
            QueueName = Environment.GetEnvironmentVariable("QueueName");
            QueueType = Environment.GetEnvironmentVariable("QueueType"); // Valid options are "AzureEventGrid", "AzureServiceBus", "Dapr", "None", or "RabbitMQ" (Default: "None")
                // If QueueType=AzureEventGrid then QueueName will be the topic's full URL, and QueueConnectionString will be the AzureKeyCredential
                // If QueueType=RabbitMQ then QueueConnectionString will be the HostName
           TelemetryConnectionString = Environment.GetEnvironmentVariable("TelemetryConnectionString"); // Blank if TelemetryType="Console"
            TelemetryType = Environment.GetEnvironmentVariable("TelemetryType"); // Valid options are "AppInsights", "Console", or "Zipkin" (Default: "Console")
#pragma warning restore CS8601
        }
    }
}

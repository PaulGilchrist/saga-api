using System;

namespace API.Models {
    public class ApplicationSettings {
        public string AppEnvironment { get; }
        public string BasePath { get; }
        public string ContactsCollectionName { get; }
        public string DatabaseConnectionString { get; }
        public string DatabaseName { get; }
        public string[] OAuthAudience { get; }
        public string[] OAuthAuthority { get; }
        public int ODataMaxPageSize { get; }
        public string QueueConnectionString { get; }
        public string QueueType { get; }
        public string TelemetryConnectionString { get; }
        public string TelemetryType { get; }

        public ApplicationSettings() {
#pragma warning disable CS8601
            AppEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"); // Development, QA, Staging, Production
            BasePath = Environment.GetEnvironmentVariable("BASE_PATH") ?? "/"; // Change if using layer 7 routing like when using an ingress-gateway or reverse-proxy
            ContactsCollectionName = Environment.GetEnvironmentVariable("CONTACTS_COLLECTION_NAME");
            DatabaseConnectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
            DatabaseName = Environment.GetEnvironmentVariable("DATABASE_NAME");
            OAuthAudience = Environment.GetEnvironmentVariable("OAUTH_AUDIENCE")?.Split(","); // Required
            OAuthAuthority = Environment.GetEnvironmentVariable("OAUTH_AUTHORITY")?.Split(",");  // Required
            ODataMaxPageSize = Convert.ToInt32(Environment.GetEnvironmentVariable("ODATA_MAX_PAGE_SIZE") ?? "3000");
            QueueConnectionString = Environment.GetEnvironmentVariable("QUEUE_CONNECTION_STRING"); // Same as HostName for QueueType="RabbitMQ"
            QueueType = Environment.GetEnvironmentVariable("QUEUE_TYPE"); // Valid options are "AzureEventGrid", "AzureServiceBus", "Dapr", "None", or "RabbitMQ" (Default: "None")
                // If QueueType=AzureEventGrid then QueueName will be the topic's full URL, and QueueConnectionString will be the AzureKeyCredential
                // If QueueType=RabbitMQ then QueueConnectionString will be the HostName
            TelemetryConnectionString = Environment.GetEnvironmentVariable("TELEMETRY_CONNECTION_STRING"); // Blank if TelemetryType="Console"
            TelemetryType = Environment.GetEnvironmentVariable("TELEMETRY_TYPE"); // Valid options are "AppInsights", "Console", or "Zipkin" (Default: "Console")
#pragma warning restore CS8601
        }
    }
}

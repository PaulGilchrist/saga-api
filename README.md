# saga-api

## Example of minimal REST API for .Net 6

Main additions to the Visual Studio template include:
* Swagger integration with code comments
* Docker support
* CORS support
* OData support
* OpenTelemetry support with exporters for AppInsights, Console, or Zipkin
* Messaging support for AzureServiceBus, Dapr, or RabbitMQ
  * SAGA pattern used to ensure both event messaging and DB changes both succeed or both fail
    * This is the recommended approach over transactions (prepare/commit)
* Ingress-gateway or reverse-proxy support for Open Api when defining BasePath environment variable
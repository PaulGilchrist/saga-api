using System.Reflection;
using System.Text;
using System.Text.Json;
using API.Classes;
using API.Models;
using API.Services;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using Polly.Extensions.Http;
using Swashbuckle.AspNetCore.SwaggerUI;


static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy() {
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
}

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() {
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

static Task WriteReadinessResponse(HttpContext context, HealthReport result) {
    context.Response.ContentType = "application/json; charset=utf-8";
    var options = new JsonWriterOptions
    {
        Indented = true
    };
    using (var stream = new MemoryStream())
    {
        using (var writer = new Utf8JsonWriter(stream, options))
        {
            writer.WriteStartObject();
            writer.WriteString("status", result.Status.ToString());
            writer.WriteStartObject("results");
            foreach (var entry in result.Entries)
            {
                writer.WriteStartObject(entry.Key);
                writer.WriteString("status", entry.Value.Status.ToString());
                writer.WriteString("description", entry.Value.Description);
                writer.WriteStartObject("data");
                foreach (var item in entry.Value.Data)
                {
                    writer.WritePropertyName(item.Key);
                    JsonSerializer.Serialize(
                        writer, item.Value, item.Value?.GetType() ??
                        typeof(object));
                }
                writer.WriteEndObject();
                writer.WriteEndObject();
            }
            writer.WriteEndObject();
            writer.WriteEndObject();
        }
        var json = Encoding.UTF8.GetString(stream.ToArray());
        return context.Response.WriteAsync(json);
    }
}

var builder = WebApplication.CreateBuilder(args);
// Define some important OpenTelemetry constants and the activity source
var serviceName = "Api.Contacts";
var serviceVersion = "1.0.0";
var applicationSettings = new ApplicationSettings();
// Configure important OpenTelemetry settings and the console exporter
builder.Services.AddOpenTelemetryTracing(b => {
    b
    .AddSource(serviceName)
    .SetResourceBuilder(
        ResourceBuilder.CreateDefault()
            .AddService(serviceName: serviceName,serviceVersion: serviceVersion))
    .AddConsoleExporter() // Will always be added regardless of which TelemetryType is chosen
    // .AddSqlClientInstrumentation()
    .AddHttpClientInstrumentation()
    .AddAspNetCoreInstrumentation();
    switch(applicationSettings.QueueType) {
        case "AppInsights":
            b.AddAzureMonitorTraceExporter(o => {
                o.ConnectionString = applicationSettings.TelemetryConnectionString;
            });
            break;
        case "Zipkin":  // Default telemetry destination for Dapr
            b.AddZipkinExporter(o => {
                o.Endpoint = new Uri(applicationSettings.TelemetryConnectionString); //ex: http://localhost:9411/api/v2/spans
            });
            break;
        default: // Logging to the console (exporter already added)
            break;
    }
});
var healthCheckBuilder = builder.Services.AddHealthChecks();
// Example showing support for multiple messaging platforms
switch(applicationSettings.QueueType) {
    case "AzureServiceBus":
        builder.Services.AddSingleton<IMessageService,MessageServiceAzureServiceBus>();
        healthCheckBuilder.AddAzureServiceBusTopic(applicationSettings.QueueConnectionString, "contacts", null, Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy);
        break;
    case "AzureEventGrid":
        builder.Services.AddSingleton<IMessageService,MessageServiceAzureEventGrid>();
        break;
    case "Dapr":
        builder.Services.AddHttpClient<IMessageService, MessageServiceDapr>()
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))  //Set lifetime to five minutes
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());
        break;
    case "RabbitMQ":
        builder.Services.AddSingleton<IMessageService,MessageServiceRabbitMQ>();
        var uri = new Uri("amqp://guest:guest@" + applicationSettings.QueueConnectionString + ":5672");
        healthCheckBuilder.AddRabbitMQ(uri, null, null, Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy);
        break;
    default: // None
        builder.Services.AddSingleton<IMessageService,MessageServiceNone>();
        break;
}
healthCheckBuilder.AddMongoDb(applicationSettings.DatabaseConnectionString, applicationSettings.DatabaseName, null, Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy);
// Add services to the container.
// CORS support
builder.Services.AddCors(options => {
    options.AddPolicy("AllOrigins",
         builder => {
             builder
         .AllowAnyOrigin()
         .AllowAnyMethod()
         .AllowAnyHeader();
         });
});
builder.Services.AddSingleton<ApplicationSettings>();
builder.Services.AddSingleton<ContactService>();
builder.Services.AddControllers().AddOData(options => options.Count().Expand().Filter().OrderBy().Select().SetMaxTop(1000));


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    if(basePath != null) {
        options.IncludeXmlComments(Path.Combine(basePath,"contacts-api.xml"));
    }
    //Configure Swagger to filter out $expand objects to improve performance for large highly relational APIs
    options.SchemaFilter<SwaggerIgnoreFilter>();
    options.OperationFilter<ODataEnableQueryFiler>();
});
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {}
app.UseODataQueryRequest();
app.UseODataBatching();
app.UseSwagger(options => {
    options.PreSerializeFilters.Add((swaggerDoc,httpReq) => {
        var contact = new OpenApiContact();
        contact.Name = "Paul Gilchrist";
        contact.Email = "paul.gilchrist@outlook.com";
        contact.Url = new Uri("https://github.com/PaulGilchrist");
        swaggerDoc.Info.Contact = contact;
        swaggerDoc.Info.Contact.Email = "paul.gilchrist@outlook.com";
        swaggerDoc.Info.Contact.Url = new Uri("https://github.com/PaulGilchrist");
        swaggerDoc.Info.Description = "All Contact related business objects";
        swaggerDoc.Info.Title = "Contacts API";
        swaggerDoc.Info.Version = "1.0.0";
        swaggerDoc.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"https://{httpReq.Host.Value}{applicationSettings.BasePath}" } };
    });
});
app.UseSwaggerUI(options => {
    options.DocumentTitle = "Contacts API";
    options.DefaultModelExpandDepth(2);
    options.DefaultModelsExpandDepth(-1);
    options.DefaultModelRendering(ModelRendering.Model);
    options.DisplayRequestDuration();
});
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health/readiness", new HealthCheckOptions() { ResponseWriter = WriteReadinessResponse }).AllowAnonymous();
app.MapHealthChecks("/health/liveness", new HealthCheckOptions() { Predicate = (_) => false }).AllowAnonymous();
app.Run();


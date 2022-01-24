using System.Reflection;
using API.Classes;
using API.Models;
using API.Services;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.AspNetCore.OData;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Swashbuckle.AspNetCore.SwaggerUI;

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
// Example showing support for multiple messaging platforms
switch(applicationSettings.QueueType) {
    case "AzureServiceBus":
        builder.Services.AddSingleton<IMessageService,MessageServiceAzureServiceBus>();
        break;
    case "Dapr":
        builder.Services.AddSingleton<IMessageService,MessageServiceDapr>();
        break;
    case "RabbitMQ":
        builder.Services.AddSingleton<IMessageService,MessageServiceRabbitMQ>();
        break;
    default: // None
        builder.Services.AddSingleton<IMessageService,MessageServiceNone>();
        break;
}
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
app.Run();

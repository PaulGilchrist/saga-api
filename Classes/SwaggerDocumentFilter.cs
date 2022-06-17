using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace API.Classes;

public class SwaggerDocumentFilter : IDocumentFilter {
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context) {
        // remove Metadata controller
        foreach (ApiDescription apiDescription in context.ApiDescriptions) {
            var actionDescriptor = (ControllerActionDescriptor)apiDescription.ActionDescriptor;
            if (actionDescriptor.ControllerName == "Metadata") {
                swaggerDoc.Paths.Remove($"/{apiDescription.RelativePath}");
            }
        }

        // remove schemas
        foreach ((string key, _) in swaggerDoc.Components.Schemas) {
            if (key.Contains("Edm") || key.Contains("OData")) {
                swaggerDoc.Components.Schemas.Remove(key);
            }
        }
    }
}

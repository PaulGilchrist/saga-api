using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace API.Classes {
    class ODataEnableQueryFiler: IOperationFilter {
        static List<OpenApiParameter> s_Parameters = (new List<(string Name, OpenApiSchema schema, string Description)>()
                {
                ( "$count", new OpenApiSchema { Type = "boolean" }, "Indicates whether the total count of items within a collection are returned in the result."),
                ( "$expand", new OpenApiSchema { Type = "string" }, "Use to add related query data."),
                ( "$filter", new OpenApiSchema { Type = "string" }, "A function that must evaluate to true for a record to be returned."),
                ( "$orderby", new OpenApiSchema { Type = "string" }, "Determines what values are used to order a collection of records."),
                ( "$select", new OpenApiSchema { Type = "string" }, "Specifies a subset of properties to return. Use a comma separated list."),
                ( "$skip", new OpenApiSchema { Type = "integer", Format = "int32" }, "The number of records to skip."),
                ( "$top", new OpenApiSchema { Type = "integer", Format = "int32" }, "The max number of records.")
            }).Select(pair => new OpenApiParameter {
                Name = pair.Name,
                Required = false,
                Schema = pair.schema,
                In = ParameterLocation.Query,
                Description = pair.Description,

            }).ToList();

        public void Apply(OpenApiOperation operation,OperationFilterContext context) {
            if(context.ApiDescription.ActionDescriptor.EndpointMetadata.Any(em => em is Microsoft.AspNetCore.OData.Query.EnableQueryAttribute)) {
                operation.Parameters ??= new List<OpenApiParameter>();
                foreach(var item in s_Parameters)
                    operation.Parameters.Add(item);
            }
        }
    }
}



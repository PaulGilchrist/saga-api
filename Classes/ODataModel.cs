using API.Models;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;


namespace API.Classes {
    static public class ODataModel {
        static public IEdmModel Get(int maxPageSize) {
            ODataConventionModelBuilder builder = new();
            builder.EnableLowerCamelCase();
            builder.Namespace = "EdhOdataApi";
            builder.ContainerName = "EdhOdataApi";
            builder.EntitySet<Contact>("contacts").EntityType.Count().Expand(3).Filter().OrderBy().Page(null, maxPageSize).Select();
            return builder.GetEdmModel();
        }
    }
}
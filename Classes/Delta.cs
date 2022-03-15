using System.Text.Json;

namespace API.Classes {
    public static class Delta {
        public static T? Patch<T>(dynamic? deltaObject,T fullObject) {
            var newObject = JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(fullObject));
            // Update fullObject with any matching properties found in deltaObject
#pragma warning disable CS8602
            if(deltaObject != null && newObject != null) {
                var fullObjectProperties = typeof(T).GetProperties(); // All Properties
                // Loop through the changed properties updating the object
                foreach(var deltaObjectProperty in deltaObject.Properties()) {
                    var deltaObjectPropertyName = deltaObjectProperty.Name;
                    if(deltaObjectPropertyName != "id") { // Cannot change the id but it will always be passed in
                        // Using for instead of foreach due to performance difference
                        for(int i = 0;i < fullObjectProperties.Length;i++) {
                            var fullObjectPropertyName = fullObjectProperties[i].Name;
                            var nonEditablePropertyInfo = fullObjectProperties[i].GetType().GetProperty("NonEditable");
                            if(String.Equals(deltaObjectPropertyName,fullObjectPropertyName,StringComparison.OrdinalIgnoreCase) && nonEditablePropertyInfo == null) {
                                var property = newObject.GetType().GetProperty(fullObjectPropertyName);
                                if(property != null) {
                                    property.SetValue(newObject,Convert.ChangeType(deltaObjectProperty.Value,fullObjectProperties[i].PropertyType));
                                    break;
                                    // Could optionally even support deltas within deltas here
                                }
                            }
                        }
                    }
                }
            }
#pragma warning restore CS8602
        return newObject;
        }
    }
}


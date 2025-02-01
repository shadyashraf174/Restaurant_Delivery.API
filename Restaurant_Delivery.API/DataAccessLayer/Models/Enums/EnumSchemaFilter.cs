using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Runtime.Serialization;

public class EnumSchemaFilter : ISchemaFilter {
    public void Apply(OpenApiSchema schema, SchemaFilterContext context) {
        if (context.Type.IsEnum) {
            var enumType = context.Type;
            var values = Enum.GetValues(enumType);
            schema.Enum = new List<IOpenApiAny>(); // Change to List<IOpenApiAny>
            foreach (var value in values) {
                var member = enumType.GetMember(value.ToString());
                if (member.Length > 0) {
                    var enumMemberAttribute = member[0].GetCustomAttributes(typeof(EnumMemberAttribute), false).FirstOrDefault() as EnumMemberAttribute;
                    if (enumMemberAttribute != null) {
                        schema.Enum.Add(new OpenApiString(enumMemberAttribute.Value)); // Add OpenApiString
                    } else {
                        schema.Enum.Add(new OpenApiString(value.ToString())); // Add OpenApiString
                    }
                }
            }
        }
    }
}
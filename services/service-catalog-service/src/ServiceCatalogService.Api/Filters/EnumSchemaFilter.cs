using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Any;

namespace ServiceCatalogService.Api.Filters;

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            schema.Enum.Clear();
            var enumNames = Enum.GetNames(context.Type);
            var enumValues = Enum.GetValues(context.Type);

            for (int i = 0; i < enumNames.Length; i++)
            {
                var enumName = enumNames[i];
                var enumValue = Convert.ToInt32(enumValues.GetValue(i));
                schema.Enum.Add(new OpenApiString(enumName));
            }

            schema.Type = "string";
            schema.Format = null;
        }
    }
}
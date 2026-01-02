using System.Runtime.CompilerServices;
using NJsonSchema;
using NJsonSchema.Generation;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace ReData.DemoApp;

public class XEnumVarnamesNswagSchemaProcessor : ISchemaProcessor
{
    public void Process(SchemaProcessorContext context)
    {
        if (context.ContextualType.Type.IsEnum)
        {
            if (context.Schema.ExtensionData is not null)
            {
                context.Schema.ExtensionData.Add("x-enum-varnames", context.Schema.EnumerationNames.ToArray());
            }
            else
            {
                context.Schema.ExtensionData = new Dictionary<string, object?>()
                {
                    ["x-enum-varnames"] = context.Schema.EnumerationNames.ToArray(),
                };
            }
        }
    }
}

public class RequiredPropertiesSchemaProcessor : ISchemaProcessor
{
    public void Process(SchemaProcessorContext context)
    {
        if (context.Schema.Type != JsonObjectType.Object)
        {
            return;
        }

        foreach (var (_, schemaProp) in context.Schema.Properties)
        {
            // NOTE: this type and schema properties match won't work for exotic namings such as kebab-case and snake_case
            var typeProp = context.ContextualType.Properties
                .FirstOrDefault(x => x.Name.Equals(schemaProp.Name, StringComparison.OrdinalIgnoreCase));

            if (typeProp is null)
            {
                continue;
            }

            var requiredAttrs = typeProp.GetCustomAttributes(typeof(RequiredMemberAttribute), true);

            if (requiredAttrs.Length is 0)
            {
                continue;
            }

            schemaProp.IsRequired = true;
        }
    }
}
using ReData.Query.Core.Template;

namespace ReData.Query.Core.Types;

public record struct Field
{
    public required string Alias { get; init; }
    
    public required FieldType Type { get; init; }
}
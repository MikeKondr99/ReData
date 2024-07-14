using System.ComponentModel.DataAnnotations.Schema;

namespace ReData.Database.Entities;

public record Transformation 
{
    public Guid DataSetId { get; init; } // PK
    
    public int Order { get; init; } // PK
    
    [Column(TypeName = "jsonb")]
    public required ITransformation Data { get; init; }
}
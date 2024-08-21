

using System.Text.Json.Serialization;
// ReSharper disable once CheckNamespace
using System.Collections;

namespace ReData.Database.Entities;

public record DataSet : IEntity
{
    public required Guid Id { get; init; }
    
    public required string Name { get; init; }

    public required ICollection<Transformation> Transformations { get; init; }
}


public record Transformation
{
    public required Guid DataSetId { get; init; }
    
    public required uint Order { get; init; }
    
    public required ITransformation Data { get; init; }
}

[JsonDerivedType(typeof(LoadTransformation), typeDiscriminator:"Load")]
[JsonDerivedType(typeof(TestTransformantion), typeDiscriminator:"Test")]
public interface ITransformation;


public sealed record LoadTransformation : ITransformation
{
    public required Guid DataSourceId { get; init; }
    
    public required string Table { get; init; }
    
    public required Field[] Fields { get; init; }
}

public sealed record TestTransformantion : ITransformation
{
    public required string TestText { get; init; }
}

public sealed record Field
{
    public required string Name { get; init; }

    public required FieldType Type { get; init; }
    
    public required bool Optional { get; init; }
}

public enum FieldType
{
    Boolean = 1,
    Integer = 2,
    Real = 3,
    Text = 4,
    TimeStamp = 5
}
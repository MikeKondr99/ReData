using System.Collections;
using ReData.Query.Core.Types;

namespace ReData.DemoApplication.Database.Entities;

public sealed record DataSourceEntity
{
    public required Guid Id { get; init; }

    public required string Name { get; set; }

    public required Guid TableId { get; set; }
    public required IReadOnlyList<Field> FieldList { get; set; } // Json

    public required DateTimeOffset CreatedAt { get; init; }

    public required DateTimeOffset UpdatedAt { get; set; }
}

public struct Field
{
    public required string Alias { get; init; }

    public required DataType DataType { get; init; }

    public required bool CanBeNull { get; init; }
}
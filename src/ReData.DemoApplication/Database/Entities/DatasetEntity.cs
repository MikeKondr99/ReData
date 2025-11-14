namespace ReData.DemoApplication.Database.Entities;

public sealed record DataSetEntity
{
    public required Guid Id { get; init; }

    public required string Name { get; set; }

    public required Guid? TableId { get; init; }

    public required List<Field>? FieldList { get; init; }

    public required List<TransformationEntity> Transformations { get; set; }
}
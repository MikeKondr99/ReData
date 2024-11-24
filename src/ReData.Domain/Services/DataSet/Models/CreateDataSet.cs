namespace ReData.Domain.Services.DataSet.Models;

public sealed record CreateDataSet
{
    public required string Name { get; init; }

    public required FromDataSource DataSource { get; init; }

    public required Entity.ITransformation Transformations { get; init; }
}


public sealed record FromDataSource
{
    public required Guid DataSourceId { get; init; }
    
    public required string Table { get; init; }
    
    public required Entity.Field[] Fields { get; init; }
}
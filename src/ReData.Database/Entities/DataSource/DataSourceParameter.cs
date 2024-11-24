using System.ComponentModel.DataAnnotations.Schema;

namespace ReData.Database.Entities;

public record DataSourceParameter
{
    public required Guid DataSourceId { get; init; }

    public required string Key { get; init; }
    
    public required string Value { get; init; }

}
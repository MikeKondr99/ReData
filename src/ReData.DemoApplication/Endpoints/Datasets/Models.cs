using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using FluentValidation;
using ReData.DemoApplication.Transformations;

namespace ReData.DemoApplication.Endpoints.Datasets;

public record GetByIdRequest
{
    public required Guid Id { get; init; }
}

public sealed record DataSetResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    
    public required Guid DataConnectorId { get; init; }
    public required IReadOnlyList<TransformationBlockResponse> Transformations { get; init; }
}

public sealed record DataSetListItem
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
}

public sealed record TransformationBlockResponse
{
    public required bool Enabled { get; init; }
    public required string? Description { get; init; }
    public required ITransformation Transformation { get; init; }
}

/// <summary>
/// Запрос на создание набора данных
/// </summary>
public sealed record CreateDataSetRequest
{
    public required string Name { get; init; }
    
    public required Guid ConnectorId { get; init; }
    public required IReadOnlyList<TransformationBlock> Transformations { get; init; }
}

public sealed class CreateDataSetRequestValidator : Validator<CreateDataSetRequest> 
{
    public CreateDataSetRequestValidator()
    {
        RuleFor(req => req.Name)
            .NotNull()
            .MinimumLength(3);
        RuleFor(req => req.Transformations)
            .NotNull();
    }
    
}

public sealed record CreateDataSetResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
}

public sealed record UpdateDataSetRequest
{
    public required string Name { get; init; }
    
    public required Guid ConnectorId { get; init; }
    public required IReadOnlyList<TransformationBlock> Transformations { get; init; }
}

public sealed record DeleteDataSetRequest
{
    public required Guid Id { get; init; }
}
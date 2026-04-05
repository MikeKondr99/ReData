using FastEndpoints;
using FluentValidation;

namespace ReData.DemoApp.Endpoints.Transform;

/// <inheritdoc />
public sealed class TransformRequestValidator : Validator<TransformRequest>
{
    public TransformRequestValidator()
    {
        RuleFor(req => req.DataConnectorId);

        RuleFor(req => req.Transformations)
            .NotNull();

        RuleFor(req => req.PageNumber).GreaterThanOrEqualTo(1u);

        RuleFor(req => req.PageSize).LessThanOrEqualTo(100u);
    }
}
using ReData.Query.Core.Types;

namespace ReData.Query.Core.Template;

public record struct FieldTemplate(ITemplate Template, FieldType Type) : IResolvedTemplate, IResolvedType
{
    public IReadOnlyList<ResolvedExpr>? Arguments => null;

    ExprType IResolvedType.Type =>
        new ExprType()
        {
            DataType = this.Type.Type,
            CanBeNull = Type.CanBeNull,
            IsConstant = false,
            Aggregated = false,
        };
}
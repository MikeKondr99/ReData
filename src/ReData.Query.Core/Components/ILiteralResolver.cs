using ReData.Query.Core.Template;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Core.Components;

public interface ILiteralResolver
{
    public ResolvedExpr Resolve(Literal literal);
    
}
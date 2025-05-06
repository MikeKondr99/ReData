using ReData.Query.Core.Template;

namespace ReData.Query.Core.Types;

public record struct Map(string Alias, IResolvedTemplate Column, ResolvedExpr ResolvedExpr);
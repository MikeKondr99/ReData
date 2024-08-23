using ReData.Query.Visitors;

namespace ReData.Query;

public struct FunctionInfo
{
    public required string Name;
    public required IReadOnlyList<ExprType> Parameters;
    public required ExprType ReturnType;
    public required ITemplate Template;
}
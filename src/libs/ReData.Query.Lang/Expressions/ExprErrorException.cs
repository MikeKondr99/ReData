using ReData.Query.Common;

namespace ReData.Query.Lang.Expressions;

public class ExprErrorException : Exception
{
    public ExprErrorException(Exception e)
        : base(string.Empty, e)
    {
    }

    public required ExprError Error { get; init; }
}
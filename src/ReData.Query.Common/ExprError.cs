namespace ReData.Query.Common;

public record ExprError
{
    public required ExprSpan Span { get; init; }
    
    public required string Message { get; init; }
}
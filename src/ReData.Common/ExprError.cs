namespace ReData.Query.Core;

public record ExprError
{
    public required ExprSpan Span { get; init; }
    
    public required string Message { get; init; }
}

public record struct ExprSpan(int Line, int Column, int Length);



using ReData.Query.Lang.Expressions;
using ReData.Query.Visitors;

namespace ReData.Query.Visitors;

public interface ILiteralResolver
{
    public Node Resolve(ILiteral literal);
}


public record struct TableTemplate(ITemplate Template) : IResolvedTemplate
{
    public IReadOnlyList<Node>? Arguments => null;
}

public record struct FieldType(DataType Type, bool CanBeNull);

public record struct FieldTemplate(ITemplate Template, FieldType Type) : IResolvedTemplate, IResolvedType
{
    public IReadOnlyList<Node>? Arguments => null;

    ExprType IResolvedType.Type =>
        new ExprType()
        {
            Type = this.Type.Type,
            CanBeNull = Type.CanBeNull,
            IsConstant = false,
            Aggregated = false,
        };
}

public interface INameResolver
{
    public TableTemplate ResolveTableName(ReadOnlySpan<string> path);
    
    public FieldTemplate ResolveFieldName(ReadOnlySpan<string> path, FieldType type);
}

public sealed class BasicNameResolver(string open, string close) : INameResolver
{
    public TableTemplate ResolveTableName(ReadOnlySpan<string> path)
    {
        List<IToken> tokens = new List<IToken>();
        foreach (var p in path)
        {
            tokens.Add(new ConstToken(open));
            tokens.Add(new ConstToken(p));
            tokens.Add(new ConstToken(close));
            tokens.Add(new ConstToken("."));
        }
        tokens.RemoveAt(tokens.Count - 1);
        return new TableTemplate(new Template
        {
            Tokens = tokens
        });
    }

    public FieldTemplate ResolveFieldName(ReadOnlySpan<string> path, FieldType type)
    {
        var temp = ResolveTableName(path);
        return new FieldTemplate(temp.Template, type);
    }
}
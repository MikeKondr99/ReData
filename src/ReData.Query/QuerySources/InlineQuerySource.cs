using ReData.Common;
using ReData.Query.Core;
using ReData.Query.Core.Components;
using ReData.Query.Core.Components.Implementation;
using ReData.Query.Core.Template;
using ReData.Query.Core.Types;
using ReData.Query.Lang.Expressions;
using static ReData.Query.Core.Types.DataType;

namespace ReData.Query.QuerySources;

public record struct InlineQuerySource : IQuerySource
{
    public IResolvedTemplate Name { get; init; }

    private readonly IFieldStorage _fieldStorage;

    private readonly DatabaseType _databaseType;
    public IFieldStorage Fields() => _fieldStorage;
    
    public InlineQuerySource(string columnName, string[] dataExpressions, DatabaseType databaseType)
    {
        if (databaseType is DatabaseType.Oracle)
        {
            throw new Exception("Создание запроса из списка значений не поддерживается для Oracle");
        }
        
        var ft = new Factory();
        var resolver= ft.CreateExpressionResolver(databaseType);
        IExpressionCompiler compiler = new ExpressionCompiler();
        var fields = new FieldStorage([]);

        var exprs = dataExpressions
            .Select(d => Expr.Parse(d).Expect("Inline expression must be valid"))
            .Select(e => resolver.ResolveExpr(e, fields).Expect("Inline expression must be resolvable")).ToArray();

        var fieldType = exprs.Aggregate(new FieldType(Unknown, false), (t, e) => ResolveField(t, e.Type));

        var compiled = exprs.Select(exp => compiler.Compile(exp)).ToArray();

        string data =
            $"SELECT {compiled.First()} AS {resolver.ResolveName([columnName]).Template}\n{(compiled.Length > 1 ? "UNION ALL\n" : "")}";

        if (compiled.Length > 1)
        {
            data += compiled
                .Skip(1)
                .Select(s => $"SELECT {s}\n")
                .JoinBy("UNION ALL\n");
        }

        _databaseType = databaseType;
        Name = new NameTemplate(Template.Create($"({data}) AS {resolver.ResolveName(["InlineQuery"]).Template.ToString()}"));
        _fieldStorage = new FieldStorage([
            new Field()
            {
                Alias = columnName,
                Type = fieldType,
            }
        ]);
    }
    
    private static FieldType ResolveField(FieldType field, ExprType type)
    {
        if (field.Type is Unknown or Null)
        {
            return new FieldType(type.DataType, type.CanBeNull);
        }

        if (field.Type != type.DataType && type.DataType is not Null)
        {
            throw new Exception("Типы в inline не должны быть разные");
        }

        return new FieldType(field.Type, field.CanBeNull || type.CanBeNull);
    }
    
    public QueryBuilder ToQueryBuilder()
    {
        var resolver= new Factory().CreateExpressionResolver(_databaseType);
        return new QueryBuilder(new Core.Query()
        {
            Name = resolver.ResolveName(["InlineQuery"]),
            From = this,
        }, resolver);
    }

}
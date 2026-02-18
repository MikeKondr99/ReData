using ReData.Query;
using ReData.Query.Core;
using ReData.Query.Core.Components.Implementation;
using ReData.Query.Core.Template;
using ReData.Query.Core.Types;
using ReData.Query.Lang.Expressions;

namespace ReData.ExprTree.Cli;

public static class PostgresExpressionSqlResolver
{
    public static SqlTraceResult Resolve(Expr expr)
    {
        var resolver = Factory.CreateExpressionResolver(DatabaseType.PostgreSql);
        var functions = Factory.CreateFunctionStorage(DatabaseType.PostgreSql);

        var fields = CollectFieldNames(expr)
            .Select(name => new Field
            {
                Alias = name,
                Template = resolver.ResolveName([name]).Template,
                Type = new FieldType(DataType.Number, true),
            })
            .ToArray();

        var source = new TableQuerySource(
            resolver.ResolveName(["src"]),
            fields);

        var context = new ResolutionContext
        {
            QuerySource = source,
            Errors = [],
            Functions = functions,
            Variables = new Dictionary<string, ReData.Query.Runners.Value.IValue>(),
        };

        var resolved = resolver.ResolveExpr(expr, context);
        if (resolved is null)
        {
            var message = context.Errors.Count == 0
                ? "Expression cannot be resolved for PostgreSQL."
                : string.Join(Environment.NewLine, context.Errors.Select(e => e.Message));
            throw new InvalidOperationException(message);
        }

        return SqlTraceCompiler.Compile(resolved.Value);
    }

    private static IReadOnlyCollection<string> CollectFieldNames(Expr expr)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        Visit(expr, result);
        return result;
    }

    private static void Visit(Expr expr, ISet<string> names)
    {
        switch (expr)
        {
            case NameExpr name:
                names.Add(name.Value);
                return;
            case FuncExpr func:
                foreach (var argument in func.Arguments)
                {
                    Visit(argument, names);
                }

                return;
            default:
                return;
        }
    }
}

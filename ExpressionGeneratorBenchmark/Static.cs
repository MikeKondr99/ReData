using ReData.Query;
using ReData.Query.Visitors;

namespace ExpressionGeneratorBenchmark;

public static class Static
{
    public static FunctionStorage Functions { get; } =
    [
        // new FunctionInfo()
        // {
        //     Name = "+",
        //     Parameters = [ExprType.Integer, ExprType.Integer],
        //     ReturnType = ExprType.Integer,
        //     Template = Template.Compile($"({0} + {1})")
        // }
    ];

    public static IFieldStorage Fields { get; } = new EmptyFieldStorage();



}
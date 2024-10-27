// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using ExpressionGeneratorBenchmark;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsTCPIP;
using ReData.Query;
using ReData.Query.Impl.LiteralBuilders;
using ReData.Query.Lang.Expressions;
using ReData.Query.Visitors;

// BenchmarkRunner.Run<ExprGenerationTest>();

var test = new ExprGenerationTest();
using (Measure.TimeAndMemory())
{
    // test.ExprGenerator();
}

[MemoryDiagnoser]
public class ExprGenerationTest
{
    [Params(1, 5, 10)] public int N;

    private int[] numbers;

    private IExpr expr;

    [GlobalSetup]
    public void Setup()
    {
        numbers = Enumerable.Range(1, N).ToArray();
        expr = new IntegerLiteral(1);
        for (int i = 2; i <= N; i++)
        {
            expr = new FuncExpr
            {
                Name = "+",
                Arguments = [expr, new IntegerLiteral(i)]
            };
        }
    }

    // [Benchmark]
    // public string ExprGenerator()
    // {
    //     var res = new ExpressionBuilder()
    //     {
    //         FunctionStorage = Static.Functions,
    //         LiteralBuilder = new PostgresLiteralBuilder()
    //     };
    //     StringBuilder sb = new StringBuilder();
    //     res.Write(sb, expr, Static.Fields);
    //
    //     // GC.Collect();
    //     // sb = new StringBuilder();
    //     // res.Write(sb,expr,Static.Fields);
    //
    //     return sb.ToString();
    // }
    //
    // [Benchmark(Baseline = true)]
    // public string NaiveConcatination()
    // {
    //     var result = "";
    //     for (int i = 0; i < numbers.Length; i++)
    //     {
    //         result += "(";
    //         result += numbers[i].ToString();
    //         if (i != 9)
    //         {
    //             result += " + ";
    //         }
    //
    //         result += ")";
    //     }
    //
    //     return result;
    // }
}

public static class Measure
{
    public static MeasureScope TimeAndMemory()
    {
        return new MeasureScope();
    }

    public class MeasureScope : IDisposable
    {
        private readonly Stopwatch _watch;
        private readonly long _memory;

        public MeasureScope()
        {
            _memory = System.Diagnostics.Process.GetCurrentProcess().WorkingSet64;
            _watch = new Stopwatch();
            _watch.Start();
        }

        public void Dispose()
        {
            _watch.Stop();
            var mem = System.Diagnostics.Process.GetCurrentProcess().WorkingSet64;
            Console.WriteLine($"TIME: {_watch.ElapsedMilliseconds} MEM: {mem - _memory}");
        }
    }
}
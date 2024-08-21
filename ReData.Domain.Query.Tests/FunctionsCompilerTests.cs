using ReData.Domain.Query.Lang.Expressions;

namespace ReData.Domain.Query.Tests;

public class FunctionsCompilerTests
{
    [Fact]
    public void ShouldCompileBasicFunction()
    {
        var func = WriterCompiler.Compile($"UPPER({0})");

        FunctionWriterContext context = new FunctionWriterContext();

        func(context, [new StringLiteral("TeSt")]);
        
        
    }
    
    
}
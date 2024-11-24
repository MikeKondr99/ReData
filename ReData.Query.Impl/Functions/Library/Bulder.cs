
using System.Runtime.InteropServices.JavaScript;
using Microsoft.VisualBasic.CompilerServices;
using ReData.Query.Functions;
using ReData.Query.Visitors;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;
using static DataType;

public abstract class FunctionsDescriptor
{
    private List<FunctionBuilder> builders = new ();

    

    protected FunctionBuilder Function(string name)
    {
        var builder = FunctionBuilder.Function(name);
        builders.Add(builder);
        return builder;
    }

    protected FunctionBuilder Method(string name)
    {
        var builder = FunctionBuilder.Method(name);
        builders.Add(builder);
        return builder;

    }

    protected FunctionBuilder Binary(string name)
    {
        var builder = FunctionBuilder.Binary(name);
        builders.Add(builder);
        return builder;
    }

    protected FunctionBuilder Binary(string name, DataType left, DataType right)
    {
        var builder = FunctionBuilder.Binary(name).Arg("left", left).Arg("right", right);
        builders.Add(builder);
        return builder;
    }
    
    protected FunctionBuilder Unary(string name)
    {
        var builder = FunctionBuilder.Binary(name);
        builders.Add(builder);
        return builder;
    }

    public IEnumerable<FunctionDefinition> GetFunctions()
    {
        builders = new List<FunctionBuilder>();
        Functions();
        foreach (var builder in builders)
        {
            yield return builder.Build();
        }
    }

    protected abstract void Functions();
}

public record FunctionBuilder
{
    public FunctionBuilder()
    {
    }

    private string Name { get; set; }
    private FunctionKind Kind { get; set; }

    private string? _doc;

    private List<FunctionArgument> Arguments { get; set; } = [];

    private FunctionReturnType? ReturnType { get; set; }

    private IReadOnlyDictionary<DatabaseTypeFlags, ITemplate>? _templates { get; set; }

    private ImplicitCastMetadata? ImplicitCast { get; set; }

    private Func<bool[], bool>? NullIf { get; set; }

    public static FunctionBuilder Function(string name)
    {
        return new FunctionBuilder()
        {
            Name = name,
            Kind = FunctionKind.Default
        };
    }

    public static FunctionBuilder Method(string name)
    {
        return new FunctionBuilder()
        {
            Name = name,
            Kind = FunctionKind.Method
        };
    }
    
    public static FunctionBuilder Binary(string name)
    {
        return new FunctionBuilder()
        {
            Name = name,
            Kind = FunctionKind.Binary
        };
    }
    
    public static FunctionBuilder Unary(string name)
    {
        return new FunctionBuilder()
        {
            Name = name,
            Kind = FunctionKind.Binary
        };
    }
    
    public FunctionBuilder Doc(string doc)
    {
        this._doc = doc;
        return this;
    }

    public FunctionBuilder Arg(string name, DataType type)
    {
        this.Arguments.Add(new FunctionArgument()
        {
            Name = name,
            Type = new ()
            {
                DataType = type,
                CanBeNull = true,
            }
        });
        return this;
    }
    
    public FunctionBuilder ReqArg(string name, DataType type)
    {
        this.Arguments.Add(new FunctionArgument()
        {
            Name = name,
            Type = new ()
            {
                DataType = type,
                CanBeNull = false,
            }
        });
        return this;
    }
    
    public FunctionBuilder Returns(DataType type)
    {
        this.ReturnType = new FunctionReturnType()
        {
            DataType = type,
            CanBeNull = true,
            Aggregated = false,
        };
        return this;
    }
    
    public FunctionBuilder ReturnsNotNull(DataType type)
    {
        this.ReturnType = new FunctionReturnType()
        {
            DataType = type,
            CanBeNull = false,
            Aggregated = false,
        };
        return this;
    }
    
    public FunctionBuilder Returns(DataType type, Func<bool[],bool> nullIf)
    {
        this.ReturnType = new FunctionReturnType()
        {
            DataType = type,
            CanBeNull = true,
            Aggregated = false,
        };
        this.NullIf = nullIf;
        return this;
    }
    
    public FunctionBuilder Templates(Dictionary<DatabaseTypeFlags, TemplateInterpolatedStringHandler> templates)
    {
        this._templates = templates.ToDictionary(kv => kv.Key, kv => (ITemplate) new Template() { Tokens = kv.Value.tokens });
        return this;
    }

    public FunctionDefinition Build()
    {
        ArgumentNullException.ThrowIfNull(ReturnType);
        ArgumentNullException.ThrowIfNull(_templates);
        return new FunctionDefinition
        {
            Name = this.Name,
            Doc = this._doc,
            Arguments = this.Arguments,
            ReturnType = this.ReturnType,
            Kind = this.Kind,
            Templates = this._templates,
            NullIf = this.NullIf ?? ((args) => args.Any(a => a)),
        };

    }
}

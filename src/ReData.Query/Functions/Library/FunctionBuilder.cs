using ReData.Query.Core.Template;
using ReData.Query.Core.Types;

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
    
    protected FunctionBuilder AggFunction(string name)
    {
        var builder = FunctionBuilder.AggFunction(name);
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
        var builder = FunctionBuilder.Unary(name);
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


    public DataTypes Types { get; } = new DataTypes();

    public struct DataTypes
    {
        public DataTypes() { }
        
        public DataType[] All { get; } = [Number, Integer, Text, Bool, DateTime];
        
        public DataType[] AllWithoutBool { get; } = [Number, Integer, Text, DateTime];
        
        public DataType[] Numbers { get; } = [Number, Integer];
        
        public DataType[] NumbersAndDate { get; } = [Number, Integer, DateTime];
    }
}

public record FunctionBuilder
{
    private FunctionBuilder(string name)
    {
        Name = name;
    }

    private string Name { get; init; }
    private FunctionKind Kind { get; set; }

    private string? _doc;

    private List<FunctionArgument> Arguments { get; set; } = [];

    private FunctionReturnType? ReturnType { get; set; }
    
    private ConstPropagation ConstPropagation { get; set; }

    private bool IsAggregated { get; set; } = false;

    private IReadOnlyDictionary<DatabaseTypeFlags, ITemplate>? _templates { get; set; }

    private uint? ImplicitCastCost { get; set; }

    private Func<IEnumerable<bool>, bool> _customNullPropagation;

    public static FunctionBuilder Function(string name)
    {
        return new FunctionBuilder(name)
        {
            Kind = FunctionKind.Default
        };
    }
    
    public static FunctionBuilder AggFunction(string name)
    {
        return new FunctionBuilder(name)
        {
            Kind = FunctionKind.Default,
            IsAggregated = true,
        };
    }

    public static FunctionBuilder Method(string name)
    {
        return new FunctionBuilder(name)
        {
            Kind = FunctionKind.Method
        };
    }
    
    public static FunctionBuilder Binary(string name)
    {
        return new FunctionBuilder(name)
        {
            Kind = FunctionKind.Binary
        };
    }
    
    public static FunctionBuilder Unary(string name)
    {
        return new FunctionBuilder(name)
        {
            Kind = FunctionKind.Unary
        };
    }
    
    public FunctionBuilder Doc(string doc)
    {
        this._doc = doc;
        return this;
    }

    public FunctionBuilder Arg(string name, DataType type, FunctionArgumentOptions options = FunctionArgumentOptions.None)
    {
        Arguments.Add(new FunctionArgument()
        {
            Name = name,
            Type = new ()
            {
                DataType = type,
                CanBeNull = true,
            },
            Options = options,
        });
        return this;
    }
    
    public FunctionBuilder ReqArg(string name, DataType type, FunctionArgumentOptions options = FunctionArgumentOptions.None)
    {
        Arguments.Add(new FunctionArgument()
        {
            Name = name,
            Type = new ()
            {
                DataType = type,
                CanBeNull = false,
            },
            Options = options,
        });
        return this;
    }
    
    public FunctionBuilder Returns(DataType type, ConstPropagation @const = ConstPropagation.Default)
    {
        ReturnType = new FunctionReturnType()
        {
            DataType = type,
            CanBeNull = true,
            Aggregated = IsAggregated,
        };
        ConstPropagation = @const;
        return this;
    }
    
    public FunctionBuilder ReturnsNotNull(DataType type, ConstPropagation @const = ConstPropagation.Default)
    {
        ReturnType = new FunctionReturnType()
        {
            DataType = type,
            CanBeNull = false,
            Aggregated = IsAggregated,
        };
        ConstPropagation = @const;
        return this;
    }
    
    public FunctionBuilder ImplicitCast(uint cost)
    {
        this.ImplicitCastCost = cost;
        return this;
    }
    
    public FunctionBuilder Templates(Dictionary<DatabaseTypeFlags, TemplateInterpolatedStringHandler> templates)
    {
        this._templates = templates.ToDictionary(kv => kv.Key, kv => (ITemplate) new Template() { Tokens = kv.Value.tokens });
        return this;
    }
    
    public FunctionBuilder Template(TemplateInterpolatedStringHandler template)
    {
        return Templates(new()
        {
            [All] = template
        });
    }
    
    public FunctionBuilder CustomNullPropagation(Func<IEnumerable<bool>, bool> func)
    {
        _customNullPropagation = func;
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
            ImplicitCast = ImplicitCastCost is not null ? new ImplicitCastMetadata()
            {
                Cost = ImplicitCastCost.Value
            } : null,
            CustomNullPropagation = _customNullPropagation,
            ConstPropagation = this.ConstPropagation,
        };

    }
}

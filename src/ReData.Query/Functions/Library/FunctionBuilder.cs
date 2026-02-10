using System.Diagnostics;
using ReData.Query.Core.Template;
using ReData.Query.Core.Types;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypes;
using static DataType;

public abstract class FunctionsDescriptor
{
    private List<FunctionBuilder> builders = new();

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


#pragma warning disable SA1129
    public DataTypes Types { get; } = new DataTypes();
#pragma warning restore SA1129

    public struct DataTypes
    {
        public DataTypes()
        {
        }

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

    private string? doc;

    private List<FunctionArgument> Arguments { get; set; } = [];

    private FunctionReturnType? ReturnType { get; set; }

    private ConstPropagation ConstPropagation { get; set; }

    private bool IsAggregated { get; set; }

    private IReadOnlyDictionary<DatabaseTypes, IFunctionTemplate>? templates;

    private uint? ImplicitCastCost { get; set; }

    private Func<IEnumerable<bool>, bool>? customNullPropagation;

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
        this.doc = doc;
        return this;
    }

    public FunctionBuilder Arg(string name, DataType type, bool propagateNull = true, bool isConst = false)
    {
        this.Arguments.Add(new FunctionArgument()
        {
            Name = name,
            Type = new()
            {
                DataType = type,
                CanBeNull = true,
            },
            PropagateNull = propagateNull,
            IsConstRequired = isConst,
        });
        return this;
    }

    public FunctionBuilder ReqArg(string name, DataType type, bool isConst = false)
    {
        this.Arguments.Add(new FunctionArgument()
        {
            Name = name,
            Type = new()
            {
                DataType = type,
                CanBeNull = false,
            },
            PropagateNull = false,
            IsConstRequired = isConst,
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

    public FunctionBuilder Templates(Dictionary<DatabaseTypes, TemplateInterpolatedStringHandler> templates)
    {
        this.templates = templates.ToDictionary(
            kv => kv.Key,
            kv => (IFunctionTemplate)new StaticFunctionTemplate(new Template()
            {
                Tokens = kv.Value.Tokens
            }));
        return this;
    }

    public FunctionBuilder TemplatesDynamic(Dictionary<DatabaseTypes, Func<TemplateContext, ITemplate>> templates)
    {
        this.templates = templates.ToDictionary(
            kv => kv.Key,
            kv => (IFunctionTemplate)new DynamicFunctionTemplate(kv.Value));
        return this;
    }

    public FunctionBuilder TemplatesDynamic(Dictionary<DatabaseTypes, Func<TemplateContext, TemplateInterpolatedStringHandler>> templates)
    {
        this.templates = templates.ToDictionary(
            kv => kv.Key,
            kv => (IFunctionTemplate)new DynamicFunctionTemplate(ctx => new Template()
            {
                Tokens = kv.Value(ctx).Tokens
            }));
        return this;
    }
    
    public FunctionBuilder TemplatesX(Func<int, Dictionary<DatabaseTypes, TemplateInterpolatedStringHandler>> templates)
    {
        return Templates(templates(0));
    }
    
    public FunctionBuilder TemplatesX(Func<int, int, Dictionary<DatabaseTypes, TemplateInterpolatedStringHandler>> templates)
    {
        return Templates(templates(0, 1));
    }
    
    public FunctionBuilder TemplatesX(Func<int, int, int, Dictionary<DatabaseTypes, TemplateInterpolatedStringHandler>> templates)
    {
        return Templates(templates(0, 1, 2));
    }

    public FunctionBuilder Template(TemplateInterpolatedStringHandler template)
    {
        templates = new Dictionary<DatabaseTypes, IFunctionTemplate>()
        {
            [All] = new StaticFunctionTemplate(new Template()
            {
                Tokens = template.Tokens
            })
        };
        return this;
    }

    public FunctionBuilder CustomNullPropagation(Func<IEnumerable<bool>, bool> func)
    {
        customNullPropagation = func;
        return this;
    }

    public FunctionDefinition Build()
    {
        ArgumentNullException.ThrowIfNull(ReturnType);
        if (templates is null)
        {
            throw new ArgumentNullException(nameof(templates));
        }

        return new FunctionDefinition
        {
            Name = Name,
            Doc = doc,
            Arguments = Arguments,
            ReturnType = ReturnType,
            Kind = Kind,
            Templates = templates,
            ImplicitCast = ImplicitCastCost is not null
                ? new ImplicitCastMetadata()
                {
                    Cost = ImplicitCastCost.Value
                }
                : null,
            CustomNullPropagation = customNullPropagation,
            ConstPropagation = this.ConstPropagation,
        };
    }

}

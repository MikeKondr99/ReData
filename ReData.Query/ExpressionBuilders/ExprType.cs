namespace ReData.Query;


public record struct ExprType
{
    public required DataType Type { get; init; }

    public bool CanBeNull { get; init; }

    public bool Aggregated { get; init; }

    public bool IsConstant { get; init; }


    public static ExprType Unknown()
    {
        return new ExprType()
        {
            Type = DataType.Unknown,
        };
    }

    public static ExprType FromDataType(DataType type)
    {
        return new ExprType()
        {
            Type = type,
            IsConstant = false,
            CanBeNull = false,
        };
        
    }

    public static ExprType Null()
    {
        return new ExprType()
        {
            Type = DataType.Null,
            IsConstant = true,
            CanBeNull = true,
        };
    }
    
    public static ExprType Number()
    {
        return new ExprType()
        {
            Type = DataType.Number,
        };
    }
    
    public static ExprType Integer()
    {
        return new ExprType()
        {
            Type = DataType.Integer,
        };
    }
    
    public static ExprType Text()
    {
        return new ExprType()
        {
            Type = DataType.Text,
        };
    }
    
    public static ExprType Boolean()
    {
        return new ExprType()
        {
            Type = DataType.Boolean,
        };
    }

    public ExprType Optional() => this with
    {
        CanBeNull = true
    };

    public ExprType Aggr() => this with
    {
        Aggregated = true,
    };

    public ExprType Const() => this with
    {
        IsConstant = true,
    };

    public override string ToString()
    {
        return $"{(IsConstant ? "const " : "")}{(Aggregated ? "aggr " : "")}{Type}{(CanBeNull ? "?" : "")}";
    }
}

public enum DataType
{
    Unknown = 0,
    Null = 1,
    Number = 2,
    Integer = 3,
    Text = 4,
    Boolean = 5,
}
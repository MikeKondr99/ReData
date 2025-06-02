namespace ReData.Query.Core.Types;


public record struct ExprType
{
    public required DataType DataType { get; init; }

    public bool CanBeNull { get; init; }

    public bool Aggregated { get; init; }

    public bool IsConstant { get; init; }


    public static ExprType Unknown()
    {
        return new ExprType()
        {
            DataType = DataType.Unknown,
        };
    }

    public static ExprType FromDataType(DataType type)
    {
        return new ExprType()
        {
            DataType = type,
            IsConstant = false,
            CanBeNull = false,
        };
        
    }

    public static ExprType Null()
    {
        return new ExprType()
        {
            DataType = DataType.Null,
            IsConstant = true,
            CanBeNull = true,
        };
    }
    
    public static ExprType Number()
    {
        return new ExprType()
        {
            DataType = DataType.Number,
        };
    }
    
    public static ExprType DateTime()
    {
        return new ExprType()
        {
            DataType = DataType.DateTime,
        };
    }
    
    public static ExprType Int()
    {
        return new ExprType()
        {
            DataType = DataType.Integer,
        };
    }
    
    public static ExprType Text()
    {
        return new ExprType()
        {
            DataType = DataType.Text,
        };
    }
    
    public static ExprType Boolean()
    {
        return new ExprType()
        {
            DataType = DataType.Bool,
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
        return $"{(IsConstant ? "const " : "")}{(Aggregated ? "aggr " : "")}{DataType}{(CanBeNull ? "?" : "")}";
    }
}

public enum DataType
{
    Unknown = 0,
    Null = 1,
    Number = 2,
    Integer = 3,
    Text = 4,
    Bool = 5,
    DateTime = 6,
}


public static class DataTypeExtensions
{
    public static string Display(this DataType dataType)
    {
        return dataType switch
        {
            DataType.Unknown => "unk",
            DataType.Null => "null",
            DataType.Number => "num",
            DataType.Integer => "int",
            DataType.Text => "text",
            DataType.Bool => "bool",
            DataType.DateTime => "date",
            _ => throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null)
        };

    }
    
}
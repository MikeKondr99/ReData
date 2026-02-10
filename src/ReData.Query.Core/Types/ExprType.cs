using System.ComponentModel;

namespace ReData.Query.Core.Types;

public record struct ExprType
{
    /// <summary>
    /// Базовый тип данных выражения.
    /// </summary>
    public required DataType DataType { get; init; }

    /// <summary>
    /// Может ли выражение быть NULL.
    /// </summary>
    public bool CanBeNull { get; init; }

    /// <summary>
    /// Признак агрегированного выражения.
    /// </summary>
    public bool Aggregated { get; init; }

    /// <summary>
    /// Признак константного выражения в SQL.
    /// </summary>
    public bool IsConstant { get; init; }

    /// <summary>
    /// Признак литерала, доступного как константный аргумент.
    /// </summary>
    public bool IsLiteral { get; init; }


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

    /// <summary>
    /// Помечает выражение как допускающее NULL.
    /// </summary>
    /// <returns>Тип с признаком nullable.</returns>
    public ExprType Optional() => this with
    {
        CanBeNull = true
    };

    /// <summary>
    /// Помечает выражение как агрегированное.
    /// </summary>
    /// <returns>Тип с признаком агрегации.</returns>
    public ExprType Aggr() => this with
    {
        Aggregated = true,
    };

    /// <summary>
    /// Помечает выражение как константное в SQL.
    /// </summary>
    /// <returns>Тип с признаком константы.</returns>
    public ExprType Const() => this with
    {
        IsConstant = true,
    };

    /// <summary>
    /// Помечает выражение как литерал.
    /// </summary>
    /// <returns>Тип, помеченный как литерал.</returns>
    public ExprType Literal() => this with
    {
        IsConstant = true,
        IsLiteral = true,
    };

    public override string ToString()
    {
        return $"{DataType}{(CanBeNull ? "?" : "")}";
    }
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

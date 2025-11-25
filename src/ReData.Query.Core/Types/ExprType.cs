using System.ComponentModel;
using System.Text.Json.Serialization;

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

/// <summary>
/// Базовые типы данных в системе типов.
/// Определяет примитивные типы, с которыми работают функции.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<DataType>))]
public enum DataType
{
    /// <summary>
    /// Неизвестный тип. Используется для ошибок типизации или когда тип не может быть определен.
    /// </summary>
    [JsonStringEnumMemberName("unk")]
    Unknown = 0,
    
    /// <summary>
    /// Тип NULL. Используется для представления отсутствующего значения.
    /// </summary>
    [JsonStringEnumMemberName("null")]
    Null = 1,
    
    /// <summary>
    /// Число с плавающей точкой.
    /// </summary>
    [JsonStringEnumMemberName("num")]
    Number = 2,
    
    /// <summary>
    /// Целое число. Аналог int/long в языках программирования.
    /// </summary>
    [JsonStringEnumMemberName("int")]
    Integer = 3,
    
    /// <summary>
    /// Текстовый тип. Аналог string в языках программирования.
    /// </summary>
    [JsonStringEnumMemberName("text")]
    Text = 4,
    
    /// <summary>
    /// Логический тип. Значения true/false.
    /// </summary>
    [JsonStringEnumMemberName("bool")]
    Bool = 5,
    
    /// <summary>
    /// Дата и время. Используется для временных меток и операций с датами.
    /// </summary>
    [JsonStringEnumMemberName("date")]
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
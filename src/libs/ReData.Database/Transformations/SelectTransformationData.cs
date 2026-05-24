using System.Text.Json.Serialization;

namespace ReData.DemoApp.Transformations;

/// <summary>
/// $type = "select"
/// </summary>
public sealed record SelectTransformationData : TransformationData
{
    public required SelectItemData[] Items { get; init; }

    public SelectRestOptions RestOptions { get; init; } = SelectRestOptions.Delete;
}

/// <summary>
/// Опция для трансформации, что делать с не указанными полями
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<SelectRestOptions>))]
public enum SelectRestOptions
{
    /// <summary>
    /// Оставить остальные поля как есть
    /// </summary>
    NoAction = 1,

    /// <summary>
    /// Удалить не указанные поля (раньше было вариантом по умолчанию)
    /// </summary>
    Delete = 2,
}

public sealed record SelectItemData
{
    public required string Field { get; init; }
    public required string Expression { get; init; }
}

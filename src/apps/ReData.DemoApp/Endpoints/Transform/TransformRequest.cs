using FluentValidation;
using ReData.DemoApp.Transformations;

namespace ReData.DemoApp.Endpoints.Transform;

/// <summary>
/// «апрос на выполнение трансформаций
/// </summary>
public sealed record TransformRequest
{
    /// <summary>
    /// Id коннектора данных выбранного как источник данных
    /// </summary>
    public required Guid DataConnectorId { get; init; }

    /// <summary>
    /// Ќомер страницы дл€ пагинации
    /// </summary>
    public required uint PageNumber { get; init; }

    /// <summary>
    /// –азмер страницы дл€ пагинации
    /// </summary>
    public required uint PageSize { get; init; }

    /// <summary>
    /// »м€ по пол€ по которому нужно отсортировать конечные данные
    /// ≈сли такого пол€ нет сортировка не происходит
    /// </summary>
    public string? OrderByName { get; init; }

    /// <summary>
    /// ¬ыбрать конечную сортировку по полю <see cref="OrderByName"/> по убыванию
    /// ≈сли такого пол€ с названием <see cref="OrderByName"/> сортировка не происходит
    /// </summary>
    public bool? OrderByDescending { get; init; }

    /// <summary>
    /// Ћист трансформаций выполн€емых по очереди
    /// </summary>
    public required List<TransformationData> Transformations { get; init; } = new();
}

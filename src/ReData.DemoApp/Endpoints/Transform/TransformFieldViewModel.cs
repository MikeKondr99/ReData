using ReData.Query.Core.Types;

namespace ReData.DemoApp.Endpoints.Transform;

/// <summary>
/// Информация по полю полученных данных
/// </summary>
public sealed record TransformFieldViewModel
{
    /// <summary>
    /// Наименование поля
    /// </summary>
    public required string Alias { get; init; }
    
    /// <summary>
    /// Тип поля
    /// </summary>
    public required DataType Type { get; init; }
    
    /// <summary>
    /// Может ли поле быть `null`
    /// </summary>
    public required bool CanBeNull { get; init; }
}
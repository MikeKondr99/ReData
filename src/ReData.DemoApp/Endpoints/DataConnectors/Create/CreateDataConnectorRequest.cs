using FastEndpoints;

namespace ReData.DemoApp.Endpoints.DataConnectors.Create;

/// <summary>
/// Запрос на создание загрузку файла и создания коннектора данных из CSV файла
/// </summary>
public sealed class CreateDataConnectorRequest
{
    /// <summary>
    /// Название нового коннектора данных
    /// </summary>
    [QueryParam]
    [BindFrom("name")]
    public required string Name { get; set; }
    
    /// <summary>
    /// Знак разделитель ожидаемого в CSV файле
    /// </summary>
    [QueryParam]
    [BindFrom("separator")]
    public required char Separator { get; set; }

    /// <summary>
    /// Содержит ли поданный CSV файл заголовок
    /// </summary>
    [QueryParam]
    [BindFrom("withHeader")]
    public required bool WithHeader { get; set; }

    /// <summary>
    /// CSV файл из которого будут взяты данные для нового коннектора данных
    /// </summary>
    public required IFormFile File { get; init; }
}
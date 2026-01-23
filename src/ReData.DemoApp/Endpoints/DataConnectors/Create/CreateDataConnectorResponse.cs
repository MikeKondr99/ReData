namespace ReData.DemoApp.Endpoints.DataConnectors.Create;

/// <summary>
/// Данные
/// </summary>
public class CreateDataConnectorResponse
{
    /// <summary>
    /// Id нового коннектора данных
    /// </summary>
    public required Guid Id { get; init; }
    
    /// <summary>
    /// Имя нового коннектора данных
    /// </summary>
    public required string Name { get; init; }
}
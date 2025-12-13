using ReData.DemoApp.Endpoints.Datasets;
using ReData.DemoApp.Endpoints.Datasets.Create;
using ReData.DemoApp.Endpoints.Datasets.CreateDataset;
using ReData.DemoApp.Transformations;

namespace ReData.DemoApp.Tests.Datasets;

public class CreateDatasetTests(App App) : DemoAppTestBase<App>(App)
{
    private static string FakeDatasetName() => $"dataset{Guid.NewGuid().ToString("N")[..6]}";
    
    private Task<TestResult<CreateDataSetResponse>> Endpoint(CreateDataSetRequest req) =>
        App.Client.POSTAsync<CreateDatasetEndpoint, CreateDataSetRequest, CreateDataSetResponse>(req);
    private Task<TestResult<ErrorResponse>> EndpointError(CreateDataSetRequest req) =>
        App.Client.POSTAsync<CreateDatasetEndpoint, CreateDataSetRequest, ErrorResponse>(req);

    [Fact(DisplayName = "Создание набора с верными данными должно вернуть 'успех'")]
    public async Task CreateDataset_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations = [],
            ConnectorId = Guid.Empty,
        };

        // Act
        var (rsp, res) = await Endpoint(req);

        // Assert
        rsp.IsSuccessStatusCode.Should().BeTrue();
        res.Name.Should().BeEquivalentTo(req.Name);
        res.Id.Should().NotBeEmpty();

        Db.DataSets.Should().Contain(ds => ds.Id == res.Id);
    }

    [Fact(DisplayName = "Создание набора с пустым именем должно вернуть ошибку валидации")]
    public async Task CreateDataset_WithEmptyName_ShouldReturnValidatinoError()
    {
        // Arrange
        var req = new CreateDataSetRequest
        {
            Name = "",
            ConnectorId = Guid.Empty,
            Transformations = []
        };

        // Act
        var rsp = await EndpointError(req);

        // Assert
        rsp.ShouldBeError("name");
        Db.DataSets.Should().NotContain(ds => ds.Name == req.Name);
    }

    [Fact(DisplayName = "Создание набора с null именем должно вернуть ошибку валидации")]
    public async Task CreateDataset_WithNullName_ShouldReturnValidationError()
    {
        // Arrange
        var req = new CreateDataSetRequest
        {
            Name = null!,
            Transformations = [],
            ConnectorId = Guid.Empty,
        };

        // Act
        var rsp = await EndpointError(req);

        // Assert
        rsp.ShouldBeError("name");
        Db.DataSets.Should().NotContain(ds => ds.Name == req.Name);
    }

    [Fact(DisplayName = "Создание набора с слишком коротким именем должно вернуть ошибку валидации")]
    public async Task CreateDataset_WithShortName_ShouldReturnValidationError()
    {
        // Arrange 
        var req = new CreateDataSetRequest
        {
            Name = "x",
            Transformations = [],
            ConnectorId = Guid.Empty
        };

        // Act
        var rsp = await EndpointError(req);

        // Assert
        rsp.ShouldBeError("name");
        Db.DataSets.Should().NotContain(ds => ds.Name == req.Name);
    }

    [Fact(DisplayName = "Создание набора с не уникальным именем должно вернуть ошибку валидации")]
    public async Task CreateDataset_WithNotUniqueName_ShouldReturnConflict()
    {
        // Arrange
        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations = [],
            ConnectorId = Guid.Empty,
        };
        var (rsp, _) = await Endpoint(req);
        rsp.IsSuccessStatusCode.Should().BeTrue();

        // Act
        var response = await EndpointError(req);

        // Assert
        response.ShouldBeError("name");
    }

    [Fact(DisplayName = "Создание набора с null трансформациями должно вернуть ошибку валидации")]
    public async Task CreateDataset_WithNullTransformations_ShouldReturnValidationError()
    {
        // Arrange
        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations = null!,
            ConnectorId = Guid.Empty,
        };

        // Act
        var rsp = await EndpointError(req);

        // Assert
        rsp.ShouldBeError("transformations");
    }

    [Fact(DisplayName = "Создание набора с существующим коннектором должно быть успешным")]
    public async Task CreateDataset_WithExistingConnector_ShouldReturnCreated()
    {
        // Arrange
        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations = [],
            ConnectorId = App.Data.ExistingDataConnector.Id,
        };

        // Act
        var (rsp, res) = await Endpoint(req);

        // Assert
        rsp.IsSuccessStatusCode.Should().BeTrue();
        res.Name.Should().BeEquivalentTo(req.Name);
        res.Id.Should().NotBeEmpty();

        Db.DataSets.Should().Contain(ds =>
            ds.Id == res.Id &&
            ds.DataConnectorId == App.Data.ExistingDataConnector.Id);
    }

    [Fact(DisplayName = "Создание набора с несуществующим коннектором должно вернуть ошибку валидации")]
    public async Task CreateDataset_WithNonExistentConnector_ShouldReturnValidationError()
    {
        // Arrange
        var nonExistentConnectorId = Guid.NewGuid();
        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations = [],
            ConnectorId = nonExistentConnectorId,
        };

        // Act
        var rsp = await EndpointError(req);

        // Assert
        rsp.ShouldBeError("connectorId");
        Db.DataSets.Should().NotContain(ds => ds.DataConnectorId == nonExistentConnectorId);
    }

    [Fact(DisplayName = "Создание набора с пустым GUID коннектором должно быть успешным")]
    public async Task CreateDataset_WithEmptyGuidConnector_ShouldReturnCreated()
    {
        // Arrange
        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations = [],
            ConnectorId = Guid.Empty,
        };

        // Act
        var (rsp, res) = await Endpoint(req);

        // Assert
        rsp.IsSuccessStatusCode.Should().BeTrue();
        Db.DataSets.Should().Contain(ds =>
            ds.Id == res.Id &&
            ds.DataConnectorId == Guid.Empty);
    }

    [Fact(DisplayName = "Создание набора с валидным коннектором и трансформациями должно быть успешным")]
    public async Task CreateDataset_WithValidConnectorAndTransformations_ShouldReturnCreated()
    {
        // Arrange
        TransformationBlock[] transformations =
        [
            new TransformationBlock()
            {
                Enabled = true,
                Transformation = new WhereTransformation
                {
                    Condition = "true"
                }
            }
        ];

        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations = transformations,
            ConnectorId = App.Data.ExistingDataConnector.Id,
        };

        // Act
        var (rsp, res) = await Endpoint(req);

        // Assert
        rsp.IsSuccessStatusCode.Should().BeTrue();
        Db.DataSets.Should().Contain(ds =>
            ds.Id == res.Id &&
            ds.DataConnectorId == App.Data.ExistingDataConnector.Id);
    }
}
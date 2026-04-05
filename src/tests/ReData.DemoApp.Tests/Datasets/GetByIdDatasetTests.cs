using System.DirectoryServices.Protocols;
using ReData.DemoApp.Endpoints.Datasets;
using ReData.DemoApp.Endpoints.Datasets.Create;
using ReData.DemoApp.Endpoints.Datasets.Delete;
using ReData.DemoApp.Endpoints.Datasets.GetById;
using ReData.DemoApp.Transformations;

namespace ReData.DemoApp.Tests.Datasets;

public class GetDatasetByIdTests(App App) : DemoAppTestBase<App>(App)
{
    private static string FakeDatasetName() => $"dataset{Guid.NewGuid().ToString("N")[..6]}";

    private Task<TestResult<DataSetResponse>> Endpoint(GetDatasetByIdRequest req) =>
        App.Client.GETAsync<GetDatasetByIdEndpoint, GetDatasetByIdRequest, DataSetResponse>(req);

    // private Task<TestResult<ErrorResponse>> EndpointError(CreateDataSetRequest req) =>
    //     App.Client.PUTAsync<CreateDatasetEndpoint, CreateDataSetRequest, ErrorResponse>(req);

    private async Task<CreateDataSetResponse> CreateTestDatasetAsync(
        Guid? connectorId = null,
        string? name = null,
        TransformationBlock[]? transformations = null)
    {
        var req = new CreateDataSetRequest
        {
            Name = name ?? FakeDatasetName(),
            Transformations = transformations ?? [],
            ConnectorId = connectorId ?? Guid.Empty,
        };

        var (rsp, res) =
            await App.Client.POSTAsync<CreateDatasetEndpoint, CreateDataSetRequest, CreateDataSetResponse>(req);
        rsp.IsSuccessStatusCode.Should().BeTrue();
        return res;
    }

    [Fact(DisplayName = "Получение существующего набора по ID должно вернуть данные")]
    public async Task GetDatasetById_WithExistingId_ShouldReturnDataset()
    {
        // Arrange
        var existingDataset = await CreateTestDatasetAsync();
        var req = new GetDatasetByIdRequest()
        {
            Id = existingDataset.Id
        };

        // Act
        var (rsp, res) = await Endpoint(req);

        // Assert
        rsp.Should().BeSuccessful();
        res.Id.Should().Be(existingDataset.Id);
        res.Name.Should().Be(existingDataset.Name);
        res.DataConnectorId.Should().Be(Guid.Empty);
        res.Transformations.Should().BeEmpty();
    }

    [Fact(DisplayName = "Получение набора с существующим коннектором должно вернуть данные")]
    public async Task GetDatasetById_WithExistingConnector_ShouldReturnDataset()
    {
        // Arrange
        var existingDataset = await CreateTestDatasetAsync(App.Data.ExistingDataConnector.Id);
        var req = new GetDatasetByIdRequest
        {
            Id = existingDataset.Id
        };

        // Act
        var (rsp, res) = await Endpoint(req);

        // Assert
        rsp.Should().BeSuccessful();
        res.Id.Should().Be(existingDataset.Id);
        res.Name.Should().Be(existingDataset.Name);
        res.DataConnectorId.Should().Be(App.Data.ExistingDataConnector.Id);
        res.Transformations.Should().BeEmpty();
    }

    [Fact(DisplayName = "Получение набора с трансформациями должно вернуть все трансформации")]
    public async Task GetDatasetById_WithTransformations_ShouldReturnTransformations()
    {
        // Arrange
        TransformationBlock[] transformations =
        [
            new TransformationBlock()
            {
                Enabled = true,
                Transformation = new WhereTransformation()
                {
                    Condition = "true"
                },
            },
            new TransformationBlock()
            {
                Enabled = false,
                Transformation = new SelectTransformation()
                {
                    Items =
                    [
                        new SelectItem()
                        {
                            Expression = "id",
                            Field = "Id"
                        }
                    ]
                }
            }
        ];

        var existingDataset = await CreateTestDatasetAsync(transformations: transformations);
        var req = new GetDatasetByIdRequest
        {
            Id = existingDataset.Id
        };

        // Act
        var (rsp, res) = await Endpoint(req);

        // Assert
        rsp.Should().BeSuccessful();
        res.Id.Should().Be(existingDataset.Id);
        res.Transformations.Should().HaveCount(2);
        res.Transformations.Should().BeEquivalentTo(transformations, options => options.WithStrictOrdering());
    }

    [Fact(DisplayName = "Получение несуществующего набора должно вернуть NotFound")]
    public async Task GetDatasetById_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var req = new GetDatasetByIdRequest
        {
            Id = nonExistentId
        };

        // Act
        var (rsp, _) = await Endpoint(req);

        // Assert
        rsp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Получение набора с пустым GUID должно вернуть NotFound")]
    public async Task GetDatasetById_WithEmptyGuid_ShouldReturnNotFound()
    {
        // Arrange
        var req = new GetDatasetByIdRequest
        {
            Id = Guid.Empty
        };

        // Act
        var (rsp, _) = await Endpoint(req);

        // Assert
        rsp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Получение удаленного набора должно вернуть NotFound")]
    public async Task GetDatasetById_AfterDeletion_ShouldReturnNotFound()
    {
        // Arrange
        var existingDataset = await CreateTestDatasetAsync();

        // Delete the dataset first
        var deleteReq = new DeleteDataSetRequest
        {
            Id = existingDataset.Id
        };
        var deleteRsp = await App.Client.DELETEAsync<DeleteDatasetEndpoint, DeleteDataSetRequest>(deleteReq);
        deleteRsp.Should().BeSuccessful();

        var req = new GetDatasetByIdRequest
        {
            Id = existingDataset.Id
        };

        // Act
        var (rsp, _) = await Endpoint(req);

        // Assert
        rsp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Получение набора должно вернуть все поля корректно")]
    public async Task GetDatasetById_ShouldReturnAllFieldsCorrectly()
    {
        // Arrange
        TransformationBlock[] transformations =
        [
            new TransformationBlock()
            {
                Enabled = true,
                Transformation = new WhereTransformation()
                {
                    Condition = "age > 18",
                }
            },
            new TransformationBlock()
            {
                Enabled = false,
                Transformation = new OrderByTransformation()
                {
                    Items =
                    [
                        new OrderItem()
                        {
                            Expression = "name",
                            Descending = false
                        }
                    ]
                }
            }
        ];

        var datasetName = FakeDatasetName();
        var existingDataset = await CreateTestDatasetAsync(
            connectorId: App.Data.ExistingDataConnector.Id,
            name: datasetName,
            transformations: transformations
        );

        var req = new GetDatasetByIdRequest
        {
            Id = existingDataset.Id
        };

        // Act
        var (rsp, res) = await Endpoint(req);

        // Assert
        rsp.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Id.Should().Be(existingDataset.Id);
        res.Name.Should().Be(datasetName);
        res.DataConnectorId.Should().Be(App.Data.ExistingDataConnector.Id);
        res.Transformations.Should().BeEquivalentTo(transformations, options => options.WithStrictOrdering());
        res.Transformations.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Получение набора с пустым коннектором должно вернуть корректные данные")]
    public async Task GetDatasetById_WithEmptyConnector_ShouldReturnCorrectData()
    {
        // Arrange
        var existingDataset = await CreateTestDatasetAsync(Guid.Empty);
        var req = new GetDatasetByIdRequest
        {
            Id = existingDataset.Id
        };

        // Act
        var (rsp, res) = await Endpoint(req);

        // Assert
        rsp.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Id.Should().Be(existingDataset.Id);
        res.DataConnectorId.Should().Be(Guid.Empty);
        res.Transformations.Should().BeEmpty();
    }
}

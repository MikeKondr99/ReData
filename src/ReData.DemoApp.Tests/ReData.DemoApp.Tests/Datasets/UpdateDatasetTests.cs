using Namotion.Reflection;
using ReData.DemoApp.Endpoints.Datasets;
using ReData.DemoApp.Endpoints.Datasets.Create;
using ReData.DemoApp.Endpoints.Datasets.CreateDataset;
using ReData.DemoApp.Endpoints.Datasets.Update;
using ReData.DemoApp.Transformations;

namespace ReData.DemoApp.Tests.Datasets;

public class UpdateDatasetTests(App App) : RollbackTestBase<App>(App)
{
    private static string FakeDatasetName() => $"dataset{Guid.NewGuid().ToString("N")[..6]}";

    private Task<TestResult<TResponse>> Endpoint<TResponse>(UpdateDataSetRequest req) =>
        App.Client.PUTAsync<UpdateDatasetEndpoint, UpdateDataSetRequest, TResponse>(req);

    private async Task<CreateDataSetResponse> CreateTestDatasetAsync(Guid? connectorId = null)
    {
        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations = [],
            ConnectorId = Guid.Empty,
        };

        var (rsp, res) = await App.Client.POSTAsync<CreateDatasetEndpoint, CreateDataSetRequest, CreateDataSetResponse>(req);
        rsp.Should().BeSuccessful();
        return res;
    }

    [Fact(DisplayName = "Обновление набора с верными данными должно вернуть 'обновлено'")]
    public async Task UpdateDataset_WithValidData_ShouldReturnUpdated()
    {
        // Arrange
        var existingDataset = await CreateTestDatasetAsync();
        var updateReq = new UpdateDataSetRequest
        {
            Id = existingDataset.Id,
            Name = FakeDatasetName(),
            Transformations = [],
            ConnectorId = Guid.Empty,
        };

        // Act
        var (rsp, res) = await Endpoint<UpdateDataSetResponse>(updateReq);

        // Assert
        rsp.IsSuccessStatusCode.Should().BeTrue();
        res.Name.Should().BeEquivalentTo(updateReq.Name);
        res.Id.Should().Be(existingDataset.Id);

        Db.DataSets.Should().Contain(ds =>
            ds.Id == res.Id &&
            ds.Name == updateReq.Name);
    }

    [Fact(DisplayName = "Обновление набора с пустым именем должно вернуть ошибку валидации")]
    public async Task UpdateDataset_WithEmptyName_ShouldReturnValidationError()
    {
        // Arrange
        var existingDataset = await CreateTestDatasetAsync();
        var updateReq = new UpdateDataSetRequest
        {
            Id = existingDataset.Id,
            Name = "",
            Transformations = [],
            ConnectorId = Guid.Empty,
        };

        // Act
        var rsp = await Endpoint<ErrorResponse>(updateReq);

        // Assert
        rsp.ShouldBeError("name");

        // Verify dataset wasn't updated
        Db.DataSets.Should().Contain(ds =>
            ds.Id == existingDataset.Id &&
            ds.Name == existingDataset.Name);
    }

    [Fact(DisplayName = "Обновление набора с null именем должно вернуть ошибку валидации")]
    public async Task UpdateDataset_WithNullName_ShouldReturnValidationError()
    {
        // Arrange
        var existingDataset = await CreateTestDatasetAsync();
        var updateReq = new UpdateDataSetRequest
        {
            Id = existingDataset.Id,
            Name = null!,
            Transformations = [],
            ConnectorId = Guid.Empty,
        };

        // Act
        var rsp = await Endpoint<ErrorResponse>(updateReq);

        // Assert
        rsp.ShouldBeError("name");

        // Verify dataset wasn't updated
        Db.DataSets.Should().Contain(ds =>
            ds.Id == existingDataset.Id &&
            ds.Name == existingDataset.Name);
    }

    [Fact(DisplayName = "Обновление набора с слишком коротким именем должно вернуть ошибку валидации")]
    public async Task UpdateDataset_WithShortName_ShouldReturnValidationError()
    {
        // Arrange
        var existingDataset = await CreateTestDatasetAsync();
        var updateReq = new UpdateDataSetRequest
        {
            Id = existingDataset.Id,
            Name = "x",
            Transformations = [],
            ConnectorId = Guid.Empty,
        };

        // Act
        var rsp = await Endpoint<ErrorResponse>(updateReq);

        // Assert
        rsp.ShouldBeError("name");

        // Verify dataset wasn't updated
        Db.DataSets.Should().Contain(ds =>
            ds.Id == existingDataset.Id &&
            ds.Name == existingDataset.Name);
    }

    [Fact(DisplayName = "Обновление набора с не уникальным именем должно вернуть ошибку валидации")]
    public async Task UpdateDataset_WithNotUniqueName_ShouldReturnValidationError()
    {
        // Arrange
        var existingDataset1 = await CreateTestDatasetAsync();
        var existingDataset2 = await CreateTestDatasetAsync();

        var updateReq = new UpdateDataSetRequest
        {
            Id = existingDataset1.Id,
            Name = existingDataset2.Name, // Try to use name from another dataset
            Transformations = [],
            ConnectorId = Guid.Empty,
        };

        // Act
        var rsp = await Endpoint<ErrorResponse>(updateReq);

        // Assert
        rsp.ShouldBeError("name");

        // Verify dataset wasn't updated
        Db.DataSets.Should().Contain(ds =>
            ds.Id == existingDataset1.Id &&
            ds.Name == existingDataset1.Name);
    }

    [Fact(DisplayName = "Обновление набора с null трансформациями должно вернуть ошибку валидации")]
    public async Task UpdateDataset_WithNullTransformations_ShouldReturnValidationError()
    {
        // Arrange
        var existingDataset = await CreateTestDatasetAsync();
        var updateReq = new UpdateDataSetRequest
        {
            Id = existingDataset.Id,
            Name = FakeDatasetName(),
            Transformations = null!,
            ConnectorId = Guid.Empty,
        };

        // Act
        var rsp = await Endpoint<ErrorResponse>(updateReq);

        // Assert
        rsp.ShouldBeError("transformations");

        // Verify dataset wasn't updated
        Db.DataSets.Should().Contain(ds =>
            ds.Id == existingDataset.Id &&
            ds.Name == existingDataset.Name);
    }

    [Fact(DisplayName = "Обновление набора с тем же именем должно быть успешным")]
    public async Task UpdateDataset_WithSameName_ShouldReturnSuccess()
    {
        // Arrange
        var existingDataset = await CreateTestDatasetAsync();
        var updateReq = new UpdateDataSetRequest
        {
            Id = existingDataset.Id,
            Name = existingDataset.Name, // Same name should be allowed
            Transformations = [],
            ConnectorId = Guid.Empty,
        };

        // Act
        var (rsp, res) = await Endpoint<UpdateDataSetResponse>(updateReq);

        // Assert
        rsp.IsSuccessStatusCode.Should().BeTrue();
        res.Name.Should().BeEquivalentTo(existingDataset.Name);
        res.Id.Should().Be(existingDataset.Id);

        Db.DataSets.Should().Contain(ds =>
            ds.Id == res.Id &&
            ds.Name == existingDataset.Name);
    }

    [Fact(DisplayName = "Обновление несуществующего набора должно вернуть 'не найдено'")]
    public async Task UpdateDataset_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateReq = new UpdateDataSetRequest
        {
            Id = nonExistentId,
            Name = FakeDatasetName(),
            Transformations = [],
            ConnectorId = Guid.Empty,
        };

        // Act
        var (rsp, _) = await Endpoint<UpdateDataSetResponse>(updateReq);

        // Assert
        rsp.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify no dataset was created with this ID
        Db.DataSets.Should().NotContain(ds => ds.Id == nonExistentId);
    }


    [Fact(DisplayName = "Обновление набора с существующим коннектором должно быть успешным")]
    public async Task UpdateDataset_WithExistingConnector_ShouldReturnSuccess()
    {
        // Arrange
        var existingDataset = await CreateTestDatasetAsync(Guid.Empty);
        var updateReq = new UpdateDataSetRequest
        {
            Id = existingDataset.Id,
            Name = FakeDatasetName(),
            Transformations = [],
            ConnectorId = App.Data.ExistingDataConnector.Id,
        };

        // Act
        var (rsp, res) = await Endpoint<UpdateDataSetResponse>(updateReq);

        // Assert
        rsp.IsSuccessStatusCode.Should().BeTrue();
        Db.DataSets.Should().Contain(ds =>
            ds.Id == res.Id &&
            ds.DataConnectorId == App.Data.ExistingDataConnector.Id);
    }

    [Fact(DisplayName = "Обновление набора с несуществующим коннектором должно вернуть ошибку валидации")]
    public async Task UpdateDataset_WithNonExistentConnector_ShouldReturnValidationError()
    {
        // Arrange
        var existingDataset = await CreateTestDatasetAsync(Guid.Empty);
        var nonExistentConnectorId = Guid.NewGuid();
        var updateReq = new UpdateDataSetRequest
        {
            Id = existingDataset.Id,
            Name = FakeDatasetName(),
            Transformations = [],
            ConnectorId = nonExistentConnectorId,
        };

        // Act
        var rsp = await Endpoint<ErrorResponse>(updateReq);

        // Assert
        rsp.ShouldBeError("connectorId");
        Db.DataSets.Should().Contain(ds =>
            ds.Id == existingDataset.Id &&
            ds.DataConnectorId == Guid.Empty); // Should remain unchanged
    }

    [Fact(DisplayName = "Обновление набора с пустым GUID коннектором должно быть успешным")]
    public async Task UpdateDataset_WithEmptyGuidConnector_ShouldReturnSuccess()
    {
        // Arrange
        var existingDataset = await CreateTestDatasetAsync(App.Data.ExistingDataConnector.Id);
        var updateReq = new UpdateDataSetRequest
        {
            Id = existingDataset.Id,
            Name = FakeDatasetName(),
            Transformations = [],
            ConnectorId = Guid.Empty,
        };

        // Act
        var (rsp, res) = await Endpoint<UpdateDataSetResponse>(updateReq);

        // Assert
        rsp.IsSuccessStatusCode.Should().BeTrue();
        Db.DataSets.Should().Contain(ds =>
            ds.Id == res.Id &&
            ds.DataConnectorId == Guid.Empty);
    }

    [Fact(DisplayName = "Обновление коннектора на существующий должно быть успешным")]
    public async Task UpdateDataset_ChangingToExistingConnector_ShouldReturnSuccess()
    {
        // Arrange
        var existingDataset = await CreateTestDatasetAsync(Guid.Empty);
        var updateReq = new UpdateDataSetRequest
        {
            Id = existingDataset.Id,
            Name = existingDataset.Name, // Keep same name
            Transformations = [],
            ConnectorId = App.Data.ExistingDataConnector.Id,
        };

        // Act
        var (rsp, res) = await Endpoint<UpdateDataSetResponse>(updateReq);

        // Assert
        rsp.IsSuccessStatusCode.Should().BeTrue();
        Db.DataSets.Should().Contain(ds =>
            ds.Id == res.Id &&
            ds.DataConnectorId == App.Data.ExistingDataConnector.Id);
    }

    [Fact(DisplayName = "Обновление набора с валидным коннектором и трансформациями должно быть успешным")]
    public async Task UpdateDataset_WithValidConnectorAndTransformations_ShouldReturnSuccess()
    {
        // Arrange
        var existingDataset = await CreateTestDatasetAsync(Guid.Empty);
        TransformationBlock[] transformations =
        [
            new TransformationBlock()
            {
                Enabled = true,
                Transformation = new OrderByTransformation()
                {
                    Items =
                    [
                        new OrderItem()
                        {
                            Expression = "field",
                            Descending = true,
                        }
                    ]
                }
            }
        ];

        var updateReq = new UpdateDataSetRequest
        {
            Id = existingDataset.Id,
            Name = FakeDatasetName(),
            Transformations = transformations,
            ConnectorId = App.Data.ExistingDataConnector.Id,
        };

        // Act
        var (rsp, res) = await Endpoint<UpdateDataSetResponse>(updateReq);

        // Assert
        rsp.IsSuccessStatusCode.Should().BeTrue();
        Db.DataSets.Should().Contain(ds =>
            ds.Id == res.Id &&
            ds.DataConnectorId == App.Data.ExistingDataConnector.Id);
    }

    [Fact(DisplayName = "Обновление набора с тем же коннектором должно быть успешным")]
    public async Task UpdateDataset_WithSameConnector_ShouldReturnSuccess()
    {
        // Arrange
        var existingDataset = await CreateTestDatasetAsync(App.Data.ExistingDataConnector.Id);
        var updateReq = new UpdateDataSetRequest
        {
            Id = existingDataset.Id,
            Name = FakeDatasetName(),
            Transformations = [],
            ConnectorId = App.Data.ExistingDataConnector.Id, // Same connector
        };

        // Act
        var (rsp, res) = await Endpoint<UpdateDataSetResponse>(updateReq);


        // Assert
        rsp.IsSuccessStatusCode.Should().BeTrue();
        Db.DataSets.Should().Contain(ds =>
            ds.Id == res.Id &&
            ds.DataConnectorId == App.Data.ExistingDataConnector.Id);
    }
}
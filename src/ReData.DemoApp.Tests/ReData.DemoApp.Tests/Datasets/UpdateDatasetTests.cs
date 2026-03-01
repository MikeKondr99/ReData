using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Namotion.Reflection;
using ReData.DemoApp.Database.Entities;
using ReData.DemoApp.Endpoints.Datasets;
using ReData.DemoApp.Endpoints.Datasets.Create;
using ReData.DemoApp.Endpoints.Datasets.GetById;
using ReData.DemoApp.Endpoints.Datasets.Update;
using ReData.DemoApp.Transformations;

namespace ReData.DemoApp.Tests.Datasets;

public class UpdateDatasetTests(App App) : DemoAppTestBase<App>(App)
{
    private static string FakeDatasetName() => $"dataset{Guid.NewGuid().ToString("N")[..6]}";

    private Task<TestResult<TResponse>> Endpoint<TResponse>(UpdateDataSetRequest req) =>
        App.Client.PUTAsync<UpdateDatasetEndpoint, UpdateDataSetRequest, TResponse>(req);

    private Task<TestResult<DataSetResponse>> GetByIdEndpoint(Guid id) =>
        App.Client.GETAsync<GetDatasetByIdEndpoint, GetDatasetByIdRequest, DataSetResponse>(new GetDatasetByIdRequest { Id = id });

    private async Task<DataSetResponse> GetByIdShouldSucceed(Guid id)
    {
        var (rsp, res) = await GetByIdEndpoint(id);
        rsp.Should().BeSuccessful();
        return res;
    }

    private async Task<CreateDataSetResponse> CreateTestDatasetAsync(
        Guid? connectorId = null,
        IReadOnlyList<TransformationBlock>? transformations = null)
    {
        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations = transformations?.ToList() ?? [],
            ConnectorId = connectorId ?? Guid.Empty,
        };

        var (rsp, res) = await App.Client.POSTAsync<CreateDatasetEndpoint, CreateDataSetRequest, CreateDataSetResponse>(req);
        rsp.Should().BeSuccessful();
        return res;
    }

    private Task<List<TransformationEntity>> GetTransformationsAsync(Guid dataSetId) =>
        Db.Set<TransformationEntity>()
            .Where(t => t.DataSetId == dataSetId)
            .OrderBy(t => t.Order)
            .ToListAsync();

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
        var api = await GetByIdShouldSucceed(existingDataset.Id);
        api.Name.Should().Be(updateReq.Name);
    }

    [Fact(DisplayName = "Обновление набора должно сбрасывать кэш GetById и возвращать свежие данные")]
    public async Task UpdateDataset_AfterCachedGetById_ShouldReturnFreshGetById()
    {
        // Arrange
        var existingDataset = await CreateTestDatasetAsync();
        var updatedName = FakeDatasetName();

        // Warm cache for GetById.
        var (firstGetRsp, firstGet) = await GetByIdEndpoint(existingDataset.Id);
        firstGetRsp.Should().BeSuccessful();
        firstGet.Name.Should().Be(existingDataset.Name);

        var updateReq = new UpdateDataSetRequest
        {
            Id = existingDataset.Id,
            Name = updatedName,
            Transformations = [],
            ConnectorId = Guid.Empty,
        };

        // Act
        var (updateRsp, _) = await Endpoint<UpdateDataSetResponse>(updateReq);
        var (secondGetRsp, secondGet) = await GetByIdEndpoint(existingDataset.Id);

        // Assert
        updateRsp.Should().BeSuccessful();
        secondGetRsp.Should().BeSuccessful();
        secondGet.Name.Should().Be(updatedName);
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
        var api = await GetByIdShouldSucceed(existingDataset.Id);
        api.Name.Should().Be(existingDataset.Name);
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
        var api = await GetByIdShouldSucceed(existingDataset.Id);
        api.DataConnectorId.Should().Be(App.Data.ExistingDataConnector.Id);
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
        var api = await GetByIdShouldSucceed(existingDataset.Id);
        api.DataConnectorId.Should().Be(Guid.Empty);
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
        var api = await GetByIdShouldSucceed(existingDataset.Id);
        api.DataConnectorId.Should().Be(App.Data.ExistingDataConnector.Id);
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
        var api = await GetByIdShouldSucceed(existingDataset.Id);
        api.DataConnectorId.Should().Be(App.Data.ExistingDataConnector.Id);
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
        var api = await GetByIdShouldSucceed(existingDataset.Id);
        api.DataConnectorId.Should().Be(App.Data.ExistingDataConnector.Id);
    }

    [Fact(DisplayName = "Обновление набора должно полностью заменить список трансформаций")]
    public async Task UpdateDataset_ShouldReplaceTransformationsInStorage()
    {
        // Arrange
        var originalTransformations = new List<TransformationBlock>
        {
            new SelectTransformation
            {
                Items =
                [
                    new SelectItem
                    {
                        Field = "id",
                        Expression = "id",
                    }
                ],
                RestOptions = SelectRestOptions.Delete,
            }.Block(),
            new WhereTransformation
            {
                Condition = "id > 1",
            }.Block(),
        };

        var existingDataset = await CreateTestDatasetAsync(
            connectorId: App.Data.ExistingDataConnector.Id,
            transformations: originalTransformations);

        var updateTransformations = new List<TransformationBlock>
        {
            new SelectTransformation
            {
                Items =
                [
                    new SelectItem
                    {
                        Field = "id2",
                        Expression = "id * 2",
                    }
                ],
                RestOptions = SelectRestOptions.Delete,
            }.Block(),
        };

        var updateReq = new UpdateDataSetRequest
        {
            Id = existingDataset.Id,
            Name = existingDataset.Name,
            ConnectorId = App.Data.ExistingDataConnector.Id,
            Transformations = updateTransformations,
        };

        // Act
        var (rsp, res) = await Endpoint<UpdateDataSetResponse>(updateReq);
        var stored = await GetTransformationsAsync(existingDataset.Id);

        // Assert
        rsp.IsSuccessStatusCode.Should().BeTrue();
        res.Transformations.Should().HaveCount(1);
        stored.Should().HaveCount(1);
        stored[0].Order.Should().Be(0);
        stored[0].Data.Should().BeOfType<SelectTransformation>();
    }

    [Fact(DisplayName = "Обновление набора с пустыми трансформациями должно очистить старые")]
    public async Task UpdateDataset_WithEmptyTransformations_ShouldDeleteExistingTransformations()
    {
        // Arrange
        var originalTransformations = new List<TransformationBlock>
        {
            new WhereTransformation
            {
                Condition = "id > 0",
            }.Block(),
        };

        var existingDataset = await CreateTestDatasetAsync(
            connectorId: App.Data.ExistingDataConnector.Id,
            transformations: originalTransformations);

        var updateReq = new UpdateDataSetRequest
        {
            Id = existingDataset.Id,
            Name = existingDataset.Name,
            ConnectorId = App.Data.ExistingDataConnector.Id,
            Transformations = [],
        };

        // Act
        var (rsp, res) = await Endpoint<UpdateDataSetResponse>(updateReq);
        var stored = await GetTransformationsAsync(existingDataset.Id);

        // Assert
        rsp.IsSuccessStatusCode.Should().BeTrue();
        res.Transformations.Should().BeEmpty();
        stored.Should().BeEmpty();
    }

    [Fact(DisplayName = "Update с payload из GetById должен сохранять трансформации")]
    public async Task UpdateDataset_WithRawPayloadFromGetById_ShouldKeepTransformations()
    {
        // Arrange
        var originalTransformations = new List<TransformationBlock>
        {
            new WhereTransformation { Condition = "id > 0" }.Block(),
            new SelectTransformation
            {
                Items = [new SelectItem { Field = "id", Expression = "id" }],
                RestOptions = SelectRestOptions.Delete,
            }.Block(),
        };

        var existingDataset = await CreateTestDatasetAsync(
            connectorId: App.Data.ExistingDataConnector.Id,
            transformations: originalTransformations);

        var existing = await GetByIdShouldSucceed(existingDataset.Id);
        existing.Transformations.Should().HaveCount(2);

        var payload = new
        {
            id = existingDataset.Id,
            name = $"{existing.Name}_updated",
            connectorId = existing.DataConnectorId,
            transformations = existing.Transformations.Select(t => new
            {
                enabled = t.Enabled,
                description = t.Description,
                transformation = t.Transformation,
            }),
        };

        // Act
        var updateRsp = await App.Client.PutAsJsonAsync($"/api/datasets/{existingDataset.Id}", payload);
        var afterUpdate = await GetByIdShouldSucceed(existingDataset.Id);

        // Assert
        updateRsp.Should().BeSuccessful();
        afterUpdate.Transformations.Should().HaveCount(2);
    }
}

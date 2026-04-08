using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using ReData.DemoApp.Database;
using ReData.DemoApp.Database.Entities;
using ReData.DemoApp.Endpoints.Datasets;
using ReData.DemoApp.Endpoints.Datasets.Create;
using ReData.DemoApp.Endpoints.Datasets.GetById;
using ReData.DemoApp.Endpoints.Datasets.Update;
using ReData.DemoApp.Transformations;
using TUnit.Core;

namespace ReData.DemoApp.TUnit.Datasets;

public class UpdateDatasetTests
{
    [ClassDataSource<DefaultReDataApp>(Shared = SharedType.PerTestSession)]
    public required DefaultReDataApp App { get; init; }

    private static string FakeDatasetName() => $"dataset{Guid.NewGuid().ToString("N")[..6]}";

    private static UpdateDataSetRequest Request(Guid id) => new()
    {
        Id = id,
        Name = FakeDatasetName(),
        Transformations = [],
        ConnectorId = Guid.Empty,
    };

    private Task<TestResult<TResponse>> Endpoint<TResponse>(UpdateDataSetRequest req) =>
        App.Client.PUTAsync<UpdateDatasetEndpoint, UpdateDataSetRequest, TResponse>(req);

    private Task<TestResult<DataSetResponse>> GetByIdEndpoint(Guid id) =>
        App.Client.GETAsync<GetDatasetByIdEndpoint, GetDatasetByIdRequest, DataSetResponse>(new GetDatasetByIdRequest { Id = id });

    private async Task<DataSetResponse> GetByIdShouldSucceed(Guid id)
    {
        var (rsp, res) = await GetByIdEndpoint(id);
        await Assert.That((int)rsp.StatusCode).IsBetween(200, 299);
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

        return await App.Client.POSTAsync<CreateDatasetEndpoint, CreateDataSetRequest, CreateDataSetResponse>(req).IsSuccess();
    }

    private async Task<bool> DatasetExists(Expression<Func<DataSetEntity, bool>> predicate)
    {
        await using var scope = App.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();
        return await db.DataSets.AsNoTracking().AnyAsync(predicate);
    }

    private async Task<DataSetEntity?> FindDataset(Guid id)
    {
        await using var scope = App.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();
        return await db.DataSets.AsNoTracking().FirstOrDefaultAsync(ds => ds.Id == id);
    }

    private async Task<List<TransformationEntity>> GetTransformationsAsync(Guid dataSetId)
    {
        await using var scope = App.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();
        return await db.Set<TransformationEntity>()
            .AsNoTracking()
            .Where(t => t.DataSetId == dataSetId)
            .OrderBy(t => t.Order)
            .ToListAsync();
    }

    [Test]
    [DisplayName("Обновление набора с верными данными должно вернуть 'обновлено'")]
    public async Task UpdateDataset_WithValidData_ShouldReturnUpdated()
    {
        var existingDataset = await CreateTestDatasetAsync();
        var updateReq = Request(existingDataset.Id);

        var res = await Endpoint<UpdateDataSetResponse>(updateReq).IsSuccess();

        await Assert.That(res.Name).IsEqualTo(updateReq.Name);
        await Assert.That(res.Id).IsEqualTo(existingDataset.Id);

        var api = await GetByIdShouldSucceed(existingDataset.Id);
        await Assert.That(api.Name).IsEqualTo(updateReq.Name);
    }

    [Test]
    [DisplayName("Обновление набора должно сбрасывать кэш GetById и возвращать свежие данные")]
    public async Task UpdateDataset_AfterCachedGetById_ShouldReturnFreshGetById()
    {
        var existingDataset = await CreateTestDatasetAsync();
        var updatedName = FakeDatasetName();

        var firstGet = await GetByIdShouldSucceed(existingDataset.Id);
        await Assert.That(firstGet.Name).IsEqualTo(existingDataset.Name);

        var updateReq = Request(existingDataset.Id) with
        {
            Name = updatedName,
        };

        await Endpoint<UpdateDataSetResponse>(updateReq).IsSuccess();
        var secondGet = await GetByIdShouldSucceed(existingDataset.Id);

        await Assert.That(secondGet.Name).IsEqualTo(updatedName);
    }

    [Test]
    [DisplayName("Обновление набора с пустым именем должно вернуть ошибку валидации")]
    public async Task UpdateDataset_WithEmptyName_ShouldReturnValidationError()
    {
        var existingDataset = await CreateTestDatasetAsync();
        var updateReq = Request(existingDataset.Id) with
        {
            Name = "",
        };

        var rsp = await Endpoint<ErrorResponse>(updateReq);

        await Assert.That(rsp).IsValidationError("name");
        await Assert.That(await DatasetExists(ds => ds.Id == existingDataset.Id && ds.Name == existingDataset.Name)).IsTrue();
    }

    [Test]
    [DisplayName("Обновление набора с null именем должно вернуть ошибку валидации")]
    public async Task UpdateDataset_WithNullName_ShouldReturnValidationError()
    {
        var existingDataset = await CreateTestDatasetAsync();
        var updateReq = Request(existingDataset.Id) with
        {
            Name = null!,
        };

        var rsp = await Endpoint<ErrorResponse>(updateReq);

        await Assert.That(rsp).IsValidationError("name");
        await Assert.That(await DatasetExists(ds => ds.Id == existingDataset.Id && ds.Name == existingDataset.Name)).IsTrue();
    }

    [Test]
    [DisplayName("Обновление набора с слишком коротким именем должно вернуть ошибку валидации")]
    public async Task UpdateDataset_WithShortName_ShouldReturnValidationError()
    {
        var existingDataset = await CreateTestDatasetAsync();
        var updateReq = Request(existingDataset.Id) with
        {
            Name = "x",
        };

        var rsp = await Endpoint<ErrorResponse>(updateReq);

        await Assert.That(rsp).IsValidationError("name");
        await Assert.That(await DatasetExists(ds => ds.Id == existingDataset.Id && ds.Name == existingDataset.Name)).IsTrue();
    }

    [Test]
    [DisplayName("Обновление набора с не уникальным именем должно вернуть ошибку валидации")]
    public async Task UpdateDataset_WithNotUniqueName_ShouldReturnValidationError()
    {
        var existingDataset1 = await CreateTestDatasetAsync();
        var existingDataset2 = await CreateTestDatasetAsync();

        var updateReq = Request(existingDataset1.Id) with
        {
            Name = existingDataset2.Name,
        };

        var rsp = await Endpoint<ErrorResponse>(updateReq);

        await Assert.That(rsp).IsValidationError("name");
        await Assert.That(await DatasetExists(ds => ds.Id == existingDataset1.Id && ds.Name == existingDataset1.Name)).IsTrue();
    }

    [Test]
    [DisplayName("Обновление набора с null трансформациями должно вернуть ошибку валидации")]
    public async Task UpdateDataset_WithNullTransformations_ShouldReturnValidationError()
    {
        var existingDataset = await CreateTestDatasetAsync();
        var updateReq = Request(existingDataset.Id) with
        {
            Transformations = null!,
        };

        var rsp = await Endpoint<ErrorResponse>(updateReq);

        await Assert.That(rsp).IsValidationError("transformations");
        await Assert.That(await DatasetExists(ds => ds.Id == existingDataset.Id && ds.Name == existingDataset.Name)).IsTrue();
    }

    [Test]
    [DisplayName("Обновление набора с тем же именем должно быть успешным")]
    public async Task UpdateDataset_WithSameName_ShouldReturnSuccess()
    {
        var existingDataset = await CreateTestDatasetAsync();
        var updateReq = Request(existingDataset.Id) with
        {
            Name = existingDataset.Name,
        };

        var res = await Endpoint<UpdateDataSetResponse>(updateReq).IsSuccess();

        await Assert.That(res.Name).IsEqualTo(existingDataset.Name);
        await Assert.That(res.Id).IsEqualTo(existingDataset.Id);

        var api = await GetByIdShouldSucceed(existingDataset.Id);
        await Assert.That(api.Name).IsEqualTo(existingDataset.Name);
    }

    [Test]
    [DisplayName("Обновление несуществующего набора должно вернуть 'не найдено'")]
    public async Task UpdateDataset_WithNonExistentId_ShouldReturnNotFound()
    {
        var nonExistentId = Guid.NewGuid();
        var updateReq = Request(nonExistentId);

        var (rsp, _) = await Endpoint<UpdateDataSetResponse>(updateReq);

        await Assert.That(rsp.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        await Assert.That(await DatasetExists(ds => ds.Id == nonExistentId)).IsFalse();
    }

    [Test]
    [DisplayName("Обновление набора с существующим коннектором должно быть успешным")]
    public async Task UpdateDataset_WithExistingConnector_ShouldReturnSuccess()
    {
        var existingDataset = await CreateTestDatasetAsync(Guid.Empty);
        var updateReq = Request(existingDataset.Id) with
        {
            ConnectorId = App.Data.ExistingDataConnector.Id,
        };

        await Endpoint<UpdateDataSetResponse>(updateReq).IsSuccess();
        var api = await GetByIdShouldSucceed(existingDataset.Id);

        await Assert.That(api.DataConnectorId).IsEqualTo(App.Data.ExistingDataConnector.Id);
    }

    [Test]
    [DisplayName("Обновление набора с несуществующим коннектором должно вернуть ошибку валидации")]
    public async Task UpdateDataset_WithNonExistentConnector_ShouldReturnValidationError()
    {
        var existingDataset = await CreateTestDatasetAsync(Guid.Empty);
        var nonExistentConnectorId = Guid.NewGuid();
        var updateReq = Request(existingDataset.Id) with
        {
            ConnectorId = nonExistentConnectorId,
        };

        var rsp = await Endpoint<ErrorResponse>(updateReq);

        await Assert.That(rsp).IsValidationError("connectorId");
        await Assert.That(await DatasetExists(ds => ds.Id == existingDataset.Id && ds.DataConnectorId == Guid.Empty)).IsTrue();
    }

    [Test]
    [DisplayName("Обновление набора с пустым GUID коннектором должно быть успешным")]
    public async Task UpdateDataset_WithEmptyGuidConnector_ShouldReturnSuccess()
    {
        var existingDataset = await CreateTestDatasetAsync(App.Data.ExistingDataConnector.Id);
        var updateReq = Request(existingDataset.Id);

        await Endpoint<UpdateDataSetResponse>(updateReq).IsSuccess();
        var api = await GetByIdShouldSucceed(existingDataset.Id);

        await Assert.That(api.DataConnectorId).IsEqualTo(Guid.Empty);
    }

    [Test]
    [DisplayName("Обновление коннектора на существующий должно быть успешным")]
    public async Task UpdateDataset_ChangingToExistingConnector_ShouldReturnSuccess()
    {
        var existingDataset = await CreateTestDatasetAsync(Guid.Empty);
        var updateReq = Request(existingDataset.Id) with
        {
            Name = existingDataset.Name,
            ConnectorId = App.Data.ExistingDataConnector.Id,
        };

        await Endpoint<UpdateDataSetResponse>(updateReq).IsSuccess();
        var api = await GetByIdShouldSucceed(existingDataset.Id);

        await Assert.That(api.DataConnectorId).IsEqualTo(App.Data.ExistingDataConnector.Id);
    }

    [Test]
    [DisplayName("Обновление набора с валидным коннектором и трансформациями должно быть успешным")]
    public async Task UpdateDataset_WithValidConnectorAndTransformations_ShouldReturnSuccess()
    {
        var existingDataset = await CreateTestDatasetAsync(Guid.Empty);

        var updateReq = Request(existingDataset.Id) with
        {
            ConnectorId = App.Data.ExistingDataConnector.Id,
            Transformations =
            [
                new OrderByTransformation
                {
                    Items =
                    [
                        "field".Desc(),
                    ],
                }.Block(),
            ],
        };

        await Endpoint<UpdateDataSetResponse>(updateReq).IsSuccess();
        var api = await GetByIdShouldSucceed(existingDataset.Id);

        await Assert.That(api.DataConnectorId).IsEqualTo(App.Data.ExistingDataConnector.Id);
    }

    [Test]
    [DisplayName("Обновление набора с тем же коннектором должно быть успешным")]
    public async Task UpdateDataset_WithSameConnector_ShouldReturnSuccess()
    {
        var existingDataset = await CreateTestDatasetAsync(App.Data.ExistingDataConnector.Id);
        var updateReq = Request(existingDataset.Id) with
        {
            ConnectorId = App.Data.ExistingDataConnector.Id,
        };

        await Endpoint<UpdateDataSetResponse>(updateReq).IsSuccess();
        var api = await GetByIdShouldSucceed(existingDataset.Id);

        await Assert.That(api.DataConnectorId).IsEqualTo(App.Data.ExistingDataConnector.Id);
    }

    [Test]
    [DisplayName("Обновление набора должно полностью заменить список трансформаций")]
    public async Task UpdateDataset_ShouldReplaceTransformationsInStorage()
    {
        var originalTransformations = new List<TransformationBlock>
        {
            new SelectTransformation
            {
                Items =
                [
                    "id".As("id"),
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

        var updateReq = Request(existingDataset.Id) with
        {
            Name = existingDataset.Name,
            ConnectorId = App.Data.ExistingDataConnector.Id,
            Transformations =
            [
                new SelectTransformation
                {
                    Items =
                    [
                        "id * 2".As("id2"),
                    ],
                    RestOptions = SelectRestOptions.Delete,
                }.Block(),
            ],
        };

        var res = await Endpoint<UpdateDataSetResponse>(updateReq).IsSuccess();
        var stored = await GetTransformationsAsync(existingDataset.Id);

        await Assert.That(res.Transformations.Count).IsEqualTo(1);
        await Assert.That(stored.Count).IsEqualTo(1);
        await Assert.That((int)stored[0].Order).IsEqualTo(0);
        await Assert.That(stored[0].Data is SelectTransformation).IsTrue();
    }

    [Test]
    [DisplayName("Обновление набора с пустыми трансформациями должно очистить старые")]
    public async Task UpdateDataset_WithEmptyTransformations_ShouldDeleteExistingTransformations()
    {
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

        var updateReq = Request(existingDataset.Id) with
        {
            Name = existingDataset.Name,
            ConnectorId = App.Data.ExistingDataConnector.Id,
        };

        var res = await Endpoint<UpdateDataSetResponse>(updateReq).IsSuccess();
        var stored = await GetTransformationsAsync(existingDataset.Id);

        await Assert.That(res.Transformations.Count).IsEqualTo(0);
        await Assert.That(stored.Count).IsEqualTo(0);
    }

    [Test]
    [DisplayName("Update с payload из GetById должен сохранять трансформации")]
    public async Task UpdateDataset_WithRawPayloadFromGetById_ShouldKeepTransformations()
    {
        var originalTransformations = new List<TransformationBlock>
        {
            new WhereTransformation { Condition = "id > 0" }.Block(),
            new SelectTransformation
            {
                Items =
                [
                    "id".As("id"),
                ],
                RestOptions = SelectRestOptions.Delete,
            }.Block(),
        };

        var existingDataset = await CreateTestDatasetAsync(
            connectorId: App.Data.ExistingDataConnector.Id,
            transformations: originalTransformations);

        var existing = await GetByIdShouldSucceed(existingDataset.Id);
        await Assert.That(existing.Transformations.Count).IsEqualTo(2);

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

        var updateRsp = await App.Client.PutAsJsonAsync($"/api/datasets/{existingDataset.Id}", payload);
        var afterUpdate = await GetByIdShouldSucceed(existingDataset.Id);
        var stored = await FindDataset(existingDataset.Id);

        await Assert.That(updateRsp.IsSuccessStatusCode).IsTrue();
        await Assert.That(afterUpdate.Transformations.Count).IsEqualTo(2);
        await Assert.That(stored).IsNotNull();
        await Assert.That(stored!.Name).IsEqualTo($"{existing.Name}_updated");
    }
}

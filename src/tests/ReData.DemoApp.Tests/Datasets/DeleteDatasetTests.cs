using System.Linq.Expressions;
using System.Net;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using ReData.DemoApp.Database.Entities;
using ReData.DemoApp.Endpoints.Datasets;
using ReData.DemoApp.Endpoints.Datasets.Create;
using ReData.DemoApp.Endpoints.Datasets.Delete;
using TUnit.Core;

namespace ReData.DemoApp.Tests.Datasets;

public class DeleteDatasetTests
    : DatasetTestBase
{
    private static string FakeDatasetName() => $"dataset{Guid.NewGuid().ToString("N")[..6]}";

    private static DeleteDataSetRequest Request(Guid id) => new()
    {
        Id = id,
    };

    private Task<HttpResponseMessage> Endpoint(DeleteDataSetRequest req) =>
        App.Client.DELETEAsync<DeleteDatasetEndpoint, DeleteDataSetRequest>(req);

    private async Task<CreateDataSetResponse> CreateTestDatasetAsync(Guid? connectorId = null, string? name = null)
    {
        var req = new CreateDataSetRequest
        {
            Name = name ?? FakeDatasetName(),
            Transformations = [],
            ConnectorId = connectorId ?? Guid.Empty,
        };

        return await App.Client.POSTAsync<CreateDatasetEndpoint, CreateDataSetRequest, CreateDataSetResponse>(req).IsSuccess();
    }

    private async Task<bool> DatasetExists(Expression<Func<DataSetEntity, bool>> predicate)
    {
        return await Db.DataSets.AsNoTracking().AnyAsync(predicate);
    }

    [Test]
    [DisplayName("Удаление существующего набора должно вернуть успех")]
    public async Task DeleteDataset_WithExistingId_ShouldReturnSuccess()
    {
        var existingDataset = await CreateTestDatasetAsync();
        var req = Request(existingDataset.Id);

        var rsp = await Endpoint(req);

        await Assert.That(rsp.IsSuccessStatusCode).IsTrue();
        await Assert.That(await DatasetExists(ds => ds.Id == existingDataset.Id)).IsFalse();
    }

    [Test]
    [DisplayName("Удаление набора с существующим коннектором должно быть успешным")]
    public async Task DeleteDataset_WithExistingConnector_ShouldReturnSuccess()
    {
        var existingDataset = await CreateTestDatasetAsync(App.Data.ExistingDataConnector.Id);
        var req = Request(existingDataset.Id);

        var rsp = await Endpoint(req);

        await Assert.That(rsp.IsSuccessStatusCode).IsTrue();
        await Assert.That(await DatasetExists(ds => ds.Id == existingDataset.Id)).IsFalse();
    }

    [Test]
    [DisplayName("Удаление несуществующего набора должно вернуть NotFound")]
    public async Task DeleteDataset_WithNonExistentId_ShouldReturnNotFound()
    {
        var nonExistentId = Guid.NewGuid();
        var req = Request(nonExistentId);

        var rsp = await Endpoint(req);

        await Assert.That(rsp.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    [DisplayName("Последовательное удаление одного набора дважды должно вернуть NotFound при втором запросе")]
    public async Task DeleteDataset_Twice_ShouldReturnNotFoundOnSecondAttempt()
    {
        var existingDataset = await CreateTestDatasetAsync();
        var req = Request(existingDataset.Id);

        var firstRsp = await Endpoint(req);
        await Assert.That(firstRsp.IsSuccessStatusCode).IsTrue();

        var secondRsp = await Endpoint(req);

        await Assert.That(secondRsp.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        await Assert.That(await DatasetExists(ds => ds.Id == existingDataset.Id)).IsFalse();
    }

    [Test]
    [DisplayName("Удаление набора не должно влиять на другие наборы")]
    public async Task DeleteDataset_ShouldNotAffectOtherDatasets()
    {
        var dataset1 = await CreateTestDatasetAsync();
        var dataset2 = await CreateTestDatasetAsync();
        var dataset3 = await CreateTestDatasetAsync();

        var req = Request(dataset2.Id);

        var rsp = await Endpoint(req);

        await Assert.That(rsp.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(await DatasetExists(ds => ds.Id == dataset2.Id)).IsFalse();
        await Assert.That(await DatasetExists(ds => ds.Id == dataset1.Id)).IsTrue();
        await Assert.That(await DatasetExists(ds => ds.Id == dataset3.Id)).IsTrue();
    }

    [Test]
    [DisplayName("Удаление набора с пустым GUID должно вернуть NotFound")]
    public async Task DeleteDataset_WithEmptyGuid_ShouldReturnNotFound()
    {
        var req = Request(Guid.Empty);

        var rsp = await Endpoint(req);

        await Assert.That(rsp.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    [DisplayName("Удаление набора должно возвращать 200 OK при успехе")]
    public async Task DeleteDataset_ShouldReturn200OkOnSuccess()
    {
        var existingDataset = await CreateTestDatasetAsync();
        var req = Request(existingDataset.Id);

        var rsp = await Endpoint(req);

        await Assert.That(rsp.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(await DatasetExists(ds => ds.Id == existingDataset.Id)).IsFalse();
    }
}


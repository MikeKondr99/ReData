using System.Net;
using FastEndpoints;
using ReData.DemoApp.Endpoints.Datasets;
using ReData.DemoApp.Endpoints.Datasets.Create;
using ReData.DemoApp.Endpoints.Datasets.Delete;
using ReData.DemoApp.Endpoints.Datasets.GetById;
using ReData.DemoApp.Transformations;
using TUnit.Core;

namespace ReData.DemoApp.Tests.Datasets;

public class GetDatasetByIdTests
    : DatasetTestBase
{
    private static string FakeDatasetName() => $"dataset{Guid.NewGuid().ToString("N")[..6]}";

    private static GetDatasetByIdRequest Request(Guid id) => new()
    {
        Id = id,
    };

    private Task<TestResult<DataSetResponse>> Endpoint(GetDatasetByIdRequest req) =>
        App.Client.GETAsync<GetDatasetByIdEndpoint, GetDatasetByIdRequest, DataSetResponse>(req);

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

        return await App.Client.POSTAsync<CreateDatasetEndpoint, CreateDataSetRequest, CreateDataSetResponse>(req).IsSuccess();
    }

    private static async Task AssertTransformationsEquivalent(
        IReadOnlyList<TransformationBlockResponse> actual,
        IReadOnlyList<TransformationBlock> expected)
    {
        await Assert.That(actual.Count).IsEqualTo(expected.Count);

        for (var i = 0; i < expected.Count; i++)
        {
            var expectedBlock = expected[i];
            var actualBlock = actual[i];

            await Assert.That(actualBlock.Enabled).IsEqualTo(expectedBlock.Enabled);
            await Assert.That(actualBlock.Transformation.GetType() == expectedBlock.Transformation.GetType()).IsTrue();

            switch (expectedBlock.Transformation)
            {
                case WhereTransformation expectedWhere:
                {
                    var actualWhere = (WhereTransformation)actualBlock.Transformation;
                    await Assert.That(actualWhere.Condition).IsEqualTo(expectedWhere.Condition);
                    break;
                }
                case SelectTransformation expectedSelect:
                {
                    var actualSelect = (SelectTransformation)actualBlock.Transformation;
                    await Assert.That(actualSelect.RestOptions).IsEqualTo(expectedSelect.RestOptions);
                    var actualItemsCount = actualSelect.Items.Count();
                    var expectedItemsCount = expectedSelect.Items.Count();
                    await Assert.That(actualItemsCount).IsEqualTo(expectedItemsCount);

                    for (var j = 0; j < expectedItemsCount; j++)
                    {
                        await Assert.That(actualSelect.Items[j].Field).IsEqualTo(expectedSelect.Items[j].Field);
                        await Assert.That(actualSelect.Items[j].Expression).IsEqualTo(expectedSelect.Items[j].Expression);
                    }

                    break;
                }
                case OrderByTransformation expectedOrderBy:
                {
                    var actualOrderBy = (OrderByTransformation)actualBlock.Transformation;
                    var actualItemsCount = actualOrderBy.Items.Count();
                    var expectedItemsCount = expectedOrderBy.Items.Count();
                    await Assert.That(actualItemsCount).IsEqualTo(expectedItemsCount);

                    for (var j = 0; j < expectedItemsCount; j++)
                    {
                        await Assert.That(actualOrderBy.Items[j].Expression).IsEqualTo(expectedOrderBy.Items[j].Expression);
                        await Assert.That(actualOrderBy.Items[j].Descending).IsEqualTo(expectedOrderBy.Items[j].Descending);
                    }

                    break;
                }
                default:
                    throw new InvalidOperationException($"Unsupported transformation type: {expectedBlock.Transformation.GetType().Name}");
            }
        }
    }

    [Test]
    [DisplayName("Получение существующего набора по ID должно вернуть данные")]
    public async Task GetDatasetById_WithExistingId_ShouldReturnDataset()
    {
        var existingDataset = await CreateTestDatasetAsync();
        var req = Request(existingDataset.Id);

        var res = await Endpoint(req).IsSuccess();

        await Assert.That(res.Id).IsEqualTo(existingDataset.Id);
        await Assert.That(res.Name).IsEqualTo(existingDataset.Name);
        await Assert.That(res.DataConnectorId).IsEqualTo(Guid.Empty);
        await Assert.That(res.Transformations.Count).IsEqualTo(0);
    }

    [Test]
    [DisplayName("Получение набора с существующим коннектором должно вернуть данные")]
    public async Task GetDatasetById_WithExistingConnector_ShouldReturnDataset()
    {
        var existingDataset = await CreateTestDatasetAsync(App.Data.ExistingDataConnector.Id);
        var req = Request(existingDataset.Id);

        var res = await Endpoint(req).IsSuccess();

        await Assert.That(res.Id).IsEqualTo(existingDataset.Id);
        await Assert.That(res.Name).IsEqualTo(existingDataset.Name);
        await Assert.That(res.DataConnectorId).IsEqualTo(App.Data.ExistingDataConnector.Id);
        await Assert.That(res.Transformations.Count).IsEqualTo(0);
    }

    [Test]
    [DisplayName("Получение набора с трансформациями должно вернуть все трансформации")]
    public async Task GetDatasetById_WithTransformations_ShouldReturnTransformations()
    {
        TransformationBlock[] transformations =
        [
            new WhereTransformation
            {
                Condition = "true",
            }.Block(),
            new SelectTransformation
            {
                Items =
                [
                    "id".As("Id"),
                ],
            }.Block(enabled: false),
        ];

        var existingDataset = await CreateTestDatasetAsync(transformations: transformations);
        var req = Request(existingDataset.Id);

        var res = await Endpoint(req).IsSuccess();

        await Assert.That(res.Id).IsEqualTo(existingDataset.Id);
        await AssertTransformationsEquivalent(res.Transformations, transformations);
    }

    [Test]
    [DisplayName("Получение несуществующего набора должно вернуть NotFound")]
    public async Task GetDatasetById_WithNonExistentId_ShouldReturnNotFound()
    {
        var req = Request(Guid.NewGuid());

        var (rsp, _) = await Endpoint(req);

        await Assert.That(rsp.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    [DisplayName("Получение набора с пустым GUID должно вернуть NotFound")]
    public async Task GetDatasetById_WithEmptyGuid_ShouldReturnNotFound()
    {
        var req = Request(Guid.Empty);

        var (rsp, _) = await Endpoint(req);

        await Assert.That(rsp.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    [DisplayName("Получение удаленного набора должно вернуть NotFound")]
    public async Task GetDatasetById_AfterDeletion_ShouldReturnNotFound()
    {
        var existingDataset = await CreateTestDatasetAsync();

        var deleteRsp = await App.Client.DELETEAsync<DeleteDatasetEndpoint, DeleteDataSetRequest>(new DeleteDataSetRequest
        {
            Id = existingDataset.Id,
        });
        await Assert.That(deleteRsp.IsSuccessStatusCode).IsTrue();

        var req = Request(existingDataset.Id);

        var (rsp, _) = await Endpoint(req);

        await Assert.That(rsp.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    [DisplayName("Получение набора должно вернуть все поля корректно")]
    public async Task GetDatasetById_ShouldReturnAllFieldsCorrectly()
    {
        var datasetName = FakeDatasetName();
        TransformationBlock[] transformations =
        [
            new WhereTransformation
            {
                Condition = "age > 18",
            }.Block(),
            new OrderByTransformation
            {
                Items =
                [
                    "name".Asc(),
                ],
            }.Block(enabled: false),
        ];

        var existingDataset = await CreateTestDatasetAsync(
            connectorId: App.Data.ExistingDataConnector.Id,
            name: datasetName,
            transformations: transformations);

        var req = Request(existingDataset.Id);

        var (rsp, res) = await Endpoint(req);

        await Assert.That(rsp.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(res.Id).IsEqualTo(existingDataset.Id);
        await Assert.That(res.Name).IsEqualTo(datasetName);
        await Assert.That(res.DataConnectorId).IsEqualTo(App.Data.ExistingDataConnector.Id);
        await AssertTransformationsEquivalent(res.Transformations, transformations);
    }

    [Test]
    [DisplayName("Получение набора с пустым коннектором должно вернуть корректные данные")]
    public async Task GetDatasetById_WithEmptyConnector_ShouldReturnCorrectData()
    {
        var existingDataset = await CreateTestDatasetAsync(Guid.Empty);
        var req = Request(existingDataset.Id);

        var (rsp, res) = await Endpoint(req);

        await Assert.That(rsp.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(res.Id).IsEqualTo(existingDataset.Id);
        await Assert.That(res.DataConnectorId).IsEqualTo(Guid.Empty);
        await Assert.That(res.Transformations.Count).IsEqualTo(0);
    }
}


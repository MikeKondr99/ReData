using System.Net;
using FastEndpoints;
using ReData.DemoApp.Endpoints.Datasets;
using ReData.DemoApp.Endpoints.Datasets.Create;
using ReData.DemoApp.Endpoints.Datasets.Delete;
using ReData.DemoApp.Endpoints.Datasets.GetAll;
using TUnit.Core;

namespace ReData.DemoApp.TUnit.Datasets;

public class GetAllDatasetsTests
{
    [ClassDataSource<DefaultReDataApp>(Shared = SharedType.PerTestSession)]
    public required DefaultReDataApp App { get; init; }

    private static string FakeDatasetName() => $"dataset{Guid.NewGuid().ToString("N")[..6]}";

    private Task<TestResult<List<DataSetListItem>>> Endpoint() =>
        App.Client.GETAsync<GetAllDatasetsEndpoint, List<DataSetListItem>>();

    private async Task<CreateDataSetResponse> CreateTestDatasetAsync()
    {
        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations = [],
            ConnectorId = Guid.Empty,
        };

        return await App.Client.POSTAsync<CreateDatasetEndpoint, CreateDataSetRequest, CreateDataSetResponse>(req).IsSuccess();
    }

    [Test]
    [DisplayName("Получение всех наборов должно включать вновь созданные наборы")]
    public async Task GetAllDatasets_ShouldIncludeNewlyCreatedDatasets()
    {
        var dataset1 = await CreateTestDatasetAsync();
        var dataset2 = await CreateTestDatasetAsync();

        var res = await Endpoint().IsSuccess();

        await Assert.That(res.Any(d => d.Id == dataset1.Id)).IsTrue();
        await Assert.That(res.Any(d => d.Id == dataset2.Id)).IsTrue();
    }

    [Test]
    [DisplayName("Удаленный набор должен исчезать из списка")]
    public async Task DeletedDataset_ShouldDisappearFromList()
    {
        var dataset = await CreateTestDatasetAsync();

        var deleteRsp = await App.Client.DELETEAsync<DeleteDatasetEndpoint, DeleteDataSetRequest>(new DeleteDataSetRequest
        {
            Id = dataset.Id,
        });
        await Assert.That(deleteRsp.IsSuccessStatusCode).IsTrue();

        var (rsp, res) = await Endpoint();

        await Assert.That(rsp.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(res.Any(d => d.Id == dataset.Id)).IsFalse();
    }
}

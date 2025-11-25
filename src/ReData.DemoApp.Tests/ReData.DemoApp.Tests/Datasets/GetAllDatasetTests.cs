using ReData.DemoApp.Endpoints.Datasets;
using ReData.DemoApp.Endpoints.Datasets.Create;
using ReData.DemoApp.Endpoints.Datasets.CreateDataset;
using ReData.DemoApp.Endpoints.Datasets.Delete;
using ReData.DemoApp.Endpoints.Datasets.GetAll;
using ReData.DemoApp.Transformations;

namespace ReData.DemoApp.Tests.Datasets;

public class GetAllDatasetsTests(App App) : RollbackTestBase<App>(App)
{
    private static string FakeDatasetName() => $"dataset{Guid.NewGuid().ToString("N")[..6]}";

    private async Task<CreateDataSetResponse> CreateTestDatasetAsync(string? name = null)
    {
        var req = new CreateDataSetRequest
        {
            Name = name ?? FakeDatasetName(),
            Transformations = [],
            ConnectorId = Guid.Empty,
        };

        var (rsp, res) = await App.Client.POSTAsync<CreateDatasetEndpoint, CreateDataSetRequest, CreateDataSetResponse>(req);
        rsp.IsSuccessStatusCode.Should().BeTrue();
        return res;
    }

    [Fact(DisplayName = "Получение всех наборов должно включать вновь созданные наборы")]
    public async Task GetAllDatasets_ShouldIncludeNewlyCreatedDatasets()
    {
        // Arrange - get baseline
        var (initialRsp, initialList) = await App.Client.GETAsync<GetAllDatasetsEndpoint, List<DataSetListItem>>();
        var initialCount = initialList.Count;

        // Create test datasets
        var dataset1 = await CreateTestDatasetAsync("test-getall-1");
        var dataset2 = await CreateTestDatasetAsync("test-getall-2");

        // Act
        var (rsp, res) = await App.Client.GETAsync<GetAllDatasetsEndpoint, List<DataSetListItem>>();

        // Assert
        rsp.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Should().Contain(d => d.Id == dataset1.Id);
        res.Should().Contain(d => d.Id == dataset2.Id);
    }

    [Fact(DisplayName = "Удаленный набор должен исчезать из списка")]
    public async Task DeletedDataset_ShouldDisappearFromList()
    {
        // Arrange
        var dataset = await CreateTestDatasetAsync("test-delete-from-list");
        
        // Verify it exists
        var (beforeRsp, beforeList) = await App.Client.GETAsync<GetAllDatasetsEndpoint, List<DataSetListItem>>();
        var countBefore = beforeList.Count;

        // Delete it
        var deleteRsp = await App.Client.DELETEAsync<DeleteDatasetEndpoint, DeleteDataSetRequest>(
            new DeleteDataSetRequest { Id = dataset.Id });
        deleteRsp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act
        var (afterRsp, afterList) = await App.Client.GETAsync<GetAllDatasetsEndpoint, List<DataSetListItem>>();

        // Assert
        afterRsp.StatusCode.Should().Be(HttpStatusCode.OK);
        afterList.Should().NotContain(d => d.Id == dataset.Id);
    }
}
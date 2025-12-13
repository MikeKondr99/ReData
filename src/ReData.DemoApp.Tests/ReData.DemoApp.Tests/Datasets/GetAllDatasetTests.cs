using ReData.DemoApp.Endpoints.Datasets;
using ReData.DemoApp.Endpoints.Datasets.Create;
using ReData.DemoApp.Endpoints.Datasets.CreateDataset;
using ReData.DemoApp.Endpoints.Datasets.Delete;
using ReData.DemoApp.Endpoints.Datasets.GetAll;
using ReData.DemoApp.Transformations;

namespace ReData.DemoApp.Tests.Datasets;

public class GetAllDatasetsTests(App App) : DemoAppTestBase<App>(App)
{
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

        var (rsp, res) = await App.Client.POSTAsync<CreateDatasetEndpoint, CreateDataSetRequest, CreateDataSetResponse>(req);
        rsp.Should().BeSuccessful();
        return res;
    }

    [Fact(DisplayName = "Получение всех наборов должно включать вновь созданные наборы")]
    public async Task GetAllDatasets_ShouldIncludeNewlyCreatedDatasets()
    {
        // Arrange
        var dataset1 = await CreateTestDatasetAsync();
        var dataset2 = await CreateTestDatasetAsync();

        // Act
        var (rsp, res) = await Endpoint();

        // Assert
        rsp.Should().BeSuccessful();
        res.Should().Contain(d => d.Id == dataset1.Id);
        res.Should().Contain(d => d.Id == dataset2.Id);
    }

    [Fact(DisplayName = "Удаленный набор должен исчезать из списка")]
    public async Task DeletedDataset_ShouldDisappearFromList()
    {
        // Arrange
        var dataset = await CreateTestDatasetAsync();

        // Delete it
        var deleteRsp = await App.Client.DELETEAsync<DeleteDatasetEndpoint, DeleteDataSetRequest>(
            new DeleteDataSetRequest { Id = dataset.Id });
        deleteRsp.Should().BeSuccessful();

        // Act
        var (rsp, res) = await Endpoint();

        // Assert
        rsp.Should().BeSuccessful();
        res.Should().NotContain(d => d.Id == dataset.Id);
    }
}
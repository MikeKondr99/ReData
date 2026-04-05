using ReData.DemoApp.Endpoints.Datasets;
using ReData.DemoApp.Endpoints.Datasets.Create;
using ReData.DemoApp.Endpoints.Datasets.Delete;
using ReData.DemoApp.Endpoints.Datasets.Update;
using ReData.DemoApp.Transformations;

namespace ReData.DemoApp.Tests.Datasets;

public class DeleteDatasetTests(App App) : DemoAppTestBase<App>(App)
{
    private static string FakeDatasetName() => $"dataset{Guid.NewGuid().ToString("N")[..6]}";
    
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

        var (rsp, res) = await App.Client.POSTAsync<CreateDatasetEndpoint, CreateDataSetRequest, CreateDataSetResponse>(req);
        rsp.IsSuccessStatusCode.Should().BeTrue();
        return res;
    }

    [Fact(DisplayName = "Удаление существующего набора должно вернуть успех")]
    public async Task DeleteDataset_WithExistingId_ShouldReturnSuccess()
    {
        // Arrange
        var existingDataset = await CreateTestDatasetAsync();
        var req = new DeleteDataSetRequest
        {
            Id = existingDataset.Id
        };

        // Act
        var rsp = await Endpoint(req);

        // Assert
        rsp.Should().BeSuccessful();
        Db.DataSets.Should().NotContain(ds => ds.Id == existingDataset.Id);
    }

    [Fact(DisplayName = "Удаление набора с существующим коннектором должно быть успешным")]
    public async Task DeleteDataset_WithExistingConnector_ShouldReturnSuccess()
    {
        // Arrange
        var existingDataset = await CreateTestDatasetAsync(App.Data.ExistingDataConnector.Id);
        var req = new DeleteDataSetRequest
        {
            Id = existingDataset.Id
        };

        // Act
        var rsp = await Endpoint(req);

        // Assert
        rsp.Should().BeSuccessful();
        Db.DataSets.Should().NotContain(ds => ds.Id == existingDataset.Id);
    }

    [Fact(DisplayName = "Удаление несуществующего набора должно вернуть NotFound")]
    public async Task DeleteDataset_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var req = new DeleteDataSetRequest
        {
            Id = nonExistentId
        };

        // Act
        var rsp = await Endpoint(req);

        // Assert
        rsp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Последовательное удаление одного набора дважды должно вернуть NotFound при втором запросе")]
    public async Task DeleteDataset_Twice_ShouldReturnNotFoundOnSecondAttempt()
    {
        // Arrange
        var existingDataset = await CreateTestDatasetAsync();
        var req = new DeleteDataSetRequest
        {
            Id = existingDataset.Id
        };

        // Act - First delete
        var firstRsp = await Endpoint(req);
        firstRsp.Should().BeSuccessful();

        // Act - Second delete
        var secondRsp = await Endpoint(req);

        // Assert
        secondRsp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        Db.DataSets.Should().NotContain(ds => ds.Id == existingDataset.Id);
    }

    [Fact(DisplayName = "Удаление набора не должно влиять на другие наборы")]
    public async Task DeleteDataset_ShouldNotAffectOtherDatasets()
    {
        // Arrange
        var dataset1 = await CreateTestDatasetAsync();
        var dataset2 = await CreateTestDatasetAsync();
        var dataset3 = await CreateTestDatasetAsync();

        var req = new DeleteDataSetRequest
        {
            Id = dataset2.Id
        };

        // Act
        var rsp = await Endpoint(req);

        // Assert
        rsp.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify only dataset2 was deleted
        Db.DataSets.Should().NotContain(ds => ds.Id == dataset2.Id);
        Db.DataSets.Should().Contain(ds => ds.Id == dataset1.Id);
        Db.DataSets.Should().Contain(ds => ds.Id == dataset3.Id);
    }

    [Fact(DisplayName = "Удаление набора с пустым GUID должно вернуть NotFound")]
    public async Task DeleteDataset_WithEmptyGuid_ShouldReturnNotFound()
    {
        // Arrange
        var req = new DeleteDataSetRequest
        {
            Id = Guid.Empty
        };

        // Act
        var rsp = await Endpoint(req);

        // Assert
        rsp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Удаление набора должно возвращать 200 OK при успехе")]
    public async Task DeleteDataset_ShouldReturn200OkOnSuccess()
    {
        // Arrange
        var existingDataset = await CreateTestDatasetAsync();
        var req = new DeleteDataSetRequest
        {
            Id = existingDataset.Id
        };

        // Act
        var rsp = await Endpoint(req);

        // Assert - Specifically check for 200 OK, not just success status
        rsp.StatusCode.Should().Be(HttpStatusCode.OK);
        Db.DataSets.Should().NotContain(ds => ds.Id == existingDataset.Id);
    }
}

using ReData.DemoApp.Endpoints.Datasets;
using ReData.DemoApp.Endpoints.Datasets.Create;
using ReData.DemoApp.Endpoints.Datasets.Export;
using ReData.DemoApp.Transformations;

namespace ReData.DemoApp.Tests.Datasets;

public class ExportDatasetTests(App App) : DemoAppTestBase<App>(App)
{
    private static string FakeDatasetName() => $"dataset{Guid.NewGuid().ToString("N")[..6]}";

    private Task<TestResult<ExportDatasetErrorResponse>> ExportEndpoint(ExportDataSetRequest req) =>
        App.Client.GETAsync<ExportDatasetEndpoint, ExportDataSetRequest, ExportDatasetErrorResponse>(req);

    [Fact(DisplayName = "Экспорт набора с невалидной трансформацией должен вернуть BadRequest вместо 500")]
    [Trait("Issue", "https://github.com/MikeKondr99/ReData/issues/112")]
    public async Task ExportDataset_WithInvalidTransformation_ShouldReturnBadRequest()
    {
        // Regression test for GH-112:
        // https://github.com/MikeKondr99/ReData/issues/112
        // Arrange
        var createReq = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            ConnectorId = App.Data.DataConnectors["numbers"].Id,
            Transformations =
            [
                new SelectTransformation
                {
                    Items =
                    [
                        new ReData.DemoApp.Transformations.SelectItem
                        {
                            Field = "id",
                            Expression = "id",
                        }
                    ],
                }.Block(),
                new WhereTransformation
                {
                    Condition = "((",
                }.Block(),
            ],
        };

        var (createRsp, createdDataset) =
            await App.Client.POSTAsync<CreateDatasetEndpoint, CreateDataSetRequest, CreateDataSetResponse>(createReq);
        createRsp.StatusCode.Should().Be(HttpStatusCode.Created);

        var exportReq = new ExportDataSetRequest
        {
            Id = createdDataset.Id,
            FileType = ExportFileType.Csv,
        };

        // Act
        var (exportRsp, exportError) = await ExportEndpoint(exportReq);

        // Assert
        exportRsp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        exportError.Index.Should().Be(1);
        exportError.Message.Should().NotBeNullOrWhiteSpace();
    }
}

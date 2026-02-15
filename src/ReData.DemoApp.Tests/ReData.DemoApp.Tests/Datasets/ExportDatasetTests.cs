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

    public static TheoryData<string, TransformationBlock, string> InvalidTransformationConfigurations => new()
    {
        {
            "groupBy.items = null",
            new GroupByTransformation
            {
                Groups =
                [
                    new ReData.DemoApp.Transformations.SelectItem
                    {
                        Field = "id",
                        Expression = "id",
                    }
                ],
                Items = null!,
            }.Block(),
            "\"items\":null"
        },
        {
            "groupBy.groups = null",
            new GroupByTransformation
            {
                Groups = null!,
                Items =
                [
                    new ReData.DemoApp.Transformations.SelectItem
                    {
                        Field = "id",
                        Expression = "id",
                    }
                ],
            }.Block(),
            "\"groups\":null"
        },
        {
            "orderBy.items = null",
            new OrderByTransformation
            {
                Items = null!,
            }.Block(),
            "\"items\":null"
        },
        {
            "select.items = null",
            new SelectTransformation
            {
                Items = null!,
            }.Block(),
            "\"items\":null"
        },
        {
            "select.items = []",
            new SelectTransformation
            {
                Items = [],
            }.Block(),
            "\"items\":[]"
        },
        {
            "orderBy.items = []",
            new OrderByTransformation
            {
                Items = [],
            }.Block(),
            "\"items\":[]"
        },
        {
            "where.condition = \"\"",
            new WhereTransformation
            {
                Condition = string.Empty,
            }.Block(),
            "\"condition\":\"\""
        },
    };

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

    [Theory(DisplayName = "Экспорт набора с невалидной трансформацией (null/empty) должен вернуть BadRequest вместо 500")]
    [Trait("Issue", "https://github.com/MikeKondr99/ReData/issues/116")]
    [Trait("Issue", "https://github.com/MikeKondr99/ReData/issues/117")]
    [MemberData(nameof(InvalidTransformationConfigurations))]
    public async Task ExportDataset_WithInvalidTransformationConfiguration_ShouldReturnBadRequest(
        string caseName,
        TransformationBlock transformation,
        string expectedNullJsonFragment)
    {
        // Regression test for GH-116:
        // https://github.com/MikeKondr99/ReData/issues/116
        // Regression test for GH-117:
        // https://github.com/MikeKondr99/ReData/issues/117
        // Arrange
        var createReq = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            ConnectorId = App.Data.DataConnectors["numbers"].Id,
            Transformations =
            [
                transformation,
            ],
        };

        var (createRsp, createdDataset) =
            await App.Client.POSTAsync<CreateDatasetEndpoint, CreateDataSetRequest, CreateDataSetResponse>(createReq);
        createRsp.StatusCode.Should().Be(HttpStatusCode.Created, caseName);

        var getRsp = await App.Client.GetAsync($"/api/datasets/{createdDataset.Id}");
        getRsp.StatusCode.Should().Be(HttpStatusCode.OK, caseName);
        var getBody = await getRsp.Content.ReadAsStringAsync();
        getBody.Should().Contain(expectedNullJsonFragment, caseName);

        var exportReq = new ExportDataSetRequest
        {
            Id = createdDataset.Id,
            FileType = ExportFileType.Csv,
        };

        // Act
        var (exportRsp, exportError) = await ExportEndpoint(exportReq);

        // Assert
        exportRsp.StatusCode.Should().Be(HttpStatusCode.BadRequest, caseName);
        exportError.Message.Should().NotBeNullOrWhiteSpace(caseName);
    }
}

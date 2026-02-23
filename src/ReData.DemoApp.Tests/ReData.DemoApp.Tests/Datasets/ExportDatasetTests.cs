using System.Text;
using Apache.Arrow;
using Apache.Arrow.Ipc;
using Apache.Arrow.Types;
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

    public static TheoryData<string, string> InvalidTransformationConfigurations => new()
    {
        {
            "groupBy.items = null",
            "\"items\":null"
        },
        {
            "groupBy.groups = null",
            "\"groups\":null"
        },
        {
            "orderBy.items = null",
            "\"items\":null"
        },
        {
            "select.items = null",
            "\"items\":null"
        },
        {
            "select.items = []",
            "\"items\":[]"
        },
        {
            "orderBy.items = []",
            "\"items\":[]"
        },
        {
            "where.condition = \"\"",
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
        string expectedJsonFragment)
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
                BuildInvalidTransformation(caseName),
            ],
        };

        var (createRsp, createdDataset) =
            await App.Client.POSTAsync<CreateDatasetEndpoint, CreateDataSetRequest, CreateDataSetResponse>(createReq);
        createRsp.StatusCode.Should().Be(HttpStatusCode.Created, caseName);

        var getRsp = await App.Client.GetAsync($"/api/datasets/{createdDataset.Id}");
        getRsp.StatusCode.Should().Be(HttpStatusCode.OK, caseName);
        var getBody = await getRsp.Content.ReadAsStringAsync();
        getBody.Should().Contain(expectedJsonFragment, caseName);

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

    [Fact]
    public async Task ExportDataset_AsArrow_ShouldReturnArrowFile()
    {
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
            ],
        };

        var (createRsp, createdDataset) =
            await App.Client.POSTAsync<CreateDatasetEndpoint, CreateDataSetRequest, CreateDataSetResponse>(createReq);
        createRsp.StatusCode.Should().Be(HttpStatusCode.Created);

        var rsp = await App.Client.GetAsync($"/api/datasets/{createdDataset.Id}/export?fileType=Arrow");

        rsp.StatusCode.Should().Be(HttpStatusCode.OK);
        rsp.Content.Headers.ContentType?.MediaType.Should().Be("application/vnd.apache.arrow.file");
        rsp.Content.Headers.ContentDisposition?.FileName.Should().Contain(".arrow");

        var bytes = await rsp.Content.ReadAsByteArrayAsync();
        bytes.Length.Should().BeGreaterThan(6);
        Encoding.ASCII.GetString(bytes[..6]).Should().Be("ARROW1");

        await using var ms = new MemoryStream(bytes);
        using var arrowReader = new ArrowFileReader(ms);
        using var batch = await arrowReader.ReadNextRecordBatchAsync();
        batch.Should().NotBeNull();
        batch!.Schema.GetFieldByName("id").Should().NotBeNull();
        batch.Schema.GetFieldByName("id")!.DataType.Should().BeOfType<Int64Type>();
    }

    private static TransformationBlock BuildInvalidTransformation(string caseName) => caseName switch
    {
        "groupBy.items = null" => new GroupByTransformation
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
        "groupBy.groups = null" => new GroupByTransformation
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
        "orderBy.items = null" => new OrderByTransformation
        {
            Items = null!,
        }.Block(),
        "select.items = null" => new SelectTransformation
        {
            Items = null!,
        }.Block(),
        "select.items = []" => new SelectTransformation
        {
            Items = [],
        }.Block(),
        "orderBy.items = []" => new OrderByTransformation
        {
            Items = [],
        }.Block(),
        "where.condition = \"\"" => new WhereTransformation
        {
            Condition = string.Empty,
        }.Block(),
        _ => throw new ArgumentOutOfRangeException(nameof(caseName), caseName, "Unknown test case"),
    };
}

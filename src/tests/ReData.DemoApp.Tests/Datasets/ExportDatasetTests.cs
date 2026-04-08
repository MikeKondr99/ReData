using System.Net;
using System.Text;
using Apache.Arrow.Ipc;
using Apache.Arrow.Types;
using ReData.DemoApp.Endpoints.Datasets;
using ReData.DemoApp.Endpoints.Datasets.Create;
using ReData.DemoApp.Endpoints.Datasets.Export;
using ReData.DemoApp.Transformations;
using TUnit.Core;

namespace ReData.DemoApp.Tests.Datasets;

public class ExportDatasetTests
    : DatasetTestBase
{
    private static string FakeDatasetName() => $"dataset{Guid.NewGuid().ToString("N")[..6]}";

    private static CreateDataSetRequest CreateRequest() => new()
    {
        Name = FakeDatasetName(),
        ConnectorId = Guid.Empty,
        Transformations = [],
    };

    private static ExportDataSetRequest ExportRequest(Guid id, ExportFileType fileType = ExportFileType.Csv) => new()
    {
        Id = id,
        FileType = fileType,
    };

    private Task<TestResult<ExportDatasetErrorResponse>> ExportEndpoint(ExportDataSetRequest req) =>
        App.Client.GETAsync<ExportDatasetEndpoint, ExportDataSetRequest, ExportDatasetErrorResponse>(req);

    [Test]
    [DisplayName("Экспорт набора с невалидной трансформацией должен вернуть BadRequest вместо 500")]
    public async Task ExportDataset_WithInvalidTransformation_ShouldReturnBadRequest()
    {
        // Regression test for GH-112: https://github.com/MikeKondr99/ReData/issues/112
        var createReq = CreateRequest() with
        {
            ConnectorId = App.Data.DataConnectors["numbers"].Id,
            Transformations =
            [
                new SelectTransformation
                {
                    Items =
                    [
                        "id".As("id"),
                    ],
                }.Block(),
                new WhereTransformation
                {
                    Condition = "((",
                }.Block(),
            ],
        };

        var createdDataset = await App.Client.POSTAsync<CreateDatasetEndpoint, CreateDataSetRequest, CreateDataSetResponse>(createReq).IsSuccess();

        var exportReq = ExportRequest(createdDataset.Id);

        var (exportRsp, exportError) = await ExportEndpoint(exportReq);

        await Assert.That(exportRsp.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(exportError.Index).IsEqualTo(1);
        await Assert.That(string.IsNullOrWhiteSpace(exportError.Message)).IsFalse();
    }

    [Test]
    [DisplayName("Экспорт набора с невалидной трансформацией (null/empty) должен вернуть BadRequest вместо 500")]
    [Arguments("groupBy.items = null", "\"items\":null")]
    [Arguments("groupBy.groups = null", "\"groups\":null")]
    [Arguments("orderBy.items = null", "\"items\":null")]
    [Arguments("select.items = null", "\"items\":null")]
    [Arguments("select.items = []", "\"items\":[]")]
    [Arguments("orderBy.items = []", "\"items\":[]")]
    [Arguments("where.condition = \"\"", "\"condition\":\"\"")]
    public async Task ExportDataset_WithInvalidTransformationConfiguration_ShouldReturnBadRequest(
        string caseName,
        string expectedJsonFragment)
    {
        // Regression test for GH-116: https://github.com/MikeKondr99/ReData/issues/116
        // Regression test for GH-117: https://github.com/MikeKondr99/ReData/issues/117
        var createReq = CreateRequest() with
        {
            ConnectorId = App.Data.DataConnectors["numbers"].Id,
            Transformations =
            [
                BuildInvalidTransformation(caseName),
            ],
        };

        var createdDataset = await App.Client.POSTAsync<CreateDatasetEndpoint, CreateDataSetRequest, CreateDataSetResponse>(createReq).IsSuccess();

        var getRsp = await App.Client.GetAsync($"/api/datasets/{createdDataset.Id}");
        await Assert.That(getRsp.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var getBody = await getRsp.Content.ReadAsStringAsync();
        await Assert.That(getBody.Contains(expectedJsonFragment)).IsTrue();

        var (exportRsp, exportError) = await ExportEndpoint(ExportRequest(createdDataset.Id));

        await Assert.That(exportRsp.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(string.IsNullOrWhiteSpace(exportError.Message)).IsFalse();
    }

    [Test]
    public async Task ExportDataset_AsArrow_ShouldReturnArrowFile()
    {
        var createReq = CreateRequest() with
        {
            ConnectorId = App.Data.DataConnectors["numbers"].Id,
            Transformations =
            [
                new SelectTransformation
                {
                    Items =
                    [
                        "id".As("id"),
                    ],
                }.Block(),
            ],
        };

        var createdDataset = await App.Client.POSTAsync<CreateDatasetEndpoint, CreateDataSetRequest, CreateDataSetResponse>(createReq).IsSuccess();

        var rsp = await App.Client.GetAsync($"/api/datasets/{createdDataset.Id}/export?fileType=Arrow");

        await Assert.That(rsp.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(rsp.Content.Headers.ContentType?.MediaType).IsEqualTo("application/vnd.apache.arrow.file");

        var bytes = await rsp.Content.ReadAsByteArrayAsync();
        await Assert.That(bytes.Length > 6).IsTrue();
        await Assert.That(Encoding.ASCII.GetString(bytes[..6])).IsEqualTo("ARROW1");

        await using var ms = new MemoryStream(bytes);
        using var arrowReader = new ArrowFileReader(ms);
        using var batch = await arrowReader.ReadNextRecordBatchAsync();

        await Assert.That(batch).IsNotNull();
        await Assert.That(batch!.Schema.GetFieldByName("id") is not null).IsTrue();
        await Assert.That(batch.Schema.GetFieldByName("id")!.DataType is Int64Type).IsTrue();
    }

    private static TransformationBlock BuildInvalidTransformation(string caseName) => caseName switch
    {
        "groupBy.items = null" => new GroupByTransformation
        {
            Groups =
            [
                "id".As("id"),
            ],
            Items = null!,
        }.Block(),
        "groupBy.groups = null" => new GroupByTransformation
        {
            Groups = null!,
            Items =
            [
                "id".As("id"),
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


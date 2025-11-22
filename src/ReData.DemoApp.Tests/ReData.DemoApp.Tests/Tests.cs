using System.Text.Json;
using FluentValidation.Results;
using ReData.DemoApp.Database;
using ReData.DemoApp.Endpoints.Datasets;
using ReData.DemoApp.Endpoints.DataSets;
using ReData.DemoApp.Endpoints.Functions;

namespace ReData.DemoApp.Tests;

using Endpoint = DemoApp.Endpoints.Functions.Endpoint;

public class Tests(App App) : TestBase<App>
{
    private static string FakeDatasetName() => $"dataset{Guid.NewGuid().ToString("N")[..6]}";

    [Fact(DisplayName = "Создание набора с верными данными должно вернуть 'создано'")]
    public async Task CreateDataset_WithValidData_ShouldReturnCreated()
    {
        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations = [],
            ConnectorId = Guid.Empty,
        };
        var (rsp, res) = await App.Client.POSTAsync<CreateEndpoint, CreateDataSetRequest, CreateDataSetResponse>(req);
        rsp.IsSuccessStatusCode.Should().BeTrue();
        res.Name.Should().BeEquivalentTo(req.Name);
        res.Id.Should().NotBeEmpty();


        var db = App.Services.GetRequiredService<ApplicationDatabaseContext>();
        var dataset = db.DataSets.FirstOrDefault(ds => ds.Id == res.Id);
        dataset.Should().NotBeNull();
    }

    [Fact(DisplayName = "Создание набора с пустым именем должно вернуть ошибку валидации")]
    public async Task CreateDataset_WithEmptyName_ShouldReturnValidatinoError()
    {
        var req = new CreateDataSetRequest
        {
            Name = "",
            ConnectorId = Guid.Empty,
            Transformations = []
        };
        var rsp = await App.Client.POSTAsync<CreateEndpoint, CreateDataSetRequest>(req);
        rsp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Создание набора с null именем должно вернуть ошибку валидации")]
    public async Task CreateDataset_WithNullName_ShouldReturnValidationError()
    {
        var req = new CreateDataSetRequest
        {
            Name = null!,
            Transformations = [],
            ConnectorId = Guid.Empty,
        };
        var rsp = await App.Client.POSTAsync<CreateEndpoint, CreateDataSetRequest>(req);
        rsp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Создание набора с слишком коротким именем должно вернуть ошибку валидации")]
    public async Task CreateDataset_WithShortName_ShouldReturnValidationError()
    {
        var req = new CreateDataSetRequest
        {
            Name = "x",
            Transformations = [],
            ConnectorId = Guid.Empty
        };
        var (rsp, res) = await App.Client.POSTAsync<CreateEndpoint, CreateDataSetRequest, ErrorResponse>(req);
        rsp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        res.Errors.Keys.Should().Contain("name");

        var db = App.Services.GetRequiredService<ApplicationDatabaseContext>();
        var dataset = db.DataSets.FirstOrDefault(ds => ds.Name == req.Name);
        dataset.Should().BeNull();
    }

    [Fact(DisplayName = "Создание набора с не уникальным именем должно вернуть конфликт")]
    public async Task CreateDataset_WithNotUniqueName_ShouldReturnConflict()
    {
        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations = [],
            ConnectorId = Guid.Empty,
        };
        var (rsp, res) = await App.Client.POSTAsync<CreateEndpoint, CreateDataSetRequest, CreateDataSetResponse>(req);
        rsp.IsSuccessStatusCode.Should().BeTrue();

        rsp = await App.Client.POSTAsync<CreateEndpoint, CreateDataSetRequest>(req);
        rsp.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact(DisplayName = "Создание набора с null трансформациями должно вернуть ошибку валидации")]
    public async Task CreateDataset_WithNullTransformations_ShouldReturnValidationError()
    {
        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations = null,
            ConnectorId = Guid.Empty,
        };
        var rsp = await App.Client.POSTAsync<CreateEndpoint, CreateDataSetRequest>(req);
        rsp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
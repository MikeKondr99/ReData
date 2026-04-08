using System.Linq.Expressions;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using ReData.DemoApp.Database.Entities;
using ReData.DemoApp.Endpoints.Datasets;
using ReData.DemoApp.Endpoints.Datasets.Create;
using ReData.DemoApp.Transformations;
using ReData.Query.Core.Types;
using TUnit.Core;

namespace ReData.DemoApp.TUnit.Datasets;

public class CreateDatasetTests
    : DatasetTestBase
{
    private static DataSetField TextField(string name) => Field(name, DataType.Text);
    private static DataSetField IntField(string name) => Field(name, DataType.Integer);
    private static DataSetField BoolField(string name) => Field(name, DataType.Bool);

    private static DataSetField Field(string name, DataType type, bool canBeNull = true) => new()
    {
        Alias = name,
        DataType = type,
        CanBeNull = canBeNull,
    };

    private Task<TestResult<CreateDataSetResponse>> Endpoint(CreateDataSetRequest req) =>
        App.Client.POSTAsync<CreateDatasetEndpoint, CreateDataSetRequest, CreateDataSetResponse>(req);

    private Task<TestResult<ErrorResponse>> EndpointError(CreateDataSetRequest req) =>
        App.Client.POSTAsync<CreateDatasetEndpoint, CreateDataSetRequest, ErrorResponse>(req);

    private CreateDataSetRequest Request() => new()
    {
        ConnectorId = Guid.Empty,
        Name = $"dataset{Guid.NewGuid().ToString("N")[..6]}",
        Transformations = [],
    };

    private async Task<bool> DatasetExists(Expression<Func<DataSetEntity, bool>> predicate)
    {
        return await Db.DataSets.AsNoTracking().AnyAsync(predicate);
    }

    private async Task<DataSetEntity?> FindDataset(Guid id)
    {
        return await Db.DataSets.AsNoTracking().FirstOrDefaultAsync(ds => ds.Id == id);
    }

    private static async Task AssertFieldListEquivalent(IReadOnlyCollection<DataSetField>? actual, params DataSetField[] expected)
    {
        await Assert.That(actual).IsNotNull();
        var actualFields = actual!;
        await Assert.That(actualFields.Count).IsEqualTo(expected.Length);

        foreach (var expectedField in expected)
        {
            var exists = actualFields.Any(actualField =>
                actualField.Alias == expectedField.Alias &&
                actualField.DataType == expectedField.DataType &&
                actualField.CanBeNull == expectedField.CanBeNull);

            await Assert.That(exists).IsTrue();
        }
    }

    [Test]
    [DisplayName("РЎРѕР·РґР°РЅРёРµ РЅР°Р±РѕСЂР° СЃ РІРµСЂРЅС‹РјРё РґР°РЅРЅС‹РјРё РґРѕР»Р¶РЅРѕ РІРµСЂРЅСѓС‚СЊ 'СѓСЃРїРµС…'")]
    public async Task CreateDataset_WithValidData_ShouldReturnCreated()
    {
        var req = Request();

        var res = await Endpoint(req).IsSuccess();

        await Assert.That(res.Name).IsEqualTo(req.Name);
        await Assert.That(res.Id).IsNotEmptyGuid();
        await Assert.That(await DatasetExists(ds => ds.Id == res.Id)).IsTrue();
    }

    [Test]
    [DisplayName("РЎРѕР·РґР°РЅРёРµ РЅР°Р±РѕСЂР° СЃ РїСѓСЃС‚С‹Рј РёРјРµРЅРµРј РґРѕР»Р¶РЅРѕ РІРµСЂРЅСѓС‚СЊ РѕС€РёР±РєСѓ РІР°Р»РёРґР°С†РёРё")]
    public async Task CreateDataset_WithEmptyName_ShouldReturnValidatinoError()
    {
        var req = Request() with
        {
            Name = "",
        };

        var rsp = await EndpointError(req);

        await Assert.That(rsp).IsValidationError("name");
        await Assert.That(await DatasetExists(ds => ds.Name == req.Name)).IsFalse();
    }

    [Test]
    [DisplayName("РЎРѕР·РґР°РЅРёРµ РЅР°Р±РѕСЂР° СЃ null РёРјРµРЅРµРј РґРѕР»Р¶РЅРѕ РІРµСЂРЅСѓС‚СЊ РѕС€РёР±РєСѓ РІР°Р»РёРґР°С†РёРё")]
    public async Task CreateDataset_WithNullName_ShouldReturnValidationError()
    {
        var req = Request() with
        {
            Name = null!,
        };

        var rsp = await EndpointError(req);

        await Assert.That(rsp).IsValidationError("name");
        await Assert.That(await DatasetExists(ds => ds.Name == req.Name)).IsFalse();
    }

    [Test]
    [DisplayName("РЎРѕР·РґР°РЅРёРµ РЅР°Р±РѕСЂР° СЃ СЃР»РёС€РєРѕРј РєРѕСЂРѕС‚РєРёРј РёРјРµРЅРµРј РґРѕР»Р¶РЅРѕ РІРµСЂРЅСѓС‚СЊ РѕС€РёР±РєСѓ РІР°Р»РёРґР°С†РёРё")]
    public async Task CreateDataset_WithShortName_ShouldReturnValidationError()
    {
        var req = Request() with
        {
            Name = "x",
        };

        var rsp = await EndpointError(req);

        await Assert.That(rsp).IsValidationError("name");
        await Assert.That(await DatasetExists(ds => ds.Name == req.Name)).IsFalse();
    }

    [Test]
    [DisplayName("РЎРѕР·РґР°РЅРёРµ РЅР°Р±РѕСЂР° СЃ РЅРµ СѓРЅРёРєР°Р»СЊРЅС‹Рј РёРјРµРЅРµРј РґРѕР»Р¶РЅРѕ РІРµСЂРЅСѓС‚СЊ РѕС€РёР±РєСѓ РІР°Р»РёРґР°С†РёРё")]
    public async Task CreateDataset_WithNotUniqueName_ShouldReturnConflict()
    {
        var req = Request();
        _ = await Endpoint(req).IsSuccess();

        var response = await EndpointError(req);

        await Assert.That(response).IsValidationError("name");
    }

    [Test]
    [DisplayName("РЎРѕР·РґР°РЅРёРµ РЅР°Р±РѕСЂР° СЃ null С‚СЂР°РЅСЃС„РѕСЂРјР°С†РёСЏРјРё РґРѕР»Р¶РЅРѕ РІРµСЂРЅСѓС‚СЊ РѕС€РёР±РєСѓ РІР°Р»РёРґР°С†РёРё")]
    public async Task CreateDataset_WithNullTransformations_ShouldReturnValidationError()
    {
        var req = Request() with
        {
            Transformations = null!,
        };

        var rsp = await EndpointError(req);

        await Assert.That(rsp).IsValidationError("transformations");
    }

    [Test]
    [DisplayName("РЎРѕР·РґР°РЅРёРµ РЅР°Р±РѕСЂР° СЃ СЃСѓС‰РµСЃС‚РІСѓСЋС‰РёРј РєРѕРЅРЅРµРєС‚РѕСЂРѕРј РґРѕР»Р¶РЅРѕ Р±С‹С‚СЊ СѓСЃРїРµС€РЅС‹Рј")]
    public async Task CreateDataset_WithExistingConnector_ShouldReturnCreated()
    {
        var req = Request() with
        {
            ConnectorId = App.Data.ExistingDataConnector.Id,
        };

        var res = await Endpoint(req).IsSuccess();

        await Assert.That(res.Name).IsEqualTo(req.Name);
        await Assert.That(res.Id).IsNotEmptyGuid();
        await Assert.That(await DatasetExists(ds =>
            ds.Id == res.Id &&
            ds.DataConnectorId == App.Data.ExistingDataConnector.Id)).IsTrue();
    }

    [Test]
    [DisplayName("РЎРѕР·РґР°РЅРёРµ РЅР°Р±РѕСЂР° СЃ РЅРµСЃСѓС‰РµСЃС‚РІСѓСЋС‰РёРј РєРѕРЅРЅРµРєС‚РѕСЂРѕРј РґРѕР»Р¶РЅРѕ РІРµСЂРЅСѓС‚СЊ РѕС€РёР±РєСѓ РІР°Р»РёРґР°С†РёРё")]
    public async Task CreateDataset_WithNonExistentConnector_ShouldReturnValidationError()
    {
        var nonExistentConnectorId = Guid.NewGuid();
        var req = Request() with
        {
            ConnectorId = nonExistentConnectorId,
        };

        var rsp = await EndpointError(req);

        await Assert.That(rsp).IsValidationError("connectorId");
        await Assert.That(await DatasetExists(ds => ds.DataConnectorId == nonExistentConnectorId)).IsFalse();
    }

    [Test]
    [DisplayName("РЎРѕР·РґР°РЅРёРµ РЅР°Р±РѕСЂР° СЃ РїСѓСЃС‚С‹Рј GUID РєРѕРЅРЅРµРєС‚РѕСЂРѕРј РґРѕР»Р¶РЅРѕ Р±С‹С‚СЊ СѓСЃРїРµС€РЅС‹Рј")]
    public async Task CreateDataset_WithEmptyGuidConnector_ShouldReturnCreated()
    {
        var req = Request() with
        {
            ConnectorId = Guid.Empty,
        };

        var res = await Endpoint(req).IsSuccess();

        await Assert.That(await DatasetExists(ds =>
            ds.Id == res.Id &&
            ds.DataConnectorId == Guid.Empty)).IsTrue();
    }

    [Test]
    [DisplayName("РЎРѕР·РґР°РЅРёРµ РЅР°Р±РѕСЂР° СЃ РІР°Р»РёРґРЅС‹Рј РєРѕРЅРЅРµРєС‚РѕСЂРѕРј Рё С‚СЂР°РЅСЃС„РѕСЂРјР°С†РёСЏРјРё РґРѕР»Р¶РЅРѕ Р±С‹С‚СЊ СѓСЃРїРµС€РЅС‹Рј")]
    public async Task CreateDataset_WithValidConnectorAndTransformations_ShouldReturnCreated()
    {
        var req = Request() with
        {
            ConnectorId = App.Data.ExistingDataConnector.Id,
            Transformations =
            [
                new WhereTransformation
                {
                    Condition = "true",
                }.Block(),
            ],
        };

        var res = await Endpoint(req).IsSuccess();

        await Assert.That(await DatasetExists(ds =>
            ds.Id == res.Id &&
            ds.DataConnectorId == App.Data.ExistingDataConnector.Id)).IsTrue();
    }

    [Test]
    [DisplayName("РЎРѕР·РґР°РЅРёРµ РЅР°Р±РѕСЂР° РёР· РїСѓСЃС‚РѕРіРѕ РєРѕРЅРЅРµРєС‚РѕСЂР° РґРѕР»Р¶РЅРѕ СЃРѕС…СЂР°РЅРёС‚СЊ RowsCount = 0 РІ Р‘Р”")]
    [Skip("TODO: investigate parallel metadata race (RowsCount intermittently null/non-zero in parallel run)")]
    public async Task CreateDataset_FromEmptyConnector_ShouldSaveRowsCountZero()
    {
        var req = Request() with
        {
            ConnectorId = App.Data.DataConnectors["empty"].Id,
        };

        var res = await Endpoint(req).IsSuccess();

        await Assert.That(res.Id).IsNotEmptyGuid();

        var dataset = await FindDataset(res.Id);
        await Assert.That(dataset).IsNotNull();
        await Assert.That(dataset!.RowsCount).IsEqualTo(0);
        await AssertFieldListEquivalent(dataset.FieldList, TextField("empty"));
    }

    [Test]
    [DisplayName("РЎРѕР·РґР°РЅРёРµ РЅР°Р±РѕСЂР° РёР· РєРѕРЅРЅРµРєС‚РѕСЂР° СЃ РѕРґРЅРѕР№ СЃС‚СЂРѕРєРѕР№ РґРѕР»Р¶РЅРѕ СЃРѕС…СЂР°РЅРёС‚СЊ RowsCount = 1 РІ Р‘Р”")]
    public async Task CreateDataset_FromSingleRowConnector_ShouldSaveRowsCountOne()
    {
        var req = Request() with
        {
            ConnectorId = App.Data.DataConnectors["single"].Id,
        };

        var res = await Endpoint(req).IsSuccess();

        await Assert.That(res.Id).IsNotEmptyGuid();

        var dataset = await FindDataset(res.Id);
        await Assert.That(dataset).IsNotNull();
        await Assert.That(dataset!.RowsCount).IsEqualTo(1);
        await AssertFieldListEquivalent(dataset.FieldList, BoolField("single"));
    }

    [Test]
    [DisplayName("РЎРѕР·РґР°РЅРёРµ РЅР°Р±РѕСЂР° РёР· РєРѕРЅРЅРµРєС‚РѕСЂР° СЃ 1000 СЃС‚СЂРѕРє РґРѕР»Р¶РЅРѕ СЃРѕС…СЂР°РЅРёС‚СЊ RowsCount = 1000 РІ Р‘Р”")]
    public async Task CreateDataset_FromThousandRowsConnector_ShouldSaveRowsCountThousand()
    {
        var req = Request() with
        {
            ConnectorId = App.Data.DataConnectors["numbers"].Id,
        };

        var res = await Endpoint(req).IsSuccess();

        await Assert.That(res.Id).IsNotEmptyGuid();

        var dataset = await FindDataset(res.Id);
        await Assert.That(dataset).IsNotNull();
        await Assert.That(dataset!.RowsCount).IsEqualTo(1000);
        await AssertFieldListEquivalent(dataset.FieldList, IntField("id"));
    }

    [Test]
    [DisplayName("РЎРѕР·РґР°РЅРёРµ РЅР°Р±РѕСЂР° СЃ WHERE С‚СЂР°РЅСЃС„РѕСЂРјР°С†РёРµР№ С„РёР»СЊС‚СЂСѓСЋС‰РµР№ РІСЃРµ СЃС‚СЂРѕРєРё РґРѕР»Р¶РЅРѕ СЃРѕС…СЂР°РЅРёС‚СЊ RowsCount = 0 РІ Р‘Р”")]
    public async Task CreateDataset_WithWhereFilteringAllRows_ShouldSaveRowsCountZero()
    {
        var req = Request() with
        {
            ConnectorId = App.Data.DataConnectors["numbers"].Id,
            Transformations =
            [
                new WhereTransformation
                {
                    Condition = "Int(id) = 9999",
                }.Block(),
            ],
        };

        var res = await Endpoint(req).IsSuccess();

        await Assert.That(res.Id).IsNotEmptyGuid();

        var dataset = await FindDataset(res.Id);
        await Assert.That(dataset).IsNotNull();
        await Assert.That(dataset!.RowsCount).IsEqualTo(0);
        await AssertFieldListEquivalent(dataset.FieldList, IntField("id"));
    }

    [Test]
    [DisplayName("РЎРѕР·РґР°РЅРёРµ РЅР°Р±РѕСЂР° СЃ WHERE С‚СЂР°РЅСЃС„РѕСЂРјР°С†РёРµР№ С„РёР»СЊС‚СЂСѓСЋС‰РµР№ С‡Р°СЃС‚СЊ СЃС‚СЂРѕРє РґРѕР»Р¶РЅРѕ СЃРѕС…СЂР°РЅРёС‚СЊ С‡Р°СЃС‚РёС‡РЅС‹Р№ СЃС‡РµС‚ РІ Р‘Р”")]
    public async Task CreateDataset_WithWhereFilteringSomeRows_ShouldSavePartialCount()
    {
        var req = Request() with
        {
            Transformations =
            [
                new WhereTransformation
                {
                    Condition = "Int(id) <= 100",
                }.Block(),
            ],
            ConnectorId = App.Data.DataConnectors["numbers"].Id,
        };

        var res = await Endpoint(req).IsSuccess();

        await Assert.That(res.Id).IsNotEmptyGuid();

        var dataset = await FindDataset(res.Id);
        await Assert.That(dataset).IsNotNull();
        await Assert.That(dataset!.RowsCount).IsEqualTo(100);
        await AssertFieldListEquivalent(dataset.FieldList, IntField("id"));
    }

    [Test]
    [DisplayName("РЎРѕР·РґР°РЅРёРµ РЅР°Р±РѕСЂР° СЃ РЅРµСЃРєРѕР»СЊРєРёРјРё WHERE С‚СЂР°РЅСЃС„РѕСЂРјР°С†РёСЏРјРё РґРѕР»Р¶РЅРѕ СЃРѕС…СЂР°РЅРёС‚СЊ РєСѓРјСѓР»СЏС‚РёРІРЅС‹Р№ СЃС‡РµС‚ РІ Р‘Р”")]
    public async Task CreateDataset_WithMultipleWhereTransformations_ShouldSaveCumulativeCount()
    {
        var req = Request() with
        {
            ConnectorId = App.Data.DataConnectors["numbers"].Id,
            Transformations =
            [
                new WhereTransformation
                {
                    Condition = "Int(id) <= 500",
                }.Block(),
                new WhereTransformation
                {
                    Condition = "Int(id) >= 400",
                }.Block(),
            ],
        };

        var res = await Endpoint(req).IsSuccess();

        await Assert.That(res.Id).IsNotEmptyGuid();

        var dataset = await FindDataset(res.Id);
        await Assert.That(dataset).IsNotNull();
        await Assert.That(dataset!.RowsCount).IsEqualTo(101);
        await AssertFieldListEquivalent(dataset.FieldList, IntField("id"));
    }

    [Test]
    [DisplayName("РЎРѕР·РґР°РЅРёРµ РЅР°Р±РѕСЂР° СЃ WHERE Рё РѕС‚РєР»СЋС‡РµРЅРЅРѕР№ WHERE С‚СЂР°РЅСЃС„РѕСЂРјР°С†РёРµР№ РґРѕР»Р¶РЅРѕ РёРіРЅРѕСЂРёСЂРѕРІР°С‚СЊ РѕС‚РєР»СЋС‡РµРЅРЅСѓСЋ РІ Р‘Р”")]
    public async Task CreateDataset_WithEnabledAndDisabledWhere_ShouldSaveIgnoringDisabled()
    {
        var req = Request() with
        {
            ConnectorId = App.Data.DataConnectors["numbers"].Id,
            Transformations =
            [
                new WhereTransformation
                {
                    Condition = "Int(id) <= 100",
                }.Block(),
                new WhereTransformation
                {
                    Condition = "Int(id) >= 50",
                }.Block(enabled: false),
            ],
        };

        var res = await Endpoint(req).IsSuccess();

        await Assert.That(res.Id).IsNotEmptyGuid();

        var dataset = await FindDataset(res.Id);
        await Assert.That(dataset).IsNotNull();
        await Assert.That(dataset!.RowsCount).IsEqualTo(100);
        await AssertFieldListEquivalent(dataset.FieldList, IntField("id"));
    }

    [Test]
    [DisplayName("РЎРѕР·РґР°РЅРёРµ РЅР°Р±РѕСЂР° СЃ LIMIT С‚СЂР°РЅСЃС„РѕСЂРјР°С†РёРµР№ РґРѕР»Р¶РЅРѕ СЃРѕС…СЂР°РЅРёС‚СЊ СЃС‡РµС‚ Р»РёРјРёС‚РёСЂРѕРІР°РЅРЅС‹С… СЃС‚СЂРѕРє РІ Р‘Р”")]
    public async Task CreateDataset_WithLimitTransformation_ShouldSaveLimitedCount()
    {
        var req = Request() with
        {
            ConnectorId = App.Data.DataConnectors["numbers"].Id,
            Transformations =
            [
                new LimitOffsetTransformation
                {
                    Limit = 10,
                }.Block(),
            ],
        };

        var res = await Endpoint(req).IsSuccess();

        await Assert.That(res.Id).IsNotEmptyGuid();

        var dataset = await FindDataset(res.Id);
        await Assert.That(dataset).IsNotNull();
        await Assert.That(dataset!.RowsCount).IsEqualTo(10);
        await AssertFieldListEquivalent(dataset.FieldList, IntField("id"));
    }

    [Test]
    [DisplayName("COUNT(*) РґРѕР»Р¶РµРЅ СЃС‡РёС‚Р°С‚СЊ РІСЃРµ СЃС‚СЂРѕРєРё РІРєР»СЋС‡Р°СЏ NULL РІ РїРѕР»СЏС…")]
    public async Task CreateDataset_CountShouldIncludeNullRows()
    {
        var req = Request() with
        {
            ConnectorId = App.Data.DataConnectors["numbers"].Id,
            Transformations =
            [
                new SelectTransformation
                {
                    Items =
                    [
                        "If(id.Int().Mod(2) = 0, null, id)".As("ID"),
                    ],
                }.Block(),
            ],
        };

        var res = await Endpoint(req).IsSuccess();

        var dataset = await FindDataset(res.Id);
        await Assert.That(dataset).IsNotNull();
        await Assert.That(dataset!.RowsCount).IsEqualTo(1000);
        await AssertFieldListEquivalent(dataset.FieldList, IntField("ID"));
    }

    [Test]
    [DisplayName("SELECT СЃ Alt РґРѕР»Р¶РЅР° СЃРѕР·РґР°РІР°С‚СЊ РЅРµ-nullable РїРѕР»СЏ")]
    public async Task CreateDataset_WithSelectAlt_ShouldCreateNonNullableFields()
    {
        var req = Request() with
        {
            ConnectorId = App.Data.DataConnectors["test"].Id,
            Transformations =
            [
                new SelectTransformation
                {
                    Items =
                    [
                        "id.Alt(0)".As("not_null_id"),
                    ],
                }.Block(),
            ],
        };

        var res = await Endpoint(req).IsSuccess();

        var dataset = await FindDataset(res.Id);
        await Assert.That(dataset).IsNotNull();
        await Assert.That(dataset!.RowsCount).IsEqualTo(2);
        await AssertFieldListEquivalent(dataset.FieldList, Field("not_null_id", DataType.Integer, false));
    }

    [Test]
    [DisplayName("SELECT СЃ Int() РєР°СЃС‚РѕРј РґРѕР»Р¶РЅР° РёР·РјРµРЅСЏС‚СЊ С‚РёРї РїРѕР»СЏ РЅР° Int")]
    public async Task CreateDataset_WithIntCast_ShouldChangeToIntField()
    {
        var req = Request() with
        {
            ConnectorId = App.Data.DataConnectors["test"].Id,
            Transformations =
            [
                new SelectTransformation
                {
                    Items =
                    [
                        "Int(id)".As("id_int"),
                    ],
                }.Block(),
            ],
        };

        var res = await Endpoint(req).IsSuccess();

        var dataset = await FindDataset(res.Id);
        await Assert.That(dataset).IsNotNull();
        await AssertFieldListEquivalent(dataset!.FieldList, Field("id_int", DataType.Integer));
    }

    [Test]
    [DisplayName("SELECT СЃ Num() РєР°СЃС‚РѕРј РґРѕР»Р¶РЅР° РёР·РјРµРЅСЏС‚СЊ С‚РёРї РїРѕР»СЏ РЅР° Num")]
    public async Task CreateDataset_WithNumCast_ShouldChangeToNumField()
    {
        var req = Request() with
        {
            ConnectorId = App.Data.DataConnectors["test"].Id,
            Transformations =
            [
                new SelectTransformation
                {
                    Items =
                    [
                        "Num(id)".As("id_num"),
                    ],
                }.Block(),
            ],
        };

        var res = await Endpoint(req).IsSuccess();

        var dataset = await FindDataset(res.Id);
        await Assert.That(dataset).IsNotNull();
        await AssertFieldListEquivalent(dataset!.FieldList, Field("id_num", DataType.Number));
    }

    [Test]
    [DisplayName("SELECT СЃ Date() РєР°СЃС‚РѕРј РґРѕР»Р¶РЅР° РёР·РјРµРЅСЏС‚СЊ С‚РёРї РїРѕР»СЏ РЅР° Date")]
    public async Task CreateDataset_WithDateCast_ShouldChangeToDateField()
    {
        var req = Request() with
        {
            ConnectorId = App.Data.DataConnectors["test"].Id,
            Transformations =
            [
                new SelectTransformation
                {
                    Items =
                    [
                        "Date(id)".As("id_date"),
                    ],
                }.Block(),
            ],
        };

        var res = await Endpoint(req).IsSuccess();

        var dataset = await FindDataset(res.Id);
        await Assert.That(dataset).IsNotNull();
        await AssertFieldListEquivalent(dataset!.FieldList, Field("id_date", DataType.DateTime));
    }

    [Test]
    [DisplayName("SELECT СЃ Р°РіСЂРµРіР°С†РёРµР№ РЅР° РєРѕРЅС†Рµ РґРѕР»Р¶РµРЅ РІРµСЂРЅСѓС‚СЊ РєРѕР»РёС‡РµСЃС‚РІРѕ Р·Р°РїРёСЃРµР№ 1")]
    public async Task CreateDataset_WithAggregationAtTheEnd_ShouldReturnOneRowsCount()
    {
        var req = Request() with
        {
            ConnectorId = App.Data.DataConnectors["test"].Id,
            Transformations =
            [
                new OrderByTransformation
                {
                    Items =
                    [
                        "Int(id)".Asc(),
                    ],
                }.Block(),
                new SelectTransformation
                {
                    Items =
                    [
                        "SUM(Int(id))".As("sum"),
                    ],
                }.Block(),
            ],
        };

        var res = await Endpoint(req).IsSuccess();

        var dataset = await FindDataset(res.Id);
        await Assert.That(dataset).IsNotNull();
        await Assert.That(dataset!.RowsCount).IsEqualTo(1);
        await AssertFieldListEquivalent(dataset.FieldList, Field("sum", DataType.Integer, false));
    }
}

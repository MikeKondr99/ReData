using NJsonSchema.Annotations;
using ReData.DemoApp.Database.Entities;
using ReData.DemoApp.Endpoints.Datasets;
using ReData.DemoApp.Endpoints.Datasets.Create;
using ReData.DemoApp.Transformations;
using ReData.Query.Core.Types;
using OrderItem = ReData.DemoApp.Transformations.OrderItem;
using SelectItem = ReData.Query.Core.Types.SelectItem;

namespace ReData.DemoApp.Tests.Datasets;

public class CreateDatasetTests(App App) : DemoAppTestBase<App>(App)
{
    private static string FakeDatasetName() => $"dataset{Guid.NewGuid().ToString("N")[..6]}";

    private static DataSetField TextField(string name) => Field(name, DataType.Text);
    private static DataSetField IntField(string name) => Field(name, DataType.Integer);
    
    private static DataSetField BoolField(string name) => Field(name, DataType.Bool);
    private static DataSetField NumField(string name) => Field(name, DataType.Number);

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

    [Fact(DisplayName = "Создание набора с верными данными должно вернуть 'успех'")]
    public async Task CreateDataset_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations = [],
            ConnectorId = Guid.Empty,
        };

        // Act
        var (rsp, res) = await Endpoint(req);

        // Assert
        rsp.IsSuccessStatusCode.Should().BeTrue();
        res.Name.Should().BeEquivalentTo(req.Name);
        res.Id.Should().NotBeEmpty();

        Db.DataSets.Should().Contain(ds => ds.Id == res.Id);
    }

    [Fact(DisplayName = "Создание набора с пустым именем должно вернуть ошибку валидации")]
    public async Task CreateDataset_WithEmptyName_ShouldReturnValidatinoError()
    {
        // Arrange
        var req = new CreateDataSetRequest
        {
            Name = "",
            ConnectorId = Guid.Empty,
            Transformations = []
        };

        // Act
        var rsp = await EndpointError(req);

        // Assert
        rsp.ShouldBeError("name");
        Db.DataSets.Should().NotContain(ds => ds.Name == req.Name);
    }

    [Fact(DisplayName = "Создание набора с null именем должно вернуть ошибку валидации")]
    public async Task CreateDataset_WithNullName_ShouldReturnValidationError()
    {
        // Arrange
        var req = new CreateDataSetRequest
        {
            Name = null!,
            Transformations = [],
            ConnectorId = Guid.Empty,
        };

        // Act
        var rsp = await EndpointError(req);

        // Assert
        rsp.ShouldBeError("name");
        Db.DataSets.Should().NotContain(ds => ds.Name == req.Name);
    }

    [Fact(DisplayName = "Создание набора с слишком коротким именем должно вернуть ошибку валидации")]
    public async Task CreateDataset_WithShortName_ShouldReturnValidationError()
    {
        // Arrange 
        var req = new CreateDataSetRequest
        {
            Name = "x",
            Transformations = [],
            ConnectorId = Guid.Empty
        };

        // Act
        var rsp = await EndpointError(req);

        // Assert
        rsp.ShouldBeError("name");
        Db.DataSets.Should().NotContain(ds => ds.Name == req.Name);
    }

    [Fact(DisplayName = "Создание набора с не уникальным именем должно вернуть ошибку валидации")]
    public async Task CreateDataset_WithNotUniqueName_ShouldReturnConflict()
    {
        // Arrange
        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations = [],
            ConnectorId = Guid.Empty,
        };
        var (rsp, _) = await Endpoint(req);
        rsp.IsSuccessStatusCode.Should().BeTrue();

        // Act
        var response = await EndpointError(req);

        // Assert
        response.ShouldBeError("name");
    }

    [Fact(DisplayName = "Создание набора с null трансформациями должно вернуть ошибку валидации")]
    public async Task CreateDataset_WithNullTransformations_ShouldReturnValidationError()
    {
        // Arrange
        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations = null!,
            ConnectorId = Guid.Empty,
        };

        // Act
        var rsp = await EndpointError(req);

        // Assert
        rsp.ShouldBeError("transformations");
    }

    [Fact(DisplayName = "Создание набора с существующим коннектором должно быть успешным")]
    public async Task CreateDataset_WithExistingConnector_ShouldReturnCreated()
    {
        // Arrange
        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations = [],
            ConnectorId = App.Data.ExistingDataConnector.Id,
        };

        // Act
        var (rsp, res) = await Endpoint(req);

        // Assert
        rsp.IsSuccessStatusCode.Should().BeTrue();
        res.Name.Should().BeEquivalentTo(req.Name);
        res.Id.Should().NotBeEmpty();

        Db.DataSets.Should().Contain(ds =>
            ds.Id == res.Id &&
            ds.DataConnectorId == App.Data.ExistingDataConnector.Id);
    }

    [Fact(DisplayName = "Создание набора с несуществующим коннектором должно вернуть ошибку валидации")]
    public async Task CreateDataset_WithNonExistentConnector_ShouldReturnValidationError()
    {
        // Arrange
        var nonExistentConnectorId = Guid.NewGuid();
        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations = [],
            ConnectorId = nonExistentConnectorId,
        };

        // Act
        var rsp = await EndpointError(req);

        // Assert
        rsp.ShouldBeError("connectorId");
        Db.DataSets.Should().NotContain(ds => ds.DataConnectorId == nonExistentConnectorId);
    }

    [Fact(DisplayName = "Создание набора с пустым GUID коннектором должно быть успешным")]
    public async Task CreateDataset_WithEmptyGuidConnector_ShouldReturnCreated()
    {
        // Arrange
        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations = [],
            ConnectorId = Guid.Empty,
        };

        // Act
        var (rsp, res) = await Endpoint(req);

        // Assert
        rsp.IsSuccessStatusCode.Should().BeTrue();
        Db.DataSets.Should().Contain(ds =>
            ds.Id == res.Id &&
            ds.DataConnectorId == Guid.Empty);
    }

    [Fact(DisplayName = "Создание набора с валидным коннектором и трансформациями должно быть успешным")]
    public async Task CreateDataset_WithValidConnectorAndTransformations_ShouldReturnCreated()
    {
        // Arrange
        TransformationBlock[] transformations =
        [
            new TransformationBlock()
            {
                Enabled = true,
                Transformation = new WhereTransformation
                {
                    Condition = "true"
                }
            }
        ];

        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations = transformations,
            ConnectorId = App.Data.ExistingDataConnector.Id,
        };

        // Act
        var (rsp, res) = await Endpoint(req);

        // Assert
        rsp.IsSuccessStatusCode.Should().BeTrue();
        Db.DataSets.Should().Contain(ds =>
            ds.Id == res.Id &&
            ds.DataConnectorId == App.Data.ExistingDataConnector.Id);
    }

    [Fact(DisplayName = "Создание набора из пустого коннектора должно сохранить RowsCount = 0 в БД")]
    public async Task CreateDataset_FromEmptyConnector_ShouldSaveRowsCountZero()
    {
        // Arrange
        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations = [],
            ConnectorId = App.Data.DataConnectors["empty"].Id,
        };

        // Act
        var (rsp, res) = await Endpoint(req);

        // Assert
        rsp.IsSuccessStatusCode.Should().BeTrue();
        res.Id.Should().NotBeEmpty();

        var dataset = Db.DataSets.FirstOrDefault(ds => ds.Id == res.Id);
        dataset.Should().NotBeNull();
        dataset!.RowsCount.Should().Be(0);
        dataset.FieldList.Should().BeEquivalentTo([
            TextField("empty"),
        ]);
    }


    [Fact(DisplayName = "Создание набора из коннектора с одной строкой должно сохранить RowsCount = 1 в БД")]
    public async Task CreateDataset_FromSingleRowConnector_ShouldSaveRowsCountOne()
    {
        // Arrange
        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations = [],
            ConnectorId = App.Data.DataConnectors["single"].Id,
        };

        // Act
        var (rsp, res) = await Endpoint(req);

        // Assert
        rsp.IsSuccessStatusCode.Should().BeTrue();
        res.Id.Should().NotBeEmpty();

        var dataset = Db.DataSets.FirstOrDefault(ds => ds.Id == res.Id);
        dataset.Should().NotBeNull();
        dataset!.RowsCount.Should().Be(1);
        dataset.FieldList.Should().BeEquivalentTo([
            BoolField("single"),
        ]);
    }

    [Fact(DisplayName = "Создание набора из коннектора с 1000 строк должно сохранить RowsCount = 1000 в БД")]
    public async Task CreateDataset_FromThousandRowsConnector_ShouldSaveRowsCountThousand()
    {
        // Arrange
        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations = [],
            ConnectorId = App.Data.DataConnectors["numbers"].Id,
        };

        // Act
        var (rsp, res) = await Endpoint(req);

        // Assert
        rsp.IsSuccessStatusCode.Should().BeTrue();
        res.Id.Should().NotBeEmpty();

        var dataset = Db.DataSets.FirstOrDefault(ds => ds.Id == res.Id);
        dataset.Should().NotBeNull();
        dataset!.RowsCount.Should().Be(1000);
        dataset.FieldList.Should().BeEquivalentTo([
            IntField("id"),
        ]);
    }

    [Fact(DisplayName =
        "Создание набора с WHERE трансформацией фильтрующей все строки должно сохранить RowsCount = 0 в БД")]
    public async Task CreateDataset_WithWhereFilteringAllRows_ShouldSaveRowsCountZero()
    {
        // Arrange
        TransformationBlock[] transformations =
        [
            new TransformationBlock()
            {
                Enabled = true,
                Transformation = new WhereTransformation
                {
                    Condition = "Int(id) = 9999" // Non-existent id
                }
            }
        ];

        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations = transformations,
            ConnectorId = App.Data.DataConnectors["numbers"].Id,
        };

        // Act
        var (rsp, res) = await Endpoint(req);

        // Assert
        rsp.IsSuccessStatusCode.Should().BeTrue();
        res.Id.Should().NotBeEmpty();

        var dataset = Db.DataSets.FirstOrDefault(ds => ds.Id == res.Id);
        dataset.Should().NotBeNull();
        dataset!.RowsCount.Should().Be(0);
        dataset.FieldList.Should().BeEquivalentTo([
            IntField("id"),
        ]);
    }

    [Fact(DisplayName =
        "Создание набора с WHERE трансформацией фильтрующей часть строк должно сохранить частичный счет в БД")]
    public async Task CreateDataset_WithWhereFilteringSomeRows_ShouldSavePartialCount()
    {
        // Arrange
        TransformationBlock[] transformations =
        [
            new TransformationBlock()
            {
                Enabled = true,
                Transformation = new WhereTransformation
                {
                    Condition = "Int(id) <= 100" // First 100 rows
                }
            }
        ];

        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations = transformations,
            ConnectorId = App.Data.DataConnectors["numbers"].Id,
        };

        // Act
        var (rsp, res) = await Endpoint(req);

        // Assert
        rsp.IsSuccessStatusCode.Should().BeTrue();
        res.Id.Should().NotBeEmpty();

        var dataset = Db.DataSets.FirstOrDefault(ds => ds.Id == res.Id);
        dataset.Should().NotBeNull();
        dataset!.RowsCount.Should().Be(100);
        dataset.FieldList.Should().BeEquivalentTo([
            IntField("id"),
        ]);
    }


    [Fact(DisplayName = "Создание набора с несколькими WHERE трансформациями должно сохранить кумулятивный счет в БД")]
    public async Task CreateDataset_WithMultipleWhereTransformations_ShouldSaveCumulativeCount()
    {
        // Arrange
        TransformationBlock[] transformations =
        [
            new TransformationBlock()
            {
                Enabled = true,
                Transformation = new WhereTransformation
                {
                    Condition = "Int(id) <= 500" // First filter: first 500
                }
            },
            new TransformationBlock()
            {
                Enabled = true,
                Transformation = new WhereTransformation
                {
                    Condition = "Int(id) >= 400" // Second filter: last 101 of first 500
                }
            }
        ];

        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations = transformations,
            ConnectorId = App.Data.DataConnectors["numbers"].Id,
        };

        // Act
        var (rsp, res) = await Endpoint(req);

        // Assert
        rsp.IsSuccessStatusCode.Should().BeTrue();
        res.Id.Should().NotBeEmpty();

        var dataset = Db.DataSets.FirstOrDefault(ds => ds.Id == res.Id);
        dataset.Should().NotBeNull();
        dataset!.RowsCount.Should().Be(101); // 400, 401, ..., 500
        dataset.FieldList.Should().BeEquivalentTo([
            IntField("id"),
        ]);
    }

    [Fact(DisplayName =
        "Создание набора с WHERE и отключенной WHERE трансформацией должно игнорировать отключенную в БД")]
    public async Task CreateDataset_WithEnabledAndDisabledWhere_ShouldSaveIgnoringDisabled()
    {
        // Arrange
        TransformationBlock[] transformations =
        [
            new TransformationBlock()
            {
                Enabled = true,
                Transformation = new WhereTransformation
                {
                    Condition = "Int(id) <= 100" // First 100 rows
                }
            },
            new TransformationBlock()
            {
                Enabled = false, // Disabled - should be ignored
                Transformation = new WhereTransformation
                {
                    Condition = "Int(id) >= 50"
                }
            }
        ];

        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations = transformations,
            ConnectorId = App.Data.DataConnectors["numbers"].Id,
        };

        // Act
        var (rsp, res) = await Endpoint(req);

        // Assert
        rsp.IsSuccessStatusCode.Should().BeTrue();
        res.Id.Should().NotBeEmpty();

        var dataset = Db.DataSets.FirstOrDefault(ds => ds.Id == res.Id);
        dataset.Should().NotBeNull();
        dataset!.RowsCount.Should().Be(100); // Only first filter applied
        dataset.FieldList.Should().BeEquivalentTo([
            IntField("id"),
        ]);
    }

    [Fact(DisplayName = "Создание набора с LIMIT трансформацией должно сохранить счет лимитированных строк в БД")]
    public async Task CreateDataset_WithLimitTransformation_ShouldSaveLimitedCount()
    {
        // Arrange
        TransformationBlock[] transformations =
        [
            new TransformationBlock()
            {
                Enabled = true,
                Transformation = new LimitOffsetTransformation()
                {
                    Limit = 10
                },
            },
        ];

        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations = transformations,
            ConnectorId = App.Data.DataConnectors["numbers"].Id,
        };

        // Act
        var (rsp, res) = await Endpoint(req);

        // Assert
        rsp.IsSuccessStatusCode.Should().BeTrue();
        res.Id.Should().NotBeEmpty();

        var dataset = Db.DataSets.FirstOrDefault(ds => ds.Id == res.Id);
        dataset.Should().NotBeNull();
        dataset!.RowsCount.Should().Be(10); // Limited to 10 rows
        dataset.FieldList.Should().BeEquivalentTo([
            IntField("id"),
        ]);
    }

    [Fact(DisplayName = "COUNT(*) должен считать все строки включая NULL в полях")]
    public async Task CreateDataset_CountShouldIncludeNullRows()
    {
        // Arrange
        var connector = App.Data.DataConnectors["numbers"];
        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations =
            [
                new SelectTransformation()
                {
                    Items =
                    [
                        "If(id.Int().Mod(2) = 0, null, id)".As("ID"),
                    ]
                }.Block()
            ],
            ConnectorId = connector.Id,
        };

        // Act
        var (rsp, res) = await Endpoint(req);

        // Assert
        rsp.IsSuccessStatusCode.Should().BeTrue();

        var dataset = Db.DataSets.FirstOrDefault(ds => ds.Id == res.Id);
        dataset.Should().NotBeNull();
        dataset!.RowsCount.Should().Be(1000);
        dataset.FieldList.Should().BeEquivalentTo([
            IntField("ID"),
        ]);
    }

    [Fact(DisplayName = "SELECT с Alt должна создавать не-nullable поля")]
    public async Task CreateDataset_WithSelectAlt_ShouldCreateNonNullableFields()
    {
        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations =
            [
                new SelectTransformation()
                {
                    Items =
                    [
                        "id.Alt(0)".As("not_null_id"),
                    ]
                }.Block()
            ],
            ConnectorId = App.Data.DataConnectors["test"].Id,
        };


        // Act
        var (rsp, res) = await Endpoint(req);

        // Assert
        rsp.IsSuccessStatusCode.Should().BeTrue();

        var dataset = Db.DataSets.FirstOrDefault(ds => ds.Id == res.Id);
        dataset.Should().NotBeNull();
        dataset!.RowsCount.Should().Be(2);
        dataset.FieldList.Should().BeEquivalentTo([
            Field("not_null_id", DataType.Integer, false),
        ]);
    }

    [Fact(DisplayName = "SELECT с Int() кастом должна изменять тип поля на Int")]
    public async Task CreateDataset_WithIntCast_ShouldChangeToIntField()
    {
        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations =
            [
                new SelectTransformation()
                {
                    Items =
                    [
                        "Int(id)".As("id_int"),
                    ]
                }.Block()
            ],
            ConnectorId = App.Data.DataConnectors["test"].Id,
        };

        // Act
        var (rsp, res) = await Endpoint(req);

        // Assert
        rsp.IsSuccessStatusCode.Should().BeTrue();

        var dataset = Db.DataSets.FirstOrDefault(ds => ds.Id == res.Id);
        dataset.Should().NotBeNull();
        dataset!.FieldList.Should().BeEquivalentTo([
            Field("id_int", DataType.Integer),
        ]);
    }

    [Fact(DisplayName = "SELECT с Num() кастом должна изменять тип поля на Num")]
    public async Task CreateDataset_WithNumCast_ShouldChangeToNumField()
    {
        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations =
            [
                new SelectTransformation()
                {
                    Items =
                    [
                        "Num(id)".As("id_num"),
                    ]
                }.Block()
            ],
            ConnectorId = App.Data.DataConnectors["test"].Id,
        };

        // Act
        var (rsp, res) = await Endpoint(req);

        // Assert
        rsp.IsSuccessStatusCode.Should().BeTrue();

        var dataset = Db.DataSets.FirstOrDefault(ds => ds.Id == res.Id);
        dataset.Should().NotBeNull();
        dataset!.FieldList.Should().BeEquivalentTo([
            Field("id_num", DataType.Number),
        ]);
    }

    [Fact(DisplayName = "SELECT с Date() кастом должна изменять тип поля на Date")]
    public async Task CreateDataset_WithDateCast_ShouldChangeToDateField()
    {
        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations =
            [
                new SelectTransformation()
                {
                    Items =
                    [
                        "Date(id)".As("id_date"),
                    ]
                }.Block()
            ],
            ConnectorId = App.Data.DataConnectors["test"].Id,
        };

        // Act
        var (rsp, res) = await Endpoint(req);

        // Assert
        rsp.IsSuccessStatusCode.Should().BeTrue();

        var dataset = Db.DataSets.FirstOrDefault(ds => ds.Id == res.Id);
        dataset.Should().NotBeNull();
        dataset!.FieldList.Should().BeEquivalentTo([
            Field("id_date", DataType.DateTime),
        ]);
    }
    
    [Fact(DisplayName = "SELECT с агрегацией на конце должен вернуть количество записей 1")]
    public async Task CreateDataset_WithAggregationAtTheEnd_ShouldReturnOneRowsCount()
    {
        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations =
            [
                new OrderByTransformation()
                {
                    Items = 
                    [
                        "Int(id)".Asc(),
                    ]
                }.Block(),
                new SelectTransformation()
                {
                    Items =
                    [
                        "SUM(Int(id))".As("sum"),
                    ]
                }.Block()
            ],
            ConnectorId = App.Data.DataConnectors["test"].Id,
        };

        // Act
        var (rsp, res) = await Endpoint(req);

        // Assert
        rsp.IsSuccessStatusCode.Should().BeTrue();

        var dataset = Db.DataSets.FirstOrDefault(ds => ds.Id == res.Id);
        dataset.Should().NotBeNull();
        dataset!.RowsCount.Should().Be(1);
        dataset.FieldList.Should().BeEquivalentTo([
            Field("sum", DataType.Integer, false),
        ]);
    }
}
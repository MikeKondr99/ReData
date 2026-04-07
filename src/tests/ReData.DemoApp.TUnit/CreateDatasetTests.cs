using System.Diagnostics;
using System.Net;
using FastEndpoints;
using ReData.DemoApp.Database.Entities;
using ReData.DemoApp.Endpoints.Datasets;
using ReData.DemoApp.Endpoints.Datasets.Create;
using ReData.DemoApp.Transformations;
using ReData.Query.Core.Types;

namespace ReData.DemoApp.TUnit;

public class CreateDatasetTests
{
    [ClassDataSource<DefaultReDataApp>(Shared = SharedType.PerTestSession)]
    public required DefaultReDataApp App { get; init; }
    
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

    private static async Task AssertError(TestResult<ErrorResponse> response, string key)
    {
        var (rsp, error) = response;

        await Assert.That(rsp.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(error.Errors.ContainsKey(key)).IsTrue();
    }

    [Test]
    [DisplayName("Создание набора с верными данными должно вернуть 'успех'")]
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
        var sw = Stopwatch.StartNew();
        var (rsp, res) = await Endpoint(req);
        sw.Stop();
        Console.WriteLine($"HTTP took: {sw.ElapsedMilliseconds}ms");

        // Assert
        await Assert.That(rsp.IsSuccessStatusCode).IsTrue();
        await Assert.That(res.Name).IsEqualTo(req.Name);
        await Assert.That(res.Id).IsNotEmptyGuid();

        // Db.DataSets.Should().Contain(ds => ds.Id == res.Id);
    }

    [Test]
    [DisplayName("Создание набора с пустым именем должно вернуть ошибку валидации")]
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
        await AssertError(rsp, "name");
        // Db.DataSets.Should().NotContain(ds => ds.Name == req.Name);
    }

    [Test]
    [DisplayName("Создание набора с null именем должно вернуть ошибку валидации")]
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
        await AssertError(rsp, "name");
        // Db.DataSets.Should().NotContain(ds => ds.Name == req.Name);
    }

    [Test]
    [DisplayName("Создание набора с слишком коротким именем должно вернуть ошибку валидации")]
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
        await AssertError(rsp, "name");
        // Db.DataSets.Should().NotContain(ds => ds.Name == req.Name);
    }

    [Test]
    [DisplayName("Создание набора с не уникальным именем должно вернуть ошибку валидации")]
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
        await Assert.That(rsp.IsSuccessStatusCode).IsTrue();

        // Act
        var response = await EndpointError(req);

        // Assert
        await AssertError(response, "name");
    }

    [Test]
    [DisplayName("Создание набора с null трансформациями должно вернуть ошибку валидации")]
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
        await AssertError(rsp, "transformations");
    }

    [Test]
    [DisplayName("Создание набора с существующим коннектором должно быть успешным")]
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
        await Assert.That(rsp.IsSuccessStatusCode).IsTrue();
        await Assert.That(res.Name).IsEqualTo(req.Name);
        await Assert.That(res.Id).IsNotEmptyGuid();

        // Db.DataSets.Should().Contain(ds =>
            // ds.Id == res.Id &&
            // ds.DataConnectorId == App.Data.ExistingDataConnector.Id);
    }

    [Test]
    [DisplayName("Создание набора с несуществующим коннектором должно вернуть ошибку валидации")]
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
        await AssertError(rsp, "connectorId");
        // Db.DataSets.Should().NotContain(ds => ds.DataConnectorId == nonExistentConnectorId);
    }

    [Test]
    [DisplayName("Создание набора с пустым GUID коннектором должно быть успешным")]
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
        await Assert.That(rsp.IsSuccessStatusCode).IsTrue();
        // Db.DataSets.Should().Contain(ds =>
            // ds.Id == res.Id &&
            // ds.DataConnectorId == Guid.Empty);
    }

    [Test]
    [DisplayName("Создание набора с валидным коннектором и трансформациями должно быть успешным")]
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
        await Assert.That(rsp.IsSuccessStatusCode).IsTrue();
        // Db.DataSets.Should().Contain(ds =>
            // ds.Id == res.Id &&
            // ds.DataConnectorId == App.Data.ExistingDataConnector.Id);
    }

    [Test]
    [DisplayName("Создание набора из пустого коннектора должно сохранить RowsCount = 0 в БД")]
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
        await Assert.That(rsp.IsSuccessStatusCode).IsTrue();
        await Assert.That(res.Id).IsNotEmptyGuid();

        // var dataset = Db.DataSets.FirstOrDefault(ds => ds.Id == res.Id);
        // dataset.Should().NotBeNull();
        // dataset!.RowsCount.Should().Be(0);
        // dataset.FieldList.Should().BeEquivalentTo([
            // TextField("empty"),
        // ]);
    }


    [Test]
    [DisplayName("Создание набора из коннектора с одной строкой должно сохранить RowsCount = 1 в БД")]
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
        await Assert.That(rsp.IsSuccessStatusCode).IsTrue();
        await Assert.That(res.Id).IsNotEmptyGuid();

        // var dataset = Db.DataSets.FirstOrDefault(ds => ds.Id == res.Id);
        // dataset.Should().NotBeNull();
        // dataset!.RowsCount.Should().Be(1);
        // dataset.FieldList.Should().BeEquivalentTo([
            // BoolField("single"),
        // ]);
    }

    [Test]
    [DisplayName("Создание набора из коннектора с 1000 строк должно сохранить RowsCount = 1000 в БД")]
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
        await Assert.That(rsp.IsSuccessStatusCode).IsTrue();
        await Assert.That(res.Id).IsNotEmptyGuid();

        // var dataset = Db.DataSets.FirstOrDefault(ds => ds.Id == res.Id);
        // dataset.Should().NotBeNull();
        // dataset!.RowsCount.Should().Be(1000);
        // dataset.FieldList.Should().BeEquivalentTo([
            // IntField("id"),
        // ]);
    }

    [Test]
    [DisplayName("Создание набора с WHERE трансформацией фильтрующей все строки должно сохранить RowsCount = 0 в БД")]
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
        await Assert.That(rsp.IsSuccessStatusCode).IsTrue();
        await Assert.That(res.Id).IsNotEmptyGuid();

        // var dataset = Db.DataSets.FirstOrDefault(ds => ds.Id == res.Id);
        // dataset.Should().NotBeNull();
        // dataset!.RowsCount.Should().Be(0);
        // dataset.FieldList.Should().BeEquivalentTo([
            // IntField("id"),
        // ]);
    }

    [Test]
    [DisplayName("Создание набора с WHERE трансформацией фильтрующей часть строк должно сохранить частичный счет в БД")]
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
        await Assert.That(rsp.IsSuccessStatusCode).IsTrue();
        await Assert.That(res.Id).IsNotEmptyGuid();

        // var dataset = Db.DataSets.FirstOrDefault(ds => ds.Id == res.Id);
        // dataset.Should().NotBeNull();
        // dataset!.RowsCount.Should().Be(100);
        // dataset.FieldList.Should().BeEquivalentTo([
            // IntField("id"),
        // ]);
    }


    [Test]
    [DisplayName("Создание набора с несколькими WHERE трансформациями должно сохранить кумулятивный счет в БД")]
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
        await Assert.That(rsp.IsSuccessStatusCode).IsTrue();
        await Assert.That(res.Id).IsNotEmptyGuid();

        // var dataset = Db.DataSets.FirstOrDefault(ds => ds.Id == res.Id);
        // dataset.Should().NotBeNull();
        // dataset!.RowsCount.Should().Be(101); // 400, 401, ..., 500
        // dataset.FieldList.Should().BeEquivalentTo([
            // IntField("id"),
        // ]);
    }

    [Test]
    [DisplayName("Создание набора с WHERE и отключенной WHERE трансформацией должно игнорировать отключенную в БД")]
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
        await Assert.That(rsp.IsSuccessStatusCode).IsTrue();
        await Assert.That(res.Id).IsNotEmptyGuid();

        // var dataset = Db.DataSets.FirstOrDefault(ds => ds.Id == res.Id);
        // dataset.Should().NotBeNull();
        // dataset!.RowsCount.Should().Be(100); // Only first filter applied
        // dataset.FieldList.Should().BeEquivalentTo([
            // IntField("id"),
        // ]);
    }

    [Test]
    [DisplayName("Создание набора с LIMIT трансформацией должно сохранить счет лимитированных строк в БД")]
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
        await Assert.That(rsp.IsSuccessStatusCode).IsTrue();
        await Assert.That(res.Id).IsNotEmptyGuid();

        // var dataset = Db.DataSets.FirstOrDefault(ds => ds.Id == res.Id);
        // dataset.Should().NotBeNull();
        // dataset!.RowsCount.Should().Be(10); // Limited to 10 rows
        // dataset.FieldList.Should().BeEquivalentTo([
            // IntField("id"),
        // ]);
    }

    [Test]
    [DisplayName("COUNT(*) должен считать все строки включая NULL в полях")]
    public async Task CreateDataset_CountShouldIncludeNullRows()
    {
        // Arrange
        var connector = App.Data.DataConnectors["numbers"];
        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations =
            [
                new TransformationBlock
                {
                    Enabled = true,
                    Transformation = new SelectTransformation()
                    {
                        Items =
                        [
                            new ReData.DemoApp.Transformations.SelectItem
                            {
                                Field = "ID",
                                Expression = "If(id.Int().Mod(2) = 0, null, id)",
                            },
                        ]
                    }
                }
            ],
            ConnectorId = connector.Id,
        };

        // Act
        var (rsp, res) = await Endpoint(req);

        // Assert
        await Assert.That(rsp.IsSuccessStatusCode).IsTrue();

        // var dataset = Db.DataSets.FirstOrDefault(ds => ds.Id == res.Id);
        // dataset.Should().NotBeNull();
        // dataset!.RowsCount.Should().Be(1000);
        // dataset.FieldList.Should().BeEquivalentTo([
            // IntField("ID"),
        // ]);
    }

    [Test]
    [DisplayName("SELECT с Alt должна создавать не-nullable поля")]
    public async Task CreateDataset_WithSelectAlt_ShouldCreateNonNullableFields()
    {
        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations =
            [
                new TransformationBlock
                {
                    Enabled = true,
                    Transformation = new SelectTransformation()
                    {
                        Items =
                        [
                            new ReData.DemoApp.Transformations.SelectItem
                            {
                                Field = "not_null_id",
                                Expression = "id.Alt(0)",
                            },
                        ]
                    }
                }
            ],
            ConnectorId = App.Data.DataConnectors["test"].Id,
        };


        // Act
        var (rsp, res) = await Endpoint(req);

        // Assert
        await Assert.That(rsp.IsSuccessStatusCode).IsTrue();

        // var dataset = Db.DataSets.FirstOrDefault(ds => ds.Id == res.Id);
        // dataset.Should().NotBeNull();
        // dataset!.RowsCount.Should().Be(2);
        // dataset.FieldList.Should().BeEquivalentTo([
            // Field("not_null_id", DataType.Integer, false),
        // ]);
    }

    [Test]
    [DisplayName("SELECT с Int() кастом должна изменять тип поля на Int")]
    public async Task CreateDataset_WithIntCast_ShouldChangeToIntField()
    {
        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations =
            [
                new TransformationBlock
                {
                    Enabled = true,
                    Transformation = new SelectTransformation()
                    {
                        Items =
                        [
                            new ReData.DemoApp.Transformations.SelectItem
                            {
                                Field = "id_int",
                                Expression = "Int(id)",
                            },
                        ]
                    }
                }
            ],
            ConnectorId = App.Data.DataConnectors["test"].Id,
        };

        // Act
        var (rsp, res) = await Endpoint(req);

        // Assert
        await Assert.That(rsp.IsSuccessStatusCode).IsTrue();

        // var dataset = Db.DataSets.FirstOrDefault(ds => ds.Id == res.Id);
        // dataset.Should().NotBeNull();
        // dataset!.FieldList.Should().BeEquivalentTo([
            // Field("id_int", DataType.Integer),
        // ]);
    }

    [Test]
    [DisplayName("SELECT с Num() кастом должна изменять тип поля на Num")]
    public async Task CreateDataset_WithNumCast_ShouldChangeToNumField()
    {
        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations =
            [
                new TransformationBlock
                {
                    Enabled = true,
                    Transformation = new SelectTransformation()
                    {
                        Items =
                        [
                            new ReData.DemoApp.Transformations.SelectItem
                            {
                                Field = "id_num",
                                Expression = "Num(id)",
                            },
                        ]
                    }
                }
            ],
            ConnectorId = App.Data.DataConnectors["test"].Id,
        };

        // Act
        var (rsp, res) = await Endpoint(req);

        // Assert
        await Assert.That(rsp.IsSuccessStatusCode).IsTrue();

        // var dataset = Db.DataSets.FirstOrDefault(ds => ds.Id == res.Id);
        // dataset.Should().NotBeNull();
        // dataset!.FieldList.Should().BeEquivalentTo([
            // Field("id_num", DataType.Number),
        // ]);
    }

    [Test]
    [DisplayName("SELECT с Date() кастом должна изменять тип поля на Date")]
    public async Task CreateDataset_WithDateCast_ShouldChangeToDateField()
    {
        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations =
            [
                new TransformationBlock
                {
                    Enabled = true,
                    Transformation = new SelectTransformation()
                    {
                        Items =
                        [
                            new ReData.DemoApp.Transformations.SelectItem
                            {
                                Field = "id_date",
                                Expression = "Date(id)",
                            },
                        ]
                    }
                }
            ],
            ConnectorId = App.Data.DataConnectors["test"].Id,
        };

        // Act
        var (rsp, res) = await Endpoint(req);

        // Assert
        await Assert.That(rsp.IsSuccessStatusCode).IsTrue();

        // var dataset = Db.DataSets.FirstOrDefault(ds => ds.Id == res.Id);
        // dataset.Should().NotBeNull();
        // dataset!.FieldList.Should().BeEquivalentTo([
            // Field("id_date", DataType.DateTime),
        // ]);
    }
    
    [Test]
    [DisplayName("SELECT с агрегацией на конце должен вернуть количество записей 1")]
    public async Task CreateDataset_WithAggregationAtTheEnd_ShouldReturnOneRowsCount()
    {
        var req = new CreateDataSetRequest
        {
            Name = FakeDatasetName(),
            Transformations =
            [
                new TransformationBlock
                {
                    Enabled = true,
                    Transformation = new OrderByTransformation()
                    {
                        Items =
                        [
                            new ReData.DemoApp.Transformations.OrderItem
                            {
                                Expression = "Int(id)",
                                Descending = false,
                            },
                        ]
                    },
                },
                new TransformationBlock
                {
                    Enabled = true,
                    Transformation = new SelectTransformation()
                    {
                        Items =
                        [
                            new ReData.DemoApp.Transformations.SelectItem
                            {
                                Field = "sum",
                                Expression = "SUM(Int(id))",
                            },
                        ]
                    }
                }
            ],
            ConnectorId = App.Data.DataConnectors["test"].Id,
        };

        // Act
        var (rsp, res) = await Endpoint(req);

        // Assert
        await Assert.That(rsp.IsSuccessStatusCode).IsTrue();

        // var dataset = Db.DataSets.FirstOrDefault(ds => ds.Id == res.Id);
        // dataset.Should().NotBeNull();
        // dataset!.RowsCount.Should().Be(1);
        // dataset.FieldList.Should().BeEquivalentTo([
            // Field("sum", DataType.Integer, false),
        // ]);
    }
}

using ReData.DemoApp.Endpoints.Transform;
using ReData.DemoApp.Transformations;
using System.Text.Json;
using TUnit.Core;

namespace ReData.DemoApp.Tests.Transform;

public class TransformEndpointTests
{
    [ClassDataSource<DefaultReDataApp>(Shared = SharedType.PerTestSession)]
    public required DefaultReDataApp App { get; init; }

    private sealed record TransformResponseForTest
    {
        public required IReadOnlyList<TransformFieldViewModel> Fields { get; init; }

        public required long? Total { get; init; }

        public required IReadOnlyList<Dictionary<string, object?>> Data { get; init; }
    }

    private Task<TestResult<TransformResponseForTest>> EndpointOk(TransformRequest req) =>
        App.Client.POSTAsync<TransformEndpoint, TransformRequest, TransformResponseForTest>(req);

    private Task<TestResult<TransformErrorResponse>> EndpointError(TransformRequest req) =>
        App.Client.POSTAsync<TransformEndpoint, TransformRequest, TransformErrorResponse>(req);

    private TransformRequest Request(params Transformation[] transformations) =>
        new()
        {
            DataConnectorId = App.Data.ExistingDataConnector.Id,
            PageNumber = 1,
            PageSize = 20,
            Transformations = transformations.ToList()
        };

    private static bool NoErrors(IReadOnlyList<ReData.Query.Common.ExprError>? errors) =>
        errors is null || errors.Count == 0;

    private static long? Int(IReadOnlyDictionary<string, object?> row, string key)
    {
        if (!row.TryGetValue(key, out var value) || value is null)
        {
            return null;
        }

        return value switch
        {
            long l => l,
            int i => i,
            short s => s,
            byte b => b,
            JsonElement je when je.ValueKind == JsonValueKind.Number && je.TryGetInt64(out var jv) => jv,
            JsonElement je when je.ValueKind == JsonValueKind.Null => null,
            _ => Convert.ToInt64(value)
        };
    }

    [Test]
    public async Task Transform_GroupBy_InvalidGroupExpression_ShouldReturnErrorAtGroupIndexWithoutDuplicates()
    {
        var req = Request(
            new GroupByTransformation
            {
                Groups =
                [
                    new SelectItem { Field = "g1", Expression = "[id]" },
                    new SelectItem { Field = "g2", Expression = "1" }
                ],
                Items =
                [
                    new SelectItem { Field = "cnt", Expression = "COUNT([id])" }
                ]
            });

        var (rsp, error) = await EndpointError(req);

        await Assert.That(rsp.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(error.Index).IsEqualTo(0);
        await Assert.That(error.Errors).IsNotNull();

        var errors = error.Errors!.ToArray();
        await Assert.That(errors.Length).IsEqualTo(3);
        await Assert.That(errors[1]).IsNotNull();
        await Assert.That(errors[1]!.Count).IsGreaterThan(0);
        await Assert.That(NoErrors(errors[0])).IsTrue();
        await Assert.That(NoErrors(errors[2])).IsTrue();
    }

    [Test]
    public async Task Transform_GroupBy_MultipleInvalidGroupExpressions_ShouldReturnErrorsInGroupsSection()
    {
        var req = Request(
            new GroupByTransformation
            {
                Groups =
                [
                    new SelectItem { Field = "g1", Expression = "1" },
                    new SelectItem { Field = "g2", Expression = "'x'" }
                ],
                Items =
                [
                    new SelectItem { Field = "cnt", Expression = "COUNT([id])" }
                ]
            });

        var (rsp, error) = await EndpointError(req);

        await Assert.That(rsp.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(error.Index).IsEqualTo(0);
        await Assert.That(error.Errors).IsNotNull();

        var errors = error.Errors!.ToArray();
        await Assert.That(errors.Length).IsEqualTo(2);
        await Assert.That(errors[0]).IsNotNull();
        await Assert.That(errors[0]!.Count).IsGreaterThan(0);
        await Assert.That(errors[1]).IsNotNull();
        await Assert.That(errors[1]!.Count).IsGreaterThan(0);
    }

    [Test]
    public async Task Transform_GroupBy_InvalidAggregationExpression_ShouldReturnErrorAfterGroups()
    {
        var req = Request(
            new GroupByTransformation
            {
                Groups =
                [
                    new SelectItem { Field = "g1", Expression = "[id]" },
                    new SelectItem { Field = "g2", Expression = "[name]" }
                ],
                Items =
                [
                    new SelectItem { Field = "cnt", Expression = "COUNT([id])" },
                    new SelectItem { Field = "invalid", Expression = "[missing_field]" }
                ]
            });

        var (rsp, error) = await EndpointError(req);

        await Assert.That(rsp.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(error.Index).IsEqualTo(0);
        await Assert.That(error.Errors).IsNotNull();

        var errors = error.Errors!.ToArray();
        await Assert.That(errors.Length).IsEqualTo(4);
        await Assert.That(errors[3]).IsNotNull();
        await Assert.That(errors[3]!.Count).IsGreaterThan(0);
        await Assert.That(NoErrors(errors[0])).IsTrue();
        await Assert.That(NoErrors(errors[1])).IsTrue();
        await Assert.That(NoErrors(errors[2])).IsTrue();
    }

    [Test]
    public async Task Transform_Select_InvalidExpression_ShouldNotProduceSpuriousConnectionErrorForConstantExpression()
    {
        var req = Request(
            new SelectTransformation
            {
                Items =
                [
                    new SelectItem { Field = "total_count", Expression = "const(COUNT([id]))" },
                    new SelectItem { Field = "broken", Expression = "[missing_field]" }
                ]
            });

        var (rsp, error) = await EndpointError(req);

        await Assert.That(rsp.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(error.Index).IsEqualTo(0);
        await Assert.That(error.Errors).IsNotNull();

        var errors = error.Errors!.ToArray();
        await Assert.That(errors.Length).IsEqualTo(2);
        await Assert.That(NoErrors(errors[0])).IsTrue();
        await Assert.That(errors[1]).IsNotNull();
        await Assert.That(errors[1]!.Any(e => e.Message.Contains("missing_field"))).IsTrue();
    }

    [Test]
    public async Task Transform_Where_StoredComputedConst_ShouldBeUsableInLaterSelect()
    {
        var req = Request(
            new WhereTransformation
            {
                Condition = "const total_rows = COUNT([id]); total_rows > 0"
            },
            new SelectTransformation
            {
                Items =
                [
                    new SelectItem { Field = "id", Expression = "[id]" },
                    new SelectItem { Field = "total_rows", Expression = "total_rows" },
                    new SelectItem { Field = "total_rows_plus_one", Expression = "total_rows + 1" }
                ]
            });

        var (rsp, ok) = await EndpointOk(req);

        await Assert.That(rsp.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(ok.Total).IsNotNull();
        await Assert.That(ok.Data.Count).IsGreaterThan(0);
        await Assert.That(ok.Fields.Select(f => f.Alias).Contains("id")).IsTrue();
        await Assert.That(ok.Fields.Select(f => f.Alias).Contains("total_rows")).IsTrue();
        await Assert.That(ok.Fields.Select(f => f.Alias).Contains("total_rows_plus_one")).IsTrue();

        var first = ok.Data[0];
        await Assert.That(Int(first, "total_rows")).IsEqualTo(ok.Total);
        await Assert.That(Int(first, "total_rows_plus_one")).IsEqualTo(ok.Total + 1);
    }

    [Test]
    public async Task Transform_Where_StoredComputedConst_WithLaterBrokenExpression_ShouldReturnOnlyActualError()
    {
        var req = Request(
            new WhereTransformation
            {
                Condition = "const total_rows = COUNT([id]); total_rows > 0"
            },
            new SelectTransformation
            {
                Items =
                [
                    new SelectItem { Field = "total_rows", Expression = "total_rows" },
                    new SelectItem { Field = "broken", Expression = "[missing_field]" }
                ]
            });

        var (rsp, error) = await EndpointError(req);

        await Assert.That(rsp.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(error.Index).IsEqualTo(1);
        await Assert.That(error.Errors).IsNotNull();

        var errors = error.Errors!.ToArray();
        await Assert.That(errors.Length).IsEqualTo(2);
        await Assert.That(NoErrors(errors[0])).IsTrue();
        await Assert.That(errors[1]).IsNotNull();
        await Assert.That(errors[1]!.Any(e => e.Message.Contains("missing_field"))).IsTrue();
    }

    [Test]
    public async Task Transform_Where_StoredComputedConst_ShouldBeUsableInLaterOrderBy()
    {
        var req = Request(
            new WhereTransformation
            {
                Condition = "const total_rows = COUNT([id]); total_rows > 0"
            },
            new OrderByTransformation
            {
                Items =
                [
                    new OrderItem
                    {
                        Expression = "total_rows",
                        Descending = true
                    }
                ]
            },
            new SelectTransformation
            {
                Items =
                [
                    new SelectItem { Field = "id", Expression = "[id]" },
                    new SelectItem { Field = "total_rows", Expression = "total_rows" }
                ]
            });

        var (rsp, ok) = await EndpointOk(req);

        await Assert.That(rsp.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(ok.Total).IsNotNull();
        await Assert.That(ok.Data.Count).IsGreaterThan(0);
        await Assert.That(Int(ok.Data[0], "total_rows")).IsEqualTo(ok.Total);
    }
}

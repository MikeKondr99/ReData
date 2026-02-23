using ReData.DemoApp.Endpoints.Transform;
using ReData.DemoApp.Transformations;
using ReData.Query.Core.Value;

namespace ReData.DemoApp.Tests.Transform;

public class TransformEndpointTests(App App) : DemoAppTestBase<App>(App)
{
    private Task<TestResult<TransformResponse>> EndpointOk(TransformRequest req) =>
        App.Client.POSTAsync<TransformEndpoint, TransformRequest, TransformResponse>(req);

    private Task<TestResult<TransformErrorResponse>> EndpointError(TransformRequest req) =>
        App.Client.POSTAsync<TransformEndpoint, TransformRequest, TransformErrorResponse>(req);

    private static TransformRequest Request(params Transformation[] transformations) =>
        new()
        {
            DataConnectorId = App.Data.ExistingDataConnector.Id,
            PageNumber = 1,
            PageSize = 20,
            Transformations = transformations.ToList()
        };

    private static bool NoErrors(IReadOnlyList<ReData.Query.Common.ExprError>? errors) =>
        errors is null || errors.Count == 0;

    [Fact(DisplayName = "groupBy: ошибка в группировке должна возвращаться в позиции groups без дублей из select")]
    public async Task Transform_GroupBy_InvalidGroupExpression_ShouldReturnErrorAtGroupIndexWithoutDuplicates()
    {
        // Regression test for groupBy error ordering: errors are returned as groups + items.
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

        rsp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Index.Should().Be(0);
        error.Errors.Should().NotBeNull();

        var errors = error.Errors!.ToArray();
        errors.Should().HaveCount(3, "errors must be aligned to groups + items");
        errors[1].Should().NotBeNull();
        errors[1]!.Should().NotBeEmpty("second group expression is invalid");
        NoErrors(errors[0]).Should().BeTrue();
        NoErrors(errors[2]).Should().BeTrue("group error must not be duplicated in aggregation section");
    }

    [Fact(DisplayName = "groupBy: несколько ошибок в группировках должны возвращаться в секции groups")]
    public async Task Transform_GroupBy_MultipleInvalidGroupExpressions_ShouldReturnErrorsInGroupsSection()
    {
        // Regression test for groupBy error ordering: multiple invalid groups should map to group slots only.
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

        rsp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Index.Should().Be(0);
        error.Errors.Should().NotBeNull();

        var errors = error.Errors!.ToArray();
        errors.Should().HaveCount(2, "when all errors are in groups, backend returns only group errors");
        errors[0].Should().NotBeNull();
        errors[0]!.Should().NotBeEmpty("first group is constant and invalid for GroupBy");
        errors[1].Should().NotBeNull();
        errors[1]!.Should().NotBeEmpty("second group is constant and invalid for GroupBy");
    }

    [Fact(DisplayName = "groupBy: ошибка в агрегации должна возвращаться после всех группировок")]
    public async Task Transform_GroupBy_InvalidAggregationExpression_ShouldReturnErrorAfterGroups()
    {
        // Regression test for groupBy error ordering: aggregation indexes must be shifted by groups count.
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

        rsp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Index.Should().Be(0);
        error.Errors.Should().NotBeNull();

        var errors = error.Errors!.ToArray();
        errors.Should().HaveCount(4, "errors must be aligned to groups + items");
        errors[3].Should().NotBeNull();
        errors[3]!.Should().NotBeEmpty("second aggregation error must be after two group expressions");
        NoErrors(errors[0]).Should().BeTrue();
        NoErrors(errors[1]).Should().BeTrue();
        NoErrors(errors[2]).Should().BeTrue();
    }
    [Fact]
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

        rsp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Index.Should().Be(0);
        error.Errors.Should().NotBeNull();

        var errors = error.Errors!.ToArray();
        var dump = string.Join(" | ", errors.Select((list, idx) =>
            $"{idx}: {(list is null ? "null" : string.Join(" || ", list.Select(e => e.Message)))}"));
        errors.Should().HaveCount(2);
        NoErrors(errors[0]).Should().BeTrue($"constant expression should not get a secondary connection failure. Actual: {dump}");
        errors[1].Should().NotBeNull();
        errors[1]!.Should().Contain(e => e.Message.Contains("missing_field"));
    }

    [Fact]
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

        rsp.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Total.Should().NotBeNull();
        ok.Data.Should().NotBeEmpty();
        ok.Fields.Select(f => f.Alias).Should().Contain(["id", "total_rows", "total_rows_plus_one"]);

        var first = ok.Data[0];
        first.Int("total_rows").Should().Be(ok.Total);
        first.Int("total_rows_plus_one").Should().Be(ok.Total + 1);
    }

    [Fact]
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

        rsp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Index.Should().Be(1);
        error.Errors.Should().NotBeNull();

        var errors = error.Errors!.ToArray();
        var dump = string.Join(" | ", errors.Select((list, idx) =>
            $"{idx}: {(list is null ? "null" : string.Join(" || ", list.Select(e => e.Message)))}"));

        errors.Should().HaveCount(2);
        NoErrors(errors[0]).Should().BeTrue($"stored const should resolve without secondary connection errors. Actual: {dump}");
        errors[1].Should().NotBeNull();
        errors[1]!.Should().Contain(e => e.Message.Contains("missing_field"));
    }

    [Fact]
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

        rsp.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Total.Should().NotBeNull();
        ok.Data.Should().NotBeEmpty();
        ok.Data[0].Int("total_rows").Should().Be(ok.Total);
    }
}

using ReData.DemoApp.Endpoints.Transform;
using ReData.DemoApp.Transformations;

namespace ReData.DemoApp.Tests.Transform;

public class TransformEndpointTests(App App) : DemoAppTestBase<App>(App)
{
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
}

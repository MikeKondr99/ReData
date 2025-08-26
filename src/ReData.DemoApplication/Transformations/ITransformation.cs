using System.Text.Json.Serialization;
using Pattern.Unions;
using ReData.Query.Common;
using ReData.Query.Core;

namespace ReData.DemoApplication.Transformations;

[JsonDerivedType(typeof(SelectTransformation), typeDiscriminator: "select")]
[JsonDerivedType(typeof(WhereTransformation), typeDiscriminator: "where")]
[JsonDerivedType(typeof(OrderByTransformation), typeDiscriminator: "orderBy")]
[JsonDerivedType(typeof(LimitOffsetTransformation), typeDiscriminator: "limit")]
[JsonDerivedType(typeof(GroupByTransformation), typeDiscriminator: "groupBy")]
public interface ITransformation
{
    Result<QueryBuilder, IEnumerable<ExprError?>> Apply(QueryBuilder builder);
}


public record Transformation
{
    public required bool Enabled { get; init; }
    
    public required ITransformation Data { get; init; }

}
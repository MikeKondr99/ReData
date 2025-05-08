using System.Text.Json.Serialization;
using Pattern.Unions;
using ReData.Query.Core;

namespace ReData.DemoApplication;

[JsonDerivedType(typeof(SelectTransformation), typeDiscriminator: "select")]
[JsonDerivedType(typeof(WhereTransformation), typeDiscriminator: "where")]
[JsonDerivedType(typeof(OrderByTransformation), typeDiscriminator: "orderBy")]
[JsonDerivedType(typeof(LimitOffsetTransformation), typeDiscriminator: "limit")]
public interface ITransformation
{
    Result<QueryBuilder, IEnumerable<ExprError?>> Apply(QueryBuilder builder);
}
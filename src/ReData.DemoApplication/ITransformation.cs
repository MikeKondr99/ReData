
using System.Text.Json.Serialization;
using ReData.Query.Visitors;

namespace ReData.DemoApplication;

[JsonDerivedType(typeof(SelectTransformation), typeDiscriminator: "select")]
[JsonDerivedType(typeof(WhereTransformation), typeDiscriminator: "where")]
[JsonDerivedType(typeof(OrderByTransformation), typeDiscriminator: "orderBy")]
[JsonDerivedType(typeof(LimitOffsetTransformation), typeDiscriminator: "limitOffset")]
public interface ITransformation
{
    QueryBuilder Apply(QueryBuilder builder);
}
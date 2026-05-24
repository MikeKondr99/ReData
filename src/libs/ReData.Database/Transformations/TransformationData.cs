using System.Text.Json.Serialization;

namespace ReData.DemoApp.Transformations;

[JsonDerivedType(typeof(SelectTransformationData), typeDiscriminator: "select")]
[JsonDerivedType(typeof(WhereTransformationData), typeDiscriminator: "where")]
[JsonDerivedType(typeof(OrderByTransformationData), typeDiscriminator: "orderBy")]
[JsonDerivedType(typeof(LimitOffsetTransformationData), typeDiscriminator: "limit")]
[JsonDerivedType(typeof(GroupByTransformationData), typeDiscriminator: "groupBy")]
[JsonDerivedType(typeof(AggInfoTransformationData), typeDiscriminator: "aggInfo")]
public abstract record TransformationData;

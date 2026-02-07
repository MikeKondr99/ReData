using System.Runtime.InteropServices.JavaScript;
using Org.BouncyCastle.Crypto.Engines;
using ReData.Query.Core.Types;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypes;
using static DataType;

public class ConditionalFunctions : FunctionsDescriptor
{
    protected override void Functions()
    {
        foreach (var type in new[]
                 {
                     Number, Text, Integer, DateTime
                 })
        {
            Function("If")
                .Doc("Условное выражение: возвращает then-значение если condition=true, иначе else-значение")
                .Arg("condition", Bool, propagateNull: false)
                .Arg("then", type)
                .Arg("else", type)
                .Returns(type)
                .TemplatesX((condition, then, @else) => new()
                {
                    [All] = $"CASE WHEN {condition} THEN {then} ELSE {@else} END",
                });
            
            Function("Case")
                .Doc("")
                .Arg("condition", Bool)
                .ReqArg("then", type)
                .Returns(type)
                .CustomNullPropagation(_ => true)
                .TemplatesX((condition, then) => new()
                {
                    [All] = $"CASE WHEN {condition} THEN {then} ELSE NULL END",
                });
            
            Method("Case")
                .Doc("")
                .Arg("input", type)
                .Arg("condition", Bool)
                .ReqArg("alt", type)
                .Returns(type)
                .CustomNullPropagation(_ => true)
                .TemplatesX((input, condition, alt) => new()
                {
                    [All] = $"COALESCE({input}, CASE WHEN {condition} THEN {alt} ELSE NULL END)",
                });

            Method("Alt")
                .Doc("Возвращает первое значение, если оно не NULL, иначе возвращает альтернативное значение")
                .Arg("input", type)
                .Arg("alt", type)
                .Returns(type)
                .CustomNullPropagation(nulls => nulls.All(x => x))
                .TemplatesX((input, alt) => new()
                {
                    [All] = $"COALESCE({input}, {alt})",
                });
        }

        foreach (var type in new[]
                 {
                     Number, Text, Integer, DateTime, Unknown
                 })
        {
            Method("IsNull")
                .Doc("Проверяет, является ли значение NULL")
                .Arg("value", type)
                .ReturnsNotNull(Bool)
                .TemplatesX((value) => new()
                {
                    [All] = $"({value} IS NULL)",
                });

            Method("NotNull")
                .Doc("Проверяет, что значение не NULL")
                .Arg("value", type)
                .ReturnsNotNull(Bool)
                .TemplatesX((value) => new()
                {
                    [All] = $"({value} IS NOT NULL)",
                });
        }
        
        
        
        
    }
}
using ReData.Query.Lang.Expressions;
using ReData.Query.Visitors;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;
using static DataType;

public class ReflectionFunctions : FunctionsDescriptor
{
    
    protected override void Functions()
    {
        foreach (var T in new [] { Text, Number, Integer, Boolean, Null, Unknown })
        {
            Method("Type")
                .Arg("input", T)
                .ReturnsNotNull(Text)
                .Templates(new()
                {
                    [All] = $"'{T.ToString()}?'",
                });
            
            Method("Type")
                .ReqArg("input", T)
                .ReturnsNotNull(Text)
                .Templates(new()
                {
                    [All] = $"'{T.ToString()}'",
                });
        }
    }
}
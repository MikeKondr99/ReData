using ReData.DemoApp.Transformations;

namespace ReData.DemoApp.Tests;

public static class Extensions
{
    public static TransformationBlock Block(this Transformation transformation, bool enabled = true)
    {
        return new TransformationBlock()
        {
            Enabled = enabled,
            Transformation = transformation,
        };
    }

    public static SelectItem As(this string expression, string alias)
    {
        return new SelectItem()
        {
            Field = alias,
            Expression = expression,
        };
    }
    
    public static OrderItem Asc(this string expression)
    {
        return new OrderItem()
        {
            Expression = expression,
            Descending = false,
        };
    }
    
    public static OrderItem Desc(this string expression)
    {
        return new OrderItem()
        {
            Expression = expression,
            Descending = true,
        };
    }
    
    
}
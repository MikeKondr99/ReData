namespace ReData.Query.Impl.Functions;

public class BinaryAttribute(BinaryOperation operation) : FunctionNameAttribute(operation switch
{
        BinaryOperation.Power => "^",
        BinaryOperation.Plus => "+",
        BinaryOperation.Minus => "-",
        BinaryOperation.Multiply => "*",
        BinaryOperation.Divide => "/",
        BinaryOperation.And => "and",
        BinaryOperation.Or => "or",
        BinaryOperation.Equal => "=",
        BinaryOperation.NotEqual => "!=",
        BinaryOperation.LessThen => "<",
        BinaryOperation.LessOrEqual => "<=",
        BinaryOperation.GreaterThen => ">",
        BinaryOperation.GreaterOrEqual => ">=",
});

public enum BinaryOperation
{
    Plus = 1,
    Minus = 2,
    Multiply = 3,
    Divide = 4,
    And = 5,
    Or = 6,
    Equal = 7,
    NotEqual = 8,
    LessThen = 9,
    LessOrEqual = 10,
    GreaterThen = 11,
    GreaterOrEqual = 12,
    Power = 13,
}

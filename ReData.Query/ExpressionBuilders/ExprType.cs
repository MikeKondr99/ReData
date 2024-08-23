namespace ReData.Query;

[Flags]
public enum ExprType
{
    Unknown = 0,
    Null = 1,
    Number = 2,
    Integer = 4,
    String = 8,
    Boolean = 16,
    OptionalNumber = ExprType.Number | ExprType.Null,
    OptionalString = ExprType.String | ExprType.Null,
    OptionalInteger = ExprType.Integer | ExprType.Null,
    OptionalBoolean = ExprType.Boolean | ExprType.Null,
}
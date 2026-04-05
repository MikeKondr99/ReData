using ReData.Query.Core.Types;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypes;
using static DataType;

public class TrigonometryFunctions : FunctionsDescriptor
{
    protected override void Functions()
    {
        int value = 0, value2 = 1;
        
        Method("Cos")
            .Doc("Высчитывает косинус угла")
            .Arg("radian", Number)
            .Returns(Number)
            .Templates(new()
            {
                [All & ~(ClickHouse | Oracle)] = $"COS({value})",
                [ClickHouse] = $"cos({value})",
                [Oracle] = $"COS(TO_BINARY_DOUBLE({value}))"
            });
        
        Method("Acos")
            .Doc("Высчитывает арккосинус угла")
            .Arg("radian", Number)
            .Returns(Number)
            .CustomNullPropagation(_=>true)
            .Templates(new()
            {
                [MySql] = $"ACOS({value})",
                [ClickHouse] = $"CASE WHEN isNaN(acos({value})) THEN NULL ELSE acos({value}) END",
                [PostgreSql | SqlServer] = $"CASE WHEN {value} < -1 OR {value} > 1 THEN NULL ELSE ACOS({value}) END",
                [Oracle] = $"CASE WHEN {value} < -1 OR {value} > 1 THEN NULL ELSE ACOS(TO_BINARY_DOUBLE({value})) END"
            });
        
        Method("Sin")
            .Doc("Вычисляет синус угла в радианах")
            .Arg("radian", Number)
            .Returns(Number)
            .Templates(new()
            {
                [All & ~(ClickHouse | Oracle)] = $"SIN({value})",
                [ClickHouse] = $"sin({value})",
                [Oracle] = $"SIN(TO_BINARY_DOUBLE({value}))"
            });

        Method("Asin")
            .Doc("Вычисляет арксинус, возвращает угол в радианах")
            .Arg("value", Number)
            .Returns(Number)
            .CustomNullPropagation(_ => true)
            .Templates(new()
            {
                [MySql] = $"ASIN({value})",
                [ClickHouse] = $"CASE WHEN isNaN(asin({value})) THEN NULL ELSE asin({value}) END",
                [PostgreSql | SqlServer] = $"CASE WHEN {value} < -1 OR {value} > 1 THEN NULL ELSE ASIN({value}) END",
                [Oracle] = $"CASE WHEN {value} < -1 OR {value} > 1 THEN NULL ELSE ASIN(TO_BINARY_DOUBLE({value})) END"
            });

        Method("Tan")
            .Doc("Вычисляет тангенс угла в радианах")
            .Arg("radian", Number)
            .Returns(Number)
            .Templates(new()
            {
                [All & ~(ClickHouse | Oracle)] = $"TAN({value})",
                [ClickHouse] = $"tan({value})",
                [Oracle] = $"TAN(TO_BINARY_DOUBLE({value}))"
            });

        Method("Atan")
            .Doc("Вычисляет арктангенс, возвращает угол в радианах")
            .Arg("value", Number)
            .Returns(Number)
            .Templates(new()
            {
                [All & ~(ClickHouse | Oracle)] = $"ATAN({value})",
                [ClickHouse] = $"atan({value})",
                [Oracle] = $"ATAN(TO_BINARY_DOUBLE({value}))"
            });

        Method("Atan2")
            .Doc("Вычисляет арктангенс y/x с учетом квадранта")
            .Arg("y", Number)
            .Arg("x", Number)
            .Returns(Number)
            .Templates(new()
            {
                [MySql | PostgreSql] = $"ATAN2({value2}, {value})",
                [ClickHouse] = $"atan2({value2}, {value})",
                [SqlServer] = $"ATN2({value2}, {value})",
                [Oracle] = $"ATAN2(TO_BINARY_DOUBLE({value2}), TO_BINARY_DOUBLE({value}))"
            });

        // Гиперболические функции
        Method("Cosh")
            .Doc("Вычисляет гиперболический косинус")
            .Arg("value", Number)
            .Returns(Number)
            .CustomNullPropagation(_=>true)
            .Templates(new()
            {
                [PostgreSql] = $"COSH({value})",
                [ClickHouse] = $"cosh({value})",
                [MySql | SqlServer] = $"(EXP({value}) + EXP(-{value}))/2",
                [Oracle] = $"(EXP(TO_BINARY_DOUBLE({value})) + EXP(-TO_BINARY_DOUBLE({value})))/2"
            });

        Method("Sinh")
            .Doc("Вычисляет гиперболический синус")
            .Arg("value", Number)
            .Returns(Number)
            .Templates(new()
            {
                [PostgreSql] = $"SINH({value})",
                [ClickHouse] = $"sinh({value})",
                [Oracle] = $"(EXP(TO_BINARY_DOUBLE({value})) - EXP(-TO_BINARY_DOUBLE({value})))/2",
                [SqlServer | MySql] = $"0.5*(EXP({value}) - EXP(-{value}))",
            });

        Method("Tanh")
            .Doc("Вычисляет гиперболический тангенс")
            .Arg("value", Number)
            .Returns(Number)
            .Templates(new()
            {
                [PostgreSql] = $"TANH({value})",
                [ClickHouse] = $"tanh({value})",
                [MySql | SqlServer] = $"(EXP(2*{value}) - 1)/(EXP(2*{value}) + 1)",
                [Oracle] = $"(EXP(2*TO_BINARY_DOUBLE({value})) - 1)/(EXP(2*TO_BINARY_DOUBLE({value})) + 1)"
            });

        // Обратные гиперболические функции
        Method("Acosh")
            .Doc("Вычисляет гиперболический арккосинус")
            .Arg("value", Number)
            .Returns(Number)
            .CustomNullPropagation(_=>true)
            .Templates(new()
            {
                [PostgreSql] = $"CASE WHEN {value} >= 1 THEN ACOSH({value}) ELSE NULL END",
                [ClickHouse] = $"if({value} >= 1, acosh({value}), NULL)",
                [MySql] = $"CASE WHEN {value} >= 1 THEN LN({value} + SQRT({value}*{value} - 1)) ELSE NULL END",
                [SqlServer] = $"CASE WHEN {value} >= 1 THEN LOG({value} + SQRT({value}*{value} - 1)) ELSE NULL END",
                [Oracle] = $"""
                            CASE 
                                WHEN {value} >= 1 THEN 
                                    LN(TO_BINARY_DOUBLE({value}) + SQRT(TO_BINARY_DOUBLE({value})*TO_BINARY_DOUBLE({value}) - 1)) 
                                ELSE NULL 
                            END
                            """
            });

        Method("Asinh")
            .Doc("Вычисляет гиперболический арксинус")
            .Arg("value", Number)
            .Returns(Number)
            .Templates(new()
            {
                [PostgreSql] = $"ASINH({value})",
                [ClickHouse] = $"asinh({value})",
                [MySql] = $"LN({value} + SQRT({value}*{value} + 1))",
                [SqlServer] = $"LOG({value} + SQRT({value}*{value} + 1))",
                [Oracle] = $"LN(TO_BINARY_DOUBLE({value}) + SQRT(TO_BINARY_DOUBLE({value})*TO_BINARY_DOUBLE({value}) + 1))"
            });

        Method("Atanh")
            .Doc("Вычисляет гиперболический арктангенс")
            .Arg("value", Number)
            .Returns(Number)
            .CustomNullPropagation(_=>true)
            .Templates(new()
            {
                [PostgreSql] = $"CASE WHEN ABS({value}) < 1 THEN ATANH({value}) ELSE NULL END",
                [ClickHouse] = $"if(ABS({value}) < 1, atanh({value}), NULL)",
                [MySql] = $"CASE WHEN ABS({value}) < 1 THEN 0.5*LN((1 + {value})/(1 - {value})) ELSE NULL END",
                [SqlServer] = $"CASE WHEN ABS({value}) < 1 THEN 0.5*LOG((1 + {value})/(1 - {value})) ELSE NULL END",
                [Oracle] = $"""
                            CASE 
                                WHEN ABS(TO_BINARY_DOUBLE({value})) < 1 THEN 
                                    0.5*LN((1 + TO_BINARY_DOUBLE({value}))/(1 - TO_BINARY_DOUBLE({value}))) 
                                ELSE NULL 
                            END
                            """
            });

        // Дополнительные функции
        Method("Rad")
            .Doc("Преобразует градусы в радианы")
            .Arg("degrees", Number)
            .Returns(Number)
            .Templates(new()
            {
                [MySql] = $"RADIANS({value})",
                [Oracle] = $"TO_BINARY_DOUBLE({value}) * 3.141592653589793/180",
                [All & ~(MySql | Oracle)] = $"{value} * PI()/180"

            });

        Method("Deg")
            .Doc("Преобразует радианы в градусы")
            .Arg("radians", Number)
            .Returns(Number)
            .Templates(new()
            {
                [MySql] = $"DEGREES({value})",
                [Oracle] = $"TO_BINARY_DOUBLE({value}) * 180/3.141592653589793",
                [All & ~(MySql | Oracle)] = $"{value} * 180/PI()",
            });
    }
}
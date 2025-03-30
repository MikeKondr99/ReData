namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;
using static DataType;

public class DateFunctions : FunctionsDescriptor
{
    protected override void Functions()
    {
        int input = 0, start = 1, count = 2;
        int year = 0, month = 1, day = 2;
        
        Method("Date")
            .Arg("year", Integer)
            .Returns(DateTime)
            .Templates(new()
            {
                [SqlServer] = $"DATETIMEFROMPARTS({year}, 1, 1, 0, 0, 0, 0)",
                [MySql] = $"STR_TO_DATE(CONCAT({year}, '-01-01 00:00:00'), '%Y-%m-%d %H:%i:%s')",
                [PostgreSql] = $"MAKE_TIMESTAMP({year}, 1, 1, 0, 0, 0)",
                [Oracle] = $"TO_DATE({year}||'0101000000','YYYYMMDDHH24MISS')",
                [ClickHouse] = $"toDateTime(concat(toString({year}), '-01-01 00:00:00'))"
            });
        
        Method("Date")
            .Arg("year", Integer)
            .Arg("month", Integer)
            .Returns(DateTime)
            .Templates(new()
            {
                [SqlServer] = $"DATETIMEFROMPARTS({year}, {month}, 1, 0, 0, 0, 0)",
                [MySql] = $"STR_TO_DATE(CONCAT({year}, '-', {month}, '-01 00:00:00'), '%Y-%m-%d %H:%i:%s')",
                [PostgreSql] = $"MAKE_TIMESTAMP({year}, {month}, 1, 0, 0, 0)",
                [Oracle] = $"TO_DATE({year}||LPAD({month},2,'0')||'01000000','YYYYMMDDHH24MISS')",
                [ClickHouse] = $"toDateTime(concat(toString({year}), '-', toString({month}), '-01 00:00:00'))"
            });
        
        Method("Date")
            .Arg("year", Integer)
            .Arg("month", Integer)
            .Arg("day", Integer)
            .Returns(DateTime)
            .Templates(new()
            {
                [SqlServer] = $"DATETIMEFROMPARTS({year}, {month}, {day}, 0, 0, 0, 0)",
                [MySql] = $"STR_TO_DATE(CONCAT({year}, '-', {month}, '-', {day}, ' 00:00:00'), '%Y-%m-%d %H:%i:%s')",
                [PostgreSql] = $"MAKE_TIMESTAMP({year}, {month}, {day}, 0, 0, 0)",
                [Oracle] = $"TO_DATE({year}||LPAD({month},2,'0')||LPAD({day},2,'0')||'000000','YYYYMMDDHH24MISS')",
                [ClickHouse] = $"toDateTime(concat(toString({year}), '-', toString({month}), '-', toString({day}), ' 00:00:00'))"
            });
        
        Method("Now")
            .Returns(DateTime)
            .Templates(new()
            {
                [SqlServer] = $"GETUTCDATE()", // or GETDATE() for local time
                [MySql] = $"UTC_TIMESTAMP()", // or NOW() for local time
                [PostgreSql] = $"CURRENT_TIMESTAMP",
                [Oracle] = $"SYSTIMESTAMP AT TIME ZONE 'UTC'",
                [ClickHouse] = $"now()"
            });
        
        Method("Today")
            .Returns(DateTime)
            .Templates(new()
            {
                [SqlServer] = $"CAST(CAST(GETUTCDATE() AS DATE) AS DATETIME)",
                [MySql] = $"DATE_FORMAT(CURDATE(), '%Y-%m-%d 00:00:00')",
                [PostgreSql] = $"date_trunc('day', CURRENT_TIMESTAMP)",
                [Oracle] = $"TRUNC(SYSDATE)",
                [ClickHouse] = $"toStartOfDay(today())"
            });
        
        Method("AddDays")
            .Arg("input", DateTime)
            .Arg("days", Integer)
            .Returns(DateTime)
            .Templates(new()
            {
                [SqlServer] = $"DATEADD(day, {1}, {input})",
                [MySql] = $"DATE_ADD({input}, INTERVAL {1} DAY)",
                [PostgreSql] = $"({input} + ({1} * INTERVAL '1 DAY'))",
                [Oracle] = $"({input} + NUMTODSINTERVAL({1}, 'DAY'))",
                [ClickHouse] = $"dateAdd(day, {1}, {input})"
            });

        Method("AddMonths")
            .Arg("input", DateTime)
            .Arg("months", Integer)
            .Returns(DateTime)
            .Templates(new()
            {
                [SqlServer] = $"DATEADD(month, {1}, {input})",
                [MySql] = $"DATE_ADD({input}, INTERVAL {1} MONTH)",
                [PostgreSql] = $"({input} + ({1} * INTERVAL '1 MONTH'))",
                [Oracle] = $"ADD_MONTHS({input}, {1})", // Oracle has special optimized function
                [ClickHouse] = $"dateAdd(month, {1}, {input})"
            });

        Method("AddYears")
            .Arg("input", DateTime)
            .Arg("years", Integer)
            .Returns(DateTime)
            .Templates(new()
            {
                [SqlServer] = $"DATEADD(year, {1}, {input})",
                [MySql] = $"DATE_ADD({input}, INTERVAL {1} YEAR)",
                [PostgreSql] = $"({input} + ({1} * INTERVAL '1 YEAR'))",
                [Oracle] = $"ADD_MONTHS({input}, {1} * 12)", // Oracle uses months for year arithmetic
                [ClickHouse] = $"dateAdd(year, {1}, {input})"
            });


        // Year extraction
        Method("Year")
            .Arg("input", DateTime)
            .Returns(Integer)
            .Templates(new()
            {
                [SqlServer] = $"YEAR({input})",
                [MySql] = $"YEAR({input})",
                [PostgreSql] = $"EXTRACT(YEAR FROM {input})",
                [Oracle] = $"EXTRACT(YEAR FROM {input})",
                [ClickHouse] = $"toYear({input})"
            });

        // Month extraction (1-12)
        Method("Month")
            .Arg("input", DateTime)
            .Returns(Integer)
            .Templates(new()
            {
                [SqlServer] = $"MONTH({input})",
                [MySql] = $"MONTH({input})",
                [PostgreSql] = $"EXTRACT(MONTH FROM {input})",
                [Oracle] = $"EXTRACT(MONTH FROM {input})",
                [ClickHouse] = $"toMonth({input})"
            });

        // Day extraction (1-31)
        Method("Day")
            .Arg("input", DateTime)
            .Returns(Integer)
            .Templates(new()
            {
                [SqlServer] = $"DAY({input})",
                [MySql] = $"DAYOFMONTH({input})",
                [PostgreSql] = $"EXTRACT(DAY FROM {input})",
                [Oracle] = $"EXTRACT(DAY FROM {input})",
                [ClickHouse] = $"toDayOfMonth({input})"
            });

        // Hour extraction (0-23)
        Method("Hour")
            .Arg("input", DateTime)
            .Returns(Integer)
            .Templates(new()
            {
                [SqlServer] = $"DATEPART(HOUR, {input})",
                [MySql] = $"HOUR({input})",
                [PostgreSql] = $"EXTRACT(HOUR FROM {input})",
                [Oracle] = $"EXTRACT(HOUR FROM CAST({input} AS TIMESTAMP))",
                [ClickHouse] = $"toHour({input})"
            });

        // Minute extraction (0-59)
        Method("Minute")
            .Arg("input", DateTime)
            .Returns(Integer)
            .Templates(new()
            {
                [SqlServer] = $"DATEPART(MINUTE, {input})",
                [MySql] = $"MINUTE({input})",
                [PostgreSql] = $"EXTRACT(MINUTE FROM {input})",
                [Oracle] = $"EXTRACT(MINUTE FROM CAST({input} AS TIMESTAMP))",
                [ClickHouse] = $"toMinute({input})"
            });

        // Second extraction (0-59)
        Method("Second")
            .Arg("input", DateTime)
            .Returns(Integer)
            .Templates(new()
            {
                [SqlServer] = $"DATEPART(SECOND, {input})",
                [MySql] = $"SECOND({input})",
                [PostgreSql] = $"EXTRACT(SECOND FROM {input})",
                [Oracle] = $"EXTRACT(SECOND FROM CAST({input} AS TIMESTAMP))",
                [ClickHouse] = $"toSecond({input})"
            });
        
        Method("Quarter")
            .Arg("input", DateTime)
            .Returns(Integer)
            .Templates(new()
            {
                [SqlServer] = $"DATEPART(QUARTER, {input})",
                [MySql] = $"QUARTER({input})",
                [PostgreSql] = $"EXTRACT(QUARTER FROM {input})",
                [Oracle] = $"TO_NUMBER(TO_CHAR({input}, 'Q'))",
                [ClickHouse] = $"toQuarter({input})"
            });
        
        Method("YearMonth")
            .Arg("input", DateTime)
            .Returns(Text)
            .Templates(new()
            {
                [SqlServer] = $"FORMAT({input}, 'yyyy-MM')",
                [MySql] = $"DATE_FORMAT({input}, '%Y-%m')",
                [PostgreSql] = $"TO_CHAR({input}, 'YYYY-MM')",
                [Oracle] = $"TO_CHAR({input}, 'YYYY-MM')",
                [ClickHouse] = $"formatDateTime({input}, '%Y-%m')"
            });
        
        Method("DateOnly")
            .Arg("input", DateTime)
            .Returns(DateTime)
            .Templates(new()
            {
                [SqlServer] = $"CAST({input} AS DATE)",
                [MySql] = $"DATE({input})",
                [PostgreSql] = $"DATE_TRUNC('day', {input})",
                [Oracle] = $"TRUNC({input})",
                [ClickHouse] = $"toDate({input})"
            });
        
        Method("YearQuarter")
            .Arg("input", DateTime)
            .Returns(Text)
            .Templates(new()
            {
                [SqlServer] = $"CONCAT(YEAR({input}), '-Q', DATEPART(QUARTER, {input}))",
                [MySql] = $"CONCAT(YEAR({input}), '-Q', QUARTER({input}))",
                [PostgreSql] = $"TO_CHAR({input}, 'YYYY-\"Q\"Q')",
                [Oracle] = $"TO_CHAR({input}, 'YYYY-\"Q\"Q')",
                [ClickHouse] = $"concat(toString(toYear({input})), '-Q', toString(toQuarter({input})))"
            });
        
        // True ISO Week (YYYY-'W'01 to YYYY-'W'53)
        Method("YearWeek")
            .Arg("input", DateTime)
            .Returns(Text)
            .Templates(new()
            {
                [SqlServer] = $"FORMAT({input}, 'yyyy') + '-W' + RIGHT('00' + CAST(DATEPART(week, {input}) AS VARCHAR(2)), 2)",
                [MySql] = $"DATE_FORMAT({input}, '%x-W%v')", // ISO week (uses %x for year, %v for week)
                [PostgreSql] = $"TO_CHAR({input}, 'IYYY-\"W\"IW')", // IYYY=ISO year, IW=ISO week
                [Oracle] = $"TO_CHAR({input}, 'IYYY-\"W\"IW')",
                [ClickHouse] = $"formatDateTime({input}, '%Y-W%V')" // %V=ISO week number
            });
        
        Method("DayOfYear")
            .Arg("input", DateTime)
            .Returns(Integer)
            .Templates(new()
            {
                [SqlServer] = $"DATEPART(DAYOFYEAR, {input})",
                [MySql] = $"DAYOFYEAR({input})",
                [PostgreSql] = $"EXTRACT(DOY FROM {input})", // DOY = Day Of Year
                [Oracle] = $"TO_NUMBER(TO_CHAR({input}, 'DDD'))", // Returns 1-366 as text
                [ClickHouse] = $"toDayOfYear({input})"
            });
        
        // ISO Standard (Monday=1, Sunday=7)
        Method("DayOfWeek")
            .Arg("input", DateTime)
            .Returns(Integer)
            .Templates(new()
            {
                [PostgreSql] = $"EXTRACT(ISODOW FROM {input})", // PostgreSQL native ISO
                [ClickHouse] = $"toDayOfWeek({input})", // ClickHouse returns Mon=1 to Sun=7
                [MySql] = $"WEEKDAY({input}) + 1", // MySQL's WEEKDAY(): Mon=0, Sun=6 → +1
                [SqlServer] = $"(DATEPART(WEEKDAY, {input}) + 5) % 7 + 1", // Converts Sun=1 to Mon=1
                [Oracle] = $"MOD(TO_NUMBER(TO_CHAR({input}, 'D')) - 2 + 7, 7) + 1" // 'D'=1(Sun) to 7(Sat)
            });
        
        // ISO Standard (Week 1 contains January 4th)
        Method("Week")
            .Arg("input", DateTime)
            .Returns(Integer)
            .Templates(new()
            {
                [PostgreSql] = $"EXTRACT(WEEK FROM {input})",
                [ClickHouse] = $"toISOWeek({input})", // ClickHouse-specific function
                [MySql] = $"WEEK({input}, 3)", // Mode 3: ISO rules
                [SqlServer] = $"DATEPART(ISO_WEEK, {input})", // Built-in ISO
                [Oracle] = $"TO_NUMBER(TO_CHAR({input}, 'IW'))" // 'IW'=ISO week
            });
        
    }
}
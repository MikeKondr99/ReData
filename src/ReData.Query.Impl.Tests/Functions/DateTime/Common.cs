using ReData.Query.Impl.Tests.Fixtures;

namespace ReData.Query.Impl.Tests.Functions.DateTimeFunc;

public abstract class Сommon(IDatabaseFixture runner) : ExprTests(runner)
{
     
    [Theory(DisplayName = "AddDays")]
    [InlineData("Date('2023-01-01').AddDays(1)", "@2023-01-02 00:00")] // Simple increment
    [InlineData("Date('2023-01-01 08:30').AddDays(1)", "@2023-01-02 08:30")] // Preserves time
    [InlineData("Date('2023-12-31').AddDays(1)", "@2024-01-01 00:00")] // Year rollover
    [InlineData("Date('2024-02-28').AddDays(1)", "@2024-02-29 00:00")] // Leap day
    [InlineData("Date('2023-02-28').AddDays(1)", "@2023-03-01 00:00")] // Non-leap year
    [InlineData("Date('2023-01-01').AddDays(-1)", "@2022-12-31 00:00")] // Negative days
    [InlineData("Date('2023-01-01 16:45').AddDays(0)", "@2023-01-01 16:45")] // Zero days
    [InlineData("Date('2023-01-01').AddDays(365)", "@2024-01-01 00:00")] // Full year
    public Task FuncAddDaysTests(string expr, object? expected) => Test(expr, expected);
    
    [Theory(DisplayName = "AddMonths")]
    [InlineData("Date('2023-01-15').AddMonths(1)", "@2023-02-15 00:00")] // Simple increment
    [InlineData("Date('2023-01-31').AddMonths(1)", "@2023-02-28 00:00")] // Month length adjustment
    [InlineData("Date('2024-01-31').AddMonths(1)", "@2024-02-29 00:00")] // Leap year adjustment
    [InlineData("Date('2023-12-15').AddMonths(1)", "@2024-01-15 00:00")] // Year rollover
    [InlineData("Date('2023-01-15 12:30').AddMonths(1)", "@2023-02-15 12:30")] // Preserves time
    [InlineData("Date('2023-01-15').AddMonths(-1)", "@2022-12-15 00:00")] // Negative months
    [InlineData("Date('2023-01-31').AddMonths(12)", "@2024-01-31 00:00")] // Full year
    [InlineData("Date('2023-01-15').AddMonths(0)", "@2023-01-15 00:00")] // Zero months
    [InlineData("Date('2023-01-01').AddMonths(12 * 80)", "@2103-01-01 00:00")] // Large month addition
    public Task FuncAddMonthsTests(string expr, object? expected) => Test(expr, expected);
    
    [Theory(DisplayName = "AddYears")]
    [InlineData("Date('2023-02-15').AddYears(1)", "@2024-02-15 00:00")] // Simple increment
    [InlineData("Date('2024-02-29').AddYears(1)", "@2025-02-28 00:00")] // Leap day adjustment
    [InlineData("Date('2023-02-15 18:20').AddYears(1)", "@2024-02-15 18:20")] // Preserves time
    [InlineData("Date('2023-02-15').AddYears(-1)", "@2022-02-15 00:00")] // Negative years
    [InlineData("Date('2023-02-15').AddYears(0)", "@2023-02-15 00:00")] // Zero years
    [InlineData("Date('2020-02-29').AddYears(4)", "@2024-02-29 00:00")] // Leap to leap
    [InlineData("Date('2023-01-01').AddYears(50)", "@2073-01-01 00:00")] // Large year addition
    public Task FuncAddYearsTests(string expr, object? expected) => Test(expr, expected);
    
    [Theory(DisplayName = "Year")]
    [InlineData("Date('2023-05-15').Year()", 2023)] // Basic year
    [InlineData("Date('2024-02-29 14:30:22').Year()", 2024)] // Leap year
    [InlineData("Date('2023-01-01 00:00:00').Year()", 2023)] // Midnight
    [InlineData("Date('1999-12-31 23:59:59').Year()", 1999)] // Year boundary
    public Task FuncYearTests(string expr, object? expected) => Test(expr, expected);
    
    [Theory(DisplayName = "Month")]
    [InlineData("Date('2023-05-15').Month()", 5)] // Basic month
    [InlineData("Date('2023-01-01').Month()", 1)] // January
    [InlineData("Date('2023-12-31').Month()", 12)] // December
    [InlineData("Date('2024-02-29 14:30:22').Month()", 2)] // February leap
    [InlineData("Date('2023-06-15 23:59:59').Month()", 6)] // End of day
    [InlineData("Date('2023-07-01 00:00:00').Month()", 7)] // Start of month
    public Task FuncMonthTests(string expr, object? expected) => Test(expr, expected);
    
    [Theory(DisplayName = "Day")]
    [InlineData("Date('2023-05-15').Day()", 15)] // Basic day
    [InlineData("Date('2023-01-01').Day()", 1)] // First day
    [InlineData("Date('2023-05-31').Day()", 31)] // Last day
    [InlineData("Date('2024-02-29').Day()", 29)] // Leap day
    [InlineData("Date('2023-04-15 14:30:22').Day()", 15)] // With time
    [InlineData("Date('2023-12-31 23:59:59').Day()", 31)] // Year end
    [InlineData("Date('1970-01-01').Day()", 1)] // Unix epoch
    [InlineData("Date('2038-01-19').Day()", 19)] // Y2038 boundary
    [InlineData("Date('2079-12-31').Day()", 31)] // Future boundary
    [InlineData("Date('2023-06-15 14:30:22').Day()", 15)] // With time
    public Task FuncDayTests(string expr, object? expected) => Test(expr, expected);
    
    [Theory(DisplayName = "Hour")]
    [InlineData("Date('2023-05-15 14:30:22').Hour()", 14)] // Basic hour
    [InlineData("Date('2023-01-01 00:00:00').Hour()", 0)] // Midnight
    [InlineData("Date('2023-12-31 23:59:59').Hour()", 23)] // Max hour
    [InlineData("Date('2023-07-04 12:30:00').Hour()", 12)] // Noon
    [InlineData("Date('2023-03-15 04:05:06').Hour()", 4)] // Single digit
    [InlineData("Date('2023-01-01').Hour()", 0)] // Date-only defaults
    public Task FuncHourTests(string expr, object? expected) => Test(expr, expected);
    
    [Theory(DisplayName = "Minute")]
    [InlineData("Date('2023-05-15 14:30:22').Minute()", 30)] // Basic minute
    [InlineData("Date('2023-01-01 00:00:00').Minute()", 0)] // Start of hour
    [InlineData("Date('2023-12-31 23:59:59').Minute()", 59)] // Max minute
    [InlineData("Date('2023-07-04 12:05:00').Minute()", 5)] // Single digit
    [InlineData("Date('2023-03-15 04:30:06').Minute()", 30)] // Half hour
    [InlineData("Date('2023-01-01').Minute()", 0)] // Date-only defaults
    public Task FuncMinuteTests(string expr, object? expected) => Test(expr, expected);
    
    [Theory(DisplayName = "Quarter")]
    [InlineData("Date('2023-01-15').Quarter()", 1)] // Q1 (Jan-Mar)
    [InlineData("Date('2023-04-01').Quarter()", 2)] // Q2 (Apr-Jun)
    [InlineData("Date('2023-07-15').Quarter()", 3)] // Q3 (Jul-Sep)
    [InlineData("Date('2023-10-31').Quarter()", 4)] // Q4 (Oct-Dec)
    [InlineData("Date('2023-03-31 23:59:59').Quarter()", 1)] // Q1 boundary
    [InlineData("Date('2023-06-30').Quarter()", 2)] // Q2 boundary
    [InlineData("Date('1970-01-01').Quarter()", 1)] // Epoch Q1
    [InlineData("Date('2079-12-31').Quarter()", 4)] // Future Q4
    public Task FuncQuarterTests(string expr, object? expected) => Test(expr, expected);
    
    [Theory(DisplayName = "YearMonth")]
    [InlineData("Date('2023-05-15').YearMonth()", "2023-05")] // Basic case
    [InlineData("Date('2023-01-01 14:30:22').YearMonth()", "2023-01")] // With time
    [InlineData("Date('2024-02-29').YearMonth()", "2024-02")] // Leap year
    [InlineData("Date('1970-12-31').YearMonth()", "1970-12")] // Epoch year
    [InlineData("Date('2079-01-01').YearMonth()", "2079-01")] // Future year
    [InlineData("Date('2023-11-30 23:59:59').YearMonth()", "2023-11")] // Month end
    public Task FuncYearMonthTests(string expr, object? expected) => Test(expr, expected);
    
    
    [Theory(DisplayName = "DateOnly")]
    [InlineData("Date('2023-05-15').DateOnly()", "@2023-05-15")] // Basic date
    [InlineData("Date('2023-01-01 14:30:22').DateOnly()", "@2023-01-01")] // With time
    [InlineData("Date('2024-02-29 23:59:59').DateOnly()", "@2024-02-29")] // Leap day
    [InlineData("Date('1970-01-01').DateOnly()", "@1970-01-01")] // Epoch date
    [InlineData("Date('2079-12-31 00:00:00').DateOnly()", "@2079-12-31")] // Future date
    [InlineData("Date('2023-12-31 23:59:59').DateOnly()", "@2023-12-31")] // Year end
    public Task FuncDateOnlyTests(string expr, object? expected) => Test(expr, expected);
    
    [Theory(DisplayName = "YearQuarter")]
    [InlineData("Date('2023-01-15').YearQuarter()", "2023-Q1")] // Q1
    [InlineData("Date('2023-04-01').YearQuarter()", "2023-Q2")] // Q2
    [InlineData("Date('2023-07-15').YearQuarter()", "2023-Q3")] // Q3
    [InlineData("Date('2023-10-31').YearQuarter()", "2023-Q4")] // Q4
    [InlineData("Date('2023-03-31 23:59:59').YearQuarter()", "2023-Q1")] // Q1 boundary
    [InlineData("Date('1970-01-01').YearQuarter()", "1970-Q1")] // Epoch Q1
    [InlineData("Date('2079-12-31').YearQuarter()", "2079-Q4")] // Future Q4
    [InlineData("Date('2020-02-29').YearQuarter()", "2020-Q1")] // Leap year Q1
    public Task FuncYearQuarterTests(string expr, object? expected) => Test(expr, expected);
    
    [Theory(DisplayName = "YearWeek (ISO)")]
    // Standard cases
    [InlineData("Date('2023-01-02').YearWeek()", "2023-W01")] // First Monday of 2023
    [InlineData("Date('2023-01-01').YearWeek()", "2022-W52")] // Sunday before first week
    [InlineData("Date('2023-12-31').YearWeek()", "2023-W52")] // Last Sunday of year
    [InlineData("Date('2024-01-01').YearWeek()", "2024-W01")] // Leap year start
    [InlineData("Date('2020-12-31').YearWeek()", "2020-W53")] // Year with 53 weeks

    // Boundary cases
    [InlineData("Date('1970-01-01').YearWeek()", "1970-W01")] // Unix epoch
    [InlineData("Date('2079-12-31').YearWeek()", "2079-W52")] // Future boundary
    [InlineData("Date('2019-12-30').YearWeek()", "2020-W01")] // Week starts in next year
    [InlineData("Date('2021-01-04').YearWeek()", "2021-W01")] // First Monday

    // With time components
    [InlineData("Date('2023-06-15 14:30:22').YearWeek()", "2023-W24")] // Mid-year with time
    [InlineData("Date('2023-01-01 23:59:59').YearWeek()", "2022-W52")] // Last second of prev year
    public Task FuncYearWeekTests(string expr, object? expected) => Test(expr, expected);
    
    [Theory(DisplayName = "DayOfYear")]
    // Standard cases
    [InlineData("Date('2023-01-01').DayOfYear()", 1)] // January 1st
    [InlineData("Date('2023-02-01').DayOfYear()", 32)] // February 1st
    [InlineData("Date('2023-12-31').DayOfYear()", 365)] // Non-leap year end
    [InlineData("Date('2024-12-31').DayOfYear()", 366)] // Leap year end

    // Boundary cases
    [InlineData("Date('1970-01-01').DayOfYear()", 1)] // Unix epoch
    [InlineData("Date('2079-12-31').DayOfYear()", 365)] // Future boundary
    [InlineData("Date('2020-02-29').DayOfYear()", 60)] // Leap day

    // With time components
    [InlineData("Date('2023-07-01 00:00:00').DayOfYear()", 182)] // Year midpoint
    [InlineData("Date('2023-03-15 12:30:45').DayOfYear()", 74)] // With exact time
    [InlineData("Date('2023-12-31 23:59:59').DayOfYear()", 365)] // Last second
    public Task FuncDayOfYearTests(string expr, object? expected) => Test(expr, expected);
    
    [SkippableTheory]
    [InlineData("Date('2023-05-15').DayOfWeek()", 1)] // Monday
    [InlineData("Date('2023-05-16').DayOfWeek()", 2)] // Tuesday
    [InlineData("Date('2023-05-17').DayOfWeek()", 3)] // Wednesday
    [InlineData("Date('2023-05-18').DayOfWeek()", 4)] // Thursday
    [InlineData("Date('2023-05-19').DayOfWeek()", 5)] // Friday
    [InlineData("Date('2023-05-20').DayOfWeek()", 6)] // Saturday
    [InlineData("Date('2023-05-21').DayOfWeek()", 7)] // Sunday

    // Edge cases
    [InlineData("Date('2023-01-01').DayOfWeek()", 7)] // Sunday (2023-01-01)
    [InlineData("Date('2023-01-02').DayOfWeek()", 1)] // Monday (2023-01-02)
    [InlineData("Date('1970-01-01').DayOfWeek()", 4)] // Thursday (Unix epoch)

    // With time components
    [InlineData("Date('2023-05-15 14:30:22').DayOfWeek()", 1)] // Monday with time
    [InlineData("Date('2023-05-21 23:59:59').DayOfWeek()", 7)] // Sunday end of day
    public Task FuncDayOfWeekTests(string expr, object? expected)
    {
        Skip.If(runner.GetDatabaseType() is DatabaseType.Oracle);
        return Test(expr, expected);
    }

    [Theory(DisplayName = "Week (ISO)")]
    // Standard cases
    [InlineData("Date('2023-01-02').Week()", 1)] // First Monday of 2023 (Week 1)
    [InlineData("Date('2023-01-01').Week()", 52)] // Sunday before first week (belongs to previous year)
    [InlineData("Date('2023-12-31').Week()", 52)] // Last Sunday of 2023
    [InlineData("Date('2024-01-01').Week()", 1)] // Monday (Week 1 of 2024)
    [InlineData("Date('2020-12-31').Week()", 53)] // Year with 53 weeks

    // Boundary cases
    [InlineData("Date('1970-01-01').Week()", 1)] // Unix epoch (Thursday - Week 1)
    [InlineData("Date('1970-01-04').Week()", 1)] // Contains Jan 4th (Sunday - Week 1)
    [InlineData("Date('2079-12-31').Week()", 52)] // Future boundary (Saturday)
    [InlineData("Date('2019-12-30').Week()", 1)] // Monday (Week 1 of 2020)
    [InlineData("Date('2021-01-04').Week()", 1)] // Contains Jan 4th (Monday - Week 1)

    // With time components
    [InlineData("Date('2023-06-15 14:30:22').Week()", 24)] // Mid-year with time
    [InlineData("Date('2023-01-01 23:59:59').Week()", 52)] // Last second of previous ISO year
    public Task FuncWeekTests(string expr, object? expected) => Test(expr, expected);
    
    [Theory(DisplayName = "Date(year)")]
    [InlineData("Date(2023)", "@2023-01-01 00:00:00")]
    [InlineData("Date(1970)", "@1970-01-01 00:00:00")] // Unix epoch
    [InlineData("Date(2079)", "@2079-01-01 00:00:00")] // Upper boundary
    [InlineData("Date(2023).Hour()", 0)] // Verify hour extraction
    [InlineData("Date(2024).Day()", 1)] // Verify day extraction (leap year)
    public Task DateYearConstructor(string expr, object? expected) => Test(expr, expected);
    
    [Theory(DisplayName = "Date(year, month)")]
    [InlineData("Date(2023, 5)", "@2023-05-01 00:00:00")] // Basic case
    [InlineData("Date(2023, 12)", "@2023-12-01 00:00:00")] // December
    [InlineData("Date(2024, 2)", "@2024-02-01 00:00:00")] // Leap year
    [InlineData("Date(2023, 5).Hour()", 0)] // Verify hour extraction
    [InlineData("Date(2023, 6).Month()", 6)] // Verify month extraction
    [InlineData("Date(1970, 1).Year()", 1970)] // Epoch year
    public Task DateYearMonthConstructor(string expr, object? expected) => Test(expr, expected);
    
    [Theory(DisplayName = "Date(year, month, day)")]
    [InlineData("Date(2023, 5, 15)", "@2023-05-15 00:00:00")] // Basic case
    [InlineData("Date(2023, 12, 31)", "@2023-12-31 00:00:00")] // Year end
    [InlineData("Date(2024, 2, 29)", "@2024-02-29 00:00:00")] // Leap day
    [InlineData("Date(1970, 1, 1)", "@1970-01-01 00:00:00")] // Epoch
    [InlineData("Date(2023, 5, 15).Hour()", 0)] // Verify hour extraction
    [InlineData("Date(2023, 5, 15).Day()", 15)] // Verify day extraction
    [InlineData("Date(2079, 12, 31).Year()", 2079)] // Future boundary
    public Task DateFullConstructor(string expr, object? expected) => Test(expr, expected);
    
    [Theory(DisplayName = "Now()")]
    // [InlineData("Now() >= DateTime(1970, 1, 1)", true)] // After Unix epoch
    // [InlineData("Now() <= DateTime(2080, 1, 1)", true)] // Before upper boundary
    [InlineData("Now().Year() >= 1970", true)] // Valid year range
    public Task FuncNowTests(string expr, object? expected) => Test(expr, expected);
    
    [Theory(DisplayName = "Today()")]
    // [InlineData("Today() = Now().DateOnly()", true)] // Matches date portion of Now()
    // [InlineData("Today() >= DateTime(1970, 1, 1)", true)] // After epoch
    // [InlineData("Today() <= DateTime(2080, 1, 1)", true)] // Before boundary
    [InlineData("Today().Hour()", 0)] // Always midnight
    [InlineData("Today().Minute()", 0)] // Always midnight
    [InlineData("Today().Second()", 0)] // Always midnight
    // [InlineData("Today().AddDays(1) > Today()", true)] // Tomorrow comparison
    public Task FuncTodayTests(string expr, object? expected) => Test(expr, expected);
    
}
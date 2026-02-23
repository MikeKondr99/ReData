using Mysqlx.Crud;
using ReData.Query.Core;
using ReData.Query.Core.Types;
using ReData.Query.Core.Value;

namespace ReData.Query.Impl.Tests.Queries;

public class PostgresAssets : BaseTestAssets
{
    public override DatabaseType DatabaseType { get; } = DatabaseType.PostgreSql;
}

public sealed class ClickHouseAssets : BaseTestAssets
{
    public override DatabaseType DatabaseType { get; } = DatabaseType.ClickHouse;
}

public sealed class OracleAssets : BaseTestAssets
{
    public override DatabaseType DatabaseType { get; } = DatabaseType.Oracle;
}

public sealed class MySqlAssets : BaseTestAssets
{
    public override DatabaseType DatabaseType { get; } = DatabaseType.MySql;
}

public sealed class SqlServerAssets : BaseTestAssets
{
    public override DatabaseType DatabaseType { get; } = DatabaseType.SqlServer;
}

public abstract class BaseTestAssets : ITestAssets
{
    protected static NumberValue Val(double val) => new NumberValue(val);
    protected static IntegerValue Val(int val) => new IntegerValue(val);
    protected static TextValue Val(string val) => new TextValue(val);
    protected static DateTimeValue Val(DateTime val) => new DateTimeValue(val);

    public abstract DatabaseType DatabaseType { get; }

    public QueryBuilder CreateUsersQuery(IConstantRuntime? contantRuntime = null)
    {
        IReadOnlyList<(string name, string column, FieldType type)> fields = [
            ("UserId", "UserId", new FieldType(DataType.Integer, false)),
            ("FirstName", "FirstName", new FieldType(DataType.Text, false)),
            ("LastName", "LastName", new FieldType(DataType.Text, false)),
            ("Age", "Age", new FieldType(DataType.Integer, false)),
            ("Salary", "Salary", new FieldType(DataType.Number, false)),
            ("DateOfBirth", "DateOfBirth", new FieldType(DataType.DateTime, false)),
            ("JoinDate", "JoinDate", new FieldType(DataType.DateTime, false)),
            ("LastLoginDate", "LastLoginDate", new FieldType(DataType.DateTime, false)),
            ("Notes", "Notes", new FieldType(DataType.Text, false)),
        ];
        
        return QueryBuilder.FromTable(
            Factory.CreateExpressionResolver(DatabaseType),
            Factory.CreateFunctionStorage(DatabaseType),
            ["User"],
            fields,
            contantRuntime
        );
    }

    public dynamic[] UsersDynamicArray { get; } =
    [
        new
        {
            UserId = 1,
            FirstName = "John",
            LastName = "Doe",
            Age = 30,
            Salary = 50000.50,
            DateOfBirth = DateTime.Parse("1990-01-15"),
            JoinDate = DateTime.Parse("2020-05-10"),
            LastLoginDate = DateTime.Parse("2023-10-01"),
            Notes = "Regular user"
        },
        new
        {
            UserId = 2,
            FirstName = "Jane",
            LastName = "Smith",
            Age = 25,
            Salary = 60000.00,
            DateOfBirth = DateTime.Parse("1995-07-22"),
            JoinDate = DateTime.Parse("2021-03-15"),
            LastLoginDate = DateTime.Parse("2023-09-28"),
            Notes = "Active user"
        },
        new
        {
            UserId = 3,
            FirstName = "John",
            LastName = "Doe",
            Age = 30,
            Salary = 55000.75,
            DateOfBirth = DateTime.Parse("1990-01-15"),
            JoinDate = DateTime.Parse("2022-01-20"),
            LastLoginDate = DateTime.Parse("2023-10-02"),
            Notes = "Promoted user"
        },
        new
        {
            UserId = 4,
            FirstName = "Alice",
            LastName = "Johnson",
            Age = 40,
            Salary = 75000.00,
            DateOfBirth = DateTime.Parse("1980-11-30"),
            JoinDate = DateTime.Parse("2019-11-01"),
            LastLoginDate = DateTime.Parse("2023-09-30"),
            Notes = "Manager"
        },
        new
        {
            UserId = 5,
            FirstName = "Jane",
            LastName = "Smith",
            Age = 25,
            Salary = 62000.50,
            DateOfBirth = DateTime.Parse("1995-07-22"),
            JoinDate = DateTime.Parse("2021-03-15"),
            LastLoginDate = DateTime.Parse("2023-10-03"),
            Notes = "Active user"
        },
        new
        {
            UserId = 6,
            FirstName = "Bob",
            LastName = "Brown",
            Age = 35,
            Salary = 45000.00,
            DateOfBirth = DateTime.Parse("1985-05-10"),
            JoinDate = DateTime.Parse("2020-06-01"),
            LastLoginDate = DateTime.Parse("2023-09-25"),
            Notes = "New user"
        },
        new
        {
            UserId = 7,
            FirstName = "Alice",
            LastName = "Johnson",
            Age = 40,
            Salary = 80000.00,
            DateOfBirth = DateTime.Parse("1980-11-30"),
            JoinDate = DateTime.Parse("2019-11-01"),
            LastLoginDate = DateTime.Parse("2023-10-04"),
            Notes = "Senior Manager"
        },
        new
        {
            UserId = 8,
            FirstName = "Mike",
            LastName = "Davis",
            Age = 28,
            Salary = 48000.00,
            DateOfBirth = DateTime.Parse("1993-02-14"),
            JoinDate = DateTime.Parse("2021-07-15"),
            LastLoginDate = DateTime.Parse("2023-09-29"),
            Notes = "Junior Developer"
        },
        new
        {
            UserId = 9,
            FirstName = "Sarah",
            LastName = "Wilson",
            Age = 32,
            Salary = 70000.00,
            DateOfBirth = DateTime.Parse("1989-08-20"),
            JoinDate = DateTime.Parse("2018-12-01"),
            LastLoginDate = DateTime.Parse("2023-10-05"),
            Notes = "Team Lead"
        },
        new
        {
            UserId = 10,
            FirstName = "John",
            LastName = "Doe",
            Age = 30,
            Salary = 60000.00,
            DateOfBirth = DateTime.Parse("1990-01-15"),
            JoinDate = DateTime.Parse("2020-05-10"),
            LastLoginDate = DateTime.Parse("2023-10-06"),
            Notes = "Regular user"
        },
        new
        {
            UserId = 11,
            FirstName = "Emily",
            LastName = "Clark",
            Age = 27,
            Salary = 52000.00,
            DateOfBirth = DateTime.Parse("1994-03-25"),
            JoinDate = DateTime.Parse("2022-02-10"),
            LastLoginDate = DateTime.Parse("2023-09-27"),
            Notes = "Intern"
        },
        new
        {
            UserId = 12,
            FirstName = "Jane",
            LastName = "Smith",
            Age = 25,
            Salary = 65000.00,
            DateOfBirth = DateTime.Parse("1995-07-22"),
            JoinDate = DateTime.Parse("2021-03-15"),
            LastLoginDate = DateTime.Parse("2023-10-07"),
            Notes = "Active user"
        },
        new
        {
            UserId = 13,
            FirstName = "Chris",
            LastName = "Evans",
            Age = 38,
            Salary = 90000.00,
            DateOfBirth = DateTime.Parse("1983-09-12"),
            JoinDate = DateTime.Parse("2017-10-01"),
            LastLoginDate = DateTime.Parse("2023-10-08"),
            Notes = "Director"
        },
        new
        {
            UserId = 14,
            FirstName = "Alice",
            LastName = "Johnson",
            Age = 40,
            Salary = 85000.00,
            DateOfBirth = DateTime.Parse("1980-11-30"),
            JoinDate = DateTime.Parse("2019-11-01"),
            LastLoginDate = DateTime.Parse("2023-10-09"),
            Notes = "Senior Manager"
        },
        new
        {
            UserId = 15,
            FirstName = "Bob",
            LastName = "Brown",
            Age = 35,
            Salary = 47000.00,
            DateOfBirth = DateTime.Parse("1985-05-10"),
            JoinDate = DateTime.Parse("2020-06-01"),
            LastLoginDate = DateTime.Parse("2023-10-10"),
            Notes = "New user"
        }
    ];

    public IReadOnlyList<Dictionary<string, IValue>> UsersData { get; } =
    [
        new()
        {
            ["UserId"] = Val(1),
            ["FirstName"] = Val("John"),
            ["LastName"] = Val("Doe"),
            ["Age"] = Val(30),
            ["Salary"] = Val(50000.50),
            ["DateOfBirth"] = Val(DateTime.Parse("1990-01-15")),
            ["JoinDate"] = Val(DateTime.Parse("2020-05-10")),
            ["LastLoginDate"] = Val(DateTime.Parse("2023-10-01")),
            ["Notes"] = Val("Regular user")
        },
        new()
        {
            ["UserId"] = Val(2),
            ["FirstName"] = Val("Jane"),
            ["LastName"] = Val("Smith"),
            ["Age"] = Val(25),
            ["Salary"] = Val(60000.00),
            ["DateOfBirth"] = Val(DateTime.Parse("1995-07-22")),
            ["JoinDate"] = Val(DateTime.Parse("2021-03-15")),
            ["LastLoginDate"] = Val(DateTime.Parse("2023-09-28")),
            ["Notes"] = Val("Active user")
        },
        new()
        {
            ["UserId"] = Val(3),
            ["FirstName"] = Val("John"),
            ["LastName"] = Val("Doe"),
            ["Age"] = Val(30),
            ["Salary"] = Val(55000.75),
            ["DateOfBirth"] = Val(DateTime.Parse("1990-01-15")),
            ["JoinDate"] = Val(DateTime.Parse("2022-01-20")),
            ["LastLoginDate"] = Val(DateTime.Parse("2023-10-02")),
            ["Notes"] = Val("Promoted user")
        },
        new()
        {
            ["UserId"] = Val(4),
            ["FirstName"] = Val("Alice"),
            ["LastName"] = Val("Johnson"),
            ["Age"] = Val(40),
            ["Salary"] = Val(75000.00),
            ["DateOfBirth"] = Val(DateTime.Parse("1980-11-30")),
            ["JoinDate"] = Val(DateTime.Parse("2019-11-01")),
            ["LastLoginDate"] = Val(DateTime.Parse("2023-09-30")),
            ["Notes"] = Val("Manager")
        },
        new()
        {
            ["UserId"] = Val(5),
            ["FirstName"] = Val("Jane"),
            ["LastName"] = Val("Smith"),
            ["Age"] = Val(25),
            ["Salary"] = Val(62000.50),
            ["DateOfBirth"] = Val(DateTime.Parse("1995-07-22")),
            ["JoinDate"] = Val(DateTime.Parse("2021-03-15")),
            ["LastLoginDate"] = Val(DateTime.Parse("2023-10-03")),
            ["Notes"] = Val("Active user")
        },
        new()
        {
            ["UserId"] = Val(6),
            ["FirstName"] = Val("Bob"),
            ["LastName"] = Val("Brown"),
            ["Age"] = Val(35),
            ["Salary"] = Val(45000.00),
            ["DateOfBirth"] = Val(DateTime.Parse("1985-05-10")),
            ["JoinDate"] = Val(DateTime.Parse("2020-06-01")),
            ["LastLoginDate"] = Val(DateTime.Parse("2023-09-25")),
            ["Notes"] = Val("New user")
        },
        new()
        {
            ["UserId"] = Val(7),
            ["FirstName"] = Val("Alice"),
            ["LastName"] = Val("Johnson"),
            ["Age"] = Val(40),
            ["Salary"] = Val(80000.00),
            ["DateOfBirth"] = Val(DateTime.Parse("1980-11-30")),
            ["JoinDate"] = Val(DateTime.Parse("2019-11-01")),
            ["LastLoginDate"] = Val(DateTime.Parse("2023-10-04")),
            ["Notes"] = Val("Senior Manager")
        },
        new()
        {
            ["UserId"] = Val(8),
            ["FirstName"] = Val("Mike"),
            ["LastName"] = Val("Davis"),
            ["Age"] = Val(28),
            ["Salary"] = Val(48000.00),
            ["DateOfBirth"] = Val(DateTime.Parse("1993-02-14")),
            ["JoinDate"] = Val(DateTime.Parse("2021-07-15")),
            ["LastLoginDate"] = Val(DateTime.Parse("2023-09-29")),
            ["Notes"] = Val("Junior Developer")
        },
        new()
        {
            ["UserId"] = Val(9),
            ["FirstName"] = Val("Sarah"),
            ["LastName"] = Val("Wilson"),
            ["Age"] = Val(32),
            ["Salary"] = Val(70000.00),
            ["DateOfBirth"] = Val(DateTime.Parse("1989-08-20")),
            ["JoinDate"] = Val(DateTime.Parse("2018-12-01")),
            ["LastLoginDate"] = Val(DateTime.Parse("2023-10-05")),
            ["Notes"] = Val("Team Lead")
        },
        new()
        {
            ["UserId"] = Val(10),
            ["FirstName"] = Val("John"),
            ["LastName"] = Val("Doe"),
            ["Age"] = Val(30),
            ["Salary"] = Val(60000.00),
            ["DateOfBirth"] = Val(DateTime.Parse("1990-01-15")),
            ["JoinDate"] = Val(DateTime.Parse("2020-05-10")),
            ["LastLoginDate"] = Val(DateTime.Parse("2023-10-06")),
            ["Notes"] = Val("Regular user")
        },
        new()
        {
            ["UserId"] = Val(11),
            ["FirstName"] = Val("Emily"),
            ["LastName"] = Val("Clark"),
            ["Age"] = Val(27),
            ["Salary"] = Val(52000.00),
            ["DateOfBirth"] = Val(DateTime.Parse("1994-03-25")),
            ["JoinDate"] = Val(DateTime.Parse("2022-02-10")),
            ["LastLoginDate"] = Val(DateTime.Parse("2023-09-27")),
            ["Notes"] = Val("Intern")
        },
        new()
        {
            ["UserId"] = Val(12),
            ["FirstName"] = Val("Jane"),
            ["LastName"] = Val("Smith"),
            ["Age"] = Val(25),
            ["Salary"] = Val(65000.00),
            ["DateOfBirth"] = Val(DateTime.Parse("1995-07-22")),
            ["JoinDate"] = Val(DateTime.Parse("2021-03-15")),
            ["LastLoginDate"] = Val(DateTime.Parse("2023-10-07")),
            ["Notes"] = Val("Active user")
        },
        new()
        {
            ["UserId"] = Val(13),
            ["FirstName"] = Val("Chris"),
            ["LastName"] = Val("Evans"),
            ["Age"] = Val(38),
            ["Salary"] = Val(90000.00),
            ["DateOfBirth"] = Val(DateTime.Parse("1983-09-12")),
            ["JoinDate"] = Val(DateTime.Parse("2017-10-01")),
            ["LastLoginDate"] = Val(DateTime.Parse("2023-10-08")),
            ["Notes"] = Val("Director")
        },
        new()
        {
            ["UserId"] = Val(14),
            ["FirstName"] = Val("Alice"),
            ["LastName"] = Val("Johnson"),
            ["Age"] = Val(40),
            ["Salary"] = Val(85000.00),
            ["DateOfBirth"] = Val(DateTime.Parse("1980-11-30")),
            ["JoinDate"] = Val(DateTime.Parse("2019-11-01")),
            ["LastLoginDate"] = Val(DateTime.Parse("2023-10-09")),
            ["Notes"] = Val("Senior Manager")
        },
        new()
        {
            ["UserId"] = Val(15),
            ["FirstName"] = Val("Bob"),
            ["LastName"] = Val("Brown"),
            ["Age"] = Val(35),
            ["Salary"] = Val(47000.00),
            ["DateOfBirth"] = Val(DateTime.Parse("1985-05-10")),
            ["JoinDate"] = Val(DateTime.Parse("2020-06-01")),
            ["LastLoginDate"] = Val(DateTime.Parse("2023-10-10")),
            ["Notes"] = Val("New user")
        }
    ];

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}

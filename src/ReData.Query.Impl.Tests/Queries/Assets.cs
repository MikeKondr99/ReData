using ReData.Query.Impl.Runners;
using ReData.Query.Impl.Tests.Fixtures;

namespace ReData.Query.Impl.Tests.QueryTests;


public static class Assets
{
    private static DatabaseValuesMapper _mapper = new ();
    public readonly record struct User(
        int UserId,
        string FirstName,
        string LastName,
        int Age,
        double Salary,
        DateTime DateOfBirth,
        DateTime JoinDate,
        DateTime LastLoginDate,
        string Notes
    );
    
    public static IReadOnlyList<object?[]> RawUsers =
    [
        // UserId, FirstName, LastName, Age, Salary, DateOfBirth, JoinDate, LastLoginDate, Notes
        [1, "John", "Doe", 30, 50000.50, new DateTime(1990, 1, 15), new DateTime(2020, 5, 10), new DateTime(2023, 10, 1), "Regular user"],
        [2, "Jane", "Smith", 25, 60000.00, new DateTime(1995, 7, 22), new DateTime(2021, 3, 15), new DateTime(2023, 9, 28), "Active user"],
        [3, "John", "Doe", 30, 55000.75, new DateTime(1990, 1, 15), new DateTime(2022, 1, 20), new DateTime(2023, 10, 2), "Promoted user"],
        [4, "Alice", "Johnson", 40, 75000.00, new DateTime(1980, 11, 30), new DateTime(2019, 11, 1), new DateTime(2023, 9, 30), "Manager"],
        [5, "Jane", "Smith", 25, 62000.50, new DateTime(1995, 7, 22), new DateTime(2021, 3, 15), new DateTime(2023, 10, 3), "Active user"],
        [6, "Bob", "Brown", 35, 45000.00, new DateTime(1985, 5, 10), new DateTime(2020, 6, 1), new DateTime(2023, 9, 25), "New user"],
        [7, "Alice", "Johnson", 40, 80000.00, new DateTime(1980, 11, 30), new DateTime(2019, 11, 1), new DateTime(2023, 10, 4), "Senior Manager"],
        [8, "Mike", "Davis", 28, 48000.00, new DateTime(1993, 2, 14), new DateTime(2021, 7, 15), new DateTime(2023, 9, 29), "Junior Developer"],
        [9, "Sarah", "Wilson", 32, 70000.00, new DateTime(1989, 8, 20), new DateTime(2018, 12, 1), new DateTime(2023, 10, 5), "Team Lead"],
        [10, "John", "Doe", 30, 60000.00, new DateTime(1990, 1, 15), new DateTime(2020, 5, 10), new DateTime(2023, 10, 6), "Regular user"],
        [11, "Emily", "Clark", 27, 52000.00, new DateTime(1994, 3, 25), new DateTime(2022, 2, 10), new DateTime(2023, 9, 27), "Intern"],
        [12, "Jane", "Smith", 25, 65000.00, new DateTime(1995, 7, 22), new DateTime(2021, 3, 15), new DateTime(2023, 10, 7), "Active user"],
        [13, "Chris", "Evans", 38, 90000.00, new DateTime(1983, 9, 12), new DateTime(2017, 10, 1), new DateTime(2023, 10, 8), "Director"],
        [14, "Alice", "Johnson", 40, 85000.00, new DateTime(1980, 11, 30), new DateTime(2019, 11, 1), new DateTime(2023, 10, 9), "Senior Manager"],
        [15, "Bob", "Brown", 35, 47000.00, new DateTime(1985, 5, 10), new DateTime(2020, 6, 1), new DateTime(2023, 10, 10), "New user"]
    ];
    

    public static IReadOnlyList<IValue[]> ToData(IReadOnlyList<object?[]> input)
    {
        return input.Select(u => u 
                .Select(v => _mapper.MapField(v))
                .ToArray())
        .ToArray();
    }

    
    public static Query UsersQuery = new()
    {
        From = new Table("User", [
            new Query.Field("UserId", ExprType.Integer()),
            new Query.Field("FirstName", ExprType.Text()),
            new Query.Field("LastName", ExprType.Text()),
            new Query.Field("Age", ExprType.Integer()),
            new Query.Field("Salary", ExprType.Number()),
            new Query.Field("DateOfBirth", ExprType.DateTime()),
            new Query.Field("JoinDate", ExprType.DateTime()),
            new Query.Field("LastLoginDate", ExprType.DateTime()),
            new Query.Field("Notes", ExprType.Text()),
        ])
    };
}
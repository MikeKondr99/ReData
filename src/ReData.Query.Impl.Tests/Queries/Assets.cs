using ReData.Query.Impl.Tests.Fixtures;

namespace ReData.Query.Impl.Tests.QueryTests;


public static class Assets
{
    public static  IReadOnlyList<Player> Players = new List<Player>()
    {
          new() { id=1,Name ="George", MaxScore= 22.0M },
          new() { id=2,Name ="Tom", MaxScore= 17.0M },
          new() { id=3,Name ="Tim", MaxScore= 18.0M },
          new() { id=4,Name ="Harry", MaxScore=21.0M },
          new() { id=5,Name ="Ben", MaxScore=15.0M },
          new() { id=6,Name ="Bob", MaxScore=18.0M },
          new() { id=7,Name ="Phoebe", MaxScore=21.0M },
          new() { id=8,Name ="Max", MaxScore= 14.0M },
          new() { id=9,Name ="Lary", MaxScore= 17.0M },
          new() { id=10,Name ="Zach", MaxScore= 15.0M }
    };

    public static Query PlayersQuery = new()
    {
        From = new Table("TestTable", [
            new Query.Field("id", ExprType.Integer()),
            new Query.Field("Name", ExprType.Text()),
            new Query.Field("MaxScore", ExprType.Number())
        ])
    };
}
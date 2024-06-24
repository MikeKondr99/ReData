using ReData.Infrastructure;
using ReData.Infrastructure.Entities;

var db = new MainDatabaseContext();

db.DataSources.Add(new PostgresDataSource
{
    Id = Guid.NewGuid(),
    Name = "P1",
    Options = new PostgresDataSourceOptions()
    {
        Host = "local"
    }
});
db.SaveChanges();

db.DataSources.Add(new CsvDataSource
{
    Id = Guid.NewGuid(),
    Name = "C1",
    Options = new CsvDataSourceOptions()
    {
        Path = "./data.csv"
    }
});

db.SaveChanges();


var csv = db.Set<CsvDataSource>().ToArray();
var pg = db.Set<PostgresDataSource>().ToArray();
var all = db.DataSources.ToArray();

int a = 5;

namespace ReData.Query;

public interface IQueryBuilder
{
    string Build(Query query);
}
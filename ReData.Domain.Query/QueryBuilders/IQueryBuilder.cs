namespace ReData.Domain.Query;

public interface IQueryBuilder
{
    string Build(Query query);

}
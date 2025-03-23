namespace ReData.Query;

public interface IQueryCompiler
{
    string Compile(Query query);
}
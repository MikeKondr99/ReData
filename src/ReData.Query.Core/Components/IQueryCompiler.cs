namespace ReData.Query.Core.Components;

public interface IQueryCompiler
{
    string Compile(Query query);
}
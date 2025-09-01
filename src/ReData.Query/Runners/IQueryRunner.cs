using ReData.Query.Runners.Value;

namespace ReData.Query.Runners;

public interface IQueryRunner : IAsyncDisposable
{
    Task<IReadOnlyList<Record>> RunQueryAsync(Core.Query query);

    
    async Task<IReadOnlyList<Dictionary<string, IValue>>> RunQueryAsObjectAsync(Core.Query query)
    {
        var data = await RunQueryAsync(query);
        var fields = query.Fields().Select(f => f.Alias).ToList();
    
        List<Dictionary<string, IValue>> result = new List<Dictionary<string, IValue>>();
    
        foreach (var record in data)
        {
            var recordDict = new Dictionary<string, IValue>();
        
            for (int i = 0; i < fields.Count; i++)
            {
                // Assuming record.values is an IList<IValue> or similar
                if (i < record.values.Length)
                {
                    recordDict[fields[i]] = record.values[i];
                }
            }
        
            result.Add(recordDict);
        }
    
        return result;
    }
    
    public async Task<IValue> RunQueryAsScalar(Core.Query query)
    {
        var data = await RunQueryAsync(query);
        return data.Single()[0];
    }
    
}
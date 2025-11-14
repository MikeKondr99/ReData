namespace ReData.Common;

public static class EnumerableExtensions
{
    public static string JoinBy<T>(this IEnumerable<T> list, string join)
    {
        return String.Join(join, list.Select((item) => item?.ToString()));
    }

    public static async Task<T?> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> list)
    {
        var enm = list.GetAsyncEnumerator();
        if (await enm.MoveNextAsync())
        {
            return enm.Current;
        }
        await enm.DisposeAsync();
        return default;
    }
    
}
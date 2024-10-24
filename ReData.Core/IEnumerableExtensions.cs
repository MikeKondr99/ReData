namespace ReData.Core;

public static class EnumerableExtensions
{
    public static string JoinBy<T>(this IEnumerable<T> items, string? separator = null)
    {
        if (separator is not null)
        {
            return String.Join(separator, items.Select(x => x.ToString()));
        }
        else
        {
            return String.Concat(items.Select(x => x.ToString()));
        }
        
    }
    


}
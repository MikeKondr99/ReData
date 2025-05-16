namespace ReData.Common;

public static class EnumerableExtensions
{
    public static string JoinBy<T>(this IEnumerable<T> list, string join)
    {
        return String.Join(join, list.Select((item) => item?.ToString()));
    }
    
}
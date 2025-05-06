using System.Text;

namespace ReData.Common;

public static class StringBuilderExtension
{
    public static void TrimEnd(this StringBuilder sb)
    {
        int length = sb.Length;
        while (length > 0 && char.IsWhiteSpace(sb[length - 1]))
        {
            length--;
        }
        sb.Length = length;
    }
}

public static class EnumerableExtensions
{
    public static string JoinBy<T>(this IEnumerable<T> list, string join)
    {
        return String.Join(join, list.Select((item) => item?.ToString()));
    }
    
}
using System.Runtime.CompilerServices;
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

    [OverloadResolutionPriority(1)]
    public static void AppendStringJoin(this StringBuilder sb, string? separator, ReadOnlySpan<string?> values)
    {
        if (values.Length == 0)
        {
            return;
        }

        separator ??= string.Empty;
        sb.Append(values[0]);
        for (int i = 1; i < values.Length; i++)
        {
            sb.Append(separator);
            sb.Append(values[i]);
        }
    }

    [OverloadResolutionPriority(0)]
    public static void AppendStringJoin(this StringBuilder sb, string? separator, IEnumerable<string?> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        using var enumerator = values.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            return;
        }

        separator ??= string.Empty;
        sb.Append(enumerator.Current);
        while (enumerator.MoveNext())
        {
            sb.Append(separator);
            sb.Append(enumerator.Current);
        }
    }
}

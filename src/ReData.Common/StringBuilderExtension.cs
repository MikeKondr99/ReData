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
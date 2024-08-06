using System.Text;
using Microsoft.Extensions.Primitives;
using ReData.Core;

namespace ReData.Domain.Extensions;

public static class DictionaryExtensions
{
    public static string ConnectionString(this Dictionary<StringKey, string> dictionary)
    {
        StringBuilder res = new StringBuilder();
        foreach (var p in dictionary)
        {
            res.Append(p.Key);
            res.Append('=');
            res.Append(p.Value);
            res.Append(';');
        }
        return res.ToString();
    }
    
}
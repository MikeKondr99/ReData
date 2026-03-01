using System.Security.Cryptography;
using System.Text;

namespace ReData.DemoApp.Repositories.Datasets;

public static class DatasetCacheTags
{
    public const string List = "dataset:list";

    public static string ById(Guid id) => $"dataset:{id}";

    public static string ByName(string name) => $"dataset:name:{Hash(name)}";

    private static string Hash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}


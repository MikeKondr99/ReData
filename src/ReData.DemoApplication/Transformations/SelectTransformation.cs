using System.ComponentModel;
using System.Text.Json.Serialization;
using FastEndpoints;
using Pattern.Unions;
using ReData.Query.Common;
using ReData.Query.Core;
using ReData.Query.Core.Types;

namespace ReData.DemoApplication.Transformations;

public class SelectTransformation : ITransformation
{
    public required SelectItem[] Items { get; init; }

    public SelectRestOptions RestOptions { get; init; } = SelectRestOptions.Delete;

    public Result<QueryBuilder, IEnumerable<ExprError?>> Apply(QueryBuilder builder)
    {
        var dict = new Dictionary<string, string>();

        Field[] oldFields = [];
        if (RestOptions == SelectRestOptions.NoAction)
        {
            var newFields = Items.Select(i => i.Field).ToHashSet();

            oldFields = builder.Build().Fields().Where(f => !newFields.Contains(f.Alias)).ToArray();
            foreach (var field in oldFields)
            {
                // TODO: нехватает escape '\]'
                var alias = $"[{field.Alias}]";
                if (field.Type.Type == DataType.Bool)
                {
                    dict.Add(field.Alias, $"Int({alias})");
                }
                else
                {
                    dict.Add(field.Alias, alias);
                }
            }
        }

        foreach (var newField in Items)
        {
            dict.Add(newField.Field, newField.Expression);
        }

        return builder.Select(dict).MapError(err => err.Skip(oldFields.Length));
    }
}

/// <summary>
/// Опция для трансформации, что делать с не указанными полями
/// </summary>
public enum SelectRestOptions
{
    /// <summary>
    /// Оставить остальные поля как есть
    /// </summary>
    NoAction = 1,

    /// <summary>
    /// Удалить не указанные поля (раньше было вариантом по умолчанию)
    /// </summary>
    Delete = 2,
}

public sealed record SelectItem
{
    public required string Field { get; init; }
    public required string Expression { get; init; }
}
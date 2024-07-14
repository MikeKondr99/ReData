namespace ReData.Database.Entities;

public record FieldsTransformation : ITransformation
{
    public required Dictionary<string,string> Mapping { get; init; }
}
/*
 * {
 *   fields: {
 *     "name": { rename: "Имя" },
 *     "id": { delete: false },
 *     "age": { func: "Date(Age(@birthDate))" синтаксис еще не придуман
 *   }
 *   other: STAY || DELETE * Удалять ли не указанные здесь поля
 * }
 */

public interface IFieldAction { }

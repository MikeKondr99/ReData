using ReData.Query.Core.Types;

namespace ReData.Query.Core.Components.Implementation;

public sealed record FieldStorage(IReadOnlyList<Field> Fields) : IFieldStorage;
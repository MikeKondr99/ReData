using ReData.Query.Core.Types;

namespace ReData.Query.Core.Types;

public record struct FieldType(DataType Type, bool CanBeNull);
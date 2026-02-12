using System.Collections;
using System.Data;
using System.Data.Common;

namespace ReData.Query.Runners;

#pragma warning disable CA1010

public abstract class DbDataReaderDecorator : DbDataReader
{
    protected DbDataReaderDecorator(DbDataReader inner)
    {
        Inner = inner;
    }

    protected DbDataReader Inner { get; }

    public override bool GetBoolean(int ordinal) => Inner.GetBoolean(ordinal);

    public override byte GetByte(int ordinal) => Inner.GetByte(ordinal);

    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length) =>
        Inner.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);

    public override char GetChar(int ordinal) => Inner.GetChar(ordinal);

    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length) =>
        Inner.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);

    public override string GetDataTypeName(int ordinal) => Inner.GetDataTypeName(ordinal);

    public override DateTime GetDateTime(int ordinal) => Inner.GetDateTime(ordinal);

    public override decimal GetDecimal(int ordinal) => Inner.GetDecimal(ordinal);

    public override double GetDouble(int ordinal) => Inner.GetDouble(ordinal);

    public override Type GetFieldType(int ordinal) => Inner.GetFieldType(ordinal);

    public override float GetFloat(int ordinal) => Inner.GetFloat(ordinal);

    public override Guid GetGuid(int ordinal) => Inner.GetGuid(ordinal);

    public override short GetInt16(int ordinal) => Inner.GetInt16(ordinal);

    public override int GetInt32(int ordinal) => Inner.GetInt32(ordinal);

    public override long GetInt64(int ordinal) => Inner.GetInt64(ordinal);

    public override string GetName(int ordinal) => Inner.GetName(ordinal);

    public override int GetOrdinal(string name) => Inner.GetOrdinal(name);

    public override string GetString(int ordinal) => Inner.GetString(ordinal);

    public override object GetValue(int ordinal) => Inner.GetValue(ordinal);

    public override int GetValues(object[] values) => Inner.GetValues(values);

    public override bool IsDBNull(int ordinal) => Inner.IsDBNull(ordinal);

    public override int FieldCount => Inner.FieldCount;

    public override object this[int ordinal] => GetValue(ordinal);

    public override object this[string name] => GetValue(GetOrdinal(name));

    public override int RecordsAffected => Inner.RecordsAffected;

    public override bool HasRows => Inner.HasRows;

    public override bool IsClosed => Inner.IsClosed;

    public override bool NextResult() => Inner.NextResult();

    public override Task<bool> NextResultAsync(CancellationToken cancellationToken) =>
        Inner.NextResultAsync(cancellationToken);

    public override bool Read() => Inner.Read();

    public override Task<bool> ReadAsync(CancellationToken cancellationToken) =>
        Inner.ReadAsync(cancellationToken);

    public override int Depth => Inner.Depth;

    public override IEnumerator GetEnumerator() => Inner.GetEnumerator();

    public override DataTable? GetSchemaTable() => Inner.GetSchemaTable();

    public override async ValueTask DisposeAsync()
    {
        await Inner.DisposeAsync();
        await base.DisposeAsync();
    }
}

#pragma warning restore CA1010

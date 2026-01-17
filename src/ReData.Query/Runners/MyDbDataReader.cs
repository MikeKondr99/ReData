using System.Collections;
using System.Data;
using System.Data.Common;
using ReData.Query.Core.Types;

namespace ReData.Query.Runners;

#pragma warning disable CA1010


public class MyDbDataReader : DbDataReader
{
    public MyDbDataReader(DbDataReader inner, IEnumerable<Field> fields)
    {
        dbDataReaderImplementation = inner;
        int i = 0;
        getOrdinal = fields.ToDictionary(f => f.Alias, _ => i++);
        getName = fields.Select(f => f.Alias).ToArray();
    }
    
    private DbDataReader dbDataReaderImplementation;

    private Dictionary<string, int> getOrdinal;
    private string[] getName;

    public override string GetName(int ordinal)
    {
        return getName[ordinal];
    }

    public override int GetOrdinal(string name)
    {
        return getOrdinal[name];
    }

    #region delegate

    public override bool GetBoolean(int ordinal) => dbDataReaderImplementation.GetBoolean(ordinal);

    public override byte GetByte(int ordinal) => dbDataReaderImplementation.GetByte(ordinal);

    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length) =>
        dbDataReaderImplementation.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);

    public override char GetChar(int ordinal) => dbDataReaderImplementation.GetChar(ordinal);

    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length) =>
        dbDataReaderImplementation.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);

    public override string GetDataTypeName(int ordinal) => dbDataReaderImplementation.GetDataTypeName(ordinal);

    public override DateTime GetDateTime(int ordinal) => dbDataReaderImplementation.GetDateTime(ordinal);

    public override decimal GetDecimal(int ordinal) => dbDataReaderImplementation.GetDecimal(ordinal);

    public override double GetDouble(int ordinal) => dbDataReaderImplementation.GetDouble(ordinal);

    public override Type GetFieldType(int ordinal) => dbDataReaderImplementation.GetFieldType(ordinal);

    public override float GetFloat(int ordinal) => dbDataReaderImplementation.GetFloat(ordinal);

    public override Guid GetGuid(int ordinal) => dbDataReaderImplementation.GetGuid(ordinal);

    public override short GetInt16(int ordinal) => dbDataReaderImplementation.GetInt16(ordinal);

    public override int GetInt32(int ordinal) => dbDataReaderImplementation.GetInt32(ordinal);

    public override long GetInt64(int ordinal) => dbDataReaderImplementation.GetInt64(ordinal);

    public override string GetString(int ordinal) => dbDataReaderImplementation.GetString(ordinal);

    public override object GetValue(int ordinal) => dbDataReaderImplementation.GetValue(ordinal);

    public override int GetValues(object[] values) => dbDataReaderImplementation.GetValues(values);

    public override bool IsDBNull(int ordinal) => dbDataReaderImplementation.IsDBNull(ordinal);

    public override int FieldCount => dbDataReaderImplementation.FieldCount;

    public override object this[int ordinal] => dbDataReaderImplementation[ordinal];

    public override object this[string name] => dbDataReaderImplementation[name];

    public override int RecordsAffected => dbDataReaderImplementation.RecordsAffected;

    public override bool HasRows => dbDataReaderImplementation.HasRows;

    public override bool IsClosed => dbDataReaderImplementation.IsClosed;

    public override bool NextResult() => dbDataReaderImplementation.NextResult();

    public override bool Read() => dbDataReaderImplementation.Read();

    public override int Depth => dbDataReaderImplementation.Depth;

    public override IEnumerator GetEnumerator() => dbDataReaderImplementation.GetEnumerator();

    public override DataTable? GetSchemaTable()
    {
        return dbDataReaderImplementation.GetSchemaTable();
    }

    #endregion
}



#pragma warning restore CA1010


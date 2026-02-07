using System.Collections;
using System.Data;
using System.Data.Common;
using System.Globalization;
using ReData.DataIO.ValueFormats;

namespace ReData.DataIO.DataImporters;

#pragma warning disable CA1010

public sealed class TypedDbDataReader : DbDataReader
{
    private readonly IReadOnlyList<string[]> _buffer;
    private readonly IValueFormat?[] _formats;
    private readonly string[] _columnNames;
    private readonly Type?[] _columnTypes;
    private readonly string[] _columnTypeNames;
    private readonly object?[] _currentRowValues;
    private int _currentIndex = -1;

    public TypedDbDataReader(
        IReadOnlyList<string[]> buffer,
        IValueFormat?[] formats,
        string[] columnNames)
    {
        _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        _formats = formats ?? throw new ArgumentNullException(nameof(formats));
        _columnNames = columnNames ?? throw new ArgumentNullException(nameof(columnNames));
        
        DeDupColumnNames(columnNames);
        
        if (_buffer.Count > 0 && _buffer[0].Length != formats.Length)
            throw new ArgumentException("Buffer column count doesn't match formats count");
        
        _currentRowValues = new object?[formats.Length];
        _columnTypes = formats.Select(f => f?.GetValueType() ?? typeof(string)).ToArray();
        _columnTypeNames = _columnTypes.Select(t => t.Name).ToArray();
    }

    private static void DeDupColumnNames(string[] columnNames)
    {
        for (int i = 0; i < columnNames.Length; i++)
        {
            columnNames[i] = columnNames[i].Trim([' ', '\t']);
            if (string.IsNullOrEmpty(columnNames[i]))
            {
                columnNames[i] = GetExcelColumnNameRecursive(i);
            }
            var alias = columnNames[i];
            var counter = 1;
            while (columnNames.AsSpan(..i).Contains(columnNames[i]))
            {
                columnNames[i] = $"{alias}_{++counter}";
            }
        }
    }
    
    private static string GetExcelColumnNameRecursive(int index)
    {
        if (index < 0)
            throw new ArgumentException("Index must be non-negative", nameof(index));

        if (index < 26)
            return ((char)('A' + index)).ToString();

        return GetExcelColumnNameRecursive(index / 26 - 1) + (char)('A' + (index % 26));
    }

    public override bool NextResult()
    {
        throw new NotImplementedException();
    }

    // The key: lazy conversion on Read()
    public override bool Read()
    {
        _currentIndex++;
        if (_currentIndex >= _buffer.Count)
        {
            return false;
        }
        
        for (int i = 0; i < _currentRowValues.Length; i++)
        {
            var format = _formats[i];
            var inputValue = _buffer[_currentIndex][i];
            if (string.IsNullOrEmpty(inputValue))
            {
                _currentRowValues[i] = null;
            }
            else if (format?.TryConvert(inputValue, out var value) is true)
            {
                _currentRowValues[i] = value;
            }
            else
            {
                throw new Exception($"Значение {inputValue} не подошло к форматеру {format.GetType().Name}");
                _currentRowValues[i] = inputValue;
            }
        }

        return true;
    }

    public override Task<bool> ReadAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(Read());
    }

    public override object GetValue(int ordinal)
    {
        return _currentRowValues[ordinal] ?? DBNull.Value;
    }

    public override string GetString(int ordinal)
    {
        var value = _currentRowValues[ordinal];
        return value?.ToString() ?? string.Empty;
    }

    public override short GetInt16(int ordinal)
    {
        return Convert.ToInt16(_currentRowValues[ordinal], CultureInfo.InvariantCulture);
    }

    public override int GetInt32(int ordinal)
    {
        return Convert.ToInt32(_currentRowValues[ordinal], CultureInfo.InvariantCulture);
    }

    public override decimal GetDecimal(int ordinal)
    {
        return Convert.ToDecimal(_currentRowValues[ordinal], CultureInfo.InvariantCulture);
    }

    public override string GetDataTypeName(int ordinal)
    {
        return _columnTypeNames[ordinal];
    }

    public override DateTime GetDateTime(int ordinal)
    {
        return Convert.ToDateTime(_currentRowValues[ordinal], CultureInfo.InvariantCulture);
    }

    public override bool GetBoolean(int ordinal)
    {
        return Convert.ToBoolean(_currentRowValues[ordinal], CultureInfo.InvariantCulture);
    }

    public override byte GetByte(int ordinal)
    {
        return Convert.ToByte(_currentRowValues[ordinal], CultureInfo.InvariantCulture);
    }

    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length)
    {
        throw new NotSupportedException();
    }

    public override char GetChar(int ordinal)
    {
        return Convert.ToChar(_currentRowValues[ordinal], CultureInfo.InvariantCulture);
    }

    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length)
    {
        throw new NotSupportedException();
    }

    public override long GetInt64(int ordinal)
    {
        return Convert.ToInt64(_currentRowValues[ordinal], CultureInfo.InvariantCulture);
    }

    public override double GetDouble(int ordinal)
    {
        return Convert.ToDouble(_currentRowValues[ordinal], CultureInfo.InvariantCulture);
    }

    public override float GetFloat(int ordinal)
    {
        return (float)Convert.ToDouble(_currentRowValues[ordinal], CultureInfo.InvariantCulture);
    }

    public override Guid GetGuid(int ordinal)
    {
        var text = Convert.ToString(_currentRowValues[ordinal], CultureInfo.InvariantCulture);
        return Guid.Parse(text, CultureInfo.InvariantCulture);
    }

    public override bool IsDBNull(int ordinal)
    {
        return _currentRowValues[ordinal] == null || _currentRowValues[ordinal] == DBNull.Value;
    }

    // Schema information
    public override Type GetFieldType(int ordinal)
    {
        return _columnTypes[ordinal] ?? typeof(string);
    }

    public override string GetName(int ordinal)
    {
        return _columnNames[ordinal];
    }

    public override int FieldCount => _columnNames.Length;

    // Required DbDataReader abstract members
    public override int Depth => 0;
    public override bool HasRows => _buffer.Count > 0;
    public override bool IsClosed => false;
    public override int RecordsAffected => 0;

    public override object this[int ordinal] => GetValue(ordinal);
    public override object this[string name] => GetValue(GetOrdinal(name));

    public override int GetOrdinal(string name)
    {
        for (int i = 0; i < _columnNames.Length; i++)
        {
            if (_columnNames[i].Equals(name, StringComparison.Ordinal))
                return i;
        }
        throw new IndexOutOfRangeException($"Column '{name}' not found.");
    }

    // Optional: bulk value access
    public override int GetValues(object[] values)
    {
        var count = Math.Min(values.Length, _currentRowValues.Length);
        Array.Copy(_currentRowValues, values, count);
        return count;
    }

    // Schema table for compatibility
    public override DataTable GetSchemaTable()
    {
        var table = new DataTable();
        table.Columns.Add("ColumnName", typeof(string));
        table.Columns.Add("DataType", typeof(Type));
        table.Columns.Add("ColumnSize", typeof(int));
        table.Columns.Add("AllowDBNull", typeof(bool));

        for (int i = 0; i < _columnNames.Length; i++)
        {
            table.Rows.Add(
                _columnNames[i],
                _columnTypes[i] ?? typeof(string),
                -1, // Unknown size
                true // Always allow nulls since we have string fallback
            );
        }

        return table;
    }

    // Enumerator support
    public override IEnumerator GetEnumerator()
    {
        return new DbDataReaderEnumerator(this);
    }

    private sealed class DbDataReaderEnumerator : IEnumerator
    {
        private readonly TypedDbDataReader _reader;
        
        public DbDataReaderEnumerator(TypedDbDataReader reader)
        {
            _reader = reader;
        }
        
        public object Current => _reader;
        
        public bool MoveNext()
        {
            return _reader.Read();
        }
        
        public void Reset()
        {
            // DbDataReader doesn't support reset
            throw new NotSupportedException();
        }
    }

    public override void Close()
    {
        // Nothing to close - buffer is already in memory
    }

    // Optional: Dispose pattern
    protected override void Dispose(bool disposing)
    {
        // Nothing to dispose
        base.Dispose(disposing);
    }
}

#pragma warning restore CA1010

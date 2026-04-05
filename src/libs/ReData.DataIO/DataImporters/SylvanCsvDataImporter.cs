using System.Data.Common;
using Sylvan;
using Sylvan.Data.Csv;

namespace ReData.DataIO.DataImporters;

public sealed class SylvanCsvDataImporter(char separator, bool hasHeaders) : IDataImporter
{
    public async Task<DbDataReader> ImportAsync(Stream inputStream, CancellationToken ct)
    {
        var pool = new StringPool();
        var sr = new StreamReader(inputStream);
        // var text = await sr.ReadToEndAsync(ct);
        return await CsvDataReader.CreateAsync(
            reader: sr,
            options: new CsvDataReaderOptions()
            {
                HasHeaders = hasHeaders,
                Delimiter = separator,
                StringFactory = pool.GetString,
                
            },
            cancel: ct
        );
    }
}
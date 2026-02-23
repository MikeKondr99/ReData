using System.Data.Common;
using BenchmarkDotNet.Attributes;
using Npgsql;
using ReData.DataIO.DataExporters;
using ReData.Query.Core.Types;
using ReData.Query.Executors;

namespace FileExporterBenchmarks;

#pragma warning disable CA1822
[MemoryDiagnoser(false)]
[RankColumn]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
public class CsvExportBenchmarks
{
    [GlobalSetup]
    public async Task GlobalSetup()
    {
        Console.WriteLine("🔄 Global setup starting...");
        await DatabaseInitializer.InitializeDatabaseAsync();

        Console.WriteLine("✅ Global setup complete");
    }
    
    [Benchmark(Baseline = true)]
    [Arguments(10_000)]
    // [Arguments(50_000)]
    public async Task MiniExcel(int rowsToExport)
    {
        await using var connection = DatabaseInitializer.GetConnection();
        await connection.OpenAsync();
        var reader = await GetDataReaderAsync(connection, rowsToExport);
        await new MiniExcelExporter().ExportAsync(reader, Stream.Null, CancellationToken.None);
    }
    
    [Benchmark]
    [Arguments(10_000)]
    // [Arguments(50_000)]
    public async Task SylvanExcel(int rowsToExport)
    {
        await using var connection = DatabaseInitializer.GetConnection();
        await connection.OpenAsync();
        var reader = await GetDataReaderAsync(connection, rowsToExport);
        await new SylvanExcelExporter().ExportAsync(reader, Stream.Null, CancellationToken.None);
    }
    
    [Benchmark]
    [Arguments(10_000)]
    // [Arguments(50_000)]
    public async Task SylvanExcelWrapped(int rowsToExport)
    {
        await using var connection = DatabaseInitializer.GetConnection();
        await connection.OpenAsync();
        var reader = await GetDataReaderAsync(connection, rowsToExport);
        reader = reader.ToDomain(Enumerable.Range(1, 10).Select(i => new Field()
        {
            Alias = $"column{i}",
            Template = null,
            Type = new FieldType(DataType.Text, true)
        }));
        await new SylvanExcelExporter().ExportAsync(reader, Stream.Null, CancellationToken.None);
    }
    
    private async Task<DbDataReader> GetDataReaderAsync(NpgsqlConnection connection, int rowsToExport)
    {
        string query = $@"
            SELECT 
                customer_name,
                customer_email,
                product_name,
                category,
                quantity,
                unit_price,
                total_amount,
                transaction_date,
                region,
                status
            FROM benchmark_sales 
            LIMIT {rowsToExport}
        ";

        await using var command = new NpgsqlCommand(query, connection);
        return await command.ExecuteReaderAsync();
    }
}
#pragma warning restore CA1822

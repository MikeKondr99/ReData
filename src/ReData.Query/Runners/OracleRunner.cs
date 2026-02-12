using System.Data;
using System.Data.Common;
using Oracle.ManagedDataAccess.Client;
using ReData.Query.Core.Components;

namespace ReData.Query.Runners;

public class OracleRunner : IQueryRunner
{
    // public required OracleConnection Connection { private get; init; }
    public required IQueryCompiler QueryCompiler { private get; init; }

    // public required IFunctionStorage FunctionStorage { private get; init; }


    public async Task<DbDataReader> GetDataReaderAsync(Core.Query query, DbConnection connection)
    {
        if (connection is not OracleConnection conn)
        {
            throw new ArgumentException("Требуется OracleConnection");
        }
   
        if (connection.State is not ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var sql = QueryCompiler.Compile(query);
        await using OracleCommand command = new OracleCommand(sql, conn);
        var reader = await command.ExecuteReaderAsync();
        return reader.ClrNormalize(query.Fields());
    }
}

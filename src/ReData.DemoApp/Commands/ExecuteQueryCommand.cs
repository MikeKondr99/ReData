using FastEndpoints;
using Npgsql;
using Pattern.Unions;
using ReData.DemoApp.Endpoints.Transform;
using ReData.DemoApp.Services;
using ReData.Query;
using ReData.Query.Executors;
using Factory = ReData.Query.Factory;

namespace ReData.DemoApp.Commands;

public record ExecuteQueryCommand : ICommand<Result<TransformResponse, string>>
{
    public required Query.Core.Query Query { get; init; }
}


public class ExecuteQueryCommandHandler(DwhService dwhService) : ICommandHandler<ExecuteQueryCommand, Result<TransformResponse, string>>
{
    /// <inheritdoc />
    public async Task<Result<TransformResponse, string>> ExecuteAsync(ExecuteQueryCommand command, CancellationToken ct)
    {
        try
        {
            var query = command.Query;
            var runner = Factory.CreateQueryExecuter(DatabaseType.PostgreSql);
            await using var connection = new NpgsqlConnection(dwhService.ReadConnection);
            
            var data = await runner
                .GetDataReaderAsync(query, connection)
                .CollectToObjects();

            var response = new TransformResponse
            {
                Data = data,
                Fields = query.Fields().Select(f => new TransformFieldViewModel
                {
                    Alias = f.Alias,
                    Type = f.Type.Type,
                    CanBeNull = f.Type.CanBeNull
                }).ToList(),
                Total = data.Count
            };

            return response;
        }
        catch (Exception ex)
        {
            return $"Непредвиденная ошибка при запуске запроса:\r\n{ex.Message}";
        }
    }
}

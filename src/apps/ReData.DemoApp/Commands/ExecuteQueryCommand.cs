using System.Data.Common;
using FastEndpoints;
using Pattern.Unions;
using ReData.DemoApp.Endpoints.Transform;
using ReData.DemoApp.Services;
using ReData.Query;
using ReData.Query.Executors;
using Factory = ReData.Query.Factory;

namespace ReData.DemoApp.Commands;

public record ExecuteQueryCommand : ICommand<Result<ExecuteQueryCommandResult, string>>
{
    public required Query.Core.Query Query { get; init; }
}

public sealed record ExecuteQueryCommandResult
{
    public required IReadOnlyList<TransformFieldViewModel> Fields { get; init; }

    public required DomainDbDataReader DataReader { get; init; }

    public required DbConnection Connection { get; init; }
}

public class ExecuteQueryCommandHandler(IConnectionService connectionService)
    : ICommandHandler<ExecuteQueryCommand, Result<ExecuteQueryCommandResult, string>>
{
    /// <inheritdoc />
    public async Task<Result<ExecuteQueryCommandResult, string>> ExecuteAsync(ExecuteQueryCommand command, CancellationToken ct)
    {
        DbConnection? connection = null;
        try
        {
            var query = command.Query;
            var runner = Factory.CreateQueryExecuter(DatabaseType.PostgreSql);
            connection = await connectionService.GetConnectionAsync(ConnectionSource.DwhRead, ct);

            var reader = await runner.GetDataReaderAsync(query, connection);

            var response = new ExecuteQueryCommandResult
            {
                Fields = query.Fields().Select(f => new TransformFieldViewModel
                {
                    Alias = f.Alias,
                    Type = f.Type.Type,
                    CanBeNull = f.Type.CanBeNull
                }).ToList(),
                DataReader = reader,
                Connection = connection
            };

            return response;
        }
        catch (Exception ex)
        {
            if (connection is not null)
            {
                await connection.DisposeAsync();
            }

            return $"Непредвиденная ошибка при запуске запроса:{Environment.NewLine}{ex.Message}";
        }
    }
}

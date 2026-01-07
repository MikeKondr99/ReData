using System.Diagnostics;
using FastEndpoints;
using Pattern.Unions;
using ReData.DemoApp.Endpoints.Transform;
using ReData.DemoApp.Extensions;
using ReData.DemoApp.Services;
using ReData.Query;
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
            await using var runner = Factory.CreateQueryRunner(DatabaseType.PostgreSql, dwhService.ReadConnection);
            var data = await runner.RunQueryAsObjectAsync(query);

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
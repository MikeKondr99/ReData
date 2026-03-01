using Microsoft.Extensions.Logging;
using ReData.DemoApp.Database.Entities;
using ReData.DemoApp.Repositories.Datasets;
using StrictId;

namespace ReData.DemoApp.Logging;

public static partial class DatasetRepositoryLogger
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Dataset load by id completed: datasetId={DatasetId}, found={Found}")]
    public static partial void DatasetLoadedById(
        this ILogger<DatasetRepository> logger,
        Id<DataSet> datasetId,
        bool found);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Dataset created: datasetId={DatasetId}, name={Name}, transformations={TransformationsCount}")]
    public static partial void DatasetCreated(
        this ILogger<DatasetRepository> logger,
        Id<DataSet> datasetId,
        string name,
        int transformationsCount);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Dataset update started: datasetId={DatasetId}, name={Name}, connectorId={ConnectorId}, transformations={TransformationsCount}")]
    public static partial void DatasetUpdateStarted(
        this ILogger<DatasetRepository> logger,
        Id<DataSet> datasetId,
        string name,
        Guid connectorId,
        int transformationsCount);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Dataset update skipped: dataset not found, datasetId={DatasetId}")]
    public static partial void DatasetUpdateSkippedNotFound(
        this ILogger<DatasetRepository> logger,
        Id<DataSet> datasetId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Dataset updated: datasetId={DatasetId}, oldName={OldName}, newName={NewName}, transformations={TransformationsCount}")]
    public static partial void DatasetUpdated(
        this ILogger<DatasetRepository> logger,
        Id<DataSet> datasetId,
        string oldName,
        string newName,
        int transformationsCount);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Dataset deleted: datasetId={DatasetId}, name={Name}")]
    public static partial void DatasetDeleted(
        this ILogger<DatasetRepository> logger,
        Id<DataSet> datasetId,
        string name);
}

using ReData.Common;
using ReData.DemoApp.Commands;
using ReData.DemoApp.Database.Entities;
using ReData.DemoApp.Endpoints.Datasets.Create;

namespace ReData.DemoApp.Tests.Init;

public class InitData
{
    private static InitData? instance;
    private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

    private InitData()
    {
    }

    public static async ValueTask<InitData> CreateAsync()
    {
        if (instance is not null)
        {
            return instance;
        }

        await semaphore.WaitAsync();

        if (instance is not null)
        {
            return instance;
        }

        instance = new InitData();
        await instance.InitDataOnce();

        semaphore.Release();
        return instance;
    }

    public IReadOnlyDictionary<string, DataConnectorEntity> DataConnectors { get; private set; } = new Dictionary<string, DataConnectorEntity>();

    public DataConnectorEntity ExistingDataConnector => DataConnectors.First().Value;


    private async Task InitDataOnce()
    {
        var dataConnectors = new Dictionary<string, DataConnectorEntity>();
        foreach (var filePath in Directory.EnumerateFiles("./Init/Data"))
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            StreamReader stream = new StreamReader(filePath);
            dataConnectors[fileName] =
                await new CreateDataConnectorCommand
                {
                    Name = fileName,
                    Separator = ',',
                    WithHeader = true,
                    FileStream = stream.BaseStream,
                }.ExecuteAsync(CancellationToken.None);
        }

        DataConnectors = dataConnectors;
    }
}
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
            Console.WriteLine("Getting init data");
            return instance;
        }

        {
            Console.WriteLine("waiting init data");
            await semaphore.WaitAsync();

            try
            {
                if (instance is not null)
                {
                    Console.WriteLine("Getting init data");
                    return instance;
                }

                instance = new InitData();
                await instance.InitDataOnce();
                Console.WriteLine("Data is initialized");
            }
            finally
            {
                semaphore.Release();
            }
        }

        return instance;
    }

    public IReadOnlyDictionary<string, DataConnectorEntity> DataConnectors { get; private set; } = new Dictionary<string, DataConnectorEntity>();

    public DataConnectorEntity ExistingDataConnector => DataConnectors["test"];


    private async Task InitDataOnce()
    {
        var dataConnectors = new Dictionary<string, DataConnectorEntity>();
        foreach (var filePath in Directory.EnumerateFiles("./Init/Data"))
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            if (fileName == "test")
            {
                int a = 5;
            }
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
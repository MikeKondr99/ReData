using FastEndpoints;

namespace ReData.DemoApp.Endpoints.Groups;

/// <inheritdoc />
public class DataConnectorsGroup : Group
{
    public DataConnectorsGroup()
    {
        Configure(
            "data-connectors",
            ep =>
            {
                ep.Description(d => d.WithTags("Data connectors"));
            });
    }
}
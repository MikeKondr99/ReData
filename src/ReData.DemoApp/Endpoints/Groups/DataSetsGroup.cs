using FastEndpoints;
using FastEndpoints.Swagger;

namespace ReData.DemoApp.Endpoints.Groups;

/// <inheritdoc />
public class DataSetsGroup : Group
{
    public DataSetsGroup()
    {
        Configure(
            "datasets",
            ep =>
            {
                ep.Description(d => d.WithTags("Datasets"));
            });
    }
}
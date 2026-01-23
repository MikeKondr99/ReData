using FastEndpoints;

namespace ReData.DemoApp.Endpoints.Groups;

/// <inheritdoc />
public class FunctionsGroup : Group
{
    public FunctionsGroup()
    {
        Configure(
            "functions",
            ep =>
            {
                ep.Description(d => d.WithTags("Functions"));
            });
    }
}
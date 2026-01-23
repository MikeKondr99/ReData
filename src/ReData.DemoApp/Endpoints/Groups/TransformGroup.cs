using FastEndpoints;

namespace ReData.DemoApp.Endpoints.Groups;

/// <inheritdoc />
public class TransformGroup : Group
{
    public TransformGroup()
    {
        Configure(
            "transform",
            ep =>
            {
                ep.Description(d => d.WithTags("Transform"));
            });
    }
}
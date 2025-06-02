using ReData.DemoApplication;

namespace ReData.DemoApplication;

public class TransformRequest
{
    public List<ITransformation> Transformations { get; set; } = new ();
}
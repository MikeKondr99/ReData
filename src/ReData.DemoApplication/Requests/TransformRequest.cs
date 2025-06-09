using ReData.DemoApplication.Transformations;

namespace ReData.DemoApplication.Requests;

public class TransformRequest
{
    public List<ITransformation> Transformations { get; set; } = new ();
}
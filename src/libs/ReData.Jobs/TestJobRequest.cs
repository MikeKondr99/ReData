namespace ReData.Jobs;

public sealed class TestJobRequest
{
    public int JobCount { get; init; }
    public int MinDelayMs { get; init; }
    public int MaxDelayMs { get; init; }
}

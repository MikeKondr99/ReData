using System.Diagnostics;

namespace ReData.Query.Core.Types;

public static class Tracing
{
    public static ActivitySource Source { get; } = new ActivitySource("ReData.Query");
}
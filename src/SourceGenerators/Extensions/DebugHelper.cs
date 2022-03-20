using System.Diagnostics;

namespace AutoInterface.SourceGenerators.Extensions;

public static class DebugHelper
{
    [Conditional("DEBUG")]
    public static void Launch()
    {
        if (Debugger.IsAttached is false)
        {
            Debugger.Launch();
        }
    }
}

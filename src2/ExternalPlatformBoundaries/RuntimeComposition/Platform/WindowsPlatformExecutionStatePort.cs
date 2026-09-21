using System.Runtime.InteropServices;

namespace Terraria.ExternalPlatformBoundaries.RuntimeComposition.Platform;

public sealed class WindowsPlatformExecutionStatePort : IPlatformExecutionStatePort
{
  public uint SetExecutionState(uint flags)
  {
    if (!OperatingSystem.IsWindows())
    {
      return 0;
    }

    return NativeMethods.SetThreadExecutionState(flags);
  }

  private static class NativeMethods
  {
    [DllImport("kernel32.dll")]
    internal static extern uint SetThreadExecutionState(uint executionState);
  }
}

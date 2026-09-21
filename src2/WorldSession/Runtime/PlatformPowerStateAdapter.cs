namespace Terraria.WorldSession.Runtime;

public sealed class PlatformPowerStateAdapter
{
  public bool ExecutionStateHeld { get; private set; }

  public void Acquire()
  {
    ExecutionStateHeld = true;
  }

  public void Release()
  {
    ExecutionStateHeld = false;
  }
}

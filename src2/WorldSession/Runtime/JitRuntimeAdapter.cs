namespace Terraria.WorldSession.Runtime;

public sealed class JitRuntimeAdapter
{
  public bool IsInitialized { get; private set; }

  public void MarkInitialized()
  {
    IsInitialized = true;
  }

  public void Clear()
  {
    IsInitialized = false;
  }
}

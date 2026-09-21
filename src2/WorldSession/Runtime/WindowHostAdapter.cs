namespace Terraria.WorldSession.Runtime;

public sealed class WindowHostAdapter
{
  public bool IsAvailable { get; private set; }

  public void Attach()
  {
    IsAvailable = true;
  }

  public void Detach()
  {
    IsAvailable = false;
  }
}

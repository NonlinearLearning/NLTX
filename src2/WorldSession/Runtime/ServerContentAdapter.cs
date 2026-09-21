namespace Terraria.WorldSession.Runtime;

public sealed class ServerContentAdapter
{
  public bool IsAvailable { get; private set; }

  public void SetAvailability(bool isAvailable)
  {
    IsAvailable = isAvailable;
  }
}

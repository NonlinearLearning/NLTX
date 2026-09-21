namespace Terraria.Dome.Simulation.WorldObjects;

public sealed class ChestAccessComponent
{
  public PlayerHandle? Opener { get; private set; }
  public bool IsOpen => Opener is not null;

  public bool TryOpen(PlayerHandle player)
  {
    if (Opener is PlayerHandle current && current != player)
    {
      return false;
    }

    Opener = player;
    return true;
  }

  public void Close(PlayerHandle player)
  {
    if (Opener == player)
    {
      Opener = null;
    }
  }

  public void Clear()
  {
    Opener = null;
  }
}

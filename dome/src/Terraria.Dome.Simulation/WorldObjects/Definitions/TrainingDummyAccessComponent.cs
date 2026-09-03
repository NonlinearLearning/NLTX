using Terraria.Dome.Simulation;

namespace Terraria.Dome.Simulation.WorldObjects.Definitions;

public sealed class TrainingDummyAccessComponent
{
  public PlayerHandle? Owner { get; private set; }

  public bool TryAcquire(PlayerHandle player)
  {
    if (Owner is PlayerHandle owner && owner != player)
    {
      return false;
    }

    Owner = player;
    return true;
  }

  public void Release(PlayerHandle player)
  {
    if (Owner == player)
    {
      Owner = null;
    }
  }

  public void Clear()
  {
    Owner = null;
  }
}

using Terraria.Dome.Simulation;

namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct DoorLockPolicy(bool IsLocked, PlayerHandle? KeyHolder = null)
{
  public bool Allows(PlayerHandle? actor)
  {
    return !IsLocked || actor is PlayerHandle player && player == KeyHolder;
  }
}

namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct DoorAuthorityPolicy(bool IsLocked)
{
  public static DoorAuthorityPolicy Unlocked => new(false);
}

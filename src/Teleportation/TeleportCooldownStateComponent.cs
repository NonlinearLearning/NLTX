namespace Terraria.Teleportation;

public struct TeleportCooldownStateComponent
{
  public bool IsOnCooldown => RemainingTicks > 0;
  public int RemainingTicks;
  public TeleportSource Source;
  public long? StartedAtTick;
}

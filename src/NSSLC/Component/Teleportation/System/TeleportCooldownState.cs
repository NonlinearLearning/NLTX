namespace Terraria.Teleportation;

public sealed class TeleportCooldownState
{
  public bool IsOnCooldown => RemainingTicks > 0;
  public int RemainingTicks;
  public TeleportSource Source;
  public long? StartedAtTick;
}

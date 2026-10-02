namespace Terraria.Teleportation;

public readonly record struct TeleportCooldownSnapshot(
  int RemainingTicks,
  TeleportSource Source,
  long? StartedAtTick)
{
  public bool IsOnCooldown => RemainingTicks > 0;
}

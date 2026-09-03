namespace Terraria.Dome.Transport;

public readonly record struct ServerSnapshot(
  long Tick,
  ServerPlayerSnapshot Player,
  int NpcHealth,
  int ProjectileCount);

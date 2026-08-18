namespace Terraria.Dome.Simulation;

public readonly record struct PlayerStateSnapshot(
  PlayerHandle Player,
  bool IsActive,
  int Health,
  int MaximumHealth,
  int RespawnTicks,
  string AccountUuid = "",
  byte AssignedSlot = 0,
  int Mana = 0,
  int MaximumMana = 0);

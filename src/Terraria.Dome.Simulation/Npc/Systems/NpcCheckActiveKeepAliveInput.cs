namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcCheckActiveKeepAliveInput(
  int NpcType,
  bool IsActive,
  bool HasActivePlayer,
  bool IsBoss,
  float Ai0,
  float Ai2,
  bool IsDayTime);

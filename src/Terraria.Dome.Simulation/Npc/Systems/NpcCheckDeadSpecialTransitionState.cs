namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcCheckDeadSpecialTransitionState(
  int Life,
  float Ai0,
  float Ai1,
  float Ai2,
  float Ai3,
  bool DontTakeDamage,
  bool DontTakeDamageFromHostiles,
  bool NetUpdate);

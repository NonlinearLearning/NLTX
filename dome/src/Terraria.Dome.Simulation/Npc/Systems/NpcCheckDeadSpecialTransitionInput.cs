namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcCheckDeadSpecialTransitionInput(
  NpcHandle Npc,
  int NpcType,
  int Life,
  int MaximumHealth,
  float Ai0,
  float Ai1,
  float Ai2,
  float Ai3,
  SimulationVector Center,
  bool DontTakeDamage = false,
  bool DontTakeDamageFromHostiles = false,
  bool NetUpdate = false);

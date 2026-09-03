namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcActivitySlotContribution(
  bool ShouldContribute,
  float SlotWeight);

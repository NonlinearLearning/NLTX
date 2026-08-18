using Terraria.Dome.Simulation.Npc.Components;

namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcDeathInput(
  NpcHandle Npc,
  int Health,
  bool WasActive,
  SimulationVector Position,
  int LootTableId);

public readonly record struct NpcDeathResult(
  NpcHandle Npc,
  SimulationVector Position,
  int LootTableId,
  bool Published);

public sealed class NpcDeathSystem
{
  public NpcDeathResult Evaluate(NpcDeathInput input)
  {
    return new NpcDeathResult(
      input.Npc,
      input.Position,
      input.LootTableId,
      input.WasActive && input.Health <= 0);
  }
}

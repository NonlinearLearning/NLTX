using Terraria.Dome.Simulation.Items.Commands;

namespace Terraria.Dome.Simulation.Npc.Commands;

public readonly record struct NpcLootCommand(
  NpcHandle SourceNpc,
  CreateWorldItemCommand WorldItem)
{
  public bool IsConsistent => SourceNpc.IsValid && WorldItem.SpawnSource == SourceNpc.Value;
}

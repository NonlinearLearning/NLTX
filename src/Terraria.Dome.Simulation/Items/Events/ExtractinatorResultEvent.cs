using Terraria.Dome.Simulation;

namespace Terraria.Dome.Simulation.Items.Events;

public readonly record struct ExtractinatorResultEvent(
  PlayerHandle Player,
  ItemStack Input,
  ItemStack Output,
  ushort TargetTileType,
  long Sequence,
  ExtractinatorSourceKind SourceKind = ExtractinatorSourceKind.Player);

public enum ExtractinatorSourceKind
{
  Player,
  Wiring
}

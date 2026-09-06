namespace Terraria.Content;

public sealed record TileEnvironmentDefinition(
  bool Sand = false,
  bool Flame = false,
  bool ObsidianKill = false,
  TileLiquidInteractionKind LiquidInteraction = TileLiquidInteractionKind.None);

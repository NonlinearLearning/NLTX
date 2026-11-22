namespace Terraria.World;

/// <summary>Identifies the world operation requested by an entity for one tick.</summary>
public enum WorldInteractionOperation : byte
{
  None = 0,
  ReadTile = 1,
  BreakTile = 2,
  PlaceTile = 3
}

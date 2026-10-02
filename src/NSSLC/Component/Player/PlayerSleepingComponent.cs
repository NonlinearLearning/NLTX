using System.Numerics;

namespace Terraria.Player;

// Stores the independently owned portion of a player's sleeping relation.
// Bed eligibility, rotation, stack-manager and network effects remain external.
public sealed class PlayerSleepingComponent
{
  public TileCoordinate? AnchorTile { get; set; }

  public DirectionKind RequiredFacing { get; set; }

  public int StackIndex { get; set; } = -1;

  public int TimeSleeping { get; set; }

  public Vector2 BedVisualOffset { get; set; }
}

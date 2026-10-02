using System.Numerics;

namespace Terraria.Player;

// Stores the independently owned portion of a player's sitting relation.
// Tile/frame eligibility, ExtraSeatInfo and stack-manager effects remain external.
public sealed class PlayerSittingComponent
{
  public TileCoordinate? AnchorTile { get; set; }

  public DirectionKind RequiredFacing { get; set; }

  // RestSeatFeatures is the existing NLTX equivalent for the confirmed toilet flag.
  public RestSeatFeatures SeatFeatures { get; set; }

  public Vector2 SeatOffset { get; set; }

  public int StackIndex { get; set; } = -1;
}

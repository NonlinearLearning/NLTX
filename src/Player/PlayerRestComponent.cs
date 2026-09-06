namespace Terraria.Player;

public sealed class PlayerRestComponent
{
  public PlayerRestMode Mode { get; set; }

  public TileCoordinate? AnchorTile { get; set; }

  // Direction owner remains an integration decision.
  public DirectionKind RequiredFacing { get; set; }

  public int StackIndex { get; set; }

  public int SleepElapsedTicks { get; set; }

  public RestSeatFeatures SeatFeatures { get; set; }
}

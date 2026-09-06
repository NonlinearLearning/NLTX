namespace Terraria.Player;

public sealed class PlayerRestState
{
  public PlayerRestMode Mode { get; set; }

  public TileCoordinate? AnchorTile { get; set; }

  public Direction RequiredFacing { get; set; } = Direction.Right;

  public int StackIndex { get; set; }

  public int SleepElapsedTicks { get; set; }

  public RestSeatFeatures SeatFeatures { get; set; }

  public bool IsFullyAsleep => Mode == PlayerRestMode.Sleeping && SleepElapsedTicks >= 120;

  public bool IsResting => Mode != PlayerRestMode.None;
}

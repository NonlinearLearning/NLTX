namespace Terraria.WorldStorage;

public readonly record struct LiquidWorkEntry(
  TileCoordinate Coordinate,
  byte Delay,
  byte KillState)
{
  public int DelayTicks => Delay;
  public bool IsReady => Delay == 0;
  public int KillCount => KillState;
}

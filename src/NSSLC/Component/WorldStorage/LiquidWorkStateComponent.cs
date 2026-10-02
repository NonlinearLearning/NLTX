namespace Terraria.WorldStorage;

public struct LiquidWorkStateComponent
{
  public TileCoordinate Coordinate;
  public bool IsPending;
  public bool SkipOnce;
  public int KillScore;
  public int Delay;
  public long Sequence;
  public int RetryCount;
  public bool IsBuffered;
}

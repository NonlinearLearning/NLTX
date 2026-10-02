namespace Terraria.WorldGeneration.Support;

public readonly record struct TrackHistoryEntry
{
  public TrackHistoryEntry(
    int x,
    int y,
    TrackSlope slope,
    TrackGenerationMode mode = TrackGenerationMode.Normal)
  {
    X = unchecked((short)x);
    Y = unchecked((short)y);
    Slope = slope;
    Mode = mode;
  }

  public short X { get; }

  public short Y { get; }

  public TrackSlope Slope { get; }

  public TrackGenerationMode Mode { get; }
}

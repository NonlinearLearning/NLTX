namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// A persisted weighted pressure plate coordinate.
/// </summary>
public sealed class WorldFilePressurePlateRecord
{
  public WorldFilePressurePlateRecord(int x, int y)
  {
    X = x;
    Y = y;
  }

  public int X { get; }

  public int Y { get; }
}

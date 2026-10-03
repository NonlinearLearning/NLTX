namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// A persisted team spawn candidate from the WorldFile continuation.
/// </summary>
public readonly struct WorldFileExtraSpawnPoint
{
  public WorldFileExtraSpawnPoint(short x, short y)
  {
    X = x;
    Y = y;
  }

  public short X { get; }

  public short Y { get; }
}

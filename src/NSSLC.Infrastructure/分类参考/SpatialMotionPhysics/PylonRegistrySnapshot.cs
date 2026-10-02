namespace Terraria.SpatialMotionPhysics;

public sealed class PylonRegistrySnapshot
{
  public PylonRegistrySnapshot(
    IEnumerable<TeleportPylonSnapshotEntry> entries,
    uint revision = 0)
  {
    ArgumentNullException.ThrowIfNull(entries);
    Entries = entries
      .Distinct()
      .OrderBy(entry => entry.PositionInTiles.X)
      .ThenBy(entry => entry.PositionInTiles.Y)
      .ThenBy(entry => entry.TypeOfPylon)
      .ToArray();
    Revision = revision;
  }

  public IReadOnlyList<TeleportPylonSnapshotEntry> Entries { get; }

  public uint Revision { get; }
}

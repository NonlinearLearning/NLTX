using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

/// <summary>
/// Immutable world input exposed to a generation pass.
/// </summary>
public sealed class TileReadSnapshot
{
  public TileReadSnapshot(WorldGridSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    Snapshot = snapshot;
  }

  public WorldGridSnapshot Snapshot { get; }

  public WorldMetadata Metadata => Snapshot.Metadata;

  public WorldTile GetTile(int x, int y)
  {
    return Snapshot.GetTile(x, y);
  }

  public long GetSectionVersion(WorldSectionCoordinates coordinates)
  {
    return Snapshot.GetSectionVersion(coordinates);
  }
}

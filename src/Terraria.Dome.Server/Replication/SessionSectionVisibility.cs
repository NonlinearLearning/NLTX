using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Server.Replication;

public sealed class SessionSectionVisibility
{
  private readonly Dictionary<WorldSectionCoordinates, long> _sentVersions = new();

  public void Clear()
  {
    _sentVersions.Clear();
  }

  public void RemoveNotIn(IReadOnlySet<WorldSectionCoordinates> visibleSections)
  {
    ArgumentNullException.ThrowIfNull(visibleSections);

    List<WorldSectionCoordinates> departed = new();
    foreach (WorldSectionCoordinates coordinates in _sentVersions.Keys)
    {
      if (!visibleSections.Contains(coordinates))
      {
        departed.Add(coordinates);
      }
    }

    for (int index = 0; index < departed.Count; index++)
    {
      _sentVersions.Remove(departed[index]);
    }
  }

  public IReadOnlyList<WorldSectionSnapshot> CollectChangedSections(
    IReadOnlyList<WorldSectionSnapshot> snapshots)
  {
    ArgumentNullException.ThrowIfNull(snapshots);

    List<WorldSectionSnapshot> changed = new();
    for (int index = 0; index < snapshots.Count; index++)
    {
      WorldSectionSnapshot snapshot = snapshots[index];
      if (_sentVersions.TryGetValue(snapshot.Coordinates, out long sentVersion) &&
          sentVersion == snapshot.Version)
      {
        continue;
      }

      _sentVersions[snapshot.Coordinates] = snapshot.Version;
      changed.Add(snapshot);
    }

    return changed;
  }

  public void MarkSent(WorldSectionSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    _sentVersions[snapshot.Coordinates] = snapshot.Version;
  }
}

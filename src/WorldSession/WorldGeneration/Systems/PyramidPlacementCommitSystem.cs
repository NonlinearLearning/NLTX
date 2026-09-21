using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Commits one complete pyramid-coordinate snapshot for its generation session.
/// </summary>
public static class PyramidPlacementCommitSystem
{
  public static void Commit(
    PyramidPlacementStateComponent component,
    in PyramidPlacementSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (snapshot.GenerationId != component.GenerationId)
    {
      throw new ArgumentException(
        "Pyramid coordinates cannot be committed to another generation.",
        nameof(snapshot));
    }

    if (snapshot.Capacity != component.Capacity)
    {
      throw new ArgumentException(
        "Pyramid coordinate capacity cannot change during a generation.",
        nameof(snapshot));
    }

    if (snapshot.Count < 0 || snapshot.Count > snapshot.Capacity)
    {
      throw new ArgumentOutOfRangeException(nameof(snapshot));
    }

    ArgumentNullException.ThrowIfNull(snapshot.XPositions);
    ArgumentNullException.ThrowIfNull(snapshot.YPositions);
    if (snapshot.XPositions.Count != snapshot.Count ||
        snapshot.YPositions.Count != snapshot.Count)
    {
      throw new ArgumentException(
        "Pyramid coordinate snapshots must contain exactly the used paired positions.",
        nameof(snapshot));
    }

    component.ReplaceState(
      snapshot.Count,
      snapshot.XPositions,
      snapshot.YPositions);
  }
}

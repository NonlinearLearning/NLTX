using System;
using System.Collections.Generic;
using System.Collections.Frozen;

using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Counts requested active tile types from an explicit observation snapshot.
/// </summary>
public static class WorldGenerationTileScanQuery
{
  public readonly record struct TileObservation(
    TilePosition Position,
    bool Active,
    ushort Type);

  public sealed class Result
  {
    internal Result(
      FrozenDictionary<ushort, int> counts,
      int totalMatches)
    {
      Counts = counts;
      TotalMatches = totalMatches;
    }

    public FrozenDictionary<ushort, int> Counts { get; }

    public int TotalMatches { get; }

    /// <summary>
    /// Preserves the legacy TileScanner.GetCount sentinel for an unknown type.
    /// </summary>
    public int GetCount(ushort tileId)
    {
      return Counts.TryGetValue(tileId, out int count) ? count : -1;
    }
  }

  public static Result Count(
    IReadOnlyList<TileObservation> observations,
    IReadOnlyCollection<ushort> tileIds)
  {
    ArgumentNullException.ThrowIfNull(observations);
    ArgumentNullException.ThrowIfNull(tileIds);

    Dictionary<ushort, int> counts = new();
    foreach (ushort tileId in tileIds)
    {
      counts.TryAdd(tileId, 0);
    }

    int totalMatches = 0;
    for (int index = 0; index < observations.Count; index++)
    {
      TileObservation observation = observations[index];
      if (observation.Active && counts.TryGetValue(observation.Type, out int count))
      {
        counts[observation.Type] = count + 1;
        totalMatches++;
      }
    }

    return new Result(counts.ToFrozenDictionary(), totalMatches);
  }
}

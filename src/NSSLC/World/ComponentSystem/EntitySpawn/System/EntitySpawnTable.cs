using System;
using System.Collections.Generic;
using Terraria.Share.Entities;

namespace Terraria.Unshared.EntitySpawn;

/// <summary>Weighted, immutable spawn table consumed by <see cref="EntitySpawnSystem"/>.</summary>
public sealed class EntitySpawnTable
{
  private readonly EntitySpawnCandidate[] _candidates;
  private readonly int _totalWeight;

  public EntitySpawnTable(IEnumerable<EntitySpawnCandidate> candidates)
  {
    ArgumentNullException.ThrowIfNull(candidates);
    EntitySpawnCandidate[] source = [.. candidates];
    if (source.Length == 0)
    {
      throw new ArgumentException("A spawn table requires at least one candidate.", nameof(candidates));
    }

    HashSet<EntityId> seen = [];
    int totalWeight = 0;
    foreach (EntitySpawnCandidate candidate in source)
    {
      ArgumentNullException.ThrowIfNull(candidate);
      if (!candidate.IsValid)
      {
        throw new ArgumentException(
            $"Spawn candidate {candidate.DefinitionId} is invalid.", nameof(candidates));
      }

      if (!seen.Add(candidate.DefinitionId))
      {
        throw new ArgumentException(
            $"Duplicate spawn candidate {candidate.DefinitionId}.", nameof(candidates));
      }

      totalWeight += candidate.Weight;
    }

    _candidates = source;
    _totalWeight = totalWeight;
  }

  public IReadOnlyList<EntitySpawnCandidate> Candidates => _candidates;

  public int TotalWeight => _totalWeight;

  /// <summary>Maps a roll in <c>[0, TotalWeight)</c> onto one candidate.</summary>
  public EntitySpawnCandidate Select(int roll)
  {
    int remaining = roll % _totalWeight;
    foreach (EntitySpawnCandidate candidate in _candidates)
    {
      if (remaining < candidate.Weight)
      {
        return candidate;
      }

      remaining -= candidate.Weight;
    }

    return _candidates[_candidates.Length - 1];
  }
}

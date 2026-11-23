using System;
using System.Collections.Generic;
using Terraria.Entity;

namespace Terraria.Npc;

/// <summary>Weighted, immutable spawn table consumed by <see cref="NpcSpawnSystem"/>.</summary>
public sealed class NpcSpawnTable
{
  private readonly NpcSpawnCandidate[] _candidates;
  private readonly int _totalWeight;

  public NpcSpawnTable(IEnumerable<NpcSpawnCandidate> candidates)
  {
    ArgumentNullException.ThrowIfNull(candidates);
    NpcSpawnCandidate[] source = [.. candidates];
    if (source.Length == 0)
    {
      throw new ArgumentException("A spawn table requires at least one candidate.", nameof(candidates));
    }

    HashSet<EntityDefinitionId> seen = [];
    int totalWeight = 0;
    foreach (NpcSpawnCandidate candidate in source)
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

  public IReadOnlyList<NpcSpawnCandidate> Candidates => _candidates;

  public int TotalWeight => _totalWeight;

  /// <summary>Maps a roll in <c>[0, TotalWeight)</c> onto one candidate.</summary>
  public NpcSpawnCandidate Select(int roll)
  {
    int remaining = roll % _totalWeight;
    foreach (NpcSpawnCandidate candidate in _candidates)
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

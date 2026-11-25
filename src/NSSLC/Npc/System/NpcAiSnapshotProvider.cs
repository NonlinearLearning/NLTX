using System;
using System.Collections.Generic;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Ai;

namespace Terraria.Unshared.Npc;

/// <summary>
/// Projects authoritative ECS values into the detached facts consumed by NPC behaviors.
/// </summary>
/// <remarks>
/// This type owns no query and no fact type: it validates the contributor set once, then asks only
/// the contributors whose facts the running behavior declared. Each fact's source domain lives in
/// its own contributor, so a domain can change its projection without touching this class.
/// </remarks>
public sealed class NpcAiSnapshotProvider : IAiSnapshotProvider<ArchEntity>
{
  private readonly INpcAiSnapshotContributor[] _contributors;
  private readonly IReadOnlyList<Type> _factTypes;

  public NpcAiSnapshotProvider(
      ArchWorld world,
      Func<NpcAiWorldFact>? worldFacts = null)
      : this(NpcAiSnapshotContributors.CreateDefault(world, worldFacts))
  {
  }

  public NpcAiSnapshotProvider(IReadOnlyList<INpcAiSnapshotContributor> contributors)
  {
    ArgumentNullException.ThrowIfNull(contributors);
    List<Type> factTypes = [];
    HashSet<Type> claimed = [];
    _contributors = new INpcAiSnapshotContributor[contributors.Count];
    for (int i = 0; i < contributors.Count; i++)
    {
      INpcAiSnapshotContributor contributor = contributors[i] ??
          throw new ArgumentException(
              "A snapshot contributor cannot be null.", nameof(contributors));
      _contributors[i] = contributor;
      foreach (Type factType in contributor.FactTypes)
      {
        if (!claimed.Add(factType))
        {
          throw new ArgumentException(
              $"AI fact {factType.FullName} is claimed by more than one snapshot contributor.",
              nameof(contributors));
        }
        factTypes.Add(factType);
      }
    }
    _factTypes = factTypes.AsReadOnly();
  }

  /// <summary>Fact types this provider can project, in contributor order.</summary>
  public IReadOnlyList<Type> FactTypes => _factTypes;

  public AiSnapshot Capture(ArchEntity entity, IReadOnlyList<Type> requiredComponents)
  {
    ArgumentNullException.ThrowIfNull(requiredComponents);
    AiSnapshot snapshot = AiSnapshot.Empty;
    foreach (INpcAiSnapshotContributor contributor in _contributors)
    {
      if (!ProjectsAny(contributor, requiredComponents))
      {
        continue;
      }
      snapshot = contributor.Contribute(entity, snapshot);
    }
    return snapshot;
  }

  private static bool ProjectsAny(
      INpcAiSnapshotContributor contributor,
      IReadOnlyList<Type> requiredComponents)
  {
    foreach (Type factType in contributor.FactTypes)
    {
      if (requiredComponents.Contains(factType))
      {
        return true;
      }
    }
    return false;
  }
}

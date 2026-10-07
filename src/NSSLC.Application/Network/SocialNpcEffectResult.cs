using System;
using System.Collections.Generic;
using System.Linq;

namespace Terraria.Network;

/// <summary>Detached effects committed by one Social NPC owner operation.</summary>
public sealed class SocialNpcEffectResult
{
  private readonly IReadOnlyList<SocialNpcEffectSnapshot> _updatedNpcs;

  public SocialNpcEffectResult(
    SocialNpcEffectStatus status,
    IEnumerable<SocialNpcEffectSnapshot>? updatedNpcs = null,
    SocialProjectileSpawnSnapshot? spawnedProjectile = null)
  {
    if (!Enum.IsDefined(status))
    {
      throw new ArgumentOutOfRangeException(nameof(status));
    }

    SocialNpcEffectSnapshot[] snapshots = updatedNpcs?.ToArray() ??
      Array.Empty<SocialNpcEffectSnapshot>();
    _updatedNpcs = Array.AsReadOnly(snapshots);
    SpawnedProjectile = spawnedProjectile;
    Status = status;

    if (status == SocialNpcEffectStatus.NoEffect &&
        (snapshots.Length != 0 || spawnedProjectile is not null))
    {
      throw new ArgumentException("A no-effect result cannot contain committed effects.");
    }

    if (status == SocialNpcEffectStatus.StaleWorldRuntime &&
        (snapshots.Length != 0 || spawnedProjectile is not null))
    {
      throw new ArgumentException("A stale-world result cannot contain committed effects.");
    }
  }

  public SocialNpcEffectStatus Status { get; }

  public IReadOnlyList<SocialNpcEffectSnapshot> UpdatedNpcs => _updatedNpcs;

  public SocialProjectileSpawnSnapshot? SpawnedProjectile { get; }
}

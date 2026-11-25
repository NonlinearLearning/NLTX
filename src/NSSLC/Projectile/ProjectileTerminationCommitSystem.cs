using System;
using System.Collections.Generic;
using Arch.Core;
using Arch.System;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Entities;
using Terraria.Share.Relationships;

namespace Terraria.Unshared.Projectiles;

/// <summary>Removes pending projectiles at the explicit lifecycle commit boundary.</summary>
public sealed class ProjectileTerminationCommitSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly ProjectileLifecycleSystem _lifecycle;

  public ProjectileTerminationCommitSystem(
      ArchWorld world,
      ProjectileLifecycleSystem lifecycle,
      IEntityRuntimeHandleResolver registry)
      : base(world ?? throw new ArgumentNullException(nameof(world)))
  {
    _lifecycle = lifecycle ?? throw new ArgumentNullException(nameof(lifecycle));
    ArgumentNullException.ThrowIfNull(registry);
  }

  public override void Update(in EcsServerTickContext context)
  {
    IReadOnlyList<ProjectileTerminationRequest> pending =
        ProjectileTerminationQueue.Require(World).Drain(context.Tick);
    Dictionary<EntityRuntimeHandle, ProjectileTerminationRequest> unique = [];
    for (int index = 0; index < pending.Count; index++)
    {
      ProjectileTerminationRequest request = pending[index];
      if (!unique.ContainsKey(request.Handle))
      {
        unique.Add(request.Handle, request);
      }
    }

    List<ProjectileTerminationRequest> ordered = new(unique.Values);
    ordered.Sort(static (left, right) =>
    {
      int comparison = left.Handle.WorldScope.CompareTo(right.Handle.WorldScope);
      if (comparison != 0)
      {
        return comparison;
      }

      comparison = left.Handle.Id.Value.CompareTo(right.Handle.Id.Value);
      return comparison != 0
          ? comparison
          : left.Handle.Generation.CompareTo(right.Handle.Generation);
    });
    foreach (ProjectileTerminationRequest request in ordered)
    {
      _lifecycle.CommitTermination(request.Handle, request.Reason, context.Commands);
    }
  }
}

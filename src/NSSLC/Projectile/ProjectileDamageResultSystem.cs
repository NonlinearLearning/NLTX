using System;
using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Combat;
using Terraria.Share.Entities;
using Terraria.Share.Relationships;

namespace Terraria.Unshared.Projectiles;

/// <summary>Consumes shared Combat results and applies projectile-owned hit state.</summary>
public sealed class ProjectileDamageResultSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly IEntityRuntimeHandleResolver _runtime;
  private readonly ProjectileHitResolutionSystem _projectileHits;
  private readonly QueryDescription _query = new QueryDescription()
      .WithAll<ProjectileDispositionStateComponent>();

  public ProjectileDamageResultSystem(
      ArchWorld world,
      IEntityRuntimeHandleResolver runtime,
      ProjectileHitResolutionSystem projectileHits)
      : base(world ?? throw new ArgumentNullException(nameof(world)))
  {
    _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
    _projectileHits = projectileHits ?? throw new ArgumentNullException(nameof(projectileHits));
  }

  public override void Update(in EcsServerTickContext context)
  {
    List<IReadOnlyList<CombatDamageResult>> captured = [];
    CombatDamageResultQueueComponent resultQueue = CombatDamageResultQueue.Require(World);
    World.Query(
        in _query,
        (ArchEntity entity, ref ProjectileDispositionStateComponent disposition) =>
        {
          if (disposition.IsActive && _runtime.TryGetHandle(entity, out EntityRuntimeHandle source))
          {
            IReadOnlyList<CombatDamageResult> results = resultQueue.ConsumeResults(source);
            if (results.Count > 0)
            {
              captured.Add(results);
            }
          }
        });

    foreach (IReadOnlyList<CombatDamageResult> results in captured)
    {
      for (int index = 0; index < results.Count; index++)
      {
        DamageProcessResult result = results[index].Result;
        if (result.Applied && result.Receipt.HasValue)
        {
          DamageAppliedReceipt receipt = result.Receipt.Value;
          _projectileHits.Apply(in receipt, out _);
          continue;
        }

        // 没落地就把预扣退回：入队时已经预扣，拒绝要等 Combat 结算完才知道。
        // 顺手记下「预检放行但裁决拒绝」的差额，否则预检读的是过期状态这件事无从观测。
        if (results[index].Source is { } rejectedSource)
        {
          _projectileHits.Reject(rejectedSource);
          _projectileHits.NoteAuthorityRejection(result.RejectionReason);
        }
      }
    }
  }
}

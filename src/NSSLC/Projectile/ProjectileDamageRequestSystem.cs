using System;
using System.Collections.Generic;
using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Combat;
using Terraria.Share.Entities;
using Terraria.Share.Physics;
using Terraria.Share.Relationships;

namespace Terraria.Unshared.Projectiles;

/// <summary>Converts accepted projectile collision facts into shared Combat requests.</summary>
public sealed class ProjectileDamageRequestSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly ProjectileHitResolutionSystem _projectileHits;
  private readonly IEntityRuntimeHandleResolver _handles;
  private readonly CombatAttackCatalog _attackDefinitions;
  private readonly QueryDescription _query = new QueryDescription().WithAll<
      ProjectileDispositionStateComponent,
      ProjectileOwnerComponent,
      ProjectileCollisionPolicyComponent,
      ProjectileAttackDefinitionReferenceComponent>();
  private readonly QueryDescription _contactQueueQuery =
      new QueryDescription().WithAll<PhysicsContactQueueComponent>();

  public ProjectileDamageRequestSystem(
      ArchWorld world,
      ProjectileHitResolutionSystem projectileHits,
      IEntityRuntimeHandleResolver handles,
      CombatAttackCatalog attackDefinitions)
      : base(world ?? throw new ArgumentNullException(nameof(world)))
  {
    _projectileHits = projectileHits ?? throw new ArgumentNullException(nameof(projectileHits));
    _handles = handles ?? throw new ArgumentNullException(nameof(handles));
    _attackDefinitions = attackDefinitions ?? throw new ArgumentNullException(
        nameof(attackDefinitions));
  }

  public override void Update(in EcsServerTickContext context)
  {
    long tick = context.Tick;
    Dictionary<EntityRuntimeHandle, ArchEntity> projectiles = [];
    World.Query(
        in _query,
        (ArchEntity projectile) =>
        {
          if (_handles.TryGetHandle(projectile, out EntityRuntimeHandle handle))
          {
            projectiles[handle] = projectile;
          }
        });

    IReadOnlyList<PhysicsContactFact> facts = DrainContacts();
    CombatAttackIntentQueueComponent attackIntents = CombatAttackIntentQueue.Require(World);
    for (int index = 0; index < facts.Count; index++)
    {
      PhysicsContactFact fact = facts[index];
      if (!projectiles.TryGetValue(fact.First, out ArchEntity projectile) &&
          !projectiles.TryGetValue(fact.Second, out projectile))
      {
        continue;
      }

      EntityRuntimeHandle projectileHandle = projectiles.ContainsKey(fact.First)
          ? fact.First
          : fact.Second;
      EntityRuntimeHandle targetHandle = projectileHandle.Matches(fact.First)
          ? fact.Second
          : fact.First;
      ProjectileDispositionStateComponent disposition =
          World.Get<ProjectileDispositionStateComponent>(projectile);
      ProjectileOwnerComponent owner = World.Get<ProjectileOwnerComponent>(projectile);
      ProjectileCollisionPolicyComponent policy =
          World.Get<ProjectileCollisionPolicyComponent>(projectile);
      if (!disposition.IsActive ||
          (policy.IgnoreOwner && owner.Owner.Matches(targetHandle)) ||
          projectileHandle.Matches(targetHandle) ||
          _projectileHits.CheckAdmission(
              projectileHandle,
              targetHandle,
              tick) != DamageRejectionReason.None ||
          !targetHandle.IsValid)
      {
        continue;
      }

      // 目标免疫预检：只用来「别入队」。
      // 没有它，目标在窗口内而弹体持续接触时，每 tick 都要走一遍完整的
      // 入队 -> 攻击意图 -> 伤害请求 -> 拒绝 -> 回滚穿透，纯空转。
      // 它是经济性优化，不是保证：真正的裁决仍在 Commit 的 DamageImmunityQuery。
      CombatAttackId definitionId =
          World.Get<ProjectileAttackDefinitionReferenceComponent>(projectile).DefinitionId;
      if (!_attackDefinitions.TryGet(definitionId, out CombatAttack attack) ||
          !_projectileHits.CanHit(
              projectileHandle, targetHandle, attack.Channel, tick, attack.Payload.Type))
      {
        continue;
      }

      // 入队即预扣：回执要等到 Commit 才回来，同 tick 的第二次命中若读同一个计数，
      // "还剩几次"会算多一次。预扣失败说明预算已空，这一发不该再入队。
      if (!_projectileHits.TryPreviewConsume(projectileHandle))
      {
        continue;
      }

      attackIntents.Enqueue(
          projectileHandle,
          targetHandle,
          attack.Id,
          tick,
          DamageCause.Projectile,
          new DamageContact(fact.Contact.Point, fact.Contact.Normal));
    }
  }

  private IReadOnlyList<PhysicsContactFact> DrainContacts()
  {
    ArchEntity queueEntity = default;
    bool found = false;
    World.Query(
        in _contactQueueQuery,
        (ArchEntity entity) =>
        {
          queueEntity = entity;
          found = true;
        });

    if (!found)
    {
      throw new InvalidOperationException(
          "A PhysicsContactQueueComponent entity must exist before projectile contacts are consumed.");
    }

    return World.Get<PhysicsContactQueueComponent>(queueEntity).Drain();
  }
}

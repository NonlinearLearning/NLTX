using System;
using System.Collections.Generic;
using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Combat;
using Terraria.Entity;
using Terraria.Physics;
using Terraria.Relationships;

namespace Terraria.Projectile;

/// <summary>Converts accepted projectile collision facts into shared Combat requests.</summary>
public sealed class ProjectileDamageRequestSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly ProjectileHitResolutionSystem _projectileHits;
  private readonly IEntityRuntimeHandleResolver _handles;
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
      IEntityRuntimeHandleResolver handles)
      : base(world ?? throw new ArgumentNullException(nameof(world)))
  {
    _projectileHits = projectileHits ?? throw new ArgumentNullException(nameof(projectileHits));
    _handles = handles ?? throw new ArgumentNullException(nameof(handles));
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
              tick) != ProjectileHitAdmissionResult.Accepted ||
          !targetHandle.IsValid)
      {
        continue;
      }

      attackIntents.Enqueue(
          projectileHandle,
          targetHandle,
          World.Get<ProjectileAttackDefinitionReferenceComponent>(projectile).DefinitionId,
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

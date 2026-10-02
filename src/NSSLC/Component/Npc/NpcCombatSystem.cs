using System;
using System.Collections.Generic;
using System.Numerics;
using Terraria.Combat;
using Terraria.SpatialSimulation.Components;

namespace Terraria.Npc;

public sealed class NpcCombatSystem
{
  private readonly record struct DamageCommitState(
    NpcCombatResult Result,
    NpcHealthComponent HitNpcHealth,
    NpcHealthComponent LifeOwnerHealth,
    NpcLifecycleComponent LifeOwnerLifecycle,
    NpcTypeId LifeOwnerType,
    long Tick,
    NpcDeathPhaseContext? DeathPhaseContext);

  private readonly NpcDamageTrackingSystem _damageTracking;
  private readonly NpcDeathLifecycleSystem _deathLifecycle;
  private readonly INpcDamageOverTimeTextPort _damageOverTimeTextPort;

  public NpcCombatSystem(
    NpcDamageTrackingSystem damageTracking,
    NpcDeathLifecycleSystem deathLifecycle,
    INpcDamageOverTimeTextPort damageOverTimeTextPort)
  {
    ArgumentNullException.ThrowIfNull(damageTracking);
    ArgumentNullException.ThrowIfNull(deathLifecycle);
    ArgumentNullException.ThrowIfNull(damageOverTimeTextPort);
    _damageTracking = damageTracking;
    _deathLifecycle = deathLifecycle;
    _damageOverTimeTextPort = damageOverTimeTextPort;
  }

  public NpcCombatResult ResolveAndCommit(
    NpcTypeId npcType,
    NpcHealthComponent health,
    NpcLifecycleComponent lifecycle,
    NpcDamageRequest request,
    NpcParentRelationComponent? parentRelation = null,
    NpcParentHealthTarget? parentHealthTarget = null,
    NpcDeathPhaseContext? deathPhaseContext = null,
    DamageAcceptancePolicyComponent? damagePolicy = null)
  {
    ArgumentNullException.ThrowIfNull(health);
    ArgumentNullException.ThrowIfNull(lifecycle);

    DamageCommitState damageCommit = ResolveDamageAndTrack(
      npcType,
      health,
      lifecycle,
      request,
      parentRelation,
      parentHealthTarget,
      deathPhaseContext,
      damagePolicy);
    return ReconcileDeath(damageCommit);
  }

  public NpcStrikeResult ResolveAndCommitStrike(
    NpcTypeId npcType,
    NpcHealthComponent health,
    NpcLifecycleComponent lifecycle,
    NpcDamageRequest request,
    NpcKnockbackInput knockbackInput,
    NpcMovementSystem movementSystem,
    ref MovementStateComponent movementState,
    NpcParentRelationComponent? parentRelation = null,
    NpcParentHealthTarget? parentHealthTarget = null,
    NpcDeathPhaseContext? deathPhaseContext = null,
    DamageAcceptancePolicyComponent? damagePolicy = null)
  {
    ArgumentNullException.ThrowIfNull(health);
    ArgumentNullException.ThrowIfNull(lifecycle);
    ArgumentNullException.ThrowIfNull(movementSystem);

    DamageCommitState damageCommit = ResolveDamageAndTrack(
      npcType,
      health,
      lifecycle,
      request,
      parentRelation,
      parentHealthTarget,
      deathPhaseContext,
      damagePolicy);
    Vector2 velocityBefore = movementState.Velocity;
    NpcKnockbackResult knockbackResult = damageCommit.Result.Applied
      ? movementSystem.ApplyKnockback(
        npcType,
        knockbackInput,
        damageCommit.Result.ResolvedDamage,
        damageCommit.LifeOwnerHealth.MaximumLife,
        request.Critical,
        ref movementState)
      : new NpcKnockbackResult(
        IsEligible: false,
        VelocityBefore: velocityBefore,
        VelocityAfter: velocityBefore);

    NpcCombatResult combatResult = ReconcileDeath(damageCommit);
    return new NpcStrikeResult(combatResult, knockbackResult);
  }

  private DamageCommitState ResolveDamageAndTrack(
    NpcTypeId npcType,
    NpcHealthComponent health,
    NpcLifecycleComponent lifecycle,
    NpcDamageRequest request,
    NpcParentRelationComponent? parentRelation,
    NpcParentHealthTarget? parentHealthTarget,
    NpcDeathPhaseContext? deathPhaseContext,
    DamageAcceptancePolicyComponent? damagePolicy)
  {
    NpcHealthComponent lifeOwnerHealth = parentHealthTarget?.Health ?? health;
    NpcLifecycleComponent lifeOwnerLifecycle =
      parentHealthTarget?.Lifecycle ?? lifecycle;
    NpcTypeId lifeOwnerType = parentHealthTarget?.NpcType ?? npcType;
    bool lifeOwnerIsBoss = parentHealthTarget?.IsBoss ?? request.IsBoss;
    int lifeBefore = lifeOwnerHealth.CurrentLife;
    NpcDamageRejectionReason rejection = Validate(
      npcType,
      health,
      lifecycle,
      request,
      parentRelation,
      parentHealthTarget,
      damagePolicy);
    if (rejection != NpcDamageRejectionReason.None)
    {
      return new DamageCommitState(
        Rejected(rejection, request.Damage, lifeBefore),
        health,
        lifeOwnerHealth,
        lifeOwnerLifecycle,
        lifeOwnerType,
        request.Tick,
        deathPhaseContext);
    }

    int resolvedDamage = ResolveDamage(request);
    int acceptedDamage = Math.Min(resolvedDamage, lifeOwnerHealth.CurrentLife);
    if (acceptedDamage <= 0)
    {
      return new DamageCommitState(
        Rejected(NpcDamageRejectionReason.NoHealth, request.Damage, lifeBefore),
        health,
        lifeOwnerHealth,
        lifeOwnerLifecycle,
        lifeOwnerType,
        request.Tick,
        deathPhaseContext);
    }

    bool isImmortal = request.Immortal || damagePolicy?.IsImmortal == true;
    bool trackerRecorded = !isImmortal &&
      lifeOwnerLifecycle.IsActive &&
      !request.BypassTrackerCredit &&
      !request.IsForcedWorldDamage &&
      _damageTracking.TryRecordAppliedDamage(
        lifeOwnerType,
        npcType,
        request.Contributor,
        acceptedDamage,
        request.Tick,
        lifeOwnerIsBoss);

    int appliedDamage = 0;
    if (!isImmortal)
    {
      if (!lifeOwnerHealth.TryApplyDamage(acceptedDamage, out appliedDamage))
      {
        throw new InvalidOperationException(
          "NPC health changed between validation and damage commit.");
      }

      if (parentHealthTarget is not null)
      {
        health.SynchronizeFromParent(lifeOwnerHealth);
      }
    }

    return new DamageCommitState(
      new NpcCombatResult(
        Applied: true,
        RejectionReason: NpcDamageRejectionReason.None,
        RequestedDamage: request.Damage,
        AppliedDamage: appliedDamage,
        LifeBefore: lifeBefore,
        LifeAfter: lifeOwnerHealth.CurrentLife,
        TrackerRecorded: trackerRecorded,
        DeathTransitioned: false,
        LifeOwnerInstanceId: parentHealthTarget?.Identity.InstanceId,
        ResolvedDamage: resolvedDamage),
      health,
      lifeOwnerHealth,
      lifeOwnerLifecycle,
      lifeOwnerType,
      request.Tick,
      deathPhaseContext);
  }

  private NpcCombatResult ReconcileDeath(DamageCommitState damageCommit)
  {
    if (!damageCommit.Result.Applied)
    {
      return damageCommit.Result;
    }

    NpcDeathLifecycleResult deathResult;
    if (!damageCommit.LifeOwnerLifecycle.IsActive)
    {
      deathResult = new NpcDeathLifecycleResult(
        IsTerminal: false,
        Transitioned: false,
        AlreadyTerminal: false);
    }
    else if (damageCommit.DeathPhaseContext is NpcDeathPhaseContext phaseContext)
    {
      deathResult = _deathLifecycle.Reconcile(
        damageCommit.LifeOwnerType,
        damageCommit.LifeOwnerHealth,
        damageCommit.LifeOwnerLifecycle,
        phaseContext.IsLifeOwner,
        phaseContext.Center,
        phaseContext.Ai,
        phaseContext.IsGoodWorld,
        phaseContext.BottomY,
        phaseContext.ClothierSkeletronContext,
        phaseContext.SourceNpcInstanceId);
    }
    else
    {
      deathResult = _deathLifecycle.Reconcile(
        damageCommit.LifeOwnerHealth,
        damageCommit.LifeOwnerLifecycle);
    }

    if (!deathResult.IsTerminal &&
        deathResult.PhaseDecision is NpcDeathPhaseDecision phaseDecision &&
        phaseDecision.RestoreLifeToMaximum)
    {
      damageCommit.LifeOwnerHealth.RestoreToMaximumLife();
      if (!ReferenceEquals(
        damageCommit.HitNpcHealth,
        damageCommit.LifeOwnerHealth))
      {
        damageCommit.HitNpcHealth.SynchronizeFromParent(
          damageCommit.LifeOwnerHealth);
      }
    }

    if (deathResult.IsTerminal)
    {
      _damageTracking.MarkKilled(damageCommit.LifeOwnerType, damageCommit.Tick);
    }

    return damageCommit.Result with
    {
      LifeAfter = damageCommit.LifeOwnerHealth.CurrentLife,
      DeathTransitioned = deathResult.Transitioned,
      DeathLifecycle = deathResult,
    };
  }

  public NpcDamageOverTimeResult ApplyDamageOverTime(
    NpcTypeId npcType,
    NpcHealthComponent health,
    NpcLifecycleComponent lifecycle,
    NpcDamageOverTimeRequest request,
    NpcParentRelationComponent? parentRelation = null,
    NpcParentHealthTarget? parentHealthTarget = null,
    NpcDeathPhaseContext? deathPhaseContext = null)
  {
    ArgumentNullException.ThrowIfNull(health);
    ArgumentNullException.ThrowIfNull(lifecycle);

    NpcHealthComponent lifeOwnerHealth = parentHealthTarget?.Health ?? health;
    NpcLifecycleComponent lifeOwnerLifecycle =
      parentHealthTarget?.Lifecycle ?? lifecycle;
    NpcTypeId lifeOwnerType = parentHealthTarget?.NpcType ?? npcType;
    bool lifeOwnerIsBoss = parentHealthTarget?.IsBoss ?? request.IsBoss;
    NpcEntityIdentityComponent? lifeOwnerIdentity =
      parentHealthTarget?.Identity ?? request.TextSourceIdentity;
    int lifeBefore = lifeOwnerHealth.CurrentLife;
    NpcDamageRejectionReason rejection = ValidateDamageOverTime(
      npcType,
      health,
      lifecycle,
      request,
      parentRelation,
      parentHealthTarget);
    if (rejection != NpcDamageRejectionReason.None)
    {
      return RejectedDamageOverTime(
        rejection,
        request.Amount,
        lifeBefore,
        lifeOwnerIdentity?.InstanceId);
    }

    bool trackerRecorded = false;
    if (lifeOwnerLifecycle.IsActive)
    {
      int trackerDamage = Math.Min(request.Amount, lifeBefore);
      trackerRecorded = _damageTracking.TryRecordAppliedDamage(
        lifeOwnerType,
        npcType,
        new CombatContributorId(CombatContributorKind.World, null),
        trackerDamage,
        request.Tick,
        lifeOwnerIsBoss);
    }

    int lifeDamageApplied = 0;
    bool requiresForcedDeathStrike = false;
    if (!request.Immortal &&
        !lifeOwnerHealth.TryApplyDamageOverTime(
          request.Amount,
          out lifeDamageApplied,
          out requiresForcedDeathStrike))
    {
      throw new InvalidOperationException(
        "NPC health changed between DoT validation and health commit.");
    }

    if (parentHealthTarget is not null && lifeDamageApplied > 0)
    {
      health.SynchronizeFromParent(lifeOwnerHealth);
    }

    int lifeAfterDirectStage = lifeOwnerHealth.CurrentLife;
    var combatTextIntent = new NpcDamageOverTimeTextIntent(
      request.TextSourceIdentity.InstanceId,
      request.TextBounds,
      request.Amount,
      Dramatic: false,
      IsDamageOverTime: true);
    bool combatTextPublished = _damageOverTimeTextPort.TryPublish(in combatTextIntent);

    NpcCombatResult? forcedDeathStrikeResult = null;
    if (requiresForcedDeathStrike)
    {
      NpcDamageRequest forcedDeathStrikeRequest =
        NpcDamageRequest.FromLegacyOwner(
          damage: 9999,
          tick: request.Tick,
          legacyOwner: 255,
          isBoss: lifeOwnerIsBoss);
      forcedDeathStrikeResult = ResolveAndCommit(
        lifeOwnerType,
        lifeOwnerHealth,
        lifeOwnerLifecycle,
        forcedDeathStrikeRequest,
        deathPhaseContext: deathPhaseContext);
      if (parentHealthTarget is not null)
      {
        health.SynchronizeFromParent(lifeOwnerHealth);
      }
    }

    NpcDeathPacket28Intent? deathPacket28Intent = requiresForcedDeathStrike
      ? new NpcDeathPacket28Intent(
        lifeOwnerIdentity!.InstanceId,
        lifeOwnerIdentity.LegacySlot,
        Damage: 9999)
      : null;

    return new NpcDamageOverTimeResult(
      Accepted: true,
      RejectionReason: NpcDamageRejectionReason.None,
      RequestedDamage: request.Amount,
      DirectDamageApplied: lifeDamageApplied,
      LifeBefore: lifeBefore,
      LifeAfterDirectStage: lifeAfterDirectStage,
      TrackerRecorded: trackerRecorded,
      CombatTextIntent: combatTextIntent,
      CombatTextPublished: combatTextPublished,
      RequiresForcedDeathStrike: requiresForcedDeathStrike,
      ForcedDeathStrikeResult: forcedDeathStrikeResult,
      DeathPacket28Intent: deathPacket28Intent,
      LifeAfterForcedDeathStrike: lifeOwnerHealth.CurrentLife,
      LifeOwnerInstanceId: lifeOwnerIdentity!.InstanceId);
  }

  public NpcLifeRegenerationCommitResult CommitLifeRegeneration(
    NpcLifeRegenerationResult regeneration,
    NpcTypeId npcType,
    NpcHealthComponent health,
    NpcLifecycleComponent lifecycle,
    NpcDamageOverTimeRequest damageRequest,
    NpcParentRelationComponent? parentRelation = null,
    NpcParentHealthTarget? parentHealthTarget = null,
    NpcDeathPhaseContext? deathPhaseContext = null)
  {
    ArgumentNullException.ThrowIfNull(health);
    ArgumentNullException.ThrowIfNull(lifecycle);

    if (!regeneration.Computed)
    {
      return new NpcLifeRegenerationCommitResult(
        computed: false,
        failureReason: regeneration.FailureReason,
        healingRequested: regeneration.HealingPoints,
        healingApplied: 0,
        damageEvents: Array.Empty<NpcDamageOverTimeResult>());
    }

    if (regeneration.HealingPoints < 0 ||
        regeneration.DamageEventCount < 0 ||
        (regeneration.DamageEventCount > 0 && regeneration.DamagePerEvent <= 0))
    {
      return new NpcLifeRegenerationCommitResult(
        computed: false,
        failureReason: NpcLifeRegenerationFailureReason.InvalidInput,
        healingRequested: regeneration.HealingPoints,
        healingApplied: 0,
        damageEvents: Array.Empty<NpcDamageOverTimeResult>());
    }

    int healingApplied = health.ApplyHealing(regeneration.HealingPoints);
    var damageEvents = new List<NpcDamageOverTimeResult>(
      regeneration.DamageEventCount);
    for (int index = 0; index < regeneration.DamageEventCount; index++)
    {
      NpcDamageOverTimeRequest eventRequest = damageRequest with
      {
        Amount = regeneration.DamagePerEvent,
      };
      damageEvents.Add(ApplyDamageOverTime(
        npcType,
        health,
        lifecycle,
        eventRequest,
        parentRelation,
        parentHealthTarget,
        deathPhaseContext));
    }

    return new NpcLifeRegenerationCommitResult(
      computed: true,
      failureReason: NpcLifeRegenerationFailureReason.None,
      healingRequested: regeneration.HealingPoints,
      healingApplied: healingApplied,
      damageEvents: damageEvents.ToArray());
  }

  private static NpcDamageRejectionReason Validate(
    NpcTypeId npcType,
    NpcHealthComponent health,
    NpcLifecycleComponent lifecycle,
    NpcDamageRequest request,
    NpcParentRelationComponent? parentRelation,
    NpcParentHealthTarget? parentHealthTarget,
    DamageAcceptancePolicyComponent? damagePolicy)
  {
    if (!npcType.IsValid)
    {
      return NpcDamageRejectionReason.InvalidNpcType;
    }

    if (request.Damage <= 0)
    {
      return NpcDamageRejectionReason.InvalidDamage;
    }

    if (!request.Contributor.IsValid)
    {
      return NpcDamageRejectionReason.InvalidContributor;
    }

    if (request.LegacyOwner is int legacyOwner &&
        !request.Contributor.MatchesLegacyOwner(legacyOwner))
    {
      return NpcDamageRejectionReason.InvalidContributor;
    }

    if (float.IsNaN(request.TakenDamageMultiplier) ||
        float.IsInfinity(request.TakenDamageMultiplier) ||
        request.TakenDamageMultiplier < 0.0f)
    {
      return NpcDamageRejectionReason.InvalidMultiplier;
    }

    if (!lifecycle.IsActive)
    {
      return NpcDamageRejectionReason.Inactive;
    }

    if (!HasValidParentTarget(health, parentRelation, parentHealthTarget))
    {
      return NpcDamageRejectionReason.InvalidParentRelation;
    }

    if (health.IsDead)
    {
      return NpcDamageRejectionReason.NoHealth;
    }

    if (damagePolicy?.RejectAllDamage == true ||
        (damagePolicy?.RejectHostileDamage == true &&
         request.IsHostileDamage) ||
        (damagePolicy?.RejectTrapDamage == true && request.IsTrapDamage))
    {
      return NpcDamageRejectionReason.DamagePolicy;
    }

    return NpcDamageRejectionReason.None;
  }

  private static NpcDamageRejectionReason ValidateDamageOverTime(
    NpcTypeId npcType,
    NpcHealthComponent health,
    NpcLifecycleComponent lifecycle,
    NpcDamageOverTimeRequest request,
    NpcParentRelationComponent? parentRelation,
    NpcParentHealthTarget? parentHealthTarget)
  {
    if (!npcType.IsValid)
    {
      return NpcDamageRejectionReason.InvalidNpcType;
    }

    if (request.Amount <= 0)
    {
      return NpcDamageRejectionReason.InvalidDamage;
    }

    if (request.TextSourceIdentity is null ||
        !request.TextSourceIdentity.InstanceId.IsValid ||
        !request.TextSourceIdentity.HasAssignedSlot)
    {
      return NpcDamageRejectionReason.InvalidIdentity;
    }

    NpcEntityIdentityComponent lifeOwnerIdentity =
      parentHealthTarget?.Identity ?? request.TextSourceIdentity;
    if (!lifeOwnerIdentity.InstanceId.IsValid ||
        !lifeOwnerIdentity.HasAssignedSlot)
    {
      return NpcDamageRejectionReason.InvalidIdentity;
    }

    if (!lifecycle.IsActive)
    {
      return NpcDamageRejectionReason.Inactive;
    }

    if (!HasValidParentTarget(health, parentRelation, parentHealthTarget))
    {
      return NpcDamageRejectionReason.InvalidParentRelation;
    }

    NpcHealthComponent lifeOwnerHealth = parentHealthTarget?.Health ?? health;
    if (lifeOwnerHealth.IsDead)
    {
      return NpcDamageRejectionReason.NoHealth;
    }

    return NpcDamageRejectionReason.None;
  }

  private static bool HasValidParentTarget(
    NpcHealthComponent health,
    NpcParentRelationComponent? parentRelation,
    NpcParentHealthTarget? parentHealthTarget)
  {
    if (parentRelation is null || parentHealthTarget is null)
    {
      return parentRelation is null && parentHealthTarget is null;
    }

    if (!parentRelation.ParentInstanceId.IsValid ||
        parentRelation.ParentInstanceId != parentHealthTarget.Identity.InstanceId ||
        ReferenceEquals(health, parentHealthTarget.Health))
    {
      return false;
    }

    return !parentRelation.HasLegacyParentSlot ||
      (parentHealthTarget.Identity.HasAssignedSlot &&
       parentRelation.ParentLegacySlot == parentHealthTarget.Identity.LegacySlot);
  }

  private static int ResolveDamage(NpcDamageRequest request)
  {
    double damage = Math.Max(
      1.0,
      (double)request.Damage - (double)request.Defense * 0.5);
    if (request.Critical)
    {
      damage *= 2.0;
    }

    if (request.RedHatSkeletronAdjustmentEnabled)
    {
      damage = (int)(damage * 0.699999988079071);
      if (damage < 1.0)
      {
        damage = 1.0;
      }
    }

    if (request.TakenDamageMultiplier > 1.0f)
    {
      damage *= request.TakenDamageMultiplier;
    }

    return damage >= int.MaxValue ? int.MaxValue : (int)damage;
  }

  private static NpcCombatResult Rejected(
    NpcDamageRejectionReason reason,
    int requestedDamage,
    int lifeBefore)
  {
    return new NpcCombatResult(
      Applied: false,
      RejectionReason: reason,
      RequestedDamage: requestedDamage,
      AppliedDamage: 0,
      LifeBefore: lifeBefore,
      LifeAfter: lifeBefore,
      TrackerRecorded: false,
      DeathTransitioned: false);
  }

  private static NpcDamageOverTimeResult RejectedDamageOverTime(
    NpcDamageRejectionReason reason,
    int requestedDamage,
    int lifeBefore,
    NpcInstanceId? lifeOwnerInstanceId)
  {
    return new NpcDamageOverTimeResult(
      Accepted: false,
      RejectionReason: reason,
      RequestedDamage: requestedDamage,
      DirectDamageApplied: 0,
      LifeBefore: lifeBefore,
      LifeAfterDirectStage: lifeBefore,
      TrackerRecorded: false,
      CombatTextIntent: null,
      CombatTextPublished: false,
      RequiresForcedDeathStrike: false,
      ForcedDeathStrikeResult: null,
      DeathPacket28Intent: null,
      LifeAfterForcedDeathStrike: lifeBefore,
      LifeOwnerInstanceId: lifeOwnerInstanceId);
  }
}

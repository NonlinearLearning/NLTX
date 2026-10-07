using System;
using Terraria.Combat;

namespace Terraria.Npc;

public sealed class NpcStatusTickSystem
{
  private const int ShimmeringBuffType = 353;

  private readonly NpcStatusSystem _statusSystem;
  private readonly NpcCombatSystem _combatSystem;
  private readonly INpcSoulDrainVisualEffectPort _soulDrainVisualEffectPort;

  public NpcStatusTickSystem(
    NpcStatusSystem statusSystem,
    NpcCombatSystem combatSystem,
    INpcSoulDrainVisualEffectPort soulDrainVisualEffectPort)
  {
    ArgumentNullException.ThrowIfNull(statusSystem);
    ArgumentNullException.ThrowIfNull(combatSystem);
    ArgumentNullException.ThrowIfNull(soulDrainVisualEffectPort);
    _statusSystem = statusSystem;
    _combatSystem = combatSystem;
    _soulDrainVisualEffectPort = soulDrainVisualEffectPort;
  }

  public NpcStatusTickResult Advance(
    in NpcStatusTickInput input,
    ref StatusEffectSlotsComponent buffSlots,
    ref NpcStatusFlagsComponent statusFlags,
    ref NpcLifeRegenerationStateComponent regenerationState,
    NpcShimmerStateComponent shimmerState,
    NpcHealthComponent health,
    NpcLifecycleComponent lifecycle,
    NpcParentRelationComponent? parentRelation = null,
    NpcParentHealthTarget? parentHealthTarget = null,
    NpcDeathPhaseContext? deathPhaseContext = null)
  {
    ArgumentNullException.ThrowIfNull(health);
    ArgumentNullException.ThrowIfNull(lifecycle);
    ArgumentNullException.ThrowIfNull(shimmerState);
    if (!lifecycle.IsActive)
    {
      return new NpcStatusTickResult(
        SkippedInactive: true,
        BuffFlags: SkippedBuffFlags(),
        SoulDrainEffects: null,
        ExpiredBuffs: null,
        LifeRegeneration: null,
        LifeRegenerationCommit: null);
    }

    ArgumentNullException.ThrowIfNull(input.DamageOverTime.TextSourceIdentity);

    NpcBuffStateUpdateResult buffFlags = _statusSystem.ApplyBuffSlotPhase(
      input.LifeRegeneration.NpcType,
      input.AiState,
      input.Immunity,
      ref buffSlots,
      ref statusFlags,
      input.LowerBuffTime);
    if (!buffFlags.Applied)
    {
      return new NpcStatusTickResult(
        SkippedInactive: false,
        BuffFlags: buffFlags,
        SoulDrainEffects: null,
        ExpiredBuffs: null,
        LifeRegeneration: null,
        LifeRegenerationCommit: null);
    }

    var soulDrainInput = new NpcSoulDrainVisualEffectInput(
      new NpcSoulDrainEligibilityInput(
        statusFlags.SoulDrain,
        input.NpcCenter,
        input.SoulDrainPlayerSnapshot),
      input.NpcVelocity);
    NpcSoulDrainVisualEffectResult soulDrainEffects =
      NpcSoulDrainVisualEffectSystem.Apply(
        soulDrainInput,
        _soulDrainVisualEffectPort);

    NpcBuffStateUpdateResult expiredBuffs =
      _statusSystem.ClearExpiredBuffs(ref buffSlots);
    NpcLifeRegenerationInput lifeRegenerationInput =
      input.LifeRegeneration with
      {
        AiState1 = input.AiState.State1,
        CurrentLife = health.CurrentLife,
        MaximumLife = health.MaximumLife,
      };
    NpcLifeRegenerationResult lifeRegeneration =
      _statusSystem.ApplyDamageOverTimePhase(
        ref statusFlags,
        ref regenerationState,
        lifeRegenerationInput);
    NpcLifeRegenerationCommitResult? lifeRegenerationCommit = null;
    if (lifeRegeneration.Computed)
    {
      lifeRegenerationCommit = _combatSystem.CommitLifeRegeneration(
        lifeRegeneration,
        input.LifeRegeneration.NpcType,
        health,
        lifecycle,
        input.DamageOverTime,
        parentRelation,
        parentHealthTarget,
        deathPhaseContext);
    }

    ReadOnlySpan<bool> immuneByDefinition = input.Immunity.ImmunityByDefinitionId.Span;
    bool? shimmerBuffImmune = immuneByDefinition.Length > ShimmeringBuffType
      ? immuneByDefinition[ShimmeringBuffType]
      : null;
    NpcShimmerTransparencyInput shimmerInput = new(
      input.CanDisplayBuffs,
      statusFlags.Shimmering,
      input.JustHit,
      shimmerBuffImmune);
    NpcShimmerTransparencyResult shimmerTransparency =
      NpcShimmerTransparencySystem.Advance(shimmerState, in shimmerInput);

    NpcBloodMoonTransformationIntent? bloodMoonTransformation =
      NpcBloodMoonTransformationSystem.Evaluate(
        input.BloodMoonActive,
        input.CrimsonWorld,
        input.LifeRegeneration.NpcType,
        input.NpcValue);
    NpcGravityResult? gravity = null;
    bool gravityDeferredUntilPostTransformationSnapshot =
      bloodMoonTransformation.HasValue && input.GravityEnvironment.HasValue;
    if (input.GravityEnvironment.HasValue &&
        !gravityDeferredUntilPostTransformationSnapshot)
    {
      NpcGravityEnvironmentSnapshot environment = input.GravityEnvironment.Value;
      gravity = NpcGravitySystem.Evaluate(
        input.LifeRegeneration.NpcType,
        input.AiState,
        input.NpcVelocity.Y,
        in environment);
    }

    return new NpcStatusTickResult(
      SkippedInactive: false,
      BuffFlags: buffFlags,
      SoulDrainEffects: soulDrainEffects,
      ExpiredBuffs: expiredBuffs,
      LifeRegeneration: lifeRegeneration,
      LifeRegenerationCommit: lifeRegenerationCommit,
      BloodMoonTransformation: bloodMoonTransformation,
      Gravity: gravity,
      GravityDeferredUntilPostTransformationSnapshot:
        gravityDeferredUntilPostTransformationSnapshot,
      ShimmerTransparency: shimmerTransparency);
  }

  private static NpcBuffStateUpdateResult SkippedBuffFlags()
  {
    return new NpcBuffStateUpdateResult(
      applied: false,
      requiredInputMissing: false,
      buffSlotsChanged: false,
      effects: Array.Empty<NpcBuffStateUpdateResult.EffectIntent>());
  }
}

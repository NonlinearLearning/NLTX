using Terraria.Combat.TagEffects.Commands;
using Terraria.Content.StatusEffects;
using Terraria.EntityLifecycleAttribution;

namespace Terraria.Combat.TagEffects;

public sealed class TagEffectHitSystem
{
  public sealed class HitDecision
  {
    internal HitDecision(
      long hitId,
      EntityReference targetReference,
      int damage,
      bool critical,
      bool procEligible)
    {
      HitId = hitId;
      TargetReference = targetReference;
      Damage = damage;
      Critical = critical;
      ProcEligible = procEligible;
      Accepted = true;
    }

    private HitDecision()
    {
      Accepted = false;
    }

    public bool Accepted { get; }

    public bool Critical { get; }

    public int Damage { get; }

    public long HitId { get; }

    public bool ProcEligible { get; }

    public EntityReference TargetReference { get; }

    internal bool Committed { get; set; }

    internal static HitDecision Rejected => new();
  }

  private readonly TagEffectBehaviorPort _behavior;
  private readonly TagEffectLifecycleSystem _lifecycle;

  public TagEffectHitSystem(
    TagEffectLifecycleSystem lifecycle,
    TagEffectBehaviorPort behavior)
  {
    _lifecycle = lifecycle ?? throw new ArgumentNullException(nameof(lifecycle));
    _behavior = behavior ?? throw new ArgumentNullException(nameof(behavior));
  }

  public bool CommitAfterDamage(
    PlayerTagEffectStateComponent state,
    HitDecision decision,
    bool damageCommitted,
    int calcDamage)
  {
    ArgumentNullException.ThrowIfNull(state);
    ArgumentNullException.ThrowIfNull(decision);
    if (!damageCommitted || !decision.Accepted || decision.Committed ||
      state.ActiveEffectDefinition is not TagEffectDefinition definition)
    {
      return false;
    }

    decision.Committed = true;
    _behavior.OnTaggedHit(
      definition,
      state.OwnerReference,
      decision.TargetReference,
      calcDamage);
    if (decision.ProcEligible && _lifecycle.Apply(
      state,
      new ClearTagProcCommand(decision.TargetReference)))
    {
      _behavior.OnProcHit(
        definition,
        state.OwnerReference,
        decision.TargetReference,
        calcDamage);
    }

    return true;
  }

  public HitDecision ModifyHit(
    PlayerTagEffectStateComponent state,
    EntityReference targetReference,
    long hitId,
    int damage,
    bool critical)
  {
    ArgumentNullException.ThrowIfNull(state);
    TagEffectDefinition? definition = state.ActiveEffectDefinition;
    if (definition is null || !targetReference.IsValid ||
      !TagEffectHitEligibilityQuery.IsTagged(state, targetReference) ||
      !_behavior.CanRunHitEffects(definition, state.OwnerReference, targetReference))
    {
      return HitDecision.Rejected;
    }

    TagEffectBehaviorPort.HitModification tagged = _behavior.ModifyTaggedHit(
      definition,
      state.OwnerReference,
      targetReference,
      damage,
      critical);
    bool procEligible = TagEffectHitEligibilityQuery.CanProc(state, targetReference);
    if (procEligible)
    {
      tagged = _behavior.ModifyProcHit(
        definition,
        state.OwnerReference,
        targetReference,
        tagged.Damage,
        tagged.Critical);
    }

    return new HitDecision(
      hitId,
      targetReference,
      tagged.Damage,
      tagged.Critical,
      procEligible);
  }
}

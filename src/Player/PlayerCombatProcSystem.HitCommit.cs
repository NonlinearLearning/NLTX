namespace Terraria.Player;

public sealed partial class PlayerCombatProcSystem
{
  public bool AcceptCommittedHit(in PlayerCommittedCombatHitEvent hitEvent)
  {
    if (!IsAcceptable(hitEvent) || !_acceptedEventIds.Add(hitEvent.EventId))
    {
      return false;
    }

    PlayerCombatProcHitEffects effects = hitEvent.Effects;
    _component.GhostDmg = SaturatingAdd(
      _component.GhostDmg,
      effects.GhostDamage);
    _component.LifeSteal = MathF.Max(
      0f,
      _component.LifeSteal - effects.LifeStealCost);
    _component.OnHitDodge |= effects.OnHitDodge;
    _component.OnHitRegen |= effects.OnHitRegen;
    _component.OnHitPetal |= effects.OnHitPetal;
    _component.OnHitTitaniumStorm |= effects.OnHitTitaniumStorm;
    return true;
  }

  private static bool IsAcceptable(in PlayerCommittedCombatHitEvent hitEvent)
  {
    return hitEvent.EventId != Guid.Empty &&
      hitEvent.SourceId != Guid.Empty &&
      hitEvent.Damage > 0 &&
      hitEvent.SourceRevision >= 0 &&
      hitEvent.IsCommitted &&
      IsNonNegativeFinite(hitEvent.Effects.GhostDamage) &&
      IsNonNegativeFinite(hitEvent.Effects.LifeStealCost);
  }

  private static float SaturatingAdd(float current, float additional)
  {
    return current >= float.MaxValue - additional
      ? float.MaxValue
      : current + additional;
  }

  private static bool IsNonNegativeFinite(float value)
  {
    return float.IsFinite(value) && value >= 0f;
  }
}

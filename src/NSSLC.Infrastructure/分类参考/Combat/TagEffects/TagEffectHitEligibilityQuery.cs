using Terraria.EntityLifecycleAttribution;

namespace Terraria.Combat.TagEffects;

public static class TagEffectHitEligibilityQuery
{
  public static bool CanProc(
    PlayerTagEffectStateComponent state,
    EntityReference targetReference)
  {
    ArgumentNullException.ThrowIfNull(state);
    return state.ActiveEffectDefinition is not null &&
      state.TryGetMark(targetReference, out PlayerTagEffectStateComponent.NpcTagMark mark) &&
      mark.TagTicksRemaining > 0 && mark.ProcTicksRemaining > 0;
  }

  public static bool IsTagged(
    PlayerTagEffectStateComponent state,
    EntityReference targetReference)
  {
    ArgumentNullException.ThrowIfNull(state);
    return state.TryGetMark(targetReference, out PlayerTagEffectStateComponent.NpcTagMark mark) &&
      mark.TagTicksRemaining > 0;
  }
}

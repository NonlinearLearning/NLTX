using Terraria.Content.StatusEffects;

namespace Terraria.Combat.StatusEffects;

public sealed class NpcDebuffImmunityApplySystem
{
  public NpcDebuffImmunityApplyResult Apply(
    NpcDebuffImmunityState state,
    NpcDebuffImmunityDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(state);
    ArgumentNullException.ThrowIfNull(definition);

    bool changed = state.Replace(
      definition.ImmuneToWhips,
      definition.ImmuneToNonWhipBuffs,
      definition.SpecificEffectIds);
    return new NpcDebuffImmunityApplyResult(changed);
  }
}

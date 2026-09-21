using Terraria.Content.StatusEffects;
using Terraria.EntityLifecycleAttribution;

namespace Terraria.Combat.TagEffects;

public class TagEffectBehaviorPort
{
  public readonly record struct HitModification(int Damage, bool Critical);

  public virtual bool CanRunHitEffects(
    TagEffectDefinition definition,
    EntityReference owner,
    EntityReference target)
  {
    return true;
  }

  public virtual HitModification ModifyProcHit(
    TagEffectDefinition definition,
    EntityReference owner,
    EntityReference target,
    int damage,
    bool critical)
  {
    return new HitModification(damage, critical);
  }

  public virtual HitModification ModifyTaggedHit(
    TagEffectDefinition definition,
    EntityReference owner,
    EntityReference target,
    int damage,
    bool critical)
  {
    return new HitModification(damage, critical);
  }

  public virtual void OnProcHit(
    TagEffectDefinition definition,
    EntityReference owner,
    EntityReference target,
    int calcDamage)
  {
  }

  public virtual void OnRemovedFromPlayer(
    TagEffectDefinition definition,
    EntityReference owner)
  {
  }

  public virtual void OnSetToPlayer(
    TagEffectDefinition definition,
    EntityReference owner)
  {
  }

  public virtual void OnTagAppliedToNpc(
    TagEffectDefinition definition,
    EntityReference owner,
    EntityReference target)
  {
  }

  public virtual void OnTaggedHit(
    TagEffectDefinition definition,
    EntityReference owner,
    EntityReference target,
    int calcDamage)
  {
  }
}

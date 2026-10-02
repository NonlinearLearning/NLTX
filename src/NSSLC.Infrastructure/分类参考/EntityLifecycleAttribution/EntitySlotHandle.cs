namespace Terraria.EntityLifecycleAttribution;

public readonly record struct EntitySlotHandle(
  int RuntimeEntityId,
  int CompatibilitySlot,
  int Generation,
  EntitySlotPoolKind? PoolKind = null,
  PresentationEffectKind? PresentationKind = null)
{
  public bool IsValid => RuntimeEntityId >= 0 &&
                         CompatibilitySlot >= 0 &&
                         Generation > 0 &&
                         (PoolKind != EntitySlotPoolKind.PresentationEffect || PresentationKind.HasValue);

  public bool MatchesPool(EntitySlotPoolKind poolKind, PresentationEffectKind? presentationKind = null)
  {
    return PoolKind == poolKind &&
           (poolKind != EntitySlotPoolKind.PresentationEffect || PresentationKind == presentationKind);
  }

  public static EntitySlotHandle ForPool(
    EntitySlotPoolKind poolKind,
    int runtimeEntityId,
    int compatibilitySlot,
    int generation)
  {
    return new EntitySlotHandle(
      runtimeEntityId,
      compatibilitySlot,
      generation,
      poolKind,
      PresentationKind: null);
  }

  public static EntitySlotHandle ForPresentation(
    PresentationEffectKind presentationKind,
    int runtimeEntityId,
    int compatibilitySlot,
    int generation)
  {
    return new EntitySlotHandle(
      runtimeEntityId,
      compatibilitySlot,
      generation,
      EntitySlotPoolKind.PresentationEffect,
      presentationKind);
  }
}

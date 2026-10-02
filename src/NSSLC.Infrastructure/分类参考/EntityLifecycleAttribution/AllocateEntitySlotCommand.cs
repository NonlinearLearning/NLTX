namespace Terraria.EntityLifecycleAttribution;

public sealed record AllocateEntitySlotCommand(
  EntitySlotPoolKind PoolKind,
  int RuntimeEntityId,
  int? Owner,
  int? Identity,
  int? ProjectileType,
  WorldItemComponent? WorldItem,
  PresentationEffectKind? PresentationKind,
  int? PresentationLifetimeTicks)
{
  public static AllocateEntitySlotCommand ForProjectile(
    int runtimeEntityId,
    int owner,
    int identity,
    int projectileType)
  {
    return new AllocateEntitySlotCommand(
      EntitySlotPoolKind.Projectile,
      runtimeEntityId,
      owner,
      identity,
      projectileType,
      WorldItem: null,
      PresentationKind: null,
      PresentationLifetimeTicks: null);
  }

  public static AllocateEntitySlotCommand ForWorldItem(
    int runtimeEntityId,
    WorldItemComponent item)
  {
    ArgumentNullException.ThrowIfNull(item);
    return new AllocateEntitySlotCommand(
      EntitySlotPoolKind.WorldItem,
      runtimeEntityId,
      Owner: null,
      Identity: null,
      ProjectileType: null,
      WorldItem: item,
      PresentationKind: null,
      PresentationLifetimeTicks: null);
  }

  public static AllocateEntitySlotCommand ForNpc(int runtimeEntityId)
  {
    return new AllocateEntitySlotCommand(
      EntitySlotPoolKind.Npc,
      runtimeEntityId,
      Owner: null,
      Identity: null,
      ProjectileType: null,
      WorldItem: null,
      PresentationKind: null,
      PresentationLifetimeTicks: null);
  }

  public static AllocateEntitySlotCommand ForPresentation(
    int runtimeEntityId,
    PresentationEffectKind kind,
    int lifetimeTicks)
  {
    return new AllocateEntitySlotCommand(
      EntitySlotPoolKind.PresentationEffect,
      runtimeEntityId,
      Owner: null,
      Identity: null,
      ProjectileType: null,
      WorldItem: null,
      PresentationKind: kind,
      PresentationLifetimeTicks: lifetimeTicks);
  }
}

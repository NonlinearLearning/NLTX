namespace Terraria.EntityLifecycleAttribution;

public sealed class EntityPoolSet
{
  internal EntityPoolSet(
    WorldItemSlotStore worldItems,
    NpcEntitySlotStore npcs,
    ProjectileEntitySlotStore projectiles,
    PresentationEffectPoolStore presentationEffects,
    WorldContainerStore containers,
    WorldSignStore signs,
    ItemAnimationCatalog itemAnimations)
  {
    WorldItems = worldItems;
    Npcs = npcs;
    Projectiles = projectiles;
    PresentationEffects = presentationEffects;
    Containers = containers;
    Signs = signs;
    ItemAnimations = itemAnimations;
  }

  public WorldItemSlotStore WorldItems { get; }

  public NpcEntitySlotStore Npcs { get; }

  public ProjectileEntitySlotStore Projectiles { get; }

  public PresentationEffectPoolStore PresentationEffects { get; }

  public WorldContainerStore Containers { get; }

  public WorldSignStore Signs { get; }

  public ItemAnimationCatalog ItemAnimations { get; }
}

namespace Terraria.EntityLifecycleAttribution;

public sealed class EntityPoolInitializationSystem
{
  public EntityPoolSet Initialize(
    int worldItemCapacity,
    int npcCapacity,
    int projectileCapacity,
    int presentationCapacityPerKind)
  {
    return new EntityPoolSet(
      new WorldItemSlotStore(worldItemCapacity),
      new NpcEntitySlotStore(npcCapacity),
      new ProjectileEntitySlotStore(projectileCapacity),
      new PresentationEffectPoolStore(presentationCapacityPerKind),
      new WorldContainerStore(),
      new WorldSignStore(),
      new ItemAnimationCatalog());
  }
}

namespace Terraria.EntityLifecycleAttribution;

public sealed class WorldContainerLifecycleSystem
{
  private readonly WorldContainerStore _containers;

  public WorldContainerLifecycleSystem(WorldContainerStore containers)
  {
    _containers = containers ?? throw new ArgumentNullException(nameof(containers));
  }

  public bool TryRemoveEmptyChest(int slot)
  {
    return _containers.TryRemoveEmptyChest(slot);
  }
}

namespace Terraria.Items;

public sealed class ContainerCapacityComponent
{
  public ContainerCapacityComponent(
    ContainerKind kind,
    int slotCount,
    long? maximumWeight = null,
    bool allowsNestedContainers = false)
  {
    Kind = kind;
    SlotCount = slotCount;
    MaximumWeight = maximumWeight;
    AllowsNestedContainers = allowsNestedContainers;
  }

  public ContainerKind Kind;
  public int SlotCount;
  public long? MaximumWeight;
  public bool AllowsNestedContainers;

  public bool IsValid =>
    SlotCount >= 0 &&
    (!MaximumWeight.HasValue || MaximumWeight.Value >= 0);
}

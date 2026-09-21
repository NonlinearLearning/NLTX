using System;

namespace Terraria.Dome.Simulation.WorldObjects;

public readonly record struct ChestCapacityDefinition(
  int MaximumItems = ChestInventoryComponent.SlotCount)
{
  public const int AbsoluteMaximumItems = 200;

  public ChestCapacityDefinition Validate()
  {
    if (MaximumItems < ChestInventoryComponent.SlotCount || MaximumItems > AbsoluteMaximumItems)
    {
      throw new ArgumentOutOfRangeException(nameof(MaximumItems));
    }

    return this;
  }

  public bool IsExtended => MaximumItems > ChestInventoryComponent.SlotCount;
}

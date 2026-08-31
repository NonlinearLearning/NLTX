using System;

namespace Terraria.Dome.Simulation.Components;

public readonly record struct ProjectileMinionSpawnSourceComponent
{
  public ProjectileMinionSpawnSourceComponent(ushort itemType, int itemPrefix = 0)
  {
    if (itemType == 0 || itemPrefix < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(itemType));
    }

    ItemType = itemType;
    ItemPrefix = itemPrefix;
  }

  public ushort ItemType { get; }

  public int ItemPrefix { get; }

  public bool IsEnabled => ItemType != 0;
}

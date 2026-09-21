using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

public static class HellChestLootCycleQuery
{
  public static int CurrentItem(HellChestLootCycleComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.CurrentItem;
  }

  public static HellChestLootCycleSnapshot Snapshot(
    HellChestLootCycleComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.CreateSnapshot();
  }
}

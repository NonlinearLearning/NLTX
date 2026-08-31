using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TreeShakeWorklistPolicy
{
  public static bool TryEnqueue(
    TreeShakeWorklistSnapshot current,
    TreeShakeWorkItem item,
    out TreeShakeWorklistSnapshot next)
  {
    if (current.Items.Count >= TreeShakeWorklistSnapshot.MaximumCount || Contains(current.Items, item))
    {
      next = current;
      return false;
    }

    var items = new List<TreeShakeWorkItem>(current.Items)
    {
      item
    };
    next = new TreeShakeWorklistSnapshot(items);
    return true;
  }

  private static bool Contains(
    IReadOnlyList<TreeShakeWorkItem> items,
    TreeShakeWorkItem item)
  {
    for (int index = 0; index < items.Count; index++)
    {
      if (items[index] == item)
      {
        return true;
      }
    }

    return false;
  }
}

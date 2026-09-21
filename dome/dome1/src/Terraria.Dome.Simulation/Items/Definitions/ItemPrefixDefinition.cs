using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Items.Definitions;

public sealed class ItemPrefixDefinition
{
  private readonly HashSet<ushort> _eligiblePrefixIds = [];

  public ItemPrefixDefinition(IEnumerable<ushort> eligiblePrefixIds)
  {
    ArgumentNullException.ThrowIfNull(eligiblePrefixIds);
    foreach (ushort prefixId in eligiblePrefixIds)
    {
      if (prefixId == 0)
      {
        throw new ArgumentOutOfRangeException(
          nameof(eligiblePrefixIds),
          "Prefix zero is reserved for resetting an item prefix.");
      }

      if (!_eligiblePrefixIds.Add(prefixId))
      {
        throw new ArgumentException(
          "Eligible prefix IDs must be unique.",
          nameof(eligiblePrefixIds));
      }
    }
  }

  public bool CanApply(ushort prefixId)
  {
    return prefixId == 0 || _eligiblePrefixIds.Contains(prefixId);
  }
}

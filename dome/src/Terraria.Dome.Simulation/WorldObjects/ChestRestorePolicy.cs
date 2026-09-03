using System;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Items;

namespace Terraria.Dome.Simulation.WorldObjects;

public readonly record struct ChestRestorePolicy(
  bool RejectDuplicateIdentity = true,
  bool RejectDuplicateCoordinate = true,
  bool RejectOverflow = true)
{
  public bool TryValidate(
    ChestPersistentState snapshot,
    ChestCapacityDefinition capacity,
    out string? error)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    capacity.Validate();
    if (snapshot.Slots.Count > capacity.MaximumItems && RejectOverflow)
    {
      error = "The chest snapshot exceeds its configured capacity.";
      return false;
    }

    for (int index = 0; index < snapshot.Slots.Count; index++)
    {
      ItemStack stack = snapshot.Slots[index];
      if (stack.IsEmpty && stack != ItemStack.Empty)
      {
        error = "The chest snapshot contains a non-canonical empty slot.";
        return false;
      }
    }

    error = null;
    return true;
  }

  public bool ShouldRejectOverflow(int slotCount, ChestCapacityPolicy policy)
  {
    return policy == ChestCapacityPolicy.RejectOverflow &&
      slotCount > ChestCapacityDefinition.AbsoluteMaximumItems;
  }
}

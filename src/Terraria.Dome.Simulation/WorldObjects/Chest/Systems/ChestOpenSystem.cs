using System;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.WorldObjects.Chest.Commands;

namespace Terraria.Dome.Simulation.WorldObjects.Chest.Systems;

public sealed class ChestOpenSystem
{
  public bool TryApply(
    ChestComponent chest,
    ChestOpenCommand command,
    InventoryComponent? inventory = null)
  {
    ArgumentNullException.ThrowIfNull(chest);
    if (command.Sequence < 0 || !IsInRange(chest, command.PlayerPosition))
    {
      return false;
    }

    if (chest.IsLocked)
    {
      if (inventory is null || !HasKey(inventory, chest.LockDefinition.KeyItemType))
      {
        return false;
      }
    }

    if (chest.Opener is PlayerHandle opener && opener != command.Player)
    {
      return false;
    }

    if (chest.Revision == long.MaxValue)
    {
      return false;
    }

    if (chest.IsLocked)
    {
      if (chest.LockDefinition.ConsumesKey && !inventory!.TryConsume(chest.LockDefinition.KeyItemType))
      {
        return false;
      }

      chest.SetLocked(false);
    }

    _ = chest.TryOpen(command.Player);

    return chest.TryIncrementRevision();
  }

  private static bool HasKey(InventoryComponent inventory, ushort itemType)
  {
    for (int index = 0; index < InventoryComponent.SlotCount; index++)
    {
      ItemStack stack = inventory.GetSlot(index);
      if (!stack.IsEmpty && stack.ItemType == itemType)
      {
        return true;
      }
    }

    return false;
  }

  private static bool IsInRange(ChestComponent chest, SimulationVector position)
  {
    float deltaX = position.X - chest.TileX;
    float deltaY = position.Y - chest.TileY;
    return float.IsFinite(position.X) && float.IsFinite(position.Y) &&
      deltaX * deltaX + deltaY * deltaY <= 6.0f * 6.0f;
  }
}

using System;
using Terraria.Dome.Simulation.Items;

namespace Terraria.Dome.Simulation.Player.Components;

public sealed class PlayerEquipmentInventoryComponent
{
  public const int ArmorSlotCount = 20;
  public const int DyeSlotCount = 10;
  public const int MiscEquipmentSlotCount = 5;
  public const int MiscDyeSlotCount = 5;

  private readonly ItemStack[] _armor = new ItemStack[ArmorSlotCount];
  private readonly ItemStack[] _dye = new ItemStack[DyeSlotCount];
  private readonly ItemStack[] _miscEquipment = new ItemStack[MiscEquipmentSlotCount];
  private readonly ItemStack[] _miscDye = new ItemStack[MiscDyeSlotCount];

  public ReadOnlySpan<ItemStack> Armor => _armor;
  public ReadOnlySpan<ItemStack> Dye => _dye;
  public ReadOnlySpan<ItemStack> MiscEquipment => _miscEquipment;
  public ReadOnlySpan<ItemStack> MiscDye => _miscDye;

  public long Revision { get; private set; }

  public void SetArmor(int slot, ItemStack item)
  {
    Set(_armor, slot, item);
  }

  public void SetDye(int slot, ItemStack item)
  {
    Set(_dye, slot, item);
  }

  public void SetMiscEquipment(int slot, ItemStack item)
  {
    Set(_miscEquipment, slot, item);
  }

  public void SetMiscDye(int slot, ItemStack item)
  {
    Set(_miscDye, slot, item);
  }

  private void Set(ItemStack[] slots, int slot, ItemStack item)
  {
    if (slot < 0 || slot >= slots.Length)
    {
      throw new ArgumentOutOfRangeException(nameof(slot));
    }

    ItemStack normalized = item.IsEmpty ? ItemStack.Empty : item;
    if (slots[slot] == normalized)
    {
      return;
    }

    slots[slot] = normalized;
    Revision = checked(Revision + 1);
  }
}

namespace Terraria.Dome.Simulation.Items.Definitions;

public enum ItemEquipmentSlot : byte
{
  None,
  Head,
  Body,
  Legs,
  Accessory,
  Wings,
  Shield,
  Vanity
}

public readonly record struct ItemEquipmentDefinition(
  ItemEquipmentSlot Slot = ItemEquipmentSlot.None,
  int Defense = 0,
  bool Accessory = false,
  bool Vanity = false,
  bool Social = false);

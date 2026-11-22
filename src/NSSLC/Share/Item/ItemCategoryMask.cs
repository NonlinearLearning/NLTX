using System;

namespace Terraria.Item;

/// <summary>Categories used by inventory slots to accept or reject item stacks.</summary>
[Flags]
public enum ItemCategoryMask : ushort {
  None = 0,
  General = 1 << 0,
  Currency = 1 << 1,
  Armor = 1 << 2,
  Accessory = 1 << 3,
  Weapon = 1 << 4,
  Ammo = 1 << 5,
  Tool = 1 << 6,
  Block = 1 << 7,
  Consumable = 1 << 8,
  Material = 1 << 9,
  Any = ushort.MaxValue
}

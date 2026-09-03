namespace Terraria.Dome.Simulation.Players;

public readonly record struct PlayerPersistentItem(
  int SlotId,
  int Stack,
  byte Prefix,
  int ItemType,
  bool IsFavorited,
  bool IsNewAndShiny,
  ushort VariantId = 0,
  byte Dye = 0,
  byte Paint = 0,
  string? NameOverride = null);

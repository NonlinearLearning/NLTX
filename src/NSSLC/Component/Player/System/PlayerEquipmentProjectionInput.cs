namespace Terraria.Player;

public readonly record struct PlayerEquipmentProjectionInput(
  IReadOnlyList<bool> UsableArmorSlots,
  IReadOnlyList<bool> HiddenVisibleAccessories,
  bool WearsRobe);

namespace Terraria.Player;

public readonly record struct PlayerLoadoutSwitchPlan(
  Guid CommandId,
  int CurrentLoadoutIndex,
  int TargetLoadoutIndex,
  EquipmentLoadoutState CurrentLoadoutAfter,
  EquipmentLoadoutState TargetLoadoutAfter,
  ItemEntityRef[] EquipmentAfter,
  ItemEntityRef[] DyeAfter,
  bool[] HiddenAccessoriesAfter);

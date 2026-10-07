namespace Terraria.Player.Progression;

public readonly record struct PlayerConsumedUpgradeItemUseInput(
  int ItemType,
  int ItemAnimation,
  bool ItemTimeIsZero);

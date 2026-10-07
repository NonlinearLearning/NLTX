namespace Terraria.Player.Luck;

public readonly record struct PlayerLuckCalculationInput(
  int LadyBugLuckTimeLeft,
  int LadyBugGoodLuckTime,
  int LadyBugBadLuckTime,
  float TorchLuck,
  byte LuckPotion,
  byte KiteLuckLevel,
  bool UsedGalaxyPearl,
  bool LanternsUp,
  bool HasGardenGnomeNearby,
  bool Stinky,
  float EquipmentBasedLuckBonus,
  float CoinLuck,
  bool BrokenMirrorBadLuck);

namespace Terraria.Dome.Simulation.Combat.Systems;

public readonly record struct PlayerLuckInputs(
  int LadyBugLuckTimeLeft,
  float TorchLuck,
  int LuckPotionLevel,
  int KiteLuckLevel,
  bool UsedGalaxyPearl,
  bool LanternsUp,
  bool HasGardenGnomeNearby,
  bool Stinky,
  float EquipmentBasedLuckBonus,
  float CoinLuck,
  bool BrokenMirrorBadLuck);

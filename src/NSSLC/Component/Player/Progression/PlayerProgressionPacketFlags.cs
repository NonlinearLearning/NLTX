namespace Terraria.Player.Progression;

public readonly record struct PlayerProgressionPacketFlags(
  bool UnlockedBiomeTorches,
  bool UnlockedSuperCart,
  bool EnabledSuperCart,
  bool UsedAegisCrystal,
  bool UsedAegisFruit,
  bool UsedArcaneCrystal,
  bool UsedGalaxyPearl,
  bool UsedGummyWorm,
  bool UsedAmbrosia,
  bool AteArtisanBread);

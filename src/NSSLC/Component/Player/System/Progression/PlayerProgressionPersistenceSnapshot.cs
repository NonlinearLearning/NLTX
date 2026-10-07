namespace Terraria.Player.Progression;

public readonly record struct PlayerProgressionPersistenceSnapshot(
  bool UnlockedBiomeTorches,
  bool UsingBiomeTorches,
  bool AteArtisanBread,
  bool UsedAegisCrystal,
  bool UsedAegisFruit,
  bool UsedArcaneCrystal,
  bool UsedGalaxyPearl,
  bool UsedGummyWorm,
  bool UsedAmbrosia,
  bool DownedDd2EventAnyDifficulty,
  int AnglerQuestsFinished,
  int GolferScoreAccumulated,
  bool UnlockedSuperCart,
  bool EnabledSuperCart);

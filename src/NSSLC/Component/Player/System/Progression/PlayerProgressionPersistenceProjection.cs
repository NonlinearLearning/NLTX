namespace Terraria.Player.Progression;

public static class PlayerProgressionPersistenceProjection
{
  public static PlayerProgressionPersistenceSnapshot Project(
    PlayerUnlockProgressionLedgerComponent unlockProgression,
    PlayerConsumedProgressionLedgerComponent consumedProgression,
    PlayerQuestEventProgressComponent questEventProgress,
    bool biomeTorchPreference)
  {
    ArgumentNullException.ThrowIfNull(unlockProgression);
    ArgumentNullException.ThrowIfNull(consumedProgression);
    ArgumentNullException.ThrowIfNull(questEventProgress);

    return new PlayerProgressionPersistenceSnapshot(
      unlockProgression.UnlockedBiomeTorches,
      biomeTorchPreference,
      unlockProgression.AteArtisanBread,
      consumedProgression.UsedAegisCrystal,
      consumedProgression.UsedAegisFruit,
      consumedProgression.UsedArcaneCrystal,
      consumedProgression.UsedGalaxyPearl,
      consumedProgression.UsedGummyWorm,
      consumedProgression.UsedAmbrosia,
      questEventProgress.DownedDd2EventAnyDifficulty,
      questEventProgress.AnglerQuestsFinished,
      questEventProgress.GolferScoreAccumulated,
      unlockProgression.UnlockedSuperCart,
      unlockProgression.EnabledSuperCart);
  }
}

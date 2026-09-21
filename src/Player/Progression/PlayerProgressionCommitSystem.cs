using Terraria.Player;

namespace Terraria.Player.Progression;

public static class PlayerProgressionCommitSystem
{
  public static PlayerUnlockProgressCommitResult Commit(
    in UnlockPlayerProgressCommand command,
    ref PlayerUnlockProgressionLedgerComponent ledger)
  {
    if (!command.Token.IsValid)
    {
      return new PlayerUnlockProgressCommitResult(
        PlayerUnlockProgressCommitStatus.RejectedInvalidToken,
        command.Progression);
    }

    switch (command.Progression)
    {
      case PlayerUnlockProgressionKind.BiomeTorches:
        if (ledger.UnlockedBiomeTorches)
        {
          return new PlayerUnlockProgressCommitResult(
            PlayerUnlockProgressCommitStatus.AlreadyUnlocked,
            command.Progression);
        }

        ledger.UnlockedBiomeTorches = true;
        break;
      case PlayerUnlockProgressionKind.ArtisanBread:
        if (ledger.AteArtisanBread)
        {
          return new PlayerUnlockProgressCommitResult(
            PlayerUnlockProgressCommitStatus.AlreadyUnlocked,
            command.Progression);
        }

        ledger.AteArtisanBread = true;
        break;
      case PlayerUnlockProgressionKind.SuperCart:
        if (ledger.UnlockedSuperCart)
        {
          return new PlayerUnlockProgressCommitResult(
            PlayerUnlockProgressCommitStatus.AlreadyUnlocked,
            command.Progression);
        }

        ledger.UnlockedSuperCart = true;
        break;
      default:
        return new PlayerUnlockProgressCommitResult(
          PlayerUnlockProgressCommitStatus.RejectedUnknownProgression,
          command.Progression);
    }

    return new PlayerUnlockProgressCommitResult(
      PlayerUnlockProgressCommitStatus.Committed,
      command.Progression);
  }

  public static PlayerUnlockProgressCommitResult Commit(
    in SetPlayerSuperCartPreferenceCommand command,
    ref PlayerUnlockProgressionLedgerComponent ledger)
  {
    if (!command.Token.IsValid)
    {
      return new PlayerUnlockProgressCommitResult(
        PlayerUnlockProgressCommitStatus.RejectedInvalidToken,
        PlayerUnlockProgressionKind.SuperCart);
    }

    ledger.EnabledSuperCart = command.Enabled;
    return new PlayerUnlockProgressCommitResult(
      PlayerUnlockProgressCommitStatus.Committed,
      PlayerUnlockProgressionKind.SuperCart);
  }

  public static PlayerQuestEventProgressCommitResult Commit(
    in RecordAnglerQuestCommand command,
    ref PlayerQuestEventProgressComponent progress)
  {
    if (!command.Token.IsValid)
    {
      return new PlayerQuestEventProgressCommitResult(
        PlayerQuestEventProgressCommitStatus.RejectedInvalidToken);
    }

    if (!progress.TryMarkApplied(command.Token))
    {
      return new PlayerQuestEventProgressCommitResult(
        PlayerQuestEventProgressCommitStatus.AlreadyApplied);
    }

    progress.AnglerQuestsFinished++;
    return new PlayerQuestEventProgressCommitResult(
      PlayerQuestEventProgressCommitStatus.Committed);
  }

  public static PlayerQuestEventProgressCommitResult Commit(
    in AccumulateGolferScoreCommand command,
    ref PlayerQuestEventProgressComponent progress)
  {
    if (!command.Token.IsValid)
    {
      return new PlayerQuestEventProgressCommitResult(
        PlayerQuestEventProgressCommitStatus.RejectedInvalidToken);
    }

    if (command.Score < 0)
    {
      return new PlayerQuestEventProgressCommitResult(
        PlayerQuestEventProgressCommitStatus.RejectedInvalidScore);
    }

    if (!progress.TryMarkApplied(command.Token))
    {
      return new PlayerQuestEventProgressCommitResult(
        PlayerQuestEventProgressCommitStatus.AlreadyApplied);
    }

    var remaining = PlayerQuestEventProgressComponent.MaximumGolferScore -
      progress.GolferScoreAccumulated;
    progress.GolferScoreAccumulated += Math.Min(command.Score, remaining);
    return new PlayerQuestEventProgressCommitResult(
      PlayerQuestEventProgressCommitStatus.Committed);
  }

  public static PlayerQuestEventProgressCommitResult Commit(
    in RecordDd2ProgressCommand command,
    ref PlayerQuestEventProgressComponent progress)
  {
    if (!command.Token.IsValid)
    {
      return new PlayerQuestEventProgressCommitResult(
        PlayerQuestEventProgressCommitStatus.RejectedInvalidToken);
    }

    if (!progress.TryMarkApplied(command.Token))
    {
      return new PlayerQuestEventProgressCommitResult(
        PlayerQuestEventProgressCommitStatus.AlreadyApplied);
    }

    progress.DownedDd2EventAnyDifficulty = true;
    return new PlayerQuestEventProgressCommitResult(
      PlayerQuestEventProgressCommitStatus.Committed);
  }

  public static PlayerProgressionCommitResult Commit(
    in ConsumePlayerUpgradeCommand command,
    ref PlayerConsumedProgressionLedgerComponent ledger)
  {
    if (!command.Token.IsValid)
    {
      return new PlayerProgressionCommitResult(
        PlayerProgressionCommitStatus.RejectedInvalidToken,
        command.Upgrade);
    }

    if (!ConsumedUpgradeEligibilityQuery.IsSupported(command.Upgrade))
    {
      return new PlayerProgressionCommitResult(
        PlayerProgressionCommitStatus.RejectedUnknownUpgrade,
        command.Upgrade);
    }

    if (!ConsumedUpgradeEligibilityQuery.CanConsume(ledger, command.Upgrade))
    {
      return new PlayerProgressionCommitResult(
        PlayerProgressionCommitStatus.AlreadyConsumed,
        command.Upgrade);
    }

    switch (command.Upgrade)
    {
      case PlayerConsumedProgressionUpgrade.AegisCrystal:
        ledger.UsedAegisCrystal = true;
        break;
      case PlayerConsumedProgressionUpgrade.AegisFruit:
        ledger.UsedAegisFruit = true;
        break;
      case PlayerConsumedProgressionUpgrade.ArcaneCrystal:
        ledger.UsedArcaneCrystal = true;
        break;
      case PlayerConsumedProgressionUpgrade.GalaxyPearl:
        ledger.UsedGalaxyPearl = true;
        break;
      case PlayerConsumedProgressionUpgrade.GummyWorm:
        ledger.UsedGummyWorm = true;
        break;
      case PlayerConsumedProgressionUpgrade.Ambrosia:
        ledger.UsedAmbrosia = true;
        break;
      default:
        return new PlayerProgressionCommitResult(
          PlayerProgressionCommitStatus.RejectedUnknownUpgrade,
          command.Upgrade);
    }

    return new PlayerProgressionCommitResult(
      PlayerProgressionCommitStatus.Committed,
      command.Upgrade);
  }
}

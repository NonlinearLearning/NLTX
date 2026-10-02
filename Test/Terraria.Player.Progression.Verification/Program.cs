using Terraria.Player.Progression;

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

var progress = new PlayerQuestEventProgressComponent();
var firstCommand = new AccumulateGolferScoreCommand(
  750_000_000,
  new PlayerProgressionCommandToken(Guid.Parse("11111111-1111-1111-1111-111111111111")));
var firstResult = PlayerProgressionCommitSystem.Commit(firstCommand, ref progress);
Assert(firstResult.Status == PlayerQuestEventProgressCommitStatus.Committed,
  "The first golf score should commit.");
Assert(progress.GolferScoreAccumulated == 750_000_000,
  "The first golf score should accumulate in full.");

var capCommand = new AccumulateGolferScoreCommand(
  500_000_000,
  new PlayerProgressionCommandToken(Guid.Parse("22222222-2222-2222-2222-222222222222")));
var capResult = PlayerProgressionCommitSystem.Commit(capCommand, ref progress);
Assert(capResult.Status == PlayerQuestEventProgressCommitStatus.Committed,
  "The score that reaches the cap should commit.");
Assert(progress.GolferScoreAccumulated == 1_000_000_000,
  "Accumulated golf score should stop at one billion.");

var duplicateResult = PlayerProgressionCommitSystem.Commit(capCommand, ref progress);
Assert(duplicateResult.Status == PlayerQuestEventProgressCommitStatus.AlreadyApplied,
  "A repeated command token should be rejected as already applied.");
Assert(progress.GolferScoreAccumulated == 1_000_000_000,
  "A duplicate command must not add golf score again.");

var afterCapCommand = new AccumulateGolferScoreCommand(
  1,
  new PlayerProgressionCommandToken(Guid.Parse("33333333-3333-3333-3333-333333333333")));
var afterCapResult = PlayerProgressionCommitSystem.Commit(afterCapCommand, ref progress);
Assert(afterCapResult.Status == PlayerQuestEventProgressCommitStatus.Committed,
  "A distinct command after the cap should be accepted without changing the total.");
Assert(progress.GolferScoreAccumulated == 1_000_000_000,
  "The accumulated golf score should remain capped.");

var otherPlayerProgress = new PlayerQuestEventProgressComponent();
var otherPlayerResult = PlayerProgressionCommitSystem.Commit(
  firstCommand,
  ref otherPlayerProgress);
Assert(otherPlayerResult.Status == PlayerQuestEventProgressCommitStatus.Committed,
  "A command token applied to one player must not suppress another player.");
Assert(otherPlayerProgress.GolferScoreAccumulated == 750_000_000,
  "The second player's progress should be isolated from the first player.");

var retryProgress = new PlayerQuestEventProgressComponent();
var retryToken = new PlayerProgressionCommandToken(
  Guid.Parse("44444444-4444-4444-4444-444444444444"));
var invalidScoreResult = PlayerProgressionCommitSystem.Commit(
  new AccumulateGolferScoreCommand(-1, retryToken),
  ref retryProgress);
Assert(invalidScoreResult.Status == PlayerQuestEventProgressCommitStatus.RejectedInvalidScore,
  "A negative score should be rejected.");
Assert(retryProgress.GolferScoreAccumulated == 0,
  "A rejected score must not change player progress.");

var retryResult = PlayerProgressionCommitSystem.Commit(
  new AccumulateGolferScoreCommand(5, retryToken),
  ref retryProgress);
Assert(retryResult.Status == PlayerQuestEventProgressCommitStatus.Committed,
  "A rejected command must not consume its token before a valid retry.");
Assert(retryProgress.GolferScoreAccumulated == 5,
  "The valid retry should apply its score exactly once.");

var questProgress = progress;
var anglerCommand = new RecordAnglerQuestCommand(
  new PlayerProgressionCommandToken(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")));
var anglerResult = PlayerProgressionCommitSystem.Commit(anglerCommand, ref questProgress);
Assert(anglerResult.Status == PlayerQuestEventProgressCommitStatus.Committed,
  "The first Angler quest progress command should commit.");
Assert(questProgress.AnglerQuestsFinished == 1,
  "The first Angler quest progress command should increment the counter once.");
var repeatedAnglerResult = PlayerProgressionCommitSystem.Commit(anglerCommand, ref questProgress);
Assert(repeatedAnglerResult.Status == PlayerQuestEventProgressCommitStatus.AlreadyApplied,
  "A repeated Angler quest progress token should be rejected.");
Assert(questProgress.AnglerQuestsFinished == 1,
  "A repeated Angler quest progress command must not increment the counter again.");

var dd2Command = new RecordDd2ProgressCommand(
  new PlayerProgressionCommandToken(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")));
var dd2Result = PlayerProgressionCommitSystem.Commit(dd2Command, ref questProgress);
Assert(dd2Result.Status == PlayerQuestEventProgressCommitStatus.Committed,
  "The first DD2 progress command should commit.");
Assert(questProgress.DownedDd2EventAnyDifficulty,
  "The DD2 progress command should set its local progression flag.");
var repeatedDd2Result = PlayerProgressionCommitSystem.Commit(dd2Command, ref questProgress);
Assert(repeatedDd2Result.Status == PlayerQuestEventProgressCommitStatus.AlreadyApplied,
  "A repeated DD2 progress token should be rejected.");
Assert(questProgress.DownedDd2EventAnyDifficulty,
  "A repeated DD2 progress command must preserve the committed flag.");

var unlockLedger = new PlayerUnlockProgressionLedgerComponent();
var biomeTorchCommand = new UnlockPlayerProgressCommand(
  PlayerUnlockProgressionKind.BiomeTorches,
  new PlayerProgressionCommandToken(Guid.Parse("55555555-5555-5555-5555-555555555555")));
var biomeTorchResult = PlayerProgressionCommitSystem.Commit(
  biomeTorchCommand,
  ref unlockLedger);
Assert(biomeTorchResult.Status == PlayerUnlockProgressCommitStatus.Committed,
  "The first biome-torch unlock should commit.");
Assert(unlockLedger.UnlockedBiomeTorches,
  "The biome-torch unlock should update its ledger flag.");
Assert(UsingBiomeTorchesQuery.IsEnabled(unlockLedger, biomeTorchPreferenceEnabled: true),
  "Biome torches should be enabled when both unlock and preference are true.");
Assert(!UsingBiomeTorchesQuery.IsEnabled(unlockLedger, biomeTorchPreferenceEnabled: false),
  "Biome-torch preference should remain independent from its unlock.");

var repeatedBiomeTorchResult = PlayerProgressionCommitSystem.Commit(
  biomeTorchCommand,
  ref unlockLedger);
Assert(repeatedBiomeTorchResult.Status == PlayerUnlockProgressCommitStatus.AlreadyUnlocked,
  "A previously unlocked progression should reject another unlock commit.");

var invalidUnlockResult = PlayerProgressionCommitSystem.Commit(
  new UnlockPlayerProgressCommand(
    PlayerUnlockProgressionKind.ArtisanBread,
    default),
  ref unlockLedger);
Assert(invalidUnlockResult.Status == PlayerUnlockProgressCommitStatus.RejectedInvalidToken,
  "An empty unlock token should be rejected.");
Assert(!unlockLedger.AteArtisanBread,
  "A rejected unlock must leave its ledger flag unchanged.");
var artisanBreadResult = PlayerProgressionCommitSystem.Commit(
  new UnlockPlayerProgressCommand(
    PlayerUnlockProgressionKind.ArtisanBread,
    new PlayerProgressionCommandToken(Guid.Parse("99999999-9999-9999-9999-999999999999"))),
  ref unlockLedger);
Assert(artisanBreadResult.Status == PlayerUnlockProgressCommitStatus.Committed,
  "The Artisan Bread unlock should commit independently.");
Assert(unlockLedger.AteArtisanBread,
  "The Artisan Bread unlock should update its ledger flag.");

var unknownUnlockResult = PlayerProgressionCommitSystem.Commit(
  new UnlockPlayerProgressCommand(
    (PlayerUnlockProgressionKind)byte.MaxValue,
    biomeTorchCommand.Token),
  ref unlockLedger);
Assert(unknownUnlockResult.Status == PlayerUnlockProgressCommitStatus.RejectedUnknownProgression,
  "An unknown progression kind should be rejected.");
Assert(!unlockLedger.UnlockedSuperCart,
  "An unknown progression must not modify a known ledger flag.");

Assert(!UsingSuperCartQuery.IsEnabled(unlockLedger),
  "Super Cart should remain unavailable before unlock.");
var superCartUnlockResult = PlayerProgressionCommitSystem.Commit(
  new UnlockPlayerProgressCommand(
    PlayerUnlockProgressionKind.SuperCart,
    new PlayerProgressionCommandToken(Guid.Parse("66666666-6666-6666-6666-666666666666"))),
  ref unlockLedger);
Assert(superCartUnlockResult.Status == PlayerUnlockProgressCommitStatus.Committed,
  "The Super Cart unlock should commit independently.");
Assert(UsingSuperCartQuery.IsEnabled(unlockLedger),
  "The default enabled preference should take effect after unlock.");

var disableSuperCartCommand = new SetPlayerSuperCartPreferenceCommand(
  false,
  new PlayerProgressionCommandToken(Guid.Parse("77777777-7777-7777-7777-777777777777")));
var disableSuperCartResult = PlayerProgressionCommitSystem.Commit(
  disableSuperCartCommand,
  ref unlockLedger);
Assert(disableSuperCartResult.Status == PlayerUnlockProgressCommitStatus.Committed,
  "A valid Super Cart preference change should commit.");
Assert(!unlockLedger.EnabledSuperCart && !UsingSuperCartQuery.IsEnabled(unlockLedger),
  "Disabling the preference should hide an already unlocked Super Cart.");

var enableSuperCartCommand = new SetPlayerSuperCartPreferenceCommand(
  true,
  new PlayerProgressionCommandToken(Guid.Parse("88888888-8888-8888-8888-888888888888")));
var enableSuperCartResult = PlayerProgressionCommitSystem.Commit(
  enableSuperCartCommand,
  ref unlockLedger);
Assert(enableSuperCartResult.Status == PlayerUnlockProgressCommitStatus.Committed,
  "A valid Super Cart preference change should commit.");
Assert(UsingSuperCartQuery.IsEnabled(unlockLedger),
  "Enabling the preference should restore the effective unlocked state.");
Assert(PlayerProgressionCommitSystem.Commit(enableSuperCartCommand, ref unlockLedger).Status ==
  PlayerUnlockProgressCommitStatus.Committed,
  "Preference assignment should remain a direct state assignment rather than token deduplication.");

var invalidPreferenceResult = PlayerProgressionCommitSystem.Commit(
  new SetPlayerSuperCartPreferenceCommand(false, default),
  ref unlockLedger);
Assert(invalidPreferenceResult.Status == PlayerUnlockProgressCommitStatus.RejectedInvalidToken,
  "An empty preference token should be rejected.");
Assert(unlockLedger.EnabledSuperCart && UsingSuperCartQuery.IsEnabled(unlockLedger),
  "A rejected preference change must preserve the existing enabled state.");

var consumedLedger = new PlayerConsumedProgressionLedgerComponent();
foreach (PlayerConsumedProgressionUpgrade upgrade in new[]
         {
           PlayerConsumedProgressionUpgrade.AegisCrystal,
           PlayerConsumedProgressionUpgrade.AegisFruit,
           PlayerConsumedProgressionUpgrade.ArcaneCrystal,
           PlayerConsumedProgressionUpgrade.GalaxyPearl,
           PlayerConsumedProgressionUpgrade.GummyWorm,
           PlayerConsumedProgressionUpgrade.Ambrosia,
         })
{
  var consumeResult = PlayerProgressionCommitSystem.Commit(
    new ConsumePlayerUpgradeCommand(
      upgrade,
      new PlayerProgressionCommandToken(Guid.NewGuid())),
    ref consumedLedger);
  Assert(consumeResult.Status == PlayerProgressionCommitStatus.Committed,
    "Each supported consumed upgrade should be present in persistence state.");
}

PlayerProgressionPersistenceSnapshot persistenceSnapshot =
  PlayerProgressionPersistenceProjection.Project(
    unlockLedger,
    consumedLedger,
    questProgress,
    biomeTorchPreference: true);
var expectedPersistenceSnapshot = new PlayerProgressionPersistenceSnapshot(
  UnlockedBiomeTorches: true,
  UsingBiomeTorches: true,
  AteArtisanBread: true,
  UsedAegisCrystal: true,
  UsedAegisFruit: true,
  UsedArcaneCrystal: true,
  UsedGalaxyPearl: true,
  UsedGummyWorm: true,
  UsedAmbrosia: true,
  DownedDd2EventAnyDifficulty: true,
  AnglerQuestsFinished: 1,
  GolferScoreAccumulated: 1_000_000_000,
  UnlockedSuperCart: true,
  EnabledSuperCart: true);
Assert(persistenceSnapshot == expectedPersistenceSnapshot,
  "The persistence projection should preserve each P05 progression value.");
Assert(PlayerProgressionPersistenceProjection.Project(
    unlockLedger,
    consumedLedger,
    questProgress,
    biomeTorchPreference: false) is { UnlockedBiomeTorches: true, UsingBiomeTorches: false },
  "The persisted biome-torch preference should remain independent from its unlock.");

Console.WriteLine(
  "PASS: progression commits and persistence snapshot projection");

using Terraria.Player.Progression;
using Terraria.Relationships;

if (args.Length == 1 && args[0] == "--boss-pet-only")
{
  VerifyBossPetCapabilityRebuild();
  Console.WriteLine("P05 focused verifier passed: boss pet rebuild and reset.");
  return;
}

if (args.Length == 1 && args[0] == "--crossover-pet-only")
{
  VerifyCrossoverPetCapabilityRebuild();
  Console.WriteLine("P05 focused verifier passed: crossover pet rebuild and reset.");
  return;
}

if (args.Length == 1 && args[0] == "--world-object-pet-only")
{
  VerifyWorldObjectPetCapabilityRebuild();
  Console.WriteLine("P05 focused verifier passed: world-object pet rebuild and reset.");
  return;
}

VerifyConsumedUpgradeEligibility();
VerifyBossPetCapabilityRebuild();
VerifyCrossoverPetCapabilityRebuild();
VerifyWorldObjectPetCapabilityRebuild();
VerifyStandardNamedPetCapabilityRebuild();

var owner = new EntityReference(Guid.NewGuid(), EntityReferenceScope.Player);
var component = new PlayerMinionCapacityComponent(owner);
var minionCore = new PlayerCoreMinionCapabilityComponent();
var crossoverMinions = new PlayerCrossoverMinionCapabilityComponent();
var minionInput = new PlayerMinionCapabilityRebuildInput
{
  Pygmy = true,
  Raven = true,
  Slime = true,
  HornetMinion = true,
  ImpMinion = true,
  TwinsMinion = true,
  SpiderMinion = true,
  PirateMinion = true,
  SharknadoMinion = true,
  UfoMinion = true,
  DeadlySphereMinion = true,
  StardustMinion = true,
  StardustGuardian = true,
  StardustDragon = true,
  BatsOfLight = true,
  BabyBird = true,
  VampireFrog = true,
  StormTiger = true,
  Smolstar = true,
  EmpressBlade = true,
  FlinxMinion = true,
  AbigailMinion = true,
  DeadCellsMushroomBoiMinion = true,
  PalworldCattivaMinion = true,
  PalworldFoxsparksMinion = true,
};

PlayerMinionCapabilityRebuildSystem.Rebuild(minionInput, minionCore, crossoverMinions);
AssertAllCapabilityFlags(minionCore, expected: true);
AssertAllCapabilityFlags(crossoverMinions, expected: true);
PlayerMinionCapabilityRebuildSystem.Reset(minionCore, crossoverMinions);
AssertAllCapabilityFlags(minionCore, expected: false);
AssertAllCapabilityFlags(crossoverMinions, expected: false);

var seasonalPets = new PlayerSeasonalEventPetCapabilityComponent();
var seasonalPetInput = new PlayerSeasonalEventPetCapabilityRebuildInput
{
  PetFlagDD2Gato = true,
  PetFlagDD2Ghost = true,
  PetFlagDD2Dragon = true,
  PetFlagPumpkingPet = true,
  PetFlagEverscreamPet = true,
  PetFlagIceQueenPet = true,
  PetFlagMartianPet = true,
  PetFlagDD2OgrePet = true,
  PetFlagDD2BetsyPet = true,
};
PlayerSeasonalEventPetCapabilityRebuildSystem.Rebuild(seasonalPetInput, seasonalPets);
Assert(
  typeof(PlayerSeasonalEventPetCapabilityComponent).GetProperties().Length == 9,
  "The seasonal pet rebuild verifier must cover all nine capability flags.");
AssertAllCapabilityFlags(seasonalPets, expected: true);
PlayerSeasonalEventPetCapabilityRebuildSystem.Reset(seasonalPets);
AssertAllCapabilityFlags(seasonalPets, expected: false);

var minionDamage = new PlayerMinionDamageHighWaterMarkComponent();
PlayerMinionDamageHighWaterMarkSystem.AccumulateStormTigerGemOriginalDamage(
  minionDamage,
  originalDamage: 100);
PlayerMinionDamageHighWaterMarkSystem.AccumulateStormTigerGemOriginalDamage(
  minionDamage,
  originalDamage: 75);
PlayerMinionDamageHighWaterMarkSystem.AccumulateAbigailCounterOriginalDamage(
  minionDamage,
  originalDamage: 40);
PlayerMinionDamageHighWaterMarkSystem.AccumulateAbigailCounterOriginalDamage(
  minionDamage,
  originalDamage: 55);
Assert(
  minionDamage.HighestStormTigerGemOriginalDamage == 100,
  "Storm Tiger damage should keep its high-water mark.");
Assert(
  minionDamage.HighestAbigailCounterOriginalDamage == 55,
  "Abigail damage should keep an independent high-water mark.");
PlayerMinionDamageHighWaterMarkSystem.AccumulateAbigailCounterOriginalDamage(
  minionDamage,
  originalDamage: -1);
Assert(
  minionDamage.HighestAbigailCounterOriginalDamage == 55,
  "A lower damage observation must not reduce the high-water mark.");
PlayerMinionDamageHighWaterMarkSystem.Reset(minionDamage);
Assert(
  minionDamage.HighestStormTigerGemOriginalDamage == 0 &&
    minionDamage.HighestAbigailCounterOriginalDamage == 0,
  "Reset should clear both damage high-water marks.");

var fishing = new PlayerFishingCapabilitySnapshotComponent();
PlayerFishingCapabilityRebuildSystem.Reset(ref fishing);
var buffFishingContribution = new PlayerFishingCapabilityContributionInput(
  FishingSkillDelta: 15,
  CratePotion: true,
  SonarPotion: true,
  AccFishingLine: false,
  AccFishingBobber: false,
  AccTackleBox: false,
  AccLavaFishing: false);
var permanentUpgradeFishingContribution = new PlayerFishingCapabilityContributionInput(
  FishingSkillDelta: 3,
  CratePotion: false,
  SonarPotion: false,
  AccFishingLine: false,
  AccFishingBobber: false,
  AccTackleBox: false,
  AccLavaFishing: false);
var equipmentFishingContribution = new PlayerFishingCapabilityContributionInput(
  FishingSkillDelta: 10,
  CratePotion: false,
  SonarPotion: false,
  AccFishingLine: true,
  AccFishingBobber: true,
  AccTackleBox: true,
  AccLavaFishing: true);
PlayerFishingCapabilityRebuildSystem.Contribute(buffFishingContribution, ref fishing);
PlayerFishingCapabilityRebuildSystem.Contribute(
  permanentUpgradeFishingContribution,
  ref fishing);
PlayerFishingCapabilityRebuildSystem.Contribute(equipmentFishingContribution, ref fishing);
Assert(
  FishingCapabilityQuery.GetFishingSkill(fishing) == 28,
  "Fishing skill contributions should add.");
Assert(
  FishingCapabilityQuery.HasCratePotion(fishing),
  "Crate potion contributions should combine.");
Assert(
  FishingCapabilityQuery.HasSonarPotion(fishing),
  "Sonar potion contributions should combine.");
Assert(
  FishingCapabilityQuery.HasFishingLineProtection(fishing),
  "Fishing line capability should combine.");
Assert(FishingCapabilityQuery.HasBobberBonus(fishing), "Bobber capability should combine.");
Assert(
  FishingCapabilityQuery.HasTackleBoxBonus(fishing),
  "Tackle box capability should combine.");
Assert(
  FishingCapabilityQuery.CanFishInLava(fishing),
  "Lava fishing capability should combine.");
PlayerFishingCapabilityRebuildSystem.Reset(ref fishing);
Assert(
  FishingCapabilityQuery.GetFishingSkill(fishing) == 0,
  "Fishing reset should clear skill.");
Assert(
  !FishingCapabilityQuery.HasCratePotion(fishing),
  "Fishing reset should clear crate potion.");
Assert(
  !FishingCapabilityQuery.HasSonarPotion(fishing),
  "Fishing reset should clear sonar potion.");
Assert(
  !FishingCapabilityQuery.HasFishingLineProtection(fishing),
  "Fishing reset should clear line capability.");
Assert(
  !FishingCapabilityQuery.HasBobberBonus(fishing),
  "Fishing reset should clear bobber capability.");
Assert(
  !FishingCapabilityQuery.HasTackleBoxBonus(fishing),
  "Fishing reset should clear tackle capability.");
Assert(
  !FishingCapabilityQuery.CanFishInLava(fishing),
  "Fishing reset should clear lava capability.");

Assert(component.MaxMinions == 1, "Capacity should start with one available minion.");
Assert(component.NumMinions == 0, "Capacity should start with no active minions.");
Assert(component.SlotsMinions == 0, "Capacity should start with no occupied slots.");
Assert(RemainingMinionCapacityQuery.Get(component) == 1, "Remaining capacity should be one slot.");

var first = new SubmitMinionCapacityDeltaCommand(
  owner,
  owner,
  ProjectileOwnerSlot: 7,
  ProjectileIdentity: 7,
  MinionCountDelta: 1,
  SlotDelta: 0.5f,
  SourceRevision: 1,
  IdempotencyToken: Guid.NewGuid());

var system = new PlayerMinionCapacityCommitSystem(component);
var otherOwner = new EntityReference(Guid.NewGuid(), EntityReferenceScope.Player);
var commandForOtherOwner = first with
{
  Owner = otherOwner,
  ProjectileOwner = otherOwner,
};
Assert(
  system.Apply(commandForOtherOwner) == PlayerMinionCapacityCommitStatus.RejectedInvalidCommand,
  "A command for another player must not mutate this player's capacity.");
Assert(component.NumMinions == 0, "A rejected owner must not change the target player's count.");

var mismatchedOwner = first with
{
  ProjectileOwner = new EntityReference(Guid.NewGuid(), EntityReferenceScope.Player),
};
Assert(
  system.Apply(mismatchedOwner) == PlayerMinionCapacityCommitStatus.RejectedInvalidCommand,
  "The command must reject a projectile identity from another owner.");

var invalidOwnerSlot = first with { ProjectileOwnerSlot = byte.MaxValue + 1 };
Assert(
  system.Apply(invalidOwnerSlot) == PlayerMinionCapacityCommitStatus.RejectedInvalidCommand,
  "The command must reject an owner slot outside the source byte range.");

var invalidProjectileIdentity = first with { ProjectileIdentity = -1 };
Assert(
  system.Apply(invalidProjectileIdentity) == PlayerMinionCapacityCommitStatus.RejectedInvalidCommand,
  "The command must reject a negative projectile identity.");

var firstResult = system.Apply(first);
Assert(firstResult == PlayerMinionCapacityCommitStatus.Committed, "First delta should commit.");
Assert(component.SlotsMinions == 0.5f, "First delta should update occupied slots.");

var duplicateSystem = new PlayerMinionCapacityCommitSystem(component);
var duplicateResult = duplicateSystem.Apply(first);
Assert(duplicateResult == PlayerMinionCapacityCommitStatus.AlreadyApplied, "Duplicate delta should be idempotent.");
Assert(component.SlotsMinions == 0.5f, "Duplicate delta must not update occupied slots after system recreation.");

var countInput = new SubmitMinionCapacityDeltaCommand(
  owner,
  owner,
  ProjectileOwnerSlot: 7,
  ProjectileIdentity: 8,
  MinionCountDelta: 1,
  SlotDelta: 0.5f,
  SourceRevision: 2,
  IdempotencyToken: Guid.NewGuid());
var countResult = system.Apply(countInput);
Assert(countResult == PlayerMinionCapacityCommitStatus.Committed, "A second minion should commit.");
Assert(component.NumMinions == 2, "A positive count delta should update the aggregate count.");
Assert(component.SlotsMinions == 1f, "A positive slot delta should fill the capacity.");
Assert(!RemainingMinionCapacityQuery.HasCapacity(component), "A full capacity should not admit another minion.");

var rejected = new SubmitMinionCapacityDeltaCommand(
  owner,
  owner,
  ProjectileOwnerSlot: 7,
  ProjectileIdentity: 9,
  MinionCountDelta: 1,
  SlotDelta: -2f,
  SourceRevision: 3,
  IdempotencyToken: Guid.NewGuid());
var rejectedResult = system.Apply(rejected);
Assert(rejectedResult == PlayerMinionCapacityCommitStatus.RejectedInvalidDelta, "Negative capacity must be rejected.");

system.Reset();
Assert(component.MaxMinions == 1, "Reset should restore the default maximum.");
Assert(component.NumMinions == 0, "Reset should clear the active count.");
Assert(component.SlotsMinions == 0, "Reset should clear occupied slots.");

var concurrentComponent = new PlayerMinionCapacityComponent(owner);
var concurrentSystems = new PlayerMinionCapacityCommitSystem[8];
for (var index = 0; index < concurrentSystems.Length; index++)
{
  concurrentSystems[index] = new PlayerMinionCapacityCommitSystem(concurrentComponent);
}

var concurrentCommand = new SubmitMinionCapacityDeltaCommand(
  owner,
  owner,
  ProjectileOwnerSlot: 7,
  ProjectileIdentity: 12,
  MinionCountDelta: 1,
  SlotDelta: 0.5f,
  SourceRevision: 6,
  IdempotencyToken: Guid.NewGuid());
var concurrentResults = new PlayerMinionCapacityCommitStatus[32];
System.Threading.Tasks.Parallel.For(
  0,
  concurrentResults.Length,
  index => concurrentResults[index] =
    concurrentSystems[index % concurrentSystems.Length].Apply(concurrentCommand));
Assert(
  System.Array.FindAll(
    concurrentResults,
    status => status == PlayerMinionCapacityCommitStatus.Committed).Length == 1,
  "Concurrent duplicates should commit exactly once.");
Assert(
  System.Array.FindAll(
    concurrentResults,
    status => status == PlayerMinionCapacityCommitStatus.AlreadyApplied).Length ==
    concurrentResults.Length - 1,
  "Every duplicate after the first commit should be rejected.");
Assert(concurrentComponent.NumMinions == 1, "Concurrent duplicates must not overcount minions.");
Assert(concurrentComponent.SlotsMinions == 0.5f, "Concurrent duplicates must not oversell slots.");

var maximumCount = new SubmitMinionCapacityDeltaCommand(
  owner,
  owner,
  ProjectileOwnerSlot: 7,
  ProjectileIdentity: 10,
  MinionCountDelta: int.MaxValue,
  SlotDelta: 0,
  SourceRevision: 4,
  IdempotencyToken: Guid.NewGuid());
Assert(
  system.Apply(maximumCount) == PlayerMinionCapacityCommitStatus.Committed,
  "A count within the component range should commit.");

var countOverflow = maximumCount with
{
  ProjectileIdentity = 11,
  MinionCountDelta = 1,
  SourceRevision = 5,
  IdempotencyToken = Guid.NewGuid(),
};
Assert(
  system.Apply(countOverflow) == PlayerMinionCapacityCommitStatus.RejectedInvalidDelta,
  "A count delta that exceeds the component range should be rejected.");
Assert(component.NumMinions == int.MaxValue, "A rejected count overflow must preserve the existing count.");

Console.WriteLine(
  "P05 focused verifier passed: progression, pet, capacity, fishing, minion and damage cores.");

static void VerifyConsumedUpgradeEligibility()
{
  var ledger = new PlayerConsumedProgressionLedgerComponent();
  var supportedItems = new (int ItemType, PlayerConsumedProgressionUpgrade Upgrade)[]
  {
    (5337, PlayerConsumedProgressionUpgrade.AegisCrystal),
    (5338, PlayerConsumedProgressionUpgrade.AegisFruit),
    (5339, PlayerConsumedProgressionUpgrade.ArcaneCrystal),
    (5340, PlayerConsumedProgressionUpgrade.GalaxyPearl),
    (5341, PlayerConsumedProgressionUpgrade.GummyWorm),
    (5342, PlayerConsumedProgressionUpgrade.Ambrosia),
  };

  var firstUpgrade = supportedItems[0].Upgrade;
  Assert(
    !ConsumedUpgradeEligibilityQuery.TryResolveEligibleUpgrade(
      new PlayerConsumedUpgradeItemUseInput(5337, ItemAnimation: 0, ItemTimeIsZero: true),
      ledger,
      out _),
    "An inactive item animation must not resolve a consumed upgrade.");
  Assert(
    !ConsumedUpgradeEligibilityQuery.TryResolveEligibleUpgrade(
      new PlayerConsumedUpgradeItemUseInput(5337, ItemAnimation: 1, ItemTimeIsZero: false),
      ledger,
      out _),
    "A nonzero item time must not resolve a consumed upgrade.");
  Assert(
    !ConsumedUpgradeEligibilityQuery.TryResolveEligibleUpgrade(
      new PlayerConsumedUpgradeItemUseInput(int.MaxValue, ItemAnimation: 1, ItemTimeIsZero: true),
      ledger,
      out _),
    "An unsupported item type must not resolve a consumed upgrade.");
  Assert(
    !ConsumedUpgradeEligibilityQuery.IsConsumed(ledger, firstUpgrade),
    "Eligibility queries must not mutate the consumed-upgrade ledger.");

  foreach (var supportedItem in supportedItems)
  {
    var input = new PlayerConsumedUpgradeItemUseInput(
      supportedItem.ItemType,
      ItemAnimation: 1,
      ItemTimeIsZero: true);
    Assert(
      ConsumedUpgradeEligibilityQuery.TryResolveEligibleUpgrade(input, ledger, out var resolved) &&
        resolved == supportedItem.Upgrade,
      $"Item type {supportedItem.ItemType} should resolve to {supportedItem.Upgrade}.");

    var command = new ConsumePlayerUpgradeCommand(
      supportedItem.Upgrade,
      new PlayerProgressionCommandToken(Guid.NewGuid()));
    var result = PlayerProgressionCommitSystem.Commit(command, ref ledger);
    Assert(
      result.Status == PlayerProgressionCommitStatus.Committed,
      $"The first {supportedItem.Upgrade} commit should succeed.");
    Assert(
      !ConsumedUpgradeEligibilityQuery.TryResolveEligibleUpgrade(input, ledger, out _),
      $"A consumed {supportedItem.Upgrade} must no longer resolve as eligible.");

    var repeatedCommand = new ConsumePlayerUpgradeCommand(
      supportedItem.Upgrade,
      new PlayerProgressionCommandToken(Guid.NewGuid()));
    var repeatedResult = PlayerProgressionCommitSystem.Commit(repeatedCommand, ref ledger);
    Assert(
      repeatedResult.Status == PlayerProgressionCommitStatus.AlreadyConsumed,
      $"A repeated {supportedItem.Upgrade} commit must be rejected.");
  }

  var unsupportedUpgrade = (PlayerConsumedProgressionUpgrade)byte.MaxValue;
  Assert(
    !ConsumedUpgradeEligibilityQuery.CanConsume(ledger, unsupportedUpgrade),
    "An unsupported upgrade value must not be eligible.");
  var unsupportedResult = PlayerProgressionCommitSystem.Commit(
    new ConsumePlayerUpgradeCommand(
      unsupportedUpgrade,
      new PlayerProgressionCommandToken(Guid.NewGuid())),
    ref ledger);
  Assert(
    unsupportedResult.Status == PlayerProgressionCommitStatus.RejectedUnknownUpgrade,
    "An unsupported upgrade commit must be rejected.");
}

static void VerifyStandardNamedPetCapabilityRebuild()
{
  var capability = new PlayerStandardNamedPetCapabilityComponent();
  var input = new PlayerStandardNamedPetCapabilityRebuildInput
  {
    PetFlagUpbeatStar = true,
    PetFlagSugarGlider = true,
    PetFlagBabyShark = true,
    PetFlagLilHarpy = true,
    PetFlagFennecFox = true,
    PetFlagGlitteryButterfly = true,
    PetFlagBabyImp = true,
    PetFlagBabyRedPanda = true,
    PetFlagPlantero = true,
    PetFlagDynamiteKitten = true,
    PetFlagBabyWerewolf = true,
    PetFlagShadowMimic = true,
    PetFlagVoltBunny = true,
  };

  Assert(
    typeof(PlayerStandardNamedPetCapabilityComponent).GetProperties().Length == 13,
    "The standard named pet verifier must cover all thirteen capability flags.");
  PlayerStandardNamedPetCapabilityRebuildSystem.Rebuild(input, capability);
  AssertAllCapabilityFlags(capability, expected: true);
  PlayerStandardNamedPetCapabilityRebuildSystem.Reset(capability);
  AssertAllCapabilityFlags(capability, expected: false);
}

static void VerifyBossPetCapabilityRebuild()
{
  var capability = new PlayerBossPetCapabilityComponent();
  var input = new PlayerBossPetCapabilityRebuildInput
  {
    PetFlagKingSlimePet = true,
    PetFlagEyeOfCthulhuPet = true,
    PetFlagEaterOfWorldsPet = true,
    PetFlagBrainOfCthulhuPet = true,
    PetFlagSkeletronPet = true,
    PetFlagQueenBeePet = true,
    PetFlagDestroyerPet = true,
    PetFlagTwinsPet = true,
    PetFlagSkeletronPrimePet = true,
    PetFlagPlanteraPet = true,
    PetFlagGolemPet = true,
    PetFlagDukeFishronPet = true,
    PetFlagLunaticCultistPet = true,
    PetFlagMoonLordPet = true,
    PetFlagFairyQueenPet = true,
    PetFlagQueenSlimePet = true,
  };

  Assert(
    typeof(PlayerBossPetCapabilityComponent).GetProperties().Length == 16,
    "The boss pet verifier must cover all sixteen capability flags.");
  PlayerBossPetCapabilityRebuildSystem.Rebuild(input, capability);
  AssertAllCapabilityFlags(capability, expected: true);
  PlayerBossPetCapabilityRebuildSystem.Reset(capability);
  AssertAllCapabilityFlags(capability, expected: false);
}

static void VerifyCrossoverPetCapabilityRebuild()
{
  var capability = new PlayerCrossoverPetCapabilityComponent();
  var input = new PlayerCrossoverPetCapabilityRebuildInput
  {
    PetFlagBerniePet = true,
    PetFlagGlommerPet = true,
    PetFlagDeerclopsPet = true,
    PetFlagPigPet = true,
    PetFlagChesterPet = true,
    PetFlagJunimoPet = true,
    PetFlagBlueChickenPet = true,
    PetFlagSpiffo = true,
    PetFlagCaveling = true,
    PetFlagDeadCellsSwarmBiter = true,
    PetFlagPufferfish = true,
    PetFlagChillet = true,
    PetFlagChilletIgnis = true,
  };

  Assert(
    typeof(PlayerCrossoverPetCapabilityComponent).GetProperties().Length == 13,
    "The crossover pet verifier must cover all thirteen capability flags.");
  PlayerCrossoverPetCapabilityRebuildSystem.Rebuild(input, capability);
  AssertAllCapabilityFlags(capability, expected: true);
  PlayerCrossoverPetCapabilityRebuildSystem.Reset(capability);
  AssertAllCapabilityFlags(capability, expected: false);
}

static void VerifyWorldObjectPetCapabilityRebuild()
{
  var capability = new PlayerWorldObjectPetCapabilityComponent();
  var input = new PlayerWorldObjectPetCapabilityRebuildInput
  {
    PetFlagDirtiestBlock = true,
    PetFlagBoulderPet = true,
    PetFlagRainbowBoulderPet = true,
    PetFlagAxeFairyPet = true,
  };

  Assert(
    typeof(PlayerWorldObjectPetCapabilityComponent).GetProperties().Length == 4,
    "The world-object pet verifier must cover all four capability flags.");
  PlayerWorldObjectPetCapabilityRebuildSystem.Rebuild(input, capability);
  AssertAllCapabilityFlags(capability, expected: true);
  PlayerWorldObjectPetCapabilityRebuildSystem.Reset(capability);
  AssertAllCapabilityFlags(capability, expected: false);
}

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

static void AssertAllCapabilityFlags<TComponent>(TComponent component, bool expected)
  where TComponent : class
{
  foreach (var property in typeof(TComponent).GetProperties())
  {
    Assert(
      property.PropertyType == typeof(bool) &&
        property.GetValue(component) is bool value &&
        value == expected,
      $"Capability flag {property.Name} should be {expected}.");
  }
}

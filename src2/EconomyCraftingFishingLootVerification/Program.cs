using System.Collections.Immutable;
using NLTX.EconomyCraftingFishingLoot.Commerce;
using NLTX.EconomyCraftingFishingLoot.Content;
using NLTX.EconomyCraftingFishingLoot.Crafting;
using NLTX.EconomyCraftingFishingLoot.Fishing;
using NLTX.EconomyCraftingFishingLoot.Loot;

namespace NLTX.EconomyCraftingFishingLootVerification;

internal static class Program
{
  private static int Main()
  {
    TestRecipeGroupCatalogCopiesInputAndResolvesFakeItemId();
    TestRecipeGroupCatalogRejectsDuplicateKeys();
    TestRecipeDefinitionSnapshotsInputs();
    TestRecipeQualificationAcceptsItemAndRecipeGroupMaterials();
    TestRecipeQualificationReportsMissingMaterialBeforeOtherFailures();
    TestRecipeQualificationRejectsMissingTileAndCondition();
    TestRecipeQualificationIsPure();
    TestItemEconomyAndUseDefinitionsPreserveSourceValues();
    TestItemUseTimingConsumptionDefinitionSnapshotsCapabilities();
    TestFishingConditionAndRarityCatalogs();
    TestFishingCatalogsRejectDuplicateNamesAndSnapshotInputs();
    TestFishingQuestConditionDefinitionPreservesCheckedType();
    TestFishingEnvironmentPredicates();
    TestFishingDropRuleDefinitionsAndCatalog();
    TestFishingAttemptResolution();
    TestDropResolutionContextAndCatalog();
    TestDropRateProjectionAndAttemptResults();
    TestDropConditionsAndBranches();
    TestDropResolutionUsesExplicitRandomAndRerolls();
    TestDropChainsAndOptionSelectors();
    TestCommerceAndShopState();
    TestCraftingRequestBoundary();
    TestLootSimulationAndAttribution();
    TestP10SourceInventoryAndImplementationBoundary();
    Console.WriteLine("P10 verifier passed.");
    return 0;
  }

  private static void TestRecipeGroupCatalogCopiesInputAndResolvesFakeItemId()
  {
    List<int> itemTypeIds = [10, 11];
    RecipeGroupDefinition definition = new(
      new RecipeGroupId(7),
      fakeItemId: 9007,
      defaultCombineFormat: "Any {0}",
      itemTypeIds,
      decraftItemTypeId: 10,
      registeredId: 7);
    RecipeGroupCatalog catalog = new([definition]);

    itemTypeIds[0] = 99;

    Assert(catalog.TryGetByFakeItemId(9007, out RecipeGroupDefinition resolved),
      "The fake item ID should resolve to its recipe group.");
    Assert(resolved.Id == new RecipeGroupId(7),
      "The resolved recipe group ID should remain stable.");
    Assert(resolved.ItemTypeIds.SequenceEqual([10, 11]),
      "The catalog must snapshot the input item IDs.");
    Assert(catalog.Count == 1, "The catalog should expose one registered group.");
  }

  private static void TestRecipeGroupCatalogRejectsDuplicateKeys()
  {
    RecipeGroupDefinition first = new(
      new RecipeGroupId(1),
      fakeItemId: 9001,
      defaultCombineFormat: "Any {0}",
      itemTypeIds: [1],
      decraftItemTypeId: 1,
      registeredId: 1);
    RecipeGroupDefinition duplicateFakeId = new(
      new RecipeGroupId(2),
      fakeItemId: 9001,
      defaultCombineFormat: "Any {0}",
      itemTypeIds: [2],
      decraftItemTypeId: 2,
      registeredId: 2);

    AssertThrows<ArgumentException>(
      () => new RecipeGroupCatalog([first, duplicateFakeId]),
      "Duplicate fake item IDs must be rejected.");
  }

  private static void TestRecipeDefinitionSnapshotsInputs()
  {
    List<RecipeIngredientDefinition> ingredients =
    [
      new RecipeIngredientDefinition(itemTypeId: 10, stack: 2),
      new RecipeIngredientDefinition(new RecipeGroupId(7), stack: 1)
    ];
    List<RecipeResultDefinition> shimmerResults =
    [new RecipeResultDefinition(itemTypeId: 101, stack: 1)];
    List<RecipeConditionDefinition> conditions =
    [new RecipeConditionDefinition(RecipeConditionKind.NeedHoney)];
    RecipeDefinition recipe = new(
      new RecipeId(42),
      new RecipeResultDefinition(itemTypeId: 100, stack: 1),
      ingredients,
      requiredTileId: 18,
      shimmerResults,
      conditions,
      notDecraftable: true);

    ingredients.Clear();
    shimmerResults.Clear();
    conditions.Clear();

    Assert(recipe.Ingredients.Length == 2,
      "Recipe ingredients must be snapshotted at construction.");
    Assert(recipe.CustomShimmerResults.Length == 1,
      "Recipe shimmer results must be snapshotted at construction.");
    Assert(recipe.Conditions.Length == 1,
      "Recipe conditions must be snapshotted at construction.");
    Assert(recipe.RequiredTileId == 18,
      "Recipe required tile must remain part of the definition.");
    Assert(recipe.NotDecraftable,
      "Recipe decrafting policy must remain part of the definition.");
  }

  private static void TestRecipeQualificationAcceptsItemAndRecipeGroupMaterials()
  {
    RecipeGroupDefinition group = new(
      new RecipeGroupId(7),
      fakeItemId: 9007,
      defaultCombineFormat: "Any {0}",
      itemTypeIds: [11, 12],
      decraftItemTypeId: 11,
      registeredId: 7);
    RecipeGroupCatalog catalog = new([group]);
    RecipeDefinition recipe = new(
      new RecipeId(43),
      new RecipeResultDefinition(itemTypeId: 100, stack: 1),
      [
        new RecipeIngredientDefinition(itemTypeId: 10, stack: 2),
        new RecipeIngredientDefinition(new RecipeGroupId(7), stack: 1)
      ],
      requiredTileId: 18,
      customShimmerResults: [],
      conditions: [new RecipeConditionDefinition(RecipeConditionKind.NeedHoney)]);
    RecipeQualificationContext context = new(
      itemCounts: new Dictionary<int, int>
      {
        [10] = 2,
        [12] = 1
      },
      availableTileIds: [18],
      conditionValues: new Dictionary<RecipeConditionKind, bool>
      {
        [RecipeConditionKind.NeedHoney] = true
      });

    RecipeQualificationResult result =
      RecipeQualificationQuery.Evaluate(recipe, context, catalog);

    Assert(result.IsQualified,
      "A recipe should qualify when item and recipe-group materials are present.");
    Assert(result.FailureReason == RecipeQualificationFailureReason.None,
      "A qualified recipe should have no failure reason.");
  }

  private static void TestRecipeQualificationReportsMissingMaterialBeforeOtherFailures()
  {
    RecipeDefinition recipe = new(
      new RecipeId(44),
      new RecipeResultDefinition(itemTypeId: 100, stack: 1),
      [new RecipeIngredientDefinition(itemTypeId: 10, stack: 3)],
      requiredTileId: 18,
      customShimmerResults: [],
      conditions: [new RecipeConditionDefinition(RecipeConditionKind.NeedHoney)]);
    RecipeQualificationContext context = new(
      itemCounts: new Dictionary<int, int> { [10] = 1 },
      availableTileIds: [],
      conditionValues: new Dictionary<RecipeConditionKind, bool>());

    RecipeQualificationResult result =
      RecipeQualificationQuery.Evaluate(recipe, context, new RecipeGroupCatalog([]));

    Assert(!result.IsQualified, "A recipe with insufficient materials must be rejected.");
    Assert(result.FailureReason == RecipeQualificationFailureReason.MissingIngredient,
      "Missing material must be reported before tile or condition failures.");
    Assert(result.MissingIngredientIndex == 0,
      "The first missing ingredient index should be reported.");
  }

  private static void TestRecipeQualificationRejectsMissingTileAndCondition()
  {
    RecipeDefinition recipe = new(
      new RecipeId(45),
      new RecipeResultDefinition(itemTypeId: 100, stack: 1),
      [new RecipeIngredientDefinition(itemTypeId: 10, stack: 1)],
      requiredTileId: 18,
      customShimmerResults: [],
      conditions: [new RecipeConditionDefinition(RecipeConditionKind.NeedHoney)]);
    RecipeQualificationContext missingTileContext = new(
      itemCounts: new Dictionary<int, int> { [10] = 1 },
      availableTileIds: [],
      conditionValues: new Dictionary<RecipeConditionKind, bool>());
    RecipeQualificationResult missingTile =
      RecipeQualificationQuery.Evaluate(
        recipe,
        missingTileContext,
        new RecipeGroupCatalog([]));

    Assert(missingTile.FailureReason == RecipeQualificationFailureReason.MissingRequiredTile,
      "A missing required tile must reject the recipe after materials qualify.");

    RecipeQualificationContext missingConditionContext = new(
      itemCounts: new Dictionary<int, int> { [10] = 1 },
      availableTileIds: [18],
      conditionValues: new Dictionary<RecipeConditionKind, bool>
      {
        [RecipeConditionKind.NeedHoney] = false
      });
    RecipeQualificationResult missingCondition =
      RecipeQualificationQuery.Evaluate(
        recipe,
        missingConditionContext,
        new RecipeGroupCatalog([]));

    Assert(missingCondition.FailureReason == RecipeQualificationFailureReason.MissingCondition,
      "A missing recipe condition must reject the recipe.");
    Assert(missingCondition.MissingConditionKind == RecipeConditionKind.NeedHoney,
      "The missing recipe condition kind should be reported.");
  }

  private static void TestRecipeQualificationIsPure()
  {
    RecipeDefinition recipe = new(
      new RecipeId(46),
      new RecipeResultDefinition(itemTypeId: 100, stack: 1),
      [new RecipeIngredientDefinition(itemTypeId: 10, stack: 1)],
      requiredTileId: null,
      customShimmerResults: [],
      conditions: []);
    RecipeQualificationContext context = new(
      itemCounts: new Dictionary<int, int> { [10] = 1 },
      availableTileIds: [],
      conditionValues: new Dictionary<RecipeConditionKind, bool>());
    RecipeGroupCatalog catalog = new([]);

    RecipeQualificationResult first =
      RecipeQualificationQuery.Evaluate(recipe, context, catalog);
    RecipeQualificationResult second =
      RecipeQualificationQuery.Evaluate(recipe, context, catalog);

    Assert(first == second,
      "Recipe qualification should be deterministic for the same immutable inputs.");
    Assert(context.ItemCounts[10] == 1,
      "Recipe qualification must not mutate the supplied item snapshot.");
  }

  private static void TestItemEconomyAndUseDefinitionsPreserveSourceValues()
  {
    ItemCurrencyEconomyDefinition currency = new(
      copper: 1,
      silver: 100,
      gold: 10000,
      platinum: 1000000,
      goldCritterRarityColor: 3);
    ItemEventPricingDefinition eventPricing = new(
      shadowOrbPrice: 750,
      dungeonPrice: 875,
      queenBeePrice: 100000,
      hellPrice: 1250,
      eclipsePrice: 3750,
      eclipsePostPlanteraPrice: 500000,
      eclipseMothronPrice: 625000);
    ItemPickupTimingDefinition pickup = new(
      coinGrabRange: 350,
      manaGrabRange: 300,
      lifeGrabRange: 250,
      treasureGrabRange: 150);
    ItemBuffDurationDefinition buffs = new(
      luckPotionDuration1: 18000,
      luckPotionDuration2: 36000,
      luckPotionDuration3: 54000,
      flaskTime: 72000);
    ItemStackAndUseDelayDefinition stackAndDelay = new(
      commonMaxStack: 9999,
      potionDelay: 3600,
      restorationDelay: 2700,
      eggnogDelay: 2400,
      mushroomDelay: 1800);
    ItemPlacementAndPickupPolicyDefinition placement = new(
      foodWidth: 22,
      foodHeight: 22,
      wallPlacementUseTime: 7,
      pickupReplacementTime: 1200,
      slotsRemainingBeforeEmergencyStacking: 40);

    Assert(currency.Copper == 1 && currency.Silver == 100 &&
      currency.Gold == 10000 && currency.Platinum == 1000000,
      "Currency unit values must preserve the source constants.");
    Assert(currency.GoldCritterRarityColor == 3,
      "Critter rarity color must preserve the source constant.");
    Assert(eventPricing.ShadowOrbPrice == 750 && eventPricing.DungeonPrice == 875 &&
      eventPricing.QueenBeePrice == 100000 && eventPricing.HellPrice == 1250 &&
      eventPricing.EclipsePrice == 3750 &&
      eventPricing.EclipsePostPlanteraPrice == 500000 &&
      eventPricing.EclipseMothronPrice == 625000,
      "Event pricing values must preserve the source sell-price calculations.");
    Assert(pickup.CoinGrabRange == 350 && pickup.ManaGrabRange == 300 &&
      pickup.LifeGrabRange == 250 && pickup.TreasureGrabRange == 150,
      "Pickup ranges must preserve the source values.");
    Assert(buffs.LuckPotionDuration1 == 18000 &&
      buffs.LuckPotionDuration2 == 36000 && buffs.LuckPotionDuration3 == 54000 &&
      buffs.FlaskTime == 72000,
      "Buff durations must preserve the source values.");
    Assert(stackAndDelay.CommonMaxStack == 9999 &&
      stackAndDelay.PotionDelay == 3600 && stackAndDelay.RestorationDelay == 2700 &&
      stackAndDelay.EggnogDelay == 2400 && stackAndDelay.MushroomDelay == 1800,
      "Stack and use delays must preserve the source values.");
    Assert(placement.FoodWidth == 22 && placement.FoodHeight == 22 &&
      placement.WallPlacementUseTime == 7 && placement.PickupReplacementTime == 1200 &&
      placement.SlotsRemainingBeforeEmergencyStacking == 40,
      "Placement and pickup policy values must preserve the source values.");
  }

  private static void TestItemUseTimingConsumptionDefinitionSnapshotsCapabilities()
  {
    ItemUseTimingConsumptionDefinition definition = new(
      holdStyle: 1,
      useStyle: 2,
      channel: true,
      accessory: true,
      useAnimation: 15,
      useTime: 10,
      potion: true,
      consumable: true,
      autoReuse: true,
      useTurn: false,
      noUseGraphic: false,
      noMelee: true,
      noWet: true,
      shootsEveryUse: true,
      reuseDelay: 4);

    Assert(definition.HoldStyle == 1 && definition.UseStyle == 2 &&
      definition.Channel && definition.Accessory && definition.UseAnimation == 15 &&
      definition.UseTime == 10 && definition.Potion && definition.Consumable &&
      definition.AutoReuse && !definition.UseTurn && !definition.NoUseGraphic &&
      definition.NoMelee && definition.NoWet && definition.ShootsEveryUse &&
      definition.ReuseDelay == 4,
      "Item use timing and consumption capabilities must be preserved.");
  }

  private static void TestFishingConditionAndRarityCatalogs()
  {
    FishingConditionEvaluationContext context = new(
      random: new FixedFishingRandomSource(),
      common: false,
      uncommon: true,
      rare: false,
      veryRare: false,
      legendary: false,
      junk: true,
      crate: false,
      rolledEnemySpawn: 1,
      questFishType: 42,
      isHardMode: true,
      combatBookWasUsed: false);

    FishingConditionCatalog conditions = FishingConditionCatalog.CreateDefault();
    Assert(conditions.TryGet("HardMode", out FishingConditionDelegateDefinition hardMode) &&
      hardMode.Matches(context),
      "The hard-mode fishing condition should match an explicit hard-mode context.");
    Assert(conditions.TryGet("Junk", out FishingConditionDelegateDefinition junk) &&
      junk.Matches(context),
      "The junk fishing condition should read the attempt snapshot.");
    Assert(conditions.TryGet("AnyEnemies", out FishingConditionDelegateDefinition enemies) &&
      enemies.Matches(context),
      "The enemy condition should match a positive rolled enemy count.");
    Assert(conditions.TryGet(
        "DidNotUseCombatBook",
        out FishingConditionDelegateDefinition combatBook) && combatBook.Matches(context),
      "The combat-book condition should match when the book was not used.");

    FishingRarityCatalog rarities = FishingRarityCatalog.CreateDefault();
    Assert(rarities.TryGet("Uncommon", out FishingRarityConditionDefinition uncommon) &&
      uncommon.Matches(context) && uncommon.FrequencyOfAppearanceForVisuals == 0.8f,
      "The uncommon rarity should preserve its predicate and display frequency.");
    Assert(rarities.TryGet(
        "BombRarityOfNotLegendaryAndNotVeryRareAndUncommon",
        out FishingRarityConditionDefinition bombRarity) && bombRarity.Matches(context),
      "The compound bomb rarity should preserve its source predicate.");
    Assert(rarities.TryGet("Any", out FishingRarityConditionDefinition any) &&
      any.HackedIsAny && any.Matches(context),
      "The any rarity should preserve the source marker and always match.");
  }

  private static void TestFishingCatalogsRejectDuplicateNamesAndSnapshotInputs()
  {
    List<FishingConditionDelegateDefinition> conditionDefinitions =
    [
      new FishingConditionDelegateDefinition(
        "Custom",
        context => context.Junk,
        new FishingConditionDisplayMetadata(canBeSkippedForDisplay: true))
    ];
    FishingConditionCatalog conditions = new(conditionDefinitions);
    conditionDefinitions.Clear();

    Assert(conditions.Count == 1 &&
      conditions.TryGet("Custom", out FishingConditionDelegateDefinition custom) &&
      custom.DisplayMetadata.CanBeSkippedForDisplay,
      "The fishing condition catalog must snapshot definitions and display metadata.");

    AssertThrows<ArgumentException>(
      () => new FishingConditionCatalog(
      [
        new FishingConditionDelegateDefinition(
          "Duplicate",
          context => true,
          new FishingConditionDisplayMetadata(canBeSkippedForDisplay: false)),
        new FishingConditionDelegateDefinition(
          "Duplicate",
          context => false,
          new FishingConditionDisplayMetadata(canBeSkippedForDisplay: false))
      ]),
      "Duplicate fishing condition names must be rejected.");

    AssertThrows<ArgumentException>(
      () => new FishingRarityCatalog(
      [
        new FishingRarityConditionDefinition(
          "Duplicate",
          new FishingRarityPredicate(context => true),
          frequencyOfAppearanceForVisuals: 1f,
          hackedIsAny: false),
        new FishingRarityConditionDefinition(
          "Duplicate",
          new FishingRarityPredicate(context => false),
          frequencyOfAppearanceForVisuals: 1f,
          hackedIsAny: false)
      ]),
      "Duplicate fishing rarity names must be rejected.");
  }

  private static void TestFishingQuestConditionDefinitionPreservesCheckedType()
  {
    FishingQuestConditionDefinition normal =
      new(checkedType: 42, isRemixVariant: false);
    FishingQuestConditionDefinition remix =
      new(checkedType: 43, isRemixVariant: true);

    Assert(normal.CheckedType == 42 && !normal.IsRemixVariant,
      "The normal quest-fish definition must preserve its checked type.");
    Assert(remix.CheckedType == 43 && remix.IsRemixVariant,
      "The remix quest-fish definition must preserve its checked type and variant.");

    FishingConditionEvaluationContext context = new(
      random: new FixedFishingRandomSource(),
      questFishType: 42);
    Assert(normal.Matches(context) && !remix.Matches(context),
      "Quest-fish matching must use the explicit context quest-fish type.");
  }

  private static void TestFishingEnvironmentPredicates()
  {
    FishingConditionEvaluationContext context = new(
      random: new FixedFishingRandomSource(),
      inLava: true,
      inHoney: true,
      canFishInLava: true,
      waterTilesCount: 1001,
      fishingY: 100,
      heightLevel: 2,
      rolledCorruption: true,
      rolledCrimson: true,
      rolledJungle: true,
      rolledSnow: true,
      rolledDesert: true,
      rolledInfectedDesert: true,
      rolledRemixOcean: true,
      remixWorld: true,
      bloodMoon: true,
      zoneDungeon: true,
      zoneBeach: true,
      zoneHallow: true,
      zoneGlowshroom: true,
      zoneDesert: true,
      zoneSnow: true,
      downedBoss3: true,
      rockLayerY: 90,
      originalOcean: true);

    Assert(FishingFluidPredicate.InLava(context) &&
      FishingFluidPredicate.InHoney(context) &&
      FishingFluidPredicate.CanFishInLava(context),
      "Fluid predicates should read the explicit liquid snapshot.");
    Assert(FishingBiomePredicate.Dungeon(context) &&
      FishingBiomePredicate.Beach(context) &&
      FishingBiomePredicate.Hallow(context) &&
      FishingBiomePredicate.GlowingMushrooms(context) &&
      FishingBiomePredicate.TrueDesert(context) &&
      FishingBiomePredicate.TrueSnow(context) &&
      FishingBiomePredicate.Corruption(context) &&
      FishingBiomePredicate.Crimson(context) &&
      FishingBiomePredicate.Jungle(context) &&
      FishingBiomePredicate.Snow(context) &&
      FishingBiomePredicate.Desert(context) &&
      FishingBiomePredicate.RolledHallowDesert(context),
      "Biome predicates should preserve the player-zone and rolled-biome distinctions.");
    Assert(FishingDepthPredicate.Height1(context) == false &&
      FishingDepthPredicate.Height1And2(context) &&
      FishingDepthPredicate.HeightAbove1(context) &&
      FishingDepthPredicate.HeightAboveAnd1(context) &&
      !FishingDepthPredicate.HeightUnder2(context) &&
      !FishingDepthPredicate.HeightAbove2(context) &&
      !FishingDepthPredicate.Height0(context) &&
      FishingDepthPredicate.Height2(context) &&
      !FishingDepthPredicate.Height3(context),
      "Depth predicates should use the explicit height level.");
    Assert(FishingWorldPredicate.UnderRockLayer(context) &&
      FishingWorldPredicate.OriginalOcean(context) &&
      FishingWorldPredicate.RemixOcean(context) &&
      FishingWorldPredicate.Ocean(context) &&
      FishingWorldPredicate.Water1000(context) &&
      FishingWorldPredicate.Remix(context),
      "World predicates should use explicit rock-layer, ocean, water, and seed inputs.");
    Assert(FishingEventPredicate.BloodMoon(context),
      "The blood-moon predicate should read the event snapshot.");

    FishingConditionEvaluationContext incompleteContext = new(
      random: new FixedFishingRandomSource(),
      fishingY: 100,
      waterTilesCount: 1000);
    Assert(!FishingWorldPredicate.UnderRockLayer(incompleteContext) &&
      !FishingWorldPredicate.OriginalOcean(incompleteContext) &&
      !FishingWorldPredicate.Water1000(incompleteContext),
      "World predicates must not invent missing external snapshot values.");
  }

  private static void TestFishingDropRuleDefinitionsAndCatalog()
  {
    FishingConditionEvaluationContext context = new(
      random: new FixedFishingRandomSource(),
      uncommon: true,
      questFishType: 42);
    FishingConditionDelegateDefinition condition =
      new("Uncommon", value => value.Uncommon,
        new FishingConditionDisplayMetadata(canBeSkippedForDisplay: false));
    FishingRarityConditionDefinition rarity =
      new("Uncommon", new FishingRarityPredicate(value => value.Uncommon),
        frequencyOfAppearanceForVisuals: 0.8f,
        hackedIsAny: false);
    FishingQuestConditionDefinition questCondition =
      new(checkedType: 42, isRemixVariant: false);
    FishingDropRuleDefinition rule = new(
      possibleItems: [100, 101],
      chanceNumerator: 1,
      chanceDenominator: 20,
      conditions: [condition, questCondition],
      rarity);

    Assert(rule.PossibleItems.SequenceEqual([100, 101]),
      "Fish drop rule candidates must be snapshotted in source order.");
    Assert(rule.ChanceNumerator == 1 && rule.ChanceDenominator == 20,
      "Fish drop rule chance values must preserve the source fields.");
    Assert(rule.Conditions.Length == 2 &&
      rule.Conditions.All(value => value.Matches(context)),
      "Fish drop rule conditions must be evaluated through the common read-only port.");
    Assert(rule.Rarity.Matches(context),
      "Fish drop rule rarity must remain attached to the definition.");

    List<FishingDropRuleDefinition> rules = [rule];
    FishingDropRuleCatalog catalog = new(rules);
    rules.Clear();
    Assert(catalog.Count == 1 && catalog.Rules[0] == rule,
      "The fish drop rule catalog must snapshot the ordered rule collection.");

    FishingPossibilityEntry possibility = new(itemType: 200, frequency: 0.25f);
    Assert(possibility.ItemType == 200 && possibility.Frequency == 0.25f,
      "Fish possibility entries must preserve item and frequency values.");
  }

  private static void TestFishingAttemptResolution()
  {
    PlayerFishingInputSnapshot input = new(
      polePower: 45,
      poleItemType: 2298,
      baitPower: 35,
      baitItemType: 267);
    PlayerFishingLevelQueryResult level = PlayerFishingLevelQuery.Evaluate(
      input,
      levelMultipliers: 1.25f,
      finalFishingLevel: 73);
    Assert(level.Input == input && level.LevelMultipliers == 1.25f &&
      level.FinalFishingLevel == 73,
      "Fishing level results must preserve explicit derived values without inventing a formula.");

    FishingRollClassification roll = new(
      common: true,
      uncommon: true,
      rare: false,
      veryRare: false,
      legendary: false,
      crate: false,
      junk: false);
    FishingEnvironmentSnapshot environment = new(
      inLava: false,
      inHoney: false,
      waterTilesCount: 1200,
      waterNeededToFish: 300,
      waterQuality: 0.75f,
      chumsInWater: 2);
    FishingPowerSnapshot power = new(
      fishingLevel: level.FinalFishingLevel,
      canFishInLava: true);
    FishingWorldPredicateSnapshot world = new(
      atmosphericValue: 0.4f,
      fishingY: 120,
      heightLevel: 2,
      rockLayerY: 100,
      isOriginalOcean: true);
    FishingAttemptConditionSnapshot conditions = new(
      roll,
      environment,
      power,
      world,
      questFishType: 2450,
      rolledEnemySpawn: 0,
      rolledCorruption: true,
      rolledCrimson: false,
      rolledJungle: false,
      rolledSnow: false,
      rolledDesert: false,
      rolledInfectedDesert: false,
      rolledRemixOcean: false,
      isHardMode: true,
      remixWorld: false,
      bloodMoon: false,
      combatBookWasUsed: false,
      zoneDungeon: false,
      zoneBeach: true,
      zoneHallow: false,
      zoneGlowshroom: false,
      zoneDesert: false,
      zoneSnow: false,
      downedBoss3: true);
    FishingAttemptLocationContext location = new(
      x: 18,
      y: 120,
      bobberType: 3);

    FishingConditionEvaluationContext evaluation =
      conditions.CreateEvaluationContext(new FixedFishingRandomSource());
    Assert(location.X == 18 && location.Y == 120 && location.BobberType == 3 &&
      evaluation.Common && evaluation.Uncommon && evaluation.WaterTilesCount == 1200 &&
      evaluation.FishingLevel == 73 && evaluation.RolledCorruption &&
      evaluation.ZoneBeach && evaluation.RockLayerY == 100 &&
      evaluation.IsOriginalOcean == true,
      "Fishing attempt snapshots must retain scalar location, environment, power, and world inputs.");

    FishingRarityConditionDefinition anyRarity = new(
      "any",
      new FishingRarityPredicate(_ => true),
      frequencyOfAppearanceForVisuals: 1f,
      hackedIsAny: true);
    FishingDropRuleDefinition rule = new(
      possibleItems: [42, 43],
      chanceNumerator: 1,
      chanceDenominator: 1,
      conditions: [],
      rarity: anyRarity);
    SequenceFishingRandomSource random = new(0, 1);
    FishingResultDecision decision = FishingAttemptResolutionQuery.Resolve(
      new FishingDropRuleCatalog([rule]),
      conditions,
      random);
    Assert(decision.HasItemDrop && decision.RolledItemDrop == 43 &&
      decision.RolledEnemySpawn == 0 && decision.RulesEvaluated == 1 &&
      decision.RandomRollCount == 2 && random.CallCount == 2,
      "Fishing resolution must use the explicit RNG for chance and item selection after conditions match.");

    FishingConditionDelegateDefinition blockedCondition =
      new("blocked", _ => false, new FishingConditionDisplayMetadata(false));
    FishingDropRuleDefinition blockedRule = new(
      possibleItems: [99],
      chanceNumerator: 1,
      chanceDenominator: 1,
      conditions: [blockedCondition],
      rarity: anyRarity);
    SequenceFishingRandomSource blockedRandom = new(0);
    FishingResultDecision blocked = FishingAttemptResolutionQuery.Resolve(
      new FishingDropRuleCatalog([blockedRule]),
      conditions,
      blockedRandom);
    Assert(!blocked.HasItemDrop && !blocked.HasEnemySpawn &&
      blocked.RulesEvaluated == 1 && blockedRandom.CallCount == 0,
      "Fishing conditions that fail must not consume random values or create a result.");

    FishingResultDecision uncommitted = FishingResultDecision.ItemDrop(
      itemTypeId: 100,
      enemyTypeId: 0,
      rulesEvaluated: 1,
      randomRollCount: 1);
    Assert(uncommitted.IsDecisionOnly && uncommitted.RolledItemDrop == 100,
      "Fishing result decisions must remain pre-commit values without bait, inventory, or world effects.");
  }

  private static void TestDropResolutionContextAndCatalog()
  {
    List<float> aiValues = [1.5f, 2.5f];
    DropResolutionContext context = new(
      random: new SequenceDropRandomSource(0),
      npcNetId: 17,
      npcTypeId: 42,
      npcName: "Test NPC",
      npcAiValues: aiValues,
      wave: 8,
      isExpertMode: true);
    aiValues[0] = 99f;

    Assert(context.TryGetNpcAiValue(0, out float firstAiValue) &&
      firstAiValue == 1.5f,
      "Drop resolution context must snapshot scalar NPC AI values.");
    Assert(context.NpcNetId == 17 && context.NpcTypeId == 42 &&
      context.IsExpertMode && !context.IsMasterMode,
      "Drop resolution context must preserve identity and mode inputs.");

    DropRuleReference globalRule = new("global-rule");
    DropRuleReference npcRule = new("npc-rule");
    DropRuleCatalog catalog = new(
      globalEntries: [globalRule],
      entriesByNpcNetId: new Dictionary<int, IReadOnlyList<DropRuleReference>>
      {
        [17] = [npcRule]
      },
      npcNetIdsByType: new Dictionary<int, IReadOnlyList<int>>
      {
        [42] = [17]
      },
      masterModeDropRng: 4);

    DropRuleResolver resolver = new(catalog);
    ImmutableArray<DropRuleReference> resolved = resolver.Resolve(context);
    Assert(resolved.Select(rule => rule.RuleId).SequenceEqual(["global-rule", "npc-rule"]),
      "Drop rule resolver must preserve global-then-NPC registration order.");
    Assert(catalog.MasterModeDropRng == 4 && catalog.Count == 2,
      "Drop rule catalog must preserve the master-mode drop RNG configuration and count.");
  }

  private static void TestDropRateProjectionAndAttemptResults()
  {
    List<DropConditionDefinition> conditions =
    [
      DropConditionDefinition.NamedNpc("Test NPC")
    ];
    DropRateProjection projection = new(
      itemId: 100,
      stackMinimum: 1,
      stackMaximum: 2,
      dropRate: 0.25f,
      conditions);
    conditions.Clear();

    Assert(projection.ItemId == 100 && projection.StackMinimum == 1 &&
      projection.StackMaximum == 2 && projection.DropRate == 0.25f &&
      projection.Conditions.Length == 1,
      "Drop-rate projections must snapshot item, quantity, rate, and conditions.");

    DropRateChainFeed feed = new(parentDropRateChance: 0.2f);
    DropRateChainFeed derivedFeed = feed
      .With(0.5f)
      .AddCondition(DropConditionDefinition.FromWave(5));
    Assert(feed.ParentDropRateChance == 0.2f && feed.Conditions.Length == 0,
      "Deriving a drop-rate chain feed must not mutate the parent feed.");
    Assert(derivedFeed.ParentDropRateChance == 0.1f && derivedFeed.Conditions.Length == 1,
      "Drop-rate chain feeds must preserve multiplier and condition composition.");

    DropAttemptResult success = DropAttemptResult.Success(100, 1, 2, rollCount: 1);
    Assert(success.State == DropAttemptResultState.Success && success.ItemId == 100 &&
      success.StackMinimum == 1 && success.StackMaximum == 2 && success.RollCount == 1,
      "Successful drop results must carry the pre-commit item decision.");
    Assert(DropAttemptResult.DoesntFillConditions().State ==
      DropAttemptResultState.DoesntFillConditions &&
      DropAttemptResult.FailedRandomRoll(2).RollCount == 2 &&
      DropAttemptResult.DidNotRunCode().State == DropAttemptResultState.DidNotRunCode,
      "Drop attempt results must expose all Version4 result states.");
  }

  private static void TestDropConditionsAndBranches()
  {
    DropResolutionContext context = new(
      random: new SequenceDropRandomSource(0),
      npcNetId: 17,
      npcTypeId: 42,
      npcName: "Test NPC",
      npcAiValues: [1.5f],
      wave: 8);

    Assert(DropConditionDefinition.NamedNpc("Test NPC").Matches(context),
      "Named NPC drop conditions must match the explicit NPC name snapshot.");
    Assert(DropConditionDefinition.SpecificAiValue(0, 1.5f).Matches(context),
      "AI-value drop conditions must read the explicit AI snapshot.");
    Assert(DropConditionDefinition.FromWave(8).Matches(context) &&
      !DropConditionDefinition.FromWave(9).Matches(context),
      "Wave drop conditions must use an inclusive threshold.");

    DropConditionBranchRuntime branch = new(
      DropConditionBranchKind.ItemDropWithCondition,
      DropConditionDefinition.NamedNpc("Test NPC"));
    Assert(branch.IsEvaluable && branch.Matches(context),
      "Evaluable drop condition branches must delegate to their immutable condition.");

    DropConditionBranchRuntime dummyBranch =
      DropConditionBranchRuntime.MechanicalBossesDummy();
    Assert(dummyBranch.RequiresIntegrationOwnedCondition && !dummyBranch.IsEvaluable,
      "Incomplete mechanical-boss condition behavior must remain an explicit integration seam.");
  }

  private static void TestDropResolutionUsesExplicitRandomAndRerolls()
  {
    SequenceDropRandomSource random = new(1, 2, 0);
    DropResolutionContext context = new(random);
    CommonDropChanceQuantityDefinition common = new(
      itemId: 100,
      chanceDenominator: 3,
      amountDroppedMinimum: 1,
      amountDroppedMaximum: 2,
      chanceNumerator: 1);

    DropAttemptResult result = DropRuleResolutionQuery.EvaluateCommonDrop(
      common,
      context,
      new DropRerollPolicy(timesToRoll: 2));

    Assert(result.State == DropAttemptResultState.Success && result.ItemId == 100 &&
      result.RollCount == 3 && random.CallCount == 3,
      "Common drop resolution must use the injected RNG once per documented roll and stop on success.");

    SequenceDropRandomSource blockedRandom = new(0);
    DropResolutionContext blockedContext = new(
      blockedRandom,
      npcName: "Other NPC");
    DropConditionBranchRuntime blockedBranch = new(
      DropConditionBranchKind.ItemDropWithCondition,
      DropConditionDefinition.NamedNpc("Test NPC"));
    DropAttemptResult blocked = DropRuleResolutionQuery.EvaluateCommonDrop(
      common,
      blockedContext,
      conditionBranch: blockedBranch);
    Assert(blocked.State == DropAttemptResultState.DoesntFillConditions &&
      blockedRandom.CallCount == 0,
      "Failed drop conditions must not consume random values or commit a result.");
  }

  private static void TestDropChainsAndOptionSelectors()
  {
    DropRuleReference failedRule = new("failed-rule");
    DropRuleReference successRule = new("success-rule");
    DropRuleReference conditionRule = new("condition-rule");
    DropRuleChainDefinition chain = new(
    [
      new DropChainAttemptContract(
        failedRule,
        DropChainTriggerKind.FailedRandomRoll,
        new DropChainVisibilityPolicy(hideLootReport: true)),
      new DropChainAttemptContract(
        successRule,
        DropChainTriggerKind.Succeeded),
      new DropChainAttemptContract(
        conditionRule,
        DropChainTriggerKind.DoesntFillConditions)
    ]);

    Assert(chain.GetTriggeredChains(DropAttemptResult.FailedRandomRoll(1)).Length == 1 &&
      chain.GetTriggeredChains(DropAttemptResult.FailedRandomRoll(1))[0].RuleToChain == failedRule,
      "Failed-random-roll chains must trigger only for the matching parent state.");
    Assert(chain.GetTriggeredChains(DropAttemptResult.Success(1, 1, 1, 1))[0].RuleToChain ==
      successRule,
      "Success chains must trigger only for successful parent results.");
    Assert(chain.Attempts[0].VisibilityPolicy.HideLootReport &&
      chain.Attempts[0].GetRateMultiplier(0.25f) == 0.75f,
      "Chain visibility and failed-roll rate projection must preserve source semantics.");

    DropResolutionContext expertContext = new(
      random: new SequenceDropRandomSource(0),
      isExpertMode: true);
    DropModeOptionSelector modeSelector = DropModeOptionSelector.ForExpertMode(
      new DropRuleReference("normal"),
      new DropRuleReference("expert"));
    Assert(modeSelector.Select(expertContext) == new DropRuleReference("expert"),
      "Mode selectors must choose the expert rule from an expert snapshot.");

    DropItemOptionSelector itemSelector = new(
      itemIds: [10, 20],
      chanceDenominator: 1,
      chanceNumerator: 1);
    DropAttemptResult selected = DropRuleResolutionQuery.EvaluateItemOptions(
      itemSelector,
      new DropResolutionContext(new SequenceDropRandomSource(0, 1)));
    Assert(selected.State == DropAttemptResultState.Success && selected.ItemId == 20,
      "Item option selectors must use the explicit chance and option random draws.");

    DropOptionsWithoutRepeatsState noRepeats = new([10, 20, 30]);
    SequenceDropRandomSource noRepeatRandom = new(0, 0);
    Assert(noRepeats.TryTake(noRepeatRandom, out int first) && first == 10 &&
      noRepeats.TryTake(noRepeatRandom, out int second) && second == 20 &&
      noRepeats.AvailableItemCount == 1,
      "No-repeat option state must remove only the selected temporary option.");

    DropOneByOneParameters parameters = new(
      chanceNumerator: 1,
      chanceDenominator: 2,
      minimumItemDropsCount: 1,
      maximumItemDropsCount: 2,
      minimumStackPerChunkBase: 1,
      maximumStackPerChunkBase: 3,
      bonusMinDropsPerChunkPerPlayer: 1,
      bonusMaxDropsPerChunkPerPlayer: 2);
    DropOneByOneDefinition oneByOne = new(itemId: 300, parameters);
    Assert(oneByOne.ItemId == 300 && oneByOne.Parameters.MaximumStackPerChunkBase == 3,
      "One-by-one definitions must preserve quantity and player-bonus parameters.");
  }

  private static void TestCommerceAndShopState()
  {
    List<int> shopSlots = [10, 0, 20];
    ShopInventorySlotsComponent shop = new(shopSlots, slotCapacity: 4);
    shopSlots[0] = 99;
    Assert(shop.SlotContents.SequenceEqual([10, 0, 20]) &&
      shop.SlotCapacity == 4 && shop.OccupiedSlotCount == 2,
      "Shop inventory slots must snapshot contents and expose the explicit capacity.");
    Assert(shop.TryGetItemTypeId(2, out int shopItemTypeId) && shopItemTypeId == 20 &&
      !shop.TryGetItemTypeId(3, out _),
      "Shop slot lookup must not invent entries outside the captured slot snapshot.");

    TravelShopCatalogState travelShop = new([100, 200]);
    Assert(travelShop.MaxSlots == 40 && travelShop.ItemTypeIds.SequenceEqual([100, 200]),
      "Travel shop state must preserve the source maximum slot count and ordered items.");
    AssertThrows<ArgumentException>(
      () => new TravelShopCatalogState(Enumerable.Repeat(1, 41)),
      "Travel shop state must reject entries beyond the source slot limit.");

    AnglerQuestStateComponent angler = AnglerQuestStateComponent.CreateDefault();
    Assert(angler.QuestItemNetIds.SequenceEqual([
        2450, 2451, 2452, 2453, 2454, 2455, 2456, 2457, 2458, 2459,
        2460, 2461, 2462, 2463, 2464, 2465, 2466, 2467, 2468, 2469,
        2470, 2471, 2472, 2473, 2474, 2475, 2476, 2477, 2478, 2479,
        2480, 2481, 2482, 2483, 2484, 2485, 2486, 2487, 2488, 4393,
        4394]) && angler.CurrentQuestItemNetId == 2450,
      "Angler quest state must preserve the source quest-item catalog and index.");
    AnglerQuestStateComponent completedByPlayer =
      angler.WithPlayerCompletion("Alice");
    Assert(completedByPlayer.HasPlayerFinished("Alice") &&
      !angler.HasPlayerFinished("Alice"),
      "Angler completion updates must return a new state without mutating the prior snapshot.");

    ItemCommerceDefinition itemCommerce = new(
      isShopItem: true,
      buyOnce: true,
      baseValue: 500,
      canBuy: true,
      shopSpecialCurrency: 7,
      shopCustomPrice: 450);
    Assert(itemCommerce.IsShopItem && itemCommerce.BuyOnce &&
      itemCommerce.BaseValue == 500 && itemCommerce.CanBuy &&
      itemCommerce.ShopSpecialCurrency == 7 &&
      itemCommerce.ShopCustomPrice == 450,
      "Item commerce definitions must preserve shop, price, and currency fields.");

    List<SellbackMemoryEntry> sellbackEntries =
      [new SellbackMemoryEntry(itemTypeId: 100, prefixId: 2, stack: 3)];
    SellbackMemoryState sellback = new(sellbackEntries);
    sellbackEntries.Clear();
    Assert(sellback.TryGetMemo(100, 2, out SellbackMemoryEntry memo) &&
      memo.Stack == 3,
      "Sellback memory must snapshot the source memo list.");
    SellbackMemoryState updatedSellback = sellback.WithMemo(
      new SellbackMemoryEntry(itemTypeId: 100, prefixId: 2, stack: 9));
    Assert(updatedSellback.TryGetMemo(100, 2, out SellbackMemoryEntry updatedMemo) &&
      updatedMemo.Stack == 9 && sellback.TryGetMemo(100, 2, out memo) &&
      memo.Stack == 3,
      "Sellback updates must be isolated value-state transitions.");

    Assert(ShopPricePolicyDefinition.ClampPriceAdjustment(0.5f) == 0.75f &&
      ShopPricePolicyDefinition.ClampPriceAdjustment(2f) == 1.5f &&
      ShopPricePolicyDefinition.ClampPriceAdjustment(1.1f) == 1.1f,
      "Shop price policy must preserve the evidenced lower and upper multipliers.");

    ShopMoodEvaluationContext mood = new(
      buyerKey: "player:7",
      sellerKey: "npc:4",
      currentPriceAdjustment: 1.1f,
      currentHappiness: "pending");
    Assert(mood.BuyerKey == "player:7" && mood.SellerKey == "npc:4" &&
      mood.CurrentPriceAdjustment == 1.1f && mood.CurrentHappiness == "pending",
      "Shop mood context must retain scalar participant keys and calculated values only.");

    ShopMoodWeightsDefinition weights = new();
    Assert(weights.LikeMultiplier == 0.94f && weights.DislikeMultiplier == 1.06f &&
      weights.LoveMultiplier == 0.88f && weights.HateMultiplier == 1.12f,
      "Shop mood weights must preserve the source constants without live personality state.");

    ShopPersonalityCatalog personalities = new(["Merchant", "Angler"]);
    ShopBiomeModifierCatalog biomes = new([
      "CorruptionBiome", "CrimsonBiome", "DungeonBiome"]);
    Assert(personalities.Contains("Merchant") && biomes.Contains("DungeonBiome"),
      "Shop catalogs must expose explicit normalized adapter keys.");

    Assert(ShoppingSettingsProjection.NotInShop.PriceAdjustment == 1f &&
      ShoppingSettingsProjection.NotInShop.HappinessReport == string.Empty,
      "Shopping settings must expose the source NotInShop projection.");
  }

  private static void TestCraftingRequestBoundary()
  {
    RecipeDefinition recipe = new(
      new RecipeId(80),
      new RecipeResultDefinition(itemTypeId: 500, stack: 1),
      [new RecipeIngredientDefinition(itemTypeId: 10, stack: 2)],
      requiredTileId: 18,
      customShimmerResults: [],
      conditions: []);
    List<CraftingItemSnapshot> consumed =
      [new CraftingItemSnapshot(itemTypeId: 10, prefixId: 0, stack: 2)];
    List<CraftingIngredientRequest> requested =
      [new CraftingIngredientRequest(itemTypeOrGroupId: 10, stack: 2)];
    CraftingRequestCommand request = new(
      requestId: "request-80",
      recipe,
      new CraftingItemSnapshot(itemTypeId: 500, prefixId: 0, stack: 1),
      consumed,
      requested,
      quickCraft: true);
    consumed.Clear();
    requested.Clear();

    Assert(request.RequestId == "request-80" && request.Recipe.Id == new RecipeId(80) &&
      request.Result.ItemTypeId == 500 && request.QuickCraft &&
      request.ConsumedItems.Length == 1 && request.RequestedIngredients.Length == 1,
      "Craft requests must snapshot recipe, result, consumed items, and requested ingredients.");
    AssertThrows<ArgumentException>(
      () => new CraftingRequestCommand(
        "",
        recipe,
        request.Result,
        [],
        [],
        quickCraft: false),
      "Craft requests must require a stable request identity.");

    CraftingRequestQueueState queue = new();
    Assert(!queue.HasPendingRequests && !queue.TryDequeue(out _),
      "A new craft request queue must be empty.");
    queue.Enqueue(request);
    Assert(queue.HasPendingRequests && queue.PendingRequestCount == 1,
      "Queue state must expose the pending-request readiness boundary.");
    Assert(queue.TryDequeue(out CraftingRequestCommand dequeued) &&
      dequeued.RequestId == "request-80" && !queue.HasPendingRequests,
      "Queue dequeue must preserve FIFO request identity and clear readiness.");

    RecipeCraftingRuntimeState runtime = new(
      ownedItems: new Dictionary<int, int> { [10] = 2 },
      recipeChestItemCounts: new Dictionary<int, int> { [10] = 1 });
    Assert(runtime.GetOwnedItemCount(10) == 2 &&
      runtime.GetRecipeChestItemCount(10) == 1 &&
      runtime.GetTotalAvailableItemCount(10) == 3,
      "Recipe runtime state must expose explicit owned-item and recipe-chest snapshots.");
    Assert(runtime.GetOwnedItemCount(99) == 0 && runtime.GetRecipeChestItemCount(99) == 0,
      "Missing recipe runtime keys must resolve to zero without global lookups.");
  }

  private static void TestLootSimulationAndAttribution()
  {
    long[] normalCounts = [1, 2, 3];
    long[] expertCounts = [4, 5, 6];
    LootSimulationCounterProjection counter = new(normalCounts, expertCounts);
    normalCounts[0] = 99;
    expertCounts[0] = 98;

    Assert(counter.ItemTypeCount == 3 &&
      counter.GetObtainedCount(itemTypeId: 0, expertMode: false) == 1 &&
      counter.GetObtainedCount(itemTypeId: 0, expertMode: true) == 4,
      "Loot simulation counters must defensively snapshot normal and expert arrays.");

    LootSimulationCounterProjection incremented = counter.WithObtained(
      itemTypeId: 1,
      expertMode: false,
      amount: 3);
    Assert(counter.GetObtainedCount(1, expertMode: false) == 2 &&
      incremented.GetObtainedCount(1, expertMode: false) == 5,
      "Counter updates must return isolated projections without mutating the prior result.");

    LootSimulationContext context = new(
      playerKey: "player:7",
      originalDayTimeCounter: 123.5,
      originalDayTimeFlag: true,
      originalPlayerPositionX: 12.5f,
      originalPlayerPositionY: 20.25f,
      runningExpertMode: true,
      itemCounter: incremented,
      npcVictimKey: "npc:42");
    Assert(context.PlayerKey == "player:7" &&
      context.OriginalDayTimeCounter == 123.5 &&
      context.OriginalDayTimeFlag &&
      context.OriginalPlayerPositionX == 12.5f &&
      context.OriginalPlayerPositionY == 20.25f &&
      context.RunningExpertMode &&
      context.ItemCounter == incremented &&
      context.NpcVictimKey == "npc:42",
      "Loot simulation context must preserve explicit scalar snapshots and projection state.");

    DropSourceAttribution lootSource = DropSourceAttribution.Loot("npc:42");
    Assert(lootSource.Kind == DropSourceKind.Loot &&
      lootSource.SourceEntityKey == "npc:42" &&
      !lootSource.OwnsEntity,
      "Drop source attribution must retain a source reference without owning entity identity.");

    FishingCatchAttributionAdapter adapter = new();
    DropSourceAttribution fishedOutSource = adapter.Adapt("player:7");
    Assert(fishedOutSource.Kind == DropSourceKind.FishedOut &&
      fishedOutSource.SourceEntityKey == "player:7" &&
      !fishedOutSource.OwnsEntity,
      "Fishing catch attribution must adapt the source reference without creating an entity.");

    ChumFrameCache chum = new();
    chum.AddPending(x: 10, y: 11, count: 2);
    chum.AddPending(x: 10, y: 11, count: 1);
    Assert(!chum.TryGetPrevious(x: 10, y: 11, out _),
      "Pending chum counts must not be visible as previous-frame counts before the frame boundary.");
    chum.AdvanceFrame();
    Assert(chum.TryGetPrevious(10, 11, out int previousCount) && previousCount == 3,
      "Advancing the chum frame must expose the accumulated pending count as previous-frame state.");

    chum.AddPending(x: 12, y: 13, count: 4);
    chum.AdvanceFrame();
    Assert(!chum.TryGetPrevious(10, 11, out _) &&
      chum.TryGetPrevious(12, 13, out previousCount) && previousCount == 4,
      "A chum frame advance must replace the previous snapshot and clear the new pending map.");
  }

  private static void TestP10SourceInventoryAndImplementationBoundary()
  {
    P10SourceInventoryVerificationResult result =
      P10SourceInventoryVerifier.Verify(FindRepositoryRoot());
    Assert(result.IsValid, result.FailureMessage);
  }

  private static string FindRepositoryRoot()
  {
    string[] startingPaths =
    [
      Directory.GetCurrentDirectory(),
      AppContext.BaseDirectory
    ];

    foreach (string startingPath in startingPaths)
    {
      DirectoryInfo? directory = new DirectoryInfo(startingPath);
      while (directory is not null)
      {
        if (File.Exists(Path.Combine(directory.FullName, "AGENTS.md")) &&
            Directory.Exists(Path.Combine(directory.FullName, "src2")))
        {
          return directory.FullName;
        }

        directory = directory.Parent;
      }
    }

    throw new InvalidOperationException(
      "The P10 verifier could not locate the NLTX repository root.");
  }

  private static void Assert(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }

  private static void AssertThrows<TException>(Action action, string message)
    where TException : Exception
  {
    try
    {
      action();
    }
    catch (TException)
    {
      return;
    }

    throw new InvalidOperationException(message);
  }

  private sealed class FixedFishingRandomSource : IFishingRandomSource
  {
    public int Next(int exclusiveUpperBound)
    {
      return 0;
    }
  }

  private sealed class SequenceFishingRandomSource : IFishingRandomSource
  {
    private readonly Queue<int> _values;

    public SequenceFishingRandomSource(params int[] values)
    {
      _values = new Queue<int>(values);
    }

    public int CallCount { get; private set; }

    public int Next(int exclusiveUpperBound)
    {
      CallCount++;
      if (_values.Count == 0)
      {
        return 0;
      }

      return _values.Dequeue();
    }
  }

  private sealed class SequenceDropRandomSource : IDropRandomSource
  {
    private readonly Queue<int> _values;

    public SequenceDropRandomSource(params int[] values)
    {
      _values = new Queue<int>(values);
    }

    public int CallCount { get; private set; }

    public int Next(int exclusiveUpperBound)
    {
      CallCount++;
      if (_values.Count == 0)
      {
        return 0;
      }

      return _values.Dequeue();
    }
  }
}

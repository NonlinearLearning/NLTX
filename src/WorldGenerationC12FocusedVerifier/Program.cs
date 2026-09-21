using System;
using System.Collections.Generic;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Queries;
using Terraria.WorldGeneration.Systems;

static class Program
{
  private static int Main()
  {
    try
    {
      TestHellChestLootCycle();
      TestStatueCatalogAndTrapRule();
      TestInfectionAlignmentAndSpecialSeedFlags();
      TestShimmerAnchorCommitBoundary();
      Console.WriteLine("C12 hell and special-structure focused verifier passed.");
      return 0;
    }
    catch (Exception exception)
    {
      Console.Error.WriteLine(exception);
      return 1;
    }
  }

  private static void TestHellChestLootCycle()
  {
    HellChestLootCycleComponent component =
      HellChestGenerationSystem.CreateForGeneration(
        generationId: 61,
        remixWorld: false,
        shuffledItemSequence: new[] { 274, 220, 112, 218, 3019 });

    Require(component.CurrentIndex == 0, "hell chest cursor starts at zero");
    Require(
      HellChestLootCycleQuery.CurrentItem(component) == 274,
      "hell chest current item comes from the sequence");
    Require(
      !HellChestGenerationSystem.TryAdvanceAfterPlacement(component, placementSucceeded: false),
      "failed hell chest placement must not advance");
    Require(component.CurrentIndex == 0, "failed hell chest placement preserves cursor");

    for (int index = 0; index < 5; index++)
    {
      Require(
        HellChestGenerationSystem.TryAdvanceAfterPlacement(
          component,
          placementSucceeded: true),
        "successful hell chest placement advances cursor");
    }

    Require(component.CurrentIndex == 0, "hell chest cursor wraps after the sequence");
    HellChestLootCycleSnapshot snapshot =
      HellChestLootCycleQuery.Snapshot(component);
    Require(snapshot.GenerationId == 61, "hell chest generation identity");
    Require(snapshot.ItemSequence.Count == 5, "hell chest sequence length");
    Require(
      ((IList<int>)snapshot.ItemSequence).IsReadOnly,
      "hell chest snapshot sequence is read-only");

    HellChestLootCycleComponent remixComponent =
      HellChestGenerationSystem.CreateForGeneration(
        generationId: 62,
        remixWorld: true,
        shuffledItemSequence: new[] { 274, 220, 683, 218, 3019 });
    Require(
      HellChestLootCycleQuery.CurrentItem(remixComponent) == 274,
      "remix hell chest pool is accepted");
    RequireThrows<ArgumentException>(
      () => HellChestGenerationSystem.CreateForGeneration(
        generationId: 63,
        remixWorld: false,
        shuffledItemSequence: new[] { 274, 220, 683, 218, 3019 }),
      "normal hell chest pool rejects the remix-only item");
  }

  private static void TestStatueCatalogAndTrapRule()
  {
    StatuePlacementCatalogDefinition catalog =
      GenerationRuleInitializationSystem.CreateStatuePlacementCatalog();
    Require(catalog.Options.Count == 73, "statue catalog preserves Version4 count");
    Require(
      StatuePlacementCatalogQuery.GetOption(catalog, 0) == new StatuePlacementOption(105, 0),
      "statue catalog first option");
    Require(
      StatuePlacementCatalogQuery.GetOption(catalog, 34) == new StatuePlacementOption(349, 0),
      "statue catalog preserves Version4 replacement");
    Require(
      StatuePlacementCatalogQuery.GetOption(catalog, 43) == new StatuePlacementOption(105, 50),
      "statue catalog preserves Version4 style replacement");
    Require(
      StatuePlacementCatalogQuery.GetOption(catalog, 72) == new StatuePlacementOption(105, 2),
      "statue catalog preserves final append order");
    Require(
      ((IList<StatuePlacementOption>)catalog.Options).IsReadOnly,
      "statue catalog is read-only");

    List<StatuePlacementOption> mutableOptions =
      new() { new StatuePlacementOption(105, 0) };
    StatuePlacementCatalogDefinition copiedCatalog =
      new(mutableOptions);
    mutableOptions[0] = new StatuePlacementOption(349, 0);
    Require(
      StatuePlacementCatalogQuery.GetOption(copiedCatalog, 0) ==
        new StatuePlacementOption(105, 0),
      "statue catalog copies its input options");

    StatueTrapSelectionDefinition traps =
      GenerationRuleInitializationSystem.CreateStatueTrapSelection();
    Require(StatueTrapRuleQuery.Contains(traps, 4), "statue trap index four");
    Require(StatueTrapRuleQuery.Contains(traps, 7), "statue trap index seven");
    Require(StatueTrapRuleQuery.Contains(traps, 10), "statue trap index ten");
    Require(StatueTrapRuleQuery.Contains(traps, 18), "statue trap index eighteen");
    Require(!StatueTrapRuleQuery.Contains(traps, 3), "non-trap statue index");

    List<int> mutableTrapIndexes = new() { 4 };
    StatueTrapSelectionDefinition copiedTraps =
      new(mutableTrapIndexes);
    mutableTrapIndexes[0] = 3;
    Require(
      StatueTrapRuleQuery.Contains(copiedTraps, 4) &&
        !StatueTrapRuleQuery.Contains(copiedTraps, 3),
      "statue trap definition copies its input indexes");
  }

  private static void TestInfectionAlignmentAndSpecialSeedFlags()
  {
    InfectionAlignmentComponent alignment =
      InfectionGenerationSystem.CreateAlignment(
        generationId: 63,
        crimsonLeft: true,
        drunkWorld: true,
        getGoodWorld: true,
        remixWorld: false);
    InfectionAlignmentSnapshot alignmentSnapshot =
      InfectionAlignmentQuery.Snapshot(alignment);
    Require(alignmentSnapshot.CrimsonLeft, "infection alignment preserves evil side");
    Require(alignmentSnapshot.FlipInfections, "infection flip conjunction");

    InfectionAlignmentComponent nonFlippedAlignment =
      InfectionGenerationSystem.CreateAlignment(
        generationId: 64,
        crimsonLeft: false,
        drunkWorld: true,
        getGoodWorld: true,
        remixWorld: true);
    Require(
      !InfectionAlignmentQuery.Snapshot(nonFlippedAlignment).FlipInfections,
      "remix disables the infection flip conjunction");

    SpecialSeedGenerationRuleFlagsComponent flags =
      GenerationRuleInitializationSystem.CreateSpecialSeedGenerationRuleFlags(
        generationId: 65,
        notTheBeesWorld: true,
        noTrapsWorld: false,
        getGoodWorld: true,
        tenthAnniversaryWorld: false);
    SpecialSeedGenerationRuleFlagsSnapshot flagSnapshot =
      SpecialSeedGenerationRuleQuery.Snapshot(flags);
    Require(
      flagSnapshot.NotTheBeesAndForTheWorthyNoCelebration,
      "bee and worthy rule conjunction");
    Require(
      !flagSnapshot.NoTrapsAndForTheWorthyNoCelebration,
      "no-traps rule conjunction");
  }

  private static void TestShimmerAnchorCommitBoundary()
  {
    ShimmerBiomeAnchorComponent component =
      new(generationId: 66);
    Require(!component.HasAnchor, "shimmer anchor starts clear");
    Require(
      ShimmerGenerationSystem.CommitAfterSuccessfulBiome(
          component,
          new ShimmerBiomeAnchorPoint(300, 400),
          biomeCommitted: false)
        .Status == ShimmerBiomeCommitStatus.RejectedBiomeCommit,
      "failed shimmer biome does not publish an anchor");
    Require(!component.HasAnchor, "failed shimmer biome preserves clear anchor");

    ShimmerBiomeCommitResult commitResult =
      ShimmerGenerationSystem.CommitAfterSuccessfulBiome(
        component,
        new ShimmerBiomeAnchorPoint(300, 400),
        biomeCommitted: true);
    Require(
      commitResult.Status == ShimmerBiomeCommitStatus.Published,
      "successful shimmer biome publishes an anchor");
    Require(component.HasAnchor, "successful shimmer biome sets anchor presence");
    Require(
      ShimmerAnchorQuery.HasAnchorWithinDistance(
        ShimmerAnchorQuery.Snapshot(component),
        new ShimmerBiomeAnchorPoint(599, 400),
        distance: 300),
      "shimmer exclusion uses strict distance");
    Require(
      !ShimmerAnchorQuery.HasAnchorWithinDistance(
        ShimmerAnchorQuery.Snapshot(component),
        new ShimmerBiomeAnchorPoint(600, 400),
        distance: 300),
      "shimmer exclusion allows exact distance boundary");

    ShimmerBiomeAnchorSnapshot beforeReset = ShimmerAnchorQuery.Snapshot(component);
    ShimmerGenerationSystem.Reset(component);
    Require(!component.HasAnchor, "shimmer reset clears anchor presence");
    Require(beforeReset.HasAnchor, "shimmer snapshot is isolated from reset");
  }

  private static void Require(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }

  private static void RequireThrows<TException>(Action action, string message)
    where TException : Exception
  {
    try
    {
      action.Invoke();
    }
    catch (TException)
    {
      return;
    }

    throw new InvalidOperationException(message);
  }
}

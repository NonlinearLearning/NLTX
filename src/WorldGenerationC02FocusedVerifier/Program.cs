using Terraria.WorldGeneration.Adapters;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Definitions;
using Terraria.WorldGeneration.Passes;
using Terraria.WorldGeneration.Queries;
using Terraria.WorldGeneration.Systems;

static class Program
{
  private static int Main()
  {
    try
    {
      TestVersion4BaselineMetrics();
      TestInitialBeachPaddingDelaysFeatureSelection();
      TestComponentCopiesPairedSnowColumns();
      TestCommitRejectsWrongGeneration();
      Console.WriteLine("C02 world layer metrics focused verifier passed.");
      return 0;
    }
    catch (Exception exception)
    {
      Console.Error.WriteLine(exception);
      return 1;
    }
  }

  private static void TestVersion4BaselineMetrics()
  {
    WorldGenerationConfigurationDefinition configuration =
      WorldGenerationConfigurationDefinition.Create(
        worldWidth: 4200,
        worldHeight: 1200,
        seedText: "c02-baseline",
        rulesVersion: 1);
    WorldLayerMetricsCalculationInput input = new(
      generationId: 1,
      configuration,
      leftBeachEnd: 0,
      rightBeachStart: 4200,
      flatBeachPadding: 0,
      isRemixWorld: false,
      isDrunkWorld: false,
      isGoodWorld: false,
      isNoSurfaceWorld: false,
      isSurfaceInSpace: false);
    WorldLayerMetricsComponent component = new(generationId: 1);

    WorldLayerMetricsSnapshot snapshot =
      WorldLayerMetricsSystem.CalculateAndCommit(
        component,
        input,
        new BaselineRandomSource());

    Require(snapshot.GenerationId == 1, "baseline generation");
    Require(snapshot.LowestCloud == -1, "baseline cloud sentinel");
    Require(snapshot.WorldSurfaceLow == 180d, "baseline surface low");
    Require(snapshot.WorldSurface == 228d, "baseline surface");
    Require(snapshot.WorldSurfaceHigh == 228d, "baseline surface high");
    Require(snapshot.RockLayerLow == 420d, "baseline rock low");
    Require(snapshot.RockLayer == 420d, "baseline rock");
    Require(snapshot.RockLayerHigh == 420d, "baseline rock high");
    Require(snapshot.SnowMinX.Count == 1200, "baseline snow minimum length");
    Require(snapshot.SnowMaxX.Count == 1200, "baseline snow maximum length");
    Require(snapshot.SnowMinX[1199] == 0, "baseline snow minimum default");
    Require(snapshot.SnowMaxX[1199] == 0, "baseline snow maximum default");
  }

  private static void TestInitialBeachPaddingDelaysFeatureSelection()
  {
    WorldGenerationConfigurationDefinition configuration =
      WorldGenerationConfigurationDefinition.Create(
        worldWidth: 20,
        worldHeight: 100,
        seedText: "c02-padding",
        rulesVersion: 1);
    WorldLayerMetricsCalculationInput input = new(
      generationId: 2,
      configuration,
      leftBeachEnd: 5,
      rightBeachStart: 15,
      flatBeachPadding: 2,
      isRemixWorld: false,
      isDrunkWorld: false,
      isGoodWorld: false,
      isNoSurfaceWorld: false,
      isSurfaceInSpace: false);
    PaddingRandomSource random = new();

    _ = WorldLayerMetricsSystem.CalculateAndCommit(
      new WorldLayerMetricsComponent(generationId: 2),
      input,
      random);

    Require(
      random.FirstFeatureSelectionCall == 17,
      "initial beach padding must precede feature selection");
  }

  private static void TestComponentCopiesPairedSnowColumns()
  {
    int[] minimums = [10, 20];
    int[] maximums = [11, 21];
    WorldLayerMetricsComponent component = new(
      generationId: 3,
      snowMinX: minimums,
      snowMaxX: maximums);
    minimums[0] = -1;
    maximums[0] = -1;

    WorldLayerMetricsSnapshot snapshot =
      WorldLayerMetricsQuery.Snapshot(component);
    Require(snapshot.SnowMinX[0] == 10, "component input copy");
    Require(snapshot.SnowMaxX[0] == 11, "component paired input copy");

    bool rejected = false;
    try
    {
      _ = new WorldLayerMetricsComponent(
        generationId: 4,
        snowMinX: [1],
        snowMaxX: Array.Empty<int>());
    }
    catch (ArgumentException)
    {
      rejected = true;
    }

    Require(rejected, "unpaired snow columns must be rejected");
  }

  private static void TestCommitRejectsWrongGeneration()
  {
    WorldLayerMetricsComponent component = new(generationId: 5);
    WorldLayerMetricsSnapshot snapshot = new(
      6,
      -1,
      1d,
      2d,
      3d,
      4d,
      5d,
      6d,
      0,
      0,
      0,
      0,
      Array.Empty<int>(),
      Array.Empty<int>());
    bool rejected = false;

    try
    {
      WorldLayerMetricsSystem.Commit(component, snapshot);
    }
    catch (ArgumentException)
    {
      rejected = true;
    }

    Require(rejected, "wrong generation commit rejection");
    Require(component.GenerationId == 5, "wrong generation preserves owner");
  }

  private static void Require(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }

  private sealed class BaselineRandomSource : IGenerationRandomSource
  {
    public int NextInt(
      GenerationRandomStream stream,
      int minimumInclusive,
      int maximumExclusive)
    {
      if (minimumInclusive == 90 && maximumExclusive == 110)
      {
        return 100;
      }

      if (minimumInclusive == 0 && maximumExclusive == 5)
      {
        return 0;
      }

      if (minimumInclusive == 5 && maximumExclusive == 40)
      {
        return 5;
      }

      if (minimumInclusive == 5 && maximumExclusive == 30)
      {
        return 5;
      }

      if (minimumInclusive == 0 && maximumExclusive == 7)
      {
        return 1;
      }

      if (minimumInclusive == 0 && maximumExclusive == 3)
      {
        return 1;
      }

      return minimumInclusive + (maximumExclusive - minimumInclusive) / 2;
    }
  }

  private sealed class PaddingRandomSource : IGenerationRandomSource
  {
    public int CallCount { get; private set; }

    public int FirstFeatureSelectionCall { get; private set; } = -1;

    public int NextInt(
      GenerationRandomStream stream,
      int minimumInclusive,
      int maximumExclusive)
    {
      CallCount++;
      if (stream == GenerationRandomStream.Terrain &&
          minimumInclusive == 0 && maximumExclusive == 5 &&
          FirstFeatureSelectionCall < 0)
      {
        FirstFeatureSelectionCall = CallCount;
        return 0;
      }

      if (minimumInclusive == 90 && maximumExclusive == 110)
      {
        return 100;
      }

      if (minimumInclusive == 5 && maximumExclusive == 40)
      {
        return 5;
      }

      if (minimumInclusive == 5 && maximumExclusive == 30)
      {
        return 5;
      }

      if (minimumInclusive == -2 && maximumExclusive == 3)
      {
        return 0;
      }

      if (minimumInclusive == 0)
      {
        return 1;
      }

      return minimumInclusive + (maximumExclusive - minimumInclusive) / 2;
    }
  }
}

using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacySiltPass
{
  public static IReadOnlyList<LegacyTileRunnerPassInvocation> CreateInvocationRecords(
    WorldGridSnapshot snapshot,
    LegacySiltPassDefinition definition,
    LegacySiltPassDefinition secondaryDefinition,
    LegacyPassRandomState random,
    int rockLayerHigh,
    int worldSurface,
    int rockLayer,
    bool isRemixWorld,
    bool isSkyblockWorld)
  {
    IReadOnlyList<LegacyTileRunnerRequest> requests = CreateInvocations(
      snapshot,
      definition,
      secondaryDefinition,
      random,
      rockLayerHigh,
      worldSurface,
      rockLayer,
      isRemixWorld,
      isSkyblockWorld);
    LegacyTileRunnerPassInput primaryRecipe = CreateRecipe("primary", definition, isRemixWorld);
    LegacyTileRunnerPassInput secondaryRecipe =
      CreateRecipe("secondary", secondaryDefinition, isRemixWorld);
    int primaryCount = definition.CalculateInvocationCount(
      snapshot.Metadata.Width,
      snapshot.Metadata.Height);
    List<LegacyTileRunnerPassInvocation> records = new();
    for (int index = 0; index < requests.Count; index++)
    {
      LegacyTileRunnerRequest request = requests[index];
      LegacyTileRunnerPassInput recipe = index < primaryCount ? primaryRecipe : secondaryRecipe;
      records.Add(new LegacyTileRunnerPassInvocation(
        recipe,
        request,
        request.X,
        request.Y,
        (int)request.Strength,
        request.Steps,
        RandomDrawCount: 4));
    }

    return records.AsReadOnly();
  }

  public static IReadOnlyList<LegacyTileRunnerRequest> CreateInvocations(
    WorldGridSnapshot snapshot,
    LegacySiltPassDefinition definition,
    LegacySiltPassDefinition secondaryDefinition,
    LegacyPassRandomState random,
    int rockLayerHigh,
    int worldSurface,
    int rockLayer,
    bool isRemixWorld,
    bool isSkyblockWorld)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(definition);
    ArgumentNullException.ThrowIfNull(secondaryDefinition);
    ArgumentNullException.ThrowIfNull(random);
    if (rockLayerHigh < 0 || worldSurface < 0 || rockLayer <= worldSurface ||
        rockLayerHigh >= snapshot.Metadata.Height || rockLayer >= snapshot.Metadata.Height)
    {
      throw new ArgumentOutOfRangeException(nameof(rockLayerHigh));
    }

    List<LegacyTileRunnerRequest> invocations = new();
    if (isSkyblockWorld)
    {
      return invocations.AsReadOnly();
    }

    AppendBand(
      snapshot,
      definition,
      random,
      definition.CalculateInvocationCount(snapshot.Metadata.Width, snapshot.Metadata.Height),
      snapshot.Metadata.Height,
      isRemixWorld ? worldSurface : rockLayerHigh,
      isRemixWorld ? rockLayer : snapshot.Metadata.Height,
      invocations);
    AppendBand(
      snapshot,
      secondaryDefinition,
      random,
      secondaryDefinition.CalculateInvocationCount(
        snapshot.Metadata.Width,
        snapshot.Metadata.Height),
      snapshot.Metadata.Height,
      isRemixWorld ? worldSurface : rockLayerHigh,
      isRemixWorld ? rockLayer : snapshot.Metadata.Height,
      invocations);
    return invocations.AsReadOnly();
  }

  private static void AppendBand(
    WorldGridSnapshot snapshot,
    LegacySiltPassDefinition definition,
    LegacyPassRandomState random,
    int count,
    int height,
    int minimumY,
    int maximumYExclusive,
    ICollection<LegacyTileRunnerRequest> invocations)
  {
    for (int index = 0; index < count; index++)
    {
      int x = random.Next(snapshot.Metadata.Width);
      int y = random.Next(minimumY, Math.Min(maximumYExclusive, height));
      WorldTile tile = snapshot.GetTile(x, y);
      if (!definition.IsWallEligible(tile.WallType))
      {
        continue;
      }

      invocations.Add(LegacySiltInvocationFactory.Create(
        definition,
        random,
        x,
        y));
    }
  }

  private static LegacyTileRunnerPassInput CreateRecipe(
    string recipeName,
    LegacySiltPassDefinition definition,
    bool isRemixWorld)
  {
    return new LegacyTileRunnerPassInput(
      PassName: "Silt",
      RecipeName: recipeName,
      TileType: definition.TileType,
      AddTile: true,
      MinimumStrength: definition.MinimumStrength,
      MaximumStrengthExclusive: definition.MaximumStrengthExclusive,
      MinimumSteps: definition.MinimumSteps,
      MaximumStepsExclusive: definition.MaximumStepsExclusive,
      VerticalRange: isRemixWorld ? "worldSurface..rockLayer" : "rockLayerHigh..maxTilesY",
      UsesRandomX: true,
      UsesRandomY: true,
      ResetsRandomFromWorldSeed: true);
  }
}

using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration.Systems;

public sealed class LegacyCavePassSystem
{
  private const int CaveSourceLine = 10957;

  public void AppendCommands(
    WorldGridSnapshot snapshot,
    WorldGenerationRequest request,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(request);
    ArgumentNullException.ThrowIfNull(commands);
    if (state.Stage < WorldGenerationStage.Cave &&
        !state.TryAdvance(WorldGenerationStage.Cave))
    {
      throw new InvalidOperationException("Cave stage could not be started.");
    }

    IReadOnlyList<LegacyTileRunnerPassInput> recipes =
      LegacyTileRunnerPassInputDefinition.CreateDefaultRecipes();
    IReadOnlyList<LegacyTileRunnerPassLoopDefinition> loops =
      LegacyTileRunnerPassLoopContractDefinition.CreateDefault();
    foreach (LegacyCavePassDefinition pass in
      LegacyCavePassContractDefinition.CreateDefaultSchedule())
    {
      if (pass.Name is not ("DirtLayerCaves" or "RockLayerCaves" or "SurfaceCaves"))
      {
        continue;
      }

      if (pass.Name == "DirtLayerCaves" &&
          request.TerrainProfile is LegacyTerrainRuntimeProfile runtimeProfile &&
          LegacyDirtLayerCavesPass.IsSupportedWorldWidth(snapshot.Metadata.Width))
      {
        LegacyDirtLayerCavesPass.AppendCommands(
          snapshot,
          runtimeProfile,
          new LegacyPassRandomState(snapshot.Metadata.Seed.Value),
          snapshot.Metadata.IsRemixWorld ?? false,
          isSkyblockWorld: request.IsSkyblockWorld,
          ref state,
          commands);
        continue;
      }

      if (pass.Name == "RockLayerCaves" &&
          request.TerrainProfile is LegacyTerrainRuntimeProfile)
      {
        continue;
      }

      if (pass.Name == "SurfaceCaves" &&
          request.TerrainProfile is LegacyTerrainRuntimeProfile)
      {
        continue;
      }

      LegacyPassRandomState random = new(snapshot.Metadata.Seed.Value);
      foreach (LegacyTileRunnerPassInput recipe in recipes)
      {
        if (recipe.PassName != pass.Name)
        {
          continue;
        }

        LegacyTileRunnerPassLoopDefinition loop = FindLoop(loops, recipe);
        int count = loop.CalculateInvocationCount(
          snapshot.Metadata.Width,
          snapshot.Metadata.Height,
          remixWorld: false);
        for (int index = 0; index < count; index++)
        {
          (int minimumY, int maximumY) = ResolveVerticalRange(
            recipe,
            request,
            snapshot.Metadata.Height);
          LegacyTileRunnerPassInvocation invocation = LegacyTileRunnerPassInvocationFactory.Create(
            recipe,
            random,
            0,
            snapshot.Metadata.Width,
            minimumY,
            maximumY);
          LegacyTileRunnerTraversal.AppendCommands(
            snapshot,
            invocation,
            random,
            request.SurfaceY,
            request.RockLayerY,
            ref state,
            commands);
        }
      }
    }
  }

  private static LegacyTileRunnerPassLoopDefinition FindLoop(
    IReadOnlyList<LegacyTileRunnerPassLoopDefinition> loops,
    LegacyTileRunnerPassInput recipe)
  {
    foreach (LegacyTileRunnerPassLoopDefinition loop in loops)
    {
      if (loop.PassName == recipe.PassName && loop.RecipeName == recipe.RecipeName)
      {
        return loop;
      }
    }

    throw new InvalidOperationException(
      $"Missing TileRunner loop definition for {recipe.PassName}.{recipe.RecipeName}.");
  }

  private static (int MinimumY, int MaximumY) ResolveVerticalRange(
    LegacyTileRunnerPassInput recipe,
    WorldGenerationRequest request,
    int height)
  {
    LegacyTerrainRuntimeProfile? runtimeProfile = request.TerrainProfile;
    int worldSurfaceLow = runtimeProfile is null
      ? request.SurfaceY
      : Math.Clamp((int)runtimeProfile.WorldSurfaceLow, 0, height - 1);
    int worldSurfaceHigh = runtimeProfile is null
      ? request.SurfaceY
      : Math.Clamp((int)runtimeProfile.WorldSurfaceHigh, 0, height - 1);
    int rockLayerLow = runtimeProfile is null
      ? request.RockLayerY
      : Math.Clamp((int)runtimeProfile.RockLayerLow, 0, height - 1);
    int rockLayerHigh = runtimeProfile is null
      ? request.RockLayerY
      : Math.Clamp((int)runtimeProfile.RockLayerHigh, 0, height - 1);
    return recipe.VerticalRange switch
    {
      "0..worldSurfaceLow" => (0, Math.Min(height, worldSurfaceLow + 1)),
      "worldSurfaceLow..worldSurfaceHigh" =>
        (worldSurfaceLow, Math.Min(height, worldSurfaceHigh + 1)),
      "worldSurfaceHigh..rockLayerHigh" =>
        (worldSurfaceHigh, Math.Min(height, rockLayerHigh + 1)),
      "rockLayerLow..maxTilesY" => (rockLayerLow, height),
      _ => throw new InvalidOperationException(
        $"Unsupported cave vertical range: {recipe.VerticalRange}")
    };
  }
}

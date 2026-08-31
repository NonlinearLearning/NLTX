using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyTunnelsPass
{
  public static int CalculateInvocationCount(
    int width,
    bool isRemixWorld = false,
    bool isTenthAnniversaryWorld = false)
  {
    return LegacyTunnelsPassDefinition.CreateDefault().CalculateInvocationCount(width, isRemixWorld);
  }

  public static bool IsCandidateXAccepted(
    int x,
    int width,
    bool isRemixWorld,
    bool isTenthAnniversaryWorld)
  {
    return LegacyTunnelsPassDefinition.CreateDefault().IsCandidateXAccepted(
      x,
      width,
      isRemixWorld,
      isTenthAnniversaryWorld);
  }

  public static (int MinimumInclusive, int MaximumExclusive) GetCandidateXRange(
    int width,
    bool isRemixWorld,
    bool isTenthAnniversaryWorld)
  {
    return LegacyTunnelsPassDefinition.CreateDefault().GetCandidateXRange(
      width,
      isRemixWorld,
      isTenthAnniversaryWorld);
  }

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    LegacyTerrainRuntimeProfile profile,
    LegacyPassRandomState random,
    bool isRemixWorld,
    bool isSkyblockWorld,
    bool isNoSurfaceWorld,
    bool isSurfaceDesertWorld,
    bool isTenthAnniversaryWorld,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> tileCommands,
    List<LiquidChangeCommand> liquidCommands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(profile);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(tileCommands);
    ArgumentNullException.ThrowIfNull(liquidCommands);
    LegacyTunnelsPassDefinition definition = LegacyTunnelsPassDefinition.CreateDefault();
    definition.Validate();
    if (isSkyblockWorld || isNoSurfaceWorld || isSurfaceDesertWorld)
    {
      return;
    }

    profile.Validate(snapshot.Metadata);
    int invocationCount = Math.Min(
      definition.CalculateInvocationCount(snapshot.Metadata.Width, isRemixWorld),
      definition.MaximumTunnelCount);
    List<int> tunnelXs = new();
    for (int index = 0; index < invocationCount; index++)
    {
      if (tunnelXs.Count >= definition.MaximumTunnelCount - 1)
      {
        break;
      }

      (int minimumStartX, int maximumStartXExclusive) = definition.GetCandidateXRange(
        snapshot.Metadata.Width,
        isRemixWorld,
        isTenthAnniversaryWorld);
      int startX = random.Next(minimumStartX, maximumStartXExclusive);
      while (!definition.IsCandidateXAccepted(
               startX,
               snapshot.Metadata.Width,
               isRemixWorld,
               isTenthAnniversaryWorld))
      {
        startX = random.Next(minimumStartX, maximumStartXExclusive);
      }

      int[] pointsX = new int[definition.TunnelPointCount];
      int[] pointsY = new int[definition.TunnelPointCount];
      int scanY = 0;
      bool hasSand = true;
      while (hasSand && scanY < snapshot.Metadata.Height)
      {
        hasSand = false;
        for (int point = 0; point < definition.TunnelPointCount; point++)
        {
          startX %= snapshot.Metadata.Width;
          while (scanY < snapshot.Metadata.Height &&
                 !snapshot.GetTile(startX, scanY).IsActive)
          {
            scanY++;
          }

          if (scanY >= snapshot.Metadata.Height)
          {
            break;
          }

          hasSand |= snapshot.GetTile(startX, scanY).Type == 53;
          pointsX[point] = startX;
          pointsY[point] = scanY - random.Next(
            definition.MinimumVerticalOffset,
            definition.MaximumVerticalOffsetExclusive);
          startX += random.Next(
            definition.MinimumHorizontalStep,
            definition.MaximumHorizontalStepExclusive);
        }
      }

      if (hasSand)
      {
        continue;
      }

      tunnelXs.Add(pointsX[definition.TunnelPointCount / 2]);
      for (int point = 0; point < definition.TunnelPointCount; point++)
      {
        AppendTunnelRunner(
          snapshot,
          pointsX[point],
          pointsY[point],
          random,
          -2.0,
          profile,
          ref state,
          tileCommands,
          liquidCommands);
        AppendTunnelRunner(
          snapshot,
          pointsX[point],
          pointsY[point],
          random,
          2.0,
          profile,
          ref state,
          tileCommands,
          liquidCommands);
      }
    }
  }

  private static void AppendTunnelRunner(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    LegacyPassRandomState random,
    double speedX,
    LegacyTerrainRuntimeProfile profile,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> tileCommands,
    List<LiquidChangeCommand> liquidCommands)
  {
    LegacyTileRunnerPassInput recipe = new(
      "Tunnels",
      "surface-tunnel",
      0,
      true,
      5,
      8,
      6,
      9,
      "0..worldSurfaceLow",
      false,
      false,
      true);
    LegacyTileRunnerRequest request = new(
      x,
      y,
      random.Next(recipe.MinimumStrength, recipe.MaximumStrengthExclusive),
      random.Next(recipe.MinimumSteps, recipe.MaximumStepsExclusive),
      recipe.TileType,
      recipe.AddTile,
      speedX,
      -0.3,
      false,
      false,
      -1);
    LegacyTileRunnerTraversal.AppendCommands(
      snapshot,
      new LegacyTileRunnerPassInvocation(recipe, request, x, y, (int)request.Strength, request.Steps, 2),
      random,
      (int)profile.WorldSurface,
      (int)profile.RockLayer,
      ref state,
      tileCommands,
      liquidCommands: null);
  }
}

using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyWebsPass
{
  private static readonly LegacyTileRunnerPassInput Recipe = new(
    "Webs",
    "spider-web",
    51,
    true,
    4,
    11,
    2,
    4,
    "worldSurfaceHigh..maxTilesY-20",
    true,
    true,
    false);

  public static IReadOnlyList<LegacyTileRunnerPassInvocation> CreateInvocations(
    WorldGridSnapshot snapshot,
    LegacyWebsPassDefinition definition,
    LegacyPassRandomState random,
    int worldSurfaceY,
    int worldSurfaceLowY,
    int minimumY,
    int maximumYExclusive,
    int iterationCount,
    IReadOnlyList<LegacyCaveCoordinate>? caveHistory = null)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(definition);
    ArgumentNullException.ThrowIfNull(random);
    if (worldSurfaceY < 0 || worldSurfaceLowY < 0 || worldSurfaceLowY > worldSurfaceY ||
        minimumY < 0 || maximumYExclusive <= minimumY || iterationCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldSurfaceY));
    }

    List<LegacyTileRunnerPassInvocation> invocations = new(iterationCount);
    for (int index = 0; index < iterationCount; index++)
    {
      int initialX = random.Next(definition.MinimumXInset, snapshot.Metadata.Width - definition.MinimumXInset);
      int initialY = random.Next(minimumY, maximumYExclusive);
      if (caveHistory is not null && index < caveHistory.Count)
      {
        initialX = caveHistory[index].X;
        initialY = caveHistory[index].Y;
      }
      if (!LegacyWebsCandidateSelector.TrySelect(
            snapshot,
            random,
            initialX,
            initialY,
            worldSurfaceY,
            worldSurfaceLowY,
            out LegacyWebsCandidate candidate))
      {
        continue;
      }

      int strength = random.Next(definition.MinimumStrength, definition.MaximumStrengthExclusive);
      int steps = random.Next(definition.MinimumSteps, definition.MaximumStepsExclusive);
      LegacyTileRunnerRequest request = new(
        candidate.X,
        candidate.Y,
        strength,
        steps,
        definition.TileType,
        addTile: true,
        candidate.Direction,
        definition.SpeedY,
        noYChange: false,
        overwrite: false,
        ignoreTileType: -1);
      invocations.Add(new LegacyTileRunnerPassInvocation(
        Recipe,
        request,
        initialX,
        initialY,
        strength,
        steps,
        RandomDrawCount: 5));
    }

    return invocations.AsReadOnly();
  }
}

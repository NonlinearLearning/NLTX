using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyUnderworldLowerPass
{
  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    LegacyPassRandomState random,
    int worldSurfaceY,
    int rockLayerY,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands,
    bool isDrunkWorld = false,
    bool isRemixWorld = false,
    bool isSkyblockWorld = false)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    if (isSkyblockWorld)
    {
      return;
    }

    IReadOnlyList<LegacyTileRunnerRequest> requests = LegacyUnderworldLowerScheduler.CreateRequests(
      snapshot.Metadata.Width,
      snapshot.Metadata.Height,
      random,
      isDrunkWorld,
      isRemixWorld,
      isSkyblockWorld);
    Dictionary<(int X, int Y), WorldTile> projectedTiles = new();
    foreach (LegacyTileRunnerRequest request in requests)
    {
      LegacyTileRunnerPassInput recipe = new(
        "Underworld",
        request.TileType == 58 ? "obsidian" : "lower-evil",
        request.TileType,
        request.AddTile,
        1,
        2,
        1,
        2,
        "height-180..height",
        false,
        false,
        false);
      LegacyTileRunnerPassInvocation invocation = new(
        recipe,
        request,
        request.X,
        request.Y,
        (int)request.Strength,
        request.Steps,
        RandomDrawCount: 0);
      LegacyTileRunnerTraversal.AppendCommands(
        snapshot,
        invocation,
        random,
        worldSurfaceY,
        rockLayerY,
        ref state,
        commands,
        projectedTiles: projectedTiles);
    }
  }
}

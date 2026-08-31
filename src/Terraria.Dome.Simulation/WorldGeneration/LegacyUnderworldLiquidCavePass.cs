using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyUnderworldLiquidCavePass
{
  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    LegacyPassRandomState random,
    int worldSurfaceY,
    int rockLayerY,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> tileCommands,
    bool isDrunkWorld = false,
    bool isRemixWorld = false,
    bool isSkyblockWorld = false)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(tileCommands);
    if (isSkyblockWorld)
    {
      return;
    }

    IReadOnlyList<LegacyTileRunnerRequest> requests =
      LegacyUnderworldLiquidCaveScheduler.CreateRequests(
        snapshot,
        random,
        isDrunkWorld,
        isRemixWorld,
        isSkyblockWorld);
    Dictionary<(int X, int Y), WorldTile> projectedTiles = new();
    foreach (LegacyTileRunnerRequest request in requests)
    {
      LegacyTileRunnerPassInvocation invocation = CreateInvocation(request);
      LegacyTileRunnerTraversal.AppendCommands(
        snapshot,
        invocation,
        random,
        worldSurfaceY,
        rockLayerY,
        ref state,
        tileCommands,
        projectedTiles: projectedTiles);
    }
  }

  private static LegacyTileRunnerPassInvocation CreateInvocation(LegacyTileRunnerRequest request)
  {
    LegacyTileRunnerPassInput recipe = new(
      "Underworld",
      request.TileType == 57 ? "surface-ash" : "evil-cave",
      request.TileType,
      request.AddTile,
      1,
      2,
      1,
      2,
      "underworld",
      false,
      false,
      false);
    return new LegacyTileRunnerPassInvocation(
      recipe,
      request,
      request.X,
      request.Y,
      (int)request.Strength,
      request.Steps,
      RandomDrawCount: 0);
  }
}

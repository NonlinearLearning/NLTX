using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyGemsPass
{
  private const int FirstGemTileType = 63;
  private const int LastGemTileType = 68;
  private const int RetryCount = 3;
  private const int StoneTileType = 1;

  public static IReadOnlyList<LegacyTileRunnerRequest> CreateRequests(
    WorldGridSnapshot snapshot,
    int worldSurfaceY,
    LegacyPassRandomState random,
    bool isSkyblockWorld = false)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(random);
    if (worldSurfaceY < 0 || worldSurfaceY >= snapshot.Metadata.Height)
    {
      throw new ArgumentOutOfRangeException(nameof(worldSurfaceY));
    }

    if (isSkyblockWorld)
    {
      return Array.Empty<LegacyTileRunnerRequest>();
    }

    List<LegacyTileRunnerRequest> requests = new();
    for (int tileType = FirstGemTileType; tileType <= LastGemTileType; tileType++)
    {
      int count = CalculateInvocationCount(snapshot.Metadata.Width, tileType);
      for (int index = 0; index < count; index++)
      {
        int retriesRemaining = RetryCount;
        int x = 0;
        int y = 0;
        do
        {
          x = random.Next(snapshot.Metadata.Width);
          y = random.Next(worldSurfaceY, snapshot.Metadata.Height);
        }
        while ((!snapshot.GetTile(x, y).IsActive || snapshot.GetTile(x, y).Type != StoneTileType) &&
               --retriesRemaining > 0);
        if (retriesRemaining == 0)
        {
          continue;
        }

        requests.Add(new LegacyTileRunnerRequest(
          x,
          y,
          random.Next(2, 6),
          random.Next(3, 7),
          tileType,
          addTile: true,
          speedX: 0.0,
          speedY: 0.0,
          noYChange: false,
          overwrite: true,
          ignoreTileType: -1));
      }
    }

    return requests.AsReadOnly();
  }

  public static int CalculateInvocationCount(int width, int tileType)
  {
    if (width <= 0 || tileType < FirstGemTileType || tileType > LastGemTileType)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    double multiplier = tileType switch
    {
      63 => 0.3,
      64 => 0.1,
      65 => 0.25,
      66 => 0.45,
      67 => 0.5,
      68 => 0.05,
      _ => throw new InvalidOperationException("Gem tile type was invalid.")
    };
    return (int)(width * multiplier * 0.2);
  }

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    int worldSurfaceY,
    int rockLayerY,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands,
    bool isSkyblockWorld = false)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    IReadOnlyList<LegacyTileRunnerRequest> requests = CreateRequests(
      snapshot,
      worldSurfaceY,
      random,
      isSkyblockWorld);
    Dictionary<(int X, int Y), WorldTile> projectedTiles = new();
    foreach (LegacyTileRunnerRequest request in requests)
    {
      LegacyTileRunnerPassInput recipe = new(
        "Gems",
        "gem",
        request.TileType,
        request.AddTile,
        2,
        6,
        3,
        7,
        "worldSurface..maxTilesY",
        true,
        true,
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

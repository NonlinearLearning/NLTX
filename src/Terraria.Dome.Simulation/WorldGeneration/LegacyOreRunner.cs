using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyOreRunnerArea(int X, int Y, int Width, int Height)
{
  public bool Contains(int x, int y)
  {
    return x >= X && y >= Y && x < X + Width && y < Y + Height;
  }
}

public sealed record LegacyOreRunnerRequest(
  int X,
  int Y,
  double Strength,
  int Steps,
  int TileType = -1,
  int WallType = -1,
  LegacyOreRunnerArea? StayInArea = null,
  int OnlyReplaceTileType = -1,
  int OnlyReplaceWallType = -1,
  IReadOnlySet<ushort>? MossTileTypes = null,
  bool IsNotTheBeesWorld = false);

public static class LegacyOreRunner
{
  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    LegacyOreRunnerRequest request,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(request);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    if (request.X < 0 || request.Y < 0 || !double.IsFinite(request.Strength) ||
        request.Strength <= 0 || request.Steps <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(request));
    }

    double remainingSteps = request.Steps;
    double centerX = request.X;
    double centerY = request.Y;
    double driftX = random.Next(-10, 11) * 0.1;
    double driftY = random.Next(-10, 11) * 0.1;
    while (remainingSteps > 0)
    {
      if (centerY < 0 && request.TileType == 59)
      {
        break;
      }

      double radius = request.Strength * (remainingSteps / request.Steps);
      remainingSteps--;
      int minimumX = Math.Max(0, (int)(centerX - radius * 0.5));
      int maximumX = Math.Min(snapshot.Metadata.Width, (int)(centerX + radius * 0.5));
      int minimumY = Math.Max(0, (int)(centerY - radius * 0.5));
      int maximumY = Math.Min(snapshot.Metadata.Height, (int)(centerY + radius * 0.5));
      for (int x = minimumX; x < maximumX; x++)
      {
        for (int y = minimumY; y < maximumY; y++)
        {
          double threshold = request.Strength * 0.5 * (1.0 + random.Next(-10, 11) * 0.015);
          if (Math.Abs(x - centerX) + Math.Abs(y - centerY) >= threshold ||
              (request.StayInArea is { } area && !area.Contains(x, y)))
          {
            continue;
          }

          WorldTile tile = snapshot.GetTile(x, y);
          if (request.TileType >= 0 && tile.IsActive &&
              (request.OnlyReplaceTileType < 0 || tile.Type == request.OnlyReplaceTileType) &&
              CanReplaceTile(
                tile,
                request.MossTileTypes ?? MossTileTypeRegistry.TileTypes,
                request.IsNotTheBeesWorld))
          {
            commands.Add(new TileChangeCommand(
              state.ReserveSequence(), x, y, TileChangeKind.UpdateTileType,
              checked((ushort)request.TileType), Source: "worldgen.ore.OreRunner"));
          }

          if (request.WallType >= 0 &&
              (request.OnlyReplaceWallType < 0 || tile.WallType == request.OnlyReplaceWallType))
          {
            commands.Add(new TileChangeCommand(
              state.ReserveSequence(), x, y, TileChangeKind.SetWall, 0,
              WallType: checked((ushort)request.WallType), Source: "worldgen.ore.OreRunner"));
          }
        }
      }

      centerX += driftX;
      centerY += driftY;
      driftX = Math.Clamp(driftX + random.Next(-10, 11) * 0.05, -1.0, 1.0);
    }
  }

  private static bool CanReplaceTile(
    WorldTile tile,
    IReadOnlySet<ushort>? mossTileTypes,
    bool isNotTheBeesWorld)
  {
    if (mossTileTypes?.Contains(tile.Type) == true)
    {
      return true;
    }

    if (tile.Type == 230)
    {
      return isNotTheBeesWorld;
    }

    if (tile.Type == 225)
    {
      return tile.WallType != 108;
    }

    return tile.Type is 0 or 1 or 23 or 25 or 40 or 53 or 57 or 59 or 60 or 70 or 109 or 112 or 116 or
      117 or 147 or 161 or 163 or 164 or 199 or 200 or 203 or 234 or 396 or 397 or 398 or 399 or 400 or
      401 or 402 or 403;
  }
}

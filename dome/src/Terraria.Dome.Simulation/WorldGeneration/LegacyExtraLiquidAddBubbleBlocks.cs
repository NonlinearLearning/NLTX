using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyExtraLiquidAddBubbleBlocks
{
  private const int BorderMargin = 100;
  private const int BubbleTileType = 379;
  private const int MaximumRadius = 30;
  private const int MinimumRadius = 10;
  private const int SpawnRadiusAdjustment = 15;
  private const int SpawnRadiusMinimum = 10;
  private const int SpawnRadiusMaximum = 30;
  private const int AttemptMultiplier = 20;
  private const ushort HoneyWallType = 86;
  private const ushort LavaWallType = 62;
  private const string Source = "worldgen.secretseed.ExtraLiquid.addBubbleBlocks";

  public static IReadOnlyList<LegacyExtraLiquidBubbleSquare> AppendCommands(
    WorldGridSnapshot snapshot,
    int worldSurfaceY,
    int underworldLayerY,
    bool isRemixWorld,
    bool isSkyblockWorld,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> tileCommands,
    List<LiquidChangeCommand> liquidCommands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(tileCommands);
    ArgumentNullException.ThrowIfNull(liquidCommands);
    if (worldSurfaceY < 0 || worldSurfaceY >= snapshot.Metadata.Height ||
        underworldLayerY < 0 || underworldLayerY >= snapshot.Metadata.Height)
    {
      throw new ArgumentOutOfRangeException(nameof(worldSurfaceY));
    }

    if (isSkyblockWorld)
    {
      return Array.Empty<LegacyExtraLiquidBubbleSquare>();
    }

    List<LegacyExtraLiquidBubbleSquare> bubbles = new();
    int attempts = checked(snapshot.Metadata.Width * AttemptMultiplier);
    for (int attempt = 0; attempt < attempts; attempt++)
    {
      int x = random.Next(BorderMargin, snapshot.Metadata.Width - BorderMargin);
      int y = random.Next(BorderMargin, snapshot.Metadata.Height - BorderMargin);
      int radius = IsConsideredSpawnArea(x, y, worldSurfaceY, underworldLayerY, isRemixWorld)
        ? random.Next(SpawnRadiusMinimum, SpawnRadiusMaximum + 1)
        : random.Next(MinimumRadius, MaximumRadius + 1);
      if (!IsSquareInside(snapshot.Metadata, x, y, radius) ||
          IsTileNearby(snapshot, x, y, BubbleTileType, radius + SpawnRadiusAdjustment))
      {
        continue;
      }

      if (!IsSuitableBubble(snapshot, x, y, radius))
      {
        continue;
      }

      AppendBubbleCommands(snapshot, x, y, radius, ref state, tileCommands, liquidCommands);
      bubbles.Add(new LegacyExtraLiquidBubbleSquare(x, y, radius));
    }

    for (int x = 15; x < snapshot.Metadata.Width - 15; x++)
    {
      for (int y = 15; y < snapshot.Metadata.Height - 15; y++)
      {
        WorldTile tile = snapshot.GetTile(x, y);
        if (IsSolid(tile))
        {
          liquidCommands.Add(new LiquidChangeCommand(
            state.ReserveSequence(), x, y, 0, tile.LiquidType, Source: Source));
        }
      }
    }

    return bubbles;
  }

  private static bool IsSuitableBubble(WorldGridSnapshot snapshot, int x, int y, int radius)
  {
    int solidBorderCount = 0;
    double density = 0;
    int side = radius * 2 + 1;
    for (int currentX = x - radius; currentX <= x + radius; currentX++)
    {
      for (int currentY = y - radius; currentY <= y + radius; currentY++)
      {
        WorldTile tile = snapshot.GetTile(currentX, currentY);
        bool border = currentX == x - radius || currentX == x + radius ||
          currentY == y - radius || currentY == y + radius;
        if (border && (IsSolid(tile) || tile.LiquidAmount > 0))
        {
          solidBorderCount++;
        }

        if (border && tile.IsActive && (!IsSolid(tile) || tile.Type == 10 || tile.Type == 192 ||
            tile.Type == 384 || tile.Type == 189 || tile.Type == 196 || tile.Type == 460 ||
            tile.Type == 48 || tile.Type == 232))
        {
          return false;
        }

        if (IsSolid(tile))
        {
          density += 0.334;
        }
        else if (tile.LiquidAmount > 0)
        {
          density += 1;
        }

        if (tile.WallType is HoneyWallType or LavaWallType)
        {
          return false;
        }
      }
    }

    return density >= side * side / 2.0 && solidBorderCount >= side / 3;
  }

  private static void AppendBubbleCommands(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    int radius,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> tileCommands,
    List<LiquidChangeCommand> liquidCommands)
  {
    for (int currentX = x - radius; currentX <= x + radius; currentX++)
    {
      for (int currentY = y - radius; currentY <= y + radius; currentY++)
      {
        WorldTile tile = snapshot.GetTile(currentX, currentY);
        bool border = currentX == x - radius || currentX == x + radius ||
          currentY == y - radius || currentY == y + radius;
        if (border && !tile.IsActive)
        {
          tileCommands.Add(new TileChangeCommand(
            state.ReserveSequence(), currentX, currentY, TileChangeKind.PlaceTile,
            BubbleTileType, IsFullbrightBlock: true, IsHalfBrick: false, Slope: 0,
            Source: Source));
        }

        liquidCommands.Add(new LiquidChangeCommand(
          state.ReserveSequence(), currentX, currentY, 0, tile.LiquidType, Source: Source));
        tileCommands.Add(new TileChangeCommand(
          state.ReserveSequence(), currentX, currentY, TileChangeKind.SetCoating, tile.Type,
          IsFullbrightBlock: true, Source: Source));
      }
    }
  }

  private static bool IsSquareInside(WorldMetadata metadata, int x, int y, int radius)
  {
    return x - radius >= 0 && x + radius < metadata.Width &&
      y - radius >= 0 && y + radius < metadata.Height;
  }

  private static bool IsConsideredSpawnArea(
    int x, int y, int worldSurfaceY, int underworldLayerY, bool isRemixWorld)
  {
    return isRemixWorld
      ? y > worldSurfaceY && y < underworldLayerY
      : y > worldSurfaceY && y < underworldLayerY;
  }

  private static bool IsSolid(WorldTile tile)
  {
    return tile.IsActive && !tile.IsInactive;
  }

  private static bool IsTileNearby(
    WorldGridSnapshot snapshot, int x, int y, ushort tileType, int distance)
  {
    int minX = Math.Max(0, x - distance);
    int maxX = Math.Min(snapshot.Metadata.Width - 1, x + distance);
    int minY = Math.Max(0, y - distance);
    int maxY = Math.Min(snapshot.Metadata.Height - 1, y + distance);
    for (int currentX = minX; currentX <= maxX; currentX++)
    {
      for (int currentY = minY; currentY <= maxY; currentY++)
      {
        if (snapshot.GetTile(currentX, currentY).Type == tileType)
        {
          return true;
        }
      }
    }

    return false;
  }
}

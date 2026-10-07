using System;
using System.Collections.Immutable;

using EntityEcs.Components;
using NSSLC.WorldGeneration;
using Terraria.SpatialSimulation;

namespace Terraria.WorldGeneration.Adapters;

/// <summary>
/// Projects a bounded region of the legacy runtime tile map into spatial
/// collision facts for an owner to include in a collision snapshot.
/// </summary>
/// <remarks>
/// Call after the host initializes tile rules, on the thread that owns
/// <see cref="Main.tile"/>, and capture subject geometry at the same simulation
/// boundary. This adapter only reads legacy tile state; callers retain
/// scheduling and query ownership.
/// </remarks>
public static class LegacySpatialTileSnapshotAdapter
{
  /// <summary>
  /// Captures a complete tile-aligned region. Returns false with an empty
  /// result if the runtime map or any requested tile facts are unavailable.
  /// </summary>
  public static bool TryCapture(
    int leftTile,
    int topTile,
    int width,
    int height,
    out ImmutableArray<SpatialTileSnapshot> tiles)
  {
    tiles = ImmutableArray<SpatialTileSnapshot>.Empty;
    if (leftTile < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(leftTile));
    }

    if (topTile < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(topTile));
    }

    if (width <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    if (height <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(height));
    }

    Tile[,] runtimeTiles = Main.tile;
    bool[] solidTypes = Main.tileSolid;
    bool[] solidTopTypes = Main.tileSolidTop;
    if (runtimeTiles is null || solidTypes is null || solidTopTypes is null)
    {
      return false;
    }

    long rightTileExclusive = (long)leftTile + width;
    long bottomTileExclusive = (long)topTile + height;
    if (rightTileExclusive > runtimeTiles.GetLength(0) ||
      bottomTileExclusive > runtimeTiles.GetLength(1))
    {
      return false;
    }

    long tileCount = (long)width * height;
    if (tileCount > int.MaxValue)
    {
      return false;
    }

    ImmutableArray<SpatialTileSnapshot>.Builder capturedTiles =
      ImmutableArray.CreateBuilder<SpatialTileSnapshot>((int)tileCount);
    for (int x = leftTile; x < rightTileExclusive; x++)
    {
      for (int y = topTile; y < bottomTileExclusive; y++)
      {
        Tile runtimeTile = runtimeTiles[x, y];
        if (runtimeTile is null)
        {
          return false;
        }

        bool isActive = runtimeTile.active();
        bool isInactive = runtimeTile.inActive();
        bool isSolid = false;
        bool isSolidTop = false;
        if (isActive)
        {
          int tileType = runtimeTile.type;
          if ((uint)tileType >= (uint)solidTypes.Length ||
            (uint)tileType >= (uint)solidTopTypes.Length)
          {
            return false;
          }

          isSolid = solidTypes[tileType];
          isSolidTop = solidTopTypes[tileType];
        }

        byte slope = runtimeTile.slope();
        if (slope > 5)
        {
          return false;
        }

        byte liquidAmount = runtimeTile.liquid;
        LiquidKind liquidKind;
        if (!TryGetLiquidKind(runtimeTile.liquidType(), liquidAmount, out liquidKind))
        {
          return false;
        }

        capturedTiles.Add(new SpatialTileSnapshot(
          x,
          y,
          exists: true,
          isActive: isActive,
          blocksMovement: isActive && !isInactive && isSolid && !isSolidTop,
          isSolid: isSolid,
          isSolidTop: isSolidTop,
          isHalfBrick: runtimeTile.halfBrick(),
          slope: slope,
          liquidAmount: liquidAmount,
          liquidKind: liquidKind,
          isInactive: isInactive));
      }
    }

    tiles = capturedTiles.MoveToImmutable();
    return true;
  }

  private static bool TryGetLiquidKind(
    byte legacyLiquidType,
    byte liquidAmount,
    out LiquidKind liquidKind)
  {
    if (liquidAmount == 0)
    {
      liquidKind = LiquidKind.Nano;
      return true;
    }

    switch (legacyLiquidType)
    {
      case 0:
        liquidKind = LiquidKind.Water;
        return true;
      case 1:
        liquidKind = LiquidKind.Lava;
        return true;
      case 2:
        liquidKind = LiquidKind.Honey;
        return true;
      case 3:
        liquidKind = LiquidKind.Shimmer;
        return true;
      default:
        liquidKind = LiquidKind.Nano;
        return false;
    }
  }
}

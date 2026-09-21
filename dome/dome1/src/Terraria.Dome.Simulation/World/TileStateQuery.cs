using System;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldModel;

public static class TileStateQuery
{
  private const ushort DoorTileType = 10;
  private const int PlatformFrameWidth = 18;
  private const ushort SpecialPlatformTileType = 380;

  public static int GetActiveTileType(WorldGridSnapshot snapshot, int x, int y)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    WorldTile tile = snapshot.GetTile(x, y);
    return tile.IsActive ? tile.Type : -1;
  }

  public static bool IsEmpty(WorldGridSnapshot snapshot, int x, int y)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    WorldTile tile = snapshot.GetTile(x, y);
    return !tile.IsActive || tile.IsInactive;
  }

  public static bool IsSolid(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y,
    bool noDoors = false)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    return IsSolid(snapshot.GetTile(x, y), tileDefinitions, noDoors);
  }

  public static bool IsSolid(
    WorldTile tile,
    TileDefinitionRegistry tileDefinitions,
    bool noDoors = false)
  {
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    if (!tile.IsActive || tile.IsInactive || tile.IsHalfBrick || tile.Slope != 0 ||
        !tileDefinitions.TryGet(tile.Type, out TileDefinition definition) ||
        !definition.BlocksLiquid || definition.IsPlatform)
    {
      return false;
    }

    return !noDoors || tile.Type != DoorTileType;
  }

  public static bool IsSolidOrSloped(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    return IsSolidOrSloped(snapshot.GetTile(x, y), tileDefinitions);
  }

  public static bool IsSolidOrSloped(WorldTile tile, TileDefinitionRegistry tileDefinitions)
  {
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    return tile.IsActive && !tile.IsInactive &&
      tileDefinitions.TryGet(tile.Type, out TileDefinition definition) &&
      definition.BlocksLiquid && !definition.IsPlatform;
  }

  public static bool IsSolidWithoutPlatformTop(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    return IsSolidWithoutPlatformTop(snapshot.GetTile(x, y), tileDefinitions);
  }

  public static bool IsSolidWithoutPlatformTop(
    WorldTile tile,
    TileDefinitionRegistry tileDefinitions)
  {
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    if (!tile.IsActive || tile.IsInactive || tile.IsHalfBrick ||
        !tileDefinitions.TryGet(tile.Type, out TileDefinition definition) ||
        !definition.BlocksLiquid)
    {
      return false;
    }

    return tile.Slope == 0 || (definition.IsPlatform && IsTopSlope(tile.Slope));
  }

  public static bool IsSolidAllowingBottomSlope(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      return true;
    }

    return IsSolidAllowingBottomSlope(snapshot.GetTile(x, y), tileDefinitions);
  }

  public static bool IsSolidAllowingBottomSlope(
    WorldTile tile,
    TileDefinitionRegistry tileDefinitions)
  {
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    if (!tile.IsActive || tile.IsInactive || tile.IsHalfBrick ||
        !tileDefinitions.TryGet(tile.Type, out TileDefinition definition) ||
        (!definition.BlocksLiquid && !definition.IsPlatform))
    {
      return false;
    }

    return !IsTopSlope(tile.Slope) ||
      (definition.IsPlatform && IsPlatformProperTopFrame(tile.FrameX));
  }

  public static bool IsPlatformProperTopFrame(short frameX)
  {
    int frameColumn = frameX / PlatformFrameWidth;
    if ((frameColumn < 0 || frameColumn > 7) &&
        (frameColumn < 12 || frameColumn > 16))
    {
      return frameColumn >= 25 && frameColumn <= 26;
    }

    return true;
  }

  public static bool IsSolidWithoutPlatforms(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      return true;
    }

    return IsSolidWithoutPlatforms(snapshot.GetTile(x, y), tileDefinitions);
  }

  public static bool IsSolidWithoutPlatforms(
    WorldTile tile,
    TileDefinitionRegistry tileDefinitions)
  {
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    return tile.IsActive && !tile.IsInactive &&
      tileDefinitions.TryGet(tile.Type, out TileDefinition definition) &&
      !definition.IsPlatform && definition.BlocksLiquid;
  }

  public static bool IsSolidAllowingTopSlope(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    return IsSolidAllowingTopSlope(snapshot.GetTile(x, y), tileDefinitions);
  }

  public static bool IsSolidAllowingTopSlope(
    WorldTile tile,
    TileDefinitionRegistry tileDefinitions)
  {
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    if (!tile.IsActive || tile.IsInactive ||
        !tileDefinitions.TryGet(tile.Type, out TileDefinition definition) ||
        (!definition.BlocksLiquid && tile.Type != SpecialPlatformTileType))
    {
      return false;
    }

    return (!definition.IsPlatform && !IsBottomSlope(tile.Slope)) ||
      (definition.IsPlatform && tile.IsHalfBrick);
  }

  public static bool IsSolidAllowingLeftSlope(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    return IsSolidAllowingLeftSlope(snapshot.GetTile(x, y), tileDefinitions);
  }

  public static bool IsSolidAllowingLeftSlope(
    WorldTile tile,
    TileDefinitionRegistry tileDefinitions)
  {
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    return tile.IsActive && !tile.IsInactive && !tile.IsHalfBrick &&
      tileDefinitions.TryGet(tile.Type, out TileDefinition definition) &&
      definition.BlocksLiquid && !definition.IsPlatform && !IsRightSlope(tile.Slope);
  }

  public static bool IsSolidAllowingRightSlope(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    return IsSolidAllowingRightSlope(snapshot.GetTile(x, y), tileDefinitions);
  }

  public static bool IsSolidAllowingRightSlope(
    WorldTile tile,
    TileDefinitionRegistry tileDefinitions)
  {
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    return tile.IsActive && !tile.IsInactive && !tile.IsHalfBrick &&
      tileDefinitions.TryGet(tile.Type, out TileDefinition definition) &&
      definition.BlocksLiquid && !definition.IsPlatform && !IsLeftSlope(tile.Slope);
  }

  public static bool IsSolidWithLegacyTile3Semantics(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (!IsInsideWithFluff(snapshot, x, y, fluff: 1))
    {
      return false;
    }

    return IsSolidWithLegacyTile3Semantics(snapshot.GetTile(x, y), tileDefinitions);
  }

  public static bool IsSolidWithLegacyTile3Semantics(
    WorldTile tile,
    TileDefinitionRegistry tileDefinitions)
  {
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    return tile.IsActive && !tile.IsInactive &&
      tileDefinitions.TryGet(tile.Type, out TileDefinition definition) &&
      definition.BlocksLiquid && !definition.IsPlatform;
  }

  public static bool CanAttachToTop(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    WorldTile tile = snapshot.GetTile(x, y);
    if (!tile.IsActive || tile.IsInactive || tile.IsHalfBrick ||
        !tileDefinitions.TryGet(tile.Type, out TileDefinition definition) ||
        !definition.BlocksLiquid)
    {
      return false;
    }

    return !IsTopSlope(tile.Slope) ||
      (definition.IsPlatform && IsPlatformProperTopFrame(tile.FrameX));
  }

  public static bool CanAttachToRight(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y)
  {
    return CanAttachToSide(snapshot, tileDefinitions, x, y, IsRightSlope);
  }

  public static bool CanAttachToLeft(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y)
  {
    return CanAttachToSide(snapshot, tileDefinitions, x, y, IsLeftSlope);
  }

  public static bool CanAttachToBottom(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    WorldTile tile = snapshot.GetTile(x, y);
    return tile.IsActive && !tile.IsInactive &&
      tileDefinitions.TryGet(tile.Type, out TileDefinition definition) &&
      definition.BlocksLiquid && !definition.IsPlatform && !definition.IsNoAttach &&
      !IsBottomSlope(tile.Slope);
  }

  private static bool CanAttachToSide(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y,
    Func<byte, bool> isBlockedSlope)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    WorldTile tile = snapshot.GetTile(x, y);
    return tile.IsActive && !tile.IsInactive && !tile.IsHalfBrick &&
      tileDefinitions.TryGet(tile.Type, out TileDefinition definition) &&
      definition.BlocksLiquid && !definition.IsPlatform && !definition.IsNoAttach &&
      !isBlockedSlope(tile.Slope);
  }

  private static bool IsBottomSlope(byte slope)
  {
    return slope is 3 or 4;
  }

  private static bool IsLeftSlope(byte slope)
  {
    return slope is 2 or 4;
  }

  private static bool IsRightSlope(byte slope)
  {
    return slope is 1 or 3;
  }

  private static bool IsTopSlope(byte slope)
  {
    return slope is 1 or 2;
  }

  private static bool IsInsideWithFluff(WorldGridSnapshot snapshot, int x, int y, int fluff)
  {
    return x >= fluff && x < snapshot.Metadata.Width - fluff &&
      y >= fluff && y < snapshot.Metadata.Height - fluff;
  }
}

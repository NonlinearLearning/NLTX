using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TilePileValidationQuery
{
  private const ushort PileTileType = 185;
  private const ushort TwoByOneTileType = 649;

  public static TilePileValidationResult Evaluate(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y,
    IReadOnlySet<ushort> snowTileTypes,
    IReadOnlySet<ushort> iceTileTypes,
    IReadOnlySet<ushort> sandTileTypes,
    IReadOnlySet<ushort> hardenedSandTileTypes,
    IReadOnlySet<ushort> sandstoneTileTypes)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    ArgumentNullException.ThrowIfNull(snowTileTypes);
    ArgumentNullException.ThrowIfNull(iceTileTypes);
    ArgumentNullException.ThrowIfNull(sandTileTypes);
    ArgumentNullException.ThrowIfNull(hardenedSandTileTypes);
    ArgumentNullException.ThrowIfNull(sandstoneTileTypes);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    WorldTile source = snapshot.GetTile(x, y);
    if (!source.IsActive)
    {
      return new TilePileValidationResult(false, false, false);
    }

    if (source.FrameY == 18 || source.Type == TwoByOneTileType)
    {
      return new TilePileValidationResult(false, false, true);
    }

    if (!TileStateQuery.IsSolidAllowingBottomSlope(snapshot, tileDefinitions, x, y + 1))
    {
      return new TilePileValidationResult(false, true, false);
    }

    if (source.Type != PileTileType)
    {
      return new TilePileValidationResult(true, false, false);
    }

    WorldTile support = snapshot.Metadata.IsInside(x, y + 1)
      ? snapshot.GetTile(x, y + 1)
      : default;
    if (!support.IsActive || support.Type >= TileDefinitionRegistry.Version4TileCount)
    {
      return new TilePileValidationResult(true, false, false);
    }

    int frameColumn = source.FrameX / 18;
    return IsValidSupportBand(
      frameColumn,
      support.Type,
      snowTileTypes,
      iceTileTypes,
      sandTileTypes,
      hardenedSandTileTypes,
      sandstoneTileTypes)
      ? new TilePileValidationResult(true, false, false)
      : new TilePileValidationResult(false, true, false);
  }

  private static bool IsValidSupportBand(
    int frameColumn,
    ushort supportType,
    IReadOnlySet<ushort> snowTileTypes,
    IReadOnlySet<ushort> iceTileTypes,
    IReadOnlySet<ushort> sandTileTypes,
    IReadOnlySet<ushort> hardenedSandTileTypes,
    IReadOnlySet<ushort> sandstoneTileTypes)
  {
    if (frameColumn is >= 36 and <= 47)
    {
      return snowTileTypes.Contains(supportType) || iceTileTypes.Contains(supportType) ||
        supportType is 162 or 224;
    }

    if (frameColumn is >= 54 and <= 59 or >= 73 and <= 76)
    {
      return sandTileTypes.Contains(supportType) ||
        hardenedSandTileTypes.Contains(supportType) || sandstoneTileTypes.Contains(supportType);
    }

    return frameColumn is >= 48 and <= 72;
  }
}

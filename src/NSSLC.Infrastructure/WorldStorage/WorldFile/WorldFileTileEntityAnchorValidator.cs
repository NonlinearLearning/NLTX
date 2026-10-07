using Terraria.NonAuthoritative.Persistence;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.WorldStorage;

/// <summary>Validates WorldFile 319 TileEntity anchors against their persisted anchor tiles.</summary>
public sealed class WorldFileTileEntityAnchorValidator : IWorldTileEntityAnchorValidator
{
  private const ushort ActiveTileFlag = 0x20;

  public WorldTileEntityAnchorValidity Validate(
    TileEntitySnapshot entity,
    TileCellState anchorTile)
  {
    if (entity.Type.Value > 7)
    {
      return WorldTileEntityAnchorValidity.Unknown;
    }

    if ((anchorTile.TileHeader & ActiveTileFlag) == 0)
    {
      return WorldTileEntityAnchorValidity.Invalid;
    }

    bool isValid = entity.Type.Value switch
    {
      0 => HasAnchorTile(anchorTile, 378) &&
        anchorTile.FrameY == 0 && anchorTile.FrameX % 36 == 0,
      1 => HasAnchorTile(anchorTile, 395) &&
        anchorTile.FrameY == 0 && anchorTile.FrameX % 36 == 0,
      2 => HasAnchorTile(anchorTile, 423) &&
        anchorTile.FrameY % 18 == 0 && anchorTile.FrameX % 18 == 0,
      3 => HasAnchorTile(anchorTile, 470) &&
        anchorTile.FrameY == 0 && anchorTile.FrameX % 36 == 0,
      4 => HasAnchorTile(anchorTile, 471) &&
        anchorTile.FrameY == 0 && anchorTile.FrameX % 54 == 0,
      5 => HasAnchorTile(anchorTile, 475) &&
        anchorTile.FrameY == 0 && anchorTile.FrameX % 54 == 0,
      6 => HasAnchorTile(anchorTile, 520) && anchorTile.FrameY == 0,
      7 => HasAnchorTile(anchorTile, 597) &&
        anchorTile.FrameY == 0 && anchorTile.FrameX % 54 == 0,
      _ => false,
    };

    return isValid
      ? WorldTileEntityAnchorValidity.Valid
      : WorldTileEntityAnchorValidity.Invalid;
  }

  private static bool HasAnchorTile(TileCellState tile, ushort expectedType)
  {
    return tile.Type == expectedType;
  }
}

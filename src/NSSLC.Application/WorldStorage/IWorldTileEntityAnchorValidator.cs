using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

public interface IWorldTileEntityAnchorValidator
{
  WorldTileEntityAnchorValidity Validate(
    TileEntitySnapshot entity,
    TileCellState anchorTile);
}

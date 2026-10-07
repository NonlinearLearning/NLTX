using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

public interface IWorldTileEntityDecoder {
  IReadOnlyList<TileEntitySnapshot> Decode(WorldFileTileEntitySection section);
}

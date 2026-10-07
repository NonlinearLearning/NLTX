using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

public interface IWorldTilePayloadDecoder {
  TileMapSnapshot Decode(WorldFileTilePayloadSection section);
}

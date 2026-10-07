using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

public sealed record WorldTilesPrepared(TileMapSnapshot Tiles, IReadOnlyList<bool> FrameImportant);

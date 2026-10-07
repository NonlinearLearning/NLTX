using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

[WorldLoadApi("world.tiles.load", OwnerId, WorldFileTilePayloadSection.SectionId,
    319, 319, WorldLoadSectionRequirement.Required, "world.header.load")]
public sealed class WorldTilePayloadLoadApi :
    IWorldLoadApi<LoadedWorldSession, WorldFileTilePayloadSection, WorldTilesPrepared> {
  public const string OwnerId = "world.tiles";
  private readonly IWorldTilePayloadDecoder _decoder;

  public WorldTilePayloadLoadApi(IWorldTilePayloadDecoder decoder) {
    _decoder = decoder ?? throw new ArgumentNullException(nameof(decoder));
  }

  public WorldLoadPrepareResult<WorldTilesPrepared> PrepareLoad(LoadedWorldSession ownerContext,
      WorldLoadSection<WorldFileTilePayloadSection> section) {
    if (!ownerContext.IsFresh || !section.IsPresent) {
      return WorldLoadPrepareResult<WorldTilesPrepared>.Rejected(WorldLoadApiFailure.Create(
          "InvalidTileTarget", "A fresh target and a tile payload are required."));
    }
    TileMapSnapshot tiles = _decoder.Decode(section.Value);
    return WorldLoadPrepareResult<WorldTilesPrepared>.Prepared(new WorldTilesPrepared(
        tiles, Array.AsReadOnly(section.Value.FrameImportant.ToArray())));
  }

  public WorldLoadCommitResult CommitLoad(LoadedWorldSession ownerContext,
      in WorldTilesPrepared preparedData) {
    if (ownerContext.World.Descriptor.SizeX != preparedData.Tiles.Width ||
        ownerContext.World.Descriptor.SizeY != preparedData.Tiles.Height) {
      return WorldLoadCommitResult.Rejected(WorldLoadApiFailure.Create(
          "TileDimensionsMismatch", "The tile dimensions do not match the world header."));
    }
    TileMapRestoreSystem.Apply(ownerContext.Storage.TileMap, preparedData.Tiles);
    ownerContext.FrameImportant = preparedData.FrameImportant;
    ownerContext.RecordCommit(WorldFileTilePayloadSection.SectionId);
    return WorldLoadCommitResult.Committed();
  }

  public void DiscardPrepared(LoadedWorldSession ownerContext, in WorldTilesPrepared preparedData) {
    // The staged immutable buffer is reclaimed when the dispatcher releases its reference.
  }
}

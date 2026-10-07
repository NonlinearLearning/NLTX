using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

[WorldLoadApi("world.tile-entities.load", OwnerId, WorldFileTileEntitySection.SectionId,
    319, 319, WorldLoadSectionRequirement.Optional, "world.tiles.load")]
public sealed class WorldTileEntityLoadApi :
    IWorldLoadApi<LoadedWorldSession, WorldFileTileEntitySection,
        WorldTileEntitiesPrepared> {
  public const string OwnerId = "world.tile-entities";
  private const int WorldEdgeClearance = 1;
  private readonly IWorldTileEntityDecoder _decoder;
  private readonly IWorldTileEntityAnchorValidator _anchorValidator;

  public WorldTileEntityLoadApi(
      IWorldTileEntityDecoder decoder,
      IWorldTileEntityAnchorValidator anchorValidator) {
    _decoder = decoder ?? throw new ArgumentNullException(nameof(decoder));
    _anchorValidator = anchorValidator ??
        throw new ArgumentNullException(nameof(anchorValidator));
  }

  public WorldLoadPrepareResult<WorldTileEntitiesPrepared> PrepareLoad(
      LoadedWorldSession ownerContext, WorldLoadSection<WorldFileTileEntitySection> section) {
    if (!ownerContext.IsFresh) {
      return WorldLoadPrepareResult<WorldTileEntitiesPrepared>.Rejected(
          WorldLoadApiFailure.Create("TargetNotFresh", "Load into a fresh unpublished session."));
    }

    IReadOnlyList<TileEntitySnapshot> entities = section.IsPresent
        ? _decoder.Decode(section.Value)
        : Array.Empty<TileEntitySnapshot>();
    int nextId = section.IsPresent ? section.Value.EntityCount : 0;
    return WorldLoadPrepareResult<WorldTileEntitiesPrepared>.Prepared(
        new WorldTileEntitiesPrepared(entities, nextId));
  }

  public WorldLoadCommitResult CommitLoad(LoadedWorldSession ownerContext,
      in WorldTileEntitiesPrepared preparedData) {
    TileEntityRestoreSystem.Apply(
        ownerContext.Storage.TileEntities,
        RetainValidInWorld(ownerContext, preparedData.Entities),
        preparedData.NextId);
    ownerContext.Storage.TileEntityUpdates.Replace(
        ownerContext.Storage.TileEntities.CreateScheduledIdSnapshot());
    ownerContext.RecordCommit(WorldFileTileEntitySection.SectionId);
    return WorldLoadCommitResult.Committed();
  }

  public void DiscardPrepared(LoadedWorldSession ownerContext,
      in WorldTileEntitiesPrepared preparedData) { }

  // Version4 LoadTileEntities removes out-of-world and invalid-anchor entities.
  private IReadOnlyList<TileEntitySnapshot> RetainValidInWorld(
      LoadedWorldSession ownerContext,
      IReadOnlyList<TileEntitySnapshot> preparedData) {
    int firstAllowedX = WorldEdgeClearance;
    int firstAllowedY = WorldEdgeClearance;
    int endAllowedX = ownerContext.Storage.TileMap.Width - WorldEdgeClearance;
    int endAllowedY = ownerContext.Storage.TileMap.Height - WorldEdgeClearance;
    var retained = new List<TileEntitySnapshot>(preparedData.Count);
    foreach (TileEntitySnapshot entity in preparedData) {
      if (entity.Anchor.X < firstAllowedX || entity.Anchor.X >= endAllowedX ||
          entity.Anchor.Y < firstAllowedY || entity.Anchor.Y >= endAllowedY) {
        continue;
      }

      TileCellState anchorTile = ownerContext.Storage.TileMap.GetTile(
          entity.Anchor.X,
          entity.Anchor.Y);
      WorldTileEntityAnchorValidity validity = _anchorValidator.Validate(entity, anchorTile);
      if (validity is WorldTileEntityAnchorValidity.Valid or
          WorldTileEntityAnchorValidity.Unknown) {
        retained.Add(entity);
      }
      else if (validity != WorldTileEntityAnchorValidity.Invalid) {
        throw new InvalidDataException("The TileEntity anchor validator returned an unknown result.");
      }
    }

    return retained.AsReadOnly();
  }
}

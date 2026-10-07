using Terraria.Items;
using Terraria.NonAuthoritative.Persistence;
using Terraria.NonAuthoritative.WorldStorage;
using Terraria.Relationships;
using Terraria.WorldInteraction.TileEntities;
using Terraria.WorldStorage;
using RuntimeTileEntities = NSSLC.WorldGeneration.GameContent.Tile_Entities;

namespace Terraria.NonAuthoritative.SimulationHost;

internal readonly record struct TileEntityRemovalProbe(
  TileEntityId Id,
  TileCoordinate Anchor,
  TileEntityId TrainingDummyId,
  TileCoordinate TrainingDummyAnchor,
  int InitialNpcCount);

internal static class TileEntityRemovalProbeInstaller
{
  private const ushort ActiveTileFlag = 0x20;
  private const ushort LogicSensorTileType = 423;
  private const short LogicOnFrameX = 18;

  public static TileEntityRemovalProbe Install(
    LoadedWorldSession session,
    RuntimeNpcStore npcs)
  {
    ArgumentNullException.ThrowIfNull(session);
    ArgumentNullException.ThrowIfNull(npcs);
    if (!session.IsPublished)
    {
      throw new InvalidOperationException(
        "The TileEntity removal probe requires a published world.");
    }

    int storeNextId = session.Storage.TileEntities.NextId;
    int runtimeNextId = NSSLC.WorldGeneration.TileEntity.CreatePersistenceSnapshot().NextId;
    if (runtimeNextId != storeNextId)
    {
      throw new InvalidOperationException(
        "The runtime TileEntity identity does not match the published world owner.");
    }

    TileCoordinate anchor = FindAvailableAnchor(session);
    TileCellState tile = session.Storage.TileMap.GetTile(anchor.X, anchor.Y);
    tile.TileHeader = ActiveTileFlag;
    tile.Type = LogicSensorTileType;
    tile.FrameX = LogicOnFrameX;
    tile.FrameY = 0;
    session.Storage.TileMap.CommitTile(anchor.X, anchor.Y, tile);
    PublishTile(session, anchor);

    int runtimeId = NSSLC.WorldGeneration.TileEntity.Register<
      NSSLC.WorldGeneration.TELogicSensor>(anchor.X, anchor.Y, LogicSensorTileType);
    if (runtimeId != storeNextId)
    {
      throw new InvalidOperationException(
        "The runtime TileEntity allocator produced an unexpected identity.");
    }

    var id = new TileEntityId(runtimeId);
    TileCoordinate trainingDummyAnchor = FindAvailableAnchor(session, anchor);
    TileCellState trainingDummyTile = session.Storage.TileMap.GetTile(
      trainingDummyAnchor.X,
      trainingDummyAnchor.Y);
    trainingDummyTile.TileHeader = ActiveTileFlag;
    trainingDummyTile.Type = 378;
    trainingDummyTile.FrameX = 0;
    trainingDummyTile.FrameY = 0;
    session.Storage.TileMap.CommitTile(
      trainingDummyAnchor.X,
      trainingDummyAnchor.Y,
      trainingDummyTile);
    PublishTile(session, trainingDummyAnchor);

    int trainingDummyRuntimeId = NSSLC.WorldGeneration.TileEntity.Register<
      RuntimeTileEntities.TETrainingDummy>(
        trainingDummyAnchor.X,
        trainingDummyAnchor.Y,
        378);
    if (trainingDummyRuntimeId != runtimeId + 1)
    {
      throw new InvalidOperationException(
        "The runtime TrainingDummy allocator produced an unexpected identity.");
    }

    var snapshot = new TileEntitySnapshot(
      id,
      new TileEntityTypeId(2),
      anchor,
      Array.Empty<ItemState>(),
      logicCheck: (byte)LogicCheckType.PlayerAbove,
      logicOn: true);
    var trainingDummySnapshot = new TileEntitySnapshot(
      new TileEntityId(trainingDummyRuntimeId),
      new TileEntityTypeId(0),
      trainingDummyAnchor,
      Array.Empty<ItemState>());
    TileEntitySnapshot[] entities = session.Storage.TileEntities
      .CreateSnapshot()
      .Append(snapshot)
      .Append(trainingDummySnapshot)
      .ToArray();
    session.Storage.TileEntities.CommitRuntimeSnapshot(
      new TileEntityStoreSnapshot(entities, checked(trainingDummyRuntimeId + 1)));
    RequireSharedRoot(session.Storage.TileEntities, id, anchor);
    RequireSharedRoot(
      session.Storage.TileEntities,
      trainingDummySnapshot.Id,
      trainingDummyAnchor);
    return new TileEntityRemovalProbe(
      id,
      anchor,
      trainingDummySnapshot.Id,
      trainingDummyAnchor,
      npcs.ActiveCount);
  }

  private static void RequireSharedRoot(
    TileEntityStore store,
    TileEntityId id,
    TileCoordinate anchor)
  {
    if (!store.TryGetEntityReference(id, out EntityReference byId) ||
        !store.TryGetEntityReferenceByAnchor(anchor, out EntityReference byAnchor) ||
        byId != byAnchor)
    {
      throw new InvalidOperationException(
        "The TileEntity ID and anchor indexes do not resolve to the same runtime root.");
    }
  }

  public static void InvalidateAnchor(
    LoadedWorldSession session,
    TileEntityRemovalProbe probe)
  {
    ArgumentNullException.ThrowIfNull(session);
    RequireLogicSensorTransition(session, probe);
    InvalidateAnchor(session, probe.Id, probe.Anchor);
  }

  private static void RequireLogicSensorTransition(
    LoadedWorldSession session,
    TileEntityRemovalProbe probe)
  {
    if (!session.Storage.TileEntities.TryGetRuntimeState(
          probe.Id,
          out TileEntityRuntimeState state) ||
        state.LogicCheck != (byte)LogicCheckType.PlayerAbove ||
        state.LogicOn)
    {
      throw new InvalidOperationException(
        "The PlayerAbove Logic Sensor did not transition from on to off during its first tick.");
    }

    TileCellState tile = session.Storage.TileMap.GetTile(probe.Anchor.X, probe.Anchor.Y);
    if (tile.FrameX != 0)
    {
      throw new InvalidOperationException(
        "The transitioned Logic Sensor frame was not projected back to the tile map.");
    }
  }

  public static void InvalidateTrainingDummyAnchor(
    LoadedWorldSession session,
    TileEntityRemovalProbe probe)
  {
    InvalidateAnchor(session, probe.TrainingDummyId, probe.TrainingDummyAnchor);
  }

  private static void InvalidateAnchor(
    LoadedWorldSession session,
    TileEntityId id,
    TileCoordinate anchor)
  {
    ArgumentNullException.ThrowIfNull(session);
    if (!session.Storage.TileEntities.TryGetSnapshot(id, out TileEntitySnapshot? entity) ||
        entity is null || entity.Anchor != anchor)
    {
      throw new InvalidOperationException(
        "The TileEntity probe disappeared before its anchor was invalidated.");
    }

    TileCellState tile = session.Storage.TileMap.GetTile(anchor.X, anchor.Y);
    tile.TileHeader = 0;
    tile.Type = 0;
    tile.FrameX = 0;
    tile.FrameY = 0;
    session.Storage.TileMap.CommitTile(anchor.X, anchor.Y, tile);
    PublishTile(session, anchor);
  }

  private static TileCoordinate FindAvailableAnchor(
    LoadedWorldSession session,
    TileCoordinate? additionalOccupiedAnchor = null)
  {
    var occupiedAnchors = session.Storage.TileEntities.CreateSnapshot()
      .Select(static entity => entity.Anchor)
      .ToHashSet();
    if (additionalOccupiedAnchor is TileCoordinate additionalAnchor)
    {
      occupiedAnchors.Add(additionalAnchor);
    }
    int preferredX = session.World.Descriptor.SpawnTileX;
    int preferredY = session.World.Descriptor.SpawnTileY;
    for (int distance = 0; distance < session.Storage.TileMap.Width; distance++)
    {
      int x = (preferredX + distance) % session.Storage.TileMap.Width;
      if (x < 1 || x >= session.Storage.TileMap.Width - 1)
      {
        continue;
      }

      int y = Math.Clamp(preferredY + 2, 1, session.Storage.TileMap.Height - 2);
      var anchor = new TileCoordinate(x, y);
      if (!occupiedAnchors.Contains(anchor))
      {
        return anchor;
      }
    }

    throw new InvalidOperationException("The world has no available TileEntity probe anchor.");
  }

  private static void PublishTile(LoadedWorldSession session, TileCoordinate anchor)
  {
    WorldStorageOperationResult projection = LegacyWorldTileMapProjection.PublishCommittedTile(
      session,
      anchor.X,
      anchor.Y);
    if (!projection.Succeeded)
    {
      throw new InvalidOperationException(
        $"The TileEntity probe anchor could not be projected: " +
        $"{projection.Failure.Kind} {projection.Failure.Detail}");
    }
  }
}

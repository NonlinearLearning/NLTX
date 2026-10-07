using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NSSLC.WorldGeneration;
using Terraria.NonAuthoritative.Persistence;
using Terraria.WorldSession.Components;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.WorldStorage;

/// <summary>
/// Stages a committed domain tile snapshot in the legacy runtime representation.
/// </summary>
/// <remarks>
/// <see cref="CreateRuntimeTiles"/> returns a detached array that a host may hold until its commit.
/// <see cref="PublishRuntimeTileMap"/> assigns the core Tile and spatial fields after staging
/// succeeds. WorldFile does not persist liquid work-queue state, so only persisted tile fields are
/// copied.
/// </remarks>
public static class LegacyWorldTileMapProjection
{
  private static LoadedWorldSession? _observedSession;
  private static Action<int, int>? _mutationObserver;

  /// <summary>
  /// Publishes a staged tile buffer and its persisted world-space metadata to legacy Main.
  /// </summary>
  /// <remarks>
  /// This is one step of host publication, not a complete world-session projection. Call it on
  /// the runtime's owning thread while the load gate is raised, and compose it with the host's
  /// remaining world-state commits before reporting publication success.
  /// </remarks>
  public static WorldStorageOperationResult PublishRuntimeTileMap(
    LoadedWorldSession session,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(session);
    return PublishRuntimeTileMap(session, cancellationToken, allowPublished: false);
  }

  internal static WorldStorageOperationResult ReprojectPublishedRuntimeTileMap(
    LoadedWorldSession session)
  {
    ArgumentNullException.ThrowIfNull(session);
    if (!IsActivePublishedSession(session))
    {
      return Invalid("Only the active published session can restore the runtime tile map.");
    }

    return PublishRuntimeTileMap(session, CancellationToken.None, allowPublished: true);
  }

  private static WorldStorageOperationResult PublishRuntimeTileMap(
    LoadedWorldSession session,
    CancellationToken cancellationToken,
    bool allowPublished)
  {
    if (allowPublished
          ? !IsActivePublishedSession(session)
          : !IsPendingPublicationSession(session))
    {
      return Invalid("The session is not eligible for runtime tile-map projection.");
    }

    try
    {
      Tile[,] runtimeTiles = CreateRuntimeTiles(session, cancellationToken);
      cancellationToken.ThrowIfCancellationRequested();

      WorldDescriptorState descriptor = session.World.Descriptor;
      _observedSession = null;
      _mutationObserver = null;
      Main.tile = runtimeTiles;
      Main.maxTilesX = descriptor.SizeX;
      Main.maxTilesY = descriptor.SizeY;
      Main.maxSectionsX = (int)(((long)descriptor.SizeX + 199) / 200);
      Main.maxSectionsY = (int)(((long)descriptor.SizeY + 149) / 150);
      Main.leftWorld = (float)descriptor.LeftWorld;
      Main.rightWorld = (float)descriptor.RightWorld;
      Main.topWorld = (float)descriptor.TopWorld;
      Main.bottomWorld = (float)descriptor.BottomWorld;
      // WorldFile stores the height; the legacy runtime derives the underworld from it.
      Main.UnderworldLayer = descriptor.SizeY - 200;
      Main.worldSurface = descriptor.SurfaceLayer;
      Main.rockLayer = descriptor.RockLayer;
      Main.spawnTileX = descriptor.SpawnTileX;
      Main.spawnTileY = descriptor.SpawnTileY;
      Main.dungeonX = descriptor.DungeonTileX;
      Main.dungeonY = descriptor.DungeonTileY;
      Main.isThereAWorldSurface = descriptor.HasSurface;
      Main.worldName = descriptor.Name;
      return WorldStorageOperationResult.Success;
    }
    catch (OperationCanceledException exception)
    {
      return WorldStorageOperationResult.Failed(
        WorldStorageFailure.Create(WorldStorageFailureKind.Canceled, exception.Message));
    }
    catch (InvalidDataException exception)
    {
      return Invalid(exception.Message);
    }
    catch (InvalidOperationException exception)
    {
      return Invalid(exception.Message);
    }
  }

  public static Tile[,] CreateRuntimeTiles(
    LoadedWorldSession session,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(session);
    if (!session.IsComplete)
    {
      throw new InvalidOperationException(
        "Only a complete loaded world session can be projected to runtime tiles.");
    }

    TileMapStore source = session.Storage.TileMap;
    int width = session.World.Descriptor.SizeX;
    int height = session.World.Descriptor.SizeY;
    if (width <= 0 || height <= 0 || source.Width != width || source.Height != height)
    {
      throw new InvalidDataException(
        "The loaded tile map dimensions do not match the world descriptor.");
    }

    IReadOnlyList<bool> frameImportant = session.FrameImportant;
    if (frameImportant.Count == 0)
    {
      throw new InvalidDataException("The loaded world has no tile frame-importance table.");
    }

    cancellationToken.ThrowIfCancellationRequested();
    var runtimeTiles = new Tile[width, height];
    for (int x = 0; x < width; x++)
    {
      cancellationToken.ThrowIfCancellationRequested();
      for (int y = 0; y < height; y++)
      {
        TileCellState cell = source.GetTile(x, y);
        runtimeTiles[x, y] = CreateRuntimeTile(cell, frameImportant);
      }
    }

    cancellationToken.ThrowIfCancellationRequested();
    return runtimeTiles;
  }

  /// <summary>Publishes one owner-committed tile change to the legacy collision view.</summary>
  public static WorldStorageOperationResult PublishCommittedTile(
    LoadedWorldSession session,
    int x,
    int y)
  {
    return PublishCommittedTiles(session, new[] { new TileCoordinate(x, y) });
  }

  /// <summary>Projects multiple owner-committed tiles after validating the complete change set.</summary>
  public static WorldStorageOperationResult PublishCommittedTiles(
    LoadedWorldSession session,
    IReadOnlyCollection<TileCoordinate> changedCoordinates)
  {
    ArgumentNullException.ThrowIfNull(session);
    ArgumentNullException.ThrowIfNull(changedCoordinates);
    if (!IsActivePublishedSession(session))
    {
      return Invalid("A tile can only be projected for the active published session.");
    }

    return PublishCommittedTilesCore(session, changedCoordinates, candidate: false);
  }

  private static WorldStorageOperationResult PublishCommittedTilesCore(
    LoadedWorldSession session,
    IReadOnlyCollection<TileCoordinate> changedCoordinates,
    bool candidate)
  {
    if (!IsExpectedProjectionSession(session, candidate))
    {
      return Invalid("The session is not eligible for tile projection at this lifecycle stage.");
    }
    if (changedCoordinates.Count == 0)
    {
      return WorldStorageOperationResult.Success;
    }

    List<TileCoordinate> orderedCoordinates = OrderUniqueCoordinates(changedCoordinates);
    var stagedTiles = new List<(TileCoordinate Coordinate, Tile Tile)>(orderedCoordinates.Count);
    try
    {
      foreach (TileCoordinate coordinate in orderedCoordinates)
      {
        if (!HasRuntimeTileMap(session, coordinate.X, coordinate.Y))
        {
          return Invalid("A committed tile coordinate is outside the active runtime map.");
        }

        TileCellState cell = session.Storage.TileMap.GetTile(coordinate.X, coordinate.Y);
        stagedTiles.Add((coordinate, CreateRuntimeTile(cell, session.FrameImportant)));
      }
    }
    catch (InvalidDataException exception)
    {
      return Invalid(exception.Message);
    }

    if (!IsExpectedProjectionSession(session, candidate))
    {
      return Invalid("The session changed before committed tiles could be projected.");
    }

    foreach ((TileCoordinate coordinate, Tile tile) in stagedTiles)
    {
      if (ReferenceEquals(_observedSession, session))
      {
        tile.BindMutationObserver(
          coordinate.X,
          coordinate.Y,
          _mutationObserver);
      }
      Main.tile[coordinate.X, coordinate.Y] = tile;
    }

    return WorldStorageOperationResult.Success;
  }

  /// <summary>
  /// Tracks legacy tile mutations for the active session so an owner can commit only changed
  /// coordinates after a legacy runtime phase.
  /// </summary>
  public static WorldStorageOperationResult BindMutationObserver(
    LoadedWorldSession session,
    Action<int, int> observer)
  {
    ArgumentNullException.ThrowIfNull(session);
    ArgumentNullException.ThrowIfNull(observer);
    if (!IsActivePublishedSession(session))
    {
      return Invalid("Tile mutation tracking requires the active published session.");
    }
    return BindRuntimeMutationObserver(session, observer);
  }

  /// <summary>
  /// Tracks tile writes while a complete candidate is being published, before it becomes active.
  /// </summary>
  public static WorldStorageOperationResult BindCandidateMutationObserver(
    LoadedWorldSession session,
    Action<int, int> observer)
  {
    ArgumentNullException.ThrowIfNull(session);
    ArgumentNullException.ThrowIfNull(observer);
    if (!IsPendingPublicationSession(session))
    {
      return Invalid("Candidate tile tracking requires a complete session pending publication.");
    }
    return BindRuntimeMutationObserver(session, observer);
  }

  private static WorldStorageOperationResult BindRuntimeMutationObserver(
    LoadedWorldSession session,
    Action<int, int> observer)
  {
    if (!HasRuntimeTileMap(session, 0, 0) ||
        Main.tile.GetLength(0) != session.Storage.TileMap.Width ||
        Main.tile.GetLength(1) != session.Storage.TileMap.Height)
    {
      return Invalid("The active session does not have a matching runtime tile map.");
    }

    _observedSession = session;
    _mutationObserver = observer;
    for (int x = 0; x < Main.tile.GetLength(0); x++)
    {
      for (int y = 0; y < Main.tile.GetLength(1); y++)
      {
        Main.tile[x, y].BindMutationObserver(x, y, observer);
      }
    }

    return WorldStorageOperationResult.Success;
  }

  /// <summary>Removes a session's temporary tile observer after its tracked phase completes.</summary>
  public static WorldStorageOperationResult UnbindMutationObserver(LoadedWorldSession session)
  {
    ArgumentNullException.ThrowIfNull(session);
    if (!ReferenceEquals(_observedSession, session))
    {
      return WorldStorageOperationResult.Success;
    }
    if (Main.tile is null ||
        Main.tile.GetLength(0) != session.Storage.TileMap.Width ||
        Main.tile.GetLength(1) != session.Storage.TileMap.Height)
    {
      return Invalid("The observed session no longer has a matching runtime tile map.");
    }

    for (int x = 0; x < Main.tile.GetLength(0); x++)
    {
      for (int y = 0; y < Main.tile.GetLength(1); y++)
      {
        Main.tile[x, y]?.BindMutationObserver(x, y, null);
      }
    }

    _observedSession = null;
    _mutationObserver = null;
    return WorldStorageOperationResult.Success;
  }

  /// <summary>
  /// Copies one legacy tile mutation back into the loaded-world owner before it is saved.
  /// Call this immediately after the legacy algorithm mutates <see cref="Main.tile"/>.
  /// </summary>
  public static WorldStorageOperationResult CommitLegacyTileMutation(
    LoadedWorldSession session,
    int x,
    int y)
  {
    return CommitLegacyTileMutations(
      session,
      new[] { new TileCoordinate(x, y) });
  }

  /// <summary>
  /// Commits explicitly listed legacy tile mutations to the loaded-world owner, then projects
  /// the committed values back into the runtime map.
  /// </summary>
  /// <remarks>
  /// The caller must provide every changed coordinate. This method deliberately does not scan
  /// the world map, so it can be used at known legacy mutation boundaries without per-tick cost.
  /// All coordinates and runtime tiles are validated before any owner state is changed.
  /// </remarks>
  public static WorldStorageOperationResult CommitLegacyTileMutations(
    LoadedWorldSession session,
    IReadOnlyCollection<TileCoordinate> changedCoordinates)
  {
    ArgumentNullException.ThrowIfNull(session);
    ArgumentNullException.ThrowIfNull(changedCoordinates);
    if (!IsActivePublishedSession(session))
    {
      return Invalid("A legacy tile mutation requires the active published session.");
    }

    return CommitLegacyTileMutationsCore(session, changedCoordinates, candidate: false);
  }

  /// <summary>
  /// Commits tracked candidate tile writes before publication completes.
  /// </summary>
  public static WorldStorageOperationResult CommitCandidateLegacyTileMutations(
    LoadedWorldSession session,
    IReadOnlyCollection<TileCoordinate> changedCoordinates)
  {
    ArgumentNullException.ThrowIfNull(session);
    ArgumentNullException.ThrowIfNull(changedCoordinates);
    if (!IsPendingPublicationSession(session) || !ReferenceEquals(_observedSession, session))
    {
      return Invalid("Candidate tile changes require the tracked session pending publication.");
    }

    return CommitLegacyTileMutationsCore(session, changedCoordinates, candidate: true);
  }

  private static WorldStorageOperationResult CommitLegacyTileMutationsCore(
    LoadedWorldSession session,
    IReadOnlyCollection<TileCoordinate> changedCoordinates,
    bool candidate)
  {

    if (changedCoordinates.Count == 0)
    {
      return WorldStorageOperationResult.Success;
    }

    List<TileCoordinate> orderedCoordinates = OrderUniqueCoordinates(changedCoordinates);

    var stagedMutations = new List<(TileCoordinate Coordinate, TileCellState Tile)>(
      orderedCoordinates.Count);
    foreach (TileCoordinate coordinate in orderedCoordinates)
    {
      if (!HasRuntimeTileMap(session, coordinate.X, coordinate.Y))
      {
        return Invalid("A changed tile coordinate is outside the active runtime map.");
      }

      Tile? runtimeTile = Main.tile[coordinate.X, coordinate.Y];
      if (runtimeTile is null)
      {
        return Invalid("A legacy runtime tile mutation produced a null tile.");
      }
      if (runtimeTile.active() && runtimeTile.type >= session.FrameImportant.Count)
      {
        return Invalid(
          $"Active tile type {runtimeTile.type} has no frame-importance entry.");
      }

      TileCellState current = session.Storage.TileMap.GetTile(coordinate.X, coordinate.Y);
      bool liquidChanged = runtimeTile.liquid != current.LiquidAmount ||
        (runtimeTile.liquid > 0 && runtimeTile.liquidType() != current.LiquidType);
      var committed = new TileCellState
      {
        FrameX = runtimeTile.frameX,
        FrameY = runtimeTile.frameY,
        Header = runtimeTile.bTileHeader,
        Header2 = runtimeTile.bTileHeader2,
        Header3 = (byte)(runtimeTile.bTileHeader3 & ~0x18),
        IsCheckingLiquid = runtimeTile.checkingLiquid(),
        LastLiquidChangedRevision = liquidChanged
          ? checked(current.LastLiquidChangedRevision + 1)
          : current.LastLiquidChangedRevision,
        LiquidAmount = runtimeTile.liquid,
        LiquidType = runtimeTile.liquid > 0 ? runtimeTile.liquidType() : (byte)0,
        ShouldSkipLiquid = runtimeTile.skipLiquid(),
        TileHeader = runtimeTile.sTileHeader,
        Type = runtimeTile.type,
        Wall = runtimeTile.wall,
      };
      stagedMutations.Add((coordinate, committed));
    }

    foreach ((TileCoordinate coordinate, TileCellState tile) in stagedMutations)
    {
      session.Storage.TileMap.CommitTile(coordinate.X, coordinate.Y, tile);
    }

    if (candidate)
    {
      return PublishCommittedTilesCore(session, orderedCoordinates, candidate: true);
    }

    return PublishCommittedTiles(session, orderedCoordinates);
  }

  private static List<TileCoordinate> OrderUniqueCoordinates(
    IReadOnlyCollection<TileCoordinate> coordinates)
  {
    var orderedCoordinates = new List<TileCoordinate>(coordinates.Count);
    var seenCoordinates = new HashSet<TileCoordinate>();
    foreach (TileCoordinate coordinate in coordinates)
    {
      if (seenCoordinates.Add(coordinate))
      {
        orderedCoordinates.Add(coordinate);
      }
    }
    orderedCoordinates.Sort(static (left, right) =>
    {
      int yComparison = left.Y.CompareTo(right.Y);
      return yComparison != 0 ? yComparison : left.X.CompareTo(right.X);
    });
    return orderedCoordinates;
  }

  private static Tile CreateRuntimeTile(
    TileCellState cell,
    IReadOnlyList<bool> frameImportant)
  {
    bool active = (cell.TileHeader & 0x20) != 0;
    if (active && cell.Type >= frameImportant.Count)
    {
      throw new InvalidDataException(
        $"Active tile type {cell.Type} has no frame-importance entry.");
    }

    var tile = new Tile
    {
      type = cell.Type,
      wall = cell.Wall,
      liquid = cell.LiquidAmount,
      sTileHeader = cell.TileHeader,
      bTileHeader = cell.Header,
      bTileHeader2 = cell.Header2,
      bTileHeader3 = cell.Header3
    };
    tile.checkingLiquid(cell.IsCheckingLiquid);
    tile.skipLiquid(cell.ShouldSkipLiquid);
    if (active && frameImportant[cell.Type])
    {
      tile.frameX = cell.FrameX;
      tile.frameY = cell.FrameY;
    }
    return tile;
  }

  private static bool IsActivePublishedSession(LoadedWorldSession session)
  {
    if (!session.IsComplete || session.IsPublicationUncertain || !session.IsPublished)
    {
      return false;
    }

    if (ReferenceEquals(WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession, session))
    {
      return true;
    }

    // During the load recovery settle phase the candidate has been projected into legacy Main
    // and marked published, but the generated-world owner is exchanged only after settling and
    // finalization succeed. Allow the candidate to track and commit its own tile mutations while
    // that load gate is still raised; normal runtime callers still require the active owner above.
    return session.Lifecycle.IsGeneratingOrLoadingWorld &&
      ReferenceEquals(Main.ActiveWorldSession, session.World);
  }

  private static bool IsPendingPublicationSession(LoadedWorldSession session)
  {
    return session.IsComplete && session.IsPublicationUncertain && !session.IsPublished &&
      session.Lifecycle.IsGeneratingOrLoadingWorld;
  }

  private static bool IsExpectedProjectionSession(LoadedWorldSession session, bool candidate)
  {
    return candidate
      ? IsPendingPublicationSession(session)
      : IsActivePublishedSession(session);
  }

  private static bool HasRuntimeTileMap(LoadedWorldSession session, int x, int y)
  {
    return x >= 0 && y >= 0 &&
      x < session.Storage.TileMap.Width && y < session.Storage.TileMap.Height &&
      Main.tile is not null &&
      Main.tile.GetLength(0) == session.Storage.TileMap.Width &&
      Main.tile.GetLength(1) == session.Storage.TileMap.Height;
  }

  private static WorldStorageOperationResult Invalid(string detail)
  {
    return WorldStorageOperationResult.Failed(
      WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData, detail));
  }
}

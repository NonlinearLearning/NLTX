using NSSLC.WorldGeneration;
using Terraria.NonAuthoritative.Persistence;
using Terraria.NonAuthoritative.Simulation;
using Terraria.NonAuthoritative.WorldStorage;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.SimulationHost;

internal sealed class ActiveLiquidTickPhase : IWorldSimulationTickPhase
{
  private readonly LoadedWorldSession _session;
  private readonly LegacyTileMutationTracker _mutationTracker = new();

  public ActiveLiquidTickPhase(LoadedWorldSession session, bool runLiquidProbe = false)
  {
    _session = session ?? throw new ArgumentNullException(nameof(session));
    if (!session.IsPublished || session.IsPublicationUncertain ||
        !ReferenceEquals(WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession, session))
    {
      throw new InvalidOperationException(
        "Liquid runtime state requires the active published world session.");
    }

    Liquid.ReInit();
    LiquidBuffer.numLiquidBuffer = 0;
    WorldStorageOperationResult bindResult =
      LegacyWorldTileMapProjection.BindMutationObserver(session, _mutationTracker.MarkChanged);
    if (!bindResult.Succeeded)
    {
      throw new InvalidOperationException(
        $"Liquid owner tracking could not be bound: " +
        $"{bindResult.Failure.Kind} {bindResult.Failure.Detail}");
    }

    if (runLiquidProbe)
    {
      StartLiquidProbe();
    }
  }

  public WorldSimulationPhase Phase => WorldSimulationPhase.WorldSystems;

  public long TickCount { get; private set; }

  public long CommittedTileMutationCount { get; private set; }

  public long LiquidStateChangeCount { get; private set; }

  public int LastTickMutationCount { get; private set; }

  public int LastTickLiquidStateChangeCount { get; private set; }

  public TileCoordinate? ProbeCoordinate { get; private set; }

  public void Execute(WorldSimulationTickContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    if (!ReferenceEquals(context.Session, _session))
    {
      throw new InvalidOperationException(
        "A liquid runtime phase cannot be reused across world sessions.");
    }

    Liquid.UpdateLiquid();
    TickCount++;
    CommitPendingMutations();
  }

  private void StartLiquidProbe()
  {
    TileMapStore tileMap = _session.Storage.TileMap;
    int centerX = Math.Clamp(_session.World.Descriptor.SpawnTileX, 5, tileMap.Width - 6);
    int searchHeight = Math.Min(
      tileMap.Height - 20,
      Math.Max(20, (int)_session.World.Descriptor.SurfaceLayer - 20));
    for (int radius = 0; radius < tileMap.Width / 2; radius++)
    {
      int[] xCandidates = radius == 0
        ? new[] { centerX }
        : new[] { centerX - radius, centerX + radius };
      foreach (int x in xCandidates)
      {
        if (x < 5 || x >= tileMap.Width - 5)
        {
          continue;
        }

        for (int y = 10; y < searchHeight; y++)
        {
          if (!IsClearProbeContainer(tileMap, x, y))
          {
            continue;
          }

          var changedCoordinates = new List<TileCoordinate>(8);
          PlaceStoneWall(tileMap, x - 1, y, changedCoordinates);
          PlaceStoneWall(tileMap, x + 1, y, changedCoordinates);
          PlaceStoneWall(tileMap, x - 1, y + 1, changedCoordinates);
          PlaceStoneWall(tileMap, x + 1, y + 1, changedCoordinates);
          PlaceStoneWall(tileMap, x - 1, y + 2, changedCoordinates);
          PlaceStoneWall(tileMap, x, y + 2, changedCoordinates);
          PlaceStoneWall(tileMap, x + 1, y + 2, changedCoordinates);
          TileCellState source = tileMap.GetTile(x, y);
          source.LiquidAmount = byte.MaxValue;
          source.LiquidType = 0;
          source.Header = (byte)(source.Header & ~0x60);
          source.LastLiquidChangedRevision = checked(source.LastLiquidChangedRevision + 1);
          tileMap.CommitTile(x, y, source);
          changedCoordinates.Add(new TileCoordinate(x, y));
          WorldStorageOperationResult projection =
            LegacyWorldTileMapProjection.PublishCommittedTiles(
              _session,
              changedCoordinates);
          if (!projection.Succeeded)
          {
            throw new InvalidOperationException(
              $"The liquid probe source could not be projected: " +
              $"{projection.Failure.Kind} {projection.Failure.Detail}");
          }

          Liquid.AddWater(x, y);
          ProbeCoordinate = new TileCoordinate(x, y);
          CommitPendingMutations();
          return;
        }
      }
    }

    throw new InvalidOperationException(
      "The requested liquid probe could not find a liquid tile away from world edges.");
  }

  private static bool IsClearProbeContainer(TileMapStore tileMap, int x, int startY)
  {
    if (x <= 5 || x >= tileMap.Width - 6 || startY + 2 >= tileMap.Height - 5)
    {
      return false;
    }

    for (int y = startY; y <= startY + 2; y++)
    {
      for (int offsetX = -1; offsetX <= 1; offsetX++)
      {
        TileCellState cell = tileMap.GetTile(x + offsetX, y);
        if ((cell.TileHeader & 0x20) != 0 || cell.LiquidAmount != 0)
        {
          return false;
        }
      }
    }

    return true;
  }

  private static void PlaceStoneWall(
    TileMapStore tileMap,
    int x,
    int y,
    List<TileCoordinate> changedCoordinates)
  {
    TileCellState wall = tileMap.GetTile(x, y);
    wall.Type = 1;
    wall.TileHeader = (ushort)((wall.TileHeader & 0x0FE0) | 0x20);
    wall.LiquidAmount = 0;
    wall.LiquidType = 0;
    wall.Header = (byte)(wall.Header & ~0x60);
    wall.IsCheckingLiquid = false;
    wall.ShouldSkipLiquid = false;
    tileMap.CommitTile(x, y, wall);
    changedCoordinates.Add(new TileCoordinate(x, y));
  }

  private void CommitPendingMutations()
  {
    TileCoordinate[] changedCoordinates = _mutationTracker.Drain();
    LastTickMutationCount = changedCoordinates.Length;
    LastTickLiquidStateChangeCount = 0;
    if (changedCoordinates.Length == 0)
    {
      return;
    }

    int liquidStateChanges = CountLiquidStateChanges(changedCoordinates);

    WorldStorageOperationResult commitResult =
      LegacyWorldTileMapProjection.CommitLegacyTileMutations(
        _session,
        changedCoordinates);
    if (!commitResult.Succeeded)
    {
      throw new InvalidOperationException(
        $"Liquid tile changes could not be committed to their owner: " +
        $"{commitResult.Failure.Kind} {commitResult.Failure.Detail}");
    }

    VerifyCommittedTiles(changedCoordinates);
    LastTickLiquidStateChangeCount = liquidStateChanges;
    LiquidStateChangeCount = checked(LiquidStateChangeCount + liquidStateChanges);
    CommittedTileMutationCount = checked(
      CommittedTileMutationCount + changedCoordinates.Length);
  }

  private int CountLiquidStateChanges(IReadOnlyList<TileCoordinate> coordinates)
  {
    int changedCount = 0;
    foreach (TileCoordinate coordinate in coordinates)
    {
      TileCellState owner = _session.Storage.TileMap.GetTile(coordinate.X, coordinate.Y);
      Tile runtime = Main.tile[coordinate.X, coordinate.Y];
      byte runtimeLiquidType = runtime.liquid > 0 ? runtime.liquidType() : (byte)0;
      if (owner.LiquidAmount != runtime.liquid || owner.LiquidType != runtimeLiquidType)
      {
        changedCount++;
      }
    }

    return changedCount;
  }

  private void VerifyCommittedTiles(IReadOnlyList<TileCoordinate> coordinates)
  {
    foreach (TileCoordinate coordinate in coordinates)
    {
      TileCellState owner = _session.Storage.TileMap.GetTile(coordinate.X, coordinate.Y);
      Tile runtime = Main.tile[coordinate.X, coordinate.Y];
      if (owner.Type != runtime.type ||
          owner.Wall != runtime.wall ||
          owner.LiquidAmount != runtime.liquid ||
          owner.LiquidType != (runtime.liquid > 0 ? runtime.liquidType() : 0) ||
          owner.TileHeader != runtime.sTileHeader ||
          owner.Header != runtime.bTileHeader ||
          owner.Header2 != runtime.bTileHeader2 ||
          owner.Header3 != (byte)(runtime.bTileHeader3 & ~0x18) ||
          owner.IsCheckingLiquid != runtime.checkingLiquid() ||
          owner.ShouldSkipLiquid != runtime.skipLiquid() ||
          owner.FrameX != runtime.frameX ||
          owner.FrameY != runtime.frameY)
      {
        throw new InvalidOperationException(
          $"Legacy tile ({coordinate.X}, {coordinate.Y}) diverged from the loaded-world owner.");
      }
    }
  }
}

using Terraria.NonAuthoritative.Simulation;
using Terraria.NonAuthoritative.Persistence;
using Terraria.NonAuthoritative.WorldStorage;
using Terraria.Content;
using Terraria.WorldInteraction.TileEntities;
using Terraria.WorldSession.Components;
using Terraria.WorldStorage;
using RuntimeTileEntityProjection =
  Terraria.NonAuthoritative.WorldStorage.LegacyWorldTileEntityProjection;

namespace Terraria.NonAuthoritative.SimulationHost;

internal sealed class ActiveTileEntityTickPhase : IWorldSimulationTickPhase
{
  private const ushort TrainingDummyTileType = 378;
  private const ushort LogicSensorTileType = 423;
  private readonly RuntimeNpcStore _npcs;
  private readonly RuntimePlayerStore _players;
  private readonly ContentCatalog _content;
  private readonly WiredActuatorTrigger _wiringActuatorTrigger = new();

  public ActiveTileEntityTickPhase(
    LoadedWorldSession session,
    RuntimeNpcStore npcs,
    RuntimePlayerStore players,
    ContentCatalog content)
  {
    ArgumentNullException.ThrowIfNull(session);
    _npcs = npcs ?? throw new ArgumentNullException(nameof(npcs));
    _players = players ?? throw new ArgumentNullException(nameof(players));
    _content = content ?? throw new ArgumentNullException(nameof(content));
    ValidateSupportedEntities(session);
  }

  public WorldSimulationPhase Phase => WorldSimulationPhase.TileEntities;

  public int UpdatePassCount { get; private set; }

  public IReadOnlyList<ushort> RecognizedUnsupportedWiredDeviceTileTypes =>
    _wiringActuatorTrigger.RecognizedUnsupportedWiredDeviceTileTypes;

  public void Execute(WorldSimulationTickContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    TileEntityUpdateSchedule schedule = context.Session.Storage.TileEntityUpdates;
    IReadOnlyList<TileEntityId> scheduledIds = schedule.CaptureTickSnapshot();
    try
    {
      foreach (TileEntityId id in scheduledIds)
      {
        UpdatePassCount++;
        if (!context.Session.Storage.TileEntities.TryGetRuntimeState(
              id,
              out TileEntityRuntimeState entity))
        {
          schedule.Unschedule(id);
          continue;
        }

        switch (entity.Type.Value)
        {
          case 0:
            UpdateTrainingDummyReference(context, entity);
            break;
          case 2:
            UpdateLogicSensor(context, entity);
            break;
          default:
            throw new InvalidOperationException(
              $"Tile entity type {entity.Type.Value} has no registered simulation handler.");
        }
      }
    }
    finally
    {
      schedule.CompleteTickSnapshot();
    }
  }

  private void UpdateTrainingDummyReference(
    WorldSimulationTickContext context,
    TileEntityRuntimeState entity)
  {
    if (!HasValidAnchor(context.Session, entity.Anchor, TrainingDummyTileType))
    {
      Remove(context.Session, entity);
      return;
    }

    if (entity.NpcIndex >= 0 &&
        _npcs.TryCaptureTrainingDummyBinding(entity.NpcIndex, entity.Anchor, out _))
    {
      return;
    }

    if (entity.NpcIndex >= 0)
    {
      if (!context.Session.Storage.TileEntities.CommitTrainingDummyNpcIndex(entity.Id, -1))
      {
        throw new InvalidOperationException(
          "A stale training dummy slot reference could not be cleared from its owner.");
      }
      PublishCommittedState(context.Session, entity.Id);
      if (!IsCurrentEntity(context.Session, entity))
      {
        return;
      }
    }

    if (!_npcs.TrySpawnTrainingDummy(
          entity.Anchor.X,
          entity.Anchor.Y,
          _content,
          context.Session.World.Descriptor.WorldId,
          out RuntimeNpcTrainingDummyBinding binding))
    {
      return;
    }

    if (!IsCurrentEntity(context.Session, entity))
    {
      if (!_npcs.TryReleaseTrainingDummy(binding))
      {
        throw new InvalidOperationException(
          "A training dummy spawned for a replaced TileEntity could not release its slot.");
      }
      return;
    }

    if (!context.Session.Storage.TileEntities.CommitTrainingDummyNpcIndex(
          entity.Id,
           checked((short)binding.Slot.Value)))
    {
      if (!_npcs.TryReleaseTrainingDummy(binding))
      {
        throw new InvalidOperationException(
          "A training dummy allocated for a removed TileEntity could not release its slot.");
      }
      return;
    }

    PublishCommittedState(context.Session, entity.Id);
  }

  private void UpdateLogicSensor(
    WorldSimulationTickContext context,
    TileEntityRuntimeState entity)
  {
    if (!HasValidAnchor(context.Session, entity.Anchor, LogicSensorTileType))
    {
      Remove(context.Session, entity);
      return;
    }

    bool desiredState = EvaluateLogicSensor(
      context,
      entity,
      entity.LogicSensorCountedData,
      out int countedData);
    bool stateChanged = context.Session.Storage.TileEntities.CommitLogicSensorState(
      entity.Id,
      desiredState,
      countedData);
    if (stateChanged)
    {
      PublishCommittedState(context.Session, entity.Id);
      if (!IsCurrentEntity(context.Session, entity))
      {
        return;
      }
    }

    TileCellState tile = context.Session.Storage.TileMap.GetTile(
      entity.Anchor.X,
      entity.Anchor.Y);
    short desiredFrameX = (short)(desiredState ? 18 : 0);
    if (tile.FrameX != desiredFrameX)
    {
      tile.FrameX = desiredFrameX;
      context.Session.Storage.TileMap.CommitTile(entity.Anchor.X, entity.Anchor.Y, tile);
      WorldStorageOperationResult result = LegacyWorldTileMapProjection.PublishCommittedTile(
        context.Session,
        entity.Anchor.X,
        entity.Anchor.Y);
      if (!result.Succeeded)
      {
        throw new InvalidOperationException(
          $"The logic sensor tile could not be projected to the runtime: " +
          $"{result.Failure.Kind} {result.Failure.Detail}");
      }

      if (!IsCurrentEntity(context.Session, entity))
      {
        return;
      }
    }

    if (stateChanged && ShouldTripWireOnStateChange(entity.LogicCheck, desiredState) &&
        IsCurrentEntity(context.Session, entity))
    {
      _wiringActuatorTrigger.Trigger(context.Session, entity.Anchor);
    }
  }

  private bool EvaluateLogicSensor(
    WorldSimulationTickContext context,
    TileEntityRuntimeState entity,
    int currentCountedData,
    out int countedData)
  {
    countedData = currentCountedData;
    switch ((LogicCheckType)entity.LogicCheck)
    {
      case LogicCheckType.None:
        return entity.LogicOn;
      case LogicCheckType.Day:
        return RequireClock(context).DayTime;
      case LogicCheckType.Night:
        return !RequireClock(context).DayTime;
      case LogicCheckType.PlayerAbove:
        return HasPlayerAbove(entity.Anchor);
      case LogicCheckType.Water:
      case LogicCheckType.Lava:
      case LogicCheckType.Honey:
      case LogicCheckType.Liquid:
        return EvaluateLiquidSensor(
          context.Session,
          entity,
          currentCountedData,
          out countedData);
      default:
        throw new NotSupportedException(
          $"Logic sensor check {entity.LogicCheck} is not supported by this simulation host.");
    }
  }

  private static bool EvaluateLiquidSensor(
    LoadedWorldSession session,
    TileEntityRuntimeState entity,
    int currentCountedData,
    out int countedData)
  {
    TileCellState tile = session.Storage.TileMap.GetTile(entity.Anchor.X, entity.Anchor.Y);
    return TileEntityLiquidSensorSystem.Evaluate(
      entity.LogicCheck,
      tile,
      entity.LogicOn,
      currentCountedData,
      out countedData);
  }

  private static WorldClockSnapshotValue RequireClock(WorldSimulationTickContext context)
  {
    return context.WorldSnapshot.Clock ??
      throw new InvalidOperationException(
        "The tile entity phase requires a committed world clock.");
  }

  private bool HasPlayerAbove(TileCoordinate anchor)
  {
    float left = anchor.X * 16f - 33f;
    float top = anchor.Y * 16f - 161f;
    return _players.HasAlivePlayerIntersecting(left, top, width: 82f, height: 162f);
  }

  private static void ValidateSupportedEntities(LoadedWorldSession session)
  {
    foreach (TileEntitySnapshot entity in session.Storage.TileEntities.CreateSnapshot())
    {
      if (entity.Type.Value is not (0 or 2))
      {
        throw new NotSupportedException(
          $"Tile entity type {entity.Type.Value} has no registered simulation handler.");
      }

      if (entity.Type.Value == 2 && entity.LogicCheck > (byte)LogicCheckType.Liquid)
      {
        throw new NotSupportedException(
          $"Logic sensor check {entity.LogicCheck} is not supported by this simulation host.");
      }
    }
  }

  private static bool HasValidAnchor(
    LoadedWorldSession session,
    TileCoordinate anchor,
    ushort tileType)
  {
    if (!IsWithinTileMap(session, anchor))
    {
      return false;
    }

    TileCellState tile = session.Storage.TileMap.GetTile(anchor.X, anchor.Y);
    return (tile.TileHeader & 0x20) != 0 && tile.Type == tileType;
  }

  private static bool IsWithinTileMap(LoadedWorldSession session, TileCoordinate anchor)
  {
    return (uint)anchor.X < (uint)session.Storage.TileMap.Width &&
      (uint)anchor.Y < (uint)session.Storage.TileMap.Height;
  }

  private static bool ShouldTripWireOnStateChange(byte logicCheck, bool newState)
  {
    switch ((LogicCheckType)logicCheck)
    {
      case LogicCheckType.Day:
      case LogicCheckType.Night:
        return newState;
      case LogicCheckType.PlayerAbove:
      case LogicCheckType.Water:
      case LogicCheckType.Lava:
      case LogicCheckType.Honey:
      case LogicCheckType.Liquid:
        return true;
      default:
        return false;
    }
  }

  private static bool TripsWireWhenRemoved(byte logicCheck)
  {
    switch ((LogicCheckType)logicCheck)
    {
      case LogicCheckType.PlayerAbove:
      case LogicCheckType.Water:
      case LogicCheckType.Lava:
      case LogicCheckType.Honey:
      case LogicCheckType.Liquid:
        return true;
      default:
        return false;
    }
  }

  private void Remove(LoadedWorldSession session, TileEntityRuntimeState entity)
  {
    TileEntityStore tileEntities = session.Storage.TileEntities;
    if (entity.Type.Value == 2 && entity.LogicOn &&
        TripsWireWhenRemoved(entity.LogicCheck) &&
        IsWithinTileMap(session, entity.Anchor))
    {
      _wiringActuatorTrigger.Trigger(session, entity.Anchor);
      if (!TryGetCurrentEntity(session, entity, out TileEntityRuntimeState current))
      {
        return;
      }

      entity = current;
    }

    if (entity.Type.Value == 0 &&
        _npcs.TryCaptureTrainingDummyBinding(
          entity.NpcIndex,
          entity.Anchor,
          out RuntimeNpcTrainingDummyBinding binding))
    {
      if (!TryGetCurrentEntity(session, entity, out _))
      {
        return;
      }

      if (!_npcs.TryReleaseTrainingDummy(binding))
      {
        throw new InvalidOperationException(
          "A training dummy being removed with its TileEntity could not release its slot.");
      }
    }

    if (!TryGetCurrentEntity(session, entity, out TileEntityRuntimeState currentEntity))
    {
      return;
    }

    if (HasValidAnchor(session, currentEntity.Anchor, GetAnchorTileType(currentEntity.Type)))
    {
      return;
    }

    if (!tileEntities.TryGetSnapshot(currentEntity.Id, out TileEntitySnapshot? snapshot) ||
        snapshot is null)
    {
      return;
    }

    if (!tileEntities.Remove(currentEntity.Id))
    {
      if (TryGetCurrentEntity(session, currentEntity, out _))
      {
        throw new InvalidOperationException("A stale TileEntity could not be removed.");
      }

      return;
    }

    WorldStorageOperationResult result = RuntimeTileEntityProjection.RemoveCommittedSnapshot(
      session,
      snapshot);
    if (!result.Succeeded)
    {
      throw new InvalidOperationException(
        $"The committed TileEntity removal could not be projected: " +
        $"{result.Failure.Kind} {result.Failure.Detail}");
    }
  }

  private static bool IsCurrentEntity(
    LoadedWorldSession session,
    TileEntityRuntimeState expected)
  {
    return TryGetCurrentEntity(session, expected, out _);
  }

  private static bool TryGetCurrentEntity(
    LoadedWorldSession session,
    TileEntityRuntimeState expected,
    out TileEntityRuntimeState current)
  {
    return session.Storage.TileEntities.TryGetRuntimeState(expected.Id, out current) &&
      current.RuntimeReference == expected.RuntimeReference;
  }

  private static ushort GetAnchorTileType(TileEntityTypeId type)
  {
    return type.Value switch
    {
      0 => TrainingDummyTileType,
      2 => LogicSensorTileType,
      _ => throw new InvalidOperationException(
        $"Tile entity type {type.Value} has no registered anchor tile.")
    };
  }

  private static void PublishCommittedState(LoadedWorldSession session, TileEntityId id)
  {
    if (!session.Storage.TileEntities.TryGetSnapshot(id, out TileEntitySnapshot? snapshot) ||
        snapshot is null)
    {
      throw new InvalidOperationException("A committed TileEntity snapshot could not be read.");
    }

    WorldStorageOperationResult result = RuntimeTileEntityProjection.PublishCommittedSnapshot(
      session,
      snapshot);
    if (!result.Succeeded)
    {
      throw new InvalidOperationException(
        $"The committed TileEntity state could not be projected: " +
        $"{result.Failure.Kind} {result.Failure.Detail}");
    }
  }
}

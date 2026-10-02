using System;
using System.Linq;
using Terraria.WorldInteraction.Tiles;

namespace Terraria.WorldInteraction.Wiring;

public sealed class WiringPropagationCommitPort : IWiringPropagationCommitPort
{
  private readonly WirePropagationScratchComponent _state;

  public WiringPropagationCommitPort(WirePropagationScratchComponent state)
  {
    ArgumentNullException.ThrowIfNull(state);
    _state = state;
  }

  public WiringPropagationCommitResult Commit(
    in WiringPropagationCommand command)
  {
    return command.Kind switch
    {
      WiringPropagationCommandKind.Initialize => Initialize(),
      WiringPropagationCommandKind.BeginTrip => BeginTrip(command),
      WiringPropagationCommandKind.BeginWireColorPass => BeginWireColorPass(command),
      WiringPropagationCommandKind.SkipTile => SkipTile(command),
      WiringPropagationCommandKind.QueueTile => QueueTile(command),
      WiringPropagationCommandKind.QueueLamp => QueueLamp(command),
      WiringPropagationCommandKind.QueueGate => QueueGate(command),
      WiringPropagationCommandKind.QueueNextGate => QueueNextGate(command),
      WiringPropagationCommandKind.CompleteGate => CompleteGate(command),
      WiringPropagationCommandKind.RecordPixelBoxTrigger => RecordPixelBoxTrigger(command),
      WiringPropagationCommandKind.EndTrip => EndTrip(),
      WiringPropagationCommandKind.Reset => Reset(),
      _ => WiringPropagationCommitResult.Rejected("invalid-command")
    };
  }

  public WiringPropagationSnapshot Snapshot()
  {
    return _state.CreateSnapshot();
  }

  private WiringPropagationCommitResult Initialize()
  {
    WiringPropagationSnapshot before = _state.CreateSnapshot();
    _state.InitializePropagation();
    return WiringPropagationCommitResult.Accepted(
      changed: before.IsRunning || before.CurrentWireColor.HasValue ||
        before.CurrentUser != byte.MaxValue || before.SkippedTiles.Count != 0 ||
        before.Frontier.Count != 0 || before.CurrentGates.Count != 0 ||
        before.NextGates.Count != 0 || before.LampsToCheck.Count != 0 ||
        before.TilesToProcess.Count != 0 || before.CompletedGates.Count != 0 ||
        before.PixelBoxTriggers.Count != 0 ||
        before.TeleportTargets.Any(target => target.HasValue) ||
        before.BlockPlayerTeleportationForOneIteration);
  }

  private WiringPropagationCommitResult BeginTrip(
    WiringPropagationCommand command)
  {
    return _state.BeginPropagation(
      WiringPropagationQuery.NormalizeCurrentUser(command.CurrentUser))
      ? WiringPropagationCommitResult.Accepted(changed: true)
      : WiringPropagationCommitResult.Rejected("already-running");
  }

  private WiringPropagationCommitResult BeginWireColorPass(
    WiringPropagationCommand command)
  {
    if (!WiringPropagationQuery.IsValidWireColor(command.Value))
    {
      return WiringPropagationCommitResult.Rejected("wire-color");
    }

    return _state.SetWireColor(command.Value)
      ? WiringPropagationCommitResult.Accepted(changed: true)
      : WiringPropagationCommitResult.Rejected("not-running");
  }

  private WiringPropagationCommitResult SkipTile(
    WiringPropagationCommand command)
  {
    if (!_state.IsRunning)
    {
      return WiringPropagationCommitResult.Rejected("not-running");
    }

    if (_state.SkippedTiles.Contains(command.Coordinate))
    {
      return WiringPropagationCommitResult.Duplicate;
    }

    return _state.TrySkip(command.Coordinate)
      ? WiringPropagationCommitResult.Accepted(changed: true)
      : WiringPropagationCommitResult.Duplicate;
  }

  private WiringPropagationCommitResult QueueTile(
    WiringPropagationCommand command)
  {
    if (!_state.IsRunning)
    {
      return WiringPropagationCommitResult.Rejected("not-running");
    }

    if (_state.CurrentWireColor is null)
    {
      return WiringPropagationCommitResult.Rejected("wire-color-not-set");
    }

    if (_state.SkippedTiles.Contains(command.Coordinate) ||
        _state.TilesToProcess.ContainsKey(command.Coordinate))
    {
      return WiringPropagationCommitResult.Duplicate;
    }

    return _state.TryQueueTile(command.Coordinate, command.Value)
      ? WiringPropagationCommitResult.Accepted(changed: true)
      : WiringPropagationCommitResult.Duplicate;
  }

  private WiringPropagationCommitResult QueueLamp(
    WiringPropagationCommand command)
  {
    if (!_state.IsRunning)
    {
      return WiringPropagationCommitResult.Rejected("not-running");
    }

    if (_state.LampsToCheck.Contains(command.Coordinate))
    {
      return WiringPropagationCommitResult.Duplicate;
    }

    return _state.TryQueueLamp(command.Coordinate)
      ? WiringPropagationCommitResult.Accepted(changed: true)
      : WiringPropagationCommitResult.Duplicate;
  }

  private WiringPropagationCommitResult QueueGate(
    WiringPropagationCommand command)
  {
    if (!_state.IsRunning)
    {
      return WiringPropagationCommitResult.Rejected("not-running");
    }

    if (_state.CurrentGates.Contains(command.Coordinate))
    {
      return WiringPropagationCommitResult.Duplicate;
    }

    return _state.TryQueueCurrentGate(command.Coordinate)
      ? WiringPropagationCommitResult.Accepted(changed: true)
      : WiringPropagationCommitResult.Duplicate;
  }

  private WiringPropagationCommitResult CompleteGate(
    WiringPropagationCommand command)
  {
    if (!_state.IsRunning)
    {
      return WiringPropagationCommitResult.Rejected("not-running");
    }

    if (_state.CompletedGates.Contains(command.Coordinate))
    {
      return WiringPropagationCommitResult.Duplicate;
    }

    return _state.TryCompleteGate(command.Coordinate)
      ? WiringPropagationCommitResult.Accepted(changed: true)
      : WiringPropagationCommitResult.Duplicate;
  }

  private WiringPropagationCommitResult QueueNextGate(
    WiringPropagationCommand command)
  {
    if (!_state.IsRunning)
    {
      return WiringPropagationCommitResult.Rejected("not-running");
    }

    if (_state.NextGates.Contains(command.Coordinate))
    {
      return WiringPropagationCommitResult.Duplicate;
    }

    return _state.TryQueueNextGate(command.Coordinate)
      ? WiringPropagationCommitResult.Accepted(changed: true)
      : WiringPropagationCommitResult.Duplicate;
  }

  private WiringPropagationCommitResult RecordPixelBoxTrigger(
    WiringPropagationCommand command)
  {
    if (!_state.IsRunning)
    {
      return WiringPropagationCommitResult.Rejected("not-running");
    }

    if (_state.PixelBoxTriggers.ContainsKey(command.Coordinate))
    {
      return WiringPropagationCommitResult.Duplicate;
    }

    return _state.TryRecordPixelBoxTrigger(
      command.Coordinate,
      command.Value)
      ? WiringPropagationCommitResult.Accepted(changed: true)
      : WiringPropagationCommitResult.Duplicate;
  }

  private WiringPropagationCommitResult EndTrip()
  {
    return _state.EndPropagation()
      ? WiringPropagationCommitResult.Accepted(changed: true)
      : WiringPropagationCommitResult.Rejected("not-running");
  }

  private WiringPropagationCommitResult Reset()
  {
    bool changed = _state.IsRunning ||
      _state.CurrentWireColor.HasValue ||
      _state.CurrentUser != byte.MaxValue ||
      _state.SkippedTiles.Count != 0 ||
      _state.Frontier.Count != 0 ||
      _state.CurrentGates.Count != 0 ||
      _state.NextGates.Count != 0 ||
      _state.LampsToCheck.Count != 0 ||
      _state.TilesToProcess.Count != 0 ||
      _state.CompletedGates.Count != 0 ||
      _state.PixelBoxTriggers.Count != 0;
    _state.ResetPropagation();
    return WiringPropagationCommitResult.Reset(changed);
  }
}

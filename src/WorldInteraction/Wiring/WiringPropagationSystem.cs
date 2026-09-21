using System;

namespace Terraria.WorldInteraction.Wiring;

public sealed class WiringPropagationSystem
{
  private readonly IWiringPropagationCommitPort _commitPort;

  public WiringPropagationSystem(
    IWiringPropagationCommitPort commitPort)
  {
    ArgumentNullException.ThrowIfNull(commitPort);
    _commitPort = commitPort;
  }

  public WiringPropagationCommitResult Execute(
    in WiringPropagationCommand command,
    int worldWidth,
    int worldHeight)
  {
    if (!WiringPropagationQuery.IsKnownCommand(command))
    {
      return WiringPropagationCommitResult.Rejected("invalid-command");
    }

    if (!WiringPropagationQuery.IsWithinBounds(
      command,
      worldWidth,
      worldHeight))
    {
      return WiringPropagationCommitResult.Rejected("bounds");
    }

    if (command.Kind is WiringPropagationCommandKind.SkipTile or
        WiringPropagationCommandKind.QueueTile or
        WiringPropagationCommandKind.QueueLamp or
        WiringPropagationCommandKind.QueueGate or
        WiringPropagationCommandKind.CompleteGate or
        WiringPropagationCommandKind.RecordPixelBoxTrigger)
    {
      if (!WiringPropagationQuery.IsWithinBounds(
        command.Coordinate,
        worldWidth,
        worldHeight))
      {
        return WiringPropagationCommitResult.Rejected("bounds");
      }
    }

    return _commitPort.Commit(in command);
  }

  public WiringPropagationSnapshot Snapshot()
  {
    return _commitPort.Snapshot();
  }
}

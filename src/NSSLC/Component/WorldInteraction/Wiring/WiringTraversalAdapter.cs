using System;
using System.Collections.Generic;
using Terraria.Relationships;
using Terraria.WorldInteraction.Tiles;

namespace Terraria.WorldInteraction.Wiring;

public sealed class WiringTraversalAdapter
{
  private readonly IWiringTraversalCommitPort _commitPort;

  public WiringTraversalAdapter(IWiringTraversalCommitPort commitPort)
  {
    ArgumentNullException.ThrowIfNull(commitPort);
    _commitPort = commitPort;
  }

  public WiringTraversalFlushResult Flush(
    WirePropagationScratchComponent propagation,
    PumpTransferScratchComponent pumps,
    byte wireColor,
    IReadOnlyList<EntityReference> subjects)
  {
    ArgumentNullException.ThrowIfNull(propagation);
    ArgumentNullException.ThrowIfNull(pumps);
    ArgumentNullException.ThrowIfNull(subjects);

    WiringTraversalFlushStatus status = WiringTraversalFlushStatus.Empty;
    string? failureReason = null;
    int pumpCommandsSubmitted = 0;
    int teleportCommandsSubmitted = 0;
    bool pumpScratchReset = false;
    bool teleportScratchReset = false;
    bool scratchCleanupSucceeded = false;

    try
    {
      PumpTransferCommandBatch pumpBatch =
        PumpTransferPolicy.CreateCommands(pumps, wireColor);
      if (pumpBatch.IsRejected)
      {
        status = WiringTraversalFlushStatus.Rejected;
        failureReason = pumpBatch.FailureReason;
      }
      else
      {
        if (!TryCreateTeleportCommands(
              propagation,
              wireColor,
              subjects,
              out List<WiringTeleportCommand> teleportCommands))
        {
          status = WiringTraversalFlushStatus.Rejected;
          failureReason = "invalid-teleport-intent";
        }
        else if (pumpBatch.Commands.Count == 0 &&
                 teleportCommands.Count == 0)
        {
          status = WiringTraversalFlushStatus.Empty;
        }
        else
        {
          status = WiringTraversalFlushStatus.Accepted;
          bool pumpsCommitted = CommitPumps(
            pumpBatch.Commands,
            ref pumpCommandsSubmitted,
            ref status,
            ref failureReason);
          if (pumpsCommitted)
          {
            CommitTeleports(
              teleportCommands,
              ref teleportCommandsSubmitted,
              ref status,
              ref failureReason);
          }
        }
      }
    }
    catch (Exception exception)
    {
      status = WiringTraversalFlushStatus.Failed;
      failureReason = $"commit-port-exception:{exception.GetType().Name}";
    }
    finally
    {
      bool pumpCleanupSucceeded = false;
      bool teleportCleanupSucceeded = false;
      try
      {
        pumpScratchReset = pumps.Reset();
        pumpCleanupSucceeded = true;
      }
      catch (Exception exception)
      {
        status = WiringTraversalFlushStatus.Failed;
        failureReason = $"scratch-cleanup-exception:{exception.GetType().Name}";
      }

      try
      {
        teleportScratchReset = propagation.ResetTeleportState();
        teleportCleanupSucceeded = true;
      }
      catch (Exception exception)
      {
        status = WiringTraversalFlushStatus.Failed;
        if (failureReason is null)
        {
          failureReason =
            $"scratch-cleanup-exception:{exception.GetType().Name}";
        }
      }

      scratchCleanupSucceeded =
        pumpCleanupSucceeded && teleportCleanupSucceeded;
    }

    return CompleteResult();

    WiringTraversalFlushResult CompleteResult()
    {
      return new WiringTraversalFlushResult(
        status,
        pumpCommandsSubmitted,
        teleportCommandsSubmitted,
        pumpScratchReset,
        teleportScratchReset,
        scratchCleanupSucceeded,
        failureReason);
    }
  }

  private static bool TryCreateTeleportCommands(
    WirePropagationScratchComponent propagation,
    byte wireColor,
    IReadOnlyList<EntityReference> subjects,
    out List<WiringTeleportCommand> commands)
  {
    commands = new List<WiringTeleportCommand>();
    TileCoordinate? source = propagation.TeleportTargets[0];
    TileCoordinate? destination = propagation.TeleportTargets[1];
    if (source.HasValue != destination.HasValue)
    {
      return false;
    }

    if (!source.HasValue)
    {
      return true;
    }

    for (int index = 0; index < subjects.Count; index++)
    {
      if (!WiringTeleportCommand.TryCreate(
        source.Value,
        destination!.Value,
        wireColor,
        subjects[index],
        propagation.BlockPlayerTeleportationForOneIteration,
        out WiringTeleportCommand command))
      {
        return false;
      }

      commands.Add(command);
    }

    return true;
  }

  private bool CommitPumps(
    IReadOnlyList<PumpTransferCommand> commands,
    ref int submitted,
    ref WiringTraversalFlushStatus status,
    ref string? failureReason)
  {
    for (int index = 0; index < commands.Count; index++)
    {
      PumpTransferCommand command = commands[index];
      WiringTraversalCommitResult result = _commitPort.CommitPump(
        in command);
      submitted++;
      if (result.Succeeded)
      {
        continue;
      }

      status = result.Status switch
      {
        WiringTraversalCommitStatus.Failed => WiringTraversalFlushStatus.Failed,
        WiringTraversalCommitStatus.Unknown => WiringTraversalFlushStatus.Unknown,
        _ => WiringTraversalFlushStatus.Rejected,
      };
      failureReason = $"pump:{result.FailureReason ?? result.Status.ToString()}";
      return false;
    }

    return true;
  }

  private void CommitTeleports(
    IReadOnlyList<WiringTeleportCommand> commands,
    ref int submitted,
    ref WiringTraversalFlushStatus status,
    ref string? failureReason)
  {
    for (int index = 0; index < commands.Count; index++)
    {
      WiringTeleportCommand command = commands[index];
      WiringTraversalCommitResult result = _commitPort.CommitTeleport(
        in command);
      submitted++;
      if (result.Succeeded)
      {
        continue;
      }

      status = result.Status switch
      {
        WiringTraversalCommitStatus.Failed => WiringTraversalFlushStatus.Failed,
        WiringTraversalCommitStatus.Unknown => WiringTraversalFlushStatus.Unknown,
        _ => WiringTraversalFlushStatus.Rejected,
      };
      failureReason = $"teleport:{result.FailureReason ?? result.Status.ToString()}";
      return;
    }
  }
}

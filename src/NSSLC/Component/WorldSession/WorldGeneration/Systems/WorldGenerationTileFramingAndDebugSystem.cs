using System;

using Terraria.WorldGeneration.Actions;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Executes one framing or diagnostic intent through explicit effect ports.
/// </summary>
/// <remarks>
/// The system does not access tile state, graphics objects, or ambient world
/// services. The caller owns the supplied port and diagnostic sink until the
/// synchronous call returns.
/// </remarks>
public static class WorldGenerationTileFramingAndDebugSystem
{
  public enum ExceptionPolicy : byte
  {
    Propagate,
    TreatAsFailure,
    TreatAsFailureAndStop,
  }

  public readonly record struct Result(
    bool Accepted,
    bool FramesApplied,
    bool DiagnosticPublished,
    bool StopRequested,
    string? FailureReason)
  {
    public static Result Succeeded(
      bool framesApplied = false,
      bool diagnosticPublished = false)
    {
      return new Result(
        true,
        framesApplied,
        diagnosticPublished,
        false,
        null);
    }

    public static Result Failed(string reason, bool stopRequested)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(reason);
      return new Result(
        false,
        false,
        false,
        stopRequested,
        reason);
    }
  }

  public interface IFramingPort
  {
    void Frame(TilePosition target, bool frameNeighbors);
  }

  public static Result Execute(
    in WorldGenerationTileFramingAndDebugActionsCommand command,
    IFramingPort? framingPort,
    ExceptionPolicy exceptionPolicy = ExceptionPolicy.Propagate)
  {
    command.Validate();
    if (!Enum.IsDefined(exceptionPolicy))
    {
      throw new ArgumentOutOfRangeException(nameof(exceptionPolicy));
    }

    try
    {
      return command.Kind switch
      {
        WorldGenerationTileFramingAndDebugActionsCommand.OperationKind.SetFrames =>
          ExecuteFrames(command, framingPort),
        WorldGenerationTileFramingAndDebugActionsCommand.OperationKind.DebugDraw =>
          ExecuteDiagnostic(command),
        _ => throw new ArgumentOutOfRangeException(nameof(command)),
      };
    }
    catch (Exception exception) when (
      exceptionPolicy != ExceptionPolicy.Propagate)
    {
      return Result.Failed(
        $"CallbackException:{exception.GetType().Name}",
        exceptionPolicy == ExceptionPolicy.TreatAsFailureAndStop);
    }
  }

  private static Result ExecuteFrames(
    WorldGenerationTileFramingAndDebugActionsCommand command,
    IFramingPort? framingPort)
  {
    if (framingPort is null)
    {
      return Result.Failed("FramingPortMissing", stopRequested: true);
    }

    framingPort.Frame(command.Target, command.FrameNeighbors);
    return Result.Succeeded(framesApplied: true);
  }

  private static Result ExecuteDiagnostic(
    WorldGenerationTileFramingAndDebugActionsCommand command)
  {
    command.DiagnosticSink!.Publish(command.Target, command.DiagnosticColor);
    return Result.Succeeded(diagnosticPublished: true);
  }
}

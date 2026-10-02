using System;

using Terraria.WorldGeneration.Actions;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Queries;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Executes one tile-scan or control action against one explicit tile observation.
/// </summary>
/// <remarks>
/// This is a synchronous effect boundary. It does not retain the command or any
/// callback, enqueue work, or read ambient world state. The caller owns the
/// lifetime of every reference until this call returns.
/// </remarks>
public static class WorldGenerationTileScanAndControlSystem
{
  public enum ExceptionPolicy : byte
  {
    Propagate,
    TreatAsFailure,
    TreatAsFailureAndStop,
  }

  public readonly record struct ExecutionPolicy(
    ExceptionPolicy ExceptionHandling = ExceptionPolicy.Propagate);

  public readonly record struct Result(
    bool Accepted,
    bool? CallbackResult,
    bool CountUpdated,
    bool TileMatched,
    bool BoundsUpdated,
    bool StopRequested,
    string? FailureReason)
  {
    public bool CallbackInvoked => CallbackResult.HasValue;

    public static Result Succeeded(
      bool? callbackResult = null,
      bool countUpdated = false,
      bool tileMatched = false,
      bool boundsUpdated = false)
    {
      return new Result(
        true,
        callbackResult,
        countUpdated,
        tileMatched,
        boundsUpdated,
        false,
        null);
    }

    public static Result Failed(
      string failureReason,
      bool stopRequested)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(failureReason);
      return new Result(
        false,
        null,
        false,
        false,
        false,
        stopRequested,
        failureReason);
    }
  }

  /// <summary>
  /// Provides the explicit mutation boundary for the legacy UpdateBounds action.
  /// </summary>
  public interface IWritableBoundsReference :
    WorldGenerationTileScanAndControlActionsCommand.IBoundsReference
  {
    void Update(TilePosition position);
  }

  public static Result Execute(
    in WorldGenerationTileScanAndControlActionsCommand command,
    WorldGenerationTileScanQuery.TileObservation observation,
    ExecutionPolicy policy = default)
  {
    command.Validate();
    ValidatePolicy(policy);

    try
    {
      return command.Kind switch
      {
        WorldGenerationTileScanAndControlActionsCommand.OperationKind.Continue =>
          ExecuteContinuation(command, observation.Position),
        WorldGenerationTileScanAndControlActionsCommand.OperationKind.Count =>
          ExecuteCount(command.CountSink!),
        WorldGenerationTileScanAndControlActionsCommand.OperationKind.Scanner =>
          ExecuteCount(command.CountSink!),
        WorldGenerationTileScanAndControlActionsCommand.OperationKind.TileScanner =>
          ExecuteTileScanner(command.TileAccumulator!, observation),
        WorldGenerationTileScanAndControlActionsCommand.OperationKind.Custom =>
          ExecuteCustom(command.PerUnitAction!, observation.Position),
        WorldGenerationTileScanAndControlActionsCommand.OperationKind.UpdateBounds =>
          ExecuteBounds(command.Bounds!, observation.Position),
        _ => throw new ArgumentOutOfRangeException(nameof(command)),
      };
    }
    catch (Exception exception) when (
      policy.ExceptionHandling != ExceptionPolicy.Propagate)
    {
      return Result.Failed(
        $"CallbackException:{exception.GetType().Name}",
        policy.ExceptionHandling == ExceptionPolicy.TreatAsFailureAndStop);
    }
  }

  private static Result ExecuteContinuation(
    WorldGenerationTileScanAndControlActionsCommand command,
    TilePosition position)
  {
    bool callbackResult = command.Continuation!.Continue(position);
    // The reference ContinueWrapper ignores the wrapped action result and lets
    // UnitApply decide the chain result. With no NextAction in this boundary,
    // UnitApply succeeds while the callback result remains observable.
    return Result.Succeeded(callbackResult: callbackResult);
  }

  private static Result ExecuteCount(
    WorldGenerationTileScanAndControlActionsCommand.ICountResultSink sink)
  {
    sink.Add(1);
    return Result.Succeeded(countUpdated: true);
  }

  private static Result ExecuteTileScanner(
    WorldGenerationTileScanAndControlActionsCommand.TileCountAccumulator accumulator,
    WorldGenerationTileScanQuery.TileObservation observation)
  {
    if (!observation.Active)
    {
      return Result.Succeeded();
    }

    bool matched = accumulator.TryRecord(observation.Type);
    return Result.Succeeded(tileMatched: matched);
  }

  private static Result ExecuteCustom(
    WorldGenerationTileScanAndControlActionsCommand.IPerUnitAction action,
    TilePosition position)
  {
    bool callbackResult = action.Apply(position);
    // Actions.Custom combines the callback with UnitApply using OR. The
    // explicit boundary has no NextAction, so the unit continuation succeeds.
    return Result.Succeeded(callbackResult: callbackResult);
  }

  private static Result ExecuteBounds(
    WorldGenerationTileScanAndControlActionsCommand.IBoundsReference bounds,
    TilePosition position)
  {
    if (bounds is not IWritableBoundsReference writableBounds)
    {
      return Result.Failed("BoundsReferenceNotWritable", stopRequested: true);
    }

    writableBounds.Update(position);
    return Result.Succeeded(boundsUpdated: true);
  }

  private static void ValidatePolicy(ExecutionPolicy policy)
  {
    if (!Enum.IsDefined(policy.ExceptionHandling))
    {
      throw new ArgumentOutOfRangeException(nameof(policy));
    }
  }
}

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Terraria.WorldGeneration.Adapters;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Traverses immutable shape points and evaluates explicit, side-effect-free action descriptors.
/// </summary>
public static class ShapeTraversalQuery
{
  public enum UnitApplyResultMode : byte
  {
    UseContinuationResult,
    UseOwnResult,
    OwnResultOrContinuation,
  }

  public enum CallbackExceptionPolicy : byte
  {
    Propagate,
    TreatAsFailure,
    TreatAsFailureAndStop,
  }

  public readonly record struct OutputPolicy(
    bool RecordShapePoints,
    bool RecordActionPoints)
  {
    public static OutputPolicy LegacyCompatible => new(true, true);
  }

  public readonly record struct ActionResult(
    bool OwnResult,
    bool CalledUnitApply,
    UnitApplyResultMode ResultMode = UnitApplyResultMode.UseContinuationResult,
    string? FailureReason = null)
  {
    public static ActionResult Success()
    {
      return new ActionResult(true, true);
    }

    public static ActionResult Failure(string? reason = null, bool ignoreFailure = false)
    {
      return new ActionResult(ignoreFailure, false, UnitApplyResultMode.UseOwnResult, reason);
    }

    public static ActionResult ApplyAndCombine(
      bool ownResult,
      UnitApplyResultMode resultMode,
      string? failureReason = null)
    {
      return new ActionResult(
        ownResult,
        true,
        resultMode,
        failureReason);
    }
  }

  /// <summary>
  /// Describes one pure action evaluation in a legacy NextAction chain.
  /// The callback must use only explicit immutable inputs and must not write state or call effect ports.
  /// </summary>
  public sealed class ActionDescriptor
  {
    public ActionDescriptor(
      string id,
      Func<TilePosition, ActionResult> evaluate)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(id);
      ArgumentNullException.ThrowIfNull(evaluate);
      Id = id;
      Evaluate = evaluate;
    }

    public string Id { get; }

    public Func<TilePosition, ActionResult> Evaluate { get; }
  }

  public readonly record struct PointResult(
    WorldGenerationShapeDataDefinitionQuery.ShapePoint RelativePoint,
    TilePosition AbsolutePoint,
    bool Succeeded,
    string? FailedActionId,
    string? FailureReason);

  public readonly record struct ActionOutput(
    string ActionId,
    ImmutableArray<WorldGenerationShapeDataDefinitionQuery.ShapePoint> RelativePoints);

  public sealed class Result
  {
    internal Result(
      ImmutableArray<PointResult> points,
      ImmutableArray<WorldGenerationShapeDataDefinitionQuery.ShapePoint> shapeOutput,
      ImmutableArray<ActionOutput> actionOutputs,
      int? firstFailedPointIndex,
      bool stoppedOnFailure,
      GenerationRandomStream randomStream)
    {
      Points = points;
      ShapeOutput = shapeOutput;
      ActionOutputs = actionOutputs;
      FirstFailedPointIndex = firstFailedPointIndex;
      StoppedOnFailure = stoppedOnFailure;
      RandomStream = randomStream;
    }

    public ImmutableArray<PointResult> Points { get; }

    public ImmutableArray<WorldGenerationShapeDataDefinitionQuery.ShapePoint> ShapeOutput { get; }

    public ImmutableArray<ActionOutput> ActionOutputs { get; }

    public int? FirstFailedPointIndex { get; }

    public bool StoppedOnFailure { get; }

    public bool Succeeded => FirstFailedPointIndex is null;

    public bool Completed => !StoppedOnFailure;

    public GenerationRandomStream RandomStream { get; }

    public ImmutableArray<ShapeTraversalRandomFact> ConsumedRandomFacts =>
      ImmutableArray<ShapeTraversalRandomFact>.Empty;
  }

  /// <summary>
  /// The traversal itself consumes no random values; this type records future explicit draws.
  /// </summary>
  public readonly record struct ShapeTraversalRandomFact(
    Terraria.WorldGeneration.Adapters.GenerationRandomStream Stream,
    int MinimumInclusive,
    int MaximumExclusive,
    int Value);

  public static Result Execute(
    WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition shape,
    TilePosition origin,
    IReadOnlyList<ActionDescriptor> actions,
    WorldGenerationShapeDataDefinitionQuery.ShapeExecutionPolicy executionPolicy)
  {
    return Execute(
      shape,
      origin,
      actions,
      executionPolicy,
      OutputPolicy.LegacyCompatible,
      GenerationRandomStream.WorldGeneration,
      CallbackExceptionPolicy.Propagate);
  }

  public static Result Execute(
    WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition shape,
    TilePosition origin,
    IReadOnlyList<ActionDescriptor> actions,
    WorldGenerationShapeDataDefinitionQuery.ShapeExecutionPolicy executionPolicy,
    OutputPolicy outputPolicy,
    GenerationRandomStream randomStream = GenerationRandomStream.WorldGeneration,
    CallbackExceptionPolicy callbackExceptionPolicy = CallbackExceptionPolicy.Propagate)
  {
    ArgumentNullException.ThrowIfNull(shape);
    ArgumentNullException.ThrowIfNull(actions);
    if (!Enum.IsDefined(callbackExceptionPolicy))
    {
      throw new ArgumentOutOfRangeException(nameof(callbackExceptionPolicy));
    }

    if (!Enum.IsDefined(randomStream))
    {
      throw new ArgumentOutOfRangeException(nameof(randomStream));
    }

    for (int actionIndex = 0; actionIndex < actions.Count; actionIndex++)
    {
      ArgumentNullException.ThrowIfNull(actions[actionIndex]);
    }

    IReadOnlyList<WorldGenerationShapeDataDefinitionQuery.ShapePoint> points = shape.Points;
    List<PointResult> pointResults = new(points.Count);
    List<WorldGenerationShapeDataDefinitionQuery.ShapePoint> shapeOutput = new();
    List<List<WorldGenerationShapeDataDefinitionQuery.ShapePoint>> actionOutput =
      new(actions.Count);
    for (int actionIndex = 0; actionIndex < actions.Count; actionIndex++)
    {
      actionOutput.Add(new List<WorldGenerationShapeDataDefinitionQuery.ShapePoint>());
    }

    int? firstFailedPointIndex = null;
    bool stoppedOnFailure = false;
    for (int pointIndex = 0; pointIndex < points.Count; pointIndex++)
    {
      WorldGenerationShapeDataDefinitionQuery.ShapePoint relativePoint = points[pointIndex];
      TilePosition absolutePoint = new(
        checked(origin.X + relativePoint.X),
        checked(origin.Y + relativePoint.Y));
      if (outputPolicy.RecordShapePoints)
      {
        shapeOutput.Add(relativePoint);
      }

      bool pointSucceeded = true;
      string? failedActionId = null;
      string? failureReason = null;
      bool stopTraversal = false;
      if (actions.Count > 0)
      {
        (pointSucceeded, failedActionId, failureReason, stopTraversal) = EvaluateChain(
          actions,
          actionOutput,
          absolutePoint,
          relativePoint,
          0,
          outputPolicy.RecordActionPoints,
          callbackExceptionPolicy);
        if (!pointSucceeded)
        {
          firstFailedPointIndex ??= pointIndex;
        }
      }

      pointResults.Add(
        new PointResult(
          relativePoint,
          absolutePoint,
          pointSucceeded,
          failedActionId,
          failureReason));
      if (!pointSucceeded && (executionPolicy.QuitOnFail || stopTraversal))
      {
        stoppedOnFailure = true;
        break;
      }
    }

    ImmutableArray<ActionOutput>.Builder actionOutputs =
      ImmutableArray.CreateBuilder<ActionOutput>(actions.Count);
    for (int actionIndex = 0; actionIndex < actions.Count; actionIndex++)
    {
      actionOutputs.Add(
        new ActionOutput(
          actions[actionIndex].Id,
          ImmutableArray.CreateRange(actionOutput[actionIndex])));
    }

    return new Result(
      ImmutableArray.CreateRange(pointResults),
      ImmutableArray.CreateRange(shapeOutput),
      actionOutputs.MoveToImmutable(),
      firstFailedPointIndex,
      stoppedOnFailure,
      randomStream);
  }

  private static (
    bool Succeeded,
    string? FailedActionId,
    string? FailureReason,
    bool StopTraversal)
    EvaluateChain(
      IReadOnlyList<ActionDescriptor> actions,
      IReadOnlyList<List<WorldGenerationShapeDataDefinitionQuery.ShapePoint>> actionOutput,
      TilePosition absolutePoint,
      WorldGenerationShapeDataDefinitionQuery.ShapePoint relativePoint,
      int actionIndex,
      bool recordActionPoints,
      CallbackExceptionPolicy exceptionPolicy)
  {
    if (actionIndex >= actions.Count)
    {
      return (true, null, null, false);
    }

    ActionDescriptor descriptor = actions[actionIndex];
    ActionResult actionResult;
    try
    {
      actionResult = descriptor.Evaluate(absolutePoint);
    }
    catch (Exception exception) when (
      exceptionPolicy != CallbackExceptionPolicy.Propagate)
    {
      actionResult = ActionResult.Failure(
        $"CallbackException:{exception.GetType().Name}");
      if (exceptionPolicy == CallbackExceptionPolicy.TreatAsFailureAndStop)
      {
        return (false, descriptor.Id, actionResult.FailureReason, true);
      }
    }

    if (!Enum.IsDefined(actionResult.ResultMode))
    {
      throw new ArgumentOutOfRangeException(nameof(actionResult.ResultMode));
    }

    if (!actionResult.CalledUnitApply)
    {
      return actionResult.OwnResult
        ? (true, null, null, false)
        : (false, descriptor.Id, actionResult.FailureReason, false);
    }

    if (recordActionPoints)
    {
      actionOutput[actionIndex].Add(relativePoint);
    }

    (
      bool continuationResult,
      string? failedActionId,
      string? failureReason,
      bool continuationStop) =
      EvaluateChain(
        actions,
        actionOutput,
        absolutePoint,
        relativePoint,
        actionIndex + 1,
        recordActionPoints,
        exceptionPolicy);
    bool succeeded = actionResult.ResultMode switch
    {
      UnitApplyResultMode.UseContinuationResult => continuationResult,
      UnitApplyResultMode.UseOwnResult => actionResult.OwnResult,
      UnitApplyResultMode.OwnResultOrContinuation =>
        actionResult.OwnResult || continuationResult,
      _ => throw new ArgumentOutOfRangeException(nameof(actionResult.ResultMode)),
    };

    return succeeded
      ? (true, null, null, false)
      : (
        false,
        failedActionId ?? descriptor.Id,
        failureReason ?? actionResult.FailureReason,
        continuationStop);
  }
}

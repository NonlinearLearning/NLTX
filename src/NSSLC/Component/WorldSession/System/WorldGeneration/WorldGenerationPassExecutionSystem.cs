using System;
using Terraria.WorldGeneration.Adapters;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Definitions;
using Terraria.WorldGeneration.Passes;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Coordinates active pass execution, progress reporting, and ordered result commits.
/// </summary>
public static class WorldGenerationPassExecutionSystem
{
  public static bool TryGetNextPass(
    WorldGenerationPlanComponent plan,
    in WorldGenerationPassStateComponent current,
    WorldGenerationManifestComponent manifest,
    out GenerationPassDescriptor descriptor)
  {
    ArgumentNullException.ThrowIfNull(plan);
    ArgumentNullException.ThrowIfNull(manifest);
    EnsureGeneration(plan, in current);
    EnsureManifestResultPrefix(plan, manifest);

    int nextPassIndex = manifest.PassResults.Count;
    if (nextPassIndex == plan.PassDescriptors.Count)
    {
      if (current.HasActivePass)
      {
        throw new InvalidOperationException(
          "An active pass cannot exist after every planned pass has been committed.");
      }

      descriptor = default;
      return false;
    }

    GenerationPassDescriptor next = plan.PassDescriptors[nextPassIndex];
    if (current.HasActivePass &&
        !string.Equals(current.ActivePassId, next.Id, StringComparison.Ordinal))
    {
      throw new InvalidOperationException(
        "The active pass does not match the next uncommitted plan entry.");
    }

    if (current.HasFailure || current.AbortRequested)
    {
      descriptor = default;
      return false;
    }

    descriptor = next;
    return true;
  }

  public static void RefreshCommittedProgress(
    WorldGenerationPlanComponent plan,
    in WorldGenerationPassStateComponent current,
    WorldGenerationManifestComponent manifest,
    WorldGenerationProgressComponent progress)
  {
    ArgumentNullException.ThrowIfNull(plan);
    ArgumentNullException.ThrowIfNull(manifest);
    ArgumentNullException.ThrowIfNull(progress);
    EnsureGeneration(plan, in current);
    EnsureManifestResultPrefix(plan, manifest);

    double totalWeight = 0;
    double totalWeightedProgress = 0;
    for (int index = 0; index < plan.PassDescriptors.Count; index++)
    {
      GenerationPassDescriptor descriptor = plan.PassDescriptors[index];
      if (plan.DisabledPassIds.Contains(descriptor.Id, StringComparer.Ordinal))
      {
        continue;
      }

      totalWeight += descriptor.Weight;
      if (index < manifest.PassResults.Count)
      {
        totalWeightedProgress += descriptor.Weight;
      }
    }

    progress.ReplaceState(
      progress.MessageNoFormatting,
      progress.Value,
      totalWeightedProgress,
      totalWeight,
      progress.CurrentPassWeight);
  }

  public static WorldGenerationPassStartResult BeginPass(
    WorldGenerationPlanComponent plan,
    in WorldGenerationPassStateComponent current,
    string passId,
    WorldGenerationStage stage,
    ulong readSnapshotRevision)
  {
    ArgumentNullException.ThrowIfNull(plan);
    ArgumentException.ThrowIfNullOrWhiteSpace(passId);
    EnsureGeneration(plan, in current);
    EnsurePassStage(stage);

    GenerationPassDescriptor descriptor = FindDescriptor(plan, passId);
    if (current.HasActivePass)
    {
      throw new InvalidOperationException(
        "A generation pass cannot begin while another pass is active.");
    }

    if (current.HasFailure || current.AbortRequested)
    {
      throw new InvalidOperationException(
        "A failed or aborted generation cannot begin another pass.");
    }

    if (plan.DisabledPassIds.Contains(passId, StringComparer.Ordinal))
    {
      return new WorldGenerationPassStartResult(
        WorldGenerationPassStartStatus.SkippedDisabled,
        current);
    }

    WorldGenerationPassStateComponent next = new(
      current.GenerationId,
      stage,
      descriptor.Id,
      descriptor.Version,
      cursor: 0,
      readSnapshotRevision,
      current.CheckpointRevision,
      failureReason: null,
      current.PauseRequested,
      abortRequested: false);
    return new WorldGenerationPassStartResult(
      WorldGenerationPassStartStatus.Started,
      next);
  }

  /// <summary>
  /// Starts the pass selected by the committed manifest prefix.
  /// </summary>
  public static WorldGenerationPassStartResult BeginPass(
    WorldGenerationPlanComponent plan,
    in WorldGenerationPassStateComponent current,
    WorldGenerationManifestComponent manifest,
    string passId,
    WorldGenerationStage stage,
    ulong readSnapshotRevision)
  {
    ArgumentNullException.ThrowIfNull(plan);
    ArgumentNullException.ThrowIfNull(manifest);
    ArgumentException.ThrowIfNullOrWhiteSpace(passId);
    EnsureGeneration(plan, in current);
    EnsureManifestResultPrefix(plan, manifest);

    int nextPassIndex = manifest.PassResults.Count;
    if (nextPassIndex >= plan.PassDescriptors.Count)
    {
      throw new InvalidOperationException(
        "All generation passes have already been committed.");
    }

    GenerationPassDescriptor nextDescriptor = plan.PassDescriptors[nextPassIndex];
    if (!string.Equals(nextDescriptor.Id, passId, StringComparison.Ordinal))
    {
      throw new InvalidOperationException(
        "A generation pass must begin at the next uncommitted plan entry.");
    }

    return BeginPass(
      plan,
      in current,
      passId,
      stage,
      readSnapshotRevision);
  }

  public static WorldGenerationPassStateComponent CompletePass(
    WorldGenerationPlanComponent plan,
    in WorldGenerationPassStateComponent current,
    string passId,
    WorldGenerationStage nextStage,
    ulong checkpointRevision)
  {
    ArgumentNullException.ThrowIfNull(plan);
    ArgumentException.ThrowIfNullOrWhiteSpace(passId);
    EnsureGeneration(plan, in current);
    EnsurePassStage(nextStage);
    if (!current.HasActivePass ||
        !string.Equals(current.ActivePassId, passId, StringComparison.Ordinal))
    {
      throw new InvalidOperationException(
        "Only the active generation pass can be completed.");
    }

    if (current.HasFailure || current.AbortRequested)
    {
      throw new InvalidOperationException(
        "A failed or aborted generation pass cannot be completed.");
    }

    return new WorldGenerationPassStateComponent(
      current.GenerationId,
      nextStage,
      activePassId: null,
      passVersion: 0,
      cursor: 0,
      readSnapshotRevision: null,
      checkpointRevision,
      failureReason: null,
      current.PauseRequested,
      abortRequested: false);
  }

  public static WorldGenerationPassStateComponent CompletePassWithResult(
    WorldGenerationPlanComponent plan,
    in WorldGenerationPassStateComponent current,
    WorldGenerationManifestComponent manifest,
    WorldGenerationPassResultComponent result,
    WorldGenerationStage nextStage,
    ulong checkpointRevision)
  {
    ArgumentNullException.ThrowIfNull(plan);
    ArgumentNullException.ThrowIfNull(manifest);
    ArgumentNullException.ThrowIfNull(result);
    EnsureGeneration(plan, in current);
    EnsurePassResultOrder(plan, manifest, result);

    if (result.Skipped)
    {
      throw new ArgumentException(
        "A skipped result cannot complete an active pass.",
        nameof(result));
    }

    if (plan.DisabledPassIds.Contains(result.PassId, StringComparer.Ordinal))
    {
      throw new ArgumentException(
        "A disabled pass must be committed as skipped.",
        nameof(result));
    }

    WorldGenerationPassStateComponent completed = CompletePass(
      plan,
      in current,
      result.PassId,
      nextStage,
      checkpointRevision);
    manifest.AppendPassResult(result);
    return completed;
  }

  public static WorldGenerationPassExecutionAttempt ExecuteActivePass(
    WorldGenerationPlanComponent plan,
    in WorldGenerationPassStateComponent current,
    WorldGenerationManifestComponent manifest,
    WorldGenerationProgressComponent progress,
    WorldGenerationConfigurationSnapshot configuration,
    IWorldGenerationPassRunner runner,
    WorldGenerationStage nextStage,
    ulong checkpointRevision)
  {
    ArgumentNullException.ThrowIfNull(plan);
    ArgumentNullException.ThrowIfNull(manifest);
    ArgumentNullException.ThrowIfNull(progress);
    ArgumentNullException.ThrowIfNull(runner);
    EnsurePassStage(nextStage);

    WorldGenerationPassStateComponent activeState = current;
    GenerationPassDescriptor descriptor = GetActivePassDescriptor(
      plan,
      in activeState,
      manifest);
    int passIndex = manifest.PassResults.Count;
    ReportActivePassProgress(
      plan,
      in activeState,
      manifest,
      progress,
      progress.MessageNoFormatting,
      0);

    WorldGenerationPassRunOutput output;
    try
    {
      output = runner.Execute(
        in activeState,
        descriptor,
        configuration,
        (message, value) => ReportActivePassProgress(
          plan,
          in activeState,
          manifest,
          progress,
          message,
          value)) ?? throw new InvalidOperationException(
            "The generation pass runner returned no output.");
    }
    catch (OperationCanceledException)
    {
      throw;
    }
    catch (Exception exception)
    {
      WorldGenerationPassStateComponent failed = FailPass(
        plan,
        in activeState,
        DescribeFailure(exception));
      return WorldGenerationPassExecutionAttempt.Failed(failed, exception);
    }

    WorldGenerationPassResultComponent result = new(
      plan.GenerationId,
      passIndex,
      descriptor.Id,
      output.DurationMs,
      output.Hash,
      skipped: false,
      randomNextValue: output.RandomNextValue);
    WorldGenerationPassStateComponent completed = CompletePassWithResult(
      plan,
      in activeState,
      manifest,
      result,
      nextStage,
      checkpointRevision);
    RefreshCommittedProgress(plan, in completed, manifest, progress);
    progress.ReplaceState(
      progress.MessageNoFormatting,
      0,
      progress.TotalWeightedProgress,
      progress.TotalWeight,
      progress.CurrentPassWeight);
    return WorldGenerationPassExecutionAttempt.Completed(completed, result);
  }

  public static void ReportActivePassProgress(
    WorldGenerationPlanComponent plan,
    in WorldGenerationPassStateComponent current,
    WorldGenerationManifestComponent manifest,
    WorldGenerationProgressComponent progress,
    string messageNoFormatting,
    double value)
  {
    ArgumentNullException.ThrowIfNull(plan);
    ArgumentNullException.ThrowIfNull(manifest);
    ArgumentNullException.ThrowIfNull(progress);
    ArgumentNullException.ThrowIfNull(messageNoFormatting);
    if (!double.IsFinite(value))
    {
      throw new ArgumentOutOfRangeException(nameof(value));
    }

    GenerationPassDescriptor descriptor = GetActivePassDescriptor(
      plan,
      in current,
      manifest);
    RefreshCommittedProgress(plan, in current, manifest, progress);
    progress.ReplaceState(
      messageNoFormatting,
      value,
      progress.TotalWeightedProgress,
      progress.TotalWeight,
      descriptor.Weight);
  }

  public static void CommitSkippedPassResult(
    WorldGenerationPlanComponent plan,
    in WorldGenerationPassStateComponent current,
    WorldGenerationManifestComponent manifest,
    WorldGenerationPassResultComponent result)
  {
    ArgumentNullException.ThrowIfNull(plan);
    ArgumentNullException.ThrowIfNull(manifest);
    ArgumentNullException.ThrowIfNull(result);
    EnsureGeneration(plan, in current);
    EnsurePassResultOrder(plan, manifest, result);

    if (current.HasActivePass || current.HasFailure || current.AbortRequested)
    {
      throw new InvalidOperationException(
        "A skipped result cannot be committed while a pass is active, failed, or aborted.");
    }

    if (!result.Skipped ||
        !plan.DisabledPassIds.Contains(result.PassId, StringComparer.Ordinal))
    {
      throw new ArgumentException(
        "Only a disabled pass can be committed as skipped.",
        nameof(result));
    }

    manifest.AppendPassResult(result);
  }

  public static WorldGenerationPassStateComponent FailPass(
    WorldGenerationPlanComponent plan,
    in WorldGenerationPassStateComponent current,
    string reason)
  {
    ArgumentNullException.ThrowIfNull(plan);
    ArgumentException.ThrowIfNullOrWhiteSpace(reason);
    EnsureGeneration(plan, in current);
    if (!current.HasActivePass)
    {
      throw new InvalidOperationException(
        "Only an active generation pass can be failed.");
    }

    return new WorldGenerationPassStateComponent(
      current.GenerationId,
      current.Stage,
      current.ActivePassId,
      current.PassVersion,
      current.Cursor,
      current.ReadSnapshotRevision,
      current.CheckpointRevision,
      reason,
      current.PauseRequested,
      current.AbortRequested);
  }

  public static WorldGenerationPassStateComponent RequestPause(
    WorldGenerationPlanComponent plan,
    in WorldGenerationPassStateComponent current)
  {
    ArgumentNullException.ThrowIfNull(plan);
    EnsureGeneration(plan, in current);
    return WithControlFlags(in current, pauseRequested: true, current.AbortRequested);
  }

  public static WorldGenerationPassStateComponent RequestAbort(
    WorldGenerationPlanComponent plan,
    in WorldGenerationPassStateComponent current)
  {
    ArgumentNullException.ThrowIfNull(plan);
    EnsureGeneration(plan, in current);
    return WithControlFlags(in current, current.PauseRequested, abortRequested: true);
  }

  private static WorldGenerationPassStateComponent WithControlFlags(
    in WorldGenerationPassStateComponent current,
    bool pauseRequested,
    bool abortRequested)
  {
    return new WorldGenerationPassStateComponent(
      current.GenerationId,
      current.Stage,
      current.ActivePassId,
      current.PassVersion,
      current.Cursor,
      current.ReadSnapshotRevision,
      current.CheckpointRevision,
      current.FailureReason,
      pauseRequested,
      abortRequested);
  }

  private static GenerationPassDescriptor FindDescriptor(
    WorldGenerationPlanComponent plan,
    string passId)
  {
    foreach (GenerationPassDescriptor descriptor in plan.PassDescriptors)
    {
      if (string.Equals(descriptor.Id, passId, StringComparison.Ordinal))
      {
        return descriptor;
      }
    }

    throw new ArgumentException(
      "The pass is not part of the generation plan.",
      nameof(passId));
  }

  private static GenerationPassDescriptor GetActivePassDescriptor(
    WorldGenerationPlanComponent plan,
    in WorldGenerationPassStateComponent current,
    WorldGenerationManifestComponent manifest)
  {
    if (!current.HasActivePass ||
        current.PauseRequested ||
        current.AbortRequested ||
        current.HasFailure)
    {
      throw new InvalidOperationException(
        "Only the next unpaused plan pass can be executed while active.");
    }

    if (!TryGetNextPass(plan, in current, manifest, out GenerationPassDescriptor descriptor) ||
        !string.Equals(current.ActivePassId, descriptor.Id, StringComparison.Ordinal) ||
        current.PassVersion != descriptor.Version)
    {
      throw new InvalidOperationException(
        "Only the next unpaused plan pass can be executed while active.");
    }

    if (plan.DisabledPassIds.Contains(descriptor.Id, StringComparer.Ordinal))
    {
      throw new InvalidOperationException(
        "A disabled pass cannot be executed as active work.");
    }

    return descriptor;
  }

  private static string DescribeFailure(Exception exception)
  {
    string typeName = exception.GetType().FullName ?? exception.GetType().Name;
    return string.IsNullOrWhiteSpace(exception.Message)
      ? typeName
      : $"{typeName}: {exception.Message}";
  }

  private static void EnsurePassResultOrder(
    WorldGenerationPlanComponent plan,
    WorldGenerationManifestComponent manifest,
    WorldGenerationPassResultComponent result)
  {
    if (result.GenerationId != plan.GenerationId)
    {
      throw new ArgumentException(
        "The pass result belongs to another generation.",
        nameof(result));
    }

    EnsureManifestResultPrefix(plan, manifest);

    if (result.PassIndex != manifest.PassResults.Count)
    {
      throw new InvalidOperationException(
        "Pass results must be committed in plan order.");
    }

    if (result.PassIndex >= plan.PassDescriptors.Count ||
        !string.Equals(
          plan.PassDescriptors[result.PassIndex].Id,
          result.PassId,
          StringComparison.Ordinal))
    {
      throw new ArgumentException(
        "The pass result does not match its position in the generation plan.",
        nameof(result));
    }
  }

  private static void EnsureManifestResultPrefix(
    WorldGenerationPlanComponent plan,
    WorldGenerationManifestComponent manifest)
  {
    if (manifest.PassResults.Count > plan.PassDescriptors.Count)
    {
      throw new InvalidOperationException(
        "The manifest contains more pass results than the generation plan.");
    }

    for (int index = 0; index < manifest.PassResults.Count; index++)
    {
      WorldGenerationPassResultComponent committed = manifest.PassResults[index];
      if (committed.GenerationId != plan.GenerationId ||
          committed.PassIndex != index ||
          !string.Equals(
            committed.PassId,
            plan.PassDescriptors[index].Id,
            StringComparison.Ordinal))
      {
        throw new InvalidOperationException(
          "The manifest result prefix does not match the generation plan.");
      }

      bool disabled = plan.DisabledPassIds.Contains(
        committed.PassId,
        StringComparer.Ordinal);
      if (committed.Skipped != disabled)
      {
        throw new InvalidOperationException(
          "The manifest result skipped state does not match the generation plan.");
      }
    }
  }

  private static void EnsureGeneration(
    WorldGenerationPlanComponent plan,
    in WorldGenerationPassStateComponent current)
  {
    if (plan.GenerationId != current.GenerationId)
    {
      throw new ArgumentException(
        "The pass state belongs to another generation.",
        nameof(current));
    }
  }

  private static void EnsurePassStage(WorldGenerationStage stage)
  {
    if (!Enum.IsDefined(stage) ||
        stage is WorldGenerationStage.Created or
        WorldGenerationStage.Committed or
        WorldGenerationStage.Validated)
    {
      throw new ArgumentOutOfRangeException(nameof(stage));
    }
  }
}

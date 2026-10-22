using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MinimalImplementation.Persistence;

/// <summary>Runs one idempotent stop sequence and retains its completion for every caller.</summary>
public sealed class WorldStopCoordinator
{
  private readonly object _gate = new();
  private readonly IWorldHostStopBoundary _boundary;
  private readonly Func<CancellationToken, Task<WorldDocumentSaveResult>> _save;
  private Task<WorldStopResult>? _completion;

  public WorldStopCoordinator(
    IWorldHostStopBoundary boundary,
    Func<CancellationToken, Task<WorldDocumentSaveResult>> save)
  {
    ArgumentNullException.ThrowIfNull(boundary);
    ArgumentNullException.ThrowIfNull(save);
    _boundary = boundary;
    _save = save;
  }

  public Task<WorldStopResult> StopAsync(
    bool save,
    TimeSpan deadline,
    CancellationToken cancellationToken = default)
  {
    if (deadline <= TimeSpan.Zero)
    {
      throw new ArgumentOutOfRangeException(nameof(deadline));
    }

    lock (_gate)
    {
      _completion ??= StopCoreAsync(save, deadline, cancellationToken);
      return _completion;
    }
  }

  private async Task<WorldStopResult> StopCoreAsync(
    bool save,
    TimeSpan deadline,
    CancellationToken cancellationToken)
  {
    using var deadlineSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    deadlineSource.CancelAfter(deadline);
    CancellationToken token = deadlineSource.Token;
    WorldDocumentSaveResult? saveResult = null;

    WorldStopStage failedStage = await RunStageAsync(
      WorldStopStage.StopAdmissions,
      () => _boundary.StopAdmissionsAsync(token),
      token).ConfigureAwait(false);
    if (failedStage == WorldStopStage.None)
    {
      failedStage = await RunStageAsync(
        WorldStopStage.StopTickAdmission,
        () => _boundary.StopTickAdmissionAsync(token),
        token).ConfigureAwait(false);
    }
    if (failedStage == WorldStopStage.None)
    {
      failedStage = await RunStageAsync(
        WorldStopStage.DrainAcceptedCommands,
        () => _boundary.DrainAcceptedCommandsAsync(token),
        token).ConfigureAwait(false);
    }

    string? failureDetail = null;
    if (failedStage == WorldStopStage.None && save)
    {
      failedStage = WorldStopStage.Save;
      try
      {
        saveResult = await _save(token).WaitAsync(token).ConfigureAwait(false);
        if (!saveResult.IsPrimaryCommitted)
        {
          failureDetail = saveResult.CommitResult.Failure.Detail ??
            "The primary world document was not committed.";
        }
        failedStage = saveResult.IsPrimaryCommitted ? WorldStopStage.None : WorldStopStage.Save;
      }
      catch (Exception exception) when (IsExpectedStopFailure(exception))
      {
        failureDetail = exception.Message;
      }
    }

    if (failedStage == WorldStopStage.None)
    {
      failedStage = await RunStageAsync(
        WorldStopStage.CloseTransport,
        () => _boundary.CloseTransportAsync(token),
        token).ConfigureAwait(false);
    }
    if (failedStage == WorldStopStage.None)
    {
      failedStage = await RunStageAsync(
        WorldStopStage.RetireWorldAndResources,
        () => _boundary.RetireWorldAndResourcesAsync(token),
        token).ConfigureAwait(false);
    }
    if (failedStage == WorldStopStage.None)
    {
      failedStage = await RunStageAsync(
        WorldStopStage.StopCommitLoop,
        () => _boundary.StopCommitLoopAsync(token),
        token).ConfigureAwait(false);
    }

    if (failedStage == WorldStopStage.None)
    {
      return new WorldStopResult(true, save, saveResult, WorldStopStage.Stopped, null);
    }

    return await FinishAfterFailureAsync(
      save,
      saveResult,
      failedStage,
      failureDetail ??
        (token.IsCancellationRequested
          ? "The stop deadline elapsed or the operation was canceled."
          : "A host stop boundary failed."),
      deadline).ConfigureAwait(false);
  }

  private async Task<WorldStopResult> FinishAfterFailureAsync(
    bool save,
    WorldDocumentSaveResult? saveResult,
    WorldStopStage failedStage,
    string detail,
    TimeSpan cleanupDeadline)
  {
    // The original stop token may already be canceled because the save or an earlier
    // boundary reached its deadline. Each cleanup stage gets its own bounded token so a
    // timed-out close does not prevent the world and commit loop from receiving cleanup.
    if (failedStage is WorldStopStage.StopAdmissions or WorldStopStage.StopTickAdmission or
        WorldStopStage.DrainAcceptedCommands or WorldStopStage.Save)
    {
      await RunCleanupStageAsync(
        WorldStopStage.CloseTransport,
        _boundary.CloseTransportAsync,
        cleanupDeadline).ConfigureAwait(false);
      await RunCleanupStageAsync(
        WorldStopStage.RetireWorldAndResources,
        _boundary.RetireWorldAndResourcesAsync,
        cleanupDeadline).ConfigureAwait(false);
      await RunCleanupStageAsync(
        WorldStopStage.StopCommitLoop,
        _boundary.StopCommitLoopAsync,
        cleanupDeadline).ConfigureAwait(false);
    }
    else if (failedStage == WorldStopStage.CloseTransport)
    {
      await RunCleanupStageAsync(
        WorldStopStage.RetireWorldAndResources,
        _boundary.RetireWorldAndResourcesAsync,
        cleanupDeadline).ConfigureAwait(false);
      await RunCleanupStageAsync(
        WorldStopStage.StopCommitLoop,
        _boundary.StopCommitLoopAsync,
        cleanupDeadline).ConfigureAwait(false);
    }
    else if (failedStage == WorldStopStage.RetireWorldAndResources)
    {
      await RunCleanupStageAsync(
        WorldStopStage.StopCommitLoop,
        _boundary.StopCommitLoopAsync,
        cleanupDeadline).ConfigureAwait(false);
    }

    return new WorldStopResult(false, save, saveResult, failedStage, detail);
  }

  private static async Task RunCleanupStageAsync(
    WorldStopStage stage,
    Func<CancellationToken, Task> operation,
    TimeSpan deadline)
  {
    using var cleanupSource = new CancellationTokenSource();
    cleanupSource.CancelAfter(deadline);
    CancellationToken cleanupToken = cleanupSource.Token;
    await RunStageAsync(
      stage,
      () => operation(cleanupToken),
      cleanupToken).ConfigureAwait(false);
  }

  private static async Task<WorldStopStage> RunStageAsync(
    WorldStopStage stage,
    Func<Task> operation,
    CancellationToken cancellationToken)
  {
    try
    {
      await operation().WaitAsync(cancellationToken).ConfigureAwait(false);
      return WorldStopStage.None;
    }
    catch (Exception exception) when (IsExpectedStopFailure(exception))
    {
      return stage;
    }
  }

  private static bool IsExpectedStopFailure(Exception exception) => exception is
    OperationCanceledException or TimeoutException or IOException or UnauthorizedAccessException or
    InvalidDataException;
}

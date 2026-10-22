using System;
using System.Threading;
using System.Threading.Tasks;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Persistence;

/// <summary>Captures on the owner queue, then performs file I/O using only the detached document.</summary>
public sealed class WorldDocumentSaveCoordinator
{
  private readonly IWorldCommitQueue _commitQueue;
  private readonly WorldDocumentSaver _saver;
  private readonly Func<long, WorldDocumentSaveSnapshot> _capture;

  public WorldDocumentSaveCoordinator(
    IWorldCommitQueue commitQueue,
    WorldDocumentSaver saver,
    Func<long, WorldDocumentSaveSnapshot> capture)
  {
    ArgumentNullException.ThrowIfNull(commitQueue);
    ArgumentNullException.ThrowIfNull(saver);
    ArgumentNullException.ThrowIfNull(capture);
    _commitQueue = commitQueue;
    _saver = saver;
    _capture = capture;
  }

  public async Task<WorldDocumentSaveResult> SaveAsync(
    long worldScope,
    CommitVersion requestedVersion,
    string primaryPath,
    int maximumFileBytes,
    WorldBackupPolicy backupPolicy,
    CancellationToken cancellationToken = default)
  {
    if (worldScope <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldScope));
    }

    if (!requestedVersion.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(requestedVersion));
    }

    WorldDocumentSaveSnapshot? snapshot = null;
    await _commitQueue.ExecuteAsync(
      () => snapshot = _capture(worldScope),
      cancellationToken).ConfigureAwait(false);

    if (snapshot is null || snapshot.WorldScope != worldScope)
    {
      throw new InvalidOperationException("The save capture did not return the requested world scope.");
    }

    if (snapshot.CapturedVersion.Value < requestedVersion.Value)
    {
      throw new InvalidOperationException(
        "The captured world version is older than the requested save version.");
    }

    var request = new WorldDocumentSaveRequest(
      primaryPath,
      snapshot.CapturedVersion,
      snapshot.Document,
      maximumFileBytes,
      backupPolicy);
    return await _saver.SaveAsync(request, cancellationToken).ConfigureAwait(false);
  }
}

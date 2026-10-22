using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>
/// Result of one stable NPC capture. Commit records and packet-23 snapshots are ordered by
/// NPC owner commit version; packet 28 producers can merge by that same key.
/// </summary>
public sealed class NpcTickBatchResult {
  internal NpcTickBatchResult(
      NpcTickFrameContext frame,
      OwnerCommitResult result,
      IReadOnlyList<NpcTickEntryResult> entries,
      IReadOnlyList<NpcTickCommitRecord> commits,
      IReadOnlyList<NpcView> packet23Snapshots,
      int capturedNpcCount,
      bool isComplete) {
    ArgumentNullException.ThrowIfNull(frame);
    ArgumentNullException.ThrowIfNull(result);
    ArgumentNullException.ThrowIfNull(entries);
    ArgumentNullException.ThrowIfNull(commits);
    ArgumentNullException.ThrowIfNull(packet23Snapshots);
    Frame = frame;
    Result = result;
    Entries = Array.AsReadOnly(entries.ToArray());
    Commits = Array.AsReadOnly(commits.ToArray());
    Packet23Snapshots = Array.AsReadOnly(packet23Snapshots.ToArray());
    CapturedNpcCount = capturedNpcCount;
    IsComplete = isComplete;
  }

  public NpcTickFrameContext Frame { get; }

  public OwnerCommitResult Result { get; }

  public IReadOnlyList<NpcTickEntryResult> Entries { get; }

  public IReadOnlyList<NpcTickCommitRecord> Commits { get; }

  /// <summary>Latest dirty snapshot per entity, ordered by its final NPC commit version.</summary>
  public IReadOnlyList<NpcView> Packet23Snapshots { get; }

  public int CapturedNpcCount { get; }

  /// <summary>False if any captured NPC was unsupported, rejected, or left unprocessed.</summary>
  public bool IsComplete { get; }
}

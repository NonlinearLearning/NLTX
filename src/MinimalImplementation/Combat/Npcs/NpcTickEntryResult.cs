using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>Per-captured-NPC progress, including any successfully committed prefix.</summary>
public sealed class NpcTickEntryResult {
  public enum Disposition {
    Committed,
    SkippedDead,
    UnsupportedType,
    Rejected,
    NotProcessed,
    Interrupted
  }

  internal NpcTickEntryResult(
      NpcView capturedNpc,
      Disposition disposition,
      OwnerCommitResult? aiResult,
      OwnerCommitResult? activityResult,
      NpcView? latestCommittedNpc,
      string? detail) {
    ArgumentNullException.ThrowIfNull(capturedNpc);
    CapturedNpc = capturedNpc;
    Status = disposition;
    AiResult = aiResult;
    ActivityResult = activityResult;
    LatestCommittedNpc = latestCommittedNpc;
    Detail = detail;
  }

  public NpcView CapturedNpc { get; }

  public Disposition Status { get; }

  public OwnerCommitResult? AiResult { get; }

  public OwnerCommitResult? ActivityResult { get; }

  public NpcView? LatestCommittedNpc { get; }

  public string? Detail { get; }
}

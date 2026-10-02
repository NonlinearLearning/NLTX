namespace Terraria.DeathPenaltyAndRevenge;

public readonly record struct RevengeRegistryCommitResult(
  bool Accepted,
  bool RejectedAsStale,
  bool Missing,
  bool Duplicate,
  uint CurrentRevision,
  RevengeMarkerSnapshot? Snapshot)
{
  public static RevengeRegistryCommitResult Stale(
    uint currentRevision,
    RevengeMarkerSnapshot? snapshot = null)
  {
    return new RevengeRegistryCommitResult(
      Accepted: false,
      RejectedAsStale: true,
      Missing: false,
      Duplicate: false,
      CurrentRevision: currentRevision,
      Snapshot: snapshot);
  }

  public static RevengeRegistryCommitResult MissingMarker(uint currentRevision)
  {
    return new RevengeRegistryCommitResult(
      Accepted: false,
      RejectedAsStale: false,
      Missing: true,
      Duplicate: false,
      CurrentRevision: currentRevision,
      Snapshot: null);
  }

  public static RevengeRegistryCommitResult DuplicateMarker(
    uint currentRevision,
    RevengeMarkerSnapshot snapshot)
  {
    return new RevengeRegistryCommitResult(
      Accepted: false,
      RejectedAsStale: false,
      Missing: false,
      Duplicate: true,
      CurrentRevision: currentRevision,
      Snapshot: snapshot);
  }

  public static RevengeRegistryCommitResult AcceptedMarker(
    uint currentRevision,
    RevengeMarkerSnapshot snapshot)
  {
    return new RevengeRegistryCommitResult(
      Accepted: true,
      RejectedAsStale: false,
      Missing: false,
      Duplicate: false,
      CurrentRevision: currentRevision,
      Snapshot: snapshot);
  }
}

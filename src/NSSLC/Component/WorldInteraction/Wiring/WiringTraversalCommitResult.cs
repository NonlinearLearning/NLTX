namespace Terraria.WorldInteraction.Wiring;

public readonly record struct WiringTraversalCommitResult(
  WiringTraversalCommitStatus Status,
  bool Changed,
  string? FailureReason)
{
  public bool Succeeded => Status is
    WiringTraversalCommitStatus.Accepted or
    WiringTraversalCommitStatus.Duplicate;

  public bool WasDuplicate => Status == WiringTraversalCommitStatus.Duplicate;

  public static WiringTraversalCommitResult Accepted(bool changed)
  {
    return new(WiringTraversalCommitStatus.Accepted, changed, null);
  }

  public static WiringTraversalCommitResult Duplicate =>
    new(WiringTraversalCommitStatus.Duplicate, false, null);

  public static WiringTraversalCommitResult Unknown(string reason)
  {
    return new(WiringTraversalCommitStatus.Unknown, false, reason);
  }

  public static WiringTraversalCommitResult Rejected(string reason)
  {
    return new(WiringTraversalCommitStatus.Rejected, false, reason);
  }

  public static WiringTraversalCommitResult Failed(string reason)
  {
    return new(WiringTraversalCommitStatus.Failed, false, reason);
  }
}

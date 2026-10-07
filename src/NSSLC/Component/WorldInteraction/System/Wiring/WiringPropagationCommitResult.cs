namespace Terraria.WorldInteraction.Wiring;

public readonly record struct WiringPropagationCommitResult(
  WiringPropagationCommitStatus Status,
  bool Changed,
  string? FailureReason)
{
  public bool Succeeded => Status != WiringPropagationCommitStatus.Rejected;

  public bool WasDuplicate => Status == WiringPropagationCommitStatus.Duplicate;

  public static WiringPropagationCommitResult Accepted(bool changed)
  {
    return new(WiringPropagationCommitStatus.Accepted, changed, null);
  }

  public static WiringPropagationCommitResult Duplicate =>
    new(WiringPropagationCommitStatus.Duplicate, false, null);

  public static WiringPropagationCommitResult Reset(bool changed)
  {
    return new(WiringPropagationCommitStatus.Reset, changed, null);
  }

  public static WiringPropagationCommitResult Rejected(string reason)
  {
    return new(WiringPropagationCommitStatus.Rejected, false, reason);
  }
}

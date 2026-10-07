namespace Terraria.DeathPenaltyAndRevenge;

public readonly record struct RevengeEnemyCaptureResult(
  bool CanCapture,
  RevengeEnemyCaptureRejectionReason RejectionReason)
{
  public bool IsRejected => !CanCapture;
}

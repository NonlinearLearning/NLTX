namespace Terraria.WorldSession.Components;

public readonly record struct SessionReadinessSnapshotValue(
  SessionReadinessPhase Phase,
  bool GenerationBarrierActive,
  int? FailureStatusCode)
{
  public bool CanUpdateEntities =>
    Phase == SessionReadinessPhase.Ready &&
    !GenerationBarrierActive &&
    FailureStatusCode is null;
}

namespace Terraria.WorldSession.Components;

public sealed class SessionReadinessState
{
  public SessionReadinessPhase Phase = SessionReadinessPhase.AwaitingData;
  public bool GenerationBarrierActive;
  public int? FailureStatusCode;

  public bool CanUpdateEntities =>
    Phase == SessionReadinessPhase.Ready &&
    !GenerationBarrierActive &&
    FailureStatusCode is null;
}

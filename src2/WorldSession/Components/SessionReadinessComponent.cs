namespace Terraria.WorldSession.Components;

public sealed class SessionReadinessComponent
{
  public SessionReadinessPhase Phase { get; internal set; } = SessionReadinessPhase.AwaitingData;

  public bool InMenu { get; internal set; }

  public string? FailureCode { get; internal set; }

  public bool GenerationBarrierActive { get; internal set; }
}

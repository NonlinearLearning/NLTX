using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Session;

public sealed class SessionAdmissionPreparation
{
  private SessionAdmissionPreparation(
    OwnerCommitResult? rejection,
    PreparedSessionTransition? transition)
  {
    Rejection = rejection;
    Transition = transition;
  }

  public OwnerCommitResult? Rejection { get; }

  public PreparedSessionTransition? Transition { get; }

  public bool IsReady => Transition is not null;

  public object? ResponsePacket => Transition?.ResponsePacket;

  public byte? ResponseMessageId => Transition?.ResponseMessageId;

  public static SessionAdmissionPreparation Failed(OwnerCommitResult result) => new(result, null);

  internal static SessionAdmissionPreparation Ready(PreparedSessionTransition transition) =>
    new(null, transition);
}

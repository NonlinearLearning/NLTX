namespace Terraria.Npc;

public sealed class NpcAiProfileRegistrationException : InvalidOperationException
{
  public NpcAiProfileRegistrationException(
    NpcAiProfileRegistrationFailure failure,
    NpcAiProfileIdentity identity,
    string message)
    : base(message)
  {
    Failure = failure;
    Identity = identity;
  }

  public NpcAiProfileRegistrationFailure Failure { get; }

  public NpcAiProfileIdentity Identity { get; }
}

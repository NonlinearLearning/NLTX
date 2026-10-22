using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Modules.Social.Chat;

public sealed record ChatCommandResult(
  bool IsHandled,
  ProtocolErrorCode? ErrorCode,
  CommitVersion? CommitVersion,
  IReadOnlyList<OutboundPacketIntent> OutboundIntents,
  string? Detail)
{
  public static ChatCommandResult Handled(
    CommitVersion commitVersion,
    IReadOnlyCollection<OutboundPacketIntent> outboundIntents)
  {
    ArgumentNullException.ThrowIfNull(outboundIntents);
    return new ChatCommandResult(
      true,
      null,
      commitVersion,
      Array.AsReadOnly(outboundIntents.ToArray()),
      null);
  }

  public static ChatCommandResult HandledWithoutCommit()
  {
    return new ChatCommandResult(
      true,
      null,
      null,
      Array.Empty<OutboundPacketIntent>(),
      null);
  }

  public static ChatCommandResult Rejected(ProtocolErrorCode errorCode, string detail)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(detail);
    return new ChatCommandResult(
      false,
      errorCode,
      null,
      Array.Empty<OutboundPacketIntent>(),
      detail);
  }
}

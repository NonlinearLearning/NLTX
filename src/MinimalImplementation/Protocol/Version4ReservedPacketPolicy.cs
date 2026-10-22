namespace MinimalImplementation.Server.Protocol;

public enum Version4PacketHandling
{
  Typed,
  ConsumedReserved,
  ConsumedUnknownPacket,
  ConsumedUnknownModule,
  RejectStage,
  RejectDirection
}

public sealed record Version4PacketPolicyResult(
  byte MessageId,
  Version4PacketHandling Handling,
  string Reason);

/// <summary>Classifies Version4 compatibility packets before typed owner dispatch.</summary>
public static class Version4ReservedPacketPolicy
{
  private static readonly IReadOnlyList<byte> ReservedMessageIds =
    Array.AsReadOnly<byte>([0, 15, 25, 26, 44, 67, 83, 94]);

  public static IReadOnlyList<byte> CompatibilityMessageIds => ReservedMessageIds;

  public static Version4PacketPolicyResult Evaluate(
    byte messageId,
    string stage,
    bool enableDebugCommands)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(stage);
    if (!IsReserved(messageId))
    {
      return new Version4PacketPolicyResult(
        messageId,
        Version4PacketHandling.Typed,
        "The message is dispatched through its registered typed codec.");
    }

    if (!string.Equals(stage, "Active", StringComparison.Ordinal))
    {
      return new Version4PacketPolicyResult(
        messageId,
        Version4PacketHandling.RejectStage,
        "The Version4 compatibility packet is not accepted before the active stage.");
    }

    if (messageId == 94)
    {
      return enableDebugCommands
        ? new Version4PacketPolicyResult(
          messageId,
          Version4PacketHandling.Typed,
          "Debug command payload is enabled and must be validated by DebugCommandHandler.")
        : new Version4PacketPolicyResult(
          messageId,
          Version4PacketHandling.ConsumedReserved,
          "Debug command payload is consumed without a state effect while debug commands are disabled.");
    }

    return new Version4PacketPolicyResult(
      messageId,
      Version4PacketHandling.ConsumedReserved,
      "The Version4 compatibility packet is consumed in the active stage without a domain effect.");
  }

  public static bool IsReserved(byte messageId)
  {
    return ReservedMessageIds.Contains(messageId);
  }
}

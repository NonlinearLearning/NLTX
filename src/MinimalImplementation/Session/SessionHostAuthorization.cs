using System;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Player;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Session;

public sealed class SessionHostAuthorization
{
  public const byte HostTokenMessageId = 161;

  private static readonly SessionStage[] AllowedStages =
  [
    SessionStage.AwaitPlayerData,
    SessionStage.AwaitSectionRequest,
    SessionStage.Synchronizing,
    SessionStage.Active
  ];

  private readonly SessionAdmissionPolicy _policy;
  private readonly PlayerOwner _players;
  private readonly SessionAdmissionCoordinator _sessions;

  public SessionHostAuthorization(
    SessionAdmissionPolicy policy,
    PlayerOwner players,
    SessionAdmissionCoordinator sessions)
  {
    ArgumentNullException.ThrowIfNull(policy);
    ArgumentNullException.ThrowIfNull(players);
    ArgumentNullException.ThrowIfNull(sessions);
    _policy = policy;
    _players = players;
    _sessions = sessions;
  }

  public OwnerCommitResult Apply(
    InboundPacketEnvelope envelope,
    HostTokenPacket packet)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    return _sessions.ExecuteAuthenticatedPlayerPacket(
      envelope,
      HostTokenMessageId,
      AllowedStages,
      binding =>
      {
        bool isHost = _policy.HostToken is string expectedToken
          && !string.IsNullOrWhiteSpace(expectedToken)
          && SecretEquals(expectedToken, packet.HostToken);
        // Packet 161 assigns the source host flag on every accepted packet. A wrong or empty
        // token therefore revokes a previous authorization, matching the Version4 branch.
        return _players.SetHostAuthorization(binding.PlayerHandle, isHost);
      });
  }

  private static bool SecretEquals(string expected, string? actual)
  {
    if (actual is null)
    {
      return false;
    }

    byte[] expectedBytes = System.Text.Encoding.UTF8.GetBytes(expected);
    byte[] actualBytes = System.Text.Encoding.UTF8.GetBytes(actual);
    try
    {
      return expectedBytes.Length == actualBytes.Length
        && System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
          expectedBytes,
          actualBytes);
    }
    finally
    {
      System.Security.Cryptography.CryptographicOperations.ZeroMemory(expectedBytes);
      System.Security.Cryptography.CryptographicOperations.ZeroMemory(actualBytes);
    }
  }
}

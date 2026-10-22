using System;
using System.Collections.Generic;
using MinimalImplementation.Contracts;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Session;

/// <summary>Owns the ordinary 154 ping response and its active-session stage gate.</summary>
public sealed class SessionPingHandler
{
  private static readonly SessionStage[] AllowedStages = [SessionStage.Active];

  private readonly SessionAdmissionCoordinator _sessions;

  public SessionPingHandler(SessionAdmissionCoordinator sessions)
  {
    ArgumentNullException.ThrowIfNull(sessions);
    _sessions = sessions;
  }

  /// <summary>
  /// Prepares the empty packet-154 response for the sending epoch. The callback runs before the
  /// session sequence is accepted and must only encode/capture the proposed projection.
  /// </summary>
  public SessionPingResult Handle(
      InboundPacketEnvelope envelope,
      PingPacket packet,
      Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    IReadOnlyList<SessionPacketProjection> projections = Array.Empty<SessionPacketProjection>();
    OwnerCommitResult result = _sessions.ExecuteAuthenticatedPlayerPacket(
      envelope,
      SessionAdmissionCoordinator.PingMessageId,
      AllowedStages,
      binding =>
      {
        CommitVersion version = _sessions.AllocateCommitVersion();
        var prepared = Array.AsReadOnly(new[] {
          new SessionPacketProjection(
            SessionAdmissionCoordinator.PingMessageId,
            new PingPacket(),
            [binding.Epoch],
            null,
            version)
        });
        OwnerCommitResult? output = prepareOutput?.Invoke(prepared);
        if (output is not null && output.Status != OwnerCommitStatus.Applied)
        {
          return output;
        }

        projections = prepared;
        return OwnerCommitResult.Applied(version);
      });

    return new SessionPingResult(result, projections);
  }
}

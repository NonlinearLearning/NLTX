using System;
using System.Collections.Generic;
using System.Linq;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Session;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Player;

/// <summary>A spectating state and its section-before-150 output captured at one commit.</summary>
public sealed class PlayerSpectatingCommit {
  internal PlayerSpectatingCommit(
      OwnerCommitResult result,
      PlayerSpectating state,
      IReadOnlyCollection<SessionPacketProjection> projections) {
    Result = result;
    State = state;
    Projections = Array.AsReadOnly(projections.ToArray());
  }

  public OwnerCommitResult Result { get; }

  public PlayerSpectating State { get; }

  public IReadOnlyList<SessionPacketProjection> Projections { get; }
}

internal sealed class PreparedPlayerSpectatingChange {
  private readonly IPlayerSpectatingSectionPreparation? _sections;
  private readonly IReadOnlyList<JoinBatchPacket> _packets;
  private readonly IReadOnlyList<SessionEpoch> _recipients;
  private readonly SessionBinding _binding;
  private readonly short _targetSlot;
  private readonly bool _project;

  internal PreparedPlayerSpectatingChange(
      SessionBinding binding,
      PlayerSpectating state,
      short targetSlot,
      IPlayerSpectatingSectionPreparation? sections,
      IReadOnlyList<JoinBatchPacket> packets,
      IReadOnlyList<SessionEpoch> recipients,
      bool project) {
    _binding = binding;
    State = state;
    _targetSlot = targetSlot;
    _sections = sections;
    _packets = packets;
    _recipients = recipients;
    _project = project;
  }

  internal PlayerSpectating State { get; }

  internal void AppendProjections(
      CommitVersion version, List<SessionPacketProjection> projections) {
    foreach (JoinBatchPacket packet in _packets) {
      projections.Add(new SessionPacketProjection(packet.MessageId, packet.Packet,
        [_binding.Epoch], null, version));
    }

    if (_project && _recipients.Count > 0) {
      projections.Add(new SessionPacketProjection(
        SessionAdmissionCoordinator.SpectatePlayerMessageId,
        new SpectatePlayerPacket { Player = _binding.PlayerSlot, TargetPlayer = _targetSlot },
        _recipients, null, version));
    }
  }

  internal void CommitSections() {
    _sections?.Commit();
  }
}

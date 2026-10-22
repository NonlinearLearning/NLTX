using System;
using System.Collections.Generic;
using System.Linq;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Session;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Player;

/// <summary>Commits 45/157 and orders P03 section work before the self-only 158 notification.</summary>
public sealed class PlayerTeamChangeCoordinator {
  private readonly PlayerOwner _players;
  private readonly SessionAdmissionCoordinator _sessions;

  public PlayerTeamChangeCoordinator(
      PlayerOwner players, SessionAdmissionCoordinator sessions) {
    ArgumentNullException.ThrowIfNull(players);
    ArgumentNullException.ThrowIfNull(sessions);
    _players = players;
    _sessions = sessions;
  }

  /// <summary>Prepares canonical output before committing the authenticated player's team.</summary>
  /// <remarks>
  /// Call on the serialized commit loop. The optional callback synchronously encodes/captures
  /// the proposed output; Applied permits commit. It must not mutate inputs or owners, perform
  /// I/O, or reenter the session owner. Rejection publishes no projections and consumes no
  /// accepted sequence. Reserved player versions may have gaps; unexpected exceptions do not
  /// roll back owner writes or make transport submission transactional.
  /// </remarks>
  public PlayerTeamChangeCommit HandleTeamChange(
      InboundPacketEnvelope envelope,
      TeamChangePacket packet,
      Func<PlayerTeamChangeCommit, OwnerCommitResult>? prepareOutput = null) {
    ArgumentNullException.ThrowIfNull(packet);
    return Apply(envelope, packet.Team,
      SessionAdmissionCoordinator.TeamChangeMessageId, null, prepareOutput);
  }

  /// <summary>Prepares section and notification output before either participant commits.</summary>
  /// <remarks>
  /// The output callback follows HandleTeamChange's preparation contract. Spawn preparation and
  /// validation must remain read-only; its commit follows successful output preparation in the
  /// same exclusive loop window, without await or reentry.
  /// </remarks>
  public PlayerTeamChangeCommit HandleTeamChangeFromUi(
      InboundPacketEnvelope envelope,
      TeamChangeFromUIPacket packet,
      Func<SessionBinding, byte, IPlayerTeamSpawnPreparation> prepareSpawn,
      Func<PlayerTeamChangeCommit, OwnerCommitResult>? prepareOutput = null) {
    ArgumentNullException.ThrowIfNull(packet);
    ArgumentNullException.ThrowIfNull(prepareSpawn);
    return Apply(envelope, packet.Team,
      SessionAdmissionCoordinator.TeamChangeFromUiMessageId, prepareSpawn, prepareOutput);
  }

  /// <summary>The original server consumes reverse 158 without spawning or changing stage.</summary>
  public OwnerCommitResult HandleExtraSpawnSectionLoaded(
      InboundPacketEnvelope envelope, ExtraSpawnSectionLoadedPacket packet) {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    return _sessions.ExecuteAuthenticatedPlayerPacket(envelope,
      SessionAdmissionCoordinator.ExtraSpawnSectionLoadedMessageId, [SessionStage.Active],
      binding => packet.Player == binding.PlayerSlot
        ? OwnerCommitResult.Applied(_sessions.AllocateCommitVersion())
        : OwnerCommitResult.Rejected(
          ProtocolErrorCode.Forbidden,
          "The extra-spawn acknowledgement must identify the authenticated player."));
  }

  private PlayerTeamChangeCommit Apply(
      InboundPacketEnvelope envelope,
      byte requestedTeam,
      byte messageId,
      Func<SessionBinding, byte, IPlayerTeamSpawnPreparation>? prepareSpawn,
      Func<PlayerTeamChangeCommit, OwnerCommitResult>? prepareOutput) {
    ArgumentNullException.ThrowIfNull(envelope);
    PlayerTeamChangeCommit? committed = null;
    OwnerCommitResult result = _sessions.ExecuteAuthenticatedPlayerPacket(
      envelope, messageId, [SessionStage.Active], binding => {
        if (!_players.TryGetForBinding(binding, out Player? player) || player is null) {
          return OwnerCommitResult.Rejected(ProtocolErrorCode.StaleEntity,
            "The team-change player identity is no longer active.");
        }

        if (!player.Team.TryChange(requestedTeam, out PlayerTeam team)) {
          return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden,
            "The requested team is outside the Version4 range.");
        }

        IPlayerTeamSpawnPreparation? spawn = prepareSpawn?.Invoke(binding, requestedTeam);
        if (messageId == SessionAdmissionCoordinator.TeamChangeFromUiMessageId && spawn is null) {
          return OwnerCommitResult.Rejected(ProtocolErrorCode.NotReady,
            "P03 did not provide a prepared team-spawn arrangement.");
        }

        JoinBatchPacket[] sectionPackets = Array.Empty<JoinBatchPacket>();
        bool requiresExtraSpawn = false;
        if (spawn is not null) {
          if (spawn.Epoch != binding.Epoch || spawn.Team != requestedTeam) {
            return OwnerCommitResult.Rejected(ProtocolErrorCode.Conflict,
              "The prepared team spawn belongs to another session or team.");
          }

          requiresExtraSpawn = spawn.RequiresExtraSpawn;
          sectionPackets = spawn.SectionPackets.ToArray();
          if ((!requiresExtraSpawn && sectionPackets.Length != 0)
              || sectionPackets.Any(packet => packet.MessageId is not (10 or 11))) {
            return OwnerCommitResult.Rejected(ProtocolErrorCode.Conflict,
              "The team-spawn arrangement contains invalid section output.");
          }

          OwnerCommitResult validation = spawn.Validate();
          if (validation.Status != OwnerCommitStatus.Applied) {
            return validation;
          }
        }

        IReadOnlyList<SessionEpoch> recipients = _sessions.CaptureActiveRecipients(envelope.Epoch);
        var notifications = new List<SessionEpoch> { binding.Epoch };
        foreach (Player other in _players.CapturePlayers()) {
          if (recipients.Contains(other.Epoch)
              && ((team.Previous > 0 && other.Team.Current == team.Previous)
                || (team.Current > 0 && other.Team.Current == team.Current))) {
            notifications.Add(other.Epoch);
          }
        }

        // Reserve the player version and prepare output before either participant writes.
        CommitVersion version = _players.ReserveCommitVersion();
        var projections = new List<SessionPacketProjection>();
        if (recipients.Count > 0) {
          projections.Add(new SessionPacketProjection(
            SessionAdmissionCoordinator.TeamChangeMessageId,
            new TeamChangePacket { Player = binding.PlayerSlot, Team = team.Current },
            recipients, envelope.Epoch, version));
        }

        foreach (JoinBatchPacket section in sectionPackets) {
          projections.Add(new SessionPacketProjection(section.MessageId, section.Packet,
            [binding.Epoch], null, version));
        }

        if (requiresExtraSpawn) {
          projections.Add(new SessionPacketProjection(
            SessionAdmissionCoordinator.ExtraSpawnSectionLoadedMessageId,
            new ExtraSpawnSectionLoadedPacket { Player = binding.PlayerSlot },
            [binding.Epoch], null, version));
        }

        var proposed = new PlayerTeamChangeCommit(OwnerCommitResult.Applied(version), team,
          player.Appearance.Name, notifications, projections);
        OwnerCommitResult? prepared = prepareOutput?.Invoke(proposed);
        if (prepared is not null && prepared.Status != OwnerCommitStatus.Applied) {
          return prepared;
        }

        spawn?.Commit();
        _players.CommitPreparedTeamChange(player, team, version);
        committed = proposed;
        return committed.Result;
      });

    return committed ?? new PlayerTeamChangeCommit(result, default, string.Empty, [], []);
  }
}

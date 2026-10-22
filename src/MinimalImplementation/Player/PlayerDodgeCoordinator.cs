using System;
using System.Collections.Generic;
using System.Linq;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Session;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Player;

/// <summary>Commits packet 62 or an already decided server dodge on the exclusive loop.</summary>
public sealed class PlayerDodgeCoordinator {
  private readonly PlayerOwner _players;
  private readonly SessionAdmissionCoordinator _sessions;
  private readonly Func<Player, bool> _captureLongInvincibility;
  private readonly Func<Player, PlayerLuckCalculationContext> _captureLuckContext;

  public PlayerDodgeCoordinator(
      PlayerOwner players,
      SessionAdmissionCoordinator sessions,
      Func<Player, bool> captureLongInvincibility,
      Func<Player, PlayerLuckCalculationContext> captureLuckContext) {
    ArgumentNullException.ThrowIfNull(players);
    ArgumentNullException.ThrowIfNull(sessions);
    ArgumentNullException.ThrowIfNull(captureLongInvincibility);
    ArgumentNullException.ThrowIfNull(captureLuckContext);
    _players = players;
    _sessions = sessions;
    _captureLongInvincibility = captureLongInvincibility;
    _captureLuckContext = captureLuckContext;
  }

  public PlayerDodgeCommit Handle(
      InboundPacketEnvelope envelope,
      Unknown62Packet packet,
      Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null) {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    byte dodgeType = packet.DodgeType;
    PlayerDodgeCommit? committed = null;
    OwnerCommitResult gate = _sessions.ExecuteAuthenticatedPlayerPacket(
      envelope, SessionAdmissionCoordinator.DodgeMessageId, [SessionStage.Active], binding => {
        if (!_players.TryGetForBinding(binding, out Player? player) || player is null) {
          return OwnerCommitResult.Rejected(ProtocolErrorCode.StaleEntity,
            "The dodging player is no longer active.");
        }

        // Dedicated-server reception does not run the controlling player's buff branches.
        committed = Commit(player, binding, dodgeType, false, null, prepareOutput);
        return committed.Result;
      });
    return committed ?? new PlayerDodgeCommit(gate, false, default, null, []);
  }

  /// <summary>
  /// P05 calls this only after deciding the dodge. No I/O, random decision or damage mutation
  /// occurs here. Self receives 62 and the stock controlling client sends 62 back; the current
  /// inbound relay may therefore duplicate remote visuals. Unknown delivery must not be replayed.
  /// </summary>
  public PlayerDodgeCommit ApplyAuthoritative(
      EntityHandle handle,
      byte dodgeType,
      PlayerBuffApplicationContext brainBuffContext,
      Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null) {
    ArgumentNullException.ThrowIfNull(brainBuffContext);
    if (!PlayerDodgeRules.IsRecognized(dodgeType)) {
      return new PlayerDodgeCommit(OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden,
        "An authoritative dodge must be Ninja, Shadow or Brain."), false, default, null, []);
    }

    if (!_players.TryGet(handle, out Player? player) || player is null
        || !_sessions.TryGetBinding(player.Epoch, out SessionBinding? binding) || binding is null
        || binding.PlayerHandle != handle
        || !_sessions.TryGetStage(player.Epoch, out SessionStage stage)
        || stage != SessionStage.Active) {
      return new PlayerDodgeCommit(OwnerCommitResult.Rejected(ProtocolErrorCode.StaleEntity,
        "The dodging player is no longer active."), true, default, null, []);
    }

    return Commit(player, binding, dodgeType, true, brainBuffContext, prepareOutput);
  }

  private PlayerDodgeCommit Commit(
      Player player, SessionBinding binding, byte dodgeType, bool authoritative,
      PlayerBuffApplicationContext? brainBuffContext,
      Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput) {
    bool recognized = PlayerDodgeRules.IsRecognized(dodgeType);
    bool longInvincibility = recognized && _captureLongInvincibility.Invoke(player);
    PlayerImmunities immunities = PlayerDodgeRules.ApplyImmunity(
      player.Immunities, dodgeType, longInvincibility);
    PlayerBuffs buffs = player.Buffs;
    PlayerLuck luck = player.Luck;
    if (authoritative) {
      buffs = PlayerDodgeRules.ApplyControlledBuffs(buffs, dodgeType, brainBuffContext!);
      if (dodgeType == PlayerDodgeRules.Shadow) {
        immunities = PlayerDodgeRules.ApplyShadowCooldown(immunities);
      }

      PlayerLuckCalculationContext context = _captureLuckContext.Invoke(player);
      PlayerLuck factors = luck.WithAuthoritativeBuffs(buffs, player.Buffs);
      luck = factors.WithCurrentLuck(PlayerLuckCalculator.Calculate(factors,
        player.Appearance.HasUsedGalaxyPearl,
        new PlayerLuckCalculationContext(context.LanternsUp,
          buffs.HasBuff(Version4PlayerBuffCatalog.StinkyBuffType))));
    }

    CommitVersion version = _players.ReserveCommitVersion();
    IReadOnlyList<SessionEpoch> recipients = authoritative
      ? _sessions.CaptureActiveSessionEpochs() : _sessions.CaptureActiveRecipients(binding.Epoch);
    var projections = new List<SessionPacketProjection>();
    if (recipients.Count > 0) {
      projections.Add(new SessionPacketProjection(SessionAdmissionCoordinator.DodgeMessageId,
        new Unknown62Packet { Player = binding.PlayerSlot, DodgeType = dodgeType },
        recipients, authoritative ? null : binding.Epoch, version));
    }

    IReadOnlyList<SessionEpoch> others = _sessions.CaptureActiveRecipients(binding.Epoch);
    if (authoritative && others.Count > 0 && !player.Buffs.Types.SequenceEqual(buffs.Types)) {
      projections.Add(new SessionPacketProjection(SessionAdmissionCoordinator.PlayerBuffsMessageId,
        new PlayerBuffsPacket {
          Player = binding.PlayerSlot, BuffTypes = buffs.Types.Where(type => type != 0).ToArray()
        }, others, binding.Epoch, version));
    }

    if (authoritative && player.Luck.LuckPotion != luck.LuckPotion && recipients.Count > 0) {
      projections.Add(new SessionPacketProjection(
        SessionAdmissionCoordinator.UpdatePlayerLuckFactorsMessageId,
        PlayerPacketProjection.CreateLuckPacket(binding.PlayerSlot, luck),
        recipients, null, version));
    }

    var result = new PlayerDodgeCommit(
      OwnerCommitResult.Applied(version), recognized, immunities, buffs, projections);

    OwnerCommitResult? outputValidation = prepareOutput?.Invoke(projections.AsReadOnly());
    if (outputValidation is not null && outputValidation.Status != OwnerCommitStatus.Applied) {
      return new PlayerDodgeCommit(outputValidation, recognized, default, null, []);
    }

    _players.CommitPreparedDodge(player, immunities, buffs, luck, version);
    return result;
  }
}

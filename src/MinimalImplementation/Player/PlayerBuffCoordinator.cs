using System;
using System.Collections.Generic;
using System.Linq;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Session;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Player;

/// <summary>Applies server effects and captures 55 to self and 50 to other active players.</summary>
public sealed class PlayerBuffCoordinator {
  private readonly PlayerOwner _players;
  private readonly SessionAdmissionCoordinator _sessions;
  private readonly Func<Player, PlayerLuckCalculationContext> _captureCalculationContext;

  public PlayerBuffCoordinator(
      PlayerOwner players,
      SessionAdmissionCoordinator sessions,
      Func<Player, PlayerLuckCalculationContext> captureCalculationContext) {
    ArgumentNullException.ThrowIfNull(players);
    ArgumentNullException.ThrowIfNull(sessions);
    ArgumentNullException.ThrowIfNull(captureCalculationContext);
    _players = players;
    _sessions = sessions;
    _captureCalculationContext = captureCalculationContext;
  }

  public PlayerBuffCommit Apply(
      EntityHandle handle,
      ushort type,
      int durationTicks,
      PlayerBuffApplicationContext context,
      Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null) {
    ArgumentNullException.ThrowIfNull(context);
    if (!TryResolve(handle, out Player? player, out SessionBinding? binding)
        || player is null || binding is null) {
      return Rejected(ProtocolErrorCode.StaleEntity, "The buff target is no longer active.");
    }

    if (!player.Buffs.TryAddAuthoritative(type, durationTicks, context,
        out PlayerBuffs buffs, out int durationToAdd)) {
      return Rejected(ProtocolErrorCode.Forbidden,
        "The buff cannot be applied with the supplied immunity, duration or capacity facts.");
    }

    PlayerLuck luck = CalculateLuck(player, buffs);
    CommitVersion version = _players.ReserveCommitVersion();
    var projections = new List<SessionPacketProjection> {
      new(SessionAdmissionCoordinator.AddPlayerBuffMessageId,
        new AddPlayerBuffPvPPacket {
          Player = binding.PlayerSlot,
          BuffType = type,
          // Sending the final total would double-count existing additive buff time.
          BuffTime = durationToAdd
        }, [binding.Epoch], null, version)
    };
    AppendOtherPlayerProjection(binding, buffs, version, projections);
    AppendLuckProjection(player, binding, luck, version, projections);

    OwnerCommitResult? outputValidation = prepareOutput?.Invoke(projections.AsReadOnly());
    if (outputValidation is not null && outputValidation.Status != OwnerCommitStatus.Applied) {
      return Rejected(outputValidation);
    }

    var result = new PlayerBuffCommit(OwnerCommitResult.Applied(version), buffs, luck, [], projections);
    _players.CommitPreparedBuffs(player, buffs, luck, version);
    return result;
  }

  public PlayerBuffCommit Remove(
      EntityHandle handle,
      ushort type,
      Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null) {
    if (!Version4PlayerBuffCatalog.IsValidBuff(type)) {
      return Rejected(ProtocolErrorCode.Forbidden, "The buff type is outside the supported range.");
    }

    if (!TryResolve(handle, out Player? player, out SessionBinding? binding)
        || player is null || binding is null) {
      return Rejected(ProtocolErrorCode.StaleEntity, "The buff target is no longer active.");
    }

    bool removed = player.Buffs.HasBuff(type);
    PlayerBuffs buffs = player.Buffs.Remove(type);
    PlayerLuck luck = CalculateLuck(player, buffs);
    CommitVersion version = _players.ReserveCommitVersion();
    var projections = new List<SessionPacketProjection>();
    if (removed) {
      AppendOtherPlayerProjection(binding, buffs, version, projections);
      AppendLuckProjection(player, binding, luck, version, projections);
    }

    // The current protocol has no general own-player removal message for a non-SSC client.
    OwnerCommitResult? outputValidation = prepareOutput?.Invoke(projections.AsReadOnly());
    if (outputValidation is not null && outputValidation.Status != OwnerCommitStatus.Applied) {
      return Rejected(outputValidation);
    }

    var result = new PlayerBuffCommit(OwnerCommitResult.Applied(version), buffs, luck,
      removed ? new[] { type } : [], projections);
    _players.CommitPreparedBuffs(player, buffs, luck, version);
    return result;
  }

  private bool TryResolve(
      EntityHandle handle, out Player? player, out SessionBinding? binding) {
    binding = null;
    return _players.TryGet(handle, out player) && player is not null
      && _sessions.TryGetBinding(player.Epoch, out binding) && binding is not null
      && binding.PlayerHandle == handle
      && _sessions.TryGetStage(player.Epoch, out SessionStage stage) && stage == SessionStage.Active;
  }

  private void AppendOtherPlayerProjection(
      SessionBinding binding,
      PlayerBuffs buffs,
      CommitVersion version,
      List<SessionPacketProjection> projections) {
    IReadOnlyList<SessionEpoch> recipients = _sessions.CaptureActiveRecipients(binding.Epoch);
    if (recipients.Count > 0) {
      projections.Add(new SessionPacketProjection(SessionAdmissionCoordinator.PlayerBuffsMessageId,
        new PlayerBuffsPacket {
          Player = binding.PlayerSlot,
          BuffTypes = buffs.Types.Where(type => type != 0).ToArray()
        }, recipients, binding.Epoch, version));
    }
  }

  private static PlayerBuffCommit Rejected(ProtocolErrorCode code, string detail) {
    return new PlayerBuffCommit(OwnerCommitResult.Rejected(code, detail), null, null, [], []);
  }

  private static PlayerBuffCommit Rejected(OwnerCommitResult result) {
    return new PlayerBuffCommit(result, null, null, [], []);
  }

  private void AppendLuckProjection(
      Player player,
      SessionBinding binding,
      PlayerLuck luck,
      CommitVersion version,
      List<SessionPacketProjection> projections) {
    if (luck.LuckPotion != player.Luck.LuckPotion) {
      SessionEpoch[] recipients = _sessions.CaptureActiveSessionEpochs().ToArray();
      projections.Add(new SessionPacketProjection(
        SessionAdmissionCoordinator.UpdatePlayerLuckFactorsMessageId,
        PlayerPacketProjection.CreateLuckPacket(binding.PlayerSlot, luck),
        recipients, null, version));
    }
  }

  private PlayerLuck CalculateLuck(Player player, PlayerBuffs buffs) {
    PlayerLuckCalculationContext context = _captureCalculationContext.Invoke(player);
    PlayerLuck factors = player.Luck.WithAuthoritativeBuffs(buffs, player.Buffs);
    return factors.WithCurrentLuck(PlayerLuckCalculator.Calculate(factors,
      player.Appearance.HasUsedGalaxyPearl,
      new PlayerLuckCalculationContext(context.LanternsUp,
        buffs.HasBuff(Version4PlayerBuffCatalog.StinkyBuffType))));
  }
}

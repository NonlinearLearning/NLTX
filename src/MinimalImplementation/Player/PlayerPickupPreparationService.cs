using System;
using System.Collections.Generic;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Inventory.State;
using MinimalImplementation.Server.Session;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Player;

/// <summary>Prepares the Player participant consumed by P04's pickup transaction.</summary>
public sealed class PlayerPickupPreparationService {
  private readonly PlayerOwner _players;
  private readonly SessionAdmissionCoordinator _sessions;
  private readonly PlayerSpatialFactsService _spatial;
  private readonly Func<SessionBinding, Player, IPlayerPickupFactsCapture?>? _captureSources;
  private PlayerRuntimeFactsCaptureService? _runtimeFacts;

  public PlayerPickupPreparationService(
      PlayerOwner players, SessionAdmissionCoordinator sessions, PlayerSpatialFactsService spatial,
      Func<SessionBinding, Player, IPlayerPickupFactsCapture?>? captureSources = null) {
    ArgumentNullException.ThrowIfNull(players);
    ArgumentNullException.ThrowIfNull(sessions);
    ArgumentNullException.ThrowIfNull(spatial);
    _players = players;
    _sessions = sessions;
    _spatial = spatial;
    _captureSources = captureSources;
  }

  public PlayerPickupPreparationService(
    PlayerOwner players,
    SessionAdmissionCoordinator sessions,
    PlayerSpatialFactsService spatial,
    PlayerRuntimeFactsCaptureService runtimeFacts) : this(players, sessions, spatial)
  {
    ArgumentNullException.ThrowIfNull(runtimeFacts);
    _runtimeFacts = runtimeFacts;
  }

  /// <summary>
  /// Read-only preparation. P04 validates the actual item revision and world epoch alongside
  /// this participant, then commits all owners in one exclusive ServerLoop window.
  /// </summary>
  public OwnerCommitResult Prepare(
      SessionEpoch epoch, WorldItemSnapshot item, ulong worldEpoch,
      out PlayerPickupPreparation? preparation) {
    ArgumentNullException.ThrowIfNull(item);
    preparation = null;
    OwnerCommitResult result = _spatial.Capture(epoch, out PlayerSpatialSnapshot? spatial);
    if (result.Status != OwnerCommitStatus.Applied || spatial is null) {
      return result;
    }
    if (!_players.TryGetForBinding(spatial.Binding, out Player? player) || player is null
        || player.IsDead || item.Item.IsEmpty || worldEpoch == 0 || !item.Revision.IsValid
        || item.Handle.WorldScope != player.Handle.WorldScope) {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden,
        "Pickup requires a living player and an item in the same world scope.");
    }

    IPlayerPickupFactsCapture? sources;
    if (_runtimeFacts is not null)
    {
      OwnerCommitResult captureResult = _runtimeFacts.TryCapturePickup(
        spatial.Binding, player, out sources);
      if (captureResult.Status != OwnerCommitStatus.Applied || sources is null)
      {
        return captureResult;
      }
    }
    else
    {
      sources = _captureSources?.Invoke(spatial.Binding, player);
    }
    if (sources is null) {
      return Unavailable("Authoritative pickup equipment, void bag or animation facts are missing.");
    }
    PlayerPickupSourceFacts facts = sources.Facts;
    if (sources.Epoch != epoch || sources.PlayerHandle != player.Handle
        || sources.PlayerVersion != spatial.Version || !sources.SourceRevision.IsValid
        || facts is null) {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden,
        "Pickup sources do not match this player version.");
    }
    if (facts.ItemAnimation < 0 || facts.EffectiveMaximumLife is <= 0 or > short.MaxValue
        || facts.EffectiveMaximumMana is < 0 or > short.MaxValue) {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden,
        "Pickup sources contain invalid item-animation or maximum-stat facts.");
    }

    WorldItemPickupEffect effect = WorldItemPickupEffect.ForItem(item.Item.Type);
    if (effect.NebulaBuff != 0) {
      return Unavailable("Nebula pickup requires the full prepared area/levelup behavior.");
    }
    PlayerVitals vitals = player.Vitals;
    if (effect.Heal > 0) {
      if (facts.EffectiveMaximumLife is not int maximumLife) {
        return Unavailable("Heart pickup requires the actual effective maximum life.");
      }
      vitals = vitals.WithHeal(effect.Heal, maximumLife);
    }
    if (effect.Mana > 0) {
      if (facts.EffectiveMaximumMana is not int maximumMana) {
        return Unavailable("Mana pickup requires the actual effective maximum mana.");
      }
      vitals = vitals with { Mana = (short)Math.Min(vitals.Mana + effect.Mana, maximumMana) };
    }

    CommitVersion version = spatial.Version;
    if (effect != default && !_players.TryPeekNextCommitVersion(out version)) {
      return Unavailable("The Player commit version sequence is exhausted.");
    }
    IReadOnlyList<SessionEpoch> recipients = _sessions.CaptureActiveSessionEpochs();
    var projections = new List<SessionPacketProjection>();
    if (effect.Heal > 0) {
      projections.Add(new SessionPacketProjection(SessionAdmissionCoordinator.PlayerLifeManaMessageId,
        new PlayerLifeManaPacket {
          Player = spatial.Binding.PlayerSlot, Life = vitals.Life, MaximumLife = vitals.MaximumLife
        }, recipients, null, version));
      projections.Add(new SessionPacketProjection(SessionAdmissionCoordinator.PlayerHealMessageId,
        new PlayerHealPacket { Player = spatial.Binding.PlayerSlot, HealAmount = (short)effect.Heal },
        recipients, null, version));
    }
    if (effect.Mana > 0) {
      projections.Add(new SessionPacketProjection(SessionAdmissionCoordinator.Unknown42MessageId,
        new Unknown42Packet {
          Player = spatial.Binding.PlayerSlot, Mana = vitals.Mana, MaximumMana = vitals.MaximumMana
        }, recipients, null, version));
    }

    var captured = new PlayerPickupPreparation(
      _players, player, spatial, sources, item, worldEpoch, effect, vitals, version, projections);
    result = captured.Validate();
    if (result.Status == OwnerCommitStatus.Applied) {
      preparation = captured;
    }
    return result;
  }

  private static OwnerCommitResult Unavailable(string detail) {
    return new OwnerCommitResult(OwnerCommitStatus.Unavailable, null, ProtocolErrorCode.NotReady,
      detail);
  }
}

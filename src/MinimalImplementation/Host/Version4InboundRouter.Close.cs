using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Combat.Projectiles;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Protocol;
using MinimalImplementation.Server.Session;
using Terraria.NetWork.Prototype.PacketDesignCompiler;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Host;

public sealed partial class Version4InboundRouter {
  private OwnerCommitResult ReleaseCombatSessionState(
      SessionCloseContext context,
      IReadOnlyList<SessionEpoch> recipients,
      List<OutboundPacketIntent> intents) {
    SessionEpoch epoch = context.Connection.Epoch;
    long worldScope = _composition.IdentityRegistry.CurrentWorldScope;
    if (worldScope <= 0 || worldScope != _composition.CombatWorldScope) {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.StaleEntity,
        "Combat owners must join the current world scope before session cleanup.");
    }

    var identities = new HashSet<CombatEntityIdentity>();
    var killBodies = new Dictionary<CombatEntityIdentity, byte[]>();
    IReadOnlyList<Version4EncodedPacket> trackerPackets;
    try {
      foreach (ProjectileView projectile in _composition.ProjectileManager.Capture()) {
        if (projectile.OwnerEpoch != epoch) {
          continue;
        }

        if (projectile.Entity.WorldScope != worldScope ||
            !_composition.CombatIdentityAuthority.IsActive(projectile.Entity)) {
          return OwnerCommitResult.Rejected(ProtocolErrorCode.StaleEntity,
            "Session cleanup found a stale owned projectile.");
        }

        identities.Add(projectile.Entity);
        killBodies.Add(projectile.Entity, ProjectilePacketPayloadCodec.EncodeKillBody(projectile));
      }

      trackerPackets = CaptureCloseTrackerPackets(identities, recipients);
    } catch (Exception exception) when (exception is ArgumentException or
        InvalidDataException or InvalidOperationException or NotSupportedException) {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.NotReady,
        $"Combat close output preparation failed: {exception.Message}");
    }

    OwnerCommitResult tags = _composition.TagEffectStateOwner.RetireSession(epoch);
    if (tags.Status != OwnerCommitStatus.Applied) {
      return tags;
    }

    // Release references while every identity is still available. RemoveOwner can commit a
    // partial removal; a later retry would otherwise no longer capture those removed identities.
    OwnerCommitResult trackers = _composition.ProjectileTrackerManager.RemoveEntities(identities);
    if (trackers.Status != OwnerCommitStatus.Applied) {
      return trackers;
    }

    if (recipients.Count > 0) {
      CommitVersion trackerVersion = trackers.Version
        ?? throw new InvalidOperationException("Applied tracker cleanup has no commit version.");
      foreach (Version4EncodedPacket packet in trackerPackets) {
        intents.Add(new OutboundPacketIntent(
          recipients, null, packet.MessageId, packet.Payload, trackerVersion));
      }
    }

    OwnerCommitResult trackerSession = _composition.ProjectileTrackerManager.RemoveSession(epoch);
    if (trackerSession.Status != OwnerCommitStatus.Applied) {
      return trackerSession;
    }

    ProjectileOwnerCleanupResult projectiles = _composition.ProjectileManager.RemoveOwner(epoch);
    if (recipients.Count > 0) {
      // Removed views carry their earlier live versions. Stamp this batch with the manager's
      // actual cleanup commit, including when RemoveOwner returned a partially committed result.
      CommitVersion removalVersion = _composition.ProjectileManager.CommitVersion;
      foreach (ProjectileView removed in projectiles.Removed) {
        intents.Add(new OutboundPacketIntent(
          recipients, null, 29, killBodies[removed.Entity], removalVersion));
      }
    }

    if (projectiles.Result.Status != OwnerCommitStatus.Applied) {
      return projectiles.Result;
    }

    if (projectiles.TrackerCleanup is { Status: not OwnerCommitStatus.Applied } trackerFailure) {
      return trackerFailure;
    }

    if (projectiles.TrackerSessionCleanup is
        { Status: not OwnerCommitStatus.Applied } trackerSessionFailure) {
      return trackerSessionFailure;
    }

    return projectiles.Result;
  }

  private IReadOnlyList<Version4EncodedPacket> CaptureCloseTrackerPackets(
      IReadOnlySet<CombatEntityIdentity> identities, IReadOnlyList<SessionEpoch> recipients) {
    var packets = new List<Version4EncodedPacket>();
    if (identities.Count == 0 || recipients.Count == 0) {
      return packets.AsReadOnly();
    }

    foreach (SessionBinding binding in _composition.SessionBindings.CaptureBindings()) {
      if (!recipients.Contains(binding.Epoch)) {
        continue;
      }

      if (!_composition.ProjectileTrackerManager.TryCapture(
          binding.PlayerSlot, binding.Epoch, _composition.ProjectileManager,
          out ProjectileTrackerSnapshotView? snapshot) || snapshot is null) {
        throw new InvalidOperationException("An active player's projectile trackers are unavailable.");
      }

      bool clearPiggyBank = snapshot.PiggyBank is not null &&
        identities.Contains(snapshot.PiggyBank.Entity);
      bool clearVoidLens = snapshot.VoidLens is not null &&
        identities.Contains(snapshot.VoidLens.Entity);
      if (!clearPiggyBank && !clearVoidLens) {
        continue;
      }

      SyncProjectileTrackersPacket packet = _composition.CapturePlayerProjectileTrackers(binding);
      if (clearPiggyBank) {
        packet.PiggyBank = new PacketTrackedProjectileRef(-1, null, null);
      }
      if (clearVoidLens) {
        packet.VoidLens = new PacketTrackedProjectileRef(-1, null, null);
      }
      packets.Add(_composition.Version4WireProfile.Encode(WireDirection.ServerToClient, packet));
    }

    return packets.AsReadOnly();
  }

  private async ValueTask<string?> SendCloseProjectionsAsync(
      IReadOnlyList<OutboundPacketIntent> intents) {
    var failures = new List<string>();
    foreach (OutboundPacketIntent intent in intents) {
      foreach (SessionEpoch target in intent.TargetEpochs) {
        if (target == intent.IgnoreEpoch) {
          continue;
        }

        var targeted = new OutboundPacketIntent(
          [target], null, intent.MessageId, intent.Payload, intent.CommitVersion);
        IReadOnlyList<SendReceipt> receipts = await SendWithReceiptsAsync(
          targeted, CancellationToken.None).ConfigureAwait(false);
        if (!IsSuccessfulSubmission(receipts, target)) {
          string outcome = receipts.Count == 0 ? "NotSubmitted" : string.Join(", ",
            receipts.Select(receipt => $"{receipt.Epoch.Value}:{receipt.Certainty}"));
          failures.Add($"packet {intent.MessageId}, version {intent.CommitVersion.Value}, " +
            $"target {target.Value}: {outcome}");
        }
      }
    }

    // Close retries recapture only current state. Replaying a historical 29 could terminate
    // a new projectile using the same protocol identity; 142 could overwrite newer references.
    return failures.Count == 0 ? null :
      $"Committed close projection submission failed ({string.Join("; ", failures)}).";
  }
}

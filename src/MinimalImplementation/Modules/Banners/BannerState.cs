using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Modules.Protocol;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NonAuthoritative.Persistence;

namespace MinimalImplementation.Server.Modules.Banners;

public enum BannerCommitStatus
{
  Applied,
  Duplicate,
  Rejected
}

public sealed record BannerCommitResult(
  BannerCommitStatus Status,
  CommitVersion? Version,
  ProtocolErrorCode? ErrorCode,
  IReadOnlyList<OutboundPacketIntent> OutboundIntents,
  ushort? GrantedAmount = null);

/// <summary>
/// P07's sole owner for the 293 banner kill and claim counters. P05 supplies committed kills;
/// claim responses let the requesting client settle its FakeCursor and item delivery.
/// </summary>
public sealed class BannerState
{
  private readonly int[] _killCounts = new int[BannerSnapshot.BannerCount];
  private readonly ushort[] _claimableCounts = new ushort[BannerSnapshot.BannerCount];
  private readonly Dictionary<SessionEpoch, CommitVersion> _acknowledgedSnapshotVersions = new();
  private readonly Dictionary<SessionEpoch, BannerSnapshotIntentBatch> _pendingSnapshots = new();
  private readonly Dictionary<P05NpcDeathKey,
      (short BannerId, int KillsRequired, CommitVersion Version)> _pendingP05Kills = new();
  private readonly Packet82ModuleIntentProducer _producer;
  private CommitVersion _version = CommitVersion.Initial;
  private CommitVersion _lastP05CommitVersion;
  private long _p05WorldScope;

  private const int MaximumTrackedRecipients = byte.MaxValue;
  private const int MaximumPendingP05Kills = 1024;

  public BannerState(Packet82ModuleIntentProducer producer)
  {
    ArgumentNullException.ThrowIfNull(producer);
    _producer = producer;
  }

  public bool AnyNewClaimableBanners { get; private set; }

  public BannerSnapshot CaptureSnapshot() => new(
    _version,
    _killCounts,
    _claimableCounts,
    AnyNewClaimableBanners,
    _lastP05CommitVersion);

  public OutboundPacketIntent CreateFullStateIntent(SessionEpoch target)
  {
    if (!target.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(target));
    }

    return _producer.CreateIntent(
      (ushort)Packet82ModuleId.Banners,
      new Packet82BannerData(
        0,
        Array.AsReadOnly((int[])_killCounts.Clone()),
        Array.AsReadOnly((ushort[])_claimableCounts.Clone()),
        null,
        null,
        null),
      new[] { target },
      _version);
  }

  /// <summary>Captures the full Banner projection only when this target is dirty.</summary>
  public BannerSnapshotIntentBatch? CaptureDirtySnapshot(SessionEpoch target)
  {
    if (!target.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(target));
    }

    if (_pendingSnapshots.ContainsKey(target))
    {
      return null;
    }

    if (_acknowledgedSnapshotVersions.TryGetValue(target, out CommitVersion acknowledged)
        && acknowledged.Value >= _version.Value)
    {
      return null;
    }

    if (!_acknowledgedSnapshotVersions.ContainsKey(target)
        && _acknowledgedSnapshotVersions.Keys.Union(_pendingSnapshots.Keys).Count()
          >= MaximumTrackedRecipients)
    {
      throw new InvalidOperationException(
        "Retire an inactive Banner recipient before capturing another target.");
    }

    var batch = new BannerSnapshotIntentBatch(
      target,
      _version,
      CreateFullStateIntent(target));
    _pendingSnapshots.Add(target, batch);
    return batch;
  }

  /// <summary>
  /// Settles a dirty Banner projection from local transport receipts. This does not claim a
  /// client-level acknowledgement.
  /// </summary>
  public ModuleDeliveryResult AcknowledgeSnapshot(
    BannerSnapshotIntentBatch batch,
    IReadOnlyCollection<SendReceipt> receipts)
  {
    ArgumentNullException.ThrowIfNull(batch);
    ArgumentNullException.ThrowIfNull(receipts);
    return CompleteSnapshot(batch, ModuleDeliveryReceiptEvaluator.Evaluate(
      batch.Intents, batch.Target, receipts));
  }

  public ModuleDeliveryResult AcknowledgeSnapshot(
      BannerSnapshotIntentBatch batch,
      IReadOnlyList<OutboundIntentDelivery> deliveries) {
    ArgumentNullException.ThrowIfNull(batch);
    return CompleteSnapshot(batch, ModuleDeliveryReceiptEvaluator.Evaluate(
        batch.Intents, batch.Target, deliveries));
  }

  private ModuleDeliveryResult CompleteSnapshot(
      BannerSnapshotIntentBatch batch, ModuleDeliveryResult result) {
    if (!_pendingSnapshots.TryGetValue(batch.Target, out BannerSnapshotIntentBatch? pending)
        || !ReferenceEquals(pending, batch))
    {
      return ModuleDeliveryResult.Stale(
        "The Banner batch was not captured by this owner or its epoch was retired.");
    }

    if (batch.Version.Value > _version.Value)
    {
      return ModuleDeliveryResult.Stale(
        "A Banner projection cannot acknowledge a future owner version.");
    }

    if (_acknowledgedSnapshotVersions.TryGetValue(batch.Target, out CommitVersion acknowledged)
        && batch.Version.Value < acknowledged.Value)
    {
      return ModuleDeliveryResult.Stale(
        "A Banner projection acknowledgement is older than the settled target version.");
    }

    _pendingSnapshots.Remove(batch.Target);
    if (!result.IsSettled)
    {
      return result;
    }

    if (!_acknowledgedSnapshotVersions.ContainsKey(batch.Target)
        && _acknowledgedSnapshotVersions.Count == MaximumTrackedRecipients)
    {
      return ModuleDeliveryResult.Invalid(
        "The Banner owner reached its tracked-recipient limit; retire the epoch first.");
    }

    _acknowledgedSnapshotVersions[batch.Target] = batch.Version;
    return result;
  }

  /// <summary>
  /// Discards only session-bound projection acknowledgements, never Banner world state.
  /// </summary>
  public void RetireEpoch(SessionEpoch epoch)
  {
    if (!epoch.IsValid)
    {
      throw new ArgumentException("A valid session epoch is required.", nameof(epoch));
    }

    _acknowledgedSnapshotVersions.Remove(epoch);
    _pendingSnapshots.Remove(epoch);
  }

  public BannerCommitResult ApplyP05Kill(
      P05NpcDeathKey key, short bannerId, int killsRequired,
      IReadOnlyCollection<SessionEpoch> broadcastTargets) {
    if (!key.IsValid || bannerId <= 0 || bannerId >= BannerSnapshot.BannerCount
        || killsRequired <= 0 || !HasValidTargets(broadcastTargets)) {
      return Rejected(ProtocolErrorCode.InvalidLength);
    }

    if (_p05WorldScope != 0 && key.WorldScope != _p05WorldScope) {
      return Rejected(ProtocolErrorCode.Conflict);
    }

    if (_pendingP05Kills.TryGetValue(key, out var receipt)) {
      if (receipt.BannerId != bannerId || receipt.KillsRequired != killsRequired) {
        return Rejected(ProtocolErrorCode.Conflict);
      }

      return new BannerCommitResult(
          BannerCommitStatus.Duplicate, receipt.Version, null,
          Array.Empty<OutboundPacketIntent>());
    }

    if (_pendingP05Kills.Keys.Any(pending => pending.NpcId == key.NpcId
        && pending.WorldScope == key.WorldScope && pending.Generation == key.Generation)) {
      return Rejected(ProtocolErrorCode.Conflict);
    }

    if (key.SourceCommitVersion.Value <= _lastP05CommitVersion.Value
        || _pendingP05Kills.Count == MaximumPendingP05Kills) {
      return Rejected(ProtocolErrorCode.Conflict);
    }

    BannerCommitResult result = ApplyP05Kill(
        key.SourceCommitVersion, bannerId, killsRequired, broadcastTargets);
    if (result.Status == BannerCommitStatus.Applied) {
      _pendingP05Kills.Add(key, (bannerId, killsRequired, _version));
      _p05WorldScope = key.WorldScope;
    }

    return result;
  }

  /// <summary>Call only after P05 successfully acknowledges the retained death record.</summary>
  public bool AcknowledgeP05Kill(P05NpcDeathKey key) {
    return _pendingP05Kills.Remove(key);
  }

  public BannerCommitResult ApplyP05Kill(
    CommitVersion sourceCommitVersion,
    short bannerId,
    int killsRequired,
    IReadOnlyCollection<SessionEpoch> broadcastTargets)
  {
    if (!sourceCommitVersion.IsValid
      || bannerId <= 0
      || bannerId >= BannerSnapshot.BannerCount
      || killsRequired <= 0
      || !HasValidTargets(broadcastTargets))
    {
      return Rejected(ProtocolErrorCode.InvalidLength);
    }

    if (sourceCommitVersion.Value <= _lastP05CommitVersion.Value)
    {
      if (sourceCommitVersion == _lastP05CommitVersion)
      {
        return new BannerCommitResult(
          BannerCommitStatus.Duplicate,
          _version,
          null,
          Array.Empty<OutboundPacketIntent>());
      }

      return Rejected(ProtocolErrorCode.Conflict);
    }

    if (_killCounts[bannerId] == int.MaxValue
      || (_killCounts[bannerId] + 1) % killsRequired == 0
        && _claimableCounts[bannerId] == ushort.MaxValue)
    {
      return Rejected(ProtocolErrorCode.Conflict);
    }

    int nextKillCount = _killCounts[bannerId] + 1;
    bool grantsClaim = nextKillCount % killsRequired == 0;
    ushort nextClaimableCount = grantsClaim
      ? (ushort)(_claimableCounts[bannerId] + 1)
      : _claimableCounts[bannerId];
    CommitVersion nextVersion = _version.Next();
    List<OutboundPacketIntent> intents = new(2);
    if (broadcastTargets.Count > 0)
    {
      intents.Add(_producer.CreateIntent(
        (ushort)Packet82ModuleId.Banners,
        new Packet82BannerData(1, null, null, bannerId, nextKillCount, null),
        broadcastTargets,
        nextVersion));
      if (grantsClaim)
      {
        intents.Add(_producer.CreateIntent(
          (ushort)Packet82ModuleId.Banners,
          new Packet82BannerData(2, null, null, bannerId, null, nextClaimableCount),
          broadcastTargets,
          nextVersion));
      }
    }

    _killCounts[bannerId] = nextKillCount;
    if (grantsClaim)
    {
      _claimableCounts[bannerId] = nextClaimableCount;
      AnyNewClaimableBanners = true;
    }

    _version = nextVersion;
    _lastP05CommitVersion = sourceCommitVersion;
    return new BannerCommitResult(
      BannerCommitStatus.Applied,
      _version,
      null,
      Array.AsReadOnly(intents.ToArray()));
  }

  public BannerCommitResult ProcessClaimRequest(
    SessionEpoch authenticatedSender,
    BannerClaimRequest request,
    IReadOnlyCollection<SessionEpoch> broadcastTargets)
  {
    ArgumentNullException.ThrowIfNull(request);
    if (!authenticatedSender.IsValid)
    {
      return Rejected(ProtocolErrorCode.StaleEpoch);
    }

    if (request.BannerId <= 0 || request.BannerId >= BannerSnapshot.BannerCount
        || !HasValidTargets(broadcastTargets))
    {
      return Rejected(ProtocolErrorCode.InvalidLength);
    }

    ushort requested = request.Amount == 0 ? (ushort)1 : request.Amount;
    ushort claimable = _claimableCounts[request.BannerId];
    ushort granted = Math.Min(requested, claimable);
    ushort remainingCount = (ushort)(claimable - granted);
    ushort remainder = (ushort)(requested - granted);
    CommitVersion nextVersion = granted > 0 ? _version.Next() : _version;
    List<OutboundPacketIntent> intents = new(3);
    List<BannerClaimResponse> responses = new(2);
    if (granted > 0)
    {
      if (broadcastTargets.Count > 0)
      {
        intents.Add(_producer.CreateIntent(
          (ushort)Packet82ModuleId.Banners,
          new Packet82BannerData(2, null, null, request.BannerId, null, remainingCount),
          broadcastTargets,
          nextVersion));
      }

      responses.Add(new BannerClaimResponse(request.BannerId, granted, true));
    }

    if (remainder > 0)
    {
      responses.Add(new BannerClaimResponse(request.BannerId, remainder, false));
    }

    foreach (BannerClaimResponse response in responses)
    {
      intents.Add(_producer.CreateIntent(
        (ushort)Packet82ModuleId.Banners,
        new Packet82BannerClaimResponseData(response.BannerId, response.Amount, response.Granted),
        new[] { authenticatedSender },
        nextVersion));
    }

    _claimableCounts[request.BannerId] = remainingCount;
    _version = nextVersion;
    return new BannerCommitResult(
      BannerCommitStatus.Applied,
      _version,
      null,
      Array.AsReadOnly(intents.ToArray()),
      granted);
  }

  public static BannerState Restore(
    Packet82ModuleIntentProducer producer,
    WorldFileBannerSection section,
    CommitVersion version,
    bool anyNewClaimableBanners = false,
    CommitVersion lastP05CommitVersion = default)
  {
    ArgumentNullException.ThrowIfNull(section);
    if (!version.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(version));
    }

    if (lastP05CommitVersion.Value < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(lastP05CommitVersion));
    }

    int count = section.KillCounts.Count;
    if (count != 0 && count != BannerSnapshot.BannerCount
      || section.ClaimableCounts.Count != 0
        && section.ClaimableCounts.Count != BannerSnapshot.BannerCount
      || count != section.ClaimableCounts.Count)
    {
      throw new InvalidDataException(
        "The saved banner state must be empty or contain both complete 293-entry tables.");
    }

    BannerState state = new(producer)
    {
      _version = version,
      _lastP05CommitVersion = lastP05CommitVersion,
      AnyNewClaimableBanners = anyNewClaimableBanners
    };
    for (int index = 0; index < count; index++)
    {
      if (section.KillCounts[index] < 0)
      {
        throw new InvalidDataException("A saved banner kill count cannot be negative.");
      }

      state._killCounts[index] = section.KillCounts[index];
      state._claimableCounts[index] = section.ClaimableCounts[index];
    }

    return state;
  }

  public void ClearNewClaimableBannerFlag()
  {
    AnyNewClaimableBanners = false;
  }

  private static bool HasValidTargets(IReadOnlyCollection<SessionEpoch>? targets)
  {
    if (targets is null)
    {
      return false;
    }

    HashSet<SessionEpoch> uniqueTargets = new();
    foreach (SessionEpoch target in targets)
    {
      if (!target.IsValid)
      {
        return false;
      }

      if (uniqueTargets.Add(target) && uniqueTargets.Count > MaximumTrackedRecipients)
      {
        return false;
      }
    }

    return true;
  }

  private static BannerCommitResult Rejected(ProtocolErrorCode errorCode) => new(
    BannerCommitStatus.Rejected,
    null,
    errorCode,
    Array.Empty<OutboundPacketIntent>());
}

using System.Text;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Modules.Protocol;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NonAuthoritative.Persistence;

namespace MinimalImplementation.Server.Modules.Bestiary;

/// <summary>
/// P07's single-writer Bestiary facts. P05 supplies committed NPC contributions; this class
/// never owns or queries NPC entities.
/// </summary>
public sealed class BestiaryState
{
  private readonly record struct P05ContributionIdentity(
    BestiaryContributionKind Kind,
    short NpcNetId,
    string PersistentNpcId);

  public const int MaximumKillCount = 999_999_999;

  internal const int MaximumEntryCount = 100_000;
  private const int MaximumIdentifierUtf8Bytes = 1_048_576;
  private static readonly UTF8Encoding StrictUtf8 = new(false, true);

  private readonly Dictionary<string, int> _killCounts = new(StringComparer.Ordinal);
  private readonly HashSet<string> _seenNpcIds = new(StringComparer.Ordinal);
  private readonly HashSet<string> _chattedNpcIds = new(StringComparer.Ordinal);
  private readonly Dictionary<SessionEpoch, CommitVersion> _acknowledgedSnapshotVersions = new();
  private readonly Dictionary<SessionEpoch, BestiarySnapshotIntentBatch> _pendingSnapshots = new();
  private readonly Dictionary<P05NpcDeathKey,
      (short NpcNetId, string PersistentNpcId, CommitVersion Version)> _pendingP05Kills = new();
  private readonly Packet82ModuleIntentProducer _producer;
  private CommitVersion _version = CommitVersion.Initial;
  private CommitVersion _lastP05CommitVersion;
  private P05ContributionIdentity? _lastP05Contribution;
  private long _p05WorldScope;

  private const int MaximumTrackedRecipients = byte.MaxValue;
  private const int MaximumPendingP05Kills = 1024;

  public BestiaryState(Packet82ModuleIntentProducer producer)
  {
    ArgumentNullException.ThrowIfNull(producer);
    _producer = producer;
  }

  public BestiarySnapshot CaptureSnapshot() => new(
    _version,
    _killCounts,
    _seenNpcIds,
    _chattedNpcIds,
    _lastP05CommitVersion);

  public IReadOnlyList<OutboundPacketIntent> CreateSnapshotIntents(
    SessionEpoch target,
    IReadOnlyDictionary<string, short> persistentNpcNetIds)
  {
    if (!target.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(target));
    }

    ArgumentNullException.ThrowIfNull(persistentNpcNetIds);
    List<OutboundPacketIntent> intents = new(
      _killCounts.Count + _seenNpcIds.Count + _chattedNpcIds.Count);
    foreach ((string persistentId, int count) in _killCounts.OrderBy(
      item => item.Key,
      StringComparer.Ordinal))
    {
      short npcNetId = ResolveNpcNetId(persistentId, persistentNpcNetIds);
      intents.Add(_producer.CreateIntent(
        (ushort)Packet82ModuleId.Bestiary,
        new Packet82BestiaryData(0, npcNetId, count),
        new[] { target },
        _version));
    }

    foreach (string persistentId in _seenNpcIds.Order(StringComparer.Ordinal))
    {
      short npcNetId = ResolveNpcNetId(persistentId, persistentNpcNetIds);
      intents.Add(_producer.CreateIntent(
        (ushort)Packet82ModuleId.Bestiary,
        new Packet82BestiaryData(1, npcNetId, null),
        new[] { target },
        _version));
    }

    foreach (string persistentId in _chattedNpcIds.Order(StringComparer.Ordinal))
    {
      short npcNetId = ResolveNpcNetId(persistentId, persistentNpcNetIds);
      intents.Add(_producer.CreateIntent(
        (ushort)Packet82ModuleId.Bestiary,
        new Packet82BestiaryData(2, npcNetId, null),
        new[] { target },
        _version));
    }

    return Array.AsReadOnly(intents.ToArray());
  }

  /// <summary>
  /// Captures the current Bestiary projection only when this target has not settled this owner
  /// version. The caller must acknowledge the complete returned batch after transport settlement.
  /// </summary>
  public BestiarySnapshotIntentBatch? CaptureDirtySnapshot(
    SessionEpoch target,
    IReadOnlyDictionary<string, short> persistentNpcNetIds)
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
        "Retire an inactive Bestiary recipient before capturing another target.");
    }

    var batch = new BestiarySnapshotIntentBatch(
      target,
      _version,
      CreateSnapshotIntents(target, persistentNpcNetIds));
    _pendingSnapshots.Add(target, batch);
    return batch;
  }

  /// <summary>
  /// Settles a dirty batch from local transport receipts. This is not client acknowledgement;
  /// failed, unknown, or unsubmitted transport leaves the target dirty for retry.
  /// </summary>
  public ModuleDeliveryResult AcknowledgeSnapshot(
    BestiarySnapshotIntentBatch batch,
    IReadOnlyCollection<SendReceipt> receipts)
  {
    ArgumentNullException.ThrowIfNull(batch);
    ArgumentNullException.ThrowIfNull(receipts);
    return CompleteSnapshot(batch, ModuleDeliveryReceiptEvaluator.Evaluate(
      batch.Intents, batch.Target, receipts));
  }

  public ModuleDeliveryResult AcknowledgeSnapshot(
      BestiarySnapshotIntentBatch batch,
      IReadOnlyList<OutboundIntentDelivery> deliveries) {
    ArgumentNullException.ThrowIfNull(batch);
    return CompleteSnapshot(batch, ModuleDeliveryReceiptEvaluator.Evaluate(
        batch.Intents, batch.Target, deliveries));
  }

  private ModuleDeliveryResult CompleteSnapshot(
      BestiarySnapshotIntentBatch batch, ModuleDeliveryResult result) {
    if (!_pendingSnapshots.TryGetValue(batch.Target, out BestiarySnapshotIntentBatch? pending)
        || !ReferenceEquals(pending, batch))
    {
      return ModuleDeliveryResult.Stale(
        "The Bestiary batch was not captured by this owner or its epoch was retired.");
    }

    if (batch.Version.Value > _version.Value)
    {
      return ModuleDeliveryResult.Stale(
        "A Bestiary projection cannot acknowledge a future owner version.");
    }

    if (_acknowledgedSnapshotVersions.TryGetValue(batch.Target, out CommitVersion acknowledged)
        && batch.Version.Value < acknowledged.Value)
    {
      return ModuleDeliveryResult.Stale(
        "A Bestiary projection acknowledgement is older than the settled target version.");
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
        "The Bestiary owner reached its tracked-recipient limit; retire the epoch first.");
    }

    _acknowledgedSnapshotVersions[batch.Target] = batch.Version;
    return result;
  }

  /// <summary>
  /// Discards only session-bound projection acknowledgements, never Bestiary world state.
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

  /// <summary>Retains a kill receipt until P05 retires the exact source death record.</summary>
  public BestiaryContributionResult ApplyP05Kill(
      P05NpcDeathKey key, short npcNetId, string persistentNpcId,
      IReadOnlyCollection<SessionEpoch> visibleTargets) {
    if (!key.IsValid || npcNetId <= 0 || !IsValidIdentifier(persistentNpcId)
        || !HasValidTargets(visibleTargets)) {
      return Rejected(ProtocolErrorCode.InvalidLength);
    }

    if (_p05WorldScope != 0 && key.WorldScope != _p05WorldScope) {
      return Rejected(ProtocolErrorCode.Conflict);
    }

    if (_pendingP05Kills.TryGetValue(key, out var receipt)) {
      if (receipt.NpcNetId != npcNetId
          || !StringComparer.Ordinal.Equals(receipt.PersistentNpcId, persistentNpcId)) {
        return Rejected(ProtocolErrorCode.Conflict);
      }

      return new BestiaryContributionResult(
          BestiaryContributionStatus.Duplicate, receipt.Version, null,
          Array.Empty<OutboundPacketIntent>());
    }

    if (_pendingP05Kills.Keys.Any(pending => pending.NpcId == key.NpcId
        && pending.WorldScope == key.WorldScope && pending.Generation == key.Generation)) {
      return Rejected(ProtocolErrorCode.Conflict);
    }

    // A missing old key is not proof of delivery, even if its source version matches.
    if (key.SourceCommitVersion.Value <= _lastP05CommitVersion.Value
        || _pendingP05Kills.Count == MaximumPendingP05Kills) {
      return Rejected(ProtocolErrorCode.Conflict);
    }

    BestiaryContributionResult result = ApplyP05Contribution(new BestiaryContribution(
        key.SourceCommitVersion, BestiaryContributionKind.Kill, npcNetId, persistentNpcId,
        visibleTargets));
    if (result.Status == BestiaryContributionStatus.Applied) {
      _pendingP05Kills.Add(key, (npcNetId, persistentNpcId, _version));
      _p05WorldScope = key.WorldScope;
    }

    return result;
  }

  /// <summary>Call only after P05 successfully acknowledges the retained death record.</summary>
  public bool AcknowledgeP05Kill(P05NpcDeathKey key) {
    return _pendingP05Kills.Remove(key);
  }

  public BestiaryContributionResult ApplyP05Contribution(BestiaryContribution contribution)
  {
    ArgumentNullException.ThrowIfNull(contribution);
    if (!contribution.SourceCommitVersion.IsValid
      || contribution.NpcNetId <= 0
      || !IsValidIdentifier(contribution.PersistentNpcId)
      || !Enum.IsDefined(contribution.Kind)
      || !HasValidTargets(contribution.VisibleTargets))
    {
      return Rejected(ProtocolErrorCode.InvalidLength);
    }

    if (contribution.SourceCommitVersion.Value <= _lastP05CommitVersion.Value)
    {
      if (contribution.SourceCommitVersion == _lastP05CommitVersion)
      {
        P05ContributionIdentity sourceIdentity = new(
          contribution.Kind,
          contribution.NpcNetId,
          contribution.PersistentNpcId);
        if (_lastP05Contribution != sourceIdentity)
        {
          return Rejected(ProtocolErrorCode.Conflict);
        }

        return new BestiaryContributionResult(
          BestiaryContributionStatus.Duplicate,
          _version,
          null,
          Array.Empty<OutboundPacketIntent>());
      }

      return Rejected(ProtocolErrorCode.Conflict);
    }

    object payload;
    bool updateKillCount = false;
    bool updateSeen = false;
    bool updateChatted = false;
    int nextKillCount = 0;
    switch (contribution.Kind)
    {
      case BestiaryContributionKind.Kill:
        _killCounts.TryGetValue(contribution.PersistentNpcId, out int currentKillCount);
        if (!_killCounts.ContainsKey(contribution.PersistentNpcId)
            && _killCounts.Count == MaximumEntryCount) {
          return Rejected(ProtocolErrorCode.Conflict);
        }

        // Version4 clamps the stored count but broadcasts the increment before clamping.
        int reportedKillCount = currentKillCount + 1;
        nextKillCount = Math.Min(reportedKillCount, MaximumKillCount);
        updateKillCount = true;
        payload = new Packet82BestiaryData(0, contribution.NpcNetId, reportedKillCount);
        break;
      case BestiaryContributionKind.Sight:
        if (_seenNpcIds.Contains(contribution.PersistentNpcId))
        {
          _lastP05CommitVersion = contribution.SourceCommitVersion;
          _lastP05Contribution = new P05ContributionIdentity(
            contribution.Kind,
            contribution.NpcNetId,
            contribution.PersistentNpcId);
          return new BestiaryContributionResult(
            BestiaryContributionStatus.Duplicate,
            _version,
            null,
            Array.Empty<OutboundPacketIntent>());
        }

        if (_seenNpcIds.Count == MaximumEntryCount)
        {
          return Rejected(ProtocolErrorCode.Conflict);
        }

        updateSeen = true;
        payload = new Packet82BestiaryData(1, contribution.NpcNetId, null);
        break;
      case BestiaryContributionKind.Chat:
        if (_chattedNpcIds.Contains(contribution.PersistentNpcId))
        {
          _lastP05CommitVersion = contribution.SourceCommitVersion;
          _lastP05Contribution = new P05ContributionIdentity(
            contribution.Kind,
            contribution.NpcNetId,
            contribution.PersistentNpcId);
          return new BestiaryContributionResult(
            BestiaryContributionStatus.Duplicate,
            _version,
            null,
            Array.Empty<OutboundPacketIntent>());
        }

        if (_chattedNpcIds.Count == MaximumEntryCount)
        {
          return Rejected(ProtocolErrorCode.Conflict);
        }

        updateChatted = true;
        payload = new Packet82BestiaryData(2, contribution.NpcNetId, null);
        break;
      default:
        return Rejected(ProtocolErrorCode.UnknownModule);
    }

    CommitVersion nextVersion = _version.Next();
    IReadOnlyList<OutboundPacketIntent> intents = contribution.VisibleTargets.Count == 0
      ? Array.Empty<OutboundPacketIntent>()
      : Array.AsReadOnly(new[]
      {
        _producer.CreateIntent(
          (ushort)Packet82ModuleId.Bestiary,
          payload,
          contribution.VisibleTargets,
          nextVersion)
      });

    if (updateKillCount)
    {
      _killCounts[contribution.PersistentNpcId] = nextKillCount;
    }

    if (updateSeen)
    {
      _seenNpcIds.Add(contribution.PersistentNpcId);
    }

    if (updateChatted)
    {
      _chattedNpcIds.Add(contribution.PersistentNpcId);
    }

    _version = nextVersion;
    _lastP05CommitVersion = contribution.SourceCommitVersion;
    _lastP05Contribution = new P05ContributionIdentity(
      contribution.Kind,
      contribution.NpcNetId,
      contribution.PersistentNpcId);
    return new BestiaryContributionResult(
      BestiaryContributionStatus.Applied,
      _version,
      null,
      intents);
  }

  public static BestiaryState Restore(
    Packet82ModuleIntentProducer producer,
    WorldFileBestiarySection section,
    CommitVersion version,
    CommitVersion lastP05CommitVersion = default)
  {
    ArgumentNullException.ThrowIfNull(section);
    BestiaryState state = new(producer);
    if (!version.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(version));
    }

    if (lastP05CommitVersion.Value < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(lastP05CommitVersion));
    }

    ValidateSection(section);
    foreach (WorldFileBestiaryKillCount item in section.KillCounts)
    {
      state._killCounts.Add(item.PersistentId, item.Count);
    }

    state._seenNpcIds.UnionWith(section.SeenNpcIds);
    state._chattedNpcIds.UnionWith(section.ChattedNpcIds);
    state._version = version;
    state._lastP05CommitVersion = lastP05CommitVersion;
    return state;
  }

  private static void ValidateSection(WorldFileBestiarySection section)
  {
    if (section.KillCounts.Count > MaximumEntryCount
      || section.SeenNpcIds.Count > MaximumEntryCount
      || section.ChattedNpcIds.Count > MaximumEntryCount)
    {
      throw new InvalidDataException("The saved Bestiary section contains too many entries.");
    }

    HashSet<string> killNpcIds = new(StringComparer.Ordinal);
    foreach (WorldFileBestiaryKillCount item in section.KillCounts)
    {
      if (item.Count < 0
        || item.Count > MaximumKillCount
        || !IsValidIdentifier(item.PersistentId)
        || !killNpcIds.Add(item.PersistentId))
      {
        throw new InvalidDataException("The saved Bestiary kill table is invalid or duplicated.");
      }
    }

    ValidateIdentifiers(section.SeenNpcIds);
    ValidateIdentifiers(section.ChattedNpcIds);
  }

  private static void ValidateIdentifiers(IReadOnlyList<string> identifiers)
  {
    HashSet<string> uniqueIdentifiers = new(StringComparer.Ordinal);
    foreach (string identifier in identifiers)
    {
      if (!IsValidIdentifier(identifier) || !uniqueIdentifiers.Add(identifier))
      {
        throw new InvalidDataException(
          "The saved Bestiary identifier set is invalid or duplicated.");
      }
    }
  }

  internal static bool IsValidIdentifier(string? identifier)
  {
    if (string.IsNullOrWhiteSpace(identifier))
    {
      return false;
    }

    try
    {
      return StrictUtf8.GetByteCount(identifier) <= MaximumIdentifierUtf8Bytes;
    }
    catch (EncoderFallbackException)
    {
      return false;
    }
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

  private static BestiaryContributionResult Rejected(ProtocolErrorCode errorCode) => new(
    BestiaryContributionStatus.Invalid,
    null,
    errorCode,
    Array.Empty<OutboundPacketIntent>());

  private static short ResolveNpcNetId(
    string persistentId,
    IReadOnlyDictionary<string, short> persistentNpcNetIds)
  {
    if (!persistentNpcNetIds.TryGetValue(persistentId, out short npcNetId) || npcNetId <= 0)
    {
      throw new InvalidOperationException(
        $"The P05 NPC catalog has no valid network ID for Bestiary entry '{persistentId}'.");
    }

    return npcNetId;
  }
}

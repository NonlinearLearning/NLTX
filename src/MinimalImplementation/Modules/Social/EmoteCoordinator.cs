using System.IO;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Modules;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace MinimalImplementation.Server.Modules.Social;

public enum EmoteOutcomeStatus
{
  Applied,
  Duplicate,
  Ignored,
  Rejected
}

/// <summary>Authenticated facts for ordinary Packet 120 Emoji input.</summary>
public sealed record EmoteRequest(
  SessionEpoch Epoch,
  PacketSequence Sequence,
  ModuleSessionStage Stage,
  byte ClaimedPlayerSlot,
  byte AuthenticatedPlayerSlot,
  byte Emote,
  IReadOnlyCollection<SessionEpoch> VisibleTargets);

/// <summary>The current server projection encoded by Packet 91.</summary>
public sealed record EmoteBubbleSnapshot(
  int BubbleId,
  byte AnchorKind,
  ushort AnchorId,
  ushort Lifetime,
  byte Emote,
  short? Metadata);

public sealed record EmoteCommitResult(
  EmoteOutcomeStatus Status,
  CommitVersion? Version,
  ProtocolErrorCode? ErrorCode,
  string? Detail,
  EmoteBubbleSnapshot? Bubble,
  OutboundPacketIntent? Intent)
{
  public static EmoteCommitResult Applied(
    CommitVersion version,
    EmoteBubbleSnapshot bubble,
    OutboundPacketIntent intent)
  {
    ArgumentNullException.ThrowIfNull(bubble);
    ArgumentNullException.ThrowIfNull(intent);
    return new EmoteCommitResult(
      EmoteOutcomeStatus.Applied,
      version,
      null,
      null,
      bubble,
      intent);
  }

  public static EmoteCommitResult Ignored(string detail)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(detail);
    return new EmoteCommitResult(
      EmoteOutcomeStatus.Ignored,
      null,
      null,
      detail,
      null,
      null);
  }

  public static EmoteCommitResult Rejected(
    ProtocolErrorCode errorCode,
    string detail)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(detail);
    return new EmoteCommitResult(
      EmoteOutcomeStatus.Rejected,
      null,
      errorCode,
      detail,
      null,
      null);
  }

  public EmoteCommitResult AsDuplicate()
  {
    return Status is EmoteOutcomeStatus.Rejected or EmoteOutcomeStatus.Ignored
      ? this
      : this with { Status = EmoteOutcomeStatus.Duplicate };
  }
}

/// <summary>
/// Owns only the short-lived P07 emote projection. Player/NPC facts and visibility are supplied
/// by other owners; rebinding the claimed player slot never makes the wire claim authoritative.
/// </summary>
public sealed class EmoteCoordinator : IModuleEpochRetirable
{
  // Visibility is captured only on first execution; replay retains the original intent targets.
  private sealed record ReplayEntry(
    ModuleSessionStage Stage,
    byte ClaimedPlayerSlot,
    byte AuthenticatedPlayerSlot,
    byte Emote,
    EmoteCommitResult Result);

  public const int VanillaEmoteCount = 151;
  public const ushort BubbleLifetimeTicks = 360;

  private const byte PlayerAnchorKind = 1;
  private const int SupersededBubbleLifetimeTicks = 6;
  private const int MaximumTrackedBubbles = 4096;
  private const int MaximumReplayEntries = 4096;
  private const int MaximumTrackedEpochs = byte.MaxValue;
  private const int MaximumRecipientCount = byte.MaxValue;
  private const string RemovalDeliveryOwnerKey = "P07.Emote";

  private readonly Dictionary<ReplayKey, ReplayEntry> _replay = new();
  private readonly Dictionary<SessionEpoch, long> _lastSequenceByEpoch = new();
  private readonly Dictionary<int, ActiveBubble> _activeBubbles = new();
  private readonly Dictionary<CommitVersion, PendingRemoval> _pendingRemovals = new();
  private CommitVersion _version = CommitVersion.Initial;
  private int _nextBubbleId = 1;
  private RemovalDeliveryBatch? _inFlightRemovalBatch;

  public int ActiveBubbleCount => _activeBubbles.Count;

  public EmoteCommitResult Handle(EmoteRequest request)
  {
    ArgumentNullException.ThrowIfNull(request);
    if (!TryBeginRequest(
        request.Epoch, request.Sequence, request, out EmoteCommitResult? replay))
    {
      return replay!;
    }

    if (request.Stage != ModuleSessionStage.Active)
    {
      return Remember(request.Epoch, request.Sequence, request, EmoteCommitResult.Rejected(
        ProtocolErrorCode.WrongStage,
        "Emoji requires an active authenticated session."));
    }

    if (request.AuthenticatedPlayerSlot >= byte.MaxValue)
    {
      return Remember(request.Epoch, request.Sequence, request, EmoteCommitResult.Rejected(
        ProtocolErrorCode.InvalidLength,
        "The authenticated player slot is outside the Version4 player range."));
    }

    if (!TryBuildTargets(
      request.Epoch,
      request.VisibleTargets,
      out SessionEpoch[] targets))
    {
      return Remember(request.Epoch, request.Sequence, request, EmoteCommitResult.Rejected(
        ProtocolErrorCode.InvalidLength,
        "Emoji visibility facts must contain valid session epochs."));
    }

    if (request.Emote >= VanillaEmoteCount)
    {
      return Remember(request.Epoch, request.Sequence, request, EmoteCommitResult.Ignored(
        "The Version4 emote id is outside EmoteID.Count and has no effect."));
    }

    if (_activeBubbles.Count + _pendingRemovals.Count >= MaximumTrackedBubbles)
    {
      return Remember(request.Epoch, request.Sequence, request, EmoteCommitResult.Rejected(
        ProtocolErrorCode.RateLimited,
        "The bounded active and pending emote projections are full."));
    }

    int bubbleId = AllocateBubbleId();
    EmoteBubbleSnapshot bubble = new(
      bubbleId,
      PlayerAnchorKind,
      request.AuthenticatedPlayerSlot,
      BubbleLifetimeTicks,
      request.Emote,
      null);
    CommitVersion nextVersion = _version.Next();
    OutboundPacketIntent intent = CreateBubbleIntent(bubble, targets, nextVersion);
    foreach (ActiveBubble activeBubble in _activeBubbles.Values)
    {
      if (activeBubble.OwnerEpoch == request.Epoch
          && activeBubble.Bubble.AnchorId == request.AuthenticatedPlayerSlot)
      {
        activeBubble.RemainingTicks = SupersededBubbleLifetimeTicks;
      }
    }

    _activeBubbles.Add(
      bubbleId,
      new ActiveBubble(
        request.Epoch,
        bubble,
        BubbleLifetimeTicks,
        new HashSet<SessionEpoch>(targets)));
    _version = nextVersion;

    return Remember(
      request.Epoch,
      request.Sequence,
      request,
      EmoteCommitResult.Applied(nextVersion, bubble, intent));
  }

  /// <summary>Advances the projection clock and retains Packet 91 removals for delivery.</summary>
  public void AdvanceTicks(int ticks)
  {
    if (ticks <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(ticks));
    }

    int[] expiredIds = _activeBubbles
      .Where(pair => pair.Value.RemainingTicks <= ticks)
      .Select(pair => pair.Key)
      .OrderBy(id => id)
      .ToArray();
    foreach (int bubbleId in expiredIds)
    {
      ActiveBubble activeBubble = _activeBubbles[bubbleId];
      if (activeBubble.Targets.Count > 0)
      {
        QueueRemoval(bubbleId, activeBubble.Targets);
      }

      _activeBubbles.Remove(bubbleId);
    }

    foreach (ActiveBubble activeBubble in _activeBubbles.Values)
    {
      activeBubble.RemainingTicks -= ticks;
    }
  }

  /// <summary>
  /// Captures one immutable transport batch. Unsuccessful target receipts stay pending and are
  /// recaptured for retry; queued/local sends settle only transport acceptance, not client receipt.
  /// </summary>
  public IOutboundDeliverySettlement? CapturePendingRemovalDelivery()
  {
    if (_inFlightRemovalBatch is not null)
    {
      return null;
    }

    if (_pendingRemovals.Count == 0)
    {
      return null;
    }

    Guid batchId = Guid.NewGuid();
    List<RemovalDeliveryEntry> entries = new(_pendingRemovals.Count);
    foreach (PendingRemoval removal in _pendingRemovals.Values
      .OrderBy(value => value.Version.Value))
    {
      foreach (SessionEpoch target in removal.Targets.OrderBy(target => target.Value))
      {
        OutboundDeliveryToken token = new(
          RemovalDeliveryOwnerKey,
          batchId,
          target,
          entries.Count);
        OutboundPacketIntent intent = CreateRemovalIntent(
          removal.BubbleId,
          target,
          removal.Version,
          token);
        entries.Add(new RemovalDeliveryEntry(removal, intent, target));
      }
    }

    if (entries.Count == 0)
    {
      return null;
    }

    RemovalDeliveryBatch batch = new(this, batchId, entries);
    _inFlightRemovalBatch = batch;
    return batch;
  }

  /// <summary>
  /// Creates current active projections for a reconnecting target without replaying input.
  /// </summary>
  public IReadOnlyList<OutboundPacketIntent> CreateResyncIntents(SessionEpoch target)
  {
    if (!target.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(target));
    }

    List<OutboundPacketIntent> intents = new(_activeBubbles.Count);
    foreach (ActiveBubble activeBubble in _activeBubbles.Values
      .OrderBy(value => value.Bubble.BubbleId))
    {
      intents.Add(CreateBubbleIntent(
        activeBubble.Bubble with { Lifetime = checked((ushort)activeBubble.RemainingTicks) },
        new[] { target },
        _version));
      activeBubble.Targets.Add(target);
    }

    return Array.AsReadOnly(intents.ToArray());
  }

  /// <summary>
  /// Removes connection-bound state and queues cleanup for the remaining recipients.
  /// </summary>
  public void RetireEpoch(SessionEpoch epoch)
  {
    if (!epoch.IsValid)
    {
      throw new ArgumentException("A valid session epoch is required.", nameof(epoch));
    }

    ReplayKey[] replayKeys = _replay.Keys
      .Where(key => key.Epoch == epoch)
      .ToArray();
    foreach (ReplayKey key in replayKeys)
    {
      _replay.Remove(key);
    }

    _lastSequenceByEpoch.Remove(epoch);
    int[] ownedBubbleIds = _activeBubbles
      .Where(pair => pair.Value.OwnerEpoch == epoch)
      .Select(pair => pair.Key)
      .ToArray();
    foreach (int bubbleId in ownedBubbleIds)
    {
      ActiveBubble activeBubble = _activeBubbles[bubbleId];
      activeBubble.Targets.Remove(epoch);
      if (activeBubble.Targets.Count > 0)
      {
        QueueRemoval(bubbleId, activeBubble.Targets);
      }

      _activeBubbles.Remove(bubbleId);
    }

    int[] remainingBubbleIds = _activeBubbles.Keys.ToArray();
    foreach (int bubbleId in remainingBubbleIds)
    {
      ActiveBubble activeBubble = _activeBubbles[bubbleId];
      activeBubble.Targets.Remove(epoch);
      if (activeBubble.Targets.Count == 0)
      {
        _activeBubbles.Remove(bubbleId);
      }
    }

    foreach (PendingRemoval removal in _pendingRemovals.Values.ToArray())
    {
      removal.Targets.Remove(epoch);
      if (removal.Targets.Count == 0)
      {
        _pendingRemovals.Remove(removal.Version);
      }
    }
  }

  private void QueueRemoval(int bubbleId, IReadOnlyCollection<SessionEpoch> targets)
  {
    CommitVersion nextVersion = _version.Next();
    PendingRemoval removal = new(bubbleId, nextVersion, targets);
    _pendingRemovals.Add(nextVersion, removal);
    _version = nextVersion;
  }

  private OutboundDeliverySettlementResult SettleRemovalDelivery(
    RemovalDeliveryBatch batch,
    IReadOnlyList<OutboundIntentDelivery> deliveries)
  {
    ArgumentNullException.ThrowIfNull(deliveries);
    if (!ReferenceEquals(_inFlightRemovalBatch, batch))
    {
      return OutboundDeliverySettlementResult.Invalid(
        RemovalDeliveryOwnerKey,
        "The emote removal batch is not the current owner capture.");
    }

    if (deliveries.Count != batch.Entries.Count)
    {
      _inFlightRemovalBatch = null;
      return OutboundDeliverySettlementResult.Invalid(
        RemovalDeliveryOwnerKey,
        "The emote removal settlement must retain one record per captured intent.");
    }

    List<(PendingRemoval Removal, SessionEpoch Target)> acknowledgedTargets = new();
    HashSet<(SessionEpoch Epoch, long Sequence)> sequences = new();
    for (int index = 0; index < batch.Entries.Count; index++)
    {
      RemovalDeliveryEntry entry = batch.Entries[index];
      OutboundIntentDelivery? delivery = deliveries[index];
      if (delivery is null || !ReferenceEquals(delivery.Intent, entry.Intent))
      {
        _inFlightRemovalBatch = null;
        return OutboundDeliverySettlementResult.Invalid(
          RemovalDeliveryOwnerKey,
          "The emote removal settlement must preserve intent identity and order.");
      }

      OutboundDeliveryToken? token = delivery.Intent.DeliveryToken;
      if (token is null
          || token.OwnerKey != RemovalDeliveryOwnerKey
          || token.BatchId != batch.BatchId
          || token.Target != entry.Target
          || token.IntentIndex != index
          || delivery.Intent.CommitVersion != entry.Removal.Version
          || delivery.Intent.IgnoreEpoch is not null
          || delivery.Intent.TargetEpochs.Count != 1
          || delivery.Intent.TargetEpochs[0] != entry.Target)
      {
        _inFlightRemovalBatch = null;
        return OutboundDeliverySettlementResult.Invalid(
          RemovalDeliveryOwnerKey,
          "Every emote removal intent must carry its ordered target delivery token.");
      }

      if (delivery.Receipts.Count > 1)
      {
        _inFlightRemovalBatch = null;
        return OutboundDeliverySettlementResult.Invalid(
          RemovalDeliveryOwnerKey,
          "An emote removal intent cannot have multiple target receipts.");
      }

      if (delivery.Receipts.Count == 0)
      {
        continue;
      }

      SendReceipt? receipt = delivery.Receipts[0];
      if (receipt is null || receipt.Epoch != entry.Target || !receipt.Sequence.IsValid)
      {
        _inFlightRemovalBatch = null;
        return OutboundDeliverySettlementResult.Invalid(
          RemovalDeliveryOwnerKey,
          "Every emote removal receipt must match its target and carry a valid sequence.");
      }

      switch (receipt.Certainty)
      {
        case TransportSendCertainty.Queued:
        case TransportSendCertainty.LocallySent:
          if (!sequences.Add((receipt.Epoch, receipt.Sequence.Value)))
          {
            _inFlightRemovalBatch = null;
            return OutboundDeliverySettlementResult.Invalid(
              RemovalDeliveryOwnerKey,
              "Accepted emote removal receipts must use unique target sequences.");
          }

          acknowledgedTargets.Add((entry.Removal, entry.Target));
          break;
        case TransportSendCertainty.NotSubmitted:
        case TransportSendCertainty.OutcomeUnknown:
        case TransportSendCertainty.Failed:
          break;
        default:
          _inFlightRemovalBatch = null;
          return OutboundDeliverySettlementResult.Invalid(
            RemovalDeliveryOwnerKey,
            "The transport returned an unknown emote removal certainty.");
      }
    }

    foreach ((PendingRemoval removal, SessionEpoch target) in acknowledgedTargets)
    {
      if (!_pendingRemovals.TryGetValue(removal.Version, out PendingRemoval? current)
          || !ReferenceEquals(current, removal))
      {
        continue;
      }

      current.Targets.Remove(target);
      if (current.Targets.Count == 0)
      {
        _pendingRemovals.Remove(current.Version);
      }
    }

    _inFlightRemovalBatch = null;
    bool settled = batch.Entries.All(entry =>
      !_pendingRemovals.TryGetValue(entry.Removal.Version, out PendingRemoval? current)
      || !ReferenceEquals(current, entry.Removal)
      || !current.Targets.Contains(entry.Target));
    return new OutboundDeliverySettlementResult(
      RemovalDeliveryOwnerKey,
      settled,
      settled
        ? "Every captured emote removal target was queued or locally sent, or its epoch retired."
        : "Some emote removal targets remain pending for a later capture.");
  }

  private bool TryBeginRequest(
    SessionEpoch epoch,
    PacketSequence sequence,
    EmoteRequest request,
    out EmoteCommitResult? replay)
  {
    if (!epoch.IsValid || !sequence.IsValid)
    {
      replay = EmoteCommitResult.Rejected(
        ProtocolErrorCode.MalformedFrame,
        "An Emoji request requires a valid session epoch and sequence.");
      return false;
    }

    ReplayKey key = new(epoch, sequence);
    if (_replay.TryGetValue(key, out ReplayEntry? cached))
    {
      bool matches = cached.Stage == request.Stage
        && cached.ClaimedPlayerSlot == request.ClaimedPlayerSlot
        && cached.AuthenticatedPlayerSlot == request.AuthenticatedPlayerSlot
        && cached.Emote == request.Emote;
      replay = matches
        ? cached.Result.AsDuplicate()
        : EmoteCommitResult.Rejected(
          ProtocolErrorCode.Conflict,
          "The Emoji sequence was reused for a different request.");
      return false;
    }

    if (_lastSequenceByEpoch.TryGetValue(epoch, out long lastSequence)
        && sequence.Value <= lastSequence)
    {
      replay = EmoteCommitResult.Rejected(
        ProtocolErrorCode.Conflict,
        "The Emoji sequence is older than the active replay window.");
      return false;
    }

    if (!_lastSequenceByEpoch.ContainsKey(epoch)
        && _lastSequenceByEpoch.Count >= MaximumTrackedEpochs)
    {
      replay = EmoteCommitResult.Rejected(
        ProtocolErrorCode.RateLimited,
        "The bounded active emote session table is full.");
      return false;
    }

    _lastSequenceByEpoch[epoch] = sequence.Value;
    replay = null;
    return true;
  }

  private EmoteCommitResult Remember(
    SessionEpoch epoch,
    PacketSequence sequence,
    EmoteRequest request,
    EmoteCommitResult result)
  {
    if (_replay.Count >= MaximumReplayEntries)
    {
      ReplayKey oldest = _replay.Keys.First();
      _replay.Remove(oldest);
    }

    _replay[new ReplayKey(epoch, sequence)] = new ReplayEntry(
      request.Stage,
      request.ClaimedPlayerSlot,
      request.AuthenticatedPlayerSlot,
      request.Emote,
      result);
    return result;
  }

  private int AllocateBubbleId()
  {
    for (int attempt = 0; attempt < int.MaxValue; attempt++)
    {
      int candidate = _nextBubbleId;
      _nextBubbleId = _nextBubbleId == int.MaxValue ? 1 : _nextBubbleId + 1;
      if (!_activeBubbles.ContainsKey(candidate)
          && !_pendingRemovals.Values.Any(removal => removal.BubbleId == candidate))
      {
        return candidate;
      }
    }

    throw new InvalidOperationException("The emote bubble id space is exhausted.");
  }

  private static bool TryBuildTargets(
    SessionEpoch sender,
    IReadOnlyCollection<SessionEpoch>? visibleTargets,
    out SessionEpoch[] targets)
  {
    targets = Array.Empty<SessionEpoch>();
    if (visibleTargets is null)
    {
      return false;
    }

    HashSet<SessionEpoch> uniqueTargets = new();
    foreach (SessionEpoch target in visibleTargets)
    {
      if (!target.IsValid)
      {
        return false;
      }

      uniqueTargets.Add(target);
    }

    uniqueTargets.Add(sender);
    if (uniqueTargets.Count > MaximumRecipientCount)
    {
      return false;
    }

    targets = uniqueTargets.OrderBy(epoch => epoch.Value).ToArray();
    return true;
  }

  private static OutboundPacketIntent CreateBubbleIntent(
    EmoteBubbleSnapshot bubble,
    IReadOnlyCollection<SessionEpoch> targets,
    CommitVersion version)
  {
    using MemoryStream stream = new();
    PacketWireWriter writer = new(stream, 1_048_576);
    SyncEmoteBubblePacket.Write(
      writer,
      bubble.BubbleId,
      bubble.AnchorKind,
      bubble.AnchorId,
      bubble.Lifetime,
      bubble.Emote,
      bubble.Metadata);
    return new OutboundPacketIntent(targets, null, 91, stream.ToArray(), version);
  }

  private static OutboundPacketIntent CreateRemovalIntent(
    int bubbleId,
    SessionEpoch target,
    CommitVersion version,
    OutboundDeliveryToken deliveryToken)
  {
    using MemoryStream stream = new();
    PacketWireWriter writer = new(stream, 1_048_576);
    SyncEmoteBubblePacket.Write(
      writer,
      bubbleId,
      byte.MaxValue,
      null,
      null,
      null,
      null);
    return new OutboundPacketIntent(
      new[] { target },
      null,
      91,
      stream.ToArray(),
      version,
      deliveryToken);
  }

  private sealed class ActiveBubble
  {
    public ActiveBubble(
      SessionEpoch ownerEpoch,
      EmoteBubbleSnapshot bubble,
      int remainingTicks,
      HashSet<SessionEpoch> targets)
    {
      OwnerEpoch = ownerEpoch;
      Bubble = bubble;
      RemainingTicks = remainingTicks;
      Targets = targets;
    }

    public SessionEpoch OwnerEpoch { get; }

    public EmoteBubbleSnapshot Bubble { get; }

    public int RemainingTicks { get; set; }

    public HashSet<SessionEpoch> Targets { get; }
  }

  private sealed class PendingRemoval
  {
    public PendingRemoval(
      int bubbleId,
      CommitVersion version,
      IReadOnlyCollection<SessionEpoch> targets)
    {
      BubbleId = bubbleId;
      Version = version;
      Targets = new HashSet<SessionEpoch>(targets);
    }

    public int BubbleId { get; }

    public CommitVersion Version { get; }

    public HashSet<SessionEpoch> Targets { get; }
  }

  private sealed record RemovalDeliveryEntry(
    PendingRemoval Removal,
    OutboundPacketIntent Intent,
    SessionEpoch Target);

  private sealed class RemovalDeliveryBatch : IOutboundDeliverySettlement
  {
    private readonly EmoteCoordinator _owner;

    public RemovalDeliveryBatch(
      EmoteCoordinator owner,
      Guid batchId,
      IReadOnlyList<RemovalDeliveryEntry> entries)
    {
      _owner = owner;
      BatchId = batchId;
      Entries = Array.AsReadOnly(entries.ToArray());
      Intents = Array.AsReadOnly(Entries.Select(entry => entry.Intent).ToArray());
    }

    public Guid BatchId { get; }

    public IReadOnlyList<RemovalDeliveryEntry> Entries { get; }

    public string OwnerKey => RemovalDeliveryOwnerKey;

    public IReadOnlyList<OutboundPacketIntent> Intents { get; }

    public OutboundDeliverySettlementResult Settle(
      IReadOnlyList<OutboundIntentDelivery> deliveries)
    {
      return _owner.SettleRemovalDelivery(this, deliveries);
    }
  }

  private readonly record struct ReplayKey(SessionEpoch Epoch, PacketSequence Sequence);
}

using System.Runtime.CompilerServices;
using MinimalImplementation.Contracts;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Modules.Creative;

/// <summary>Owns the mutable Version4 creative-power values and their permission levels.</summary>
public sealed class CreativePowerStateOwner
{
  private sealed class EffectDeliveryProgress {
    public EffectDeliveryProgress(
        CreativePowerEffectDeliveryBatch original,
        Dictionary<SessionEpoch, object> lifetimes) {
      Original = original;
      Lifetimes = lifetimes;
      InFlightOriginalIndices = Enumerable.Range(0, original.Intents.Count).ToArray();
      PendingIntentIndices = new HashSet<int>(InFlightOriginalIndices);
      InFlightBatch = original;
    }

    public CreativePowerEffectDeliveryBatch Original { get; }

    public Dictionary<SessionEpoch, object> Lifetimes { get; }

    public HashSet<int> PendingIntentIndices { get; }

    public CreativePowerEffectDeliveryBatch? InFlightBatch { get; set; }

    public int[] InFlightOriginalIndices { get; set; }

    public bool HasQueuedIntents { get; set; }
  }

  private const int PlayerCount = 255;
  private readonly bool[] _sharedToggles = new bool[15];
  private readonly float[] _sharedSliders = new float[15];
  private readonly bool[][] _playerToggles = CreatePlayerToggleTables();
  private readonly float[][] _playerSliders = CreatePlayerSliderTables();
  private readonly CreativePowerPermissionLevel[] _permissions = CreateDefaultPermissions();
  private readonly Dictionary<SessionEpoch, CommitVersion> _acknowledgedPermissionVersions = new();
  private readonly Dictionary<SessionEpoch, CreativePermissionSnapshotBatch>
      _pendingPermissionSnapshots = new();
  private readonly Dictionary<SessionEpoch, CommitVersion> _acknowledgedEffectVersions = new();
  private readonly Dictionary<SessionEpoch, object> _effectRecipientLifetimes = new();
  // A retained attempt keeps its original effect and partial progress alive for later captures.
  private readonly ConditionalWeakTable<CreativePowerEffectDeliveryBatch, EffectDeliveryProgress>
      _effectDeliveries = new();
  private CommitVersion _version = CommitVersion.Initial;

  private const int MaximumTrackedRecipients = byte.MaxValue;

  public CreativePowerStateOwner()
  {
    Array.Fill(_playerToggles[11], true);
    Array.Fill(_playerSliders[14], 0.5f);
  }

  public CreativePowerApplyResult Apply(
    byte authenticatedPlayerId,
    bool countsAsGameplayHost,
    CreativePowerWireMessage message)
  {
    ArgumentNullException.ThrowIfNull(message);
    if (authenticatedPlayerId >= PlayerCount)
    {
      return Rejected(
        ProtocolErrorCode.StaleEntity,
        "Authenticated player id is outside the 255-player table.");
    }

    if (message.PowerId > 14 || !HasExpectedValueKind(message.PowerId, message.ValueKind))
    {
      return Rejected(
        ProtocolErrorCode.MalformedFrame,
        "The creative-power value kind does not match the Version4 power registration.");
    }

    if ((message.ValueKind is CreativePowerValueKind.SharedSlider
        or CreativePowerValueKind.PerPlayerSlider)
        && (!float.IsFinite(message.SliderValue)
          || message.SliderValue is < 0f or > 1f))
    {
      return Rejected(
        ProtocolErrorCode.InvalidLength,
        "Creative power slider input must be finite and within the 0 to 1 range.");
    }

    if (message.ValueKind == CreativePowerValueKind.PerPlayerToggleSync
        && message.PlayerStates is not { Count: PlayerCount })
    {
      return Rejected(
        ProtocolErrorCode.InvalidLength,
        "Per-player creative toggles require exactly 255 player states.");
    }

    if (!IsAvailableForPlayer(message.PowerId, countsAsGameplayHost))
    {
      return Rejected(
        ProtocolErrorCode.Forbidden,
        "The current creative-power permission denies this player.");
    }

    return message.ValueKind switch
    {
      CreativePowerValueKind.SharedToggle => ApplySharedToggle(authenticatedPlayerId, message),
      CreativePowerValueKind.Button => ApplyButton(authenticatedPlayerId, message),
      CreativePowerValueKind.PerPlayerToggleSync =>
        ApplyPlayerToggleSync(authenticatedPlayerId, message),
      CreativePowerValueKind.PerPlayerToggle => ApplyPlayerToggle(authenticatedPlayerId, message),
      CreativePowerValueKind.SharedSlider => ApplySharedSlider(authenticatedPlayerId, message),
      CreativePowerValueKind.PerPlayerSlider => ApplyPlayerSlider(authenticatedPlayerId, message),
      _ => Rejected(
        ProtocolErrorCode.MalformedFrame,
        "The creative-power value kind is not supported by Version4.")
    };
  }

  public OwnerCommitResult SetPermission(ushort powerId, CreativePowerPermissionLevel level)
  {
    if (powerId > 14 || !Enum.IsDefined(level))
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.MalformedFrame,
        "Creative permission update has an unknown power or permission level.");
    }

    if (_permissions[powerId] == level)
    {
      return OwnerCommitResult.Applied(_version);
    }

    CommitVersion nextVersion = _version.Next();
    _permissions[powerId] = level;
    _version = nextVersion;
    return OwnerCommitResult.Applied(_version);
  }

  public CreativePowerSnapshot CaptureSnapshot()
  {
    var sharedToggles = new Dictionary<ushort, bool>();
    var sharedSliders = new Dictionary<ushort, float>();
    var playerToggles = new Dictionary<ushort, IReadOnlyList<bool>>();
    var playerSliders = new Dictionary<ushort, IReadOnlyList<float>>();
    var permissions = new Dictionary<ushort, CreativePowerPermissionLevel>();

    for (ushort powerId = 0; powerId < 15; powerId++)
    {
      permissions.Add(powerId, _permissions[powerId]);
      if (IsSharedToggle(powerId))
      {
        sharedToggles.Add(powerId, _sharedToggles[powerId]);
      }
      else if (IsSharedSlider(powerId))
      {
        sharedSliders.Add(powerId, _sharedSliders[powerId]);
      }
      else if (IsPerPlayerToggle(powerId))
      {
        playerToggles.Add(powerId, Array.AsReadOnly((bool[])_playerToggles[powerId].Clone()));
      }
      else if (powerId == 14)
      {
        playerSliders.Add(powerId, Array.AsReadOnly((float[])_playerSliders[powerId].Clone()));
      }
    }

    IReadOnlyDictionary<ushort, IReadOnlyList<bool>> readOnlyPlayerToggles =
      new System.Collections.ObjectModel.ReadOnlyDictionary<ushort, IReadOnlyList<bool>>(
        playerToggles);
    IReadOnlyDictionary<ushort, IReadOnlyList<float>> readOnlyPlayerSliders =
      new System.Collections.ObjectModel.ReadOnlyDictionary<ushort, IReadOnlyList<float>>(
        playerSliders);
    IReadOnlyDictionary<ushort, CreativePowerPermissionLevel> readOnlyPermissions =
      new System.Collections.ObjectModel.ReadOnlyDictionary<ushort, CreativePowerPermissionLevel>(
        permissions);

    return new CreativePowerSnapshot(
      _version,
      new System.Collections.ObjectModel.ReadOnlyDictionary<ushort, bool>(sharedToggles),
      new System.Collections.ObjectModel.ReadOnlyDictionary<ushort, float>(sharedSliders),
      readOnlyPlayerToggles,
      readOnlyPlayerSliders,
      readOnlyPermissions);
  }

  public CreativeWorldPowerState CaptureWorldState()
  {
    return new CreativeWorldPowerState(
      FreezeTime: _sharedToggles[0],
      ModifyTimeRate: _sharedSliders[8],
      FreezeRain: _sharedToggles[9],
      FreezeWind: _sharedToggles[10],
      DifficultySlider: _sharedSliders[12],
      StopBiomeSpread: _sharedToggles[13]);
  }

  public bool IsAvailableForPlayer(ushort powerId, bool countsAsGameplayHost)
  {
    return powerId <= 14 && IsAvailableForPlayer(_permissions[powerId], countsAsGameplayHost);
  }

  public static CreativePowerStateOwner Restore(CreativePowerSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    CreativePowerStateOwner owner = new()
    {
      _version = snapshot.Version
    };

    foreach ((ushort powerId, bool value) in snapshot.SharedToggles)
    {
      owner._sharedToggles[powerId] = value;
    }

    foreach ((ushort powerId, float value) in snapshot.SharedSliders)
    {
      owner._sharedSliders[powerId] = value;
    }

    foreach ((ushort powerId, IReadOnlyList<bool> values) in snapshot.PlayerToggles)
    {
      for (int playerId = 0; playerId < PlayerCount; playerId++)
      {
        owner._playerToggles[powerId][playerId] = values[playerId];
      }
    }

    foreach ((ushort powerId, IReadOnlyList<float> values) in snapshot.PlayerSliders)
    {
      for (int playerId = 0; playerId < PlayerCount; playerId++)
      {
        owner._playerSliders[powerId][playerId] = values[playerId];
      }
    }

    foreach ((ushort powerId, CreativePowerPermissionLevel level) in snapshot.Permissions)
    {
      owner._permissions[powerId] = level;
    }

    return owner;
  }

  public IReadOnlyList<CreativePermissionSnapshotEntry> CapturePermissionSnapshot()
  {
    CreativePermissionSnapshotEntry[] entries =
      new CreativePermissionSnapshotEntry[_permissions.Length];
    for (ushort powerId = 0; powerId < _permissions.Length; powerId++)
    {
      entries[powerId] = new CreativePermissionSnapshotEntry(
        powerId,
        _permissions[powerId],
        _version);
    }

    return Array.AsReadOnly(entries);
  }

  public IReadOnlyList<Packet82PermissionData> CapturePermissionPacketData()
  {
    CreativePermissionSnapshotEntry[] entries = CapturePermissionSnapshot().ToArray();
    Packet82PermissionData[] packetData = new Packet82PermissionData[entries.Length];
    for (int index = 0; index < entries.Length; index++)
    {
      packetData[index] = entries[index].ToPacketData();
    }

    return Array.AsReadOnly(packetData);
  }

  /// <summary>
  /// Captures the permission projection only when this target has not settled this owner version.
  /// One batch per target stays in flight until settlement or retirement. The caller must submit
  /// one transport intent for each returned entry before acknowledging the exact returned batch.
  /// </summary>
  public CreativePermissionSnapshotBatch? CaptureDirtyPermissionSnapshot(SessionEpoch target)
  {
    if (!target.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(target));
    }

    if (_pendingPermissionSnapshots.ContainsKey(target)) {
      return null;
    }

    if (_acknowledgedPermissionVersions.TryGetValue(target, out CommitVersion acknowledged)
        && acknowledged.Value >= _version.Value)
    {
      return null;
    }

    if (!_acknowledgedPermissionVersions.ContainsKey(target)
        && _acknowledgedPermissionVersions.Keys.Union(_pendingPermissionSnapshots.Keys).Count()
          >= MaximumTrackedRecipients) {
      throw new InvalidOperationException(
          "Retire an inactive Creative permission recipient before capturing another target.");
    }

    var batch = new CreativePermissionSnapshotBatch(
      target,
      _version,
      CapturePermissionPacketData());
    _pendingPermissionSnapshots.Add(target, batch);
    return batch;
  }

  /// <summary>
  /// Settles one permission projection from one receipt per permission entry. Failed, unknown,
  /// missing, or invalid receipts release the current capture and leave the target dirty for retry.
  /// Stale or foreign batches cannot release a newer capture or acknowledge a retired epoch.
  /// </summary>
  public CreativePermissionDeliveryResult AcknowledgePermissionSnapshot(
    CreativePermissionSnapshotBatch batch,
    IReadOnlyCollection<SendReceipt> receipts)
  {
    ArgumentNullException.ThrowIfNull(batch);
    ArgumentNullException.ThrowIfNull(receipts);
    if (!_pendingPermissionSnapshots.TryGetValue(
        batch.Target, out CreativePermissionSnapshotBatch? pending)
        || !ReferenceEquals(pending, batch)) {
      return CreativePermissionDeliveryResult.Stale(
          "The Creative permission batch is not this owner's current capture.");
    }

    _pendingPermissionSnapshots.Remove(batch.Target);
    if (batch.Version.Value > _version.Value)
    {
      return CreativePermissionDeliveryResult.Stale(
        "A Creative permission projection cannot acknowledge a future owner version.");
    }

    if (_acknowledgedPermissionVersions.TryGetValue(batch.Target, out CommitVersion acknowledged)
        && batch.Version.Value < acknowledged.Value)
    {
      return CreativePermissionDeliveryResult.Stale(
        "A Creative permission acknowledgement is older than the settled target version.");
    }

    if (batch.Version == _version && !MatchesCurrentPermissionState(batch.Entries))
    {
      return CreativePermissionDeliveryResult.Invalid(
        "The Creative permission batch does not match the current owner version.");
    }

    CreativePermissionDeliveryResult result = EvaluatePermissionReceipts(batch, receipts);
    if (!result.IsSettled)
    {
      return result;
    }

    if (!_acknowledgedPermissionVersions.ContainsKey(batch.Target)
        && _acknowledgedPermissionVersions.Count == MaximumTrackedRecipients)
    {
      return CreativePermissionDeliveryResult.Invalid(
        "The Creative owner reached its tracked-recipient limit; retire the epoch first.");
    }

    _acknowledgedPermissionVersions[batch.Target] = batch.Version;
    return result;
  }

  /// <summary>
  /// Freezes the owner/effect association after the root has selected recipients and encoded one
  /// intent per message and target. Retain this batch or its settlement until delivery completes.
  /// Capture, settlement and retirement must run on the same server commit boundary.
  /// </summary>
  public CreativePowerEffectDeliveryBatch CreateEffectDeliveryBatch(
    CreativePowerEffect effect,
    IReadOnlyCollection<OutboundPacketIntent> intents)
  {
    ArgumentNullException.ThrowIfNull(effect);
    ArgumentNullException.ThrowIfNull(intents);
    if (effect.CommitVersion.Value > _version.Value)
    {
      throw new ArgumentException(
        "A creative-power effect cannot reference a future owner version.",
        nameof(effect));
    }

    var batch = new CreativePowerEffectDeliveryBatch(effect, intents);
    SessionEpoch[] targets = batch.Targets.Distinct().ToArray();
    int newTargetCount = targets.Count(target => !_effectRecipientLifetimes.ContainsKey(target));
    if (_effectRecipientLifetimes.Count + newTargetCount > MaximumTrackedRecipients) {
      throw new InvalidOperationException(
          "Retire an inactive Creative effect recipient before capturing another target.");
    }

    var lifetimes = new Dictionary<SessionEpoch, object>(targets.Length);
    foreach (SessionEpoch target in targets) {
      if (!_effectRecipientLifetimes.TryGetValue(target, out object? lifetime)) {
        lifetime = new object();
        _effectRecipientLifetimes.Add(target, lifetime);
      }
      lifetimes.Add(target, lifetime);
    }
    _effectDeliveries.Add(batch, new EffectDeliveryProgress(batch, lifetimes));
    return batch;
  }

  /// <summary>
  /// Settles one receipt per active target intent. Retired targets cannot advance a watermark.
  /// A report ends this attempt; failed, unknown or missing intents await a new pending capture.
  /// Invalid reports retain all previously pending intents without accepting partial successes.
  /// Raw receipts must retain every intent's position; production uses the associated settlement.
  /// </summary>
  public CreativePowerEffectDeliveryResult AcknowledgeEffectDelivery(
    CreativePowerEffectDeliveryBatch batch,
    IReadOnlyCollection<SendReceipt> receipts)
  {
    ArgumentNullException.ThrowIfNull(batch);
    ArgumentNullException.ThrowIfNull(receipts);
    if (receipts.Count != 0 && receipts.Count != batch.Intents.Count) {
      return CreativePowerEffectDeliveryResult.Invalid(
          "The effect receipt count must preserve one receipt per ordered intent.");
    }

    SendReceipt[] copiedReceipts = receipts.ToArray();
    var deliveries = new List<OutboundIntentDelivery>(batch.Intents.Count);
    for (int index = 0; index < batch.Intents.Count; index++) {
      SendReceipt[] intentReceipts = copiedReceipts.Length == 0
        ? Array.Empty<SendReceipt>()
        : new[] { copiedReceipts[index] };
      deliveries.Add(new OutboundIntentDelivery(
          batch.Intents[index], intentReceipts));
    }
    return SettleEffectDelivery(batch, deliveries);
  }

  /// <summary>
  /// Discards target delivery records and invalidates that target in captured batches.
  /// </summary>
  public void RetireEpoch(SessionEpoch epoch)
  {
    if (!epoch.IsValid)
    {
      throw new ArgumentException("A valid session epoch is required.", nameof(epoch));
    }

    _acknowledgedPermissionVersions.Remove(epoch);
    _pendingPermissionSnapshots.Remove(epoch);
    _acknowledgedEffectVersions.Remove(epoch);
    _effectRecipientLifetimes.Remove(epoch);
  }

  internal CreativePowerEffectDeliveryResult SettleEffectDelivery(
      CreativePowerEffectDeliveryBatch batch,
      IReadOnlyList<OutboundIntentDelivery> deliveries) {
    ArgumentNullException.ThrowIfNull(batch);
    ArgumentNullException.ThrowIfNull(deliveries);
    if (!_effectDeliveries.TryGetValue(batch, out EffectDeliveryProgress? progress)
        || !ReferenceEquals(progress.InFlightBatch, batch)) {
      return CreativePowerEffectDeliveryResult.Stale(
          "The Creative effect batch is not the current capture from this owner.");
    }
    if (batch.Version.Value > _version.Value) {
      return CreativePowerEffectDeliveryResult.Stale(
          "A creative-power effect cannot acknowledge a future owner version.");
    }

    progress.InFlightBatch = null;
    HashSet<SessionEpoch> activeTargets = GetActiveEffectTargets(progress);
    if (progress.Original.Intents.Count != 0 && activeTargets.Count == 0) {
      progress.PendingIntentIndices.Clear();
      return CreativePowerEffectDeliveryResult.Stale(
          "Every captured Creative effect recipient has retired.");
    }

    CreativePowerEffectDeliveryResult result = EvaluateEffectReceipts(
        batch, deliveries, activeTargets, progress.HasQueuedIntents,
        out List<int> acceptedIntentIndices);
    if (result.Status == CreativePowerEffectDeliveryStatus.InvalidReceipt) {
      return result;
    }

    progress.PendingIntentIndices.RemoveWhere(
        index => !activeTargets.Contains(progress.Original.Targets[index]));
    foreach (int index in acceptedIntentIndices) {
      progress.PendingIntentIndices.Remove(progress.InFlightOriginalIndices[index]);
      if (deliveries[index].Receipts[0].Certainty == TransportSendCertainty.Queued) {
        progress.HasQueuedIntents = true;
      }
    }
    var pendingTargets = new HashSet<SessionEpoch>(progress.PendingIntentIndices.Select(
        index => progress.Original.Targets[index]));
    foreach (SessionEpoch target in activeTargets) {
      if (pendingTargets.Contains(target)) {
        continue;
      }
      // Later effects may settle first; their watermark does not prove delivery of this effect.
      if (!_acknowledgedEffectVersions.TryGetValue(target, out CommitVersion acknowledged)
          || batch.Version.Value > acknowledged.Value) {
        _acknowledgedEffectVersions[target] = batch.Version;
      }
    }
    return result;
  }

  internal bool IsEffectDeliverySettled(CreativePowerEffectDeliveryBatch batch) {
    if (!_effectDeliveries.TryGetValue(batch, out EffectDeliveryProgress? progress)) {
      return false;
    }

    HashSet<SessionEpoch> activeTargets = GetActiveEffectTargets(progress);
    return !progress.PendingIntentIndices.Any(
        index => activeTargets.Contains(progress.Original.Targets[index]));
  }

  internal CreativePowerEffectDeliveryBatch? CapturePendingEffectDelivery(
      CreativePowerEffectDeliveryBatch previous) {
    if (!_effectDeliveries.TryGetValue(previous, out EffectDeliveryProgress? progress)
        || progress.InFlightBatch is not null) {
      return null;
    }

    HashSet<SessionEpoch> activeTargets = GetActiveEffectTargets(progress);
    int[] originalIndices = progress.PendingIntentIndices
      .Where(index => activeTargets.Contains(progress.Original.Targets[index]))
      .OrderBy(index => index)
      .ToArray();
    if (originalIndices.Length == 0) {
      return null;
    }

    Guid batchId = Guid.NewGuid();
    var intents = new List<OutboundPacketIntent>(originalIndices.Length);
    foreach (int index in originalIndices) {
      OutboundPacketIntent original = progress.Original.Intents[index];
      SessionEpoch target = progress.Original.Targets[index];
      var token = new OutboundDeliveryToken(
          CreativePowerEffectDeliveryBatch.DeliveryOwnerKey, batchId, target, intents.Count);
      intents.Add(new OutboundPacketIntent(
          new[] { target }, null, original.MessageId, original.Payload, original.CommitVersion,
          token));
    }

    var batch = new CreativePowerEffectDeliveryBatch(progress.Original.Effect, intents);
    _effectDeliveries.Add(batch, progress);
    progress.InFlightOriginalIndices = originalIndices;
    progress.InFlightBatch = batch;
    return batch;
  }

  private HashSet<SessionEpoch> GetActiveEffectTargets(EffectDeliveryProgress progress) {
    var activeTargets = new HashSet<SessionEpoch>();
    foreach ((SessionEpoch target, object lifetime) in progress.Lifetimes) {
      if (_effectRecipientLifetimes.TryGetValue(target, out object? current)
          && ReferenceEquals(current, lifetime)) {
        activeTargets.Add(target);
      }
    }
    return activeTargets;
  }

  private static bool[][] CreatePlayerToggleTables()
  {
    bool[][] tables = new bool[15][];
    for (int powerId = 0; powerId < tables.Length; powerId++)
    {
      tables[powerId] = new bool[PlayerCount];
    }

    return tables;
  }

  private static float[][] CreatePlayerSliderTables()
  {
    float[][] tables = new float[15][];
    for (int powerId = 0; powerId < tables.Length; powerId++)
    {
      tables[powerId] = new float[PlayerCount];
    }

    return tables;
  }

  private static CreativePowerPermissionLevel[] CreateDefaultPermissions()
  {
    var permissions = new CreativePowerPermissionLevel[15];
    Array.Fill(permissions, CreativePowerPermissionLevel.CanBeChangedByEveryone);
    return permissions;
  }

  private static bool IsAvailableForPlayer(
    CreativePowerPermissionLevel level,
    bool countsAsGameplayHost)
  {
    return level switch
    {
      CreativePowerPermissionLevel.LockedForEveryone => false,
      CreativePowerPermissionLevel.CanBeChangedByHostAlone => countsAsGameplayHost,
      CreativePowerPermissionLevel.CanBeChangedByEveryone => true,
      _ => false
    };
  }

  private static bool HasExpectedValueKind(ushort powerId, CreativePowerValueKind kind)
  {
    return powerId switch
    {
      0 or 9 or 10 or 13 => kind == CreativePowerValueKind.SharedToggle,
      1 or 2 or 3 or 4 => kind == CreativePowerValueKind.Button,
      5 or 11 => kind is CreativePowerValueKind.PerPlayerToggle
        or CreativePowerValueKind.PerPlayerToggleSync,
      6 or 7 or 8 or 12 => kind == CreativePowerValueKind.SharedSlider,
      14 => kind == CreativePowerValueKind.PerPlayerSlider,
      _ => false
    };
  }

  private bool MatchesCurrentPermissionState(
    IReadOnlyList<Packet82PermissionData> entries)
  {
    IReadOnlyList<Packet82PermissionData> current = CapturePermissionPacketData();
    for (int index = 0; index < current.Count; index++)
    {
      Packet82PermissionData expected = current[index];
      Packet82PermissionData actual = entries[index];
      if (actual.Action != expected.Action
          || actual.PowerId != expected.PowerId
          || actual.Level != expected.Level)
      {
        return false;
      }
    }

    return true;
  }

  private static CreativePermissionDeliveryResult EvaluatePermissionReceipts(
    CreativePermissionSnapshotBatch batch,
    IReadOnlyCollection<SendReceipt> receipts)
  {
    if (receipts.Count == 0)
    {
      return new CreativePermissionDeliveryResult(
        CreativePermissionDeliveryStatus.NotSubmitted,
        "No transport receipt was returned for the permission projection batch.");
    }

    if (receipts.Count != batch.Entries.Count)
    {
      return CreativePermissionDeliveryResult.Invalid(
        "The permission receipt count does not match the 15 projection entries.");
    }

    HashSet<(SessionEpoch Epoch, long Sequence)> sequences = new();
    bool locallySent = true;
    foreach (SendReceipt receipt in receipts)
    {
      if (receipt is null
          || receipt.Epoch != batch.Target
          || !receipt.Sequence.IsValid
          || !sequences.Add((receipt.Epoch, receipt.Sequence.Value)))
      {
        return CreativePermissionDeliveryResult.Invalid(
          "Every permission receipt must identify the target epoch with a unique sequence.");
      }

      switch (receipt.Certainty)
      {
        case TransportSendCertainty.Failed:
          return new CreativePermissionDeliveryResult(
            CreativePermissionDeliveryStatus.Failed,
            receipt.ErrorCode is ProtocolErrorCode error
              ? $"The permission intent failed with protocol error {error}."
              : "The permission intent failed before local delivery.");
        case TransportSendCertainty.OutcomeUnknown:
          return new CreativePermissionDeliveryResult(
            CreativePermissionDeliveryStatus.OutcomeUnknown,
            "The permission transport outcome is unknown; the projection remains dirty.");
        case TransportSendCertainty.NotSubmitted:
          return new CreativePermissionDeliveryResult(
            CreativePermissionDeliveryStatus.NotSubmitted,
            "The permission intent did not enter the local outbound stream.");
        case TransportSendCertainty.Queued:
          locallySent = false;
          break;
        case TransportSendCertainty.LocallySent:
          break;
        default:
          return CreativePermissionDeliveryResult.Invalid(
            "The transport returned an unknown permission certainty value.");
      }
    }

    return new CreativePermissionDeliveryResult(
      locallySent
        ? CreativePermissionDeliveryStatus.LocallySent
        : CreativePermissionDeliveryStatus.Queued,
      locallySent
        ? "Every permission intent was locally sent."
        : "Every permission intent was queued; local socket delivery is not yet proven.");
  }

  private static CreativePowerEffectDeliveryResult EvaluateEffectReceipts(
      CreativePowerEffectDeliveryBatch batch,
      IReadOnlyList<OutboundIntentDelivery> deliveries,
      IReadOnlySet<SessionEpoch> activeTargets,
      bool previouslyQueued,
      out List<int> acceptedIntentIndices) {
    acceptedIntentIndices = new List<int>();
    if (deliveries.Count != batch.Intents.Count) {
      return CreativePowerEffectDeliveryResult.Invalid(
          "The Creative effect settlement must contain one delivery record per intent.");
    }
    if (batch.Intents.Count == 0) {
      return CreativePowerEffectDeliveryResult.NoRecipients();
    }

    HashSet<(SessionEpoch Epoch, long Sequence)> sequences = new();
    bool locallySent = !previouslyQueued;
    CreativePowerEffectDeliveryResult? incomplete = null;
    for (int index = 0; index < deliveries.Count; index++) {
      OutboundIntentDelivery? delivery = deliveries[index];
      if (delivery is null || !ReferenceEquals(delivery.Intent, batch.Intents[index])) {
        return CreativePowerEffectDeliveryResult.Invalid(
            "The Creative effect settlement must preserve captured intent identity and order.");
      }
      if (delivery.Receipts.Count > 1) {
        return CreativePowerEffectDeliveryResult.Invalid(
            "A target-specific Creative effect intent cannot have multiple receipts.");
      }

      SessionEpoch target = batch.Targets[index];
      bool active = activeTargets.Contains(target);
      if (delivery.Receipts.Count == 0) {
        if (active) {
          incomplete ??= new CreativePowerEffectDeliveryResult(
              CreativePowerEffectDeliveryStatus.NotSubmitted,
              "An active Creative effect intent has no transport receipt.");
        }
        continue;
      }

      SendReceipt receipt = delivery.Receipts[0];
      if (receipt is null
          || receipt.Epoch != target
          || !receipt.Sequence.IsValid) {
        return CreativePowerEffectDeliveryResult.Invalid(
            "Every effect receipt must match its ordered target with a valid transport sequence.");
      }
      if (receipt.Certainty is (TransportSendCertainty.Queued or TransportSendCertainty.LocallySent)
          && !sequences.Add((receipt.Epoch, receipt.Sequence.Value))) {
        return CreativePowerEffectDeliveryResult.Invalid(
            "Accepted Creative effect receipts must use unique target sequences.");
      }

      switch (receipt.Certainty)
      {
        case TransportSendCertainty.Failed:
          if (active) {
            incomplete ??= new CreativePowerEffectDeliveryResult(
                CreativePowerEffectDeliveryStatus.Failed,
                receipt.ErrorCode is ProtocolErrorCode error
                  ? $"The creative-power effect failed with protocol error {error}."
                  : "The creative-power effect failed before local delivery.");
          }
          break;
        case TransportSendCertainty.OutcomeUnknown:
          if (active) {
            incomplete ??= new CreativePowerEffectDeliveryResult(
                CreativePowerEffectDeliveryStatus.OutcomeUnknown,
                "The Creative effect outcome is unknown; this intent remains pending.");
          }
          break;
        case TransportSendCertainty.NotSubmitted:
          if (active) {
            incomplete ??= new CreativePowerEffectDeliveryResult(
                CreativePowerEffectDeliveryStatus.NotSubmitted,
                "The Creative effect intent did not enter the local outbound stream.");
          }
          break;
        case TransportSendCertainty.Queued:
          if (active) {
            acceptedIntentIndices.Add(index);
            locallySent = false;
          }
          break;
        case TransportSendCertainty.LocallySent:
          if (active) {
            acceptedIntentIndices.Add(index);
          }
          break;
        default:
          return CreativePowerEffectDeliveryResult.Invalid(
            "The transport returned an unknown creative-power effect certainty value.");
      }
    }

    return incomplete ?? new CreativePowerEffectDeliveryResult(
      locallySent
        ? CreativePowerEffectDeliveryStatus.LocallySent
        : CreativePowerEffectDeliveryStatus.Queued,
      locallySent
        ? "Every active creative-power effect intent was locally sent."
        : "Active effect intents were queued or locally sent; full socket delivery is unproven.");
  }

  private static bool IsSharedToggle(ushort powerId) => powerId is 0 or 9 or 10 or 13;

  private static bool IsSharedSlider(ushort powerId) => powerId is 6 or 7 or 8 or 12;

  private static bool IsPerPlayerToggle(ushort powerId) => powerId is 5 or 11;

  private CreativePowerApplyResult ApplySharedToggle(
    byte playerId,
    CreativePowerWireMessage message)
  {
    CommitVersion nextVersion = _version.Next();
    _sharedToggles[message.PowerId] = message.ToggleValue;
    return CommitEffect(playerId, message, 0, nextVersion);
  }

  private CreativePowerApplyResult ApplyButton(byte playerId, CreativePowerWireMessage message)
  {
    return CommitEffect(playerId, message, 0, _version.Next());
  }

  private CreativePowerApplyResult ApplyPlayerToggleSync(
    byte playerId,
    CreativePowerWireMessage message)
  {
    bool[] states = message.CopyPlayerStates();
    if (states.Length != PlayerCount)
    {
      return Rejected(
        ProtocolErrorCode.InvalidLength,
        "Player toggle state must contain 255 entries.");
    }

    CommitVersion nextVersion = _version.Next();
    Array.Copy(states, _playerToggles[message.PowerId], PlayerCount);
    return CommitEffect(playerId, message, playerId, nextVersion);
  }

  private CreativePowerApplyResult ApplyPlayerToggle(
    byte playerId,
    CreativePowerWireMessage message)
  {
    var reboundMessage = CreativePowerWireMessage.PerPlayerToggle(
      message.PowerId,
      playerId,
      message.ToggleValue);
    CommitVersion nextVersion = _version.Next();
    _playerToggles[message.PowerId][playerId] = message.ToggleValue;
    return CommitEffect(playerId, reboundMessage, playerId, nextVersion);
  }

  private CreativePowerApplyResult ApplySharedSlider(
    byte playerId,
    CreativePowerWireMessage message)
  {
    CommitVersion nextVersion = _version.Next();
    _sharedSliders[message.PowerId] = message.SliderValue;
    return CommitEffect(playerId, message, 0, nextVersion);
  }

  private CreativePowerApplyResult ApplyPlayerSlider(
    byte playerId,
    CreativePowerWireMessage message)
  {
    var reboundMessage = CreativePowerWireMessage.PerPlayerSlider(
      message.PowerId,
      playerId,
      message.SliderValue);
    CommitVersion nextVersion = _version.Next();
    _playerSliders[message.PowerId][playerId] = message.SliderValue;
    return CommitEffect(playerId, reboundMessage, playerId, nextVersion);
  }

  private CreativePowerApplyResult CommitEffect(
    byte playerId,
    CreativePowerWireMessage message,
    byte affectedPlayerId,
    CommitVersion nextVersion)
  {
    _version = nextVersion;
    var effect = new CreativePowerEffect(
      message.PowerId,
      playerId,
      message,
      affectedPlayerId,
      _version);
    return new CreativePowerApplyResult(OwnerCommitResult.Applied(_version), effect);
  }

  private static CreativePowerApplyResult Rejected(ProtocolErrorCode code, string detail)
  {
    return new CreativePowerApplyResult(OwnerCommitResult.Rejected(code, detail), null);
  }
}

public sealed record CreativePermissionSnapshotEntry(
  ushort PowerId,
  CreativePowerPermissionLevel Level,
  CommitVersion Version)
{
  public Packet82PermissionData ToPacketData() => new(0, PowerId, (byte)Level);
}

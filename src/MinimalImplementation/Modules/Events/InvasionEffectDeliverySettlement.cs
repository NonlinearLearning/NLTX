using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Protocol;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Modules.Events;

/// <summary>
/// Retains one applied invasion result and settles tokenized, target-specific effect intents.
/// Capture, settlement and epoch retirement run on the server commit boundary.
/// </summary>
public sealed class InvasionEffectDeliverySettlement
    : IOutboundDeliverySettlement, IModuleEpochRetirable {
  /// <summary>A null intent records that the effect had no effective captured recipients.</summary>
  public sealed record IntentBinding(int EffectIndex, OutboundPacketIntent? Intent);

  private sealed class DeliveryBatch : IOutboundDeliverySettlement {
    private readonly InvasionEffectDeliverySettlement _owner;

    public DeliveryBatch(
        InvasionEffectDeliverySettlement owner,
        Guid batchId,
        IReadOnlyList<int> originalIntentIndices,
        IReadOnlyList<OutboundPacketIntent> intents) {
      _owner = owner;
      BatchId = batchId;
      OriginalIntentIndices = Array.AsReadOnly(originalIntentIndices.ToArray());
      Intents = Array.AsReadOnly(intents.ToArray());
    }

    public Guid BatchId { get; }

    public IReadOnlyList<int> OriginalIntentIndices { get; }

    public string OwnerKey => DeliveryOwnerKey;

    public IReadOnlyList<OutboundPacketIntent> Intents { get; }

    public OutboundDeliverySettlementResult Settle(
        IReadOnlyList<OutboundIntentDelivery> deliveries) {
      return _owner.SettleDelivery(this, deliveries);
    }
  }

  public const string DeliveryOwnerKey = "InvasionEffects";

  private readonly IReadOnlyList<IntentBinding> _bindings;
  private readonly IReadOnlyList<OutboundPacketIntent> _intents;
  private readonly DeliveryBatch _firstBatch;
  private readonly HashSet<int> _pendingIntentIndices;
  private DeliveryBatch? _inFlightBatch;

  public InvasionEffectDeliverySettlement(
    InvasionCommitResult result,
    IReadOnlyList<IntentBinding> bindings)
  {
    ArgumentNullException.ThrowIfNull(result);
    ArgumentNullException.ThrowIfNull(bindings);
    if (result.Status != WorldEventOutcomeStatus.Applied
        || result.Version is not CommitVersion version || !version.IsValid)
    {
      throw new ArgumentException(
        "Invasion delivery requires the original applied result and its valid version.",
        nameof(result));
    }

    int networkEffectCount = CountNetworkEffects(result.Effects);
    if (bindings.Count != networkEffectCount)
    {
      throw new ArgumentException(
        "Every network-producing invasion effect must have exactly one binding.",
        nameof(bindings));
    }

    List<IntentBinding> copiedBindings = new(networkEffectCount);
    List<OutboundPacketIntent> intents = new();
    Guid batchId = Guid.NewGuid();
    int bindingIndex = 0;
    for (int effectIndex = 0; effectIndex < result.Effects.Count; effectIndex++)
    {
      InvasionEffect effect = result.Effects[effectIndex]
        ?? throw new ArgumentException("An invasion effect cannot be null.", nameof(result));
      if (effect is InvasionEffect.Started)
      {
        continue;
      }

      IntentBinding binding = bindings[bindingIndex]
        ?? throw new ArgumentException("An invasion binding cannot be null.", nameof(bindings));
      if (binding.EffectIndex != effectIndex)
      {
        throw new ArgumentException(
          "Invasion effect bindings must cover network effects in their original result order.",
          nameof(bindings));
      }

      copiedBindings.Add(binding);
      bindingIndex++;
      if (binding.Intent is not OutboundPacketIntent intent)
      {
        continue;
      }

      if (!MatchesExpectedMessage(effect, intent))
      {
        throw new ArgumentException(
          "An invasion network intent does not match its bound effect.",
          nameof(bindings));
      }

      if (intent.DeliveryToken is not null)
      {
        throw new ArgumentException(
          "An invasion presentation intent cannot belong to another delivery batch.",
          nameof(bindings));
      }

      if (effect is not InvasionEffect.SynchronizeWorldData
          && intent.CommitVersion != version)
      {
        throw new ArgumentException(
          "An invasion presentation intent must use the applied result version.",
          nameof(bindings));
      }

      HashSet<SessionEpoch> targets = new();
      foreach (SessionEpoch target in intent.TargetEpochs)
      {
        if (!target.IsValid || !targets.Add(target))
        {
          throw new ArgumentException(
            "An invasion intent must have unique valid target epochs.",
            nameof(bindings));
        }
      }

      foreach (SessionEpoch target in intent.TargetEpochs.OrderBy(epoch => epoch.Value))
      {
        if (intent.IgnoreEpoch == target)
        {
          continue;
        }

        intents.Add(CreateTargetIntent(intent, target, batchId, intents.Count));
      }
    }

    if (bindingIndex != bindings.Count)
    {
      throw new ArgumentException(
        "An invasion binding cannot target a local-only effect or omit a network effect.",
        nameof(bindings));
    }

    Result = result;
    _bindings = Array.AsReadOnly(copiedBindings.ToArray());
    _intents = Array.AsReadOnly(intents.ToArray());
    int[] originalIntentIndices = Enumerable.Range(0, intents.Count).ToArray();
    _pendingIntentIndices = new HashSet<int>(originalIntentIndices);
    _firstBatch = new DeliveryBatch(this, batchId, originalIntentIndices, _intents);
    _inFlightBatch = _firstBatch;
  }

  public string OwnerKey => DeliveryOwnerKey;

  /// <summary>
  /// The exact first commit result stays attached while these intents are retried.
  /// </summary>
  public InvasionCommitResult Result { get; }

  public IReadOnlyList<IntentBinding> Bindings => _bindings;

  /// <summary>
  /// The immutable first attempt; submit this instance to the delivery pump once.
  /// </summary>
  public IReadOnlyList<OutboundPacketIntent> Intents => _intents;

  /// <summary>All targets were accepted by local transport or their epochs were retired.</summary>
  public bool IsSettled => _pendingIntentIndices.Count == 0;

  /// <summary>
  /// After the previous attempt reports its outcome, captures pending targets with fresh tokens.
  /// Submit each returned handle once. Unknown outcomes can still duplicate presentation on retry.
  /// </summary>
  public IOutboundDeliverySettlement? CapturePendingDelivery() {
    if (_inFlightBatch is not null || IsSettled) {
      return null;
    }

    Guid batchId = Guid.NewGuid();
    int[] originalIntentIndices = _pendingIntentIndices.OrderBy(index => index).ToArray();
    List<OutboundPacketIntent> intents = new(originalIntentIndices.Length);
    foreach (int originalIndex in originalIntentIndices) {
      OutboundPacketIntent original = _intents[originalIndex];
      intents.Add(CreateTargetIntent(original, original.TargetEpochs[0], batchId, intents.Count));
    }

    DeliveryBatch batch = new(this, batchId, originalIntentIndices, intents);
    _inFlightBatch = batch;
    return batch;
  }

  public void RetireEpoch(SessionEpoch epoch) {
    if (!epoch.IsValid) {
      throw new ArgumentException("A valid session epoch is required.", nameof(epoch));
    }

    _pendingIntentIndices.RemoveWhere(index => _intents[index].TargetEpochs[0] == epoch);
  }

  public OutboundDeliverySettlementResult Settle(
      IReadOnlyList<OutboundIntentDelivery> deliveries) {
    return SettleDelivery(_firstBatch, deliveries);
  }

  private OutboundDeliverySettlementResult SettleDelivery(
      DeliveryBatch batch,
      IReadOnlyList<OutboundIntentDelivery> deliveries) {
    ArgumentNullException.ThrowIfNull(deliveries);
    if (!ReferenceEquals(_inFlightBatch, batch)) {
      return OutboundDeliverySettlementResult.Invalid(
          OwnerKey,
          "The invasion delivery batch is not the current capture.");
    }

    // A terminal report releases this attempt. Pending targets change only after full validation.
    _inFlightBatch = null;
    if (deliveries.Count != batch.Intents.Count) {
      return OutboundDeliverySettlementResult.Invalid(
          OwnerKey,
          "The invasion settlement must retain one delivery record per captured intent.");
    }

    List<int> submittedIntentIndices = new();
    HashSet<(SessionEpoch Epoch, long Sequence)> sequences = new();
    for (int index = 0; index < batch.Intents.Count; index++) {
      OutboundIntentDelivery? delivery = deliveries[index];
      OutboundPacketIntent intent = batch.Intents[index];
      if (delivery is null || !ReferenceEquals(delivery.Intent, intent)) {
        return OutboundDeliverySettlementResult.Invalid(
            OwnerKey,
            "The invasion settlement must preserve captured intent identity and order.");
      }

      OutboundDeliveryToken? token = intent.DeliveryToken;
      if (token is null
          || token.OwnerKey != OwnerKey
          || token.BatchId != batch.BatchId
          || token.IntentIndex != index
          || intent.TargetEpochs.Count != 1
          || intent.IgnoreEpoch is not null
          || token.Target != intent.TargetEpochs[0]) {
        return OutboundDeliverySettlementResult.Invalid(
            OwnerKey,
            "Every invasion delivery intent must carry its ordered target token.");
      }

      if (delivery.Receipts.Count == 0) {
        continue;
      }

      if (delivery.Receipts.Count != 1) {
        return OutboundDeliverySettlementResult.Invalid(
            OwnerKey,
            "Each invasion target intent must have exactly one receipt.");
      }

      SendReceipt? receipt = delivery.Receipts[0];
      if (receipt is null || receipt.Epoch != token.Target || !receipt.Sequence.IsValid) {
        return OutboundDeliverySettlementResult.Invalid(
            OwnerKey,
            "Every invasion receipt must match one ordered target and carry a valid sequence.");
      }

      switch (receipt.Certainty) {
        case TransportSendCertainty.Queued:
        case TransportSendCertainty.LocallySent:
          if (!sequences.Add((receipt.Epoch, receipt.Sequence.Value))) {
            return OutboundDeliverySettlementResult.Invalid(
                OwnerKey,
                "Accepted invasion receipts must use unique target sequences.");
          }

          submittedIntentIndices.Add(batch.OriginalIntentIndices[index]);
          break;
        case TransportSendCertainty.Failed:
        case TransportSendCertainty.NotSubmitted:
        case TransportSendCertainty.OutcomeUnknown:
          break;
        default:
          return OutboundDeliverySettlementResult.Invalid(
              OwnerKey,
              "The transport returned an unknown invasion delivery certainty.");
      }
    }

    foreach (int originalIndex in submittedIntentIndices) {
      _pendingIntentIndices.Remove(originalIndex);
    }

    return new OutboundDeliverySettlementResult(
        OwnerKey,
        IsSettled,
        IsSettled
            ? "Every captured invasion target was queued or locally sent, or its epoch retired."
            : "Some invasion targets remain pending for a later capture.");
  }

  private static OutboundPacketIntent CreateTargetIntent(
      OutboundPacketIntent original,
      SessionEpoch target,
      Guid batchId,
      int intentIndex) {
    OutboundDeliveryToken token = new(DeliveryOwnerKey, batchId, target, intentIndex);
    return new OutboundPacketIntent(
        new[] { target },
        ignoreEpoch: null,
        original.MessageId,
        original.Payload,
        original.CommitVersion,
        token);
  }

  private static byte GetExpectedMessageId(InvasionEffect effect)
  {
    return effect switch
    {
      InvasionEffect.ClearEvent => 98,
      InvasionEffect.Warning => PacketFrameCodec.NetModulesMessageId,
      InvasionEffect.SynchronizeWorldData => 7,
      InvasionEffect.ReportProgress => 78,
      InvasionEffect.Started => throw new ArgumentException(
        "Started effects are applied locally and have no direct presentation packet.",
        nameof(effect)),
      _ => throw new ArgumentException("Unknown invasion effect.", nameof(effect))
    };
  }

  private static bool MatchesExpectedMessage(
    InvasionEffect effect,
    OutboundPacketIntent intent)
  {
    if (effect is InvasionEffect.Warning)
    {
      return intent.MessageId == GetExpectedMessageId(effect)
        && intent.Payload.Length >= sizeof(ushort)
        && BinaryPrimitives.ReadUInt16LittleEndian(intent.Payload.Span) ==
          (ushort)Packet82ModuleId.Text;
    }

    return intent.MessageId == GetExpectedMessageId(effect);
  }

  private static int CountNetworkEffects(IReadOnlyList<InvasionEffect> effects)
  {
    int count = 0;
    foreach (InvasionEffect effect in effects)
    {
      if (effect is not InvasionEffect.Started)
      {
        _ = GetExpectedMessageId(effect);
        count++;
      }
    }

    return count;
  }
}

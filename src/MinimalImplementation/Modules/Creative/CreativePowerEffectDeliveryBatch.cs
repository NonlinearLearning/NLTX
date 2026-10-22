using System;
using System.Collections.Generic;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Modules.Creative;

/// <summary>
/// Associates one committed effect with ordered single-target intents for its broadcasts.
/// </summary>
public sealed class CreativePowerEffectDeliveryBatch
{
  public const string DeliveryOwnerKey = "CreativePower";

  public CreativePowerEffectDeliveryBatch(
    CreativePowerEffect effect,
    IReadOnlyCollection<OutboundPacketIntent> intents)
  {
    ArgumentNullException.ThrowIfNull(effect);
    ArgumentNullException.ThrowIfNull(intents);
    if (!effect.CommitVersion.IsValid)
    {
      throw new ArgumentException(
        "A creative-power effect batch requires a valid owner version.",
        nameof(effect));
    }

    List<OutboundPacketIntent> copiedIntents = new(intents.Count);
    List<SessionEpoch> targets = new(intents.Count);
    Guid? batchId = null;
    int intentIndex = 0;
    foreach (OutboundPacketIntent intent in intents)
    {
      ArgumentNullException.ThrowIfNull(intent);
      if (intent.CommitVersion != effect.CommitVersion)
      {
        throw new ArgumentException(
          "Every creative-power effect intent must use the effect commit version.",
          nameof(intents));
      }

      if (intent.TargetEpochs.Count != 1)
      {
        throw new ArgumentException(
          "Creative-power effect delivery requires single-target intents.",
          nameof(intents));
      }

      SessionEpoch target = intent.TargetEpochs[0];
      if (!target.IsValid || intent.IgnoreEpoch == target)
      {
        throw new ArgumentException(
          "Every creative-power effect intent must deliver to its valid target epoch.",
          nameof(intents));
      }

      if (intent.DeliveryToken is not OutboundDeliveryToken deliveryToken)
      {
        throw new ArgumentException(
          "Every creative-power effect intent must carry an outbound delivery token.",
          nameof(intents));
      }

      if (deliveryToken.OwnerKey != DeliveryOwnerKey
          || deliveryToken.Target != target
          || deliveryToken.IntentIndex != intentIndex)
      {
        throw new ArgumentException(
          "Every creative-power effect intent must carry its ordered owner and target token.",
          nameof(intents));
      }

      if (batchId is null)
      {
        batchId = deliveryToken.BatchId;
      }
      else if (deliveryToken.BatchId != batchId.Value)
      {
        throw new ArgumentException(
          "Every creative-power effect intent must use the same delivery batch id.",
          nameof(intents));
      }

      copiedIntents.Add(intent);
      targets.Add(target);
      intentIndex++;
    }

    Effect = effect;
    Version = effect.CommitVersion;
    BatchId = batchId ?? Guid.Empty;
    Intents = Array.AsReadOnly(copiedIntents.ToArray());
    Targets = Array.AsReadOnly(targets.ToArray());
  }

  public CreativePowerEffect Effect { get; }

  public CommitVersion Version { get; }

  public Guid BatchId { get; }

  public IReadOnlyList<OutboundPacketIntent> Intents { get; }

  /// <summary>One entry per intent; a target repeats for a multi-message broadcast.</summary>
  public IReadOnlyList<SessionEpoch> Targets { get; }
}

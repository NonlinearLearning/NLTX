using System.IO;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Protocol;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace MinimalImplementation.Server.Modules.Protocol;

/// <summary>Builds bounded Packet 82 module payloads without sending them to a socket.</summary>
public sealed class Packet82ModuleIntentProducer
{
  private const int MaximumRecipientCount = byte.MaxValue;
  private const ushort Version4ModuleCount = 15;

  private readonly IReadOnlyDictionary<ushort, IPacket82ModuleWireCodec> _codecs;
  private readonly int _tagEffectNpcSlotCount;
  private readonly Func<short, bool> _tagEffectUsesProcTimes;

  public Packet82ModuleIntentProducer(
    IReadOnlyDictionary<ushort, IPacket82ModuleWireCodec> codecs,
    int tagEffectNpcSlotCount,
    Func<short, bool> tagEffectUsesProcTimes)
  {
    ArgumentNullException.ThrowIfNull(codecs);
    ArgumentNullException.ThrowIfNull(tagEffectUsesProcTimes);
    if (tagEffectNpcSlotCount is < 0 or > byte.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(tagEffectNpcSlotCount));
    }

    _codecs =
      new System.Collections.ObjectModel.ReadOnlyDictionary<ushort, IPacket82ModuleWireCodec>(
        new Dictionary<ushort, IPacket82ModuleWireCodec>(codecs));
    _tagEffectNpcSlotCount = tagEffectNpcSlotCount;
    _tagEffectUsesProcTimes = tagEffectUsesProcTimes;
  }

  public OutboundPacketIntent CreateIntent(
    ushort moduleId,
    object payload,
    IReadOnlyCollection<SessionEpoch> targets,
    CommitVersion commitVersion,
    OutboundDeliveryToken? deliveryToken = null)
  {
    return CreateIntent(moduleId, payload, targets, commitVersion, null, deliveryToken);
  }

  /// <summary>
  /// Creates a bounded Packet 82 intent with an optional source epoch excluded by the transport
  /// fan-out. The overload keeps the existing call shape for producers that already filter their
  /// targets explicitly.
  /// </summary>
  public OutboundPacketIntent CreateIntent(
    ushort moduleId,
    object payload,
    IReadOnlyCollection<SessionEpoch> targets,
    CommitVersion commitVersion,
    SessionEpoch? ignoreEpoch,
    OutboundDeliveryToken? deliveryToken = null)
  {
    ArgumentNullException.ThrowIfNull(payload);
    ArgumentNullException.ThrowIfNull(targets);
    if (moduleId >= Version4ModuleCount)
    {
      throw new ArgumentOutOfRangeException(
        nameof(moduleId),
        "Packet 82 module ids must remain within the Version4 range 0-14.");
    }

    if (!commitVersion.IsValid)
    {
      throw new ArgumentException(
        "A Packet 82 intent must reference a committed version.",
        nameof(commitVersion));
    }

    if (ignoreEpoch is SessionEpoch ignored && !ignored.IsValid)
    {
      throw new ArgumentException(
        "An ignored Packet 82 epoch must be valid.",
        nameof(ignoreEpoch));
    }

    SessionEpoch[] normalizedTargets = targets
      .Distinct()
      .OrderBy(epoch => epoch.Value)
      .ToArray();
    if (normalizedTargets.Length == 0)
    {
      throw new ArgumentException(
        "A Packet 82 intent must have at least one target.",
        nameof(targets));
    }

    if (normalizedTargets.Length > MaximumRecipientCount)
    {
      throw new ArgumentOutOfRangeException(
        nameof(targets),
        "A Packet 82 intent cannot exceed the Version4 player-slot limit.");
    }

    if (normalizedTargets.Any(static epoch => !epoch.IsValid))
    {
      throw new ArgumentException(
        "Packet 82 intent targets must carry valid session epochs.",
        nameof(targets));
    }

    if (ignoreEpoch is SessionEpoch excluded && normalizedTargets.Contains(excluded))
    {
      throw new ArgumentException(
        "A Packet 82 intent cannot both target and ignore the same session epoch.",
        nameof(ignoreEpoch));
    }

    using MemoryStream stream = new();
    PacketWireWriter writer = new(stream, PacketFrameCodec.MaximumPayloadBytes);
    NetModulesPacket.Write(
      writer,
      Version4SteamModuleIdMap.ToWireModuleId(moduleId),
      payload,
      _codecs,
      _tagEffectNpcSlotCount,
      _tagEffectUsesProcTimes);
    return new OutboundPacketIntent(
      normalizedTargets,
      ignoreEpoch,
      messageId: 82,
      stream.ToArray(),
      commitVersion,
      deliveryToken);
  }
}

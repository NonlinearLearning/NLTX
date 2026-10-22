using MinimalImplementation.Contracts;
using MinimalImplementation.Server.World;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Mechanisms;

/// <summary>Owns packet 59 behavior while leaving transport admission and socket routing outside P06.</summary>
public sealed class HitSwitchPacketHandler
{
  public const byte MessageId = 59;

  private readonly MechanismWorldPort _world;
  private readonly WiringPropagationService _wiring;
  private readonly MechanismPacketProducer _producer;
  private readonly MechanismCooldownService? _cooldowns;

  public HitSwitchPacketHandler(
    MechanismWorldPort world,
    WiringPropagationService wiring,
    MechanismPacketProducer producer,
    MechanismCooldownService? cooldowns = null)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(wiring);
    ArgumentNullException.ThrowIfNull(producer);
    _world = world;
    _wiring = wiring;
    _producer = producer;
    _cooldowns = cooldowns;
  }

  public HitSwitchDispatchResult Handle(
    InboundPacketEnvelope envelope,
    byte authenticatedPlayerSlot,
    HitSwitchPacket packet,
    IReadOnlyCollection<SessionEpoch> activeSessionEpochs)
  {
    ValidateEnvelope(envelope, authenticatedPlayerSlot, packet, activeSessionEpochs);
    SessionEpoch[] recipients = activeSessionEpochs.Distinct().ToArray();
    if (!recipients.Contains(envelope.Epoch))
    {
      return HitSwitchDispatchResult.SessionUnavailable;
    }

    WorldSnapshot before = _world.CaptureSnapshot();
    if (_cooldowns is not null &&
        (before.WorldEpoch != (ulong)_cooldowns.WorldScope ||
          envelope.WorldScope != _cooldowns.WorldScope))
    {
      return new HitSwitchDispatchResult(
        HitSwitchStatus.InvalidMechanism,
        "The switch and timer owner require the current world scope.",
        before.Version,
        Array.Empty<TileCoordinate>(),
        Array.Empty<OutboundPacketIntent>());
    }
    MechanismCooldownService.Preparation? cooldowns = _cooldowns?.Prepare();
    WiringPropagationPlan plan = _wiring.Plan(before, packet, cooldowns);
    if (!plan.Accepted)
    {
      return new HitSwitchDispatchResult(
        HitSwitchStatus.InvalidMechanism,
        plan.Rejection,
        before.Version,
        plan.VisitedTiles,
        Array.Empty<OutboundPacketIntent>());
    }

    var deltas = plan.Deltas
      .OrderBy(pair => pair.Key.Y)
      .ThenBy(pair => pair.Key.X)
      .ToArray();
    if (!_world.TryPrepareTileDeltas(
      before.WorldEpoch,
      before.Version,
      deltas,
      producesImmediateBroadcast: true,
      out WorldTileMutationPreparation? preparation,
      out _) || preparation is null ||
        preparation.Result.Version is not CommitVersion commitVersion)
    {
      return new HitSwitchDispatchResult(
        HitSwitchStatus.WorldCommitRejected,
        "The P03 world rejected the mechanism preparation.",
        null,
        plan.VisitedTiles,
        Array.Empty<OutboundPacketIntent>());
    }

    var intents = new List<OutboundPacketIntent>(2)
    {
      _producer.CreateHitSwitchIntent(
        packet,
        recipients,
        envelope.Epoch,
        commitVersion)
    };
    intents.AddRange(_producer.CreateWiringIntents(
      before,
      preparation,
      recipients,
      plan.TileSquareEffects));
    var result = new HitSwitchDispatchResult(
      HitSwitchStatus.Applied,
      null,
      commitVersion,
      plan.VisitedTiles,
      Array.AsReadOnly(intents.ToArray()));
    if (!preparation.Validate().IsValid || cooldowns is not null && !cooldowns.Validate())
    {
      return new HitSwitchDispatchResult(
        HitSwitchStatus.WorldCommitRejected,
        "The prepared world or cooldowns changed before mechanism publication.",
        null,
        plan.VisitedTiles,
        Array.Empty<OutboundPacketIntent>());
    }
    // ServerLoop must retain its exclusive owner window through this publication.
    preparation.Commit();
    cooldowns?.Commit();
    return result;
  }

  private static void ValidateEnvelope(
    InboundPacketEnvelope envelope,
    byte authenticatedPlayerSlot,
    HitSwitchPacket packet,
    IReadOnlyCollection<SessionEpoch> activeSessionEpochs)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    ArgumentNullException.ThrowIfNull(activeSessionEpochs);
    if (envelope.MessageId != MessageId)
    {
      throw new ArgumentException(
        "The envelope message id does not match packet 59.",
        nameof(envelope));
    }
    if (authenticatedPlayerSlot == byte.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(authenticatedPlayerSlot));
    }
    if (activeSessionEpochs.Count == 0)
    {
      throw new ArgumentException(
        "Packet 59 requires the current active session set.",
        nameof(activeSessionEpochs));
    }
  }
}

public enum HitSwitchStatus
{
  Applied,
  SessionUnavailable,
  InvalidMechanism,
  WorldCommitRejected
}

public sealed record HitSwitchDispatchResult(
  HitSwitchStatus Status,
  string? Rejection,
  CommitVersion? CommitVersion,
  IReadOnlyList<TileCoordinate> VisitedTiles,
  IReadOnlyList<OutboundPacketIntent> OutboundIntents)
{
  public static HitSwitchDispatchResult SessionUnavailable { get; } = new(
    HitSwitchStatus.SessionUnavailable,
    "The sender is not an active session.",
    null,
    Array.Empty<TileCoordinate>(),
    Array.Empty<OutboundPacketIntent>());
}

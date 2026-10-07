using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.Relationships;

namespace NSSLC.Infrastructure.Network;

public sealed class SocialPacketHandlers : IPacketHandler<InstrumentSoundPacket>,
    IPacketHandler<ItemUseSoundPacket>, IPacketHandler<RequestLucyPopupPacket>,
    IPacketHandler<EmojiPacket>, IPacketHandler<RequestQuestEffectPacket> {
  private const byte LucyMessageSourceCount = 7;
  private readonly SocialEmoteBubbleStateOwner _emoteBubbles;
  private readonly NetworkWorldOwner? _worldOwner;
  private readonly NetworkPlayerOwner? _players;
  private readonly SocialNpcEffectOwner? _npcEffects;

  public SocialEmoteBubbleStateOwner EmoteBubbles => _emoteBubbles;

  public SocialPacketHandlers() : this(new SocialEmoteBubbleStateOwner(), null, null, null) {
  }

  public SocialPacketHandlers(SocialEmoteBubbleStateOwner emoteBubbles)
      : this(emoteBubbles, null, null, null) {
  }

  public SocialPacketHandlers(NetworkWorldOwner worldOwner, NetworkPlayerOwner players,
      SocialNpcEffectOwner npcEffects)
      : this(new SocialEmoteBubbleStateOwner(), worldOwner, players, npcEffects) {
  }

  private SocialPacketHandlers(SocialEmoteBubbleStateOwner emoteBubbles,
      NetworkWorldOwner? worldOwner, NetworkPlayerOwner? players,
      SocialNpcEffectOwner? npcEffects) {
    _emoteBubbles = emoteBubbles ?? throw new ArgumentNullException(nameof(emoteBubbles));
    bool hasWorldOwner = worldOwner is not null;
    if (hasWorldOwner != (players is not null) || hasWorldOwner != (npcEffects is not null)) {
      throw new ArgumentException(
          "Packet-120 NPC effects require the shared world, player and NPC owners together.");
    }
    _worldOwner = worldOwner;
    _players = players;
    _npcEffects = npcEffects;
  }

  public ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      InstrumentSoundPacket packet, CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    if (context.Stage != NetworkSessionStage.Active) {
      return Rejected("SocialRequiresActiveSession");
    }
    if (!float.IsFinite(packet.Pitch)) {
      return Rejected("InvalidInstrumentPitch");
    }

    var relay = new InstrumentSoundPacket {
      Player = context.Actor.PlayerSlot,
      Pitch = packet.Pitch
    };
    return Relayed(relay);
  }

  public ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      ItemUseSoundPacket packet, CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    if (context.Stage != NetworkSessionStage.Active) {
      return Rejected("SocialRequiresActiveSession");
    }

    var relay = new ItemUseSoundPacket { Player = context.Actor.PlayerSlot };
    return Relayed(relay);
  }

  public ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      RequestLucyPopupPacket packet, CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    if (context.Stage != NetworkSessionStage.Active) {
      return Rejected("SocialRequiresActiveSession");
    }
    if (packet.MessageSource >= LucyMessageSourceCount
        || !float.IsFinite(packet.Velocity.X)
        || !float.IsFinite(packet.Velocity.Y)) {
      return Rejected("InvalidLucyPopup");
    }

    var relay = new RequestLucyPopupPacket {
      MessageSource = packet.MessageSource,
      Variation = packet.Variation,
      Velocity = packet.Velocity,
      PositionX = packet.PositionX,
      PositionY = packet.PositionY
    };
    return Relayed(relay);
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      EmojiPacket packet, CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    if (context.Stage != NetworkSessionStage.Active) {
      return new PacketHandlingResult(false, rejectionCode: "SocialRequiresActiveSession");
    }
    if (packet.Emote >= SocialEmoteBubbleStateOwner.VanillaEmoteCount) {
      return new PacketHandlingResult(true);
    }

    SocialNpcEffectResult? reaction = null;
    if (_npcEffects is not null) {
      NetworkPlayerOwner players = _players!;
      SocialNpcEffectOwner npcEffects = _npcEffects;
      reaction = await _worldOwner!.InvokeAsync(session => {
        if (!players.TryCaptureOnOwnerThread(session, context,
                out NetworkPlayerSnapshot player)) {
          return null;
        }

        var interaction = new SocialPlayerNpcInteractionSnapshot(
            player.PlayerSlot,
            player.Position,
            player.Width,
            player.Height,
            player.Active,
            player.Dead,
            player.ShouldNotDraw,
            player.Stealth,
            player.WorldRuntimeId);
        return npcEffects.ApplyPlayerEmoteOnOwnerThread(
            session,
            player.WorldRuntimeId,
            in interaction);
      }, cancellationToken).ConfigureAwait(false);
      if (reaction is null) {
        return new PacketHandlingResult(false, rejectionCode: "SocialPlayerUnavailable");
      }
      if (reaction.Status == SocialNpcEffectStatus.StaleWorldRuntime) {
        return new PacketHandlingResult(false, rejectionCode: "SocialWorldRuntimeMismatch");
      }
    }

    SocialEmoteBubbleSnapshot bubble = _emoteBubbles.CreatePlayerBubble(
        context.Actor.PlayerSlot, packet.Emote);
    var update = new SyncEmoteBubblePacket {
      BubbleId = bubble.BubbleId,
      AnchorKind = bubble.AnchorKind,
      AnchorId = bubble.AnchorId,
      Lifetime = bubble.Lifetime,
      Emote = bubble.Emote,
      Metadata = bubble.Metadata
    };
    var outbound = new List<OutboundDispatch> {
      new OutboundDispatch(update, PacketDispatchKind.Single, [context.Connection]),
      new OutboundDispatch(update, PacketDispatchKind.AllActiveExceptSender)
    };
    if (_npcEffects is not null && reaction is SocialNpcEffectResult npcReaction) {
      AddNpcSnapshotDispatches(context, npcReaction, outbound);
    }

    return new PacketHandlingResult(true, outbound);
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      RequestQuestEffectPacket packet, CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    if (context.Stage != NetworkSessionStage.Active) {
      return new PacketHandlingResult(false, rejectionCode: "SocialRequiresActiveSession");
    }
    if (_npcEffects is null || context.WorldRuntimeId is not EntityRuntimeId expectedWorldRuntimeId) {
      return new PacketHandlingResult(false, rejectionCode: "SocialNpcEffectsNotComposed");
    }

    SocialNpcEffectResult effect = await _npcEffects.RequestDryadAnimationAsync(
        expectedWorldRuntimeId,
        cancellationToken).ConfigureAwait(false);
    if (effect.Status == SocialNpcEffectStatus.StaleWorldRuntime) {
      return new PacketHandlingResult(false, rejectionCode: "SocialWorldRuntimeMismatch");
    }

    var outbound = new List<OutboundDispatch>();
    AddNpcSnapshotDispatches(context, effect, outbound);
    if (effect.SpawnedProjectile is SocialProjectileSpawnSnapshot projectile) {
      AddSelfEchoAndBroadcast(context, SocialPacketProducers.CreateProjectileSync(projectile), outbound);
    }

    return new PacketHandlingResult(true, outbound);
  }

  public bool TryRemoveEmoteBubble(int bubbleId, out SyncEmoteBubblePacket removalPacket) {
    if (!_emoteBubbles.RemoveBubble(bubbleId, out _)) {
      removalPacket = null!;
      return false;
    }

    removalPacket = SocialPacketProducers.CreateEmoteBubbleRemoval(bubbleId);
    return true;
  }

  private static ValueTask<PacketHandlingResult> Relayed(object packet) {
    var outbound = new OutboundDispatch(packet, PacketDispatchKind.AllActiveExceptSender);
    return ValueTask.FromResult(new PacketHandlingResult(true, [outbound]));
  }

  private static ValueTask<PacketHandlingResult> Rejected(string code) {
    return ValueTask.FromResult(new PacketHandlingResult(false, rejectionCode: code));
  }

  private static ValueTask<PacketHandlingResult> Accepted() {
    return ValueTask.FromResult(new PacketHandlingResult(true));
  }

  private static void AddNpcSnapshotDispatches(
      NetworkSessionContext context,
      SocialNpcEffectResult effect,
      List<OutboundDispatch> outbound) {
    foreach (SocialNpcEffectSnapshot snapshot in effect.UpdatedNpcs) {
      AddSelfEchoAndBroadcast(
          context,
          SocialPacketProducers.CreateSyncNpc(snapshot),
          outbound);
    }
  }

  private static void AddSelfEchoAndBroadcast(
      NetworkSessionContext context,
      object packet,
      List<OutboundDispatch> outbound) {
    outbound.Add(new OutboundDispatch(packet, PacketDispatchKind.Single, [context.Connection]));
    outbound.Add(new OutboundDispatch(packet, PacketDispatchKind.AllActiveExceptSender));
  }
}

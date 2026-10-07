using System.Linq;
using System.Net;
using System.Numerics;
using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.Relationships;

namespace NSSLC.NetworkVerification;

internal static class SocialPacketVerification {
  public static async Task RunAsync() {
    ProtocolProfile profile = GatewayVerification.CreateProfile();
    VerifyDirectionsAndOutboundLayouts(profile);
    VerifyAuthoritativeBubbleAnchorBounds();
    var authority = new RecordingAuthority();
    await using var gateway = new PacketGateway(profile, authority);
    // The progression owners below only advance gateway admission stages; this is a relay fixture,
    // not evidence of a loaded Terraria world or its NPC simulation.
    GatewayVerification.RegisterProgression(gateway);
    var socialPackets = new SocialPacketHandlers();
    SocialPacketRegistration.Register(gateway, socialPackets);

    var budget = new PacketByteBudget(2 * 1024 * 1024);
    await using var server = gateway.CreateServer(IPAddress.Loopback, 0, budget: budget);
    server.Start();
    int port = ((IPEndPoint)server.EndPoint).Port;
    var factory = new PacketConnectionFactory(profile, budget: budget);
    IPacketConnection sender = await factory.ConnectAsync("127.0.0.1", port,
        TimeSpan.FromSeconds(5));
    IPacketConnection observer = await factory.ConnectAsync("127.0.0.1", port,
        TimeSpan.FromSeconds(5));
    try {
      byte senderSlot = await ActivateAsync(sender, gateway);
      byte observerSlot = await ActivateAsync(observer, gateway);
      Verify.That(senderSlot != observerSlot,
          "Separate TCP clients must receive separate authenticated player slots.");

      await sender.WritePacketAsync(new InstrumentSoundPacket { Player = 201, Pitch = 0.375f });
      InstrumentSoundPacket instrument = await ReadAsync<InstrumentSoundPacket>(observer);
      Verify.That(instrument.Player == senderSlot && instrument.Pitch == 0.375f,
          "Packet 58 must replace a spoofed player byte with the sender slot and preserve pitch for the other player.");
      await ExpectNoPacketAsync(sender,
          "Packet 58 must not echo the instrument sound to its source client.");

      await sender.WritePacketAsync(new ItemUseSoundPacket { Player = 202 });
      ItemUseSoundPacket itemSound = await ReadAsync<ItemUseSoundPacket>(observer);
      Verify.That(itemSound.Player == senderSlot,
          "Packet 152 must replace a spoofed player byte with the sender slot for the other player.");
      await ExpectNoPacketAsync(sender,
          "Packet 152 must not echo the item-use sound to its source client.");

      var lucyRequest = new RequestLucyPopupPacket {
        MessageSource = 5,
        Variation = 213,
        Velocity = new PacketVector2(3.5f, -7.25f),
        PositionX = 3200,
        PositionY = -16
      };
      await sender.WritePacketAsync(lucyRequest);
      RequestLucyPopupPacket lucyPopup = await ReadAsync<RequestLucyPopupPacket>(observer);
      Verify.That(lucyPopup.MessageSource == lucyRequest.MessageSource
          && lucyPopup.Variation == lucyRequest.Variation
          && lucyPopup.Velocity == lucyRequest.Velocity
          && lucyPopup.PositionX == lucyRequest.PositionX
          && lucyPopup.PositionY == lucyRequest.PositionY,
          "Packet 141 must relay its source, variation, velocity and integer world coordinates unchanged.");
      await ExpectNoPacketAsync(sender,
          "Packet 141 must not echo the Lucy popup to its source client.");

      await sender.WritePacketAsync(new EmojiPacket { Player = 201, Emote = 55 });
      SyncEmoteBubblePacket senderBubble = await ReadAsync<SyncEmoteBubblePacket>(sender);
      SyncEmoteBubblePacket observerBubble = await ReadAsync<SyncEmoteBubblePacket>(observer);
      VerifyBubble(senderBubble, observerBubble, senderSlot, 55,
          "The first valid packet 120 request must create one source and observer bubble update.");
      Verify.That(socialPackets.EmoteBubbles.TryGetBubble(senderBubble.BubbleId, out var firstBubble)
          && firstBubble.Lifetime == SocialEmoteBubbleStateOwner.PlayerEmoteLifetimeTicks
          && socialPackets.EmoteBubbles.GetPlayerEmoteTime(senderSlot)
              == SocialEmoteBubbleStateOwner.PlayerEmoteLifetimeTicks,
          "A valid packet 120 request must commit a 360 tick bubble and player emote timer.");

      await sender.WritePacketAsync(new EmojiPacket { Player = 202, Emote = 56 });
      SyncEmoteBubblePacket nextSenderBubble = await ReadAsync<SyncEmoteBubblePacket>(sender);
      SyncEmoteBubblePacket nextObserverBubble = await ReadAsync<SyncEmoteBubblePacket>(observer);
      VerifyBubble(nextSenderBubble, nextObserverBubble, senderSlot, 56,
          "A subsequent player emote must relay exactly once to the source and observer.");
      Verify.That(nextSenderBubble.BubbleId != senderBubble.BubbleId
          && socialPackets.EmoteBubbles.TryGetBubble(senderBubble.BubbleId, out firstBubble)
          && firstBubble.Lifetime == SocialEmoteBubbleStateOwner.PreviousBubbleFadeTicks
          && socialPackets.EmoteBubbles.TryGetBubble(nextSenderBubble.BubbleId, out var nextBubble)
          && nextBubble.Lifetime == SocialEmoteBubbleStateOwner.PlayerEmoteLifetimeTicks,
          "A replacement player bubble must use a unique ID and shorten the previous bubble to six ticks.");

      int activeBubbles = socialPackets.EmoteBubbles.GetBubbles().Count;
      await sender.WritePacketAsync(new EmojiPacket { Player = 203, Emote = 151 });
      await ExpectNoPacketAsync(sender,
          "An out-of-range emote must be ignored without a packet 91 update.");
      await ExpectNoPacketAsync(observer,
          "An out-of-range emote must not affect other active players.");
      Verify.That(socialPackets.EmoteBubbles.GetBubbles().Count == activeBubbles,
          "An invalid emote ID must not create bubble state.");

      IReadOnlyList<int> expiredBubbles = [];
      for (int tick = 0; tick < SocialEmoteBubbleStateOwner.PreviousBubbleFadeTicks; tick++) {
        expiredBubbles = socialPackets.EmoteBubbles.AdvanceTick();
      }
      Verify.That(expiredBubbles.Contains(senderBubble.BubbleId)
          && !socialPackets.EmoteBubbles.TryGetBubble(senderBubble.BubbleId, out _)
          && socialPackets.EmoteBubbles.TryGetBubble(nextSenderBubble.BubbleId, out nextBubble)
          && nextBubble.Lifetime == SocialEmoteBubbleStateOwner.PlayerEmoteLifetimeTicks
              - SocialEmoteBubbleStateOwner.PreviousBubbleFadeTicks,
          "The bubble owner must remove the faded prior bubble at expiry and advance the active bubble lifetime.");
      await VerifyBubbleTickSchedulerAsync();
      await ExpectNoPacketAsync(observer,
          "Each Social request must produce exactly one relay for the observer.");
    } finally {
      await observer.DisposeAsync();
      await sender.DisposeAsync();
    }

    await Verify.EventuallyAsync(() => gateway.Sessions.Count == 0 && authority.Releases == 2,
        "Social TCP peers must release both authenticated bindings on disconnect.");
    Verify.That(budget.Used == 0 && server.LastError is null,
        "Social TCP verification must release network bytes without a listener error.");

    await VerifyNpcEffectsOverTcpAsync();
  }

  private static async Task VerifyNpcEffectsOverTcpAsync() {
    EntityRuntimeId worldRuntimeId = default;
    await using var worldOwner = SocialNpcEffectVerification.CreateWorldOwner(
        solidTile: null,
        runtimeId => worldRuntimeId = runtimeId);
    await worldOwner.Ready.WaitAsync(TimeSpan.FromSeconds(5));

    ProtocolProfile profile = ServerProtocolProfile.Create(
        SteamProtocolProfile.Create(GeneratedProfileVerification.CreateFacts()));
    var authority = new RecordingAuthority();
    await using var host = new NetworkGatewayHost(IPAddress.Loopback, 0, profile, authority,
        worldRuntimeIdProvider: () => worldRuntimeId);
    var players = new NetworkPlayerOwner(worldOwner, host.Gateway.IsCurrentSender);
    var effects = new SocialNpcEffectOwner(
        worldOwner,
        SocialNpcEffectVerification.CreateContentCatalog());
    var socialPackets = new SocialPacketHandlers(worldOwner, players, effects);
    RegisterSocialEffectsProgression(host.Gateway, players);
    SocialPacketRegistration.Register(host.Gateway, socialPackets);
    SocialPacketRegistration.RegisterSteamNpcEffects(host.Gateway, socialPackets);
    host.Start();

    IPacketConnection sender = await host.Connections.ConnectAsync(
        "127.0.0.1", ((IPEndPoint)host.EndPoint).Port, TimeSpan.FromSeconds(5));
    IPacketConnection? observer = null;
    try {
      IPacketConnection connectedObserver = await host.Connections.ConnectAsync(
          "127.0.0.1", ((IPEndPoint)host.EndPoint).Port, TimeSpan.FromSeconds(5));
      observer = connectedObserver;
      byte senderSlot = await ActivateWithPlayerOwnerAsync(sender, host);
      byte observerSlot = await ActivateWithPlayerOwnerAsync(connectedObserver, host);
      Verify.That(senderSlot != observerSlot,
          "NPC effect TCP clients must have distinct authenticated player slots.");

      await sender.WritePacketAsync(new EmojiPacket { Player = 201, Emote = 55 });
      SyncEmoteBubblePacket senderBubble = await ReadAsync<SyncEmoteBubblePacket>(sender);
      SyncEmoteBubblePacket observerBubble = await ReadAsync<SyncEmoteBubblePacket>(connectedObserver);
      VerifyBubble(senderBubble, observerBubble, senderSlot, 55,
          "Packet 120 must first return packet 91 to its source and observer.");

      SyncNPCPacket senderReaction = await ReadAsync<SyncNPCPacket>(sender);
      SyncNPCPacket observerReaction = await ReadAsync<SyncNPCPacket>(connectedObserver);
      VerifyNpcReaction(senderReaction, observerReaction, senderSlot,
          "Packet 120 must send the committed town-NPC packet 23 to both TCP clients.");

      await sender.WritePacketAsync(new RequestQuestEffectPacket());
      SyncNPCPacket senderDryad = await ReadAsync<SyncNPCPacket>(sender);
      SyncNPCPacket observerDryad = await ReadAsync<SyncNPCPacket>(connectedObserver);
      Verify.That(senderDryad.NpcSlot == 0 && senderDryad.NetId == 20
          && senderDryad.Ai[0] == 24.0f && senderDryad.Ai[1] == 480.0f
          && observerDryad.NpcSlot == senderDryad.NpcSlot
          && observerDryad.NetId == senderDryad.NetId
          && observerDryad.Ai.SequenceEqual(senderDryad.Ai),
          "Packet 144 must send the committed Dryad packet-23 update to both TCP clients.");

      SteamProjectileSyncPacket senderProjectile =
          await ReadAsync<SteamProjectileSyncPacket>(sender);
      SteamProjectileSyncPacket observerProjectile =
          await ReadAsync<SteamProjectileSyncPacket>(connectedObserver);
      Verify.That(senderProjectile == observerProjectile
          && senderProjectile.State.ProjectileType == 995
          && senderProjectile.State.OwnerSlot == byte.MaxValue
          && senderProjectile.State.ProjectileUuid == -1
          && senderProjectile.State.Position == new Vector2(192, 280),
          "Packet 144 must broadcast the actually committed Steam packet-27 Stardew projectile to both clients.");
      await ExpectNoPacketAsync(sender,
          "One Social NPC request must not enqueue duplicate packet 23 or 27 echoes.");
      await ExpectNoPacketAsync(connectedObserver,
          "One Social NPC request must not enqueue duplicate packet 23 or 27 broadcasts.");
    } finally {
      if (observer is not null) {
        await observer.DisposeAsync();
      }
      await sender.DisposeAsync();
    }

    await Verify.EventuallyAsync(() => host.Gateway.Sessions.Count == 0
        && authority.Releases == 2,
        "Social NPC effect TCP clients must release both authenticated bindings.");
    Verify.That(host.Budget.Used == 0 && host.LastTransportError is null,
        "Social NPC effect TCP verification must release network bytes without a listener error.");
  }

  private static void RegisterSocialEffectsProgression(
      PacketGateway gateway,
      NetworkPlayerOwner players) {
    gateway.Register(new PacketPolicy(6, NetworkSessionStage.AwaitPlayerData),
        new RecordingHandler<RequestWorldDataPacket>((context, _, cancellationToken) =>
          HandleWorldDataAsync(players, context, cancellationToken)));
    gateway.Register(new PacketPolicy(8, NetworkSessionStage.AwaitSectionRequest),
        new RecordingHandler<SpawnTileDataPacket>((_, _, _) => ValueTask.FromResult(
            new PacketHandlingResult(true, nextStage: NetworkSessionStage.Synchronizing))));
    gateway.Register(new PacketPolicy(12, NetworkSessionStage.Synchronizing),
        new RecordingHandler<PlayerSpawnPacket>((context, _, cancellationToken) =>
          HandlePlayerSpawnAsync(players, context, cancellationToken)));
  }

  private static async ValueTask<PacketHandlingResult> HandleWorldDataAsync(
      NetworkPlayerOwner players,
      NetworkSessionContext context,
      CancellationToken cancellationToken) {
    NetworkPlayerBindingResult result = await players.EnsurePlayerAsync(
        context, cancellationToken).ConfigureAwait(false);
    if (!result.Succeeded) {
      throw new InvalidOperationException(
          $"Packet 6 did not bind a network player: {result.Status}.");
    }

    return new PacketHandlingResult(true,
        nextStage: NetworkSessionStage.AwaitSectionRequest);
  }

  private static async ValueTask<PacketHandlingResult> HandlePlayerSpawnAsync(
      NetworkPlayerOwner players,
      NetworkSessionContext context,
      CancellationToken cancellationToken) {
    NetworkPlayerMutationResult active = await players.ApplyConnectionAsync(
        context, true, cancellationToken).ConfigureAwait(false);
    NetworkPlayerMovementResult movement = await players.CommitMovementAsync(
        context,
        new NetworkPlayerMovementInput(
            ApplyPosition: true,
            Position: new Vector2(180, 100),
            ApplyVelocity: false,
            Velocity: Vector2.Zero),
        cancellationToken).ConfigureAwait(false);
    if (!active.Succeeded || !movement.Succeeded) {
      throw new InvalidOperationException(
          "Packet 12 did not activate and place the verification player.");
    }

    return new PacketHandlingResult(true, nextStage: NetworkSessionStage.Active);
  }

  private static async Task<byte> ActivateWithPlayerOwnerAsync(
      IPacketConnection connection,
      NetworkGatewayHost host) {
    await connection.WritePacketAsync(new HelloPacket { Version = host.Profile.HelloVersion });
    PlayerInfoPacket admission = await ReadAsync<PlayerInfoPacket>(connection);
    await connection.WritePacketAsync(new RequestWorldDataPacket());
    await Verify.EventuallyAsync(() => FindSession(host.Gateway, admission.Player).Stage
        == NetworkSessionStage.AwaitSectionRequest,
        "The Social effect test did not bind its player during packet 6.");
    await connection.WritePacketAsync(new SpawnTileDataPacket());
    await Verify.EventuallyAsync(() => FindSession(host.Gateway, admission.Player).Stage
        == NetworkSessionStage.Synchronizing,
        "The Social effect test did not reach packet 12 synchronization.");
    await connection.WritePacketAsync(new PlayerSpawnPacket());
    await Verify.EventuallyAsync(() => FindSession(host.Gateway, admission.Player).Stage
        == NetworkSessionStage.Active,
        "The Social effect test did not activate its player during packet 12.");
    return admission.Player;
  }

  private static void VerifyNpcReaction(
      SyncNPCPacket source,
      SyncNPCPacket observer,
      byte playerSlot,
      string message) {
    Verify.That(source.NpcSlot == 0 && source.NetId == 20
        && source.Ai[0] == 19.0f && source.Ai[1] == 220.0f
        && source.Ai[2] == playerSlot
        && source.SpawnNeedsSyncing
        && observer.NpcSlot == source.NpcSlot && observer.NetId == source.NetId
        && observer.PositionX == source.PositionX && observer.PositionY == source.PositionY
        && observer.Ai.SequenceEqual(source.Ai),
        message);
  }

  private static void VerifyAuthoritativeBubbleAnchorBounds() {
    var owner = new SocialEmoteBubbleStateOwner();
    var valid = new SocialEmoteBubbleSnapshot(1,
        SocialEmoteBubbleStateOwner.PlayerAnchorKind, 254, 360, 360, 1, null);
    Verify.That(owner.UpsertAuthoritativeBubble(valid)
        && owner.GetPlayerEmoteTime(254) == SocialEmoteBubbleStateOwner.PlayerEmoteLifetimeTicks,
        "The highest valid player anchor slot 254 must be accepted and indexed safely.");

    foreach (ushort invalidSlot in new ushort[] { 255, 256, ushort.MaxValue }) {
      var invalid = new SocialEmoteBubbleSnapshot(2,
          SocialEmoteBubbleStateOwner.PlayerAnchorKind, invalidSlot, 360, 360, 1, null);
      Verify.That(!owner.UpsertAuthoritativeBubble(invalid),
          $"Player anchor slot {invalidSlot} must be rejected before indexing the 255-slot timer array.");
    }
  }

  private static async Task VerifyBubbleTickSchedulerAsync() {
    var owner = new SocialEmoteBubbleStateOwner();
    SocialEmoteBubbleSnapshot bubble = owner.CreatePlayerBubble(1, 55);
    using var stopping = new CancellationTokenSource();
    Task ticking = SocialEmoteBubbleTickScheduler.RunAsync(owner, TimeProvider.System,
        stopping.Token);
    try {
      await Verify.EventuallyAsync(() => owner.TryGetBubble(bubble.BubbleId,
          out SocialEmoteBubbleSnapshot current)
          && current.Lifetime < SocialEmoteBubbleStateOwner.PlayerEmoteLifetimeTicks,
          "The TimeProvider-backed 60 Hz scheduler did not advance bubble state.");
    } finally {
      stopping.Cancel();
    }

    await ticking.WaitAsync(TimeSpan.FromSeconds(1));
    Verify.That(owner.TryGetBubble(bubble.BubbleId, out SocialEmoteBubbleSnapshot stopped)
        && stopped.Lifetime < SocialEmoteBubbleStateOwner.PlayerEmoteLifetimeTicks,
        "The TimeProvider-backed 60 Hz scheduler must leave authoritative state advanced.");
    await Task.Delay(TimeSpan.FromMilliseconds(50));
    Verify.That(owner.TryGetBubble(bubble.BubbleId, out SocialEmoteBubbleSnapshot unchanged)
        && unchanged.Lifetime == stopped.Lifetime,
        "The bubble scheduler must stop advancing after cancellation has been awaited.");
  }

  private static async Task<byte> ActivateAsync(IPacketConnection connection,
      PacketGateway gateway) {
    await connection.WritePacketAsync(new HelloPacket { Version = "Terraria319" });
    PlayerInfoPacket admission = await ReadAsync<PlayerInfoPacket>(connection);
    await connection.WritePacketAsync(new RequestWorldDataPacket());
    await Verify.EventuallyAsync(() => FindSession(gateway, admission.Player).Stage
        == NetworkSessionStage.AwaitSectionRequest,
        "Packet 6 must advance the authenticated TCP session to section admission.");
    await connection.WritePacketAsync(new SpawnTileDataPacket());
    await Verify.EventuallyAsync(() => FindSession(gateway, admission.Player).Stage
        == NetworkSessionStage.Synchronizing,
        "Packet 8 must advance the authenticated TCP session to synchronization.");
    await connection.WritePacketAsync(new PlayerSpawnPacket());
    await Verify.EventuallyAsync(() => FindSession(gateway, admission.Player).Stage
        == NetworkSessionStage.Active,
        "Packet 12 must make the authenticated TCP session active.");
    return admission.Player;
  }

  private static NetworkSession FindSession(PacketGateway gateway, byte playerSlot) {
    return gateway.Sessions.Single(session => session.Binding?.PlayerSlot == playerSlot);
  }

  private static async Task<TPacket> ReadAsync<TPacket>(IPacketConnection connection)
      where TPacket : notnull {
    PacketMessage message = (await connection.ReadPacketAsync()
        .AsTask().WaitAsync(TimeSpan.FromSeconds(5)))!;
    return message.Get<TPacket>();
  }

  private static void VerifyBubble(SyncEmoteBubblePacket source,
      SyncEmoteBubblePacket observer, byte playerSlot, byte emote, string message) {
    Verify.That(source.BubbleId >= 0 && source.BubbleId == observer.BubbleId
        && source.AnchorKind == SocialEmoteBubbleStateOwner.PlayerAnchorKind
        && source.AnchorId == playerSlot && source.Lifetime == 360 && source.Emote == emote
        && observer.AnchorKind == source.AnchorKind && observer.AnchorId == source.AnchorId
        && observer.Lifetime == source.Lifetime && observer.Emote == source.Emote,
        message);
  }

  private static async Task ExpectNoPacketAsync(IPacketConnection connection, string message) {
    using var timeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
    try {
      _ = await connection.ReadPacketAsync(timeout.Token);
    } catch (OperationCanceledException) when (timeout.IsCancellationRequested) {
      return;
    }
    throw new InvalidOperationException(message);
  }

  private static void VerifyDirectionsAndOutboundLayouts(ProtocolProfile profile) {
    foreach (byte id in new byte[] { 91, 107, 132 }) {
      _ = profile.Find(PacketDirection.ServerToClient, id);
      Verify.Throws<PacketProtocolException>(() => profile.Find(PacketDirection.ClientToServer, id));
    }
    foreach (byte id in new byte[] { 120, 144 }) {
      _ = profile.Find(PacketDirection.ClientToServer, id);
      Verify.Throws<PacketProtocolException>(() => profile.Find(PacketDirection.ServerToClient, id));
    }
    foreach (byte id in new byte[] { 58, 141, 152 }) {
      _ = profile.Find(PacketDirection.ClientToServer, id);
      _ = profile.Find(PacketDirection.ServerToClient, id);
    }

    VerifySmartTextLayout(profile);
    VerifyLegacySoundLayout(profile);
    VerifyEmoteBubbleLayout(profile);
  }

  private static void VerifySmartTextLayout(ProtocolProfile profile) {
    PacketRgb color = new(19, 97, 231);
    NetworkText text = NetworkText.Formattable("{0} met {1}",
        NetworkText.Literal("旅行者"), NetworkText.Key("NPCName", NetworkText.Literal("Guide")));
    SmartTextMessagePacket expected = SocialPacketProducers.CreateSmartTextMessage(
        color, text, 320);
    PacketBinding binding = profile.Find(PacketDirection.ServerToClient,
        typeof(SmartTextMessagePacket));
    byte[] frame = binding.Encode(expected);
    var actual = (SmartTextMessagePacket)binding.Decode(frame.AsMemory(3));
    Verify.That(actual.Color == expected.Color
        && actual.WidthLimit == expected.WidthLimit
        && binding.Encode(actual).AsSpan(3).SequenceEqual(frame.AsSpan(3)),
        "Packet 107 must retain RGB bytes, nested UTF-8 NetworkText and the Int16 width limit.");
  }

  private static void VerifyLegacySoundLayout(ProtocolProfile profile) {
    PacketBinding binding = profile.Find(PacketDirection.ServerToClient,
        typeof(PlayLegacySoundPacket));
    for (int flags = 0; flags < 8; flags++) {
      int? style = (flags & 1) == 0 ? null : 5;
      float? volume = (flags & 2) == 0 ? null : 0.625f;
      float? pitch = (flags & 4) == 0 ? null : -0.375f;
      PlayLegacySoundPacket expected = SocialPacketProducers.CreateLegacySound(
          new PacketVector2(12.5f, -36.25f), 301, style, volume, pitch);
      byte[] frame = binding.Encode(expected);
      var actual = (PlayLegacySoundPacket)binding.Decode(frame.AsMemory(3));
      Verify.That(actual.Position == expected.Position && actual.SoundIndex == expected.SoundIndex
          && actual.Style == expected.Style && actual.Volume == expected.Volume
          && actual.PitchOffset == expected.PitchOffset,
          "Packet 132 must decode every optional style, volume and pitch flag combination.");
    }

    byte[] reservedFlags = new byte[11];
    reservedFlags[^1] = 8;
    Verify.Throws<PacketProtocolException>(() => binding.Decode(reservedFlags));
  }

  private static void VerifyEmoteBubbleLayout(ProtocolProfile profile) {
    PacketBinding binding = profile.Find(PacketDirection.ServerToClient,
        typeof(SyncEmoteBubblePacket));
    var normal = new SyncEmoteBubblePacket {
      BubbleId = 17,
      AnchorKind = 1,
      AnchorId = 3,
      Lifetime = 360,
      Emote = 42
    };
    var normalActual = (SyncEmoteBubblePacket)binding.Decode(binding.Encode(normal).AsMemory(3));
    Verify.That(normalActual.BubbleId == normal.BubbleId
        && normalActual.AnchorKind == normal.AnchorKind
        && normalActual.AnchorId == normal.AnchorId
        && normalActual.Lifetime == normal.Lifetime
        && normalActual.Emote == normal.Emote
        && normalActual.Metadata is null,
        "Packet 91 must decode normal player anchored bubble updates without metadata.");

    var signed = new SyncEmoteBubblePacket {
      BubbleId = 18,
      AnchorKind = 1,
      AnchorId = 3,
      Lifetime = 360,
      Emote = byte.MaxValue,
      Metadata = -12
    };
    var signedActual = (SyncEmoteBubblePacket)binding.Decode(binding.Encode(signed).AsMemory(3));
    Verify.That(signedActual.Metadata == signed.Metadata,
        "Packet 91 must interpret emote byte 255 as a signed emote with Int16 metadata.");

    var removed = new SyncEmoteBubblePacket { BubbleId = 18, AnchorKind = byte.MaxValue };
    var removedActual = (SyncEmoteBubblePacket)binding.Decode(binding.Encode(removed).AsMemory(3));
    Verify.That(removedActual.BubbleId == removed.BubbleId
        && removedActual.AnchorKind == byte.MaxValue && removedActual.AnchorId is null,
        "Packet 91 anchor kind 255 must decode as a bubble removal.");
    Verify.Throws<PacketEncodingException>(() => binding.Encode(new SyncEmoteBubblePacket {
      BubbleId = 19,
      AnchorKind = 1,
      AnchorId = 2,
      Lifetime = 360,
      Emote = byte.MaxValue
    }));
  }
}

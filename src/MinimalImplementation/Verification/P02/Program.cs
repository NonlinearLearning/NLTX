using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Player;
using MinimalImplementation.Server.Protocol;
using MinimalImplementation.Server.Session;
using Terraria.NetWork.Prototype.PacketDesignCompiler;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

if (!TryReadArguments(args, out string scenario, out string outputRoot))
{
  Console.Error.WriteLine(
    "Usage: --scenario SC011|SC012 --output-root <Build/diagnostics directory>");
  return 2;
}

Directory.CreateDirectory(outputRoot);
try
{
  string evidence = scenario switch
  {
    "SC011" => RunSc011(),
    "SC012" => RunSc012(),
    _ => throw new InvalidOperationException($"Unknown scenario '{scenario}'.")
  };
  string evidencePath = Path.Combine(outputRoot, $"{scenario}-result.md");
  File.WriteAllText(evidencePath, evidence);
  Console.WriteLine($"{scenario} PASS (P02 owner-level evidence)");
  Console.WriteLine($"Evidence: {evidencePath}");
  return 0;
}
catch (Exception exception)
{
  Console.Error.WriteLine($"{scenario} FAIL {exception.GetType().Name}: {exception.Message}");
  Console.Error.WriteLine(exception.ToString());
  return 1;
}

static string RunSc011()
{
  (Version4WireProfile profile, Fixture fixture) = CreateFixture(
    password: "P02Password",
    hostToken: "P02Host");
  SessionEpoch epoch = SessionEpoch.Create(1101);
  RequireApplied(fixture.Sessions.OpenConnection(epoch), "SC011 open connection");
  var stages = new List<SessionStage>();
  long sequence = 0;

  InboundPacketEnvelope helloEnvelope = CreateEnvelope(
    profile,
    epoch,
    sequence++,
    SessionStage.AwaitHello,
    new HelloPacket { Version = "Terraria319" });
  stages.Add(ReadStage(fixture.Sessions, epoch));
  SessionAdmissionPreparation hello = fixture.SessionHandlers.HandleHello(
    helloEnvelope,
    new HelloPacket { Version = "Terraria319" },
    isBanned: false);
  Require(hello.IsReady && hello.ResponseMessageId == SessionAdmissionCoordinator.RequestPasswordMessageId,
    "SC011 Hello must request the configured password.");
  SessionTransitionCommitResult helloCommit = fixture.SessionHandlers.CommitAdmission(hello, profile);
  RequireApplied(helloCommit.Result, "SC011 password request commit");
  Require(helloCommit.Response?.MessageId == SessionAdmissionCoordinator.RequestPasswordMessageId,
    "SC011 must emit packet 37 after Hello.");
  stages.Add(ReadStage(fixture.Sessions, epoch));

  InboundPacketEnvelope passwordEnvelope = CreateEnvelope(
    profile,
    epoch,
    sequence++,
    SessionStage.AwaitPassword,
    new SendPasswordPacket { Password = "P02Password" });
  SessionAdmissionPreparation password = fixture.SessionHandlers.HandlePassword(
    passwordEnvelope,
    new SendPasswordPacket { Password = "P02Password" });
  Require(password.IsReady && password.ResponseMessageId == SessionAdmissionCoordinator.PlayerInfoMessageId,
    "SC011 correct password must produce PlayerInfo.");
  SessionTransitionCommitResult passwordCommit = fixture.SessionHandlers.CommitAdmission(password, profile);
  RequireApplied(passwordCommit.Result, "SC011 player admission commit");
  Require(passwordCommit.Response?.MessageId == SessionAdmissionCoordinator.PlayerInfoMessageId,
    "SC011 must emit packet 3 after the password.");
  stages.Add(ReadStage(fixture.Sessions, epoch));

  Require(
    fixture.Sessions.TryGetBinding(epoch, out SessionBinding? binding) && binding is not null,
    "SC011 admission must create one slot binding.");
  byte playerSlot = binding!.PlayerSlot;
  Require(
    fixture.Players.TryGetBoundPlayer(epoch, fixture.Sessions, out Player? player) && player is not null,
    "SC011 admission must create one Player object.");
  EntityHandle handle = player!.Handle;

  InboundPacketEnvelope loadoutEnvelope = CreateEnvelope(
    profile,
    epoch,
    sequence++,
    SessionStage.AwaitPlayerData,
    new SyncLoadoutPacket
    {
      Player = byte.MaxValue,
      LoadoutIndex = 1,
      AccessoryVisibilityMask = 0x0123
    });
  PlayerLoadoutCommit loadout = fixture.PlayerHandlers.HandleLoadout(
    loadoutEnvelope,
    new SyncLoadoutPacket
    {
      Player = byte.MaxValue,
      LoadoutIndex = 1,
      AccessoryVisibilityMask = 0x0123
    },
    static (_, _, _) => OwnerCommitResult.Applied(CommitVersion.Initial));
  RequireApplied(loadout.Result, "SC011 early loadout transaction projection");
  Require(loadout.LoadoutIndex == 1 && loadout.AccessoryVisibilityMask == 0x0123,
    "SC011 must preserve the P04 loadout result in the P02 player projection.");

  InboundPacketEnvelope hostEnvelope = CreateEnvelope(
    profile,
    epoch,
    sequence++,
    SessionStage.AwaitPlayerData,
    new HostTokenPacket { HostToken = "P02Host" });
  RequireApplied(
    fixture.SessionHandlers.HandleHostToken(
      hostEnvelope,
      new HostTokenPacket { HostToken = "P02Host" }),
    "SC011 HostToken authorization");

  InboundPacketEnvelope appearanceEnvelope = CreateEnvelope(
    profile,
    epoch,
    sequence++,
    SessionStage.AwaitPlayerData,
    CreateAppearancePacket(byte.MaxValue, "P02-Join"));
  PlayerPacketMutationResult appearance = fixture.PlayerHandlers.HandleAppearance(
    appearanceEnvelope,
    CreateAppearancePacket(byte.MaxValue, "P02-Join"),
    skinVariantCount: 8,
    maximumNameLength: 20,
    isJourneyWorld: false,
    checkDuplicateName: true);
  RequireApplied(appearance.Result, "SC011 appearance upload");
  stages.Add(ReadStage(fixture.Sessions, epoch));
  Require(fixture.Players.TryGet(handle, out player) && player is not null,
    "SC011 appearance upload must keep the original Player handle.");

  var buffsPacket = new PlayerBuffsPacket
  {
    Player = byte.MaxValue,
    BuffTypes = [Version4PlayerBuffCatalog.StinkyBuffType]
  };
  PlayerPacketMutationResult buffs = fixture.PlayerHandlers.HandleBuffs(
    CreateEnvelope(
      profile,
      epoch,
      sequence++,
      SessionStage.AwaitPlayerData,
      buffsPacket),
    buffsPacket,
    Version4PlayerBuffCatalog.IsValidBuff);
  RequireApplied(buffs.Result, "SC011 player-buffs upload");
  Require(
    buffs.ResponseMessageId is null
      && buffs.ResponsePacket is null
      && buffs.TargetEpochs.Count == 0
      && buffs.IgnoreEpoch is null,
    "SC011 early player-buffs upload must commit without an Active-session projection.");
  Require(
    fixture.Players.TryGet(handle, out player)
      && player is not null
      && player.Buffs.Types[0] == Version4PlayerBuffCatalog.StinkyBuffType
      && player.Buffs.Times[0] == PlayerBuffs.ClientReportedDurationTicks,
    "SC011 player-buffs upload must commit the Version4 duration through PlayerOwner.");

  InboundPacketEnvelope worldRequestEnvelope = CreateEnvelope(
    profile,
    epoch,
    sequence++,
    SessionStage.AwaitPlayerData,
    new RequestWorldDataPacket());
  JoinStepResult worldRequest = fixture.Join.HandleWorldDataRequest(
    worldRequestEnvelope,
    CreateWorldDataPacket());
  RequireApplied(worldRequest.Result, "SC011 world-data request");
  Require(worldRequest.Projections.Count == 1
    && worldRequest.Projections[0].MessageId == SessionAdmissionCoordinator.WorldDataMessageId,
    "SC011 must produce one packet 7 projection.");
  stages.Add(ReadStage(fixture.Sessions, epoch));

  InboundPacketEnvelope sectionEnvelope = CreateEnvelope(
    profile,
    epoch,
    sequence++,
    SessionStage.AwaitSectionRequest,
    new SpawnTileDataPacket { X = 100, Y = 200, Team = 0 });
  JoinStepResult synchronization = fixture.Join.HandleSpawnTileData(
    sectionEnvelope,
    new SpawnTileDataPacket { X = 100, Y = 200, Team = 0 },
    (currentBinding, _) =>
    {
      if (!fixture.Players.TryCaptureJoinSnapshot(
            currentBinding,
            out PlayerJoinSnapshot? snapshot)
          || snapshot is null)
      {
        throw new InvalidOperationException("SC011 must capture a PlayerJoinSnapshot.");
      }

      IReadOnlyList<PlayerPacketProjection> playerPackets = PlayerPacketProjection.From(snapshot);
      var packets = playerPackets
        .Select(projection => new JoinBatchPacket(projection.MessageId, projection.Packet))
        .ToArray();
      return new JoinSynchronizationBatch(
        worldGeneration: 1,
        new StatusTextSizePacket
        {
          Value = packets.Length,
          Text = NetworkText.Literal("SC011 synchronization"),
          Flags = 0
        },
        packets);
    });
  RequireApplied(synchronization.Result, "SC011 section synchronization");
  JoinSynchronizationBatch synchronizationBatch = synchronization.Batch
    ?? throw new InvalidOperationException("SC011 must retain the captured synchronization batch.");
  Require(synchronizationBatch.WorldGeneration == 1,
    "SC011 must retain the captured world generation.");
  Require(synchronization.Projections.Count == synchronizationBatch.Packets.Count + 2,
    "SC011 projection count must include progress and packet 49.");
  Require(synchronization.Projections[0].MessageId == SessionAdmissionCoordinator.StatusTextSizeMessageId,
    "SC011 progress must precede the synchronization batch.");
  Require(synchronization.Projections[^1].MessageId == SessionAdmissionCoordinator.InitialSpawnMessageId,
    "SC011 packet 49 must be the final join-batch projection.");
  _ = fixture.SessionHandlers.CreateIntents(synchronization.Projections, profile);
  stages.Add(ReadStage(fixture.Sessions, epoch));

  InboundPacketEnvelope spawnEnvelope = CreateEnvelope(
    profile,
    epoch,
    sequence++,
    SessionStage.Synchronizing,
    new PlayerSpawnPacket
    {
      Player = byte.MaxValue,
      SpawnX = 100,
      SpawnY = 200,
      RespawnTimer = 0,
      PveDeaths = 0,
      PvpDeaths = 0,
      Team = 0,
      SpawnContext = 0
    });
  PlayerLifecycleResult spawn = fixture.PlayerHandlers.HandleSpawn(
    spawnEnvelope,
    new PlayerSpawnPacket
    {
      Player = byte.MaxValue,
      SpawnX = 100,
      SpawnY = 200,
      RespawnTimer = 0,
      PveDeaths = 0,
      PvpDeaths = 0,
      Team = 0,
      SpawnContext = 0
    },
    sendGameplayHostProjection: true);
  RequireApplied(spawn.Result, "SC011 initial spawn");
  Require(fixture.Sessions.TryGetStage(epoch, out SessionStage finalStage)
    && finalStage == SessionStage.Active, "SC011 must finish at Active.");
  Require(spawn.Projections.Any(projection =>
      projection.MessageId == SessionAdmissionCoordinator.FinishedConnectingMessageId),
    "SC011 must emit packet 129 only after packet 12 commits.");
  Player admittedPlayer = player
    ?? throw new InvalidOperationException("SC011 Player disappeared before the final assertion.");
  Require(admittedPlayer.IsHost,
    "SC011 HostToken must set player.host on the same Player instance.");
  Require(admittedPlayer.LoadoutIndex == 1 && admittedPlayer.AccessoryVisibilityMask == 0x0123,
    "SC011 loadout visibility must survive appearance, join, and spawn.");
  stages.Add(finalStage);

  return $"""
# SC011 result

Status: PASS (P02 owner-level evidence; complete server integration pending)

Input: production SessionAdmissionCoordinator/SessionPacketHandlers received one loopback-shaped
Version4 packet sequence: Hello `Terraria319`, correct password `P02Password`, early packet 147,
correct HostToken packet 161, packet 4, packet 6, packet 8, and packet 12. The packet ids and
bodies were encoded through the production Version4WireProfile before envelopes were created.

Actual: one non-zero EntityUuid/EntityGeneration `{handle.Generation.Value}` remained bound to slot
`{playerSlot}` from admission through appearance, loadout projection, synchronization, and Active.
The staged transitions were `{string.Join(" -> ", stages)}`. Packet 37, packet 3, packet 7, the
progress projection, the provider-produced batch, packet 49, and packet 129 were produced in order.
The early loadout projection retained index 1 and visibility mask `0x0123`; packet 161 set
`Player.IsHost` independently of the gameplay-host 139 projection.

Boundary: this is owner/handler evidence, not a claim that the root TCP listener, P03 world producer,
P04 inventory transaction, or final Host registration is complete. The verifier supplied an explicit
P04 transaction callback and a finite world/join snapshot provider. SC013-SC025 were not run.
""";
}

static string RunSc012()
{
  (Version4WireProfile profile, Fixture fixture) = CreateFixture(
    password: null,
    hostToken: null);
  (SessionEpoch firstEpoch, byte firstSlot) = CreateActivePlayer(
    profile,
    fixture,
    SessionEpoch.Create(1201),
    "P02-A",
    new PacketVector2(100f, 200f));
  (SessionEpoch secondEpoch, byte secondSlot) = CreateActivePlayer(
    profile,
    fixture,
    SessionEpoch.Create(1202),
    "P02-B",
    new PacketVector2(300f, 400f));

  Require(firstSlot != secondSlot, "SC012 players must have different server slots.");
  Require(fixture.Players.TryGet(firstEpoch, out Player? first) && first is not null,
    "SC012 first player is missing.");
  Require(fixture.Players.TryGet(secondEpoch, out Player? second) && second is not null,
    "SC012 second player is missing.");
  PacketVector2 secondPositionBefore = second!.Movement.Position;
  PacketVector2 submittedPosition = new(101f, 200f);
  var buffsPacket = new PlayerBuffsPacket
  {
    Player = byte.MaxValue,
    BuffTypes = [Version4PlayerBuffCatalog.StinkyBuffType]
  };
  PlayerPacketMutationResult buffs = fixture.PlayerHandlers.HandleBuffs(
    CreateEnvelope(
      profile,
      firstEpoch,
      sequence: 5,
      SessionStage.Active,
      buffsPacket),
    buffsPacket,
    Version4PlayerBuffCatalog.IsValidBuff);
  RequireApplied(buffs.Result, "SC012 active player-buffs upload");
  Require(
    buffs.ResponseMessageId == SessionAdmissionCoordinator.PlayerBuffsMessageId
      && buffs.ResponsePacket is PlayerBuffsPacket buffProjection
      && buffProjection.Player == firstSlot
      && buffProjection.BuffTypes.Count == 1
      && buffProjection.BuffTypes[0] == Version4PlayerBuffCatalog.StinkyBuffType,
    "SC012 active player-buffs upload must produce the authenticated packet-50 projection.");
  Require(
    buffs.TargetEpochs.Count == 1
      && buffs.TargetEpochs[0] == secondEpoch
      && buffs.IgnoreEpoch == firstEpoch,
    "SC012 packet-50 projection must target Player B and exclude Player A.");
  Require(
    fixture.Players.TryGet(firstEpoch, out first)
      && first is not null
      && first.Buffs.Types[0] == Version4PlayerBuffCatalog.StinkyBuffType
      && first.Buffs.Times[0] == PlayerBuffs.ClientReportedDurationTicks,
    "SC012 player-buffs upload must commit the Version4 duration through PlayerOwner.");

  InboundPacketEnvelope controlsEnvelope = CreateEnvelope(
    profile,
    firstEpoch,
    sequence: 6,
    SessionStage.Active,
    new PlayerControlsPacket
    {
      Player = secondSlot,
      ControlFlags = 0x40,
      MovementFlags = 0,
      PlayerFeatureFlags = 0,
      ActionFlags = 0,
      SelectedItem = 0,
      Position = submittedPosition,
      Velocity = null,
      MountType = null,
      PotionOfReturnUsePosition = null,
      PotionOfReturnHomePosition = null,
      CameraTarget = null
    });
  PlayerPacketMutationResult mutation = fixture.PlayerHandlers.HandleControls(
    controlsEnvelope,
    new PlayerControlsPacket
    {
      Player = secondSlot,
      ControlFlags = 0x40,
      MovementFlags = 0,
      PlayerFeatureFlags = 0,
      ActionFlags = 0,
      SelectedItem = 0,
      Position = submittedPosition
    },
    preserveServerPosition: false,
    isSelectedItemValid: static selectedItem => selectedItem == 0);
  RequireApplied(mutation.Result, "SC012 controls submission");
  if (mutation.ResponsePacket is not PlayerControlsPacket controlsProjection)
  {
    throw new InvalidOperationException(
      "SC012 must produce a typed player-controls projection.");
  }
  Require(controlsProjection.Player == firstSlot,
    "SC012 must normalize the packet player field to the authenticated sender slot.");
  Require(controlsProjection.Position == submittedPosition,
    "SC012 must broadcast the sender's committed position.");
  Require(mutation.TargetEpochs.Count == 1 && mutation.TargetEpochs[0] == secondEpoch,
    "SC012 must target the other active player only.");
  Require(mutation.IgnoreEpoch == firstEpoch,
    "SC012 must retain the sender exclusion in the outbound projection.");
  OutboundPacketIntent outbound = fixture.SessionHandlers.CreateIntent(mutation, profile)
    ?? throw new InvalidOperationException("SC012 must create a P01 outbound intent.");
  Require(outbound.MessageId == SessionAdmissionCoordinator.PlayerControlsMessageId,
    "SC012 outbound intent must remain packet 13.");
  Require(outbound.TargetEpochs.Count == 1 && outbound.TargetEpochs[0] == secondEpoch
    && !outbound.TargetEpochs.Contains(firstEpoch),
    "SC012 P01 intent must exclude the sender and target only Player B.");
  Require(
    profile.Decode(
      WireDirection.ServerToClient,
      outbound.MessageId,
      outbound.Payload) is PlayerControlsPacket decoded
      && decoded.Player == firstSlot
      && decoded.Position == submittedPosition,
    "SC012 P01 intent must decode to the committed sender packet.");
  Require(fixture.Players.TryGet(firstEpoch, out first) && first is not null
    && first.Movement.Position == submittedPosition,
    "SC012 sender position was not committed to Player A.");
  Require(fixture.Players.TryGet(secondEpoch, out second) && second is not null
    && second.Movement.Position == secondPositionBefore,
    "SC012 claimed Player B must not be mutated by Player A's packet.");

  return $"""
# SC012 result

Status: PASS (P02 owner-level evidence; complete server integration pending)

Input: two players reached Active through the production P02 admission/join/lifecycle path. Player A
slot `{firstSlot}` sent one packet 13 whose claimed player field was Player B slot `{secondSlot}`,
with control flag `0x40`, selected item 0, and position `{submittedPosition.X},{submittedPosition.Y}`.

Actual: the authenticated session epoch selected Player A, committed A's position, rewrote the typed
outbound packet player field to `{firstSlot}`, encoded a production P01 `OutboundPacketIntent`, and
decoded it back as packet 13 with exactly one target epoch `{secondEpoch.Value}`.
The sender epoch `{firstEpoch.Value}` was retained as the explicit exclusion. Player B's position
remained `{secondPositionBefore.X},{secondPositionBefore.Y}`.

Boundary: this proves the P02 sender/owner normalization and target policy using real owner objects and
the production Version4 codec for envelope input. It does not claim root TCP dispatch, client receipt,
or final Host registration. SC013-SC025 were not run.
""";
}

static (Version4WireProfile Profile, Fixture Fixture) CreateFixture(
  string? password,
  string? hostToken)
{
  Version4ProtocolFactsInput facts = CreateFacts();
  if (ProtocolInputs.IsInitialized)
  {
    throw new InvalidOperationException("P02 facts were initialized more than once in one verifier process.");
  }

  _ = Version4ProtocolFactsFactory.Create(facts);
  var profile = new Version4WireProfile(facts);
  var identities = new EntityIdentityRegistry();
  var players = new PlayerOwner(identities);
  var bindings = new SessionBindingStore();
  var policy = new SessionAdmissionPolicy("Terraria319", 8, password, hostToken);
  var sessions = new SessionAdmissionCoordinator(bindings, players, policy);
  var hostAuthorization = new SessionHostAuthorization(policy, players, sessions);
  var join = new JoinCoordinator(sessions, players);
  var sessionHandlers = new SessionPacketHandlers(sessions, hostAuthorization, join);
  var playerHandlers = new PlayerStatePacketHandlers(players, sessions);
  return (profile, new Fixture(
    sessions,
    players,
    join,
    sessionHandlers,
    playerHandlers));
}

static (SessionEpoch Epoch, byte Slot) CreateActivePlayer(
  Version4WireProfile profile,
  Fixture fixture,
  SessionEpoch epoch,
  string name,
  PacketVector2 position)
{
  RequireApplied(fixture.Sessions.OpenConnection(epoch), $"SC012 open {name}");
  long sequence = 0;
  SessionAdmissionPreparation preparation = fixture.SessionHandlers.HandleHello(
    CreateEnvelope(
      profile,
      epoch,
      sequence++,
      SessionStage.AwaitHello,
      new HelloPacket { Version = "Terraria319" }),
    new HelloPacket { Version = "Terraria319" },
    isBanned: false);
  Require(preparation.ResponseMessageId == SessionAdmissionCoordinator.PlayerInfoMessageId,
    "A no-password Hello must directly return packet 3.");
  RequireApplied(
    fixture.SessionHandlers.CommitAdmission(preparation, profile).Result,
    $"SC012 admission {name}");

  Require(
    fixture.Sessions.TryGetBinding(epoch, out SessionBinding? binding) && binding is not null,
    $"SC012 binding {name}");
  byte slot = binding!.PlayerSlot;
  PlayerPacketMutationResult appearance = fixture.PlayerHandlers.HandleAppearance(
    CreateEnvelope(
      profile,
      epoch,
      sequence++,
      SessionStage.AwaitPlayerData,
      CreateAppearancePacket(byte.MaxValue, name)),
    CreateAppearancePacket(byte.MaxValue, name),
    skinVariantCount: 8,
    maximumNameLength: 20,
    isJourneyWorld: false,
    checkDuplicateName: true);
  RequireApplied(appearance.Result, $"SC012 appearance {name}");

  RequireApplied(
    fixture.Join.HandleWorldDataRequest(
      CreateEnvelope(
        profile,
        epoch,
        sequence++,
        SessionStage.AwaitPlayerData,
        new RequestWorldDataPacket()),
      CreateWorldDataPacket()).Result,
    $"SC012 world data {name}");
  JoinStepResult synchronization = fixture.Join.HandleSpawnTileData(
    CreateEnvelope(
      profile,
      epoch,
      sequence++,
      SessionStage.AwaitSectionRequest,
      new SpawnTileDataPacket { X = (int)position.X, Y = (int)position.Y, Team = 0 }),
    new SpawnTileDataPacket { X = (int)position.X, Y = (int)position.Y, Team = 0 },
    (currentBinding, _) =>
    {
      if (!fixture.Players.TryCaptureJoinSnapshot(
            currentBinding,
            out PlayerJoinSnapshot? snapshot)
          || snapshot is null)
      {
        throw new InvalidOperationException($"SC012 snapshot {name}");
      }

      IReadOnlyList<PlayerPacketProjection> packets = PlayerPacketProjection.From(snapshot);
      return new JoinSynchronizationBatch(
        worldGeneration: 1,
        new StatusTextSizePacket
        {
          Value = packets.Count,
          Text = NetworkText.Literal("SC012 synchronization"),
          Flags = 0
        },
        packets.Select(packet => new JoinBatchPacket(packet.MessageId, packet.Packet)).ToArray());
    });
  RequireApplied(synchronization.Result, $"SC012 synchronization {name}");

  PlayerLifecycleResult spawn = fixture.PlayerHandlers.HandleSpawn(
    CreateEnvelope(
      profile,
      epoch,
      sequence++,
      SessionStage.Synchronizing,
      new PlayerSpawnPacket
      {
        Player = byte.MaxValue,
        SpawnX = (short)position.X,
        SpawnY = (short)position.Y,
        RespawnTimer = 0,
        PveDeaths = 0,
        PvpDeaths = 0,
        Team = 0,
        SpawnContext = 0
      }),
    new PlayerSpawnPacket
    {
      Player = byte.MaxValue,
      SpawnX = (short)position.X,
      SpawnY = (short)position.Y,
      RespawnTimer = 0,
      PveDeaths = 0,
      PvpDeaths = 0,
      Team = 0,
      SpawnContext = 0
    },
    sendGameplayHostProjection: false);
  RequireApplied(spawn.Result, $"SC012 spawn {name}");
  Require(fixture.Sessions.TryGetStage(epoch, out SessionStage stage)
    && stage == SessionStage.Active, $"SC012 active stage {name}");
  return (epoch, slot);
}

static InboundPacketEnvelope CreateEnvelope(
  Version4WireProfile profile,
  SessionEpoch epoch,
  long sequence,
  SessionStage stage,
  object packet)
{
  Version4EncodedPacket encoded = profile.Encode(WireDirection.ClientToServer, packet);
  byte[] rawFrame = PacketFrameCodec.Encode(encoded.MessageId, encoded.Payload.Span);
  return new InboundPacketEnvelope(
    epoch,
    new PacketSequence(sequence),
    DateTimeOffset.UnixEpoch.AddSeconds(sequence),
    encoded.MessageId,
    moduleId: null,
    rawFrame,
    stage.ToString(),
    worldScope: null,
    entityGeneration: null,
    expectedRevision: null);
}

static SyncPlayerPacket CreateAppearancePacket(byte player, string name)
{
  return new SyncPlayerPacket
  {
    Player = player,
    SkinVariant = 0,
    VoiceVariant = 1,
    VoicePitchOffset = 0,
    Hair = 1,
    Name = name,
    HairDye = 0,
    HiddenAccessories = 0,
    HideMisc = 0,
    HairColor = new PacketRgb(1, 2, 3),
    SkinColor = new PacketRgb(4, 5, 6),
    EyeColor = new PacketRgb(7, 8, 9),
    ShirtColor = new PacketRgb(10, 11, 12),
    UnderShirtColor = new PacketRgb(13, 14, 15),
    PantsColor = new PacketRgb(16, 17, 18),
    ShoeColor = new PacketRgb(19, 20, 21),
    DifficultyAndAccessoryFlags = 0,
    BiomeAndCartFlags = 0,
    PermanentUpgradeFlags = 0
  };
}

static WorldDataPacket CreateWorldDataPacket()
{
  return new WorldDataPacket
  {
    WorldName = "P02 fixture",
    WorldGuid = new byte[16],
    MaxTilesX = 840,
    MaxTilesY = 240,
    SpawnTileX = 100,
    SpawnTileY = 200,
    WorldId = 2,
    GameMode = 0
  };
}

static SessionStage ReadStage(SessionAdmissionCoordinator sessions, SessionEpoch epoch)
{
  Require(sessions.TryGetStage(epoch, out SessionStage stage), "The session stage must be readable.");
  return stage;
}

static void RequireApplied(OwnerCommitResult result, string operation)
{
  Require(
    result.Status == OwnerCommitStatus.Applied && result.Version is CommitVersion version && version.IsValid,
    $"{operation} was not applied: {result.ErrorCode} {result.Detail}");
}

static Version4ProtocolFactsInput CreateFacts()
{
  const string facts = "8E5F2D0A3AA1E5A5E1AFC8E8D9CC7C0D84CB4B2E5BCEDB1B7AE9D49FE2BA88B1";
  return new Version4ProtocolFactsInput(
    new[] { true },
    new[] { false },
    PacketTileEntityCodecsV4.Create(),
    new[] { true },
    static (_, _, _) => 1,
    static _ => false,
    40,
    0,
    static _ => false,
    facts);
}

static bool TryReadArguments(
  IReadOnlyList<string> arguments,
  out string scenario,
  out string outputRoot)
{
  scenario = string.Empty;
  outputRoot = string.Empty;
  if (arguments.Count != 4
    || arguments[0] != "--scenario"
    || arguments[2] != "--output-root"
    || string.IsNullOrWhiteSpace(arguments[1])
    || string.IsNullOrWhiteSpace(arguments[3]))
  {
    return false;
  }

  scenario = arguments[1];
  outputRoot = Path.GetFullPath(arguments[3]);
  return scenario is "SC011" or "SC012";
}

static void Require(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

sealed class Fixture
{
  public Fixture(
    SessionAdmissionCoordinator sessions,
    PlayerOwner players,
    JoinCoordinator join,
    SessionPacketHandlers sessionHandlers,
    PlayerStatePacketHandlers playerHandlers)
  {
    Sessions = sessions;
    Players = players;
    Join = join;
    SessionHandlers = sessionHandlers;
    PlayerHandlers = playerHandlers;
  }

  public SessionAdmissionCoordinator Sessions { get; }

  public PlayerOwner Players { get; }

  public JoinCoordinator Join { get; }

  public SessionPacketHandlers SessionHandlers { get; }

  public PlayerStatePacketHandlers PlayerHandlers { get; }
}

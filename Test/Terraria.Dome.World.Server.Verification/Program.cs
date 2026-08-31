using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Protocol.V1456.Protocol;
using Terraria.Dome.Server.Replication;
using Terraria.Dome.Server.Startup;
using Terraria.Dome.Server;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldObjects.Sign;

VerifyBootstrapRequestRejectsInvalidInputs();
VerifyBootstrapRequestCarriesValidatedEntityLimits();
VerifySimulationEntityLimitsRejectMutationAfterCapacity();
VerifyBootstrapRequestIsIdempotent();
VerifyBootstrapRestoreDoesNotAdvanceBeforeFirstTick();

WorldGrid world = new(width: 400, height: 300);
WorldSectionCoordinates first = new(0, 0);
WorldSectionCoordinates second = new(1, 0);
SessionSectionVisibility visibility = new();

WorldSectionSnapshot firstSnapshot = world.CreateSectionSnapshot(first);
WorldSectionSnapshot secondSnapshot = world.CreateSectionSnapshot(second);
IReadOnlyList<WorldSectionSnapshot> initial = visibility.CollectChangedSections(
  [firstSnapshot, secondSnapshot]);
if (initial.Count != 2 || initial[0].Coordinates != first || initial[1].Coordinates != second)
{
  throw new InvalidOperationException("A new session did not receive each subscribed section.");
}

if (visibility.CollectChangedSections([firstSnapshot, secondSnapshot]).Count != 0)
{
  throw new InvalidOperationException("A session received an unchanged section twice.");
}

_ = world.TrySetTile(10, 10, new WorldTile(IsActive: true, Type: 1));
WorldSectionSnapshot changedFirstSnapshot = world.CreateSectionSnapshot(first);
IReadOnlyList<WorldSectionSnapshot> dirty = visibility.CollectChangedSections(
  [changedFirstSnapshot, secondSnapshot]);
if (dirty.Count != 1 || dirty[0].Coordinates != first || dirty[0].Version != 1)
{
  throw new InvalidOperationException("A session did not receive only the dirty section version.");
}

visibility.Clear();
if (visibility.CollectChangedSections([changedFirstSnapshot, secondSnapshot]).Count != 2)
{
  throw new InvalidOperationException("Clearing a disconnected session did not release section versions.");
}

Console.WriteLine("PASS: server-owned section visibility versions");

WorldGrid streamedWorld = new(width: 4200, height: 1200);
_ = streamedWorld.TrySetTile(2100, 300, new WorldTile(IsActive: true, Type: 1));
WorldSectionReplication replication = new(streamedWorld);
SessionSectionVisibility initialVisibility = new();
IReadOnlyList<byte[]> initialStream = replication.CreateInitialWorldStream(
  new SpawnTileDataRequestPacket(2100, 300, 0),
  initialVisibility);
if (initialStream.Count != 17 ||
    TerrariaFrameCodec.Decode(initialStream[0]).MessageId != TerrariaMessageId.StatusTextSize ||
    TerrariaFrameCodec.Decode(initialStream[16]).MessageId != TerrariaMessageId.InitialSpawn)
{
  throw new InvalidOperationException("Server world replication did not produce the initial V1456 stream.");
}

TerrariaFrame centerSectionFrame = TerrariaFrameCodec.Decode(initialStream[8]);
using MemoryStream compressedStream = new(centerSectionFrame.Payload.ToArray(), writable: false);
using DeflateStream decompressor = new(compressedStream, CompressionMode.Decompress);
using BinaryReader reader = new(decompressor);
int originX = reader.ReadInt32();
int originY = reader.ReadInt32();
if (originX != 2000 || originY != 300)
{
  throw new InvalidOperationException("Server world replication selected the wrong spawn-center section.");
}

Console.WriteLine("PASS: server world section selection and initial replication");

using (DomeServer signServer = new(new WorldGrid(400, 300)))
{
  int signId = signServer.CreateSign(12, 10, "Server-owned");
  long revision = signServer.CreateSignSnapshots()[0].Revision;
  if (!signServer.TryDeleteSign(signId, revision) ||
      signServer.CreateSignTombstoneProjections().Count != 1 ||
      signServer.CreateSignTombstoneProjections()[0].Status !=
        Terraria.Dome.Protocol.V1456.Packets.SignTombstoneProjectionStatus.Deferred ||
      signServer.TryDeleteSign(signId, revision))
  {
    throw new InvalidOperationException(
      "Server Sign deletion did not retain one explicit deferred tombstone result.");
  }

  if (signServer.CreateSignDeletionFrames().Count != 0)
  {
    throw new InvalidOperationException(
      "Server emitted a Sign deletion frame without negotiated capabilities.");
  }

  IReadOnlyList<byte[]> signDeletionFrames = signServer.CreateSignDeletionFrames(
    new SessionContractCapabilities(
      ContractNegotiationState.Negotiated,
      SignDeletionVersions: 1,
      ChestTransferRevisionVersions: 0));
  SignDeletionFrame decodedSignDeletion = ContractExtensionCodec.DecodeSignDeletion(signDeletionFrames[0]);
  if (signDeletionFrames.Count != 1 ||
      decodedSignDeletion !=
        new SignDeletionFrame(signId, revision + 1, SignTombstoneReason.Deleted))
  {
    throw new InvalidOperationException(
      $"Server did not emit the typed Sign deletion frame: {decodedSignDeletion}.");
  }

  SessionContractCapabilities capabilities = new(
    ContractNegotiationState.Negotiated,
    SignDeletionVersions: 1,
    ChestTransferRevisionVersions: 0);
  WorldSectionSnapshot signSection = new WorldGrid(400, 300).CreateSectionSnapshot(
    new WorldSectionCoordinates(0, 0));
  using SessionReplicationState firstSession = new((_, _) => Task.CompletedTask);
  firstSession.SetContractCapabilities(capabilities);
  if (signServer.CreateSignDeletionFrames(firstSession).Count != 0)
  {
    throw new InvalidOperationException("Sign deletion escaped the session PVS boundary.");
  }

  _ = firstSession.ReplaceVisibleSections([signSection]);
  if (signServer.CreateSignDeletionFrames(firstSession).Count != 1 ||
      signServer.CreateSignDeletionFrames(firstSession).Count != 0)
  {
    throw new InvalidOperationException(
      "Sign deletion cursor did not enforce one send per visible session.");
  }

  using SessionReplicationState reconnectedSession = new((_, _) => Task.CompletedTask);
  reconnectedSession.SetContractCapabilities(capabilities);
  _ = reconnectedSession.ReplaceVisibleSections([signSection]);
  if (signServer.CreateSignDeletionFrames(reconnectedSession).Count != 1)
  {
    throw new InvalidOperationException(
      "A reconnecting session did not receive the retained Sign tombstone.");
  }
}

Console.WriteLine("PASS: server exposes non-silent Sign tombstone projection state");
Console.WriteLine("PASS: server emits versioned Sign deletion frames");

static void VerifyBootstrapRequestRejectsInvalidInputs()
{
  ExpectArgument(() => WorldBootstrapRequest.Create(
    "invalid-dimensions",
    new WorldSeed(1),
    width: 401,
    height: 300,
    spawnX: 200,
    spawnY: 75,
    new WorldRuleState()));
  ExpectArgument(() => WorldBootstrapRequest.Create(
    "invalid-spawn",
    new WorldSeed(1),
    width: 400,
    height: 300,
    spawnX: 400,
    spawnY: 75,
    new WorldRuleState()));
  ExpectArgument(() => WorldBootstrapRequest.Create(
    "invalid-difficulty",
    new WorldSeed(1),
    width: 400,
    height: 300,
    spawnX: 200,
    spawnY: 75,
    new WorldRuleState(difficulty: 4)));
  Console.WriteLine("PASS: bootstrap request rejects malformed dimensions, spawn and rules");
}

static void VerifyBootstrapRequestIsIdempotent()
{
  WorldBootstrapRequest request = WorldBootstrapRequest.Create(
    "idempotent",
    new WorldSeed(17),
    width: 400,
    height: 300,
    spawnX: 200,
    spawnY: 75,
    new WorldRuleState());
  WorldBootstrapResult first = WorldBootstrap.Create(request);
  WorldBootstrapResult second = WorldBootstrap.Create(request);
  Assert(
    first.Snapshot.World.Metadata == second.Snapshot.World.Metadata &&
    first.Snapshot.TickNumber == 0 && second.Snapshot.TickNumber == 0 &&
    first.InitialSnapshotRevision == second.InitialSnapshotRevision &&
    first.FirstScheduledTick == second.FirstScheduledTick,
    "Equivalent bootstrap requests did not produce an idempotent initial result.");
  WorldBootstrapRequest differentSeedRequest = WorldBootstrapRequest.Create(
    "idempotent",
    new WorldSeed(18),
    width: 400,
    height: 300,
    spawnX: 200,
    spawnY: 75,
    new WorldRuleState());
  WorldBootstrapResult differentSeed = WorldBootstrap.Create(differentSeedRequest);
  Assert(
    first.Snapshot.WorldEventRandomState != differentSeed.Snapshot.WorldEventRandomState,
    "Different bootstrap seeds shared the same event random state.");
  Console.WriteLine("PASS: equivalent bootstrap requests are idempotent at tick zero");
}

static void VerifyBootstrapRequestCarriesValidatedEntityLimits()
{
  WorldEntityLimits limits = new(
    MaximumPlayers: 2,
    MaximumNpcs: 3,
    MaximumProjectiles: 4,
    MaximumWorldItems: 5,
    MaximumChests: 6);
  WorldBootstrapRequest request = WorldBootstrapRequest.Create(
    "limits",
    new WorldSeed(91),
    400,
    300,
    200,
    75,
    new WorldRuleState(),
    entityLimits: limits);

  WorldBootstrapResult result = WorldBootstrap.Create(request);
  if (result.EntityLimits != limits)
  {
    throw new InvalidOperationException(
      "Bootstrap did not preserve the validated entity limits.");
  }

  using DomeSimulation source = new(
    new WorldGrid(400, 300),
    new WorldSeed(91),
    new SimulationEntityLimits(MaximumChests: 2));
  _ = source.CreateChest(30, 30);
  _ = source.CreateChest(31, 30);
  DomeSimulationSnapshot chestSnapshot = source.CreatePersistenceSnapshot(
    request.Metadata,
    request.Rules);
  WorldBootstrapRequest constrainedRestore = WorldBootstrapRequest.Create(
    "limits",
    new WorldSeed(91),
    400,
    300,
    200,
    75,
    new WorldRuleState(),
    entityLimits: new WorldEntityLimits(MaximumChests: 1));
  ExpectInvalidOperation(
    () => WorldBootstrap.Restore(constrainedRestore, chestSnapshot),
    "persistence chest capacity");

  ExpectArgument(
    () => WorldBootstrapRequest.Create(
      "too-many-players",
      new WorldSeed(92),
      400,
      300,
      200,
      75,
      new WorldRuleState(),
      entityLimits: new WorldEntityLimits(MaximumPlayers: byte.MaxValue + 1)));
}

static void VerifySimulationEntityLimitsRejectMutationAfterCapacity()
{
  SimulationEntityLimits limits = new(
    MaximumPlayers: 2,
    MaximumNpcs: 1,
    MaximumProjectiles: 1,
    MaximumWorldItems: 1,
    MaximumChests: 1);
  using DomeSimulation simulation = new(new WorldGrid(400, 300), new WorldSeed(1), limits);
  _ = simulation.CreateNpc(new SimulationVector(20.0f, 20.0f));
  ExpectInvalidOperation(
    () => simulation.CreateNpc(new SimulationVector(21.0f, 20.0f)),
    "NPC capacity");
  int worldItemId = simulation.SpawnWorldItem(
    new ItemStack(1, 1),
    new SimulationVector(10.0f, 10.0f));
  simulation.QueueDestroyWorldItem(worldItemId, expectedRevision: 1);
  simulation.Tick(new SimulationInputBatch());
  _ = simulation.SpawnWorldItem(new ItemStack(1, 1), new SimulationVector(11.0f, 10.0f));
  ExpectInvalidOperation(
    () => simulation.SpawnWorldItem(new ItemStack(1, 1), new SimulationVector(12.0f, 10.0f)),
    "world-item capacity");
  _ = simulation.CreateChest(30, 30);
  ExpectInvalidOperation(
    () => simulation.CreateChest(31, 30),
    "chest capacity");
  Console.WriteLine("PASS: Simulation entity limits reject NPC, world-item and chest overflow");
}

static void VerifyBootstrapRestoreDoesNotAdvanceBeforeFirstTick()
{
  WorldBootstrapRequest request = WorldBootstrapRequest.Create(
    "restore",
    new WorldSeed(19),
    width: 400,
    height: 300,
    spawnX: 200,
    spawnY: 75,
    new WorldRuleState());
  WorldBootstrapResult initial = WorldBootstrap.Create(request);
  WorldBootstrapResult restored = WorldBootstrap.Restore(request, initial.Snapshot);
  Assert(
    restored.Snapshot.TickNumber == initial.Snapshot.TickNumber &&
    restored.FirstScheduledTick == initial.Snapshot.TickNumber + 1,
    "Bootstrap restore advanced the clock before the first accepted tick.");
  WorldBootstrapRequest mismatchedRequest = WorldBootstrapRequest.Create(
    "restore",
    new WorldSeed(20),
    width: 400,
    height: 300,
    spawnX: 200,
    spawnY: 75,
    new WorldRuleState());
  ExpectArgument(() => WorldBootstrap.Restore(mismatchedRequest, initial.Snapshot));
  Console.WriteLine("PASS: bootstrap restore preserves the first scheduled tick boundary");
}

static void ExpectArgument(Action action)
{
  try
  {
    action();
    throw new InvalidOperationException("An invalid bootstrap request was accepted.");
  }
  catch (ArgumentException)
  {
  }
}

static void ExpectInvalidOperation(Action action, string scenario)
{
  try
  {
    action();
    throw new InvalidOperationException($"Simulation accepted {scenario} overflow.");
  }
  catch (InvalidOperationException exception) when (
    exception.Message.StartsWith("The configured", StringComparison.Ordinal))
  {
  }
}

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

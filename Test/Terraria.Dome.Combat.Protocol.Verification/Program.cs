using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Terraria.Dome.Protocol.V1456.Dispatch;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Protocol.V1456.Protocol;
using Terraria.Dome.Protocol.V1456.Session;
using Terraria.Dome.Server.Replication;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.WorldModel;

VerifyTypedV1456CombatFrames();
VerifySparseProjectileProjection();
VerifyPvsReplicationCursors();
VerifyCombatCatalogAuthority();
VerifyClientProjectileTerminationBoundary();
VerifyActivePlayerEquipmentBoundary();
VerifyActivePlayerVitalsBoundary();
VerifyClientSyncedInventoryBoundary();
VerifyClientTalkNpcBoundary();
VerifyTypedV1456WorldItemFrame();
VerifyItemPvsReplicationCursor();
Console.WriteLine("PASS: V1456 combat projection and PVS revision cursors");

static void VerifyTypedV1456CombatFrames()
{
  WorldSectionCoordinates section = new(10, 1);
  NpcReplicationSnapshot npc = new(
    ReplicationId: 7,
    NpcType: 1,
    Position: new SimulationVector(2100.0f, 155.0f),
    Velocity: new SimulationVector(-1.0f, 0.0f),
    Health: 90,
    IsActive: true,
    Revision: 3,
    Section: section);
  ProjectileReplicationSnapshot projectile = new(
    ReplicationId: 8,
    ProjectileType: 1,
    Owner: new PlayerHandle(2),
    Position: new SimulationVector(2101.0f, 155.0f),
    Velocity: new SimulationVector(4.0f, 0.0f),
    Damage: 10,
    RemainingLifetime: 20,
    IsActive: true,
    Revision: 2,
    Section: section);

  TerrariaFrame npcFrame = TerrariaFrameCodec.Decode(TerrariaPacketCodec.EncodeNpcReplication(npc));
  TerrariaFrame projectileFrame = TerrariaFrameCodec.Decode(
    TerrariaPacketCodec.EncodeProjectileReplication(projectile));
  TerrariaFrame killFrame = TerrariaFrameCodec.Decode(
    TerrariaPacketCodec.EncodeProjectileDespawn(projectile with
    {
      IsActive = false,
      Revision = 3
    }));
  if (npcFrame.MessageId != TerrariaMessageId.SyncNPC ||
      projectileFrame.MessageId != TerrariaMessageId.SyncProjectile ||
      killFrame.MessageId != TerrariaMessageId.KillProjectile ||
      npcFrame.Payload.Span[0] != 7 || projectileFrame.Payload.Span[0] != 8 ||
      killFrame.Payload.Span[0] != 8 || projectileFrame.Payload.Span[^2] != 10 ||
      projectileFrame.Payload.Span[^1] != 0)
  {
    throw new InvalidOperationException("Combat projection did not use the V1456 entity messages.");
  }

  if (npcFrame.Payload.Span[^2] != 1 || npcFrame.Payload.Span[^1] != 90)
  {
    throw new InvalidOperationException("Combat NPC projection did not encode server-owned health.");
  }

  AssertWireVector(
    npcFrame.Payload.Span,
    expectedPositionX: 33600.0f,
    expectedPositionY: 2480.0f,
    expectedVelocityX: -16.0f,
    expectedVelocityY: 0.0f,
    "NPC");
  AssertWireVector(
    projectileFrame.Payload.Span,
    expectedPositionX: 33616.0f,
    expectedPositionY: 2480.0f,
    expectedVelocityX: 64.0f,
    expectedVelocityY: 0.0f,
    "Projectile");
}

static void VerifySparseProjectileProjection()
{
  ProjectileSyncPacket packet = new(
    Identity: 8,
    Position: new SimulationVector(1.0f, 2.0f),
    Velocity: new SimulationVector(-3.0f, 4.0f),
    Owner: 2,
    ProjectileType: 99,
    Ai0: 1.5f,
    Ai1: null,
    Ai2: 2.5f,
    Banner: 7,
    Damage: 12,
    Knockback: 3.5f,
    OriginalDamage: 14,
    Uuid: 23);
  byte[] frameBytes = TerrariaPacketCodec.EncodeProjectileSync(packet);
  ProjectileSyncPacket decoded = TerrariaPacketCodec.DecodeProjectileSync(frameBytes);
  if (decoded != packet)
  {
    throw new InvalidOperationException("Sparse SyncProjectile flags did not round-trip exactly.");
  }

  TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
  if ((frame.Payload.Span[21] & 0xF9) != 0xF9 || frame.Payload.Span[21] != 0xFD ||
      frame.Payload.Span[22] != 0x01)
  {
    throw new InvalidOperationException("SyncProjectile flags did not encode AI0, AI2 and suffixes.");
  }
}

static void VerifyTypedV1456WorldItemFrame()
{
  ItemReplicationSnapshot item = new(
    ReplicationId: 9,
    Stack: new ItemStack(1, 2),
    Position: new SimulationVector(2100.0f, 155.0f),
    IsActive: true,
    Revision: 1,
    Section: new WorldSectionCoordinates(10, 1));
  TerrariaFrame frame = TerrariaFrameCodec.Decode(TerrariaPacketCodec.EncodeItemReplication(item));
  if (frame.MessageId != TerrariaMessageId.SyncItem || frame.Payload.Length != 24 ||
      BitConverter.ToInt16(frame.Payload.Span[..2]) != 9 ||
      BitConverter.ToInt16(frame.Payload.Span[^2..]) != 1)
  {
    throw new InvalidOperationException("World item projection did not use V1456 SyncItem.");
  }

  AssertWireVector(
    frame.Payload.Span,
    expectedPositionX: 33600.0f,
    expectedPositionY: 2480.0f,
    expectedVelocityX: 0.0f,
    expectedVelocityY: 0.0f,
    "World item");
}

static void AssertWireVector(
  ReadOnlySpan<byte> payload,
  float expectedPositionX,
  float expectedPositionY,
  float expectedVelocityX,
  float expectedVelocityY,
  string entityName)
{
  float positionX = BitConverter.ToSingle(payload.Slice(2, 4));
  float positionY = BitConverter.ToSingle(payload.Slice(6, 4));
  float velocityX = BitConverter.ToSingle(payload.Slice(10, 4));
  float velocityY = BitConverter.ToSingle(payload.Slice(14, 4));
  if (positionX != expectedPositionX || positionY != expectedPositionY ||
      velocityX != expectedVelocityX || velocityY != expectedVelocityY)
  {
    throw new InvalidOperationException(
      $"{entityName} projection must use Terraria pixel coordinates.");
  }
}

static void VerifyItemPvsReplicationCursor()
{
  using SessionReplicationState session = new((_, _) => Task.CompletedTask);
  session.MarkActive();
  WorldSectionCoordinates visible = new(10, 1);
  WorldSectionCoordinates hidden = new(0, 0);
  _ = session.ReplaceVisibleSections(CreateSections(visible));
  ItemReplicationSnapshot visibleItem = new(
    1,
    new ItemStack(1, 2),
    new SimulationVector(2100.0f, 155.0f),
    true,
    1,
    visible);
  ItemReplicationSnapshot hiddenItem = visibleItem with { ReplicationId = 2, Section = hidden };
  ItemReplicationAssembler assembler = new();
  if (assembler.CollectFrames(session, [visibleItem, hiddenItem]).Count != 1 ||
      assembler.CollectFrames(session, [visibleItem]).Count != 0 ||
      assembler.CollectFrames(session, [visibleItem with { Revision = 2 }]).Count != 1)
  {
    throw new InvalidOperationException("World item PVS cursor did not filter and deduplicate revisions.");
  }
}

static void VerifyCombatCatalogAuthority()
{
  TerrariaMessageId[] messageIds =
  [
    TerrariaMessageId.SyncNPC,
    TerrariaMessageId.SyncProjectile,
    TerrariaMessageId.KillProjectile
  ];
  for (int index = 0; index < messageIds.Length; index++)
  {
    TerrariaMessageDescriptor descriptor = TerrariaMessageCatalog.Get(messageIds[index]);
    TerrariaPacketDirection expectedDirection =
      messageIds[index] == TerrariaMessageId.SyncNPC
        ? TerrariaPacketDirection.ServerToClient
        : TerrariaPacketDirection.Bidirectional;
    if (descriptor.Direction != expectedDirection ||
        descriptor.Support != TerrariaPacketSupport.Handled)
    {
      throw new InvalidOperationException("Combat message catalog lacks a server authority claim.");
    }
  }
}

static void VerifyClientProjectileTerminationBoundary()
{
  byte[] capturedFrame = [0x06, 0x00, 0x1D, 0x01, 0x00, 0x05];
  ClientProjectileTermination termination = TerrariaPacketCodec.DecodeClientProjectileTermination(
    capturedFrame);
  if (termination.Identity != 1 || termination.Owner != 5)
  {
    throw new InvalidOperationException("Client KillProjectile did not preserve its wire fields.");
  }

  TerrariaSession session = CreateActiveSession(assignedPlayerSlot: 5);
  TerrariaPacketDispatchResult result = new TerrariaPacketDispatcher().Dispatch(
    session,
    capturedFrame);
  if (result.Outcome != TerrariaPacketDispatchOutcome.ClientProjectileTerminationAccepted ||
      session.State != TerrariaSessionState.Active)
  {
    throw new InvalidOperationException("Owned client KillProjectile closed an active session.");
  }

  byte[] forgedOwner = [0x06, 0x00, 0x1D, 0x01, 0x00, 0x06];
  try
  {
    _ = new TerrariaPacketDispatcher().Dispatch(
      CreateActiveSession(assignedPlayerSlot: 5),
      forgedOwner);
    throw new InvalidOperationException("A forged client KillProjectile owner was accepted.");
  }
  catch (InvalidDataException)
  {
  }

  try
  {
    _ = TerrariaPacketCodec.DecodeClientProjectileTermination(
      [0x07, 0x00, 0x1D, 0x01, 0x00, 0x05, 0x00]);
    throw new InvalidOperationException("Client KillProjectile accepted trailing payload data.");
  }
  catch (InvalidDataException)
  {
  }
}

static void VerifyActivePlayerEquipmentBoundary()
{
  TerrariaSession session = CreateActiveSession(assignedPlayerSlot: 5);
  byte[] ownedEquipment = TerrariaPacketCodec.Encode(new PlayerEquipmentPacket(
    5,
    0,
    0,
    0,
    0,
    IsFavorited: false,
    IsNewAndShiny: false));
  TerrariaPacketDispatchResult result = new TerrariaPacketDispatcher().Dispatch(
    session,
    ownedEquipment);
  if (result.Outcome != TerrariaPacketDispatchOutcome.ActiveSynchronizationAccepted ||
      session.State != TerrariaSessionState.Active)
  {
    throw new InvalidOperationException("Owned active SyncEquipment closed an active session.");
  }

  byte[] forgedOwner = TerrariaPacketCodec.Encode(new PlayerEquipmentPacket(
    6,
    0,
    0,
    0,
    0,
    IsFavorited: false,
    IsNewAndShiny: false));
  try
  {
    _ = new TerrariaPacketDispatcher().Dispatch(
      CreateActiveSession(assignedPlayerSlot: 5),
      forgedOwner);
    throw new InvalidOperationException("A forged active SyncEquipment owner was accepted.");
  }
  catch (InvalidDataException)
  {
  }

  try
  {
    _ = new TerrariaPacketDispatcher().Dispatch(
      CreateActiveSession(assignedPlayerSlot: 5),
      [0x0D, 0x00, 0x05, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]);
    throw new InvalidOperationException("Active SyncEquipment accepted a trailing payload byte.");
  }
  catch (InvalidDataException)
  {
  }
}

static void VerifyActivePlayerVitalsBoundary()
{
  TerrariaSession session = CreateActiveSession(assignedPlayerSlot: 5);
  TerrariaPacketDispatchResult lifeResult = new TerrariaPacketDispatcher().Dispatch(
    session,
    TerrariaPacketCodec.EncodePlayerLifeMana(5, 1, 20));
  TerrariaPacketDispatchResult manaResult = new TerrariaPacketDispatcher().Dispatch(
    session,
    TerrariaPacketCodec.EncodePlayerMana(5, 1, 20));
  if (lifeResult.Outcome != TerrariaPacketDispatchOutcome.ActiveSynchronizationAccepted ||
      manaResult.Outcome != TerrariaPacketDispatchOutcome.ActiveSynchronizationAccepted ||
      session.State != TerrariaSessionState.Active)
  {
    throw new InvalidOperationException("Owned active player vitals closed an active session.");
  }

  try
  {
    _ = new TerrariaPacketDispatcher().Dispatch(
      CreateActiveSession(assignedPlayerSlot: 5),
      TerrariaPacketCodec.EncodePlayerLifeMana(6, 1, 20));
    throw new InvalidOperationException("A forged active PlayerLifeMana owner was accepted.");
  }
  catch (InvalidDataException)
  {
  }
}

static void VerifyClientSyncedInventoryBoundary()
{
  byte[] validNotification = [0x03, 0x00, 0x8A];
  TerrariaSession activeSession = CreateActiveSession(assignedPlayerSlot: 5);
  TerrariaPacketDispatchResult result = new TerrariaPacketDispatcher().Dispatch(
    activeSession,
    validNotification);
  if (result.Outcome != TerrariaPacketDispatchOutcome.ActiveSynchronizationAccepted ||
      activeSession.State != TerrariaSessionState.Active)
  {
    throw new InvalidOperationException(
      "ClientSyncedInventory must preserve an active session without server mutation.");
  }

  try
  {
    _ = new TerrariaPacketDispatcher().Dispatch(
      CreateActiveSession(assignedPlayerSlot: 5),
      [0x04, 0x00, 0x8A, 0x00]);
    throw new InvalidOperationException("ClientSyncedInventory accepted trailing payload data.");
  }
  catch (InvalidDataException)
  {
  }

  try
  {
    _ = new TerrariaPacketDispatcher().Dispatch(new TerrariaSession(5), validNotification);
    throw new InvalidOperationException("ClientSyncedInventory was accepted before activation.");
  }
  catch (InvalidDataException)
  {
  }
}

static void VerifyClientTalkNpcBoundary()
{
  byte[] clearedTalkNpc = [0x06, 0x00, 0x28, 0x05, 0xFF, 0xFF];
  TerrariaSession activeSession = CreateActiveSession(assignedPlayerSlot: 5);
  TerrariaPacketDispatchResult result = new TerrariaPacketDispatcher().Dispatch(
    activeSession,
    clearedTalkNpc);
  if (result.Outcome != TerrariaPacketDispatchOutcome.ActiveSynchronizationAccepted ||
      activeSession.State != TerrariaSessionState.Active)
  {
    throw new InvalidOperationException("SyncTalkNPC must preserve an active session.");
  }

  try
  {
    _ = new TerrariaPacketDispatcher().Dispatch(
      CreateActiveSession(assignedPlayerSlot: 5),
      [0x07, 0x00, 0x28, 0x05, 0xFF, 0xFF, 0x00]);
    throw new InvalidOperationException("SyncTalkNPC accepted trailing payload data.");
  }
  catch (InvalidDataException)
  {
  }

  try
  {
    _ = new TerrariaPacketDispatcher().Dispatch(
      CreateActiveSession(assignedPlayerSlot: 5),
      [0x06, 0x00, 0x28, 0x06, 0xFF, 0xFF]);
    throw new InvalidOperationException("SyncTalkNPC accepted another player's notification.");
  }
  catch (InvalidDataException)
  {
  }
}

static TerrariaSession CreateActiveSession(byte assignedPlayerSlot)
{
  TerrariaSession session = new(assignedPlayerSlot);
  _ = session.AcceptHello(TerrariaPacketCodec.Encode(new HelloPacket()));
  _ = session.AcceptPlayerProfile(TerrariaPacketCodec.Encode(new PlayerProfilePacket(
    assignedPlayerSlot,
    0,
    0,
    0.0f,
    0,
    "ClientProjectileTermination",
    0,
    0,
    0,
    new TerrariaColor(0, 0, 0),
    new TerrariaColor(0, 0, 0),
    new TerrariaColor(0, 0, 0),
    new TerrariaColor(0, 0, 0),
    new TerrariaColor(0, 0, 0),
    new TerrariaColor(0, 0, 0),
    new TerrariaColor(0, 0, 0),
    0,
    0,
    0)));
  _ = session.AcceptPlayerUuid(TerrariaPacketCodec.Encode(new PlayerUuidPacket(
    "22222222-2222-2222-2222-222222222222")));
  _ = session.AcceptRequestWorldData(TerrariaPacketCodec.EncodeRequestWorldData());
  _ = session.AcceptSpawnTileData(TerrariaPacketCodec.EncodeSpawnTileData(
    new SpawnTileDataRequestPacket(2100, 300, 0)));
  session.MarkInitialWorldStreamSent();
  _ = session.AcceptPlayerSpawn(TerrariaPacketCodec.Encode(new PlayerSpawnPacket(
    assignedPlayerSlot,
    2100,
    300,
    0,
    0,
    0,
    0,
    0)));
  return session;
}

static void VerifyPvsReplicationCursors()
{
  List<byte[]> writtenFrames = new();
  using SessionReplicationState session = new((frame, _) =>
  {
    writtenFrames.Add(frame);
    return Task.CompletedTask;
  });
  session.MarkActive();
  WorldSectionCoordinates visible = new(10, 1);
  WorldSectionCoordinates hidden = new(0, 0);
  _ = session.ReplaceVisibleSections(CreateSections(visible));
  NpcReplicationSnapshot visibleNpc = CreateNpc(1, visible, revision: 1, isActive: true);
  ProjectileReplicationSnapshot hiddenProjectile = CreateProjectile(2, hidden, revision: 1, isActive: true);
  IReadOnlyList<byte[]> firstFrames = new CombatReplicationAssembler().CollectFrames(
    session,
    [visibleNpc],
    [hiddenProjectile]);
  if (firstFrames.Count != 1 ||
      TerrariaFrameCodec.Decode(firstFrames[0]).MessageId != TerrariaMessageId.SyncNPC)
  {
    throw new InvalidOperationException("PVS replication emitted a hidden combat entity.");
  }

  if (new CombatReplicationAssembler().CollectFrames(session, [visibleNpc], []).Count != 0)
  {
    throw new InvalidOperationException("Combat replication emitted an unchanged revision.");
  }

  NpcReplicationSnapshot changedNpc = visibleNpc with { Health = 90, Revision = 2 };
  ProjectileReplicationSnapshot enteredProjectile = CreateProjectile(3, visible, 1, true);
  IReadOnlyList<byte[]> changedFrames = new CombatReplicationAssembler().CollectFrames(
    session,
    [changedNpc],
    [enteredProjectile]);
  if (changedFrames.Count != 2 ||
      !changedFrames.Any(frame => TerrariaFrameCodec.Decode(frame).MessageId == TerrariaMessageId.SyncNPC) ||
      !changedFrames.Any(frame => TerrariaFrameCodec.Decode(frame).MessageId ==
        TerrariaMessageId.SyncProjectile))
  {
    throw new InvalidOperationException("Combat replication did not emit changed visible entities.");
  }

  ProjectileReplicationSnapshot destroyedProjectile = enteredProjectile with
  {
    IsActive = false,
    Revision = 2
  };
  IReadOnlyList<byte[]> destroyFrames = new CombatReplicationAssembler().CollectFrames(
    session,
    [changedNpc],
    [destroyedProjectile]);
  if (destroyFrames.Count != 1 ||
      TerrariaFrameCodec.Decode(destroyFrames[0]).MessageId != TerrariaMessageId.KillProjectile)
  {
    throw new InvalidOperationException("Combat replication did not project the projectile tombstone.");
  }

  NpcReplicationSnapshot destroyedNpc = changedNpc with { IsActive = false, Health = 0, Revision = 3 };
  IReadOnlyList<byte[]> npcDestroyFrames = new CombatReplicationAssembler().CollectFrames(
    session,
    [destroyedNpc],
    []);
  if (npcDestroyFrames.Count != 1 ||
      TerrariaFrameCodec.Decode(npcDestroyFrames[0]).MessageId != TerrariaMessageId.SyncNPC)
  {
    throw new InvalidOperationException("Combat replication did not project the NPC death revision.");
  }
}

static IReadOnlyList<WorldSectionSnapshot> CreateSections(WorldSectionCoordinates coordinates)
{
  WorldGrid world = new(4200, 1200);
  return [world.CreateSectionSnapshot(coordinates)];
}

static NpcReplicationSnapshot CreateNpc(
  int replicationId,
  WorldSectionCoordinates section,
  long revision,
  bool isActive)
{
  return new NpcReplicationSnapshot(
    replicationId,
    1,
    new SimulationVector(section.X * 200, section.Y * 150),
    new SimulationVector(0.0f, 0.0f),
    100,
    isActive,
    revision,
    section);
}

static ProjectileReplicationSnapshot CreateProjectile(
  int replicationId,
  WorldSectionCoordinates section,
  long revision,
  bool isActive)
{
  return new ProjectileReplicationSnapshot(
    replicationId,
    1,
    new PlayerHandle(1),
    new SimulationVector(section.X * 200, section.Y * 150),
    new SimulationVector(4.0f, 0.0f),
    10,
    20,
    isActive,
    revision,
    section);
}

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
using Terraria.Dome.Server.Persistence;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Items.Definitions;
using Terraria.Dome.Simulation.StatusEffects.Snapshots;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldObjects;
using Terraria.Dome.Simulation.WorldObjects.Placement;

bool projectileNetworkSchedulingOnly = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--projectile-network-scheduling-only"));
if (projectileNetworkSchedulingOnly)
{
  VerifyProjectileNetworkUpdateScheduling();
  Console.WriteLine(
    "SUMMARY: projectile network-update scheduling focused verification completed; remaining " +
    "Protocol checks were intentionally not run");
  Environment.Exit(0);
}

VerifyTypedV1456CombatFrames();
VerifySparseProjectileProjection();
VerifyNpcProjectileTypedReplicationContract();
VerifyNpcProjectileWireEnvelope();
VerifyNpcProjectileV2WireEnvelope();
VerifyNpcProjectileV3WireEnvelope();
VerifyNpcProjectileSessionCapability();
VerifyNpcProjectileCapabilityNegotiationReconciliation();
VerifyNpcProjectileConsumerCursor();
VerifyNpcProjectileServerCursorRestore();
VerifyNpcProjectileServerCursorPrune();
VerifyNpcProjectileCursorPersistenceSidecar();
VerifyPvsReplicationCursors();
VerifyProjectilePvsReentry();
VerifyNetworkImportantProjectilePvsBypass();
VerifyLegacyNetworkImportantAdmission();
VerifyNpcProjectilePvsReentry();
VerifyNpcStatusEffectPvsCursor();
VerifyProjectileServerCursorPrune();
VerifyCombatCatalogAuthority();
VerifyClientProjectileTerminationBoundary();
VerifyActivePlayerEquipmentBoundary();
VerifyActivePlayerVitalsBoundary();
VerifyClientSyncedInventoryBoundary();
VerifyClientTalkNpcBoundary();
VerifyTypedV1456WorldItemFrame();
VerifyItemPvsReplicationCursor();
VerifyObjectPlacementReplicationOrdering();
Console.WriteLine("PASS: V1456 combat projection and PVS revision cursors");

static void VerifyObjectPlacementReplicationOrdering()
{
  using Arch.Core.World entityWorld = Arch.Core.World.Create();
  Arch.Core.Entity projectile = entityWorld.Create();
  Arch.Core.Entity owner = entityWorld.Create();
  WorldObjectPlacementRequest request = new(
    12,
    projectile,
    owner,
    40,
    50,
    WorldObjectPlacementRequest.SignObjectType,
    2,
    -1,
    "replicated sign");
  WorldObjectPlacementCommittedEvent placement = new(
    12,
    request,
    [new WorldObjectTileMutation(
      40,
      50,
      default,
      new WorldTile(true, WorldObjectPlacementRequest.SignObjectType))],
    new SignSnapshot(3, 40, 50, "replicated sign", 2, new WorldSectionCoordinates(0, 0)),
    [new WorldSectionCoordinates(0, 0)])
  {
    SectionVersion = 9,
    SectionVersions = new Dictionary<WorldSectionCoordinates, long>
    {
      [new WorldSectionCoordinates(0, 0)] = 9
    },
    ProjectileIdentity = 17,
    ProjectileUuid = Guid.Parse("d2719a6f-1b44-4f77-9c1a-bc3f4a0de785")
  };
  ObjectPlacementReplicationAssembler metadataAssembler = new();
  if (!metadataAssembler.TryProject(
        placement,
        out ObjectPlacementReplicationFrame metadataFrame) ||
      metadataFrame.SectionVersion != 9 ||
      metadataFrame.SectionVersions.Count != 1 ||
      metadataFrame.ProjectileIdentity != 17 ||
      metadataFrame.ProjectileUuid != placement.ProjectileUuid)
  {
    throw new InvalidOperationException(
      "Object placement replication did not preserve section and projectile linkage metadata.");
  }

  ObjectPlacementReplicationAssembler assembler = new();
  if (!assembler.TryEncodeOrderedFrames(placement, playerSlot: 7, out IReadOnlyList<byte[]> frames) ||
      frames.Count != 2)
  {
    throw new InvalidOperationException("Committed object placement/sign sequence was not encoded.");
  }

  ObjectPlacementPacket decoded = TerrariaPacketCodec.DecodeObjectPlacement(frames[0]);
  if (decoded.TileX != 40 || decoded.TileY != 50 || decoded.ObjectType != 85 ||
      decoded.Style != 2 || decoded.DirectionRight)
  {
    throw new InvalidOperationException("Object placement packet did not preserve commit parameters.");
  }

  SignUpdateIntent sign = TerrariaPacketCodec.DecodeSignUpdate(frames[1]);
  if (sign.SignId != 3 || sign.TileX != 40 || sign.TileY != 50 || sign.Text != "replicated sign" ||
      sign.PlayerSlot != 7)
  {
    throw new InvalidOperationException("Sign update did not follow object placement commit order.");
  }

  if (assembler.TryProject(placement, out _))
  {
    throw new InvalidOperationException("Object placement sequence was replicated twice.");
  }
}

static void VerifyProjectileNetworkUpdateScheduling()
{
  using SessionReplicationState session = new((_, _) => Task.CompletedTask);
  session.MarkActive();
  WorldSectionCoordinates section = new(10, 1);
  _ = session.ReplaceVisibleSections([new WorldGrid(4200, 1200).CreateSectionSnapshot(section)]);
  ProjectileReplicationSnapshot initial = CreateProjectile(91, section, 1, true) with
  {
    NetworkUpdateReady = false
  };
  CombatReplicationAssembler assembler = new();
  CombatReplicationBatch first = assembler.CollectBatch(session, [], [initial], currentTick: 1);
  if (first.Frames.Count != 1)
  {
    throw new InvalidOperationException(
      "An unseen projectile must receive its initial replication despite cadence gating.");
  }

  session.ConfirmCombatBatch(first);
  CombatReplicationBatch deferred = assembler.CollectBatch(
    session,
    [],
    [initial with { Revision = 2 }],
    currentTick: 2);
  if (deferred.Frames.Count != 0)
  {
    throw new InvalidOperationException(
      "A projectile revision without a network-update decision was replicated immediately.");
  }

  CombatReplicationBatch requested = assembler.CollectBatch(
    session,
    [],
    [initial with { Revision = 3, NetworkUpdateReady = true }],
    currentTick: 3);
  if (requested.Frames.Count != 1)
  {
    throw new InvalidOperationException(
      "A deferred projectile revision was not emitted when the policy allowed a send.");
  }

  using SessionReplicationState npcSession = new((_, _) => Task.CompletedTask);
  npcSession.MarkActive();
  _ = npcSession.ReplaceVisibleSections(
    [new WorldGrid(4200, 1200).CreateSectionSnapshot(section)]);
  npcSession.SetContractCapabilities(new SessionContractCapabilities(
    ContractNegotiationState.Negotiated,
    1,
    1)
  {
    NpcProjectileVersions = 1
  });
  NpcProjectileReplicationSnapshot npcInitial = new(
    ReplicationId: 92,
    ProjectileType: 3,
    Owner: new NpcHandle(7),
    Position: new SimulationVector(2000.0f, 150.0f),
    Velocity: new SimulationVector(1.0f, 0.0f),
    Damage: 10,
    RemainingLifetime: 20,
    IsActive: true,
    Revision: 1,
    Section: section,
    Identity: 92,
    ProjectileUuid: Guid.Parse("92929292-9292-9292-9292-929292929292"),
    NetworkUpdateReady: false);
  CombatReplicationBatch npcFirst = assembler.CollectBatch(
    npcSession,
    [],
    [],
    [npcInitial],
    currentTick: 1);
  if (npcFirst.Frames.Count != 1)
  {
    throw new InvalidOperationException(
      "An unseen NPC projectile must receive its initial replication.");
  }

  npcSession.ConfirmCombatBatch(npcFirst);
  CombatReplicationBatch npcDeferred = assembler.CollectBatch(
    npcSession,
    [],
    [],
    [npcInitial with { Revision = 2 }],
    currentTick: 2);
  if (npcDeferred.Frames.Count != 0)
  {
    throw new InvalidOperationException(
      "An NPC projectile revision without a network-update decision was replicated.");
  }

  Console.WriteLine("PASS: projectile network-update decision gates authoritative replication");
}

static void VerifyTypedV1456CombatFrames()
{
  WorldSectionCoordinates section = new(1, 1);
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

  ProjectileSyncPacket authoritative = ProjectileStateProjection.Project(
    new ProjectileReplicationSnapshot(
      8,
      99,
      new PlayerHandle(2),
      new SimulationVector(1.0f, 2.0f),
      new SimulationVector(-3.0f, 4.0f),
      12,
      20,
      true,
      1,
      new WorldSectionCoordinates(0, 0),
      Identity: 8,
      DefinitionKnockback: 3.5f,
      DefinitionOriginalDamage: 14));
  if (authoritative.Knockback != 3.5f || authoritative.OriginalDamage != 14)
  {
    throw new InvalidOperationException(
      "SyncProjectile projection did not use authoritative definition damage metadata.");
  }
}

static void VerifyNpcProjectileTypedReplicationContract()
{
  List<byte[]> writtenFrames = new();
  using SessionReplicationState session = new((frame, _) =>
  {
    writtenFrames.Add(frame);
    return Task.CompletedTask;
  });
  session.MarkActive();
  WorldSectionCoordinates visible = new(10, 1);
  _ = session.ReplaceVisibleSections(CreateSections(visible));
  NpcProjectileReplicationSnapshot active = new(
    ReplicationId: 31,
    ProjectileType: 3,
    Owner: new NpcHandle(7),
    Position: new SimulationVector(2100.0f, 155.0f),
    Velocity: new SimulationVector(1.0f, 0.0f),
    Damage: 10,
    RemainingLifetime: 20,
    IsActive: true,
    Revision: 2,
    Section: visible,
    Identity: 31,
    DefinitionKnockback: 1.0f,
    DefinitionOriginalDamage: 10);

  CombatReplicationBatch activeBatch = new CombatReplicationAssembler().CollectBatch(
    session,
    [],
    [],
    [active],
    currentTick: 10);
  if (activeBatch.Frames.Count != 0 || activeBatch.NpcProjectiles.Count != 1 ||
      activeBatch.NpcProjectiles[0].Owner != new NpcHandle(7) ||
      activeBatch.NpcProjectiles[0].Identity != 31)
  {
    throw new InvalidOperationException(
      "NPC projectile typed replication was not isolated from V1456 frames.");
  }

  NpcProjectileReplicationSnapshot tombstone = active with
  {
    IsActive = false,
    RemainingLifetime = 0,
    Revision = 3,
    TombstoneReason = ProjectileTombstoneReason.Expired,
    TombstoneRetainedUntilTick = 11
  };
  CombatReplicationBatch retainedBatch = new CombatReplicationAssembler().CollectBatch(
    session,
    [],
    [],
    [tombstone],
    currentTick: 10);
  if (retainedBatch.Frames.Count != 0 || retainedBatch.NpcProjectiles.Count != 1 ||
      retainedBatch.NpcProjectiles[0].TombstoneReason != ProjectileTombstoneReason.Expired)
  {
    throw new InvalidOperationException(
      "NPC projectile typed tombstone was not retained by the non-wire contract.");
  }

  CombatReplicationBatch expiredBatch = new CombatReplicationAssembler().CollectBatch(
    session,
    [],
    [],
    [tombstone with { TombstoneRetainedUntilTick = 10 }],
    currentTick: 10);
  if (expiredBatch.NpcProjectiles.Count != 0)
  {
    throw new InvalidOperationException(
      "NPC projectile typed replication emitted an expired tombstone.");
  }
}

static void VerifyNpcProjectileWireEnvelope()
{
  NpcProjectileReplicationSnapshot active = new(
    ReplicationId: 31,
    ProjectileType: 3,
    Owner: new NpcHandle(7),
    Position: new SimulationVector(2100.0f, 155.0f),
    Velocity: new SimulationVector(1.0f, 0.0f),
    Damage: 10,
    RemainingLifetime: 20,
    IsActive: true,
    Revision: 2,
    Section: new WorldSectionCoordinates(10, 1),
    Identity: 31,
    DefinitionKnockback: 1.5f,
    DefinitionOriginalDamage: 12);
  NpcProjectileReplicationSnapshot decodedActive =
    ContractExtensionCodec.DecodeNpcProjectileReplication(
      ContractExtensionCodec.EncodeNpcProjectileReplication(active));
  if (decodedActive != active)
  {
    throw new InvalidOperationException(
      "NPC projectile active wire envelope did not round-trip typed state.");
  }

  NpcProjectileReplicationSnapshot tombstone = active with
  {
    IsActive = false,
    RemainingLifetime = 0,
    Revision = 3,
    TombstoneReason = ProjectileTombstoneReason.Expired,
    TombstoneRetainedUntilTick = 32
  };
  NpcProjectileReplicationSnapshot decodedTombstone =
    ContractExtensionCodec.DecodeNpcProjectileReplication(
      ContractExtensionCodec.EncodeNpcProjectileReplication(tombstone));
  if (decodedTombstone != tombstone)
  {
    throw new InvalidOperationException(
      "NPC projectile tombstone wire envelope did not preserve termination state.");
  }

  ushort negotiated = ContractExtensionCodec.DecodeNpcProjectileCapabilityOffer(
    ContractExtensionCodec.EncodeNpcProjectileCapabilityOffer(3));
  if (negotiated != 3 ||
      ContractExtensionCodec.DecodeNpcProjectileCapabilityAck(
        ContractExtensionCodec.EncodeNpcProjectileCapabilityAck(3)) != 3)
  {
    throw new InvalidOperationException(
      "NPC projectile capability envelope did not round-trip version 1.");
  }
}

static void VerifyNpcProjectileV2WireEnvelope()
{
  NpcProjectileReplicationSnapshot active = new(
    ReplicationId: 32,
    ProjectileType: 3,
    Owner: new NpcHandle(7),
    Position: new SimulationVector(2100.0f, 155.0f),
    Velocity: new SimulationVector(1.0f, 0.0f),
    Damage: 10,
    RemainingLifetime: 20,
    IsActive: true,
    Revision: 2,
    Section: new WorldSectionCoordinates(10, 1),
    Identity: 32,
    Ai0: 1.25f,
    Ai1: 8.0f,
    Ai2: 0.0f,
    DefinitionKnockback: 1.5f,
    DefinitionOriginalDamage: 12);
  NpcProjectileReplicationSnapshot decoded =
    ContractExtensionCodec.DecodeNpcProjectileReplicationV2(
      ContractExtensionCodec.EncodeNpcProjectileReplicationV2(active));
  if (decoded != active)
  {
    throw new InvalidOperationException(
      "NPC projectile V2 envelope did not round-trip behavior state.");
  }

  if (decoded.ProjectileUuid != null)
  {
    throw new InvalidOperationException(
      "NPC projectile V2 envelope unexpectedly contains a Guid.");
  }

  try
  {
    _ = ContractExtensionCodec.EncodeNpcProjectileReplicationV2(active with { Ai0 = float.NaN });
    throw new InvalidOperationException("NPC projectile V2 envelope accepted non-finite AI state.");
  }
  catch (ArgumentOutOfRangeException)
  {
  }
}

static void VerifyNpcProjectileV3WireEnvelope()
{
  NpcProjectileReplicationSnapshot active = new(
    ReplicationId: 33,
    ProjectileType: 3,
    Owner: new NpcHandle(7),
    Position: new SimulationVector(2100.0f, 155.0f),
    Velocity: new SimulationVector(1.0f, 0.0f),
    Damage: 10,
    RemainingLifetime: 20,
    IsActive: true,
    Revision: 2,
    Section: new WorldSectionCoordinates(10, 1),
    Identity: 33,
    ProjectileUuid: Guid.Parse("11111111-1111-1111-1111-111111111111"),
    Ai0: 1.25f,
    Ai1: 8.0f,
    Ai2: 0.0f,
    DefinitionKnockback: 1.5f,
    DefinitionOriginalDamage: 12);
  NpcProjectileReplicationSnapshot decoded =
    ContractExtensionCodec.DecodeNpcProjectileReplicationV3(
      ContractExtensionCodec.EncodeNpcProjectileReplicationV3(active));
  if (decoded != active)
  {
    throw new InvalidOperationException(
      "NPC projectile V3 envelope did not round-trip Guid behavior state.");
  }
}

static void VerifyNpcProjectileSessionCapability()
{
  TerrariaSession session = CreateActiveSession(assignedPlayerSlot: 5);
  NetModulePacket baseNegotiation = session.AcceptNetModule(
    ContractExtensionCodec.EncodeCapabilityOffer(new ContractCapabilityOffer(1, 1)));
  if (baseNegotiation.ResponseFrame is null ||
      session.ContractCapabilities.State != ContractNegotiationState.Negotiated)
  {
    throw new InvalidOperationException("Base contract capability negotiation did not complete.");
  }

  NetModulePacket npcNegotiation = session.AcceptNetModule(
    ContractExtensionCodec.EncodeNpcProjectileCapabilityOffer(7));
  if (npcNegotiation.ResponseFrame is null ||
      ContractExtensionCodec.DecodeNpcProjectileCapabilityAck(
        npcNegotiation.ResponseFrame) != 7 ||
      !session.ContractCapabilities.SupportsNpcProjectileV3)
  {
    throw new InvalidOperationException(
      "NPC projectile capability negotiation did not enable version 1.");
  }

  try
  {
    _ = session.AcceptNetModule(ContractExtensionCodec.EncodeNpcProjectileCapabilityOffer(1));
    throw new InvalidOperationException(
      "NPC projectile capability negotiation was accepted more than once.");
  }
  catch (InvalidDataException)
  {
  }
}

static void VerifyNpcProjectileCapabilityNegotiationReconciliation()
{
  using SessionReplicationState session = new((_, _) => Task.CompletedTask);
  session.MarkActive();
  WorldSectionCoordinates visible = new(10, 1);
  _ = session.ReplaceVisibleSections(CreateSections(visible));
  NpcProjectileReplicationSnapshot projectile = new(
    ReplicationId: 34,
    ProjectileType: 3,
    Owner: new NpcHandle(7),
    Position: new SimulationVector(2100.0f, 155.0f),
    Velocity: new SimulationVector(1.0f, 0.0f),
    Damage: 10,
    RemainingLifetime: 20,
    IsActive: true,
    Revision: 2,
    Section: visible,
    Identity: 34,
    ProjectileUuid: Guid.Parse("33333333-3333-3333-3333-333333333333"),
    DefinitionKnockback: 1.0f,
    DefinitionOriginalDamage: 10);
  CombatReplicationAssembler assembler = new();

  CombatReplicationBatch preNegotiation = assembler.CollectBatch(session, [], [], [projectile]);
  session.ConfirmCombatBatch(preNegotiation);
  if (session.WasNpcProjectileSent(projectile.ReplicationId))
  {
    throw new InvalidOperationException(
      "An unnegotiated NPC projectile was incorrectly confirmed as sent.");
  }

  session.SetContractCapabilities(new SessionContractCapabilities(
    ContractNegotiationState.Negotiated,
    1,
    1)
  {
    NpcProjectileVersions = 7
  });
  CombatReplicationBatch negotiated = assembler.CollectBatch(session, [], [], [projectile]);
  if (negotiated.Frames.Count != 1 ||
      !ContractExtensionCodec.IsNpcProjectileReplicationV3(negotiated.Frames[0]))
  {
    throw new InvalidOperationException(
      "A visible NPC projectile was not reconciled after capability negotiation.");
  }
}

static void VerifyNpcProjectileConsumerCursor()
{
  NpcProjectileReplicationSnapshot active = new(
    ReplicationId: 31,
    ProjectileType: 3,
    Owner: new NpcHandle(7),
    Position: new SimulationVector(2100.0f, 155.0f),
    Velocity: new SimulationVector(1.0f, 0.0f),
    Damage: 10,
    RemainingLifetime: 20,
    IsActive: true,
    Revision: 2,
    Section: new WorldSectionCoordinates(10, 1),
    Identity: 31,
    DefinitionKnockback: 1.0f,
    DefinitionOriginalDamage: 10);
  NpcProjectileReplicationConsumer consumer = new();
  byte[] activeFrame = ContractExtensionCodec.EncodeNpcProjectileReplication(active);
  if (!consumer.TryApply(activeFrame, currentTick: 10, out _) ||
      consumer.TryApply(activeFrame, currentTick: 10, out _))
  {
    throw new InvalidOperationException(
      "NPC projectile consumer did not enforce monotonic revision delivery.");
  }

  NpcProjectileReplicationSnapshot tombstone = active with
  {
    IsActive = false,
    RemainingLifetime = 0,
    Revision = 3,
    TombstoneReason = ProjectileTombstoneReason.Expired,
    TombstoneRetainedUntilTick = 12
  };
  if (!consumer.TryApply(
        ContractExtensionCodec.EncodeNpcProjectileReplication(tombstone),
        currentTick: 11,
        out NpcProjectileReplicationSnapshot applied) ||
      applied.TombstoneReason != ProjectileTombstoneReason.Expired ||
      consumer.TryApply(
        ContractExtensionCodec.EncodeNpcProjectileReplication(tombstone),
        currentTick: 12,
        out _))
  {
    throw new InvalidOperationException(
      "NPC projectile consumer did not enforce tombstone retention.");
  }

  NpcProjectileReplicationConsumerSnapshot saved = consumer.CreateSnapshot();
  NpcProjectileReplicationConsumer restored = new();
  restored.Restore(saved, currentTick: 11);
  if (restored.TryApply(activeFrame, currentTick: 10, out _) ||
      restored.Snapshots.Count != 1 ||
      restored.Snapshots.Single().IsActive)
  {
    throw new InvalidOperationException(
      "NPC projectile consumer restore did not preserve the tombstone cursor state.");
  }
}

static void VerifyNpcProjectileServerCursorRestore()
{
  using SessionReplicationState first = new((_, _) => Task.CompletedTask);
  first.MarkActive();
  first.RestoreNpcProjectileCursor([new NpcProjectileCursorEntry(31, 3)]);
  IReadOnlyList<NpcProjectileCursorEntry> saved = first.CreateNpcProjectileCursorSnapshot();
  using SessionReplicationState reconnected = new((_, _) => Task.CompletedTask);
  reconnected.MarkActive();
  reconnected.RestoreNpcProjectileCursor(saved);
  if (!reconnected.WasNpcProjectileSent(31) ||
      reconnected.ShouldSendNpcProjectile(new NpcProjectileReplicationSnapshot(
        31,
        3,
        new NpcHandle(7),
        new SimulationVector(1.0f, 1.0f),
        default,
        10,
        10,
        true,
        3,
        new WorldSectionCoordinates(0, 0),
        31,
        DefinitionKnockback: 1.0f,
        DefinitionOriginalDamage: 10)))
  {
    throw new InvalidOperationException(
      "Server NPC projectile cursor restore did not reject an already sent revision.");
  }
}

static void VerifyNpcProjectileServerCursorPrune()
{
  using SessionReplicationState session = new((_, _) => Task.CompletedTask);
  session.MarkActive();
  session.RestoreNpcProjectileCursor([
    new NpcProjectileCursorEntry(31, 3),
    new NpcProjectileCursorEntry(32, 4)]);
  session.PruneNpcProjectileCursor(
    [new NpcProjectileReplicationSnapshot(
      31,
      3,
      new NpcHandle(7),
      new SimulationVector(1.0f, 1.0f),
      default,
      10,
      0,
      false,
      5,
      new WorldSectionCoordinates(0, 0),
      31,
      TombstoneReason: ProjectileTombstoneReason.Expired,
      TombstoneRetainedUntilTick: 12,
      DefinitionKnockback: 1.0f,
      DefinitionOriginalDamage: 10)],
    currentTick: 11);
  if (!session.WasNpcProjectileSent(31) || session.WasNpcProjectileSent(32))
  {
    throw new InvalidOperationException(
      "NPC projectile cursor prune removed retained state or kept missing state.");
  }

  session.PruneNpcProjectileCursor([], currentTick: 12);
  if (session.WasNpcProjectileSent(31))
  {
    throw new InvalidOperationException(
      "NPC projectile cursor prune kept an expired tombstone.");
  }
}

static void VerifyNpcProjectileCursorPersistenceSidecar()
{
  string accountUuid = "11111111-1111-1111-1111-111111111111";
  NpcProjectileCursorAccountState state = new(
    accountUuid,
    [new NpcProjectileCursorEntry(31, 5), new NpcProjectileCursorEntry(44, 8)]);
  using MemoryStream stream = new();
  NpcProjectileCursorPersistenceFormat.Write(stream, [state]);
  stream.Position = 0;
  IReadOnlyList<NpcProjectileCursorAccountState> restored =
    NpcProjectileCursorPersistenceFormat.Read(stream);
  if (restored.Count != 1 || restored[0].AccountUuid != accountUuid ||
      restored[0].Entries.Count != 2 || restored[0].Entries[1].Revision != 8)
  {
    throw new InvalidOperationException(
      "NPC projectile cursor sidecar did not round-trip account state.");
  }

  using MemoryStream legacy = new();
  using (BinaryWriter writer = new(legacy, System.Text.Encoding.UTF8, leaveOpen: true))
  {
    writer.Write(0x44535444);
    writer.Write(1);
  }

  legacy.Position = 0;
  try
  {
    _ = NpcProjectileCursorPersistenceFormat.Read(legacy);
    throw new InvalidOperationException("NPC projectile cursor sidecar accepted a legacy world header.");
  }
  catch (InvalidDataException)
  {
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
  ItemDefinitionRegistry itemDefinitions = new([new ItemDefinition(1, 2)]);
  ItemReplicationAssembler assembler = new(itemDefinitions);
  if (assembler.CollectFrames(session, [visibleItem, hiddenItem]).Count != 1 ||
      assembler.CollectFrames(session, [visibleItem]).Count != 0 ||
      assembler.CollectFrames(session, [visibleItem with { Revision = 2 }]).Count != 1)
  {
    throw new InvalidOperationException("World item PVS cursor did not filter and deduplicate revisions.");
  }

  ItemReplicationSnapshot forgedItem = visibleItem with
  {
    ReplicationId = 3,
    Stack = new ItemStack(600, 1)
  };
  bool unknownItemTypeRejected = false;
  try
  {
    _ = assembler.CollectFrames(session, [forgedItem]);
  }
  catch (InvalidOperationException)
  {
    unknownItemTypeRejected = true;
  }

  if (!unknownItemTypeRejected)
  {
    throw new InvalidOperationException(
      "World-item replication accepted an item type missing from authoritative definitions.");
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
  if (result.Outcome != TerrariaPacketDispatchOutcome.ClientProjectileTerminationIgnored ||
      session.State != TerrariaSessionState.Active)
  {
    throw new InvalidOperationException(
      "Owned client KillProjectile was reported as an authoritative termination.");
  }

  byte[] clientSync = [
    0x19, 0x00, 0x1B,
    0x00, 0x00, 0x00, 0x3E, 0x03, 0x47, 0x00, 0x08, 0x95, 0x45,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x8A, 0x02, 0x00
  ];
  clientSync[21] = 5;
  TerrariaPacketDispatchResult syncResult = new TerrariaPacketDispatcher().Dispatch(
    CreateActiveSession(assignedPlayerSlot: 5),
    clientSync);
  if (syncResult.Outcome != TerrariaPacketDispatchOutcome.ClientProjectileSyncIgnored)
  {
    throw new InvalidOperationException(
      "Client SyncProjectile was reported as authoritative synchronization.");
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

  ProjectileReplicationSnapshot expiredTombstone = enteredProjectile with
  {
    IsActive = false,
    Revision = 3,
    TombstoneRetainedUntilTick = 10
  };
  if (new CombatReplicationAssembler().CollectFrames(
        session,
        [],
        [expiredTombstone],
        currentTick: 10).Count != 0)
  {
    throw new InvalidOperationException("Combat replication emitted an expired projectile tombstone.");
  }

  ProjectileReplicationSnapshot retainedTombstone = expiredTombstone with
  {
    Revision = 4,
    TombstoneRetainedUntilTick = 11
  };
  IReadOnlyList<byte[]> retainedFrames = new CombatReplicationAssembler().CollectFrames(
    session,
    [],
    [retainedTombstone],
    currentTick: 10);
  if (retainedFrames.Count != 1 ||
      TerrariaFrameCodec.Decode(retainedFrames[0]).MessageId != TerrariaMessageId.KillProjectile)
  {
    throw new InvalidOperationException("Combat replication dropped a retained projectile tombstone.");
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

static void VerifyProjectilePvsReentry()
{
  using SessionReplicationState session = new((_, _) => Task.CompletedTask);
  session.MarkActive();
  WorldSectionCoordinates firstSection = new(10, 1);
  WorldSectionCoordinates secondSection = new(11, 1);
  ProjectileReplicationSnapshot projectile = CreateProjectile(41, firstSection, 1, true);
  CombatReplicationAssembler assembler = new();

  _ = session.ReplaceVisibleSections(CreateSections(firstSection));
  if (assembler.CollectFrames(session, [], [projectile]).Count != 1)
  {
    throw new InvalidOperationException("Initial visible projectile was not replicated.");
  }

  _ = session.ReplaceVisibleSections(CreateSections(secondSection));
  if (assembler.CollectFrames(session, [], [projectile]).Count != 0)
  {
    throw new InvalidOperationException("Hidden projectile was replicated after PVS leave.");
  }

  if (!session.WasCombatProjectileSkipped(projectile.ReplicationId))
  {
    throw new InvalidOperationException(
      "Hidden projectile update was not retained for PVS re-entry reconciliation.");
  }

  _ = session.ReplaceVisibleSections(CreateSections(firstSection));
  if (assembler.CollectFrames(session, [], [projectile]).Count != 1)
  {
    throw new InvalidOperationException(
      "Unchanged projectile was not reconciled after leaving and re-entering PVS.");
  }

  if (session.WasCombatProjectileSkipped(projectile.ReplicationId))
  {
    throw new InvalidOperationException(
      "Projectile PVS skip marker was not cleared after reconciliation.");
  }
}

static void VerifyNetworkImportantProjectilePvsBypass()
{
  using SessionReplicationState session = new((_, _) => Task.CompletedTask);
  session.MarkActive();
  WorldSectionCoordinates sourceSection = new(10, 1);
  WorldSectionCoordinates hiddenSection = new(11, 1);
  ProjectileReplicationSnapshot projectile = CreateProjectile(43, sourceSection, 1, true) with
  {
    IsNetworkImportant = true
  };
  CombatReplicationAssembler assembler = new();

  _ = session.ReplaceVisibleSections(CreateSections(hiddenSection));
  IReadOnlyList<byte[]> frames = assembler.CollectFrames(session, [], [projectile]);
  if (frames.Count != 1 ||
      TerrariaFrameCodec.Decode(frames[0]).MessageId != TerrariaMessageId.SyncProjectile)
  {
    throw new InvalidOperationException(
      "Network-important projectile was incorrectly filtered by section visibility.");
  }

  if (session.WasCombatProjectileSkipped(projectile.ReplicationId))
  {
    throw new InvalidOperationException(
      "Network-important projectile was incorrectly marked as PVS-skipped.");
  }

  if (assembler.CollectFrames(session, [], [projectile]).Count != 0)
  {
    throw new InvalidOperationException(
      "Network-important projectile cursor resent an unchanged revision.");
  }
}

static void VerifyLegacyNetworkImportantAdmission()
{
  (int ProjectileType, int LegacyAiStyle)[] sourceBackedAdmissions =
  [
    (12, 0),
    (864, 0),
    (43, 11)
  ];
  WorldSectionCoordinates sourceSection = new(10, 1);
  WorldSectionCoordinates hiddenSection = new(11, 1);

  for (int index = 0; index < sourceBackedAdmissions.Length; index++)
  {
    (int projectileType, int legacyAiStyle) = sourceBackedAdmissions[index];
    using SessionReplicationState session = new((_, _) => Task.CompletedTask);
    session.MarkActive();
    ProjectileReplicationSnapshot projectile = CreateProjectile(
      projectileType,
      sourceSection,
      1,
      true) with
    {
      ProjectileType = projectileType,
      LegacyAiStyle = legacyAiStyle
    };

    _ = session.ReplaceVisibleSections(CreateSections(hiddenSection));
    IReadOnlyList<byte[]> frames = new CombatReplicationAssembler().CollectFrames(
      session,
      [],
      [projectile]);
    if (frames.Count != 1 ||
        TerrariaFrameCodec.Decode(frames[0]).MessageId != TerrariaMessageId.SyncProjectile)
    {
      throw new InvalidOperationException(
        $"Legacy network-important admission was filtered for projectile type " +
        $"{projectileType} (aiStyle {legacyAiStyle}).");
    }

    if (session.WasCombatProjectileSkipped(projectile.ReplicationId))
    {
      throw new InvalidOperationException(
        $"Legacy network-important projectile type {projectileType} was marked PVS-skipped.");
    }
  }

  Console.WriteLine("PASS: source-backed legacy network-important admission bypasses PVS");
}

static void VerifyNpcProjectilePvsReentry()
{
  using SessionReplicationState session = new((_, _) => Task.CompletedTask);
  session.MarkActive();
  session.SetContractCapabilities(new SessionContractCapabilities(
    ContractNegotiationState.Negotiated,
    1,
    1)
  {
    NpcProjectileVersions = 7
  });
  WorldSectionCoordinates firstSection = new(10, 1);
  WorldSectionCoordinates secondSection = new(11, 1);
  NpcProjectileReplicationSnapshot projectile = new(
    42,
    3,
    new NpcHandle(7),
    new SimulationVector(2100.0f, 155.0f),
    new SimulationVector(1.0f, 0.0f),
    10,
    20,
    true,
    1,
    firstSection,
    Identity: 42,
    ProjectileUuid: Guid.Parse("22222222-2222-2222-2222-222222222222"),
    DefinitionKnockback: 1.0f,
    DefinitionOriginalDamage: 10);
  CombatReplicationAssembler assembler = new();

  _ = session.ReplaceVisibleSections(CreateSections(firstSection));
  CombatReplicationBatch initialBatch = assembler.CollectBatch(session, [], [], [projectile]);
  if (initialBatch.Frames.Count != 1 ||
      !ContractExtensionCodec.IsNpcProjectileReplicationV3(initialBatch.Frames[0]))
  {
    throw new InvalidOperationException("Initial visible NPC projectile was not replicated.");
  }

  session.ConfirmCombatBatch(initialBatch);

  _ = session.ReplaceVisibleSections(CreateSections(secondSection));
  if (assembler.CollectBatch(session, [], [], [projectile]).Frames.Count != 0)
  {
    throw new InvalidOperationException("Hidden NPC projectile was replicated after PVS leave.");
  }

  if (!session.WasNpcProjectileSkipped(projectile.ReplicationId))
  {
    throw new InvalidOperationException(
      "Hidden NPC projectile update was not retained for PVS re-entry reconciliation.");
  }

  _ = session.ReplaceVisibleSections(CreateSections(firstSection));
  CombatReplicationBatch reconciledBatch = assembler.CollectBatch(
    session,
    [],
    [],
    [projectile]);
  if (reconciledBatch.Frames.Count != 1)
  {
    throw new InvalidOperationException(
      "Unchanged NPC projectile was not reconciled after leaving and re-entering PVS.");
  }

  session.ConfirmCombatBatch(reconciledBatch);

  if (session.WasNpcProjectileSkipped(projectile.ReplicationId))
  {
    throw new InvalidOperationException(
      "NPC projectile PVS skip marker was not cleared after reconciliation.");
  }
}

static void VerifyNpcStatusEffectPvsCursor()
{
  WorldSectionCoordinates section = new(1, 1);
  NpcReplicationSnapshot npc = new(
    7,
    1,
    new SimulationVector(2100.0f, 155.0f),
    new SimulationVector(0.0f, 0.0f),
    90,
    true,
    3,
    section);
  NpcStatusEffectStateSnapshot active = new(
    7,
    11,
    [new StatusEffectSnapshot(StatusEffectTargetKind.Npc, 7, 11, 119, 1800)]);
  using SessionReplicationState session = new((_, _) => Task.CompletedTask);
  session.MarkActive();
  session.SetContractCapabilities(new SessionContractCapabilities(
    ContractNegotiationState.Negotiated,
    1,
    1)
  {
    NpcStatusEffectVersions = 1
  });
  CombatReplicationAssembler assembler = new();
  if (assembler.CollectBatch(session, [npc], [], [], [active]).Frames.Count != 0)
  {
    throw new InvalidOperationException("NPC status effect escaped the session PVS boundary.");
  }

  _ = session.ReplaceVisibleSections([new WorldGrid(400, 300).CreateSectionSnapshot(section)]);
  CombatReplicationBatch first = assembler.CollectBatch(session, [npc], [], [], [active]);
  if (first.Frames.Count != 2 || !ContractExtensionCodec.IsNpcStatusEffect(first.Frames[^1]))
  {
    throw new InvalidOperationException("Visible negotiated NPC status effect was not replicated.");
  }

  NpcStatusEffectEnvelope decoded = ContractExtensionCodec.DecodeNpcStatusEffect(first.Frames[^1]);
  if (decoded.ReplicationId != 7 || decoded.Revision != 11 || decoded.Effects.Count != 1 ||
      decoded.Effects[0].Type != 119 || decoded.Effects[0].RemainingTicks != 1800)
  {
    throw new InvalidOperationException("NPC status effect extension did not preserve its state.");
  }

  session.ConfirmCombatBatch(first);
  if (assembler.CollectBatch(session, [npc], [], [], [active]).Frames.Count != 0)
  {
    throw new InvalidOperationException("NPC status effect cursor resent an unchanged revision.");
  }

  NpcStatusEffectStateSnapshot cleared = new(7, 12, []);
  CombatReplicationBatch clear = assembler.CollectBatch(session, [npc], [], [], [cleared]);
  if (clear.Frames.Count != 1 ||
      ContractExtensionCodec.DecodeNpcStatusEffect(clear.Frames[0]).Effects.Count != 0)
  {
    throw new InvalidOperationException("NPC status effect expiration did not emit a clear frame.");
  }
}

static void VerifyProjectileServerCursorPrune()
{
  using SessionReplicationState session = new((_, _) => Task.CompletedTask);
  session.MarkActive();
  WorldSectionCoordinates section = new(10, 1);
  ProjectileReplicationSnapshot retained = CreateProjectile(51, section, 2, false) with
  {
    TombstoneReason = ProjectileTombstoneReason.Expired,
    TombstoneRetainedUntilTick = 12
  };
  ProjectileReplicationSnapshot missing = CreateProjectile(52, section, 2, true);
  session.ConfirmCombatBatch(new CombatReplicationBatch([], [], [retained, missing]));
  session.PruneCombatProjectileCursor([retained], currentTick: 11);
  if (!session.WasCombatProjectileSent(51) || session.WasCombatProjectileSent(52))
  {
    throw new InvalidOperationException(
      "Projectile cursor prune did not preserve retained state or remove missing state.");
  }

  session.PruneCombatProjectileCursor([retained], currentTick: 12);
  if (session.WasCombatProjectileSent(51))
  {
    throw new InvalidOperationException("Projectile cursor prune kept an expired tombstone.");
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

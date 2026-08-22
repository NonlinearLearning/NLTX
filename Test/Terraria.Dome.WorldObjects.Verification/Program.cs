using System;
using System.Linq;
using System.Threading.Tasks;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Protocol.V1456.Protocol;
using Terraria.Dome.Server.Replication;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Npc.Commands;
using Terraria.Dome.Simulation.Npc.Components;
using Terraria.Dome.Simulation.Npc.Definitions;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldObjects;
using Terraria.Dome.Simulation.WorldObjects.Definitions;

VerifyTrainingDummyTileValidity();
VerifyTrainingDummyPersistentState();
VerifyTrainingDummyNpcLinkValidity();
VerifyTrainingDummyActivationEligibility();
VerifyTrainingDummyActivationDecision();
VerifyTrainingDummyDeactivationDecision();
VerifyTrainingDummyOwnershipTransitions();
VerifySimulationTrainingDummyOwnership();
VerifySimulationTrainingDummyTickLifecycle();
VerifyTrainingDummyPersistenceProjection();
VerifyTrainingDummyProtocolProjection();
VerifyTrainingDummyReplicationCursor();
VerifyTrainingDummyPlacementRemovalOwner();

using DomeSimulation simulation = new(new WorldGrid(400, 300));
PlayerHandle firstPlayer = simulation.CreatePlayer(new SimulationVector(10.0f, 10.0f));
PlayerHandle secondPlayer = simulation.CreatePlayer(new SimulationVector(11.0f, 10.0f));
int chestId = simulation.CreateChest(12, 10);
simulation.GetChest(chestId).SetSlot(0, new ItemStack(1, 3));

if (!simulation.TryOpenChest(chestId, firstPlayer, new SimulationVector(10.0f, 10.0f)) ||
    simulation.TryOpenChest(chestId, secondPlayer, new SimulationVector(11.0f, 10.0f)) ||
    simulation.TryOpenChest(chestId, secondPlayer, new SimulationVector(100.0f, 10.0f)))
{
  throw new InvalidOperationException("Chest ownership or interaction range was not authoritative.");
}

simulation.CloseChest(firstPlayer);
if (!simulation.TryOpenChest(chestId, secondPlayer, new SimulationVector(11.0f, 10.0f)))
{
  throw new InvalidOperationException("Chest close did not release the exclusive opener.");
}

if (!simulation.TryTransferChestItem(
      chestId,
      secondPlayer,
      inventorySlot: 0,
      chestSlot: 0,
      withdraw: true,
      new SimulationVector(11.0f, 10.0f)) ||
    simulation.GetInventory(secondPlayer).GetSlot(0) != new ItemStack(1, 3) ||
    !simulation.GetChest(chestId).GetSlot(0).IsEmpty ||
    simulation.TryTransferChestItem(
      chestId,
      firstPlayer,
      inventorySlot: 1,
      chestSlot: 0,
      withdraw: false,
      new SimulationVector(10.0f, 10.0f)))
{
  throw new InvalidOperationException("Chest transfer did not enforce opener ownership.");
}

using DomeSimulation staleRevisionSimulation = new(new WorldGrid(400, 300));
PlayerHandle staleRevisionPlayer = staleRevisionSimulation.CreatePlayer(
  new SimulationVector(20.0f, 20.0f));
int staleRevisionChestId = staleRevisionSimulation.CreateChest(20, 20);
staleRevisionSimulation.SetChestItem(staleRevisionChestId, 0, new ItemStack(1, 1));
if (!staleRevisionSimulation.TryOpenChest(
      staleRevisionChestId,
      staleRevisionPlayer,
      new SimulationVector(20.0f, 20.0f)))
{
  throw new InvalidOperationException("Stale-revision fixture could not open its chest.");
}

long staleRevision = staleRevisionSimulation.CreateChestSnapshots().Single().Revision;
if (!staleRevisionSimulation.TryTransferChestItem(
      staleRevisionChestId,
      staleRevisionPlayer,
      inventorySlot: 0,
      chestSlot: 0,
      withdraw: true,
      new SimulationVector(20.0f, 20.0f),
      staleRevision) ||
    staleRevisionSimulation.TryTransferChestItem(
      staleRevisionChestId,
      staleRevisionPlayer,
      inventorySlot: 1,
      chestSlot: 0,
      withdraw: false,
      new SimulationVector(20.0f, 20.0f),
      staleRevision) ||
    staleRevisionSimulation.CreateChestSnapshots().Single().Revision != staleRevision + 1)
{
  throw new InvalidOperationException(
    "Chest transfer did not reject a stale expected revision without mutating state.");
}

Console.WriteLine("PASS: chest transfer expected revision is an atomic stale-command guard");

Console.WriteLine("PASS: section-local chest ownership and range are authoritative");

static void VerifyTrainingDummyTileValidity()
{
  WorldTile validTile = new(
    IsActive: true,
    Type: 378,
    FrameX: 36,
    FrameY: 0);
  if (!TileEntityTrainingDummyValidityQuery.IsValid(validTile))
  {
    throw new InvalidOperationException("A valid Training Dummy tile was rejected.");
  }

  WorldTile[] invalidTiles =
  [
    validTile with { IsActive = false },
    validTile with { Type = 377 },
    validTile with { FrameX = 18 },
    validTile with { FrameY = 18 }
  ];
  for (int index = 0; index < invalidTiles.Length; index++)
  {
    if (TileEntityTrainingDummyValidityQuery.IsValid(invalidTiles[index]))
    {
      throw new InvalidOperationException(
        $"Invalid Training Dummy tile case {index} was accepted.");
    }
  }

  if (!TileEntityDefinitionRegistry.TryGet(0, out TileEntityDefinition definition) ||
      definition.Name != "TrainingDummy")
  {
    throw new InvalidOperationException("Training Dummy definition registration is missing.");
  }

  Console.WriteLine("PASS: Training Dummy tile validity follows the complete oracle");
}

static void VerifyTrainingDummyPersistentState()
{
  TileEntityPersistentState source = new(
    id: 4,
    type: 0,
    tileX: 12,
    tileY: 14,
    payload: [0xFB, 0xFF],
    isOpaque: false);
  if (!TrainingDummyTileEntityState.TryRead(source, out TrainingDummyTileEntityState state) ||
      state.EntityId != 4 ||
      state.TileX != 12 ||
      state.TileY != 14 ||
      state.NpcId != -5)
  {
    throw new InvalidOperationException("Training Dummy typed persistence state was not read.");
  }

  TileEntityPersistentState malformed = new(
    id: 5,
    type: 0,
    tileX: 12,
    tileY: 14,
    payload: [0x00],
    isOpaque: false);
  if (TrainingDummyTileEntityState.TryRead(malformed, out _))
  {
    throw new InvalidOperationException("Malformed Training Dummy payload was accepted.");
  }

  Console.WriteLine("PASS: Training Dummy typed persistence state rejects malformed payloads");
}

static void VerifyTrainingDummyNpcLinkValidity()
{
  TrainingDummyTileEntityState entity = new(4, 12, 14, 7);
  TrainingDummyNpcLinkSnapshot validNpc = new(
    IsActive: true,
    NpcType: 488,
    AiTileX: 12,
    AiTileY: 14);
  if (!TrainingDummyNpcLinkValidityQuery.IsValid(entity, validNpc))
  {
    throw new InvalidOperationException("A valid Training Dummy NPC link was rejected.");
  }

  TrainingDummyNpcLinkSnapshot[] invalidNpcs =
  [
    validNpc with { IsActive = false },
    validNpc with { NpcType = 487 },
    validNpc with { AiTileX = 13 },
    validNpc with { AiTileY = 15 }
  ];
  for (int index = 0; index < invalidNpcs.Length; index++)
  {
    if (TrainingDummyNpcLinkValidityQuery.IsValid(entity, invalidNpcs[index]))
    {
      throw new InvalidOperationException($"Invalid Training Dummy NPC link {index} was accepted.");
    }
  }

  TrainingDummyTileEntityState unlinked = entity with { NpcId = -1 };
  if (TrainingDummyNpcLinkValidityQuery.IsValid(unlinked, validNpc))
  {
    throw new InvalidOperationException("An unlinked Training Dummy was treated as linked.");
  }

  Console.WriteLine("PASS: Training Dummy NPC link validity follows the oracle");
}

static void VerifyTrainingDummyActivationEligibility()
{
  TrainingDummyTileEntityState entity = new(4, 12, 14, -1);
  TrainingDummyPlayerHitboxSnapshot nearbyPlayer = new(
    IsActive: true,
    X: 12 * 16 + 16,
    Y: 14 * 16 + 24,
    Width: 20,
    Height: 40);
  if (!TrainingDummyActivationEligibilityQuery.IsPlayerInRange(entity, nearbyPlayer))
  {
    throw new InvalidOperationException("A nearby active player did not activate Training Dummy eligibility.");
  }

  TrainingDummyPlayerHitboxSnapshot[] invalidPlayers =
  [
    nearbyPlayer with { IsActive = false },
    nearbyPlayer with { X = 12 * 16 + 32 + 1601 },
    nearbyPlayer with { Y = 14 * 16 + 48 + 1601 }
  ];
  for (int index = 0; index < invalidPlayers.Length; index++)
  {
    if (TrainingDummyActivationEligibilityQuery.IsPlayerInRange(entity, invalidPlayers[index]))
    {
      throw new InvalidOperationException($"Invalid Training Dummy activation case {index} was accepted.");
    }
  }

  Console.WriteLine("PASS: Training Dummy activation eligibility uses inflated tile bounds");
}

static void VerifyTrainingDummyActivationDecision()
{
  TrainingDummyTileEntityState entity = new(4, 12, 14, -1);
  TrainingDummyPlayerHitboxSnapshot player = new(true, 12 * 16, 14 * 16, 32, 48);
  TrainingDummyActivationDecisionResult result =
    TrainingDummyActivationDecisionQuery.Evaluate(entity, [player], npcSlotsFull: false);
  if (!result.ShouldActivate || result.Reason != TrainingDummyActivationDecisionReason.PlayerNearby)
  {
    throw new InvalidOperationException("Training Dummy activation decision did not accept a nearby player.");
  }

  TrainingDummyActivationDecisionResult fullResult =
    TrainingDummyActivationDecisionQuery.Evaluate(entity, [player], npcSlotsFull: true);
  if (fullResult.ShouldActivate ||
      fullResult.Reason != TrainingDummyActivationDecisionReason.NpcSlotsFull)
  {
    throw new InvalidOperationException("Full NPC slots did not suppress Training Dummy activation.");
  }

  TrainingDummyTileEntityState linked = entity with { NpcId = 3 };
  TrainingDummyActivationDecisionResult linkedResult =
    TrainingDummyActivationDecisionQuery.Evaluate(linked, [player], npcSlotsFull: false);
  if (linkedResult.ShouldActivate ||
      linkedResult.Reason != TrainingDummyActivationDecisionReason.AlreadyLinked)
  {
    throw new InvalidOperationException("An already-linked Training Dummy was reactivated.");
  }

  Console.WriteLine("PASS: Training Dummy activation decision preserves slot and link guards");
}

static void VerifyTrainingDummyDeactivationDecision()
{
  TrainingDummyTileEntityState linked = new(4, 12, 14, 7);
  TrainingDummyNpcLinkSnapshot invalidNpc = new(
    IsActive: false,
    NpcType: 488,
    AiTileX: 12,
    AiTileY: 14);
  if (!TrainingDummyDeactivationDecisionQuery.ShouldDeactivate(linked, invalidNpc))
  {
    throw new InvalidOperationException("An invalid linked NPC did not request deactivation.");
  }

  TrainingDummyNpcLinkSnapshot validNpc = invalidNpc with { IsActive = true };
  if (TrainingDummyDeactivationDecisionQuery.ShouldDeactivate(linked, validNpc))
  {
    throw new InvalidOperationException("A valid linked NPC was marked for deactivation.");
  }

  TrainingDummyTileEntityState unlinked = linked with { NpcId = -1 };
  if (TrainingDummyDeactivationDecisionQuery.ShouldDeactivate(unlinked, invalidNpc))
  {
    throw new InvalidOperationException("An unlinked Training Dummy requested deactivation.");
  }

  Console.WriteLine("PASS: Training Dummy deactivation decision repairs invalid links only");
}

static void VerifyTrainingDummyOwnershipTransitions()
{
  TrainingDummyTileEntityState unlinked = new(4, 12, 14, -1);
  TrainingDummyNpcLinkSnapshot npc = new(true, 488, 12, 14);
  if (!TrainingDummyOwnershipState.TryLink(
        unlinked,
        new NpcHandle(9),
        npc,
        out TrainingDummyOwnershipState linked) ||
      linked.EntityId != 4 ||
      linked.Npc.Value != 9 ||
      linked.Revision != 1)
  {
    throw new InvalidOperationException("Training Dummy ownership link was not created atomically.");
  }

  if (TrainingDummyOwnershipState.TryLink(
        unlinked,
        new NpcHandle(0),
        npc,
        out _))
  {
    throw new InvalidOperationException("Invalid NPC handle was accepted for Training Dummy ownership.");
  }

  if (!TrainingDummyOwnershipState.TryClear(
        linked,
        unlinked,
        npc with { IsActive = false },
        out TrainingDummyOwnershipState cleared) ||
      cleared.Npc.IsValid ||
      cleared.Revision != 2)
  {
    throw new InvalidOperationException("Training Dummy ownership link was not cleared with a revision.");
  }

  Console.WriteLine("PASS: Training Dummy ownership transitions are typed and revisioned");
}

static void VerifySimulationTrainingDummyOwnership()
{
  WorldGrid grid = new(400, 300);
  WorldMetadata metadata = new("training-dummy", new WorldSeed(1), 400, 300);
  TileEntityPersistentState tileEntity = new(
    id: 4,
    type: 0,
    tileX: 12,
    tileY: 14,
    payload: [0xFF, 0xFF],
    isOpaque: false);
  NpcReplicationSnapshot npc = new(
    ReplicationId: 9,
    NpcType: 488,
    Position: new SimulationVector(12.0f, 14.0f),
    Velocity: default,
    Health: 1000,
    IsActive: true,
    Revision: 1,
    Section: grid.GetSectionCoordinates(12, 14),
    DefinitionId: 488,
    MaximumHealth: 1000,
    BehaviorId: NpcBehaviorId.TrainingDummy);
  DomeSimulationSnapshot snapshot = new(
    grid.CreateSnapshot(metadata),
    [npc],
    [],
    tickNumber: 0,
    tileEntities: [tileEntity]);
  using DomeSimulation simulation = new(snapshot);
  TrainingDummyNpcLinkSnapshot link = new(true, 488, 12, 14);
  if (!simulation.TryLinkTrainingDummyNpc(4, new NpcHandle(9), link, out long linkRevision) ||
      linkRevision != 1 ||
      simulation.TryLinkTrainingDummyNpc(4, new NpcHandle(9), link, out _))
  {
    throw new InvalidOperationException("Simulation did not guard Training Dummy ownership linking.");
  }

  TileEntityPersistentState linkedSnapshot = simulation.CreateTileEntitySnapshots()
    .Single(entity => entity.Id == 4);
  if (linkedSnapshot.Payload.Count != 2 ||
      linkedSnapshot.Payload[0] != 9 ||
      linkedSnapshot.Payload[1] != 0)
  {
    throw new InvalidOperationException(
      "Simulation snapshot did not project the linked Training Dummy Int16 payload.");
  }

  DomeSimulationSnapshot linkedPersistence = simulation.CreatePersistenceSnapshot(metadata);
  using (DomeSimulation linkedRestart = new(linkedPersistence))
  {
    TileEntityPersistentState restoredLinkedSnapshot = linkedRestart
      .CreateTileEntitySnapshots()
      .Single(entity => entity.Id == 4);
    if (restoredLinkedSnapshot.Payload.Count != 2 ||
        restoredLinkedSnapshot.Payload[0] != 9 ||
        restoredLinkedSnapshot.Payload[1] != 0)
    {
      throw new InvalidOperationException(
        "Simulation restart did not reconstruct linked Training Dummy ownership.");
    }

    if (!linkedRestart.TryClearTrainingDummyNpc(
          4,
          link with { IsActive = false },
          out long restoredClearRevision) ||
        restoredClearRevision != 2)
    {
      throw new InvalidOperationException(
        "Restored Training Dummy ownership did not accept the typed clear transition.");
    }
  }

  if (!simulation.TryClearTrainingDummyNpc(
        4,
        link with { IsActive = false },
        out long clearRevision) ||
      clearRevision != 2)
  {
    throw new InvalidOperationException("Simulation did not clear Training Dummy ownership revisionally.");
  }

  TileEntityPersistentState clearedSnapshot = simulation.CreateTileEntitySnapshots()
    .Single(entity => entity.Id == 4);
  if (clearedSnapshot.Payload.Count != 2 ||
      clearedSnapshot.Payload[0] != 0xFF ||
      clearedSnapshot.Payload[1] != 0xFF)
  {
    throw new InvalidOperationException(
      "Simulation snapshot did not project the cleared Training Dummy Int16 payload.");
  }

  DomeSimulationSnapshot persisted = simulation.CreatePersistenceSnapshot(metadata);
  using DomeSimulation restarted = new(persisted);
  TileEntityPersistentState restartedSnapshot = restarted.CreateTileEntitySnapshots()
    .Single(entity => entity.Id == 4);
  if (restartedSnapshot.Payload.Count != 2 ||
      restartedSnapshot.Payload[0] != 0xFF ||
      restartedSnapshot.Payload[1] != 0xFF)
  {
    throw new InvalidOperationException(
      "Simulation restart did not preserve the cleared Training Dummy payload.");
  }

  Console.WriteLine("PASS: Simulation owns Training Dummy link and clear transitions");
}

static void VerifySimulationTrainingDummyTickLifecycle()
{
  WorldGrid grid = new(400, 300);
  WorldMetadata metadata = new("training-dummy-tick", new WorldSeed(2), 400, 300);
  _ = grid.TrySetTile(12, 14, new WorldTile(
    IsActive: true,
    Type: 378,
    FrameX: 0,
    FrameY: 0));
  TileEntityPersistentState tileEntity = new(
    id: 4,
    type: 0,
    tileX: 12,
    tileY: 14,
    payload: [0xFF, 0xFF],
    isOpaque: false);
  DomeSimulationSnapshot snapshot = new(
    grid.CreateSnapshot(metadata),
    [],
    [],
    tickNumber: 0,
    tileEntities: [tileEntity]);
  using DomeSimulation simulation = new(snapshot);
  PlayerHandle activationPlayer = simulation.CreatePlayer(new SimulationVector(12 * 16, 14 * 16));

  simulation.Tick(new SimulationInputBatch());
  NpcReplicationSnapshot npc = simulation.CreateNpcReplicationSnapshots()
    .Single(candidate => candidate.NpcType == 488);
  TileEntityPersistentState linked = simulation.CreateTileEntitySnapshots()
    .Single(entity => entity.Id == 4);
  if (npc.Position != new SimulationVector(12, 14) ||
      linked.Payload.Count != 2 ||
      linked.Payload[0] != (byte)npc.ReplicationId ||
      linked.Payload[1] != 0)
  {
    throw new InvalidOperationException(
      $"Training Dummy tick did not create and link NPC type 488 authoritatively: " +
      $"npcPosition={npc.Position}, payload=[{string.Join(',', linked.Payload)}].");
  }

  simulation.QueueNpcDespawn(new DespawnNpcCommand(
    new NpcHandle(npc.ReplicationId),
    NpcDespawnReason.OutOfRange));
  if (!simulation.DestroyPlayer(activationPlayer))
  {
    throw new InvalidOperationException("Training Dummy activation player was not removed.");
  }
  simulation.Tick(new SimulationInputBatch());
  TileEntityPersistentState deactivated = simulation.CreateTileEntitySnapshots()
    .Single(entity => entity.Id == 4);
  NpcReplicationSnapshot deactivatedNpc = simulation.CreateNpcReplicationSnapshots()
    .Single(candidate => candidate.ReplicationId == npc.ReplicationId);
  if (deactivatedNpc.IsActive ||
      deactivated.Payload.Count != 2 ||
      deactivated.Payload[0] != 0xFF ||
      deactivated.Payload[1] != 0xFF)
  {
    throw new InvalidOperationException(
      $"Training Dummy tick did not clear ownership after NPC deactivation: " +
      $"active={deactivatedNpc.IsActive}, payload=[{string.Join(',', deactivated.Payload)}].");
  }

  Console.WriteLine("PASS: Simulation tick activates and deactivates Training Dummy NPC");
}

static void VerifyTrainingDummyPersistenceProjection()
{
  TileEntityPersistentState source = new(
    id: 4,
    type: 0,
    tileX: 12,
    tileY: 14,
    payload: [0xFF, 0xFF],
    isOpaque: false);
  TrainingDummyOwnershipState linked = new(4, 12, 14, new NpcHandle(9), 1);
  if (!TrainingDummyPersistenceProjection.TryProject(
        source,
        linked,
        out TileEntityPersistentState projected) ||
      projected.Payload.Count != 2 ||
      projected.Payload[0] != 9 ||
      projected.Payload[1] != 0)
  {
    throw new InvalidOperationException("Training Dummy ownership was not projected to Int16 payload.");
  }

  TrainingDummyOwnershipState cleared = linked with { Npc = default, Revision = 2 };
  if (!TrainingDummyPersistenceProjection.TryProject(
        source,
        cleared,
        out TileEntityPersistentState clearedProjection) ||
      clearedProjection.Payload[0] != 0xFF ||
      clearedProjection.Payload[1] != 0xFF)
  {
    throw new InvalidOperationException("Training Dummy clear was not projected as -1.");
  }

  TileEntityPersistentState opaque = new(4, 0, 12, 14, [0xFF, 0xFF], isOpaque: true);
  if (TrainingDummyPersistenceProjection.TryProject(opaque, linked, out _))
  {
    throw new InvalidOperationException("Opaque Training Dummy payload was projected as typed state.");
  }

  Console.WriteLine("PASS: Training Dummy persistence projection preserves typed Int16 link state");
}

static void VerifyTrainingDummyProtocolProjection()
{
  TileEntityPersistentState entity = new(
    id: 4,
    type: 0,
    tileX: 12,
    tileY: 14,
    payload: [9, 0],
    isOpaque: false);
  TerrariaFrame sharing = TerrariaFrameCodec.Decode(
    TerrariaPacketCodec.EncodeTrainingDummyTileEntitySharing(entity));
  if (sharing.MessageId != TerrariaMessageId.TileEntitySharing ||
      sharing.Payload.Length != 16 ||
      BitConverter.ToInt32(sharing.Payload.Span[..4]) != 4 ||
      sharing.Payload.Span[4] != 1 ||
      sharing.Payload.Span[5] != 0 ||
      BitConverter.ToInt32(sharing.Payload.Span[6..10]) != 4 ||
      BitConverter.ToInt16(sharing.Payload.Span[10..12]) != 12 ||
      BitConverter.ToInt16(sharing.Payload.Span[12..14]) != 14 ||
      sharing.Payload.Span[14] != 9 ||
      sharing.Payload.Span[15] != 0)
  {
    throw new InvalidOperationException("Training Dummy message 86 sharing projection was invalid.");
  }

  TerrariaFrame removal = TerrariaFrameCodec.Decode(
    TerrariaPacketCodec.EncodeTrainingDummyTileEntityRemoval(4));
  TerrariaFrame placement = TerrariaFrameCodec.Decode(
    TerrariaPacketCodec.EncodeTrainingDummyTileEntityPlacement(12, 14));
  if (removal.MessageId != TerrariaMessageId.TileEntitySharing ||
      removal.Payload.Length != 5 ||
      BitConverter.ToInt32(removal.Payload.Span[..4]) != 4 ||
      removal.Payload.Span[4] != 0 ||
      placement.MessageId != TerrariaMessageId.TileEntityPlacement ||
      placement.Payload.Length != 5 ||
      BitConverter.ToInt16(placement.Payload.Span[..2]) != 12 ||
      BitConverter.ToInt16(placement.Payload.Span[2..4]) != 14 ||
      placement.Payload.Span[4] != 0)
  {
    throw new InvalidOperationException("Training Dummy message 86/87 projection was invalid.");
  }

  Console.WriteLine("PASS: Training Dummy V1456 message 86/87 projection preserves wire fields");
}

static void VerifyTrainingDummyReplicationCursor()
{
  using SessionReplicationState state = new((_, _) => Task.CompletedTask);
  TileEntityPersistentState first = new(4, 0, 12, 14, [9, 0], isOpaque: false);
  TileEntityPersistentState same = new(4, 0, 12, 14, [9, 0], isOpaque: false);
  TileEntityPersistentState changed = new(4, 0, 12, 14, [0xFF, 0xFF], isOpaque: false);
  if (!state.ShouldSendTileEntity(first))
  {
    throw new InvalidOperationException("TileEntity cursor suppressed an initial state.");
  }

  state.MarkTileEntitySent(first);
  if (state.ShouldSendTileEntity(same) || !state.WasTileEntitySent(4) ||
      !state.ShouldSendTileEntity(changed))
  {
    throw new InvalidOperationException("TileEntity cursor did not track payload revisions.");
  }

  state.MarkTileEntityRemoved(4);
  if (state.WasTileEntitySent(4))
  {
    throw new InvalidOperationException("TileEntity cursor did not clear removed state.");
  }

  Console.WriteLine("PASS: Training Dummy replication cursor tracks visible state changes and removal");
}

static void VerifyTrainingDummyPlacementRemovalOwner()
{
  WorldGrid grid = new(400, 300);
  WorldMetadata metadata = new("training-dummy-placement", new WorldSeed(3), 400, 300);
  _ = grid.TrySetTile(20, 20, new WorldTile(true, 378, FrameX: 0, FrameY: 0));
  using DomeSimulation simulation = new(new DomeSimulationSnapshot(
    grid.CreateSnapshot(metadata),
    [],
    [],
    tickNumber: 0));
  if (!simulation.TryPlaceTrainingDummy(20, 20, out int entityId) ||
      entityId <= 0 ||
      simulation.TryPlaceTrainingDummy(20, 20, out _))
  {
    throw new InvalidOperationException("Training Dummy placement owner accepted an invalid duplicate.");
  }

  TileEntityPersistentState placed = simulation.CreateTileEntitySnapshots().Single();
  if (placed.Id != entityId || placed.Type != 0 || placed.Payload[0] != 0xFF ||
      placed.Payload[1] != 0xFF)
  {
    throw new InvalidOperationException("Training Dummy placement owner did not create typed state.");
  }

  if (!simulation.TryRemoveTrainingDummy(20, 20) ||
      simulation.CreateTileEntitySnapshots().Count != 0 ||
      simulation.TryRemoveTrainingDummy(20, 20))
  {
    throw new InvalidOperationException("Training Dummy removal owner was not idempotently guarded.");
  }

  Console.WriteLine("PASS: Training Dummy placement/removal owner is typed and tile-validity guarded");
}

using DomeSimulation chestMutationSimulation = new(new WorldGrid(400, 300));
PlayerHandle chestMutationPlayer = chestMutationSimulation.CreatePlayer(
  new SimulationVector(100.0f, 100.0f));
int indexedChestId = chestMutationSimulation.CreateChest(100, 100);
if (!chestMutationSimulation.TryGetChestIdAt(100, 100, out int foundChestId) ||
    foundChestId != indexedChestId ||
    chestMutationSimulation.TryDestroyChest(indexedChestId + 1))
{
  throw new InvalidOperationException("Chest coordinate indexing accepted an unknown destroy.");
}

bool rejectedDuplicateCoordinate = false;
try
{
  _ = chestMutationSimulation.CreateChest(100, 100);
}
catch (InvalidOperationException)
{
  rejectedDuplicateCoordinate = true;
}

chestMutationSimulation.SetChestItem(indexedChestId, 0, new ItemStack(1, 1));
if (!rejectedDuplicateCoordinate || chestMutationSimulation.TryDestroyChest(indexedChestId))
{
  throw new InvalidOperationException("Chest mutation accepted a duplicate or nonempty removal.");
}

chestMutationSimulation.SetChestItem(indexedChestId, 0, ItemStack.Empty);
if (!chestMutationSimulation.TryOpenChest(
      indexedChestId,
      chestMutationPlayer,
      new SimulationVector(100.0f, 100.0f)) ||
    chestMutationSimulation.TryDestroyChest(indexedChestId))
{
  throw new InvalidOperationException("Chest mutation removed an opened chest.");
}

chestMutationSimulation.CloseChest(chestMutationPlayer);
if (!chestMutationSimulation.TryDestroyChest(indexedChestId) ||
    chestMutationSimulation.TryGetChestIdAt(100, 100, out _) ||
    chestMutationSimulation.CreateChest(100, 100) == indexedChestId)
{
  throw new InvalidOperationException("Chest removal did not release coordinate ownership.");
}

Console.WriteLine("PASS: world chest placement, indexing and destruction are authoritative");

ChestOpenIntent intent = new(2, chestId, 12, 10);
TerrariaFrame openFrame = TerrariaFrameCodec.Decode(TerrariaPacketCodec.EncodeChestOpen(intent));
if (TerrariaPacketCodec.DecodeChestOpen(TerrariaPacketCodec.EncodeChestOpen(intent)) != intent ||
    openFrame.MessageId != TerrariaMessageId.RequestChestOpen)
{
  throw new InvalidOperationException("Typed V1456 RequestChestOpen did not round-trip.");
}

byte[] chestItemFrameBytes = TerrariaPacketCodec.EncodeChestItem(
  new ChestItemReplicationSnapshot(chestId, 0, new ItemStack(1, 3), 2, 4));
TerrariaFrame itemFrame = TerrariaFrameCodec.Decode(chestItemFrameBytes);
byte[] expectedChestItemFrame = [0x0B, 0x00, 0x20, 0x01, 0x00, 0x00, 0x03, 0x00, 0x00,
  0x01, 0x00];
if (itemFrame.MessageId != TerrariaMessageId.SyncChestItem ||
    !chestItemFrameBytes.SequenceEqual(expectedChestItemFrame))
{
  throw new InvalidOperationException(
    "SyncChestItem did not use the Version4 chest, slot, stack, prefix and item-type layout.");
}

Console.WriteLine("PASS: typed V1456 chest request and item projection are backed");

byte[] chestSizeFrameBytes = TerrariaPacketCodec.EncodeChestSize(chestId, 40);
TerrariaFrame chestSizeFrame = TerrariaFrameCodec.Decode(chestSizeFrameBytes);
byte[] expectedChestSizeFrame = [0x07, 0x00, 0x9B, 0x01, 0x00, 0x28, 0x00];
if (chestSizeFrame.MessageId != TerrariaMessageId.SyncChestSize ||
    !chestSizeFrameBytes.SequenceEqual(expectedChestSizeFrame))
{
  throw new InvalidOperationException(
    "SyncChestSize did not use the Version4 chest identity and slot-count layout.");
}

Console.WriteLine("PASS: typed V1456 chest size projection is backed");

ChestTransferIntent transferIntent = new(2, chestId, 0, 0, true);
if (TerrariaPacketCodec.DecodeChestTransfer(TerrariaPacketCodec.EncodeChestTransfer(transferIntent)) !=
    transferIntent)
{
  throw new InvalidOperationException("Typed V1456 chest transfer did not round-trip.");
}

Console.WriteLine("PASS: chest item transfer is server-owned and typed");

int signId = simulation.CreateSign(14, 10, "Initial");
if (!simulation.TryUpdateSign(signId, new SimulationVector(10.0f, 10.0f), "Updated") ||
    simulation.TryUpdateSign(signId, new SimulationVector(100.0f, 10.0f), "Forged"))
{
  throw new InvalidOperationException("Sign range validation was not authoritative.");
}

SignSnapshot sign = simulation.CreateSignSnapshots().Single();
if (sign.Text != "Updated" || sign.Revision != 2 || sign.Section != new WorldSectionCoordinates(0, 0))
{
  throw new InvalidOperationException("Sign state did not preserve authoritative revision and section data.");
}

SignUpdateIntent signIntent = new(2, signId, 14, 10, "Updated");
if (TerrariaPacketCodec.DecodeSignUpdate(TerrariaPacketCodec.EncodeSignUpdate(signIntent)) != signIntent ||
    TerrariaFrameCodec.Decode(TerrariaPacketCodec.EncodeSignState(
      new SignReplicationSnapshot(signId, 14, 10, "Updated", byte.MaxValue, true, 2))).MessageId !=
      TerrariaMessageId.OpenSignResponse)
{
  throw new InvalidOperationException("Typed V1456 sign request and response were not backed.");
}

Console.WriteLine("PASS: section-local sign state, revision and V1456 projection are authoritative");

using DomeSimulation signCapacitySimulation = new(new WorldGrid(400, 300));
const int maximumV1456SignCount = 32000;
for (int index = 0; index < maximumV1456SignCount; index++)
{
  int createdSignId = signCapacitySimulation.CreateSign(14, 10, "Capacity");
  if (createdSignId != index)
  {
    throw new InvalidOperationException("Sign identifiers did not retain the V1456 zero-based range.");
  }
}

bool rejectedCapacityOverflow = false;
try
{
  _ = signCapacitySimulation.CreateSign(14, 10, "Overflow");
}
catch (InvalidOperationException)
{
  rejectedCapacityOverflow = true;
}

if (!rejectedCapacityOverflow ||
    signCapacitySimulation.CreateSignSnapshots().Count != maximumV1456SignCount)
{
  throw new InvalidOperationException("The V1456 sign capacity was not enforced.");
}

Console.WriteLine("PASS: sign identifiers preserve the V1456 capacity and wire range");

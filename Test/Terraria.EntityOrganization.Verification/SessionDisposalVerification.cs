using EntityEcs;
using Terraria.Items;
using Terraria.NonAuthoritative.Persistence;
using Terraria.Npc;
using Terraria.Relationships;
using Terraria.WorldStorage;
using StorageNpcSlot = Terraria.WorldStorage.NpcSlot;

internal static class SessionDisposalVerification
{
  public static void Run()
  {
    var session = new LoadedWorldSession();
    EntityRuntime runtime = session.EntityRuntime;
    EntityRuntimeId tileEntityRuntimeId = session.WorldRuntimeId;
    WorldStorageRoot storage = session.Storage;

    RuntimeEntityHandle npcHandle = runtime.CreateEntity();
    Require(
      runtime.TryAttach(
        npcHandle,
        new NpcEntityIdentityComponent(new NpcInstanceId(1), new Terraria.Npc.NpcSlot(0))),
      "The fixture NPC root must accept its identity component.");
    Require(runtime.TryPublishEntity(npcHandle), "The fixture NPC root must publish.");
    Require(
      runtime.TryGetReference(npcHandle, EntityReferenceScope.Npc, out EntityReference npcReference),
      "The fixture NPC root must expose a scoped reference.");

    var savedNpc = new WorldNpcState(
      netId: 1,
      legacyTypeName: null,
      isTownNpc: false,
      name: "session-dispose-probe",
      x: 16.0f,
      y: 32.0f,
      homeless: false,
      home: default,
      variation: null,
      homelessDespawn: false);
    Require(
      storage.Npcs.TryAllocate(savedNpc, out StorageNpcSlot npcSlot, out uint npcGeneration),
      "The fixture must register an NPC slot projection.");

    var projectileIdentity = new OwnerProjectileIdentity(new PlayerSlot(0), 23);
    var projectileHandle = new ProjectileHandle(new ProjectileSlot(4), 1);
    Require(
      storage.ProjectileIdentities.TryRegister(projectileIdentity, projectileHandle),
      "The fixture must register the Projectile identity projection.");

    var tileEntityId = new TileEntityId(7);
    var tileEntityAnchor = new TileCoordinate(12, 16);
    TileEntityStore tileEntities = storage.TileEntities;
    var tileSnapshot = new TileEntitySnapshot(
      tileEntityId,
      new TileEntityTypeId(2),
      tileEntityAnchor,
      Array.Empty<ItemState>(),
      logicOn: true);
    var tileEntitySnapshot = new TileEntityStoreSnapshot(new[] { tileSnapshot }, nextId: 8);
    tileEntities.CommitRuntimeSnapshot(tileEntitySnapshot);
    Require(tileEntities.Count == 1, "The fixture TileEntity root must exist.");
    Require(tileEntities.TryGetEntityReference(tileEntityId, out EntityReference tileEntityReference),
      "The fixture TileEntity root must expose a runtime reference.");
    Require(storage.TileEntityUpdates.Count == 1,
      "The fixture TileEntity schedule projection must exist.");

    int entityCountBeforeRejectedDispose = runtime.EntityCount;
    bool completeBeforeRejectedDispose = session.IsComplete;
    bool publishedBeforeRejectedDispose = session.IsPublished;
    bool publicationUncertainBeforeRejectedDispose = session.IsPublicationUncertain;
    bool npcInspectCallbackRan = false;
    bool npcIdentityWasAssigned = false;
    bool borrowRejectedDispose = false;
    try
    {
      _ = runtime.TryInspect<NpcEntityIdentityComponent>(
        npcHandle,
        (in NpcEntityIdentityComponent npcIdentity) =>
        {
          npcInspectCallbackRan = true;
          npcIdentityWasAssigned = npcIdentity.InstanceId.IsValid;
          session.Dispose();
        });
    }
    catch (InvalidOperationException)
    {
      borrowRejectedDispose = true;
    }

    Require(borrowRejectedDispose,
      "Session disposal must reject before projection cleanup while an NPC component is borrowed.");
    Require(npcInspectCallbackRan && npcIdentityWasAssigned,
      "The disposal rejection must occur inside an inspected live NPC component borrow.");
    Require(!session.IsDisposed, "A rejected session dispose must preserve session liveness.");
    Require(session.IsComplete == completeBeforeRejectedDispose &&
        session.IsPublished == publishedBeforeRejectedDispose &&
        session.IsPublicationUncertain == publicationUncertainBeforeRejectedDispose,
      "A rejected session dispose must preserve publication flags.");
    Require(runtime.EntityCount == entityCountBeforeRejectedDispose,
      "A rejected session dispose must preserve every runtime root.");
    Require(runtime.TryGetStatus(npcHandle, out EntityRuntimeStatus npcStatus) &&
        npcStatus == EntityRuntimeStatus.Running &&
        runtime.TryResolve(npcReference, out RuntimeEntityHandle resolvedNpc) &&
        resolvedNpc == npcHandle,
      "A rejected session dispose must preserve the borrowed NPC root and reference.");
    Require(storage.Npcs.TryGet(npcSlot, npcGeneration, out WorldEntityState? resolvedSavedNpc) &&
        ReferenceEquals(savedNpc, resolvedSavedNpc),
      "A rejected session dispose must preserve the NPC slot projection.");
    Require(storage.ProjectileIdentities.Count == 1 &&
        storage.ProjectileIdentities.TryGetHandle(projectileIdentity, out ProjectileHandle resolvedProjectile) &&
        resolvedProjectile == projectileHandle,
      "A rejected session dispose must preserve both Projectile identity indexes.");
    Require(tileEntities.Count == 1 &&
        tileEntities.TryGetSnapshot(tileEntityId, out _) &&
        storage.TileEntityUpdates.Count == 1 &&
        storage.TileEntityUpdates.IsScheduled(tileEntityId),
      "A rejected session dispose must preserve TileEntity roots and scheduled IDs.");

    session.Dispose();
    Require(session.IsDisposed, "A released borrow must allow session disposal.");
    Require(runtime.EntityCount == 0,
      "Successful session disposal must remove all NPC and TileEntity runtime roots.");
    int entityCountAfterDisposal = runtime.EntityCount;
    bool disposedTileEntityStoreRejectsAccess =
      ThrowsObjectDisposed(() => _ = tileEntities.Count);
    disposedTileEntityStoreRejectsAccess &=
      ThrowsObjectDisposed(() => _ = tileEntities.NextId);
    disposedTileEntityStoreRejectsAccess &=
      ThrowsObjectDisposed(() => _ = tileEntities.MutationRevision);
    disposedTileEntityStoreRejectsAccess &=
      ThrowsObjectDisposed(() => _ = tileEntities.CreateSnapshot());
    disposedTileEntityStoreRejectsAccess &=
      ThrowsObjectDisposed(() => _ = tileEntities.CreateScheduledIdSnapshot());
    disposedTileEntityStoreRejectsAccess &=
      ThrowsObjectDisposed(() => _ = tileEntities.TryGetSnapshot(tileEntityId, out _));
    disposedTileEntityStoreRejectsAccess &=
      ThrowsObjectDisposed(() => _ = tileEntities.TryGetEntityReference(
        tileEntityId,
        out _));
    disposedTileEntityStoreRejectsAccess &=
      ThrowsObjectDisposed(() => _ = tileEntities.TryGetEntityReferenceByAnchor(
        tileEntityAnchor,
        out _));
    disposedTileEntityStoreRejectsAccess &=
      ThrowsObjectDisposed(() => _ = tileEntities.TryGetRuntimeState(tileEntityId, out _));
    disposedTileEntityStoreRejectsAccess &=
      ThrowsObjectDisposed(() => _ = tileEntities.TryGetLogicSensorCountedData(
        tileEntityId,
        out _));
    disposedTileEntityStoreRejectsAccess &=
      ThrowsObjectDisposed(() => _ = tileEntities.CommitLogicSensorState(
        tileEntityId,
        isOn: false,
        countedData: 0));
    disposedTileEntityStoreRejectsAccess &=
      ThrowsObjectDisposed(() => _ = tileEntities.CommitTrainingDummyNpcIndex(
        tileEntityId,
        npcIndex: -1));
    disposedTileEntityStoreRejectsAccess &=
      ThrowsObjectDisposed(() => _ = tileEntities.Remove(tileEntityId));
    bool disposedTileEntityStoreRejectsReplacement = ThrowsObjectDisposed(() =>
      tileEntities.CommitRuntimeSnapshot(tileEntitySnapshot));
    Require(disposedTileEntityStoreRejectsReplacement &&
        runtime.EntityCount == entityCountAfterDisposal &&
        entityCountAfterDisposal == 0,
      "A captured disposed TileEntity store must not recreate runtime roots.");
    Require(disposedTileEntityStoreRejectsAccess,
      "A disposed TileEntity store must reject all public reads and writes.");
    Require(
      ThrowsObjectDisposed(() => storage.Npcs.TryGet(npcSlot, npcGeneration, out _)) &&
      ThrowsObjectDisposed(() => _ = storage.ProjectileIdentities.Count) &&
      ThrowsObjectDisposed(() => _ = storage.TileEntityUpdates.Count) &&
      ThrowsObjectDisposed(() => _ = storage.TileMap.Width),
      "Successful session disposal must reject access through captured storage projections.");
    Require(tileEntityReference.EntityId.Value != Guid.Empty &&
        tileEntityReference.RuntimeId == tileEntityRuntimeId &&
        ThrowsObjectDisposed(() => _ = runtime.TryResolve(tileEntityReference, out _)),
      "The captured TileEntity reference must retain its value while the disposed runtime rejects resolution.");

    session.Dispose();
    Require(session.IsDisposed && runtime.EntityCount == 0,
      "Repeated session disposal must remain idempotent.");
    Require(ThrowsObjectDisposed(() => _ = session.Storage),
      "A disposed session must reject access to its storage root.");
  }

  private static bool ThrowsObjectDisposed(Action action)
  {
    try
    {
      action.Invoke();
      return false;
    }
    catch (ObjectDisposedException)
    {
      return true;
    }
  }

  private static void Require(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }
}

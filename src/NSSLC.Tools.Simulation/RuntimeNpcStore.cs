using System.Numerics;
using EntityEcs;
using EntityEcs.Components;
using Terraria.Content;
using Terraria.Combat;
using Terraria.Npc;
using Terraria.NonAuthoritative.Persistence;
using Terraria.Player.Movement;
using Terraria.Projectile;
using Terraria.SpatialSimulation.Components;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Systems;
using Terraria.WorldStorage;
using Terraria.Relationships;
using Terraria.NonAuthoritative.WorldStorage;
using NSSLC.WorldGeneration;
using NSSLC.WorldGeneration.ID;
using LegacyVector2 = NSSLC.WorldGeneration.Geometry.Vector2;
using NpcRuntimeSlot = Terraria.Npc.NpcSlot;
using SavedNpcSlot = Terraria.WorldStorage.NpcSlot;
using WorldSessionSecretSeedFlags = Terraria.WorldSession.Components.WorldSecretSeedFlags;

namespace Terraria.NonAuthoritative.SimulationHost;

internal sealed class RuntimeNpcStore
{
  internal readonly record struct SpawnSnapshot(
    int SlotIndex,
    Vector2 Center,
    bool IsTownNpc,
    bool IsHostile);

  internal readonly record struct NpcParentRelationBinding(
    EntityReference ParentReference,
    EntityReference ChildReference,
    NpcInstanceId ParentInstanceId,
    NpcInstanceId ChildInstanceId,
    int ParentSlot,
    int ChildSlot,
    long AttachedAtTick);

  internal readonly record struct NpcRelationChainBinding(
    IReadOnlyList<EntityReference> References,
    IReadOnlyList<NpcInstanceId> InstanceIds,
    IReadOnlyList<int> Slots,
    long AttachedAtTick);

  internal readonly record struct NpcDeathDropSnapshot(int NetId, Vector2 Position);

  internal const int MaximumNpcCapacity = 200;
  private const float NaturalDespawnRangePixels = 3000f;
  internal const int NaturalDespawnGraceTicks = 300;
  private const int LegacyHousingRepairExcludedNpcType = 368;
  private const int LegacyOldManNpcType = 37;
  private EntityIdentityRegistry _identityRegistry;
  private EntityRuntime _entityRuntime;
  private bool _ownsEntityRuntime;
  private LoadedWorldSession? _session;
  private ContentCatalog? _contentCatalog;
  private Dictionary<NpcInstanceId, RuntimeEntityHandle> _handlesByInstanceId = new();
  private EntitySlotStore<RuntimeNpcEntity, NpcRuntimeSlot> _entities = CreateEntityStore();
  private readonly ProjectileStaticNpcImmunityRegistryComponent _projectileStaticNpcImmunity;
  private readonly uint[] _slotGenerations = new uint[MaximumNpcCapacity];
  private EntitySlotStore<WorldEntityState, ProjectileSlot>? _projectileEntityStore;
  private readonly NpcDamageTrackingSystem _damageTracking = new(static (_, _) => null);
  private readonly NpcCombatSystem _combatSystem;
  private readonly NpcMovementSystem _movementSystem = new();
  private readonly NpcAiSystem _aiSystem = new();
  private readonly Random _npcAiRandom = new(0x4E504341);
  private bool _isDisposed;

  public RuntimeNpcStore(
    ProjectileStaticNpcImmunityRegistryComponent projectileStaticNpcImmunity,
    EntityIdentityRegistry? identityRegistry = null)
  {
    _projectileStaticNpcImmunity = projectileStaticNpcImmunity ??
      throw new ArgumentNullException(nameof(projectileStaticNpcImmunity));
    _identityRegistry = identityRegistry ?? new EntityIdentityRegistry();
    _entityRuntime = new EntityRuntime(_identityRegistry);
    _ownsEntityRuntime = true;
    _combatSystem = new NpcCombatSystem(
      _damageTracking,
      new NpcDeathLifecycleSystem(),
      new SilentDamageOverTimeTextPort());
  }

  public int ActiveCount => _entities.ActiveCount;

  private bool HasLifeCrystalSlime()
  {
    for (int index = 0; index < _entities.Capacity; index++)
    {
      if (_entities.TryGetOccupiedAt(index, out _, out _, out RuntimeNpcEntity? npc) &&
          npc is not null &&
          npc.IsActive &&
          npc.Definition.TypeId == 1 &&
          npc.CaptureAiState().State1 == 29f)
      {
        return true;
      }
    }

    return false;
  }

  public int DespawnedCount { get; private set; }

  internal EntityIdentityRegistry IdentityRegistry => _identityRegistry;

  public bool TryGetEntityReference(NpcInstanceId instanceId, out EntityReference reference)
  {
    if (!_isDisposed &&
        _handlesByInstanceId.TryGetValue(instanceId, out RuntimeEntityHandle handle) &&
        _entityRuntime.TryGetReference(handle, EntityReferenceScope.Npc, out reference))
    {
      return true;
    }

    reference = EntityReference.None;
    return false;
  }

  public bool TryResolveEntityReference(EntityReference reference, out RuntimeEntityHandle handle)
  {
    if (!_isDisposed && reference.Scope == EntityReferenceScope.Npc &&
        _entityRuntime.TryResolve(reference, out handle))
    {
      return true;
    }

    handle = default;
    return false;
  }

  public void Hydrate(LoadedWorldSession session, ContentCatalog catalog)
  {
    ThrowIfDisposed();
    ArgumentNullException.ThrowIfNull(session);
    ArgumentNullException.ThrowIfNull(catalog);
    if (!session.IsComplete || session.IsPublished)
    {
      throw new InvalidOperationException(
        "Runtime NPC hydration requires a complete, unpublished load candidate.");
    }

    int worldWidth = session.World.Descriptor.SizeX;
    int worldHeight = session.World.Descriptor.SizeY;
    if (worldWidth <= 0 || worldHeight <= 0 || Main.tile is null ||
        Main.tile.GetLength(0) != worldWidth || Main.tile.GetLength(1) != worldHeight)
    {
      throw new InvalidDataException(
        "The published runtime tile map does not match the loaded world's dimensions.");
    }

    EntityRuntime candidateRuntime = session.EntityRuntime;
    var candidateRootHandles = new List<RuntimeEntityHandle>();
    var candidateHandles = new Dictionary<NpcInstanceId, RuntimeEntityHandle>();
    bool previousDomainRetired = false;
    bool candidateCommitted = false;
    try
    {
      EntitySlotStore<RuntimeNpcEntity, NpcRuntimeSlot> candidateEntities = CreateEntityStore();
      var candidateGenerations = (uint[])_slotGenerations.Clone();
      var repairedNpcStates = new List<(NpcRuntimeSlot Slot, uint Generation, WorldNpcState State)>();
      var residentsToMarkHomeless = new List<TownHousingResidentKey>();
      var residentsAlreadyKickedOut = new HashSet<TownHousingResidentKey>();
      for (int index = 0; index < session.Storage.Npcs.Capacity; index++)
      {
        if (!session.Storage.Npcs.TryGetOccupiedAt(
              index,
              out SavedNpcSlot savedSlot,
              out uint sessionGeneration,
              out WorldEntityState? entity) || entity is not WorldNpcState savedNpc)
        {
          continue;
        }

        var slot = new NpcRuntimeSlot(savedSlot.Value);
        bool hasResolvedNetId = savedNpc.NetId.HasValue;
        int netId = savedNpc.NetId.GetValueOrDefault();
        if (!hasResolvedNetId && savedNpc.LegacyTypeName is string legacyTypeName)
        {
          hasResolvedNetId = SimulationLegacyNpcTypeNameResolver.TryResolveNetId(
            legacyTypeName,
            out netId);
        }

        if (!hasResolvedNetId ||
            !catalog.Npcs.TryGetByNetId(netId, out NpcDefinition definition))
        {
          throw new InvalidDataException(
            $"Saved NPC slot {slot.Value} has no supported definition for net id " +
            $"{(hasResolvedNetId ? netId.ToString() : "<unresolved>")} " +
            $"or legacy type name '{savedNpc.LegacyTypeName ?? "<missing>"}'.");
        }

        if (definition.Town.IsTownNpc != savedNpc.IsTownNpc)
        {
          throw new InvalidDataException(
            $"Saved NPC {netId} town identity does not match its content definition.");
        }

        WorldNpcState runtimeSavedNpc = RepairLoadedHousing(
          savedNpc,
          definition.TypeId,
          session,
          worldWidth,
          worldHeight,
          residentsAlreadyKickedOut,
          out bool markHomeless);
        if (!ReferenceEquals(runtimeSavedNpc, savedNpc))
        {
          repairedNpcStates.Add((slot, sessionGeneration, runtimeSavedNpc));
        }
        if (markHomeless)
        {
          TownHousingResidentKey resident = new(definition.TypeId);
          residentsToMarkHomeless.Add(resident);
          residentsAlreadyKickedOut.Add(resident);
        }

        RuntimeNpcEntity runtimeNpc = CreateRuntimeNpc(
          candidateRuntime,
          slot,
          slotGeneration: 0,
          runtimeSavedNpc,
          definition,
          checked((int)catalog.CatalogRevision));
        candidateRootHandles.Add(runtimeNpc.RuntimeHandle);
        if (!candidateEntities.TryAllocateAt(slot, runtimeNpc, out _))
        {
          throw new InvalidDataException(
            $"Runtime NPC slot {slot.Value} could not be restored without slot aliasing.");
        }

        if (!candidateHandles.TryAdd(runtimeNpc.InstanceId, runtimeNpc.RuntimeHandle))
        {
          throw new InvalidDataException("Loaded NPC instance identity was registered more than once.");
        }

      }

      foreach ((NpcRuntimeSlot slot, uint generation, WorldNpcState state) in repairedNpcStates)
      {
        var savedSlot = new SavedNpcSlot(slot.Value);
        if (!session.Storage.Npcs.TryReplace(savedSlot, generation, state, out _))
        {
          throw new InvalidDataException(
            $"Repaired NPC slot {slot.Value} changed before the load candidate was committed.");
        }
      }

      foreach (TownHousingResidentKey resident in residentsToMarkHomeless)
      {
        TownHousingRegistrySystem.MarkHomeless(session.TownHousing, resident);
      }

      EnsureRuntimeEntitiesReadyForTermination(
        _entityRuntime,
        _handlesByInstanceId.Values,
        skipBecauseRuntimeDisposed: _session?.IsDisposed == true);
      RemoveOwnedRuntimeEntities(
        _entityRuntime,
        _handlesByInstanceId.Values,
        skipBecauseRuntimeDisposed: _session?.IsDisposed == true);
      ReleaseOccupiedNpcSlots();
      previousDomainRetired = true;
      if (_ownsEntityRuntime && !ReferenceEquals(_entityRuntime, candidateRuntime))
      {
        _entityRuntime.Dispose();
      }

      for (int index = 0; index < candidateEntities.Capacity; index++)
      {
        if (!candidateEntities.TryGetOccupiedAt(
              index,
              out _,
              out _,
              out RuntimeNpcEntity? candidateNpc) || candidateNpc is null)
        {
          continue;
        }

        if (!_entities.TryAllocateAt(
              candidateNpc.Slot,
              candidateNpc,
              out uint runtimeGeneration))
        {
          throw new InvalidDataException(
            $"Runtime NPC slot {candidateNpc.Slot.Value} could not be committed without aliasing.");
        }

        candidateNpc.SlotGeneration = runtimeGeneration;
        candidateGenerations[candidateNpc.Slot.Value] = runtimeGeneration;
        _slotGenerations[candidateNpc.Slot.Value] = runtimeGeneration;
      }

      _identityRegistry = session.IdentityRegistry;
      _entityRuntime = candidateRuntime;
      _ownsEntityRuntime = false;
      _session = session;
      _handlesByInstanceId = candidateHandles;
      Array.Copy(candidateGenerations, _slotGenerations, MaximumNpcCapacity);
      _projectileEntityStore = session.Storage.Projectiles;
      DespawnedCount = 0;
      _damageTracking.Reset();
      candidateCommitted = true;
    }
    finally
    {
      if (!candidateCommitted)
      {
        ReleaseCandidateNpcSlots(candidateHandles.Keys);
        RemoveOwnedRuntimeEntities(
          candidateRuntime,
          candidateRootHandles,
          endReason: NpcTaskEndReason.Removal);
        if (previousDomainRetired)
        {
          _handlesByInstanceId.Clear();
          _projectileEntityStore = null;
        }
      }
    }
  }

  private static WorldNpcState RepairLoadedHousing(
    WorldNpcState savedNpc,
    int npcType,
    LoadedWorldSession session,
    int worldWidth,
    int worldHeight,
    HashSet<TownHousingResidentKey> residentsAlreadyKickedOut,
    out bool markHomeless)
  {
    markHomeless = false;
    if (!savedNpc.IsTownNpc || savedNpc.Homeless ||
        npcType is LegacyHousingRepairExcludedNpcType or LegacyOldManNpcType)
    {
      return savedNpc;
    }

    if (WorldGen.IsHousingRoomValidAt(
          savedNpc.Home.X,
          savedNpc.Home.Y - 1,
          worldWidth,
          worldHeight))
    {
      return savedNpc;
    }

    TownHousingResidentKey resident = new(npcType);
    TileCoordinate repairedHome = savedNpc.Home;
    if (!residentsAlreadyKickedOut.Contains(resident) &&
        TownHousingRegistrySystem.TryGetRoom(session.TownHousing, resident, out TilePosition room))
    {
      repairedHome = new TileCoordinate(room.X, room.Y);
      if (WorldGen.IsHousingRoomValidAt(
            room.X,
            room.Y - 1,
            worldWidth,
            worldHeight))
      {
        return WithHousing(savedNpc, homeless: false, repairedHome);
      }
    }

    markHomeless = true;
    return WithHousing(savedNpc, homeless: true, repairedHome);
  }

  private static WorldNpcState WithHousing(
    WorldNpcState npc,
    bool homeless,
    TileCoordinate home)
  {
    return new WorldNpcState(
      npc.NetId,
      npc.LegacyTypeName,
      npc.IsTownNpc,
      npc.Name,
      npc.X,
      npc.Y,
      homeless,
      home,
      npc.Variation,
      npc.HomelessDespawn);
  }

  public void Reset()
  {
    ThrowIfDisposed();
    EnsureRuntimeEntitiesReadyForTermination(
      _entityRuntime,
      _handlesByInstanceId.Values,
      skipBecauseRuntimeDisposed: _session?.IsDisposed == true);
    for (int npcSlot = 0; npcSlot < MaximumNpcCapacity; npcSlot++)
    {
      ProjectileStaticNpcImmunitySystem.ClearStaticNpcSlot(
        _projectileStaticNpcImmunity,
        npcSlot);
    }

    _projectileEntityStore = null;
    RemoveOwnedRuntimeEntities(
      _entityRuntime,
      _handlesByInstanceId.Values,
      skipBecauseRuntimeDisposed: _session?.IsDisposed == true);
    ReleaseOccupiedNpcSlots();
    if (_ownsEntityRuntime)
    {
      _entityRuntime.Dispose();
      _entityRuntime = new EntityRuntime(_identityRegistry);
    }

    _handlesByInstanceId.Clear();
    _session = null;
    DespawnedCount = 0;
    _damageTracking.Reset();
  }

  public void Dispose()
  {
    if (_isDisposed)
    {
      return;
    }

    EnsureRuntimeEntitiesReadyForTermination(
      _entityRuntime,
      _handlesByInstanceId.Values,
      skipBecauseRuntimeDisposed: _session?.IsDisposed == true);
    for (int npcSlot = 0; npcSlot < MaximumNpcCapacity; npcSlot++)
    {
      ProjectileStaticNpcImmunitySystem.ClearStaticNpcSlot(
        _projectileStaticNpcImmunity,
        npcSlot);
    }

    RemoveOwnedRuntimeEntities(
      _entityRuntime,
      _handlesByInstanceId.Values,
      skipBecauseRuntimeDisposed: _session?.IsDisposed == true);
    ReleaseOccupiedNpcSlots();
    if (_ownsEntityRuntime)
    {
      _entityRuntime.Dispose();
    }

    _handlesByInstanceId.Clear();
    _projectileEntityStore = null;
    _damageTracking.Reset();
    _session = null;
    _isDisposed = true;
  }

  private static void RemoveOwnedRuntimeEntities(
    EntityRuntime runtime,
    IEnumerable<RuntimeEntityHandle> handles,
    bool skipBecauseRuntimeDisposed = false,
    NpcTaskEndReason endReason = NpcTaskEndReason.WorldUnload)
  {
    if (skipBecauseRuntimeDisposed)
    {
      return;
    }

    RuntimeEntityHandle[] ownedHandles = handles.ToArray();
    EnsureRuntimeEntitiesReadyForTermination(runtime, ownedHandles);
    foreach (RuntimeEntityHandle handle in ownedHandles)
    {
      if (!runtime.TryGetStatus(handle, out EntityRuntimeStatus status))
      {
        continue;
      }

      if (status == EntityRuntimeStatus.Running)
      {
        bool ended = false;
        bool edited = runtime.TryEdit(
          handle,
          (ref NpcTaskStateComponent task) =>
          {
            var reference = new NpcTaskReference(handle, task.TaskGeneration);
            ended = NpcTaskLifecycleSystem.Terminate(task, handle, reference, endReason).Accepted;
          });
        if (!edited || !ended || !runtime.TryBeginTermination(handle))
        {
          throw new InvalidOperationException("An NPC task and root could not begin domain cleanup.");
        }
      }

      if (!runtime.TryRemoveEntity(handle))
      {
        throw new InvalidOperationException("An NPC root could not be removed during domain cleanup.");
      }
    }
  }

  private static void EnsureRuntimeEntitiesReadyForTermination(
    EntityRuntime runtime,
    IEnumerable<RuntimeEntityHandle> handles,
    bool skipBecauseRuntimeDisposed = false)
  {
    if (skipBecauseRuntimeDisposed)
    {
      return;
    }

    if (!AreRuntimeEntitiesReadyForTermination(runtime, handles))
    {
      throw new InvalidOperationException(
        "Every affected NPC entity root must be unborrowed before domain cleanup.");
    }
  }

  private static bool AreRuntimeEntitiesReadyForTermination(
    EntityRuntime runtime,
    IEnumerable<RuntimeEntityHandle> handles)
  {
    foreach (RuntimeEntityHandle handle in handles.ToArray())
    {
      if (runtime.TryGetStatus(handle, out _) && !runtime.IsReadyForTermination(handle))
      {
        return false;
      }
    }

    return true;
  }

  private void ReleaseOccupiedNpcSlots()
  {
    for (int index = 0; index < _entities.Capacity; index++)
    {
      if (!_entities.TryGetOccupiedAt(index, out NpcRuntimeSlot slot, out uint generation, out _))
      {
        continue;
      }

      if (!_entities.TryRelease(slot, generation, out _))
      {
        throw new InvalidOperationException("An NPC compatibility slot could not be released.");
      }
    }
  }

  private void ReleaseCandidateNpcSlots(IEnumerable<NpcInstanceId> candidateInstanceIds)
  {
    HashSet<NpcInstanceId> candidateInstances = candidateInstanceIds.ToHashSet();
    if (candidateInstances.Count == 0)
    {
      return;
    }

    for (int index = 0; index < _entities.Capacity; index++)
    {
      if (!_entities.TryGetOccupiedAt(
            index,
            out NpcRuntimeSlot slot,
            out uint generation,
            out RuntimeNpcEntity? npc) ||
          npc is null || !candidateInstances.Contains(npc.InstanceId))
      {
        continue;
      }

      _ = _entities.TryRelease(slot, generation, out _);
    }
  }

  private static RuntimeNpcEntity CreateRuntimeNpc(
    EntityRuntime runtime,
    NpcRuntimeSlot slot,
    uint slotGeneration,
    WorldNpcState savedState,
    NpcDefinition definition,
    int catalogRevision,
    bool isNaturallySpawned = false)
  {
    RuntimeEntityHandle handle = runtime.CreateEntity();
    try
    {
      RuntimeNpcEntity npc = RuntimeNpcEntity.Hydrate(
        runtime,
        handle,
        slot,
        slotGeneration,
        savedState,
        definition,
        catalogRevision,
        isNaturallySpawned);
      if (!runtime.TryPublishEntity(handle))
      {
        throw new InvalidOperationException("The NPC entity could not be published.");
      }

      return npc;
    }
    catch
    {
      if (runtime.TryGetStatus(handle, out EntityRuntimeStatus status))
      {
        if (status == EntityRuntimeStatus.Running && !runtime.TryBeginTermination(handle))
        {
          throw new InvalidOperationException("A failed NPC creation could not begin identity cleanup.");
        }

        if (!runtime.TryRemoveEntity(handle))
        {
          throw new InvalidOperationException("A failed NPC creation left a live entity identity.");
        }
      }

      throw;
    }
  }

  private void RemoveRuntimeNpcIdentity(RuntimeNpcEntity npc)
  {
    if (!_entityRuntime.TryBeginTermination(npc.RuntimeHandle) ||
        !_entityRuntime.TryRemoveEntity(npc.RuntimeHandle))
    {
      throw new InvalidOperationException("A rejected NPC slot allocation left an active entity identity.");
    }
  }

  private void ThrowIfDisposed()
  {
    ObjectDisposedException.ThrowIf(_isDisposed, this);
  }

  public bool TrySpawn(
    int netId,
    System.Numerics.Vector2 position,
    ContentCatalog catalog,
    int worldId,
    out RuntimeNpcEntity? spawnedNpc,
    bool isNaturallySpawned = false)
  {
    ThrowIfDisposed();
    ArgumentNullException.ThrowIfNull(catalog);
    if (!NpcAiAuthorityGate.IsAuthoritative(Main.netMode))
    {
      spawnedNpc = null;
      return false;
    }

    _contentCatalog = catalog;
    if (!catalog.Npcs.TryGetByNetId(netId, out NpcDefinition definition))
    {
      spawnedNpc = null;
      return false;
    }

    for (int slotValue = 0; slotValue < MaximumNpcCapacity; slotValue++)
    {
      if (_entities.TryGetOccupiedAt(slotValue, out _, out _, out _))
      {
        continue;
      }

      if (_slotGenerations[slotValue] == uint.MaxValue)
      {
        continue;
      }
      uint nextGeneration = _slotGenerations[slotValue] + 1;

      var slot = new NpcRuntimeSlot(slotValue);
      var savedState = new WorldNpcState(
        definition.NetId,
        legacyTypeName: null,
        isTownNpc: definition.Town.IsTownNpc,
        $"Simulation NPC {netId}",
        position.X,
        position.Y,
        homeless: true,
        new TileCoordinate(-1, -1),
        variation: null,
        homelessDespawn: false);
      RuntimeNpcEntity candidate = CreateRuntimeNpc(
        _entityRuntime,
        slot,
        nextGeneration,
        savedState,
        definition,
        checked((int)catalog.CatalogRevision),
        isNaturallySpawned);
      if (!_entities.TryAllocateAt(slot, candidate, out uint allocatedGeneration))
      {
        RemoveRuntimeNpcIdentity(candidate);
        continue;
      }

      if (allocatedGeneration != nextGeneration)
      {
        _ = _entities.TryRelease(slot, allocatedGeneration, out _);
        RemoveRuntimeNpcIdentity(candidate);
        throw new InvalidOperationException(
          "The NPC slot generation changed during owner-thread allocation.");
      }

      if (!_handlesByInstanceId.TryAdd(candidate.InstanceId, candidate.RuntimeHandle))
      {
        _ = _entities.TryRelease(slot, allocatedGeneration, out _);
        RemoveRuntimeNpcIdentity(candidate);
        throw new InvalidOperationException("An NPC instance projection was registered more than once.");
      }

      _slotGenerations[slotValue] = allocatedGeneration;
      spawnedNpc = candidate;
      return true;
    }

    spawnedNpc = null;
    return false;
  }

  public bool TrySpawnTrainingDummy(
    int tileX,
    int tileY,
    ContentCatalog catalog,
    int worldId,
    out RuntimeNpcTrainingDummyBinding binding)
  {
    if (!TrySpawn(
          SimulationContentSupportManifest.TrainingDummyNetId,
          new System.Numerics.Vector2(tileX * 16f, tileY * 16f),
          catalog,
          worldId,
          out RuntimeNpcEntity? spawnedNpc) ||
        spawnedNpc is null)
    {
      binding = default;
      return false;
    }

    spawnedNpc.CommitAiState(new NpcAiStateComponent(
      spawnedNpc.Definition.Spawn.AiStyle,
      tileX,
      tileY,
      0f,
      0f,
      0),
      action: 0);

    if (!TryCreateTrainingDummyBinding(spawnedNpc, tileX, tileY, out binding))
    {
      if (!TryRelease(spawnedNpc))
      {
        throw new InvalidOperationException(
          "A training dummy without a valid root binding could not be released.");
      }

      binding = default;
      return false;
    }

    return true;
  }

  public bool TrySpawnParentChild(
    int parentNetId,
    int childNetId,
    Vector2 position,
    long attachedAtTick,
    ContentCatalog catalog,
    int worldId,
    out NpcParentRelationBinding binding)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    ArgumentOutOfRangeException.ThrowIfNegative(attachedAtTick);

    if (!TrySpawn(
          parentNetId,
          position,
          catalog,
          worldId,
          out RuntimeNpcEntity? parent) ||
        parent is null)
    {
      binding = default;
      return false;
    }

    if (!TrySpawn(
          childNetId,
          position + new Vector2(16f, 0f),
          catalog,
          worldId,
          out RuntimeNpcEntity? child) ||
        child is null)
    {
      _ = TryRelease(parent);
      binding = default;
      return false;
    }

    if (!TryGetEntityReference(parent.InstanceId, out EntityReference parentReference) ||
        !TryGetEntityReference(child.InstanceId, out EntityReference childReference) ||
        !child.TryAttachParentRelation(parent, attachedAtTick))
    {
      _ = TryRelease(child);
      _ = TryRelease(parent);
      binding = default;
      return false;
    }

    binding = new NpcParentRelationBinding(
      parentReference,
      childReference,
      parent.InstanceId,
      child.InstanceId,
      parent.Slot.Value,
      child.Slot.Value,
      attachedAtTick);
    return true;
  }

  public bool TrySpawnParentChildChain(
    IReadOnlyList<int> netIds,
    Vector2 position,
    long attachedAtTick,
    ContentCatalog catalog,
    int worldId,
    out NpcRelationChainBinding binding)
  {
    ArgumentNullException.ThrowIfNull(netIds);
    ArgumentNullException.ThrowIfNull(catalog);
    ArgumentOutOfRangeException.ThrowIfNegative(attachedAtTick);
    if (netIds.Count < 2)
    {
      throw new ArgumentException(
        "A relation chain requires at least a root and one child.",
        nameof(netIds));
    }

    var chain = new List<RuntimeNpcEntity>(netIds.Count);
    bool committed = false;
    try
    {
      for (int index = 0; index < netIds.Count; index++)
      {
        if (!TrySpawn(
              netIds[index],
              position + new Vector2(index * 16f, 0f),
              catalog,
              worldId,
              out RuntimeNpcEntity? entity) ||
            entity is null)
        {
          binding = default;
          return false;
        }

        chain.Add(entity);
        if (index == 0 || entity.TryAttachParentRelation(chain[index - 1], attachedAtTick))
        {
          continue;
        }

        binding = default;
        return false;
      }

      var references = new EntityReference[chain.Count];
      var instanceIds = new NpcInstanceId[chain.Count];
      var slots = new int[chain.Count];
      for (int index = 0; index < chain.Count; index++)
      {
        if (!TryGetEntityReference(chain[index].InstanceId, out EntityReference reference))
        {
          binding = default;
          return false;
        }

        references[index] = reference;
        instanceIds[index] = chain[index].InstanceId;
        slots[index] = chain[index].Slot.Value;
      }

      binding = new NpcRelationChainBinding(
        references,
        instanceIds,
        slots,
        attachedAtTick);
      committed = true;
      return true;
    }
    finally
    {
      if (!committed)
      {
        for (int index = chain.Count - 1; index >= 0; index--)
        {
          RuntimeNpcEntity entity = chain[index];
          if (_entities.TryGet(entity.Slot, entity.SlotGeneration, out RuntimeNpcEntity? current) &&
              ReferenceEquals(current, entity))
          {
            _ = TryRelease(entity);
          }
        }
      }
    }
  }

  public bool TryRelease(RuntimeNpcEntity npc)
  {
    return TryRelease(npc, NpcTaskEndReason.Removal, out _);
  }

  internal bool TryRelease(
    RuntimeNpcEntity npc,
    NpcTaskEndReason endReason,
    out NpcTaskTerminationResult taskTermination)
  {
    taskTermination = default;
    if (_isDisposed)
    {
      return false;
    }

    ArgumentNullException.ThrowIfNull(npc);
    if (!_entities.TryGet(npc.Slot, npc.SlotGeneration, out RuntimeNpcEntity? current) ||
        !ReferenceEquals(current, npc))
    {
      return false;
    }

    RuntimeEntityHandle[] handles = _handlesByInstanceId.Values.ToArray();
    if (!_entityRuntime.TryGetStatus(npc.RuntimeHandle, out _) ||
        !AreRuntimeEntitiesReadyForTermination(_entityRuntime, handles))
    {
      return false;
    }

    NpcInstanceId instanceId = npc.InstanceId;
    if (!_handlesByInstanceId.TryGetValue(instanceId, out RuntimeEntityHandle handle) ||
        handle != npc.RuntimeHandle)
    {
      return false;
    }

    if (endReason is not (NpcTaskEndReason.Death or
                         NpcTaskEndReason.Removal or
                         NpcTaskEndReason.WorldUnload) ||
        !npc.TryCaptureTaskReference(out NpcTaskReference taskReference) ||
        !npc.TryTerminateTask(taskReference, endReason, out taskTermination))
    {
      return false;
    }

    if (_projectileEntityStore is null)
    {
      ProjectileStaticNpcImmunitySystem.ClearStaticNpcSlot(
        _projectileStaticNpcImmunity,
        npc.Slot.Value);
    }
    else
    {
      ProjectileStaticNpcImmunitySystem.ResetNpcSlotData(
        _projectileStaticNpcImmunity,
        _projectileEntityStore,
        _entityRuntime,
        npc.Slot.Value);
    }

    DetachChildren(instanceId);
    if (_entityRuntime.TryGetStatus(npc.RuntimeHandle, out EntityRuntimeStatus status) &&
        status == EntityRuntimeStatus.Running &&
        !_entityRuntime.TryBeginTermination(npc.RuntimeHandle))
    {
      return false;
    }

    if (!_entities.TryRelease(npc.Slot, npc.SlotGeneration, out _))
    {
      throw new InvalidOperationException("An NPC runtime handle lost its compatibility slot during removal.");
    }

    _handlesByInstanceId.Remove(instanceId);
    if (!_entityRuntime.TryRemoveEntity(npc.RuntimeHandle))
    {
      throw new InvalidOperationException("An NPC runtime identity could not be removed after slot release.");
    }

    return true;
  }

  private void DetachChildren(NpcInstanceId parentInstanceId)
  {
    for (int index = 0; index < _entities.Capacity; index++)
    {
      if (!_entities.TryGetOccupiedAt(index, out _, out _, out RuntimeNpcEntity? entity) ||
          entity is null ||
          !entity.TryCaptureParentRelation(
            out RuntimeNpcEntity.NpcParentRelationSnapshot relation) ||
          relation.ParentInstanceId != parentInstanceId)
      {
        continue;
      }

      if (!entity.TryDetachParentRelation())
      {
        throw new InvalidOperationException(
          "A child NPC parent relation could not be detached before parent release.");
      }
    }
  }

  public IReadOnlyList<RuntimeNpcEntity> CreateActiveSnapshot()
  {
    var activeNpcs = new List<RuntimeNpcEntity>(_entities.ActiveCount);
    for (int index = 0; index < _entities.Capacity; index++)
    {
      if (_entities.TryGetOccupiedAt(index, out _, out _, out RuntimeNpcEntity? entity))
      {
        activeNpcs.Add(entity ??
          throw new InvalidOperationException("An occupied NPC slot has no runtime entity."));
      }
    }

    return activeNpcs.AsReadOnly();
  }

  public IReadOnlyList<SpawnSnapshot> CreateSpawnSnapshots()
  {
    var spawnSnapshots = new List<SpawnSnapshot>(_entities.ActiveCount);
    for (int index = 0; index < _entities.Capacity; index++)
    {
      if (!_entities.TryGetOccupiedAt(index, out _, out _, out RuntimeNpcEntity? entity))
      {
        continue;
      }

      RuntimeNpcEntity npc = entity ??
        throw new InvalidOperationException("An occupied NPC slot has no runtime entity.");
      NpcTargetGeometrySnapshot geometry = CaptureColliderGeometry(npc.Location, npc.Collider);
      NpcDefinition definition = npc.Definition;
      spawnSnapshots.Add(new SpawnSnapshot(
        npc.Slot.Value,
        geometry.Center,
        definition.Town.IsTownNpc,
        definition.Capabilities.Hostile));
    }

    return spawnSnapshots.AsReadOnly();
  }

  public IReadOnlyList<RuntimeNpcContactSnapshot> CreateContactSnapshots()
  {
    var contactSnapshots = new List<RuntimeNpcContactSnapshot>(_entities.ActiveCount);
    for (int index = 0; index < _entities.Capacity; index++)
    {
      if (!_entities.TryGetOccupiedAt(index, out _, out _, out RuntimeNpcEntity? entity))
      {
        continue;
      }

      RuntimeNpcEntity npc = entity ??
        throw new InvalidOperationException("An occupied NPC slot has no runtime entity.");
      contactSnapshots.Add(npc.CaptureContactSnapshot());
    }

    return contactSnapshots.AsReadOnly();
  }

  internal IReadOnlyList<RuntimeNpcProjectileTargetSnapshot> CreateProjectileTargetSnapshot()
  {
    var targets = new List<RuntimeNpcProjectileTargetSnapshot>(_entities.ActiveCount);
    for (int index = 0; index < _entities.Capacity; index++)
    {
      if (!_entities.TryGetOccupiedAt(
            index,
            out NpcRuntimeSlot slot,
            out uint slotGeneration,
            out RuntimeNpcEntity? entity))
      {
        continue;
      }

      RuntimeNpcEntity npc = entity ??
        throw new InvalidOperationException("An occupied NPC slot has no runtime entity.");
      if (!_entityRuntime.TryGetReference(
            npc.RuntimeHandle,
            EntityReferenceScope.Npc,
            out EntityReference reference))
      {
        throw new InvalidOperationException("An occupied NPC slot has no running entity reference.");
      }

      NpcDefinition definition = npc.Definition;
      targets.Add(new RuntimeNpcProjectileTargetSnapshot(
        reference,
        npc.RuntimeHandle,
        slot,
        slotGeneration,
        definition.TypeId,
        definition.NetId,
        npc.IsActive,
        definition.Capabilities.Friendly,
        definition.Town.IsTownNpc,
        definition.Spawn.AiStyle,
        npc.CaptureAiState().State2,
        npc.Movement,
        CaptureColliderGeometry(npc.Location, npc.Collider)));
    }

    return targets.AsReadOnly();
  }

  internal bool TryApplyProjectileHit(
    RuntimeNpcProjectileTargetSnapshot target,
    int damage,
    int ownerSlot,
    long tickNumber,
    float knockback,
    int hitDirection,
    out NpcStrikeResult strike,
    out NpcDeathDropSnapshot deathDrop)
  {
    if (_isDisposed ||
        target.Reference.Scope != EntityReferenceScope.Npc ||
        !_entityRuntime.TryResolve(target.Reference, out RuntimeEntityHandle resolvedHandle) ||
        resolvedHandle != target.RuntimeHandle ||
        !_entities.TryGet(target.Slot, target.SlotGeneration, out RuntimeNpcEntity? npc) ||
        npc is null ||
        npc.RuntimeHandle != resolvedHandle)
    {
      strike = default;
      deathDrop = default;
      return false;
    }

    strike = ApplyProjectileHit(
      npc,
      damage,
      ownerSlot,
      tickNumber,
      knockback,
      hitDirection,
      out deathDrop);
    if (strike.CombatResult.Applied && !strike.CombatResult.DeathTransitioned)
    {
      npc.CommitJustHit();
    }

    return true;
  }

  private NpcStrikeResult ApplyProjectileHit(
    RuntimeNpcEntity npc,
    int damage,
    int ownerSlot,
    long tickNumber,
    float knockback,
    int hitDirection,
    out NpcDeathDropSnapshot deathDrop)
  {
    ArgumentNullException.ThrowIfNull(npc);
    if ((uint)ownerSlot >= byte.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(ownerSlot));
    }

    var request = new NpcDamageRequest(
      new CombatContributorId(
        CombatContributorKind.Player,
        $"simulation-player-{ownerSlot}"),
      damage,
      tickNumber,
      Defense: npc.Definition.Core.DefaultDefense,
      LegacyOwner: ownerSlot);
    MovementStateComponent movement = npc.Movement;
    NpcStrikeResult result = default;
    RuntimeNpcEntity? parent = null;
    RuntimeNpcEntity.NpcParentRelationSnapshot parentRelation = default;
    bool hasParentRelation = npc.HasParentRelation;
    bool capturedParentRelation = !hasParentRelation ||
      npc.TryCaptureParentRelation(out parentRelation);

    if (!hasParentRelation)
    {
      result = ResolveProjectileHitOnEntity(
        npc,
        request,
        knockback,
        hitDirection,
        parentRelation: null,
        ref movement);
    }
    else
    {
      NpcParentRelationComponent relationComponent = new(
        capturedParentRelation ? parentRelation.ParentInstanceId : default,
        capturedParentRelation ? parentRelation.ParentLegacySlot : default,
        capturedParentRelation ? parentRelation.AttachedAtTick : tickNumber);
      parent = capturedParentRelation
        ? TryResolveLifeRoot(npc, parentRelation, ref relationComponent)
        : null;
      result = parent is null
        ? ResolveProjectileHitOnEntity(
          npc,
          request,
          knockback,
          hitDirection,
          relationComponent,
          ref movement)
        : ResolveProjectileHitWithParent(
          npc,
          parent,
          relationComponent,
          request,
          knockback,
          hitDirection,
          ref movement);
    }

    npc.Movement = movement;
    RuntimeNpcEntity deathOwner = parent is not null &&
      result.CombatResult.LifeOwnerInstanceId == parent.InstanceId
        ? parent
        : npc;
    result = CompletePendingDeathEffects(
      deathOwner,
      result,
      tickNumber);
    deathDrop = default;
    if (result.CombatResult.DeathTransitioned)
    {
      MovementStateComponent deathOwnerMovement = deathOwner.Movement;
      deathDrop = new NpcDeathDropSnapshot(
        deathOwner.Definition.NetId,
        deathOwnerMovement.Position);

      ReleaseParentAndChildren(deathOwner);
    }

    return result;
  }

  private NpcStrikeResult CompletePendingDeathEffects(
    RuntimeNpcEntity deathOwner,
    NpcStrikeResult result,
    long tickNumber)
  {
    if (result.CombatResult.DeathLifecycle is not NpcDeathLifecycleResult pending ||
        !pending.TerminalCommitPending ||
        pending.PhaseDecision is not NpcDeathPhaseDecision decision ||
        decision.MotherSlimeDeathSplitIntent is not NpcMotherSlimeDeathSplitIntent splitIntent ||
        _session is null ||
        _contentCatalog is null)
    {
      return result;
    }

    MovementStateComponent parentMovement = deathOwner.Movement;
    RuntimeNpcEntity.NpcDirectionSnapshot direction = deathOwner.CaptureDirection();
    NpcMotherSlimeDeathSplitSystem.Apply(
      in splitIntent,
      parentMovement.Position,
      (int)deathOwner.Collider.Width,
      (int)deathOwner.Collider.Height,
      parentMovement.Velocity,
      direction.Sprite,
      Main.netMode,
      new RuntimeNpcMotherSlimeDeathSplitEffectPort(
        this,
        deathOwner,
        _session,
        _contentCatalog,
        Main.rand));

    NpcDeathLifecycleResult committedDeath = default;
    if (!deathOwner.TryEditCombatComponents(
          (ref NpcHealthComponent health, ref NpcLifecycleComponent lifecycle) =>
          {
            committedDeath = _combatSystem.CommitAfterPreTerminalEffects(
              new NpcTypeId(deathOwner.Definition.TypeId),
              tickNumber,
              health,
              lifecycle,
              pending);
          }))
    {
      throw new InvalidOperationException(
        "The Mother Slime terminal death could not be committed after its split effect.");
    }

    return result with
    {
      CombatResult = result.CombatResult with
      {
        DeathTransitioned = committedDeath.Transitioned,
        LifeAfter = deathOwner.CurrentLife,
        DeathLifecycle = committedDeath,
      },
    };
  }

  private NpcStrikeResult ResolveProjectileHitOnEntity(
    RuntimeNpcEntity npc,
    NpcDamageRequest request,
    float knockback,
    int hitDirection,
    NpcParentRelationComponent? parentRelation,
    ref MovementStateComponent movement)
  {
    MovementStateComponent localMovement = movement;
    NpcDeathPhaseContext deathPhaseContext = CreateDeathPhaseContext(
      npc,
      movement);
    NpcStrikeResult result = default;
    if (!npc.TryEditCombatComponents(
          (ref NpcHealthComponent health, ref NpcLifecycleComponent lifecycle) =>
          {
            result = ResolveProjectileStrike(
              npc,
              ref health,
              ref lifecycle,
              request,
              knockback,
              hitDirection,
              parentRelation,
              parentHealthTarget: null,
              deathPhaseContext,
              ref localMovement);
          }))
    {
      throw new InvalidOperationException("The NPC combat component set is unavailable.");
    }

    movement = localMovement;
    return result;
  }

  private NpcStrikeResult ResolveProjectileHitWithParent(
    RuntimeNpcEntity npc,
    RuntimeNpcEntity parent,
    NpcParentRelationComponent parentRelation,
    NpcDamageRequest request,
    float knockback,
    int hitDirection,
    ref MovementStateComponent movement)
  {
    NpcStrikeResult result = default;
    NpcInstanceId parentInstanceId = parent.InstanceId;
    NpcRuntimeSlot parentSlot = parent.Slot;
    NpcTypeId parentType = new(parent.Definition.TypeId);
    bool parentIsBoss = parent.Definition.Capabilities.IsBoss;
    MovementStateComponent localMovement = movement;
    NpcDeathPhaseContext parentDeathPhaseContext = CreateDeathPhaseContext(
      parent,
      parent.Movement);

    if (ReferenceEquals(npc, parent))
    {
      NpcStrikeResult selfParentResult = default;
      if (!npc.TryEditCombatComponents(
            (ref NpcHealthComponent health, ref NpcLifecycleComponent lifecycle) =>
            {
              var selfTarget = new NpcParentHealthTarget(
                new NpcEntityIdentityComponent(parentInstanceId, parentSlot),
                parentType,
                health,
                lifecycle,
                parentIsBoss);
              selfParentResult = ResolveProjectileStrike(
                npc,
                ref health,
                ref lifecycle,
                request,
                knockback,
                hitDirection,
                parentRelation,
                selfTarget,
                parentDeathPhaseContext,
                ref localMovement);
            }))
      {
        throw new InvalidOperationException("The self-parent NPC combat component set is unavailable.");
      }

      movement = localMovement;
      return selfParentResult;
    }

    if (!parent.TryEditCombatComponents(
          (ref NpcHealthComponent parentHealth, ref NpcLifecycleComponent parentLifecycle) =>
          {
            NpcHealthComponent parentHealthValue = parentHealth;
            NpcLifecycleComponent parentLifecycleValue = parentLifecycle;
            if (!npc.TryEditCombatComponents(
                  (ref NpcHealthComponent health, ref NpcLifecycleComponent lifecycle) =>
                  {
                    var parentTarget = new NpcParentHealthTarget(
                      new NpcEntityIdentityComponent(parentInstanceId, parentSlot),
                      parentType,
                      parentHealthValue,
                      parentLifecycleValue,
                      parentIsBoss);
                    result = ResolveProjectileStrike(
                      npc,
                      ref health,
                      ref lifecycle,
                      request,
                      knockback,
                    hitDirection,
                    parentRelation,
                    parentTarget,
                    parentDeathPhaseContext,
                    ref localMovement);
                  }))
            {
              throw new InvalidOperationException(
                "A child NPC combat component set is unavailable while its parent is borrowed.");
            }
          }))
    {
      throw new InvalidOperationException("The parent NPC combat component set is unavailable.");
    }

    movement = localMovement;
    return result;
  }

  private NpcStrikeResult ResolveProjectileStrike(
    RuntimeNpcEntity npc,
    ref NpcHealthComponent health,
    ref NpcLifecycleComponent lifecycle,
    NpcDamageRequest request,
    float knockback,
    int hitDirection,
    NpcParentRelationComponent? parentRelation,
    NpcParentHealthTarget? parentHealthTarget,
    NpcDeathPhaseContext deathPhaseContext,
    ref MovementStateComponent movement)
  {
    return _combatSystem.ResolveAndCommitStrike(
      new NpcTypeId(npc.Definition.TypeId),
      health,
      lifecycle,
      request,
      new NpcKnockbackInput(
        knockback,
        hitDirection,
        KnockbackResistance: 1f,
        OnFire2: false,
        ExpertMode: false,
        NoGravity: false),
      _movementSystem,
      ref movement,
      parentRelation,
      parentHealthTarget,
      deathPhaseContext);
  }

  private NpcDeathPhaseContext CreateDeathPhaseContext(
    RuntimeNpcEntity npc,
    MovementStateComponent movement)
  {
    ColliderComponent collider = npc.Collider;
    return new NpcDeathPhaseContext(
      npc.CaptureAiState(),
      movement.Position + new Vector2(collider.Width * 0.5f, collider.Height * 0.5f),
      IsLifeOwner: true,
      IsGoodWorld: _session is not null &&
        (_session.World.Rules.SecretSeeds & WorldSessionSecretSeedFlags.ForTheWorthy) != 0,
      BottomY: movement.Position.Y + collider.Height,
      SourceNpcInstanceId: npc.InstanceId,
      NetMode: Main.netMode);
  }

  private RuntimeNpcEntity? TryResolveParentEntity(
    RuntimeNpcEntity.NpcParentRelationSnapshot relation)
  {
    if (!relation.IsAttached ||
        !relation.ParentLegacySlot.IsAssigned ||
        relation.ParentReference.Scope != EntityReferenceScope.Npc ||
        !_handlesByInstanceId.TryGetValue(
          relation.ParentInstanceId,
          out RuntimeEntityHandle parentHandle) ||
        !_entityRuntime.TryResolve(relation.ParentReference, out RuntimeEntityHandle referenceHandle) ||
        referenceHandle != parentHandle ||
        !_entityRuntime.TryGetReference(
          parentHandle,
          EntityReferenceScope.Npc,
          out EntityReference currentParentReference) ||
        currentParentReference != relation.ParentReference)
    {
      return null;
    }

    int parentSlotIndex = relation.ParentLegacySlot.Value;
    if ((uint)parentSlotIndex >= (uint)_entities.Capacity ||
        !_entities.TryGetOccupiedAt(
          parentSlotIndex,
          out NpcRuntimeSlot parentSlot,
          out uint parentGeneration,
          out RuntimeNpcEntity? parent) ||
        parent is null ||
        parentSlot != relation.ParentLegacySlot ||
        parent.SlotGeneration != parentGeneration ||
        parent.RuntimeHandle != parentHandle ||
        parent.Slot != relation.ParentLegacySlot ||
        parent.InstanceId != relation.ParentInstanceId)
    {
      return null;
    }

    return parent;
  }

  private RuntimeNpcEntity? TryResolveLifeRoot(
    RuntimeNpcEntity child,
    RuntimeNpcEntity.NpcParentRelationSnapshot relation,
    ref NpcParentRelationComponent relationComponent)
  {
    RuntimeNpcEntity? parent = TryResolveParentEntity(relation);
    if (parent is null)
    {
      return null;
    }

    var visited = new HashSet<NpcInstanceId> { child.InstanceId, parent.InstanceId };
    RuntimeNpcEntity.NpcParentRelationSnapshot currentRelation = relation;
    while (parent.HasParentRelation)
    {
      if (!parent.TryCaptureParentRelation(
            out RuntimeNpcEntity.NpcParentRelationSnapshot ancestorRelation) ||
          !ancestorRelation.IsAttached ||
          !visited.Add(ancestorRelation.ParentInstanceId))
      {
        return null;
      }

      RuntimeNpcEntity? ancestor = TryResolveParentEntity(ancestorRelation);
      if (ancestor is null)
      {
        return null;
      }

      parent = ancestor;
      currentRelation = ancestorRelation;
    }

    relationComponent = new NpcParentRelationComponent(
      parent.InstanceId,
      parent.Slot,
      currentRelation.AttachedAtTick);
    return parent;
  }

  private void ReleaseParentAndChildren(RuntimeNpcEntity parent)
  {
    var pending = new Queue<RuntimeNpcEntity>();
    var releaseOrder = new List<RuntimeNpcEntity>();
    var visited = new HashSet<NpcInstanceId> { parent.InstanceId };
    pending.Enqueue(parent);
    while (pending.Count > 0)
    {
      RuntimeNpcEntity current = pending.Dequeue();
      releaseOrder.Add(current);
      for (int index = 0; index < _entities.Capacity; index++)
      {
        if (!_entities.TryGetOccupiedAt(index, out _, out _, out RuntimeNpcEntity? entity) ||
            entity is null ||
            !entity.TryCaptureParentRelation(
              out RuntimeNpcEntity.NpcParentRelationSnapshot relation) ||
            relation.ParentInstanceId != current.InstanceId ||
            !visited.Add(entity.InstanceId))
        {
          continue;
        }

        pending.Enqueue(entity);
      }
    }

    for (int index = releaseOrder.Count - 1; index >= 1; index--)
    {
      if (!TryRelease(releaseOrder[index], NpcTaskEndReason.Death, out _))
      {
        throw new InvalidOperationException(
          "A defeated NPC parent left an attached child in its owner slot.");
      }
    }

    if (!TryRelease(parent, NpcTaskEndReason.Death, out _))
    {
      throw new InvalidOperationException(
        "A defeated NPC parent could not be released from its owner slot.");
    }
  }

  public void Update(
    long tickNumber,
    LoadedWorldSession session,
    RuntimePlayerStore players)
  {
    ArgumentNullException.ThrowIfNull(session);
    ArgumentNullException.ThrowIfNull(players);

    if (!NpcAiAuthorityGate.IsAuthoritative(Main.netMode))
    {
      ObserveClientAuthorityOnly();
      return;
    }

    AdvanceDamageTrackingTo(tickNumber);

    for (int index = 0; index < _entities.Capacity; index++)
    {
      if (!_entities.TryGetOccupiedAt(index, out _, out _, out RuntimeNpcEntity? entity))
      {
        continue;
      }

      RuntimeNpcEntity activeEntity = entity ??
        throw new InvalidOperationException("An occupied NPC slot has no runtime entity.");

      if (!activeEntity.IsActive)
      {
        throw new InvalidOperationException("An inactive NPC remains in the runtime slot store.");
      }

      if (ShouldDespawnNaturally(activeEntity, players))
      {
        if (!TryRelease(activeEntity))
        {
          throw new InvalidOperationException("A naturally despawned NPC could not release its slot.");
        }
        DespawnedCount++;
        continue;
      }

      UpdateMovement(activeEntity, session, players);
      activeEntity.MarkBehaviorUpdated(tickNumber);
    }
  }

  internal void AdvanceDamageTrackingTo(long tickNumber)
  {
    var activeNpcTypes = new HashSet<NpcTypeId>();
    for (int index = 0; index < _entities.Capacity; index++)
    {
      if (_entities.TryGetOccupiedAt(index, out _, out _, out RuntimeNpcEntity? entity))
      {
        activeNpcTypes.Add(new NpcTypeId(entity!.Definition.TypeId));
      }
    }
    _damageTracking.AdvanceTo(tickNumber, activeNpcTypes);
  }

  private void ObserveClientAuthorityOnly()
  {
    for (int index = 0; index < _entities.Capacity; index++)
    {
      if (!_entities.TryGetOccupiedAt(index, out _, out _, out RuntimeNpcEntity? entity) ||
          entity is null)
      {
        continue;
      }

      if (NpcEyeOfCthulhuProfile.CanHandle(
            entity.Definition.TypeId,
            entity.Definition.NetId,
            entity.Definition.Spawn.AiStyle))
      {
        // Preserve the existing authority observation without evaluating the
        // profile, selecting a target, consuming random state, or mutating AI.
        entity.RecordEyeOfCthulhuEffect("ServantSpawnSkipped:ClientAuthority");
      }
    }
  }

  public IReadOnlyList<WorldNpcState> CreatePersistenceSnapshot()
  {
    var snapshots = new List<WorldNpcState>(_entities.ActiveCount);
    for (int index = 0; index < _entities.Capacity; index++)
    {
      if (!_entities.TryGetOccupiedAt(index, out _, out _, out RuntimeNpcEntity? entity))
      {
        continue;
      }

      RuntimeNpcEntity npc = entity ??
        throw new InvalidOperationException("An occupied NPC slot has no runtime entity.");
      if (!npc.Definition.Town.IsTownNpc && !npc.Definition.Capabilities.SavesAndLoads)
      {
        continue;
      }

      WorldNpcState saved = npc.SavedState;
      RuntimeNpcEntity.NpcHousingRelationSnapshot housingRelation =
        npc.CaptureHousingRelationSnapshot();
      RuntimeNpcEntity.NpcPresentationSnapshot presentation =
        npc.CapturePresentationSnapshot();
      LocationComponent location = npc.Location;
      TileCoordinate home = housingRelation.HasHome && housingRelation.HomeTile is { } homeTile
        ? new TileCoordinate(homeTile.X, homeTile.Y)
        : housingRelation.IsHomeless
          ? new TileCoordinate(-1, -1)
          : saved.Home;
      snapshots.Add(new WorldNpcState(
        npc.Definition.NetId,
        saved.LegacyTypeName,
        npc.Definition.Town.IsTownNpc,
        npc.CaptureGivenName(),
        location.X,
        location.Y,
        housingRelation.IsHomeless,
        home,
        saved.Variation.HasValue ? presentation.TownNpcVariationIndex : null,
        housingRelation.HomelessDespawn));
    }

    return snapshots.AsReadOnly();
  }

  public bool TryCaptureTrainingDummyBinding(
    int slotValue,
    TileCoordinate anchor,
    out RuntimeNpcTrainingDummyBinding binding)
  {
    if (slotValue < 0 || slotValue >= _entities.Capacity ||
        !_entities.TryGetOccupiedAt(
          slotValue,
          out _,
          out _,
          out RuntimeNpcEntity? entity))
    {
      binding = default;
      return false;
    }

    RuntimeNpcEntity npc = entity ??
      throw new InvalidOperationException("An occupied NPC slot has no runtime entity.");
    return TryCreateTrainingDummyBinding(npc, anchor.X, anchor.Y, out binding);
  }

  public bool TryReleaseTrainingDummy(RuntimeNpcTrainingDummyBinding binding)
  {
    if (_isDisposed ||
        binding.Reference.Scope != EntityReferenceScope.Npc ||
        !_entityRuntime.TryResolve(binding.Reference, out RuntimeEntityHandle handle) ||
        handle != binding.RuntimeHandle ||
        !_entities.TryGet(binding.Slot, binding.SlotGeneration, out RuntimeNpcEntity? npc) ||
        npc is null ||
        npc.RuntimeHandle != handle ||
        !TryCreateTrainingDummyBinding(
          npc,
          binding.Anchor.X,
          binding.Anchor.Y,
          out RuntimeNpcTrainingDummyBinding currentBinding) ||
        currentBinding.Reference != binding.Reference)
    {
      return false;
    }

    return TryRelease(npc);
  }

  private bool TryCreateTrainingDummyBinding(
    RuntimeNpcEntity npc,
    int tileX,
    int tileY,
    out RuntimeNpcTrainingDummyBinding binding)
  {
    NpcAiStateComponent ai = npc.CaptureAiState();
    if (npc.Definition.NetId != SimulationContentSupportManifest.TrainingDummyNetId ||
        ai.State0 != tileX || ai.State1 != tileY ||
        !_entityRuntime.TryGetReference(
          npc.RuntimeHandle,
          EntityReferenceScope.Npc,
          out EntityReference reference))
    {
      binding = default;
      return false;
    }

    binding = new RuntimeNpcTrainingDummyBinding(
      reference,
      npc.RuntimeHandle,
      npc.Slot,
      npc.SlotGeneration,
      new TileCoordinate(tileX, tileY));
    return true;
  }

  public bool TryGetAt(int slotValue, out RuntimeNpcEntity? npc)
  {
    if (slotValue < 0 || slotValue >= _entities.Capacity ||
        !_entities.TryGetOccupiedAt(slotValue, out _, out _, out npc))
    {
      npc = null;
      return false;
    }

    return npc is not null;
  }

  private void UpdateMovement(
    RuntimeNpcEntity npc,
    LoadedWorldSession session,
    RuntimePlayerStore players) {
    if (npc.Definition.NetId == SimulationContentSupportManifest.TrainingDummyNetId) {
      return;
    }

    MovementStateComponent movement = npc.Movement;
    ColliderComponent collider = npc.Collider;
    int npcWidth = GetCollisionWidth(collider);
    int npcHeight = GetCollisionHeight(collider);
    bool isSourceFloatingEye = NpcFloatingEyeProfile.CanHandle(
      npc.Definition.TypeId,
      npc.Definition.NetId,
      npc.Definition.Spawn.AiStyle);
    bool isSourceEyeOfCthulhu = NpcEyeOfCthulhuProfile.CanHandle(
      npc.Definition.TypeId,
      npc.Definition.NetId,
      npc.Definition.Spawn.AiStyle);
    bool isSourceServantOfCthulhu = NpcServantOfCthulhuProfile.CanHandle(
      npc.Definition.TypeId,
      npc.Definition.NetId,
      npc.Definition.Spawn.AiStyle);
    bool isSourceGuide = npc.Definition.NetId == SimulationContentSupportManifest.GuideNetId;
    bool isSourceBlueSlime = NpcBlueSlimeProfile.CanHandle(
      npc.Definition.TypeId,
      npc.Definition.NetId,
      npc.Definition.Spawn.AiStyle);
    bool isSourceMotherSlime = NpcMotherSlimeProfile.CanHandle(
      npc.Definition.TypeId,
      npc.Definition.NetId,
      npc.Definition.Spawn.AiStyle);
    bool isSourceFighter = NpcFighterProfile.CanHandle(
      npc.Definition.TypeId,
      npc.Definition.NetId,
      npc.Definition.Spawn.AiStyle);
    Vector2 oldVelocity = movement.Velocity;
    RuntimeNpcEntity.NpcMovementTickSnapshot previousMovementTick =
      npc.CaptureMovementTickSnapshot();
    RuntimeNpcEntity.NpcDirectionSnapshot oldDirection = npc.CaptureDirection();
    int oldTarget = npc.CaptureTargetSlot();
    var targetSelectionAdapter = new RuntimeNpcTargetSelectionAdapter(
      npc,
      session,
      players,
      CaptureNpcNpcTargetSnapshots,
      oldDirection,
      oldTarget,
      previousMovementTick.CollideX,
      previousMovementTick.CollideY);
    bool previousCollideX = previousMovementTick.CollideX;
    bool previousCollideY = previousMovementTick.CollideY;
    bool previousNoTileCollide = previousMovementTick.NoTileCollide;
    Vector2 colliderPosition = GetColliderPosition(movement.Position, collider);
    LegacyVector2 legacyPosition = new(colliderPosition.X, colliderPosition.Y);
    bool wet = Collision.WetCollision(legacyPosition, npcWidth, npcHeight);
    bool shimmerWet = Collision.shimmer;
    bool honeyWet = Collision.honey;
    npc.BeginMovementTick(
      movement.Position,
      movement.Velocity,
      wet,
      shimmerWet,
      honeyWet);
    if (isSourceFloatingEye ||
        isSourceEyeOfCthulhu ||
         isSourceBlueSlime ||
         isSourceMotherSlime ||
         isSourceFighter ||
         isSourceGuide) {
      npc.BeginImmediateEffectsTick();
    }
    NpcAiStateComponent aiState = npc.CaptureAiState();
    NpcGravityResult gravity = default;
    if (!isSourceFloatingEye && !isSourceEyeOfCthulhu && !isSourceServantOfCthulhu) {
      NpcAiStateComponent gravityState = new(
        aiState.Style,
        aiState.State0,
        aiState.State1,
        aiState.State2,
        aiState.State3,
        timer: 0);
      gravity = NpcGravitySystem.Evaluate(
        new NpcTypeId(npc.Definition.TypeId),
        gravityState,
        movement.Velocity.Y,
        new NpcGravityEnvironmentSnapshot(
          movement.Position.Y,
          session.World.Descriptor.SizeX,
          session.World.Descriptor.SurfaceLayer,
          wet,
          shimmerWet,
          honeyWet));
      movement.Velocity.Y = gravity.VelocityYAfter;
    }

    RevalidateHousing(npc, session);

    var targets = new List<NpcAiTargetSnapshot>(players.Players.Count);
    foreach (RuntimePlayerEntity player in players.Players) {
      NpcTargetGeometrySnapshot geometry = CaptureColliderGeometry(
        player.Location,
        player.Collider);
      targets.Add(new NpcAiTargetSnapshot(
        player.Slot,
        geometry,
        new Vector2(player.Velocity.X, player.Velocity.Y),
        isActive: !player.Lifecycle.IsDead,
        isDead: player.Lifecycle.IsDead));
    }

    if (isSourceFloatingEye) {
      UpdateFloatingEyeMovement(
        npc,
        session,
        movement,
        targets,
        npcWidth,
        npcHeight,
        oldVelocity,
        previousCollideX,
        previousCollideY,
        previousNoTileCollide,
        wet,
        targetSelectionAdapter);
      return;
    }

    if (isSourceEyeOfCthulhu) {
      UpdateEyeOfCthulhuMovement(
        npc,
        session,
        movement,
        targets,
        npcWidth,
        npcHeight,
        targetSelectionAdapter);
      return;
    }

    if (isSourceServantOfCthulhu) {
      UpdateServantOfCthulhuMovement(
        npc,
        session,
        movement,
        targets,
        npcWidth,
        npcHeight,
        targetSelectionAdapter);
      return;
    }

    if (isSourceBlueSlime) {
      UpdateBlueSlimeMovement(
        npc,
        session,
        movement,
        targets,
        npcWidth,
        npcHeight,
        oldVelocity,
        gravity,
        previousCollideX,
        previousCollideY,
        wet,
        targetSelectionAdapter);
      return;
    }

    if (isSourceMotherSlime) {
      UpdateMotherSlimeMovement(
        npc,
        session,
        movement,
        targets,
        npcWidth,
        npcHeight,
        oldVelocity,
        gravity,
        previousCollideX,
        previousCollideY,
        wet,
        targetSelectionAdapter);
      return;
    }

    if (isSourceFighter) {
      UpdateFighterMovement(
        npc,
        session,
        movement,
        targets,
        npcWidth,
        npcHeight,
        gravity,
        targetSelectionAdapter);
      return;
    }

    RuntimeNpcEntity.NpcHousingRelationSnapshot housingRelation =
      npc.CaptureHousingRelationSnapshot();
    System.Numerics.Vector2 homeCenter = System.Numerics.Vector2.Zero;
    if (housingRelation.HasHome && housingRelation.HomeTile is { } home) {
      homeCenter = new System.Numerics.Vector2(home.X * 16f + 8f, home.Y * 16f + 8f);
    }

    SyncGuideTask(
      npc,
      session.World.TimeWeather.DayTime,
      housingRelation.HasHome);

    if (isSourceGuide && !session.World.TimeWeather.DayTime)
    {
      RuntimeNpcEntity.NpcTaskSnapshot task = npc.CaptureTaskSnapshot();
      if (task.Kind == NpcTaskKind.GuideReturnHome &&
          task.Phase == NpcTaskPhase.Running &&
          task.Cursor >= NpcGuideHomeReturnProfile.TeleportTriggerCursor)
      {
        bool returnedHome = TryReturnGuideHome(
          npc,
          session,
          ref movement,
          npcWidth,
          npcHeight,
          housingRelation,
          new NpcTaskReference(npc.RuntimeHandle, task.TaskGeneration));
        if (returnedHome)
        {
          return;
        }

        housingRelation = npc.CaptureHousingRelationSnapshot();
      }
      else if (task.Kind == NpcTaskKind.GuideReturnHome &&
               task.Phase == NpcTaskPhase.Completed)
      {
        movement.Velocity = Vector2.Zero;
        movement.IsGrounded = true;
        npc.CommitMovementPhysicsFlags(noGravity: false, noTileCollide: false);
        npc.CommitMovementCollision(false, false);
        npc.Movement = movement;
        return;
      }
    }

    NpcAiDecision decision = _aiSystem.Evaluate(
      new NpcAiInput(
        npc.Definition,
        npc.Slot.Value,
        movement.Position,
        movement.Velocity,
        aiState,
        new NpcAiEnvironmentSnapshot(
          session.World.TimeWeather.DayTime,
          movement.IsGrounded,
          housingRelation.HasHome,
          homeCenter),
        targets));
    npc.CommitAiState(decision.State, decision.Action);
    if (decision.SkipMovement) {
      npc.CommitMovementPhysicsFlags(
        decision.NoGravity,
        decision.NoTileCollide);
      npc.Movement = movement;
      return;
    }

    npc.CommitMovementPhysicsFlags(
      decision.NoGravity,
      decision.NoTileCollide);

    if (decision.HasVelocityOverride) {
      movement.Velocity = decision.Velocity;
    }

    if (!decision.NoGravity) {
      movement.Velocity.Y = Math.Min(
        movement.Velocity.Y + gravity.Gravity,
        gravity.MaximumFallSpeed);
    }

    if (decision.NoTileCollide) {
      float nextX = movement.Position.X + movement.Velocity.X;
      float nextY = movement.Position.Y + movement.Velocity.Y;
      Vector2 boundedPosition = ClampEntityPositionToWorld(
        new Vector2(nextX, nextY),
        collider,
        npcWidth,
        npcHeight,
        session.World.Descriptor.SizeX * 16f,
        session.World.Descriptor.SizeY * 16f);
      movement.Position = boundedPosition;
      movement.Velocity = new System.Numerics.Vector2(
        boundedPosition.X == nextX ? movement.Velocity.X : 0f,
        boundedPosition.Y == nextY ? movement.Velocity.Y : 0f);
      movement.IsGrounded = false;
      npc.CommitMovementCollision(false, false);
      npc.Movement = movement;
      return;
    }

    LegacyVector2 requestedVelocity = new(movement.Velocity.X, movement.Velocity.Y);
    LegacyVector2 resolvedVelocity = Collision.TileCollision(
      GetLegacyColliderPosition(movement.Position, collider),
      requestedVelocity,
      npcWidth,
      npcHeight);
    bool collidedX = MathF.Abs(resolvedVelocity.X - requestedVelocity.X) > 0.001f;
    bool collidedY = MathF.Abs(resolvedVelocity.Y - requestedVelocity.Y) > 0.001f;
    movement.Position += new System.Numerics.Vector2(resolvedVelocity.X, resolvedVelocity.Y);
    movement.Velocity = new System.Numerics.Vector2(
      collidedX ? 0f : resolvedVelocity.X,
      collidedY ? 0f : resolvedVelocity.Y);
    movement.IsGrounded = collidedY && requestedVelocity.Y > 0f && Collision.down;
    npc.CommitMovementCollision(collidedX, collidedY);
    bool canJumpObstacles = npc.Definition.Spawn.AiStyle == 3 ||
      npc.Definition.NetId == SimulationContentSupportManifest.GuideNetId;
    if (collidedX && movement.IsGrounded && decision.Action != 0 && canJumpObstacles) {
      movement.Velocity.Y = -5f;
      movement.IsGrounded = false;
    }

    npc.Movement = movement;
  }

  private static void RevalidateHousing(
    RuntimeNpcEntity npc,
    LoadedWorldSession session)
  {
    if (!npc.Definition.Town.IsTownNpc)
    {
      return;
    }

    RuntimeNpcEntity.NpcHousingRelationSnapshot relation =
      npc.CaptureHousingRelationSnapshot();
    TownHousingResidentKey resident = new(npc.Definition.TypeId);
    if (!relation.HasHome || relation.HomeTile is not { } homeTile)
    {
      if (relation.IsHomeless && !TownHousingRegistrySystem.IsHomeless(
            session.TownHousing,
            resident))
      {
        TownHousingRegistrySystem.MarkHomeless(session.TownHousing, resident);
        npc.CommitHousingRegistrySynchronizationEffect();
      }

      return;
    }

    bool roomIsValid = WorldGen.IsHousingRoomValidAt(
      homeTile.X,
      homeTile.Y - 1,
      session.World.Descriptor.SizeX,
      session.World.Descriptor.SizeY);
    if (!roomIsValid)
    {
      bool relationChanged = !relation.IsHomeless || relation.HomeTile.HasValue;
      if (relationChanged)
      {
        npc.CommitHousingRelation(
          isHomeless: true,
          relation.HomelessDespawn,
          homeTile: null);
        npc.CommitHousingRevalidationFailureEffect();
        npc.CommitNetworkUpdateIntent();
      }

      if (!TownHousingRegistrySystem.IsHomeless(session.TownHousing, resident))
      {
        TownHousingRegistrySystem.MarkHomeless(session.TownHousing, resident);
        npc.CommitHousingRegistrySynchronizationEffect();
      }

      return;
    }

    var expectedRoom = new TilePosition(homeTile.X, homeTile.Y);
    bool registryMatches =
      !TownHousingRegistrySystem.IsHomeless(session.TownHousing, resident) &&
      TownHousingRegistrySystem.TryGetRoom(
        session.TownHousing,
        resident,
        out TilePosition assignedRoom) &&
      assignedRoom == expectedRoom;
    if (!registryMatches)
    {
      TownHousingRegistrySystem.AssignRoom(session.TownHousing, resident, expectedRoom);
      npc.CommitHousingRegistrySynchronizationEffect();
    }
  }

  private static void SyncGuideTask(
    RuntimeNpcEntity npc,
    bool dayTime,
    bool hasHome)
  {
    if (npc.Definition.NetId != SimulationContentSupportManifest.GuideNetId) {
      return;
    }

    NpcTaskKind taskKind = dayTime
      ? NpcTaskKind.GuideDayPatrol
      : hasHome
        ? NpcTaskKind.GuideReturnHome
        : NpcTaskKind.None;
    if (taskKind == NpcTaskKind.None) {
      RuntimeNpcEntity.NpcTaskSnapshot currentTask = npc.CaptureTaskSnapshot();
      if (currentTask.Phase == NpcTaskPhase.Running) {
        var reference = new NpcTaskReference(npc.RuntimeHandle, currentTask.TaskGeneration);
        if (!npc.TryInterruptTask(
              reference, currentTask.Kind, NpcTaskFailureReason.TargetUnavailable, out _))
        {
          throw new InvalidOperationException("The Guide current task could not be interrupted.");
        }
      }

      return;
    }

    RuntimeNpcEntity.NpcTaskSnapshot snapshot = npc.CaptureTaskSnapshot();
    if (snapshot.Kind == taskKind &&
        snapshot.Phase is NpcTaskPhase.Completed or NpcTaskPhase.Failed)
    {
      return;
    }

    npc.EnterTask(taskKind);
    if (!npc.TryCaptureTaskReference(out NpcTaskReference activeReference) ||
        !npc.TryAdvanceTask(activeReference, taskKind, out _))
    {
      throw new InvalidOperationException("The Guide current task could not be advanced.");
    }
  }

  private static bool TryReturnGuideHome(
    RuntimeNpcEntity npc,
    LoadedWorldSession session,
    ref MovementStateComponent movement,
    int npcWidth,
    int npcHeight,
    RuntimeNpcEntity.NpcHousingRelationSnapshot housingRelation,
    NpcTaskReference taskReference)
  {
    if (!housingRelation.HasHome || housingRelation.HomeTile is not { } homeTile)
    {
      return false;
    }

    var collisionQuery = new RuntimeNpcHomeReturnCollisionQuery(session);
    if (!NpcHomeReturnDestinationQuery.TryFindDestination(
          homeTile.X,
          homeTile.Y,
          npcWidth,
          npcHeight,
          collisionQuery,
          out NpcHomeReturnDestination destination))
    {
      const NpcTaskFailureReason failureReason = NpcTaskFailureReason.NoPath;
      npc.CommitHomeTeleportFailureEffect(failureReason);
      if (!npc.TryFailTask(taskReference, NpcTaskKind.GuideReturnHome, failureReason, out _))
      {
        throw new InvalidOperationException("The Guide home return task could not be failed.");
      }
      npc.CommitHousingRelation(
        isHomeless: true,
        homelessDespawn: housingRelation.HomelessDespawn,
        homeTile: null);
      return false;
    }

    movement.Position = destination.Position;
    movement.Velocity = Vector2.Zero;
    movement.IsGrounded = true;
    npc.CommitMovementPhysicsFlags(noGravity: false, noTileCollide: false);
    npc.CommitMovementCollision(false, false);
    npc.Movement = movement;
    npc.CommitHomeTeleportEffect(destination.Position, destination.CandidateOffset);
    npc.CommitNetworkUpdateIntent();
    if (!npc.TryCompleteTask(taskReference, NpcTaskKind.GuideReturnHome, out _))
    {
      throw new InvalidOperationException("The Guide home return task could not be completed.");
    }
    return true;
  }

  private void UpdateBlueSlimeMovement(
    RuntimeNpcEntity npc,
    LoadedWorldSession session,
    MovementStateComponent movement,
    IReadOnlyList<NpcAiTargetSnapshot> targets,
    int npcWidth,
    int npcHeight,
    Vector2 oldVelocity,
    NpcGravityResult gravity,
    bool previousCollideX,
    bool previousCollideY,
    bool wet,
    RuntimeNpcTargetSelectionAdapter targetSelectionAdapter)
  {
    NpcAiStateComponent aiState = npc.CaptureAiState();
    RuntimeNpcEntity.NpcDirectionSnapshot direction = npc.CaptureDirection();
    int targetSlot = npc.CaptureTargetSlot();
    NpcBlueSlimeProfileInput input = new(
      Position: movement.Position,
      Velocity: movement.Velocity,
      State: new NpcBlueSlimeProfileState(
        aiState.State0,
        aiState.State1,
        aiState.State2,
        aiState.State3),
      Direction: direction.Sprite,
      TargetSlot: targetSlot,
      DayTime: session.World.TimeWeather.DayTime,
      IsDamaged: npc.CurrentLife < npc.MaximumLife,
      IsBelowSurface: movement.Position.Y > session.World.Descriptor.SurfaceLayer * 16f,
      SlimeRain: session.World.TimeWeather.SlimeRain,
      Wet: wet,
      CollideX: previousCollideX,
      CollideY: previousCollideY,
      OldVelocity: oldVelocity,
      Gravity: gravity.Gravity,
      IsClient: Main.netMode == 1,
      CanContainItems: true,
      Value: npc.Definition.Core.Value,
      BaseDefense: npc.Definition.Core.DefaultDefense);
    RuntimeNpcBlueSlimeEffectPort effectPort = new(
      this,
      npc,
      session,
      Main.rand,
      movement.Position,
      npcWidth,
      npcHeight,
      targetSelectionAdapter);
    NpcBlueSlimeTypeOneSelectionResult? itemSelection = null;
    if (NpcBlueSlimeProfile.ShouldGenerateContainedItem(in input))
    {
      NpcBlueSlimeTypeOneSelectionResult selection =
        effectPort.GenerateContainedItem(isBallooned: aiState.State0 == -999f);
      itemSelection = selection;
      if (selection.Attempted)
      {
        aiState = npc.CaptureAiState();
        input = NpcBlueSlimeProfile.WithContainedItemSelection(in input, in selection);
      }
    }

    NpcBlueSlimeProfileResult result = NpcBlueSlimeProfile.Evaluate(in input);
    if (itemSelection.HasValue)
    {
      result = result with
      {
        ContainedItemGenerationRequested = true,
        Branches = result.Branches |
          NpcBlueSlimeSourceBranch.ContainedItemGeneration |
          (itemSelection.Value.NetUpdateRequested
            ? NpcBlueSlimeSourceBranch.NetworkSync
            : NpcBlueSlimeSourceBranch.None),
      };
    }

    npc.CommitDirection(result.Direction, direction.Vertical);
    npc.CommitAiState(
      new NpcAiStateComponent(
        aiState.Style,
        result.State.Ai0,
        result.State.Ai1,
        result.State.Ai2,
        result.State.Ai3,
        aiState.Timer,
        aiState.LocalAi0,
        aiState.LocalAi1,
        aiState.LocalAi2,
        aiState.LocalAi3),
      result.AiAction);
    NpcBlueSlimeProfile.ApplyEffects(
      in result,
      isBallooned: aiState.State0 == -999f,
      effectPort,
      itemSelection);

    npc.CommitMovementPhysicsFlags(noGravity: false, noTileCollide: false);
    movement.Velocity = result.Velocity;
    if (result.IsFrozen) {
      npc.CommitMovementCollision(false, false);
      npc.Movement = movement;
      return;
    }

    movement.Velocity.Y = Math.Min(
      movement.Velocity.Y + gravity.Gravity,
      gravity.MaximumFallSpeed);
    LegacyVector2 requestedVelocity = new(movement.Velocity.X, movement.Velocity.Y);
    LegacyVector2 resolvedVelocity = Collision.TileCollision(
      GetLegacyColliderPosition(movement.Position, npc.Collider),
      requestedVelocity,
      npcWidth,
      npcHeight);
    bool collidedX = MathF.Abs(resolvedVelocity.X - requestedVelocity.X) > 0.001f;
    bool collidedY = MathF.Abs(resolvedVelocity.Y - requestedVelocity.Y) > 0.001f;
    movement.Position += new Vector2(resolvedVelocity.X, resolvedVelocity.Y);
    movement.Velocity = new Vector2(
      collidedX ? 0f : resolvedVelocity.X,
      collidedY ? 0f : resolvedVelocity.Y);
    movement.IsGrounded = collidedY && requestedVelocity.Y > 0f && Collision.down;
    npc.CommitMovementCollision(collidedX, collidedY);
    npc.Movement = movement;
  }

  private void UpdateMotherSlimeMovement(
    RuntimeNpcEntity npc,
    LoadedWorldSession session,
    MovementStateComponent movement,
    IReadOnlyList<NpcAiTargetSnapshot> targets,
    int npcWidth,
    int npcHeight,
    Vector2 oldVelocity,
    NpcGravityResult gravity,
    bool previousCollideX,
    bool previousCollideY,
    bool wet,
    RuntimeNpcTargetSelectionAdapter targetSelectionAdapter)
  {
    NpcAiStateComponent aiState = npc.CaptureAiState();
    RuntimeNpcEntity.NpcDirectionSnapshot direction = npc.CaptureDirection();
    bool solidCollision = previousCollideY &&
      oldVelocity.Y != 0f &&
      movement.Velocity.Y == 0f &&
      Collision.SolidCollision(
        GetLegacyColliderPosition(movement.Position, npc.Collider),
        npcWidth,
        npcHeight);
    NpcMotherSlimeProfileInput input = new(
      TypeId: npc.Definition.TypeId,
      NetId: npc.Definition.NetId,
      AiStyle: npc.Definition.Spawn.AiStyle,
      Position: movement.Position,
      Velocity: movement.Velocity,
      State: new NpcMotherSlimeProfileState(
        aiState.State0,
        aiState.State1,
        aiState.State2,
        aiState.State3),
      Direction: direction.Sprite,
      TargetSlot: npc.CaptureTargetSlot(),
      DayTime: session.World.TimeWeather.DayTime,
      IsDamaged: npc.CurrentLife < npc.MaximumLife,
      IsBelowSurface: movement.Position.Y > session.World.Descriptor.SurfaceLayer * 16f,
      SlimeRain: session.World.TimeWeather.SlimeRain,
      Wet: wet,
      CollideX: previousCollideX,
      CollideY: previousCollideY,
      OldVelocity: oldVelocity,
      SolidCollision: solidCollision);
    NpcMotherSlimeProfileResult result = NpcMotherSlimeProfile.Evaluate(in input);

    npc.CommitDirection(result.Direction, direction.Vertical);
    npc.CommitAiState(
      new NpcAiStateComponent(
        aiState.Style,
        result.State.Ai0,
        result.State.Ai1,
        result.State.Ai2,
        result.State.Ai3,
        aiState.Timer,
        aiState.LocalAi0,
        aiState.LocalAi1,
        aiState.LocalAi2,
        aiState.LocalAi3),
      result.AiAction);
    NpcMotherSlimeProfile.ApplyEffects(
      in result,
      new RuntimeNpcMotherSlimeEffectPort(
        npc,
        input.Position,
        npcWidth,
        npcHeight,
        targetSelectionAdapter));

    npc.CommitMovementPhysicsFlags(noGravity: false, noTileCollide: false);
    movement.Position = result.Position;
    movement.Velocity = result.Velocity;
    if (result.IsFrozen)
    {
      npc.CommitMovementCollision(false, false);
      npc.Movement = movement;
      return;
    }

    movement.Velocity.Y = Math.Min(
      movement.Velocity.Y + gravity.Gravity,
      gravity.MaximumFallSpeed);
    LegacyVector2 requestedVelocity = new(movement.Velocity.X, movement.Velocity.Y);
    LegacyVector2 resolvedVelocity = Collision.TileCollision(
      GetLegacyColliderPosition(movement.Position, npc.Collider),
      requestedVelocity,
      npcWidth,
      npcHeight);
    bool collidedX = MathF.Abs(resolvedVelocity.X - requestedVelocity.X) > 0.001f;
    bool collidedY = MathF.Abs(resolvedVelocity.Y - requestedVelocity.Y) > 0.001f;
    movement.Position += new Vector2(resolvedVelocity.X, resolvedVelocity.Y);
    movement.Velocity = new Vector2(
      collidedX ? 0f : resolvedVelocity.X,
      collidedY ? 0f : resolvedVelocity.Y);
    movement.IsGrounded = collidedY && requestedVelocity.Y > 0f && Collision.down;
    npc.CommitMovementCollision(collidedX, collidedY);
    npc.Movement = movement;
  }

  private void UpdateEyeOfCthulhuMovement(
    RuntimeNpcEntity npc,
    LoadedWorldSession session,
    MovementStateComponent movement,
    IReadOnlyList<NpcAiTargetSnapshot> targets,
    int npcWidth,
    int npcHeight,
    RuntimeNpcTargetSelectionAdapter targetSelectionAdapter)
  {
    npc.BeginEyeOfCthulhuEffectTraceTick();
    int previousTargetSlot = npc.CaptureTargetSlot();
    if (!TryFindLivingTarget(targets, previousTargetSlot, out _)) {
      npc.RecordEyeOfCthulhuEffect("TargetReacquire");
      SelectFinitePlayerTarget(
        targetSelectionAdapter,
        movement.Position,
        npcWidth,
        npcHeight);
    }

    int targetSlot = npc.CaptureTargetSlot();
    bool targetIsAvailable = TryFindLivingTarget(
      targets,
      targetSlot,
      out NpcAiTargetSnapshot target);
    bool targetIsDead = !targetIsAvailable &&
      targets.Any(candidate => candidate.Slot == previousTargetSlot && candidate.IsDead);
    NpcAiStateComponent aiState = npc.CaptureAiState();
    NpcEyeOfCthulhuProfileInput input = new(
      TypeId: npc.Definition.TypeId,
      NetId: npc.Definition.NetId,
      AiStyle: npc.Definition.Spawn.AiStyle,
      State: new NpcEyeOfCthulhuProfileState(
        aiState.State0,
        aiState.State1,
        aiState.State2,
        aiState.State3),
      Position: movement.Position,
      Velocity: movement.Velocity,
      TargetCenter: targetIsAvailable ? target.Center : Vector2.Zero,
      Width: npcWidth,
      Height: npcHeight,
      Life: npc.CurrentLife,
      LifeMax: npc.MaximumLife,
      TargetIsAvailable: targetIsAvailable,
      TargetIsDead: targetIsDead,
      DayTime: session.World.TimeWeather.DayTime,
      ExpertMode: session.World.Rules.GameMode is
        Terraria.WorldSession.Components.WorldGameMode.Expert or
        Terraria.WorldSession.Components.WorldGameMode.Master,
      GetGoodWorld: (session.World.Rules.SecretSeeds & WorldSessionSecretSeedFlags.ForTheWorthy) != 0,
      DustRoll: 0)
    {
      TargetPosition = targetIsAvailable ? target.Geometry.Position : Vector2.Zero,
    };
    var randomPort = new RuntimeNpcEyeOfCthulhuRandomPort(npc, _npcAiRandom);
    NpcEyeOfCthulhuProfileResult result = NpcEyeOfCthulhuProfile.EvaluateWithRandom(
      in input,
      randomPort);
    npc.RecordEyeOfCthulhuEffect(
      result.IsSupported ? "ProfileSupported:true" : "ProfileSupported:false");
    NpcAiStateComponent committedState = new(
      aiState.Style,
      result.State.Ai0,
      result.State.Ai1,
      result.State.Ai2,
      result.State.Ai3,
      aiState.Timer,
      aiState.LocalAi0,
      aiState.LocalAi1,
      aiState.LocalAi2,
      aiState.LocalAi3);
    npc.CommitAiState(in committedState, npc.BehaviorAction);
    if (result.TargetResetRequested)
    {
      npc.CommitTargetSelection(-1);
    }

    npc.CommitMovementPhysicsFlags(noGravity: true, noTileCollide: true);
    NpcEyeOfCthulhuProfile.ApplyEffects(
      in input,
      in result,
      new RuntimeNpcEyeOfCthulhuEffectPort(this, npc, session.World.Descriptor.WorldId),
      randomPort);
    movement.Velocity = result.Velocity;
    Vector2 nextPosition = movement.Position + movement.Velocity;
    Vector2 boundedPosition = ClampEntityPositionToWorld(
      nextPosition,
      npc.Collider,
      npcWidth,
      npcHeight,
      session.World.Descriptor.SizeX * 16f,
      session.World.Descriptor.SizeY * 16f);
    movement.Position = boundedPosition;
    movement.Velocity = new Vector2(
      boundedPosition.X == nextPosition.X ? movement.Velocity.X : 0f,
      boundedPosition.Y == nextPosition.Y ? movement.Velocity.Y : 0f);
    movement.IsGrounded = false;
    npc.CommitMovementCollision(false, false);
    npc.Movement = movement;
  }

  private void UpdateServantOfCthulhuMovement(
    RuntimeNpcEntity npc,
    LoadedWorldSession session,
    MovementStateComponent movement,
    IReadOnlyList<NpcAiTargetSnapshot> targets,
    int npcWidth,
    int npcHeight,
    RuntimeNpcTargetSelectionAdapter targetSelectionAdapter)
  {
    NpcTargetSelectionResult targetSelection = SelectFinitePlayerTarget(
      targetSelectionAdapter,
      movement.Position,
      npcWidth,
      npcHeight);

    NpcAiTargetSnapshot? target = null;
    if (targetSelection.HasTarget)
    {
      foreach (NpcAiTargetSnapshot candidate in targets)
      {
        if (candidate.Slot == targetSelection.LegacyTargetIndex && candidate.IsLiving)
        {
          target = candidate;
          break;
        }
      }
    }

    NpcServantOfCthulhuProfileInput input = new(
      TypeId: npc.Definition.TypeId,
      NetId: npc.Definition.NetId,
      AiStyle: npc.Definition.Spawn.AiStyle,
      Position: movement.Position,
      Velocity: movement.Velocity,
      TargetCenter: target?.Center ?? Vector2.Zero,
      Width: npcWidth,
      Height: npcHeight);
    NpcServantOfCthulhuProfileResult result =
      NpcServantOfCthulhuProfile.Evaluate(in input);
    if (!result.IsSupported)
    {
      throw new InvalidOperationException(
        "Servant of Cthulhu movement returned an unsupported result.");
    }

    npc.CommitMovementPhysicsFlags(noGravity: true, noTileCollide: true);
    movement.Velocity = result.Velocity;
    Vector2 nextPosition = movement.Position + movement.Velocity;
    Vector2 boundedPosition = ClampEntityPositionToWorld(
      nextPosition,
      npc.Collider,
      npcWidth,
      npcHeight,
      session.World.Descriptor.SizeX * 16f,
      session.World.Descriptor.SizeY * 16f);
    movement.Position = boundedPosition;
    movement.Velocity = new Vector2(
      boundedPosition.X == nextPosition.X ? movement.Velocity.X : 0f,
      boundedPosition.Y == nextPosition.Y ? movement.Velocity.Y : 0f);
    movement.IsGrounded = false;
    npc.CommitMovementCollision(false, false);
    npc.Movement = movement;
  }

  private void UpdateFloatingEyeMovement(
    RuntimeNpcEntity npc,
    LoadedWorldSession session,
    MovementStateComponent movement,
    IReadOnlyList<NpcAiTargetSnapshot> targets,
    int npcWidth,
    int npcHeight,
    Vector2 oldVelocity,
    bool previousCollideX,
    bool previousCollideY,
    bool previousNoTileCollide,
    bool wet,
    RuntimeNpcTargetSelectionAdapter targetSelectionAdapter)
  {
    RuntimeNpcEntity.NpcDirectionSnapshot direction = npc.CaptureDirection();
    NpcFloatingEyeProfileInput input = new(
      TypeId: npc.Definition.TypeId,
      NetId: npc.Definition.NetId,
      AiStyle: npc.Definition.Spawn.AiStyle,
      Position: movement.Position,
      Velocity: movement.Velocity,
      OldVelocity: oldVelocity,
      Width: npcWidth,
      Height: npcHeight,
      Scale: npc.Definition.Movement.Scale,
      Direction: direction.Sprite == 0 ? 1 : direction.Sprite,
      DirectionY: direction.Vertical == 0 ? 1 : direction.Vertical,
      TargetSlot: npc.CaptureTargetSlot(),
      NoTileCollide: previousNoTileCollide,
      CollideX: previousCollideX,
      CollideY: previousCollideY,
      DayTime: session.World.TimeWeather.DayTime,
      ZoneGraveyard: false,
      WorldSurfacePixels: (float)session.World.Descriptor.SurfaceLayer * 16f,
      Wet: wet,
      DustRoll: null);
    NpcFloatingEyeProfileResult result = NpcFloatingEyeProfile.EvaluateWithRandom(
      in input,
      new RuntimeNpcRandomPort(_npcAiRandom));
    npc.CommitDirection(result.Direction, result.DirectionY);
    npc.CommitMovementPhysicsFlags(result.NoGravity, result.NoTileCollide);
    NpcFloatingEyeProfile.ApplyEffects(
      in input,
      in result,
      new RuntimeNpcFloatingEyeEffectPort(
        npc,
        movement.Position,
        npcWidth,
        npcHeight,
        targetSelectionAdapter));
    movement.Velocity = result.Velocity;
    if (result.NoTileCollide) {
      float nextX = movement.Position.X + movement.Velocity.X;
      float nextY = movement.Position.Y + movement.Velocity.Y;
      Vector2 boundedPosition = ClampEntityPositionToWorld(
        new Vector2(nextX, nextY),
        npc.Collider,
        npcWidth,
        npcHeight,
        session.World.Descriptor.SizeX * 16f,
        session.World.Descriptor.SizeY * 16f);
      movement.Position = boundedPosition;
      movement.Velocity = new Vector2(
        boundedPosition.X == nextX ? movement.Velocity.X : 0f,
        boundedPosition.Y == nextY ? movement.Velocity.Y : 0f);
      npc.CommitMovementCollision(false, false);
      npc.Movement = movement;
      return;
    }

    LegacyVector2 requestedVelocity = new(movement.Velocity.X, movement.Velocity.Y);
    LegacyVector2 resolvedVelocity = Collision.TileCollision(
      GetLegacyColliderPosition(movement.Position, npc.Collider),
      requestedVelocity,
      npcWidth,
      npcHeight);
    bool collidedX = MathF.Abs(resolvedVelocity.X - requestedVelocity.X) > 0.001f;
    bool collidedY = MathF.Abs(resolvedVelocity.Y - requestedVelocity.Y) > 0.001f;
    movement.Position += new Vector2(resolvedVelocity.X, resolvedVelocity.Y);
    movement.Velocity = new Vector2(
      collidedX ? 0f : resolvedVelocity.X,
      collidedY ? 0f : resolvedVelocity.Y);
    movement.IsGrounded = false;
    npc.CommitMovementCollision(collidedX, collidedY);
    npc.Movement = movement;
  }

  private void UpdateFighterMovement(
    RuntimeNpcEntity npc,
    LoadedWorldSession session,
    MovementStateComponent movement,
    IReadOnlyList<NpcAiTargetSnapshot> targets,
    int npcWidth,
    int npcHeight,
    NpcGravityResult gravity,
    RuntimeNpcTargetSelectionAdapter targetSelectionAdapter)
  {
    ColliderComponent collider = npc.Collider;
    NpcAiStateComponent aiState = npc.CaptureAiState();
    RuntimeNpcEntity.NpcDirectionSnapshot direction = npc.CaptureDirection();
    RuntimeNpcEntity.NpcMovementTickSnapshot previousMovementTick =
      npc.CaptureMovementTickSnapshot();
    bool justHit = npc.ConsumeJustHit();
    int targetSlot = npc.CaptureTargetSlot();
    NpcAiTargetSnapshot? target = null;
    foreach (NpcAiTargetSnapshot candidate in targets)
    {
      if (candidate.Slot == targetSlot)
      {
        target = candidate;
        break;
      }
    }

    NpcFighterProfileResult result = NpcFighterProfile.Evaluate(
      new NpcFighterProfileInput(
        TypeId: npc.Definition.TypeId,
        NetId: npc.Definition.NetId,
        AiStyle: npc.Definition.Spawn.AiStyle,
        Position: movement.Position,
        Velocity: movement.Velocity,
        State: new NpcFighterProfileState(
          aiState.State0,
          aiState.State1,
          aiState.State2,
          aiState.State3),
        Direction: direction.Sprite,
        TargetSlot: targetSlot,
        Scale: npc.Definition.Movement.Scale,
        DayTime: session.World.TimeWeather.DayTime,
        IsBelowSurface: movement.Position.Y > session.World.Descriptor.SurfaceLayer * 16f,
        IsGrounded: movement.IsGrounded || previousMovementTick.CollideY,
        JustHit: justHit,
        Traversal: CaptureFighterTraversal(
          session,
          movement.Position,
          npcWidth,
          npcHeight,
          collider,
          direction.Sprite,
          targets,
          targetSlot),
        TargetCenter: target?.Center ?? Vector2.Zero,
        TargetHeight: target is null ? 0 : target.Value.Geometry.Height,
        TargetIsAvailable: target is { IsActive: true, IsDead: false },
        Height: npcHeight,
        DirectionY: direction.Vertical));

    npc.CommitDirection(result.Direction, result.DirectionY);
    npc.CommitAiState(
      new NpcAiStateComponent(
        aiState.Style,
        result.State.Ai0,
        result.State.Ai1,
        result.State.Ai2,
        result.State.Ai3,
        aiState.Timer,
        aiState.LocalAi0,
        aiState.LocalAi1,
        aiState.LocalAi2,
        aiState.LocalAi3),
      result.AiAction);
    NpcFighterProfile.ApplyEffects(
      in result,
      new RuntimeNpcFighterEffectPort(
        npc,
        session,
        movement.Position,
        npcWidth,
        npcHeight,
        targetSelectionAdapter));

    npc.CommitMovementPhysicsFlags(noGravity: false, noTileCollide: false);
    movement.Velocity = result.Velocity;
    if (result.JumpRequested && movement.Velocity.Y >= 0f)
    {
      movement.Velocity.Y = result.JumpVelocityY;
      movement.IsGrounded = false;
      npc.CommitJumpEffect(result.JumpVelocityY);
    }
    movement.Velocity.Y = Math.Min(
      movement.Velocity.Y + gravity.Gravity,
      gravity.MaximumFallSpeed);

    LegacyVector2 requestedVelocity = new(movement.Velocity.X, movement.Velocity.Y);
    LegacyVector2 resolvedVelocity = Collision.TileCollision(
      GetLegacyColliderPosition(movement.Position, npc.Collider),
      requestedVelocity,
      npcWidth,
      npcHeight);
    bool collidedX = MathF.Abs(resolvedVelocity.X - requestedVelocity.X) > 0.001f;
    bool collidedY = MathF.Abs(resolvedVelocity.Y - requestedVelocity.Y) > 0.001f;
    movement.Position += new Vector2(resolvedVelocity.X, resolvedVelocity.Y);
    movement.Velocity = new Vector2(
      collidedX ? 0f : resolvedVelocity.X,
      collidedY ? 0f : resolvedVelocity.Y);
    movement.IsGrounded = collidedY && requestedVelocity.Y > 0f && Collision.down;
    npc.CommitMovementCollision(collidedX, collidedY);
    if (collidedX && movement.IsGrounded && result.AiAction != 0)
    {
      movement.Velocity.Y = -5f;
      movement.IsGrounded = false;
    }

    npc.Movement = movement;
  }

  private static NpcFighterTraversalInput CaptureFighterTraversal(
    LoadedWorldSession session,
    Vector2 position,
    int width,
    int height,
    ColliderComponent collider,
    int direction,
    IReadOnlyList<NpcAiTargetSnapshot> targets,
    int targetSlot)
  {
    int safeDirection = direction == 0 ? 1 : direction;
    Vector2 colliderPosition = GetColliderPosition(position, collider);
    int tileX = (int)((colliderPosition.X + (width / 2f) + (15f * safeDirection)) / 16f);
    int tileY = (int)((colliderPosition.Y + height - 15f) / 16f);
    bool solidOne = WorldGen.SolidTileNoPlatforms(tileX, tileY - 1);
    bool solidTwo = WorldGen.SolidTileNoPlatforms(tileX, tileY - 2);
    bool solidThree = WorldGen.SolidTileNoPlatforms(tileX, tileY - 3);
    bool door = TryGetTile(tileX, tileY - 1, out Tile? doorTile) &&
      doorTile is not null &&
      doorTile.active() &&
      (doorTile.type == TileID.ClosedDoor || doorTile.type == TileID.TallGateClosed);
    bool targetAbove = false;
    bool targetLineOfSight = false;
    foreach (NpcAiTargetSnapshot target in targets)
    {
      if (target.Slot != targetSlot || !target.IsActive || target.IsDead)
      {
        continue;
      }

      targetAbove = target.Center.Y + 50f < colliderPosition.Y;
      targetLineOfSight = Collision.CanHit(
        new LegacyVector2(colliderPosition.X, colliderPosition.Y),
        width,
        height,
        new LegacyVector2(target.Geometry.Position.X, target.Geometry.Position.Y),
        target.Geometry.Width,
        target.Geometry.Height);
      break;
    }

    return new NpcFighterTraversalInput(
      solidOne,
      solidTwo,
      solidThree,
      door,
      door && IsWithinWorld(session, tileX, tileY - 1),
      tileX,
      tileY - 1,
      targetAbove,
      targetLineOfSight,
      ExpertMode: false);
  }

  private static bool TryGetTile(int x, int y, out Tile? tile)
  {
    if (!WorldGen.InWorld(x, y, 4))
    {
      tile = null;
      return false;
    }

    tile = Main.tile[x, y];
    return tile is not null;
  }

  private static bool IsWithinWorld(LoadedWorldSession session, int x, int y)
  {
    return (uint)x < (uint)session.Storage.TileMap.Width &&
      (uint)y < (uint)session.Storage.TileMap.Height;
  }

  private static NpcTargetGeometrySnapshot CaptureColliderGeometry(
    LocationComponent location,
    ColliderComponent collider)
  {
    return new NpcTargetGeometrySnapshot(
      new Vector2(
        location.X + collider.OffsetX,
        location.Y + collider.OffsetY),
      GetCollisionWidth(collider),
      GetCollisionHeight(collider));
  }

  private static Vector2 GetColliderPosition(
    Vector2 location,
    ColliderComponent collider)
  {
    return location + new Vector2(collider.OffsetX, collider.OffsetY);
  }

  private static LegacyVector2 GetLegacyColliderPosition(
    Vector2 location,
    ColliderComponent collider)
  {
    Vector2 colliderPosition = GetColliderPosition(location, collider);
    return new LegacyVector2(colliderPosition.X, colliderPosition.Y);
  }

  private static int GetCollisionWidth(ColliderComponent collider)
  {
    return Math.Max(1, (int)collider.Width);
  }

  private static int GetCollisionHeight(ColliderComponent collider)
  {
    return Math.Max(1, (int)collider.Height);
  }

  private static Vector2 ClampEntityPositionToWorld(
    Vector2 position,
    ColliderComponent collider,
    int width,
    int height,
    float worldWidth,
    float worldHeight)
  {
    float minimumX = -collider.OffsetX;
    float minimumY = -collider.OffsetY;
    float maximumX = Math.Max(minimumX, worldWidth - collider.OffsetX - width);
    float maximumY = Math.Max(minimumY, worldHeight - collider.OffsetY - height);
    return new Vector2(
      Math.Clamp(position.X, minimumX, maximumX),
      Math.Clamp(position.Y, minimumY, maximumY));
  }

  private static bool ShouldDespawnNaturally(
    RuntimeNpcEntity npc,
    RuntimePlayerStore players)
  {
    if (!npc.IsNaturallySpawned)
    {
      return false;
    }

    if (!SimulationContentSupportManifest.TryGetNpcNaturalDespawnPolicy(
          npc.Definition.NetId,
          out SimulationContentSupportManifest.NpcNaturalDespawnPolicy policy))
    {
      throw new InvalidOperationException(
        $"NPC {npc.Definition.NetId} has no natural despawn policy.");
    }

    if (policy == SimulationContentSupportManifest.NpcNaturalDespawnPolicy.Persistent)
    {
      return false;
    }

    if (policy != SimulationContentSupportManifest.NpcNaturalDespawnPolicy
          .OutsideLivingPlayerRangeAfterGracePeriod)
    {
      throw new InvalidOperationException(
        $"NPC {npc.Definition.NetId} has unsupported natural despawn policy {policy}.");
    }

    Vector2 npcCenter = CaptureColliderGeometry(npc.Location, npc.Collider).Center;
    float rangeSquared = NaturalDespawnRangePixels * NaturalDespawnRangePixels;
    bool hasNearbyPlayer = players.Players.Any(player =>
    {
      if (player.Lifecycle.IsDead)
      {
        return false;
      }

      Vector2 playerCenter = CaptureColliderGeometry(player.Location, player.Collider).Center;
      float deltaX = npcCenter.X - playerCenter.X;
      float deltaY = npcCenter.Y - playerCenter.Y;
      return deltaX * deltaX + deltaY * deltaY <= rangeSquared;
    });
    if (hasNearbyPlayer)
    {
      if (!npc.TryResetOutsidePlayerRangeTicks())
      {
        throw new InvalidOperationException("An NPC natural despawn state could not be reset.");
      }

      return false;
    }

    if (!npc.TryAdvanceOutsidePlayerRangeTicks(
          NaturalDespawnGraceTicks,
          out bool shouldDespawn))
    {
      throw new InvalidOperationException("An NPC natural despawn state could not be advanced.");
    }

    return shouldDespawn;
  }

  public bool WasUpdatedAtEveryOccupiedSlot(long tickNumber)
  {
    for (int index = 0; index < _entities.Capacity; index++)
    {
      if (_entities.TryGetOccupiedAt(index, out _, out _, out RuntimeNpcEntity? entity) &&
          (entity is null || entity.LastBehaviorUpdatedTick != tickNumber))
      {
        return false;
      }
    }

    return true;
  }

  private static EntitySlotStore<RuntimeNpcEntity, NpcRuntimeSlot> CreateEntityStore()
  {
    return new EntitySlotStore<RuntimeNpcEntity, NpcRuntimeSlot>(
      static slot => slot.Value,
      static value => new NpcRuntimeSlot(value),
      maximumCapacity: MaximumNpcCapacity);
  }

  internal IReadOnlyList<NpcNpcTargetSnapshot> CaptureNpcNpcTargetSnapshots()
  {
    var snapshots = new List<NpcNpcTargetSnapshot>(_entities.ActiveCount);
    for (int index = 0; index < _entities.Capacity; index++)
    {
      if (!_entities.TryGetOccupiedAt(index, out NpcRuntimeSlot slot, out _, out RuntimeNpcEntity? npc) ||
          npc is null ||
          !npc.IsActive)
      {
        continue;
      }

      snapshots.Add(new NpcNpcTargetSnapshot(
        slot.Value,
        npc.Definition.TypeId,
        CaptureColliderGeometry(npc.Location, npc.Collider),
        IsActive: true));
    }

    return snapshots.AsReadOnly();
  }

  internal static NpcTargetSelectionResult SelectFinitePlayerTarget(
    RuntimeNpcTargetSelectionAdapter adapter,
    Vector2 position,
    int width,
    int height,
    NpcTargetSelectionStrategy? strategy = null,
    Vector2? checkPosition = null,
    bool faceTarget = true)
  {
    ArgumentNullException.ThrowIfNull(adapter);
    return adapter.SelectAndCommit(position, width, height, strategy, checkPosition, faceTarget);
  }

  private sealed class RuntimeNpcRandomPort(Random random) : INpcFloatingEyeRandomPort
  {
    public int Next(int maxExclusive)
    {
      return random.Next(maxExclusive);
    }
  }

  private sealed class RuntimeNpcFloatingEyeEffectPort(
    RuntimeNpcEntity npc,
    Vector2 position,
    int width,
    int height,
    RuntimeNpcTargetSelectionAdapter targetSelectionAdapter) : INpcFloatingEyeProfileEffectPort
  {
    public void EncourageDespawn(int ticks)
    {
      npc.CommitDespawnEncouragement(ticks);
    }

    public void RequestTargetReacquire()
    {
      SelectFinitePlayerTarget(
        targetSelectionAdapter,
        position,
        width,
        height);
    }

    public void SpawnDust(Vector2 dustPosition, int dustWidth, int dustHeight, Vector2 velocity)
    {
      npc.CommitDustEffect(dustPosition, velocity);
    }
  }

  private sealed class RuntimeNpcBlueSlimeEffectPort(
    RuntimeNpcStore store,
    RuntimeNpcEntity npc,
    LoadedWorldSession session,
    NSSLC.WorldGeneration.Utilities.UnifiedRandom random,
    Vector2 position,
    int width,
    int height,
    RuntimeNpcTargetSelectionAdapter targetSelectionAdapter) : INpcBlueSlimeProfileEffectPort
  {
    public NpcBlueSlimeTypeOneSelectionResult GenerateContainedItem(
      bool isBallooned)
    {
      NpcAiStateComponent aiState = npc.CaptureAiState();
      Vector2 colliderPosition = GetColliderPosition(position, npc.Collider);
      Vector2 center = colliderPosition + new Vector2(width * 0.5f, height * 0.5f);
      double surfaceTiles = session.World.Descriptor.SurfaceLayer;
      double rockLayer = session.World.Descriptor.RockLayer;
      int positionTileY = (int)(colliderPosition.Y / 16f);
      int centerTileY = (int)(center.Y / 16f);
      bool remixWorld = HasSecretSeed(WorldSessionSecretSeedFlags.Remix);
    NpcBlueSlimeTypeOneSelectionInput input = new(
        CurrentItemState: aiState.State1,
        NetId: npc.Definition.NetId,
        NpcValue: npc.Definition.Core.Value,
        PositionY: colliderPosition.Y,
        CenterY: center.Y,
        WorldSurfaceTiles: surfaceTiles,
        PositionInRockLayer: remixWorld
          ? positionTileY > surfaceTiles && positionTileY <= rockLayer
          : positionTileY > rockLayer,
        CenterInRockLayer: remixWorld
          ? centerTileY > surfaceTiles && centerTileY <= rockLayer
          : centerTileY > rockLayer,
        NoTrapsWorld: HasSecretSeed(WorldSessionSecretSeedFlags.NoTraps),
        GetGoodWorld: HasSecretSeed(WorldSessionSecretSeedFlags.GetGoodWorld),
        RemixWorld: remixWorld,
        VampireSeed: HasSecretSeed(WorldSessionSecretSeedFlags.Vampire),
        SlimeRain: session.World.TimeWeather.SlimeRain,
        GenuineParty: session.World.TimeWeather.BirthdayParty.GenuineParty,
        IsBallooned: isBallooned,
        IsSkyblockWorld: session.World.Rules.IsSkyblockWorld,
        HardMode: session.World.Rules.HardMode,
        MoonPhase: session.World.TimeWeather.MoonPhase,
        NetMode: Main.netMode,
        LowTiles: WorldGen.Skyblock.lowTiles,
        NoLifeCrystals: WorldGen.Skyblock.noLifeCrystals,
        AnyLifeCrystalSlime: store.HasLifeCrystalSlime());
      NpcBlueSlimeTypeOneSelectionResult selection =
        NpcBlueSlimeContainedItemGenerator.SelectForTypeOne(
          in input,
          new RuntimeNpcBlueSlimeRandomPort(random));
      if (selection.Attempted)
      {
        npc.CommitAiState(
          new NpcAiStateComponent(
            aiState.Style,
            aiState.State0,
            selection.ItemState,
            aiState.State2,
            aiState.State3,
            aiState.Timer,
            aiState.LocalAi0,
            aiState.LocalAi1,
            aiState.LocalAi2,
            aiState.LocalAi3),
          npc.BehaviorAction);
      }

      return selection;
    }

    public void RequestNetworkSync()
    {
      npc.CommitNetworkUpdateIntent();
    }

    public void RequestTargetReacquire()
    {
      SelectFinitePlayerTarget(
        targetSelectionAdapter,
        position,
        width,
        height);
    }

    private bool HasSecretSeed(WorldSessionSecretSeedFlags seed)
    {
      return (session.World.Rules.SecretSeeds & seed) != 0;
    }
  }

  private sealed class RuntimeNpcMotherSlimeEffectPort(
    RuntimeNpcEntity npc,
    Vector2 position,
    int width,
    int height,
    RuntimeNpcTargetSelectionAdapter targetSelectionAdapter) : INpcMotherSlimeProfileEffectPort
  {
    public void RequestNetworkSync()
    {
      npc.CommitNetworkUpdateIntent();
    }

    public void RequestTargetReacquire()
    {
      SelectFinitePlayerTarget(
        targetSelectionAdapter,
        position,
        width,
        height);
    }
  }

  private sealed class RuntimeNpcBlueSlimeRandomPort(
    NSSLC.WorldGeneration.Utilities.UnifiedRandom random)
    : INpcBlueSlimeRandomPort
  {
    public int Next(int maxExclusive)
    {
      return random.Next(maxExclusive);
    }

    public int Next(int minInclusive, int maxExclusive)
    {
      return random.Next(minInclusive, maxExclusive);
    }

    public int GetRandomVoiceItem()
    {
      return NSSLC.WorldGeneration.Item.GetRandomVoiceItem();
    }
  }

  private sealed class RuntimeNpcMotherSlimeDeathSplitEffectPort(
    RuntimeNpcStore store,
    RuntimeNpcEntity parent,
    LoadedWorldSession session,
    ContentCatalog catalog,
    NSSLC.WorldGeneration.Utilities.UnifiedRandom random)
    : INpcMotherSlimeDeathSplitEffectPort
  {
    public int MaxNpcSlots => MaximumNpcCapacity;

    public int Next(int maxExclusive)
    {
      return random.Next(maxExclusive);
    }

    public int Next(int minInclusive, int maxExclusive)
    {
      return random.Next(minInclusive, maxExclusive);
    }

    public int SpawnBlueSlimeChild(
      NpcInstanceId sourceNpcInstanceId,
      int positionX,
      int positionY)
    {
      if (sourceNpcInstanceId != parent.InstanceId ||
          !store.TrySpawn(
            1,
            new Vector2(positionX, positionY),
            catalog,
            session.World.Descriptor.WorldId,
            out RuntimeNpcEntity? child) ||
          child is null)
      {
        return -1;
      }

      return child.Slot.Value;
    }

    public void ConfigureBlueSlimeChild(
      int npcSlot,
      Vector2 velocity,
      float ai0)
    {
      if (!store._entities.TryGetOccupiedAt(
            npcSlot,
            out _,
            out _,
            out RuntimeNpcEntity? child) ||
          child is null)
      {
        throw new InvalidOperationException(
          "The Mother Slime split child slot could not be resolved after spawning.");
      }

      child.ApplySmallBlueSlimeDefaults();
      child.CommitAiState(
        new NpcAiStateComponent(
          child.Definition.Spawn.AiStyle,
          ai0,
          0f,
          0f,
          0f,
          0),
        child.BehaviorAction);
      MovementStateComponent movement = child.Movement;
      movement.Velocity = velocity;
      movement.IsGrounded = false;
      child.Movement = movement;
    }

    public void SendNpcSyncPacket(int npcSlot)
    {
      if (!store._entities.TryGetOccupiedAt(
            npcSlot,
            out _,
            out _,
            out RuntimeNpcEntity? child) ||
          child is null)
      {
        throw new InvalidOperationException(
          "The Mother Slime split child slot could not be resolved for synchronization.");
      }

      child.CommitNetworkUpdateIntent();
    }
  }

  private sealed class RuntimeNpcFighterEffectPort(
    RuntimeNpcEntity npc,
    LoadedWorldSession session,
    Vector2 position,
    int width,
    int height,
    RuntimeNpcTargetSelectionAdapter targetSelectionAdapter) : INpcFighterProfileEffectPort
  {
    public void EncourageDespawn(int ticks)
    {
      npc.CommitDespawnEncouragement(ticks);
    }

    public void RequestTargetReacquire()
    {
      SelectFinitePlayerTarget(
        targetSelectionAdapter,
        position,
        width,
        height);
    }

    public void RequestNetworkSync()
    {
      npc.CommitNetworkUpdateIntent();
    }

    public void RequestOpenDoor(int tileX, int tileY, int direction)
    {
      npc.CommitDoorOpenEffect(tileX, tileY, direction);
      if (!WorldGen.OpenDoor(tileX, tileY, direction))
      {
        return;
      }

      var changedCoordinates = new List<TileCoordinate>();
      for (int x = tileX - 1; x <= tileX + 1; x++)
      {
        for (int y = tileY; y <= tileY + 2; y++)
        {
          if (IsWithinWorld(session, x, y))
          {
            changedCoordinates.Add(new TileCoordinate(x, y));
          }
        }
      }

      WorldStorageOperationResult commit =
        LegacyWorldTileMapProjection.CommitLegacyTileMutations(
          session,
          changedCoordinates);
      if (!commit.Succeeded)
      {
        throw new InvalidOperationException(
          $"The Fighter door mutation could not be committed: " +
          $"{commit.Failure.Kind} {commit.Failure.Detail}");
      }
    }
  }

  private static bool TryFindLivingTarget(
    IReadOnlyList<NpcAiTargetSnapshot> targets,
    int targetSlot,
    out NpcAiTargetSnapshot target)
  {
    foreach (NpcAiTargetSnapshot candidate in targets)
    {
      if (candidate.Slot == targetSlot && candidate.IsLiving)
      {
        target = candidate;
        return true;
      }
    }

    target = default;
    return false;
  }

  private sealed class RuntimeNpcEyeOfCthulhuRandomPort(
    RuntimeNpcEntity npc,
    Random random) : INpcEyeOfCthulhuRandomPort
  {
    public int Next(int maxExclusive)
    {
      int value = random.Next(maxExclusive);
      npc.RecordEyeOfCthulhuEffect($"Random.Next({maxExclusive})={value}");
      return value;
    }

    public int Next(int minInclusive, int maxExclusive)
    {
      int value = random.Next(minInclusive, maxExclusive);
      npc.RecordEyeOfCthulhuEffect(
        $"Random.Next({minInclusive},{maxExclusive})={value}");
      return value;
    }
  }

  private sealed class RuntimeNpcEyeOfCthulhuEffectPort(
    RuntimeNpcStore store,
    RuntimeNpcEntity npc,
    int worldId)
    : INpcEyeOfCthulhuProfileEffectPort
  {
    public void SpawnDust(Vector2 position, int width, int height, Vector2 velocity)
    {
      npc.RecordEyeOfCthulhuEffect("SpawnDust");
      npc.CommitDustEffect(position, velocity);
    }

    public bool TrySpawnServant(Vector2 position, Vector2 velocity)
    {
      // The shared NPC tick gate prevents clients from evaluating the Eye
      // profile. Keep this effect-level guard as defense in depth for direct
      // calls: a client may observe the authority rejection, but it must not
      // allocate a replicated child or consume a slot.
      if (Main.netMode == 1)
      {
        npc.RecordEyeOfCthulhuEffect("ServantSpawnSkipped:ClientAuthority");
        return false;
      }

      ContentCatalog catalog = store._contentCatalog ??
        throw new InvalidOperationException(
          "The Eye of Cthulhu cannot summon without an active content catalog.");
      if (!catalog.Npcs.TryGetByNetId(
            SimulationContentSupportManifest.ServantOfCthulhuNetId,
            out _))
      {
        throw new InvalidOperationException(
          "Servant of Cthulhu is missing from the validated simulation content catalog.");
      }

      if (!store.TrySpawn(
            SimulationContentSupportManifest.ServantOfCthulhuNetId,
            position,
            catalog,
            worldId,
            out RuntimeNpcEntity? servant) ||
          servant is null)
      {
        npc.RecordEyeOfCthulhuEffect("ServantSpawnRejected:Capacity");
        return false;
      }

      MovementStateComponent movement = servant.Movement;
      movement.Velocity = velocity;
      servant.Movement = movement;
      npc.RecordEyeOfCthulhuEffect($"ServantSpawned(slot={servant.Slot.Value})");
      return true;
    }

    public void PlaySound(int soundId, Vector2 position)
    {
      npc.RecordEyeOfCthulhuEffect(
        $"PlaySound({soundId},{(int)position.X},{(int)position.Y})");
    }

    public void SpawnGore(int goreId, Vector2 position, Vector2 velocity)
    {
      npc.RecordEyeOfCthulhuEffect($"SpawnGore({goreId})");
    }

    public void SetReflectsProjectiles(bool reflectsProjectiles)
    {
      npc.RecordEyeOfCthulhuEffect($"ReflectsProjectiles:{reflectsProjectiles}");
    }

    public void BeginDash(Vector2 velocity)
    {
      npc.RecordEyeOfCthulhuEffect("BeginDash");
    }

    public void EncourageDespawn(int ticks)
    {
      npc.RecordEyeOfCthulhuEffect($"EncourageDespawn({ticks})");
      npc.CommitDespawnEncouragement(ticks);
    }

    public void RequestNetworkUpdate()
    {
      npc.RecordEyeOfCthulhuEffect("RequestNetworkUpdate");
      npc.CommitNetworkUpdateIntent();
    }
  }

  private sealed class SilentDamageOverTimeTextPort : INpcDamageOverTimeTextPort
  {
    public bool TryPublish(in NpcDamageOverTimeTextIntent intent) => false;
  }
}

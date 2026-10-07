using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;

using EntityEcs;
using EntityEcs.Components;

using Terraria.Content;
using Terraria.Npc;
using Terraria.Npc.Network;
using Terraria.NonAuthoritative.Persistence;
using Terraria.Projectile;
using Terraria.Relationships;
using Terraria.SpatialSimulation;
using Terraria.WorldStorage;

namespace Terraria.Network;

/// <summary>Applies Social packet NPC effects to the host's shared world runtime.</summary>
/// <remarks>
/// Persisted NPC records are hydrated into that same runtime and linked by their existing
/// legacy slot projection plus slot generation. This owner keeps no runtime or component
/// references between commands; every result is a detached snapshot.
/// </remarks>
public sealed class SocialNpcEffectOwner
{
  private const int DryadTypeId = 20;
  private const int DryadNetId = 20;
  private const int StardewProjectileType = 995;
  private const int NpcTownEmoteAiStyle = 7;
  private const int NpcReactionAiState = 19;
  private const int NpcReactionAiTimer = 220;
  private const float DryadAnimationAiState = 24.0f;
  private const float DryadAnimationLifetime = 480.0f;
  private const float StardewProjectileYOffset = -52.0f;
  private const float StardewProjectileXOffset = 100.0f;
  private const int StardewTileScanOffset = 10;
  private const int StardewTileScanCount = 15;
  private const int StardewTileScanMargin = 10;

  private readonly IContentCatalog _content;
  private readonly NetworkWorldOwner _worldOwner;

  public SocialNpcEffectOwner(NetworkWorldOwner worldOwner, IContentCatalog content)
  {
    _worldOwner = worldOwner ?? throw new ArgumentNullException(nameof(worldOwner));
    _content = content ?? throw new ArgumentNullException(nameof(content));
  }

  /// <summary>
  /// Applies packet-120's town-NPC reaction using player facts captured from the same world.
  /// The source emote ID is intentionally absent: EmoteBubble.cs accepts it but never reads it.
  /// </summary>
  public ValueTask<SocialNpcEffectResult> ApplyPlayerEmoteAsync(
    EntityRuntimeId expectedWorldRuntimeId,
    SocialPlayerNpcInteractionSnapshot player,
    CancellationToken cancellationToken = default)
  {
    ValidateExpectedRuntimeId(expectedWorldRuntimeId);
    ValidatePlayerSnapshot(in player);
    cancellationToken.ThrowIfCancellationRequested();
    SocialPlayerNpcInteractionSnapshot playerSnapshot = player;

    return _worldOwner.InvokeAsync(
      session => ApplyPlayerEmoteCoreOnOwnerThread(
        session,
        expectedWorldRuntimeId,
        in playerSnapshot),
      cancellationToken);
  }

  /// <summary>
  /// Applies packet-120 NPC reactions during an existing world-owner callback.
  /// This method is synchronous and never queues nested world-owner work.
  /// </summary>
  public SocialNpcEffectResult ApplyPlayerEmoteOnOwnerThread(
    LoadedWorldSession session,
    EntityRuntimeId expectedWorldRuntimeId,
    in SocialPlayerNpcInteractionSnapshot player)
  {
    ArgumentNullException.ThrowIfNull(session);
    ValidateExpectedRuntimeId(expectedWorldRuntimeId);
    ValidatePlayerSnapshot(in player);
    return ApplyPlayerEmoteCoreOnOwnerThread(session, expectedWorldRuntimeId, in player);
  }

  /// <summary>
  /// Applies packet-144's Dryad animation and returns the projectile actually committed by
  /// <see cref="ProjectileLifecycleSystem"/>. No spawn plan is returned as a successful effect.
  /// </summary>
  public ValueTask<SocialNpcEffectResult> RequestDryadAnimationAsync(
    EntityRuntimeId expectedWorldRuntimeId,
    CancellationToken cancellationToken = default)
  {
    ValidateExpectedRuntimeId(expectedWorldRuntimeId);
    cancellationToken.ThrowIfCancellationRequested();

    return _worldOwner.InvokeAsync(
      session => RequestDryadAnimationOnOwnerThread(session, expectedWorldRuntimeId),
      cancellationToken);
  }

  private SocialNpcEffectResult ApplyPlayerEmoteCoreOnOwnerThread(
    LoadedWorldSession session,
    EntityRuntimeId expectedWorldRuntimeId,
    in SocialPlayerNpcInteractionSnapshot player)
  {
    if (!IsExpectedRuntime(session, expectedWorldRuntimeId) ||
        player.WorldRuntimeId != expectedWorldRuntimeId)
    {
      return new SocialNpcEffectResult(SocialNpcEffectStatus.StaleWorldRuntime);
    }

    HydratePersistedNpcs(session);
    NpcRuntimeState[] npcs = CaptureNpcs(session);
    bool playerCanBeTalkedTo = player.Active && !player.Dead &&
      !player.ShouldNotDraw && player.Stealth == 1.0f;
    Vector2 playerCenter = new(
      player.Position.X + player.Width * 0.5f,
      player.Position.Y + player.Height * 0.5f);
    var updated = new List<SocialNpcEffectSnapshot>();
    var lineOfSight = new SessionSpatialTileLookup(session.Storage.TileMap, _content.Tiles);

    foreach (NpcRuntimeState npc in npcs)
    {
      if (!npc.IsActive || !npc.IsTownNpc || npc.AiStyle != NpcTownEmoteAiStyle ||
          !(npc.Ai[0] < 2.0f))
      {
        continue;
      }

      Vector2 npcCenter = new(
        npc.Position.X + npc.Width * 0.5f,
        npc.Position.Y + npc.Height * 0.5f);
      bool playerIsNearAndTalkable = playerCanBeTalkedTo &&
        Vector2.Distance(playerCenter, npcCenter) < 200.0f;
      if (!playerIsNearAndTalkable &&
          CanNpcAndPlayerSeeEachOther(session, npc, player, lineOfSight))
      {
        continue;
      }

      ApplyNpcEffect(
        session.EntityRuntime,
        npc,
        ai0: NpcReactionAiState,
        ai1: NpcReactionAiTimer,
        ai2: player.PlayerSlot,
        horizontalDirection: npc.Position.X < player.Position.X ? 1 : -1,
        spriteDirection: null,
        localAi2: null,
        localAi3: null);
      updated.Add(CaptureNpcSnapshot(session, npc.Handle));
    }

    return updated.Count == 0
      ? new SocialNpcEffectResult(SocialNpcEffectStatus.NoEffect)
      : new SocialNpcEffectResult(SocialNpcEffectStatus.Applied, updated);
  }

  private SocialNpcEffectResult RequestDryadAnimationOnOwnerThread(
    LoadedWorldSession session,
    EntityRuntimeId expectedWorldRuntimeId)
  {
    if (!IsExpectedRuntime(session, expectedWorldRuntimeId))
    {
      return new SocialNpcEffectResult(SocialNpcEffectStatus.StaleWorldRuntime);
    }

    RequireDryadDefinition();
    ProjectileDefinition projectileDefinition = RequireStardewProjectileDefinition();
    HydratePersistedNpcs(session);
    NpcRuntimeState[] npcs = CaptureNpcs(session);
    NpcRuntimeState? dryad = null;
    foreach (NpcRuntimeState npc in npcs)
    {
      if (npc.IsActive && npc.TypeId == DryadTypeId)
      {
        dryad = npc;
        break;
      }
    }

    if (dryad is not NpcRuntimeState source)
    {
      return new SocialNpcEffectResult(SocialNpcEffectStatus.NoEffect);
    }

    Vector2 dryadBottom = new(
      source.Position.X + source.Width * 0.5f,
      source.Position.Y + source.Height);
    Vector2 tileScanOrigin = dryadBottom + new Vector2(StardewProjectileXOffset, 0.0f);
    Vector2 projectileCenter = FindStardewProjectileCenter(
      session,
      tileScanOrigin,
      _content.Tiles);

    ApplyNpcEffect(
      session.EntityRuntime,
      source,
      ai0: DryadAnimationAiState,
      ai1: DryadAnimationLifetime,
      ai2: 0.0f,
      horizontalDirection: 1,
      spriteDirection: 1,
      localAi2: DryadAnimationLifetime,
      localAi3: 0.0f);

    int catalogRevision = GetCatalogRevision();
    int npcCapacity = Math.Max(255, session.Storage.Npcs.Capacity);
    var hydrationContext = new ProjectileDefinitionHydrationContext(
      catalogRevision,
      npcCapacity,
      playerCapacity: byte.MaxValue);
    var spawn = new ProjectileSpawnCommand(
      projectileDefinition.TypeId,
      ProjectileOwnerReference.None,
      projectileCenter,
      Vector2.Zero,
      damage: 0,
      originalDamage: 0,
      knockback: 0.0f);
    var lifecycle = new ProjectileLifecycleSystem(session.Storage);
    if (!lifecycle.TrySpawn(
      spawn,
      _content.Projectiles,
      hydrationContext,
      out ProjectileHandle projectileHandle))
    {
      throw new InvalidOperationException(
        "Projectile 995 was not committed by the projectile lifecycle owner.");
    }

    SocialNpcEffectSnapshot updatedDryad = CaptureNpcSnapshot(session, source.Handle);
    SocialProjectileSpawnSnapshot spawnedProjectile = CaptureProjectileSnapshot(
      session,
      lifecycle,
      projectileHandle);
    return new SocialNpcEffectResult(
      SocialNpcEffectStatus.Applied,
      new[] { updatedDryad },
      spawnedProjectile);
  }

  private void HydratePersistedNpcs(LoadedWorldSession session)
  {
    EntityRuntime runtime = session.EntityRuntime;
    Dictionary<(int Slot, uint Generation), RuntimeEntityHandle> boundNpcs =
      new(BuildNpcBindingIndex(runtime));
    EntitySlotStore<WorldEntityState, Terraria.WorldStorage.NpcSlot> store =
      session.Storage.Npcs;

    for (int index = 0; index < store.Capacity; index++)
    {
      if (!store.TryGetOccupiedAt(
        index,
        out Terraria.WorldStorage.NpcSlot persistedSlot,
        out uint generation,
        out WorldEntityState? storedState))
      {
        continue;
      }

      if (storedState is not WorldNpcState worldNpc)
      {
        throw new InvalidOperationException(
          $"NPC slot {persistedSlot.Value} does not contain a persisted WorldNpcState.");
      }

      if (boundNpcs.ContainsKey((persistedSlot.Value, generation)))
      {
        continue;
      }

      NpcDefinition definition = ResolveNpcDefinition(worldNpc, persistedSlot);
      RuntimeEntityHandle handle = CreateNpcRuntimeEntity(
        session,
        new Terraria.Npc.NpcSlot(persistedSlot.Value),
        generation,
        worldNpc,
        definition);
      boundNpcs.Add((persistedSlot.Value, generation), handle);
    }
  }

  private static Dictionary<(int Slot, uint Generation), RuntimeEntityHandle>
    BuildNpcBindingIndex(EntityRuntime runtime)
  {
    var bindings = new Dictionary<(int Slot, uint Generation), RuntimeEntityHandle>();
    foreach (RuntimeEntityHandle handle in runtime.Match<NpcLegacySlotComponent>())
    {
      NpcLegacySlotComponent legacySlot = ReadComponent<NpcLegacySlotComponent>(runtime, handle);
      if (!legacySlot.IsAssigned || legacySlot.Generation == 0)
      {
        continue;
      }

      var key = (legacySlot.LegacySlot.Value, legacySlot.Generation);
      if (!bindings.TryAdd(key, handle))
      {
        throw new InvalidOperationException(
          $"NPC slot {key.Value} generation {key.Generation} is bound to multiple runtime entities.");
      }
    }

    return bindings;
  }

  private RuntimeEntityHandle CreateNpcRuntimeEntity(
    LoadedWorldSession session,
    Terraria.Npc.NpcSlot slot,
    uint generation,
    WorldNpcState worldNpc,
    NpcDefinition definition)
  {
    EntityRuntime runtime = session.EntityRuntime;
    RuntimeEntityHandle handle = runtime.CreateEntity();
    try
    {
      int catalogRevision = GetCatalogRevision();
      float width = definition.Movement.Width * definition.Movement.Scale;
      float height = definition.Movement.Height * definition.Movement.Scale;
      Attach(runtime, handle, new NpcDefinitionReferenceComponent(
        new NpcTypeId(definition.TypeId),
        new NpcNetId(definition.NetId),
        catalogRevision));
      Attach(runtime, handle, new NpcLifecycleComponent(
        isActive: true,
        remainingActiveTicks: 0,
        NpcLifecycleStage.Active));
      Attach(runtime, handle, new NpcLegacySlotComponent(slot, generation));
      Attach(runtime, handle, new NpcBehaviorComponent(
        definition.Spawn.AiStyle,
        action: 0,
        new float[NpcBehaviorComponent.AiSlotCount]));
      Attach(runtime, handle, new NpcBehaviorStateComponent(
        behaviorKind: 0,
        legacyAiStyle: definition.Spawn.AiStyle,
        action: 0));
      Attach(runtime, handle, new NpcLocalBehaviorStateComponent(
        new float[NpcLocalBehaviorStateComponent.LocalAiSlotCount]));
      Attach(runtime, handle, new NpcDirectionComponent(vertical: 0, sprite: -1));
      Attach(runtime, handle, new NpcImmediateEffectStateComponent());
      Attach(runtime, handle, new NpcNetworkSyncIntentComponent());
      Attach(runtime, handle, new NpcHealthComponent(
        definition.Core.DefaultLifeMax,
        definition.Core.DefaultLifeMax));
      Attach(runtime, handle, new NpcTargetComponent(legacyTargetIndex: 255));
      Attach(runtime, handle, new NpcSpawnAndCritterStateComponent());
      Attach(runtime, handle, new NpcStatusFlagsComponent());
      Attach(runtime, handle, new LocationComponent(worldNpc.X, worldNpc.Y));
      Attach(runtime, handle, new VelocityComponent(0.0f, 0.0f));
      Attach(runtime, handle, new ColliderComponent(width, height));
      Attach(runtime, handle, new DirectionComponent(horizontal: 0));
      if (!runtime.TryPublishEntity(handle))
      {
        throw new InvalidOperationException("The NPC entity root could not be published.");
      }

      return handle;
    }
    catch
    {
      RemoveIncompleteEntity(runtime, handle);
      throw;
    }
  }

  private NpcDefinition ResolveNpcDefinition(
    WorldNpcState worldNpc,
    Terraria.WorldStorage.NpcSlot slot)
  {
    NpcDefinition definition;
    if (worldNpc.NetId is int netId &&
        _content.Npcs.TryGetByNetId(netId, out definition))
    {
      return definition;
    }

    if (TryParseLegacyNpcType(worldNpc.LegacyTypeName, out int typeId) &&
        _content.Npcs.TryGetByTypeId(typeId, out definition))
    {
      return definition;
    }

    string identity = worldNpc.NetId?.ToString(CultureInfo.InvariantCulture) ??
      worldNpc.LegacyTypeName ?? "unknown";
    throw new InvalidOperationException(
      $"The content catalog cannot resolve persisted NPC slot {slot.Value} ({identity}).");
  }

  private static bool TryParseLegacyNpcType(string? legacyTypeName, out int typeId)
  {
    typeId = 0;
    return legacyTypeName is not null &&
      legacyTypeName.StartsWith("NPC.", StringComparison.Ordinal) &&
      int.TryParse(
        legacyTypeName.AsSpan(4),
        NumberStyles.None,
        CultureInfo.InvariantCulture,
        out typeId) &&
      typeId > 0;
  }

  private NpcRuntimeState[] CaptureNpcs(LoadedWorldSession session)
  {
    EntityRuntime runtime = session.EntityRuntime;
    var npcs = new List<NpcRuntimeState>();
    foreach (RuntimeEntityHandle handle in runtime.Match<NpcLegacySlotComponent>())
    {
      NpcLegacySlotComponent slot = ReadComponent<NpcLegacySlotComponent>(runtime, handle);
      if (!slot.IsAssigned)
      {
        continue;
      }

      NpcRuntimeState npc = CaptureNpcState(session, handle);
      npcs.Add(npc);
    }

    return npcs.OrderBy(npc => npc.Slot.Value).ToArray();
  }

  private NpcRuntimeState CaptureNpcState(
    LoadedWorldSession session,
    RuntimeEntityHandle handle)
  {
    EntityRuntime runtime = session.EntityRuntime;
    NpcLifecycleComponent lifecycle = ReadComponent<NpcLifecycleComponent>(runtime, handle);
    NpcDefinitionReferenceComponent reference =
      ReadComponent<NpcDefinitionReferenceComponent>(runtime, handle);
    NpcLegacySlotComponent legacySlot = ReadComponent<NpcLegacySlotComponent>(runtime, handle);
    NpcBehaviorComponent behavior = ReadComponent<NpcBehaviorComponent>(runtime, handle);
    NpcBehaviorStateComponent behaviorState =
      ReadComponent<NpcBehaviorStateComponent>(runtime, handle);
    NpcLocalBehaviorStateComponent localState =
      ReadComponent<NpcLocalBehaviorStateComponent>(runtime, handle);
    NpcDirectionComponent npcDirection = ReadComponent<NpcDirectionComponent>(runtime, handle);
    DirectionComponent direction = ReadComponent<DirectionComponent>(runtime, handle);
    LocationComponent location = ReadComponent<LocationComponent>(runtime, handle);
    VelocityComponent velocity = ReadComponent<VelocityComponent>(runtime, handle);
    ColliderComponent collider = ReadComponent<ColliderComponent>(runtime, handle);
    NpcNetworkSyncIntentComponent networkSync =
      ReadComponent<NpcNetworkSyncIntentComponent>(runtime, handle);
    NpcHealthComponent health = ReadComponent<NpcHealthComponent>(runtime, handle);
    NpcTargetComponent target = ReadComponent<NpcTargetComponent>(runtime, handle);
    NpcSpawnAndCritterStateComponent spawnAndCritter =
      ReadComponent<NpcSpawnAndCritterStateComponent>(runtime, handle);
    NpcStatusFlagsComponent statusFlags = ReadComponent<NpcStatusFlagsComponent>(runtime, handle);

    if (behavior.AiSlots.Length != NpcBehaviorComponent.AiSlotCount ||
        behaviorState.AuthoritativeAiSlots.Length != NpcBehaviorComponent.AiSlotCount ||
        localState.LocalAiSlots.Length != NpcLocalBehaviorStateComponent.LocalAiSlotCount)
    {
      throw new InvalidOperationException("An NPC runtime entity has incomplete AI state.");
    }

    if (!_content.Npcs.TryGetByTypeId(reference.TypeId.Value, out NpcDefinition definition))
    {
      throw new InvalidOperationException(
        $"The content catalog cannot resolve runtime NPC type {reference.TypeId.Value}.");
    }

    bool isTownNpc = definition.Town.IsTownNpc;
    if (legacySlot.Generation != 0 &&
        session.Storage.Npcs.TryGet(
          new Terraria.WorldStorage.NpcSlot(legacySlot.LegacySlot.Value),
          legacySlot.Generation,
          out WorldEntityState? storedState) &&
        storedState is WorldNpcState persistedNpc)
    {
      isTownNpc = persistedNpc.IsTownNpc;
    }

    return new NpcRuntimeState(
      handle,
      legacySlot.LegacySlot,
      legacySlot.Generation,
      lifecycle.IsActive,
      reference.TypeId.Value,
      reference.NetId.Value,
      definition.Spawn.AiStyle,
      isTownNpc,
      new Vector2(location.X, location.Y),
      new Vector2(velocity.X, velocity.Y),
      collider.Width,
      collider.Height,
      direction.Horizontal,
      npcDirection.Vertical,
      npcDirection.Sprite,
      target.LegacyTargetIndex,
      health.CurrentLife,
      health.MaximumLife,
      spawnAndCritter.SpawnedFromStatue,
      definition.Spawn.SpawnNeedsSyncing,
      statusFlags.Shimmering,
      definition.Spawn.CanBeCaught,
      (float[])behavior.AiSlots.Clone(),
      (float[])localState.LocalAiSlots.Clone(),
      networkSync.IsPending,
      networkSync.Revision);
  }

  private void ApplyNpcEffect(
    EntityRuntime runtime,
    NpcRuntimeState npc,
    float ai0,
    float ai1,
    float ai2,
    int? horizontalDirection,
    int? spriteDirection,
    float? localAi2,
    float? localAi3)
  {
    if (!runtime.Has<NpcNetworkSyncIntentComponent>(npc.Handle) ||
        !runtime.Has<NpcImmediateEffectStateComponent>(npc.Handle) ||
        !npc.NetworkUpdatePending && npc.NetworkUpdateRevision == uint.MaxValue)
    {
      throw new InvalidOperationException("NPC network update state cannot accept another effect.");
    }

    bool changed = runtime.TryEditComponents<
      NpcBehaviorComponent,
      NpcBehaviorStateComponent,
      NpcLocalBehaviorStateComponent,
      NpcDirectionComponent,
      DirectionComponent,
      NpcImmediateEffectStateComponent>(
        npc.Handle,
        (ref NpcBehaviorComponent behavior,
          ref NpcBehaviorStateComponent behaviorState,
          ref NpcLocalBehaviorStateComponent localState,
          ref NpcDirectionComponent npcDirection,
          ref DirectionComponent direction,
          ref NpcImmediateEffectStateComponent immediateEffects) =>
        {
          behavior.AiSlots[0] = ai0;
          behavior.AiSlots[1] = ai1;
          behavior.AiSlots[2] = ai2;
          behaviorState.AuthoritativeAiSlots[0] = ai0;
          behaviorState.AuthoritativeAiSlots[1] = ai1;
          behaviorState.AuthoritativeAiSlots[2] = ai2;
          if (localAi2.HasValue)
          {
            localState.LocalAiSlots[2] = localAi2.Value;
          }

          if (localAi3.HasValue)
          {
            localState.LocalAiSlots[3] = localAi3.Value;
          }

          if (horizontalDirection.HasValue)
          {
            direction.Horizontal = horizontalDirection.Value;
          }

          if (spriteDirection.HasValue)
          {
            npcDirection.Sprite = spriteDirection.Value;
          }

          immediateEffects.CommitNetworkUpdate();
        });
    if (!changed)
    {
      throw new InvalidOperationException("NPC effect state could not be committed.");
    }

    if (!runtime.TryEdit<NpcNetworkSyncIntentComponent>(
      npc.Handle,
      (ref NpcNetworkSyncIntentComponent networkSync) => networkSync.Mark()))
    {
      throw new InvalidOperationException("NPC network synchronization intent could not be marked.");
    }
  }

  private static bool CanNpcAndPlayerSeeEachOther(
    LoadedWorldSession session,
    NpcRuntimeState npc,
    SocialPlayerNpcInteractionSnapshot player,
    SessionSpatialTileLookup tiles)
  {
    TileMapStore tileMap = session.Storage.TileMap;
    if (tileMap.Width < 2 || tileMap.Height <= 41)
    {
      throw new InvalidOperationException("The world tile map is not ready for line-of-sight queries.");
    }

    Vector2 npcTop = new(
      npc.Position.X + npc.Width * 0.5f,
      npc.Position.Y);
    Vector2 playerTop = new(
      player.Position.X + player.Width * 0.5f,
      player.Position.Y);
    bool queryCompleted = SpatialCanHitLineQuery.TryEvaluate(
      npcTop,
      playerTop,
      tiles,
      tileMap.Width,
      tileMap.Height,
      out bool canHitLine);
    if (!queryCompleted)
    {
      string reason = tiles.MissingTileDefinition is int tileType
        ? $"Tile definition {tileType} is missing from the content catalog."
        : "The world did not provide all tile facts needed for line of sight.";
      throw new InvalidOperationException(reason);
    }

    return canHitLine;
  }

  private static Vector2 FindStardewProjectileCenter(
    LoadedWorldSession session,
    Vector2 scanOrigin,
    ITileDefinitionQuery tileDefinitions)
  {
    TileMapStore tileMap = session.Storage.TileMap;
    if (tileMap.Width < 1 || tileMap.Height <= 20)
    {
      throw new InvalidOperationException("The world tile map is not ready for the Dryad effect.");
    }

    int tileX = (int)MathF.Floor(scanOrigin.X / SpatialTileSnapshot.TileSize);
    int tileY = (int)MathF.Floor(scanOrigin.Y / SpatialTileSnapshot.TileSize);
    int scanStartY = Math.Clamp(tileY - StardewTileScanOffset, 10, tileMap.Height - StardewTileScanMargin);
    int scanEndY = checked(scanStartY + StardewTileScanCount);
    int finalScanY = scanStartY;
    bool firstTileIsSolid = false;

    for (int y = scanStartY; y < scanEndY; y++)
    {
      finalScanY = y;
      if ((uint)tileX >= (uint)tileMap.Width || (uint)y >= (uint)tileMap.Height)
      {
        continue;
      }

      TileCellState tile = tileMap.GetTile(tileX, y);
      if (!IsTileActive(tile))
      {
        continue;
      }

      if (!tileDefinitions.TryGet(tile.Type, out TileDefinition definition))
      {
        throw new InvalidOperationException(
          $"Tile definition {tile.Type} is missing from the content catalog.");
      }

      if (!definition.Solid)
      {
        continue;
      }

      firstTileIsSolid = y == scanStartY;
      break;
    }

    Vector2 floorPosition = new(
      tileX * SpatialTileSnapshot.TileSize + 8.0f,
      finalScanY * SpatialTileSnapshot.TileSize);
    if (firstTileIsSolid)
    {
      floorPosition = scanOrigin;
    }

    return floorPosition + new Vector2(0.0f, StardewProjectileYOffset);
  }

  private SocialNpcEffectSnapshot CaptureNpcSnapshot(
    LoadedWorldSession session,
    RuntimeEntityHandle handle)
  {
    EntityRuntime runtime = session.EntityRuntime;
    NpcRuntimeState npc = CaptureNpcState(session, handle);
    if (!runtime.TryGetReference(
      handle,
      EntityReferenceScope.Npc,
      out EntityReference entityReference))
    {
      throw new InvalidOperationException("The NPC runtime entity is not published.");
    }

    return new SocialNpcEffectSnapshot(
      handle,
      entityReference,
      npc.Slot,
      npc.SlotGeneration,
      npc.TypeId,
      npc.NetId,
      npc.Position,
      npc.Velocity,
      npc.Direction,
      npc.DirectionY,
      npc.SpriteDirection,
      npc.Target,
      npc.CurrentLife,
      npc.MaximumLife,
      npc.SpawnedFromStatue,
      npc.SpawnNeedsSyncing,
      npc.Shimmering,
      npc.CanBeCaught,
      npc.Ai,
      npc.LocalAi,
      npc.NetworkUpdatePending,
      npc.NetworkUpdateRevision);
  }

  private SocialProjectileSpawnSnapshot CaptureProjectileSnapshot(
    LoadedWorldSession session,
    ProjectileLifecycleSystem lifecycle,
    ProjectileHandle handle)
  {
    EntityRuntime runtime = session.EntityRuntime;
    if (!lifecycle.TryGetRuntimeHandle(handle, out RuntimeEntityHandle runtimeHandle) ||
        runtimeHandle.RuntimeId != session.WorldRuntimeId ||
        !lifecycle.TryGetEntityReference(handle, out EntityReference entityReference))
    {
      throw new InvalidOperationException(
        "The projectile lifecycle did not expose the committed projectile runtime root.");
    }

    ProjectileIdentityComponent identity = ReadComponent<ProjectileIdentityComponent>(
      runtime,
      runtimeHandle);
    ProjectileDefinitionComponent definition = ReadComponent<ProjectileDefinitionComponent>(
      runtime,
      runtimeHandle);
    ProjectileLifetimeStateComponent lifetime = ReadComponent<ProjectileLifetimeStateComponent>(
      runtime,
      runtimeHandle);
    ProjectileNetworkStateComponent network = ReadComponent<ProjectileNetworkStateComponent>(
      runtime,
      runtimeHandle);
    LocationComponent location = ReadComponent<LocationComponent>(runtime, runtimeHandle);
    VelocityComponent velocity = ReadComponent<VelocityComponent>(runtime, runtimeHandle);
    ProjectileBehaviorStateComponent behavior =
      ReadComponent<ProjectileBehaviorStateComponent>(runtime, runtimeHandle);
    ProjectileDamagePayloadComponent damage =
      ReadComponent<ProjectileDamagePayloadComponent>(runtime, runtimeHandle);
    ProjectileSourceMetadataComponent source =
      ReadComponent<ProjectileSourceMetadataComponent>(runtime, runtimeHandle);

    if (!lifetime.Active || !identity.HasProjectileSlot ||
        identity.SlotIndex != handle.Slot.Value || network.SectionSyncSkippedForPlayer is null)
    {
      throw new InvalidOperationException("The spawned projectile is not in a publishable state.");
    }

    return new SocialProjectileSpawnSnapshot(
      runtimeHandle,
      entityReference,
      handle.Slot,
      handle.Generation,
      identity.OwnerSlot,
      identity.Identity,
      identity.ProjectileUuid >= 0 ? identity.ProjectileUuid : null,
      definition.ProjectileType,
      new Vector2(location.X, location.Y),
      new Vector2(velocity.X, velocity.Y),
      damage.CurrentDamage,
      damage.OriginalDamage,
      damage.Knockback,
      behavior.Ai0,
      behavior.Ai1,
      behavior.Ai2,
      source.BannerIdToRespondTo,
      lifetime.TimeLeft,
      network.NetworkImportant,
      network.PrimaryUpdatePending,
      network.SecondaryUpdatePending,
      network.NetSpam,
      network.SendRequested,
      network.SectionSyncSkippedForPlayer);
  }

  private NpcDefinition RequireDryadDefinition()
  {
    if (!_content.Npcs.TryGetByTypeId(DryadTypeId, out NpcDefinition definition) ||
        definition.NetId != DryadNetId)
    {
      throw new InvalidOperationException(
        "The host content catalog must provide NPC type 20 / net ID 20 (Dryad).");
    }

    return definition;
  }

  private ProjectileDefinition RequireStardewProjectileDefinition()
  {
    if (!_content.Projectiles.TryGet(StardewProjectileType, out ProjectileDefinition definition) ||
        definition.Identity.NeedsUuid is not bool)
    {
      throw new InvalidOperationException(
        "The host content catalog must provide the complete projectile type 995 definition.");
    }

    return definition;
  }

  private int GetCatalogRevision()
  {
    if (_content.CatalogRevision <= 0 || _content.CatalogRevision > int.MaxValue)
    {
      throw new InvalidOperationException(
        "The Social NPC effect requires a positive Int32 content catalog revision.");
    }

    return (int)_content.CatalogRevision;
  }

  private static void ValidateExpectedRuntimeId(EntityRuntimeId runtimeId)
  {
    if (!runtimeId.IsAssigned)
    {
      throw new ArgumentException("An expected loaded-world runtime ID is required.", nameof(runtimeId));
    }
  }

  private static void ValidatePlayerSnapshot(in SocialPlayerNpcInteractionSnapshot player)
  {
    if (player.PlayerSlot == byte.MaxValue ||
        player.Width < 0 || player.Height < 0 ||
        !float.IsFinite(player.Position.X) || !float.IsFinite(player.Position.Y) ||
        !float.IsFinite(player.Stealth))
    {
      throw new ArgumentException("The player interaction snapshot is invalid.", nameof(player));
    }
  }

  private static bool IsExpectedRuntime(
    LoadedWorldSession session,
    EntityRuntimeId expectedWorldRuntimeId)
  {
    return !session.IsDisposed &&
      session.WorldRuntimeId == expectedWorldRuntimeId &&
      ReferenceEquals(session.EntityRuntime, session.Storage.EntityRuntime);
  }

  private static bool IsTileActive(TileCellState tile)
  {
    return (tile.TileHeader & 0x20) != 0 && (tile.TileHeader & 0x40) == 0;
  }

  private static TComponent ReadComponent<TComponent>(
    EntityRuntime runtime,
    RuntimeEntityHandle handle)
    where TComponent : notnull
  {
    TComponent component = default!;
    bool captured = false;
    if (!runtime.TryInspect<TComponent>(
      handle,
      (in TComponent current) =>
      {
        component = current;
        captured = true;
      }) || !captured)
    {
      throw new InvalidOperationException(
        $"The entity root does not expose {typeof(TComponent).Name}.");
    }

    return component;
  }

  private static void Attach<TComponent>(
    EntityRuntime runtime,
    RuntimeEntityHandle handle,
    TComponent component)
    where TComponent : notnull
  {
    if (!runtime.TryAttach(handle, component))
    {
      throw new InvalidOperationException(
        $"The NPC {typeof(TComponent).Name} component could not be attached.");
    }
  }

  private static void RemoveIncompleteEntity(
    EntityRuntime runtime,
    RuntimeEntityHandle handle)
  {
    if (!runtime.TryGetStatus(handle, out EntityRuntimeStatus status))
    {
      return;
    }

    if (status == EntityRuntimeStatus.Running && !runtime.TryBeginTermination(handle))
    {
      throw new InvalidOperationException("An incomplete NPC root could not begin termination.");
    }

    if (!runtime.TryRemoveEntity(handle))
    {
      throw new InvalidOperationException("An incomplete NPC root could not be removed.");
    }
  }

  private readonly record struct NpcRuntimeState(
    RuntimeEntityHandle Handle,
    Terraria.Npc.NpcSlot Slot,
    uint SlotGeneration,
    bool IsActive,
    int TypeId,
    int NetId,
    int AiStyle,
    bool IsTownNpc,
    Vector2 Position,
    Vector2 Velocity,
    float Width,
    float Height,
    int Direction,
    int DirectionY,
    int SpriteDirection,
    int Target,
    int CurrentLife,
    int MaximumLife,
    bool SpawnedFromStatue,
    bool SpawnNeedsSyncing,
    bool Shimmering,
    bool CanBeCaught,
    float[] Ai,
    float[] LocalAi,
    bool NetworkUpdatePending,
    uint NetworkUpdateRevision);

  private sealed class SessionSpatialTileLookup : ISpatialTileLookup
  {
    private readonly TileMapStore _tileMap;
    private readonly ITileDefinitionQuery _definitions;

    public SessionSpatialTileLookup(
      TileMapStore tileMap,
      ITileDefinitionQuery definitions)
    {
      _tileMap = tileMap;
      _definitions = definitions;
    }

    public int? MissingTileDefinition { get; private set; }

    public bool TryGetTile(int x, int y, out SpatialTileSnapshot tile)
    {
      if ((uint)x >= (uint)_tileMap.Width || (uint)y >= (uint)_tileMap.Height)
      {
        tile = new SpatialTileSnapshot(
          x,
          y,
          exists: false,
          isActive: false,
          blocksMovement: false,
          isSolid: false,
          isSolidTop: false,
          isHalfBrick: false,
          slope: 0,
          liquidAmount: 0,
          isInactive: false);
        return true;
      }

      TileCellState cell = _tileMap.GetTile(x, y);
      bool active = (cell.TileHeader & 0x20) != 0;
      bool inactive = (cell.TileHeader & 0x40) != 0;
      bool solid = false;
      bool solidTop = false;
      if (active)
      {
        if (!_definitions.TryGet(cell.Type, out TileDefinition definition))
        {
          MissingTileDefinition = cell.Type;
          tile = default;
          return false;
        }

        solid = definition.Solid;
        solidTop = definition.SolidTop;
      }

      tile = new SpatialTileSnapshot(
        x,
        y,
        exists: true,
        isActive: active,
        blocksMovement: solid && !solidTop,
        isSolid: solid,
        isSolidTop: solidTop,
        isHalfBrick: false,
        slope: 0,
        liquidAmount: cell.LiquidAmount,
        isInactive: inactive);
      return true;
    }
  }
}

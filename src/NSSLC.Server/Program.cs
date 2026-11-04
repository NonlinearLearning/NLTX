using System.Net;
using System.Numerics;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Ai;
using Terraria.Attributes;
using Terraria.Combat;
using Terraria.Communication;
using Terraria.Diagnostics;
using Terraria.Entity;
using Terraria.Inventory;
using Terraria.Item;
using Terraria.Liquid;
using Terraria.Movement;
using Terraria.Npc;
using Terraria.Network;
using Terraria.Physics;
using Terraria.Player;
using Terraria.Projectile;
using Terraria.Server;
using Terraria.Relationships;
using Terraria.World;
using MovementVelocityComponent = Terraria.Movement.VelocityComponent;

namespace NSSLC.Server;

internal static class Program
{
  private const float TickDeltaTime = 1f / 60f;
  private const int ListenPort = 7777;
  // 参考客户端主背包宽度与宽松堆叠上限；逐物品 maxStack 数据尚未引入。
  private const int PlayerInventorySlotCount = 59;
  private const int MaxInventoryStack = short.MaxValue;

  /// <summary>重生延迟（tick）。600 tick = 10 秒，与参考客户端的默认重生时间一致。</summary>
  /// <remarks>可由 <c>PlayerRespawnStateComponent.ConfigureDelay</c> 在运行时调整。</remarks>
  private const int PlayerRespawnDelayTicks = 600;

  public static async Task<int> Main(string[] args)
  {
    LoadedServerWorld loadedWorld = ServerWorldLoader.Load(
        args.Length > 0 ? args[0] : null);
    int worldWidth = loadedWorld.Header.MaxTilesX;
    int worldHeight = loadedWorld.Header.MaxTilesY;
    using ArchWorld ecsWorld = ArchWorld.Create();
    WorldRuntime worldRuntime = new(worldWidth, worldHeight);
    WorldItemStore worldItems = new();
    ItemDefinitionRegistry itemDefinitions = BuildItemDefinitions();
    InventorySystem inventories = new(ecsWorld, itemDefinitions);
    WorldInteractionResult worldLoad = worldRuntime.Load(loadedWorld.Tiles);
    if (!worldLoad.Succeeded)
    {
      throw new InvalidDataException($"The decoded world could not be published: {worldLoad.Failure}");
    }
    int guideSpawnTileX = loadedWorld.Environment.SpawnTileX;
    int slimeSpawnTileX = guideSpawnTileX + 3;
    int guideSurfaceTileY = FindSpawnSurfaceTileY(
        worldRuntime,
        guideSpawnTileX,
        loadedWorld.Environment.SpawnTileY,
        colliderWidth: 18f);
    int slimeSurfaceTileY = FindSpawnSurfaceTileY(
        worldRuntime,
        slimeSpawnTileX,
        loadedWorld.Environment.SpawnTileY,
        colliderWidth: 24f);
    float guideSurfaceY = guideSurfaceTileY * WorldRuntimeStateComponent.DefaultTileSize;
    float slimeSurfaceY = slimeSurfaceTileY * WorldRuntimeStateComponent.DefaultTileSize;
    Vector2 playerSpawnPosition = new(
        guideSpawnTileX * WorldRuntimeStateComponent.DefaultTileSize,
        guideSurfaceY - 48f);
    EntityRuntimeRegistry entities = new(worldScope: 1);
    using NpcAiRuntime npcAi = new(ecsWorld);
    ArchEntity guide = CreateNpc(
        ecsWorld,
        entities,
        new Vector2(guideSpawnTileX * 16f, guideSurfaceY),
        isSlime: false,
        networkSlot: 0,
        networkType: 22);
    ArchEntity slime = CreateNpc(
        ecsWorld,
        entities,
        new Vector2(slimeSpawnTileX * 16f, slimeSurfaceY),
        isSlime: true,
        networkSlot: 1,
        networkType: 1);
    npcAi.AttachGuide(guide);
    npcAi.AttachSlime(slime);
    ServerLog.Info(
        "ecs.npc.spawned",
        $"guide=world-spawn:{loadedWorld.Environment.SpawnTileX},{loadedWorld.Environment.SpawnTileY} " +
        $"slime=world-spawn+3tiles:{loadedWorld.Environment.SpawnTileX + 3}," +
        $"{loadedWorld.Environment.SpawnTileY} guideSurfaceTileY={guideSurfaceTileY} " +
        $"slimeSurfaceTileY={slimeSurfaceTileY}");
    ProjectileRuntimeRegistry projectiles = new(entities);
    CombatAttackDefinitionCatalog attackDefinitions = CreateAttackDefinitions();
    // 攻击意图队列挂在专用世界级实体上，各域生产者按组件查询，宿主不再持有队列对象。
    _ = ecsWorld.Create(new CombatAttackIntentQueueComponent());
    // 伤害请求与结果队列各自挂在专用世界级实体上，跨域只通过组件传递。
    _ = ecsWorld.Create(new CombatDamageRequestQueueComponent());
    _ = ecsWorld.Create(new CombatDamageResultQueueComponent());
    // 投射物终止队列挂在世界级实体上，生命周期、命中适配器与寿命推进共享同一组件。
    _ = ecsWorld.Create(new ProjectileTerminationQueueComponent());
    _ = ecsWorld.Create(new ProjectileSpawnQueueComponent());
    // 世界物品变更请求队列：跨域只表达意图，由 World 域自己执行。
    _ = ecsWorld.Create(new WorldItemRequestQueueComponent());
    // 生命恢复请求队列：Player 决定复活，Combat 执行满血写入。
    _ = ecsWorld.Create(new LifeRestoreRequestQueueComponent());
    // 玩家输入请求队列：Communication 只翻译意图，Player 域执行坐标与物品栏写入。
    _ = ecsWorld.Create(new PlayerInputRequestQueueComponent());
    // 移动意图请求队列：输入适配器与 NPC AI 都只提交意图，由 Movement 域统一写入。
    _ = ecsWorld.Create(new MovementIntentRequestQueueComponent());
    // 接触事实队列挂在专用世界级实体上，宿主不再持有队列对象。
    _ = ecsWorld.Create(new PhysicsContactQueueComponent());
    ProjectileDefinitionCatalog projectileDefinitions = new();
    projectileDefinitions.Seal();
    ProjectileLifecycleSystem projectileLifecycle = new(
        ecsWorld,
        projectileDefinitions,
        projectiles);
    ProjectileHitResolutionSystem projectileHits = new(ecsWorld, entities);
    PhysicsContactCaptureSystem physicsContacts = new(
        ecsWorld,
        worldRuntime,
        entities);
    WorldTickSystem worldTick = new(ecsWorld, worldRuntime);
    WorldInteractionSystem worldInteractions = new(ecsWorld, worldRuntime);
    BuffDefinitionRegistry buffDefinitions = new();
    // 注册参考客户端药水表的三条样例，避免注册表为空导致 TryApply 恒返回 UnknownDefinition。
    BuffContent.RegisterAll(buffDefinitions);
    AttributeTickSystem attributeTick = new(ecsWorld);
    // 装备/饰品/武器的属性贡献由库存事实重建。它必须排在所有读取 AttributeSetComponent
    // 的系统之前，否则本 tick 内换装要到下一 tick 才生效。
    InventoryAttributeContributionSystem attributeContributions = new(ecsWorld, itemDefinitions);
    BuffTickSystem buffTick = new(ecsWorld, new BuffSystem(buffDefinitions));
    GravityTickSystem gravityTick = new(ecsWorld);
    NpcAiIntentAdapter npcAiAdapter = new(
        ecsWorld,
        npcAi.Entities,
        npcAi.Snapshots,
        entities,
        attackDefinitions);
    SpatialSimulationCoordinatorSystem spatialSimulation = new(ecsWorld, worldRuntime);
    Dictionary<SessionEpoch, EntityRuntimeHandle> sessions = [];
    object lifecycleGate = new();

    CommunicationHostRuntime? communication = null;
    communication = new(
        ecsWorld,
        new IPEndPoint(IPAddress.Loopback, ListenPort),
        onSessionOpened: epoch =>
        {
          lock (lifecycleGate)
          {
            EntityRuntimeHandle handle = CreatePlayer(
                ecsWorld,
                entities,
                playerSpawnPosition);
            if (!communication!.Ingress.TryBindPlayer(epoch, handle))
            {
              entities.Remove(handle, out ArchEntity entity);
              if (ecsWorld.IsAlive(entity))
              {
                ecsWorld.Destroy(entity);
              }

              throw new InvalidOperationException(
                  "The new communication session could not bind its player entity.");
            }

            sessions.Add(epoch, handle);
            ServerLog.Info("ecs.session.player_bound", $"epoch={epoch.Value}");
          }

          return ValueTask.CompletedTask;
        },
        onSessionClosed: closed =>
        {
          lock (lifecycleGate)
          {
            if (sessions.Remove(closed.Epoch, out EntityRuntimeHandle handle) &&
                entities.Remove(handle, out ArchEntity entity) && ecsWorld.IsAlive(entity))
            {
              ecsWorld.Destroy(entity);
            }

            ServerLog.Info(
                "ecs.session.player_unbound",
                $"epoch={closed.Epoch.Value} reason={ServerLog.Quote(closed.Reason)}");
          }

          return ValueTask.CompletedTask;
        });

    IReadOnlyList<EcsServerSystemRegistration> registrations = BuildRegistrations(
        ecsWorld,
        entities,
        attackDefinitions,
        physicsContacts,
        projectileHits,
        projectileLifecycle,
        communication,
        worldTick,
        worldInteractions,
        worldItems,
        inventories,
        npcAiAdapter,
        spatialSimulation,
        attributeTick,
        attributeContributions,
        buffTick,
        gravityTick,
        new NpcNetworkProjectionSystem(ecsWorld),
        new WorldNetworkProjectionSystem(
            ecsWorld,
            worldRuntime,
            worldItems,
            loadedWorld.FrameImportant),
        new TerrariaProtocolOptions
        {
          WorldWidth = worldWidth,
          WorldHeight = worldHeight,
          SpawnTileX = loadedWorld.Environment.SpawnTileX,
          SpawnTileY = loadedWorld.Environment.SpawnTileY,
          WorldName = loadedWorld.Header.WorldName,
          WorldId = loadedWorld.Header.WorldId,
          WorldGuid = loadedWorld.Header.UniqueId ?? Guid.Empty,
          WorldGeneratorVersion = loadedWorld.Header.WorldGeneratorVersion ?? 0,
          GameMode = loadedWorld.Header.GameMode,
          WorldSurface = checked((short)loadedWorld.Environment.WorldSurface),
          RockLayer = checked((short)loadedWorld.Environment.RockLayer),
          WorldTime = checked((int)loadedWorld.Environment.Time),
          DayTime = loadedWorld.Environment.DayTime,
          MoonPhase = checked((byte)loadedWorld.Environment.MoonPhase),
          WorldTiles = loadedWorld.Tiles,
          FrameImportant = loadedWorld.FrameImportant.ToArray()
        });
    using EcsServer server = EcsServer.Create(ecsWorld, registrations);
    server.Initialize();

    using CancellationTokenSource shutdown = new();
    ConsoleCancelEventHandler cancel = (_, args) =>
    {
      args.Cancel = true;
      shutdown.Cancel();
    };
    Console.CancelKeyPress += cancel;
    try
    {
      await communication!.StartAsync(shutdown.Token).ConfigureAwait(false);
      ServerLog.Info(
          "ecs.server.ready",
          $"listen=127.0.0.1:{ListenPort} world={worldWidth}x{worldHeight} " +
          $"path={ServerLog.Quote(loadedWorld.Path)} spawn={loadedWorld.Environment.SpawnTileX}," +
          $"{loadedWorld.Environment.SpawnTileY}");
      await RunTicksAsync(server, shutdown.Token).ConfigureAwait(false);
    }
    catch (OperationCanceledException) when (shutdown.IsCancellationRequested)
    {
      ServerLog.Info("ecs.server.canceled", "The ECS server shutdown was requested.");
    }
    catch (Exception exception)
    {
      ServerLog.Error("ecs.server.failed", exception);
      return 1;
    }
    finally
    {
      Console.CancelKeyPress -= cancel;
      await communication!.StopAsync().ConfigureAwait(false);
      ServerLog.Info("ecs.server.stopped", $"lastTick={server.LastTick}");
    }

    await communication!.DisposeAsync().ConfigureAwait(false);

    return 0;
  }

  /// <summary>
  /// 玩家主背包槽位定义表。Share/Inventory 由定义表驱动槽位，这里按参考客户端的主背包宽度声明，
  /// 分类放开为 Any 以保持迁移前的"任意物品可入任意格"行为。
  /// </summary>
  private static InventorySlotDefinition[] PlayerInventorySlots()
  {
    InventorySlotDefinition[] slots = new InventorySlotDefinition[PlayerInventorySlotCount];
    for (int index = 0; index < PlayerInventorySlotCount; index++)
    {
      slots[index] = new InventorySlotDefinition(
          index,
          InventorySlotKind.General,
          ItemCategoryMask.Any,
          MaxInventoryStack);
    }

    return slots;
  }

  /// <summary>
  /// 为物品表中每个已知 id 注册一条宽松定义。Share/Inventory 要求每次写入都有定义可用，
  /// 而参考客户端的逐物品 maxStack/分类数据尚未引入，因此先与迁移前行为保持一致。
  /// </summary>
  private static ItemDefinitionRegistry BuildItemDefinitions()
  {
    ItemDefinitionRegistry registry = new();
    foreach (ItemId itemId in Enum.GetValues<ItemId>())
    {
      if (itemId == ItemId.None)
      {
        continue;
      }

      registry.Register(new ItemDefinition(itemId, MaxInventoryStack, ItemCategoryMask.Any));
    }

    return registry;
  }

  private static IReadOnlyList<EcsServerSystemRegistration> BuildRegistrations(
      ArchWorld world,
      EntityRuntimeRegistry entities,
      CombatAttackDefinitionCatalog attackDefinitions,
      PhysicsContactCaptureSystem physicsContacts,
      ProjectileHitResolutionSystem projectileHits,
      ProjectileLifecycleSystem projectileLifecycle,
      CommunicationHostRuntime communication,
      WorldTickSystem worldTick,
      WorldInteractionSystem worldInteractions,
      WorldItemStore worldItems,
      InventorySystem inventories,
      NpcAiIntentAdapter npcAiAdapter,
      SpatialSimulationCoordinatorSystem spatialSimulation,
      AttributeTickSystem attributeTick,
      InventoryAttributeContributionSystem attributeContributions,
      BuffTickSystem buffTick,
      GravityTickSystem gravityTick,
      NpcNetworkProjectionSystem npcProjection,
      WorldNetworkProjectionSystem worldProjection,
      TerrariaProtocolOptions protocolOptions)
  {
    List<EcsServerSystemRegistration> registrations = [];
    // 必须排在本阶段最前：准备阶段收包写出的玩家输入要在任何移动/属性系统读取前落地。
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Simulation,
        new PlayerInputRequestSystem(world, entities, inventories)));
    // 属性贡献重建紧随输入落地之后：包里的换装在本 tick 就要影响 AttributeSetComponent，
    // 而下面 CombatSystemRegistration 里的投射物命中结算会读取它。
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Simulation,
        attributeContributions));
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Projection,
        npcProjection));
    // 世界物品请求跑两遍：投影之前一遍，消化准备阶段收包产生的请求（保持同帧生效）；
    // 投影之后一遍，消化投影自身写出的掉落物请求。队列取空后为空，跑两遍无副作用。
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Projection,
        new WorldItemRequestSystem(world, worldItems)));
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Projection,
        worldProjection));
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Projection,
        new WorldItemRequestSystem(world, worldItems)));
    registrations.AddRange(CommunicationSystemRegistration.CreateGame(
        world,
        entities,
        attackDefinitions,
        communication.OutboundPort,
        protocolOptions: protocolOptions,
        worldItems: worldItems));
    registrations.AddRange(CombatSystemRegistration.Create(
        world,
        entities,
        entities,
        physicsContacts,
        projectileHits,
        projectileLifecycle,
        attackDefinitions));
    registrations.Add(new EcsServerSystemRegistration(EcsServerPhase.Simulation, worldTick));
    registrations.Add(new EcsServerSystemRegistration(EcsServerPhase.Simulation, npcAiAdapter));
    // 重力投影必须先于空间求解刷新，SpatialSimulationCoordinatorSystem 直接读取 GravityStateComponent。
    registrations.Add(new EcsServerSystemRegistration(EcsServerPhase.Simulation, gravityTick));
    // 属性与 Buff 的期限在 Commit 之前推进，使本 tick 内新加的来源获得完整时长。
    registrations.Add(new EcsServerSystemRegistration(EcsServerPhase.Simulation, attributeTick));
    registrations.Add(new EcsServerSystemRegistration(EcsServerPhase.Simulation, buffTick));
    // 必须紧邻空间求解之前：本帧由 AI 与输入适配器提交的意图要在求解读取前落地。
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Simulation,
        new MovementIntentRequestSystem(world, entities)));
    registrations.Add(new EcsServerSystemRegistration(EcsServerPhase.Simulation, spatialSimulation));
    registrations.Add(new EcsServerSystemRegistration(EcsServerPhase.Commit, worldInteractions));
    return registrations.AsReadOnly();
  }

  private static EntityRuntimeHandle CreatePlayer(
      ArchWorld world,
      EntityRuntimeRegistry entities,
      Vector2 spawnPosition)
  {
    EntityUuid id = EntityUuid.Create();
    EntityRuntimeHandle handle = entities.Allocate(id);
    AttributeSetComponent playerAttributes = new();
    PlayerAttributeBaseline.Apply(playerAttributes);
    ArchEntity entity = world.Create(
        new EntityIdentityComponent(id),
        new EntityFactionComponent(Faction.Friendly),
        new LifeComponent(current: 100, maximum: 100),
        // 没有这两项时 PlayerDeathRespawnSystem 与 PlayerCombatResultSystem 在真实服务器中
        // 匹配 0 个实体：前者永不处理死亡，后者永不排空 CombatDamageResultQueue，
        // 对局越久玩家伤害结果堆积越多（与 AttributeTickSystem 那次的成因同类）。
        // 对存活玩家 PlayerDeathRespawnSystem 是空操作，只在 life 归零后才登记死亡与重生计划。
        new PlayerDeathStateComponent(),
        new PlayerRespawnStateComponent(PlayerRespawnDelayTicks),
        // 属性基值取自参考客户端默认值（statDefense 0、moveSpeed 1），与 null 路径等价；
        // 没有这两项时 AttributeTickSystem 与 BuffTickSystem 在真实服务器中匹配 0 个实体。
        playerAttributes,
        new BuffListComponent(),
        // 没有这份能力时 BuffAdmissionSystem.Check 返回 MissingReceptionCapability，
        // 即使内容侧已注册定义，TryApply 也会被准入拒绝。
        new BuffReceptionCapabilityComponent(BuffReceptionMode.AllowAll),
        new LocationComponent(spawnPosition),
        new InventoryComponent(PlayerInventorySlots()),
        Terraria.Movement.VelocityComponent.Stationary,
        new ColliderComponent(new Vector2(16f, 32f)),
        new EntityCollisionCapabilityComponent(Enabled: true, Layer: 1, Mask: uint.MaxValue),
        MovementIntent.Stop(tick: 0),
        new WorldInteractionCapabilityComponent(
            WorldInteractionPermission.ReadTile |
            WorldInteractionPermission.BreakTile |
            WorldInteractionPermission.PlaceTile),
        WorldInteractionRequestComponent.ReadTile(
            requestId: 0,
            tick: 0,
            coordinate: new WorldTileCoordinate(0, 0)),
        new WorldInteractionResultComponent());
    try
    {
      entities.Register(handle, entity);
      return handle;
    }
    catch
    {
      if (world.IsAlive(entity))
      {
        world.Destroy(entity);
      }

      throw;
    }
  }

  private static int FindSpawnSurfaceTileY(
      WorldRuntime world,
      int spawnTileX,
      int spawnTileY,
      float colliderWidth)
  {
    int firstColumn = Math.Clamp(spawnTileX, 0, world.State.Width - 1);
    int lastColumn = Math.Clamp(
        (int)MathF.Floor((spawnTileX * world.State.TileSize + colliderWidth - 0.0001f) /
            world.State.TileSize),
        firstColumn,
        world.State.Width - 1);
    int surface = world.State.Height - 1;
    for (int x = firstColumn; x <= lastColumn; x++)
    {
      int candidate = Math.Clamp(spawnTileY, 0, world.State.Height - 1);
      while (candidate < world.State.Height &&
          (!world.TryReadTile(new WorldTileCoordinate(x, candidate), out WorldTileState tile) ||
              !tile.Active))
      {
        candidate++;
      }

      if (candidate >= world.State.Height)
      {
        continue;
      }

      while (candidate > 0 &&
          world.TryReadTile(new WorldTileCoordinate(x, candidate - 1),
              out WorldTileState above) &&
          above.Active)
      {
        candidate--;
      }

      surface = Math.Min(surface, candidate);
    }

    return surface == world.State.Height - 1
        ? Math.Clamp(spawnTileY, 0, world.State.Height - 1)
        : surface;
  }

  private static ArchEntity CreateNpc(
      ArchWorld world,
      EntityRuntimeRegistry entities,
      Vector2 position,
      bool isSlime,
      byte networkSlot,
      short networkType)
  {
    EntityUuid id = EntityUuid.Create();
    EntityRuntimeHandle handle = entities.Allocate(id);
    ArchEntity entity = world.Create(
        new EntityIdentityComponent(id),
        new EntityFactionComponent(isSlime ? Faction.Hostile : Faction.Friendly),
        new LifeComponent(isSlime ? 50 : 250, isSlime ? 50 : 250),
        new LocationComponent(position - new Vector2(0f, isSlime ? 18f : 40f)),
        MovementVelocityComponent.Stationary,
        new ColliderComponent(isSlime ? new Vector2(24f, 18f) : new Vector2(18f, 40f)),
        GroundingComponent.Airborne,
        new EntityCollisionCapabilityComponent(Enabled: true, Layer: 1, Mask: uint.MaxValue),
        new MovementCapabilityComponent(
            enabled: true,
            maximumSpeed: isSlime ? 2.5f : 1f,
            acceleration: isSlime ? 12f : 8f,
            canJump: true,
            jumpSpeed: isSlime ? 6f : 5f),
        new GravityCapabilityComponent(Enabled: true, CanToggleDirection: false),
        new GravityConfigurationComponent(),
        new GravityDirectionStateComponent(GravityDirection.Normal),
        new GravityModifierCollectionComponent(),
        new GravityStateComponent(0.4f, 10f, GravityDirection.Normal, false, false),
        new TileCollisionCapabilityComponent(Enabled: true),
        new LiquidInteractionCapabilityComponent(
            Enabled: true,
            AffectedLiquids: LiquidContactFlags.Water | LiquidContactFlags.Honey |
                LiquidContactFlags.Lava | LiquidContactFlags.Shimmer),
        MovementIntent.Stop(tick: 0),
        new NpcBehaviorStateComponent(),
        new NpcNetworkIdentityComponent(networkSlot, generation: 1, netId: networkType),
        isSlime ? AiComponent.For<SlimeAiBehavior>() : AiComponent.For<GuideAiBehavior>(),
        new AiBehaviorSelectionComponent(
            isSlime ? "Slime" : "Guide",
            isSlime ? "slime.default" : "guide.default"));
    try
    {
      entities.Register(handle, entity);
      return entity;
    }
    catch
    {
      if (world.IsAlive(entity))
      {
        world.Destroy(entity);
      }

      throw;
    }
  }

  private static CombatAttackDefinitionCatalog CreateAttackDefinitions()
  {
    CombatAttackDefinitionCatalog definitions = new();
    definitions.Register(new CombatAttackDefinition(
        new CombatAttackDefinitionId("player.default"),
        new DamagePayload(baseDamage: 10, type: DamageType.Melee)));
    definitions.Seal();
    return definitions;
  }

  private static async Task RunTicksAsync(EcsServer server, CancellationToken cancellationToken)
  {
    using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(1000d / 60d));
    long tick = 0;
    while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
    {
      server.Update(tick++, TickDeltaTime);
    }
  }
}

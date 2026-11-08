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

using Terraria.Chat;

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

  /// <summary>受击后无敌窗口时长（tick）。60 tick = 1 秒。</summary>
  /// <remarks>
  /// 参考客户端受击后的短无敌多在 60 tick 量级（同类量级还有 120/240）。
  /// 其受击路径在 <c>Hurt</c> 内部按伤害来源分别计算，本仓库未逐行对齐，
  /// 因此这里作为宿主层可调常量暴露，便于后续按手感调整。
  /// </remarks>
  private const int PlayerHurtProtectionTicks = 60;

  public static async Task<int> Main(string[] args)
  {
    LoadedServerWorld loadedWorld = ServerWorldLoader.Load(
        args.Length > 0 ? args[0] : null);
    ServerLog.Info(
        "ecs.world.tile-entities",
        $"chests={loadedWorld.TileEntities.ChestCount} signs={loadedWorld.TileEntities.SignCount} " +
        $"tileEntities={loadedWorld.TileEntities.TileEntityCount} " +
        $"pylons={loadedWorld.TileEntities.CountTileEntities(WorldTileEntityKind.TeleportationPylon)}");
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
    // 实体定义框架的宿主与生成器：玩家与 NPC 创建都走 EntityDefinitionSystem，
    // 不再是调用处逐个 new 组件。
    EntityDefinitionRegistry entityDefinitions = new();
    PlayerEntityDefinition.Register(
        entityDefinitions,
        PlayerInventorySlotCount,
        MaxInventoryStack,
        PlayerRespawnDelayTicks);
    NpcEntityDefinition.Register(entityDefinitions);
    EntityDefinitionSystem<ArchEntity> entitySpawner = new(
        entityDefinitions,
        new ArchEntityDefinitionHost(ecsWorld, entities));
    using NpcAiRuntime npcAi = new(ecsWorld);
    ArchEntity guide = CreateNpc(
        ecsWorld,
                entitySpawner,
        new Vector2(guideSpawnTileX * 16f, guideSurfaceY),
        isSlime: false,
        networkSlot: 0,
        networkType: 22);
    ArchEntity slime = CreateNpc(
        ecsWorld,
                entitySpawner,
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
    // 世界物品拾取请求队列：World 只提出"这件物品可以捡"，Inventory 决定能收下多少。
    _ = ecsWorld.Create(new InventoryPickupRequestQueueComponent());
    // 刷怪事实队列：NPC 域创建实体，Network 域分配客户端槽位。
    _ = ecsWorld.Create(new NpcSpawnFactQueueComponent());
    // 箱子/告示牌/Tile 实体交互：Communication 只提交意图，World 域持有并执行。
    _ = ecsWorld.Create(new WorldTileEntityRequestQueueComponent());
    _ = ecsWorld.Create(new WorldTileEntityResultQueueComponent());
    // 物品使用意图与结果：Communication 只提交意图，Player 域执行使用/弹药/通道。
    _ = ecsWorld.Create(new PlayerItemUseRequestQueueComponent());
    _ = ecsWorld.Create(new PlayerItemUseFactQueueComponent());
    // 聊天意图与广播事实：Communication 收文本帧，Chat 域判定，Network 投影成文本模块包。
    _ = ecsWorld.Create(new ChatMessageRequestQueueComponent());
    _ = ecsWorld.Create(new ChatBroadcastQueueComponent());
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
    // 刷怪表：目前只有史莱姆可被创建，表中权重与网络类型即 Version4 的出生判定常量。
    NpcSpawnTable npcSpawnTable = new(
    [
      new NpcSpawnCandidate(
          NpcEntityDefinition.SlimeId,
          NetworkType: 1,
          AiComponent.For<SlimeAiBehavior>(),
          Weight: 1),
    ]);
    // 困难模式/血月/日食的推进尚未建模，这里只带入存档头与时间事实。
    NpcSpawnEnvironment npcSpawnEnvironment = new(
        HardMode: false,
        WorldSurfaceTileY: (int)loadedWorld.Environment.WorldSurface,
        RockLayerTileY: (int)loadedWorld.Environment.RockLayer,
        UnderworldTileY: Math.Clamp(
            worldHeight - 205,
            (int)loadedWorld.Environment.RockLayer,
            worldHeight),
        DayTime: loadedWorld.Environment.DayTime,
        BloodMoon: false,
        Eclipse: false);
    NpcSpawnSystem npcSpawn = new(
        ecsWorld,
        worldRuntime,
        entities,
        entitySpawner,
        npcAi,
        npcSpawnTable,
        npcSpawnEnvironment,
        worldSeed: WorldSeed(loadedWorld.Header.UniqueId));
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
                entitySpawner,
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
        worldRuntime,
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
        npcSpawn,
        loadedWorld.TileEntities,
        CreateItemUseDefinitions(),
        new ChatStore(),
        CreateChatCommands(),
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

  /// <summary>
  /// 物品使用表：射速、动画、连射、通道与弹药。数值取自参考客户端对应物品的
  /// <c>useTime/useAnimation/autoReuse/channel/useAmmo</c> 量级，尚未逐物品从物品表导入。
  /// </summary>
  /// <remarks>
  /// 只登记少量示范物品：表格结构（每 id 一条不可变档案）才是本次引入的能力，
  /// 全表导入需要物品表数据，属于后续工作。蓝量（<c>ManaCost</c>）随档案传递但暂不扣减。
  /// </remarks>
  private static ItemUseDefinitionCatalog CreateItemUseDefinitions()
  {
    ItemUseDefinitionCatalog definitions = new();
    definitions.Register(new ItemUseDefinition(
        ItemId.Torch,
        UseTime: 15,
        UseAnimation: 15,
        AutoReuse: true,
        Channel: false,
        UseAmmo: ItemId.None,
        ConsumeAmmo: false,
        ManaCost: 0));
    definitions.Register(new ItemUseDefinition(
        ItemId.CopperBow,
        UseTime: 25,
        UseAnimation: 25,
        AutoReuse: false,
        Channel: false,
        UseAmmo: ItemId.WoodenArrow,
        ConsumeAmmo: true,
        ManaCost: 0));
    definitions.Register(new ItemUseDefinition(
        ItemId.MagicMissile,
        UseTime: 12,
        UseAnimation: 12,
        AutoReuse: true,
        Channel: true,
        UseAmmo: ItemId.None,
        ConsumeAmmo: false,
        ManaCost: 10));
    definitions.Seal();
    return definitions;
  }

  /// <summary>
  /// 聊天命令表：名字对齐参考客户端的命令类（<c>SayChatCommand</c>、<c>HelpCommand</c>、
  /// <c>ListPlayersCommand</c>、<c>RollCommand</c>）。属性名在参考 dump 中被裁剪，这里统一用小写。
  /// </summary>
  private static ChatCommandCatalog CreateChatCommands()
  {
    ChatCommandCatalog commands = new();
    commands.Register(new ChatCommandDefinition(
        ChatCommandRules.DefaultCommandName,
        "Say something in world chat.",
        RequiresOperator: false));
    commands.Register(new ChatCommandDefinition(
        "help",
        "List the commands this server understands.",
        RequiresOperator: false));
    commands.Register(new ChatCommandDefinition(
        "playing",
        "Show how many players are online.",
        RequiresOperator: false));
    commands.Register(new ChatCommandDefinition(
        "roll",
        "Roll a number from 1 to 100.",
        RequiresOperator: false));
    commands.Seal();
    return commands;
  }

  private static IReadOnlyList<EcsServerSystemRegistration> BuildRegistrations(
      ArchWorld world,
      WorldRuntime worldRuntime,
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
      NpcSpawnSystem npcSpawn,
      WorldTileEntityStore tileEntities,
      ItemUseDefinitionCatalog itemUseDefinitions,
      ChatStore chatStore,
      ChatCommandCatalog chatCommands,
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
    // 聊天投影紧挨 NPC 投影：两者都只写 outbound 意图，顺序不影响语义。
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Projection,
        new ChatNetworkProjectionSystem(world, entities)));
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
        attackDefinitions,
        PlayerHurtProtectionTicks));
    registrations.Add(new EcsServerSystemRegistration(EcsServerPhase.Simulation, worldTick));
    // 世界物品 tick 紧跟世界时间推进：本 tick 的重力/碰撞结果不依赖后续系统，
    // 但拾取请求必须在同一 tick 的 Inventory 步骤之前入队。
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Simulation,
        new WorldItemTickSystem(
            world,
            worldItems,
            worldRuntime,
            worldRuntime,
            entities,
            MaxInventoryStack)));
    // 拾取请求由 Inventory 域结算并回写 Consume 请求；投影阶段之前跑完，同帧内一致。
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Simulation,
        new InventoryPickupRequestSystem(world, entities, inventories)));
    // 刷怪在移动求解之前跑：本 tick 创建的 NPC 从下一 tick 起由 AI 与空间求解驱动，
    // 避免在同一个 tick 里对新实体重复计算移动。
    registrations.Add(new EcsServerSystemRegistration(EcsServerPhase.Simulation, npcSpawn));
    // 槽位分配必须早于 Projection 阶段的 NpcNetworkProjectionSystem，否则新 NPC 本 tick 不可见。
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Simulation,
        new NpcNetworkIdentitySystem(world)));
    // 箱子/告示牌意图在投影之前落地，开箱状态与文本改动同 tick 可被读取。
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Simulation,
        new WorldTileEntityRequestSystem(world, tileEntities)));
    // 物品使用在投影之前：本 tick 的射击事实留给投射物/战斗域消费。
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Simulation,
        new PlayerItemUseSystem(world, entities, itemUseDefinitions, inventories)));
    // 聊天：能力组件决定谁能说话，系统的广播事实交给 Projection 阶段的聊天投影。
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Simulation,
        new ChatSystem(world, entities, chatCommands, chatStore)));
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
      EntityDefinitionSystem<ArchEntity> entitySpawner,
      Vector2 spawnPosition)
  {
    ArchEntity entity = entitySpawner.Spawn(PlayerEntityDefinition.Id);
    // 出生坐标属于实例级输入，定义里只有占位值，这里在创建后立即覆盖。
    world.Set(entity, new LocationComponent(spawnPosition));
    if (!entities.TryGetHandle(entity, out EntityRuntimeHandle handle))
    {
      throw new InvalidOperationException("玩家实体创建后未登记运行时句柄。");
    }

    return handle;
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
      EntityDefinitionSystem<ArchEntity> entitySpawner,
      Vector2 position,
      bool isSlime,
      byte networkSlot,
      short networkType)
  {
    ArchEntity entity = entitySpawner.Spawn(
        isSlime ? NpcEntityDefinition.SlimeId : NpcEntityDefinition.GuideId);
    // 出生坐标与网络标识属于实例级输入，定义里只有占位值。
    world.Set(
        entity,
        new LocationComponent(
            position - new Vector2(0f, NpcEntityDefinition.SpawnOffsetY(isSlime))));
    world.Add(
        entity,
        new NpcNetworkIdentityComponent(networkSlot, generation: 1, netId: networkType));
    return entity;
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

  /// <summary>把存档 GUID 折成一个确定性的世界种子，供刷怪随机流使用。</summary>
  /// <remarks>同一存档重放同一 tick 序列会得到同一批出生结果，便于比对日志与回放。</remarks>
  private static long WorldSeed(Guid? uniqueId)
  {
    if (uniqueId is not Guid id || id == Guid.Empty)
    {
      return 1;
    }

    return BitConverter.ToInt64(id.ToByteArray(), 0);
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

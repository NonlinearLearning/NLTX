using System.Diagnostics;
using System.Net;
using System.Numerics;
using System.Text;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using NSSLC.Infrastructure.Lvcs;
using NSSLC.Server.Content.ItemUse;
using Terraria.Ai;
using Terraria.Attributes;
using Terraria.Combat;
using Terraria.Session;
using Terraria.Diagnostics;
using Terraria.Entity;
using Terraria.Lvcs;
using Terraria.EntitySpawn;
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
  private const float TickDeltaTime = 1f / ServerTickPacing.TargetTicksPerSecond;

  /// <summary>等待尾段的忙等阈值（毫秒）；超过此值先用粗粒度定时器。</summary>
  private const double WaitSpinThresholdMilliseconds = 2d;

  /// <summary>忙等时每次 <see cref="Thread.SpinWait"/> 的迭代次数。</summary>
  private const int SpinIterations = 100;

  /// <summary>单次 tick 的更新耗时告警阈值（毫秒）。</summary>
  private const double SlowTickThresholdMilliseconds = 12d;

  /// <summary>同一诊断的最小告警间隔，单位是 <see cref="Stopwatch"/> 时间戳（1 秒）。</summary>
  private static readonly long _logThrottleTimestamp = Stopwatch.Frequency;

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
    string? worldPath = args.FirstOrDefault(static argument =>
        !argument.StartsWith("--", StringComparison.Ordinal));
    LoadedServerWorld loadedWorld = ServerWorldLoader.Load(
        worldPath);
    ServerLog.Info(
        "ecs.world.tile-entities",
        $"chests={loadedWorld.TileEntities.ChestCount} signs={loadedWorld.TileEntities.SignCount} " +
        $"tileEntities={loadedWorld.TileEntities.TileEntityCount} " +
        $"pylons={loadedWorld.TileEntities.CountTileEntities(WorldTileEntityKind.TeleportationPylon)}");
    int worldWidth = loadedWorld.Header.MaxTilesX;
    int worldHeight = loadedWorld.Header.MaxTilesY;
    using ArchWorld ecsWorld = ArchWorld.Create();
    WorldRuntime worldRuntime = new(
        new WorldRuntimeStateComponent(
            worldWidth,
            worldHeight,
            initialDayTime: loadedWorld.Environment.DayTime),
        new WorldTileStorageComponent(worldWidth, worldHeight));
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
    // 会话玩家名录：领域侧只认 ISessionPlayerSource 只读端口，实现在网络程序集。
    SessionPlayerSource sessionPlayers = new(ecsWorld, entities);
    // 实体定义框架的宿主与生成器：玩家与 NPC 创建都走 EntityDefinitionSystem，
    // 不再是调用处逐个 new 组件。
    EntityDefinitionRegistry entityDefinitions = new();
    PlayerEntityDefinition.Register(
        entityDefinitions,
        PlayerInventorySlotCount,
        MaxInventoryStack,
        PlayerRespawnDelayTicks);
    NpcEntityDefinition.Register(entityDefinitions);
    float worldSurfaceY = checked((float)(loadedWorld.Environment.WorldSurface *
        WorldRuntimeStateComponent.DefaultTileSize));
    NpcAiSnapshotProvider npcAiSnapshots = new(
        ecsWorld,
        () => new NpcAiWorldFact(
            worldRuntime.State.DayTime,
            worldSurfaceY,
            slimeRain: false));
    using NpcAiRuntime npcAi = new(ecsWorld, npcAiSnapshots);
    NpcNetworkIdentitySystem npcNetwork = new(ecsWorld);
    EntityDefinitionSystem<ArchEntity> entitySpawner = new(
        entityDefinitions,
        new ArchEntityDefinitionHost(
            ecsWorld,
            entities,
            entity =>
            {
              npcAi.TryDetach(entity, out _);
              npcNetwork.TryRelease(entity);
            }));
    ArchEntity guide = CreateNpc(
        ecsWorld,
        entitySpawner,
        npcNetwork,
        new Vector2(guideSpawnTileX * 16f, guideSurfaceY),
        false,
        22);
    ArchEntity slime = CreateNpc(
        ecsWorld,
        entitySpawner,
        npcNetwork,
        new Vector2(slimeSpawnTileX * 16f, slimeSurfaceY),
        true,
        1);
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
    // 实体生成事实队列：实体生成域创建实体，Network 域分配客户端槽位。
    _ = ecsWorld.Create(new EntitySpawnFactQueueComponent());
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
    WorldInteractionSystem worldInteractions = new(ecsWorld, worldRuntime, worldRuntime);
    BuffDefinitionRegistry buffDefinitions = new();
    // 注册参考客户端药水表的三条样例，避免注册表为空导致 TryApply 恒返回 UnknownDefinition。
    BuffContent.RegisterAll(buffDefinitions);
    AttributeTickSystem attributeTick = new(ecsWorld);
    LifeAttributeSynchronizationSystem lifeAttributeSynchronization =
        new(ecsWorld);
    BuffTickSystem buffTick = new(ecsWorld, new BuffSystem(buffDefinitions));
    BuffApplyRequestSystem buffApply =
        new(ecsWorld, new BuffSystem(buffDefinitions));
    StatusEffectBehaviorSystem statusEffectBehavior =
        new(ecsWorld, buffDefinitions);
    StatusEffectPeriodicSystem statusEffectPeriodic =
        new(ecsWorld, entities);
    CooldownTickSystem cooldownTick = new(ecsWorld);
    GravityTickSystem gravityTick = new(ecsWorld);
    NpcAiIntentAdapter npcAiAdapter = new(
        ecsWorld,
        npcAi.Entities,
        npcAiSnapshots,
        entities,
        attackDefinitions);
    SpatialSimulationCoordinatorSystem spatialSimulation = new(ecsWorld, worldRuntime);
    // 刷怪表：目前只有史莱姆可被创建，表中权重与网络类型即 Version4 的出生判定常量。
    EntitySpawnTable entitySpawnTable = new(
    [
      new EntitySpawnCandidate(
          NpcEntityDefinition.SlimeId,
          NetworkType: 1,
          AiComponent.For<SlimeAiBehavior>(),
          Weight: 1),
    ]);
    // 困难模式/血月/日食的推进尚未建模，这里只带入存档头与时间事实。
    EntitySpawnEnvironment entitySpawnEnvironment = new(
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
    EntitySpawnSystem entitySpawnSystem = new(
        ecsWorld,
        worldRuntime,
        sessionPlayers,
        entitySpawner,
        npcAi,
        entitySpawnTable,
        entitySpawnEnvironment,
        worldSeed: WorldSeed(loadedWorld.Header.UniqueId));
    ICommunicationSessionPlayerLifecycle playerLifecycle =
        new ServerCommunicationSessionPlayerLifecycle(
            ecsWorld,
            entities,
            entitySpawner,
            playerSpawnPosition);
    CommunicationHostRuntime communication = new(
        new IPEndPoint(IPAddress.Loopback, ListenPort));
    CommunicationIngressPreparationSystem preparation =
        new(ecsWorld, communication.Ingress, playerLifecycle);

    IReadOnlyList<EcsServerSystemRegistration> registrations = BuildRegistrations(
        ecsWorld,
        worldRuntime,
        entities,
        sessionPlayers,
        attackDefinitions,
        physicsContacts,
        projectileHits,
        projectileLifecycle,
        communication,
        playerLifecycle,
        worldTick,
        worldInteractions,
        worldItems,
        inventories,
        npcAiAdapter,
        spatialSimulation,
        entitySpawnSystem,
        loadedWorld.TileEntities,
        LoadItemUseCatalog(),
        new ChatStore(),
        CreateChatCommands(),
        attributeTick,
        lifeAttributeSynchronization,
        buffApply,
        buffTick,
        statusEffectBehavior,
        statusEffectPeriodic,
        cooldownTick,
        gravityTick,
        npcNetwork,
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
        },
        preparation);
    using EcsServer server = EcsServer.Create(
        ecsWorld, registrations, LoadCoordinationRules());
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
      await communication.StartAsync(shutdown.Token).ConfigureAwait(false);
      ServerLog.Info(
          "ecs.server.ready",
          $"listen=127.0.0.1:{ListenPort} " +
          $"world={worldWidth}x{worldHeight} " +
          $"path={ServerLog.Quote(loadedWorld.Path)} spawn={loadedWorld.Environment.SpawnTileX}," +
          $"{loadedWorld.Environment.SpawnTileY}");
      await RunTicksAsync(server, worldTick, communication, shutdown.Token).ConfigureAwait(false);
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
      await communication.StopAsync().ConfigureAwait(false);
      using EcsServerCommandBuffer shutdownCommands = new();
      preparation.Update(new EcsServerTickContext(ecsWorld, Math.Max(0, server.LastTick), 1f / 60f, shutdownCommands));
      preparation.CloseAllSessions(shutdownCommands);
      shutdownCommands.Playback(ecsWorld);
      EntitySpawnFactQueue.Require(ecsWorld).Clear();
      if (ecsWorld.IsAlive(guide))
      {
        entitySpawner.Despawn(guide);
      }
      if (ecsWorld.IsAlive(slime))
      {
        entitySpawner.Despawn(slime);
      }
      npcAi.Dispose();
      npcNetwork.Clear();
      entities.Clear();
      ServerLog.Info("ecs.server.stopped", $"lastTick={server.LastTick}");
    }

    await communication.DisposeAsync().ConfigureAwait(false);

    return 0;
  }

  /// <summary>
  /// 加载声明式 ECS 协作规则。文件缺失或规则写错都在启动期失败，而不是等到某个 tick 才
  /// 表现出"系统跑在了错误的时点"。
  /// </summary>
  private static EcsCoordinationRuleSet LoadCoordinationRules()
  {
    string path = Path.Combine(
        AppContext.BaseDirectory, "Registration", "EcsCoordinationRules.rules");
    if (!File.Exists(path))
    {
      throw new InvalidDataException(
          $"ECS coordination rules were not found at '{path}'. The rules file is copied to the " +
          "output directory at build time; a missing file means the deployment is incomplete.");
    }

    return EcsCoordinationRuleLoader.Parse(File.ReadAllText(path));
  }

  /// <summary>
  /// 通过 LVCS 管线加载物品使用档案。内容错误以诊断列表返回，加载失败即启动失败。
  /// </summary>
  /// <remarks>
  /// 走分派而非直接 new codec：装配代码只声明"要 ItemUseDefinitionCatalog"，
  /// 由注册表选出唯一能产出该类型的 codec。加新族时这里不变。
  /// </remarks>
  private static ItemUseDefinitionCatalog LoadItemUseCatalog()
  {
    LvcsCodecRegistry registry = new(new LvcsLoader());
    registry.Register(new ItemUseCodec());

    if (!registry.TryResolve<ItemUseDefinitionCatalog>(
            ".json", out ILvcsCodecDescriptor? descriptor, out string? reason))
    {
      throw new InvalidDataException($"Item use codec could not be resolved: {reason}");
    }

    string directory = Path.Combine(
        AppContext.BaseDirectory, "Content", "Prototypes", "ItemUse");
    FileLvcsSource source = new(descriptor!.Family, directory, "*.json");
    object? loaded = descriptor.Load(source, out IReadOnlyList<LvcsDiagnostic> diagnostics);
    if (loaded is not ItemUseDefinitionCatalog catalog)
    {
      StringBuilder report = new();
      report.AppendLine("Item use content could not be loaded:");
      foreach (LvcsDiagnostic diagnostic in diagnostics)
      {
        report.AppendLine(diagnostic.ToString());
      }

      throw new InvalidDataException(report.ToString());
    }

    return catalog;
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
      SessionPlayerSource sessionPlayers,
      CombatAttackDefinitionCatalog attackDefinitions,
      PhysicsContactCaptureSystem physicsContacts,
      ProjectileHitResolutionSystem projectileHits,
      ProjectileLifecycleSystem projectileLifecycle,
      CommunicationHostRuntime communication,
      ICommunicationSessionPlayerLifecycle playerLifecycle,
      WorldTickSystem worldTick,
      WorldInteractionSystem worldInteractions,
      WorldItemStore worldItems,
      InventorySystem inventories,
      NpcAiIntentAdapter npcAiAdapter,
      SpatialSimulationCoordinatorSystem spatialSimulation,
      EntitySpawnSystem entitySpawnSystem,
      WorldTileEntityStore tileEntities,
      ItemUseDefinitionCatalog itemUseDefinitions,
      ChatStore chatStore,
      ChatCommandCatalog chatCommands,
      AttributeTickSystem attributeTick,
      LifeAttributeSynchronizationSystem lifeAttributeSynchronization,
      BuffApplyRequestSystem buffApply,
      BuffTickSystem buffTick,
      StatusEffectBehaviorSystem statusEffectBehavior,
      StatusEffectPeriodicSystem statusEffectPeriodic,
      CooldownTickSystem cooldownTick,
      GravityTickSystem gravityTick,
      NpcNetworkIdentitySystem npcNetwork,
      NpcNetworkProjectionSystem npcProjection,
      WorldNetworkProjectionSystem worldProjection,
      TerrariaProtocolOptions protocolOptions,
      CommunicationIngressPreparationSystem preparation)
  {
    List<EcsServerSystemRegistration> registrations = [];
    // 必须排在本阶段最前：准备阶段收包写出的玩家输入要在任何移动/属性系统读取前落地。
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Simulation,
        EcsServerRegistrationKeys.PlayerInputApply,
        new PlayerInputRequestSystem(world, entities, inventories),
        reason: "Preparation communication facts must be available before player input is applied."));
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Simulation,
        EcsServerRegistrationKeys.LifeAttributeSynchronization,
        lifeAttributeSynchronization,
        reason: "Life and shield capacities observe the current attribute set."));
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Projection,
        EcsServerRegistrationKeys.NetworkNpcProjection,
        npcProjection,
        reason: "Projection observes the committed NPC identity and state phase contract."));
    // 聊天投影紧挨 NPC 投影：两者都只写 outbound 意图，顺序不影响语义。
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Projection,
        EcsServerRegistrationKeys.ChatNetworkProjection,
        new ChatNetworkProjectionSystem(world, entities)));
    // 世界物品请求只在 Commit 阶段执行。Projection 可以表达掉落意图，但权威
    // WorldItemStore 只能由这个 Commit owner 写入；投影产生的请求留到下一 tick。
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Commit,
        EcsServerRegistrationKeys.WorldItemRequestCommit,
        new WorldItemRequestSystem(world, worldItems),
        reason: "World-item requests mutate the authoritative store only at the Commit barrier."));
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Projection,
        EcsServerRegistrationKeys.NetworkWorldProjection,
        worldProjection,
        reason: "World network projection observes the committed world-item store and emits intents for the next Commit."));
    registrations.AddRange(CommunicationSystemRegistration.CreateGame(
        world,
        entities,
        attackDefinitions,
        communication.OutboundPort,
        protocolOptions: protocolOptions,
        worldItems: worldItems,
        ingress: communication.Ingress,
        playerLifecycle: playerLifecycle,
        preparation: preparation,
        outboundProjectionDependencies:
        [
          EcsServerRegistrationKeys.NetworkNpcProjection,
          EcsServerRegistrationKeys.ChatNetworkProjection,
          EcsServerRegistrationKeys.NetworkWorldProjection
        ]));
    registrations.AddRange(CombatSystemRegistration.Create(
        world,
        entities,
        entities,
        physicsContacts,
        projectileHits,
        projectileLifecycle,
        attackDefinitions,
        PlayerHurtProtectionTicks,
        attributeDependencyKeys: null));
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Simulation,
        EcsServerRegistrationKeys.WorldTick,
        worldTick));
    // 世界物品 tick 紧跟世界时间推进：本 tick 的重力/碰撞结果不依赖后续系统，
    // 但拾取请求必须在同一 tick 的 Inventory 步骤之前入队。
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Simulation,
        EcsServerRegistrationKeys.WorldItemTick,
        new WorldItemTickSystem(
            world,
            worldItems,
            worldRuntime,
            worldRuntime,
            sessionPlayers,
            MaxInventoryStack),
        after: [EcsServerRegistrationKeys.WorldTick],
        reason: "World-item motion follows the world clock and produces same-tick pickup facts."));
    // 拾取请求由 Inventory 域结算并回写 Consume 请求；投影阶段之前跑完，同帧内一致。
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Simulation,
        EcsServerRegistrationKeys.InventoryPickupRequest,
        new InventoryPickupRequestSystem(world, entities, inventories),
        after: [EcsServerRegistrationKeys.WorldItemTick],
        reason: "Inventory pickup consumes the world-item facts produced earlier in Simulation."));
    // 刷怪在移动求解之前跑：本 tick 创建的 NPC 从下一 tick 起由 AI 与空间求解驱动，
    // 避免在同一个 tick 里对新实体重复计算移动。
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Simulation,
        EcsServerRegistrationKeys.EntitySpawn,
        entitySpawnSystem));
    // 槽位分配必须早于 Projection 阶段的 NpcNetworkProjectionSystem，否则新 NPC 本 tick 不可见。
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Commit,
        EcsServerRegistrationKeys.NpcNetworkIdentity,
        npcNetwork,
        reason: "NPC network identity is a structural ECS write owned by the Commit barrier."));
    // 箱子/告示牌意图在投影之前落地，开箱状态与文本改动同 tick 可被读取。
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Simulation,
        EcsServerRegistrationKeys.WorldTileEntityRequest,
        new WorldTileEntityRequestSystem(world, tileEntities)));
    // 物品使用在投影之前：本 tick 的射击事实留给投射物/战斗域消费。
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Simulation,
        EcsServerRegistrationKeys.PlayerItemUse,
        new PlayerItemUseSystem(world, entities, itemUseDefinitions, inventories)));
    // 聊天：能力组件决定谁能说话，系统的广播事实交给 Projection 阶段的聊天投影。
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Simulation,
        EcsServerRegistrationKeys.ChatDomain,
        new ChatSystem(world, entities, sessionPlayers, chatCommands, chatStore)));
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Simulation,
        EcsServerRegistrationKeys.NpcAiIntent,
        npcAiAdapter));
    // 重力投影必须先于空间求解刷新，SpatialSimulationCoordinatorSystem 直接读取 GravityStateComponent。
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Simulation,
        EcsServerRegistrationKeys.GravityTick,
        gravityTick));
    // 属性与 Buff 的期限在 Commit 之前推进，使本 tick 内新加的来源获得完整时长。
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Simulation,
        EcsServerRegistrationKeys.AttributeTick,
        attributeTick,
        reason: "Attribute expiry runs at the Attributes lifecycle boundary."));
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Simulation,
        EcsServerRegistrationKeys.BuffApply,
        buffApply,
        after: [EcsServerRegistrationKeys.AttributeTick],
        reason: "Buff application requests are admitted before status expiry and behavior sync."));
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Simulation,
        EcsServerRegistrationKeys.BuffTick,
        buffTick,
        after: [EcsServerRegistrationKeys.BuffApply],
        reason: "Buff expiry follows the attribute lifecycle pass."));
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Simulation,
        EcsServerRegistrationKeys.StatusEffectBehavior,
        statusEffectBehavior,
        after: [EcsServerRegistrationKeys.BuffTick],
        reason: "Behavior components follow status-effect admission and expiry."));
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Simulation,
        EcsServerRegistrationKeys.StatusEffectPeriodic,
        statusEffectPeriodic,
        after: [EcsServerRegistrationKeys.StatusEffectBehavior],
        reason: "Periodic status effects emit Life and Combat requests after behavior sync."));
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Simulation,
        EcsServerRegistrationKeys.CooldownTick,
        cooldownTick));
    // 必须紧邻空间求解之前：本帧由 AI 与输入适配器提交的意图要在求解读取前落地。
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Simulation,
        EcsServerRegistrationKeys.MovementIntentApply,
        new MovementIntentRequestSystem(world, entities),
        after:
        [
          EcsServerRegistrationKeys.PlayerInputApply,
          EcsServerRegistrationKeys.NpcAiIntent
        ],
        reason: "Player and NPC intent producers must finish before movement intent is applied."));
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Simulation,
        EcsServerRegistrationKeys.SpatialSimulation,
        spatialSimulation,
        after:
        [
          EcsServerRegistrationKeys.GravityTick,
          EcsServerRegistrationKeys.MovementIntentApply
        ],
        reason: "Spatial simulation reads the refreshed gravity state and applied movement intents."));
    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Commit,
        EcsServerRegistrationKeys.WorldInteractionCommit,
        worldInteractions,
        reason: "World interactions commit before the Projection phase observes committed facts."));
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

  private sealed class ServerCommunicationSessionPlayerLifecycle
      : ICommunicationSessionPlayerLifecycle
  {
    private readonly ArchWorld _world;
    private readonly EntityRuntimeRegistry _entities;
    private readonly EntityDefinitionSystem<ArchEntity> _entitySpawner;
    private readonly Vector2 _spawnPosition;
    private readonly Dictionary<SessionEpoch, EcsServerEntityCreationTicket> _pending = [];

    public ServerCommunicationSessionPlayerLifecycle(
        ArchWorld world,
        EntityRuntimeRegistry entities,
        EntityDefinitionSystem<ArchEntity> entitySpawner,
        Vector2 spawnPosition)
    {
      _world = world ?? throw new ArgumentNullException(nameof(world));
      _entities = entities ?? throw new ArgumentNullException(nameof(entities));
      _entitySpawner = entitySpawner ?? throw new ArgumentNullException(nameof(entitySpawner));
      _spawnPosition = spawnPosition;
    }

    public EntityRuntimeHandle CreatePlayer(SessionEpoch epoch, EcsServerCommandBuffer commands)
    {
      EcsServerEntityCreationTicket ticket = _entitySpawner.SpawnDeferred(
          PlayerEntityDefinition.Id,
          commands);
      commands.Set(ticket.BufferedEntity, new LocationComponent(_spawnPosition));
      if (ticket.RuntimeHandle is not EntityRuntimeHandle handle)
      {
        throw new InvalidOperationException("The deferred player definition did not allocate a runtime handle.");
      }

      _pending[epoch] = ticket;
      commands.RegisterAfterPlayback(_ => _pending.Remove(epoch));
      commands.RegisterOnDiscard(() => _pending.Remove(epoch));
      ServerLog.Info("ecs.session.player_bound", $"epoch={epoch.Value}");
      return handle;
    }

    public void DestroyPlayer(
        SessionEpoch epoch,
        EntityRuntimeHandle player,
        EcsServerCommandBuffer commands)
    {
      if (_pending.Remove(epoch, out EcsServerEntityCreationTicket? ticket))
      {
        commands.Destroy(ticket.BufferedEntity);
      }
      else if (_entities.TryResolve(player, out ArchEntity entity) && _world.IsAlive(entity))
      {
        commands.Destroy(entity);
      }

      ServerLog.Info("ecs.session.player_unbound", $"epoch={epoch.Value}");
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
      EntityDefinitionSystem<ArchEntity> entitySpawner,
      NpcNetworkIdentitySystem npcNetwork,
      Vector2 position,
      bool isSlime,
      short networkType)
  {
    ArgumentNullException.ThrowIfNull(npcNetwork);
    ArchEntity entity = entitySpawner.Spawn(
        isSlime ? NpcEntityDefinition.SlimeId : NpcEntityDefinition.GuideId);
    try
    {
      // 出生坐标与网络标识属于实例级输入，定义里只有占位值。
      world.Set(
          entity,
          new LocationComponent(
              position - new Vector2(0f, NpcEntityDefinition.SpawnOffsetY(isSlime))));
      if (!npcNetwork.TryBind(entity, networkType, out _))
      {
        throw new InvalidOperationException();
      }
      return entity;
    }
    catch
    {
      entitySpawner.Despawn(entity);
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

  /// <summary>按固定 60 Hz 网格推进 ECS 服务器 tick。</summary>
  /// <remarks>
  /// 快照节拍优先于追赶速度：某次 tick 超时会丢弃错过的格点（模拟整体变慢），而不是立刻补发
  /// 下一条 tick——补发会让包 23 在 10/25 ms 之间摆动，而客户端在权威模式下不做插值。
  /// </remarks>
  private static async Task RunTicksAsync(
      EcsServer server,
      WorldTickSystem worldTick,
      CommunicationHostRuntime communication,
      CancellationToken cancellationToken)
  {
    long period = ServerTickPacing.PeriodTimestamp;
    long deadline = Stopwatch.GetTimestamp();
    long tick = 0;
    long skippedTotal = 0;
    long lastSkipLog = -_logThrottleTimestamp;
    long lastSlowLog = -_logThrottleTimestamp;
    long lastTimelinessLog = Stopwatch.GetTimestamp() - _logThrottleTimestamp;
    double slowestMilliseconds = 0d;
    while (!cancellationToken.IsCancellationRequested)
    {
      await WaitForDeadlineAsync(deadline, cancellationToken).ConfigureAwait(false);
      long updateStart = Stopwatch.GetTimestamp();
      server.Update(tick++, TickDeltaTime);
      long now = Stopwatch.GetTimestamp();
      double updateMilliseconds = (now - updateStart) * 1000d / Stopwatch.Frequency;
      if (updateMilliseconds > SlowTickThresholdMilliseconds)
      {
        slowestMilliseconds = Math.Max(slowestMilliseconds, updateMilliseconds);
        if (now - lastSlowLog >= _logThrottleTimestamp)
        {
          ServerLog.Warning(
              "ecs.server.tick_slow",
              $"elapsedMs={updateMilliseconds:F1} maxMs={slowestMilliseconds:F1} tick={tick} " +
              $"prep={server.LastPreparationMilliseconds:F1} " +
              $"sim={server.LastSimulationMilliseconds:F1} " +
              $"commit={server.LastCommitMilliseconds:F1} " +
              $"proj={server.LastProjectionMilliseconds:F1} " +
              $"clock={worldTick.LastClockMilliseconds:F2} " +
              $"liquid={worldTick.LastLiquidMilliseconds:F2} " +
              $"liquidOps={worldTick.LastLiquidOperations} " +
              $"top=[{string.Join(' ', server.DescribeSlowestSystems())}]");
          slowestMilliseconds = 0d;
          lastSlowLog = now;
        }
      }

      deadline = ServerTickPacing.AdvanceDeadline(deadline, now, period, out long skipped);
      if (now - lastTimelinessLog >= _logThrottleTimestamp)
      {
        // 每秒一条：直接回答“A 批能不能在自己那个 tick 里发完”，比平均间隔更接近体感。
        TickBatchTimeliness timeliness = communication.TickBatchTimeliness;
        if (timeliness.SealedBatches > 0)
        {
          ServerLog.Info(
              "ecs.outbound.timeliness",
              $"sealed={timeliness.SealedBatches} overdue={timeliness.OverdueBatches} " +
              $"overdueRatio={timeliness.OverdueRatio * 100:F2}% " +
              $"maxLatencyMs={timeliness.MaxLatencyMilliseconds:F2} tick={tick}");
        }

        lastTimelinessLog = now;
      }

      if (skipped > 0)
      {
        skippedTotal += skipped;
        if (now - lastSkipLog >= _logThrottleTimestamp)
        {
          // 重负载世界每个 tick 都可能丢格点；按秒节流，只报累计值，避免告警淹没日志。
          ServerLog.Warning(
              "ecs.server.tick_skipped",
              $"skipped={skipped} total={skippedTotal} tick={tick}");
          lastSkipLog = now;
        }
      }
    }
  }

  /// <summary>等待到指定格点；尾部用忙等换取亚毫秒精度。</summary>
  private static async Task WaitForDeadlineAsync(long deadline, CancellationToken cancellationToken)
  {
    long remaining = deadline - Stopwatch.GetTimestamp();
    while (remaining > 0)
    {
      double remainingMilliseconds = remaining * 1000d / Stopwatch.Frequency;
      if (remainingMilliseconds > WaitSpinThresholdMilliseconds)
      {
        // 粗粒度定时器负责大部分等待，尾部交给忙等，避免 15.6 ms 的系统定时器粒度主导节拍。
        TimeSpan delay = TimeSpan.FromMilliseconds(
            remainingMilliseconds - WaitSpinThresholdMilliseconds);
        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
      }
      else
      {
        Thread.SpinWait(SpinIterations);
      }

      remaining = deadline - Stopwatch.GetTimestamp();
    }

    cancellationToken.ThrowIfCancellationRequested();
  }
}

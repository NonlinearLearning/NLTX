using System;
using System.Buffers.Binary;
using System.Linq;
using System.Numerics;
using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Ai;
using Terraria.Share.Combat;
using Terraria.Network;
using Terraria.Unshared.Session;
using Terraria.Share.Entities;
using Terraria.Share.Inventory;
using Terraria.Share.Items;
using Terraria.Share.Liquid;
using Terraria.Share.Movement;
using Terraria.Unshared.Npc;
using Terraria.Share.Physics;
using Terraria.Unshared.Player;
using Terraria.Share.Relationships;
using Terraria.Unshared.World;
using MovementVelocityComponent = Terraria.Share.Movement.VelocityComponent;

using ArchWorld world = ArchWorld.Create();
EntityRuntimeRegistry entities = new(worldScope: 1);
ArchEntity player = world.Create(
    new EntityFactionComponent(Faction.Friendly),
    new LifeComponent(100, 100),
    new LocationComponent(new Vector2(300f, 100f)),
    new InventoryComponent([
        new InventorySlotDefinition(0, InventorySlotKind.General, ItemCategoryMask.Any, 9999)]));
ArchEntity guide = CreateNpc(world, entities, new Vector2(20f, 100f), isSlime: false);
ArchEntity slime = CreateNpc(world, entities, new Vector2(100f, 100f), isSlime: true);

using NpcAiRuntime ai = new(world);
ai.AttachGuide(guide);
ai.AttachSlime(slime);
CombatAttackCatalog attackDefinitions = new();
attackDefinitions.Seal();
// 攻击意图队列现在挂在世界级实体上，验证程序也必须建出这一实体。
_ = world.Create(new CombatAttackIntentQueueComponent());
_ = world.Create(new MovementIntentRequestQueueComponent());
NpcAiIntentAdapter adapter = new(
    world,
    ai.Entities,
    ai.Snapshots,
    entities,
    attackDefinitions,
    ai.Catalog);
adapter.Initialize();
// AI 现在只提交移动意图，由 Movement 域的系统安装到组件上。
MovementIntentRequestSystem movementIntents = new(world, entities);

WorldRuntime spatialWorld = new(64, 64);
WorldInteractionResult load = spatialWorld.Load(
    Enumerable.Repeat(WorldTileState.Empty, 64 * 64).ToArray());
if (!load.Succeeded)
{
  throw new InvalidOperationException(load.Failure);
}

SpatialSimulationCoordinatorSystem spatial = new(world, spatialWorld);
adapter.Update(new EcsServerTickContext(world, 0, 1f / 60f));
movementIntents.Update(new EcsServerTickContext(world, 0, 1f / 60f));
ref MovementIntent initialSlimeIntent = ref world.Get<MovementIntent>(slime);
Require(initialSlimeIntent.Source == MovementIntentSource.Ai, "Slime AI intent was not adapted.");
Require(initialSlimeIntent.DesiredVelocity.X > 0f, "Slime did not target the player to its right.");
Require(world.Get<NpcBehaviorStateComponent>(slime).Phase == NpcBehaviorPhase.Chase,
    "Slime did not enter Chase state.");
Require(world.Get<NpcBehaviorStateComponent>(guide).Phase == NpcBehaviorPhase.Idle,
    "Guide did not start in Idle state.");

float originalX = world.Get<LocationComponent>(slime).Coordinates.X;
spatial.Update(new EcsServerTickContext(world, 0, 1f / 60f));
Require(world.Get<LocationComponent>(slime).Coordinates.X > originalX,
    "Spatial simulation did not commit the Slime's movement.");

bool observedJump = false;
bool observedJumpMovement = false;
for (long tick = 1; tick <= 245; tick++)
{
  ref GroundingComponent grounding = ref world.Get<GroundingComponent>(slime);
  grounding = new GroundingComponent(true, false, Vector2.UnitY);
  ref GroundingComponent guideGrounding = ref world.Get<GroundingComponent>(guide);
  guideGrounding = new GroundingComponent(true, false, Vector2.UnitY);
  adapter.Update(new EcsServerTickContext(world, tick, 1f / 60f));
movementIntents.Update(new EcsServerTickContext(world, tick, 1f / 60f));
  if (world.Get<MovementIntent>(slime).JumpRequested)
  {
    observedJump = true;
    float beforeJumpY = world.Get<LocationComponent>(slime).Coordinates.Y;
    spatial.Update(new EcsServerTickContext(world, tick, 1f / 60f));
    observedJumpMovement |= world.Get<LocationComponent>(slime).Coordinates.Y < beforeJumpY;
  }
}

Require(observedJump, "Slime did not request a grounded jump.");
Require(observedJumpMovement, "Spatial simulation did not apply the Slime jump request.");
Require(world.Get<NpcBehaviorStateComponent>(guide).Phase == NpcBehaviorPhase.Wander,
    "Guide did not transition from Idle to Wander.");
Require(world.Get<MovementIntent>(guide).DesiredVelocity.X != 0f,
    "Guide did not produce a walking movement intent.");
for (long tick = 246; tick <= 500; tick++)
{
  ref GroundingComponent guideGrounding = ref world.Get<GroundingComponent>(guide);
  guideGrounding = new GroundingComponent(true, false, Vector2.UnitY);
  adapter.Update(new EcsServerTickContext(world, tick, 1f / 60f));
movementIntents.Update(new EcsServerTickContext(world, tick, 1f / 60f));
}

Require(world.Get<NpcBehaviorStateComponent>(guide).Phase == NpcBehaviorPhase.Idle,
    "Guide did not return to Idle state.");
Require(world.Get<MovementIntent>(guide).DesiredVelocity == Vector2.Zero,
    "Guide retained walking velocity after entering Idle.");
Require(ai.Entities.BindingCount == 2, "Guide and Slime AI bindings were not retained.");
adapter.Dispose();
VerifyDeclarationDrivesInputs(world);
VerifyAnchorOrdering();
VerifyGeneratedEdgesAreLoadBearing();
VerifyCommitTable();
VerifyNpcNetworkProjection();
VerifyNearestEntityQuery();
string sharedApiSummary = Terraria.Verification.SharedApiUsage.Verify();
Console.WriteLine(sharedApiSummary);
Console.WriteLine(
    "PASS: Guide idle/wander/stop, Slime targeting/jump, AI intent, spatial movement and NPC wire projection.");

// 声明是唯一通道：目录的读取契约必须与运行时一致，无法投影的声明必须在建服期被拒绝。
static void VerifyDeclarationDrivesInputs(ArchWorld world)
{
  NpcAiSnapshotProvider provider = new(world);
  Require(provider.FactTypes.Count == 4, "The default NPC provider no longer projects four facts.");
  NpcAiCatalog catalog = NpcAiDeclaration.CreateCatalog();
  Require(catalog.IsSealed && catalog.Declarations.Count == 2,
      "The NPC catalog is not a sealed two-behavior set.");
  NpcAiIntentCommitTable.EnsureComplete();
  foreach (NpcAiBehaviorDeclaration declaration in catalog.Declarations)
  {
    foreach (Type factType in declaration.Behavior.Reads.Components)
    {
      Require(provider.FactTypes.Contains(factType),
          $"{declaration.Behavior.BehaviorType.Name} declares an unprojectable fact: {factType.Name}");
    }

    AiBehavior<NpcAiIntent> behavior = declaration.Behavior.CreateBehavior();
    Require(behavior.IntentType == typeof(NpcAiIntent),
        "A declared runtime does not carry the NPC intent type.");
    Require(behavior.RequiredComponents.SequenceEqual(declaration.Behavior.Reads.Components),
        $"{declaration.Behavior.BehaviorType.Name} reads a different set than its declaration.");
    Require(declaration.Emits.Count != 0,
        $"{declaration.Behavior.BehaviorType.Name} declares no emitted intent kind.");
    NpcAiIntentCommitTable.EnsureDispatches(
        declaration.Emits, declaration.Behavior.BehaviorType);
  }

  // 声明了适配器没有分支的 kind 必须在建服期被拒绝，而不是等到 tick 抛异常。
  bool undispatchableRejected = false;
  try
  {
    NpcAiBehaviorDeclaration undispatchable = new(
        catalog.Declarations[0].Behavior,
        new[] { (NpcAiIntentKind)byte.MaxValue },
        after: [],
        before: []);
    NpcAiIntentCommitTable.EnsureDispatches(
        undispatchable.Emits, undispatchable.Behavior.BehaviorType);
  }
  catch (ArgumentOutOfRangeException)
  {
    undispatchableRejected = true;
  }
  catch (InvalidOperationException)
  {
    undispatchableRejected = true;
  }

  Require(undispatchableRejected, "An undefined intent kind was accepted by the declaration.");

  bool rejected = false;
  try
  {
    _ = new NpcAiRuntime(catalog, new NpcAiSnapshotProvider(Array.Empty<INpcAiSnapshotContributor>()));
  }
  catch (InvalidOperationException)
  {
    rejected = true;
  }

  Require(rejected, "A catalog whose inputs cannot be projected was accepted.");
  Console.WriteLine(
      $"PASS declaration-drives-inputs | declared={catalog.Declarations.Count} " +
      $"facts={provider.FactTypes.Count} emits={catalog.DeclaredEmits().Count} " +
      "unprojectable-rejected=True undispatchable-rejected=True");
}

static bool Throws<TException>(Action action) where TException : Exception
{
  try
  {
    action();
    return false;
  }
  catch (TException)
  {
    return true;
  }
}

// P3：锚点是能力角色，绑定是全覆盖的，生成的边与规则是同一份声明的投影。
static void VerifyAnchorOrdering()
{
  AiAnchor[] anchors = Enum.GetValues<AiAnchor>();
  foreach (AiAnchor anchor in anchors)
  {
    string name = anchor.ToString();
    Require(name.IndexOf('.') < 0, $"Anchor {name} is a registration key, not a capability role.");
    Require(!name.Contains("System", StringComparison.Ordinal),
        $"Anchor {name} names a system instead of the step it stands for.");
  }

  Dictionary<AiAnchor, string> complete = new()
  {
    [AiAnchor.InputApply] = "test.input.apply",
    [AiAnchor.MovementCommit] = "test.movement.commit",
  };
  AiAnchorBindings bindings = AiAnchorBindings.Create(complete);
  Require(bindings.Anchors.SequenceEqual(anchors),
      "The binding table does not cover the anchor set in declaration order.");
  Require(bindings.Keys().SequenceEqual(complete.Values),
      "The binding table resolves anchors to different keys than it declares.");

  // 每个锚点恰好绑定一次：少一个、重复键、未定义锚点、空白键都必须被拒绝。
  Dictionary<AiAnchor, string> missing = new(complete);
  missing.Remove(AiAnchor.MovementCommit);
  Require(Throws<ArgumentException>(() => AiAnchorBindings.Create(missing)),
      "A binding table missing an anchor was accepted.");
  Dictionary<AiAnchor, string> duplicateKey = new(complete)
  {
    [AiAnchor.MovementCommit] = complete[AiAnchor.InputApply]
  };
  Require(Throws<ArgumentException>(() => AiAnchorBindings.Create(duplicateKey)),
      "Two anchors binding the same registration key were accepted.");
  Dictionary<AiAnchor, string> undefined = new(complete)
  {
    [(AiAnchor)byte.MaxValue] = "test.undefined"
  };
  Require(Throws<ArgumentOutOfRangeException>(() => AiAnchorBindings.Create(undefined)),
      "An undefined anchor was accepted by the binding table.");
  Dictionary<AiAnchor, string> blank = new(complete)
  {
    [AiAnchor.InputApply] = " test.input.apply"
  };
  Require(Throws<ArgumentException>(() => AiAnchorBindings.Create(blank)),
      "A binding key with surrounding whitespace was accepted.");
  Require(Throws<ArgumentOutOfRangeException>(() => bindings.KeyOf((AiAnchor)byte.MaxValue)),
      "Resolving an undefined anchor did not fail.");

  // 每个绑定键真实存在：宿主拿实际调度键集合校验，缺一个就是启动失败。
  string[] scheduleKeys = [.. complete.Values, "test.unrelated"];
  bindings.EnsureResolvable(scheduleKeys);
  Require(Throws<InvalidOperationException>(() => bindings.EnsureResolvable(scheduleKeys[1..])),
      "A binding that names no registration in the schedule was accepted.");

  // 边与规则是同一份声明投影：两个行为共享锚点，只应产生两条边。
  NpcAiCatalog catalog = NpcAiDeclaration.CreateCatalog();
  IReadOnlyList<NpcAiOrderingEdge> edges = catalog.Edges(bindings, "npc.ai.intent");
  Require(edges.Count == 2, $"The declarations produced {edges.Count} edges instead of 2.");
  Require(edges[0].BeforeKey == "test.input.apply" && edges[0].AfterKey == "npc.ai.intent",
      "NPC AI is not declared to run after applied input.");
  Require(edges[1].BeforeKey == "npc.ai.intent" && edges[1].AfterKey == "test.movement.commit",
      "NPC AI is not declared to run before movement intent apply.");
  IReadOnlyList<EcsCoordinationRule> rules = catalog.OrderingRules(bindings, "npc.ai.intent");
  Require(rules.Count == edges.Count, "The generated rule set does not mirror the edge set.");
  for (int i = 0; i < rules.Count; i++)
  {
    Require(rules[i].Kind == EcsCoordinationRuleKind.Before &&
        rules[i].Key == edges[i].BeforeKey && rules[i].OtherKey == edges[i].AfterKey,
        "A generated rule does not match its edge.");
  }

  // 同一锚点不自相矛盾：一条声明要求 AI 之后执行，另一条要求它之前执行。
  NpcAiCatalog contradictory = new();
  contradictory.Add(catalog.Declarations[0]);
  contradictory.Add(new NpcAiBehaviorDeclaration(
      catalog.Declarations[1].Behavior,
      new[] { NpcAiIntentKind.Move },
      after: new[] { AiAnchor.MovementCommit },
      before: new[] { AiAnchor.InputApply }));
  contradictory.Seal();
  Require(Throws<InvalidOperationException>(
          () => contradictory.EnsureConsistent(bindings, "npc.ai.intent")),
      "Contradictory anchors across declarations were accepted.");
  Console.WriteLine(
      $"PASS anchor-ordering | anchors={anchors.Length} edges={edges.Count} " +
      $"rules={rules.Count} unbound-rejected=True duplicate-rejected=True " +
      "contradiction-rejected=True");
}

// P3 验收：删掉手写的 after 项后，调度仍由声明生成的边摆正；边一丢，顺序就错。
static void VerifyGeneratedEdgesAreLoadBearing()
{
  AiAnchorBindings bindings = AiAnchorBindings.Create(new Dictionary<AiAnchor, string>
  {
    [AiAnchor.InputApply] = "player.input.apply",
    [AiAnchor.MovementCommit] = "movement.intent.apply",
  });
  NpcAiCatalog catalog = NpcAiDeclaration.CreateCatalog();
  IReadOnlyList<NpcAiOrderingEdge> edges = catalog.Edges(bindings, "npc.ai.intent");
  string[] after = [.. edges.Where(e => e.AfterKey == "npc.ai.intent").Select(e => e.BeforeKey)];
  string[] before = [.. edges.Where(e => e.BeforeKey == "npc.ai.intent").Select(e => e.AfterKey)];
  Require(after.SequenceEqual(new[] { "player.input.apply" }),
      "The declarations do not put NPC AI after applied input.");
  Require(before.SequenceEqual(new[] { "movement.intent.apply" }),
      "The declarations do not put NPC AI before movement intent apply.");

  // 注册顺序故意反写：只有生成的边能把它摆正。
  using ArchWorld withEdgesWorld = ArchWorld.Create();
  using EcsServer withEdges = EcsServer.Create(
      withEdgesWorld,
      [
          new EcsServerSystemRegistration(
              EcsServerPhase.Simulation,
              "movement.intent.apply",
              new ProbeSystem(withEdgesWorld),
              reason: "probe"),
          new EcsServerSystemRegistration(
              EcsServerPhase.Simulation,
              "npc.ai.intent",
              new ProbeSystem(withEdgesWorld),
              before: before,
              after: after,
              reason: "probe"),
          new EcsServerSystemRegistration(
              EcsServerPhase.Simulation,
              "player.input.apply",
              new ProbeSystem(withEdgesWorld),
              reason: "probe")
      ]);
  IReadOnlyList<string> schedule = withEdges.OrderedRegistrationKeys;
  Require(Index(schedule, "player.input.apply") < Index(schedule, "npc.ai.intent"),
      "NPC AI did not run after applied input once the edge came from the declaration.");
  Require(Index(schedule, "npc.ai.intent") < Index(schedule, "movement.intent.apply"),
      "NPC AI did not run before movement intent apply once the edge came from the declaration.");

  // 同一条边删掉：注册顺序照旧，顺序就错了——探针因此是载荷，不是装饰。
  using ArchWorld bareWorld = ArchWorld.Create();
  using EcsServer bare = EcsServer.Create(
      bareWorld,
      [
          new EcsServerSystemRegistration(
              EcsServerPhase.Simulation,
              "movement.intent.apply",
              new ProbeSystem(bareWorld),
              reason: "probe"),
          new EcsServerSystemRegistration(
              EcsServerPhase.Simulation,
              "npc.ai.intent",
              new ProbeSystem(bareWorld),
              reason: "probe"),
          new EcsServerSystemRegistration(
              EcsServerPhase.Simulation,
              "player.input.apply",
              new ProbeSystem(bareWorld),
              reason: "probe")
      ]);
  Require(Index(bare.OrderedRegistrationKeys, "npc.ai.intent") >
          Index(bare.OrderedRegistrationKeys, "movement.intent.apply"),
      "Dropping the generated edge did not change the schedule, so the probe proves nothing.");
  Console.WriteLine(
      $"PASS declaration-edges | after={after.Length} before={before.Length} " +
      "generated-order-holds=True edge-dropped-changes-order=True");

  static int Index(IReadOnlyList<string> keys, string key)
  {
    for (int i = 0; i < keys.Count; i++)
    {
      if (string.Equals(keys[i], key, StringComparison.Ordinal))
      {
        return i;
      }
    }

    throw new InvalidOperationException($"Registration '{key}' is missing from the schedule.");
  }
}

// P4：提交表覆盖每个 kind，只写允许的组件，Commits 是它的投影。
static void VerifyCommitTable()
{
  NpcAiIntentCommitTable.EnsureComplete();
  NpcAiIntentKind[] kinds = Enum.GetValues<NpcAiIntentKind>();
  Require(NpcAiIntentCommitTable.Commits.Count == kinds.Length,
      "The commit table does not have one row per intent kind.");
  foreach (NpcAiIntentCommit commit in NpcAiIntentCommitTable.Commits)
  {
    Require(NpcAiIntentCommitTable.AllowedTargets.Contains(commit.Target),
        $"{commit.Kind} commits to {commit.Target.Name}, which is outside the AI write boundary.");
    Require(NpcAiIntentCommitTable.Get(commit.Kind) == commit,
        $"{commit.Kind} does not resolve to its own row.");
  }

  Require(Throws<KeyNotFoundException>(
          () => NpcAiIntentCommitTable.Get((NpcAiIntentKind)byte.MaxValue)),
      "An unhandled intent kind resolved to a commit row.");

  NpcAiCatalog catalog = NpcAiDeclaration.CreateCatalog();
  HashSet<Type> declared = [];
  foreach (NpcAiBehaviorDeclaration declaration in catalog.Declarations)
  {
    Require(declaration.Commits.Count != 0,
        $"{declaration.Behavior.BehaviorType.Name} declares no commit target.");
    Require(declaration.Commits.SequenceEqual(
            NpcAiIntentCommitTable.TargetsOf(declaration.Emits)),
        $"{declaration.Behavior.BehaviorType.Name} commits somewhere other than the table says.");
    foreach (Type target in declaration.Commits)
    {
      declared.Add(target);
    }
  }

  Require(declared.SetEquals(catalog.DeclaredCommits()),
      "The declared commit set does not project from the declarations.");
  Require(declared.Contains(typeof(NpcBehaviorStateComponent)),
      "The NPC AI domain does not declare its own behavior state as a commit target.");
  Require(declared.Contains(typeof(MovementIntentRequestQueueComponent)),
      "The NPC AI domain does not declare the movement request queue as a commit target.");
  Console.WriteLine(
      $"PASS commit-table | kinds={kinds.Length} targets={declared.Count} " +
      $"allowed={NpcAiIntentCommitTable.AllowedTargets.Count} unhandled-rejected=True");
}

static ArchEntity CreateNpc(
    ArchWorld world,
    EntityRuntimeRegistry entities,
    Vector2 position,
    bool isSlime)
{
  EntityUuid id = EntityUuid.Create();
  EntityRuntimeHandle handle = entities.Allocate(id);
  AiComponent ai = isSlime ? AiComponent.For<SlimeAiBehavior>() : AiComponent.For<GuideAiBehavior>();
  ArchEntity entity = world.Create(
      new EntityIdentityComponent(id),
      new EntityFactionComponent(isSlime ? Faction.Hostile : Faction.Friendly),
      new LifeComponent(isSlime ? 50 : 250, isSlime ? 50 : 250),
      new LocationComponent(position),
      MovementVelocityComponent.Stationary,
      new ColliderComponent(isSlime ? new Vector2(24f, 18f) : new Vector2(18f, 40f)),
      GroundingComponent.Airborne,
      new MovementCapabilityComponent(true, isSlime ? 2.5f : 1f, 12f,
          canJump: isSlime, jumpSpeed: isSlime ? 6f : 0f),
      new GravityCapabilityComponent(true, false),
      new GravityStateComponent(0.4f, 10f, GravityDirection.Normal, false, false),
      new TileCollisionCapabilityComponent(true),
      new LiquidInteractionCapabilityComponent(
          true,
          LiquidContactFlags.Water | LiquidContactFlags.Honey |
              LiquidContactFlags.Lava | LiquidContactFlags.Shimmer),
      MovementIntent.Stop(0),
      new NpcBehaviorStateComponent(),
      ai,
      new AiBehaviorSelectionComponent(
          isSlime ? "Slime" : "Guide",
          isSlime ? "slime.default" : "guide.default"));
  entities.Register(handle, entity);
  return entity;
}

// 回归：向上跳跃撞到头顶实心块时，不得被判为着地。
// 只凭 CollidedY 判定会让实体在空中“站住”，表现为跳跃到一半悬停并可再次起跳。
VerifyCeilingHitIsNotGrounding();

static void VerifyCeilingHitIsNotGrounding()
{
  const int width = 16;
  const int height = 16;
  WorldRuntime ceilingWorld = new(width, height);
  WorldTileState[] tiles = new WorldTileState[width * height];
  Array.Fill(tiles, WorldTileState.Empty);
  // 第 8 行整行实心：实体从下方跳起时会撞到它。
  for (int x = 0; x < width; x++)
  {
    tiles[(8 * width) + x] = new WorldTileState(true, 1, 0, 0, 0);
  }

  Require(ceilingWorld.Load(tiles).Succeeded, "Ceiling fixture world failed to load.");

  SpatialSimulationSystem simulation = new();
  SpatialFrame frame = new(0, ceilingWorld.Revision, ceilingWorld);
  // 起点贴近天花板：碰撞盒顶边距离第 8 行底面只有 4 单位，一帧必撞。
  Vector2 start = new((4 * 16f) + 8f, (9 * 16f) + 4f);
  SpatialEntitySnapshot<int> entity = new(
      0,
      new LocationComponent(start),
      new VelocityComponent(Vector2.Zero),
      new ColliderComponent(new Vector2(16f, 16f)),
      new GroundingComponent(false, false, Vector2.Zero));

  // 向上运动：撞到上方方块时应记为撞天花板，而不是着地。
  MovementIntent upward = new(
      tick: 0,
      desiredVelocity: new Vector2(0f, -2400f),
      jumpRequested: false,
      jumpSpeed: 0f);
  SpatialSimulationResult<int> risen = simulation.Simulate(
      in frame,
      in entity,
      new MovementCapabilityComponent(
          enabled: true,
          maximumSpeed: 3600f,
          acceleration: 144000f,
          canJump: true,
          jumpSpeed: 360f,
          mode: MovementMode.Grounded),
      new GravityCapabilityComponent(true, false),
      new GravityStateComponent(0f, 600f, GravityDirection.Normal, false, false),
      new TileCollisionCapabilityComponent(true),
      new LiquidInteractionCapabilityComponent(false, LiquidContactFlags.None),
      in upward,
      1f / 60f);

  Require(!risen.Commit.Grounding.IsGrounded,
      "A ceiling hit while moving up was incorrectly treated as grounded.");
}
static void VerifyNearestEntityQuery()
{
  using ArchWorld world = ArchWorld.Create();
  QueryDescription candidates = new QueryDescription().WithAll<
      EntityFactionComponent,
      LifeComponent,
      LocationComponent>();
  Vector2 origin = new(0f, 0f);
  ArchEntity near = world.Create(
      new EntityFactionComponent(Faction.Hostile),
      new LifeComponent(10, 10),
      new LocationComponent(new Vector2(30f, 40f)));
  world.Create(
      new EntityFactionComponent(Faction.Hostile),
      new LifeComponent(10, 10),
      new LocationComponent(new Vector2(300f, 400f)));
  ArchEntity dead = world.Create(
      new EntityFactionComponent(Faction.Hostile),
      new LifeComponent(0, 10),
      new LocationComponent(new Vector2(10f, 10f)));

  Require(
      NearestEntityQuery.TryFind(
          world,
          in candidates,
          origin,
          0f,
          (ArchEntity entity, in EntityFactionComponent faction, in LifeComponent life) => life.IsAlive,
          out NearestEntityCandidate nearest),
      "The shared nearest-entity query found no candidate inside an unlimited range.");
  Require(nearest.Entity == near, "The shared nearest-entity query did not pick the nearest candidate.");
  Require(
      nearest.DistanceSquared == 2_500f,
      $"The shared nearest-entity query reported {nearest.DistanceSquared} instead of 2500.");
  Require(
      nearest.Position == new Vector2(30f, 40f),
      "The shared nearest-entity query reported a position that is not the candidate's.");

  // 半径是严格上限：最近的存活候选在 50 像素外，40 像素的半径必须一个都选不出来。
  Require(
      !NearestEntityQuery.TryFind(
          world,
          in candidates,
          origin,
          40f,
          (ArchEntity entity, in EntityFactionComponent faction, in LifeComponent life) => life.IsAlive,
          out _),
      "The shared nearest-entity query accepted a candidate outside the radius.");

  // 判据由调用方负责：排除存活判据后，死亡且最近的那个会成为选中项。
  Require(
      NearestEntityQuery.TryFind(
          world,
          in candidates,
          origin,
          0f,
          (ArchEntity entity, in EntityFactionComponent faction, in LifeComponent life) => true,
          out NearestEntityCandidate unfiltered) &&
      unfiltered.Entity == dead,
      "The shared nearest-entity query ignored the candidate predicate's absence of filtering.");

  // 全体被拒时返回 false，而不是给出默认候选。
  Require(
      !NearestEntityQuery.TryFind(
          world,
          in candidates,
          origin,
          0f,
          (ArchEntity entity, in EntityFactionComponent faction, in LifeComponent life) => false,
          out _),
      "The shared nearest-entity query reported a hit although the predicate rejected every candidate.");
}

static void Require(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

static void VerifyNpcNetworkProjection()
{
  using ArchWorld world = ArchWorld.Create();
  SessionEpoch epoch = SessionEpoch.Create(1);
  CommunicationSessionComponent session = new(epoch);
  session.TryAdvance(CommunicationSessionStage.Active);
  ArchEntity sessionEntity = world.Create(
      session,
      new CommunicationOutboundMessageBufferComponent());
  Vector2 position = new(640f, 320f);
  world.Create(
      new NpcNetworkIdentityComponent(slot: 3, generation: 1, netId: 22),
      new LifeComponent(250, 250),
      new LocationComponent(position),
      MovementVelocityComponent.Stationary,
      new ColliderComponent(new Vector2(18f, 40f)),
      new NpcBehaviorStateComponent(
          phase: NpcBehaviorPhase.Wander,
          stateSlot0: -1f,
          stateSlot1: 2f),
      GroundingComponent.Airborne);

  NpcNetworkProjectionSystem projection = new(world);
  projection.Update(new EcsServerTickContext(world, 0, 1f / 60f));

  CommunicationOutboundMessageBufferComponent outbound =
      world.Get<CommunicationOutboundMessageBufferComponent>(sessionEntity);
  Require(outbound.TryDequeue(out OutboundPacketIntent? intent) && intent is not null,
      "NPC network projection did not enqueue a packet.");
  ReadOnlySpan<byte> payload = intent!.Payload.Span;
  Require(payload.Length == 27, "NPC packet has an unexpected payload length.");
  Require(BinaryPrimitives.ReadSingleLittleEndian(payload[2..6]) == position.X,
      "NPC packet changed the ECS x coordinate.");
  Require(BinaryPrimitives.ReadSingleLittleEndian(payload[6..10]) == position.Y,
      "NPC packet changed the ECS y coordinate.");
  byte flags1 = payload[20];
  Require((flags1 & 0x3c) == 0, "NPC packet leaked ECS behavior state into native AI slots.");
  Require((payload[21] & 0x08) != 0, "NPC packet did not mark the initial slot snapshot.");
  Require((payload[21] & 0x10) == 0,
      "NPC packet incorrectly marked the native shimmer flag.");
  Require((payload[21] & 0x20) != 0,
      "NPC packet did not mark the client-side ECS-authoritative simulation.");
}

/// <summary>无行为探针：只给调度器一个可注册的系统实例。</summary>
sealed class ProbeSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  public ProbeSystem(ArchWorld world) : base(world)
  {
  }

  public override void Update(in EcsServerTickContext context)
  {
  }
}

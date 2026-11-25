# Emission 发射接缝设计

- 日期：2026-11-25
- 状态：proposed（设计已定稿，未实施）
- 适用项目：`src/NSSLC/Terraria.Components.csproj`（`Terraria.Share.Emission`、`Terraria.Unshared.Projectiles`）、`src/NSSLC.Server`
- 上游约束：[ECS 系统输入边界约束](../../Context/项目约束/ECS系统输入边界约束.md)、
  [ECS 文件组织设计约束](../../Context/架构约束/ECS文件组织设计约束.md)、
  [组件命名设计约束](../../Context/架构约束/组件命名设计约束.md)、
  [构建与验证约束](../../Context/项目约束/构建与验证约束.md)
- 相关文档：[Projectile 领域 README](../../src/NSSLC/Projectile/README.md)、
  [投射物 P0–P4 CR](../cr/CR-2026-11-24-projectile-p0-p1-content-and-shoot-seam.md)、
  [Projectile 链路修复执行计划](../plans/2026-11-25-projectile-chain-repair-execution-plan.md)

## 1. 问题陈述

投射物链路已接通，但**发射侧只有一条路，且初始运动有两个来源**。

### 1.1 生产者只有一个

| 证据 | 位置 |
| --- | --- |
| 全仓唯一的生成请求生产者 | `Projectile/ProjectileItemUseSpawnSystem.cs:109` |
| `NSSLC/Npc` 下 grep `Projectile` | 零命中 |
| 入射缝只认物品使用的 `ItemShootProfile` | `Share/ItemUse/Content/ItemShootProfile.cs` |

参考端 `Projectile.NewProjectile` 有 436 处调用（`Player.cs` 197、`NPC.cs` 187、
`WorldGen` 30、`Wiring` 9、`Mount` 6）。NLTX 现状把「玩家物品使用」这一条接上了，
NPC 射击、套装效果、鞭子、召唤物、链式弹、图格与电路全部没有接缝。

### 1.2 几何装配散落在生成请求里

`ProjectileSpawnRequest` 携带 `ColliderComponent` 与 `EntitySizeComponent`，由
`ProjectileItemUseSpawnSystem` 从 `ProjectileBasicsSection` 的标量构造后传入。三条证据说明这是错位的：

- `ColliderComponent` 全仓 25 处引用中，物理系统（`EntityCollisionSystem`、
  `SpatialSimulationCoordinatorSystem`、`PhysicsContactCaptureSystem`、`SpatialEntitySnapshot`）
  **只读不写**——物理是碰撞体的消费者，不是生产者；
- NPC 与玩家的碰撞体写在**实体定义**里（`NpcEntityDefinition.cs` 的 `Add(() => new ColliderComponent(...))`），
  只有投射物写进了请求，等于三种实体三种写法；
- `EntitySizeComponent` 全仓仅 5 处引用，其中 2 处是自身定义，剩余 3 处全在投射物生成链上，
  **物理系统不读它**——它是只为投射物存在、且无人消费的字段。

### 1.3 初始运动有两个来源

生成时 `ProjectileLifecycleSystem.TrySpawn` 直接写 `MovementVelocityComponent`（出生速度）；
同时 `ProjectileMovementIntentSystem` 每帧投 `MovementIntent.Stop(tick)`，而 `ProjectileAiAdapter`
每帧投 `MovementIntent(DesiredVelocity)`。同一 tick 里存在两个意图生产者，谁后入队谁覆盖。

`ProjectileMovementIntentSystem` 的自身注释承认了这种临时性：

> 续的是 `MovementIntent.Stop`：投射物不追求任何目标速度，配合生成时设定的
> `acceleration = 0` 与 `Mode = Flight`，速度原样保留，表现为匀速直线运动。

即出生速度能存活，靠的是「加速度为 0 使意图无效」这个巧合，而不是设计。

### 1.4 AI 存在空窗帧

`ProjectileAiAdapter` 把 `AiComponent` 的挂载推迟到查询之后（`_pendingAttachments`，
`ProjectileAiAdapter.cs:52`）。实体存在的第一个 tick 必然走「补挂」分支并 `return`，
该帧不产出任何 AI 意图。这与「轨迹完全由 AI 控制」不相容。

## 2. 决策记录

以下决策已由用户确认，是本设计的依据，不再作为待议项：

| # | 决策 |
| --- | --- |
| D1 | Emission 作为独立系统放在 `Share/` 下 |
| D2 | Emission 只处理生成：入参为发射者、内容 id、生成位置；**不传速度，不传碰撞体与尺寸** |
| D3 | 出生速度由 AI 系统负责计算，Emission 不提供 |
| D4 | AI **不得靠补挂**：`AiComponent` 随实体生成即装配，AI 绑定在实体存在的第一个 tick 完成 |
| D5 | 删除 `ProjectileMovementIntentSystem` |
| D6 | 碰撞体与尺寸归**内容定义**（实体组合），物理系统只消费 |

D6 是「碰撞体交给物理系统」的实现口径：物理系统消费碰撞体，碰撞体由实体定义提供，
生成方不负责几何。这与 NPC、玩家的既有写法一致，也是唯一能删除 `EntitySizeComponent` 的方向。

## 3. 模型

### 3.1 EmissionRequest

落在 `Terraria.Share.Emission`，与任何非共享领域类型无关：

```csharp
namespace Terraria.Share.Emission;

/// <summary>一次发射：在哪、朝哪射什么。不含速度、几何与生命周期。</summary>
public readonly record struct EmissionRequest(
    EntityRuntimeHandle Emitter,
    string ContentId,
    Vector2 Origin);
```

三个字段，构造期校验：

- `Emitter` 必须 `IsValid`；
- `ContentId` 非空白；
- `Origin` 两个分量必须有限。

`Emitter` 不是「额外信息」而是一次发射的最小身份：它同时决定三件下游必需的事——
生成请求的 `Owner`、实体阵营的推导来源（读发射者的 `EntityFactionComponent`）、
以及 AI 行为的持有者引用。去掉它，drain 侧无法确定归属。

`Origin` 取**绝对世界坐标**。发射方的局部偏移（如 `ItemShootProfile.Offset`）由调用方
在入队前加上，Emission 不读发射者位置——那会引入一次跨域状态读取，而 Emission 的输入
必须是自足的。

### 3.2 为什么不是 `ProjectileId`

`Share/` 对 `Unshared` 的引用当前只有 9 处，且无一处指向 `Projectiles`。
`ItemShootProfile` 已确立先例：

> 投射物 id 以内容 id 文本表达，而不是 `Terraria.Unshared.Projectiles.ProjectileId`：
> 共享层不得依赖非共享领域类型，解析与存在性校验由投射物域在生成接缝处完成。

`EmissionRequest.ContentId` 沿用同一口径：字符串 id，解析与存在性校验在 drain 侧完成。
依赖方向保持 `Unshared.Projectiles` → `Share.Emission`，不产生反向依赖。

### 3.3 队列

与现有 `ProjectileSpawnQueueComponent` 同形：挂在专职世界级实体上的单例组件，
`BeginTick(tick)` / `Drain(tick)` 严格同 tick 语义，跨 tick 未排空的请求直接丢弃。

严格同 tick 是唯一可行语义：请求由 drain 转成生成请求后进入 `ProjectileSpawnQueueComponent`，
而后者本身就是 tick 号排空的。两级队列若采用不同语义，「滞留一次」会在第二级被静默吞掉。

**后果是硬约束**：任何发射者必须与 drain 处于同一相位（Preparation）。图格与电路在
Simulation 产生发射事实，接入时必须先经一个事实队列跨到 Preparation，不能直接入队。
这条与 CR-2026-11-24 §3 记录的真实事故同源：

> 若接缝放 Simulation，请求会带上 `tick N` 而排空方在 `tick N+1` 的 Preparation 调 `Drain`
> ——永远取不到，还会在下一次 `Enqueue` 时被 `BeginTick` 清掉。

### 3.4 Source 归因

现有 `ProjectileSourceMetadataComponent` 是 `(string SourceKind, string SourceId, long SourceRevision)`，
由 `ProjectileItemUseSpawnSystem` 唯一生产。按 D2，Emission 不承载归因字段。

归因在 drain 侧由已有信息重建：`Emitter` 给出「谁发」，`ContentId` 给出「用什么发」。
本设计**不引入 `EmissionSourceKind` 枚举**——那是另一种「传入额外值」，与 D2 冲突。
归因精度不足的部分列入 §8 未闭合，待 NPC 接入后按实际需求再定，不在本轮预支。

## 4. 数据流

```
发射者（Preparation，与 drain 同相位）
  ├─ Player 物品使用   → EmissionQueue.Enqueue(Emitter, shoot.ProjectileId, pos + shoot.Offset)
  ├─ NPC AI            → EmissionQueue.Enqueue(Emitter, attack.projectileId, origin)
  ├─ Projectile 链式   → EmissionQueue.Enqueue(Emitter=父弹, childId, origin)
  └─ 套装 / 鞭 / 召唤  → EmissionQueue.Enqueue(Emitter, contentId, origin)
        ↓
EmissionDrainSystem（Preparation，Unshared.Projectiles）
  ├─ 解析 ContentId → ProjectileCatalog.TryGet
  ├─ 解析 Emitter   → 生成请求 Owner
  ├─ 推导 Faction   → 读 Emitter 的 EntityFactionComponent，缺失按 Neutral
  └─ Enqueue → ProjectileSpawnQueueComponent
        ↓
ProjectileSpawnRequestSystem（Preparation）
        ↓
ProjectileLifecycleSystem.TrySpawn
  ├─ 实体组合来自 ProjectileRecipe.Composition（含 ColliderComponent）
  ├─ AiComponent 随组合装配（D4）
  └─ 不再写 MovementVelocityComponent（D3）
        ↓ 同一 tick 或下一个 Simulation tick
ProjectileAiAdapter → 绑定 → Calculate → MovementIntent → SpatialSimulation
```

`EmissionDrainSystem` 的失败分支不抛异常（tick 管线里抛异常会打断整帧），
逐条记入 `Results`，沿用 `ProjectileItemUseSpawnStatus` 的既有形态。

## 5. AI 接管时序

D4 要求「实体存在的第一个 tick 就完成绑定并算出速度」。落地要点：

### 5.1 AiComponent 由内容定义装配

`ProjectileBasicsSection` 已有 `BehaviorId` / `ProfileId`，但不含 `behaviorType`——
该字段在 `ProjectileBehaviorProfile` 上。因此：

- `ProjectileComposition` 需要 `ProjectileBehaviorCatalog` 解析 `behaviorType`；
- `ProjectileLifecycleSystem` 在 spawn 时把 `AiComponent.For(behaviorType)` 加入实体，
  与既有 `EntityFactionComponent`、`LocationComponent` 等实例级输入同处一列。

当前代码刻意不在内容定义里放 AI 类型，注释理由是「投射物域不希望为此把 AI 插件类型塞进内容定义」。
D4 推翻了这一取舍：不塞类型就无法在生成期装配，必然留空窗帧。

### 5.2 绑定可以在查询内完成

`AiEntitySystem.Attach` 只是操作内部 `Dictionary<TEntity, AiBinding>`，**不是 ECS 结构变更**。
因此 `ProjectileAiAdapter` 可以在查询回调里：

```csharp
if (!_ai.IsAttached(entity))
{
    _ai.Attach(entity, aiComponent);   // 纯字典操作，查询内安全
}
// 同一帧继续 Calculate
```

现有代码把 attach 推迟到查询之后，是为了规避**结构变更**（`World.Add<AiComponent>`）。
D4 把组件装配前移到生成期后，这个约束不再成立，绑定可以在首次看到的同一帧完成。

### 5.3 每帧必须产出速度意图

删除 `ProjectileMovementIntentSystem`（D5）后，弹体不再有「Stop 续命」兜底。
`SpatialSimulationSystem` 的第一道校验是 `intent.Tick != frame.Tick` 即拒绝，
因此**AI 行为必须在每帧产出至少一条速度意图**，否则该帧速度意图过期、弹体不动。

`ProjectileAiAdapter` 现有分支 `if (intents.Count == 0) return;` 会让这种情形静默发生。改为：

- 无任何速度意图时，兜底续一次**当前速度**（读 `VelocityComponent`，非 `Stop`）；
- 这是兜底而非设计，AI 行为缺失输出属于内容缺陷，应由验证项目断言暴露。

## 6. 文件清单

### 6.1 新增

| 路径 | 职责 |
| --- | --- |
| `Share/Emission/Command/EmissionRequest.cs` | 三字段发射意图 |
| `Share/Emission/Component/EmissionQueueComponent.cs` | tick 级队列，与 `ProjectileSpawnQueueComponent` 同形 |
| `Share/Emission/Command/EmissionQueue.cs` | 单例定位 + 入队入口，与 `ProjectileSpawnQueue` 同形 |
| `Projectile/EmissionDrainSystem.cs` | 排空意图 → 生成请求 |
| `Projectile/System/EmissionDrainResult.cs` / `EmissionDrainStatus.cs` | 逐条路由结果 |

### 6.2 改写

| 路径 | 变更 |
| --- | --- |
| `Projectile/Command/ProjectileSpawnRequest.cs` | 删除 `Collider` / `Size` / `Velocity` |
| `Projectile/ProjectileItemUseSpawnSystem.cs` | 缩为「查 shoot profile → 入 Emission 队列」，几何与目录查询移出 |
| `Projectile/ProjectileLifecycleSystem.cs` | 新增 `ProjectileBehaviorCatalog` 依赖；spawn 时装配 `AiComponent`；不再写速度 |
| `Projectile/ProjectileAiAdapter.cs` | 查询内绑定（5.2）；无速度意图时兜底当前速度（5.3） |
| `Projectile/System/ProjectileBasicsSection.cs` | `Contribute` 补 `ColliderComponent`（D6） |
| `Projectile/System/ProjectileComposition.cs` | 传入行为目录以解析 `behaviorType` |
| `Share/Entity/Component/EntitySizeComponent.cs` | 删除（物理不读，删后无消费者） |
| `NSSLC.Server/Registration/CombatSystemRegistration.cs` | 注册 drain；移除 `ProjectileMovementIntentSystem` |
| `NSSLC.Server/Registration/EcsServerRegistrationKeys.cs` | 新增 `CombatProjectileEmissionDrain`；删除 `CombatProjectileMovementIntent` |

### 6.3 删除

| 路径 | 依据 |
| --- | --- |
| `Projectile/ProjectileMovementIntentSystem.cs` | D5 |

### 6.4 验证项目

`Test/Terraria.Components/Terraria.Projectile/Program.cs` 有 6 处引用 `ProjectileMovementIntentSystem`
（第 315、403、428、474、480、921 行）。按 D5，这些用例改为驱动 AI 适配器产出意图，
断言口径不变：生成 → 下一 tick 位移由空间管线完成。

## 7. 迁移与分批

| 批次 | 内容 | 独立可验收 |
| --- | --- | --- |
| P0 | D6：碰撞体进内容组合，删除 `EntitySizeComponent`，`ProjectileSpawnRequest` 去几何 | 是（不依赖 Emission） |
| P1 | `Share/Emission` 三件套 + `EmissionDrainSystem` | 是（可先用现有物品使用驱动） |
| P2 | `ProjectileItemUseSpawnSystem` 改走 Emission | 是（行为等价） |
| P3 | D3/D4：速度移出生成、`AiComponent` 生成期装配、查询内绑定 | 是 |
| P4 | D5：删除 `ProjectileMovementIntentSystem`，适配器补兜底 | 是（P3 之后才安全） |
| P5 | NPC 发射接入 | 是（Emission 已定型） |

P0 与 P1 可在 [链路修复执行计划](../plans/2026-11-25-projectile-chain-repair-execution-plan.md)
之前或之后独立落地；P3–P4 会与该计划的缺陷修复路径相交，**须与计划所有者对齐顺序**。
该计划当前为 `proposed`，缺陷文件未删即未闭合。

## 8. 未闭合

1. **初始方向来源**。D3 把出生速度交给 AI，但 AI 计算初始速度需要方向输入，而当前
   `ItemShootProfile.Velocity` 是内容写死的常数（其注释已记录「瞄准方向尚未建模，
   协议里没有可用的朝向状态」）。若 AI 首帧算不出方向，弹体将有一帧静止。
   需在 NPC 接入前确定：方向由内容声明、由发射者朝向、还是等瞄准建模。
2. **Source 归因精度**。§3.4 选择丢弃 `SourceId` / `SourceRevision`。参考端依赖类型化 source
   做继承（父弹的 tag 伤害、NPC 的 banner、召唤物来源）。这些能力在 NPC 或鞭子接入时会重新需要，
   届时须在不违反 D2 的前提下补，可能的形式是 drain 侧按内容族推导而非请求字段。
3. **相位约束**。§3.3 的严格同 tick 语义使图格、电路在 Simulation 的发射事实无法直连。
   接入前需要一个跨相位事实队列，形态未定。
4. **容量**。参考端 1000 槽满后覆盖最老弹体；NLTX 的 Arch 无上限。若后续需要网络投影与
   快照回放，需要有明确的容量策略与断言。当前按「不限」处理，不继承静默覆盖。
5. **确定性随机**。参考端大量 per-type 随机初始化（`ai[0] = Main.rand.Next(-100,101)*0.0005f`）。
   D2 的极简请求结构没有随机字段；若需回放一致，随机须在入队前摇完，或由 AI 行为内部
   用确定性 PRNG 承担。本设计不预支，列为未闭合。

# ECS 系统输入边界审计（2026-11-01）

范围：`src/NSSLC/Components/` 下 451 个 `.cs`，其中 24 个调度系统、6 个静态规则类、17 个服务/规则对象。
目的：按 [ECS 系统输入边界约束](../../../Context/项目约束/ECS系统输入边界约束.md) 检查现状，列出存量迁移债。

## 1. 结论摘要

| 项 | 结果 |
| --- | --- |
| 调度系统（`BaseSystem<ArchWorld, EcsServerTickContext>`） | 24 |
| 构造参数含可变队列 | 10 |
| 其中跨域注入 | 5 |
| 其中同域注入（仅违反规则 3） | 5 |
| 被 2 个以上系统共享的可变对象 | 3 个实例 |
| 跨域直接改状态 | 2 处 + 2 类服务入口 |
| 以组件承载请求（合规） | 3 个系统 |
| 域内推进、无需请求层（符合规则 2） | 9 个系统 |

## 2. 基线：tick 上下文没有命令通道

```csharp
public readonly record struct EcsServerTickContext(ArchWorld World, long Tick, float DeltaTime)
```

三个字段都是只读输入，没有任何命令入口。因此"跨域输入"在现状下只有两条路：挂在实体上的
组件，或宿主在构造时注入的对象。本约束要收敛的正是后者。

## 3. 规则 3 违规：宿主注入可变队列（10 个系统）

| 系统 | 所属域 | 注入的可变队列 | 队列所属域 | 性质 |
| --- | --- | --- | --- | --- |
| `CombatAttackRequestSystem` | Combat | `CombatAttackIntentBuffer`、`CombatDamageRequestBuffer` | Combat | 同域注入 |
| `CombatDamageRequestSystem` | Combat | `CombatDamageRequestBuffer` | Combat | 同域注入 |
| `PhysicsContactCaptureSystem` | Physics | `PhysicsContactBuffer` | Physics | 同域注入 |
| `ProjectileLifetimeTickSystem` | Projectile | `ProjectileTerminationBuffer` | Projectile | 同域注入 |
| `ProjectileTerminationCommitSystem` | Projectile | `ProjectileTerminationBuffer` | Projectile | 同域注入 |
| `NpcAiIntentAdapter` | Npc | `CombatAttackIntentBuffer` | Combat | **跨域** |
| `NpcCombatResultSystem` | Npc | `CombatDamageRequestBuffer` | Combat | **跨域** |
| `PlayerCombatResultSystem` | Player | `CombatDamageRequestBuffer` | Combat | **跨域** |
| `ProjectileDamageRequestSystem` | Projectile | `CombatAttackIntentBuffer`、`PhysicsContactBuffer` | Combat、Physics | **跨域** |
| `ProjectileDamageResultSystem` | Projectile | `CombatDamageRequestBuffer` | Combat | **跨域** |

## 4. 共享可变对象

`src/NSSLC.Server/Program.cs:86-88` 一次性创建三个实例，再经 `BuildRegistrations` 分发：

| 对象 | 使用方数 | 使用方 |
| --- | --- | --- |
| `CombatAttackIntentBuffer` | 3 | `CombatAttackRequestSystem`、`NpcAiIntentAdapter`、`ProjectileDamageRequestSystem` |
| `CombatDamageRequestBuffer` | 5 | `CombatAttackRequestSystem`、`CombatDamageRequestSystem`、`NpcCombatResultSystem`、`PlayerCombatResultSystem`、`ProjectileDamageResultSystem` |
| `PhysicsContactBuffer` | 2 | `PhysicsContactCaptureSystem`、`ProjectileDamageRequestSystem` |

`ProjectileTerminationBuffer` 只被 Projectile 域内 2 个系统使用，同样违反"同一可变对象不得共享"。

## 5. 规则 1 违规：跨域直接改状态

**5.1 Inventory → Attributes**

`Share/Inventory/InventoryAttributeContributionSystem.cs:125` 与 `:130` 直接写属性组件：

```csharp
attributes.RemoveBySourceCategory(category);
...
attributes.AddModifier(modifier);
```

`AttributeSetComponent` 归 `Terraria.Attributes` 所有，却由 `Terraria.Inventory` 的系统直接改写，
既不经过请求组件，也不经过 `AttributeTickSystem`。

**严重度更正**：该类型全仓只有两处命中——自身声明（`:23`）与构造函数（`:36`）——**零构造点**，
且不在 `Program.cs` 的注册表内。所以这是**形态违规但从未激活**，与 `PlayerAttackIntentAdapter`
同属"未接线"，不应按活跃违规处理。

**5.2 Communication → World**

`WorldNetworkProjectionSystem`（`Terraria.Communication`）构造注入 `WorldItemStore` 与
`WorldRuntime`，跨域持有可变世界状态并直接读取。

**5.3 Npc → Ai**

`NpcAiIntentAdapter`（`Terraria.Npc`）构造注入 `AiEntitySystem<ArchEntity, NpcAiIntent>` 与
`IAiSnapshotProvider<ArchEntity>`，跨域持有可变 AI 运行时。

**5.4 服务方法直调**

`BuffSystem.TryApply(...)` 与 `InventorySystem` 的转移方法是无调度、无请求组件的入口。
外部想改 Buff 或背包状态，只能拿到引用直接调方法；同一件事在 Combat 走的是请求缓冲。

## 6. 合规样例

| 系统 | 输入形态 | 为什么合规 | 附加问题 |
| --- | --- | --- | --- |
| `WorldInteractionSystem` | `WorldInteractionRequestComponent` | 输入侧组件承载、同域、由系统在 tick 读取 | 构造注入的可变 `IWorldInteractionWorld` 仍违反规则 3，见 §10.3 |
| `SpatialSimulationCoordinatorSystem` | `MovementIntent` | 组件承载意图 | 依赖 `ISpatialWorldQuery` 属真只读端口，合规 |
| `ProjectileTerminationCommitSystem` 的请求侧 | `ProjectileTerminationRequest` | 请求是值类型，问题只在它靠注入队列传递 | — |

符合规则 2（域内推进、无需请求层）的 8 个系统：`AttributeTickSystem`、`BuffTickSystem`、
`GravityTickSystem`、`PlayerResourceSystem`、`PlayerTargetingSystem`、`ProtectionWindowSystem`、
`WorldTickSystem`，以及 `InventoryAttributeContributionSystem` 的读侧。

**已撤销的两条**：

- `PlayerDeathRespawnSystem` 原列于此，实际在 `:47` 用 `life = life.WithCurrent(life.Maximum)`
  改写的 `LifeComponent` 属 `Terraria.Combat`（`LifeComponent.cs:3`），是跨域写，见 §10.4。
- `WorldInteractionSystem` 的"完整合规"结论只对输入侧成立；其构造依赖见 §10.3。同时结果组件
  `WorldInteractionResultComponent` 没有下游消费者，回程只到组件、未投影给客户端，闭环是断的。

## 7. 迁移建议（按收益排序）

1. **把三个共享缓冲改成组件**。一次改动同时消掉 10 个系统的注入违规与 5 个跨域违规，形态可照抄
   唯一已合规的样例 `WorldInteractionRequestComponent`。
2. **`InventoryAttributeContributionSystem` 改为产出请求组件**，由 Attributes 域的系统消费，
   使属性写入重新回到拥有者的系统里。
3. **`WorldNetworkProjectionSystem` 与 `NpcAiIntentAdapter` 的跨域对象降为只读端口**，
   去掉对可变存储的直接持有。
4. **`BuffSystem` / `InventorySystem` 在各自域内补请求组件入口**，让外部驱动与 Combat 一致。

## 8. 本次未做的工作

本审计只做规则落地与检查，**未修改任何生产代码**。上述 4 项迁移需要单独任务，且第 1 项会
同时改动 10 个系统的构造签名与 `Program.cs` 的装配，建议单独一轮并保留行为等价验证。

## 9. 补充扫描：适配器、非调度服务与静态状态

§3 的数只覆盖 `BaseSystem` 派生类。补扫 `*Adapter`、非调度 `*System` 与静态状态后，计数需要修订。

### 9.1 修订后的违规计数

| 类别 | 持有注入的可变队列/存储 | 其中跨域 |
| --- | --- | --- |
| 调度系统（`BaseSystem`） | 10 | 5 |
| 适配器 | 2 | 2 |
| 非调度服务 | 2 | 0 |
| **合计** | **14** | **7** |

### 9.2 新发现的适配器

| 类型 | 所属域 | 注入 | 说明 |
| --- | --- | --- | --- |
| `GameCommunicationAdapter` | Communication | `CombatAttackIntentBuffer`（Combat）、`WorldItemStore`（World） | 由 `CommunicationSystemRegistration.cs:34` 构造，在生产路径上 |
| `PlayerAttackIntentAdapter` | Player | `CombatAttackIntentBuffer`（Combat） | **全仓只在定义处出现，从未被构造**，属未接线类型，不构成活跃违规 |

### 9.3 新发现的非调度服务

| 类型 | 实例字段 | 注入 | 性质 |
| --- | --- | --- | --- |
| `ProjectileHitResolutionSystem` | 3 | `ProjectileTerminationBuffer`（可选参数） | 同域注入 |
| `ProjectileLifecycleSystem` | 4 | `ProjectileRuntimeRegistry`、`ProjectileTerminationBuffer`（可选） | 同域注入 |

两者又被 `ProjectileDamageRequestSystem` 等注入到调度系统里，形成"服务持有队列、系统持有服务"的两级传递。

### 9.4 装配职责住进了 Components

`CombatSystemRegistration.Create(...)` 与 `CommunicationSystemRegistration.CreateGame(...)` 都在
`Components` 内，接收 `WorldItemStore`、三个缓冲并直接 `new` 出系统。宿主的装配职责渗进了组件库。

更关键的是 `CombatSystemRegistration.cs:45-49` **显式强制共享**：

```csharp
if (!ReferenceEquals(contacts, physicsContacts.Buffer))
{
  throw new ArgumentException(
      "Physics contact capture and Combat adapters must share one contact buffer.",
      nameof(contactBuffer));
}
```

"多个系统共用同一个可变缓冲"不是疏漏，而是被断言保护的既定设计。本约束与它正面冲突：
迁移时**必须先移除这条断言**，否则改成组件后会立刻抛异常。

### 9.5 静态状态：字段层面干净，但存在进程级可变单例

`Components` 下**可变 `static` 字段 0 个**。扫描命中的 6 处全部是只读表达式属性：
`CommitVersion.Initial`、`VelocityComponent.Stationary`、`SpatialWorldRevision.Initial`、
`GroundingComponent.Airborne`、`WorldTickResult.NotRun`、`WorldTileState.Empty`。

4 个纯静态规则类零实例字段、只有静态方法：`AttributeSystem`（6 个）、`GravityModifierSystem`（5 个）、
`AttributeContributionSystem`（3 个）、`BuffAdmissionSystem`（1 个）。它们符合"规则只依赖显式输入"的形态。

**但"零可变 static 字段"不等于没有全局可变状态**。只读 `static` 属性也能持有一个可变对象：

```csharp
// Communication/ConnectionByteBudget.cs:5
internal static ConnectionByteBudget SharedRaw { get; } =
  new(PacketTcpConnectionOptions.MaximumTransportRawBytes);
```

`SharedRaw` 是进程级单例，内部有 `_usedBytes`、`_changed`、`_gate` 三个可变字段，被
`PacketTcpConnection.cs:243`、`:399` 共享。它不受构造注入约束（不是系统依赖），但确实是"同一可变对象
多方共享"的现存实例，属于本约束管不到、也不该由本约束管的传输层限流设施——记录备查。

### 9.6 形态不一致（非本约束违规，记录备查）

以下类零实例字段，却提供实例方法而非静态方法，与上面 4 个静态规则类形态不一致：

- `DamageAdmissionSystem`、`DamageMitigationSystem`、`DamageRelationSystem`（2 静态 + 1 实例混用）
- `EntityCollisionSystem`、`ProjectileSimulationSystem`

它们被当作无状态计算注入到系统里，读者无法从类型形状判断其是否持有状态。

**修正**：`AiSystem` 曾被列在此处，属误判。它实际有 1 个实例字段 `_factories`
（`AiSystem.cs:8`）和 4 个实例方法，是一个**可写的行为注册表**，不是无状态计算类。
`AiEntitySystem` 实际有 3 个字段（`_aiSystem` / `_bindings` / `_disposed`），不是 2 个。
原统计错误源于扫描正则不接受带空格的嵌套泛型（`Dictionary<Type, Func<AiBehavior<TIntent>>>`）。

### 9.7 超出 Components 范围但值得记录

`WorldItemStore` 在 `src/MinimalImplementation/Host/` 下有 19 个持有者
（`ServerComposition`、`Version4InboundRouter.*`、`WorldItem*Coordinator` 等）。
宿主层共享同一个可变存储，超出本约束的 `Components` 范围，但与规则 3 的精神冲突，记录备查。

### 9.8 对 §7 迁移建议的修订

1. 迁移第 1 项前，先处理 `CombatSystemRegistration.cs:45-49` 的共享断言。
2. 把两个 `*Registration.Create(...)` 移出 `Components`（或降为纯数据），使组件库不再依赖宿主级存储。
3. `PlayerAttackIntentAdapter` 应并入"未接线类型"清单，与 `InventorySystem` 一并决策，而不是按违规处理。

## 10. 九域并行审计

2026-11-01 按 `Terraria.<Name>` 命名空间分九个域并行复核，覆盖 `src/NSSLC/Components/` 全部 451 个文件。
判定标准取自 `ecs-system` → `command-splitting`：按效果分类而非按名字，证据标 `confirmed` / `partial` / `unknown`。

### 10.1 覆盖与证据强度

| 域 | 文件 | 调度系统 | 静态规则类 | 服务对象 | 证据强度 |
| --- | --- | --- | --- | --- | --- |
| `Terraria.Communication` | 39 | 4 | 2 | 10 | confirmed |
| `Terraria.Diagnostics` | 6 | 0 | 0 | 2 | confirmed |
| `Terraria.Npc` | 13 | 2 | 0 | 4 | confirmed |
| `Terraria.Player` | 12 | 4 | 0 | 1 | confirmed |
| `Terraria.Projectile` | 42 | 4 | 2 | 4 | confirmed |
| `Terraria.World` | 31 | 2 | 8 | 4 | confirmed |
| `Terraria.Ai` | 154 | 0 | 4 | 150 | **扫描级**（18/154 逐行） |
| `Terraria.Attributes` | 34 | 2 | 8 | 2 | confirmed |
| `Terraria.Combat` | 28 | 3 | 3 | 5 | confirmed |
| `Terraria.Entity` + `Inventory` + `Item` | 40 | 1 | 1 | 7 | confirmed |
| `Terraria.Physics` + `Movement` | 52 | 3 | 1 | 1 | confirmed |

调度系统合计报 25，与 §1 的 24 存在 1 个差异：Entity/Inventory/Item 域报 1 个调度系统，但该域
唯一的候选 `InventoryAttributeContributionSystem` 并不派生 `BaseSystem` 且零构造。**差异未消解，记 `unknown`**，
不强行凑数。

`Terraria.Ai` 是唯一非逐行覆盖的域：154 个文件全部做了结构与依赖扫描，18 个逐行通读，136 个
（GOAP 搜索、HTN 分解实现主体）只到扫描级。其"无违规"结论强度相应降低。

### 10.2 各域结论

| 域 | 规则 3 违规 | 规则 1 违规 | 备注 |
| --- | --- | --- | --- |
| Communication | 3 | 1（4 类写入） | 最严重，见 §10.3 |
| Npc | 2 | 1 | AI 注册表 + Combat 队列 |
| Player | 1 | 1 | 写 `LifeComponent` |
| Projectile | 4 | 2 | 两级传递：服务持队列 |
| World | 2 | 1 | `WorldItemStore` 外溢 |
| Ai | 0 | 0 | 无 `ArchWorld`、无跨域组件访问 |
| Attributes | 1 | 0 | `BuffDefinitionRegistry` 未 Sealed |
| Combat | 2 | 1 | 写 Attributes 的 Shield |
| Entity + Inventory + Item | 0 | 1（未激活） | Item 整链未接线 |
| Physics + Movement | 1 | 1 | 排序靠数组顺序 |

### 10.2.1 整改后状态（2026-11-02）

上表是**整改前**的账面。按执行计划
`docs/plans/2026-11-02-ecs-system-input-boundary-execution-plan.md` 逐阶段整改后，当前实测状态：

| 检查项 | 整改前 | 现在 | 证据 |
| --- | --- | --- | --- |
| 宿主注入的 `*Buffer` 类 | 4 | **0** | 四个类型全仓库已无出现 |
| 系统构造里的可变存储静默默认 | 3 | **0** | 剩余 `?? new` 全是不可变 options 或方法内局部集合 |
| 跨域持有具体 `WorldRuntime` | 1（投影系统） | **0** | 改为 `IWorldProjectionSource` |
| 跨域持有 `WorldItemStore` | 2 | **0** | 改为 `IWorldItemQuery` + 请求队列 |
| 跨域直写组件 | 9 类 | **0** | 全部改为请求组件或域内命令 |
| 装配入口位于 `Components` | 2 | **0** | 移入 `NSSLC.Server/Registration/` |

**整改中额外发现并处理的三处**（原审计未列）：

1. `ProjectileLifecycleSystem` 与 `ProjectileHitResolutionSystem` 各自 `new ProjectileTerminationBuffer()`，
   靠注册期 `AttachTerminationBuffer` 覆写保证是同一实例——实例是否统一取决于一行注册顺序。
2. `GameCommunicationAdapter` 直接写 `MovementIntent`（`Terraria.Movement` 的组件），
   且 `NpcAiIntentAdapter` 也写同一个组件。改为 Movement 域统一持有队列、两个生产者都提交意图。
3. `WorldTickSystem.WorldRuntime` 是零使用者的公开属性，任何持有该系统的域都能借它取回具体
   `WorldRuntime` 并调 `TryBreakTile`。已删除。

**未消除的一处（已知且有理由）**：`GameCommunicationAdapter` 与 `NpcAiIntentAdapter` 仍按句柄解析
实体后提交意图；若句柄解析失败，意图被静默丢弃（与既有 `CombatAttackIntentQueue` 行为一致）。
这是既有语义，未在本次改动中变更。

### 10.3 最高优先级：Communication 域

`GameCommunicationAdapter` 一个类同时越过两条规则的边界：

- 构造注入 Combat 的 `CombatAttackIntentBuffer`（`:46`）与 World 的 `WorldItemStore`（`:48`）；
- `:56` `_worldItems = worldItems ?? new WorldItemStore();` —— 缺省时**自建第二份世界存储**，
  与 `CombatSystemRegistration.cs:42-43` 的 `requestBuffer ?? new` 是同一类反模式；
- 公开 `Handle` 路径直接改他域状态：`:251` 向 Combat 队列 `Enqueue`、`:283` 写
  `PlayerInventoryComponent.Set`、`:342/:353/:377/:398` 调 `WorldItemStore.TryCreate/TryUpdate/
  TryRelease/TryRetire`、`:163` 写 `LocationComponent.Coordinates`。

`WorldNetworkProjectionSystem` 同样持有 `WorldRuntime` + `WorldItemStore`（`:30-31`），并在 `Update`
里 `:47` `DrainPendingMutations()`、`:48` `DrainMutations()`、`:90` `TryCreate(...)` —— 直接消费并写入
World 域状态。

`CommunicationSessionTickSystem` 的形参只有 `ICommunicationPacketHandler`（只读接口），但**唯一实配实现
就是上面这个适配器**，所以规则 3 经接口仍然成立：接口只读，实现可变。

### 10.4 新增的跨域写（本轮发现，此前审计未列）

| 方向 | 证据 | 说明 |
| --- | --- | --- |
| Combat → Attributes | `DamageCommitSystem.cs:80` → `AttributeSystem.cs:41` `SetBaseValue` | 直接消耗目标 `AttributeSetComponent.Shield`，与约束点名禁止的 `AddModifier` 同形 |
| Player → Combat | `PlayerDeathRespawnSystem.cs:47` | `life = life.WithCurrent(life.Maximum)` 改 `LifeComponent`，而 `DamageCommitSystem.cs:11` 自称其唯一常规写者 |

### 10.5 `WorldRuntime` 的持有方式必须逐个区分

同一 `WorldRuntime` 实例被 5 个系统持有，按"先问归属、再问是否外流"的顺序判定后，**只有 1 个跨域外流**：

| 系统 | 收到的是 | 判定 |
| --- | --- | --- |
| `WorldTickSystem` | 具体 `WorldRuntime` | **合规**：World 域自己的推进者，归规则 2 |
| `WorldInteractionSystem` | `IWorldInteractionWorld`（含 `TryBreakTile`/`TryPlaceTile`） | **合规**：同样在 `Terraria.World` 内，是域内写入执行者，归规则 2 |
| `WorldNetworkProjectionSystem` | 具体 `WorldRuntime` | 违规（Communication 域外流）；**已于 Phase 5 改为 `IWorldProjectionSource`** |
| `PhysicsContactCaptureSystem` | `ISpatialWorldQuery` | **合规**：三个成员全为读 |
| `SpatialSimulationCoordinatorSystem` | `ISpatialWorldQuery` | **合规** |

`WorldInteractionSystem` 一行是本审计的**第三处自我修正**：初版把它判为规则 3 违规，理由是接口暴露了写入方法。
补上"适用范围：共享或跨域"条款后结论不同——它所在 namespace 是 `Terraria.World`，与 `WorldRuntime`
同属一个域，且全仓库只有它一个持有者，没有任何跨域对象拿到这个可写接口。接口宽不宽是域内封装质量问题，
不是本约束管的对象。

这说明"只读端口"这条路在本仓库已经跑通，是可以推广的形态，而不是纸面条款。

### 10.6 未接线清单（汇总）

| 类型 | 零引用证据 |
| --- | --- |
| `InventorySystem` | 零 `new`，6 个公开方法无外部调用，不实现 `ISystem` |
| `InventoryComponent` | 零 `new` |
| `ItemDefinitionRegistry` / `ItemDefinition` | 零构造点 |
| `ItemStackComponent` | 零 `new`，物品实体表示从未产生 |
| `InventoryAttributeContributionSystem` | 零构造，不进注册表 |
| `PlayerAttackIntentAdapter` | 仅声明 + 构造定义 |
| `EcsServerCommandBuffer` | `src` 内 0 引用 |
| `BuffSystem.TryApply` | 0 个调用点 |
| `ProjectileLifecycleSystem.TrySpawn` / `TryTerminate` / `TerminateOwnedBy` | 0 个调用点 |
| `Terraria.Combat` 定义链（`EntityDefinitionSystem` 等 7 个类型） | 跨目录零引用，无 `IEntityDefinitionHost` 实现 |

**Item 域除 `ItemId` 枚举外整链不在运行路径上。** 这让 §7 的"Inventory/Item 走 A 还是 B"变成一个更基本
的问题：两条路现在都没人走。

### 10.7 排序与静默丢弃风险（非约束违规，但会掩盖缺陷）

- `CombatAttackIntentBuffer.Drain(tick)`（`:53-58`）tick 不符时返回空列表，跨域投递的意图静默消失。
- `PlayerCombatResultSystem.cs:39` `_ = _results.ConsumeResults(source);` 玩家伤害结果被丢弃。
- `PhysicsContactBuffer.Drain()` 会清空，捕获必须先于消费，仅靠 `CombatSystemRegistration.cs:54,59` 的
  数组顺序保证，无断言。
- `ProjectileTerminationBuffer` 靠 `CombatSystemRegistration.cs:51` 的覆写绑定，缺该行则命中数耗尽的
  终止请求被静默清空。
- `WorldTickSystem.cs:33-36` 失败即 `throw`，异常逃出 `Update` 会中断整个 tick，无降级路径。

### 10.8 修订后的迁移优先级

1. **先处理共享断言与自建实例**：`CombatSystemRegistration.cs:44-50` 的 `ReferenceEquals` 断言、
   `:42-43` 与 `GameCommunicationAdapter.cs:56` 的 `?? new`。
2. **`PhysicsContactBuffer` 与 `CombatAttackIntentBuffer` 改为请求组件**：两者各被多个域共享，
   改一处消掉最多违规。
3. **`WorldRuntime` 对外一律用 `ISpatialWorldQuery` 形态**：已有两个系统证明可行，
   把 `WorldTickSystem`、`WorldNetworkProjectionSystem`、`WorldInteractionSystem` 三处对齐。
4. **补齐 Combat → Attributes 与 Player → Combat 的请求组件**。
5. **`WorldInteractionResultComponent` 补下游投影**，把半闭环闭合。
6. **未接线清单单独决策**（见 §10.6），不要与上述迁移混在同一轮。

### 10.9 本轮的 self-correction 记录

本轮审计推翻了本文档此前的四处结论，均已就地更正：§5.1 的严重度、§6 的两条合规样例、
§9.5 的"静态状态合规"、§9.6 的 `AiSystem` 字段数。另有约束文档中 `ItemDefinitionRegistry`
被误列为不可变目录对象，已在 `Context/项目约束/ECS系统输入边界约束.md` 修正为只承认有 `Seal()` 的目录。

**约束本身也补了一条**。`CommunicationOutboundProjectionSystem` 注入的 `ICommunicationOutboundPort`
（`CommunicationSessionSystems.cs:74`）唯一成员是 `bool TrySubmit(OutboundPacketIntent)`，但其实现
`CommunicationOutboundDeliveryPump` 持有 `Channel`、后台 `Task` 与 `_lastDelivery`（`:13-16`）。
按规则 3 的字面读法这是违规。但区分**输入边**与**输出边**后结论不同：输入边是系统读来决策的来源，
输出边是效果执行边界，二者混为一谈会把"隔离 I/O"这件事本身判成违规，并与
[副作用隔离规范](../../../Context/项目约束/非函数式编码副作用隔离规范.md) §8 冲突。

约束已补"输出型端口不在此列"一节，给出三条判定条件（单向写入、不参与游戏判定、不别名到权威状态）。
该系统的判定因此由 `unknown` 转为**合规**，`WorldRuntime` / `WorldItemStore` / `IWorldInteractionWorld`
三条仍为违规，因为它们修改的是域权威状态。

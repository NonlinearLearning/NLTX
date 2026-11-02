# Share 组件与系统接线审计（2026-11-01）

范围：`src/NSSLC/Components/Share/` 下 308 个 `.cs`、310 个公开类型。
目的：找出"写了但没接线"的组件与系统，补上注册，并清理确认为死的类型。

## 1. 结论摘要

| 项 | 变化前 | 变化后 |
| --- | --- | --- |
| 已注册系统（`NSSLC.Server`） | 19 | 22 |
| 从未构造的 Share 系统 | 4 | 1（`InventoryAttributeContributionSystem`，属推迟决策链） |
| 零引用组件壳 | 4 | 0 |
| 三个新 tick 系统在真实服务器中的匹配实体数 | 属性 0、Buff 0、重力 1 | 属性 1、Buff 1、重力 1（仅 NPC） |
| 构建 | — | 3 个项目 0 警告 0 错误 |

**限定**：三条管线已正确接入调度、逻辑经探针验证、且都有匹配实体。但属性与 Buff 的匹配
实体目前是**空容器**（无基值、无修饰器、无 Buff），重力对 NPC 同理（无修饰器），
所以可观察行为变化仍为零。详见 §3.4。

未接线的四条链路中，本轮把三条接进了调度并补齐了玩家侧组件组合（属性 tick、Buff tick、
重力 tick）。`Inventory`/`Item` 与 `Entity` 定义两条大链**刻意不接**，理由见 §6。

## 2. 已接线基线

`Program.BuildRegistrations` 装配的 19 个系统（本轮之前）：

| 阶段 | 系统 |
| --- | --- |
| Preparation | `CommunicationSessionTickSystem` |
| Simulation | `PhysicsContactCaptureSystem`、`ProjectileLifetimeTickSystem`、`ProjectileDamageRequestSystem`、`WorldTickSystem`、`NpcAiIntentAdapter`、`SpatialSimulationCoordinatorSystem` |
| Commit | `CombatAttackRequestSystem`、`CombatDamageRequestSystem`、`ProjectileDamageResultSystem`、`NpcCombatResultSystem`、`PlayerCombatResultSystem`、`PlayerDeathRespawnSystem`、`ProjectileTerminationCommitSystem`、`ProtectionWindowSystem`、`WorldInteractionSystem` |
| Projection | `NpcNetworkProjectionSystem`、`WorldNetworkProjectionSystem`、`CommunicationOutboundProjectionSystem` |

`EcsServer` 按 `EcsServerPhase` 枚举顺序（Preparation→Simulation→Commit→Projection）分组成
`Group<TickContext>`，**同一阶段内保持注册插入顺序**，并拒绝同一系统实例重复注册
（`EcsServer.cs:41-92`）。因此阶段内顺序由注册顺序决定，必须显式设计。

## 3. 改动 A：注册三个 tick 系统

### 3.1 顺序依据

| 系统 | 位置依据 |
| --- | --- |
| `GravityTickSystem` | `SpatialSimulationCoordinatorSystem` 逐实体读取 `GravityStateComponent`（`SpatialSimulationCoordinatorSystem.cs:66`），重力投影必须在本 tick 求解前刷新，因此排在它之前 |
| `AttributeTickSystem` | 放在 Commit 之前，使 Commit 阶段（伤害结算、Buff 准入）新加的来源获得完整时长，不会在加入当 tick 就被扣掉一 tick |
| `BuffTickSystem` | 同上；Buff 过期时会按来源撤销属性修饰器，与 `AttributeTickSystem` 同阶段即可 |

插入点：`npcAiAdapter` 之后、`spatialSimulation` 之前。
`NpcAiIntentAdapter` 不读取重力或属性状态（已核验），因此三者相对 AI 的顺序无依赖。

### 3.2 装配

```csharp
BuffDefinitionRegistry buffDefinitions = new();
AttributeTickSystem attributeTick = new(ecsWorld);
BuffTickSystem buffTick = new(ecsWorld, new BuffSystem(buffDefinitions));
GravityTickSystem gravityTick = new(ecsWorld);
```

`BuffSystem.Advance` 只推进 `BuffListComponent` 并在过期时撤销属性来源，不访问定义表
（`BuffSystem.cs:89-104`），因此空 `BuffDefinitionRegistry` 下注册是安全的；定义表只在
`TryApply` 使用。当前没有注册任何 `BuffDefinition`，所以本系统目前只能推进与撤销，
不能新增 Buff——这是内容侧待补的缺口，不是接线缺陷。

### 3.3 运行时证据

`Build/tickprobe` 通过真实 `EcsServer` 调度验证（不是直接调用系统方法）：

```text
t=-1 | grav mag=0.0000 max=0.0000 dir=Normal mods=2 | attr n=1 [Defense/3] | buff n=2 [probe.timed/3,probe.permanent/-1] | buffAttr n=1 [Defense/-1]
t=0  | grav mag=5.0000 max=10.0000 dir=Normal mods=2 | attr n=1 [Defense/2] | buff n=2 [probe.timed/2,probe.permanent/-1] | buffAttr n=1 [Defense/-1]
t=1  | grav mag=5.0000 max=10.0000 dir=Normal mods=2 | attr n=1 [Defense/1] | buff n=2 [probe.timed/1,probe.permanent/-1] | buffAttr n=1 [Defense/-1]
t=2  | grav mag=5.0000 max=10.0000 dir=Normal mods=1 | attr n=0 [-]           | buff n=1 [probe.permanent/-1]              | buffAttr n=0 [-]
```

读出的四条事实：

1. 重力投影从初始 `0.0000` 被重算为 `5.0000`（`MagnitudeOverride` 生效），3 帧修饰器在第 2
   tick 被摘除，永久修饰器保留。
2. 限时属性来源 `Defense/3` 递减到 `1` 后在第 2 tick 被摘除。
3. 限时 Buff `3→2→1` 后过期，永久 Buff 保留。
4. Buff 过期时 `BuffSystem` 连带撤销了来源 21 的永久属性修饰器——跨系统联动成立。

其中第 1 条同时证明 `GravityTickSystem` 的"推进 + 重算"两件事都发生了：若只推进不重算，
`mag` 会保持 `0.0000`。

### 3.4 真实服务器的匹配面

§3.3 用的是合成实体，它证明的是**系统逻辑正确**，不是"游戏实体因此改变行为"。
按 `CreatePlayer` 与 NPC 创建路径复刻真实组合后测得：

| 查询 | 改动前 | 改动后 | 原因 |
| --- | --- | --- | --- |
| `AttributeSetComponent` | 0 | 1 | `CreatePlayer` 原本不带；本轮回填空容器 |
| `BuffListComponent` | 0 | 1 | 同上 |
| 四个 `Gravity*` | 1 | 1 | 只有 NPC 路径（`Program.cs:405+`）带全，玩家不带 |
| 空间求解四件套 | 1 | 1 | 同上；玩家缺 `GroundingComponent` |

```text
real-composition | attr=1 buff=1 gravity=1 spatial=1
```

四点结论：

1. **属性与 Buff 从"零主体"变为"有主体"。** 改动前全仓 `new AttributeSetComponent` 与
   `new BuffListComponent` 各零处，两个系统在真实服务器匹配 0 个实体——注册了也等于白写。
   `CreatePlayer` 补上两个空容器后各匹配 1 个。
2. **补组合是行为中性的，有代码级依据。** `AttributeCombatCalculator.Resolve` 的实现是
   `attributes is null ? fallback : AttributeCalculator.Resolve(attributes, kind, fallback)`；
   而 `AttributeCalculator.Resolve` 在空集合上取不到基值即用 `fallback`，additive 0、
   multiplier 1，结果同样是 `fallback`。因此 `DamageMitigationSystem` 中受
   `targetAttributes is not null` 守护的防御、抗性、减伤、护盾分支，对空集合求值均为 0，
   与 null 路径数值完全一致。补组合不改变任何伤害结果。
3. **重力与空间求解的不对称是有意设计。** 玩家位置由客户端权威写入
   （`GameCommunicationAdapter.cs:161-164` 直接写 `LocationComponent.Coordinates`），
   服务器不模拟玩家移动，因此玩家不带 `GroundingComponent` 与重力组件是自洽的，不是缺陷。
4. **即便对 NPC，重力系统今天也不改变可观察值。** NPC 创建时 `GravityStateComponent` 初值
   就是 `(0.4, 10, Normal, false, false)`，与 `GravityConfigurationComponent` 默认值一致，
   且没有任何 `GravityModifier`，因此重算结果等于初值。重力 tick 的价值要到存在修饰器
   （药水、装备、区域效果）之后才体现。

准确表述：三个系统**已正确接入调度、逻辑经实测、且都有匹配实体**；属性与 Buff 目前的
作用面是"空容器"，值为零，直到内容侧加入基值与修饰器。重力对 NPC 同理。

## 4. 改动 B：删除 4 个零引用组件壳

| 文件 | 类型 | 依据 |
| --- | --- | --- |
| `Combat/HealthRegenerationStateComponent.cs` | `HealthRegenerationComponent` | 全仓仅声明，无任何使用 |
| `Combat/KnockbackPolicyComponent.cs` | `KnockbackComponent` | 全仓仅声明，无任何使用 |
| `Combat/OxygenBarComponent.cs` | `OxygenBarComponent` | 全仓仅声明，无任何使用 |
| `Physics/Movement/CombatPositionComponent.cs` | `Terraria.Combat.PositionComponent` | `[Obsolete]` 投影，零消费者；位置权威已唯一收敛到 `LocationComponent` |

前两个文件名与类型名不一致（`HealthRegenerationStateComponent.cs` 里是
`HealthRegenerationComponent`，`KnockbackPolicyComponent.cs` 里是 `KnockbackComponent`），
本身也违反"文件名 = 类型名"约束。删除同时消除了该违规。

受影响的活文档已同步：`Share/README.md`、`Share/Physics/README.md`、
`Share/Physics/Movement/README.md`、`docs/reviews/audits/2026-10-31-physics-classification-index.md`。
`docs/architecture/`、`docs/component-decomposition/`、`docs/cr/` 下的 dated 文档是历史记录，
不改写。

## 5. 判据修正：零引用不等于死代码

初版用"外部文件引用数为 0"筛选，得出 18 个候选。该判据有两处错误，已修正：

**错误一：把"仅本文件内使用"当成死。**
`DamageProcessStatus` 与 `DamageRelationRejectionReason` 确实没有外部引用，但它们被**同文件的
活跃类型**使用（`DamageProcessResult` 有 4 个外部消费者；`DamageRelationSystem` 由
`CombatDamageSystem` 调用，而后者由已注册的 `CombatDamageRequestSystem` 驱动）。
按初判删除会直接编译失败。真实判据应是「全仓（含定义文件）总出现次数为 1」。

**错误二：把"文档规定的 API"当成死。**
以下三个类型零代码引用，但文档把它们当作规定用法呈现，删除会破坏文档承诺：

| 类型 | 文档依据 |
| --- | --- |
| `ExampleAiUsage` | `Share/AI/README.md` 三处作为"可编译的共同注册与计算入口" |
| `EcsServerCommandBuffer` | `Share/Entity/EcsServer.md:140` 规定结构变化应经它延迟提交 |
| `ReferenceCondition` | `Share/AI/Goap/SOURCES.md:96` 的上游溯源表条目 |

三者已恢复保留。它们的正确处置是**补消费者或改文档**，不是静默删除。

## 6. 刻意未处理的五组

| 组 | 内容 | 理由 |
| --- | --- | --- |
| Inventory / Item | `InventorySystem` 从未构造、`ItemDefinition` 从未实例化、`InventoryAttributeContributionSystem` 未构造 | 不是漏注册，是整链未接入；且与世界物品表示（`WorldItemStore`）存在身份模型冲突，需先定权威 |
| Entity 定义 | `EntityDefinitionSystem`、`EntityDefinitionBuilder`、`EntityDefinition`、`EntityDefinitionRegistry`、`EntityComponentSet`、`IEntityDefinitionHost` | 闭合的死簇，要么整簇接上，要么整簇删除；半删会让状态更差 |
| Attributes 分类法 | `AttributeContributionKind`（11 成员）、`AttributeContributionKindMapping` | 2026-11-01 attributes 收敛时按要求建立的分类产物，当前无代码消费者但承担文档职责 |
| HTN 构建器 | `DomainBuilder`、`BaseDomainBuilder` | FluidHTN 适配面，`AI/Htn/SOURCES.md` 有溯源条目 |
| 战斗数值枚举 | `DamageProcessStatus`、`DamageRelationRejectionReason` | 实际存活，见 §5 |

### 6.1 Inventory / Item：先定身份，再选路线

证据：

- `InventorySystem(ArchWorld, ItemDefinitionRegistry)` 从未构造；`ItemDefinitionRegistry` 未注册
  任何定义；`new ItemDefinition(` 与 `new ItemStack(` 全仓**零处**。
- 服务器实际使用的是 `PlayerInventoryComponent`（`Dictionary<int, PlayerInventorySlot>`，
  槽内容是 `PlayerInventorySlot(int ItemType, int Stack, byte Prefix, bool Favorited)`），
  与 `Share/Inventory` 的 `InventoryComponent`（基于 `ItemStack` + `ItemDefinitionId`）是两套表示。
- 世界物品侧同样分裂：`WorldItemStore.WorldItemState.ItemType` 是 `int`。

| 方案 | 做法 | 代价 | 收益 |
| --- | --- | --- | --- |
| A. 协议优先 | 承认 `int ItemType` 为权威身份，删除 `Share/Item` 与 `Share/Inventory` 的 ECS 路径，堆叠/属性贡献改用数值身份重写 | 丢失类型化物品定义与属性修饰器集成 | 概念最少，与 Version4 协议一致 |
| B. 领域优先 | 给 `ItemDefinition` 增加数值协议 id，`ItemDefinitionRegistry` 支持反查；`InventoryComponent` 升为权威，`PlayerInventoryComponent` 降为网络投影 | 需要定义数据来源与一次性迁移 | 拾取、物理、属性可统一用实体查询 |

**先决条件（两条路都要）**：物品身份到底是字符串定义 id 还是 1..5455 数值类型。
这一点定下来之前，任何一侧的接线都会制造第二份权威数量。

### 6.2 Entity 定义：要么补宿主，要么整簇删

证据：

- `IEntityDefinitionHost<TEntity>` 只有两个成员（`Create(EntityDefinition, EntityComponentSet)`、
  `Destroy(TEntity)`），**全仓无任何实现**。
- `EntityDefinitionSystem<TEntity>` 依赖该宿主，因此无法构造。
- 该接口文件在 `Entity/` 目录，命名空间却是 `Terraria.Combat`，与目录归属不一致。
- 运行时实体目前全部由 `Program.CreatePlayer` 与 NPC 创建函数手写 `world.Create(...)`。

| 方案 | 做法 | 代价 |
| --- | --- | --- |
| A. 补宿主 | 为 `EntityRuntimeRegistry` 实现 `IEntityDefinitionHost<TEntity>`，把玩家/NPC 创建改为数据驱动 | 需要定义数据与创建流程迁移；命名空间需顺带修正 |
| B. 整簇删 | 删除 `EntityDefinition*`、`EntityComponentSet`、`IEntityDefinitionHost` | 放弃数据驱动创建能力 |

半删会让状态更差（留下互相引用但不可达的簇），所以必须整簇处置，不能只删其中两个类型。

## 7. 验证

| 项 | 命令 | 结果 |
| --- | --- | --- |
| `Terraria.Components` | `dotnet build src\NSSLC\Terraria.Components.csproj -v q` | 0 警告 0 错误 |
| `NSSLC.Server` | `dotnet build src\NSSLC.Server\NSSLC.Server.csproj -v q` | 0 警告 0 错误 |
| `Terraria.NpcAi.MinimalVerification` | `dotnet build Test\...\*.csproj -v q` | 0 警告 0 错误 |
| tick 探针 | `dotnet run --project Build\tickprobe` | 三条管线推进符合预期，见 §3.3；真实组合匹配面见 §3.4 |
| world 探针回归 | `dotnet run --project Build\worldprobe` | 195 行输出与改动前逐字节相同 |

世界域行为无回归：本轮改动不触碰 `Terraria.World`，探针比对用于排除程序集级副作用。

## 8. 遗留决策项

1. **Buff 内容缺口**：`BuffDefinitionRegistry` 为空，`BuffSystem.TryApply` 恒返回
   `UnknownDefinition`。要让 Buff 真正生效，需要定义来源与注册时机。
2. **属性基值缺口**：玩家已有空 `AttributeSetComponent`，但没有任何 `SetBaseValue` 调用，
   所以防御、伤害等属性全部取 fallback 0。需要内容侧写入基值。
3. **重力无修饰器**：NPC 已有完整重力组件，但没有任何 `GravityModifier` 来源，重算结果
   恒等于初值。需要内容侧提供修饰器（药水、装备、区域效果）才有可观察效果。
4. **属性修饰器来源时序**：当前 Commit 新增的来源要到下一 tick 才开始递减（Simulation 在前）。
   这是刻意的"新来源获得完整时长"，若改为同 tick 递减需显式记录意图。
5. **两条未接大链**：Inventory/Item 与 Entity 定义，需各自决定"接上还是删除"。
   前者另有一个身份模型冲突（`int ItemType` vs `ItemDefinitionId`）需先定权威。
6. `AttributeContributionKind` 分类法需要消费者，否则会在下一次清理中重新成为候选。

## 9. 补充：物品身份枚举化与内容样例

本节记录 §8 中第 1、2、3、5 条的处理结果，其中第 5 条的身份问题已定。

### 9.1 物品身份改为枚举

原问题：`Share/Item` 用字符串 `ItemDefinitionId`，服务器用 `int ItemType`
（`PlayerInventoryComponent.PlayerInventorySlot`、`WorldItemState.ItemType`），两者没有转换代码，
任一侧接线都会产生第二份权威。

处置：物品身份使用枚举。

- 从参考客户端 `D:\TRbackup\最新的客户端\Terraria.ID\ItemID.cs` 的 `public const short/int` 常量生成
  `src/NSSLC/Components/Share/Item/ItemId.g.cs`：6244 个成员，值域 -48..6195，含 `None = 0`，
  成员名与数值均与客户端一致。
- 新增 `ItemIdRules` 收敛唯一转换点：`IsKnown`、`ToNetworkType(ItemId) -> short`、
  `TryFromNetworkType(int, out ItemId)`。
- `ItemDefinition.Id`、`ItemStack.ItemId`、`ItemDefinitionRegistry` 的键全部改为 `ItemId`；
  删除字符串身份 `ItemDefinitionId.cs`。
- 协议侧 `int ItemType` **保持不变**：那是线上表示，不是领域身份。

### 9.2 内容样例（逐条取自客户端源码，非推测）

| 内容 | 客户端依据 | 映射到 NSSLC |
| --- | --- | --- |
| 铁皮 | `Player.cs:10049` `statDefense += 8`；`Item.cs:4868` 28800 tick | `Defense` Additive 8 |
| 迅捷 | `Player.cs:10041` `moveSpeed += 0.25f`；`Item.cs:4840` 28800 tick | `MovementSpeed` Additive 0.25 |
| 再生 | `Player.cs:10038` `lifeRegen += 4`；`Item.cs:4826` 28800 tick | `LifeRegeneration` Additive 4 |
| 羽落 | `Player.cs:10066` `slowFall = true`；`Player.cs:21824` `gravity / 3f`；`Item.cs:4910` 36000 tick | `MagnitudeMultiplier` 1/3 |
| 重力药水 | `Player.cs:10114` `gravControl = true`；`Item.cs:5050` 10800 tick | `ToggleDirectionCapability` 1 |
| 玩家基值 | `Player.cs:18441` `statDefense = 0`；`Player.cs:18462` `moveSpeed = 1f` | `Defense` 0、`MovementSpeed` 1 |

落地位置：`Share/Attributes/Buff/BuffContent.cs`、
`Share/Physics/Movement/GravityContent.cs`、`Share/Attributes/PlayerAttributeBaseline.cs`。
`Program.cs` 注册三条 Buff 定义，并在 `CreatePlayer` 应用玩家基值。

### 9.3 补内容后暴露的第三个组合缺口

补齐内容后实测**仍不生效**。根因不是注册表，而是准入：
`BuffAdmissionSystem.Check` 在 `capability` 为 null 时返回 `MissingReceptionCapability`，
而全仓没有任何实体组合 `BuffReceptionCapabilityComponent`。
已给 `CreatePlayer` 补上 `new BuffReceptionCapabilityComponent(BuffReceptionMode.AllowAll)`。

实测输出（`Build/tickprobe`）：

```text
content | emptyRegistryBuffs=0 contentBuffs=1 defense=8.00 moveSpeed=1.00 featherMag=0.1333
```

| 字段 | 含义 |
| --- | --- |
| `emptyRegistryBuffs=0` | 注册表为空时 `TryApply` 不落地任何 Buff（即修复前的行为） |
| `contentBuffs=1` | 铁皮定义真正进入 `BuffListComponent` |
| `defense=8.00` | 该 Buff 的属性修饰器同时写入，`Defense` 解析为 8 |
| `moveSpeed=1.00` | 玩家基值生效 |
| `featherMag=0.1333` | 0.4 × 1/3，羽落按客户端语义生效 |

### 9.4 四个零引用候选：全部补上消费者

四个候选都不是死代码，而是"只有文档引用"的 API。处置方式是给它们真实消费者，落在
`Test/Terraria.NpcAi.MinimalVerification/SharedApiUsage.cs`（受版本控制，不是 `Build/` 下的临时探针）。

| 候选 | 消费者实际做的事 | 验证到的断言 |
| --- | --- | --- |
| `ExampleAiUsage` | 调用 `CreateSystem()` 构造共同 AI 入口（注册 Slime 与 AttackGoap 两个行为） | 返回值非空 |
| `EcsServerCommandBuffer` | `Create` 登记结构意图、`Set` 替换 `LifeComponent`、`Add` 追加 `LocationComponent`、`Playback` | `Size > 0`；回放后 `LifeComponent.Maximum` 由 20 变为 100；`LocationComponent` 已挂上 |
| `ReferenceCondition` | 与两个 `WorldKey` 组合成 GOAP 条件 | 三个属性（`WorldKey`/`Comparison`/`ValueKey`）往返一致 |
| `AttributeContributionKind` + `Mapping` | 遍历 `Enum.GetValues<AttributeContributionKind>()` 逐个调用 `ToSourceCategory` | 十一类形态全部映射到已定义的 `AttributeSourceCategory`，无一抛异常 |

运行输出：

```text
shared-api | ai=ok buffer=ok condition=ok contributionKinds=11
```

引用面复核（`src/` + `Test/` 全文计数）：

| 类型 | 改动前 | 改动后 |
| --- | --- | --- |
| `ExampleAiUsage` | 1（仅定义） | 2 文件 |
| `EcsServerCommandBuffer` | 1（仅定义） | 2 文件 |
| `ReferenceCondition` | 1（仅定义） | 2 文件 |
| `AttributeContributionKindMapping` | 1（仅定义） | 2 文件 |
| `AttributeContributionKind` | 定义 + 映射 | 3 文件 |

**过程中的一个坑**：`EcsServerCommandBuffer.Create` 返回的是占位标识，`Playback` 之后
`world.IsAlive(returnedEntity)` 为 `false`——实体标识由世界在回放时分配。因此断言改为
"`Size` 增长 + 真实实体上 `Set`/`Add` 的回放效果"，而不是断言占位标识存活。

### 9.5 与 §8 的对应关系

| §8 条目 | 状态 |
| --- | --- |
| 1 Buff 内容缺口 | 已解决：三条定义注册进 `BuffDefinitionRegistry`，实测可落地 |
| 2 属性基值缺口 | 已解决：`PlayerAttributeBaseline` 写入 `Defense` 0、`MovementSpeed` 1 |
| 3 重力无修饰器 | 部分解决：`GravityContent` 提供羽落与重力药水两条来源，实测 `featherMag=0.1333`；尚未接入具体玩法触发点 |
| 4 属性来源时序 | 未变，仍是刻意设计 |
| 5 两条未接大链 | 身份已定（`ItemId` 枚举）；接线本身仍未做 |
| 6 分类法需消费者 | 已解决：`SharedApiUsage` 遍历验证十一类映射 |

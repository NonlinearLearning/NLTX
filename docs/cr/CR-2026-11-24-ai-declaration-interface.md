# CR-2026-11-24 AI 声明接口收口（P0–P4）

| 项 | 内容 |
| --- | --- |
| 文档 ID | CR-2026-11-24-ai-declaration-interface |
| 逻辑域 | NPC / 共享 AI |
| 产物类型 | 变更记录 |
| 状态 | 已实施（P0–P4） |
| 日期 | 2026-11-24 |
| canonical 路径 | docs/cr/CR-2026-11-24-ai-declaration-interface.md |
| 上游依据 | [AI 声明接口收口执行计划](../plans/2026-11-25-ai-declaration-interface-execution-plan.md) |
| 前置 | [ECS 协作规则与调度声明设计](../architecture/2026-11-24-ecs-coordination-rules-design.md) |

## 1. 授权与范围

按 [执行计划](../plans/2026-11-25-ai-declaration-interface-execution-plan.md) 第 4 节实施 P0–P4 四个批次。
P3（顺序锚点）与 P4（提交表驱动）原先按计划 §3 的启动条件推迟；2026-11-25 复核后确认条件成立
（P3 的「真实排序需求」就是 `npc.ai.intent` 与 `player.input.apply` / `movement.intent.apply`
之间的两条边，P4 的「构造期拦截声明与代码提交目标不一致」由提交表本身提供），两批一并交付。

范围：`src/NSSLC/Share/AI/**`、`src/NSSLC/Npc/System/**`、`src/NSSLC/Share/Entity/System/**`、
`src/NSSLC.Server/Program.cs` 的 AI 接线、`Test/Terraria.Components/Terraria.Npc.Ai/**`。
未改 `src/NSSLC.Infrastructure/**`，未动 `EcsServer.BuildSchedule` 与 `ValidateCoordinationRules`，
未新增调度器、事件总线或协议字段。

## 2. 理由

改动前，一条 AI 行为的「读什么」「怎么写出来」「能发什么」分散在三处：
实体定义构建期可以再挂一份快照需求，行为内部自带一份 `AiRequirements`，宿主注册时又手写一次工厂。
三处各自都能被单独修改，因此「声明」实际上不约束任何东西——它只是第三份副本。
目标是把三者收成一条声明，并让不一致成为构造期失败而不是运行期异常。

## 3. 改动

### 3.1 P0 死接缝清理

| 动作 | 文件 | 理由 |
| --- | --- | --- |
| 删除 `AiSnapshotRequirements` 属性与构造参数 | `Share/Entity/System/Entity.cs` | `git grep` 零调用点，只有 README 提到，属未接通的死接缝 |
| 删除 `WithAi<TBehavior>(AiRequirements?)` 重载 | `Share/Entity/System/EntityBuilder.cs` | 三个调用点全部使用无参重载；`Build()` 的 `Require<T>` 校验保留不动 |
| 删除「在定义构建时加入 AI 需求」示例段 | `Share/Entity/README.md` | 该能力已不存在，留着会误导 |
| 移除查询回调未使用的 `ref AiBehaviorSelectionComponent` 参数 | `Npc/System/NpcAiIntentAdapter.cs` | 保留 `WithAll<AiBehaviorSelectionComponent>` 作为准入闸门，只删回调参数 |

### 3.2 P1 输入声明化

新增（通用）：

| 类型 | 职责 |
| --- | --- |
| `AiBehaviorDeclaration<TIntent>` | 一条声明：`BehaviorType`、`Reads`、以及以 `Reads` 构造行为的工厂；`CreateBehavior()` 注入需求并核对产物类型 |
| `AiBehaviorCatalog<TIntent>` | 声明目录；`Add` 拒绝重复类型与封存后写入，`Seal()` 后只读 |

新增（NPC 域）：

| 类型 | 职责 |
| --- | --- |
| `NpcAiDeclaration` | 组合根：构造 Guide + Slime 两条声明并封存目录 |
| `NpcAiSnapshotContributors` | 按域拆分的快照投影器集合（移动 / 目标 / 生命 / 世界），每个自己拥有查询 |

修改：

| 文件 | 改动 |
| --- | --- |
| `Share/AI/AiSystem.cs` | 新增 `Register(AiBehaviorDeclaration<TIntent>)` |
| `Npc/System/NpcAiSnapshotProvider.cs` | 删除全部查询与事实字面量，改为按 `FactTypes` 分派给 contributor |
| `Npc/System/NpcAiRegistration.cs` | 从封存目录注册行为；建服期校验「声明的读取必须可被投影」 |
| `Npc/System/GuideAiBehavior.cs`、`SlimeAiBehavior.cs` | 删除 `private static AiRequirements Requirements`，改为构造函数注入 |

**需求来源的收敛方式（计划 §10 未闭合项 3）选定为构造函数注入**，不采用「行为内部查目录」。
理由：目录解析在行为内部仍要查一次表，漂移只是被检查出来（运行期）；构造注入让行为拿不到第二份需求，
行为若要自持读取就必须绕开工厂，那是显式改代码，不是静默漂移。

### 3.3 P2 输出声明化

| 类型 | 职责 |
| --- | --- |
| `NpcAiBehaviorDeclaration` | 共享声明 + `Emits`（`NpcAiIntentKind` 强类型集合，非空、已定义、无重复） |
| `NpcAiCatalog` | 域目录，包一层共享 `AiBehaviorCatalog<NpcAiIntent>`，`Emits` 因此不需要给共享层加泛型参数 |
| `NpcAiIntentDispatch` | 分支表 `HandledKinds` + `EnsureComplete()`（枚举成员与分支表对齐）+ `EnsureDispatches(emits, behaviorType)` |

**校验归属（实施中相对计划 §7 的修正）**：`Emits` 的校验落在 `NpcAiIntentAdapter` 的构造，而不是 `NpcAiRuntime`。
适配器自己拥有那份 `switch` 分支表，只有它能判断「这个 kind 我发得了吗」；放运行时会让运行时依赖适配器的
实现细节，且适配器可以不经运行时被单独构造（验证项目正是如此），那条路上的校验就会漏。
`NpcAiIntentAdapter` 因此新增 `NpcAiCatalog` 构造参数。

### 3.4 P3 顺序声明化

| 类型 | 职责 |
| --- | --- |
| `AiAnchor` | 能力角色枚举：`InputApply`、`MovementCommit`。角色不写注册键，也不叫某个系统名 |
| `AiAnchorBindings` | 组合根的锚点→注册键表。`Create` 拒绝缺锚点、重复键、未定义锚点与带空白键；`EnsureResolvable` 拒绝键不在已建调度里 |
| `NpcAiOrdering`（宿主） | 唯一同时认识两套词汇的地方：把声明里的锚点解析成调度边，并投影成同一批 `EcsCoordinationRule` |
| `NpcAiCatalog.Edges` / `OrderingRules` | 两条声明共享同一注册，锚点相同的边去重；规则与边是同一次投影 |
| `NpcAiCatalog.EnsureConsistent` | 生成边阶段拒绝「同一条注册被要求既在某键之前又在某键之后」 |

修改：

- `NpcAiBehaviorDeclaration` 增加 `After` / `Before` 锚点集合（同一锚点不得同时出现在两侧）。
- `Program.cs` 的 `MovementIntentApply` 与 `npc.ai.intent` 注册改成用 `AfterKeysOf` 生成 `after`，
  手写的 `NpcAiIntent` 项删除；`.rules` 新增 5 条 AI 条目，其中两条 `before` 是调度探针。
- 锚点集合按计划 §10 第 4 条复核：**2 个**，不是上限的 5 个（另外 3 个没有声明消费者）。

### 3.5 P4 提交表驱动

| 类型 | 职责 |
| --- | --- |
| `NpcAiIntentCommitTable` | kind → 目标组件 → 写入函数的唯一映射；`EnsureComplete` 要求「每个枚举成员一行、目标组件在允许集合内」 |
| `NpcAiIntentCommit` | 一行：`Kind`、`Target`（提交到的组件）、`Handler`（写入） |
| `NpcAiCommitContext` | 提交能触达的全部对象：实体与句柄、tick、两条请求队列、解析器与攻击目录、`ref` 行为状态 |
| `NpcAiBehaviorDeclaration.Commits` | 由 `TargetsOf(Emits)` 投影，声明里没有第二个字段可漂移 |

`ApplyIntent` 的 `switch` 与其 `default: throw` 删除，P2 的 `NpcAiIntentDispatch`（只有 kind 集合）
一并删除——两套 kind 列表正是 D-3 要避免的漂移。提交目标的校验口径按实际组件族修正为
「AI 自己的 `NpcBehaviorStateComponent`」或 `*RequestQueueComponent` / `*IntentQueueComponent`；
计划里写的 `*RequestComponent` / `*IntentComponent` 是这一族的简写，照字面匹配只会得到一条
无人能过的规则。

## 4. 验证

命令与退出码见[计划 §8.1 / §8.2](../plans/2026-11-25-ai-declaration-interface-execution-plan.md)，此处只记结论：

- `Terraria.Components`：0 warning / 0 error；`NSSLC.Server`：0 error（19 warning 全部位于未改动文件）。
- 六个组件验证项目（`Terraria.Npc.Ai`、`Entity.Definition`、`Entity.Scheduling`、`Attributes`、
  `EntitySpawn`、`Projectile`）全部 0 FAIL。
- `Terraria.Npc.Ai` 四条探针：`declaration-drives-inputs`（4 个投影事实、2 条封存声明、
  读取集 ⊆ 投影集、行为实际读取集 == 声明读取集）、`anchor-ordering`（2 个锚点、2 条边、2 条规则；
  缺锚点 / 重复键 / 未定义锚点 / 空白键 / 键不在调度里 / 跨声明矛盾六种拒绝）、
  `declaration-edges`（边在→顺序对、边删→顺序错，两个方向都断言）、
  `commit-table`（5 个 kind 各一行、目标在允许集合内、未处理 kind 被拒绝、`Commits` 等于表投影）。
- 真实启动：`ecs.npc.spawned` + `ecs.server.ready` 正常，stderr 0 行。
- `git grep AiSnapshotRequirements -- src Test` 无匹配。

未执行：客户端输入驱动 NPC AI 的全链路（本计划不含）；Projectile AI 链路接入后的锚点复核（D7 未开工）。

## 5. 未闭合

1. **D7 Projectile AI 链路悬空不在本计划**，仍需 provider + adapter + 注册 + 验证四件独立工作。
   它也是「锚点集合是否需要第三次扩容」的唯一未定输入。
2. **锚点集合当前是 2 个**（`InputApply`、`MovementCommit`）。计划的上限是 5 个；
   另外 3 个没有声明消费者，按 D-1 的「未用锚点不是免费的」删除。Projectile AI 接入后须重新复核。
3. ~~**P3 顺序锚点未开工**~~ → 已交付，见 §3.4。
4. ~~**P4 `Commits` 表驱动未开工**~~ → 已交付，见 §3.5。
5. ~~**`AiBehaviorSelectionComponent` 在 NPC 路径的归属未定**~~ → **已定：保留为准入闸门**。
   P3 不需要运行期按 profile 查声明（目录键是行为类型、锚点是能力角色），该组件只承担
   「实体是否加入 NPC AI」的组合契约，代码里没有任何字段读它。

## 6. 回滚

| 批次 | 回滚方式 | 数据影响 |
| --- | --- | --- |
| P0 | 恢复 `AiSnapshotRequirements` 与 `WithAi` 重载、恢复回调参数 | 无 |
| P1 | 删除声明与目录，恢复 `NpcAiSnapshotProvider` 的四个 `if` | 无（声明不落盘、不进存档） |
| P2 | 删除 `NpcAiCatalog` / `NpcAiIntentDispatch`，`NpcAiIntentAdapter` 去掉 catalog 参数 | 无 |
| P3 | 删除锚点类型与绑定表，恢复注册上的手写 `after` 与 `.rules` 的 AI 段 | 无（锚点不落盘） |
| P4 | 恢复 `ApplyIntent` 的 `switch` 与 `NpcAiIntentDispatch`，删除提交表 | 无（提交表只存在于构造函数） |

五个批次都不改变组件布局、协议或存档格式，不存在数据迁移。

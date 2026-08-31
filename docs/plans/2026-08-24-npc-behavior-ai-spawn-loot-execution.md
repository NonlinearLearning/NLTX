# NPC 行为、AI、生成和掉落完整迁移执行提案

> **For Codex:** REQUIRED SUB-SKILL: Use `executing-plans` to implement this proposal
> task-by-task. 每个任务必须保留 source anchor、authority owner、projection 和 executable
> verifier；局部 AI 或单个 NPC 通过不能推导完整 NPC parity。

**目标：** 将 Version4 `Terraria/NPC.cs` 的服务器相关 NPC 定义、生成授权、目标选择、AI、
运动、战斗、死亡、分段关系、掉落和复制责任迁移到 `Terraria.Dome.Simulation` 的组合式
ECS，并以确定性回放、持久化恢复、协议投影和多会话 loopback 证明每个行为族。

**架构：** NPC 是 Arch ECS 中由共享能力组件和 NPC 专属组件组合出的实体。不可变
`NpcDefinition` 与行为注册表提供静态规则；系统只读取世界/玩家/NPC snapshot，输出 typed
command 或事实 event；结构变化、伤害、死亡、掉落和实体创建在明确提交阶段完成。Server
和 Protocol 只消费 `NpcReplicationSnapshot`/`NpcSyncPacket`，不能接触旧 `Terraria.NPC`
实例、`Main.npc` 或 `ai[]`。

**技术栈：** .NET 10、C#、Arch ECS、现有 `WorldGrid`/Physics/Combat/Item 系统、V1456
`SyncNPC` 编解码、确定性随机流、serial `dotnet` verifier 和 WLD/world snapshot replay。

---

## 1. 事实基线和完成定义

### 1.1 事实源

只读来源：

```text
D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs
```

来源结构已在 [npc-ecs-migration-design.md](/D:/TRbackup/NLTX/docs/plans/2026-08-18-npc-ecs-migration-design.md)
中核对，核心责任族包括：

| Legacy 责任 | 代表入口/区域 | 目标 owner |
| --- | --- | --- |
| 类型定义和初始化 | `SetDefaults`、`SetDefaults_ForNetId` | `NpcDefinition`、注册表和实例初始化提交 |
| 自然/强制生成 | `Spawner`、`SpawnNPC`、`TrySpawnAnNPC`、`SpawnBoss`、`SpawnOnPlayer` | 生成资格、Spawn command、Spawn commit |
| 运行时状态 | `active`、`type`、`ai[0..3]`、`localAI[0..3]`、`life`、`defense`、`timeLeft` | ECS components |
| 目标选择 | `TargetClosest`、`GetTargetData`、目标追踪分支 | `NpcTargetSelectionSystem`、目标有效性规则 |
| AI | `AI()`、约 157 个 `AI_###` 行为族 | typed `BehaviorId` + 行为系统 |
| 移动和碰撞 | 速度、重力、Tile collision、平台/斜坡分支 | shared Physics + NPC movement intent |
| 受伤和死亡 | `StrikeNPC`、`CheckActive`、`checkDead` | Combat damage、lifecycle、death commit |
| 分段关系 | `realLife`、worm segment、父子关系 | `NpcSegmentComponent`、segment lifecycle |
| 掉落和世界事件 | `NPCLoot`、掉落表、死亡事件、入侵进度 | `NpcLootSystem`、Item command、WorldEvent event |
| 网络复制 | `netUpdate`、`netSpam`、NPC sync paths | replication snapshot、PVS projector、V1456 codec |

现有 [npc-ecs-migration-design.md](/D:/TRbackup/NLTX/docs/plans/2026-08-18-npc-ecs-migration-design.md)
已经定义了普通敌怪、城镇 NPC 骨架和分段 NPC 骨架的首阶段边界。该首阶段不等于完整
迁移；Boss、完整 `AI_###`、入侵/事件生成、完整城镇服务、Buff、捕捉/释放和完整掉落表
仍需独立行为族证据。

### 1.2 当前实现锚点

实施前必须重新核对这些类型是否仍为当前 owner；计划不允许按旧文件名创建平行实现：

- `src/Terraria.Dome.Simulation/Components/Npc*`
- `src/Terraria.Dome.Simulation/Npc/`
- `src/Terraria.Dome.Simulation/Combat/Commands/DamageNpcCommand.cs`
- `src/Terraria.Dome.Simulation/Snapshots/NpcSnapshot.cs`
- `src/Terraria.Dome.Simulation/Snapshots/NpcReplicationSnapshot.cs`
- `src/Terraria.Dome.Simulation/Physics/Systems/TileCollisionSystem.cs`
- `src/Terraria.Dome.Simulation/Loot/LootTable.cs`
- `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`
- `src/Terraria.Dome.Protocol.V1456/`

当前验证入口包括：

- `Test/Terraria.Dome.Npc.Verification`
- `Test/Terraria.Dome.Npc.Boundary.Verification`
- `Test/Terraria.Dome.Npc.Composition.Verification`
- `Test/Terraria.Dome.Npc.Protocol.Verification`
- `Test/Terraria.Dome.Combat.Verification`
- `Test/Terraria.Dome.Combat.Protocol.Verification`
- `Test/Terraria.Dome.Combat.Loopback.Verification`
- `Test/Terraria.Dome.PlayerAuthority.Verification`
- `Test/Terraria.Dome.WorldRules.Verification`

### 1.3 完整迁移完成条件

NPC 责任族只有同时满足以下条件才允许标记为 `Complete`：

1. `NPC.cs` 的 ServerRelevant 方法、字段、定义表和 AI family 都有 source anchor 和
   当前 owner；没有无理由的 `Unknown`/`Unmapped`。
2. 每个行为族有不可变输入、typed state、系统顺序、命令/事件和拒绝路径。
3. 生成、目标、AI、移动、伤害、死亡、掉落和复制在相同 seed/input trace 下可重放。
4. NPC snapshot、Dome persistence snapshot 和 `SyncNPC` 之间的字段来源一致。
5. 多会话 loopback 证明 PVS、owner、revision 和 tombstone 没有泄漏或重复。
6. 旧依赖扫描不再发现 `Terraria.NPC`、`Main.npc`、旧 `ai[]`、`NewNPC`、`NetMessage`
   或固定槽位作为 Simulation authority。

以下结果不计为完整迁移：

- 只有 `NpcDefinition` 名称或协议 catalog，没有完整默认值和行为。
- 只有一个 chase AI 通过，不能代表全部 `AI_###`。
- 只有 NPC 死亡事件，没有掉落表、随机 stream 和 WorldItem commit。
- 只有生成命令编码，没有服务器资格、空间、预算和重复请求拒绝。
- 只有 `SyncNPC` 字节测试，没有权威 snapshot 和多会话验证。
- 编译通过、MainBoundary 通过或导入旧 NPC snapshot 通过。

## 2. 不可违反的架构边界

### 2.1 Simulation 禁止依赖

NPC Simulation 不得引用或读取：

- `Terraria.NPC`、`Main.npc`、`Main.player`、`Main.rand`、`whoAmI`。
- `NPC.SetDefaults`、`NPC.AI()`、`NPC.NewNPC()`、`NetMessage`、`MessageBuffer`。
- Socket、session、客户端 UI、渲染、声音或客户端输入。
- 通过 `float[] ai`、`localAI[]` 或固定 NPC 数组槽位保存行为 authority。
- 反射、目录枚举或 Arch 查询创建顺序来决定 AI/system 顺序。

旧世界或旧网络数据必须在 `Terraria.WorldCompatibility`、WorldFile 或 Protocol adapter
中转换为定义输入、`NpcSnapshot` 或 `NpcReplicationSnapshot`，不得将旧 NPC 对象带入
Simulation。

### 2.2 所有权边界

| 状态/行为 | 唯一 owner |
| --- | --- |
| 类型默认值、阵营、类别、尺寸、基础生命、防御 | `NpcDefinitionRegistry` |
| 当前生命、位置、速度、活跃状态、行为阶段 | NPC ECS components |
| 目标和目标有效性 | `NpcTargetSelectionSystem` / `NpcTargetRoutingSystem` |
| 世界生成预算和随机自然生成 | `NpcSpawnEligibilitySystem` + world snapshot |
| NPC entity 创建和 ID 分配 | `NpcSpawnCommitSystem` |
| 伤害、免疫、死亡 revision | shared Combat + `NpcDeathSystem` |
| 掉落规则和世界物品创建 | `NpcLootSystem` + Item command/commit |
| 入侵/事件积分、Boss encounter | World Event resource/system，不挂在单个 NPC |
| PVS 和协议字段 | Server replication / Protocol projector |
| 客户端声音、动画、UI | Protocol/presentation adapter，Simulation 不拥有 |

### 2.3 共享契约

并发实现前先冻结以下契约，所有任务只能扩展，不得创建第二套：

```text
NpcDefinitionId -> NpcDefinition -> SpawnNpcCommand
World/Player/Npc snapshot -> eligibility/target/behavior systems
systems -> typed movement/damage/spawn/despawn/loot commands
commands -> deterministic commit -> NpcSnapshot + facts/events
NpcReplicationSnapshot -> NpcSyncPacket -> V1456 codec
```

每条 command 至少包含 `Sequence`、来源、目标 identity、expected revision 或冲突策略。
每个事实 event 只表示已提交事实，不被用作绕过 commit 的 mutation API。

## 3. 组件、定义和系统形状

### 3.1 组件

保留现有类型时优先扩展；仅在缺失时创建以下类型：

```text
NpcDefinitionComponent
NpcLifecycleComponent
NpcReplicationComponent
NpcTargetComponent
NpcBehaviorStateComponent
NpcSpawnStateComponent
NpcSegmentComponent
NpcHomeComponent
NpcInteractionComponent
```

`NpcBehaviorStateComponent` 必须表达 typed state，例如 `BehaviorId`、阶段、计时器、
方向、冷却和行为参数；禁止增加 `float[] Ai` 或无限制字典作为逃生舱。

### 3.2 系统顺序

最终顺序必须显式加入 Simulation tick schedule；不得依赖文件名：

1. `NpcSpawnEligibilitySystem`
2. `NpcSpawnCommitSystem`
3. `NpcTargetSelectionSystem`
4. `NpcTargetRoutingSystem`
5. `NpcBehaviorSystem`
6. `NpcMovementIntentSystem`
7. Shared gravity/movement/tile collision systems
8. `NpcContactEffectSystem`
9. Shared damage resolution and immunity systems
10. `NpcLifecycleSystem`
11. `NpcSegmentLifecycleSystem`
12. `NpcDeathSystem`
13. `NpcLootSystem`
14. `NpcReplicationSystem`
15. Domain commit and snapshot publication

NPC 生成和删除不得在 Arch query 遍历中直接改变结构；所有 entity mutation 在 commit 阶段
完成，并产生稳定的 ID/revision。

## 4. 执行批次

### Task 0：建立 NPC 来源、定义表和行为族基线

**目的：** 刷新事实源 hash、方法/字段/AI family inventory，并锁定当前实现和验证器结果。

**Files:**

- Create/Modify: `docs/research/2026-08-24-npc-migration-source-manifest.md`
- Create/Modify: `docs/research/2026-08-24-npc-migration-coverage.md`
- Create: `docs/research/2026-08-24-npc-behavior-family-matrix.csv`
- Test: `Test/Terraria.Dome.Npc.Verification/Program.cs`
- Evidence: `Build/diagnostics/npc-complete/task-0-baseline/<timestamp>/`

**Steps:**

1. 记录 `NPC.cs` 的路径、SHA-256、行数、方法数、字段数、`AI_###` 标记和关键入口行号。
2. 按 `Definition/Spawn/Target/AI/Movement/Combat/Lifecycle/Loot/Segment/Replication` 分类。
3. 对每一行记录 `Status`、`CurrentOwner`、`RequiredVerifier` 和 `DeferredReason`。
4. 运行当前 NPC、Combat、Protocol、Loopback 验证器，保存 exit code 和 source tree hash。
5. 写出首个行为族差分基线：普通敌怪、城镇骨架、分段骨架和已存在的掉落路径。

**验收：** 生成新的 source-backed inventory；不因现有窄边界通过而修改完整性状态；历史
manifest 不覆盖，新的证据路径唯一。

### Task 1：冻结定义、行为 ID、身份和默认值注册表

**Files:**

- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Definitions/NpcDefinition.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Definitions/NpcDefinitionRegistry.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Definitions/NpcBehaviorDefinition.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Definitions/NpcBehaviorRegistry.cs`
- Modify: `src/Terraria.Dome.Simulation/Simulation/NpcHandle.cs`
- Modify: `src/Terraria.Dome.Simulation/Snapshots/NpcSnapshot.cs`
- Test: `Test/Terraria.Dome.Npc.Composition.Verification/Program.cs`

**Steps:**

1. 为 `DefinitionId`、NetId、类别、阵营、基础生命、防御、碰撞盒、行为 ID、掉落表 ID
   编写重复、未知、非法范围和版本不匹配的 RED 用例。
2. 实现 immutable definition 和显式 registry registration；禁止在运行时隐式反射发现类型。
3. 将 `SetDefaults` 中的静态值按行为族拆分；实例 mutable state 不进入 definition。
4. 将未知 NetId/BehaviorId 变成 typed rejection，拒绝创建实体且 snapshot/revision 不变。
5. 为每个首批 definition 保存 source anchor 和 default-value evidence。

**验收：** 未注册或不合法 NPC 永远不能进入实体 store；definition、behavior 和 loot registry
可以独立版本化、持久化和重放。

### Task 2：生成资格、预算和实体提交

**目的：** 将自然生成、事件生成、雕像/释放生成和 Boss 入口统一为受验证的 Spawn command，
但不把事件随机状态伪装成 NPC 局部状态。

**Files:**

- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Commands/SpawnNpcCommand.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Commands/DespawnNpcCommand.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Components/NpcSpawnStateComponent.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Systems/NpcSpawnEligibilitySystem.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Systems/NpcSpawnCommitSystem.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Snapshots/NpcSpawnSnapshot.cs`
- Modify: `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`
- Test: `Test/Terraria.Dome.Npc.Boundary.Verification/Program.cs`
- Test: `Test/Terraria.Dome.Npc.Verification/Program.cs`

**Steps:**

1. 写 spawn bounds、玩家距离、可见/不可见区域、预算、重复 sequence、unknown definition、
   非法 source 和 ID overflow 的失败用例。
2. 将自然生成、事件生成、雕像生成、Boss 生成分别建模为 `SpawnSource`，不能让网络输入
   直接创建 NPC。
3. 实现资格 snapshot -> `SpawnNpcCommand`；资格系统不做结构 mutation。
4. 实现 commit：验证 registry、预算、空间、identity、owner/release source 后再创建实体。
5. 记录生成拒绝 reason、预算 revision 和 deterministic random stream state。
6. 验证同一输入 trace 不会重复生成，拒绝命令不改变实体、预算或 world progression。

**验收：** Spawn command 可回放；自然生成与事件生成有清晰 owner；Boss/invasion 只提供
事件级 spawn request，不把全局事件字段挂在 NPC 实体。

### Task 3：目标选择和目标路由

**目的：** 补齐 `TargetClosest`、目标有效性、玩家死亡/断线、PVS 和 tie-break 规则。

**Files:**

- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Components/NpcTargetComponent.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Systems/NpcTargetSelectionSystem.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Systems/NpcTargetRoutingSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/Npc/Systems/NpcBehaviorSystem.cs`
- Test: `Test/Terraria.Dome.Npc.Boundary.Verification/Program.cs`
- Test: `Test/Terraria.Dome.Npc.Verification/Program.cs`

**Steps:**

1. 写最近有效玩家、相同距离稳定 handle tie-break、死亡玩家、断线玩家、越界位置、隐身/无
   目标和目标已被销毁的 RED 用例。
2. 以 immutable `PlayerSnapshot`/`NpcSnapshot` 为输入，实现目标候选过滤和稳定排序。
3. 将目标选择和行为使用分开；目标系统只更新 target component/command，不改变 NPC 位置。
4. 目标失效时产生显式 clear/reselect 结果，不复用旧 target 数字槽位。
5. 记录目标变更 revision，确保复制只发布 authoritative target projection。

**验收：** 目标选择不读 `Main.player`；等价输入得到同一目标 handle；无效目标不会继续
驱动 AI 或造成伤害。

### Task 4：typed AI 行为注册和普通行为族

**目的：** 建立不依赖 `ai[]` 的行为分派，并先完成普通敌怪、城镇行走和基础逃逸行为。

**Files:**

- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Components/NpcBehaviorStateComponent.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Systems/NpcBehaviorSystem.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Systems/NpcMovementIntentSystem.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Behaviors/ChaseBehavior.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Behaviors/TownHomeBehavior.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Behaviors/DespawnBehavior.cs`
- Test: `Test/Terraria.Dome.Npc.Composition.Verification/Program.cs`
- Test: `Test/Terraria.Dome.Npc.Verification/Program.cs`

**Steps:**

1. 为 typed behavior state 写阶段、计时器、方向、冷却、目标失效和不支持行为的 RED 用例。
2. 实现 `BehaviorId -> behavior handler` 显式注册；不允许 switch/反射覆盖未注册行为。
3. 实现普通 chase：基于目标 snapshot 产生 movement intent，不直接写 Transform。
4. 实现城镇 home skeleton：家位置、昼夜选择、无家状态和最小移动，不实现商店/对话。
5. 实现行为不支持时的 fail-closed/despawn policy，并记录 reason。
6. 运行等价输入重放，比较 behavior state、movement intent 和目标距离。

**验收：** NPC 运行时不含 `float[] ai` authority；行为状态可持久化和恢复；普通 chase
通过不增加其他 AI family 的完成计数。

### Task 5：AI 行为族批量迁移

**目的：** 按 source-backed family 而不是按 `AI()` 巨型方法整体迁移特殊 AI。

**Files:**

- Modify: `docs/research/2026-08-24-npc-behavior-family-matrix.csv`
- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Behaviors/<BehaviorFamily>.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Systems/<BehaviorFamily>System.cs`
- Test: `Test/Terraria.Dome.Npc.Verification/Program.cs`
- Evidence: `Build/diagnostics/npc-complete/task-5-ai-families/<family>/<timestamp>/`

**行为族顺序：**

1. 普通地面追逐/跳跃。
2. 飞行/悬浮/水中移动。
3. 远程攻击和冷却控制。
4. 逃逸、时间耗尽和远离玩家 despawn。
5. 城镇/被动/交互行为。
6. 特殊精英和 Boss phase。
7. Worm/segment follow-up。

**每个行为族的步骤：**

1. 从 Legacy anchor 提取输入、状态更新、随机调用和副作用清单。
2. 写行为族最小 RED fixture，覆盖 valid、invalid、边界、目标变化和 restart。
3. 定义 typed state 和 output command/event；不能把旧局部字段原样复制为数组。
4. 实现系统并把 system order 注册到 tick schedule。
5. 运行 focused verifier、NPC composition verifier 和受影响的 combat loopback。
6. 更新 family matrix；没有完整证据的族保持 `Partial`。

**验收：** 每个 family 都有单独的 source/authority/behavior/evolution 证据；一个 family
的随机流、cooldown 或状态字段不能污染另一个 family。

### Task 6：运动、碰撞、接触伤害和战斗输入

**目的：** 将 AI movement intent 接入共享 Physics/Combat，同时保持 NPC 不直接修改玩家或
其他实体生命。

**Files:**

- Modify: `src/Terraria.Dome.Simulation/Npc/Systems/NpcMovementIntentSystem.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Systems/NpcContactEffectSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/Physics/Systems/TileCollisionSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/Combat/Systems/DamageResolutionSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/Combat/Commands/DamageNpcCommand.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/Combat/Commands/DamagePlayerCommand.cs`
- Test: `Test/Terraria.Dome.Combat.Verification/Program.cs`
- Test: `Test/Terraria.Dome.Npc.Verification/Program.cs`

**Steps:**

1. 写 solid tile、slope/platform、非有限位置/速度、碰撞盒、接触冷却和伤害上限的 RED 用例。
2. 实现 movement intent -> shared movement/collision -> committed transform 结果。
3. 接触效果只发出 damage command，不能在 NPC query 中扣除 Player health。
4. 将 hostile/friendly/PvP 阵营、免疫 source 和重复接触处理纳入 Combat authority。
5. 验证 rejected damage 不变更 NPC、Player、免疫 timer 或 revision。
6. 对普通 chase、飞行和 Boss movement 分别记录碰撞结果。

**验收：** 运动顺序固定；碰撞和伤害都经过共享命令/提交边界；NPC 不持有 Player/NPC
对象引用作为可变 authority。

### Task 7：生命周期、死亡、Worm segment 和重生/销毁

**目的：** 关闭 `CheckActive`/`checkDead`、timeLeft、父子段、根实体和死亡提交的顺序缺口。

**Files:**

- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Components/NpcLifecycleComponent.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Components/NpcSegmentComponent.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Systems/NpcLifecycleSystem.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Systems/NpcSegmentLifecycleSystem.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Systems/NpcDeathSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/Npc/Commands/DespawnNpcCommand.cs`
- Test: `Test/Terraria.Dome.Npc.Composition.Verification/Program.cs`
- Test: `Test/Terraria.Dome.Npc.Verification/Program.cs`
- Evidence: `Build/diagnostics/npc-complete/task-7-lifecycle/<timestamp>/`

**Steps:**

1. 写 active/timeLeft/invalid transform/owner disconnect/expected revision 的失败用例。
2. 写根 NPC、父段、子段、断链、自链、非 worm 类型和根死亡的关系 RED 用例。
3. 实现 lifecycle transition，先发布死亡事实，再按确定顺序提交 loot、segment 和 despawn。
4. 实现 segment follow-up：身份、父子关系、段序号、共享生命/独立生命策略显式定义。
5. 验证死亡不会重复掉落、重复发布 tombstone 或遗留活跃子段。
6. 从持久化 snapshot 恢复活跃 NPC、死亡 pending NPC 和 segment graph，比较 revision。

**验收：** 死亡/销毁是幂等的；断线、越界和 invalid target 的 despawn reason 可审计；Worm
segment 不通过 `realLife` 数字字段绕过 typed relationship。

### Task 8：掉落规则、世界物品提交和事件事实

**目的：** 将 `NPCLoot` 拆为 deterministic loot definition、roll、drop command 和 commit，
并与 Item/WorldItem owner 对接。

**Files:**

- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Definitions/NpcLootDefinition.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Definitions/NpcLootRegistry.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Systems/NpcLootSystem.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Events/NpcDeathEvent.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/Items/Commands/CreateWorldItemCommand.cs`
- Modify: `src/Terraria.Dome.Simulation/Items/Systems/WorldItemSpawnSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/Snapshots/DomeSimulationSnapshot.cs`
- Test: `Test/Terraria.Dome.Items.Verification/Program.cs`
- Test: `Test/Terraria.Dome.Combat.Loopback.Verification/Program.cs`
- Evidence: `Build/diagnostics/npc-complete/task-8-loot/<timestamp>/`

**Steps:**

1. 写 loot table unknown ID、invalid quantity、random stream、difficulty、player context、
   duplicate death and world-item allocator overflow 的 RED 用例。
2. 定义 loot rule 的输入：NPC definition、death reason、world rules、killer/player context、
   deterministic roll stream；客户端不能成为掉落 authority。
3. 实现 death event -> loot roll -> `CreateWorldItemCommand`，事件只能在 death commit 后发布。
4. 将掉落位置、owner、reservation、stack 和 world-item revision 交给 Item owner。
5. 验证同一 death snapshot 只产生一组掉落；重放和 persistence restore 结果一致。
6. 对 deferred 的 rare drop、NPC table、achievement、client animation、sound 记录明确理由。

**验收：** 掉落不直接修改 Inventory 或 WorldItemStore；NPC/Item 通过 typed command 连接；
掉落事件、世界物品 snapshot 和复制 revision 可追溯。

### Task 9：入侵、Boss、月事件和 NPC 生成联动

**目的：** 处理 NPC 与世界事件的边界：事件状态属于 World resources，NPC 只消费明确的
spawn request/definition，不吞并全局 progression。

**Files:**

- Modify: `src/Terraria.Dome.Simulation/World/WorldProgressionState.cs`
- Modify: `src/Terraria.Dome.Simulation/World/Events/`
- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Systems/NpcEventSpawnSystem.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Definitions/NpcEventSpawnDefinition.cs`
- Modify: `src/Terraria.Dome.Simulation/World/Systems/WorldInvasion*System.cs`
- Test: `Test/Terraria.Dome.WorldRules.Verification/Program.cs`
- Test: `Test/Terraria.Dome.WorldRules.Loopback.Verification/Program.cs`
- Test: `Test/Terraria.Dome.Npc.Verification/Program.cs`

**Steps:**

1. 从 `Main`/`NPC` source anchor 列出 invasion type、wave、progress、Boss phase、event flag
   和 NPC spawn table 的所有者。
2. 为事件未开始、冷却、无有效玩家、预算耗尽、位置非法和 progression mismatch 写 RED 用例。
3. 实现 world event -> bounded spawn request -> NPC spawn eligibility -> commit 链。
4. 将随机事件起始与 NPC table selection 分开；每条随机调用有 event stream 和 cursor。
5. 验证 invasion progress、NPC death、loot 和 event clear 的同 Tick 顺序。
6. 明确未完成的完整 Boss AI、特殊事件表、客户端 warning/chat/presentation，不标记为完整。

**验收：** 事件 resource 不挂在单个 NPC；NPC 死亡可以发布事件事实，但不能直接修改
invasion/world progression；多会话观察者得到一致的事件和 NPC revision。

### Task 10：城镇 NPC、交互和特殊生命周期

**目的：** 在不引入通用 `NpcManager` 或聚合 `InitializeAlmostEverything` 的前提下，按领域
完成城镇、住房、交互、商店和捕捉/释放的可证明子族。

**Files:**

- Modify: `src/Terraria.Dome.Simulation/Npc/Components/NpcHomeComponent.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Systems/NpcHomeSystem.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Systems/NpcInteractionSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/WorldGeneration/*Housing*Query.cs`
- Create/Modify: `src/Terraria.Dome.Simulation/Npc/Definitions/NpcInteractionDefinition.cs`
- Test: `Test/Terraria.Dome.Npc.Composition.Verification/Program.cs`
- Test: `Test/Terraria.Dome.WorldObjects.Verification/Program.cs`

**Steps:**

1. 先为 home position、homeless、昼夜、房屋资格、玩家交互 owner 和断线清理写 RED 用例。
2. 仅实现有 source-backed owner 的 home/interaction 子族；未知商店/对话调用保持 deferred。
3. 将交互输入转换成带 session/player identity 的 command，NPC 只消费已验证 intent。
4. 为 shop、pylon、NPCInteractions、捕捉/释放分别建立 coverage card；不创建总初始化器。
5. 验证 NPC home/interaction snapshot 在保存、恢复和 PVS 复制后稳定。

**验收：** 城镇服务不会伪装成普通 AI；没有 source-backed contract 的服务标记
`deferred`，而不是添加空实现。

### Task 11：NPC snapshot、Persistence 和 V1456 SyncNPC

**目的：** 完成 NPC 状态的持久化和协议投影，证明协议字段来源于权威 snapshot。

**Files:**

- Modify: `src/Terraria.Dome.Simulation/Snapshots/NpcSnapshot.cs`
- Modify: `src/Terraria.Dome.Simulation/Snapshots/NpcReplicationSnapshot.cs`
- Modify: `src/Terraria.Dome.Simulation/Npc/Systems/NpcReplicationSystem.cs`
- Modify: `src/Terraria.Dome.Protocol.V1456/`
- Modify: `src/Terraria.Dome.Transport/`
- Test: `Test/Terraria.Dome.Npc.Protocol.Verification/`
- Test: `Test/Terraria.Dome.Combat.Protocol.Verification/`
- Test: `Test/Terraria.Dome.Combat.Loopback.Verification/`
- Test: `Test/Terraria.Dome.Persistence.Verification/`

**Steps:**

1. 建立 snapshot field matrix：identity、type/netId、position/velocity、target、life、
   direction、AI compatibility projection、spawn source、owner、active、revision。
2. 对每个字段注明 authoritative owner、encoding condition、default、unsupported status。
3. 实现 typed projector；编码器只接收 `NpcSyncPacket` DTO，不访问 Arch World。
4. 写 maximal/optional AI suffix、inactive tombstone、unknown type、owner mismatch、PVS
   visibility 和 malformed frame 的 RED 用例。
5. 验证 persistence restore 后 NPC identity/revision、segment graph、pending death 和 loot
   outcome 不重复。
6. 运行两会话 loopback，确认离开 PVS 产生正确的删除/失活投影，进入 PVS 不重复创建。

**验收：** `SyncNPC` 字节 parity、snapshot parity、PVS 和 persistence 四条证据同时存在；
协议兼容不提升 Simulation 语义完成度。

### Task 12：全量 coverage、依赖、回放和删除评审

**目的：** 形成完整 NPC 迁移门禁，确认剩余缺口是显式延期而不是遗漏。

**Files:**

- Modify: `docs/research/2026-08-24-npc-migration-coverage.md`
- Modify: `docs/research/2026-08-24-npc-behavior-family-matrix.csv`
- Modify: `docs/migrations/main-server-responsibility-ledger.md`
- Modify: `docs/migrations/version4-physical-deletion-ledger.csv`
- Modify: `docs/server-completion/completion-manifest.json`
- Create: `Build/diagnostics/npc-complete/task-12-final-gate/<timestamp>/`

**Steps:**

1. 重新扫描 `NPC.cs` 方法、字段、AI markers、定义表和直接依赖。
2. 扫描 Simulation 对 `Terraria.NPC`、`Main.npc`、`ai[]`、`NewNPC`、`SetDefaults`、
   `NetMessage` 和固定槽位的引用，输出行号。
3. 串行运行 NPC、Combat、Items、WorldRules、Persistence、Protocol、Boundary 和 Loopback
   verifier。
4. 运行 `dotnet build Terraria.Dome.sln -c Release -p:UseSharedCompilation=false`。
5. 重新计算行为族状态；`Partial`、`Deferred`、`ExcludedWithEvidence` 必须有 reason、
   source anchor 和后续 owner。
6. 仅当所有 ServerRelevant NPC responsibility 都有 Complete 或 ReplacedWithEvidence，
   才能提出删除旧 NPC 编译入口的单独评审；本 Task 不直接删除旧源码。

**验收：** final gate 报告包括 source hash、coverage counts、focused/loopback exit code、
forbidden dependency count、persistence/replay fingerprint 和未完成责任清单。

## 5. 验证矩阵和命令

### 5.1 必须保存的证据

| 证据类别 | 内容 |
| --- | --- |
| Source | NPC.cs hash、方法/字段/AI anchor、行为族分类 |
| Definition | registry、默认值、版本、unknown rejection |
| Spawn | eligibility、budget、identity、空间、随机 cursor |
| Behavior | typed state、输入 snapshot、movement/attack output |
| Lifecycle | death/segment/despawn 顺序、幂等和 revision |
| Loot | roll seed、规则、world-item command、drop event |
| Protocol | SyncNPC bytes、optional fields、PVS/tombstone |
| Persistence | snapshot restore、ID/revision、segment graph、loot non-duplication |
| Boundary | forbidden dependency、MainBoundary、project references |
| Build | serial restore/build/test exit codes、warnings/errors |

### 5.2 串行验证命令

从仓库根目录执行，并使用新 evidence 目录；不要复用旧 `--no-build` 输出：

```powershell
dotnet build Terraria.Dome.sln -c Release -p:UseSharedCompilation=false

dotnet run --project Test\Terraria.Dome.Npc.Verification\Terraria.Dome.Npc.Verification.csproj `
  -c Release -p:UseSharedCompilation=false --no-build

dotnet run --project Test\Terraria.Dome.Npc.Boundary.Verification\Terraria.Dome.Npc.Boundary.Verification.csproj `
  -c Release -p:UseSharedCompilation=false --no-build

dotnet run --project Test\Terraria.Dome.Npc.Composition.Verification\Terraria.Dome.Npc.Composition.Verification.csproj `
  -c Release -p:UseSharedCompilation=false --no-build

dotnet run --project Test\Terraria.Dome.Npc.Protocol.Verification\Terraria.Dome.Npc.Protocol.Verification.csproj `
  -c Release -p:UseSharedCompilation=false --no-build

dotnet run --project Test\Terraria.Dome.Combat.Verification\Terraria.Dome.Combat.Verification.csproj `
  -c Release -p:UseSharedCompilation=false --no-build

dotnet run --project Test\Terraria.Dome.Combat.Protocol.Verification\Terraria.Dome.Combat.Protocol.Verification.csproj `
  -c Release -p:UseSharedCompilation=false --no-build

dotnet run --project Test\Terraria.Dome.Combat.Loopback.Verification\Terraria.Dome.Combat.Loopback.Verification.csproj `
  -c Release -p:UseSharedCompilation=false --no-build

dotnet run --project Test\Terraria.Dome.Persistence.Verification\Terraria.Dome.Persistence.Verification.csproj `
  -c Release -p:UseSharedCompilation=false --no-build
```

如果某个 verifier 尚未包含所需行为，先添加 source-backed focused verifier，并在 coverage
中保持该行为 `Partial`；不能因为项目不存在而跳过证据。

## 6. 状态、提交和回滚规则

### 6.1 状态定义

- `Unmapped`：没有 owner 或 source contract。
- `Mapped`：有目标路径，但没有 executable behavior evidence。
- `Partial`：有 bounded state/behavior/verifier，尚未覆盖完整 family。
- `Complete`：满足 source、authority、projection、execution 四列且通过回放/loopback。
- `ReplacedWithEvidence`：由明确的非 NPC owner 取代，例如 WorldEvent 或 Item。
- `Deferred`：需要恢复 source contract、定义表或独立行为族，不允许空实现。
- `ExcludedWithEvidence`：纯客户端/展示职责，注明来源和排除原因。

### 6.2 频繁提交边界

推荐每个 Task 拆成以下提交：

1. `docs:` source/coverage/contract。
2. `test:` RED verifier。
3. `feat:` 最小 authority implementation。
4. `test:` GREEN focused/loopback evidence。
5. `docs:` progress、matrix 和 artifact index。

### 6.3 回滚边界

- 只回滚当前行为族的生产、测试和文档文件。
- 保留失败 evidence，不覆盖历史 baseline。
- 不回滚 WorldGen、Main Tick、Item、Projectile 或 Protocol 方向的并行修改。
- 不使用 `git reset --hard`、宽范围清理或物理删除旧 `NPC.cs`。
- 如果一次行为族导致旧行为 baseline 恶化，恢复到该族前的 verified commit，并把失败
  trace 留在 `Build/diagnostics/npc-complete/`。

## 7. 最终交付物

完整 NPC 迁移的最终交付至少包含：

- `docs/research/2026-08-24-npc-migration-source-manifest.md`
- `docs/research/2026-08-24-npc-migration-coverage.md`
- `docs/research/2026-08-24-npc-behavior-family-matrix.csv`
- 所有 definition/behavior/spawn/lifecycle/loot/replication source anchor
- NPC focused、Combat、Items、Persistence、Protocol 和 Loopback verifier 结果
- `Build/diagnostics/npc-complete/` 下每个 Task 的独立证据目录
- 更新后的 `version4-physical-deletion-ledger.csv` 和 `completion-manifest.json`

最终报告必须分别列出：

1. 已完成的行为族及 source anchor。
2. 仍为 Partial/Deferred/Excluded 的 AI、Boss、入侵、城镇和掉落分支。
3. 普通 NPC、Boss、Worm segment、事件生成和掉落的 replay fingerprint。
4. NPC snapshot、persistence 和 SyncNPC 的字段/字节证据。
5. forbidden dependency、完整构建和全部 verifier 的退出码。

在上述证据全部齐备前，NPC 迁移状态保持 `IN_PROGRESS` 或 `PARTIAL`；首阶段普通追逐、
窄生成、死亡/掉落和分段骨架的通过结果不得被升级为完整 Terraria NPC parity。

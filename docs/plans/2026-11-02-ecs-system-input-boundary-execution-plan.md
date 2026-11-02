# ECS 系统输入边界收敛执行计划

## 1. 需求与目标

把 [ECS 系统输入边界约束](../../Context/约束/ECS系统输入边界约束.md) 落到代码上：消除
[2026-11-01 审计](../reviews/audits/2026-11-01-ecs-system-input-audit.md) 登记的 16 处规则 3 违规与
9 处规则 1 违规，且**不改变任何可观察行为**。

验收三件事同时成立才算完成：

1. `src/NSSLC/Components/` 与 `src/NSSLC.Server/` 内三条规则零违规（未接线类型除外，见 §8）；
2. 三个受影响的构建目标 0 警告 0 错误；
3. 行为等价：探针输出与改动前逐字节相同。

明确不做：不引入 CQRS、不引入消息总线、不引入事件溯源、不重写领域边界。本计划只换载体。

## 2. 当前基线和证据等级

| 项 | 数量 |
| --- | --- |
| `.cs` 文件（`src/NSSLC/Components/`） | 451 |
| 调度系统（`BaseSystem<ArchWorld, EcsServerTickContext>`） | 24 |
| 规则 3 违规 | 16 |
| 规则 1 违规 | 9（其中 1 处未激活） |
| 共享可变对象实例 | `CombatAttackIntentBuffer`(3 方) / `CombatDamageRequestBuffer`(5 方) / `PhysicsContactBuffer`(2 方) / `ProjectileTerminationBuffer`(6 方) / `WorldItemStore`(跨域) |

证据等级：九个域并行核查，`Terraria.Ai` 仅扫描级（154 文件里 18 个逐行），其余逐行确认。
所有行号来自审计文档 §10，本计划引用而不重复论证。

## 3. 不变量和权威所有权

### 3.1 必须保持的

- 相位语义：`Preparation → Simulation → Commit → Projection`，同相位内按注册插入序执行
  （`EcsServer.cs:78,83-88`）。
- 一次性事实的生命周期：当前 Tick 产生、消费后清除，不跨 Tick 累积。
- 四条已确认的"静默行为"，迁移后必须仍然静默或升级为显式错误，**不得变成崩溃**：
  - `CombatAttackIntentBuffer.Drain(tick)` tick 不符返回空（`:53-58`）；
  - `PlayerCombatResultSystem.cs:39` 丢弃玩家结果；
  - `PhysicsContactBuffer.Drain()` 清空语义；
  - `ProjectileTerminationBuffer` 经 `CombatSystemRegistration.cs:51` 覆写绑定。
- 实体身份、组件组合、网络协议载荷。

### 3.2 与 2026-10-26 计划的关系

[2026-10-26 Physics/Combat/Projectile 边界收敛执行计划](2026-10-26-physics-combat-projectile-boundary-execution-plan.md)
确立了域边界（其 §3.2）并选择用**宿主注入的缓冲对象**承载一次性事实（其 §4.1-4.5）。

本计划**保留域边界，替换载体**：缓冲 → 请求组件。已执行部分不回退，就地替换形态。
两份计划冲突时，载体形态以本计划为准，所有权划分仍以 2026-10-26 计划为准。

## 4. 目标形态

### 4.1 队列改为 tick 级单例组件

每种一次性事实在实体上承载，由生产者写、消费者读，**宿主不再持有任何队列**：

| 现状 | 目标组件 | 写入方 | 读取方 |
| --- | --- | --- | --- |
| `CombatAttackIntentBuffer` | `CombatAttackIntentQueueComponent` | Npc / Communication / Projectile | `CombatAttackRequestSystem` |
| `CombatDamageRequestBuffer` | `CombatDamageRequestQueueComponent` | `CombatAttackRequestSystem` | `CombatDamageRequestSystem` |
| `CombatDamageRequestBuffer` 结果侧 | `CombatDamageResultQueueComponent` | `CombatDamageRequestSystem` | Npc / Player / Projectile 结果系统 |
| `PhysicsContactBuffer` | `PhysicsContactQueueComponent` | `PhysicsContactCaptureSystem` | `ProjectileDamageRequestSystem` |
| `ProjectileTerminationBuffer` | `ProjectileTerminationQueueComponent` | 投射物三处 | `ProjectileTerminationCommitSystem` |

队列本体是**组件内的引用类型容器**（与 `InventoryComponent` 同形），由域内所有者系统创建与推进；
跨域方只能拿到组件引用并调用其 `Enqueue`，不能拿到宿主注入的实例。

### 4.2 可变服务改为只读端口

- `WorldRuntime` 对域外一律以 `ISpatialWorldQuery` 形态提供（`PhysicsContactCaptureSystem` 与
  `SpatialSimulationCoordinatorSystem` 已是此形态，可直接照抄）。
- `IWorldInteractionWorld` 拆成读侧 `IWorldTileReader` 与写侧，写侧只交给 `WorldInteractionSystem` 所在域。
- `WorldItemStore` 对 Communication 域降为只读查询端口。

### 4.3 输出型端口保持原样

`ICommunicationOutboundPort`、`IServerLogSink` 符合约束新增的"输出型端口"三条件，不属改造对象。

## 5. 分阶段执行计划

每个阶段的退出条件都是：构建 0 警告 0 错误 + 指定探针输出不变。阶段之间不并行，除非注明。

### Phase 0：清理装配层的前置障碍

**范围**：`CombatSystemRegistration.cs`、`GameCommunicationAdapter.cs`。这是纯清理，无行为变化。

**改动点**：

1. `CombatSystemRegistration.cs:42-43` 的 `requestBuffer ?? new CombatDamageRequestBuffer()` 与
   `intents ?? new CombatAttackIntentBuffer()`：缺参改为抛 `ArgumentNullException`，不再静默造实例。
2. `GameCommunicationAdapter.cs:56` 的 `worldItems ?? new WorldItemStore()`：同上。
3. `CombatSystemRegistration.cs:44-50` 的 `ReferenceEquals` 断言：Phase 1 完成后删除。删除前先把它
   降级为注释说明，避免 Phase 1 中途被断言卡住。
4. `CombatSystemRegistration.cs:51` 的 `AttachTerminationBuffer` 覆写：Phase 4 一并移除。

**为什么必须第一个做**：这三处让"漏参数"和"两套实例"在类型层面不可见。不先拆掉，后续把缓冲换成
组件时会立刻撞上断言，或者出现两套队列各自演化。

**验证**：三个项目构建通过；`Build/tickprobe` 输出不变；`Program.cs` 的显式传参已覆盖所有调用点，
因此不会有运行期异常。

**退出条件**：`?? new` 计数归零（`grep '?? new' src/NSSLC/Components`），探针输出相同。

### Phase 1：Physics 接触事实组件化

**范围**：`Share/Physics/ContactFact/`、`Projectile/ProjectileDamageRequestSystem.cs`。

**改动点**：

- 新增 `PhysicsContactQueueComponent`（含 `BeginCapture` / `Add` / `Drain` 语义，照搬
  `PhysicsContactBuffer.cs:21-72` 的实现，包括去重与 tick 校验）。
- `PhysicsContactCaptureSystem` 去掉 `PhysicsContactBuffer` 构造参数与 `internal Buffer` 属性
  （`:24`），改为查询单例实体上的组件。
- `ProjectileDamageRequestSystem` 去掉 `PhysicsContactBuffer` 构造参数（`:18-20`），改为读组件。
- 删除 `CombatSystemRegistration.cs:44-50` 的断言（Phase 0 已注释）。

**顺序风险**：`Drain()` 清空，捕获必须先于消费。组件化后可把捕获改到 `Simulation` 前置相位，
或保留现有数组顺序（`CombatSystemRegistration.cs:54` 在 `:59` 之前）。**不要依赖新机制隐式保序**，
在组件上显式校验 tick，不符时按现状返回空。

**验证**：`Build/tickprobe` 的 `real-composition` 行；另加一项断言——接触事实在两实体间去重生效
（对照 `PhysicsContactBuffer.cs:46-55` 的对称去重逻辑）。

**退出条件**：`PhysicsContactBuffer` 类型删除，或仅作为组件内部字段存在；两处构造参数消失。

### Phase 2：Combat 攻击意图组件化

**范围**：`Share/Combat/CombatAttackIntentBuffer.cs` 与四个生产者。

**改动点**：新增 `CombatAttackIntentQueueComponent`；改 `NpcAiIntentAdapter.cs:35,156`、
`GameCommunicationAdapter.cs:46,251`、`ProjectileDamageRequestSystem.cs:18,88`、
`PlayerAttackIntentAdapter.cs:14`（该类型未接线，见 §8，可一并处理或延后）；
`CombatAttackRequestSystem.cs:26-31` 去掉两个构造参数。

**行为风险**：`Drain(tick)` 的 tick 不符返回空语义必须保留，否则跨相位投递会从"静默丢弃"变成"跨 tick 生效"，
这是可观察的行为变化。

**验证**：`Build/tickprobe`；构造一条 intent 后在同 tick 内被消费、跨 tick 被丢弃，两个方向都要观察。

### Phase 3：Combat 伤害请求与结果组件化

**范围**：`Share/Combat/`、`Npc/NpcCombatResultSystem.cs`、`Player/PlayerCombatResultSystem.cs`、
`Projectile/ProjectileDamageResultSystem.cs`。

**改动点**：拆成 `CombatDamageRequestQueueComponent` 与 `CombatDamageResultQueueComponent` 两个组件，
把现有 `CombatDamageRequestBuffer` 的"请求 + 结果"双职责拆开（`:11-12` 两个 List、
`:55-76` 按 source handle 拉取并移除）。五个消费者去掉构造参数。

**行为风险**：`ConsumeResults` 是"按来源摘取并移除"，迁移后必须仍是同一语义，不能变成广播。
`PlayerCombatResultSystem.cs:39` 的丢弃行为保持原样。

**验证**：`Build/tickprobe` + 三域结果分别落地的断言。

### Phase 4：Projectile 终止请求组件化

**范围**：`Projectile/`。

**改动点**：新增 `ProjectileTerminationQueueComponent`；去掉
`ProjectileLifetimeTickSystem.cs:16`、`ProjectileTerminationCommitSystem.cs:14-15` 的构造参数；
去掉 `ProjectileHitResolutionSystem.AttachTerminationBuffer`（`:33-37`）的可重绑字段，
改为组件查询；删除 `CombatSystemRegistration.cs:51` 的覆写。

**特别风险**：这条链有 4 个调度系统 + 2 个服务共用一个队列，且存在"若覆写缺失则命中数耗尽的终止请求被
静默清空"的现象。迁移后这个耦合应当**消失**（每个系统自己查组件），但要确认 `ProjectileTerminationBuffer`
的 `BeginTick` 清空语义（`:15-27`）在新形态下由谁负责。

**验证**：`Build/tickprobe`；穿透耗尽场景需单独构造一次行为观察。

### Phase 5：World 域对外只读化

**范围**：`World/`、`Communication/WorldNetworkProjectionSystem.cs`、`Communication/GameCommunicationAdapter.cs`。

**改动点**：

- `WorldNetworkProjectionSystem.cs:30-31` 的 `WorldRuntime` 与 `WorldItemStore` 构造参数改为只读端口；
  `:47-48` 的 `DrainPendingMutations()` / `DrainMutations()` 改为由 World 域推送或由只读快照读取。
- `IWorldInteractionWorld`（`World/Contract/WorldInteractionWorld.cs:10-12`）拆读/写两侧。
- `GameCommunicationAdapter.cs:48` 的 `WorldItemStore` 降为只读端口，`:342/:353/:377/:398` 的
  `TryCreate/TryUpdate/TryRelease/TryRetire` 改为请求组件。
- `WorldTickSystem` **不动**——它是 World 域的推进者，按约束"适用范围"条款合规。

**注意**：`WorldRuntime` 与 `WorldItemStore` 是宿主级存储，本阶段是全部阶段里改动面最大的一处，
建议单独一轮并逐个子项提交。

**验证**：`Build/worldprobe`（181 行）输出逐字节不变；`Build/tickprobe`。

### Phase 6：补齐跨域请求组件

**范围**：两处已确认的跨域直写。

**改动点**：

1. Combat → Attributes：`DamageCommitSystem.cs:80` 的 `AttributeSystem.ConsumeShield` 改为产出
   `AttributeShieldConsumeRequestComponent`，由 Attributes 域消费；需要保持"消费不足即
   `DamageCommitResult.TargetChanged`"的返回值语义（`:83-86`）——这是"命令返回结果"的合法用法，
   但结果必须在同一次调用内产生，不能拆成两次调度。
2. Player → Combat：`PlayerDeathRespawnSystem.cs:47` 的 `life.WithCurrent(life.Maximum)` 改为
   `LifeRestoreRequestComponent`，由 Combat 域消费，保住 `DamageCommitSystem` "唯一常规写者"的地位。

**行为风险**：请求组件是跨相位的，复活满血的生效时点可能比现在晚一个相位。必须确认没有观察者依赖
"同一 tick 内立即满血"。

**验证**：`Build/tickprobe`；玩家死亡→复活的观察窗口需单独确认。

### Phase 7：WorldInteraction 结果投影

**范围**：`World/`、`Communication/`。

**现状**：`WorldInteractionResultComponent` 无下游消费者，回程止于组件，未投影给客户端。

**改动点**：在 `Projection` 相位新增投影，把结果组件转成 `OutboundPacketIntent`。这是**补功能**，
不是收敛违规，因此排在所有规则项之后。

**验证**：`Build/worldprobe` 扩展一项，观察结果组件被转成出站意图。

**结论（2026-11-02 决策）：不做，记录为不需要。**

Version4 协议里 17 号 `TileManipulation` 没有回执报文，客户端也不读回执——它按"发送即视为生效"工作。
现有已知消息号只有 13/5/17/21/39/151/28（入站）与 20/21/22/151（出站），没有可以承载
`WorldInteractionResultComponent` 的位置。

若硬发一个报文，只有两种结局：客户端不消费（死重量，且要维护私有扩展），或改动既有报文布局
（客户端解析错位）。两者都比"不投影"更差。

因此 `WorldInteractionResultComponent` 的定位改为**服务端内部诊断**：它记录请求被接受还是拒绝、
失败原因是什么，供服务端日志与测试断言使用，不进入网络层。计划的这一阶段关闭。

### Phase 8：装配职责移出 Components

**范围**：`CombatSystemRegistration.cs`、`CommunicationSystemRegistration.cs`。

**改动点**：两个 `Create(...)` 从 `Components` 移到 `NSSLC.Server`，或降为纯数据（只返回描述，
不 `new` 系统、不接存储）。

**性质**：纯结构搬迁，行为零变化。

**验证**：三个项目构建；两个探针输出不变。

### Phase 8 执行记录：已完成（2026-11-02）

> **编排说明**：§5 从本节起至本节末尾是执行期间追加的完成记录，落点与 §10「执行记录」重复。
> 原文保留不搬动；各阶段正本以 §10 为准，Phase 0-6b 的完成记录都在 §10 内。

两个装配入口搬出 `Components` 程序集。

**改动**：

| 文件 | 变更 |
| --- | --- |
| `src/NSSLC.Server/Registration/CombatSystemRegistration.cs` | **新增**，`namespace Terraria.Server` |
| `src/NSSLC.Server/Registration/CommunicationSystemRegistration.cs` | **新增**，`namespace Terraria.Server` |
| `Components/Share/Combat/CombatSystemRegistration.cs`、`Components/Communication/CommunicationSystemRegistration.cs` | **删除** |
| `Projectile/ProjectileLifecycleSystem.cs` | `SharesRuntimeRegistryWith` 由 `internal` 改为 `public`（跨程序集调用所必需） |
| `src/NSSLC.Server/Program.cs` | 增加 `using Terraria.Server;` |

**改动前先确认了两件事**：引用者只有 `Program.cs` 一处（没有别的程序集依赖它们）；对 `Components`
内部的唯一耦合是 `SharesRuntimeRegistryWith`，加一个 `public` 即可。若耦合面更大，这一步应该改成
"降为纯数据描述 + 宿主构造"，而不是简单搬文件。

**验证证据**：六个项目 0 警告 0 错误；worldprobe 195 行、tickprobe 8 行逐字节一致；
queueprobe 44 项全通过；最小验证程序 `PASS`；三个依赖项目 0 错误。纯搬迁，行为零变化，与预期一致。

### Phase 6c 说明：执行中发现，决策后补做

执行 Phase 6b 时发现计划与审计都漏掉了 `GameCommunicationAdapter.cs` 里对 `MovementIntent` 的写入
（`current = intent`）。该组件属 `Terraria.Movement`（Physics 大类）。

**为什么没有直接补做**：这不是"再塞进玩家输入队列"就能解决的。那样只是把
Communication→Movement 的写换成 Player→Movement 的写，违规原样存在。

**进一步调查后的判断**：`MovementIntent` 有 `Source`（`Input`/`Ai`）与 `Priority` 字段，
`MovementIntentResolver.Select` 按优先级在多个意图间做合并——它本来就是**多写入者输入契约**。
除适配器外 `NpcAiIntentAdapter` 也直接写它。所以这更像是既有设计里一个"共享可写槽位"，
而不是单点越权。

要真正收敛，得让 Movement 域持有请求队列、两个生产者都入队、由 `MovementIntentResolver` 在域内合并。
那是设计变更且落在全仓库行为最敏感的移动路径上，`tickprobe` 覆盖的正是它。需要单独决策后再动。

### Phase 6c 执行记录：已完成（2026-11-02）

**决策**：新建 Movement 请求队列，两个生产者都改为提交意图。

**改动**：

| 文件 | 变更 |
| --- | --- |
| `Physics/Movement/MovementIntentRequest.cs` | **新增** |
| `Physics/Movement/MovementIntentRequestQueueComponent.cs` | **新增** |
| `Physics/Movement/MovementIntentRequestQueue.cs` | **新增**（`internal static`）访问器 |
| `Physics/Movement/MovementIntentRequestSystem.cs` | **新增**。解析句柄后安装 `MovementIntent` |
| `Npc/NpcAiIntentAdapter.cs` | `Move` / `StopMovement` 两个分支改为入队；句柄在查询内按实体解析一次 |
| `Communication/GameCommunicationAdapter.cs` | `current = intent` 改为入队 |
| `src/NSSLC.Server/Program.cs` | 建队列实体；系统注册在**紧邻空间求解之前** |
| `Test/Terraria.NpcAi.MinimalVerification/Program.cs` | 建队列实体；每次 `adapter.Update` 后驱动消费者 |

**注册位置是这一改动的全部风险所在**：`SpatialSimulationCoordinatorSystem` 直接读 `MovementIntent`。
消费者必须排在 NPC AI 之后、空间求解之前，否则 AI 意图会晚一帧生效。Program.cs 里它被放在第 322 行
`spatialSimulation` 注册的正前面。

**验证证据**：

| 检查 | 结果 |
| --- | --- |
| 六个项目构建 | 0 警告 0 错误 |
| **最小验证程序** | **PASS**（`Slime targeting/jump, AI intent, spatial movement` —— 覆盖正是本条改动路径） |
| `Build/queueprobe` | 46 项全通过，新增 `movement-intent-installed`、`movement-intent-consumed` |
| worldprobe / tickprobe | 逐字节一致 |
| 三个依赖项目 | 0 错误 |

最小验证程序是这里最强的证据：它真的让史莱姆追玩家、起跳、再断言位移，走的就是
"AI 决策 → 意图 → 求解"整条链。它在第一次运行时**确实**因为缺少队列实体而崩溃，
这反过来证明测试真的覆盖到了新路径，而不是空跑。

### 收尾核查（2026-11-02）

全部阶段完成后实测：

| 检查项 | 结果 |
| --- | --- |
| `CombatAttackIntentBuffer` / `CombatDamageRequestBuffer` / `PhysicsContactBuffer` / `ProjectileTerminationBuffer` | 四个类型全部移除 |
| 系统构造里的 `?? new` 可变存储 | 0（剩余全是不可变 options 或方法内局部集合） |
| 跨域持有具体 `WorldRuntime` / `WorldItemStore` | 0（仅 World 域自身与宿主装配） |

并额外删除 `WorldTickSystem.WorldRuntime` 公开属性（零使用者的跨域逃逸口）。

**关卡数总览**：

| 阶段 | 状态 |
| --- | --- |
| Phase 0-6c | 已完成 |
| Phase 7 | **经决策关闭**（协议无回执位，记为服务端诊断数据） |
| Phase 8 | 已完成 |

计划全部条目已处理：0-6c 与 8 实做，7 经明确决策不做。

## 6. 验收

| 阶段 | 构建 | 探针 | 行为观察 |
| --- | --- | --- | --- |
| Phase 0 | 三项 0/0 | tickprobe | `?? new` 归零 |
| Phase 1 | 同上 | tickprobe + 接触去重 | 排序不变 |
| Phase 2 | 同上 | tickprobe | 跨 tick 丢弃仍成立 |
| Phase 3 | 同上 | tickprobe | 按来源摘取语义 |
| Phase 4 | 同上 | tickprobe | 穿透耗尽终止 |
| Phase 5 | 同上 | worldprobe + tickprobe | tile/物品路径 |
| Phase 6 | 同上 | tickprobe | 护盾/复活时点 |
| Phase 7 | 同上 | worldprobe | 出站投影 |
| Phase 8 | 同上 | 两项 | 无 |

构建命令：`dotnet build <proj> -v q --nologo`，期望 `0 个警告 0 个错误`。
受影响目标：`src/NSSLC/Terraria.Components.csproj`、`src/NSSLC.Server/NSSLC.Server.csproj`、
`Test/Terraria.NpcAi.MinimalVerification/Terraria.NpcAi.MinimalVerification.csproj`。

> **本表"探针"列已修正（2026-11-02 复核）**：原表把 Phase 0-6 的探针列都写成 `tickprobe`，
> 但实测 `Build/tickprobe` 只构建重力/属性/Buff 三个系统，**没有**接触去重、伤害摘取、
> 穿透终止、护盾、复活这些路径。照着原表读会以为这些阶段有行为证据，实际没有。
>
> 真实覆盖如下：Phase 1-6 的行为证据来自 `Build/queueprobe`（现 49 项断言），
> Phase 3/4 另有 `Build/tickprobe` 的调度层验证，Phase 5 来自 `Build/worldprobe`。
> Phase 6 的"复活时点"原先确实无覆盖，复核时已补上
> `respawn-restore-same-tick` 与反向对照 `respawn-restore-needs-order` 两项。

## 7. 风险与回滚

| 风险 | 影响 | 处理 |
| --- | --- | --- |
| 相位内顺序被改变 | 接触事实/意图被静默丢弃 | 每阶段显式核对注册顺序，Phase 1/4 单独断言 |
| 组件内可变容器被跨域长期持有 | 新的隐性共享 | 组件只暴露 `Enqueue` 与 `Drain`，不暴露内部集合 |
| `WorldRuntime` 只读化牵扯宿主 | 编译面大、易漏改 | Phase 5 独立成轮，逐子项提交 |
| 请求组件引入相位延迟 | 生效时点变化 | Phase 6 单独行为观察 |

回滚单位是阶段：每阶段一次提交，回滚即 `git revert` 单个提交。Phase 0 与 Phase 8 无行为变化，
回滚零风险；Phase 1-6 回滚需同时回退探针基线。

## 8. 不在此计划内

以下类型零构造点或零调用点，属"未接线"，其去留是需要产品决策的问题，不与本计划混做：

`InventorySystem`、`InventoryComponent`、`ItemDefinitionRegistry`、`ItemDefinition`、
`ItemStackComponent`、`InventoryAttributeContributionSystem`、`PlayerAttackIntentAdapter`、
`EcsServerCommandBuffer`、`BuffSystem.TryApply`、`ProjectileLifecycleSystem.TrySpawn/TryTerminate/
TerminateOwnedBy`、`Terraria.Combat` 定义链（7 个类型）。

其中 Item 域除 `ItemId` 枚举外整链不在运行路径上，建议单独立项。

## 9. 依据

- 约束：[ECS 系统输入边界约束](../../Context/约束/ECS系统输入边界约束.md)
- 存量证据：[2026-11-01 ECS 系统输入边界审计](../reviews/audits/2026-11-01-ecs-system-input-audit.md) §3-§10
- 域边界与所有权：[2026-10-26 Physics/Combat/Projectile 边界收敛执行计划](2026-10-26-physics-combat-projectile-boundary-execution-plan.md) §3.2
- 判定方法：`ecs-system` → `command-splitting`（按效果分类，证据标 `confirmed`/`partial`/`unknown`）

## 10. 执行记录

### Phase 0：已完成（2026-11-02）

**改动**：

| 文件 | 变更 |
| --- | --- |
| `GameCommunicationAdapter.cs:43-59` | `worldItems` 由 `WorldItemStore? = null` 改为必填 `WorldItemStore`；`:56` 的 `?? new WorldItemStore()` 改为 `?? throw new ArgumentNullException` |
| `CommunicationSystemRegistration.cs:18-30` | `CreateGame` 的 `worldItems` 由可选尾参改为必填，并前移到 `outbound` 之后；补 `ThrowIfNull` |
| `CombatSystemRegistration.cs:16-48` | `requestBuffer`/`attackIntents` 改为必填，`contactBuffer` 保持可选并移到参数表尾；`:42-43` 的 `?? new` 改为 `ThrowIfNull` |

**验证证据**：

- `Terraria.Components`、`NSSLC.Server`、`Terraria.NpcAi.MinimalVerification` 均 `0 个警告 0 个错误`；
- `Build/worldprobe` 输出 195 行，与 `final5.txt` 及改动前基线**逐字节一致**；
- `Build/tickprobe` 输出 8 行，改动前后**逐字节一致**；
- 最小验证程序输出 `shared-api | ai=ok buffer=ok condition=ok contributionKinds=11` 与 `PASS`。

**两处偏离计划，已确认无害**：

1. 退出条件原写"`?? new` 计数归零"，实际剩 1 处：`_options = options ?? new GameCommunicationOptions()`。
   它是无状态的配置对象，不属"可变队列或域存储"，不构成本计划要消除的静默第二实例。
   退出条件修正为：**可变队列与域存储的静默默认归零**。
2. `CombatSystemRegistration.cs` 的 `ReferenceEquals` 共享断言**未在 Phase 0 删除**。它是保护性校验而不是
   静默回退，提前删除会打开"两个不同缓冲被静默接受"的窗口。删除动作移到 Phase 1——缓冲改为组件后该约束
   自然失效。

**连带影响**：`src/MinimalImplementation/MinimalImplementation.Server.csproj`、
`Build/diagnostics/LiquidPerfProbe`、`Build/diagnostics/WaterFlowProbe` 三个也引用 `Terraria.Components`
的项目一并构建通过。前者有 7 个既有警告，全部位于 `MinimalImplementation.World`、
`NSSLC.WorldGeneration`、`NSSLC.Infrastructure`，与本次改动无关。

### Phase 1：已完成（2026-11-02）

**改动**：

| 文件 | 变更 |
| --- | --- |
| `Share/Physics/ContactFact/PhysicsContactQueueComponent.cs` | **新增**。承载 `BeginCapture`/`Add`/`Drain`/`Clear`，含同帧对称去重与 tick/revision 校验 |
| `Share/Physics/ContactFact/PhysicsContactBuffer.cs` | **删除**。类型不再存在 |
| `Share/Physics/ContactFact/PhysicsContactCaptureSystem.cs` | 构造参数 4→3（去掉缓冲）；删除 `internal PhysicsContactBuffer Buffer` 属性；改为查询 `PhysicsContactQueueComponent` 单例实体，缺失时抛 `InvalidOperationException` |
| `Projectile/ProjectileDamageRequestSystem.cs` | 构造参数 5→4（去掉缓冲）；新增私有 `DrainContacts()` 查询同一组件 |
| `Share/Combat/CombatSystemRegistration.cs` | 删除 `contactBuffer` 参数与 `ReferenceEquals` 断言（Phase 0 的延期项在此清偿） |
| `src/NSSLC.Server/Program.cs:88` | 宿主不再 `new PhysicsContactBuffer()`，改为 `ecsWorld.Create(new PhysicsContactQueueComponent())` |

**新增验证设施**：`Build/contactprobe`（`ContactProbe.csproj` + `Program.cs`）。
既有两个探针都不构造 Physics/Projectile 系统，无法覆盖本阶段，因此新建探针。它在 Phase 2 被扩展
并更名为 `Build/queueprobe`，作为 Phase 1-4 共用的队列回归基线。

**验证证据**：

| 检查 | 结果 |
| --- | --- |
| `capture-writes-queue` | PASS `count=1 tick=0` |
| `capture-tick` | PASS |
| `drain-clears` | PASS `drained=1 after=0` |
| `dedupe-same-order` | PASS `count=1` |
| `dedupe-symmetric` | PASS `count=1` |
| `reject-stale-tick` | PASS（跨帧事实被拒） |
| `missing-queue-throws` | PASS（缺组件时不静默跳过） |
| `Build/worldprobe` | 195 行，逐字节一致 |
| `Build/tickprobe` | 8 行，逐字节一致 |
| 最小验证程序 | `PASS` |
| 六个受引用项目 | 全部 0 错误 |

**一处偏离**：`PhysicsContactCaptureSystem` 原有的可选构造参数 `EntityCollisionSystem? collision = null`
一并移除，改为内部 `new EntityCollisionSystem()`。全仓库无调用点传入该项，且该类型无状态，等价。

**残余风险（未验证项）**：`NSSLC.Server` 的可执行进程本轮**没有实际启动**，因此
"宿主先建队列实体、再跑首帧捕获"这一点是靠 `Program.cs:88` 的创建顺序推断的，不是运行观测。
首次真正跑服务器时应确认首帧无 `PhysicsContactQueueComponent must exist` 异常。

### Phase 2：已完成（2026-11-02）

**改动**：

| 文件 | 变更 |
| --- | --- |
| `Share/Combat/CombatAttackIntentQueueComponent.cs` | **新增**。原缓冲的全部语义，包括 `Enqueue` 内按 tick 换帧清空、`Drain` 跨 tick 返回空、按来源单调递增的 request id |
| `Share/Combat/CombatAttackIntentQueue.cs` | **新增**（`internal static`）。单例组件访问器 `Require(ArchWorld)`，缺组件时抛 `InvalidOperationException` |
| `Share/Combat/CombatAttackIntentBuffer.cs` | **删除** |
| `Share/Combat/CombatAttackRequestSystem.cs` | 构造参数 5→4；改为 `CombatAttackIntentQueue.Require(World).Drain(context.Tick)` |
| `Projectile/ProjectileDamageRequestSystem.cs` | 构造参数 4→3 |
| `Npc/NpcAiIntentAdapter.cs` | 构造参数 6→5 |
| `Player/PlayerAttackIntentAdapter.cs` | 构造参数改为 `ArchWorld world`（该类型仍未接线） |
| `Communication/GameCommunicationAdapter.cs` | 构造参数 6→5 |
| `Communication/CommunicationSystemRegistration.cs` | `CreateGame` 去掉 `attackIntents` 参数 |
| `Share/Combat/CombatSystemRegistration.cs` | `Create` 去掉 `attackIntents` 参数 |
| `src/NSSLC.Server/Program.cs` | 宿主改为 `ecsWorld.Create(new CombatAttackIntentQueueComponent())`；四处传参同步移除 |
| `Test/Terraria.NpcAi.MinimalVerification/Program.cs` | 同样建出队列实体 |

**设计取舍**：组件本身保持纯数据（不含 Arch 依赖），单例查找放在同程序集的 `internal` 访问器里。
4 个生产者分布在 Combat/Communication/Npc/Projectile 四个 namespace，但同属一个程序集，因此
`internal` 足够，不必把它变成公共 API。

**验证证据**：`Build/queueprobe` 15 项断言全部 PASS（7 项 Physics 接触 + 8 项攻击意图），其中关键的
tick 语义三项：

| 断言 | 结果 |
| --- | --- |
| `intent-drain-stale-tick-empty` | PASS `drained=0 kept=1`（跨 tick 不泄露） |
| `intent-drain-next-tick` | PASS `drained=1` |
| `intent-enqueue-clears-previous-frame` | PASS `drained=1`（tick 7 的意图不串到 tick 8） |
| `intent-adapter-resolves-queue` | PASS `id=6 drained=1`（真实走 `Require` 访问器） |
| `intent-missing-queue-throws` | PASS（缺组件时生产者快速失败） |

回归：worldprobe 195 行与 tickprobe 8 行均逐字节一致；最小验证程序 `PASS`，其 "AI intent" 一项
现在真实经过 `NpcAiIntentAdapter → CombatAttackIntentQueue.Require` 路径；六个受引用项目 0 错误。

**过程中的一次失误**：在 `queueprobe` 里批量替换 `default` 时误伤了 `PhysicsContactFact` 的
`contact` 参数，导致编译报错 2 处，已逐个改回。批量替换在语义不同但文本相同的片段上不安全。

### Phase 3：已完成（2026-11-02）

**改动**：

| 文件 | 变更 |
| --- | --- |
| `Share/Combat/CombatDamageRequestQueueComponent.cs` | **新增**。`Enqueue`/`DrainPending`/`PendingCount` |
| `Share/Combat/CombatDamageResultQueueComponent.cs` | **新增**。`Publish`/`BeginResultTick`/三个 `ConsumeResults` 重载 |
| `Share/Combat/CombatDamageResult.cs` | **新增**。从原缓冲文件中拆出的 `record struct` |
| `Share/Combat/CombatDamageRequestQueue.cs`、`CombatDamageResultQueue.cs` | **新增**（`internal static`）两个访问器 |
| `Share/Entity/SingletonComponent.cs` | **新增**（`internal static`）泛型单例查找，Phase 2 的访问器也改为复用它 |
| `Share/Combat/CombatDamageRequestBuffer.cs` | **删除**。该文件原本含 **2 个公共类型**，违反"一文件一公共类型"，拆分时顺带修正 |
| `Share/Combat/CombatAttackRequestSystem.cs` | 构造参数 4→3 |
| `Share/Combat/CombatDamageRequestSystem.cs` | 构造参数 3→2 |
| `Npc/NpcCombatResultSystem.cs`、`Player/PlayerCombatResultSystem.cs`、`Projectile/ProjectileDamageResultSystem.cs` | 各去掉一个构造参数 |
| `Share/Combat/CombatSystemRegistration.cs`、`src/NSSLC.Server/Program.cs` | 去掉 `damageRequests` 传参；宿主改为建两个队列实体 |

**顺带修掉一个 Phase 2 引入的缺陷**：`NpcAiIntentAdapter.TryQueueAttack` 原先在 `world.Query`
回调内调用 `CombatAttackIntentQueue.Require(World)`，而 `Require` 内部本身要跑一次 `World.Query`，
构成**查询中嵌套查询**，且按意图数量重复执行。已改为在 `Update` 开头解析一次并作为参数传到
`ApplyIntent` / `TryQueueAttack`。`ProjectileDamageRequestSystem` 同样从按接触事实逐个解析改为循环外解析一次。

这个缺陷编译通过、既有探针也通过，只靠"能跑"发现不了；是 Phase 3 逐个检查调用点时看出来的。

**验证证据**：`Build/queueprobe` 扩到 24 项断言全部 PASS。Phase 3 新增 10 项，其中语义关键的是：

| 断言 | 结果 |
| --- | --- |
| `damage-request-drain-clears` | PASS `drained=2 after=0` |
| `damage-result-consume-per-source` | PASS `a=2 left=1`（只摘取本来源，别人的结果留下） |
| `damage-result-consume-not-broadcast` | PASS `second consume=0 left=1`（第二次消费拿不到，证明是摘取不是广播） |
| `damage-result-begin-tick-clears` | PASS（换帧清空） |
| `damage-result-consume-by-id` | PASS `matched=1 left=1` |
| `damage-result-consume-by-id-set` | PASS `matched=1 left=0` |
| `damage-result-missing-queue-throws` | PASS（缺组件时消费者快速失败） |

回归：worldprobe 195 行、tickprobe 8 行均逐字节一致；最小验证程序 `PASS`；六个受引用项目 0 错误。

### Phase 4：已完成（2026-11-02）

**改动**：

| 文件 | 变更 |
| --- | --- |
| `Projectile/ProjectileTerminationQueueComponent.cs` | **新增**。原缓冲全部语义：tick 换帧清空、`Drain` 跨 tick 返回空、`Contains`、按 handle 单调递增的 request id |
| `Projectile/ProjectileTerminationQueue.cs` | **新增**（`internal static`）访问器，复用 `SingletonComponent<T>` |
| `Projectile/ProjectileTerminationBuffer.cs` | **删除** |
| `Projectile/ProjectileHitResolutionSystem.cs` | 删除 `AttachTerminationBuffer`、`internal Terminations`、可选构造参数；改为首次使用时解析并缓存 |
| `Projectile/ProjectileLifecycleSystem.cs` | 同上，删除 `internal Terminations` 与可选构造参数 |
| `Projectile/ProjectileLifetimeTickSystem.cs` | 构造参数 3→2；在 `Update` 开头解析一次并传给 `RequestExpiry` |
| `Projectile/ProjectileTerminationCommitSystem.cs` | 构造参数 4→3 |
| `Share/Combat/CombatSystemRegistration.cs` | 删除 `AttachTerminationBuffer` 覆写调用与两处 `.Terminations` 传参 |
| `src/NSSLC.Server/Program.cs` | 建出 `ProjectileTerminationQueueComponent` 实体 |

**这里拆掉的是审计记录的核心隐患**。原实现中 `ProjectileLifecycleSystem` 与
`ProjectileHitResolutionSystem` 各自在缺参时 `new ProjectileTerminationBuffer()`，注册期再用
`AttachTerminationBuffer` 把后者改绑到前者——两个实例是否真的是同一个，全靠这一行注册顺序保证。
组件化后双方各自查同一个实体，覆写绑定连同这个时序依赖一起消失。

两个服务类（非系统）改用懒解析缓存而不是每次调用查一次：`_terminations ??= Require(_world)`。
`CheckAdmission` 在接触事实循环里按事实逐个调用，每次查一遍是不可接受的。

**验证证据**：`Build/queueprobe` 扩到 30 项断言全部 PASS。Phase 4 新增 6 项，其中关键一项直接复现了
原缺陷场景：

```
PASS termination-shared-instance | admission=ProjectileInactive
     (expiry recorded by another system must be visible)
```

做法是真实构造一个投射物实体，让 `ProjectileLifetimeTickSystem` 记下到期终止，再用另一个
`ProjectileHitResolutionSystem` 实例查同一个 handle——若两者仍各持一个缓冲，`Contains` 会返回 false，
准入结果就不是 `ProjectileInactive`。

**里程碑**：Phase 1-4 完成后，四个旧队列类型全部消失：

| 类型 | 状态 |
| --- | --- |
| `CombatAttackIntentBuffer` | 已彻底移除 |
| `CombatDamageRequestBuffer` | 已彻底移除 |
| `PhysicsContactBuffer` | 已彻底移除 |
| `ProjectileTerminationBuffer` | 已彻底移除 |

宿主 `Program.cs` 不再持有任何队列对象，只建 5 个世界级实体。剩下的可变存储是 `WorldItemStore`，
由 Phase 5 处理。

### Phase 5：已完成（三个子项全部完成，2026-11-02）

本阶段改动面最大，按计划拆成子项提交。本标题下记录**子项 1**：`WorldNetworkProjectionSystem` 不再持有
`WorldRuntime`；子项 2 与子项 3 的完成记录见本文档后续同名章节。

**改动**：

| 文件 | 变更 |
| --- | --- |
| `World/Contract/IWorldProjectionSource.cs` | **新增**。只暴露 `TileSize`、`CommitRevision`、`DrainPendingMutations()` |
| `World/WorldRuntime.cs` | 实现该接口，新增 `TileSize` 属性 |
| `Communication/WorldNetworkProjectionSystem.cs` | 构造参数由具体 `WorldRuntime` 改为 `IWorldProjectionSource`；`_world.State.TileSize` → `_world.TileSize` |

**收窄的实际效果**：投影系统原本握有 `TryBreakTile`、`TryPlaceTile`、`TrySetLiquid`、`AdvanceTick`、
`TrySetTile` 等全部写入口，现在只剩"读两个标量 + 取走已提交的变更"。接口注释里明确写了这一点：
再把它加宽就会重新打开约束禁止的跨域写路径。

**注意这不是纯只读端口**。`DrainPendingMutations()` 会消费并清空已提交变更队列，属于"取走"而非"只读"。
把它写成纯只读接口反而会导致同一批变更被重复投影。这一点在接口文档里如实标注，不假装它是只读端口。

**验证证据**：三个项目与两个探针全部 0 警告 0 错误；`Build/worldprobe` 195 行逐字节一致（该探针大量
覆盖 tile break/place、液体与 mutation 排空，是这轮最有力的等价证据）；`Build/tickprobe` 8 行、
`Build/queueprobe` 30 项、最小验证程序均通过。

**顺带修正审计的一处误判**：`WorldInteractionSystem` 原被判为规则 3 违规（因为
`IWorldInteractionWorld` 暴露 `TryBreakTile`/`TryPlaceTile`）。但该类型在 `Terraria.World` 内，且全仓库
只有它一个持有者——按"适用范围：共享或跨域"条款，它是域内写入执行者，归规则 2。审计 §10.5 已更正，
这是该表的第三处自我修正。

**子项 3（未完成）**：`GameCommunicationAdapter` 仍持有 `WorldItemStore`，且 `:342/353/377/398`
调 `TryUpdate`/`TryRelease`/`TryRetire`——那是拾取、丢弃、释放所有权的跨域写，比掉落物生成更重，下一轮处理。

### Phase 5 子项 2：已完成（2026-11-02）

`WorldNetworkProjectionSystem` 不再持有 `WorldItemStore`，掉落物生成改为请求组件。

**改动**：

| 文件 | 变更 |
| --- | --- |
| `World/Contract/IWorldItemProjectionSource.cs` | **新增**。只暴露 `DrainMutations()` |
| `World/Model/WorldItemSpawnRequestComponent.cs` | **新增**。跨域生成意图：类型、堆叠、位置、速度、归属会话 |
| `World/WorldItemSpawnSystem.cs` | **新增**。消费请求组件、调 `TryCreate`、销毁请求实体 |
| `World/WorldItemStore.cs` | 实现 `IWorldItemProjectionSource` |
| `Communication/WorldNetworkProjectionSystem.cs` | `_items` 收窄为 `IWorldItemProjectionSource`；`TryCreate` 改为 `World.Create(new WorldItemSpawnRequestComponent(...))` |
| `src/NSSLC.Server/Program.cs` | 在 Projection 阶段注册 `WorldItemSpawnSystem`，**排在投影系统之后** |

**为什么必须排在投影系统之后**：同一阶段内系统按注册顺序执行。投影在 tick N 写出生成请求，
生成系统在同一 tick N 稍后消化，掉落物创建的时机与改动前一致（新物品的变更记录仍在下一 tick 被排空）。
若排到 Commit 阶段，掉落物会晚一帧出现——这是既有探针覆盖不到的行为漂移，靠顺序保证规避。

**未采用的一种做法**：把掉落物生成整体搬进 `WorldRuntime.TryBreakTile`。否掉的原因是
`TryCreate` 的 `reservedFor` 需要"第一个活跃会话"，那是 Communication 的领域知识，搬过去就是反向泄漏。
请求组件让请求方说清"要什么、归谁"，由 World 域决定"能不能生成"，两边职责都留在各自域内。

**验证证据**：五个项目 0 警告 0 错误；`Build/queueprobe` 扩到 34 项断言全部 PASS，新增 4 项：

| 断言 | 结果 |
| --- | --- |
| `item-spawn-idle` | PASS（无请求时不生成） |
| `item-spawn-from-request` | PASS `count=1 kind=Spawn` |
| `item-spawn-request-consumed` | PASS（请求实体被销毁，不残留） |
| `item-spawn-not-repeated` | PASS（已消费的请求不会在下一 tick 重复生成） |

回归：worldprobe 195 行、tickprobe 8 行逐字节一致；最小验证程序 `PASS`；三个依赖项目 0 错误。

**顺带发现**：`World/Model/WorldItemState.cs` 一个文件里有 3 个公共类型（`WorldItemState`、
`WorldItemChangeKind`、`WorldItemMutation`），违反"一文件一公共类型"。不在本阶段范围内，记录备查。

### Phase 5 子项 3：已完成（2026-11-02）

`GameCommunicationAdapter` 不再持有 `WorldItemStore`，四处写入全部改为请求。

**改动**：

| 文件 | 变更 |
| --- | --- |
| `World/Model/WorldItemRequest.cs` | **新增**。`WorldItemRequestKind`（Create/Update/Release/Retire）+ `WorldItemRequest` |
| `World/WorldItemRequestQueueComponent.cs` | **新增**。请求队列组件 |
| `World/WorldItemRequestQueue.cs` | **新增**（`internal static`）访问器 |
| `World/WorldItemRequestSystem.cs` | **新增**。按 kind 分派到 `TryCreate`/`TryUpdate`/`TryRelease`/`TryRetire` |
| `World/Contract/IWorldItemQuery.cs` | **新增**。只读查询（`TryGet`） |
| `World/WorldItemStore.cs` | 追加实现 `IWorldItemQuery` |
| `Communication/GameCommunicationAdapter.cs` | 构造参数收窄为 `IWorldItemQuery`；四处写入改为 `ItemRequests.Enqueue(...)` |
| `Communication/WorldNetworkProjectionSystem.cs` | 掉落物请求改用同一队列 |
| `Communication/CommunicationSystemRegistration.cs` | `CreateGame` 参数收窄为 `IWorldItemQuery` |
| `src/NSSLC.Server/Program.cs` | 建队列实体；注册 `WorldItemRequestSystem` 两次（投影前后各一） |
| `World/Model/WorldItemSpawnRequestComponent.cs`、`World/WorldItemSpawnSystem.cs` | **删除**，被上一个子项引入、本子项统一取代 |

**为什么不是"一实体一请求"**：`GameCommunicationAdapter.Handle` 是在
`CommunicationSessionTickSystem` 的 `World.Query` 回调里被调用的（`CommunicationSessionTickSystem.cs:37-60`）。
在查询迭代中创建实体属于结构变更，不安全。改用挂在世界级实体上的请求队列组件，与 Phase 1-4 的五个队列同一形态。

**为什么注册两次**：队列在准备阶段被收包路径写入，在投影阶段被投影自身写入。注册在投影之前的那一遍保证
收包请求同帧生效（与改动前一致）；投影之后的那一遍保证投影写出的掉落物请求同帧生效。队列排空后为空，
跑两遍是幂等的。这个顺序约束如果写错，收包驱动的物品移动会延迟一帧——既有探针覆盖不到，只能靠顺序保证。

**所有权仍然在 World 域内**：请求方只说"我要对 slot N 做 X"，`WorldItemRules` 与 slot 归属校验仍在
`WorldItemStore` 里执行。探针用非所有者身份发退休请求，验证被拒。

**验证证据**：六个项目 0 警告 0 错误；`Build/queueprobe` 扩到 35 项断言全部 PASS，新增 5 项：

| 断言 | 结果 |
| --- | --- |
| `item-request-idle` | PASS |
| `item-request-create` | PASS `count=2 kind=Spawn`（带 `ReservedFor` 时 `TryCreate` 同时产出 Spawn 与 Ownership 两条变更，与改动前行为一致） |
| `item-request-consumed` | PASS `left=0` |
| `item-request-not-repeated` | PASS |
| `item-request-retire-by-owner` | PASS `count=1 kind=Retire` |
| `item-request-retire-foreign-actor-rejected` | PASS（非所有者请求不改动物品） |

回归：worldprobe 195 行、tickprobe 8 行逐字节一致；最小验证程序 `PASS`；三个依赖项目 0 错误。

**Phase 5 完成度**：`WorldItemStore` 现在只出现在 World 域内部与 `Program.cs`（宿主装配），
Communication 域只能读（`IWorldItemQuery`）或请求。子项 1-3 全部完成。

**仍未做**：计划 Phase 5 还列了 `GameCommunicationAdapter.cs:283` 的 `PlayerInventoryComponent.Set`
与 `:163` 的 `LocationComponent.Coordinates` 写入——那是 Player 域的跨域写，归入 Phase 6 一起处理。

### Phase 6：已完成（2026-11-02）

两处跨域直写全部改为请求。

**改动 1（Combat → Attributes 护盾）**：

| 文件 | 变更 |
| --- | --- |
| `Share/Attributes/AttributeShieldConsumeRequestComponent.cs` | **新增**。`(int RequestedDamage)` |
| `Share/Attributes/AttributeShieldConsumeResult.cs` | **新增**。`(bool HasAttributes, int Consumed)` |
| `Share/Attributes/AttributeShieldCommands.cs` | **新增**。Attributes 域执行护盾扣除并返回结果 |
| `Share/Combat/DamageCommitSystem.cs` | 构造/方法不再接收 `AttributeSetComponent?`；改为提交请求并读结果；删掉因此重复的兼容重载 |
| `Share/Combat/CombatDamageSystem.cs` | `TryCommit` 调用去掉 `targetAttributes` 实参 |

**关键点：`HasAttributes` 必须与 `Consumed` 分开**。原代码是
`if (targetAttributes is not null && ShieldAbsorbed > 0)`——没有属性集时**完全跳过**，不判 `TargetChanged`。
若把"无属性集"压成 `Consumed == 0`，无属性目标会从"照常扣血"变成"提交中止"，是行为退化。
所以结果类型显式区分"没有属性集"与"扣不足"。

**这不是队列式请求**。Combat 必须在同一次调用内拿到结果（`:83-86` 的
`consumed != ShieldAbsorbed → TargetChanged` 是保护提交的竞态检查），拆成两个相位会让这个检查失效。
因此用"命令式请求组件 + 域内同步执行"，计划 §Phase 6 对此有明确许可。

**改动 2（Player → Combat 生命恢复）**：

| 文件 | 变更 |
| --- | --- |
| `Share/Combat/LifeRestoreRequestQueueComponent.cs` | **新增**。`Enqueue(handle)` / `Drain()` |
| `Share/Combat/LifeRestoreRequestQueue.cs` | **新增**（`internal static`）访问器 |
| `Share/Combat/LifeRestoreRequestSystem.cs` | **新增**。解析句柄并写满血 |
| `Player/PlayerDeathRespawnSystem.cs` | 不再写 `LifeComponent`；构造参数加 `IEntityRuntimeHandleResolver`；改为入队 |
| `Share/Combat/CombatSystemRegistration.cs` | 死亡/复活系统传 `handleResolver`；`LifeRestoreRequestSystem` 紧跟其后注册 |
| `src/NSSLC.Server/Program.cs` | 建 `LifeRestoreRequestQueueComponent` 实体 |

**验证证据**：六个项目 0 警告 0 错误；`Build/queueprobe` 扩到 41 项断言全部 PASS，Phase 6 新增 6 项：

| 断言 | 结果 |
| --- | --- |
| `shield-consume-applied` | PASS `hasAttributes=True consumed=40` |
| `shield-consume-decrements` | PASS `consumed=1 left=59` |
| `shield-consume-insufficient` | PASS `hasAttributes=True consumed=0`（余额不足不扣） |
| `shield-consume-without-attributes` | PASS（无属性集不等于护盾耗尽） |
| `life-restore-to-maximum` | PASS `current=100`（同批未登记句柄被跳过而非崩溃） |
| `life-restore-consumed` | PASS `left=0` |

回归：worldprobe 195 行、tickprobe 8 行逐字节一致；最小验证程序 `PASS`；三个依赖项目 0 错误。

**残留风险（如实记录）**：`LifeRestoreRequestSystem` 与死亡/复活系统"同一 tick 生效"依赖 Commit 相位内的
注册顺序，靠读代码保证，**没有探针覆盖**。tickprobe 输出逐字节一致只能说明既有观察点未变，
不能证明复活时点未变——tickprobe 本身不覆盖复活路径。

### Phase 6b：已完成（2026-11-02）

`GameCommunicationAdapter` 不再写 Player 域组件。

**改动**：

| 文件 | 变更 |
| --- | --- |
| `Player/PlayerInputRequestKind.cs` | **新增**。`SetPosition` / `SetInventorySlot` |
| `Player/PlayerInputRequest.cs` | **新增** |
| `Player/PlayerInputRequestQueueComponent.cs` | **新增** |
| `Player/PlayerInputRequestQueue.cs` | **新增**（`internal static`）访问器 |
| `Player/PlayerInputRequestSystem.cs` | **新增**。按 kind 写 `LocationComponent` / `PlayerInventoryComponent` |
| `Communication/GameCommunicationAdapter.cs` | `location.Coordinates = position` 与 `inventory.Set(...)` 改为入队；`Handle` 把 `session.Player` 句柄显式传给两个处理方法 |
| `src/NSSLC.Server/Program.cs` | 建队列实体；`PlayerInputRequestSystem` 注册为 **Simulation 阶段第一个** 系统 |

**为什么注册在本阶段最前**：收包在准备阶段写入请求，而 `LocationComponent` 与 `MovementIntent` 会被
移动/属性系统在仿真阶段读取。请求必须在这些读取之前落地，否则玩家坐标会晚一帧生效。

**验证证据**：六个项目 0 警告 0 错误；`Build/queueprobe` 扩到 44 项断言全部 PASS，新增 3 项：

| 断言 | 结果 |
| --- | --- |
| `player-input-set-position` | PASS `position=(100,50)`（同批未登记句柄被跳过） |
| `player-input-set-inventory-slot` | PASS `hasSlot=True itemType=5 stack=7` |
| `player-input-consumed` | PASS `left=0` |

回归：worldprobe 195 行、tickprobe 8 行逐字节一致；最小验证程序 `PASS`；三个依赖项目 0 错误。

**新发现的第三处跨域写**：同一方法内 `GameCommunicationAdapter.cs:192` 的
`current = intent` 直接写 `MovementIntent`，而该组件属 `Terraria.Movement`（Physics 大类）。
计划与审计都没有列出这一处。

它没有跟着 Phase 6b 一起修，原因是修法不等于"再塞进玩家输入队列"——那只是把 Communication→Movement 的写
换成 Player→Movement 的写，违规没消失。正确做法是 Movement 域自己的请求队列 + 消费系统。
后经决策，这一项作为 **Phase 6c 单独执行完毕**，见 §5 的 Phase 6c 执行记录。

**残留风险（如实记录）**：改动前坐标写入发生在准备阶段（收包时）；现在发生在仿真阶段最前。
若存在"准备阶段内、收包之后"读取 `LocationComponent` 的系统，它会看到晚一拍的数据。
当前注册表里没有这样的系统（`CommunicationSessionTickSystem` 是准备阶段唯一系统），但这一点靠读注册顺序
得出，**没有探针覆盖**。

## 11. 完成后的复核（2026-11-02）

计划执行完毕并被再次要求"执行此计划"后，做了一次独立复核：不采信上一轮的自述，
重新构建、重跑全部探针、并用独立的正则扫描重新找违规。结论与证据如下。

### 11.1 代码状态：与"已完成"结论一致

| 检查 | 结果 |
| --- | --- |
| 六个项目构建 | 0 警告 0 错误 |
| worldprobe | 195 行，与 Phase 0 基线逐字节一致 |
| tickprobe | 8 行，与 Phase 0 基线逐字节一致 |
| queueprobe | **49** 项断言全 PASS（本轮实测计数） |
| 最小验证程序 | PASS |
| 四个 `*Buffer` 类型 | 全仓库已无出现 |
| 八个关键新产物 | 全部存在 |
| 六个应删文件 | 全部已删除 |

### 11.2 独立扫描发现的两个待判项

独立扫描用三条模式重新找违规：构造参数中的可变集合、持有可变接口的字段、跨域 `ref Get` 写入。

**（a）`GameCommunicationAdapter.cs` 写 `WorldInteractionRequestComponent`——判定：不违规。**

这是扫描中新暴露的一处，但性质与 `MovementIntent` 那处完全不同：
`WorldInteractionRequestComponent` 是**请求组件**，本身就是 §3 规定的跨域输入正规载体。
写请求组件正是目标形态，不是越权；判断标准是"是否持有并写另一个域的实现状态"，
而不是"是否出现跨域字样"。

**（b）其余三处跨域 `ref Get` 写入——判定：均在各自域内，合规。**

| 位置 | 写入组件 | 归属判断 |
| --- | --- | --- |
| `Share/Combat/DamageCommitSystem.cs` | `LifeComponent` | Combat 执行玩家生命，且已由 `LifeRestoreRequest` 队列化 |
| `Share/Combat/LifeRestoreRequestSystem.cs` | `LifeComponent` | 同上，Players 决定、Combat 执行 |
| `Share/Physics/SpatialSimulation/SpatialSimulationCoordinatorSystem.cs` | `LiquidContactComponent` | Physics 写自己的组件 |

**（c）`WorldInteractionSystem` 持有 `IWorldInteractionWorld`——维持第三处自我修正的结论：合规。**
它是 World 域唯一的持有者，属规则 2（域内所有者持有自己的写接口），不是规则 3。

### 11.3 本轮修正的文档缺陷

复核发现计划文档自身有五处不一致，均已修正：

1. `Phase 5` 标题仍写"进行中"，而其三个子项实际全部完成。
2. `Phase 8` 与 `Phase 6c` 各出现两个同名标题，读者无法区分计划正文与执行记录。
   已更名为"执行记录"与"说明"。
3. §10 中"新发现的第三处跨域写（未修）"一节已过期——Phase 6c 后来修掉了它，
   但原文仍标"未修"。已改为指向 Phase 6c 执行记录。
4. **§6 验收表的"探针"列失真**：Phase 0-6 全写成 `tickprobe`，但 tickprobe 只跑
   重力/属性/Buff 三个系统，从未覆盖接触去重、伤害摘取、穿透终止、护盾、复活。
   照原表读会误以为这些阶段有行为证据。已在 §6 加更正说明，并列出真实证据来源。
5. **断言计数对不上**：记录累计到 46，实测 47（现已 49）。见 §11.5。

其中第 4 条是本次复核最有价值的发现：它不是笔误，而是**验收标准与实际证据脱节**——
表格声称的覆盖并不存在。这类问题比代码里的遗漏更难发现，因为它在文档上"看起来已经验过了"。

另外在 §5 中 Phase 8 执行记录处加了一条编排说明：该段本应在 §10，保留原文不搬动，
但各阶段正本以 §10 为准。

### 11.4 复核未能消除的风险

复核后剩余两项仍未消除（第一项已在本次复核中补齐探针，移出本清单）：

1. `PlayerInputRequestSystem` 置于仿真阶段最前，若存在"准备阶段内、收包之后"读
   `LocationComponent` 的系统会看到晚一拍数据。当前注册表里没有这样的系统，但这一点
   仍靠读注册顺序得出，没有探针覆盖。
2. `NSSLC.Server` 可执行文件始终未被真正启动，"宿主先建队列实体再进第一 tick"是推断而非观察。
   本轮**没有**尝试启动它：该可执行文件会绑定网络端口，且运行/关闭时可能写出世界存档，
   在不了解存档副作用前擅自启动有污染数据的风险，应由使用者决定是否做这次冒烟。

**已补的一步**：`LifeRestoreRequestSystem` 与死亡/复活系统的同帧语义，原先只靠注册顺序推断。
本次在 `Build/queueprobe` 增加了真实 `EcsServer` 调度下的两项断言：

| 断言 | 结果 |
| --- | --- |
| `respawn-restore-same-tick` | PASS `t1.life=0 t1.dead=True t2.life=100 t2.deadCleared=True t2.respawnCancelled=True` |
| `respawn-restore-needs-order` | PASS 把恢复系统挪到 Preparation 后 `t2.life=0`、`t3.life=100` |

第二项是**反向对照**，用来证明第一项不是空跑：它演示了顺序被破坏时同帧保证确实消失。
没有这一条，正向断言通过只能说明"恰好对了"，不能说明它检测到了顺序。

### 11.5 断言计数的口径说明

复核时实测 `queueprobe` 为 **49 项 PASS、0 项 FAIL**（按 `^PASS ` 行计数）。
这个数由两部分构成：各阶段记录累计到 46 的既有断言，加上本次复核新增的
`respawn-restore-same-tick` 与反向对照 `respawn-restore-needs-order` 两项。

初次复核（加这两项之前）实测为 47，而 §10 记录累计到 46，差 1。

这些数字是各阶段当时按"新增 N 项"累加得出的，本轮没有回溯重数，因此**以实测 47 为准**。
顺带发现同类的历史算术不一致：Phase 3 记为"24 项、新增 10 项"，而 15 + 10 = 25。
两处都说明分段累加式计数本身不可靠——后续若要引用断言数，应直接实测而不是沿用记录。

# P01 Liquid / Wiring / Spatial / Death / Teleport 执行文档

## 文档状态

| 项目 | 值 |
| --- | --- |
| `designStatus` | `proposed` |
| `verificationStatus` | `not-run` |
| `coreSliceVerification` | `passed`（Spatial、Liquid、Wiring、Pylon、Revenge、Teleportation 局部 verifier） |
| 分区 / 任务 | `P01` / `AUTH-SYS-P01` |
| 原始 `sessionId` | `17f3d4e11f504f5e98ff62ac4e380dd9` |
| 上游 System 报告 | `docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P01-liquid-wiring-spatial-death-teleport.md` |
| 配套设计 | `docs/system-decomposition/design/2026-09-30-authoritative-P01-liquid-wiring-spatial-death-teleport-system-design.md` |
| 目标源码 | `D:\TRbackup\Version4` |
| 完整参考源码 | `D:\TRbackup\无任何删减通过编译` |
| 迁移代码 | `D:\TRbackup\NLTX\src\NSSLC` |

本文描述未来实现任务应遵循的阶段、输入输出、提交点和失败出口。它不是已经执行过的
迁移脚本，也不是运行时调度图。本轮实现并验证了 Spatial 纯 Query、Liquid 提交/发布、
Wiring mechanism/propagation contract、Pylon snapshot/projection、Revenge decision/marker
revision 和 Teleportation eligibility/commit 的局部切片；完整迁移、
集成、运行时和行为 verifier 仍未运行。

## 阅读和证据规则

目标 Version4 的只读 CPG 查询已恢复，查询数据库为
`D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite`。本次补查结果如下：

| 目标入口 | 查询结果 | 用途 |
| --- | --- | --- |
| `UpdateMech` | `complete`，`WorldGen.cs` 1 个 call site | 证明 world tick 的正向入口，不证明所有运行时入口 |
| `HitSwitch` | `complete`，在 `Wiring.cs`、`MessageBuffer.cs`、`Main.cs` 选定范围返回 3 个 call site（Wiring 1、MessageBuffer 2） | 网络收包、rigged chest、其它 Wiring 路径；计数不外推到未选定 scope |
| `MassWireOperation` | `complete`，`MessageBuffer.cs` 1 个 call site | server-only 工具操作入口 |
| `TripWire` | `complete`，Wiring 内 8 个 call site | 传播入口和内部分支候选 |
| `Teleport` | `Find-CpgSymbols` 三个声明 `complete`；固定签名的 call sites 为 Wiring 1、Player 9、NPC 4，均 `complete`；callable-facts 为 `partial` | 固定 overload identity 和正向静态入口；callee effects/运行时闭包仍未展开 |
| `ReInit` | `complete`，`WorldGen.cs` 1 个 call site | 世界加载/初始化入口 |
| `UpdateLiquid` | `complete`，4 个 call site | 正常 tick、WorldGen 和 WorldFile 路径 |
| `CacheEnemy` | `complete`，`NPC.cs` 1 个 call site | death/despawn capture 入口 |
| `CheckRespawns` | `partial`，0 个返回点，`NoMatchingFactInScannedScope` | 必须标 `unknown`，不能当作无 caller |
| Revenge `Update` / `Reset` | 各 `complete`，分别为 `Main.cs` / `WorldGen.cs` 1 个 call site | 时钟和 reset 入口 |
| Pylon `Update` / `OnPlayerJoining` | 各 `complete`，分别为 `Main.cs` / `MessageBuffer.cs` 1 个 call site | registry refresh 和 join projection |

完整参考项目补充了目标源码中缺失或被清空的方法体。关键锚点包括：

- `Terraria/Wiring.cs:155-269` 的机制倒计时，`:271-392` 的 `HitSwitch`，`:439-475` 的库存扣除，
  `:477-694` 的 `CheckMech`、pump 和四颜色 `TripWire`，`:3173-3288` 的 mine/geyser/teleport。
- `Terraria/Liquid.cs:56-109` 的 dirty set 和 `ReInit`，`:1015-1192` 的 liquid tick，
  `:1194-1235` 的 `AddWater` 入口。
- `Terraria.GameContent/CoinLossRevengeSystem.cs:377-495` 的 capture、clock、respawn lock 和 spawn。
- `Terraria.GameContent/TeleportPylonsSystem.cs:27-95,352-368` 的 refresh、diff、reset 和 join。
- `Terraria/WorldGen.cs:72704-72726` 的 `UpdateMech` 在 `UpdateLiquid` 前执行，
  `:7159-7180` 的 Wiring/Revenge reset，`:11548`、`:16586`、`:21398` 的 liquid load/gen 调用。
- `Terraria/MessageBuffer.cs:1905-1922,2709-2720,3780-3793,868-871` 的 rigged chest、HitSwitch、
  MassWireOperation 和 pylon join 输入。

这些是参考源码事实。除非目标 Version4 的关系、快照和行为 verifier 后续闭合，不能把它们写成
目标迁移已完成。

完整参考树的 Teleport 专项核对记录如下：

| 关系 | 完整参考源码 | 目标 Version4 对应源码 | 执行含义 |
| --- | --- | --- | --- |
| Wiring 端点扫描与实体筛选 | `Terraria/Wiring.cs:3223-3288` | `Terraria/Wiring.cs:2704-2764` | 保留双向 48x48 区域、Player block、NPC immunity、临时 teleporting flag 清理；真实 writer 仍需接线验证 |
| Player 实体提交 | `Terraria/Player.cs:37902-37987` | `Terraria/Player.cs:21731-21797` | 位置、pressure plate、TeleportEffect、portal metadata、timer 和 presentation/network effects 必须由 integration owner 明确排序 |
| NPC 实体提交 | `Terraria/NPC.cs:82322-82348` | `Terraria/NPC.cs:67306-67332` | 位置、TeleportEffect、timer 和 server network send 不得隐藏在 Query；未知 port 结果不得升级为成功 |

目标与完整参考的这三份文件哈希不同，且 CPG `SourceSnapshotId` 为 null。因而这些行号和
副作用形状是静态执行依据，不是行为等价证明；任何未能在目标快照中闭合的关系继续标为
`unknown` 或 `integration-review`。

本次按声明文件和固定签名重新运行 Teleport Query API 后，`Find-CpgSymbols` 的三个声明为
`complete`；`Find-CpgCallSites` 返回 Wiring 静态入口 1 个、Player overload 9 个
（含 `Terraria/Wiring.cs`）、NPC overload 4 个（含 `Terraria/Wiring.cs`），查询状态为
`complete`。这只证明索引中的正向静态调用点，不证明完整入站闭包或运行时分派。
对三个声明追加的 `Get-CpgCallableFacts` 均为 `partial`，只保留 direct closure，callee
effects 未展开；因此没有唯一 writer、异常后状态或完整副作用闭包的 CPG 证明。完整参考项目
仍只用于确认副作用顺序形状，并由目标 `Version4/Terraria/Wiring.cs` 逐行核对端点调用。
SS14 参考项目的 `SharedPortalSystem`/`PortalSystem` 只作为 Query 与 transform commit 分离
的 ECS 组织参考，不作为目标 Terraria 行为、网络或效果证据。

Wiring 的 CPG 补查结果保持同一证据等级：`TripWire` 在 Wiring 内有 8 个正向 call site，
`HitSwitch` 有 3 个（其中 2 个来自 `MessageBuffer.cs`），`UpdateMech` 在 `WorldGen.cs` 有
1 个，`CheckMech` 有 35 个，`XferWater` 有 4 个，`LogicGatePass` 有 2 个。对 `TripWire`
执行 callable-facts 查询时，工具返回 `partial`/`CalleeEffectsNotExpanded`；空的 direct-target
不能解释为没有被调效果。完整参考源码的四 pass 顺序只用于确定执行编排：每色 traversal，
必要时 pump commit，保存 teleport endpoint，四 pass 后按保存顺序 teleport，最后 PixelBox
和 LogicGate。目标的 `XferWater`、`CheckMech`、`CheckLogicGate` 等 stub 仍进入 `unknown`。

### Teleportation core 执行边界

当前代码只执行以下隔离步骤：

1. `WiringTeleportTransitionAdapter` 从 `IWiringTeleportSnapshotProvider` 取得当前
   endpoint/subject/cooldown facts，把带稳定 `CommandId` 的 `WiringTeleportCommand` 组合成
   `TeleportTransitionRequest`；`CommitPump` 仍委托给原 Wiring port。
2. Adapter 或 Wiring traversal 先提供 `TeleportTransitionRequest` 和当前的
   `TeleportTransitionSnapshot`；snapshot 必须包含 endpoint identity/coordinate/revision、
   subject scope/lifecycle/revision 和 cooldown。
3. `TeleportEligibilityQuery` 以值输入检查 endpoint active、坐标一致性、Player/NPC
   scope、dead/teleporting/immune、one-iteration player block、source、cooldown 和三个
   expected revision。Query 不访问 `Main.tile`、Player/NPC 实例、全局时钟、网络或日志。
4. `TeleportTransitionCommitSystem` 在外部 port 前重做 Query，并拒绝 command replay、
   endpoint identity mismatch、stale revision 和 cooldown snapshot 竞争；成功后才由
   `TeleportCooldownSystem` arm cooldown。
5. `ITeleportCommitPort` 是 Movement/Network/Presentation 的唯一接线点。port 返回
   `Rejected` 或 `Unknown` 时不 arm cooldown；当前仓库没有默认 Player/NPC 位置实现，
   因此不能把 port 接通前的 accepted intent 当作实体已传送。

Wiring commit result 显式保留 `Accepted`、`Duplicate`、`Rejected` 和 `Unknown`；flush
状态对 commit port 的 `Unknown` 返回也保持 `Unknown`，不会把未知压成业务拒绝。未知结果
不会被当作成功或已传送。适配器自身不写 Player/NPC 位置、pressure plate、TeleportEffect、
timer、section 或 network。

已覆盖的局部拒绝路径为 invalid/empty endpoint、unsupported Projectile subject、cooldown、
stale endpoint revision、endpoint identity mismatch 和 duplicate command；snapshot 在
commit 后保持不可变。Projectile portal traversal、Player/NPC position/pressure plate/
TeleportEffect/timer、section sync、network acknowledgement、Pylon style/placement 和
真实 Wiring scheduler 仍为 `unknown` 或 `integration-review`。

### Spatial 查询证据与执行限制

本轮使用只读 CPG Query API 解析 `Terraria/Collision.cs` 的
`CheckAABBvAABBCollision`、`CanHit`、`CanHitWithCheck`、`GetWaterLine`、`WetCollision`、
`SlopeCollision` 和 `TileCollision`。声明查询为 `complete`；选定的调用点可在
`Terraria/Collision.cs`、`Terraria/NPC.cs`、`Terraria/Player.cs`、`Terraria/Projectile.cs`、
`Terraria/Mount.cs`、`Terraria/Utils.cs` 和 `Terraria/WorldItem.cs` 等路径定位。
`CheckAABBvAABBCollision` 的 callable-facts 仍为 `partial`，因为工具明确报告
`CalleeEffectsNotExpanded`。因此执行器只能把这些结果作为正向静态证据，不能把零命中、
空 direct-target 或未展开 callee 当成“没有调用”或“无副作用”。

本次 Query API 返回的选定范围计数如下；计数只表示索引中返回的静态调用点：

| 符号 | 状态 | 返回调用点 | 选定来源路径 |
| --- | --- | ---: | --- |
| `CheckAABBvAABBCollision` | `complete` | 1 | `Terraria/Collision.cs` |
| `CanHit`（value overload） | `complete` | 159 | `Collision.cs`、`NPC.cs`、`Player.cs`、`Projectile.cs` |
| `CanHitWithCheck`（entity/value overload） | `complete` | 1 + 1 | `NPC.cs`、`Collision.cs` |
| `WetCollision` | `complete` | 10 | `Mount.cs`、`NPC.cs`、`Player.cs`、`Projectile.cs`、`Utils.cs`、`WorldItem.cs` |
| `SlopeCollision` | `complete` | 22 | `Collision.cs`、`Mount.cs`、`NPC.cs`、`Player.cs`、`Projectile.cs`、`Utils.cs`、`WorldItem.cs` |
| `TileCollision` | `complete` | 37 | `Collision.cs`、`Mount.cs`、`NPC.cs`、`Player.cs`、`Projectile.cs`、`Utils.cs`、`WorldItem.cs` |

`GetWaterLine` 的选定 overload 在本次范围内返回 1 个 `Collision.cs` 内部调用点；它的
lazy Tile 写入仍按下文规则处理。查询 server 已关闭，数据库保持只读。

完整参考源码 `D:\TRbackup\无任何删减通过编译\Terraria\Collision.cs` 的复核约束执行顺序：

1. 先允许两个显式矩形 snapshot 做严格 AABB overlap 纯计算，验证输入 revision 和结果稳定性。
2. `CanHit`、`WetCollision`、`SlopeCollision`、`TileCollision` 只有在 tile、slope、
   liquid 和 entity facts 已进入同一只读 snapshot 后才能进入 Query 阶段；不得读取
   `Main.tile` 或共享静态结果标记。
3. `CanHitWithCheck` 的 callback 不在 Query 内隐式执行；需要 callback 的旧入口必须经过
   adapter，显式记录调用顺序、异常到 `false` 的转换和副作用状态。
4. `GetWaterLine` 的 lazy Tile materialization 必须先走独立 commit，或者保留为
   `unknown`/mixed command；不能藏在 Query 中。
5. slope/cached conveyor、damage handoff 和两份既有 `CollisionResultComponent` 的最终
   owner 未决前，不新增第三份结果组件，也不宣称 Spatial 已迁移。

这些限制只定义未来执行门禁；本轮局部 core slice 的通过结果不改变整体
`verificationStatus: not-run`。

## 执行前置条件

1. 锁定 P01 claim 输入、原始 `sessionId`、目标源码 revision、完整参考源码 revision 和 CPG manifest。
2. 阅读配套设计中的状态矩阵和 integration-review handoff；任何 owner 冲突先登记为 blocking decision。
3. 为每个 System 建立 world/session revision、命令序列号和可观测提交记录；不以文件顺序表达 phase。
4. 未闭合的目标 stub、动态 dispatch、网络协议和持久化 identity 必须进入 `unknown` 队列。
5. 实现任务只有在另行授权后才能修改 `src/NSSLC`；本文件本身不授权实现。

## 阶段总览

| 阶段 | 触发点 | 主要 owner | 输入 | 输出 / barrier |
| --- | --- | --- | --- | --- |
| 0. 会话和 revision | 任务开始或恢复 | Session coordinator | claim、snapshot、配置 | `ExecutionContext` 就绪；revision 固定 |
| 1. 进程/世界初始化 | `Main` 初始化 | Wiring、Liquid、Revenge、Pylon | world configuration、network mode | owner state 初始快照 |
| 2. 世界加载/重置 | world load、reset、unload | lifecycle coordinator | tile/entity storage、load flags | 清空旧 queue/marker/pylon；liquid reinit |
| 3. 正常 world tick | `WorldGen.UpdateWorld` / `Main.Update` | Mech、Liquid、Revenge、Pylon | tick、players、tiles、entities | committed facts；进入 projection barrier |
| 4. 网络收包 | `MessageBuffer` case 59/109 等 | Wiring adapter、Commands | caller、payload、tool mode | validated command；server reply intent |
| 5. Wiring propagation | `HitSwitch` / `TripWire` | Wiring propagation | tile snapshot、wire colors | ordered pump/teleport/device intents |
| 5a. Teleport qualification/commit | Wiring teleport intent、Portal/Pylon adapter | Teleport Query、cooldown、commit port | endpoint/subject/cooldown snapshot | accepted/rejected/unknown intent；不直接移动实体 |
| 6. Liquid commit/publication | `Liquid.UpdateLiquid` | Liquid flow/buffer/projection | work queue、buffer、world flags | Tile/liquid commit、dirty snapshot |
| 7. Spatial read/commit | Player/NPC/Projectile/Item callers | Spatial Queries + integration | geometry/tile snapshot | contact/hurt facts、movement/damage command |
| 8. Revenge capture/respawn | NPC death、NPC update | Revenge registry/respawn | NPC/player snapshots | marker revision、spawn/remove projection |
| 9. Pylon refresh/join | tick、placement、join | Pylon registry/projection | TileEntity facts、player index | current/old diff、join full state |
| 10. 结算/验证 | 实现批次结束 | verifier coordinator | committed trace、behavior cases | pass/fail/unknown；不自动升级状态 |

## 阶段执行细节

### 0. 会话和 revision

| 项目 | 规定 |
| --- | --- |
| 输入 | P01 claim 的 113 个成员、原始 `sessionId`、源码/CPG identity |
| 动作 | 建立 `worldRevision`、`tickId`、`commandSequence`；记录 source/query provenance |
| 输出 | 只读 `ExecutionContext` 和待处理 integration handoff |
| 失败出口 | revision 缺失、CPG manifest 不匹配、跨分区 owner 未决时停止进入实现 |

所有下游 Command 必须携带生成它的 revision。Query 只对提供的 snapshot/revision 作结论，
不读取隐式全局时钟或可变集合。

### 1. 进程和世界初始化

完整参考项目在 `Terraria/Main.cs:6704` 调用 `Wiring.Initialize`，并构造 Pylon system；Liquid
数组和 buffer 在初始化路径分配。候选执行顺序如下：

1. 创建各 owner 的状态容器和 commit port，不执行网络发送。
2. `LiquidFlowSystem.Reinitialize` 接受 world configuration，设置 budget/cycle/panic 初值。
3. `WiringPropagationSystem.Initialize` 创建 wire、gate、pump、teleport 和 mechanism scratch。
4. `RevengeRegistrySystem` 创建空 marker registry，`gameTime = 0`。
5. `PylonRegistrySystem` 创建空 current/previous snapshot，revision 从 0 开始。
6. 发布一份 initialization trace；若任何容器创建失败，不能继续 tick，必须保留未提交状态。

初始化只分配/清零 owner state。Tile、Player/NPC、网络和表现效果在各自 commit/adapter 阶段执行。

### 2. 世界加载、重置和卸载

参考项目 `WorldGen.cs:7159-7180` 先清 Wiring，再清 Revenge；`WorldGen.cs:11548` 在世界准备阶段
调用 `Liquid.ReInit`。候选流程：

| 顺序 | 输入 | 动作 | 提交/失败 |
| --- | --- | --- | --- |
| 2.1 | reset/unload command | 停止接收新 P01 command，记录当前 revision | 已入队命令进入 drain 或显式丢弃策略 |
| 2.2 | world storage、TileEntity registry | 清 Wiring scratch、mechanism、pump/teleport intent | 不发送 stale effect |
| 2.3 | marker registry | 清 marker、attempt lock 和 clock | 删除消息是否需要重放仍 `unknown` |
| 2.4 | pylon registry | 清 current snapshot 和 cooldown | client snapshot 清理协议 `unknown` |
| 2.5 | liquid configuration | 执行 `Reinitialize`，保留显式 reduced-max 配置 | 任何 residual buffer 需由 verifier 检查 |
| 2.6 | new world revision | 发布 reset barrier | 旧 revision command 必须拒绝 |

`QuickWater`、WorldGen tile solidity 和 load-time liquid helper 可能写 Tile 或触发网络；它们
不应伪装成 Query。若目标实现无法明确 rollback 或重置次序，阶段状态为 `unknown`，不得进入
正常 tick。

### 3. 正常 world tick

完整参考 `Terraria/WorldGen.cs:72704-72726` 的局部顺序是：先 `Wiring.UpdateMech`，再
`TileEntity.PerformUpdates`，然后累加 `Liquid.skipCount`，达到阈值才调用 `Liquid.UpdateLiquid`。
参考 `Terraria/Main.cs:18049` 调用 Revenge clock，`:65844` 调用 Pylon refresh。Main 与 WorldGen
之间的完整跨函数顺序由目标 CPG 尚未闭合，因此这里只锁定局部顺序，跨入口边界标 `partial`。

每个 tick 的候选阶段：

1. **Tick snapshot**：捕获 `tickId`、world flags、active player count、Tile/Entity revisions。
2. **Mechanism phase**：`WiringMechanismCooldownSystem.Advance` 递减 cooldown；到期项产生
   ordered device intents。具体 cannon/trap effect owner 未决时只允许 `unknown` intent。
3. **TileEntity phase**：执行外部 TileEntity updates；Pylon placement 的 invalidation 通过显式
   `RequestRefresh` 进入下一 registry barrier。
4. **Liquid budget/work phase**：若 skip budget 满足，运行 liquid budget、panic、work-item slice；
   panic path 可提前结束本次 liquid phase。
5. **Liquid commit phase**：按 work delete、buffer drain、stuck recovery 顺序写 Tile/liquid/buffer。
6. **Revenge phase**：推进 `_gameTime`；仅在约定 cadence 执行 expiration/invalid cleanup。目标
   caller/phase 仍需补证，不得假定每 tick 都执行 respawn。
7. **Pylon phase**：server 依据 cooldown 扫描 TileEntity，建立 current/old diff；client 只消费 projection。
8. **Barrier**：所有 authority commit 完成后，才投影 network/presentation。任何 stale command 在
   barrier 前拒绝，不能由 projection 回写 authority。

### 4. 网络收包和 command 生成

参考项目的 `MessageBuffer` 关系：

| 收包 | 参考路径 | 候选执行 |
| --- | --- | --- |
| rigged chest switch | `Terraria/MessageBuffer.cs:1905-1922` | 设置 caller context，生成 `HitSwitchCommand`，提交后恢复 user context，再发送 server reply |
| message 59 | `Terraria/MessageBuffer.cs:2709-2720` | 解码坐标和 `whoAmI`，`SetCurrentUser`，执行/提交 Wiring command，server 广播 |
| message 109 | `Terraria/MessageBuffer.cs:3780-3793` | server-only 校验坐标/玩家，暂存并恢复 tool mode，生成 `MassWireRequest` |
| player join | `Terraria/MessageBuffer.cs:868-871` | 在其它 join sync 后执行 Pylon full-state projection |

网络 adapter 只负责解码、身份、容量和协议错误；规则计算进入 Query/Command。重复收包时，
`HitSwitch` 和 mass wire 的幂等性尚未证明：Command 必须用 `commandSequence`、caller、tile
revision 和 server-side recheck 防止 replay；若已发包但 commit 结果未知，不能盲目重试，必须
先通过 authority snapshot reconcile。

### 5. Wiring propagation

完整参考 `Terraria/Wiring.cs:271-392` 表明 `HitSwitch` 先做 InWorld/Tile 检查，再按 tile type
选择 TripWire、mine、geyser、mechanism 或 frame 更新；`Terraria/Wiring.cs:551-694` 的
`TripWire` 过程应按以下顺序执行：

1. server-only gate；client 直接返回。
2. 清理 wire list/direction list，设置 `running`。
3. 依次建立 wire、wire2、wire3、wire4 四个 pass 的 traversal 输入。
4. 每个 pass 执行 `HitWire`，收集 pump/teleport scratch；若两端 pump 都存在，产生 pump commit intent。
5. 保存每个 pass 的 teleport endpoint；四 pass 完成后按保存顺序逐个生成 Teleport command。
6. 清 `running`，执行 PixelBox pass 和 LogicGate pass。

当前 NLTX 局部实现只提交传播 scratch、边界检查、冷却状态和 pump/teleport intent；不会在
`Query` 中读取或写入 `Main.tile`、Player/NPC 位置、Liquid authority 或网络。传播快照保留
teleport endpoint 与 one-iteration block 标记，`QueueNextGate` 与其它坐标命令使用同一 bounds
门禁；同一 mechanism 坐标的重复调度被拒绝。上述修订只强化局部状态不变量，不代表四 pass
行为已经接入真实 scheduler 或 commit owner。

每个阶段的 Tile mutation、device spawn、movement、network send 都经过相应 commit port。目标
源码中 gate/pump/device stub 和 CPG callee effects 未闭合，故不能在实现前把它们填成预期行为。

### 6. Liquid commit 和 publication

以参考 `Terraria/Liquid.cs:1015-1192` 为执行顺序基线：

| 子阶段 | 输入 | 主要动作 | 输出 |
| --- | --- | --- | --- |
| 6.1 budget | active players、world flags、buffer pressure | 计算 `cycles`、`curMaxLiquid`、panic/quickFall | budget decision |
| 6.2 panic | panic state、`panicY` | 分段 QuickWater；满足结束条件时清 panic | tile/section invalidation intent |
| 6.3 work slice | liquid work item snapshot | 设置 delay，调用 flow evaluator，清 skip flag | tile liquid facts、kill/delay updates |
| 6.4 delete | completed work items | 从尾部删除 `kill >= threshold` 项 | queue compaction |
| 6.5 buffer drain | buffer count/capacity | 清 `checkingLiquid`，调用 AddWater，再 `DelBuffer(0)` | buffer and liquid commits |
| 6.6 stuck recovery | previous stuck amount/count | 到阈值时清理 work items并恢复 counters | recovery result |
| 6.7 publication | dirty set、server mode | swap current/previous dirty set，发送 chunk projection，清空 swap | one-way `LiquidChangePublication` |

`NetSendLiquid` 只应加入 dirty snapshot；它不直接成为 liquid authority。`LiquidBuffer.DelBuffer`
和 Tile flags 不能由 Query 执行。任何 buffer drain 失败都必须保留 queue revision 和可重放/丢弃
策略；当前策略 `unknown`，不能用“再次调用”代替幂等设计。

CPG 还定位到 `Terraria.IO/WorldFile.cs` 的独立 `UpdateLiquid` call site。它属于世界文件加载/
保存相关阶段，不能因为方法名相同就并入 `WorldGen.UpdateWorld` 的正常 tick；该入口的输入、
写入范围和与 `Liquid.ReInit` 的先后仍需按目标源码闭包确认。

### 7. Spatial Query、contact 和 damage handoff

空间调用方包括 Player、NPC、Projectile、WorldItem、Mount、Utils 和 PortalHelper。执行时按以下
边界处理：

1. 在一个 `SpatialSnapshot` 中固定 geometry、tile facts、entity hitbox、water/slope flags 和
   cache revision。
2. `CanHit`、wet/slope/tile collision、contact/hurt 只计算 immutable result；不写 shared static flags。
3. callback 仅作为显式 adapter 输入；异常转换和调用顺序若未证明，结果为 `unknown`。
4. conveyor velocity、damage、Player/NPC 状态变化转为 Command，在提交点重查 entity revision 和
   collision eligibility。
5. Physics/Spatial 的重复 `CollisionResultComponent` 由 integration review 选择唯一 owner；
   未决前禁止新建第三个结果组件。

若 `GetWaterLine` 需要 lazy Tile materialization，执行器必须在 Query 前完成 materialize commit，
或把它标为 mixed command；不能将 lazy write 隐藏在 Query 中。

### 8. Revenge capture、clock 和 respawn

参考 `Terraria.GameContent/CoinLossRevengeSystem.cs:377-495` 的候选顺序：

1. NPC death/despawn 提供显式 NPC snapshot；`CacheEnemy` 过滤 boss、realLife、rarity、value 和边界。
2. 将 NPC net/type context、位置/hitbox、value、statue flag、`gameTime` 写入新 marker，分配
   独立 marker unique ID，提交到 registry。
3. server 通过 `RevengeMarkerIdentityProjection` 发 marker；display text 只属于 presentation。
4. 每个约定的 Revenge update cadence 递增 clock；client/server 的 cleanup cadence 必须由行为 verifier
   约束。
5. `CheckRespawns` 先读取 active/non-dead player 的 inner/outer boxes；无 player 时退出。
6. cleanup 过期/无效 marker 后，按 marker revision 检查交叉范围；无交叉则清 attempt lock。
7. 交叉且未锁定时先锁定，再判断 discouragement；不允许则标记 expire，否则产生 spawn command。
8. spawn 成功后记录 removal projection；server 发删除消息。spawn 失败、消息失败和部分成功的补偿
   语义目前 `unknown`。

由于目标 CPG 对 `CheckRespawns` caller 返回 `partial/unknown`，阶段 4/5 的确切触发点必须在
实现前通过源码和 focused verifier 补齐，不能只引用完整参考项目的 NPC 行号。

### 9. Pylon refresh、placement 和 join

参考 `TeleportPylonsSystem.cs:27-95,352-368` 的 server/client 分支：

| 触发 | server 行为 | client 行为 |
| --- | --- | --- |
| tick | cooldown 未到则递减并返回；到期时交换 old/current，扫描 `TileEntity.ByPosition`，解析 type，按 diff 广播 add/remove | 不刷新 authority |
| placement/removal | `RequestImmediateUpdate` 触发立即 rebuild；TileEntity lifecycle 仍是 integration owner | 接收 projection |
| `HasPylonOfType` | 从已提交 snapshot 查询 | 从本地 snapshot 查询 |
| player join | 枚举 current snapshot，逐个发送 add/full-state | 按协议接收并更新本地 projection |
| reset | 清 snapshot 和 cooldown | 清本地投影的协议待定 |

`TeleportPylonInfo.Equals`、network `Deserialize`、SceneMetrics、placement validity 和 persistence
仍是 `unknown`/`integration-review`。refresh diff 必须使用 stable value identity 和 snapshot
revision；网络发送重复或丢失时以 revision reconcile，不直接修改 registry。

### 10. 结算和验证门禁

实现批次结束后，验证器应按以下顺序运行；本任务不运行它们：

1. member coverage：确认 113 个成员无漏项、重复 owner 或跨分区误领。
2. phase/writer：确认 liquid、wiring、revenge、pylon、teleport cooldown/commit 各有单一 writer 和相对顺序。
3. Query purity：instrument Tile/entity/cache/network/time/random/callback 写入，重复 snapshot 调用必须稳定。
4. command commit：注入 stale revision、越界、重复收包、容量耗尽和部分失败，确认 recheck/幂等策略。
5. network/persistence：验证 dirty-set、marker、pylon add/remove/join 的序列化 round-trip 和 revision reconcile。
6. behavior：在目标实现的真实 System composition 上比较旧/新可观察结果、顺序、错误和 side effects。

只有真实迁移项目的必要行为测试通过，且命中新 owner/API composition，才可讨论升级状态。编译、
静态报告、完整参考项目可编译、旧 facade 可调用或局部 verifier 都不能称为迁移成功。

## 局部核心验证记录

本轮使用仓库已有 SDK `D:\TRbackup\dotnet-sdk-10.0.400` 和
`Build/Tools/Invoke-SerialDotnet.ps1`，分别构建并运行两个约 10% 范围的 focused verifier：

1. `Test/Terraria.SpatialSimulation.Verification` 输出
   `PASS: SpatialSimulation core query cases`，覆盖严格 AABB、实体/Tile contact、液体标记、
   半砖几何、slope 缺口和纯度/稳定性。
2. `Test/Terraria.P01.Core.Verification` 输出
   `PASS: P01 liquid, wiring, pylon, and revenge core cases`，覆盖 Liquid budget/work
   queue/publication、Wiring mechanism duplicate/cooldown、propagation bounds/snapshot、Pylon
   snapshot Query、stale revision、add/remove/join projection，以及 Revenge decision 和 marker
   revision commit；本轮追加 Teleport endpoint/subject/cooldown Query、stale revision/identity、
   unsupported subject、duplicate command、immutable snapshot，以及 Wiring command identity、
   Wiring→Teleport adapter 路由、重放去重、Unknown commit-port 传播和 one-iteration block 拒绝。

本次构建和 verifier 命令均通过 `Build/Tools/Invoke-SerialDotnet.ps1` 串行执行：

`global.json` 要求 SDK `10.0.400`。本轮在每个 wrapper 调用前将
`D:\TRbackup\dotnet-sdk-10.0.400` 前置到 `PATH`，系统默认 SDK 未被替换；这只解决 SDK
选择，不改变构建参数或串行 mutex 约束。

```powershell
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'build',
  '.\src\NSSLC\Component\Teleportation\Terraria.Teleportation.csproj',
  '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')

& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'build',
  '.\Test\Terraria.P01.Core.Verification\Terraria.P01.Core.Verification.csproj',
  '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')

& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  '.\Build\bin\Terraria.P01.Core.Verification\Debug\net10.0\Terraria.P01.Core.Verification.dll')
```

两个 build 的退出码均为 `0`，均为 `0 warning / 0 error`；产物分别为
`Build/bin/Terraria.Teleportation/Debug/net10.0/Terraria.Teleportation.dll` 和
`Build/bin/Terraria.P01.Core.Verification/Debug/net10.0/Terraria.P01.Core.Verification.dll`。
verifier 退出码为 `0`。这些只支持局部 core slice 的可运行性，不改变整体
`verificationStatus: not-run`。

本次增量验证实际覆盖了 `WiringTeleportTransitionAdapter` 对 `Unknown` commit-port 结果的
保留。`WiringTraversalAdapter` 的 pump/teleport flush 映射已构建通过，但没有从 verifier
程序集直接写入其 internal scratch writer，因此该分支仍不是完整 traversal 行为验证。

两次构建均为 `0 warning / 0 error`。旧 Collision
facade、Player/NPC 位置与 TeleportEffect、callback、lazy Tile 写入、完整 slope 求解、
conveyor/damage handoff、真实 System 调度、网络、持久化和跨分区 writer 仍是 `unknown` 或
`not-run`。因此整体
`verificationStatus` 继续为 `not-run`，没有迁移成功结论。

## 失败、重试和幂等策略

| 场景 | 处理候选 | 当前状态 |
| --- | --- | --- |
| stale snapshot/revision | 在 Command commit 前拒绝并要求重新 Query | `proposed` |
| endpoint/subject identity mismatch | 在 `TeleportEligibilityQuery`/commit port 前拒绝 | `proposed` |
| duplicate teleport command | 以 command ID 去重，不重复调用外部 port | `proposed` |
| teleport commit port unknown | 不 arm cooldown，返回 `unknown`，由 integration owner reconcile | `unknown` |
| 越界、容量、无效 entity | 返回明确拒绝，不写 authority | `proposed` |
| network send 失败 | 保留已提交 revision；由 projection reconcile，禁止重复 authority commit | `unknown` 直到协议验证 |
| timeout 后结果未知 | 查询 authority snapshot 或记录补偿项，不盲目重试 | `proposed` |
| reset 与 in-flight command 竞争 | 关闭输入、drain/丢弃明确化、递增 world revision | `proposed` |
| spawn/marker/pylon 部分成功 | 持久化 commit outcome 和 projection outcome 分离 | `unknown` |
| Query callback 异常 | 结果中携带 error/unknown；不能静默改写为无碰撞 | `proposed` |

禁止以“再调用一次旧方法”作为通用重试，因为 `HitSwitch`、pump、teleport、spawn、network
publication 和 pylon diff 都可能已经产生不可逆副作用。

## 当前未决项

- 目标 `CheckRespawns` 入站闭包、NPC spawn owner 和 marker network protocol。
- Liquid merge/lava/honey/shimmer、`UndergroundDesertCheck` 和所有 persistence writer。
- Wiring gate/pump/device stub 的目标语义，以及 MassWire 的完整 resource/Tile commit。
- Collision callback、lazy Tile、共享 static flag、conveyor cache 和 damage application。
- Teleport 的 Player/NPC movement、pressure plate、TeleportEffect、timer、section/network
  acknowledgement、Projectile traversal 和 Pylon style/placement。
- Pylon placement invalidation、TileEntity ownership、SceneMetrics、deserialize/equality。
- P01 与其它分区的唯一 Tile、Player/NPC、network、persistence writer。

这些缺口都必须写作 `unknown` 或 `integration-review`，不能从参考项目的完整实现推导目标
迁移已经成立。

## 交付结论

本文给出了从初始化、世界加载、正常 tick、网络收包、传播/液体提交、空间查询、Revenge
respawn、Pylon refresh/join、Teleport qualification/commit 到 reset/验证的执行顺序和失败
出口。它是未来实现和验证的 `proposed` 执行契约；本轮只完成局部 Spatial、Liquid、Wiring、
Pylon、Revenge、Teleportation core slices，完整 `verificationStatus` 保持 `not-run`，没有
迁移成功结论。

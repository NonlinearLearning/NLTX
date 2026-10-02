# P07 玩家移动 System 实施执行计划

| 字段 | 值 |
|---|---|
| 文档 ID | DOC-2026-09-30-P07-player-mobility-execution-plan |
| 逻辑域 | plans / system-decomposition |
| 产物类型 | plan |
| 文档状态 | active |
| 设计状态 | proposed |
| 迁移状态 | deferred |
| 验证状态 | not-run |
| 范围 | 权威分区 P07 的后续实施批次；本批次新增 isolated-core helper，不切换旧入口 |
| 目标代码目录 | src/NSSLC |
| 设计基线 | [P07 玩家移动 System 拆分设计](2026-09-30-p07-player-mobility-system-design.md) |
| 权威输入 | [权威 P07 System 报告](../../system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P07-player-mobility.md) |
| canonical 路径 | 本文档唯一正式路径 |

## 1. 目标与限制

目标是在保留旧行为与调用顺序的前提下，把 P07 行为逐步交给 proposed 的 PlayerArmorSetSystem 与 PlayerMobilitySystem，并为每个跨域状态指定唯一 writer 和明确的外部提交 owner。

本文件主体是实施计划，末节记录当前小批次结果；不得据此声称 P07 已迁移、行为等价或整体验收通过。P07 报告 sessionId 为 78cc87429a4a4e8e94bf74ca6488cb65，报告任务已经结算；不要重新 claim、改写原报告或重复 settlement。

执行目标仍是 src/NSSLC。现有 Player Jump、Mobility、Grapple、Armor 状态组件可作为核查起点，不自动升级为行为 System。静态搜索未找到 PlayerMobilitySystem 或 PlayerArmorSetSystem。utility/accessory capability owner、物理提交点、Projectile/Mount/Combat/Buff/effect/network owner 均未确认。

## 2. 开工门槛

下列门槛未全部通过前，只允许证据补查与 API/数据合同设计，不得切换生产写入路径：

1. 版本与结构基线：Version4 与完整参考项目的 `Terraria/Main.cs` 都声明 `v1.4.5.6`；所检查的 P07 helper 签名、Player.Update 阶段锚点和 Solar/Beetle callback 位置相符。Version4 `Player.cs` SHA-256 为 E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86，完整参考为 367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C。静态对应已记录；完整源码历史、目标 stub 的唯一原实现及运行时行为等价仍为 unknown。参考方法体只能作为重建候选。
2. 调用闭包：解释 CPG Query API 与源码扫描差异。Version4 目标 `RefreshDoubleJumps` 有 3 个直接调用点（12286、13772、15660），CPG 在 `Terraria/Player.cs` 索引中只返回 2 条；`JumpMovement` (16319)、`GrappleMovement` (17092)、`CanMoveForwardOnRope` (15798)、`UpdateJumpHeight` (15612) 有直接源码调用，而各自 CPG 查询为 partial/0 且 `ScannedShardCount=0`。同时，`SolarDashStart` 作为 callback 传入 `DoCommonDashHandle`，CPG 未解析出调用边。还须检查其它文件 caller、动态/反射/配置/生成调用。CPG 的 partial、零命中和索引 complete 均不能单独关闭 caller 闭包。
3. 状态 writer：对候选 Jump/Dash/Flight/Grapple/Beetle/Solar 组件逐字段追踪写入、读取、reset、clone、serialize/network 与生命周期；确定一个权威 writer。utility/accessory flags 在闭包完成前继续 unknown。
4. 交叉 owner：指定 Spatial/Collision 查询及移动 commit、Combat 命中、Projectile identity 与 hook 生命周期、Mount、Buff、随机和 presentation/effect 输出的 API owner 与调用先后。任一缺口未关即阻止对应切片切换。
5. API/阶段合同：写清每个阶段输入、状态 delta、外部输出、异常与 terminal 情况；确认 Query 纯度、Command 延迟语义以及帧顺序。不得新建总线、队列或独立 scheduler phase 来绕过未知依赖。
6. 文档与代码工作约束：开始 C# 或 ECS 文件改动前，阅读仓库要求的 ECS 文件组织、C# 风格、副作用隔离和构建验证约束；本计划不授权当前 turn 运行构建/测试。

## 3. 分阶段执行

| 阶段 | 工作 | 退出条件 |
|---|---|---|
| A. 固定证据基线 | 固定目标与参考源码哈希及 `Main.cs` 版本字符串；对照 helper 签名、调用上下文、Update 顺序和 callback；使用 CPG Query API 查 symbols、call sites、member uses，并将结果与完整源码逐项对照 | 已记录版本/结构对应和哈希差异；每个进入切片的调用关系均有源码证据，未闭合的静态、动态和生成调用明确留作阻断项；不因 query status complete 自动关闭 |
| B. 定义 writer 与合同 | 对现有候选组件建立字段级 R/W/lifecycle 表；给每个跨域行为指定 owner、读入 snapshot 与输出 port；确定单 Player 有序 frame 的输入/输出 | 每个进入首批迁移的字段有唯一 writer；外部 API 与错误语义已批准；其它字段明确留在 legacy owner |
| C. 拆分 armor-set 阶段 | 先提取 Beetle/Solar 到 PlayerArmorSetSystem；保留旧 UpdateArmorSets 兼容入口；先按一个权威路径写入，Buff/random/Dust/render 经已批准 owner 处理 | UpdateArmorSets 源阶段和 dash capability 可被 mobility 阶段观察；计时、tier、orbit、Buff/effect 顺序有可比较的行为基线；没有并行双写 |
| D. 建立 mobility 顺序协调 | PlayerMobilitySystem 作为原 Player.Update 顺序帧的单一协调者；逐阶段迁移 jump parameters/availability、pulley-rope、dash、jump、wall/slide、carpet、flight/rocket、grapple | 保留原条件和调用位置；两次 Dash 分别带 phase；RefreshDoubleJumps 在全部已发现调用点按原时机执行；Spatial/Combat 等共享 owner 仍由明确接口提交 |
| E. 恢复 helper 与跨域接缝 | 在声明版本/静态锚点已对照且依赖合同通过后，评估 Nebula、grapple force、movement ability refresh、rope eligibility 与 Solar dash helper；完整参考实现只作为重建候选，逐项核对目标字段、输入、输出、依赖和边界 | 每个进入切片的 helper 都有明确读写与失败语义，并经过行为验收；剩余 unknown 项继续阻止该切片；不把默认返回值冒充游戏规则 |
| F. 行为验收 | 对新旧组合运行专用行为对照与生命周期验证，审查所有外部副作用；使用批准的串行构建/测试流程记录真实命令和结果 | 所需行为矩阵通过，关键 lifecycle/network/prediction 范围关闭；报告 verificationStatus 才能更新，且不得用 build 成功替代行为证据 |
| G. 有控制的切换与清理 | 按一个已验收边界一次切换；旧 public API 保留作 adapter，直到全部静态、动态、生成、反射、序列化/network caller 闭合；之后单独审查旧路径删除 | 单一权威 writer；回退点与数据重置/恢复合同已验证；只有行为验收通过后才能讨论 migration 成功 |

## 4. 保序要求

切换期间先把 legacy Player.Update 看成兼容 coordinator，按以下位置维持 intra-method 顺序：

1. UpdateArmorSets(i)：Player.cs 15348。
2. UpdateJumpHeight()：15612。
3. 条件满足时 RefreshDoubleJumps()：15660。
4. Pulley/rope inline 阶段：15723-15820；CanMoveForwardOnRope 调用在 15798。
5. 第一处 DashMovement()：16072。
6. JumpMovement()：16319。
7. 第二处 DashMovement()：16332。
8. WallClimbMovement 或 WallslideMovement，随后 CarpetMovement：16335-16341。
9. 条件性的 WingMovement 与 inline rocket 阶段：16393 起。
10. GrappleMovement()：17092。

完整参考项目中对应锚点为 UpdateArmorSets:26078、UpdateJumpHeight:26374、grounded/sliding RefreshDoubleJumps:26422、CanMoveForwardOnRope:26560、第一处 DashMovement:26853、JumpMovement:27100、第二处 DashMovement:27113、WingMovement:27174、GrappleMovement:28017；阶段相对顺序一致。其 RefreshDoubleJumps 还在 JumpMovement:21071、GrappleMovement:23059 调用，和 Version4 目标的三个直接调用上下文对应。

目标源码中 RefreshDoubleJumps 在 JumpMovement 内 12286、GrappleMovement 内 13772、Player.Update 内 15660 被调用。CPG 本次只返回两条索引调用边，且 SourceSnapshotId 为 null、ScannedShardCount 为 0；调用闭包仍未解决，关闭前不得重排、合并或删除任何一个调用点。完整参考的相同上下文增强了静态对应证据，但不补足其它 caller、动态分派或运行时入口。精确调用条件以目标完整方法体复核为准。

这组顺序只约束 Player.Update 体内观察到的 phase；实际宿主调度、角色权威端和跨 world 并发安排仍为 unknown。

## 5. 未来行为验收矩阵

当前只运行新增的 isolated-core verifier；以下矩阵是后续整体迁移验收要求，不由局部 verifier 代替：

- Jump：控制输入、落地/滑行 refresh、额外跳跃 consume/reset、mount 限制、重复 refresh 的可观察结果。
- Dash 与 traversal：两次 dash phase、jump/wall climb/wallslide/carpet/flight/rocket 顺序与条件。
- Armor-set：Beetle/Solar 计数、tier、orbit、Buff、随机消费顺序、Solar dash capability、护甲移除和重复帧。
- Pulley/rope：tile edge、collision 阻挡、边界坐标、helper 缺失行为补齐前不得宣称等价。
- Grapple：有效/无效/stale Projectile identity、多 hook、force、松钩、mount transition、死亡或 disconnect 清理。
- Shared effects：唯一位置/速度 commit、Combat hit/immunity、Buff/effect owner、预测/replay 重复 effect 处理。
- 生命周期：spawn/reset/death/disconnect/reconnect、save/restore、network sync、rollback、exception/retry 与 multi-world 隔离。
- Compatibility：静态 caller、跨文件调用、动态/反射/配置/生成入口和旧 adapter。

验收预期为行为结果、状态差分、副作用数量与先后、错误/终止行为和生命周期都相符。Focused verifier 只作局部诊断；不能单独判定迁移成功。

## 6. 回退与失败停点

- 阶段 A/B 任一关键来源、owner 或 writer unresolved：停止对应切片，状态维持 deferred/unknown。
- 切换期间始终只有一个权威 writer；不得为 shadow compare 同时让 legacy 与新 System 写共享状态或重复发 effect。
- 若未来选择 shadow mode，必须先证明输出可隔离并重放同一输入；否则不执行 shadow。
- 切换后发现 phase 次序、状态差分或外部 effect 不符：恢复到最近的 legacy 路由；新 owner 的 transient state 清理/恢复步骤在当前证据中为 unknown，必须先定义并验证，不能假设简单重置安全。
- 保留旧 API 和数据直到 caller、序列化、网络与生命周期闭包过审。删除旧路径属于单独审批的后续变更，不包含在本计划批次。

## 7. 当前状态

| 状态 | 值 |
|---|---|
| designStatus | proposed |
| migrationStatus | deferred |
| verificationStatus | not-run |

本批次实现与验证记录：

- `src/NSSLC/Component/Player/Mobility/PlayerMobilitySystem.cs`：实现 jump parameter update、jump availability refresh、flight/rocket resource refresh、rope eligibility 与 grapple force 结果计算；全部输入显式化，碰撞和 Projectile 身份仍由调用方提供，未接入 Player.Update。
- `src/NSSLC/Component/Player/Armor/PlayerArmorSetSystem.cs`：实现 Solar dash start 和 Nebula buff tier 转换；只返回 buff slot delta，不直接写 Buff 容器，也未接入旧 callback。
- 新增 `Test/Terraria.Player.Mobility.Core.Verification`：覆盖 jump parameters 的 mount/non-mount 路径、refresh 开关、rope 判定、grapple 速度/方向代表分支、Solar dash 与 Nebula 到期降阶。这个 verifier 是局部核心抽样，不代表完整 Player.Update 迁移。
- Build：通过 `Build/Tools/Invoke-SerialDotnet.ps1` 调用 `build Test/Terraria.Player.Mobility.Core.Verification/Terraria.Player.Mobility.Core.Verification.csproj -c Release -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`，SDK `10.0.400`，exit code 0，0 warnings、0 errors。产物位于 `Build/bin/Terraria.Player/Release/net10.0/` 与 `Build/bin/Terraria.Player.Mobility.Core.Verification/Release/net10.0/`。
- Focused verifier：通过同一 serial wrapper 运行 `run --project Test/Terraria.Player.Mobility.Core.Verification/Terraria.Player.Mobility.Core.Verification.csproj -c Release --no-build --no-restore`，exit code 0，输出 `PASS: P07 mobility and armor-set isolated core`。
- 构建环境：机器级 host 起初无法解析 `global.json` 要求的 SDK；改用已安装的用户级 .NET 10.0.400 host 并仍经仓库 serial wrapper 完成构建。本轮没有独立重建完整参考项目。

整体状态保持 `designStatus=proposed`、`migrationStatus=deferred`、`verificationStatus=not-run`。没有旧入口切换、完整调用闭包、跨域 owner 决策或行为等价结论。CPG Query API 为只读查询；原 P07 报告保持不变且不重复 settlement。

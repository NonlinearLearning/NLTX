# P05 玩家进度、召唤物、宠物与伙伴 System 执行计划

文档 ID：DOC-2026-09-30-system-p05-player-progression-pets-minions-execution-plan  
逻辑域：system-decomposition  
产物类型：plan  
状态：draft  
计划状态：partial；已实现多组隔离的局部核心，生产迁移和验收未完成
证据状态：partial  
验证状态：partial；minion/capacity/fishing 与多组 pet capability 局部核心有 focused verifier 记录；world-object pet、legacy pet/effect、accessory tick、companion rebuild/reset、Golfer/Angler/DD2/unlock-preference reducer 和 progression persistence snapshot mapping 有独立局部验证；完整 save/load、packet 行为及 P05 行为矩阵 `not-run`  
范围：authoritative P05 在 `D:\TRbackup\NLTX\src\NSSLC` 的后续设计落实与行为验收  
设计依据：[P05 System 边界设计](../../system-decomposition/2026-09-30-system-decomposition-P05-player-progression-pets-minions-design.md)  
基线报告：[P05 System 拆分报告](../../system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P05-player-progression-pets-minions.md)  
canonical 路径：`docs/plans/system-decomposition/2026-09-30-P05-player-progression-pets-minions-system-execution-plan.md`

来源 claim 的 `sessionId` 为 `65bb838ff91b4927a81ee6d3e1500515`；原始 outputReport 是 [P05 System 拆分报告](../../system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P05-player-progression-pets-minions.md)，状态已结算。本执行计划是该报告的派生材料，不重新 claim 或结算 runner partition。下方按日期记录已有局部实现/验证事实，不替代生产接入或完整行为验收。

## 执行目标

围绕 P05 的可观察行为，在生产 System/API 组合中明确唯一状态 owner、调用/调度边界、失败与重复语义以及 persistence/network 适配。目标目录为 `src/NSSLC`。完成条件是对应行为测试经生产 owner 与完整组合运行并通过；组件文件存在、文档完成、局部 verifier 通过或编译成功不构成迁移成功。

## 前置条件与阻断门

1. 锁定实现所依据的 Version4 源文件 hash。用户指定的完整参考项目 `D:\TRbackup\无任何删减通过编译` 是不同快照；先将其与目标 Version4 对照并记录差异。不得把两份源码混合成同一个旧行为基线。当前 `Player.cs` 哈希不同，目标 Version4 的 save/serialize 方法为空或 stub。
2. 先完成 `crossSubsystemOwner: integration-review` 的边界确认：Projectile capacity、Pet / Companion lifecycle、Combat damage tracking、P03 / Movement Vehicle、Fishing / Equipment 输入，以及 Player save/network 协议。未决定的行为只允许做隔离的纯计算或状态核心，不得接入第二个 writer。
3. 当前 P05 候选文件只证明候选代码存在。对 `src/NSSLC` 的文本 caller 搜索没有找到生产调用接入；实现前须确认实际 ECS 注册、scheduler、模块依赖、session-to-entity 映射和生产 caller。
4. 为每个概念行为建立 legacy 输入、可观察状态差、结果/错误、事件/外部效果和顺序清单。无法由匹配源码证据确认的项目记 `unknown`，不得拿参考源码、字段名或 CPG 零命中补成结论。

### 数据包 API 实施限制

P05 数据包相关 API 仅编写函数签名声明，不得编写具体函数体处理字段打包/拆包、编解码、序列化/反序列化、协议校验、网络传输、发送/广播或权威状态应用。当前只保留 `IPlayerProgressionPacketProjection.Project` 声明及 `PlayerProgressionPacketFlags` 数据形状；不实现接口。Version4 当前源码的字段槽位已确认：`NetMessage.cs:181-183` 写 `bitsByte21[2-4]`，`NetMessage.cs:186-192` 写 `bitsByte22[0-6]`，`MessageBuffer.cs:277-279,281-287` 对应读回。该源码映射不得扩张为跨版本协议兼容结论；协议版本、旧包/缺字段默认值、调用关系闭包、网络 owner 和运行顺序仍为 `unknown`。其他 API 签名缺少匹配证据时不臆造；packet 行为验证保持 `not-run`。该限制不改变纯本地 progression reducer、命令和 Query 的实现范围。

## 实施批次

| 批次 | 工作内容 | 交付物 | 进入下一批的门槛 |
|---|---|---|---|
| 0. 基线与差异 | 固定 Version4 / 完整参考项目的文件 hash；确认字段声明、被清空方法和可用完整实现之间的差异；保存 CPG 查询 scope 与 gaps。 | 可复核的 source identity / gap 记录 | 源快照不可混淆；序列化差异显式列出。 |
| 1. Owner 与协议 | 与 Projectile、P03/Movement、Combat、Pet/Companion、Persistence/Network owner 确认最终 writer、输入身份、提交结果、时序、失败及幂等要求。 | 集成决定和 typed boundary 草案 | 每个跨域不变量只有一个最终提交协议；未决边界继续阻断其生产接入。 |
| 2. ECS 接入骨架 | 在 `src/NSSLC` 明确 Player entity identity、状态组件、System 注册、生产入口和生命周期；将现有候选类型与生产依赖逐项对照。 | 可追踪的 registration/caller 图 | 生产入口可追溯到真实调度；不以 verifier caller 代替。 |
| 3. Durable progression | 分批接入 consumed upgrades、unlock/preference、Angler/Golfer/DD2 command；将状态 reducer 与物品、奖励、事件、网络效果分开，并明确有序应用边界。 | progression owner、来源 adapter 和副作用顺序记录 | consumed upgrade 需验证 item-time -> flag -> network 的匹配版本顺序；Angler 需验证 item consume -> counter commit -> reward -> daily/network 的提交边界；任何跨效果拒绝都需定义串行化、回滚或补偿。 |
| 4. Tick capability snapshots | 接入 fishing、minion capability、pet capability、accessory snapshots；记录 tick/revision、reset、累加、发布和消费者顺序。 | rebuild owner、只读 snapshot 和消费者接点 | reset -> contributions -> rebuild -> consume 的实际 scheduler 顺序已确认，不能从文件顺序推断。 |
| 5. Minion capacity | 在已选定的 integration owner 下协调 Projectile admission、slot 预留、实体创建、计数提交、失败回滚与 despawn/release。候选 Player commit core 绑定目标 Player identity，在组件锁内更新 delta、reset、maximum 与幂等状态。 | 一个 capacity commit 协议和结果类型 | 创建成功/失败、重复通知、实体销毁、浮点 slot 和跨 Player 隔离都有行为证据；本地锁只保护容量组件，不提供跨 Projectile 原子协议，本地 owner-bound delta core 不替代该门槛。 |
| 6. Pet / Companion lifecycle | 分离解锁、当前资格、启用和 live entity；按 owner 协议处理 spawn/sync/despawn/death/respawn/reconnect/world transfer。 | capability 与 entity lifecycle handoff | 未决定是否持久化的 flag 不迁入持久 ledger；清理不能误删解锁。 |
| 7. Damage / Accessory / Vehicle seams | Damage high-water 只接收可归属的 Combat/Projectile observation；accessory snapshot 交各 consumer owner；Mount/Minecart 仅在 P03/Movement 决策后接入。 | typed integration inputs / outputs | provenance、reset、consumer 与 phase 已确认；未决 Vehicle owner 不落 P05 writer。 |
| 8. Persistence / Network | 先锁定匹配 Version4 的协议证据与 owner；数据包相关 API 只提交函数签名声明，不实现数据包逻辑。当前提议的 progression flags projection 只包含有源码映射的字段；其它存档/网络字段映射和实现等协议缺口确认后再推进。 | `IPlayerProgressionPacketProjection.Project` 签名与 flags DTO（`proposed`）、存档/网络字段映射、恢复策略与 unknown 清单 | 跨版本字段协议、旧包/缺字段默认值、未知版本、半失败/回滚、重复和可见时点均有证据与行为测试；签名声明本身不算完成。 |
| 9. 端到端验收 | 通过实际生产调度验证全部行为组合、生命周期和跨域 handoff；保留旧入口直到行为门槛通过。 | 运行记录、未覆盖清单与删除决策 | 必需行为测试通过，旧/新结果观察向量一致，重大 unknown 已清零或有显式接受决策。 |

批次 3 与 4 可在 owner 决策明确后分开推进；批次 5、6、7 的对应集成决定未通过时不得绕过阻断门。批次 8 不得根据用户提供的完整参考快照直接复制序列化实现到 Version4 兼容层；当前 projection interface 只有函数声明，无实现。数据包行为和协议兼容仍为 `not-run`，直到匹配目标版本的实现及行为验收另行授权并完成。

## 行为验收矩阵

此矩阵仍是完整验收要求。本计划记录的局部 verifiers 仅直接调用候选核心，不触达实际生产 API composition，也不证明行为等价；完整 P05 行为矩阵仍为 `not-run`。

| 行为 | 必须覆盖的输入与状态差 | 关键观察结果 |
|---|---|---|
| consumed progression | 有效、无效、重复、乱序消费；资源不足和 owner 不匹配 | consumed flag、物品扣除、拒绝结果、奖励/事件顺序，无半提交 |
| unlock/preference | 首次解锁、重复解锁、启用/关闭偏好、旧版本缺字段 | unlock 与 preference 独立；查询、存档恢复、网络投影一致 |
| quest/event progress | 单次、重复、乱序 Angler/Golfer/DD2 事件及边界分值 | counter delta、上限、幂等结果、奖励回调顺序 |
| fishing rebuild | 连续 tick、装备/buff 增减、各 skill 增量来源、输入缺失、重复 `Contribute` / rebuild | 七项 snapshot 无跨 tick 残值；累计与重复输入结果、消费者读取 revision 和 target 可观察结果一致 |
| minion capacity | 整数/分数 slot、满载、并发候选、创建失败、kill/despawn、重复 delta | 最终 admission 结果唯一；计数与 live projectile 一致，不超卖/漏释放 |
| summon / pet / companion | 各内容映射、死亡、复活、重连、换世界、重复同步、实体创建失败 | 解锁/资格/live entity 分离；无重复实体，清理范围正确 |
| damage high-water | 多种来源、错误 owner、重复 observation、死亡/重置 | provenance、最大值、重置与消费者结果一致 |
| accessory / vehicle | accessory 输入变化及下游功能；Mount/Minecart collision、轨道和顺序 | snapshot 不越权写回；Vehicle 结果和原顺序经 integration owner 确认 |
| persistence / network | supported releases、未知/截断数据、加载失败、重复发送和重连 | 字段版本、默认/拒绝、恢复结果、复制顺序和错误恢复一致 |

## 依赖和失败处理

- 每个状态维护一个权威写入责任；Projection 和 Query 不写权威数据。跨 owner 更新必须明确提交顺序、部分成功、补偿或重建行为。
- 有容量或实体副作用的 Command 应携带可验证 owner / projectile identity 和重复语义；不能仅依赖本地进程内 token cache 实现跨会话幂等。
- 所有重建输入要标出来源、生命周期、有效 revision 和失效条件；输入未到、过期或 owner 不可用时使用明确拒绝/unknown 结果，不静默回填猜测值。
- 持久化/网络错误、超时未知、取消和重试策略必须与 adapter 责任绑定；不得把本地等待超时描述成远端未提交。
- 现有 legacy facade 在必需行为覆盖前继续保留。此计划不包含批量删除或默认 feature switch；实际回退开关若无现成机制，状态为 `unknown`，需在实现前单独明确。

## 验证与交付记录

计划初稿创建时没有修改 `src/`、测试或项目文件，也没有运行 build、test、verifier 或生成代码。后续实施必须遵循[构建与验证约束](../../../Context/约束/构建与验证约束.md)：从仓库根目录经 `Build/Tools/Invoke-SerialDotnet.ps1` 串行验证受影响项目，先检查活跃 dotnet/csc 进程；不为小范围改动构建整个 solution。生产接入项目和调度入口仍为 `unknown`。

实施阶段每次验证记录实际命令、目标项目、退出码、warning/error 数、`Build/bin/` 产物路径和测试观察结果。文档链接或类型文件存在不替代行为结果。删除 legacy writer/facade 的 gate 是所有必需行为由新 owner 和完整组合测试通过，并完成 persistence/network 与生命周期复核；当前 gate 未满足。

## 本轮状态

| 项目 | 状态 |
|---|---|
| 设计依据 | P05 proposed 设计；已查询 Version4 CPG，并分别对照目标 Version4、完整参考源码及 SS14 `SharedResearchSystem`；两套旧源码快照未混用 |
| 迁移实现 | partial：容量 commit core 绑定目标 Player `EntityReference`；minion capability rebuild/reset core 接收显式的 22+3 flag snapshot；seasonal/event、standard named、boss、crossover 与 world-object pet capability rebuild/reset core 分别接收显式 9、13、16、13、4 flag snapshot。均未接生产 caller。Projectile admission、minion/pet 输入映射、pet 生命周期协调、容量与 damage 最终 owner 仍为 `integration-review` / unknown。damage high-water reducer、consumed-upgrade item-use 预判 query、Angler/Golfer/DD2 局部 reducer已存在；P05 packet flags projection 只有 proposed interface 声明和 DTO，无实现；生产调度未发现 |
| build、test、verifier | partial：既有 P05 focused verifier 有 capacity C03、fishing、minion capability、seasonal/event、standard named、boss、crossover pet、world-object pet，以及 Golfer/Angler/DD2/unlock-preference progression commit 的局部结果；完整 P05 行为矩阵仍为 `not-run`。world-object pet 使用独立 verifier 项目验证四项 rebuild/reset；历史 verifier 结果只支持各自局部核心，都不证明 Version4 行为或生产组合 |
| 行为等价、迁移成功、旧实现删除 | 未声明；完整生产组合、持久化/网络与生命周期验收仍未完成 |

本轮还修正了 Player 项目已有的 Mount 编译阻断：补齐 Mount 命名空间导入，统一钻机 runtime 类型名，并让 catalog 返回只读 values 集合。这些改动不改变 Mount 领域行为。

验证环境使用仓库 `global.json` 指定的 .NET SDK `10.0.400`（用户级 SDK 路径 `C:\Users\shan\.dotnet`；仓库配置未改）。所有命令均经串行 wrapper，构建前检查了活跃 `dotnet`/`csc` 进程。

| 目标 | Wrapper 参数 | 退出码 | warning/error | 产物与观察 |
|---|---|---:|---|---|
| P05 minion capacity build | `build .\src\NSSLC\Component\P05MinionCapacityVerification\Terraria.P05MinionCapacityVerification.csproj --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | 0/0 | 2026-10-01，在 `PlayerMinionCapacityComponent` 保存不可变 owner 后经 wrapper 重建；SDK `10.0.400`；`Terraria.Relationships`、`Terraria.Player` 与 verifier 均输出到 `Build/bin/`；验证产物 `Build/bin/Terraria.P05MinionCapacityVerification/Debug/net10.0/Terraria.P05MinionCapacityVerification.dll` |
| P05 minion capacity verifier | `run --project .\src\NSSLC\Component\P05MinionCapacityVerification\Terraria.P05MinionCapacityVerification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | n/a | 2026-10-01 输出 `P05 C03 verifier passed: owner binding, concurrent idempotency, capacity, reset and count overflow.`；32 个相同 token 并发调用经 8 个 System 实例恰好一项提交，其余为 AlreadyApplied；不覆盖 Projectile 最终 admission/rollback 协议 |
| Player accessory build | `build .\src\NSSLC\Component\PlayerAccessoryVerification\Terraria.PlayerAccessoryVerification.csproj --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | 0/0 | 2026-10-01 在 high-water 和 consumed-upgrade query 修改后重建；`Build/bin/Terraria.PlayerAccessoryVerification/Debug/net10.0/Terraria.PlayerAccessoryVerification.dll` 已生成 |
| Player accessory verifier | `run --project .\src\NSSLC\Component\PlayerAccessoryVerification\Terraria.PlayerAccessoryVerification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | n/a | 2026-10-01 重跑；`PASS: player accessory string effects rebuild, reset, query and edge semantics`；不覆盖 progression query 或 high-water 行为 |
| Player core build（增加 high-water System 后、item-use Query 前） | `build .\src\NSSLC\Component\Player\Terraria.Player.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | 0/0 | SDK `10.0.400`（`C:\Users\shan\.dotnet`）；`Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll` 已生成；本次 build 执行了 restore |
| Progression packet projection signature build | `build .\src\NSSLC\Component\Player\Terraria.Player.csproj --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | 0/0 | 2026-10-01；SDK `10.0.400`，本进程设置用户级 `DOTNET_ROOT` / `PATH`；产物 `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll` 存在；只验证声明编译，不验证 packet 行为 |
| Progression core build | `build .\Test\Terraria.Player.Progression.Verification\Terraria.Player.Progression.Verification.csproj --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | 0/0 | 2026-10-01；使用仓库要求的 SDK `10.0.400`；`Build/bin/Terraria.Player.Progression.Verification/Debug/net10.0/Terraria.Player.Progression.Verification.dll` 已生成并复核 |
| Progression core verifier | `run --project .\Test\Terraria.Player.Progression.Verification\Terraria.Player.Progression.Verification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | n/a | 2026-10-01；输出 `PASS: golfer, Angler, DD2, and unlock/preference local progression commits`；覆盖本地 reducer/commit，不证明来源关系、奖励/事件顺序或生产接入 |

上述 `build` / `run` 参数通过 `Invoke-SerialDotnet.ps1 -DotnetArguments <string[]>` 传递。Progression verifier 本轮新增 Angler 与 DD2 本地 commit 重复语义断言，并保留 Golfer、unlock/preference 用例；它不证明 GolfHelper caller、Angler 奖励链、DD2 目标 writer、目标 Version4 API 组合或行为等价。首次 wrapper build 使用系统 dotnet host，因仅发现 SDK `10.0.100` 且仓库 `global.json` 要求 `10.0.400`，在 MSBuild 启动前退出（exit code 1）；确认用户级 SDK `C:\Users\shan\.dotnet` 已安装 `10.0.400` 后，仅在该命令进程设置 `DOTNET_ROOT` 并把该目录置于 `PATH` 首位，再经同一 wrapper build 成功，仓库 SDK 配置未改。consumed-upgrade item-use query 已有局部输入门槛、映射与重复 flag commit 断言，但仍未覆盖生产调度、跨 Player/Projectile 原子 admission、行为矩阵其余项目或整体迁移完成。完整参考项目 `D:\TRbackup\无任何删减通过编译` 仅用于源码关系与顺序复核，本轮未对该参考项目执行构建或测试。

构建初次使用系统 `dotnet` host 时因其只发现 SDK `10.0.100`、而 `global.json` 要求 `10.0.400`，在 MSBuild 启动前退出（exit code 1）；之后将已安装的 `C:\Users\shan\.dotnet` 置于本进程 `PATH` / `DOTNET_ROOT`，继续通过同一串行 wrapper 构建成功，未改仓库 SDK 配置。执行期间曾检测到另一项 `Terraria.WorldSession` 编译和其 `csc.exe`；等待它们退出后才开始本轮 verifier build，没有并行编译。

### 2026-10-01 damage high-water 局部核心

目标 Version4 `Player.cs` 的 high-water 写入和 reset 源码已复核；完整参考项目中的对应 Projectile 消费点仅作为独立快照的辅助材料。CPG Player shard 对字段 member uses 的查询为 `complete`（每字段 3 条，含 1 个 access mode unknown）；Projectile-only member uses 和 `UpdateProjectileCaches` call-site 查询为 `partial`、零命中并带 gap。`ResetProjectileCaches` 的 CPG call-site 查询虽为 `complete`，只返回 1 条，而当前源码可见两处调用。CPG manifest 为 `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`；这些关系不视为完整 inbound closure。

新增 `PlayerMinionDamageHighWaterMarkSystem`，仅对显式传入的 Storm Tiger / Abigail original damage 值做单调最大值累加和局部清零；它不持有 Projectile 依赖、实体枚举、owner/session 映射或调度入口。P05 focused verifier 已验证两个高水位独立保留最大值、较低值不回退和 reset 清零；生产 observation caller、reset phase、consumer 和唯一 writer 仍为 `unknown` / `integration-review`。完整参考项目名包含“通过编译”不构成本轮对该参考项目执行构建的证据；本轮未构建或测试完整参考项目。

本次 focused build / verifier 命令均经仓库串行 wrapper，工作目录为仓库根目录。首次未用 `-DotnetArguments` 的调用在 wrapper 参数解析阶段失败，未启动编译；改为数组参数后系统 PATH 选择了 SDK `10.0.100`，在 MSBuild 启动前因 `global.json` 要求 `10.0.400` 退出。确认用户级 SDK `C:\Users\shan\.dotnet` 存在后，仅在构建进程设置 `DOTNET_ROOT` 和 `PATH`，随后 build 与 verifier 均通过；仓库配置未改。

| 目标 | Wrapper 参数 | 退出码 | warning/error | 产物与观察 |
|---|---|---:|---|---|
| P05 focused build（含 damage high-water assertions） | `build ./src/NSSLC/Component/P05MinionCapacityVerification/Terraria.P05MinionCapacityVerification.csproj --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | 0/0 | SDK `10.0.400`；Relationships、Player、verifier 构建完成，产物 `Build/bin/Terraria.P05MinionCapacityVerification/Debug/net10.0/Terraria.P05MinionCapacityVerification.dll` |
| P05 focused verifier | `run --project ./src/NSSLC/Component/P05MinionCapacityVerification/Terraria.P05MinionCapacityVerification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | n/a | 输出 `P05 focused verifier passed: capacity, fishing, minion, pet and damage high-water cores.`；局部验证不覆盖生产 schedule、Version4 下游消费关系或行为等价 |

### 2026-10-01 consumed-upgrade item-use 预判

目标 Version4 和完整参考项目 `ItemID.cs` 的六个 permanent-upgrade item type 值均为 `5337` 至 `5342`（文件 hash 见设计文档）。完整参考项目中存在 item-use 方法及 `itemAnimation > 0`、未消费、`ItemTimeIsZero` 门控，随后依次执行 `ApplyItemTime`、写 Player flag、发送 Player net update；目标 Version4 的对应方法及 flag true 写入未能从源码 / CPG 确认，必须保持 `unknown`。CPG `ItemCheck_UseShimmerPermanentItems` symbol 查询为 `partial` / 零命中并带 gap；`AegisCrystal` ItemID symbol 查询为 `complete`，但 Player member-use 查询为 `partial` / 零命中并带 gap。

新增 `PlayerConsumedUpgradeItemUseInput` 和 `ConsumedUpgradeEligibilityQuery.TryResolveEligibleUpgrade`，只做 item type 映射与只读预判；现有 commit System 仍重验 ledger。尚未接入目标 Version4 ItemCheck caller 或 NLTX 生产 ItemCheck adapter，也未实现 `ApplyItemTime`、stack mutation 或 net sync adapter；新 Query 核心现为局部验证通过，生产组合仍为 `not-run`。完整参考的逻辑只能支持候选组合，不证明与目标 Version4 行为等价。

本轮查询 API 对目标 Version4 `usedAegisCrystal` field symbol 返回 `complete`；在 Player / Item / Main / GolfHelper 选定 scope 内，member-use 返回 `complete` / 2 项，均在 Player shard，一项 `confirmed` / `Write`（不确定写入值）、一项 `partial` / `Unknown`。manifest 为 `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`；`ItemCheck_UseShimmerPermanentItems` symbol 查询仍为 `partial` / 零项带 gap。完整参考 `Player.cs` SHA-256 为 `367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C`，该方法在 `:46048-46086` 显示六个 item type `5337` 至 `5342` 对应动画、未消费及 item-time gate，再执行 item-time、flag 写入和 net update；这仍是独立参考快照证据。

focused verifier 已从此前 `not-run` 更新为局部通过：它验证六种 item type 精确映射、动画为零及 item-time 未清零时拒绝、不支持的 item / upgrade 拒绝、eligible 查询不改 ledger、每种升级首次 flag commit 成功且重复 commit 返回 `AlreadyConsumed`。尚无目标 Version4 ItemCheck 调用闭包或 NLTX 生产 caller，也没有物品时间/stack adapter 或网络投影，因此这不是 consumed progression 完整行为组、生产接入或行为等价证明。

补查完整参考调用点 `Player.cs:44117`，可见 `ItemCheck()` 直接调用 `ItemCheck_UseShimmerPermanentItems(sItem)`；这是完整参考快照中的组合入口，不是 Version4 调用边。对 Version4 六个 `used*` 字段重新运行 CPG Query API，限定 `Player.cs`、`NetMessage.cs`、`MessageBuffer.cs` 后，每个 symbol 与 member-use 查询均为 `complete`；member-use 数量依次为 4、4、5、4、4、4，facts 分布在三份 shard，访问模式包含 `Write` / `Unknown`，证据状态包含 `confirmed` / `partial`。manifest 为 `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364` 且 `SourceSnapshotId=null`。目标 Player 源码可见这些 flag 的重置写入，但没有确认与完整参考 helper 对应的 true 写入；`complete` 只表示 scoped index 请求完成，不闭合目标 writer、ItemCheck 调用或 tick/authority 顺序。

据此保留 `ConsumedUpgradeEligibilityQuery` 为纯预判 API，不接入生产 item-use adapter。完整参考 helper 的 `NetMessage.SendData(4, ...)` 是 packet effect；packet 相关 API 只保留函数声明，不实现编码、发送或应用逻辑。`ApplyItemTime`、progression commit 与 packet publication 的生产顺序和失败/重复策略仍为 `unknown`，不将完整参考的顺序当作 Version4 契约。

| 目标 | Wrapper 参数 | 退出码 | warning/error | 产物与观察 |
|---|---|---:|---|---|
| P05 consumed-upgrade focused build | `build .\src\NSSLC\Component\P05MinionCapacityVerification\Terraria.P05MinionCapacityVerification.csproj --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | 0/0 | 2026-10-01，SDK `10.0.400`；`Terraria.Relationships`、`Terraria.Player` 和 verifier 输出到 `Build/bin/`；产物 `Build/bin/Terraria.P05MinionCapacityVerification/Debug/net10.0/Terraria.P05MinionCapacityVerification.dll` |
| P05 consumed-upgrade focused verifier | `run --project .\src\NSSLC\Component\P05MinionCapacityVerification\Terraria.P05MinionCapacityVerification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | n/a | 输出 `P05 focused verifier passed: progression, capacity, fishing, minion, pet and damage cores.`；本次仅新增 consumed-upgrade 隔离核心断言。 |

### 2026-10-01 Angler / Golfer / DD2 源码补查

完整参考 `Main.cs` 的 Angler 提交流程在扣交付物后播放声音、增加 `anglerQuestsFinished`，再调用 `GetAnglerReward`；奖励依赖已递增的 quest count，之后才设置每日完成状态并按 net mode 发消息。该顺序只能作为不同完整快照的候选行为，目标 Version4 Main 的对应 caller 未找到；现有 `RecordAnglerQuestCommand` 只提交 counter，不代表奖励/物品结算。

完整参考 Golf 链路为 `GolfHelper` -> `GolfState.GetGolfBallScore` -> `Player.AccumulateGolfingScore`。hit-distance / hit-count 公式和有界 score timer 产生非负输入；Player 只限制累计上限 `1_000_000_000`。目标 Version4 的 Player/Golf 方法与调用边未由源码或 CPG 闭合。新增的 Golfer verifier 只测 NLTX 局部 reducer，不覆盖 GolfHelper、目标 API 接入或完整参考与 Version4 的等价性。

完整参考 Player update 会将 `DD2Event.DownedInvasionAnyDifficulty` 同步为 Player flag。目标 Version4 setter/source event 仍 `unknown`。CPG 对 Player type 和三个计数字段符号为 `complete`；`Get-CpgMemberUses` 对 Angler/Golfer 的 scope 是 `Terraria/Player.cs`、`Terraria/Main.cs`、`Terraria.GameContent.Golf/GolfHelper.cs`，返回 `partial` / 零项；DD2 返回一条 `EvidenceStatus=partial`、`AccessMode=Unknown` 的 use。`Find-CpgSymbols` 对 `AccumulateGolfingScore`（`Terraria/Player.cs`）、`GetGolfBallScore`（`Terraria.GameContent.Golf/GolfState.cs`）、`GetAnglerReward`（`Terraria/Player.cs`）均为 `partial` / 零项并带 gap。数据库 manifest 仍为 `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`；这些查询不关闭 source/target 关系。

### 2026-10-01 minion capacity 源码与 API 复核

通过只读 CPG Query API 初始化并查询 manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`。在选定的 `Terraria/Player.cs`、`Terraria/Projectile.cs` shards 中，三个容量字段的 symbol / member-use 查询均返回 `complete`：`maxMinions` 23 项、`numMinions` 3 项、`slotsMinions` 3 项；其中 Projectile facts 带有 `partial` 与 `AccessMode=Unknown`，故结果不作为完整 writer/caller closure。对 `ResetProjectileCaches` 的 call-site 查询返回 1 条 Player 源码边；当前源码与 CPG 没有 source snapshot 绑定，仍须按源码校准。

目标 Version4 `Terraria/Projectile.cs:14741-14767` 直接显示 minion 路径以 `owner` 读取 Player 计数/上限，在 `owner == Main.myPlayer` 时执行超容量分支，并在成功分支递增 `numMinions` 和 `slotsMinions`。目标 `Player.cs:10024-10025`、`14947-14948`、`17659-17660` 有多个计数清零位置。完整参考项目 `D:\TRbackup\无任何删减通过编译` 的对应路径为 `Projectile.cs:15508-15535`、`Player.cs:17301-17302`、`24937-24938`、`28647-28648`。完整参考的特殊 Projectile 类型 `626` / `627` 分支与本地 owner 判定可作为行为验收用例；两份源码快照不合并，且参考项目名称不证明本轮构建过它。

目标源码中可确认这些字段与 Projectile 计数代码的直接文本关系；在 Version4 中全路径 admission 调度、跨 projectile 创建/失败/销毁原子性、远端 owner 复制语义及所有恢复路径仍为 `unknown` / `integration-review`。因此局部 capacity commit 不等同于已经接入生产 owner，也不称行为等价或迁移成功。

### 2026-10-01 minion capability rebuild 核心

Version4 `Player.ResetEffects` 在 `Terraria/Player.cs:10596-10619` 清空 22 个 core minion flags，并在 `10611-10613` 清空 3 个 crossover minion flags；完整参考快照在 `Player.cs:18984-19007` 与 `18999-19001` 有对应重置。两份 `Player.cs` 的 SHA-256 分别为 Version4 `E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86`、完整参考 `367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C`，作为两个独立快照记录。

只读 CPG 查询使用 manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`、967 shards、`SourceSnapshotId=null`。`pygmy` 在 Player / Projectile 选定 scope 的 member-use 查询为 `complete` / 5 项（3 confirmed、2 access mode unknown）；`deadCellsMushroomBoiMinion` 为 `complete` / 3 项（2 confirmed、1 unknown）。这只证明被索引的静态使用候选；CPG 未绑定本次源码 hash，也未闭合动态调用和调度关系。直接源码确认 ResetEffects 清零，且 UpdateBuffs 中存在 minion/crossover flag 写入；当前 tick 的上游映射来源和实际 ECS caller 仍为 `unknown`。

新增 `PlayerMinionCapabilityRebuildInput` 与 `PlayerMinionCapabilityRebuildSystem`：System 只将上游解析的当前 tick flag 快照写到 `PlayerCoreMinionCapabilityComponent` / `PlayerCrossoverMinionCapabilityComponent`，`Reset` 等价于应用默认输入。它不解析 buff/content 定义、不触碰 capacity 或 Projectile，也未接入生产 scheduler。单个 verifier 的本轮覆盖为 17 个字段组中的 capacity、core minion flags、crossover minion flags 3 组；完整行为矩阵仍为 `not-run`。

验证使用 SDK `10.0.400`，构建和执行均经 `Build/Tools/Invoke-SerialDotnet.ps1`，构建前未发现活跃 `dotnet` / `csc` 编译进程：

| 目标 | Wrapper 参数 | 退出码 | warning/error | 产物与观察 |
|---|---|---:|---|---|
| P05 minion focused build | `build .\src\NSSLC\Component\P05MinionCapacityVerification\Terraria.P05MinionCapacityVerification.csproj --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | 0/0 | Relationships、Player、verifier 输出到 `Build/bin/`；产物 `Build/bin/Terraria.P05MinionCapacityVerification/Debug/net10.0/Terraria.P05MinionCapacityVerification.dll` |
| P05 minion focused verifier | `run --project .\src\NSSLC\Component\P05MinionCapacityVerification\Terraria.P05MinionCapacityVerification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | n/a | 输出 `P05 C03 verifier passed: owner binding, concurrent idempotency, capacity, reset and count overflow.`；同一程序另验证 25 个 minion flag rebuild/reset。 |

此验证不覆盖内容 eligibility 输入适配、实际 Player tick ordering、生产注册、Projectile admission/rollback、pet 与 companion lifecycle、Accessory/Fishing 下游组合、持久化/网络或 Version4 行为等价；这些仍保持 `unknown` / `not-run`。完整参考项目只用于源码对照，本轮未构建或测试该项目。

### 2026-10-01 seasonal/event pet capability 核心

目标 Version4 `Player.UpdateBuffs` 将 9 个 seasonal/event flags 作为 `ref bool` 传入 pet helper；`ResetEffects` 清零这些 flag。外层 helper 写 buff time，内层 helper 置位 flag、检查 owned projectile count 并计算 spawn center，但目标 Version4 方法体没有显示 `Projectile.NewProjectile` 分支。`Projectile` 对多个同组 flags 执行死亡清理并读取它们维持 lifetime。完整参考项目的内层 helper 有 owner 条件与 `Projectile.NewProjectile` 调用，但 Player / Projectile hash 与 Version4 不同，不能作为 Version4 创建路径证据；两套快照未合并。NLTX `EntityReference`、`EntityRelationState`、`SpawnAdmissionState` 目前没有执行 Player/Projectile pet lifecycle handoff 的 API。

Version4 CPG Query API 对 `petFlagDD2Gato`、`petFlagDD2Ghost`、`petFlagDD2Dragon`、`petFlagPumpkingPet`、`petFlagEverscreamPet`、`petFlagIceQueenPet`、`petFlagMartianPet`、`petFlagDD2OgrePet`、`petFlagDD2BetsyPet` 查询了 SymbolField 与 Player/Projectile member uses。各 symbol 和选定 scope 的查询状态为 `complete`；多个 facts 仍为 `EvidenceStatus=partial` / `AccessMode=Unknown`，四个字段在 Projectile scope 中有 confirmed-write candidates。manifest `SourceSnapshotId=null`；查询不构成完整 writer closure。SS14 `FollowerSystem` 以专门的 System 维护 reciprocal relation 并处理终止/替换，只作系统组织参照，不将 ghost follower 行为作为 Terraria pet 规则。

新增 `PlayerSeasonalEventPetCapabilityRebuildInput` 与 `PlayerSeasonalEventPetCapabilityRebuildSystem`，只将上游已解析的 9 个当前 tick flags 重建到 `PlayerSeasonalEventPetCapabilityComponent`，`Reset` 应用默认输入。它不解析 buff/event、不延长 buff、不创建 Projectile、不接入生产调度；输入映射、死亡清理交接、实体生命周期 owner 与 phase 仍为 `unknown` / `integration-review`。P05 focused verifier 验证 9 项全部置真及 reset 后全部置假；它不是 pet 行为矩阵，也不证明与 Version4 等价。

| 目标 | Wrapper 参数 | 退出码 | warning/error | 产物与观察 |
|---|---|---:|---|---|
| P05 seasonal/event pet focused build | `build .\src\NSSLC\Component\P05MinionCapacityVerification\Terraria.P05MinionCapacityVerification.csproj --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | 0/0 | 2026-10-01，SDK `10.0.400`；Relationships、Player、verifier 编译产物在 `Build/bin/`；verifier 产物 `Build/bin/Terraria.P05MinionCapacityVerification/Debug/net10.0/Terraria.P05MinionCapacityVerification.dll` |
| P05 seasonal/event pet focused verifier | `run --project .\src\NSSLC\Component\P05MinionCapacityVerification\Terraria.P05MinionCapacityVerification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | n/a | 输出 `P05 focused verifier passed: capacity and minion/pet capability rebuild/reset.`；只覆盖局部 rebuild/reset 与既有 capacity core |

### 2026-10-01 standard named pet capability 核心

Version4 `Player.ResetEffects` 在 `:10536-10547,10569` 清零 13 个 standard named pet flags；`Player.UpdateBuffs` 在 `:5329-5374,5538-5539` 通过共享 pet helper 更新这些当前 tick flags。完整参考项目的对应位置为 `Player.cs:18924-18935,18957` 与 `:10963-11008,11172-11173`。Version4 与完整参考 `Player.cs` SHA-256 分别为 `E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86`、`367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C`；两份快照不合并。

只读 CPG Query API 使用 manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`、967 shards、`SourceSnapshotId=null`。13 个 field symbols 与 Player / Projectile 选定 scope 的 member-use 请求均为 `complete`；`petFlagUpbeatStar` 返回 2 项，其余字段各 4 项。所有字段都带 `EvidenceStatus=partial` / `AccessMode=Unknown` facts，不能从结果推出完整 writer/consumer 或 runtime 闭包。

新增 `PlayerStandardNamedPetCapabilityRebuildInput` 和 `PlayerStandardNamedPetCapabilityRebuildSystem`，仅复制 caller 已解析的 13 个 flags；`Reset` 应用默认输入。它不解析 buff/content，不做 buff-time、spawn/despawn 或 scheduler 处理。P05 focused verifier 覆盖 13 项全量置真和 reset 清零；输入映射、生产 caller、pet lifecycle owner 与 Version4 行为等价仍为 `unknown` / `integration-review`。

| 目标 | Wrapper 参数 | 退出码 | warning/error | 产物与观察 |
|---|---|---:|---|---|
| P05 standard named pet focused build | `build .\src\NSSLC\Component\P05MinionCapacityVerification\Terraria.P05MinionCapacityVerification.csproj --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | 0/0 | 2026-10-01，SDK `10.0.400`；Relationships、Player、verifier 输出到 `Build/bin/`；产物 `Build/bin/Terraria.P05MinionCapacityVerification/Debug/net10.0/Terraria.P05MinionCapacityVerification.dll` |
| P05 standard named pet focused verifier | `run --project .\src\NSSLC\Component\P05MinionCapacityVerification\Terraria.P05MinionCapacityVerification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | n/a | 输出 `P05 focused verifier passed: progression, pet, capacity, fishing, minion and damage cores.`；此项仅验证 13 个 standard named pet flags rebuild/reset 与既有 focused cores |

### 2026-10-01 Fishing capability contribution

目标 Version4 `Terraria/Player.cs` 的 7 个 fishing 字段在 `ResetEffects` 清零；可见 skill 增量位于 `:4573`（buff `+15`）、`:6830`（永久 boost `+3`）、`:6933-6935`（bobber bonus `+10`）、`:7377`（装备 `+5`）和 `:8301-8330`（装备能力及多个 `+10` 分支）。完整参考项目 `D:\TRbackup\无任何删减通过编译` 的 `Player.cs` / `Projectile.cs` SHA-256 分别为 `367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C`、`8D6428FA1D098B26ADFDC1D6CC028251814B40CD3ACE576F0BFF462FB19D6038`；其 Player 将 `fishingSkill` 合入 bait/pole power（`:42706`），Projectile 使用 lava、sonar、crate flags（`:19420,19532,19548,19571,19632,20318`）。目标 Version4 的 `Projectile.cs` 与全树检索没有找到这些字段的直接消费者；该缺失和 CPG 零外部 facts 均不能证明 target 没有消费者，具体映射保持 `unknown`。

只读 CPG Query API 使用 manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`、`SourceSnapshotId=null`，对 7 个字段在 Player / Projectile selected scope 查询。所有 symbol/use 请求状态为 `complete`，use facts 只落在 Player shard；`accFishingBobber` 有一个 `partial` / `AccessMode=Unknown` fact。`complete` 仅表示所选 shard 的索引请求完成，source/CPG 版本绑定、动态消费者和运行时闭包仍 unknown。

NLTX 候选 `PlayerFishingCapabilityRebuildSystem` 提供整值 `Rebuild`、逐源 `Contribute` 和 `Reset`。`Contribute` 对 `FishingSkillDelta` 做 unchecked 加法，6 个 bool 以 OR 合并；重复调用会再次累加 skill，未实现来源 token 去重。目标源映射、生产调用者、每 tick 调用次数、reset/contribution 顺序、snapshot revision 发布与 Projectile handoff 尚未确认；代码检索只发现 verifier caller。因此只保留隔离核心，不把该路径描述为生产迁移。

下表保留 fishing capability contribution 当时记录的有限验证；不包括本轮 consumed-upgrade verifier（见上节），也不代表完整矩阵。本轮未构建或测试完整参考项目：

| 目标 | Wrapper 参数 | 退出码 | warning/error | 观察 |
|---|---|---:|---|---|
| P05 fishing focused build | `build .\src\NSSLC\Component\P05MinionCapacityVerification\Terraria.P05MinionCapacityVerification.csproj --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | 0/0 | 使用 SDK `10.0.400`，经 `Build/Tools/Invoke-SerialDotnet.ps1` 串行 wrapper；产物在 `Build/bin/Terraria.P05MinionCapacityVerification/Debug/net10.0/` |
| P05 fishing focused verifier | `run --project .\src\NSSLC\Component\P05MinionCapacityVerification\Terraria.P05MinionCapacityVerification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | n/a | 输出 `P05 focused verifier passed: capacity, fishing, minion and pet cores.`；覆盖 `+15/+3/+10`、7 项合并/reset，不覆盖重复贡献、生产调度、Projectile 下游、存档/网络或 Version4 行为等价 |

P05 完整行为矩阵维持 `not-run`；以上 build 和 focused verifier 只支持局部核心范围，不代表迁移成功。

### 2026-10-01 boss pet capability 局部核心

目标 Version4 `Terraria/Player.cs`（SHA-256=`E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86`）在 `UpdateBuffs` 的 `:5378-5462` 将 16 个 boss-pet flags 传入共享 pet helper，并在 `ResetEffects` 的 `:10514-10535` 清零。用户指定的完整参考 `D:\TRbackup\无任何删减通过编译\Terraria\Player.cs`（SHA-256=`367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C`）在 `:11012-11096` 有独立对应调用、在 `:18902-18923` 清零。两个 Player 源文件不是相同快照；完整参考只辅助核对字段族，不替代 Version4 事实。

Version4 CPG Query API 使用 manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364` 与 967 shards、`SourceSnapshotId=null`。对 16 个 `SymbolField` 分别执行 `Find-CpgSymbols`，再以 `Get-CpgMemberUses` 限定 `Terraria/Player.cs` / `Terraria/Projectile.cs`。所有 symbol/use 查询为 `complete`，每字段有 2 至 5 项；13 个字段的 facts 位于 Player 与 Projectile shard，3 个字段只位于 Player shard。facts 混有 `confirmed` / `partial` 及 `Write` / `Unknown`；查询结果不能证明唯一 writer、完整消费者、运行时调度或与当前源码 hash 的绑定。

新增 `PlayerBossPetCapabilityRebuildInput` 与 `PlayerBossPetCapabilityRebuildSystem`，仅复制 caller 已解析的 16 项当前 tick flags，并用默认输入 reset。局部 verifier 断言 16 个属性全部置真、reset 后全部清零。该核心不做 buff 映射、buff-time 更新、owned-projectile 检查、实体创建/销毁或 scheduler 接入；这些关系和 lifecycle owner 仍为 `unknown` / `crossSubsystemOwner: integration-review`。

验证环境使用仓库 `global.json` 指定的 SDK `10.0.400`，仅构建 P05 focused verifier 项目，产物位于 `Build/bin/`：

| 目标 | 命令（均经 `Build/Tools/Invoke-SerialDotnet.ps1`） | 退出码 | warning/error | 观察 |
|---|---|---:|---|---|
| P05 focused build | `build .\src\NSSLC\Component\P05MinionCapacityVerification\Terraria.P05MinionCapacityVerification.csproj --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | 0/0 | 产物 `Build/bin/Terraria.P05MinionCapacityVerification/Debug/net10.0/Terraria.P05MinionCapacityVerification.dll` |
| boss pet 单组 verifier | `exec .\Build\bin\Terraria.P05MinionCapacityVerification\Debug\net10.0\Terraria.P05MinionCapacityVerification.dll --boss-pet-only` | 0 | n/a | 输出 `P05 focused verifier passed: boss pet rebuild and reset.`；仅执行本次 16 flags rebuild/reset 用例 |
| 参数传递异常的首次 `run` | `run --project .\src\NSSLC\Component\P05MinionCapacityVerification\Terraria.P05MinionCapacityVerification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false -- --boss-pet-only` | 未记录 | n/a | `--boss-pet-only` 未到达程序；实际运行默认 aggregate verifier 并输出 `P05 focused verifier passed: progression, pet, capacity, fishing, minion and damage cores.`。此执行超出原定约 10% 核心验证范围，不能隐去；之后改为直接 `dotnet exec`，限定运行已复核。 |

完整 P05 行为矩阵仍为 `not-run`；以上局部结果不证明生产接入、Version4 行为等价或迁移成功。完整参考项目只读源码，未在本轮构建或测试。

### 2026-10-01 crossover pet capability 局部核心

Version4 `Player.UpdateBuffs` 在 `:5466-5523` 将 11 个 crossover pet flags 传给共享 pet helper，`ResetEffects` 在 `:10551-10567` 清零全部 13 个字段。`Projectile.cs` 在 `:38807-38908` 对多种 crossover pet flag 执行死亡清理并读取它们维持 lifetime；Chillet / Chillet Ignis 对应 `:38510-38523`。完整参考项目的独立 `Player.cs` hash 为 `367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C`、`Projectile.cs` hash 为 `8D6428FA1D098B26ADFDC1D6CC028251814B40CD3ACE576F0BFF462FB19D6038`；其 Player helper/reset 位于 `:11100-11157` / `:18939-18955`，Projectile 生命周期路径位于 `:56337-56438` / `:56040-56053`。目标与参考快照不拼接。Chillet 两字段在两份 Player 源码中都没有对应 helper 调用，直接源码只显示 Projectile death clear/lifetime read；它们的 true writer 与交错顺序保持 `unknown`。

Version4 CPG Query API 使用 manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`、967 shards、`SourceSnapshotId=null`。13 个 field symbols 与 Player / Projectile scope 的 member-use 请求均为 `complete`，每字段返回 3 至 4 项；facts 含 `confirmed` / `partial` 与 `Write` / `Unknown`，不能据此确定唯一写者、完整动态访问集合或 tick 顺序。

新增 `PlayerCrossoverPetCapabilityRebuildInput` / `PlayerCrossoverPetCapabilityRebuildSystem`，只复制显式提供的 13 个同 tick flag 值，`Reset` 应用 default input。它不映射 buff、处理死亡清理、调整 Projectile lifetime 或接入 scheduler；上游输入解析、Chillet true writer、Player/Projectile 写入顺序、生产 caller 与生命周期 owner 仍为 `unknown` / `crossSubsystemOwner: integration-review`。

只构建 P05 focused verifier 项目并单独运行新增组：

| 目标 | 命令（经 `Build/Tools/Invoke-SerialDotnet.ps1`） | 退出码 | warning/error | 观察 |
|---|---|---:|---|---|
| P05 focused build | `build .\src\NSSLC\Component\P05MinionCapacityVerification\Terraria.P05MinionCapacityVerification.csproj --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | 0/0 | SDK `10.0.400`；产物 `Build/bin/Terraria.P05MinionCapacityVerification/Debug/net10.0/Terraria.P05MinionCapacityVerification.dll` |
| crossover pet 单组 verifier | `exec .\Build\bin\Terraria.P05MinionCapacityVerification\Debug\net10.0\Terraria.P05MinionCapacityVerification.dll --crossover-pet-only` | 0 | n/a | 输出 `P05 focused verifier passed: crossover pet rebuild and reset.`；仅执行 13 项 rebuild/reset 用例 |

此局部验证不运行 aggregate verifier，也不覆盖生产输入映射、Projectile 同 tick 行为、生命周期、save/network 或完整 P05 行为矩阵；完整矩阵保持 `not-run`，不称迁移成功。完整参考项目仅读取源码，未构建或测试。

### 2026-10-01 world-object pet capability 局部核心

P05 claim 中的四个字段为 `petFlagDirtiestBlock`、`petFlagBoulderPet`、`petFlagRainbowBoulderPet`、`petFlagAxeFairyPet`。目标 Version4 `Terraria/Player.cs`（SHA-256=`E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86`）在 `UpdateBuffs` 以 buff `354/373/382/372` 分别调用共享 helper，映射 projectile type `1018/1056/1090/1050`（`:5507-5527`）；`ResetEffects` 清零四项（`:10560-10565`）。目标 helper（`:6324` 起）置位传入 flag、检查 `ownedProjectileCounts` 并计算 `center`，方法随后结束。Version4 `Projectile.cs`（SHA-256=`97C3D68586AEF862411699E428F5C4B18CEE1F8051B684BE9F86FA7B60FAE83B`）文本检索未命中这四个字段。

用户指定的完整参考项目 `D:\TRbackup\无任何删减通过编译` 是独立快照，其 `Player.cs` SHA-256=`367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C`、`Projectile.cs` SHA-256=`8D6428FA1D098B26ADFDC1D6CC028251814B40CD3ACE576F0BFF462FB19D6038`。其 `Player.cs:12160-12188` helper 含 `flag && whoAmI == Main.myPlayer` 创建条件并调用 `Projectile.NewProjectile`；其 `Projectile.cs:47082-47108` 对前三个字段有 death clear/lifetime 分支，`:67361` 在 Axe Fairy 分支读取 flag 续命。以上不回填为目标 Version4 行为。

Version4 只读 CPG Query API 使用 manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，对四个字段逐项执行 `Find-CpgSymbols -Kind SymbolField` 与 `Get-CpgMemberUses`，后者限定 `Terraria/Player.cs` / `Terraria/Projectile.cs`。每项 symbol 与 use 查询均为 `complete`，各有 1 个 symbol、2 个 use facts；每字段都只有 Player shard 的一个 `confirmed/Write` 和一个 `partial/Unknown`，Projectile shard 没有返回 fact。CPG `SourceSnapshotId=null`，complete 只表示限定 scope 内查询完成，不能据零命中断言不存在 Projectile consumer 或 writer。

NLTX 已有 `PlayerWorldObjectPetCapabilityRebuildInput` 与 `PlayerWorldObjectPetCapabilityRebuildSystem` 候选；它们把调用方解析的四个 bool 写入 capability Component，并可 reset 到 default input。P05 aggregate verifier 的 `--world-object-pet-only` 入口没有执行；本次使用独立项目，仅链接 Component/Input/System 三个源码文件。该候选没有 buff/content 输入映射、tile/world-object query、Projectile 协调或生产 caller/scheduler。实体创建与死亡清理 owner、flag consumer、tick 顺序、完整行为保持 `unknown` / `crossSubsystemOwner: integration-review`。

独立验证使用 SDK `10.0.400` 和 `Build/Tools/Invoke-SerialDotnet.ps1`；执行前确认没有活跃 compile-capable 进程。以下 `build` / `run` 命令使用 `DOTNET_ROOT=C:\Users\shan\.dotnet` 并将该目录置于本进程 `PATH` 首位：

| 目标 | Wrapper 参数 | 退出码 | warning/error | 产物与观察 |
|---|---|---:|---|---|
| world-object pet isolated restore | `restore .\Test\Terraria.Player.WorldObjectPet.Verification\Terraria.Player.WorldObjectPet.Verification.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | 未报告 | 项目 restore 完成 |
| world-object pet isolated build | `build .\Test\Terraria.Player.WorldObjectPet.Verification\Terraria.Player.WorldObjectPet.Verification.csproj --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | 0/0 | `Build/bin/Terraria.Player.WorldObjectPet.Verification/Debug/net10.0/Terraria.Player.WorldObjectPet.Verification.dll` |
| world-object pet isolated verifier | `run --project .\Test\Terraria.Player.WorldObjectPet.Verification\Terraria.Player.WorldObjectPet.Verification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | n/a | 输出 `PASS: world-object pet rebuild and reset.`；四个 flag 均由输入置真，随后 `Reset` 全部清零 |

此前一次 P05 aggregate verifier 项目 build 尝试退出码为 `1`、`0` warnings / `3` errors，诊断位于 `PlayerSunScorchResult.cs(25,22)` 的 byte enum 常量范围和 `PlayerSunScorchInput.cs(22,17)` 的 `TileFact` 解析。该阻断与本次独立项目的源码输入不同；此处不据失败作出 world-object pet 编译结论。新的单组 verifier 通过不验证生产输入映射、Projectile 生命周期、调度、save/network 或 Version4 行为。完整 P05 行为矩阵仍为 `not-run`，不称迁移成功。

### 2026-10-01 legacy pet/effect flags 核心切片

Version4 `Terraria/Player.cs` SHA-256=`E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86`。源码直接检索确认 20 项 tick flag 在 `UpdateBuffs` / buff 分支写入（`:5298-6127`）并在 `ResetEffects` 清零（`:10409-10576`）；`HasGardenGnomeNearby` 不在 reset 列表中，而是由 `NetMessage` packet 134 序列化（`:1552`）、`MessageBuffer` packet 134 反序列化写入（`:3142`），供 `RecalculateLuck` 读取（`:17914`）。`Projectile.cs` 在宠物死亡/更新路径写入和读取多个宠物 flag，`NPC.cs:618` 读取 `sunflower`。这组字段涉及多个效果和跨系统消费者，tick reset 不能清理 garden-gnome 同步状态；死亡清理和 tick 更新的执行顺序仍是 `unknown`。

用户指定的完整参考项目 `D:\TRbackup\无任何删减通过编译` 为独立快照，`Player.cs` SHA-256=`367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C`，`Projectile.cs` SHA-256=`8D6428FA1D098B26ADFDC1D6CC028251814B40CD3ACE576F0BFF462FB19D6038`。其 `Player.cs:17019-17022` 通过 `SceneMetrics.HasGardenGnome` 更新 garden-gnome 状态；Version4 可见写入路径则在 `MessageBuffer`。该差异只作为参考快照事实，不移植为 Version4 行为。

只读 Version4 CPG Query API 使用 manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`（967 shards，`SourceSnapshotId=null`）。21 个字段的 `Find-CpgSymbols -Kind SymbolField` 与 `Get-CpgMemberUses`（scope: `Terraria/Player.cs`、`Terraria/Projectile.cs`）均为 `complete`，每项有一个 symbol 和 1 至 6 个 use facts，facts 混有 `confirmed` / `partial`、`Write` / `Unknown`。`HasGardenGnomeNearby` 对 `MessageBuffer.cs` / `NetMessage.cs` 的扩展查询、`sunflower` 对 `NPC.cs` 的扩展查询也为 `complete`。全数据集省略 `SourcePath` 的 API 请求实际返回 `SourcePathMustContainStrings`，随后按相关路径重查。CPG source snapshot 未绑定源码 hash，`complete` 不证明调用闭包、唯一 writer 或执行顺序。

新增 `PlayerLegacyPetCapabilityRebuildInput`（21 个 bool）与 `PlayerLegacyPetCapabilityRebuildSystem`。`Rebuild` 只复制调用方显式解析的输入；`ResetTickFlags` 只清 20 个 Version4 reset 字段并保留 `HasGardenGnomeNearby`。无 buff 映射、spawn/despawn、Projectile death clear、`sunflower` NPC 侧效果、网络同步或 scheduler caller。生产输入、写入 owner、Player/Projectile 同 tick 顺序与行为等价仍为 `unknown` / `crossSubsystemOwner: integration-review`。

新增独立 verifier `Test/Terraria.Player.LegacyPet.Verification/` 只链接 Component/Input/System 源码，覆盖 21 项全部置真、重建后全真、reset 后 20 项清零且 garden-gnome 保持为真。执行前检查未发现活跃 `dotnet.exe` / `csc.exe` 编译进程；使用 SDK `10.0.400`（`DOTNET_ROOT=C:\Users\shan\.dotnet`），所有 dotnet 操作经 `Build/Tools/Invoke-SerialDotnet.ps1`：

| 目标 | Wrapper 参数 | 退出码 | warning/error | 输出与观察 |
|---|---|---:|---|---|
| legacy pet isolated restore | `restore .\Test\Terraria.Player.LegacyPet.Verification\Terraria.Player.LegacyPet.Verification.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | 未报告 | 新增 verifier 项目 assets 还原完成 |
| legacy pet isolated build | `build .\Test\Terraria.Player.LegacyPet.Verification\Terraria.Player.LegacyPet.Verification.csproj --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | 0/0 | 产物 `Build/bin/Terraria.Player.LegacyPet.Verification/Debug/net10.0/Terraria.Player.LegacyPet.Verification.dll` |
| legacy pet isolated verifier | `run --project .\Test\Terraria.Player.LegacyPet.Verification\Terraria.Player.LegacyPet.Verification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | n/a | 输出 `PASS: legacy pet rebuild and tick reset preserve garden-gnome state.` |

这是约定的窄核心验证范围，不代表完整 P05 行为矩阵。完整参考项目仅读取源码，未构建或测试；完整 P05 行为矩阵继续为 `not-run`，不称迁移成功。

### 2026-10-01 accessory tick-effect snapshot 核心切片

目标 Version4 `Player.cs` SHA-256=`E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86`。17 个 accessory/effect 字段中，15 个由 `ResetEffects` 清零：`flowerBoots`、`fairyBoots`、`hellfireTreads`、`moonLordLegs`、`deadMansSweater`、`arcticDivingGear`（`:10314-10319`），`coolWhipBuff` / `cobWhipBuff`（`:10398-10399`），`magicCuffs`、`coldDash`、`desertDash`、`desertBoots`、`sailDash`、`eyeSpring`、`scope`（`:10577-10591`）。`Projectile.cs` 还会写入或清除 cool/cob whip 与 Eye Spring flags；`NPC.cs` 读取 `scope`，相关写者和顺序仍需 integration review。

目标 `wearsRobe` 在装备匹配路径先清零再由 `SetMatch` 写入（`Player.cs:20084-20100`），不是上述 ResetEffects 列表成员。Version4 `brokenMirrorBadLuck` 只有声明和 luck 读取（`Player.cs:1505,17924`），没有可见 writer；完整参考快照增加 `brokenMirrorBadLuckTime` 和 `UpdateBrokenMirrorLuck`（参考 `Player.cs:29432-29446`），不将它合并进 Version4 生命周期。目标与完整参考 Player SHA-256 分别为 `E5B301E3401F61E37BF8C421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C` 与 `367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C`。

只读 Version4 CPG Query API 使用 manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`、967 shards、`SourceSnapshotId=null`。17 个 `SymbolField` 和 `Player.cs` / `Projectile.cs` / `NPC.cs` member-use 查询均为 `complete`；每个字段 1 个 symbol、1 至 5 个 facts，facts 有 `confirmed` / `partial` 和 `Write` / `Unknown`。`complete` 仅表示所选 shard 查询完成，不能闭合跨 System writer、消费者或顺序。

新增 `PlayerAccessoryEffectSnapshotRebuildInput` 与 `PlayerAccessoryEffectSnapshotRebuildSystem`，只写入 15 个 ResetEffects tick flags，`ResetTickFlags` 清 15 项但保留 `BrokenMirrorBadLuck` / `WearsRobe`。它不解析 equipment/buff 输入、不接管 Projectile 的 whip/Eye Spring 写入或死亡清理、不实现 NPC scope consumer，也没有生产 caller/scheduler。新增独立 verifier 只链接 Component/Input/System，验证 15 项 rebuild/reset 并确保两项独立 lifecycle 字段被保留。

验证使用 SDK `10.0.400`（`DOTNET_ROOT=C:\Users\shan\.dotnet`），执行前未发现活跃编译进程；restore/build/run 均经 `Build/Tools/Invoke-SerialDotnet.ps1`：

| 目标 | Wrapper 参数 | 退出码 | warning/error | 输出与观察 |
|---|---|---:|---|---|
| accessory effect isolated restore | `restore .\Test\Terraria.Player.AccessoryEffect.Verification\Terraria.Player.AccessoryEffect.Verification.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | 未报告 | verifier 项目 assets 还原完成 |
| accessory effect isolated build | `build .\Test\Terraria.Player.AccessoryEffect.Verification\Terraria.Player.AccessoryEffect.Verification.csproj --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | 0/0 | 产物 `Build/bin/Terraria.Player.AccessoryEffect.Verification/Debug/net10.0/Terraria.Player.AccessoryEffect.Verification.dll` |
| accessory effect isolated verifier | `run --project .\Test\Terraria.Player.AccessoryEffect.Verification\Terraria.Player.AccessoryEffect.Verification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | n/a | 输出 `PASS: accessory tick snapshot rebuild preserves separate-lifecycle fields.` |

### 2026-10-01 unlock/preference core 状态复核

`PlayerProgressionCommitSystem`、`UnlockPlayerProgressCommand`、`SetPlayerSuperCartPreferenceCommand` 与 Biome Torch / Super Cart Query 已存在；解锁按 ledger 状态拒绝重复，偏好按命令值直接赋值。Version4 `Player.cs:1475-1481,3042-3070` 可见 4 项字段和 effective accessors，`NetMessage.cs:179-192` / `MessageBuffer.cs:275-287` 可见网络读写。CPG 对 4 个字段的 Player / NetMessage / MessageBuffer scoped symbol 与 member-use 请求均为 `complete`，每字段 3 至 4 facts，但包含 `partial` / `Unknown` 且 `SourceSnapshotId=null`。本轮已扩展 `Test/Terraria.Player.Progression.Verification/Program.cs`，直接验证三种 unlock、拒绝和 effective preference query；该结果只支持局部 commit/query 核心。持久化映射、外部授权、生产 caller、网络 projection 顺序和状态恢复仍为 `unknown`，不表示该能力已生产接入。

#### unlock/preference 直接核心 verifier

验证使用 SDK `10.0.400`、`DOTNET_ROOT=C:\Users\shan\.dotnet`，每次 compile-capable 命令前检查均未发现活跃 `dotnet.exe` / `csc.exe`，所有命令经 `Build/Tools/Invoke-SerialDotnet.ps1 -DotnetArguments` 串行执行：

| 目标 | Wrapper 参数 | 退出码 | warning/error | 输出与观察 |
|---|---|---:|---|---|
| progression verifier restore | `restore .\Test\Terraria.Player.Progression.Verification\Terraria.Player.Progression.Verification.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | 未报告 | verifier 与引用项目 assets 就绪；其他 2 个项目已是最新。 |
| progression verifier build | `build .\Test\Terraria.Player.Progression.Verification\Terraria.Player.Progression.Verification.csproj --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | 0/0 | `Build/bin/Terraria.Player.Progression.Verification/Debug/net10.0/Terraria.Player.Progression.Verification.dll`。 |
| progression verifier | `exec .\Build\bin\Terraria.Player.Progression.Verification\Debug\net10.0\Terraria.Player.Progression.Verification.dll` | 0 | n/a | `PASS: golfer commit; unlock/preference commit and effective queries`。 |

本次单 verifier 属于局部核心抽样，不运行 P05 aggregate、生产调度或完整行为矩阵；完整矩阵继续 `not-run`。

### 2026-10-01 companion capability 核心切片

Version4 `Player.cs` SHA-256=`E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86`。14 个 companion flags 声明于 `:1670-1710`，在 `UpdateBuffs` 写入（`:5322,5678-5865`）并在 `ResetEffects` 清零（`:10415-10428,10568-10594`）。目标 `Projectile.cs` 的宠物死亡路径对 13 个字段清 flag 并读取其 lifetime；`companionCube` 在 Version4 Projectile 源码 / 限定 CPG scope 未见相应使用。

完整参考项目 `D:\TRbackup\无任何删减通过编译` 的 `Player.cs` / `Projectile.cs` SHA-256 为 `367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C` / `8D6428FA1D098B26ADFDC1D6CC028251814B40CD3ACE576F0BFF462FB19D6038`。它在 `Projectile.cs:47070-47072` 对 `companionCube` 有 death clear/lifetime read；该路径仅属于参考快照，未回填到 Version4。

只读 Version4 CPG Query API 使用 manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`、967 shards、`SourceSnapshotId=null`。14 个 `SymbolField` 与 Player / Projectile scoped member-use 查询均为 `complete`，各有 1 个 symbol、2 至 4 个 use facts，包含 `confirmed` / `partial` 与 `Write` / `Unknown`。查询不证明完整 consumer/owner 或执行时序。

新增 `PlayerCompanionCapabilityRebuildInput` 与 `PlayerCompanionCapabilityRebuildSystem`，复制显式解析的 14 项 tick flags，并 reset 清零。独立 verifier 链接 Component/Input/System，验证全部置真和 reset 清零；它不映射 buff、不处理 Projectile 创建 / 销毁 / death clear，不接入 production caller 或 scheduler。pet entity lifecycle owner、输入来源、Player/Projectile 写入顺序和完整行为仍为 `unknown` / `crossSubsystemOwner: integration-review`。

验证使用 SDK `10.0.400`（`DOTNET_ROOT=C:\Users\shan\.dotnet`），执行前未发现活跃编译进程；restore/build/run 经 `Build/Tools/Invoke-SerialDotnet.ps1`：

| 目标 | Wrapper 参数 | 退出码 | warning/error | 输出与观察 |
|---|---|---:|---|---|
| companion capability isolated restore | `restore .\Test\Terraria.Player.CompanionCapability.Verification\Terraria.Player.CompanionCapability.Verification.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | 未报告 | verifier 项目 assets 还原完成 |
| companion capability isolated build | `build .\Test\Terraria.Player.CompanionCapability.Verification\Terraria.Player.CompanionCapability.Verification.csproj --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | 0/0 | 产物 `Build/bin/Terraria.Player.CompanionCapability.Verification/Debug/net10.0/Terraria.Player.CompanionCapability.Verification.dll` |
| companion capability isolated verifier | `run --project .\Test\Terraria.Player.CompanionCapability.Verification\Terraria.Player.CompanionCapability.Verification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | n/a | 输出 `PASS: companion capability rebuild and reset.` |

### 2026-10-01 Mount/Minecart owner 与源码关系复核

使用只读 Version4 CPG Query API（manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`）查询 `onWrongGround`、`onTrack`、`cartRampTime`、`cartFlip`、`trackBoost`、`lastBoost`、`mount`，scope 限定 Player、Mount、Minecart、Collision、网络读写 shard。`cartRampTime` member-use 为 `partial` / 0 并带 `NoMatchingFactInScannedScope`；`mount` 返回 200 facts 后以 `ItemBudgetExhausted` 为缺口；其余字段虽有 `complete` member-use 请求，facts 仍含 `partial` / `Unknown`，不能证明闭合 owner 或执行顺序。字段级状态、源文件 hash 与源码定位见设计文档的 Mount/Minecart owner 复核小节。

源码 hash 已分别记录目标 Version4 与用户指定完整参考快照，未混用两份基线。目标 Player 的地面、轨道、坡道计时、翻转、加速与 `lastBoost` 分布于 Player movement / Minecart collision / Mount reset 路径；`Player.mount` 是被广泛读取和调用的 Mount runtime 对象。NLTX 有六项 `PlayerMountVehicleIntegrationComponent`，但本轮补查没有证明 P03/Movement 唯一 owner 与提交协议。根据批次 7 门槛，本轮没有新增 P05 Vehicle writer/API，也未修改 Vehicle 代码；owner、输入映射、碰撞与 movement 顺序、网络/存档映射继续为 `unknown` / `integration-review`。

| 目标 | 操作 | 结果 | 说明 |
|---|---|---|---|
| Mount/Minecart static cross-check | CPG Query API + Version4 / 完整参考快照源码读取 | 已完成；仅静态 | 本次不运行 build/test；Vehicle 行为验证保持 `not-run`。 |

### 2026-10-01 progression packet projection 签名

目标 Version4 `NetMessage.cs:179-192` 与 `MessageBuffer.cs:275-287` 中相同的 P05 字段读写映射，为 10 个 progression bool 位提供了 signature-only projection 的输入/输出形状。完整参考项目对应 `NetMessage.cs:185-197`、`MessageBuffer.cs:344-357` 也包含同组字段，但属于独立快照。目标 / 参考 `NetMessage.cs` 与 `MessageBuffer.cs` 的 SHA-256 分别记录在设计文档；不合并源码基线。

只读 CPG Query API 使用 manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，对 10 个字段在 `Terraria/Player.cs`、`Terraria/NetMessage.cs`、`Terraria/MessageBuffer.cs` 范围内的 symbol/member-use 请求均为 `complete`：`unlockedBiomeTorches` 4 项、`ateArtisanBread` 3 项、`unlockedSuperCart` 3 项、`enabledSuperCart` 4 项、`usedAegisCrystal` 4 项、`usedAegisFruit` 4 项、`usedArcaneCrystal` 5 项、`usedGalaxyPearl` 4 项、`usedGummyWorm` 4 项、`usedAmbrosia` 4 项。facts 含 `partial` / `Unknown`，数据库 `SourceSnapshotId=null`，因此这些事实不能证明版本兼容、完整 caller closure、调用方 owner 或发送时序。

新增 `IPlayerProgressionPacketProjection.Project` 仅声明从 `PlayerUnlockProgressionLedgerComponent` 和 `PlayerConsumedProgressionLedgerComponent` 投影出 `PlayerProgressionPacketFlags`；没有实现类和方法体，不编码 wire order，也不处理同一 Player 包内的 `UsingBiomeTorches` / `happyFunTorchTime`。Version4 build 只确认 interface 和 DTO 可编译，packet 行为、网络 owner 与生产接入仍为 `unknown` / `not-run`。

| 目标 | Wrapper 参数 | 退出码 | warning/error | 产物与观察 |
|---|---|---:|---|---|
| Progression packet projection signature build | `build .\src\NSSLC\Component\Player\Terraria.Player.csproj --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | 0/0 | SDK `10.0.400`；`Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll` 存在；本次仅编译 API 声明，没有运行 packet 测试或实现 packet 逻辑。 |

### 2026-10-01 P05 生产入口复核

对当前 `src/NSSLC` 文本检索 `Register*System`、`SystemScheduler`、`ISystemScheduler`、`PlayerUpdateLoop`、`PlayerTickCoordinator` 及 P05 `*RebuildSystem` caller。未发现通用 scheduler、System 注册器或 Player runtime loop。`PlayerTickCoordinator` 定义在 `Component/Player/PlayerTickCoordinator.cs`，目前只有 `Component/PlayerItemSpaceVerification/Program.cs:1280,1322` 两个调用点；`PlayerMinionCapabilityRebuildSystem`、Fishing 与 progression commit 等 P05 API 在 `src/NSSLC` 内的调用均落在 `Component/P05MinionCapacityVerification/Program.cs`。`Terraria.Player.csproj` 的项目引用只证明程序集依赖，不证明运行时注册或调度。

补充检查 `src/NSSLC.Infrastructure` 后，发现 `WorldSession/Runtime/DeferredProcessSchedulerPort` 和 `DeferredProcessPort`；前者按显式委托与 `FrameScope` / `DeferredProcessLifetime` 排队和 drain 延迟工作（`DeferredProcessSchedulerPort.cs:3-72`），不是 Player ECS System 注册或逐 Player tick 调度。`PlayerInputGameplay/Runtime/MainInputFrameSystem` 仅捕获帧输入、推进帧计时并在帧尾清 transient edge（`MainInputFrameSystem.cs:3-26`）。对该 Infrastructure 源码的文本检索未找到 P05 progression、P05 rebuild 或 `PlayerTickCoordinator` 的使用点；此结果不证明外部宿主、源生成注册或动态组合不存在。故这两个已有 runtime API 都不能作为 P05 生产接入证据；P05 的 production scheduler、Player/session 到 entity 的映射和调用顺序继续为 `unknown`，不得从通用延迟队列或输入帧 API 推导出来。

据此，P05 局部核心继续属于候选实现，不能因 verifier 直接调用而视为生产接入。本轮不把 P05 接到只由 verifier 使用的 PlayerTickCoordinator，也不新增第二个 writer；真实 outer scheduler、Player/Session identity mapping、输入适配与 P05 执行顺序继续为 `unknown` / `integration-review`。此前跨分区 owner 与生产接入门仍未满足，完整 P05 行为矩阵保持 `not-run`。

### 2026-10-01 progression persistence snapshot mapping 核心

新增 `PlayerProgressionPersistenceSnapshot` 和纯映射入口 `PlayerProgressionPersistenceProjection.Project`。它从 unlock ledger、consumed-upgrade ledger、quest/event progress component 与显式 Biome Torch preference 输入构造 14 字段 immutable value snapshot；不写文件、不编码字段、不读数据包、不实现 deserialize/restore，也没有生产调用方。Biome Torch preference 作为独立输入，避免把偏好与 unlock ledger 状态合并。对应 focused verifier 断言全部 14 个映射值和 unlock 为真时 preference 可独立切换；该用例只验证内存 projection。

完整参考项目 `D:\TRbackup\无任何删减通过编译\Terraria\Player.cs` 中，Serialize 的字段顺序可见于 `:55377-55386`（十个 bool）、`:55491`（Angler）、`:55508`（Golfer）及 `:55512-55516`（Super Cart bits）。Deserialize 的版本条件可见于 `:55857-55878`、`:56235-56237`、`:56303-56305` 和 `:56321-56329`：Biome Torch `>=229`、Artisan Bread `>=256`、consumed flags `>=260`、DD2 `>=182`、Angler `>=98`、Golfer `>=206`、Super Cart bits `>=253`，其更旧版本 unlock 从 inventory 派生。此参考快照仅用于列出待适配语义，不是 Version4 格式依据，也未将这些版本门槛复制到实现。目标 Version4 `Player.cs:26418` 的 `Serialize` 为空，`:26455-26458` 的 `Deserialize` 不读取 Player 字段，`:26460` 的 `FixLoadedData` 为空；目标存档布局、默认值和恢复顺序继续为 `unknown`。

本轮重新初始化只读 Version4 CPG Query API（manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`、967 shards、`SourceSnapshotId=null`）：`Find-CpgSymbols` 对 Player shard 的 `Serialize` 与 `Deserialize` 各返回 1 个 symbol、状态 `complete`；`usedAegisCrystal` symbol 和 Player/NetMessage/MessageBuffer 限定 member-use 查询为 `complete`、4 条 facts，包含 `Write` 与 `Unknown` 访问方向。CPG 结果不能消除源码快照绑定缺口或证明 stub 行为。

验证使用 SDK `10.0.400`；进程级设置 `DOTNET_ROOT=C:\Users\shan\.dotnet`，并将该目录放在 `PATH` 首位。构建前发现一项独立 `Terraria.Npc.SpawnEligibility` build 与 `csc.exe`，等待其退出后才开始。构建和 verifier 均从仓库根目录经 `Build/Tools/Invoke-SerialDotnet.ps1 -DotnetArguments` 串行执行：

| 目标 | Wrapper 参数 | 退出码 | warning/error | 产物与观察 |
|---|---|---:|---:|---|
| progression snapshot verifier build | `build Test/Terraria.Player.Progression.Verification/Terraria.Player.Progression.Verification.csproj --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | 0/0 | `Build/bin/Terraria.Relationships/Debug/net10.0/Terraria.Relationships.dll`、`Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll` 和 `Build/bin/Terraria.Player.Progression.Verification/Debug/net10.0/Terraria.Player.Progression.Verification.dll`。 |
| progression snapshot focused verifier | `run --project Test/Terraria.Player.Progression.Verification/Terraria.Player.Progression.Verification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | 0 | n/a | 输出 `PASS: progression commits and persistence snapshot projection`。 |

系统 dotnet host 首次启动时未发现仓库要求的 SDK `10.0.400`，verifier 尚未启动；之后仅为该进程设置 `DOTNET_ROOT` 与 `PATH` 后运行成功，未修改 `global.json`。完整参考项目在本轮只读源文件，未执行其 build/test。此局部 build/verifier 不覆盖任何序列化行为、旧版 release gate、packet 行为、生产 scheduler 或完整 P05 行为矩阵；这些仍为 `unknown` / `not-run`，总体设计仍为 `proposed`，不称迁移成功。

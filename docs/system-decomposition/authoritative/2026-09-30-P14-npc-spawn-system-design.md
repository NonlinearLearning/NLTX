# P14 NPC 生成 System 设计

```yaml
documentType: system-design
partitionId: P14
sourceReport: ../reports/2026-09-18-system-decomposition-authoritative-P14-npc-spawn-eligibility.md
designStatus: proposed
systemImplementationStatus: partial
existingNltxSourceStatus: partial
verificationStatus: not-run
```

## 1. 设计结论

P14 以用户指定的 `Version4` 为目标行为基线，采用一个有边界的 NPC 生成协调 System 作为候选归属；生成输入按职责组成短生命周期
快照，纯规则作为只读计算，NPC 创建、slot 记账、生命周期、目标写入、网络和存档继续交给
各自现有的权威 owner。当前 `NpcSpawnSystem` 保持单一自然生成协调边界，并分别提供入口组合
`ProcessEntry(INpcSpawnEntryPort)`、可观察入口结果 `ProcessEntryDetailed(INpcSpawnEntryPort)`、
逐槽 pass `ProcessNaturalSpawnPass()` 和结构化 pass 结果 `ProcessNaturalSpawnPassDetailed()`：入口组合按旧顺序消费
`noSpawnCycle`、委托 respawn 检查，再调用逐槽协调；两个端口目前均无生产 adapter 或运行时 caller。
资格 Query 与接收 attempt-local rate snapshot 和显式随机端口的 `NpcSpawnRateSystem` 也已存在。自然
pass 顺序现在为
资格判断、slime-rain effect、per-player spawn flags 准备、rate 输入采集与计算、capacity gate、rate roll、area 输入采集、
有序 tile search、screen exclusion、chosen-tile post-check、chosen-tile flags 计算，再把通过检查的候选交给 continuation
port。`NpcSpawnChosenTileFlagsSystem.Calculate` 经显式 port 读取 world/tile facts 和随机值，按源码顺序生成
attempt-local flags；它是 effectful 步骤，不是可任意重排或重复调用的纯 Query。handoff 使用
`NpcSpawnAcceptedCandidate` 传递 player index、rate inputs/result、tile-search result、post-check
facts 与完整 `ChosenTileFlags`；`NoWormsForSpawn` 由 `ChosenTileFlags.NoWorms` 派生。continuation
是 `void`，调用后立即结束当前自然 pass 的玩家
循环，以保留 legacy loop-control。结构化结果以 `ContinuationWasInvoked` 记录是否触发 handoff，实体创建观察仍为
`unknown`，不能由旧 `TrySpawnAnNPC == true` 或 continuation 被调用推断创建了 NPC。area 计算、tile search、screen exclusion、post-check 与 chosen-tile flags 已有隔离 core API/System，并由自然 pass 串接；这些步骤仍依赖
verifier port，尚无生产 adapter、旧入口 facade、runtime 注册或权威 spawn commit。边界判定仍为
`partial`；代码文件存在不代表已接入运行时。

不按十二个 ledger 组机械创建十二个 System。它们汇聚到相同的 per-player 自然生成尝试，
并受同一候选选择顺序、随机流和生成提交边界约束。输入组是源码追踪和所有权检查单位，
不是运行时类型数量目标。

## 2. 范围与基线

本设计只覆盖 authoritative P14：`NpcSpawnAndCritterState`、
`NpcSpawnBudgetAndActivityState`、`NpcSpawnCooldownAndEnvironment`、
`NpcTargetAndIdentityProperties`、`NpcProgressionAndEnvironmentProperties`、
`NpcSpawnContextAndCapacityInputs`、`NpcSpawnSpatialEligibilityInputs`、
`NpcSpawnBiomeAndDungeonEligibilityInputs`、`NpcSpawnPolicyAndEventEligibilityInputs`、
`NpcSpawnBiomeZoneInputs`、`NpcSpawnEventAndTowerInputs`、
`NpcSpawnTargetSelectionState`，合计 117 项。完整成员身份以 source report 的覆盖表为准，
这里不重新定义相邻分区成员。

| 材料 | 本设计中的用途 | 证据边界 |
| --- | --- | --- |
| `D:\TRbackup\Version4` | P14 目标源码与 CPG 的索引项目 | 只读 CPG SQLite `D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite`；manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`；`SourceSnapshotId` 为空。 |
| `D:\TRbackup\无任何删减通过编译` | 用户补充的完整 Terraria 源码参考，含 `TerrariaServer.sln`、`TerrariaServer.csproj`；`AssemblyInfo.cs:18` 声明版本 `1.4.5.6` | 按用户说明作为无删减、已编译参考。本轮直接只读核对完整 `NPC.cs` 的自然/特殊生成入口，并检查 `Main.cs`、`MessageBuffer.cs` 调用上下文；未独立构建。它不是 CPG manifest 绑定的源码副本。 |
| `C:\Users\shan\Downloads\ECS\space-station-14-master` | System 调度、spawn 与 NPC lifecycle 的组织参考 | 只借鉴边界形态，不作为 Terraria 行为或顺序证据。 |
| `src\NSSLC\Component\Npc` | NLTX 当前状态与快照材料 | 组件/快照的存在不证明 System 注册、调度、adapter 消费或运行时提交。 |

SS14 的 `Content.Server/Spawners/EntitySystems/SpawnerSystem.cs` 具体展示了以 `Update` 驱动
`EntityQueryEnumerator<TimedSpawnerComponent>`、注入 timing/random、再调用 `SpawnAtPosition` 的
System 组织；`SpawnAfterInteractSystem` 则把输入事件、空间校验和 `Spawn` 放在同一入口。这里仅借鉴
“调度/查询/外部创建边界应显式可见”的组织方式；它不证明 Version4 的 255 槽顺序、随机消费、NPC
slot 或网络语义，因此不提升 P14 的证据状态。

关键源码哈希显示目标树与完整源码参考并非同一文件基线：

| 文件 | `Version4` SHA-256 | 完整源码参考 SHA-256 |
| --- | --- | --- |
| `Terraria/NPC.cs` | `29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17` | `ED8AA2302730A9E046EF310E543204FBA391BD35B1C9C0B213C8065DFBBE39F0` |
| `Terraria/Main.cs` | `66DBE1D1E6FB89A24A512309BB039DFEC7C06D4678D2A22D7ADECBAD3AFD6520` | `E24E61C9903BB7995F47E36C51EDBE43481B643226861B717020877E7E22B63F` |
| `Terraria/MessageBuffer.cs` | `0F474225FA9D98C539F61249619273A44E73F69A4CF388B44258433A2D15D5EE` | `48AABBBF4E0967964D97598E0168E9252ADD3DD868C66C6DA46AA87670EACAAB` |
| `Terraria/NPCSpawningFlagsForDualDungeons.cs` | source file not present in `Version4` | `856996634E2186FD3AB2EC93FC4DABB0AF04C7B0B0D60DE6B49967462933C007` |

因此，CPG 事实仅绑定其索引的 `Version4`，不能因源码名称相同就升级为完整副本的 CPG 事实。
`Version4` 是用户指定的目标行为基线；完整源码参考用于补足调用关系和解释实现，不自动覆盖
目标树中缺失、为空或条件不同的实现。完整副本中与 P14 有关的调用关系由本轮逐段源码核对；自然生成入口见
`Terraria/NPC.cs:81433-81444`，逐玩家 pass 与 continuation 见 `NPC.cs:185-256`，area/tile
search 与 footprint/screen 检查见 `NPC.cs:855-968`、`NPC.cs:5348-5413`。精确 source revision
绑定仍为 `unknown`；任何拟从完整参考引入的差异行为，必须先写明它与 Version4 目标语义的
差别和裁定，不得静默导入。

## 3. 概念行为与源行为顺序

### 3.1 自然生成入口

`Version4/Main.cs:11451-11458` 在 revenge 更新之后调用 `NPC.SpawnNPC()`，并在调用处捕获
异常；完整副本在 `Main.cs:18049-18059` 的局部调用外还显示 `netMode != 1` 守卫。两处守卫
上下文并不完全相同，不能只凭这个局部片段断言整体 server/client 行为相同。

两份 `NPC.cs` 的静态入口均执行以下源码顺序（完整参考项目 `NPC.cs:81433-81444`）：

1. 若 `noSpawnCycle` 为真，先清为 false 并返回。
2. 调用 `RevengeManager.CheckRespawns()`。
3. 建立短生命周期 `Spawner` 并调用实例 `Spawner.SpawnNPC()`。

完整参考项目 `NPC.cs:185-256` 中，`Spawner.SpawnNPC()` 遍历 255 个玩家槽。每个可尝试玩家先执行 slime-rain 专用分支，再调用
`TrySpawnAnNPC`；该方法返回 true 后结束玩家循环。不得把这个 true 直接解释为“恰好创建了
一个 NPC”：`TrySpawnAnNPC` 在 `SpawnAnNPC` 返回后直接返回 true，而 `SpawnAnNPC` 的返回类型
为 void。Version4 `NPC.cs:1190-5151` 与完整参考 `NPC.cs:1208-5169` 的方法区间各含 669 个
`SpawnNPC(` 调用表达式。该数字是静态文本计数，不代表分支都会执行或单次调用实际创建的实体数量；完整参考中这些调用位于 biome/event/tower 等条件分支。Version4 在该
void 调用后无条件执行 `SyncNewlySpawnedNPCs`，再返回 true；完整参考只在 `Main.netMode == 2`
时同步后返回 true。故 true 表达候选尾部已走完及玩家循环应退出，不表达创建数量或提交成功。

`TrySpawnAnNPC` 首先执行 `SetSpawnFlags(player)`，再调用 `GetSpawnRate`。只读 CPG Query API
对 Version4 `NPC.cs` 的符号查询返回一个精确 `SetSpawnFlags` 声明和一个来自
`TrySpawnAnNPC` 的静态调用点；其 callable facts 为 `partial/CalleeEffectsNotExpanded`。核对
Version4 与完整参考源码后确认，`SetSpawnFlags` 采集和重置每玩家生成 flags，并调用
`ShouldSpawnInvasionEnemies`；这个分支可消费 `Main.rand`。新 pass 由
`NpcSpawnPerPlayerFlagsSystem.Prepare(playerIndex, port)` 表达该 effectful 步骤，严格放在
slime-rain effect 之后、rate/capacity/rate roll 之前。`INpcSpawnPerPlayerFlagsPort` 当前只有
声明和 verifier fake，没有生产 adapter。它不同于 tile 选中并通过 post-check 后运行的
`SetSpawnFlagsForChosenTile`，不得合并或提前。

先前只读 CPG Query API 对 `TrySpawnAnNPC`、`SetSpawnFlagsForChosenTile` 和 `SpawnAnNPC` 的符号及选定
`NPC.cs` 调用点查询均完成，每个目标各返回一个静态调用点。对应的三个 `Get-CpgCallableFacts` 结果均为
`partial/CalleeEffectsNotExpanded`；其中 `SpawnAnNPC` 仅返回 7 个 CFG 节点和 57 个 operation 节点，不能替代完整源码关系。完整参考源码因此用于补读其长分支和副作用关系；CPG 不绑定该参考副本。

本次使用只读 CPG Query API 查询 `SetSpawnFlags` 与 `ShouldSpawnInvasionEnemies`：精确符号查询
各命中一个声明；`Find-CpgCallSites` 在选定 `Terraria/NPC.cs` 中为 `SetSpawnFlags` 返回一个来自
`TrySpawnAnNPC` 的静态调用点。两个 callable-facts 结果分别为 `partial`（5/3 个 CFG 节点），
均带 `CalleeEffectsNotExpanded`，且 `DirectCallTargets` 为空。因此 helper 调用及其 RNG 效果关系
由 Version4 与完整参考源码方法体核对，不从 CPG facts 推断。源码显示 `SetSpawnFlags` 调用
`ShouldSpawnInvasionEnemies`，后者在特定分支调用 `Main.rand.Next(3)`；随机消费属于 per-player
flags 准备阶段，须先于 rate 输入和 rate roll。查询 manifest 为
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`；
索引事实不绑定完整参考源码，也不闭合 callee effects。

自然生成的候选提交不能停在 `SpawnAnNPC` 的 `void` 边界。CPG 对精确
`NewNPC(IEntitySource,int,int,int,int,float,float,float,float,int)` overload 在全部 967 个索引
shard 中返回 158 个静态调用点；端点横跨自然生成、特殊入口、事件和网络相关代码。Version4
`Terraria/NPC.cs:67051-67097` 显示该入口负责可用 slot 选择、`Main.npc`/`whoAmI` 写入、对象重置、
默认值初始化、位置/active/timeLeft/AI/target 写入以及 `spawnNeedsSyncing` 标记，并以 slot 或
`Main.maxNPCs` sentinel 返回。完整参考 `Terraria/NPC.cs:82017+` 的同步标记条件不同，因而只能作为
关系补充和差异证据；不能把 `NewNPC` 的静态调用数量或 `void` 的上游边界解释为一次尝试恰好创建
一个实体。

自然尝试内部顺序为：

```text
slime-rain effect（若活动）
  -> SetSpawnFlags / NpcSpawnPerPlayerFlagsSystem.Prepare
  -> GetSpawnRate / 容量判断
  -> Main.rand rate roll
  -> FindSpawnTile / GetSpawnArea
  -> CheckNotSpawningOnScreen
  -> GetProperGroundSpawnTileTypeAndWallType
  -> PostCheckChosenSpawnTile
  -> SetSpawnFlagsForChosenTile（tile/post-check 之后的 chosen-tile flags）
  -> dual-dungeon 后置修正
  -> SpawnAnNPC
  -> SyncNewlySpawnedNPCs（副本间调用条件不同）
```

排序是行为契约的一部分。不同步预先生成随机样本，也不在迁移时重排 tile、event、screen
或 biome 判定；这样会改变 `Main.rand` 消耗、后续候选及事件可见顺序。

### 3.2 特殊生成入口

特殊生成继续分开建模，不并入自然资格 Query：

- `Main.cs` 有 Eye、hard boss、Deerclops 等 `NPC.SpawnOnPlayer` 路径；完整副本可在
  `Main.cs:65867-66020` 定位对应调用，生成入口见 `NPC.cs:81756-81918`。
- `MessageBuffer.cs` 的 shimmer/区域消息路径调用 `NPC.Spawner.SpawnFaelings`；完整副本调用点
  为 `MessageBuffer.cs:2223`，实现见 `NPC.cs:5987-6028`。
- 网络消息分支可以调用 `NPC.SpawnOnPlayer`；完整副本调用点为 `MessageBuffer.cs:2779`。
- `SpawnFaelings` 和 `SpawnOnPlayer` 自己有 tile、随机、唯一性检查与生成副作用；其准入条件、
  通知和 target 写入顺序需要各自保留。

### 3.3 同步副本差异

| 观察点 | `Version4` | 完整源码参考 | 设计处理 |
| --- | --- | --- | --- |
| Main 到 `NPC.SpawnNPC` | `Main.cs:11454`，当前片段未显示调用周围的 `netMode != 1` 条件 | `Main.cs:18050-18055` 显示 `netMode != 1` 守卫 | 按 Version4 目标行为；展开完整 caller 上下文确认影响范围，不能直接引入参考守卫。 |
| 自然尝试后同步 | `NPC.cs:251-253` 无条件调用 `SyncNewlySpawnedNPCs` | `NPC.cs:251-256` 仅 `Main.netMode == 2` 时调用 | 默认保留 Version4 条件；完整参考条件只有经显式目标行为变更裁定后才能采用。 |
| `NewNPC` 的同步标记 | `NPC.cs:67088` 无条件设置 `nPC.spawnNeedsSyncing = true` | `NPC.cs:82054-82056` 只在 `Main.netMode == 2` 设置该标记 | 按 Version4 作为目标基线；创建 owner、net mode 和清理/投影闭包未确认前保持 `unknown`。 |
| 玩家循环与成功退出 | `NPC.cs:185-205`，0..254，首个 `TrySpawnAnNPC == true` 即 break | 同一区段顺序相同 | 可作为共同结构证据，不代表完整行为等价。 |
| 双地牢选中 tile flags | `NPC.cs:341-343` 方法体为空；直接搜索 Version4 Terraria 源树未找到 `NPCSpawningFlagsForDualDungeons` 类型。CPG 类型查询 0 命中且为 `partial`，不作为不存在证明。调用在 dual-dungeon 地下区域分支的 `Next(7)` 后，并受非 active/type-48 tile 条件保护 | `NPC.cs:344-360` 调用 `NPCSpawningFlagsForDualDungeons.ScanZonesFor(false, ..., true)`，忽略其 bool 返回值，再写回 9 个 zone flags；结构体实现见 `NPCSpawningFlagsForDualDungeons.cs:5-240`。同样位于 `Next(7)` 后，且只在 `!tile.active() || tile.type != 48` 时调用 | 保持 Version4 空方法语义为当前目标基线；若要采用完整参考的 zone 推导，先裁定为显式行为变更，并补齐 tile/wall/source 写入闭包。 |

## 4. 候选 System 与协作对象

| 候选对象 | 责任 | 允许拥有 | 不拥有 | 状态 |
| --- | --- | --- | --- | --- |
| `NpcSpawnSystem` | `ProcessEntry` 组合抑制检查、respawn 检查和自然 pass；`ProcessNaturalSpawnPass` 维护玩家尝试顺序并串行调用 eligibility/context/effect port | 只拥有入口/pass 内的控制流与 attempt-local 临时状态 | `noSpawnCycle`/`RevengeManager`、NPC entity lifecycle、population allocator、target authority、network/persistence | `partial`；`INpcSpawnEntryPort` 与 `INpcSpawnPassPort` 均只有 verifier fake；入口顺序已测试，但没有生产 adapter、旧入口 facade、runtime registration/caller 或 authority spawn commit。 |
| Spawn context capture（adapter/内部职责） | 从 Player、world、tile、event 与 population owner 读取一次尝试所需输入 | 本地/不可变的 attempt snapshot | 不复制上游权威状态、不持久注册资格结果 | `proposed`。 |
| Eligibility/target policy Query（可为 System 私有方法） | 对显式输入返回资格、拒绝原因或 target value | 无权威写入 | 不能直接访问 `Main`、`Main.rand`、可变 Tile、clock、logger、network 或存档 | `proposed`；调用效果闭包待证。 |
| 候选搜索协调 | `NpcSpawnAreaQuery.Calculate` 从显式输入计算 attempt-local area；`NpcSpawnTileSearchSystem.Find` 依序读取随机与 tile facts 并返回候选、area、安全范围和最终 `SkyMob`；随后执行 screen exclusion、chosen-tile post-check 与 `NpcSpawnChosenTileFlagsSystem.Calculate` | 仅本次尝试控制状态 | 不拥有 tile/world 真值；不得新增共享静态 cache | `partial`；area/search/screen/post-check/flags core 已由隔离 coordinator 串接，输入仍只有 verifier port，没有生产 capture adapter；spawn commit 与 sync 未接通。 |
| Tower spawn branch selector | `NpcSpawnTowerSelectionSystem.Select` 按 Nebula、Vortex、Stardust、Solar 顺序选择 tower NPC，并产出 `NpcSpawnEntityRequest` | attempt-local RNG draw、按选中 type 的 active NPC count 查询及请求值 | 不写 population、slot 或 entity；不执行 `SpawnNPC`/`NewNPC` | `partial`；选择顺序、权重、上限重抽和请求字段按 Version4/完整参考源码实现；生产随机/计数 adapter 与 continuation 接线未实现。 |
| SkyMob spawn branch selector | `NpcSpawnSkyMobSelectionSystem.Select` 对 `skyMob` 分支按 invasion、Martian、Wyvern、purple slime、默认 sky NPC 的顺序选择一个 `NpcSpawnEntityRequest` | 当前 attempt 的 sky/zone/event/progression 快照、`AnyDanger`、active NPC presence、Luck roll 与随机抽取 | 不写 population、slot 或 entity；不调用 `SpawnNPC`/`NewNPC` | `partial`；完整保留 Version4 的条件与重复 Water Candle reroll 顺序；生产输入/随机/presence adapter 与 continuation 接线未实现。 |
| Invasion spawn branch selector | `NpcSpawnInvasionSelectionSystem.Select` 按 invasion type 1–4 复现入侵 NPC 的候选选择，并产出请求或明确状态 | attempt-local invasion/tile/hardmode 快照、按源码顺序消费的 RNG、active NPC presence 与 `SolidTiles` 区域查询 | 不写 population、slot 或 entity；不调用 `SpawnNPC`/`NewNPC`；未知 invasion type 显式返回 early-return 状态 | `partial`；type 1–4 的选择与随机次序有独立 core；生产输入/RNG/presence/tile adapter、legacy caller 和 continuation 未接线。 |
| Graveyard/dual-dungeon branch selector | `NpcSpawnGraveyardDualDungeonSelectionSystem.Select` 消费已接受的 tile candidate，按 Statue Mimic 后 dual-dungeon 的 `else if` 顺序返回 pre-commit request | `DownedBoss3`、graveyard zone、chosen-tile `NoWormsForSpawn`、dual-dungeon trespass、hardmode、effect port 的坏运气随机、NPC presence 和 Statue Mimic 地形检查 | 不写 population、slot 或 entity；不调用 `SpawnNPC`、`NewNPC` 或 `SetDefaults` | `partial`；Version4 与完整参考的候选条件、顺序及请求字段一致；fake-port core 有专项断言，生产 effect adapter、调用优先级接线和 continuation 未确认。 |
| Critter branch selector | `NpcSpawnCritterSelectionSystem.Select` 将 type 244 分支按水域、地表/地下及节日状态选为 pre-commit request 或明确无请求结果 | attempt-local tile/world/event facts；`RollLuck` 与 `Next` effect port 按旧条件链短路消费随机值 | 不写 population、slot 或 entity；返回 `PostSpawnTimeLeftMultiplier` 表达 gnome 创建后的 `timeLeft *= 10`，不执行创建 | `partial`；Version4 与完整参考的 type 244 分支条件、调用顺序及无生成路径一致；CPG 对 `Utils.SelectRandom<T>` 调用点为 `partial`，已回源码确认随机实现；生产 capture/effect adapter、上游优先级接线、commit 与 post-create 效果执行未确认。 |
| NPC creation/lifecycle owner | 创建、激活、初始化、销毁 NPC | 唯一实体生命周期和创建写入 | P14 不复制创建/激活 owner | `crossSubsystemOwner: integration-review`。 |
| NPC population/slot owner | capacity、`npcSlots`、`dontCountMe` 和活跃数变化 | 唯一 slot/人口记账 | P14 不另建 allocator | `crossSubsystemOwner: integration-review`。 |
| target/network/persistence/event owners | 接受 target 写入及副本投影、存档和事件状态变更 | 各自既有权威状态 | P14 Query 不回写，不跨域复制 | `crossSubsystemOwner: integration-review`。 |

此表定义的是设计候选与边界约束，不证明接收方在 `src/NSSLC` 已完成注册或接入。

## 5. API 责任设计

Query/Command 是职责说明，不要求新增 `IQuery`、命令总线、outbox 或一类一个接口。

| 旧入口/方法 | 新职责组合 | 必须保留的语义 | 状态 |
| --- | --- | --- | --- |
| `NPC.SpawnNPC()` | 旧入口 Adapter 调用 `NpcSpawnSystem.ProcessEntry(entryPort)`；审计/验证路径可调用 `ProcessEntryDetailed` 获取入口顺序和可观察结果 | `noSpawnCycle` 清除及 respawn 先后顺序；外层调用 guard 仍由入口 adapter 保留；legacy loop-control 不等于实体创建 | `partial`；协调顺序和结果区分已在 isolated core 实现，`noSpawnCycle` 生产 adapter、respawn owner 接线、caller guard、runtime registration 和实际 caller 均未确认。 |
| `Spawner.SpawnNPC()` | `NpcSpawnSystem` 中的有序 per-player 循环 | 255 槽顺序、slime-rain 分支、每玩家 `SetSpawnFlags`、首个 true 后 break | `partial`；CPG 入站查询零命中为 partial，源码调用仍可见。 |
| `CanSpawnEnemiesNear` | 对显式 player snapshot 的资格 Query | active/dead、Journey per-player spawn suppression、Moon Lord 邻近拒绝及顺序 | `partial`；`NpcSpawnEligibilityQuery` 已编码纯决策，但输入 capture、legacy adapter 和运行时调用尚未接通；CPG 选定 shard 的 4 个静态调用点不代表动态闭包。 |
| `GetSpawnRate` 与容量 gate | 显式 attempt snapshot 上的 rate/cap 计算，由随机端口维持 legacy RNG 时点；capacity gate 留在 pass coordinator | rate、cap、event、player multiplier 的整数舍入、条件随机消费与比较顺序 | `partial`；`NpcSpawnRateSystem.Calculate` 已接线到隔离 coordinator，输入仍由测试 port 提供，无生产 capture adapter，未覆盖完整差分。 |
| `GetSpawnArea` | 自然生成路径使用 `NpcSpawnAreaQuery.Calculate` 返回局部 spawn/safe area 和 safe extents；其它旧调用者暂留原 helper | 缩放道具因子、Dual Dungeons safe-area 尺寸、逐边 world clamp；不改其它 `GetSpawnArea` caller 的共享静态语义 | natural-path core 已实现；`SpawnFaelings`、`SpawnOnPlayer` 仍使用旧 helper。 |
| `FindSpawnTile` | `NpcSpawnTileSearchSystem.Find` 驱动 50 轮有序抽样；逐 tile facts 交给 `NpcSpawnTileSpaceQuery.CanSpawn` | x/y 随机顺序、首 tile solid/wall 短路、sky/near-sky 条件抽样、向下找地面、safe-area gate、footprint bounds/tile scan 与 sticky `SkyMob`。Version4 `WorldGen.InWorld(Rectangle)` 默认边界拒绝 `Left/Top < 0` 或 `Right/Bottom >= maxTiles`；`CanSpawnInTiles` 按 X 外层、Y 内层扫描并短路；`CanSpawnInTile` 拒绝 active solid 或任意 lava。完整参考对应逻辑一致 | effectful System，不是纯 Query；边界、循环次序与 active-solid/lava 规则 core 已对齐；port 仍只有 verifier fake，无生产 tile/random adapter，实际 tile facts capture 等价保持 `unknown`。 |
| `CheckNotSpawningOnScreen` | `NpcSpawnScreenExclusionQuery.IsSpawnTileOutsideScreen` 对显式 active-player snapshot 计算 footprint 与扩大 screen rectangle 是否相交 | 255 玩家槽的顺序、Dual Dungeons 墙内玩家排除、safe-range 扩展及矩形边缘语义 | `partial`；纯计算已由 pass 在 tile search 成功后调用；player/screen 输入的生产 capture 与 source revision 绑定 `unknown`。 |
| `PostCheckChosenSpawnTile` | `NpcSpawnPostCheckSystem.IsAccepted` 对已选候选 tile 执行规则判定 | Dungeon、Dual Dungeons forbidden tile、液体、tile 类型和事件随机门槛顺序 | `partial`；当前在 screen gate 通过后调用，并通过同一随机 port 只在 sandstone 条件成立时抽一次；tile/event facts 的生产 capture 和 RNG 等价性 `unknown`。 |
| `SetSpawnFlags(player)` | `NpcSpawnPerPlayerFlagsSystem.Prepare(playerIndex, INpcSpawnPerPlayerFlagsPort)`，由 `NpcSpawnSystem` 在 slime-rain effect 后调用 | 先捕获 prelude 与 invasion 输入，再按源码短路检查和 NPC slot 顺序消费可选 RNG，随后捕获 postlude 与 rate 输入；`Calculate` 生成 attempt-local flags，不写共享 legacy 状态 | 计算 core 与 fake-port focused checks 已存在；完整参考与 Version4 的主方法体一致。生产 capture adapter/runtime caller 未实现；输入与 legacy 字段的运行时写入映射仍 `unknown`。 |
| `SetSpawnFlagsForChosenTile(...)` | `NpcSpawnChosenTileFlagsSystem.Calculate` 将 chosen-tile flags 作为 attempt-local 结果交给 continuation；world/tile/RNG 由显式 port 提供 | 只在 tile 命中和 post-check 通过后运行，保留地图、tile、random 读取顺序 | chosen-tile flags core 已实现为 effectful System 步骤，不把它称作纯 Query；生产 adapter 缺失。 |
| `SetSpawnFlagsForChosenTile_ForDualDungeon(...)` | Version4 目标基线下暂不映射为具体 flag 写入 | Version4 helper 当前为空；完整参考会从 tile/wall facts 写回 9 个 flags | 保留为待裁定的版本差异；不从完整参考副本自动移植该行为。 |
| 通过候选后的 continuation | `INpcSpawnPassPort.ContinueSpawnAttempt(in NpcSpawnAcceptedCandidate)`，返回类型为 `void` | 只在候选通过 search、screen、post-check 与 chosen-tile flags 后调用；值对象传递 player index、rate inputs/result、tile-search result、post-check facts 和 `ChosenTileFlags`；随后结束自然 pass 的玩家循环；调用本身不承诺 NPC 创建 | `partial`；`NoWormsForSpawn` 从 `ChosenTileFlags.NoWorms` 派生；spawn commit 不在该值对象内；结果用 `ContinuationWasInvoked` 记录控制流，并把创建观察保持为 `Unknown`。 |
| `SpawnAnNPC` | 请求现有 NPC creation/lifecycle owner 执行分类生成 | branch 选择、单次尝试内多次 `SpawnNPC` 调用、创建失败行为与副作用顺序 | `unknown`；不假设 `void` 等于恰好成功一次。 |
| `SpawnAnNPC` 的 tower-zone 分支 | `NpcSpawnTowerSelectionSystem.Select` 将四个 tower-zone 分支映射为加权选型、人口上限重抽和 pre-commit request | Nebula/Vortex/Stardust/Solar 的优先顺序、每种候选权重、条件 random draw、按选中 type 查询 `CountNPCS` 的时点、像素坐标与 `Start=1` | 不创建实体、不提交 slot、不改人口、不将 `Target=255` 提前解析 | `partial`；隔离选择 core 已实现并有一个 Solar focused 分支验证；继续接入完整 `SpawnAnNPC` 仍受其它分支及 lifecycle/population owner 未闭合阻塞。 |
| `SpawnAnNPC` 的 graveyard/dual-dungeon 分支 | `NpcSpawnGraveyardDualDungeonSelectionSystem.Select(in NpcSpawnAcceptedCandidate, port)` 消费前序流程已选中的 tile 与 attempt snapshots，按 Statue Mimic / dual-dungeon 优先级返回请求 | 先检查 `DownedBoss3 && ZoneGraveyard && !NoWormsForSpawn`，再依序执行 `RollBadLuckExtreme(25)`、`AnyNPCs(690)`、`IsThisAGoodPlaceForAStatueMimic`；该条件链失败后才检查 `tresspassingDualDungeon && RollBadLuck(15) == 0`。type 为 690、82 或 316；坐标为 `(tileX*16+2, tileY*16)`，默认 Start/AI/Target；不创建实体、不提交 slot 或改人口 | `partial`；本地顺序与参数有 fake-port core 断言；tile predicate 与 random/presence 通过 effect port；真实 `SpawnAnNPC` caller、上游分支优先级、adapter 和 continuation 未接线。 |
| `SpawnAnNPC` 的 type 244 critter 分支 | `NpcSpawnCritterSelectionSystem.Select` 接收 `RemixWorld`、water、tile/surface、gold/gnome chance 与节日状态，返回 pre-commit request 或 `HandledWithoutRequest`；上游 dispatcher 必须先按 legacy `num` 分类，不能用候选 tile 的 `SpawnWallType` 代替 | legacy `num` 初值是 `(spawnTileX, spawnTileY - 1).wall`；若 `(x,y-2).wall == 244` 或 `(x,y).wall == 244` 则覆盖为 244。该 `else if` 位于 Statue Mimic 与 dual-dungeon 分支之后，且整个 `SpawnAnNPC` 中还有未迁入的前置分支；当前 selector 假定调用方已完成所有优先级判断。分支内部保留水域 gold roll；地下 `Next(3) -> Next(2) -> RollLuck -> Next(3)` 短路顺序及全失败时不生成；地表按 gnome、两次 gold chance、Halloween、Christmas、Birthday Party、gem pair、普通 critter 顺序选择；type 624 request 带 `PostSpawnTimeLeftMultiplier=10` | 只表达请求和创建后的 timeLeft 效果意图；不调用 `SpawnNPC`、`NewNPC`、`SetDefaults`，不执行数据包逻辑 | `partial`；Version4 与完整参考 source branch 内部规则一致；selector 尚无包含 wall 分类和前序分支优先级的生产 dispatcher、adapter、旧入口 caller、entity commit 或 post-create effect owner，未验证。 |
| `NPC.Spawner.SpawnNPC(...)` | `NpcSpawnEntityPreparationSystem.Prepare` 先执行 `FromNetId`/slime variant 随机，再按 snapshot 解析 `Target=255`；`NpcSpawnPreCommitSystem.Prepare` 随后组合 slot acquisition；`NpcSpawnDefinitionResolutionSystem.Resolve` 按最终 type 读取 definition | `FromNetId` 必须先于 slime 判断；普通 variant roll 先于可选周年 roll，后者可覆盖前者；target fallback 在 `NewNPC` 前完成；GoodWorld 类型改写后的结果必须传给 definition query 和后续 owner | `partial`；Version4 `NPC.cs:5152-5182` 与完整参考 `NPC.cs:5170-5200` 的顺序一致，fake-port trace 已验证；definition lookup 支持正 type ID 与负 net-ID variant，但不做 hydration/commit；生产 RNG/defaultTarget/slot adapter 和 runtime caller 仍缺失。 |
| `GetAvailableNPCSlot` / `IsSpawnSlotInUse` | `NpcSpawnSlotAcquisitionSystem.Acquire` 串接 type resolution、`FromNetId` slot metadata 和 slot facts capture；`NpcSpawnSlotSelectionQuery.Select` 执行纯选择；`NpcSpawnSlotSelectionSystem.SelectAndProtect` 选中后同步写 protection | `active || spawnSlotProtected > 0` 为 in-use；先按搜索方向找首个空闲槽，再找首个 `CanBeReplacedByOtherNPCs` 槽；正向从 `startIndex` 起含该索引且允许 `startIndex == slotCount` 表示无槽，反向从末槽向下并排除（调整后的）`startIndex`；slot-0 metadata 仅在输入起点为 0 时读取；有结果时在返回调用方前写 protection=2，无槽时不写；replaceable fact 可携带非零 generation，selection result 原样传递为 `ExpectedGeneration` | `partial`；Version4 与完整参考的选择循环一致；GoodWorld type remap、`FromNetId`/搜索策略读取、slot facts capture 顺序、Query、legacy result mapping 与 Query→protection 命令组合均有 isolated core。generation 缺失时选择仍保留 legacy 结果，但不提供可用于替换的 generation guard；生产 generation capture、替换资格重读、owner 和 runtime caller 仍未接通。 |
| `UpdateProtectedSpawnSlots` / slot protection 写入 | `NpcSpawnSlotProtectionSystem.AdvanceTick`、`ProtectSelectedSlot` 和 `ResetSlotProtection` 通过 `INpcSpawnSlotProtectionPort` 执行有序状态命令；调度与调用方仍由权威 slot owner 提供 | 每 tick 按槽执行 `active ? 2 : max(previous - 1, 0)`；`NewNPC` 选中槽后、替换和初始化 NPC 前写入 `2`；WorldGen 建立 NPC slots 时写入 `0`。active 为真时不读取旧 protection 值 | `partial`；隔离 core 已实现，focused checks 覆盖写值和读写顺序。Version4 与完整参考的三个写入路径及 Main tick 顺序一致。field/slot 权威 owner、WorldGen/reset 适配、System 注册和生产调用未闭合；不声称唯一 writer 或已完成迁移。 |
| `NPC.NewNPC(...)` | `NpcSpawnSlotAcquisitionSystem.Acquire` 表达 GoodWorld remap 到 slot-selection/protection 的前置链；`NpcSpawnPreCommitResult.ResolvedRequest` 将最终类型与已选槽交给 `NpcSpawnDefinitionResolutionSystem.Resolve`；replacement selection 同时携带可选 `ExpectedGeneration` | `Main.npc`/`whoAmI`、slot protection、`ResetForNewNPC`、完整 `SetDefaults`、town unique data、激活、AI/target、`spawnNeedsSyncing` 与公告等副作用的顺序；Version4 与完整参考的同步标记条件差异必须单独裁定 | `unknown`；definition lookup 只按最终 type/net ID 返回内容 profile，不创建实体，也不实现 `SetDefaults`；commit owner 仍须重新检查 replaceability 并用 `TryReplace(expectedGeneration, ...)`。生产 facts adapter、完整 hydration、创建、激活、sync commit 和 caller 闭包均未实现。 |
| `SyncNewlySpawnedNPCs` | `INpcSpawnSyncPacketPort.SendNpcSyncPacket(int npcLegacySlot)`；`INpcReplicationPacketApi` 声明 packet 23/28 decode/encode、recipient selection 与 publish | active + `spawnNeedsSyncing` 的候选、legacy slot、消息 type 23；Version4 与完整参考的 net mode gate 不同 | `partial`；两个接口只声明函数，`INpcReplicationPacketApi` 的 request/result 只声明类型。按范围不编写 packet 逻辑或 caller 接线；owner、net mode 和 flag writer 闭包仍 `unknown`。 |
| `SpawnFaelings` / `SpawnOnPlayer` | 独立特殊生成入口，后续再对齐 commit API | 唯一性、tile 搜索、target/announce/network 顺序 | `partial`。 |
| `defaultTarget` | `NpcSpawnTargetSelectionQuery.Select` 对显式请求值和 attempt snapshot 做纯选择 | 请求值为 `255` 时回退到 captured `DefaultTarget`；其它值原样返回 | 选择 core `partial`；CPG 与 Version4 源码确认 sentinel 规则；legacy index 到 entity identity 的映射、target component 写入、生产 caller 与网络更新仍 `unknown`。 |

slot protection 已增加隔离的 `NpcSpawnSlotProtectionSystem` command core；
`NpcSpawnSlotSelectionSystem.SelectAndProtect` 将纯 slot-selection Query 与 selected-slot protect 命令组合；
`NpcSpawnSlotAcquisitionSystem.Acquire` 再按源码顺序组合 GoodWorld type resolution、`FromNetId`、slot
metadata/facts capture、选择和保护写入。它们通过 port 操作调用方持有的状态，不建立并行权威数组。Version4
`Main` 调度、生产随机与 metadata adapter、`NewNPC` entity owner 及 `WorldGen` 初始化仍未接入，因此这些
core 不代表已连接 runtime writer 或完成迁移。

表中“选中后写 protection”和“有结果时”均指 commit-ready selection，不表示纯 Query 返回候选即已可提交。
纯 slot Query 返回候选不等于该槽可提交：空槽候选可直接 commit-ready；替换候选只有携带正
`ExpectedGeneration` 时才是 commit-ready。generation 缺失或为零时，`IsCommitReady` 为 false，
`SelectAndProtect` 不写 protection，且 `NpcSpawnSlotAcquisitionResult.Found` 为 false；纯 Query 仍保留
Version4 顺序下的候选索引。这不闭合生产 generation capture、替换资格重读或 NPC commit owner。

对于 Query，强约束是“同一输入值可重排/重复且不改变权威状态或外部观察”。单纯从方法名
或返回类型不能得出该性质。候选采样使用可变随机器，必须由协调职责按原时点逐次请求；禁止
预先抽完固定样本后再调用 Query，因为早退分支会改变 legacy 随机消耗数。

当前 `src/NSSLC/Component/Npc/Queries/NpcSpawnEligibilityQuery.cs` 实现
`CanSpawnEnemiesNear` 的纯 decision；`NpcSpawnRateSystem.Calculate` 基于 rate snapshots 计算
rate/cap，并把 town-NPC 与 bad-luck 随机读取经 `INpcSpawnRateRandomPort` 显式传入。它不直接
读取 `Main` 或 `Main.rand`，但由于消耗随机状态，不属于可任意重排或重复调用的纯 Query。
`NpcSpawnSystem` 按 0..254 顺序执行 eligibility、slime-rain、per-player spawn-flag preparation、
rate capture/calculate、capacity gate 与 rate roll；roll 命中后才采集 area 输入并调用
`NpcSpawnTileSearchSystem.Find`。候选成功后，
pass 读取 screen-player snapshot 并运行 `NpcSpawnScreenExclusionQuery`；通过后依序 capture chosen-tile
facts、运行 `NpcSpawnPostCheckSystem` 和 `NpcSpawnChosenTileFlagsSystem.Calculate`，最后把候选交给
continuation port。此序对应完整参考项目 `NPC.cs:206-256` 的 tile search、screen gate、chosen tile
读取、post-check 与 flags 顺序；spawn 与同步仍在 continuation/owner handoff 缺口内。双地牢 zone flags 的 Version4 空方法与完整参考
写入逻辑差异尚待显式裁定；不将参考逻辑隐含视为未来接线的默认行为。`NpcSpawnAreaQuery` 只依赖
显式值，tile search、sandstone post-check 与 chosen-tile flags 因逐次 RNG/tile facts 保持 effectful。
现有输入/tile port 只是 verifier 替身；生产 adapter、旧入口 facade、System 注册、spawn commit 与
网络同步仍未接通。pass 返回的 `true` 仅表示旧控制流退出条件，不证明 NPC 创建。

NLTX 中 `WorldStorageRoot.Npcs` 的类型是通用 `EntitySlotStore<WorldEntityState, NpcSlot>`，但
当前 `src/NSSLC` 没有 `.Npcs.TryAllocate` 调用或 NPC spawn commit 调用链。`NpcLifecycleComponent`
当前由死亡后的 despawn transition 使用，`SpawnAdmissionState` 也未接入自然生成 pass。因此这些
类型不能证明 NPC 初始化、spawn admission、slot accounting 与 authority 已有可消费 owner API；
自然生成 tile search、screen exclusion、`PostCheckChosenSpawnTile` 与 chosen-tile flags 已有隔离 core，
并由 coordinator 顺序调用，但真实 adapter 的 player/tile/random/event 读取绑定仍为 `unknown`。NPC
creation 和 sync 未接通，在 owner/source 关系闭合前维持 continuation port。`NpcSpawnEntryResult`、
`NpcSpawnPassResult` 和 `NpcSpawnCreationObservation` 不写入上述 owner；它们是 attempt-local 观察值，
`Unknown` 明确表示当前 port 没有实体创建结果契约。`NpcSpawnTileSearchResult.Area.SafeRangeX/Y`
已在隔离 pass 中用于 screen gate；尚无生产调用者证明这些值已从真实 world/player owner 正确采集。

`INpcDefinitionQuery` 现同时声明 `TryGetByNetId` 与 `TryGetByTypeId`；
`NpcSpawnDefinitionResolutionSystem.Resolve` 在 pre-commit 已选到槽后，将最终正 type ID 交给
`TryGetByTypeId`，将负 net-ID variant 交给 `TryGetByNetId`，并拒绝 catalog 返回身份不匹配的 definition。
无槽时不查询 definition。该只读组合只传递最终请求和 content profile，不创建或写入 NPC 状态。
`NpcDefinition` 字段覆盖 identity、core stats、
movement、spawn、town、capability 与 presentation。
Version4 `NPC.cs:67051-67097` 与完整参考 `NPC.cs:82017+` 显示，definition lookup 之后仍须按顺序执行
`ResetForNewNPC`、完整 `SetDefaults`、town unique data、`Bottom` 定位、激活、`timeLeft` 初始化、条件 wet
碰撞查询、AI/target 写入和同步意图设置；Type 50 还会广播公告。现有 `NpcDefinition` 无法表达完整
`SetDefaults` 结果、wet/town unique data 和实体位置/target 绑定，稳定 `NpcInstanceId` 也没有已确认的签发 owner。
本轮按用户指示不实现 `SetDefaults`；新增 type Query 只打开读取入口，不代表已完成 hydration 或 commit。
但本地没有 NPC definition hydration/commit system 或 `NpcWorldEntityState`；`src/NSSLC` 与 `Test`
内也没有 `new NpcDefinition(...)` 实例，`ContentCatalogSnapshot` 接收预构造 catalog，因此外部装载数据
仍可能存在但不能由源码确认。Infrastructure 的 `NpcEntitySlotStore` 只记录
`RuntimeEntityId`、slot 和 generation；`EntitySpawnCommitSystem` 的 NPC 分支只分配首个空闲槽，未保存 NPC
组件状态，也未提供指定 slot、替换资格或 target/network contract。`EntityPoolInitializationSystem`
会创建该 store，但其初始化器与 commit system 均没有已找到的运行时调用点。
这些基础设施不能替代 Version4 `SetDefaults` 与 `NewNPC` 的实体初始化/提交 owner，也不能与
`WorldStorageRoot.Npcs` 推定为同一 slot store；两者的归并与 authority 仍需 integration review。

## 6. 状态所有权

| 成员簇 | 推荐的状态形态 | 生命周期/唯一写入边界 | 未决项 |
| --- | --- | --- | --- |
| `NpcSpawnContextAndCapacityInputs` 及空间、biome、policy、event 输入 | 每次 attempt 的不可变 snapshot/value | 创建于一次 attempt，判定后丢弃 | 哪些源由 tile/world/town/event owner 采集；快照 revision 和失效时点 `unknown`。 |
| `SpawnedFromStatue`、替换资格及实体 critter state | 既有 NPC instance state | NPC lifecycle/spawn commit 的一次创建写入 | statue、自然和事件来源的写闭包待 integration review。 |
| 概率常量、lookup、zone 输入、safe extents | definition、world state 或派生值，不按字段名强行实体化 | 原内容/世界/窗口 owner | 当前多处读者及窗口变化待查。 |
| `npcSlots`、`dontCountMe`、active counts | population owner 状态 | NPC create/replace/despawn 单一 owner | 记账对账和 slot API 未确认。 |
| `noSpawnCycle`、延迟/周期 | NPC 生命周期写入；`NpcSpawnSystem.ProcessEntry` 只通过 `INpcSpawnEntryPort.ConsumeNoSpawnCycle()` 消费并清除 | 现有 NPC lifecycle/legacy owner（候选）；P14 不取得字段写权 | Version4 源码可见死亡/失活路径置位及入口清零；完整 writer 集、保存恢复、tick 与客户端/server 范围仍为 `unknown`。 |
| daily/event/cooldown 状态 | 世界日历或 event progression state | 事件/日状态 owner | reset、存档、加载及广播来源 `unknown`。 |
| `defaultTarget` 与派生 target/name/opacity/network properties | 既有 target、identity、presentation、network owner | 由对应领域 owner 写入 | Getter 内写入、property setter 和网络/存档闭包需逐项确认。 |

当前 NLTX 的 `NpcSpawnContextSnapshot`、spatial/biome/policy/event snapshots、
`NpcSpawnTargetSelectionSnapshot`、`NpcSpawnAndCritterStateComponent`、
`NpcTargetComponent`、`NpcLifecycleComponent`、`SpawnAdmissionState`、
`WorldSpawnPressureState` 与 `WorldNpcSpawnPacingState` 只作 `current-NLTX partial` 证据。
不因此新增同义 component，也不将一次资格结果注册为长期实体状态。

`NpcSpawnSlotSelectionQuery` 是额外的纯决策 core：调用方提供有序 `NpcSpawnSlotFact` 快照、
起始索引、反向搜索标记和 `CannotSpawnInSlot0` 元数据，返回所选索引与是否使用替换回退。
事实现在可携带当前 occupant generation；replaceable fallback 将正 generation 复制到结果的
`ExpectedGeneration`，缺失或为零时保持 `null`。纯 Query 仍返回与 Version4 顺序一致的候选；
`NpcSpawnSlotSelectionResult.IsCommitReady` 仅在空闲槽或 replacement generation guard 存在时为真。
`NpcSpawnSlotSelectionSystem.SelectAndProtect` 只保护 commit-ready 槽，generation 缺失的 replacement
保留候选索引但不写 protection；`NpcSpawnSlotAcquisitionResult.Found` 同样只报告 commit-ready 槽，阻止
definition lookup 或后续提交把无 guard 候选当作可用槽。Query 不读取
`Main.npc`/`spawnSlotProtected`/`NPCID.Sets`，也不执行槽位保护写入、NPC 初始化或替换；
这些输入的权威捕获与后续 commit 仍归未确认的 NPC creation/population owner。替换 owner 必须基于
当前 occupant 重验替换资格并将 expected generation 交给 `EntitySlotStore.TryReplace`；空槽应使用
`TryAllocateAt`，由 store 拒绝已经被占用的槽。接线侧还须先保留
`NewNPC` 中 GoodWorld type remap 的条件随机调用，再以 `NPCID.FromNetId` 后的类型查找 reverse 与 slot-0
metadata。`NpcSpawnSlotLegacyResultAdapter` 只负责已选结果到旧返回约定的纯值转换：Query 的 `null`
映射为 `GetAvailableNPCSlot` 的 `-1`，负 slot 结果映射为 `NewNPC` 的 `Main.maxNPCs` 失败 sentinel，
有效索引原样通过，并拒绝超出给定 capacity 的正索引。该 adapter 不采集 `Main.npc` 或
`spawnSlotProtected`，不执行 GoodWorld RNG、metadata 查找、slot 写入或 NPC 创建；调用顺序和运行时
接线仍未完成。

`NpcSpawnTypeResolutionSystem.Resolve` 独立表达 `NewNPC` 在 slot lookup 前的 GoodWorld type remap：
GoodWorld 为真时经显式 random port 恰好消费一次 `Next(3)`，roll 非零时将 46 映射为 614、62 映射为 66；
其他 type 不变但仍消费该 roll。该规则在 Version4 与完整参考源码中一致。此 System 只返回 resolved type
和 RNG 消费观察，不查找 `NPCID.FromNetId` metadata、不选择或写入 slot、不创建 NPC；生产 random adapter、
commit owner 和 `NewNPC` caller 接线仍为 `unknown`。

`NpcSpawnEntityPreparationSystem.Prepare` 复现 `Spawner.SpawnNPC` 在调用 `NewNPC` 前的请求准备：先以
`FromNetId(request.Type)` 判断 slime net ID，再依序消费普通与可选周年 `RollLuck(180)`，然后通过
`NpcSpawnTargetSelectionQuery` 将 sentinel `255` 替换为 snapshot 中的 `DefaultTarget`。后续
`NpcSpawnPreCommitSystem.Prepare` 组合该准备与 GoodWorld type resolution、slot metadata/facts capture、
slot selection 和 protection 写入。`NpcSpawnPreCommitResult.ResolvedRequest` 将
`EntityPreparation.PreparedRequest` 的 type 替换为 `SlotAcquisition.TypeResolution.ResolvedType`，使后续
commit owner 不会把 GoodWorld remap 前的 type 当作最终 type；只有 `SlotAcquisition.Found` 时才有有效
slot 可供提交。Version4 `NPC.cs:5152-5182` 与完整参考 `NPC.cs:5170-5200` 方法体顺序一致；focused
fake-port trace 覆盖周年 roll 覆盖普通 variant 和默认 target。此组合不调用 `NewNPC`，不分配、初始化或激活
实体，也不发送数据包；各输入的生产 adapter、真正 commit owner 和运行时 caller 仍为 `unknown`。

当前 NLTX 源码中 `WorldStorageRoot.Npcs` 是 `EntitySlotStore<WorldEntityState, NpcSlot>`：
`TryAllocate` 从首个空位分配并可扩展容量，新增的 `TryAllocateAt` 可将状态分配到指定空槽，
`TryReplace` 则要求原 slot 与 generation；这些仅是存储操作，没有 NPC commit System 调用它们。
`WorldEntityState` 在当前组件源码中只有 `ProjectileEntityState` 子类，没有 NPC 聚合状态或 hydration
System。`NpcDefinitionCatalog.TryGetByTypeId` 已实现，且 `INpcDefinitionQuery` 现在公开该只读查询合同；
但尚无 hydration consumer 能把最终 type 映射为完整 NPC 初始状态。这些缺口使 pre-commit 结果不能直接
推进为实体提交；状态 hydration、slot facts 同步及权威 owner 继续标
`unknown`。

2026-10-01 的只读 CPG Query API 对 `Terraria/NPC.cs` 解析到 3 个 `SpawnNPC` 方法符号及一个精确
`NewNPC(IEntitySource,...)` overload。`Get-CpgCallableFacts` 对选定 instance `SpawnNPC` 与 `NewNPC`
均为 `partial/CalleeEffectsNotExpanded`；前者未返回调用 operation/target，后者只显示
`GetAvailableNPCSlot` invocation 且 `DirectCallTargets` 为空。故 `Spawner.SpawnNPC -> NewNPC` 与
`NewNPC -> GetAvailableNPCSlot` 的实际关系以 Version4 和完整参考的源码方法体核对；CPG 的零 target
不是“无调用”证据。该查询使用 manifest
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`，不绑定完整参考副本。

`NpcSpawnTargetSelectionQuery` 只读取显式请求值与 `NpcSpawnTargetSelectionSnapshot`，复现
Version4 `SpawnNPC` 在请求值为 `255` 时替换为 `defaultTarget` 的选择规则；同一输入可重复求值，
不读取共享状态、不写 `NpcTargetComponent`，也不把 legacy player/NPC index 转成 entity identity。
ZoneGraveyard 对 `defaultTarget` 的写入、快照 capture、target owner 和 runtime caller 仍待闭合。

P14 数据包相关 API 仅保留函数签名和请求/结果类型声明；编解码、发送、接收、recipient selection
或广播均不实现具体逻辑，也不由本次 pre-commit 组合调用。

## 7. 依赖方向与外部参考

候选依赖方向：

```text
Main/legacy tick adapter
  -> NPC spawn coordinator (order, timer/random/tile effects)
     -> snapshot adapters (Player / world / tile / event / population owners)
        -> pure eligibility and target value calculation
           -> pre-commit slot selection and final-type definition lookup
           -> existing NPC creation/lifecycle owner
              -> population, target, network and persistence owners
```

该图表达目标依赖方向，不是当前运行时 DAG。源端 `Main.cs` 的调用顺序有直接源码证据；
完整执行注册、并发/phase 和跨 System 可见性尚未证明。

SS14 的 `SpawnerSystem.Update` 注入 `IGameTiming` 和 `IRobustRandom` 后执行定时生成，最后调用
`SpawnAtPosition`（`Content.Server/Spawners/EntitySystems/SpawnerSystem.cs:7-52`）；
`StationSpawningSystem` 发出 `PlayerSpawningEvent` 并返回 spawn result
（`Content.Server/Station/Systems/StationSpawningSystem.cs:59-67`）；`NPCSystem.Update`
更新 active NPC 的 HTN 生命周期逻辑（`Content.Server/NPC/Systems/NPCSystem.cs:146-156`）。
这些例子支持将调度、创建入口和 spawn 后 NPC 更新分开描述，但不证明 Terraria 的相同 API、
阶段或行为。

## 8. 主要风险与设计阻塞

1. CPG 数据只有 `Version4` project fingerprint，无 `SourceSnapshotId`；用户已指定 Version4
   为目标行为基线，但精确 source revision 尚未绑定，完整源码参考的三个关键文件 hash 均不同。
   需要固定两侧快照，并逐项裁定参考副本新增或变化的行为。
2. CPG 对静态 `NPC.SpawnNPC` 在 `Main.cs` 的 indexed call-site 有确认结果；对实例
   `Spawner.SpawnNPC` 的 inbound 查询为 `partial`/零命中，不能视为没有调用者。`noSpawnCycle`
   field member-use 查询虽返回 `complete`，索引只返回两个写引用；目标源码还显示入口读/清及其它
   lifecycle 写点，因此索引结果不构成完整 read/write closure。respawn 方法符号可解析到
   `CoinLossRevengeSystem.CheckRespawns`，其选定 NPC 调用点查询为 `partial`/零命中；直接源码仍确认
   `NPC.SpawnNPC()` 对它的调用。`NpcSpawnSystem.ProcessEntry` 通过端口表达这些外部操作，不声称 owner 已接通。
3. Main 调用守卫、`SyncNewlySpawnedNPCs` 的 net-mode 条件和双地牢 chosen-tile flags helper
   在两个源码副本表现不同。默认按 Version4 目标基线保留；若业务要求补入完整参考行为，应
   作为目标行为变更单独裁定。
4. `SpawnAnNPC` 负责繁多条件分支，直接调用多个 NPC spawn helper；外层 bool 与 sync scan 不能
   简化成“一个 eligibility result 对应一个新实体”。分支 effects、失败和数量须继续读源码。
5. `npcSlots`/`dontCountMe`、NPC lifecycle、target、event state、network section、GivenName、
   Opacity 和 persistence 缺少闭合 owner，当前为 `crossSubsystemOwner: integration-review`。
6. legacy `GetSpawnArea`、`FindSpawnTile`、`SetSpawnFlags`、`SetSpawnFlagsForChosenTile` 具有共享静态写入、
   本地上下文写入、随机或 tile 读取。新 area/search core 未替换特殊 caller 或旧 writer，不能单靠 API
   提取和 CPG 直接调用点宣称 source closure。

在上述阻塞决策定案前，不建立或注册新的 runtime System，不移除 legacy writer，不宣称
API/行为等价。

## 9. 设计验收条件

- P14 的 117 项仍由 source report 唯一限定；不扩入其它分区成员。
- 每个 mutable state 有单一 owner，无法证明时保持 `unknown`/integration handoff。
- Query 的输入、写集、确定性和可重入性有方法级证据；随机/tile/context builder 保持 effectful。
- legacy 返回值、状态变化、生成数量、网络 effect、顺序和生命周期分别映射。
- 目标 System/API 仍标 `proposed`，没有以文件存在或 SS14 结构把状态升级为实现。
- 单项目 focused verifier 已检查 no-spawn-cycle 短路清除、respawn-before-pass 顺序、slime-rain 后
  per-player flag preparation 且早于 rate capture、候选 gate、
  typed candidate handoff、slot-selection Query 与 protect-command 组合、GoodWorld→metadata→facts→select
  的 slot-acquisition 调用 trace、容量不匹配拒绝，以及 detailed result 对 loop-control/creation unknown 的区分；
  Tower、SkyMob、Invasion、Statue Mimic 与 dual-dungeon selector 另有有限 fake-port 核心场景。
  这些只证明隔离 core 的请求和局部效果顺序。生产运行时接入、端到端行为、网络/存档观察和迁移验收
  均未运行，整体 `verificationStatus` 为 `not-run`，不得称迁移成功。

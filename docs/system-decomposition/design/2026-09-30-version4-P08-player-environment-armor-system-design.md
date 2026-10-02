# P08 玩家环境与护甲 System 设计

documentKind: system-design
partitionId: P08
derivedFrom: docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P08-player-environment-armor.md
sourceReportSessionId: ffcb132192224d1e8b2b878e48cddb71
designStatus: proposed
systemImplementationStatus: partial
verificationStatus: isolated-core-verifiers-pass; S05-core-verifier-pass; S05-sunlight-core-pass; integration-not-run
sourceModified: true
buildRun: true
testsRun: true

## 1. 目的与范围

本文把 P08 System 拆分报告细化为候选责任、API 组合和跨域协作边界，供后续实现准备使用。范围仍是 P08 的 9 组、100 个成员；完整成员身份和旧入口行为以来源报告为准。本文不重新分配其他分区成员，也不把已有 Component 提升为 System owner。

本文是后续设计稿，不是新的 runner 分区结算，不代表 System 已接线或迁移成功。此前 Component 设计/执行文档仅作历史候选材料，本设计以 P08 System 报告和 Version4 源码为准；两份既有文档没有修改。

## 2. 证据基线

| 来源 | 可确认的事实 | 本设计的证据边界 |
| --- | --- | --- |
| Version4 目标源码 | Terraria 主版本字符串为 v1.4.5.6；Player.cs SHA-256 为 E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86 | 目标行为的首要依据；被清空的方法体保持 unknown |
| Version4 CPG | manifest SHA-256 为 6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364；索引 967 shards、8,166,789 nodes、71,038,907 edges | 查询 complete 只表示索引预算内完成，不等于写者或调用闭包 |
| CPG zone1 查询 | 选定 Player.cs、NetMessage.cs、MessageBuffer.cs 范围内得到 18 条使用；MessageBuffer 的 zone1 写入为 confirmed，NetMessage 和若干 Player 使用为 partial/Unknown | 不能单靠查询确定 zone bytes 的唯一权威 writer |
| CPG zone5 / shimmer 查询 | `zone5` 字段 symbol 查询 complete；对 Player.cs、MessageBuffer.cs、NetMessage.cs、Main.cs 查询得到 6 条 member-use，MessageBuffer whole-field assignment 为 confirmed，其他若干 access mode 为 partial/Unknown，Main.cs 无命中。`SpawnFaelings` 在 Player.cs 与 MessageBuffer.cs 有 2 个 confirmed callsite；`TrySpawningFaelings` caller 查询为 partial/零命中；`UpdateBiomes` symbol 查询为 partial/零命中 | 源码确认 Player.Update(int) 有直接调用，NetMessage serializer 也直接写出 zone5，但 CPG access mode 不足以识别该 serializer。零命中不表示无调用；zone 生产、所有写者、别名和协议 authority 均未闭合 |
| CPG P08 关系查询 | DoUnbreakableWallScan 只返回 2 个 Player.cs callsite；Version4 源码可见 3 个入口。UpdateMaxTurrets 与 TrySpawningFaelings 的 callsite 查询为 partial/零命中并带 NoMatchingFactInScannedScope。RecalculateLuck 返回 Player.cs 与 MessageBuffer.cs 两个 confirmed callsite；UpdateLuck 查询为 partial/零命中，但 Player.Update 源码有直接调用。UpdateLuckFactors、UpdateLadyBugLuckTime、UpdateCoinLuck 在 Player.cs 各返回 1 个 confirmed caller。`luckNeedsSync` 在 Player.cs/NPC.cs 的所选范围返回 4 个 confirmed 写用点 | 查询 complete 只表示所选索引范围完成；零命中不解释为无调用。字段查询未闭合别名或全部 writer；均继续核对源码 |
| CPG immunity timer 查询 | `Find-CpgSymbols(environmentBuffImmunityTimer)` 在 `Terraria/Player.cs` 返回 1 个符号；`Get-CpgMemberUses` 对 Player.cs、MessageBuffer.cs、NetMessage.cs 返回 1 个 confirmed 写用、0 gaps | Version4 源码另有 tick 衰减、雪地 debuff 条件读取、Teleport 赋值共 3 处；CPG 命中不覆盖全部用点，行为与调用次序以源码为准 |
| CPG targeting 查询 | `tileTargetX/Y` 符号查询 complete；用点各返回 1 项 `NoAssignmentEvidence` partial。2026-10-01 只读 member-use 查询中，`tileRangeX/Y` 在 Player/Main/Item/MessageBuffer 四个选定 shard 均为 partial、0 项、`NoMatchingFactInScannedScope`，扫描 0 个 shard；`adjTile` 查询状态 complete 但只返回 2 个 `EvidenceStatus=partial`、`AccessMode=Unknown` 的 use facts。display-jar helper 的 callable facts partial、仅 3 个 CFG 节点并带 `CalleeEffectsNotExpanded`；call-site 查询为 partial/零命中并带 `NoMatchingFactInScannedScope` | 索引查询没有覆盖源码事实且 `SourceSnapshotId=null`；零命中不表示无关系。Version4 源码确认 `Player.Update(int)` 在 `Player.cs:15157` 调用该 helper；成员写入、range consumers 和 helper 行为继续按两份源码逐项核对 |
| 完整参考源码 | `D:\TRbackup\无任何删减通过编译` 的 `TerrariaServer.sln` 包含 `TerrariaServer.csproj`；项目文件 SHA-256 为 `5F92BADA8F3774633FAFAB5502EB9EEFC6C7EDBC2F8ED562403574129C96C8C4`，目标为 net40/x86，AssemblyInfo 版本为 1.4.5.6。`Terraria\Player.cs` SHA-256 为 `367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C`；`MessageBuffer.cs` 为 `48AABBBF4E0967964D97598E0168E9252ADD3DD868C66C6DA46AA87670EACAAB`；`NPC.cs` 为 `ED8AA2302730A9E046EF310E543204FBA391BD35B1C9C0B213C8065DFBBE39F0`；`Main.cs` 为 `E24E61C9903BB7995F47E36C51EDBE43481B643226861B717020877E7E22B63F` | 目录无 Git 元数据；已检查 P08 相关的 Player、MessageBuffer、NPC、Main 完整方法和项目配置。相关源码 hash 与 Version4 不同；参考项目用于补充意图和差异，不替代 Version4 目标行为。本轮没有在该参考项目上构建或测试 |
| 当前 NSSLC | P08 有多个 Component 声明和 Luck/WallRescan isolated-core System；`PlayerArmorAndCombatEffectsSystem.Rebuild` 重建六项 armor/combat facts，`PlayerSunScorchSystem.UpdateLocal` 以完整参考项目候选规则更新既有 scorch counter 与 sunlight flag，并通过 verifier。两者均未接旧入口或运行时调度。`PlayerArmorSetSystem` 已存在于 P07 mobility 方案，当前只覆盖 Solar dash 状态与 Nebula buff tier 转换，并由 P07 verifier 调用。`PlayerAbilityComponent` 与 `PlayerSummonCapacityState` 都包含 MaximumTurrets 和 PreviousMaximumTurrets | 隔离核心和 verifier 只证明局部实现；Version4 sunlight 行为、运行时接线和完整迁移仍未确认；P08 不重复创建 PlayerArmorSetSystem |
| P08 traversal 参数补证 | Version4 与完整参考的 `Player.Update(int)` 都先重置 gravity/fall/run/jump 参数，再按 portal、wet/down-dash、shimmer/water、vortex 顺序覆写；两边 `UpdateJumpHeight()` 的 mount/equipment/sticky/dazed 算法相同 | 只补齐 Version4 可见的参数计算；完整参考用于核对一致性，不替代目标。CPG 部分字段用点缺失，owner/调度和跨玩家并行语义仍 unknown |
| SS14 组织参考 | AntiGravityClothingSystem 订阅装备/姿态事件并调用 SharedGravitySystem；GravityComponent 单独保存重力状态 | 只参考 System 与 Component 的协作组织，不引用为 Terraria 行为证据 |

### 完整参考源码补证边界

完整参考项目与 Version4 的主版本字符串相同，但不能据此认定源码快照等价。Version4 的 DoUnbreakableWallScan 仅在非 dual-dungeons seed 时返回；扫描由 force、冷却或距离条件触发，状态变化时调用 BroadcastChange，方法内没有 netMode 门控。完整参考源码额外在 client netMode 返回，并且只在 server netMode 且状态变化时广播。完整参考源码的 UpdateMaxTurrets 还在 Player.cs:48953 和 Player.cs:49824 出现额外调用位置，Version4 源码只确认 Player.Update(int) 中的比较后调用。

完整参考源码中的 UpdateMaxTurrets 行为是：只处理 Main.myPlayer 对应的玩家；从 1000 个 projectile 槽读取 WipableTurret；当候选数超过 maxTurrets 时反复选择 timeLeft 最小者并 Kill，直至容量满足或达到循环上限。WipableTurret 还检查 owner、sentry 和 TurretShouldPersist。该算法是需要核实的意图线索，不能直接写成 Version4 已确认行为，因为 Version4 对应方法体只有调用追踪和非本地玩家 early return。

环境 buff immunity timer 是一个例外：Version4 与完整参考在 timer 每 tick 递减到 0、雪地 debuff 条件读取以及 Teleport 将 timer 设为 4 的局部行为一致（Version4 `Player.cs:14974,17397,21738`；完整参考 `Player.cs:24968,28369,37909`）。完整参考只作为差异复核；目标行为仍以 Version4 为准，Teleport owner 到新 System 的接线和 tick 调度不由源码静态对照证明。

Version4 `Player.Update(int)` 的 tile target 主路径使用 mouse/screen/gravity 公式并按 X/Y 上界后下界 clamp（`Player.cs:15109-15129`）；完整参考同一主路径保持公式和顺序（`Player.cs:25782-25802`）。完整参考另外有两条写入同一静态 target 的路径：`Player.cs:5067-5074` 在鼠标物品非空、未暂停时写坐标；`Main.cs:17656-17660` 在本地输入/交互路径写坐标，紧接着调用 `LookForTileInteractions`、`ChestChangeEvents` 和 `UpdateNearbyCraftingTiles`。本轮源码核对未在 Version4 `Main.cs` 找到对应写入与调用路径；Version4 `Player.Update(int)` 自身仍有 `LookForTileInteractions` 调用（`Player.cs:17006`）。完整参考主路径还调用 `UpdateNearbyInteractableProjectilesList`，Version4 对应路径没有。上述额外写者和调用链仅属完整参考差异，不能推定为 Version4 目标行为。Version4 的 `Update_AdjustTileTargetForDisplayJars` 只保留资格 guard/early return，其余行为 unknown；完整参考会在半径 1 邻域搜索 display jar 并改写 target（参考 `Player.cs:28683-28704`），不能将其作为 Version4 目标行为。

Version4 在本地玩家且非 display-doll/inanimate 条件下重置共享 `tileRangeX/Y` 为 `5/3`；仅 Journey mode 且 FarPlacementRangePower 已解锁并对该玩家启用时，应用 `range * 2 + 8` 得到 `18/14`（`Player.cs:19163-19178`）。Version4 的 `TileReachCheckSettings.cs:32-33` 与 `Projectile.cs:45235-45238` 读取该共享范围；`lastTileRangeX/Y` 在 Player 更新路径从共享值复制（`Player.cs:15636-15637`）。完整参考保留相同 reset/Journey 规则，但额外在 `UpdateEquips` 对 `equippedAnyTileRangeAcc` 增加 `3/2`（参考 `Player.cs:12992-12995`），并对本地当前物品 type 1923 增加 `1/1`（参考 `Player.cs:14805-14808`）。Version4 全树没有这两个额外范围写入，故不纳入目标 API。Version4 全树中 `adjTile` 只见声明和 ResetEffects 中的清空；完整参考版另外实现 `SetAdjTile`/`AdjTiles` 并有 Recipe 与 UI 消费者，不能将参考版 adjacency 规则补进 Version4。

Zone/shimmer 也存在 authority 与副作用顺序差异。Version4 `Player.TrySpawningFaelings` 在 `!_wasInShimmerZone && ZoneShimmer` 时无 netMode gate 地调用 `NPC.Spawner.SpawnFaelings(this)`，返回后才写 `_wasInShimmerZone`；如果 spawn 调用未正常返回，latch 赋值不会发生。完整参考版只在 `Main.netMode != 1` 时 spawn，但仍会更新 latch。Version4 `MessageBuffer` type 36 先保存旧 `zone5[0]`，再读入五个 zone bytes 与 `townNPCs`，假边沿升真时 spawn，最后转发 type 36；该片段把包内玩家索引无条件改成 `whoAmI`。完整参考版只在 server mode 才改写身份、触发 spawn 和转发。两份 `NPC.Spawner.SpawnFaelings` 都检查现存 Faeling、随机生成数量、shimmer 液体位置、安全区和屏幕位置，再创建 type 677；Version4 对有效 NPC index 直接发送 type 23，参考版仅在 server mode 发送。上述完整参考版的 gate 是对照证据，不能自动成为 Version4 的实现规则；收包入口的实际 authority、协议校验、预测/重放以及异常恢复仍 unknown。

完整参考版还给出了本地 zone 生产和发送路径：`Player.UpdateBiomes` 从 `SceneMetrics` 复制 biome flags（包含 `ZoneShimmer`）；`Player.Update(int)` 调用该方法，移动阈值路径也会先更新 SceneMetrics 再更新 biomes。`Main` 会比较五个 zone bytes 与 `townNPCs` 并在变化时发送 type 36，另有周期性 type 36 发送。两版 `NetMessage` case 36 都序列化 player id、五个 zone bytes、`townNPCs`。在本次检查的 Version4 Player/Main/NetMessage/MessageBuffer 源码中，未找到 `Player.UpdateBiomes` 方法或 Main 的 type 36 发送点；CPG 对 Version4 `UpdateBiomes` 也没有 symbol 命中。因而 Version4 本地 zone producer/该发送调用闭包保持 unknown，不能由完整参考版补成目标事实。

Luck 也存在版本差异。Version4 的 `UpdateLuck` 只顺序调用 `UpdateLuckFactors` 与 `RecalculateLuck`；前者更新 ladybug timer 和 coin luck。Version4 的 `MessageBuffer` type 134 收包路径会写入 luck 输入、调用 `RecalculateLuck`，然后转发 type 134；该路径在当前源码片段没有完整的 netMode 限制。Version4 源码可见 `luckNeedsSync` 在 Player 与 NPC 中的写入，但 `UpdateLuck` 本身没有清除该标志或发送包。完整参考源码则在本地玩家 `UpdateLuck` 中清除标志并发送 type 134，在 `UpdateLuckFactors` 中增加本地 torch luck 与 broken-mirror 更新；其 `MessageBuffer` 仅 server 模式改写玩家身份并转发。上述参考版新增/改变的逻辑不纳入 Version4 的目标行为契约。

### Armor/equipment source closure

2026-10-01 使用 `CpgEvidence.ps1` 的只读 Query API 查询 Version4 CPG manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`ImportStatus=complete`，索引 967 个 source path，`SourceSnapshotId=null`。全 source-path `Get-CpgMemberUses` 结果：`thorns` 6、`turtleArmor` 2、`turtleThorns` 2、`cactusThorns` 2、`spiderArmor` 2、`anglerSetSpawnReduction` 3、`vampireBurningInSunlight` 4、`honeyCombItem` 7、`maxTurrets` 20、`maxTurretsOld` 0（`partial/NoMatchingFactInScannedScope`）、`vortexStealthActive` 5。`maxTurrets`、`vampireBurningInSunlight` 等结果含 `AccessMode=Unknown`/`EvidenceStatus=partial`。所有结果仍是索引事实，不能当作当前源码快照绑定或完整行为闭包。

同一全范围查询中，`UpdateArmorSets` 与 `UpdateMaxTurrets` 的 call-site 查询为 `partial`/零项并带 `NoMatchingFactInScannedScope`；Version4 `Player.Update(int)` 源码仍确认 `UpdateArmorSets(i)` 位于 `Player.cs:15348`，容量变化比较后调用 `UpdateMaxTurrets()` 位于 `Player.cs:15353-15356`。`ResetEffects` 的索引仅返回一处调用，而源码可见 Ghost 路径与正常更新路径（`Player.cs:3855,15207`）。这些零命中及不完整结果都不表示无调用。

2026-10-01 follow-up CpgEvidence 查询中，`UpdateSunScorch` 与 `VampireSeedSunlightExposure` 的 symbol 查询均 complete，但各自 call-site 查询均为 `partial`/零项并带 `NoMatchingFactInScannedScope`；Version4 源码仍确认 `Player.Update(int)` 调 `UpdateSunScorch()`（`Player.cs:15297`）。该结果不能解释为无调用，producer 调度与目标 body 仍按源码状态保留为 unknown。

源码复核显示，Version4 正常 Player 更新依序执行 `ResetEffects`、`UpdateBuffs`、`UpdateEquips`，之后在 `UpdateSunScorch` 和其他同步步骤后执行 `UpdateArmorSets`（`Player.cs:15207-15348`）。`ResetEffects` 将 `thorns` 与 armor/combat flags 归零（`Player.cs:10374,10690-10695`）；Buff 14 与 Dryad Ward 路径分别设置/增加 `thorns`（`Player.cs:4444-4447,4715-4718`）；`UpdateEquips` 按当前 armor visual slots 写 `turtleArmor` / `spiderArmor`（`Player.cs:20270-20276`）。活动 `UpdateArmorSets` 先以当前头/身/腿 item 构造 `ArmorSetBonus.QueryContext`，取得完整套装后执行 `Effect(this)`，随后调用 Beetle、Solar、Stardust、Chlorophyte、Vortex helpers 与 `ApplyArmorSoundAndDustChanges`（`Player.cs:9547-9557`）。Version4 `Terraria.DataStructures/ArmorSetBonuses.cs` 中 Angler、Cactus、Turtle effects 写 `anglerSetSpawnReduction`、`cactusThorns`、`thorns` 与 `turtleThorns`（`:191-211`）；目标 `NPC.cs:623` 读取 Angler flag。该文件的 Molten effect 读取 `vampireBurningInSunlight` 以决定 `buffImmune[24]`（`:373-382`）。当前 Version4 源码未见 `honeyCombItem` 的读取消费者；其 ResetEffects/赋值不构成 Item payload 生命周期闭包。Query lookup 与 effect delegate 不能合成纯 Query：effect 会改写 Player 状态，且套装效果还可修改其他能力或 buff。

当前 Version4 全树没有找到对 `turtleThorns`、`cactusThorns` 或 `turtleArmor` / `spiderArmor` 的直接 combat consumer；`thorns` 的已定位逻辑含 buff 阈值合并、套装 effect 写入，但没有找到完整参考版的 Hurt 反伤 consumer。该结论只约束当前静态 C# 源码，反射、外部生成或目标 runtime hook 闭包仍 unknown。Version4 的 `Player.Update(int)` 调用 `UpdateSunScorch()`（`Player.cs:15297`），但该方法在本地玩家 guard 后结束（`Player.cs:17763-17773`），`VampireSeedSunlightExposure()` 为空方法体（`Player.cs:17774`）。独立的 `UpdateSunScorchValues()` 会清除死亡状态、推进 scorch counter 并调整 sizzle audio（`Player.cs:17713-17757`），其可见调用点是死亡更新路径（`Player.cs:10073`）；源码没有闭合正常玩家的 exposure producer 调用。与此同时 `ResetEffects` 清除 `vampireBurningInSunlight`（`Player.cs:10695`），Molten set effect 读取该 flag 并据此设置 `buffImmune[24]`（`ArmorSetBonuses.cs:373-382`）。因此 Version4 当前 writer/完整活跃 sunlight 行为仍为 unknown，不能把空方法解释为无 producer。Version4 对 `honeyCombItem` 只确认重置和赋值，没有找到读取消费者。

完整参考项目 `D:\TRbackup\无任何删减通过编译` 与 Version4 都标记 `1.4.5.6`，但源码哈希不同，不能认定快照等价。其活动 `UpdateArmorSets` 同样走 `ArmorSetBonuses.GetCompleteSet(...).Effect(this)`，另保留无已知 caller 的 `UpdateArmorSetsOld`。参考版的 `ResetEffects` 也清除 `vampireBurningInSunlight`（`Player.cs:19083`）；本地玩家更新会走 `UpdateSunScorch -> VampireSeedSunlightExposure -> UpdateSunScorchValues`。该 exposure 实现检查 vampire seed、地表/白天/雨与 eclipse、graveyard/glowshroom、天空强度、wet、选中物品和 mount 条件，再向上扫描最多 15 格 tile，遇到可见天空墙体时将 flag 置 true（`Player.cs:28830-28919`）。scorch 达阈值后还会清 buff immunity、请求火焰粒子、添加 debuff、尝试下 mount 并清 wings/rocket boots（`Player.cs:28841-28877`）。Hurt 路径还使用 thorn flags 计算反伤（`Player.cs:31681-31725`），并消费 `honeyCombItem` 生成 bee projectile 和 buff（`Player.cs:38832-38860`）。这些完整方法体为 Version4 空/存根方法的同版本候选行为，可指导待补证范围；它们仍不是 Version4 已确认事实。完整参考和 Version4 的 `Terraria.DataStructures/ArmorSetBonuses.cs` SHA-256 分别为 `B1A56D71964D69BE91FBF81AB4981555D573C62AF4F75F451E1A55EB595C9A91` 与 `E5B960C22E731357D79A444E6438B53055344B394C6C140F4796C8D4136E8DF3`。

**S05 边界决定保持 proposed/partial：** `PlayerArmorAndCombatEffectsSystem.Rebuild` 按显式输入重建 `thorns`、`turtleArmor`、`turtleThorns`、`cactusThorns`、`spiderArmor`、`anglerSetSpawnReduction` 六项事实，不再写 `VampireBurningInSunlight`。新增的 `PlayerSunScorchSystem.UpdateLocal` 读取显式天气、区域、天空强度、wet、选中物品、mount 和脚下向上的 Tile facts，扫描最多 15 格，提交现有 `PlayerEnvironmentalPressureComponent.SunScorchCounter` 与 `PlayerArmorAndCombatEffectsComponent.VampireBurningInSunlight`，返回计数、sizzle 音量和阈值副作用请求；不直接执行音频、buff、粒子、下坐骑、wings/rocket 操作。独立 `UpdateDead` 路径负责清除 flag 并按死亡更新将 counter 减 2。完整参考项目 `D:\TRbackup\无任何删减通过编译` 的曝光/阈值规则只作为候选行为，Version4 的 `UpdateSunScorch` 仍只有本地玩家 guard，`VampireSeedSunlightExposure` 仍为空，因此目标曝光行为与效果执行次序保持 unknown。当前没有 runtime caller、Tile adapter、effect owner 或 scheduler 接线。`honeyCombItem` 仍是外部 Item payload，未被纳入组件。`PlayerArmorSetSystem` 位于 `src/NSSLC/Component/Player/Armor/`，是 P07 Beetle/Solar 与 Nebula mobility core，不视为 P08 effect owner，也不创建同名第二个 System。`AnglerSetSpawnReduction` 已作为显式输入由 NSSLC `NpcSpawnRateSystem` 消费，但其与装备 writer 的运行时桥接未实现。P06 thorn consumer、P14 spawn consumer、sunlight/environment effect、P09 item/loadout 输入的唯一 owner 与接线点需继续 integration review；当前 isolated cores 不构成完整 S05 owner。

S05 isolated-core verifier 的既有运行记录：SDK 10.0.400 经仓库串行 wrapper 构建，build exit code 0、0 warnings、0 errors；运行 exit code 0，输出 `PASS: player armor and combat effect rebuild`。该 verifier 只证明独立 `Rebuild` 的局部 reset/replacement 断言，不覆盖 Version4 旧入口、sunlight producer、完整消费者闭包或运行时调度；后续 integration verification 仍 `not-run`。

新增 sunlight core verifier 使用同一 SDK 和仓库串行 wrapper；build exit code 0、0 warnings、0 errors，run exit code 0，输出 `PASS: player sunlight exposure, scorch threshold and owner isolation`。它覆盖隔离核心的代表性规则，不作为 Version4 行为等价证据；旧入口、效果执行、运行时接线和 integration verification 仍 `not-run`。

## 3. 边界决策

**总体采用一个同步兼容协调阶段加少量能力 System 候选，状态为 proposed。** 不按九个 Component 检查点机械生成九个 update System；Component 保存状态，System 承担有明确不变量和提交边界的行为，Query、Command、Adapter、Projection 按实际调用契约选择，不预设必须新增类型。

若目标运行时已有玩家帧协调入口，初期通过该入口保持已观察的旧调用次序。现有资料没有证明 NSSLC 中存在 P08 System 注册或调度入口，因此不在本设计中创建第二个完整 PlayerUpdateSystem，也不以文件顺序表达运行顺序。

| 候选能力边界 | 责任 | 明确不负责 | 状态 |
| --- | --- | --- | --- |
| PlayerEquipmentEffectsSystem，先作为协调职责 | 按旧调用入口协调 equipment facts 与 armor/set 派生提交；安排消费者只读已提交快照 | 不拥有 P09 装备槽；不写 `VampireBurningInSunlight` | 候选；hook 与跨分区 writer 闭包 partial；当前 Rebuild 与 sunlight core 已拆开 flag 写入责任 |
| PlayerEnvironmentSystem | 在明确输入下维护 zone/detection 派生事实，并表达 shimmer transition；`PlayerShimmerTransitionSystem` 已按 Version4 本地顺序实现 transition core | 不假定它是 zone byte 唯一扫描者；不直接拥有 NPC spawn 或 Network 协议 | 本地 transition core partial；zone input producer、NPC spawn port 绑定和 packet adapter unknown |
| PlayerSunScorchSystem | 显式读取环境、物品、mount 与 Tile facts；复用 `PlayerEnvironmentalPressureComponent.SunScorchCounter` 和 `PlayerArmorAndCombatEffectsComponent.VampireBurningInSunlight`，返回 sizzle volume 与阈值效果请求 | 不新增重复状态 Component，不拥有 armor/loadout，不直接写 World/Tile 或执行音频、buff、particle、dismount 和 movement effects | 完整参考项目候选逻辑已落为 isolated core 并通过窄 verifier；Version4 活跃方法体仍被清空，目标行为 unknown。运行时输入 adapter、effect owners、调度与调用顺序未接，integration not-run |
| PlayerTargetingSystem | 将输入快照、屏幕位置和 gravity 方向转成 tile target 坐标，保留 clamp 顺序；通过显式 World/Tile port 按旧顺序确保并读取邻格，再应用 axe 修正规则；从显式旧范围输入计算 Version4 baseline，并把 effective range 写入 per-player compatibility snapshot | 不把 static target/range workspace 默认复制为 per-player authority；World tile 只经显式 port 修改；不从完整参考复制 range accessories/item effects 或 display-jar 目标调整 | 坐标、axe decision、Tile port 编排、`5/3` 与 Journey `18/14` baseline projection、last-range snapshot writer partial；具体 adapter、静态范围 authority、snapshot 调用时点、消费者、display-jar helper、adjacency 与 workspace 并发语义仍 partial/unknown |
| PlayerTraversalPhysicsSystem | 从显式帧输入解析 Version4 的重力、下落/奔跑基值与环境分支，提交实例物理状态并返回 jump 参数 snapshot 供 P07 Mobility 使用 | 不写 position/velocity；不读取/写入旧 static 字段，不裁定 Mount/P07/Collision 优先级 | isolated core 已实现；输入生产者、调用相位和跨玩家并行语义仍 unknown |
| PlayerTraversalCapabilitySystem | 向 P07/Collision 提供 gravity/water control facts 的只读投影 | 不积分或写 position/velocity，不裁定 Mount、Physics 和 Movement 的优先级 | P03/P07 integration-review |
| PlayerLuckSystem | 读取显式 luck 输入，计算结果并由单一 owner 提交 luck 状态；同步由外部协议边界处理 | 纯计算不写 luckNeedsSync、不发布网络消息 | 输入 writer 与 sync owner partial |
| PlayerWallRescanSystem | 在满足 seed/force/cooldown/distance 条件时扫描，维护扫描缓存和 inside flag，变化时通知对应 Network owner | 不把 luck 聚合放在同一 System；不推断 cache teardown 生命周期 | target 方法局部规则 confirmed，调用/生命周期 partial |
| Armor/combat effects | 候选由既有装备阶段协调能力事实重建；combat、spawn 与 environment 消费者分别读各自快照 | 不以同一 Component 或同一旧方法名推导单一 owner；不把 Item payload 或 NPC spawn 写入 P08 | writer、effect 回调闭包及真实 hook 仍 partial/unknown；本轮不新增 System |
| Turret capacity | 暂无可批准的新 System API | 不按方法名实现 projectile trim，不决定重复 Component 的最终 owner | blocking unknown |

能力名称只是文档内的 proposed 身份，不承诺最终类名或独立调度节点。若证据显示某项职责没有独立 owner、不变量或协作收益，则保持在协调 System 内部。

## 4. 状态与唯一 writer 规则

| 状态类别 | 拟定规则 | 必须保留的协作边界 |
| --- | --- | --- |
| Zone 与 shimmer latch | zone state 的写入集中在确定的 owner API；网络解码调用 owner API，而不是绕过 owner 直接改 Component。分别保留本地 tick 的 spawn-before-latch 与 type 36 入站的 commit-before-spawn-before-forward 顺序 | Version4 本地 zone producer 与 type 36 发送 caller 未找到；包内身份/角色映射、预测、去重、authority 与 spawn 权限交 integration-review |
| Environment buff immunity timer | `PlayerZoneAndEnvironmentStateComponent.EnvironmentBuffImmunityTimer` 保存 timer；Environment System 负责逐 tick `max(0, timer - 1)` 和 Teleport grant 4 ticks | 雪地 debuff 仍只在旧条件全部满足且 timer 为 0 时添加；Teleport owner 的调用时点、Player tick scheduler 和 debuff consumer 接线仍为 unknown |
| Vampire sunlight exposure/scorch | 一个 owner 每帧先清理并重算临时 exposure flag，再更新现有 `PlayerEnvironmentalPressureComponent.SunScorchCounter`；ArmorSet effect 只读 `PlayerArmorAndCombatEffectsComponent.VampireBurningInSunlight` | 不新增重复状态。Version4 `UpdateSunScorch`/producer body 缺失。完整参考同版本实现仅作为 candidate oracle；Tile/weather/mount/input 生产者以及 buff/particle/audio/dismount effects 的 owner 都需显式确认。现有 `ResetForLifecycle` 的调用/cleanup 闭包 unknown |
| Target 与 range | 从显式 mouse/screen/gravity/world-size 输入解析 tile target；从显式当前范围、本地/装饰实体、Journey 与 power facts 解析 Version4 范围 baseline；通过单一 API 把 effective range 捕获到 per-player compatibility snapshot；held-item 与 center/neighbor Tile facts 显式进入 axe correction | 坐标、range baseline 与 axe 规则为确定性 API；Tile ensure/read 经 World/Tile port；snapshot writer 不写旧 static workspace。缺少静态范围 authority、snapshot call phase、adjacency builder 与全部消费者闭包时，不提升为完整 owner |
| Movement 与 mobility | P08 仅提供能力参数或不可变 snapshot；最终 movement integration 仍属于既有 Movement/Physics owner | P03 Mount、P07 Movement/Collision 的读写方向、优先级和 commit 时点需共同确认 |
| Armor 与 equipment | 一个经过批准的 writer 重建 P08 派生 armor/combat/set flags | P09 提供 loadout/effect 输入；P06 消费 combat facts；P14 决定 spawn qualification 的 owner |
| Luck | 计算与提交拆开；`luckNeedsSync` 是 Version4 中独立于公式的同步 latch，不作为纯计算的隐藏副作用 | CPG 在 Player.cs/NPC.cs 所选范围查到 4 个 confirmed 写用点；Version4 还有 MessageBuffer type 134 入站重算/转发。完整 writer、客户端/服务器分工及 reference-only 本地发送逻辑需分开审查 |
| Wall rescan | cache、cooldown、last position 和扫描结果由同一确定 writer 更新 | tile/world scan 是外部读取；广播只在 Version4 已观察条件满足时发生。死亡、传送、world unload 的 cache 生命周期 unknown |
| Turret capacity | 在决定唯一 owner 前不增加 MaximumTurrets/PreviousMaximumTurrets 的第二套权威状态 | 当前 PlayerAbilityComponent 与 PlayerSummonCapacityState 有字段重叠；P05/P06/P15 与 projectile effect 交 integration-review |
| 句柄与对象 payload | Audio handle、Item payload、movement cache 不因分区字段归属而成为长寿命 P08 Component 状态 | 按需由音频、Item、Movement 或 cache owner 管理，资源结束/重试语义未闭合 |

## 5. 候选 API 组合

| Concept ID | 旧入口/观察点 | 拟定新组合 | 写权限与副作用 |
| --- | --- | --- | --- |
| P08.Equipment.DerivedEffects | Player.Update(int) 中的 ResetEffects、UpdateEquips、UpdateArmorSets 与 turret comparison | coordinator 开始派生重建 -> 读取 P09/P03 facts -> owner 系统提交 P08 能力 facts -> 发布只读 snapshot | 一项不变量只有一个提交者；重建次序保留为显式调用约束 |
| P08.Environment.VampireSunlight | Version4 `Player.Update(int)` 的 `ResetEffects` -> `UpdateEquips` -> `UpdateSunScorch` -> 后续 `UpdateArmorSets`；完整参考 `UpdateSunScorch` -> `VampireSeedSunlightExposure` -> `UpdateSunScorchValues` | 显式天气/区域/天空强度/wet/位置/选中物品/mount 与 tile-column facts -> 单一 `PlayerSunScorchSystem` 更新两个现有状态 Component -> threshold effects 经确认的 owners/ports -> 发布只读 flag 给 Molten armor effect | 完整参考给出同版本候选输入与 effects（`Player.cs:28830-28919`），Version4 对应方法体被清空。target observation、tick order、Tile adapter、effect owners 和异常时点未闭合；不得把 candidate 当作 confirmed |
| P08.Environment.ZoneAndTransition | zone properties、Player.TrySpawningFaelings、MessageBuffer type 36 zone packet；完整参考版另有 Player.UpdateBiomes 与 Main 的 type 36 生产/发送 | 本地 transition core：输入当前 `ZoneShimmer` -> 对 false-to-true 请求 NPC spawn -> spawn 正常返回后提交 latch；包路径先捕获旧 shimmer bit -> 提交 5 个 zone bytes 与 townNPCs -> edge 时向 NPC owner 发 spawn request -> 按已确认的网络 authority 转发原协议 | `PlayerShimmerTransitionSystem` 保留本地调用顺序；adapter 不直接写 Component。Version4 本地 zone producer 与 type 36 outbound caller unknown；包路径的 identity rewrite/转发、两条路径去重及异常后重放仍 unknown |
| P08.Environment.BuffImmunity | `Update(int)` tick decrement、雪地 debuff 条件与 `Player.Teleport` grant | tick -> `PlayerEnvironmentBuffImmunitySystem.Tick`; Teleport 在 legacy 方法进入 `try` 后、其他 Teleport side effect 前调用 `BeginTeleportImmunity`; debuff consumer 只读 timer | Environment System 是 timer 的拟定写者；Teleport 跨 P01 owner 调用、tick phase 与 debuff 路径未接入，不能把静态 API 验证写成完整组合 |
| P08.Interaction.TileTargetAndRange | Player.Update(int) 的 screen/mouse/gravity 坐标转换、world-edge clamp、null Tile 初始化、axe neighbor correction、display-jar helper 调用与范围 baseline reset/copy | `PlayerTileTargetCoordinateInput` -> `ResolveCoordinate`；显式 range input -> `ResolveRangeBaseline` -> `CaptureEffectiveRange`；`ResolveTarget` -> World/Tile port 按左、右、中心顺序 ensure -> 条件读取 center/left/right facts -> axe correction -> immutable coordinate -> display-jar boundary | 已实现并验证坐标、range baseline/snapshot writer、axe 分支及 port 编排；具体 Tile adapter、Version4 display-jar helper 完整效果、range static commit/call phase、消费者和 runtime composition 未实现或仍 unknown |
| P08.Traversal.PhysicsParameters | `Player.Update(int)` 参数 reset 与 portal/wet/shimmer/vortex 分支；其后 `UpdateJumpHeight()` | `PlayerTraversalPhysicsSystem.Resolve(explicit frame input)` -> `CommitMovementState` 更新 per-player physics component；返回 jump height/speed 交给现有 `PlayerMobilitySystem.UpdateJumpParameters`。独立只读组合 `PlayerTraversalCapabilitySystem.CreateSnapshot(physics, gravity/water, mobility)` 导出移动/碰撞能力事实 | S03 core 不读写旧 static 字段，不写 position/velocity；snapshot 不提交状态；`defaultGravity` producer、frame adapter、旧静态跳跃值共享语义、P03/P07/Collision owner 和调度可见性均 unknown |
| P08.Traversal.Capabilities | Version4 `Player.ShouldFloatInWater` 与 Player.Update/collision consumers 读取移动、水行走、重力和液体能力字段 | `PlayerTraversalCapabilitySystem.CreateSnapshot` 只读组合 physics、gravity/water 和 mobility facts；已有 `PlayerInteractionAndSelectionPropertiesQuery` 继续负责含 controlDown/mountType 条件的 `ShouldFloatInWater` 计算，不新增重复谓词 | Snapshot 只公开已定位为 traversal 输入的 facts；`HasFloatingTube` 在 Version4 仅见重置/装备写入、未见读取，因此不进入 movement/collision snapshot，仍保留在原组件待归属审查 |
| P08.Traversal.SwimTime | 输入跳跃分支刷新 `swimTime`；`PlayerFrame()` 每次调用时递减，离水时清零 | `PlayerSwimTimeSystem.RefreshForMermanJump` 使用 active-jump/airborne/merman/mount/cart facts 与 `<= 10` 阈值；`RefreshForFlipperJump` 使用新跳资格/wet/flipper facts 且仅 `swimTime == 0` 时设 30；`TickFrame` 在 `swimTime > 0` 时先减 1，随后离水归零 | System 仅改 `PlayerEnvironmentMobilityStateComponent.SwimTime`，不写 jump counter、velocity、frame 或脚手动画数据。Version4 `Player.Update(int)` 早退分支在 `UpdateBuffs` 后调用 PlayerFrame 并随后 return；正常分支在 `ItemCheckWrapped` 后调用。两个直接 callsite 已从源码确认，CPG callsite 为 partial/零命中；输入分支接线与未知外部入口仍 unknown。完整参考的 dummy/render caller 不算作目标边 |
| P08.Luck.Recalculation | UpdateLuck -> UpdateLuckFactors 先衰减 ladybug timer 与 coin luck，再执行 RecalculateLuck；MessageBuffer type 134 是独立入站路径 | 显式 factor input -> 单 owner 提交 timer/coin -> 从当前状态组装显式公式输入 -> 计算并提交 Luck -> replication adapter | `dayRate` 显式传入；因子更新不修改 luckNeedsSync 或发包。MessageBuffer 入站重算与转发是单独组合，Version4 UpdateLuck 不清除 latch |
| P08.Environment.WallRescan | DoUnbreakableWallScan(force) | 由确定的调用入口提供 center/force -> scan port -> 单 owner 更新 cache/result -> 状态变化时调用 network effect | Version4 的 seed、force、cooldown、距离和 changed-only 广播条件作为兼容契约；Version4 方法内没有 netMode gate |
| P08.Combat.TurretCapacity | UpdateMaxTurrets | API 暂不定义；先解决 Version4 空方法与完整参考算法之间的差异 | 不把参考算法或 CapacityState.RequiresTurretTrim 当作已批准的旧行为 |

除非实际调度协议要求延迟或跨边界恢复，不新增 Command 类型、事件总线或队列。Query 不执行写入；Adapter 只做输入/协议转换；Projection 只导出不可变快照。

## 6. 旧调用次序与可见性

Version4 Player.Update(int) 中可见的相对次序包括 ResetEffects、UpdateBuffs、UpdateEquips、UpdateSunScorch、DoUnbreakableWallScan(force: true)、UpdateLuck、活动 Mount.UpdateEffects、UpdateArmorSets，以及容量变化后 UpdateMaxTurrets 再保存 maxTurretsOld。Player.cs:3855 的 Ghost 路径也调用 ResetEffects。`UpdateSunScorch` 在 Version4 是 stub；此处仅记录入口位置。该顺序是源码中的同步调用事实，不是独立 ECS scheduler DAG，也不能据此判定可并行。

Zone/shimmer 是另一条交互链：Version4 本地 `Player.TrySpawningFaelings` 在 spawn 返回后更新 latch；`MessageBuffer` type 36 则先提交五个 zone bytes 与 `townNPCs`，检查 shimmer false-to-true edge，调用 spawn，再转发 type 36。二者顺序不同，迁移不可在未证明 authority/预测规则前合并或去重。完整参考版的本地路径加 client spawn gate；包路径的 identity rewrite、spawn 和转发只在 server mode 执行，`NPC.Spawner` 发送 type 23 也只在 server mode 执行。这些 reference-only gate 不属于 Version4 目标基线。完整参考版 `Player.UpdateBiomes` 从 SceneMetrics 写 zone flags，Main 变更/周期路径发送 type 36；Version4 对应 producer/caller 未找到，因此 local zone 输入、type 36 outbound 调用与网络角色仍 unknown。

Environment buff immunity timer 在 Version4 `Player.Update(int)` 的 tick 开头递减，并且在其后 snow debuff 条件里读取；Teleport 在方法开头进入 `try` 后立即设为 4，再执行 grapple、vanity、shimmer 等其他 Teleport 效果。Timer System 只编码这两个写操作；未来接线必须保留 tick/debuff/Teleport 的原相对时点。完整参考在对应三个位置给出同样行为，但不能代替目标 scheduler、P01 Teleport owner 或 debuff 路径的集成证据。

Target coordinate projection 是 `Player.Update(int)` 同步路径中的纯计算：鼠标位置加 screen offset 后以 16 像素换算 tile；仅 `gravDir == -1f` 时以 screen bottom 镜像 Y；先 clamp 到 `maxTiles - 5`，再 clamp 到至少 5。Version4 随后按左、右、中心顺序确保 tile 非 null，并按 held axe/item facts 与 tile type 323/frameY 修正邻格，再调用 `Update_AdjustTileTargetForDisplayJars`（`Player.cs:15131-15157`）；该 helper 在 Version4 只保留资格 guard/early return，完整行为 unknown。完整参考会搜索半径 1 邻域并改写 target，但该实现不属于 Version4 baseline。新 `PlayerTargetingSystem` 保留纯坐标/axe 规则，以 `ResolveTarget` 经显式 World/Tile port 按左、右、中心顺序 ensure，并仅在 axe 分支条件满足时读取 center/left/right facts；没有具体 World adapter，也不提交旧 static target。`ResolveRangeBaseline` 只表达 Version4 可见的 `5/3` 重置与 Journey `18/14` 规则，并在非本地或 display-doll/inanimate 输入下保留当前共享范围；`CaptureEffectiveRange` 把显式 effective range 写入 `PlayerTileTargetingAndRangeStateComponent`，对应旧 `lastTileRangeX/Y` 复制，但调用相位没有接线。范围 writer 与消费者的真实组合、display-jar、adjacency 和 runtime hook 仍未接线。

DoUnbreakableWallScan 在 Version4 只有 dual-dungeons seed 时继续；force、冷却到期或与上次扫描位置距离达到 256 像素时扫描，扫描后重置 20 tick 冷却并保存位置；inside 状态变化时调用 BroadcastChange，方法内不检查 netMode。完整参考源码增加 client 早退和 server-only 广播。实现 Version4 目标时必须保留该差异，除非另有经过批准的行为变更。

目标 ECS 中各 System 的调度阶段、事件注册顺序、结构变更可见点、并行规则和异常恢复仍 unknown。在拿到 scheduler/owner 证据前，文档不声明新的 phase 或 barrier。

## 7. 依赖方向

| 提供者 | 消费者 | 传递内容 | 禁止的方向 |
| --- | --- | --- | --- |
| P09 equipment/loadout | equipment effect owner、luck owner | 装备与 Item effect 输入 | P08 不维护 inventory/slot owner |
| P03 Mount 与 P07 Movement/Physics | traversal capability owner | gravity、jump、run、swim、slow-fall 输入/能力事实 | P08 不双写 Mount/velocity/position |
| Environment/World/Tile readers | zone、targeting、wall rescan owner | 当前 world/tile/position snapshot 或读取端口 | Query 不隐式修改 tile/world |
| P08 armor/combat snapshots | P06 combat | thorns、armor combat facts | combat resolution 不写回 P08 装备状态 |
| P08 zone/shimmer transition | NPC spawn owner 与 Network adapter | transition 请求和版本化协议数据 | Network adapter 不绕过 owner 更新状态 |
| P01 Teleport owner | P08 Environment buff immunity owner | Teleport 开始时的 4-tick grant 请求 | P08 不接管 Teleport movement/position commit；调用时点需由 integration-review 接入 |
| P08 snapshots | P11 presentation 与其他读者 | 只读能力/显示投影 | presentation 不成为 P08 authority |

所有跨分区共享字段、快照、协议与顺序决定均为 crossSubsystemOwner: integration-review。

## 8. 参考组织与实现落点

现有 P08 Component 文件位于 src/NSSLC/Component/Player 下的 Environment、Interaction、Movement、Combat、Luck 等能力目录。`PlayerArmorSetSystem` 在 `Armor/` 下承担 P07 Beetle/Solar armor-set 与 mobility 交接候选；其 `BeginSolarDash` / `UpdateNebulaBuff` 目前只在 P07 mobility core verifier 中被调用，不能据此视为 P08 equipment/armor 派生效果 owner。P08 应复用审定后的能力边界，不创建同名重复 System。若后续代码变更获批，新 System/Query/Adapter 应沿用对应能力目录和当前命名约束；此处不新增 catch-all 目录，也不将路径顺序解释成调度顺序。

SS14 的 AntiGravityClothingSystem 通过装备/姿态事件调用 SharedGravitySystem，由共享 GravityComponent 承载重力状态。这只支持“事件边界可委托领域 owner、状态与行为可以分开组织”的参考，不支持把 Terraria 装备、预测、网络或 System 语义照搬到 NSSLC。

## 9. 阻塞决策

1. 版本：完整参考项目与 Version4 同标记 v1.4.5.6，但 `Player.cs` hash 不同；继续确认哪些完整参考方法体对应 Version4 被清空的 body，未闭合部分保持 unknown。
2. Turret：确认 Version4 中 UpdateMaxTurrets 的目标行为来源、Projectile 删除 authority、调用入口及现有 MaximumTurrets 唯一 owner。
3. Zone/network：确认 zone byte 的生产者、协议版本、server/client/预测规则、两条 shimmer spawn 路径的去重/重复语义。
4. Targeting：确认 static target/range workspace 的多玩家隔离，tile null 初始化与 axe-neighbor correction 的 owner，Version4 display-jar helper 被截断的完整行为、range 输入及服务器端是否存在；当前坐标 core 不等于完整 target composition。
5. Traversal/equipment：由 P03/P07/P09 决定 Mount、physics、equipment 与 P08 capability 的唯一 writer 和提交可见点。
6. Combat/spawn/presentation：由 P06/P11/P14 决定效果、资格和显示字段的消费者/写者；honeyCombItem 生命周期仍 unknown。sunlight candidate 的阈值 effects 和单一 flag writer 也需经 integration-review。
7. Luck/wall rescan：确认所有 luckNeedsSync writers、网络提交、扫描 cache 清理和世界生命周期。
8. Environment immunity：P01 Teleport owner、Player tick 调度与 snow debuff consumer 的接线和先后顺序仍未确认。
9. Persistence/audio：Version4 Player.Serialize/Deserialize 是空方法；存档格式、audio handle 所有者和 cleanup 均不能由本设计补造。

上述决策影响对应能力的执行，不抹去其他可独立继续的行为描述。设计整体仍为 proposed。

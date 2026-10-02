# P08 玩家环境与护甲 System 执行计划

documentKind: system-execution-plan
partitionId: P08
derivedFromDesign: docs/system-decomposition/design/2026-09-30-version4-P08-player-environment-armor-system-design.md
derivedFromSystemReport: docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P08-player-environment-armor.md
sourceReportSessionId: ffcb132192224d1e8b2b878e48cddb71
designStatus: proposed
executionStatus: partial-execution
implementationStatus: partial
verificationStatus: isolated-core-verifiers-pass; S05-core-verifier-pass; S05-sunlight-core-pass; integration-not-run
sourceModified: true
buildRun: true
testsRun: true

## 1. 执行边界

本文记录 P08 System 迁移的分阶段执行和当前进度。P08-S01 本地 shimmer transition core、Environment buff immunity timer、P08-S06 luck、P08-S07 墙体扫描已有 isolated System 和窄范围 verifier；旧入口到新 owner 的行为等价、运行时接线和完整 P08 迁移均未验证，当前不得表述为迁移成功。

原 P08 runner session 已结算，本轮工作沿用其 claim 输入和 `sessionId` 作为来源报告 provenance；不重新 claim、不手工改 task-state/ledger/lock/outputReport，也不冒称本文件属于新的 runner settlement。当前用户请求授权的增量实现限于已证实的 P08 core；后续跨 System 接线仍须先满足对应 owner、API 和构建约束。

## 2. 基线和目标路径

P08 成员总表、Version4 source hash、CPG manifest 与已知缺口见来源 System 报告。完整参考项目 `D:\TRbackup\无任何删减通过编译` 的 `TerrariaServer.sln` 包含 `TerrariaServer.csproj`；项目 SHA-256 为 `5F92BADA8F3774633FAFAB5502EB9EEFC6C7EDBC2F8ED562403574129C96C8C4`，目标为 .NET Framework 4.0、x86，AssemblyInfo 版本为 1.4.5.6。完整参考 `Terraria\Player.cs` SHA-256 为 `367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C`，`Terraria\Main.cs` 为 `E24E61C9903BB7995F47E36C51EDBE43481B643226861B717020877E7E22B63F`。Version4 `Main.cs` SHA-256 为 `66DBE1D1E6FB89A24A512309BB039DFEC7C06D4678D2A22D7ADECBAD3AFD6520`。两个目录无内容等价声明；完整参考的调用点、Luck 同步与 wall scan 网络门控均与 Version4 有差异。本轮没有在完整参考项目上构建或测试，后续行为预期仍以 Version4 为准。

当前目标代码路径为 src/NSSLC/Component/Player。P08 Component 声明已存在于以下能力目录，但来源报告没有找到把这些状态接入运行路径的 P08 System：

| 能力目录 | 当前 Component |
| --- | --- |
| Environment | PlayerZoneAndEnvironmentStateComponent、PlayerEnvironmentMobilityStateComponent、PlayerEnvironmentDetectionAndSpawnStateComponent |
| Interaction | PlayerTileTargetingAndRangeStateComponent |
| Movement | PlayerMovementPhysicsStateComponent、PlayerGravityAndWaterTraversalStateComponent |
| Combat | PlayerArmorAndCombatEffectsComponent、PlayerArmorSetAndTurretStateComponent |
| Luck | PlayerLuckAndRescanStateComponent |

新 System、Query、Adapter 如经批准，候选路径是对应的 src/NSSLC/Component/Player/{Environment,Interaction,Movement,Combat,Luck} 能力目录；在核对更深层 AGENTS.md、同目录命名和实际依赖注册前，不预先创建类型或目录。

## 3. Gate 与阶段

| Gate | 工作 | 进入条件 | 退出条件 | 当前状态 |
| --- | --- | --- | --- | --- |
| G0 来源冻结 | 固定 Version4、CPG manifest、完整参考与 NSSLC 快照身份；比较关键方法、调用点和分支 | 开始任何实现之前 | 差异登记完成；选定每项旧行为的权威来源 | 部分完成；Player.cs/Main.cs 哈希与 wall/Luck 关键差异已复核；其他 P08 闭包仍未冻结 |
| G1 Owner/协议评审 | 解决 P03/P05/P06/P07/P09/P11/P14 与 World/Network/NPC 的读写者、协议和顺序 | G0 差异清单可用 | 每个受影响 invariant 有唯一 writer、消费者和失败/提交规则；交叉决定记录为 integration-review | 未完成；Version4 local zone producer、type 36 outbound caller、Network/NPC adapter binding、World/Tile owner 与 cache lifecycle 仍 unknown |
| G2 行为 oracle | 为当前 Version4 中有实现的旧路径建立观察向量和行为测试；对空方法单独处理 | G0、G1 对应路径已定 | 测试能命中旧入口及候选新 owner；明确状态 delta、顺序、副作用和协议输出 | S01 transition/timer、S02 coordinate/range projection 与 fake World/Tile port-order、S06/S07 局部 verifier 已运行；未执行旧入口或真实 adapter，也未验证新 owner 接线 |
| G3 API 与状态迁移 | 定稿概念 API、组件 writer、adapter、目标文件路径和调度接入点 | 对应切片的 G1/G2 通过 | 无重复 authority；legacy 到新组合映射可审查；失败/取消及恢复行为已定义 | S01 transition、S02 coordinate/range projection、last-range snapshot writer 与 Tile port 编排、Environment buff immunity timer、S06 luck、S07 wall rescan core API 已部分实现；完整 input composition、静态范围 writer、snapshot 调用相位、Teleport owner、World/Tile adapter、packet adapter 和调用入口未定义 |
| G4 分片实现与集成 | 逐能力接入；每片保持旧 facade 仅作兼容读取/输入的受控过渡 | 对应切片 G3 通过，且实现授权有效 | 真实调用到达新 owner，跨域读者只消费新 API；协议和顺序通过 targeted 验收 | S01/S02/S06/S07 与 timer mechanics 有 isolated core；没有完整注册、runtime hook 或 adapter；其余切片未实现 |
| G5 行为验收 | 运行真实迁移项目要求的行为测试和跨域回归，记录版本、命令、覆盖入口 | G4 全部预期切片已接线 | 观察向量通过且测试命中新 owner/API composition | 未运行；核心 verifier 不是旧入口或完整组合验收 |
| G6 兼容移除 | 删除旧字段/facade 或重复 Component | G5 通过且跨域 owner 明确批准 | 全部旧入口及持久化/网络/恢复消费者有替代证据；删除有回滚点 | 禁止提前执行 |

G1-G6 是未来执行门，不表示当前已完成或由本文件授权实施。与一个 unknown 无依赖的工作可独立推进，但依赖该 unknown 的 writer、API 和验收预期必须保持 blocked。

## 4. 分片计划

| Slice | 目标职责与现有路径 | 前置 owner / blocking gap | 实现范围 |
| --- | --- | --- | --- |
| P08-S01 Zone 与 shimmer | Environment/PlayerZoneAndEnvironmentStateComponent；`PlayerShimmerTransitionSystem` 本地 core、type 36 protocol adapter 与 NPC spawn effect owner | Version4 本地 zone producer/type 36 outbound caller 未找到；packet authority/身份映射、预测/去重、spawn authority、NPC/Network effect owner 未决 | 已实现本地 edge/latch core 并通过窄 verifier；保留 spawn-before-latch；packet 仍须保留 zone commit-before-spawn-before-forward；五个 zone bytes 与 `townNPCs` 不得丢失；G1 决议前不合并、去重或改网络门控 |
| P08-S02 Tile target/range | Interaction/PlayerTileTargetingAndRangeStateComponent；`PlayerTargetingSystem` coordinate/range-baseline/axe core；World/Tile access port | static target/range workspace 并发与 authority、P10 输入、真实 Tile facts adapter、Version4 display-jar helper 的完整行为、range/adjacency consumers 未决 | 已按 Version4 实现坐标、`5/3` 与 Journey `18/14` 范围 baseline、显式 axe/neighbor facts 和左/右/中心 Tile port 编排；未提交 legacy static target/range，也没有真实 World/Tile adapter 或 runtime 接线 |
| P08-S03 Movement/traversal | Movement/PlayerMovementPhysicsStateComponent 与 PlayerGravityAndWaterTraversalStateComponent；mobility snapshot | P03 Mount、P07 Movement/Collision 和现有 Physics/Movement owner 未决 | 已新增 isolated `PlayerTraversalPhysicsSystem` 与只读 `PlayerTraversalCapabilitySystem.CreateSnapshot`；前者显式解析 Version4 帧基线及 portal/wet/shimmer/vortex 参数并提交实例 physics state，后者组合 physics/gravity-water/mobility facts；窄范围 verifier 通过。未接 Player.Update/P07，静态 tuning 暂留 compatibility seam |
| P08-S04 Mobility/detection | Environment/PlayerEnvironmentMobilityStateComponent 与 PlayerEnvironmentDetectionAndSpawnStateComponent | P07、P09、P11、P14、P17 与 World/SceneMetrics inputs 未决 | 新增 isolated `PlayerSwimTimeSystem`：merman/flipper jump retention 与 `PlayerFrame` countdown/dry reset；局部 verifier 通过。`ShouldFloatInWater` 沿用既有 properties Query；其他 mobility、detection、spawn facts 仍按 consumer 分项审定，不以原 C05 合并边界决定 System owner |
| P08-S09 Environment buff immunity | Environment/PlayerZoneAndEnvironmentStateComponent 与 `PlayerEnvironmentBuffImmunitySystem` | P01 Teleport 调用方、Player tick 调度和 snow debuff consumer 的接线与先后顺序未审定 | timer tick 执行 `max(0, timer - 1)`；Teleport grant 为 4 ticks。仅验证局部状态转换，不声称 Teleport、tick 或 debuff 旧入口已迁移 |
| P08-S05 Armor/equipment effects | Combat/PlayerArmorAndCombatEffectsComponent 与 PlayerArmorSetAndTurretStateComponent | P06 combat、P09 equipment/Item payload、P14 spawn 输入以及 hook 调用闭包未决；sunlight exposure 只有完整参考候选实现，Version4 对应方法体被清空 | 先有唯一 rebuild writer，再让 P06 只读消费；把 sunlight producer 的 world/sky/tile/item/mount 输入列为独立补证切片，核实 Version4 行为后再接入；honeyCombItem 保持 Item 外部短期 payload；不为 set flags 机械拆系统 |
| P08-S06 Luck | Luck/PlayerLuckAndRescanStateComponent | P09 coin/equipment、NPC ladybug、所有 luckNeedsSync writers、network commit 未决 | 已实现显式输入的 RecalculateLuck Query、只写 Luck 的 System，以及接收显式 dayRate/current-value 的 ladybug timer 与 coin decay 更新；Version4 MessageBuffer type 134 是独立的入站重算/转发路径。完整输入组合、跨域 writers 与 replication adapter 仍待处理。不得导入完整参考版的 torch/broken-mirror 更新或本地发送逻辑 |
| P08-S07 Wall rescan | Luck/PlayerLuckAndRescanStateComponent 中的 rescan 子状态与独立 WallRescan 能力 | World/Tile scanner owner、NetModule subscriber、cache cleanup、调用频次未闭合 | 已实现 seed gate、force/cooldown/distance 与 changed-only broadcast 的 System 候选；沿用 Version4 不检查 netMode 的行为。核对全部 3 个源码调用点，不能只依赖 CPG 的 2 个结果；完整参考增加 client gate 与 server-only 广播 |
| P08-S08 Turret capacity | Combat/PlayerArmorSetAndTurretStateComponent、PlayerAbilityComponent、PlayerSummonCapacityState、Projectile | 两种 NLTX 状态重复；Version4 UpdateMaxTurrets body 空；完整参考源码身份及额外入口未闭合 | 先解决 owner、来源版本和 Projectile termination 契约；决议前禁止添加 trim/delete 行为或新增重复 capacity writer |

Slice 可按独立性排序，但不能把此表顺序当 scheduler 顺序。Reset/重建、luck、Mount、armor sets、network visibility 的源码次序必须在最终调度组合中单独验证。

## 5. 逐阶段执行约束

### A. 固定输入与旧调用闭包

1. 确认 Version4 相关文件 hash，并将 CPG manifest 固定到同一证据记录。
2. 对 Get-CpgMemberUses、Find-CpgCallSites、Get-CpgCallableFacts 结果保存 source/target、scope、Status 与 Gaps；索引的零命中或 partial 不作为无调用/无写入证明。`environmentBuffImmunityTimer` 的 CPG member-use 在 Player.cs/MessageBuffer.cs/NetMessage.cs scope 只返回一个 confirmed 写用，源码则确认 tick、雪地条件读取和 Teleport 赋值三处；`tileTargetX/Y` 的符号查询 complete，但 member-use 只返回一个 `NoAssignmentEvidence` partial，`tileRangeX/Y` 查询为 partial/零命中并有 gap，`adjTile` 两项访问也都是 partial；`Update_AdjustTileTargetForDisplayJars` callable facts partial 并带 `CalleeEffectsNotExpanded`。相关读写和顺序以 Version4 源码补齐。
3. 对 UpdateMaxTurrets、TrySpawningFaelings、DoUnbreakableWallScan、UpdateLuck、RecalculateLuck、`zone5` 和 luckNeedsSync 继续检视 Version4 declaration、implementation、caller 与 side effect call。Zone/shimmer 还需并读 Player、MessageBuffer type 36 与 NPC.Spawner.SpawnFaelings；完整参考项目中的 identity/netMode gates 与额外 callsites 必须单独标识，不能把 reference-only edge 添入 Version4 闭包。
4. 对 SS14 只借鉴 System 如何调用状态 owner 与 effect service，不复制具体事件模型或重力业务行为。

### B. 先冻结 API 与 observation vector

每个 Concept ID 记录：显式输入、默认值、状态 delta、不变量、事件/payload/次数/顺序、网络字节/版本、错误拒绝时机、生命周期、范围、重试/重复语义和外部效果。Sunlight exposure concept 还要覆盖天气/区域/天空强度/wet/位置/物品/mount/Tile inputs、exposure flag、scorch counter、阈值 side effects，以及 flag 对后续 Molten armor effect 的可见顺序。只对实现存在的 Version4 路径建立 target baseline；Version4 空方法不得用 complete reference 自动补成 target expected behavior。

签署所有跨域 owner 决定：P03 mount/gravity 优先级；P07 movement/collision 只读快照和 cache 生命周期；P09 equipment/Item；P06 combat；P11 presentation；P14 spawn；World/Tile、SceneMetrics、Network/NPC、audio 与 persistence。

### C. 按 owner 分片迁移

1. 先接入一条 P08 Component 到唯一 owner 的垂直路径，检查现有 System registration、调用点和新 API consumer；禁止只增加 System 文件却没有执行入口。
2. 保留来源可见顺序作为兼容调用约束；只有目标 scheduler/transaction 证据确定后才把它转成独立节点或 barrier。
3. 对 zone、luck、targeting、wall scan 分别设定 Query 与 effect boundary；不可让 Query 写 component、World tile、network 或 audio。
4. armor/mobility 派生事实使用单 owner 重建；消费者通过 Query/Snapshot 读取。跨系统双写同一 invariant 必须先退回 G1。
5. sunlight candidate 先完成 Version4/完整参考差异矩阵并确定唯一 flag/counter writer；保留 ResetEffects -> UpdateEquips -> UpdateSunScorch -> UpdateArmorSets 的可见次序。Tile/weather/item/mount adapter 与 buff/particle/audio/dismount effect owner 未定前不做 runtime hookup；当前 Rebuild reset 和未来 exposure producer 不得成为两个 writer。
6. turret slice 最后处理；除非同版本行为来源或获批的目标行为变更已被记录，否则保持 API 与 implementation blocked。

### D. 网络、存档与生命周期整合

1. 区分 inbound decode、owner validate/commit、outbound projection；固定 zone 五 byte 以及 vortex bit 的真实协议范围与错误路径。
2. 分别保留本地和网络输入触发的 shimmer transition 观察，只有 authority/预测/重连规则明确且行为测试命中两条路径后才能合并。
3. 先找到有效 Serialize/Deserialize 行为来源或批准新的存档协议；当前 Version4 对应方法体为空，不能报告 save/load 兼容。
4. 定义 reset、Ghost 路径、实体移除、world unload、异常和音频句柄清理；未知的 lifecycle 不用 Component 创建/销毁默认值代替。

## 6. 后续行为验收矩阵

| 能力 | 至少覆盖的输入/边界 | 观察结果 |
| --- | --- | --- |
| Zone/shimmer | 五个 zone bytes；本地与 MessageBuffer 入站；false-to-true、true-to-false、重复输入、重连 | bytes/payload、latch、spawn 请求数量与顺序、server/client authority |
| Tile target/range | 屏幕坐标、gravity flip、world edge clamp、neighbor correction、null Tile、多玩家并发输入 | target/range、Tile 读写、static 状态隔离与异常时机 |
| Equipment/armor | 默认 reset、装备/mount 重叠、armor set 变化、Ghost reset、重复重建 | 所有 P08 facts delta、更新顺序、无双写、Item payload 生命周期 |
| Vampire sunlight/scorch | seed、天气/区域/天空强度、wet、选中物品、mount、Tile 遮挡；counter 0/119/120；死亡与重复 tick | exposure flag 和 counter；与 ResetEffects/UpdateEquips/UpdateArmorSets 的次序；buff immunity、particle、debuff、dismount、wing/rocket 和 sizzle audio 的次数/时机。完整参考候选场景不代表 Version4 oracle 已闭合 |
| Traversal | default 与 mount/equipment/world gravity 冲突、水/岩浆/漂浮/noFall/slowFall | P07/Collision 消费到的 snapshot、position/velocity 未由 P08 重复提交 |
| Luck | torch、ladybug、coin、kite、potion、equipment、world modifiers、caps | luck 数值、各同步 writer 的边沿、replication payload 和发送次数 |
| Wall rescan | seed gate、force、cooldown 和距离临界、状态变化/不变化、多个 caller；对比完整参考的 client/server 分支 | Version4 观察向量、扫描次数、cache/result、变化广播条件与次数、teleport/death/unload 后状态 |
| Turret | 仅在 oracle 来源确定后定义 cases；若采纳完整参考算法，覆盖 owner、WipableTurret、持久 turret、timeLeft tie、容量变化、多入口 | Projectile kill 集合/顺序、每个网络 authority 的可见结果；未定 oracle 时保持 unknown |
| Network/save/audio | 版本、错误包、断线、缺失字段、重放、音频 stop/cleanup | 兼容字节、恢复状态、重复效果、资源释放；未闭合时不得升级状态 |

本矩阵仍是后续集成验收设计；既有记录包含窄范围 S05/S06/S07 核心 verifier，但不覆盖旧入口、sunlight producer 或运行时组合；执行结果见本文件第 8 节。

## 7. 停工条件与验收

- 一个 slice 的 owner 决议失败、出现第二个 writer 或与其他分区 API 冲突时，停止该 slice 的接线；保留已通过自身 Gate 的独立工作。
- 新 owner 未实际被调用或只由 legacy facade 转发时，不标记迁移完成。
- 网络投影、Projectile Kill、World/Tile mutation、Item transfer 或 audio effect 的顺序变化必须能由行为测试观察；缺少 oracle 时不推进旧入口删除。
- 未证明与完整参考快照的源码身份前，reference-only 行为不能替代 Version4 target baseline。
- Version4 空方法的必要语义来源不可追溯时，记录批准的行为差异或维持 unknown；不能把 UpdateMaxTurrets 方法名作为算法规格。
- 只有真实迁移项目的必需行为测试经过新 owner 与新 API composition 并通过后，才评估 verified/migration-success；编译、文档完成、静态查询或旧 facade 可用均不够。

## 8. 本轮执行记录

### 来源复核

- Version4 Player.cs SHA-256：E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86。
- 完整参考 Player.cs SHA-256：367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C。
- Version4 MessageBuffer.cs / NPC.cs SHA-256：0F474225FA9D98C539F61249619273A44E73F69A4CF388B44258433A2D15D5EE / 29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17。
- 完整参考 MessageBuffer.cs / NPC.cs SHA-256：48AABBBF4E0967964D97598E0168E9252ADD3DD868C66C6DA46AA87670EACAAB / ED8AA2302730A9E046EF310E543204FBA391BD35B1C9C0B213C8065DFBBE39F0。
- Version4 Main.cs SHA-256：66DBE1D1E6FB89A24A512309BB039DFEC7C06D4678D2A22D7ADECBAD3AFD6520；完整参考 Main.cs SHA-256：E24E61C9903BB7995F47E36C51EDBE43481B643226861B717020877E7E22B63F。完整参考项目标记 v1.4.5.6，目标框架为 net40/x86；相同版本字符串不是快照等价证明。
- CPG manifest：6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364。
- CpgEvidence 查询中，DoUnbreakableWallScan call-site 返回 2 个 confirmed，源码可见 3 处调用；UpdateMaxTurrets 与 TrySpawningFaelings 为 partial/零命中并带 NoMatchingFactInScannedScope。RecalculateLuck 有 2 个 confirmed callsite（Player.cs 与 MessageBuffer.cs）；UpdateLuck 查询为 partial/零命中，但 Version4 `Player.Update(int)` 源码在 Player.cs:15308 直接调用它。UpdateLuckFactors、UpdateLadyBugLuckTime、UpdateCoinLuck 各在 Player.cs 返回 1 个 confirmed caller。`luckNeedsSync` 在 Player.cs/NPC.cs 的所选索引范围返回 4 个 confirmed 写用点。`environmentBuffImmunityTimer` 符号查询 complete，所选 Player/MessageBuffer/NetMessage 用点查询只返回 1 个 confirmed 写用、0 gaps；源码实际还有 tick、snow debuff 条件读取和 Teleport grant 三处。零命中或不完整 member-use 结果不作为无调用/无写入证明，字段查询也不代表全部 writer 闭包。
- Version4 wall scan 不检查 netMode；完整参考新增 client 早退及 server-only 广播。目标 System 按 Version4 方法局部行为实现。
- Version4 `UpdateLadyBugLuckTime` 与 `UpdateCoinLuck` 的显式衰减规则和完整参考源码一致；`UpdateLuckFactors` 只调用这两项。Version4 `UpdateLuck` 再调用 `RecalculateLuck`。Version4 MessageBuffer type 134 收包后写入 luck 输入、重算并转发；该片段没有完整 netMode 限制。完整参考版 `UpdateLuck` 另外清除 `luckNeedsSync` 并对本地玩家发送 type 134，`UpdateLuckFactors` 增加本地 torch/broken-mirror 更新；MessageBuffer 只在 server 模式改写玩家身份与转发。这些 reference-only 行为未纳入 Version4 目标实现。

### S01 Zone/shimmer 静态关系复核

- Version4 `Player.TrySpawningFaelings`（`Terraria/Player.cs:9937-9945`）在 shimmer false-to-true 时调用 `NPC.Spawner.SpawnFaelings(this)`，然后才写 `_wasInShimmerZone`；`Player.Update(int)` 在 `Player.cs:15099` 直接调用它。Version4 此处没有 netMode gate。
- Version4 `MessageBuffer.GetData` 的 type 36（`Terraria/MessageBuffer.cs:1717-1733`）读取 byte player id 后无条件改成 `whoAmI`，保存旧 `zone5[0]`，写入五个 zone bytes 与 `townNPCs`，在 false-to-true 时 spawn，随后无条件转发 type 36。
- Version4 `NPC.Spawner.SpawnFaelings`（`Terraria/NPC.cs:5853-5888`）先拒绝已有 type 677；选择 2 到 5 个 spawn 尝试，在 spawn area 随机搜寻 shimmer 液体，避开 safe/screen 区域并创建 type 677；有效 NPC index 后发送 type 23。
- 完整参考 `Player.cs:17072-17080` 增加 `Main.netMode != 1` spawn gate，但仍在调用之后更新 latch；`MessageBuffer.cs:2204-2225` 只在 `Main.netMode == 2` 重映射 player id、触发 spawn 和转发 type 36；`NPC.cs:5987-6021` 只在 server mode 发送 type 23。参考行为不覆盖 Version4 target contract。
- 完整参考 `Player.cs:16981-17003` 的 `UpdateBiomes` 从 SceneMetrics 更新 zone facts，`Player.Update(int)` 调用 `UpdateBiomes`；`Main.cs:17875-17901` 比较五个 zone bytes 与 `townNPCs` 后发送 type 36，`Main.cs:65353-65373` 有周期 type 36 发送。Version4 检查范围内没有 `Player.UpdateBiomes` symbol/调用，也没有 Main 的 zone state/type 36 发送点。两版 `NetMessage` case 36 均写 player id、五个 zone bytes 与 `townNPCs`（Version4 `NetMessage.cs:928-936`；参考版 `NetMessage.cs:937-945`）；serializer 存在不证明 target 的 zone producer 或 sender caller 存在。
- CpgEvidence API 初始化 Version4 只读 manifest `6d6bdf09...a715a364` 后查询：`zone5` 符号状态 complete；Player.cs、MessageBuffer.cs、NetMessage.cs、Main.cs scope 返回 6 个 member-use，其中 MessageBuffer whole-field assignment 为 confirmed，NetMessage 与其他若干 access mode 为 partial/Unknown，Main 无命中；`SpawnFaelings` 在 Player.cs 与 MessageBuffer.cs 的 2 个 callsite 为 confirmed；`TrySpawningFaelings` caller 与 `UpdateBiomes` symbol 查询为 partial/零命中。源码复查证明 `Player.Update(int)` 实际调用 `TrySpawningFaelings`；零命中不作无调用结论。CPG Snapshot 的 `SourceSnapshotId` 为 null，结构结果以所选 scope 为限，并由当前源码逐处复核。
- 已新增 `PlayerShimmerTransitionInput`、`IPlayerFaelingSpawnPort` 与 `PlayerShimmerTransitionSystem.UpdateLocalTransition`。System 以显式 `ZoneShimmer` 输入检测 false-to-true，只在边沿调用 NPC effect port，effect 正常返回后写入 `WasInShimmerZone`；异常原样传播且 latch 保持旧值。该 API 只实现 Version4 本地 `TrySpawningFaelings` 的可证实顺序，没有 zone producer、NPC adapter 或调度注册。
- 新增 `Test/Terraria.Player.ShimmerTransition.Verification`，验证 enter/repeat/leave/re-enter、effect 观察到提交前 latch、effect exception 不提交 latch，以及 immunity timer grant/decrement/clamp。结果：`PASS: player shimmer edge, effect order, exception timing and immunity timer`。测试不覆盖 MessageBuffer type 36、网络 authority、Teleport 调用接线或完整旧入口行为。
- Version4 zone producer/type 36 outbound caller、packet ingress authority、合法 player id 范围、shimmer 预测/重放/重连、NPC spawn owner 注册和异常后的 relay/retry 均为 unknown；S01 仍停在 G1，不代表迁移完成。

### S09 Environment buff immunity 静态关系与 core

- Version4 `Player.Update(int)` 在 `Player.cs:14974` 以 `Math.Max(0, timer - 1)` 递减，在 `Player.cs:17397` 仅于 expert、ZoneSnow、wet 且非 lava/honey、无 arctic diving gear、timer 为 0 时添加 buff 46 持续 150；`Player.Teleport` 在进入 `try` 后、其他 Teleport effect 前于 `Player.cs:21738` 将 timer 设为 4。
- 完整参考项目 `D:\TRbackup\无任何删减通过编译` 对应位置为 `Player.cs:24968,28369,37909`，此三项局部 timer/debuff 规则相同。参考项目的相同行为只用于差异校验，P08 目标预期仍以 Version4 为准。
- `PlayerZoneAndEnvironmentStateComponent` 保存 timer；`PlayerEnvironmentBuffImmunitySystem.Tick` 实现 clamp decrement，`BeginTeleportImmunity` 写入 4。当前实现未改变旧 `Player.Teleport` 或 `Player.Update`，也未接入雪地 debuff consumer、P01 Teleport owner 或 scheduler。
- 局部 verifier 的 timer 断言已纳入并通过；它验证直接 System API，不验证真实 tick phase、Teleport 异常路径或 snow debuff 完整条件，故该 slice 仍为 partial，接线状态 unknown。

### S02 Tile target coordinate projection

- Version4 `Player.Update(int)` 通过 mouseX/Y、screenPosition、gravity direction 和 world tile dimensions 在 `Player.cs:15109-15129` 计算坐标并按上界后下界 clamp。其后有 null Tile 初始化，再按 held item 与 type 323/frameY 邻格 facts 修正 axe target（`Player.cs:15131-15156`）；Version4 与完整参考的这段分支规则相同。随后调用 `Update_AdjustTileTargetForDisplayJars`（`Player.cs:15157`），但 Version4 该 helper 只保留资格 guard/early return，完整行为 unknown。
- 完整参考项目同一主路径在 `Player.cs:25782-25802` 保留该公式；另有 `Main.mouseItem` 非空且非暂停时的 target writer（`Player.cs:5067-5074`），以及本地输入/交互路径在 `Main.cs:17656-17660` 写同一静态 target 后调用 `LookForTileInteractions`、`ChestChangeEvents`、`UpdateNearbyCraftingTiles`。Version4 `Main.cs` 无对应写入/调用；Version4 `Player.Update(int)` 有 `LookForTileInteractions` 调用（`Player.cs:17006`）。完整参考主路径还调用 `UpdateNearbyInteractableProjectilesList`，并完整实现 display-jar 半径 1 搜索及 target 修正（`Player.cs:28683-28704`）。这些额外路径不作为 Version4 目标规则。
- Version4 在 `Player.cs:19163-19178` 仅于本地玩家且非 display-doll/inanimate 时把共享 `tileRangeX/Y` reset 为 `5/3`；Journey mode 且 FarPlacementRangePower 已解锁并启用时应用 `range * 2 + 8`，结果为 `18/14`。Version4 的 `TileReachCheckSettings.cs:32-33` 与 `Projectile.cs:45235-45238` 读取共享范围，`lastTileRangeX/Y` 在 `Player.cs:15636-15637` 从共享值复制。
- 完整参考项目保持同一 baseline/Journey 规则，但额外在 `Player.cs:12992-12995` 对 `equippedAnyTileRangeAcc` 增加 `3/2`，在 `Player.cs:14805-14808` 对本地当前物品 type 1923 增加 `1/1`。Version4 全树没有这两处额外写入；这些完整参考规则不进入目标实现。Version4 全树对 `adjTile` 只找到字段声明和 ResetEffects 清空；完整参考版的 `SetAdjTile`/`AdjTiles` 及 Recipe/UI consumers 是版本差异，保持 unknown。
- CpgEvidence 只读 Query API 于 2026-10-01 对 Version4 查询：字段符号 `tileRangeX/Y`、`lastTileRangeX/Y`、`adjTile` 均为 complete。对 Player/Main/Item/MessageBuffer 四个选定 shard 的 range member-use 查询为 partial、0 facts、`NoMatchingFactInScannedScope`、0 shards scanned；`adjTile` 查询状态 complete，但 2 个 facts 均为 `EvidenceStatus=partial`、`AccessMode=Unknown`。数据库 manifest 为 `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`。CPG 未闭合字段关系，直接源码搜索确认了 Version4 的 range consumers；查询空结果不能作为无 writer/consumer 证据。
- `PlayerTargetingSystem.ResolveTarget` 现经 `IPlayerTileTargetWorldPort` 按左、右、中心顺序调用 `EnsureTileExists`；仅 axe 分支满足条件时依次读取 center、left、必要时 right facts，并复用纯 axe decision。新增 `PlayerTileTargetRangeInput/PlayerTileTargetRange` 与 `ResolveRangeBaseline`，只计算 Version4 的 baseline/Journey 范围；`CaptureEffectiveRange` 把显式 effective range 写入 `PlayerTileTargetingAndRangeStateComponent.LastTileRangeX/Y`，对应源码可见的 `lastTileRangeX/Y = tileRangeX/Y` 复制。真实 Tile adapter、静态范围 authority、snapshot 调用时点、range consumers、display-jar 语义及实际调用点仍未接入。
- S03 新增 `PlayerTraversalPhysicsInput`、`PlayerTraversalPhysicsResult` 与 `PlayerTraversalPhysicsSystem`。`Resolve` 复现 Version4 参数 reset 后的 portal、wet/down-dash、shimmer/water、vortex 覆写次序与 maxFallSpeed 每帧 `+0.01`；`CommitMovementState` 只提交五个 per-player physics 参数，jump height/speed 作为返回 snapshot 留给 P07 Mobility。Version4 与完整参考对应分支一致；`defaultGravity` 输入生产者、static jump tuning 的共享/并行语义、Player.Update 接线及 P03/P07/Collision 优先级仍 unknown。新增 verifier 只抽样基线、wet/trident、portal/down-dash/vortex 优先级和 state commit，不覆盖完整 Player.Update。
- S03 另新增只读 `PlayerTraversalCapabilitySystem.CreateSnapshot`，从 physics、gravity/water traversal 与 environment mobility 三个现有 Component 组装移动/碰撞所需 facts；不写 Component，也不把 `skyStoneEffects`、`spawnMax`、`blockRange` 混入 traversal API。snapshot 当前只由 targeted verifier 消费，没有 P07/Mount runtime caller。
- `PlayerTraversalCapabilitySnapshot` 不包含 `HasFloatingTube`：Version4 源码全文只见 ResetEffects/可见配件设置和清空，没有读取；CPG member-use 为 3 个 confirmed Write、0 个 Read，剩余读取关系范围仍按源码快照限定。当前已有 `PlayerInteractionAndSelectionPropertiesQuery` 通过 `CanFloatInWater`、`ControlDown`、mount active/type 计算 `ShouldFloatInWater`，保持复用而不再造一个同义查询。
- CpgEvidence 只读查询使用 Version4 manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`：8 个参数字段 symbol 均命中；`jumpHeight` 有 21 个用点（3 Write、4 ReadWrite、14 Unknown/partial），`jumpSpeed` 有 44 个用点（2 Write、4 ReadWrite、38 Unknown/partial），`runAcceleration` 有 8 个用点（4 ReadWrite、4 Unknown/partial），`runSlowdown` 有 29 个用点（13 ReadWrite、16 Unknown/partial）。`defaultGravity` 与 `maxFallSpeed` 在所选 Player shard 的 member-use 查询均为 partial/零结果，gap 为 `NoMatchingFactInScannedScope`，`ScannedShardCount=0`；不能将其解释为无用点。索引 `SourceSnapshotId=null`，所有结论由当前 Version4 与完整参考的 Player.cs 源码复核。
- 2026-10-01 使用 CpgEvidence 查询 traversal/能力输入字段：`gravity` member-use 为 complete、18 facts，其中 2 个 confirmed Write、16 个 Unknown access；`waterWalk` 为 9 facts（4 Write、5 Unknown），`waterWalk2` 为 7 facts（4 Write、3 Unknown）；`hasFloatingTube` 为 3 个 confirmed Write、0 个 Read。`maxFallSpeed` 为 partial/零命中并带 `NoMatchingFactInScannedScope`。索引完整只表示查询预算内完成，不能由这些结果推断唯一 writer；`SourceSnapshotId=null`，以当前 Version4 源码确认实际 reset 与 effect 重建顺序。
- 同次查询的 `UpdateEquips`、`UpdateArmorSets` 与 `UpdateMaxTurrets` callable facts 均为 partial，带 `CalleeEffectsNotExpanded`；各返回 5、5、2 个 CFG nodes，均无可用 direct call target。API 不足处回到源码：Version4 `Player.Update(int)` 可见 `UpdateEquips(i)` -> `DoUnbreakableWallScan(force: true)` -> `UpdateLuck()` -> `UpdateArmorSets(i)`，随后仅在 `maxTurretsOld != maxTurrets` 时调用 `UpdateMaxTurrets()` 并更新旧值（`Player.cs:15296-15356`）。完整参考的额外 `UpdateMaxTurrets` callsites 与 projectile trim 算法仍不是 Version4 目标证据。
- `Test/Terraria.Player.Targeting.Verification` 覆盖 normal/inverted gravity、world-edge clamp、窄 world clamp 次序、左右 axe correction、frameY boundary、左分支优先级、Tile port ensure/read 顺序，以及 local default/Journey/locked-or-disabled-power range、non-local/display-doll shared-range 保留和 per-player range snapshot commit。该 verifier 不执行 Version4 旧入口或真实 Tile adapter。

### S04 SwimTime isolated core

- Version4 `Player.cs:12186-12195` 在持续跳跃且 `merman && (!mount.Active || !mount.Cart)` 时，仅当 `swimTime <= 10` 将其设为 30；其他挂载的跳跃计数更新留在原运动逻辑。`Player.cs:12209-12216` 在新跳资格已满足后，仅在 `wet && accFlipper` 且 `swimTime == 0` 时设为 30。两种入口共享一个 Mobility 状态字段，但输入/跳跃状态的判定仍留给上游 movement adapter。
- Version4 `Player.PlayerFrame()` 在 `Player.cs:20053-20064` 对正值先减 1，之后若 `!wet` 归零；同一方法还用 `swimTime` 选择 leg/body frame。`Player.Update(int)` 的 early `flag` 路径在 `UpdateBuffs(i)` 后调用它（`Player.cs:14951-14952`），随后在 `Player.cs:14975-14977` return；正常路径则在 `ItemCheckWrapped(i)` 后调用（`Player.cs:17661-17662`）。两个源码 callsite 不会在同一 Update 执行中连续触发，但时点不同；外部调用频次仍未闭合。
- 完整参考项目对 merman `<= 10`、flipper `== 0` 和 PlayerFrame 递减/干燥归零的局部逻辑一致（参考 `Player.cs:20971-21002,36203-36214`）。参考项目额外存在 Player.cs/Main.cs 的 PlayerFrame caller；这不形成 Version4 caller edge。
- CpgEvidence 对 `swimTime` 的 Version4 member-use 返回 11 facts：3 个 confirmed assignment-left Write，8 个 `AccessMode=Unknown`/partial。`PlayerFrame` callable facts partial 并带 `CalleeEffectsNotExpanded`；call-site 查询 partial/0 facts，gap 为 `NoMatchingFactInScannedScope`。manifest 为 `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`。以上缺口由 Version4 全方法源码搜索补到两个 Player.Update 调用点，未推断未知入口不存在。
- 新增 `PlayerMermanJumpInput`、`PlayerFlipperJumpInput` 与 `PlayerSwimTimeSystem`，显式接收 active jump counter、airborne、merman、mount/cart、wet/flipper 和 new-jump-eligibility facts；只提交 swim-time 阈值刷新与 frame tick 变化，不改变 jump counter 或动画状态。`CanStartNewJump` 仍须由上游复用 Version4 的完整旧 jump gate 计算。未接旧 Player.Update、PlayerFrame、P07、输入或运行时调度。
- 新增 `Test/Terraria.Player.SwimTime.Verification` 抽样覆盖 inactive-jump rejection、merman 零值启动、10/11 阈值、cart mount 抑制、flipper 零值启动/非零保留、wet countdown 和 dry reset；不覆盖复杂 jump eligibility、完整帧调用次数、玩家动画输出或旧入口行为。

### S05 Armor/equipment relationship audit

- 使用 `CpgEvidence.ps1` 只读 Query API，初始化 Version4 SQLite CPG 并启动 reader；manifest 为 `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`。`Find-CpgSymbols -Kind SymbolField` 命中 11 个字段；`Get-CpgMemberUses` 使用 health API 返回的全部 967 个索引 source paths：`thorns` 6 项、`turtleArmor` 2 项、`turtleThorns` 2 项、`cactusThorns` 2 项、`spiderArmor` 2 项、`anglerSetSpawnReduction` 3 项、`vampireBurningInSunlight` 4 项、`honeyCombItem` 7 项、`maxTurrets` 20 项、`maxTurretsOld` 0 项（`partial/NoMatchingFactInScannedScope`）、`vortexStealthActive` 5 项。结果含 `AccessMode=Unknown`/`EvidenceStatus=partial`；索引没有绑定当前源码快照，因此所有关系均回源码核对。
- `UpdateArmorSets` 和 `UpdateMaxTurrets` 全索引 call-site 查询均为 partial/0 facts，gap 为 `NoMatchingFactInScannedScope`；Version4 源码确认 `Player.Update(int)` 调用 `UpdateArmorSets(i)`（`Player.cs:15348`）并在容量变化时调用 `UpdateMaxTurrets()`（`15353-15356`）。`ResetEffects` CPG 只给一条边，但源码至少包含 Ghost 和正常更新调用（`Player.cs:3855,15207`）。不将零命中解释为不存在调用。
- Version4 的活动帧顺序为 ResetEffects、UpdateBuffs、UpdateEquips、UpdateArmorSets。ResetEffects 清空 `thorns` 与 armor flags；UpdateBuffs 写入 thorns buff 值；UpdateEquips 从当前 armor visual slots 写 turtle/spider flags；`ArmorSetBonuses.GetCompleteSet(QueryContext).Effect(this)` 写 Angler/Cactus/Turtle facts，再执行其他 armor helpers。NPC 的 spawn-rate 代码读取 Angler flag（`NPC.cs:623`）。完整 source scan 没有定位到 Version4 对 `turtleThorns`、`cactusThorns`、`turtleArmor`、`spiderArmor` 的直接 consumer；`thorns` 当前可见的是 buff 阈值处理与 effect writer，没有找到 target 反伤 consumer。
- Version4 `Player.Update(int)` 调用 `UpdateSunScorch()`（`Player.cs:15297`），但目标方法在本地玩家 guard 后结束（`Player.cs:17763-17773`），`VampireSeedSunlightExposure()` 为空（`Player.cs:17774`）。独立 `UpdateSunScorchValues()` 仍处理死亡清除、counter 与 sizzle audio（`Player.cs:17713-17757`），可见调用点位于死亡更新路径（`Player.cs:10073`）；Molten set effect 读取 `vampireBurningInSunlight` 控制 `buffImmune[24]`。因此目标 sunlight producer/正常更新链仍为 unknown，不能把 stub 解释为无 producer。
- 完整参考 `D:\TRbackup\无任何删减通过编译` 与 Version4 都标记 `1.4.5.6`，但源码哈希不同。参考版的 `ResetEffects` 同样清除此 flag（`Player.cs:19083`）；其 `UpdateSunScorch` 调用 exposure producer 和 value updater。producer 检查 vampire seed、天气/时间/区域/天空强度、wet、选中物品、mount 与最多 15 格 tile 遮挡后写 true（`Player.cs:28830-28919`）；counter 达到阈值后还会清 buff immunity、请求粒子、添加 debuff、下 mount 并清 wings/rocket boots（`Player.cs:28841-28877`）。参考版 Hurt 读取 thorn flags（`Player.cs:31681-31725`），并消费 `honeyCombItem`（`Player.cs:38832-38860`）；这些均为候选补证，不是 Version4 已确认行为或本 slice 已实现内容。
- 当前 NSSLC 的 `PlayerArmorAndCombatEffectsSystem.Rebuild` 从显式输入重建六项 armor/combat facts，不再写 `VampireBurningInSunlight`。`PlayerSunScorchSystem.UpdateLocal` 读取显式天气、区域、天空强度、wet、选中物品、mount 与 Tile facts；它复用 `PlayerEnvironmentalPressureComponent.SunScorchCounter` 和 `PlayerArmorAndCombatEffectsComponent.VampireBurningInSunlight`，返回 sizzle 音量与阈值效果请求。该隔离核心按完整参考项目候选逻辑实现，不证明 Version4 目标行为。未接旧 Player/Item 状态、`Player.Update`、P09 装备输入、P06 consumer、Tile adapter、效果 owner 或 scheduler；`honeyCombItem` 仍是外部 Item payload。当前没有 sunlight runtime caller，P07 `PlayerArmorSetSystem` 仍是 mobility core，`NpcSpawnRateSystem` 仍只收到 Angler 显式输入。S05 与跨分区接线保持 `partial` / `integration-review`。
- S05 isolated-core verifier 已通过：SDK 10.0.400，经 `Build/Tools/Invoke-SerialDotnet.ps1` 串行构建，build exit code 0、0 warnings、0 errors；随后运行 exit code 0，输出 `PASS: player armor and combat effect rebuild`，产物为 `Build/bin/Terraria.Player.ArmorEffects.Verification/Debug/net10.0/Terraria.Player.ArmorEffects.Verification.dll`。此结果只覆盖 isolated `Rebuild`；旧入口行为测试、sunlight producer、consumer、运行时接线与集成验证仍 `not-run`。该 slice 仍为 proposed/partial，不表示迁移成功。
- 新增 S05 sunlight/scorch isolated core：`PlayerSunScorchInput` 显式传入 world/weather/sky/zone/wet/item/mount 与脚下向上的 Tile facts；`PlayerSunScorchSystem.UpdateLocal` 限制扫描 15 格，写既有 counter 与 flag，并以结果返回阈值请求。`UpdateDead` 单独复现死亡路径的 flag 清除和 counter 减 2。效果请求只表达 frame refresh/achievement、buff immunity、粒子、三项 debuff、dismount、wings/rocket 清除；音频仅返回 volume scalar，不创建或更新 audio handle。当前 NSSLC 内 `PlayerArmorAndCombatEffectsSystem.Rebuild` 不再清除此 flag，避免与 sunlight core 双写。
- 该实现复用 `PlayerEnvironmentalPressureComponent` 与 `PlayerArmorAndCombatEffectsComponent`，没有新增重复状态 Component。Tile/sky/weather adapters、外部效果执行与旧入口顺序都没有实现；参考项目与 Version4 的源码哈希不同，Version4 sunlight producer 仍为 unknown。此 core 与其 verifier 均为 partial evidence，不代表迁移成功。

### 已实现切片

- S02 目前只有 tile target 坐标、range-baseline projection、last-range snapshot writer、axe-neighbor decision 与抽象 Tile port 编排；Tile/World adapter、Version4 display-jar helper 的完整行为、旧 static range commit 和 range/adjacency consumer composition 未实现。
- S06 新增 PlayerLuckCalculationInput、PlayerLuckCalculationQuery、PlayerLuckFactorUpdateInput/Result 与 PlayerLuckSystem。`UpdateFactors` 按 Version4 规则更新 ladybug timer/coin luck，`Recalculate` 只提交 Luck，不修改 LuckNeedsSync；外部 writer、sync 消费者与所有输入的生产者仍未闭合。
- S07 新增 PlayerWallRescanInput、PlayerWallRescanResult、IPlayerWallRescanPort 与 PlayerWallRescanSystem，提交既有 rescan cache 和 inside 状态，并在变化后调用广播端口。
- S04 新增 PlayerMermanJumpInput、PlayerFlipperJumpInput 与 PlayerSwimTimeSystem，隔离 swimTime 的 jump refresh 和 frame expiry；不接旧调用入口。
- 新增 Test/Terraria.Player.Luck.Verification 与 Test/Terraria.Player.WallRescan.Verification，覆盖公式/coin threshold、seed、force、cooldown、distance boundary 和状态变化效果。
- 新增 `Test/Terraria.Player.SwimTime.Verification`，覆盖 S04 swim-time refresh threshold、cart gate 与 wet/dry frame transition。
- 新增 `Test/Terraria.Player.TraversalPhysics.Verification`，抽样 S03 基线、wet/trident 分支、portal 与 down-dash/vortex 覆写顺序、movement-state commit 和 capability snapshot 字段组合；不是完整 traversal 行为覆盖。
- 新增 `Test/Terraria.Player.SunScorch.Verification`，只抽样 eligible exposure、solid tile blocking、counter 119→120 阈值请求与重复 tick、死亡衰减，以及 armor rebuild 不清 sunlight flag；不是 Version4 行为 oracle。
- NSSLC 当前没有 P08 调用入口、调度注册或 wall scan adapter；这些代码是部分实现，不表示已接入运行路径。
- S01 的 packet/producer 接线仍未完成；S02 只有坐标/range-baseline projection core 与抽象 Tile port 编排；S03 有通过窄范围 verifier 的 isolated physics core 和未接入运行路径的只读 capability snapshot，未接 Player.Update/P07；S04 仅 swim-time mechanics core，其余 mobility/detection 未接入；S05 有显式输入重建六项 armor/combat facts 的 core，以及基于完整参考候选规则的 sunlight/scorch isolated core；后者已通过有限 verifier，但 Version4 oracle、旧入口、adapters、consumer 和调度接线仍 unknown/not-run，整体仍属 partial。S08 和完整 S06/S07/S09 集成仍未完成。S09 只有 timer mechanics core，没有 Teleport、tick scheduler 或 debuff consumer 接线。Turret capacity 保持 unknown，未实现 Projectile 删除。

### 构建记录

- S03 targeted core verifier：使用 SDK 10.0.400 经仓库 serial wrapper 构建与运行。Build exit code 0、0 warnings、0 errors；run exit code 0，输出 `PASS: player traversal physics, state commit and capability snapshot`。产物位于 `Build/bin/Terraria.Player.TraversalPhysics.Verification/Debug/net10.0/Terraria.Player.TraversalPhysics.Verification.dll`。验证只覆盖独立 System core、状态提交与只读 snapshot，不命中旧 Player.Update 或新运行时调用入口；完整行为与集成验证仍为 not-run。

- S04 targeted core verifier：使用 SDK 10.0.400 经仓库 serial wrapper 构建与运行。Build exit code 0、0 warnings、0 errors；run exit code 0，输出 `PASS: player swim-time refresh thresholds and frame expiry`。产物位于 `Build/bin/Terraria.Player.SwimTime.Verification/Debug/net10.0/Terraria.Player.SwimTime.Verification.dll`。验证只覆盖隔离状态转换，不执行旧 Player.Update/PlayerFrame，也不证明 jump gate 或 scheduler 行为等价。

~~~powershell
$taskSdkRoot = Join-Path $env:TEMP 'codex-dotnet-sdk-10.0.400'
$env:PATH = "$taskSdkRoot;$env:PATH"
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
    'build',
    '.\Test\Terraria.Player.SwimTime.Verification\Terraria.Player.SwimTime.Verification.csproj',
    '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
    '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
    'run', '--project', '.\Test\Terraria.Player.SwimTime.Verification\Terraria.Player.SwimTime.Verification.csproj',
    '--no-build', '--no-restore', '-m:1', '-nr:false',
    '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false',
    '-p:BuildInParallel=false')
~~~

~~~powershell
$taskSdkRoot = Join-Path $env:TEMP 'codex-dotnet-sdk-10.0.400'
$env:PATH = "$taskSdkRoot;$env:PATH"
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
    'build',
    '.\Test\Terraria.Player.TraversalPhysics.Verification\Terraria.Player.TraversalPhysics.Verification.csproj',
    '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
    '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
    'run', '--project', '.\Test\Terraria.Player.TraversalPhysics.Verification\Terraria.Player.TraversalPhysics.Verification.csproj',
    '--no-build', '--no-restore', '-m:1', '-nr:false',
    '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false',
    '-p:BuildInParallel=false')
~~~

- Shimmer transition verifier 使用 SDK 10.0.400，通过仓库串行入口构建；本次 build exit code 0、0 warnings、0 errors，随后 `--no-build --no-restore` run exit code 0。输出为 `PASS: player shimmer edge, effect order, exception timing and immunity timer`；断言覆盖 shimmer edge/effect/exception 与 timer grant/decrement/clamp。产物位于 `Build/bin/Terraria.Player.ShimmerTransition.Verification/Debug/net10.0`。命令如下：

~~~powershell
$taskSdkRoot = Join-Path $env:TEMP 'codex-dotnet-sdk-10.0.400'
$env:PATH = "$taskSdkRoot;$env:PATH"
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', '.\Test\Terraria.Player.ShimmerTransition.Verification\Terraria.Player.ShimmerTransition.Verification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('run', '--project', '.\Test\Terraria.Player.ShimmerTransition.Verification\Terraria.Player.ShimmerTransition.Verification.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
~~~

- Targeting verifier 使用同一 SDK 与仓库串行入口 build；exit code 0、0 warnings、0 errors；随后 `--no-build --no-restore` run exit code 0。输出为 `PASS: player tile target, range baseline, axe correction and legacy clamp order`。产物位于 `Build/bin/Terraria.Player.Targeting.Verification/Debug/net10.0`。

~~~powershell
$taskSdkRoot = Join-Path $env:TEMP 'codex-dotnet-sdk-10.0.400'
$env:PATH = "$taskSdkRoot;$env:PATH"
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', '.\Test\Terraria.Player.Targeting.Verification\Terraria.Player.Targeting.Verification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('run', '--project', '.\Test\Terraria.Player.Targeting.Verification\Terraria.Player.Targeting.Verification.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
~~~

- SDK 10.0.400 按 global.json 要求安装在任务临时目录，仓库 SDK 配置未更改。
- Luck 与 WallRescan 的两个 build 命令以及本次 Shimmer transition build 均通过 Build/Tools/Invoke-SerialDotnet.ps1 串行执行；各自 exit code 0，0 warnings，0 errors。
- 输出位于 Build/bin/Terraria.Player.Luck.Verification/Debug/net10.0、Build/bin/Terraria.Player.WallRescan.Verification/Debug/net10.0 与 Build/bin/Terraria.Player.ShimmerTransition.Verification/Debug/net10.0。
- 以下 build 与 verifier 命令均从仓库根目录运行；taskSdkRoot 是本次临时安装的 SDK 10.0.400 目录。

~~~powershell
$taskSdkRoot = Join-Path $env:TEMP 'codex-dotnet-sdk-10.0.400'
$env:PATH = "$taskSdkRoot;$env:PATH"
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', '.\Test\Terraria.Player.Luck.Verification\Terraria.Player.Luck.Verification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', '.\Test\Terraria.Player.WallRescan.Verification\Terraria.Player.WallRescan.Verification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('run', '--project', '.\Test\Terraria.Player.Luck.Verification\Terraria.Player.Luck.Verification.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('run', '--project', '.\Test\Terraria.Player.WallRescan.Verification\Terraria.Player.WallRescan.Verification.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
~~~

- Luck verifier exit code 0; output: PASS: player luck thresholds, contributions and single-owner commit. 本次运行覆盖新加的因子衰减提交、timer 正负向归零和 coin luck 衰减阈值。
- Wall verifier exit code 0; output: PASS: player wall rescan seed, force, cooldown, distance and change effects.
- Shimmer transition verifier exit code 0; output: PASS: player shimmer edge, effect order, exception timing and immunity timer.
- 未执行完整迁移行为 oracle；三个 verifier 仅覆盖局部算法和提交契约。没有验证旧入口、新 owner、Network adapter 或 scheduler 的完整行为组合。

当前为 partial-execution；未完成新 owner 接线或行为等价验收，不表示迁移成功。

### S05 sunlight/scorch core 验证记录

按 `global.json` 使用 SDK 10.0.400；构建与运行都通过仓库串行入口，未启动其他测试或解决方案构建。首轮构建发现新输入/结果类型声明错误，修正后目标 verifier 构建通过，0 warnings、0 errors；run exit code 0，输出 `PASS: player sunlight exposure, scorch threshold and owner isolation`。产物为 `Build/bin/Terraria.Player.SunScorch.Verification/Debug/net10.0/Terraria.Player.SunScorch.Verification.dll`。

```powershell
$taskSdkRoot = Join-Path $env:TEMP 'codex-dotnet-sdk-10.0.400'
$env:PATH = "$taskSdkRoot;$env:PATH"
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
    'build',
    '.\Test\Terraria.Player.SunScorch.Verification\Terraria.Player.SunScorch.Verification.csproj',
    '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
    '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
    'run',
    '--project', '.\Test\Terraria.Player.SunScorch.Verification\Terraria.Player.SunScorch.Verification.csproj',
    '--no-build', '--no-restore', '-m:1', '-nr:false',
    '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false',
    '-p:BuildInParallel=false')
```

该验证只覆盖局部核心状态转换和请求输出；旧入口调用、天气/Tile adapter、真实副作用、Molten consumer 顺序、`UpdateDead` 的全玩家 runtime hookup、网络/存档与完整行为等价仍 `not-run`。完成状态保持 `partial-execution`，不称迁移成功。

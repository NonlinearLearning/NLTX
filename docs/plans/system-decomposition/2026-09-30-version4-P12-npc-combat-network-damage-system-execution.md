# Version4 P12 NPC 战斗、网络与伤害追踪 System 执行文档

~~~yaml
partitionId: P12
taskId: AUTH-SYS-P12
originalSessionId: 64850edbdb464104ba04d413bfc362e1
designDocument: D:\TRbackup\NLTX\docs\plans\system-decomposition\2026-09-30-version4-P12-npc-combat-network-damage-system-design.md
sourceReport: D:\TRbackup\NLTX\docs\system-decomposition\reports\2026-09-18-system-decomposition-authoritative-P12-npc-combat-network-damage.md
authoritativeSource: D:\TRbackup\Version4
completeReferenceSource: D:\TRbackup\无任何删减通过编译
targetArea: D:\TRbackup\NLTX\src\NSSLC
executionStatus: partial
verificationStatus: not-run
sourceModified: true
testsRun: true
verificationScope: 10-percent-core-smoke-after-explicit-death-phase-context-composition-lethal-dot-forwarding-phase-health-owner-restore-segment-mirror-authority-ai-policy-sync-consumer-policy-preservation-and-npc-status-slot-phase-expiration-order-and-npc-dot-regen-poison-heal-celled-dryad-threshold-full-projectile-snapshot-arithmetic-atomicity
fullP12Verification: not-run
documentationPassSourceModified: true
documentationPassTestsRun: false
migrationStatus: not-claimed
~~~

## 1. 执行边界

本文记录后续实现的执行顺序和实际代码切片。P12 整体仍远未迁移；combat -> health -> tracker ->
death 核心闭环、parent/segment 首击身份路由、普通 immortal strike 语义、knockback 边界、inactive
`PendingDespawn` guard、四类 checkDead phase、局部 Movement velocity commit，以及显式
`NpcDeathPhaseContext` 从 combat/致死 DoT 到 lifecycle result 的组合、decision 请求的 owner health 恢复和
parent/segment 镜像由约 10% smoke 覆盖。生产 strike 入口、phase decision 的生产调用接线、hit effects、完整
movement/death 闭包和其余成员仍未迁移。不改 Version4 或完整
参考树，也不声称完整行为等价或迁移成功。

frontmatter 的 `sourceModified: true`/`testsRun: true` 记录 packet API 声明收敛、NPC buff/status 与 DoT
窄切片、普通 immortal strike 修正、knockback 计算及 velocity state 提交、death lifecycle guard 修正，以及显式
phase-context 组合和 lethal DoT 转交后的 10% 核心 smoke；该验证只覆盖所列核心局部行为，不覆盖 packet API 行为、
DoT 生产调用/结果消费、生产调用/decision 的生产消费接线或完整 P12。权威 outputReport 仍为
`designStatus: proposed`、`verificationStatus: not-run`、`migrationStatus: not-claimed`。

P12 唯一权威输入仍是已结算的报告：

- partition: P12
- taskId: AUTH-SYS-P12
- original sessionId: 64850edbdb464104ba04d413bfc362e1
- expected/observed: 189/189
- report designStatus: proposed
- report verificationStatus: not-run
- report migrationStatus: not-claimed

本执行文档不重新 claim 分区，也不创建新的成员范围；如果未来要修改权威 outputReport，
必须通过 runner 使用同一个 claim 输入和原 sessionId 结算，不得手工改 task-state、
ledger、lock 或报告元数据。

## 2. 不可绕过的前置条件

执行任一实现阶段前，必须有以下输入且版本一致：

| 前置项 | 门槛 | 失败处理 |
|---|---|---|
| P12 report | 189/189、成员表和 cross-partition handoff 可读 | 停止实现，补 report 证据 |
| 本设计文档 | owner、API、barrier、Observation 和 gaps 已评审 | 保持 proposed，不写代码 |
| Version4 source | 记录 NPC、MessageBuffer、NetMessage、tracker、interaction 当前 hash | hash 变化则重新做 focused source review |
| 完整参考树 | 单独记录 hash；只作 reference-only 对照 | 不得覆盖当前 Version4 事实 |
| CPG Query API | 初始化只读数据库并记录 manifest hash、query scope、Status、Gaps | partial/预算耗尽保留 gap，不压成 no relation |
| target inventory | src/NSSLC 逐成员 reader/writer、registration、scheduler 盘点 | 发现重复 writer 阻塞切换 |
| scheduler/effect owner | 明确 world/session、network、persistence、loot/progression 负责人 | owner 未确定不得进入实现 |

## 3. 目标文件规划

以下表格同时记录目标 owner 规划和当前落地状态。未列为“已落地”的文件仍只是候选位置；
已落地文件也只代表本次局部切片接通，不代表完整 P12 行为已经接通。

| 候选路径 | 责任 | 当前状态与落地条件 |
|---|---|---|
| src/NSSLC/Component/Npc/NpcAuthoritySystem.cs | identity、target/AI commit | owner 候选；stable identity、generation 和 AI writer closure partial/unknown |
| src/NSSLC/Component/Npc/NpcMovementSystem.cs | movement、collision、history、teleport；当前包含 knockback 计算和 velocity commit | `ApplyKnockback` 写入 `MovementStateComponent.Velocity`，由候选 strike coordinator 调用且核心 smoke 通过；生产调用、完整 velocity writer、world/collision/teleport 写入闭包 partial/unknown |
| src/NSSLC/Component/Npc/NpcStatusSystem.cs / NpcStatusTickSystem.cs | buff/status/regen/shimmer phase composition | local phase order through shimmer transparency, Blood Moon intent, and optional gravity calculation implemented; production scheduler/caller, packet 23 hydrate, AI state readers, remaining buff VFX, transform/gravity state consumer, life<=0 cleanup, water helper and failure recovery remain partial/unknown |
| src/NSSLC/Component/Npc/NpcCombatSystem.cs | accepted damage、NPC/parent health commit、combat result | 新增 `ResolveAndCommitStrike` 组合 damage/tracker -> Movement velocity commit -> death reconcile；Version4 strike 公式、显式 parent target、DoT direct-stage、同步 text Port 和 owner forced strike commit 已落地；生产调用、justHit/hit effects、真实 text sink、runtime packet transport、mode gate 与 legacy slot resolver 未完成 |
| src/NSSLC/Component/Npc/NpcKnockbackRequest.cs / NpcKnockbackResult.cs | 显式 knockback 输入快照和速度前后计算结果 | `NpcCombatSystem.ResolveAndCommitStrike` 调用 Movement 候选提交；局部 smoke 通过，生产入口和完整 velocity owner 闭包仍 unknown |
| src/NSSLC/Component/Npc/NpcDamageOverTimeRequest.cs / NpcDamageOverTimeResult.cs / INpcDamageOverTimeTextPort.cs | DoT 原始 amount、world credit、text 和 lethal-strike intent/result | request 带 child identity/text bounds；result 带 direct/final life、typed text intent、发布结果、forced strike result 与 owner packet 28 intent；真实 sink/transport 未接通 |
| src/NSSLC/Component/Npc/NpcDeathLifecycleSystem.cs + NpcDeathPhaseQuery.cs | terminal/despawn commit 与四类 checkDead phase 子集 | type 400 spawn 仅返回 intent；AI 单写者、Combat 生产调用、其他 phase、parent/cleanup closure 未完成 |
| src/NSSLC/Component/Npc/NpcDamageTrackingSystem.cs + INpcDamageTrackingStrategy | tracker registry、credit、tick、expiry 与 canonical tracker NPC type | composite strategy 暴露定义首类型作为 `TrackerNpcType`，snapshot 保留该身份；parent resolution、AddDamageToLastAttack、current stub semantics、session scope 未闭合 |
| src/NSSLC/Component/Npc/Network/INpcReplicationPacketApi.cs | packet 23/28 decode/encode, Packet 54 buff-slot publish, recipient selection and opaque request/result types | declaration-only; no parser, writer, recipient selection, transport, authorization or apply logic; packet 23/54 inbound remains unknown |
| proposed NpcNetworkSyncProjection shape | per-client cursor/skip/ack selection declaration | no same-named target file; target state is NpcReplicationDirtyState.StreamCursor and NpcClientReplicationState.SkippedSyncCount/LastAcknowledgedRevision; legacy connection/reset mapping partial/unknown |
| src/NSSLC/Component/Npc/NpcInteractionSystem.cs | registry、availability、commands | registry registration confirmed；current action bodies、commerce owner unknown |
| src/NSSLC/Component/Npc/NpcPresentationProjection.cs | text/frame/trail/sound snapshot | projection boundary candidate；presentation sink 与 event order partial/unknown |
| src/NSSLC/Component/Npc/NpcDamageDefinitionCatalog.cs | current-version boss-mob and composite type groups | 已落地静态登记集并与完整参考源码对照；content reload、persistence scope、invasion strategy 未关闭 |

已有同名或相似目标文件不得直接作为 owner 证明。实施前要确认 namespace、project
registration、source include 和唯一写入责任；不为凑目录新增 Shared/Common/Misc 兜底层。

## 4. 阶段和检查点

### Phase 0：冻结来源和证据登记

**输入：** P12 report、设计文档、当前/完整参考 hash、CPG manifest。

**步骤：**

1. 重新计算五个 authoritative 文件和五个 complete-reference 文件 hash。
2. 通过 CpgEvidence.ps1 初始化只读数据库并确认 import complete、manifest 一致。
3. 对计划使用的符号记录 Symbol ID、SourcePath、Status、Gaps、query scope。
4. 读取目标树的局部 AGENTS.md、项目注册和当前 writer；不修改文件。

**退出条件：** 来源 hash 未漂移，query 结果可追溯，漂移/预算 gap 已列入表格。
否则回到 source review，executionStatus 保持 not-started。

### Phase 1：补齐关系和单写者清单

**目标：** 在写代码前关闭会改变 owner 的关系。

**必须查询/阅读：**

- NPC.StrikeNPC、StrikeNPCNoInteraction、GetHurtByDebuff、UpdateNPC、
  UpdateNetworkCode、checkDead、ResetForNewNPC、SetDefaults；
- Main 对 NPC.UpdateNPC、NPCDamageTracker.Update、NPCInteractions.Initialize 的调度；
- Player/Projectile 的 StrikeNPC 入站；
- MessageBuffer packet 23/28 reader 与 NetMessage writer/recipient；
- tracker 所有 writer/reader、BossDamageTracker/InvasionDamageTracker；
- NPCInteractions 所有 action body、重复初始化和外部 UI/commerce 调用；
- src/NSSLC 中 life、active、realLife、AI、buff、tracker、replication、interaction 的
  所有读写和注册点。

**退出条件：** 189 个成员各有 observed reader/writer 清单；无法闭合的项标 partial/unknown；
同一 invariant 的双 writer 被阻塞并交 integration-review。

### Phase 2：身份、spawn、reset、parent link

**顺序：**

1. 定义 world/session scope、slot index、instance generation、type/netID 转换。
2. 建立 realLife parent/segment link 的创建、镜像、死亡和 slot reuse 规则。
3. 先接 Lifecycle/Authority commit，再接 NetworkApply 的 spawn/reset。
4. 对失败定义：无 slot、未知 type、generation mismatch、父实体缺失、重复 reset。
5. 只发布完整 authority snapshot；禁止半初始化实体进入 network 或 tracker。

**检查点：** spawn、reset、reuse、parent death 的 Observation fixture 已定义并能定位唯一写者。
packet 23 full hydrate 是独立网络 gate：当前 Version4 入站 case 在读取字段前无条件 break，
不能用不可达 body 作为基线；baseline 未裁定前保持 inbound unknown。entity owner 未闭合时
不得进入完整 P12 切换。

### Phase 3：status、movement、AI 和 tick barrier

**顺序：**

1. 记录 buff reset/apply/expire、immunity、DoT、regen、gravity/collision、AI 的读写。
2. 以输入快照定义 Status -> Authority -> Movement 的可见性和随机/时钟输入。
3. 接通 status、movement、authority 的 commit port；Projection 只读。
4. 明确客户端 unsynced tile、server-only damage、town/bee 等 mode gate。
5. 对 UpdateNPC 的提前 return、life<=0、netUpdate 清理和 justHit 清理建立 Observation。

**检查点：** 同一 tick 的 status 输入在 damage 前可见，movement/network snapshot 在 commit
后冻结；动态 AI、reflection、mod hook 未闭合时保留 unknown 并阻塞正式切换。

### Phase 4：Combat、tracker、death 组合

核心顺序：

~~~text
attribution + eligibility Query
  -> accepted amount
  -> NpcCombatSystem single life/parent commit
  -> DamageAccepted event
  -> NpcDamageTrackingSystem single credit
  -> NpcDeathLifecycleSystem single terminal decision
  -> effect/network/presentation projections
~~~

必须覆盖：

- owner=-1 归一化、owner=255 world credit、Player/Projectile/hostile 归因；
- 按 Version4 次序验证 max(1, Damage - Defense * 0.5)、crit、Red Hat 的乘法后 int 截断/下限、仅 takenDamageMultiplier > 1 生效，再对 tracker/life 做 int 转换；
- 用同 revision 的 NPC type 与 ai[3] / localAI[3] 状态解析 Red Hat 特例；在目标行为闭包完成前，不从名字/type 猜测，也不将该分支算作已迁移；
- 普通 Strike 的 immortal 命中接受但不记 tracker、不写 parent/life、不镜像 child；Damage >= 9999 且 owner == 255 的 tracker bypass，以及 Damage == 9999 的效果条件；
- realLife 父生命写入和 child life/lifeMax 镜像；
- knockback、justHit、AI reaction、HitEffect 的事件/状态顺序；
- GetHurtByDebuff 的直接扣血、致死 StrikeNPCNoInteraction(9999)、packet 28；
- DoT 单独走 raw direct amount，不走 strike defense/crit/multiplier；先记 capped world credit，immortal 仍记 credit 但不改 life；lethal direct stage 暂设 life 为 1，再提交 owner=255/damage=9999 forced strike，输出 CombatText 与 packet 28 typed intent，不镜像 child；
- tracker create/start/stop、active/recent、3 上限、54000 expiry、BossKilled；
- 完整参考版的 AddDamageToLastAttack、CompareTo、GetReport、Invasion strategy 只在
  current-version baseline 决策后接入；
- checkDead 的 phase transition、active=false、loot/progression/announcement ports。

**检查点：** 一次 accepted hit 只有一条 life commit、一条 tracker credit、一条 death
decision。任一重复或缺失都停止切换并保留旧 writer。

### Phase 5：网络协议和 session replication

**顺序：**

1. 为 packet 23/28 定义 decode、encode、recipient selection 和 publish 的函数签名；只保留声明，不写函数体、字段解析、字段编码、筛选、发送或 authority apply 逻辑。
2. 声明使用的不透明 request/result DTO 只确定类型名与位置；packet 23 reader 不可达导致的 schema 缺口仍标 unknown，不借完整参考树填充。
3. packet 28 的当前 Version4 reader/writer 差异和 mode/owner 差异记为证据，不在本切片实现策略。
4. 任何既有 packet helper 均只列为目标树历史状态，不扩展、不接线、不以其局部代码推断完整网络行为。

候选函数签名（仅声明；DTO 字段和函数体均不在本次范围内）：

| API | 候选签名 |
|---|---|
| packet 23 decode | DecodePacket23(ReadOnlyMemory<byte> payload): Packet23DecodeResult |
| packet 23 encode | EncodePacket23(Packet23EncodeRequest request): Packet23EncodeResult |
| packet 28 decode | DecodePacket28(ReadOnlyMemory<byte> payload): Packet28DecodeResult |
| packet 28 encode | EncodePacket28(Packet28EncodeRequest request): Packet28EncodeResult |
| recipient selection | SelectRecipients(PacketRecipientRequest request): PacketRecipientPlan |
| publish | Publish(PacketPublishRequest request): PacketPublishResult |

**检查点：** API 的函数签名和不透明 DTO 类型名可审阅；本切片不定义错误 DTO，也不实现 packet 解析、权限判断、
recipient 筛选、传输或状态提交，因此相应运行时行为继续标 unknown，且不执行 round-trip 验证。

### Phase 6：interaction、commerce、presentation

**顺序：**

1. 先确认 current Version4 action 是存根还是实现；不能把 complete reference 动作当当前行为。
2. 把 Condition/GetText 分为纯 availability/localization Query，把 Interact 变为显式 Command。
3. 定义 TalkNPC、LocalPlayer、shop index/custom text、quest/heal/town/chat scope。
4. 将 UI、音效、聊天、进度、commerce 通过 adapter/port 出站。
5. 检查 Initialize 重复调用、注册顺序和失败重试。

**检查点：** Query 无隐藏写；Command 只有一个 owner；动作失败不会部分写入 interaction
authority。外部 owner 未明确时保留 integration-review。

### Phase 7：持久化、world/session 和恢复

必须补证：

- NPC identity、life、realLife、active、AI/status/tracker 是否写入 world save；
- tracker 是否 session-only，disconnect/unload 是否清理；
- packet cursor、recent tracker、interaction state 的 world/connection scope；
- save/load 版本、缺字段默认、取消、重试、恢复和多 world 并行隔离。

**检查点：** persistence/session 关系未闭合时保持 unknown；不能用“没看到调用”
证明不持久化。

### Phase 8：shadow Observation 和单 writer 切换

**顺序：**

1. 用相同输入建立 legacy trace 与 new composition trace。
2. 比较 return/error、authority delta、events/effects、order/visibility、lifecycle/scope、
   retry/idempotency。
3. shadow 期间新路径不得发布 authority、network、persistence 或不可逆 effect。
4. 只在 accepted diff 获批后，于一个 commit barrier 切换唯一 writer。
5. 旧 facade 可保留兼容入口，但不能再隐式写入；不允许双写。

**检查点：** 真实目标项目的必要行为测试命中新 owner 和新组合；否则 executionStatus 只能
是 partial，migrationStatus 仍为 not-claimed。

### Phase 9：删除门禁和回滚演练

只有 189 成员、所有入口、网络、持久化、生命周期和 Observation 行为全部关闭并得到独立
review 批准后，才评估删除旧 writer。删除不是本执行文档自动动作。

## 5. 一写者和禁止双写规则

| 规则 | 执行要求 |
|---|---|
| health/life | 只有 NpcCombatSystem/DeathLifecycle 协议能提交；Health Component 不自行改变 |
| active/slot | 只有 Lifecycle owner 提交；network reader 只能发 command |
| realLife | parent link owner 维护 generation/镜像；Combat 不复制另一套关系 |
| tracker | NpcDamageTrackingSystem 是唯一 credit/start/stop/advance writer |
| packet | Adapter 编码/解码；不在 Adapter 内执行 damage/death 规则 |
| interaction | registry/availability 与 effect command 分开；UI projection 不改状态 |
| projection | 只消费 revision；投影失败不回写 authority |
| legacy facade | 切换前可读适配，切换后禁止隐式写入；shadow 不发布外部 effect |

若发现同一 invariant 的两个 writer，停止该 phase，记录 source path/line 和 revision，
交 integration-review，不通过执行顺序约定掩盖双写。

## 6. 失败、暂停和回滚

### 6.1 阻塞条件

- source hash 漂移而未重做 focused review；
- CPG query partial、预算耗尽或端点身份不一致；
- 清空方法体被当作 no-op；
- identity/realLife/active/life writer 不唯一；
- packet 23/28 未定义校验和错误路径；
- scheduler、persistence、session 或外部 effect owner unknown；
- 目标树已有重复注册或无法确认单 writer。

阻塞时只保存证据和 gap，不修改生产代码，不把失败改写为迁移完成。

### 6.2 回滚动作

1. 在下一个 commit barrier 停止新 System 接收外部写入；
2. 丢弃未提交的新 state/command；
3. 恢复 legacy adapter 的唯一写入路径；
4. 保留协议/schema 版本和已发布 effect 的补偿记录；
5. 用 Observation diff 标记触发原因，补齐缺口后从对应 phase 重试。

如果切换后发布了不兼容网络/存档数据，先回滚协议或 schema，再恢复 writer；不得直接
删除新状态或重用旧 slot generation。

## 7. 验收验证计划（未覆盖项 not-run）

| 验证层 | 场景 | 通过条件 |
|---|---|---|
| inventory | 189 成员、所有 readers/writers、注册和调度 | 每个 authority fact 恰有一个 writer |
| lifecycle | spawn、reset、packet hydrate、reuse、parent/segment、despawn、disconnect | 无 stale reference、无半提交 |
| combat | Player、Projectile、NPC、debuff/world、network damage | accepted amount、life、effect 顺序和一次性不变量一致 |
| tracker | boss/composite/invasion、credit、last attack、sort/report、3/54000 | current baseline 明确且 trace 一致 |
| network API | packet 23/28 decode/encode/recipient/publish signatures | 只审阅函数声明与 DTO 边界；不实现或验证协议逻辑，packet 行为保持 unknown |
| network behavior | packet round-trip、malformed、unauthorized、section、skip/stream、reconnect | 超出本次 packet API 声明范围；完整 P12 行为验收 not-run |
| interaction | Initialize、Condition/GetText、shop、heal/quest/town/chat | Query 纯读，Command/adapter 失败可观测 |
| persistence | save/load、world/session reset、schema upgrade、multi-world | scope、默认和恢复行为一致 |
| rollback | shadow 失败、切换后回退、旧 writer 恢复 | 不产生双写和不可解释 effect |

上表描述完整 P12 验收层，目前均为 not-run。Section 8 单独记录的历史 smoke 只覆盖约
10% 切片，包括 combat/life/tracker/death 核心和 packet 28 payload/recipient-plan/per-slot fake-port 规则；
这是本次 packet API 声明范围调整前的结果。调整后重新运行了不包含 packet 算法断言的核心 smoke；
该结果不改变权威 report 内 verificationStatus: not-run，更不构成迁移成功。

## 8. 本次执行记录

### 2026-10-01 完整参考树与 packet API 范围核对

本条记录对应当时的文档整理，只编辑设计/执行文档，没有修改 src/、Test/ 或项目文件，也没有运行构建/测试；
后续 packet API 范围调整见本文末尾的独立记录。
P12 runner List 显示 AUTH-SYS-P12 已由原 session ID 64850edbdb464104ba04d413bfc362e1
结算完成；本轮未重新 claim，未改权威 outputReport。

Version4 与完整参考树 D:\TRbackup\无任何删减通过编译的逐行比较结果：

| 路径 | Version4 权威源 | 完整参考树 | 结论 |
|---|---|---|---|
| packet 23 reader | MessageBuffer.cs:1182-1184 在任何读取前无条件 break；其后字段读取和 NPC apply 不可达 | MessageBuffer.cs:1577-1583 先按 netMode 作条件退出，client path 随后读取 | 当前 Version4 inbound behavior 为 unknown；参考 reader 仅 reference-only |
| packet 23 writer | NetMessage.cs:680 起有 outbound 字段 writer | NetMessage.cs:684 起有对应 writer | 只确认 writer 一侧，不推出 reader 或 round-trip |
| packet 28 reader | MessageBuffer.cs:1406-1437 当前应用和发送顺序可见 | MessageBuffer.cs:1819-1856 interaction/rebroadcast 有 server-mode gate，strike owner 取值不同 | 保留两树差异；当前任务不实现 packet 策略 |

只读 CPG 查询使用 CpgEvidence.ps1，manifest 为
6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364，SourceSnapshotId=null。
Find-CpgSymbols(AddDamage) 解析到两个 overload；AddDamage(NPC,int,int) 在选定
NPC.cs/MessageBuffer.cs/Main.cs shard 的调用点查询为 complete、2 个调用点且无 gap，
定位至 NPC.cs:67628 与 NPC.cs:78081。AddDamage(int,int) 在相同范围为 partial，
带 NoMatchingFactInScannedScope gap，不能解释为无调用。MessageBuffer.GetData 符号查询
complete；packet switch 的控制流可达性来自源码行复核，不由 CPG 符号结果推断。

按本轮要求，packet 相关后续代码工作限于函数声明和 request/result DTO 名称；不实现
decode/encode、recipient selection、transport、授权或 apply 逻辑。packet 23 inbound schema
没有 Version4 有效 reader 作为依据，继续为 unknown。此前已有的 packet 28 局部 helper
不代表该 API 逻辑获得完整迁移验收。

### 已实现的核心切片

| 文件 | 变更 | 限制 |
|---|---|---|
| Component/Npc/NpcHealthComponent.cs | CurrentLife 改为私有提交状态；仅内部 health commit 方法可变更生命；parent commit 后可由 Combat 内部同步 child current/max life | 同程序集仍需 reader/writer 审计；mirror 只接受显式 parent health，不负责解析 realLife 或 slot |
| Component/Npc/NpcLifecycleComponent.cs | 终态字段私有化，加入幂等 despawn commit | 只覆盖最小 Despawned transition，不覆盖完整 checkDead |
| Component/Npc/NpcCombatSystem.cs | 校验输入、按 Version4 顺序解析 damage、按 local/parent owner 记 tracker、提交 life、同步 child mirror、协调 owner death；`ResolveAndCommitStrike` 在 death reconcile 前提交 knockback velocity | parent 路径要求调用方传入 relation 与已解析 target，并在 identity/assigned slot 不匹配时拒绝；没有 world/legacy-slot resolver，也没有 AI 快照、netMode、`justHit`、HitEffect、完整 realLife/death phase 闭包或已证明的生产 strike caller |
| Component/Npc/NpcDamageRequest.cs | 表达 contributor、damage、tick、defense、crit、multiplier、immortal、tracker bypass、Red Hat 判定和 legacy owner | 已实现 `FromLegacyOwner`、owner 一致性校验和 `owner=255 && damage>=9999` forced-world 标记；仍缺 fromNet、noEffect 等 Version4 输入 |
| Component/Npc/NpcCombatResult.cs | 暴露 accepted delta、tracker 与 death commit 结果 | 不是完整 Version4 StrikeNPC 返回/事件契约 |
| Component/Npc/NpcDeathLifecycleSystem.cs + NpcDeathPhaseQuery.cs | 幂等 terminal commit；对明确输入返回 phase decision 与 type 35/604/605 前置效果 intents，有前置效果时设 `TerminalCommitPending` | type 400 仅返回 spawn intent；AI、health、damage-policy、world/player effect owners 消费 decision/intents 后再 terminal commit；生产协调、GoodWorld 后续分支、其余 checkDead phase、分段清理和外部 effects 未迁移 |
| Component/Combat/CombatContributorId.cs | 增加 contributor provenance 有效性判断和 legacy owner 归一化 | 不映射 Terraria player slot/name；slot/name 绑定仍 unknown |
| Component/Npc/NpcDamageDefinitionCatalog.cs | 固化当前 Version4 的 boss-mob 与 composite 类型登记，并按 boss 标记构造策略 | 静态登记集已与完整参考源码对照；content reload、persistence scope、invasion strategy 和 parent NPC 解析仍 unknown |
| Component/Npc/NpcDamageTrackingSystem.cs + NpcDamageSingleTypeStrategy.cs + NpcDamageCompositeStrategy.cs | 由策略提供规范 `TrackerNpcType`；复合组使用登记顺序的首个 type | 对齐 Version4 `BossDamageTracker` composite constructor；首次受击 segment 与 tracker identity 不混为一谈 |
| Component/Npc/Network/INpcReplicationPacketApi.cs | 只保留 packet 23/28 decode/encode、recipient selection 与 publish 函数声明及空 DTO | 早期 codec、recipient、broadcast 与 transport helper 已移除；协议逻辑和运行时行为不在当前实现中 |
| Test/Terraria.NpcDamageCombatVerification | 新增一个核心 smoke | 只验证约 10% 核心闭环，不覆盖完整迁移 |

核心 smoke 覆盖：Version4 半额 defense/crit、Red Hat 截断、takenDamageMultiplier <= 1 不生效、
legacy owner 归一化、owner mismatch 拒绝、owner=255 且 damage>=9999 的 forced-world tracker bypass、
accepted amount clamp、Boss 50 的 Version4
catalog 策略创建、注册 mob 1 对同一 boss encounter 的 tracker 归并、复合 segment type 14
先受击但仍以定义首项 type 13 作为 tracker identity、lethal death transition、
重复 death reconcile 幂等、无效 multiplier 无写入、死后 damage rejection、旧 tracker
factory 的 null 构造拒绝，以及 tracker recent/killed/credit snapshot。完整 tracker report、
排序、last-attack credit 和 invasion 行为仍未覆盖。该 smoke 的通过只验证显式 request 的局部
代码合同；不能作为 AI 快照传递、网络、parent 或完整 P12 parity 证据。

此前曾在 10% smoke 中加入 packet 28 codec、broadcast-plan 和 fake-port 断言；随后按用户限定移除了这些网络算法及断言。
当前核心 smoke 不验证 packet API、编解码、recipient、broadcast 或 transport；packet 23/28 行为保持 unknown。

### 2026-10-01 参考源码与调用关系补证

完整参考源码固定为 `D:\TRbackup\无任何删减通过编译`，仅作为 reference-only 线索。其
`Terraria.GameContent/NPCDamageTracker.cs` 静态注册组与当前 Version4 该文件的登记组一致：
boss-mob 映射及 composite boss 分组；引用源 hash 为
`A75A5AF322A33B3CAD8ECBC7BF4B3B44AC92B06B5CD46A1404EBFD6FA7A64B8A`，当前 Version4 hash
仍以 Section 2 的权威来源表为准。

通过只读 `CpgEvidence.ps1` 查询 Version4 CPG（manifest
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`）：
`NPCDamageTracker.AddDamage(NPC,int,int)` 在选定的 NPC/MessageBuffer/Main 源文件范围内
返回 complete、2 个静态调用点且无 gap；源码定位到 `NPC.StrikeNPC` 与
`NPC.GetHurtByDebuff`。`BossKilled(NPC)` 在选定 NPC.cs 范围返回 complete、1 个静态调用点，
定位到 `NPC.DoDeathEvents_CelebrateBossDeath`。这些查询不闭合动态调用、调度或副作用；
当前 Version4 清空方法和完整参考版独有语义继续标 unknown。

### 2026-10-01 packet 28 广播策略补充

Version4 `Terraria/NetMessage.cs:1737-1747` 的 packet 28 广播路径遍历 legacy client slot
0..255，仅向未被 ignore、启用 broadcast、已连接，且 NPC 已死亡或该 NPC 所在 section 已 active
的客户端发送。完整参考树 `D:\TRbackup\无任何删减通过编译` 的 packet 28 分支具有相同 recipient
predicate；它在 `SendData` 入口还多了 `Main.netMode == 0` 早退，而 Version4 对应入口无此
判断。该 mode 差异仅作 reference-only 对照，没有移植为当前 Version4 行为。

通过 `CpgEvidence.ps1` 重新查询 Version4 CPG，manifest 为
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，SourceSnapshotId 为 null。
`SendData` 和 `TrySendData` 符号查询及在选定 `Terraria/NPC.cs`、`Terraria/NetMessage.cs`、
`Terraria/MessageBuffer.cs` 三个 shard 的调用点查询均为 complete、无 gap，分别返回 122 和
127 个调用点；调用点结果不按 packet ID 过滤，不证明运行时 mode 或 broadcast 关系。

当前新增 `NpcPacket28RecipientPolicy`、`NpcPacket28BroadcastAdapter` 和 per-recipient transport
adapter：recipient policy 基于显式 snapshot 执行 predicate，broadcast adapter 产生 10-byte
payload 与排序后的候选 slot，transport adapter 只通过注入 port 逐 slot 调用并报告失败。核心
smoke 使用 fake port，没有真实网络发送。真实 client slot 与 target NPC section 的 snapshot 绑定、
mode/auth gate、packet 23、socket/runtime 接线、日志和重试/reconnect 生命周期仍为 partial/unknown。

### 2026-10-01 Damage 公式和行为输入补证

独立读取 Version4 Terraria/Main.cs:14254 与 Terraria/NPC.cs:53188、67461、67485-67500、
67620 附近的提交路径，确认 helper 公式为 max(1, Damage - Defense * 0.5)，返回 double；
后续依次执行 crit、Red Hat 的乘法后 int 截断并将下限设为 1、仅大于 1 的 taken multiplier，
最后以 (int)num 写 tracker 和父/本体生命。RedHatSkeletronAdjustmentsEnabled 读取 type
35/36/32/33 与 ai[3] / localAI[3]；完整参考树 Main.cs:67056、NPC.cs:67615、82485、
82509-82524 的同名 helper、StrikeNPC 顺序和判定都匹配。两个版本分开取 hash；完整参考仍只作
supplementary，不取代 Version4 行为基线。

同一 CPG manifest 下，Find-CpgSymbols 对 Terraria/Main.cs 的 CalculateDamageNPCsTake 返回
complete，1 个 double(int,int) symbol、无 gap；Find-CpgCallSites 的 scope 为
Terraria/NPC.cs、Player.cs、Projectile.cs、MessageBuffer.cs，返回 complete、1 个 NPC.cs
静态 call site、无 gap。数据库 SourceSnapshotId=null，所以这是所选索引范围内的静态关系，
不证明动态入口或当前 source snapshot 绑定。

当前目标 NpcCombatSystem.ResolveDamage 已改为半额 defense 公式，并由 NpcDamageRequest 显式
接收 Red Hat 判定；NpcBehaviorComponent/localAI 到 request 的解析和传递尚未接通。Damage=40、
Defense=10、crit=true 在 Version4 与目标局部计算均为 70；Red Hat 启用时均为 48；倍率 0.5
保持 10 点基础伤害。Version4 `NPC.cs:67625-67629` 的原始 `Damage>=9999 && owner==255`
分支也已由 `FromLegacyOwner` 显式输入合同覆盖：跳过 tracker credit，仍提交生命和 death。
该 smoke 只覆盖局部输入合同，AI、网络、parent 和外部效果缺口保持
partial/unknown。

### 2026-10-01 Parent/segment health commit

Version4 源码 `Terraria/NPC.cs:67625-67640` 先按 `realLife` 选择 parent，再扣 parent `life`
并将 parent `life/lifeMax` 镜像给被命中的 child；`NPC.cs:67817-67824` 对 parent 调用
`checkDead`，而 `checkDead` 在 `64571-64578` 会忽略 inactive NPC、非 root segment 和正生命值。
Version4 `Terraria.GameContent/NPCDamageTracker.cs:129-142,178-190` 的
`GetRealActiveNPC`/`AddDamage` 将 child 归一到 parent，再检查 parent active/life。完整参考树
的对应 health/mirror 路径在 `Terraria/NPC.cs:82652-82667`，parent death 调用在 `82846`；其
StrikeNPC tracker call 额外有 `Main.netMode != 1` gate，而当前 Version4 该 call site 没有此
gate，因此完整参考仅作交叉线索，目标代码没有照搬该条件。

CPG manifest 仍为 `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，
`SourceSnapshotId=null`。本次只读 Query API 结果：`NPCDamageTracker.AddDamage` 符号 complete，
在所选 `NPC.cs/MessageBuffer.cs/Main.cs` 范围有 2 个 NPC.cs call site、无 query gap；
`NPC.realLife` 的 24 个 NPC.cs member use 查询 complete，但 StrikeNPC 内 4 个引用的
AccessMode 均为 Unknown/EvidenceStatus partial。`NPC.life` 730 个和 `NPC.lifeMax` 1,179 个
选定 shard uses 查询 complete；damage block 中 child 的 life/lifeMax 左值写入为 confirmed，
parent field 的部分右值/receiver 关系仍 partial。偏移不是行号，源码锚点由独立读取 Version4
当前 hash `NPC.cs=29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17` 对照。

目标增加 `NpcParentHealthTarget` 作为调用方解析后的显式输入，带 parent entity identity/type、
health、lifecycle 和 boss eligibility。Combat 对 `NpcParentRelationComponent` 的 parent
instance ID 与已分配 legacy slot 做一致性检查；失配在 tracker/life 写入前返回
`InvalidParentRelation`。匹配时以 parent type/lifecycle 决定 tracker 与 death owner，提交 parent
health 后把 parent current/max life 镜像到 child。parent inactive 时仍按 StrikeNPC 提交生命，
但跳过 tracker 和 checkDead reconcile。Combat 不执行 `realLife` 数组查找；外层解析者必须保证
target 来自同一当前 world/session，且稳定 ID 和 slot generation 的全局新鲜度尚未证明。

10% 核心 smoke 新增 parent ID mismatch、slot mismatch 的无写拒绝，以及有效 parent mirror/
parent credit 和 inactive parent tracker bypass。当前 Version4 父/child phase transition、segment
cleanup、realLife 建立/复用调用链、network/persistence 和完整 death side effects 仍为
partial/unknown，不由该 smoke 升级。

### 2026-10-01 Debuff damage baseline and first implementation stage

Version4 `Terraria/NPC.cs:78077-78100` 的 `GetHurtByDebuff` 顺序为：先调用
`NPCDamageTracker.AddDamage(this, 255, amount)`；用 `realLife` 选 parent slot；对非 immortal parent
直接扣除未经 defense/multiplier 解析的 `amount`；在 child 的位置发 DoT CombatText；parent
仍存活或 immortal 时返回；否则将 parent life 置为 1，调用 parent 的
`StrikeNPCNoInteraction(9999)`，然后发送 packet 28。直接 decrement 与 9999 strike 是两个不同
阶段，不能当成普通一次 `ResolveAndCommit` 调用的现有等价路径。

完整参考树 `Terraria/NPC.cs:93683-93715` 在 tracker credit 和 lethal strike 外加
`Main.netMode != 1` 条件，并只在 server mode 发送 packet 28；Version4 当前源码没有这些 gate。
该差异仍以 Version4 为实现基线，完整参考只作 supplementary，不照搬其 mode 行为。
Version4 `MessageBuffer.cs:1406-1435` 读取 packet 28 字段、执行 player interaction 与 strike/direct
death 分支、rebroadcast packet 28，随后按 life/realLife 条件发送 packet 23；此网络入口的授权、
畸形值和真实 mode 闭包仍未关闭。

CPG Query API 的 `GetHurtByDebuff(void(int))` 符号查询 complete；在选定
`NPC.cs/MessageBuffer.cs/Main.cs` 范围有 3 个静态 call site（NPC.cs 两处、MessageBuffer.cs 一处），
无 query gap。`Get-CpgCallableFacts` 返回 partial，gap 为 `CalleeEffectsNotExpanded`；因此只能把
源码可见的直接顺序作为锚点，不能从调用事实推出 tracker、Strike 或 packet 的完整副作用。

目标首阶段已增加 `NpcCombatSystem.ApplyDamageOverTime`、`NpcDamageOverTimeRequest` 和
`NpcDamageOverTimeResult`：它用 world contributor 将 tracker credit 限制为当前 life；immortal 输入
保留 tracker/text intent 而不写 life；非 immortal 走原始 amount，越过 lethal 时将 owner life 暂置 1，
并返回 forced-death-strike intent。父 DoT 不镜像 child life/lifeMax，也不在该阶段标记 killed 或
提交 despawn。验证仅覆盖这些局部代码合同；DoT text 的实际发送、forced strike 执行、packet 28、
mode gate 和完整 death phase 仍 partial/unknown，不能称为 DoT 或 P12 迁移成功。

### 2026-10-01 初版 DoT core smoke（forced strike 接线前）

10% P12 核心 smoke 新增三个 DoT 断言：raw amount 直接扣 life 并累加 world tracker credit；
immortal DoT 保持 life、仍累加 world credit 并返回 CombatText intent；parent lethal DoT 将 parent
health 暂置为 1、返回 forced strike intent、保留 child health、且不提前转入 terminal lifecycle。
这组断言未执行真实 presentation/network/death adapter，也不验证 Version4 全部 DoT Observation；
后续 forced-strike 接线与当前状态见本文末尾的 continuation 记录。

### 实际验证记录

仓库 `global.json` 要求 SDK 10.0.400；系统 PATH 的标准 dotnet 仅提供 10.0.100、10.0.102、
9.0.302 和 8.0.302。为不修改 `global.json`，本次确认并使用
`D:\TRbackup\dotnet-sdk-10.0.400\dotnet.exe`，通过串行 wrapper 的显式 `-DotnetArguments`
数组执行：

```powershell
$env:PATH = 'D:\TRbackup\dotnet-sdk-10.0.400;' + $env:PATH
$args = [string[]]@(
  'build',
  '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments $args
```

- Build（包含必要 restore）：项目 `Test/Terraria.NpcDamageCombatVerification/Terraria.NpcDamageCombatVerification.csproj`，
  exit code 0，0 warning、0 error；依赖 `Terraria.Relationships`、`Terraria.Combat`、`Terraria.Npc` 同步生成。
- Artifact：`D:\TRbackup\NLTX\Build\bin\Terraria.NpcDamageCombatVerification\Debug\net10.0\Terraria.NpcDamageCombatVerification.dll`。
- Artifact SHA-256：`189377089F40038C0BCDFF3BA8176041ABD5D6A5D4310BF35CF51DF51360149F`。
- Smoke 精确命令：

```powershell
$env:PATH = 'D:\TRbackup\dotnet-sdk-10.0.400;' + $env:PATH
$args = [string[]]@(
  'run', '--project',
  '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '--no-build', '--no-restore', '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false')
& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments $args
```

- Smoke exit code 0，输出 `PASS: P12 core combat, life, tracker and death smoke (10% scope)`。
- 本次 smoke 约覆盖 10% 核心切片，包括公式、owner/bypass、有效 parent 提交及 child life/lifeMax
  镜像、parent identity/slot 失配无写拒绝、inactive parent tracker bypass、tracker/death 基线；
  没有运行完整 P12 验收层。

DoT 切片随后通过受影响项目的串行 build 和 10% core smoke；本节 DoT 验证记录给出最终
artifact SHA-256。首次 `--no-build` smoke 曾加载 verifier 目录里的旧 `Terraria.Npc.dll`
副本（hash `79ADD0B87F597A1FDB193DF1D6B75A857C8A80DA8740846A5FA56FD7AEF4F6A0`），导致直接 amount
out 值显示为 4。反射对照表明最新主输出（hash `385CDDB0870689E13997162D84503F04E4C6EE9D9DE3D87EA0190311DCC7E73A`）
返回 8；随后通过串行 wrapper 刷新 verifier 引用副本、核对 hash 相同后重跑，最终 smoke 通过。
旧副本结果不作为当前验证证据；此前 artifact hash 也不代表新增 DoT 代码。

### 2026-10-01 DoT build 与 core smoke 验证

构建入口使用仓库串行 wrapper 与 SDK 10.0.400。源码依赖已由前一条同项目 build 编译；为刷新
verifier 输出目录中未更新的 project-reference 副本，另执行下列增量 build：

```powershell
$env:PATH = 'D:\TRbackup\dotnet-sdk-10.0.400;' + $env:PATH
$project = '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj'
$arguments = [string[]]@(
  'build', $project, '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false', '-p:BuildProjectReferences=false', '-v:n')
& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments $arguments
```

- Copy-refresh build：exit code 0，0 warning、0 error；verification project 的增量 `CoreCompile` 为 up-to-date，MSBuild 把当前 `Terraria.Npc.dll` 复制到 verifier 输出目录。
- Copy verification：`Build/bin/Terraria.Npc/Debug/net10.0/Terraria.Npc.dll` 与 verifier dependency 的 SHA-256 均为 `385CDDB0870689E13997162D84503F04E4C6EE9D9DE3D87EA0190311DCC7E73A`。
- 受影响项目：`Test/Terraria.NpcDamageCombatVerification/Terraria.NpcDamageCombatVerification.csproj`；可执行产物位于 `D:\TRbackup\NLTX\Build\bin\Terraria.NpcDamageCombatVerification\Debug\net10.0\Terraria.NpcDamageCombatVerification.dll`，SHA-256 为 `72C2D46F3BE1B6F37FEF5C08BDCCA038358C9B9247F4AB7F6624825F6847BC25`。

Smoke 通过串行 wrapper 用已构建产物运行：

```powershell
$env:PATH = 'D:\TRbackup\dotnet-sdk-10.0.400;' + $env:PATH
$project = '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj'
$arguments = [string[]]@(
  'run', '--project', $project, '--no-build', '--no-restore', '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments $arguments
```

- Smoke exit code 0，输出 `PASS: P12 core combat, life, tracker and death smoke (10% scope)`。
- 此次 10% 核心范围验证 DoT raw direct amount、world tracker credit cap、immortal 时 tracker/text intent、parent lethal 暂置 life=1、forced-strike intent、child health 不镜像及不提前 despawn；完整 packet 28、CombatText sink、strike/death effects、network mode 和 P12 行为闭包仍未覆盖。

以上只代表 `.NET SDK 10.0.400` 下受影响项目的依赖编译和一个核心 smoke。P12 其余行为测试、
网络、存档、父子 NPC、交互、AI、完整
damage parity 和 migration success 均未验证。

### 2026-10-01 DoT owner forced-strike continuation

本阶段把初版 lethal intent 接入 Combat：direct stage 记录 world credit 并将 lethal owner 暂置为
life=1；随后以 `NpcDamageRequest.FromLegacyOwner(damage: 9999, legacyOwner: 255)` 调用同一
`ResolveAndCommit`。forced strike 不产生第二份 tracker credit，实际提交 owner life=0 并协调一次
`NpcDeathLifecycleSystem` terminal transition。结果保留 `LifeAfterDirectStage`、
`ForcedDeathStrikeResult`、`LifeAfterForcedDeathStrike` 和 `DeathPacket28Requested`。父 DoT 不镜像
child life。DoT 验证现在只检查 life owner 的 health，所以 child life 已为 0 时，仍按 Version4
直接路径对有效 parent 扣 raw amount。

10% 核心 smoke 将 parent life 设为 4、child life 设为 0、DoT amount 设为 8；检查 world credit
限幅为 4、direct stage 暂置为 1、forced strike 应用 1 点且 tracker bypass、death transition 只发生
一次、最终 parent life 为 0、child life 保持 0，并返回 packet 28 intent。该 smoke 不执行
CombatText sink、packet 28 transport、netMode gate 或完整 death effects。另验证文本 intent 保留
child instance ID、矩形和 raw amount，packet 28 intent 保留 parent instance ID、legacy slot 与 9999。

验证使用仓库 `.NET SDK 10.0.400` 和串行 wrapper，只构建并运行核心验证项目：

```powershell
$env:PATH = 'D:\TRbackup\NLTX\Build\dotnet-sdk-10.0.400;' + $env:PATH
$dotnetArguments = [string[]]@(
  'build',
  '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false')
& '.\Build\Tools\Invoke-SerialDotnet.ps1' @dotnetArguments
```

- Build exit code 0，0 warnings、0 errors；只构建受影响验证项目及其 `Terraria.Relationships`、`Terraria.Combat`、`Terraria.Npc` 依赖。
- 验证产物：`D:\TRbackup\NLTX\Build\bin\Terraria.NpcDamageCombatVerification\Debug\net10.0\Terraria.NpcDamageCombatVerification.dll`。
- 产物 SHA-256：`F346FD5272B63905747DC5E983E3D10A0E0923396319072BBCE4B699FF893CBA`。

随后以已构建产物运行同一 10% 核心 smoke：

```powershell
$env:PATH = 'D:\TRbackup\NLTX\Build\dotnet-sdk-10.0.400;' + $env:PATH
$dotnetArguments = [string[]]@(
  'run', '--project',
  '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '--no-build', '--no-restore', '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false')
& '.\Build\Tools\Invoke-SerialDotnet.ps1' @dotnetArguments
```

- Run exit code 0，输出 `PASS: P12 core combat, life, tracker and death smoke (10% scope)`。

当前状态：executionStatus: partial、verificationStatus: core-smoke-passed、
sourceModified: true、testsRun: true、migrationStatus: not-claimed。full P12 的
packet 28 runtime transport、recipient snapshot binding、mode policy/logging、death phase side effects
与其余行为验证仍未完成。DoT text Port 的局部顺序与失败返回路径见下方 continuation；它没有
接入 Terraria runtime sink。

### 2026-10-01 DoT text publication order continuation

本轮按用户指定的完整参考树复核 `Terraria/NPC.cs`，并以当前 Version4 为行为基线。Version4
`GetHurtByDebuff` 的源码顺序为 tracker credit、parent raw life decrement、child 位置
`CombatText.NewText`、lethal owner life 设为 1、forced `StrikeNPCNoInteraction(9999)`、packet 28。
完整参考树保持相同顺序，但 tracker/strike/packet 有 `netMode` 条件；该条件为 reference-only，未复制。

CPG Query API 使用 manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`。
`Find-CpgSymbols(GetHurtByDebuff, Terraria/NPC.cs)` 为 complete/confirmed；在选定
`NPC.cs`、`MessageBuffer.cs`、`Main.cs` 范围查询到 3 个静态调用点且无 query gap。Callable facts
仍为 partial，gap 是 `CalleeEffectsNotExpanded`；CPG source snapshot 为 null，所以调用关系不被升级为
运行时或副作用闭包证明。关键效果顺序来自逐行阅读两份源码。

目标增加必需注入的 `INpcDamageOverTimeTextPort`。`ApplyDamageOverTime` 在 direct stage 后立即
同步调用 `TryPublish`，再执行 lethal forced strike；result 记录 `CombatTextPublished`。Port 以
`false` 表示预期发布失败，Combat 仍完成权威 strike 与 death transition。当前只有可验证的 Port
边界，未实现或连接 Terraria runtime sink；packet 28 仍只是 strike 后返回的 intent，没有 transport、
mode gate 或 slot-generation resolver。

10% 核心 verifier 检查普通和 immortal DoT 的 text 发布状态；lethal Parent DoT 的 Port 回调观察到
parent life 已到 direct-stage 的 1 且 lifecycle 仍 active，随后模拟发布失败，仍断言 forced strike
成功、parent terminal 一次、child health 不镜像，并生成 packet 28 intent。这个验证覆盖同步调用
顺序和 Port 失败返回策略，不覆盖真实 UI、网络发送或异常抛出路径。

Build 命令（仓库根目录，SDK 10.0.400，串行 wrapper）：

```powershell
$env:PATH = 'D:\TRbackup\NLTX\Build\dotnet-sdk-10.0.400;' + $env:PATH
$dotnetArguments = [string[]]@(
  'build',
  '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false')
& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments $dotnetArguments
```

- Build exit code 0，0 warnings、0 errors；范围只有核心 verifier 与其 `Relationships`、`Combat`、`Npc` 依赖。
- Verifier artifact：`D:\TRbackup\NLTX\Build\bin\Terraria.NpcDamageCombatVerification\Debug\net10.0\Terraria.NpcDamageCombatVerification.dll`。
- Verifier artifact SHA-256：`03F098442B932541E42F6CBD986CAA8B2053AA7A2C027251354B683343C19074`。
- `Terraria.Npc.dll` SHA-256：`10C3967E7A52DFAD11AF7EE2946163A3EEF680B086858A58CD63F2CC54B1CBBB`。

随后在确认没有其他活动 compile-capable 进程后，以已构建产物运行同一 smoke：

```powershell
$env:PATH = 'D:\TRbackup\NLTX\Build\dotnet-sdk-10.0.400;' + $env:PATH
$dotnetArguments = [string[]]@(
  'run', '--project',
  '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '--no-build', '--no-restore', '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false')
& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments $dotnetArguments
```

- Run exit code 0，输出 `PASS: P12 core combat, life, tracker and death smoke (10% scope)`。
- 本轮仍只运行一个约 10% 的核心 smoke；没有运行全量 P12 行为验收。
- P12 权威报告未重新结算，保持 `verificationStatus: not-run` 和 `migrationStatus: not-claimed`。

当前仍是 `executionStatus: partial`、`verificationStatus: core-smoke-passed`、
`sourceModified: true`、`testsRun: true`、`migrationStatus: not-claimed`。真实 CombatText sink、
packet 28 runtime transport、recipient snapshot 映射、mode policy/日志、packet 23、死亡 effects、
状态 tick 调度以及其余 P12 成员读写闭包仍未完成。

### 2026-10-01 Packet 28 DoT wire codec（历史实现，已移除）

以下内容记录声明-only 范围调整前的短期实现与验证，不表示目标当前仍含 codec。其具体代码与 packet 行为断言
已按后续用户限定移除；当前状态以本文“packet API declaration-only scope correction”及其后的记录为准。

本阶段实现 `NpcPacket28Codec`，只覆盖 DoT lethal intent 的 packet 28 payload 和 raw payload 解码。
Version4 的 `NPC.GetHurtByDebuff` 直接调用
`NetMessage.SendData(28, -1, -1, null, num, 9999f)`；对应 writer 在 `NetMessage.cs:845-850`
依次写 `short number`、`short number2`、`float number3`、`byte(number4 + 1)`、`byte number5`。
该调用后续参数使用默认值，所以本 DoT intent 编成 slot `Int16`、damage `Int16`、`0f` knockback、
direction-plus-one `1` 和 critical flag `0`，共 10 bytes。当前 Version4 receiver 在
`MessageBuffer.cs:1406-1435` 读取这些字段并继续 interaction/strike/death 分支；完整参考树的
receiver 额外有 `netMode == 2` gate，这是 reference-only，未写入 codec。

CPG Query API 解析出一个 `NetMessage.SendData` 签名
`void(int,int,int,NetworkText,int,float,float,float,int,int,int)`；在选定 `NPC.cs`、
`MessageBuffer.cs` shard 中 `Find-CpgCallSites` 为 complete，89 个静态调用点且无 query gap。
该查询没有按运行时 `msgType` 参数筛选，也没有证明广播 recipient 或效果顺序；packet 28 的具体
字段和 DoT 调用来自上面逐行源码检查。数据库 manifest 仍为
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，source snapshot ID 为 null。

Codec 编码时校验 instance ID、assigned slot、slot/damage 的 `Int16` 范围和 destination 长度；解码
要求 payload 恰为 10 bytes，并保留 raw signed slot、damage、float 和两个 flag bytes。它不替 inbound
数据做授权、NPC 解析、负 damage 归一化或 authority commit。`NpcReplicationAdapter`、packet 23、
recipient/mode policy、transport、retry 与 reconnect 仍未实现。

10% 核心 smoke 增加以下断言：DoT packet28 intent 编码为 slot 10、damage 9999、knockback 0、
direction-plus-one 1、crit 0；decode 保留这些字段；截断到 9 bytes 的 payload 返回失败。没有运行
额外网络集成、畸形授权或全量 P12 测试。

构建用仓库串行 wrapper，仅构建核心 verifier 与其三个项目依赖。首次带 restore build exit code 0、
0 errors、2 warnings；警告来自 `Build/obj/Terraria.Relationships/project.nuget.cache` 首字节为
`0x00`。restore 后该 JSON cache 可解析，但增量 `--no-restore` build 仍对同一 generated cache 报
1 warning，0 errors。最终增量 build 命令为：

```powershell
$env:PATH = 'D:\TRbackup\NLTX\Build\dotnet-sdk-10.0.400;' + $env:PATH
$dotnetArguments = [string[]]@(
  'build',
  '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '--no-restore',
  '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false')
& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments $dotnetArguments
```

0-error artifact 位于
`D:\TRbackup\NLTX\Build\bin\Terraria.NpcDamageCombatVerification\Debug\net10.0\Terraria.NpcDamageCombatVerification.dll`，
SHA-256 为 `A2497D536BA731947C3E5281D4DBDB6683F8A4C3C45F2F49704310348C9F3E89`；
`Terraria.Npc.dll` SHA-256 为 `601B13597D32F3B0C8092FDD4055DCE98D9A11FD6423C08673DE366D9EDDCEB7`。

随后以 `--no-build --no-restore` 运行同一个核心 verifier，exit code 0，输出
`PASS: P12 core combat, life, tracker and death smoke (10% scope)`。运行命令为：

```powershell
$env:PATH = 'D:\TRbackup\NLTX\Build\dotnet-sdk-10.0.400;' + $env:PATH
$dotnetArguments = [string[]]@(
  'run', '--project',
  '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '--no-build', '--no-restore', '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false')
& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments $dotnetArguments
```

本轮仍只有这一项核心 smoke。
P12 权威 outputReport 继续保持 `verificationStatus: not-run` 与 `migrationStatus: not-claimed`；
迁移状态仍为 `executionStatus: partial`，不宣称迁移成功。

### 2026-10-01 packet 28 recipient-plan smoke 更新（历史实现，已移除）

本节记录后续已移除的 recipient-plan 原型及其历史 smoke；当前 packet API 仅保留声明，不包含 recipient 策略。

按仓库串行构建约束使用 SDK 10.0.400，只构建受影响的核心 verifier 项目：

```powershell
$p12SdkBin = Join-Path (Get-Location).Path 'Build\dotnet-sdk-10.0.400'
$env:PATH = "$p12SdkBin;$env:PATH"
$p12BuildArguments = [string[]]@(
  'build', '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments $p12BuildArguments
```

构建 exit code 0，0 warnings、0 errors。产物位于
`D:\TRbackup\NLTX\Build\bin\Terraria.NpcDamageCombatVerification\Debug\net10.0\Terraria.NpcDamageCombatVerification.dll`，
SHA-256 `F5264DA5BE815C275032875022BDBD5CC1B1DFB89DF3884BB4638EF235BFF648`；
`Terraria.Npc.dll` SHA-256 为
`48546EAF9310859D8CE0DDBDBA66CBD4A45A430F33E9900F23D3B9625B1D2319`。

同一项目仅运行 10% 核心 smoke，使用 `--no-build --no-restore`：

```powershell
$p12SdkBin = Join-Path (Get-Location).Path 'Build\dotnet-sdk-10.0.400'
$env:PATH = "$p12SdkBin;$env:PATH"
$p12VerifyArguments = [string[]]@(
  'run', '--project', '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments $p12VerifyArguments
```

verifier exit code 0，输出 `PASS: P12 core combat, life, tracker and death smoke (10% scope)`。
第一次运行暴露了活 NPC fixture 的预期值错误：未忽略 slot 4 时应包含该 active-section client；
修正为 `[2, 4]` 后重跑通过。最终复核还显式清零 recipient 策略的 stack-allocated slot bitmap；
之后再次构建、重跑同一 smoke 均通过，以上为最终产物 hash。该验证只覆盖核心 smoke 和
packet 28 候选发布计划，完整 P12 仍为 `executionStatus: partial`；权威 outputReport
仍保持 `verificationStatus: not-run`、
`migrationStatus: not-claimed`，不宣称迁移成功。

### 2026-10-01 packet 28 per-recipient transport adapter（历史实现，已移除）

本节记录后续已移除的 transport adapter 及其历史 smoke；当前目标没有 packet transport 实现。

在 recipient-plan 之后增加 `INpcPacket28TransportPort` 和 `NpcPacket28TransportAdapter`。Adapter
按已排序的 recipient legacy slot 顺序同步调用 port；port 返回 `false` 或抛异常时，把该 slot
加入 `failedRecipientSlots` 并继续发送其余 slot，不自动重试。它不负责 runtime/socket 接线、
连接/section snapshot 构造、mode gate 或日志映射。

同一 10% 核心 smoke 增加 fake-port 情景：slot 3 返回失败，Adapter 仍按 `[2, 3, 7]` 尝试，
结果报告失败 slot `[3]`，每次 port 调用收到相同 payload。该验证只证明局部 port contract，
不是真实网络发送或 P12 network behavior parity。执行后的产物 SHA-256：

| Artifact | SHA-256 |
|---|---|
| `Terraria.NpcDamageCombatVerification.dll` | `A1C40810F55BAD15931B91625B8805D8E9B31B2B19FB4E2008803EB8E0698F3F` |
| `Terraria.Npc.dll` | `CF7F931CD67EFD74D989E84AAF39BB5373917BE87D56CF1CC221DD0942DA13A5` |

本轮源码交叉核对覆盖完整参考树 `D:\TRbackup\无任何删减通过编译` 与权威 Version4：
两者 packet 28 `SendData` recipient predicate 相同；完整参考的 `SendData` 入口另有
`netMode == 0` 早退，未移植。Version4 `SendPacket` (`NetMessage.cs:1831-1859`) 对每 client 捕获
发送异常并写 handshake log 后继续；完整参考 (`:1861-1885`) 捕获后继续但无同类 log。完整参考
`NPC.GetHurtByDebuff` (`NPC.cs:93683` 起) 对 tracker/forced strike 使用 `netMode != 1` gate，
packet 28 使用 `netMode == 2` gate；当前 Version4 (`NPC.cs:78077` 起) 无这些 gate。完整参考
packet 28 reader (`MessageBuffer.cs:1819` 起) 仅在 server mode 内标记 interaction 并 rebroadcast/
发送后续 packet 23；strike 分支本身仍执行，owner 在 server mode 下取 `whoAmI`，否则取 255。
当前 Version4 (`MessageBuffer.cs:1406` 起) 对应分支没有这些 mode gate。这些差异只作 reference-only 记录；
目标 mode/authority policy 仍为 partial/unknown，不能从完整参考树推断当前行为。

只读 CPG Query API 对 `NetMessage.SendPacket` 的 symbol 和选定 `Terraria/NetMessage.cs` shard
调用点查询为 complete、8 个静态调用点、无 query gap。查询不按 packet id 筛选；CPG
`SourceSnapshotId` 为 null，因此不证明完整运行时调用闭包或绑定到当前源码 hash。目标
`src/NSSLC` 全树中目前只发现 packet 28 port/adapter 定义，没有真实 socket/runtime port
实现，也没有已证实的 recipient snapshot 来源；这些仍标 unknown。

该代码切片使用 SDK 10.0.400 的仓库串行 wrapper 构建受影响 verifier 项目及依赖，并运行同一
10% smoke：build exit code 0、0 warnings、0 errors；run exit code 0，输出
`PASS: P12 core combat, life, tracker and death smoke (10% scope)`。完整 P12 行为验收仍 not-run；
权威 outputReport 未修改，继续为 `designStatus: proposed`、`verificationStatus: not-run`、
`migrationStatus: not-claimed`。当前 execution 状态仍为 `partial`，不称迁移成功。

### 2026-10-01 composite tracker identity

Version4 `NPCDamageTracker.CreateTrackerFor` (`Terraria.GameContent/NPCDamageTracker.cs:164-176`)
把当前 NPC type 和 `CustomDefinition` 传给 `BossDamageTracker`；构造函数
(`Terraria.GameContent/BossDamageTracker.cs:27-34`) 在存在 composite definition 时将 tracker type
规范为 `NPCTypes[0]`。完整参考树对应方法保持相同行为，作为交叉对照，不覆盖 Version4。
本次目标策略接口增加 `TrackerNpcType`：单类型策略返回其 root type，composite strategy 保留
definition 顺序并返回首个 type。tracker 与 snapshot 分别保存首次实际受击的 `InitialNpcType`
和规范 `TrackerNpcType`，因此先受击的 segment type 14 不会被重写成 type 13，同时 tracker
仍以 composite definition 首项 type 13 表示其规范身份。

只读 CPG Query API 对 Version4 `CreateTrackerFor` 符号查询为 complete，选定
`NPCDamageTracker.cs` shard 内有 1 个静态调用点；回读该源码确认 `AddDamage` 仅在找不到现有
tracker 时创建 tracker。对静态 `AddDamage(NPC,int,int)` 在选定 NPC、MessageBuffer 和 tracker
shard 的调用点查询为 complete，返回 2 个 `NPC.cs` 调用点且无 query gap。数据库 manifest 为
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId` 为 null；
constructor 的 `NPCTypes[0]` 归一化仍以 Version4 源码为行为证据，查询结果不绑定源码快照。

核心 smoke 增加 segment type 14 先受击的用例，断言 active snapshot 同时满足
`InitialNpcType == 14` 与 `TrackerNpcType == 13`；原 boss 50 与 mob 1 tracker 归并断言继续通过。

受影响项目构建通过仓库串行 wrapper 和 SDK 10.0.400 执行：

```powershell
$p12SdkBin = Join-Path (Get-Location).Path 'Build\dotnet-sdk-10.0.400'
$env:PATH = "$p12SdkBin;$env:PATH"
$p12BuildArguments = [string[]]@(
  'build', '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments $p12BuildArguments
```

Build exit code 0，0 warnings、0 errors。Artifact 路径和 SHA-256：

| Artifact | SHA-256 |
|---|---|
| `D:\TRbackup\NLTX\Build\bin\Terraria.NpcDamageCombatVerification\Debug\net10.0\Terraria.NpcDamageCombatVerification.dll` | `8566D470EFC0198D8697B2C161E79BA52CE532FB8F3885239D66ED698CC135F7` |
| `D:\TRbackup\NLTX\Build\bin\Terraria.Npc\Debug\net10.0\Terraria.Npc.dll` | `7027650ACB0F964090B187EE45DE6D85588F20FF140BFA120D4E828441C5AD5F` |

随后以 `run --project .\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj
--no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false
-p:BuildInParallel=false` 运行同一个核心 smoke，exit code 0，输出
`PASS: P12 core combat, life, tracker and death smoke (10% scope)`。未运行其他测试项目或完整 P12
验收；权威 outputReport 未修改，继续为 `designStatus: proposed`、`verificationStatus: not-run`、
`migrationStatus: not-claimed`，整体执行仍为 partial。

### 2026-10-01 parent/segment tracker input routing

静态复核发现普通 damage 与 DoT 虽由 parent health owner 提交，却把 `lifeOwnerType` 同时用作
tracker group 查找键和首次 NPC type，导致 parent/segment 路径丢失实际命中的 segment type。
`NpcDamageTrackingSystem.TryRecordAppliedDamage` 保留原单 type overload，并增加显式
`trackerNpcType` / `initialNpcType` overload。`NpcCombatSystem` 两个入口继续以 life-owner type
选择现有 tracker group，同时传入实际受击 `npcType` 供新 tracker snapshot 保存；boss-mob 仍按 parent
boss group 查找，composite segment 14 首先受击时应同时保留 `InitialNpcType == 14` 和
`TrackerNpcType == 13`。

依据：Version4 `NPC.StrikeNPC` 将 `this` 传给 `NPCDamageTracker.AddDamage`
(`Terraria/NPC.cs:67628`)；tracker 随后在 `GetRealActiveNPC` 中把 `realLife` 子段解析为 parent
(`Terraria.GameContent/NPCDamageTracker.cs:129-136`)，并以 parent type 创建或查找 tracker。
只读 CPG Query API 对 `AddDamage(NPC,int,int)` 的选定 shard 查询返回 2 个 `NPC.cs` call sites，
状态 complete、无 query gap；source snapshot ID 为 null，因此参数含义和 parent 解析仍以逐行源码为准。
完整参考树保留相同 parent normalization；其 `AddDamageToLastAttack` 分段死亡 credit 在当前
Version4 搜索范围没有对应源引用，继续保持 reference-only/unknown，本次不移植。

本次使用仓库串行 wrapper 与 SDK 10.0.400 构建唯一受影响的核心 verifier 项目：

~~~powershell
$env:PATH = 'D:\TRbackup\dotnet-sdk-10.0.400;' + $env:PATH
$p12BuildArguments = [string[]]@(
  'build',
  '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments $p12BuildArguments
~~~

Build exit code 0，0 warnings、0 errors；artifact 位于
`D:\TRbackup\NLTX\Build\bin\Terraria.NpcDamageCombatVerification\Debug\net10.0\Terraria.NpcDamageCombatVerification.dll`，
SHA-256 为 `C9A44DB0FD99C98E449270453C43AB18A04C4B7D98FC4C35E68872B3D31554A0`；
`Terraria.Npc.dll` SHA-256 为 `DB14137931C3380D52E1813AA6DD0422CABB5D39F37A3404109F4B931858FAF6`。

随后用 `run --project .\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj
--no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false
-p:BuildInParallel=false` 执行唯一的 10% 核心 smoke，exit code 0，输出
`PASS: P12 core combat, life, tracker and death smoke (10% scope)`。本次只覆盖核心 smoke；完整 P12
行为验收仍为 not-run。当前执行状态仍为 `executionStatus: partial`；P12 权威 outputReport 未修改，
保持 `designStatus: proposed`、`verificationStatus: not-run`、`migrationStatus: not-claimed`。

### 2026-10-01 packet API declaration-only scope correction

按用户补充的范围，NPC packet 23/28 API 只保留函数声明。新增
`INpcReplicationPacketApi`，包含 decode、encode、recipient selection 与 publish 的声明，以及没有
字段的嵌套 request/result 类型。移除目标树中先前的 `NpcPacket28Codec`、recipient policy、broadcast
adapter、transport adapter、payload/snapshot DTO 和独立的 per-recipient transport-port 声明；
当前没有 packet parser、writer、recipient 筛选、发送、授权或 apply 函数体。Combat 仍产出本地 `NpcDeathPacket28Intent`，
它只表示待发布意图，不表示网络 API 已执行。

核心 verifier 移除了依赖上述具体网络算法的编码、解码、recipient 和发送失败断言，保留 Combat
产生 parent death intent 的断言。按用户限定，仅构建并运行该核心 verifier：

~~~powershell
$p12SdkBin = Join-Path (Get-Location).Path 'Build\dotnet-sdk-10.0.400'
$env:PATH = "$p12SdkBin;$env:PATH"
$p12BuildArguments = [string[]]@(
  'build', '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments $p12BuildArguments
~~~

Build exit code 0，0 warnings、0 errors。Artifact：
`D:\TRbackup\NLTX\Build\bin\Terraria.NpcDamageCombatVerification\Debug\net10.0\Terraria.NpcDamageCombatVerification.dll`，
SHA-256 `4DCB5D06113637835C0E20F25612D9EB6BBD0739D5524E4EDA93142FBE9BAF76`；其
`D:\TRbackup\NLTX\Build\bin\Terraria.Npc\Debug\net10.0\Terraria.Npc.dll` SHA-256 为
`0AE9025E97C00AD83A949DEC397716885D927C33B79620393A17D10C4626FA96`。

随后使用已构建产物运行同一个 verifier：

~~~powershell
$p12SdkBin = Join-Path (Get-Location).Path 'Build\dotnet-sdk-10.0.400'
$env:PATH = "$p12SdkBin;$env:PATH"
$p12VerifyArguments = [string[]]@(
  'run', '--project', '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments $p12VerifyArguments
~~~

Run exit code 0，输出 `PASS: P12 combat, life, tracker and death core smoke`。只运行这一项 10% 核心
smoke；packet API 协议行为和完整 P12 仍 not-run。目标设计/执行文档记录本地 `core-smoke-passed`，
但权威 `outputReport` 继续为 `verificationStatus: not-run`、`migrationStatus: not-claimed`；原 sessionId
`64850edbdb464104ba04d413bfc362e1` 未改动，也未重新 claim 或结算。

### 2026-10-01 普通 immortal Strike 行为校正

Version4 `D:\TRbackup\Version4\Terraria\NPC.cs` 的 `StrikeNPC` 在第 67623 行以
`if (!immortal)` 包围 `NPCDamageTracker.AddDamage` 和 `realLife`/本体 life 写入；其后 knockback
分支不在 immortal 条件内。对应文件 SHA-256：
`29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`。只读 CPG Query API 对
`StrikeNPC(double(int,float,int,bool,bool,bool,int))` 的符号查询为 `complete`，在选定的
`NPC.cs`、`Player.cs`、`Projectile.cs`、`MessageBuffer.cs` 范围找到 5 个直接静态调用点，无 gap；
manifest SHA-256 为 `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，
`SourceSnapshotId=null`，因此调用关系只作为选定索引范围内的结构证据。

完整参考树 `D:\TRbackup\无任何删减通过编译\Terraria\NPC.cs` 同样以 immortal 条件包住 tracker 和
life 写入，SHA-256 为 `ED8AA2302730A9E046EF310E543204FBA391BD35B1C9C0B213C8065DFBBE39F0`；它的 tracker
调用另有 `Main.netMode != 1` 条件。该 mode gate 属 reference-only，本次以 Version4 为行为基线，未移植。

目标 `NpcCombatSystem.Validate` 不再把 immortal 本身作为拒绝原因。有效 Strike 返回 accepted
结果，`AppliedDamage=0`、life 不变、`TrackerRecorded=false`；immortal 分支跳过 parent health commit
和 child mirror，同时保留后续 death reconciliation。独立 parent/segment 核心断言检查 parent 和
segment 的 life/lifeMax 均不变，且没有创建 tracker。knockback、justHit、AI reaction、HitEffect 和
完整 death effects 没有被此 System/验证器覆盖，仍为 partial/unknown。DoT 的 immortal 语义保持独立，
仍会记录 capped world tracker credit 并生成 CombatText intent。

按仓库串行 wrapper 仅构建受影响的核心 verifier：

~~~powershell
$p12SdkBin = Join-Path (Get-Location).Path 'Build\dotnet-sdk-10.0.400'
$env:PATH = "$p12SdkBin;$env:PATH"
$p12BuildArguments = [string[]]@(
  'build', '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments $p12BuildArguments
~~~

Build exit code 0，0 warnings、0 errors；verifier artifact 位于
`D:\TRbackup\NLTX\Build\bin\Terraria.NpcDamageCombatVerification\Debug\net10.0\Terraria.NpcDamageCombatVerification.dll`，
大小 22528 bytes，SHA-256 为 `12F5531214039D6B8A0C65EA81ADF968E7678CB238F67A5B6000789B4C56E531`；
`Terraria.Npc.dll` SHA-256 为 `76379F6ACAA146E4661D1BA2F421D5EF948317C1E7D48F7F911A14628C830EE6`。

随后使用同一 wrapper 和已构建产物运行单项核心 smoke：

~~~powershell
$p12VerifyArguments = [string[]]@(
  'run', '--project', '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments $p12VerifyArguments
~~~

Run exit code 0，输出 `PASS: P12 combat, life, tracker and death core smoke`。本次只运行这一项核心
verifier；packet API 协议、真实 knockback/presentation/death effects、全部 189 成员和完整 P12
仍未验证。executionStatus 仍为 `partial`；权威 outputReport 未修改，保持 `designStatus: proposed`、
`verificationStatus: not-run`、`migrationStatus: not-claimed`；未重新 claim 或结算原 session。

### 2026-10-01 knockback 纯计算切片

按 Version4 `D:\TRbackup\Version4\Terraria\NPC.cs:67641` 增加 `NpcMovementSystem.CalculateKnockback`
及其 request/result。输入包含 NPC type、命中时速度、knockback、hit direction、resistance、`onFire2`、
crit、resolved damage、life maximum、expert mode 与 `noGravity`；结果仅返回 eligible 与速度前后值。
代码实现依次应用抗性、燃烧倍率、8/10/12/14 阈值阻尼、16 截断和 crit 倍率；随后按普通/专家伤害阈值
选择速度重设或高伤害限幅，并在高伤害 type 185 路径应用额外垂直冲量。

源码关系由只读 CPG Query API 和行级源码共同核对：`StrikeNPC` 精确签名的 symbol 查询 complete；在选定的
`NPC.cs`、`MessageBuffer.cs`、`Player.cs`、`Projectile.cs` 查询到 5 个直接静态调用点，无 query gap。
`Get-CpgCallableFacts` 为 partial，存在 callee effects 未展开的 gap；CPG manifest SHA-256 为
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`。
因此公式与顺序由当前 Version4 源码 `NPC.cs:67641` 起确认，查询结果只确认所选范围的调用边，不证明运行时闭包。
完整参考树 `D:\TRbackup\无任何删减通过编译\Terraria\NPC.cs:82668` 对照片段中的公式顺序和 type 185
分支一致；参考树只读对照，未在本切片构建。

Version4 的相邻源码还确认顺序：`justHit=true` 在 `NPC.cs:67525`，tracker/生命提交在
`:67623-67640`，knockback 在 `:67641` 起，HitEffect/音效在 `:67749` 起，父体或本体 `checkDead`
在 `:67816-67824`。目标 `NpcCombatSystem.ResolveAndCommit` 当前在返回前调用 death reconcile；因此把
`CalculateKnockback` 简单放在其返回后执行，会晚于 death reconcile，改变 lethal hit 的可观察顺序。
这条路径需要先有协调提交序列，至少保证 life/tracker -> Movement velocity commit -> hit effects ->
death reconcile；`justHit` 的写入和 UpdateNPC 清理时点也未迁移。

核心 verifier 源码现有四个 knockback 纯计算断言：低伤害时速度重设为 `(2.5, -1.875)`；阈值等于 life maximum
时走低伤害分支并得到 `(-9.8, -7.35)`；专家模式高伤害、燃烧、crit、type 185 与 noGravity 组合会限幅
水平速度并产生对应垂直冲量；零抗性时保持当前速度。本小节记录时，Movement 只有纯计算，还没有
`NpcCombatSystem` 调用边或 velocity commit；`ResolveAndCommit` 在返回前完成 death reconciliation，
所以不能在该 API 返回后直接添加 knockback。紧随其后的 strike velocity commit 小节记录了后来增加的
候选协调序列。完整 Movement owner/生产接线仍为 `integration-review`，AI reaction、HitEffect 和网络投影
也未验证。

### 2026-10-01 knockback threshold-equality smoke

新增边界输入：type 185、knockback 10、resistance 1、damage 10、life maximum 100、非 expert、重力开启。
初始幅度按 `> 8` 阻尼成为 9.8；damage threshold 等于 life maximum，结果为低伤害分支速度 `(-9.8, -7.35)`。
前两次 smoke run 都因测试预期遗漏 `> 8` 阻尼而失败；按 Version4 公式修正期望后，最终构建和 smoke
通过。该串行构建命令在修正过程中执行三次，均 exit code 0、0 warnings、0 errors；最终产物为：

~~~powershell
$p12SdkBin = Join-Path (Get-Location).Path 'Build\dotnet-sdk-10.0.400'
$env:PATH = "$p12SdkBin;$env:PATH"
$p12BuildArguments = [string[]]@(
  'build', '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments $p12BuildArguments
~~~

最终 build exit code 0，0 warnings、0 errors。Verifier artifact 位于
`D:\TRbackup\NLTX\Build\bin\Terraria.NpcDamageCombatVerification\Debug\net10.0\Terraria.NpcDamageCombatVerification.dll`，
SHA-256 `A16F6792B0AA27845A1DF1AE6CBE8F1DEF53EEEDE36F532196E73C43F709D4CC`；NPC library SHA-256 为
`3C881DB888CC0B1BD7B6A24A26CB75F272DAB9C3B201623AADE486F1E8CA6F73`。随后串行运行同一 project：

~~~powershell
$p12SdkBin = Join-Path (Get-Location).Path 'Build\dotnet-sdk-10.0.400'
$env:PATH = "$p12SdkBin;$env:PATH"
$p12VerifyArguments = [string[]]@(
  'run', '--project', '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments $p12VerifyArguments
~~~

最终 run exit code 0，输出 `PASS: P12 combat, life, tracker and death core smoke`。本次只构建并运行这一个
10% 核心 verifier，没有运行其他项目或完整 P12 验收。packet API 仍为声明-only，完整 P12 仍 `not-run`；
executionStatus 保持 `partial`，权威 outputReport 保持 `designStatus: proposed`、`verificationStatus: not-run`、
`migrationStatus: not-claimed`；原 sessionId `64850edbdb464104ba04d413bfc362e1` 没有重新 claim、编辑或结算。

### 2026-10-01 strike knockback velocity commit

在纯计算之后增加 `NpcMovementSystem.ApplyKnockback`，只在结果 eligible 时把 `VelocityAfter` 写入
`MovementStateComponent.Velocity`。新增 `NpcCombatSystem.ResolveAndCommitStrike`，顺序固定为：accepted
damage 与 tracker commit -> Movement knockback velocity commit -> death reconciliation。DoT 继续走原来的
`ResolveAndCommit` 路径。`INpcReplicationPacketApi` 继续只有函数声明和空 request/result 类型；未加入解析、
编码、recipient 选择、传输或 authority apply 逻辑。

核心 verifier 增加 strike 集成断言，检查 resolved damage、knockback eligibility、velocity delta 和未触发
death transition。此前以精确向量相等比较运行的产物在综合断言失败；输出没有各子谓词值，失败的具体子条件
留为 `unknown`。将向量比较改为 0.001 容差并重建后，最新产物 smoke 通过。该断言仅证明本地候选编排，
不证明生产 caller 或完整 legacy 观察等价。

构建前通过 `C:\Users\shan\.dotnet\dotnet.exe --version` 确认仓库 `global.json` 要求的 SDK `10.0.400`；
系统级 PATH 只含 `10.0.100`，因此仅对本次 shell 临时把 `C:\Users\shan\.dotnet` 前置于 PATH，未改仓库
SDK 配置。串行构建的实际命令与结果：

~~~powershell
$env:PATH = 'C:\Users\shan\.dotnet;' + $env:PATH
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'build', '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
~~~

Build exit code 0，14 warnings、0 errors；告警为 `Terraria.WorldStorage` 中未赋值/未使用字段。
产物位于 `D:\TRbackup\NLTX\Build\bin\Terraria.NpcDamageCombatVerification\Debug\net10.0\Terraria.NpcDamageCombatVerification.dll`，
SHA-256 为 `A19667C6376139187D3A3BA5978C48F6D605C3BC54526C4B56CAF6376F2D2AC3`；`Terraria.Npc.dll` 的
SHA-256 为 `56A5EF358AA208C45B5D35CB5F9F22B9BF999D84D0D6370074D7333B73FFB42A`。

随后通过相同串行入口运行唯一的核心 verifier，不触发重建：

~~~powershell
$env:PATH = 'C:\Users\shan\.dotnet;' + $env:PATH
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'run', '--no-build', '--no-restore', '--project',
  '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj')
~~~

Run exit code 0，输出 `PASS: P12 combat, life, tracker and death core smoke`。未构建完整参考树或运行完整
P12 验收；packet API 协议和生产网络闭包未验证。当前 execution 仍 `partial`；权威 outputReport 仍为
`designStatus: proposed`、`verificationStatus: not-run`、`migrationStatus: not-claimed`，原 sessionId
`64850edbdb464104ba04d413bfc362e1` 未重新 claim、修改或结算。

### 2026-10-01 `checkDead` guard 与 inactive PendingDespawn 修正

对照 Version4 `D:\TRbackup\Version4\Terraria\NPC.cs:64571-64578`，`checkDead` 在 NPC inactive、当前
NPC 是 child segment（`realLife >= 0 && realLife != whoAmI`）或 `life > 0` 时立即返回。完整参考树对应
入口 `D:\TRbackup\无任何删减通过编译\Terraria\NPC.cs:79209-79216` 有相同 guard；此一致性只适用于入口条件，
参考树后续 phase 和网络分支仍不作为 Version4 行为。

只读 CPG Query API 结果：`checkDead:void()` symbol 为 `complete`，ID
`node-83386e93d361e4ad3d4d8d2525a1c7228229cb0978db62c74fc688413b93b808`；在选定的 `NPC.cs`、`Main.cs`、
`MessageBuffer.cs` 范围找到 9 个静态 call sites，均位于 `NPC.cs`，无 query gap。`Get-CpgCallableFacts` 为
`partial`，包含 103 个 operation nodes，gap=`CalleeEffectsNotExpanded`。CPG manifest SHA-256 为
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`；结果不闭合
动态调用、phase 顺序或副作用。

`NpcDeathLifecycleSystem.Reconcile` 现将已 `Despawned` 视为幂等终态；其余状态只在 health dead 且 lifecycle
active 时提交 despawn。验证器增加 inactive `PendingDespawn` 输入，断言状态保持不变且结果不是 terminal、
transitioned 或 already-terminal。该改动只校正候选终态提交的入口 guard，不实现完整 `checkDead` phase、
parent/segment 清理、loot/progression/network effects。

按仓库串行 wrapper 构建唯一受影响的 `Terraria.NpcDamageCombatVerification` 项目并运行同一个核心 verifier；
使用已安装的 .NET SDK `10.0.400`。实际 build 命令：

~~~powershell
$env:PATH = 'C:\Users\shan\.dotnet;' + $env:PATH
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'build', '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
~~~

build exit code 0，0 warnings、0 errors；verifier 产物位于
`D:\TRbackup\NLTX\Build\bin\Terraria.NpcDamageCombatVerification\Debug\net10.0\Terraria.NpcDamageCombatVerification.dll`。

使用已构建产物执行唯一核心 smoke 的实际命令：

~~~powershell
$env:PATH = 'C:\Users\shan\.dotnet;' + $env:PATH
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'run', '--project', '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
~~~

run exit code 0，输出 `PASS: P12 combat, life, tracker and death core smoke`。只覆盖约 10% 核心 smoke；packet API
协议、完整死亡效果和其余 P12 行为仍未运行。executionStatus 保持 `partial`，权威 outputReport 继续为
`designStatus: proposed`、`verificationStatus: not-run`、`migrationStatus: not-claimed`；原 sessionId
`64850edbdb464104ba04d413bfc362e1` 未重新 claim、编辑或结算。

### 2026-10-01 type-aware checkDead phase 子集与核心 smoke

`NpcDeathPhaseQuery` 接收显式的 NPC type、active/life-owner 条件、life、AI 和 center 输入；
`NpcDeathLifecycleSystem.Reconcile` 返回 AI after-state、恢复生命与伤害策略决策，并携带 replication-sync 请求、
type 400 spawn intent，或 type 35 announcement / type 604/605 `LadyBugKilled` 前置效果 intent。它不会直接写 AI、
health、damage-policy、world 或 player state。type 35/604/605 的结果会保持 lifecycle active 并设置
`TerminalCommitPending`，调用方处理前置效果后再单独提交 terminal transition；这条消费顺序尚无生产协调者。
当前 smoke 覆盖四组 phase、type 396/397 重入、entry guard 和上述早期效果 intent/terminal defer。
spawn/effect intent 尚无 adapter/owner consumer；phase Query 已有显式 Combat API 接入，但仍无生产调用方；
GoodWorld 后续分支、AI 单写者、parent/segment 清理及完整 death effects 仍 partial/unknown。

本轮只读 CPG Query API 结果：`Find-CpgSymbols(checkDead)` 为 complete，选定 `NPC.cs` shard 有 1 个符号；
`Find-CpgCallSites` 在选定 `NPC.cs`、`Main.cs`、`MessageBuffer.cs` 范围为 complete，9 个静态调用点均位于
`NPC.cs`，无 query gap。`NewNPC` 符号查询和 `NPC.cs` shard 调用点查询为 complete，96 个静态调用点；
`dontTakeDamage` 与 `dontTakeDamageFromHostiles` 在 `NPC.cs` shard 分别有 84 与 8 个成员使用事实。
这些查询只说明所选索引范围内的结构关系，不闭合动态调用、别名、写者或副作用。
`Get-CpgCallableFacts(checkDead)` 仍为 partial：103 个 operation nodes，gap=`CalleeEffectsNotExpanded`。
CPG manifest SHA-256 为 `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，
`SourceSnapshotId=null`；phase 行为以 Version4 `NPC.cs:64575-64627` 的源码控制流为依据。

同一只读 Query API 查询中，`LadyBugKilled` 符号和 `NPC.cs` shard 的调用点均为 complete，各 1 项；
`StingerExplosion` 符号和调用点也均为 complete，各 1 项。源码复核显示 `LadyBugKilled`
(`NPC.cs:67830` 起) 先增加并限制 `Main.ladyBugRainBoost`，再按本地玩家 active/dead 和与 NPC 距离修改
`ladyBugLuckTimeLeft` / `luckNeedsSync`；这些是 world 与 Player owner 状态，只返回调用意图。`StingerExplosion`
(`NPC.cs:42870`) 函数体为空，按 unknown 处理。入口后续 GoodWorld 分支还包含 type 13 spawn 与 type 36
随机 tile search/spawn；tile、random、spawn 和 packet 23 顺序没有移植到本切片。

按串行构建约束，只构建受影响的 verifier 项目及项目引用：

~~~powershell
$env:PATH = 'C:\Users\shan\.dotnet;' + $env:PATH
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'build', '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
~~~

build exit code 0，0 warnings、0 errors；verifier 产物位于
`D:\TRbackup\NLTX\Build\bin\Terraria.NpcDamageCombatVerification\Debug\net10.0\Terraria.NpcDamageCombatVerification.dll`，
SHA-256 `635550497348994FBE044BBB4388CFEEF5E00F216FF3E002E69665488034CC6B`。依赖产物
`Terraria.Npc.dll` 的 SHA-256 为 `A02BD674F3023C515891AC088AD83FCFC6C3C300037DDCE34CCDC6C26B347E1B`。

随后只运行这一个 10% 核心 verifier，使用已构建产物：

~~~powershell
$env:PATH = 'C:\Users\shan\.dotnet;' + $env:PATH
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'run', '--project', '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
~~~

run exit code 0，输出 `PASS: P12 combat, life, tracker and death core smoke`。未构建完整参考树，
未运行完整 P12 或 packet 行为测试。当前目标文档的局部状态为 `core-smoke-passed`、
`executionStatus: partial`；权威 outputReport 未修改，仍为 `designStatus: proposed`、
`verificationStatus: not-run`、`migrationStatus: not-claimed`；原 sessionId
`64850edbdb464104ba04d413bfc362e1` 未重新 claim 或结算。

### 2026-10-01 GoodWorld type13/36 death intents

Version4 `Terraria/NPC.cs:64633-64660` 在 `Main.getGoodWorld` 下为 type 13 请求固定坐标的 type `-12`
spawn；type 36 最多执行 3 轮地表搜索，每轮最多 1000 个随机候选，随机 x/y 偏移范围各为 `[-50, 50]`
tile，搜索边界为 `Main.maxTilesY - 200`，成功时请求 type 32 spawn。新实体有效时两条分支都会请求
packet 23 同步。完整参考树 `D:\TRbackup\无任何删减通过编译\Terraria\NPC.cs:79272-79305`
另有 `Main.netMode != 1` gate，而 Version4 没有；本次按 Version4 主源码，不移植参考树的 gate。

目标实现新增 `NpcDeathWorldEffectIntent`。type 13 使用显式可空 `BottomY`；缺少该输入时标记
`RequiredInputMissing` 并阻止 terminal commit。有输入时返回 type `-12` 固定坐标 spawn intent。type 36 返回
type `32` 的 surface-search intent，携带 spawn 上限 3、随机偏移半径 50、每轮候选上限 1000 和底部边距
200。含 world-effect intent 的 death result 保持 `TerminalCommitPending`，等待外部 owner 处理后由调用方调用
`CommitAfterPreTerminalEffects` 提交 terminal lifecycle。该方法拒绝非-pending 结果，但不执行或确认外部效果。

这里没有实现随机数、tile 查询、spawn、packet 23 writer/transport 或消费重试。Version4 的
`StingerExplosion` (`NPC.cs:42870`) 仍为空，按 `unknown` 处理；完整参考树的非空方法体没有并入目标行为。
`INpcReplicationPacketApi` 与 `INpcSpawnSyncPacketPort` 仍只含函数声明，空 request/result DTO 也不含协议字段。

只读 CPG Query API 使用 manifest SHA-256
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`、`SourceSnapshotId=null`。在
`Terraria/NPC.cs` shard 中，`checkDead` 符号查询 complete；其 callable facts 为 partial、103 个操作节点、
1 个 gap。`NewNPC` 调用点查询 complete、96 个 NPC shard 调用点、无 query gap。packet 复核中，
`NetMessage.SendData` 与 `TrySendData` 的符号查询 complete；在选定的 NPC/MessageBuffer shards 分别查询到
89/125 个静态调用点、无 query gap，调用点不按 packet id 或运行时接收者筛选。`MessageBuffer.GetData`
callable facts partial、有 1 个 gap。所有计数仅描述所选 CPG shard；manifest 没有 source snapshot binding，
不能证明完整运行时调用/效果闭包。

本切片构建命令（工作目录 `D:\TRbackup\NLTX`）：

~~~powershell
$env:PATH = 'C:\Users\shan\.dotnet;' + $env:PATH
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'build', '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
~~~

build exit code 0，0 warnings、0 errors；产物为
`Build/bin/Terraria.NpcDamageCombatVerification/Debug/net10.0/Terraria.NpcDamageCombatVerification.dll`
（SHA-256 `110F057A21EFA1A30D3666DDE9DDDAAC2B85F83EEE0A91A798F0C2605B01D7A0`）和
`Build/bin/Terraria.Npc/Debug/net10.0/Terraria.Npc.dll`（SHA-256
`9D5F0BFD37290E57FB53DCC34F2E8CA7503DF96EC2E81940C6E4AC2D332B33CB`）。

只运行同一个 10% 核心 verifier，使用已构建产物：

~~~powershell
$env:PATH = 'C:\Users\shan\.dotnet;' + $env:PATH
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'run', '--project', '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
~~~

run exit code 0，输出 `PASS: P12 combat, life, tracker and death core smoke`。测试只确认 intent 参数、
Query 和 Lifecycle 层的缺输入保护、pending result、active lifecycle、显式提交和非-pending 拒绝提交；
不执行 packet/API 行为，也不证明生产接线或整个 P12 的语义等价。整体仍为
`executionStatus: partial`；设计保持 `proposed`，完整 P12 验证仍为 `not-run`，不称迁移成功。
P12 权威 outputReport 不修改；runner 中原 `sessionId=64850edbdb464104ba04d413bfc362e1` 已是
`completed`，未重复 claim 或再次结算。

### 2026-10-01 UpdateNPC status/DoT 调度关系审查

本轮只补静态证据与切片决定，没有新增生产代码。Version4 `NPC.UpdateNPC` 的直接静态调用点在
`Main.cs:11511` 和 `Main.cs:11524`。其 `NPC.cs:76738-76742` 顺序为重置 buff flags、从 buff slot 重建
flags、清理过期 buff、累积 DOT/regen 并应用周期伤害；随后 `:76750-76754` 才在 `life <= 0` 时清除
`active`、调用 `UpdateNetworkCode`、清除 `netUpdate`/`justHit` 并提前返回。不能把 death/status 排序
压缩成“每 tick 先提交死亡再处理状态”。

只读 CPG Query API 使用 `D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite`，manifest SHA-256
为 `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`。
对 `UpdateNPC`、`UpdateNPC_BuffFlagsReset`、`UpdateNPC_BuffSetFlags`、
`UpdateNPC_BuffClearExpiredBuffs`、`UpdateNPC_BuffApplyDOTs` 和 `GetHurtByDebuff` 的符号查询均为
`complete`；选定 `NPC.cs`/`Main.cs` shard 内调用点查询均为 `complete`、无 query gap，调用点数依次为
2、1、1、1、1、2。六项 `Get-CpgCallableFacts` 均为 `partial` 且各有一个 gap，操作节点数依次为
114、149、22、26、43、27；callee effects、动态分派和调用闭包未由此查询证明。

字段查询在选定 `NPC.cs` shard 中为 `complete`：`lifeRegen` 69 uses、`lifeRegenCount` 8、`poisoned` 5、
`dontTakeDamage` 84、`buffTime` 15。访问事实分别包含 `ReadWrite`、`Write` 和 `Unknown`；例如
`buffTime` 的 15 项全部为 `Unknown`，而 `lifeRegen` 有 19 项为 `Unknown`。这些是成员使用点，不是
writer 闭包或唯一权威 owner 证明。CPG manifest 未绑定源码快照，当前 Version4 `NPC.cs` 的独立读取
SHA-256 为 `29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`；本轮结论以该文件的
源码复核为准。

目标树已有共享 `StatusEffectSlotsComponent` 和 `HealthRegenerationStateComponent`，但没有
`NpcStatusSystem.cs`、NPC 状态快照输入或状态到 DoT 的生产接线；`NpcCombatSystem.ApplyDamageOverTime`
接收的是已计算出的伤害量。Version4 的 DOT 聚合还读取多种 buff flags、AI、world/boss 条件，扫描
projectile slot，并直接累积 `lifeRegen`/`lifeRegenCount`；其中 `ApplyEelWhipDoT` 在 Version4 为清空体
(`NPC.cs:78076`)，其效果是 `unknown`。因此本切片不创建只覆盖若干状态的 `NpcStatusSystem`，也不把
尚未闭合的状态输入猜成组件契约。

完整参考树 `D:\TRbackup\无任何删减通过编译\Terraria\NPC.cs` 的 SHA-256 为
`ED8AA2302730A9E046EF310E543204FBA391BD35B1C9C0B213C8065DFBBE39F0`。其 `UpdateNPC` 仍有 reset/expire/DoT
次序，但另有 mode、unsynced-tile 等 gate；参考版 `ApplyEelWhipDoT` 有非空效果并增加
`GetHurtByDebuff` 调用 (`NPC.cs:93675`)。这些差异只作为对照，不补入 Version4 基线。Version4
`NpcCombatSystem.ReconcileDeath` 在收到 `NpcDeathPhaseContext` 时调用 type-aware lifecycle overload，并将
`NpcDeathLifecycleResult` 暴露在 `NpcCombatResult`；未提供 context 时仍调用通用 lifecycle overload。DoT 的
lethal forced strike 会转交同一 context。本目标树直接调用 `NpcCombatSystem` 的位置仍仅在核心 verifier 中，
尚无生产 UpdateNPC/status coordinator；phase decision 的消费和 NPC 状态 owner 继续为 `partial/unknown`。

本轮未改 packet API：`INpcReplicationPacketApi` 与 `INpcSpawnSyncPacketPort` 仍只有函数声明，DTO
保持不透明，未添加编解码、recipient 选择、transport、授权或 apply 逻辑。未运行构建或测试；上一节
记录的 10% 核心 smoke 结果仅对应之前的代码切片，本轮静态审查不改变其范围和结论。P12 状态仍为
`executionStatus: partial`、设计 `proposed`、完整 P12 验证 `not-run`、`migrationStatus: not-claimed`；
权威 outputReport 和已完成的原 `sessionId=64850edbdb464104ba04d413bfc362e1` 均未修改或重新结算。

### 2026-10-01 Combat 到 death phase context 组合

`NpcCombatSystem.ResolveAndCommit`、`ResolveAndCommitStrike` 和 `ApplyDamageOverTime` 现在可接收可选的
`NpcDeathPhaseContext`，内容为 AI、center、life-owner 条件、GoodWorld 标记与可空 `BottomY`。有显式上下文时，
combat 使用提交后的 life、解析出的 life-owner type/lifecycle 调用 phase-aware `Reconcile`，并通过
`NpcCombatResult.DeathLifecycle` 返回 lifecycle 结果与 phase decision。DoT 致死的 forced strike 会继续传递
上下文。没有提供上下文时仍走原通用 lifecycle overload，以保持既有调用兼容；这不构成生产迁移，当前
`src/NSSLC` 还没有接入 `NpcCombatSystem` 的 UpdateNPC caller，也没有 phase decision consumer。

核心 verifier 新增 type 398 lethal hit 和 lethal DoT 两条组合断言：显式上下文应返回
`Type398EnterAi0Two` decision、保持 lifecycle active 且不报告 terminal transition。该结果只验证候选 API
组合与 forced-strike 转交，不应用 `AiAfter`、恢复生命、damage policy 或 replication/spawn intent。

构建工作目录为 `D:\TRbackup\NLTX`，通过仓库串行入口执行：

~~~powershell
$env:PATH = 'C:\Users\shan\.dotnet;' + $env:PATH
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'build', '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
~~~

build exit code 0，0 warnings、0 errors；验证器产物位于
`Build/bin/Terraria.NpcDamageCombatVerification/Debug/net10.0/Terraria.NpcDamageCombatVerification.dll`，NPC
依赖位于 `Build/bin/Terraria.Npc/Debug/net10.0/Terraria.Npc.dll`。SHA-256 分别为
`A028380C277790904471FFF0E39371D657E47EC1DDA54EF027F8AF99E22898BE` 和
`1D781BD86797902655B7BC9C29E1132DD92E888705B705DECC38828F393A2835`。

仅运行同一个 10% 核心 verifier，使用已构建产物：

~~~powershell
$env:PATH = 'C:\Users\shan\.dotnet;' + $env:PATH
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'run', '--project', '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
~~~

run exit code 0，输出 `PASS: P12 combat, life, tracker and death core smoke`。未构建完整参考树，也未运行完整
P12 验收。packet API 仍为声明-only。整体保持 `executionStatus: partial`、设计 `proposed`、完整验证
`not-run`、`migrationStatus: not-claimed`；权威 outputReport 与原 session 未修改或重新结算。

### 2026-10-01 phase health-owner restore 与 segment mirror

`NpcCombatSystem.ReconcileDeath` 现在在显式 phase context 下收到非-terminal decision 且
`RestoreLifeToMaximum=true` 时，于 combat commit 边界恢复 life-owner health。若本次命中的是 parent segment，
随后通过 `SynchronizeFromParent` 同步当前生命和最大生命；结果中的 `LifeAfter` 读回恢复后的 life owner。
该写入仅在此 decision 分支发生。type 398 的直接致死命中和 lethal DoT forced strike 继续验证恢复；新增
parent type 398 / segment hit 场景覆盖 parent 恢复、segment 镜像和两边 lifecycle active。

Version4 静态关系使用只读 CPG Query API 查询 `D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite`，
manifest SHA-256 为 `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，
`SourceSnapshotId=null`。`checkDead` 符号及在选定 `NPC.cs`/`Main.cs` shards 的调用点查询 complete，调用点 9 个、
无 query gap；其 callable facts 为 partial，103 个 operation nodes，1 个 gap。`NetMessage.SendData` 与
`TrySendData` 符号及选定 `NPC.cs`/`MessageBuffer.cs` 调用点查询 complete，分别 89/125 个调用点、无 query gap。
这些静态事实不闭合被调函数效果、运行时接收者或完整调用范围；manifest 没有源码 snapshot binding。
当前 `src/NSSLC` 搜索只找到 `INpcReplicationPacketApi` 和 `INpcSpawnSyncPacketPort` 的 interface 声明及空 DTO，
未发现实现类；packet 编解码、授权、recipient 选择、transport 和 apply 均未实现。Version4 源码的实际控制流
仍以 `D:\TRbackup\Version4\Terraria\NPC.cs` 行级复核为准。

使用仓库 SDK `10.0.400` 和串行构建入口，只构建受影响 verifier 项目及其引用：

~~~powershell
$env:PATH = 'C:\Users\shan\.dotnet;' + $env:PATH
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'build', '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
~~~

构建前确认没有活动 `dotnet`/`csc` 编译进程。相同构建命令执行两次：首次编译依赖图时 exit code 0、0 errors、
14 warnings，warning 均来自未改的 `Terraria.WorldStorage` 字段（CS0649/CS0169）；随后加入 `LifeAfter` 返回值断言后
重跑，exit code 0、0 warnings、0 errors。最终产物位于 `Build/bin/`：
`Terraria.NpcDamageCombatVerification.dll` SHA-256
`95C3D2CEDD8F45FF575A39C29A83555FEA8DF50FC292913364A45EEDA84F94CC`，
`Terraria.Npc.dll` SHA-256 `2D152F526DBA16B9E8BD16EDF85876E22CAF272E90B8F56B6DE1D038437820EB`。

仅运行该 10% 核心 verifier，使用刚构建的产物：

~~~powershell
$env:PATH = 'C:\Users\shan\.dotnet;' + $env:PATH
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'run', '--project', '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
~~~

run exit code 0，输出 `PASS: P12 combat, life, tracker and death core smoke`。断言检查 phase owner health 恢复、
parent-to-segment 镜像和返回 `LifeAfter`。此 smoke 只验证局部组合，不证明
生产调用接线、phase decision consumer、packet 行为、完整 P12 或行为等价。没有构建完整参考树。packet API 保持
声明-only。状态仍为 `executionStatus: partial`、设计 `proposed`、完整 P12 验证 `not-run`、
`migrationStatus: not-claimed`；权威 outputReport 未改，原 session
`64850edbdb464104ba04d413bfc362e1` 已完成且未重新 claim/结算。

### 2026-10-01 death phase authority consumer 与 damage policy smoke

本轮继续 P12 局部切片。`NpcBehaviorComponent.ApplyAiState` 将 decision 的四个 AI slot 提交到现有 behavior
component；新增 `NpcAuthoritySystem.CommitDeathPhaseDecision`，只接受非 terminal、缺少必需输入时不提交、且状态确有变化的
decision。该 consumer 只将 decision 明确置为 true 的 `RejectAllDamage` / `RejectHostileDamage` 写入策略 component；
decision 中的 false 表示当前 phase 不写该策略，从而保留其它 owner 已有的限制。它按 decision 标记本地
`NpcNetworkSyncIntentComponent`，没有接到生产 `UpdateNPC` caller，也不执行 packet 发送、spawn 或其它外部效果。

`NpcDamageRequest` 新增 `IsHostileDamage` 和 `IsTrapDamage` 显式输入。`NpcCombatSystem` 在普通伤害验证时接收可选的
life-owner `DamageAcceptancePolicyComponent`，执行全伤害、hostile、trap 策略拒绝，并将 policy 的 immortal 状态
并入 Strike damage commit。DoT API 不接收该 policy；Version4 的 `GetHurtByDebuff` 与普通 Strike 是独立路径，
而 `UpdateNPC_BuffApplyDOTs` 在聚合入口有 `dontTakeDamage` gate。本轮未从普通 Strike 策略推导 DoT 拦截行为。
hostile/trap 标记目前仅由 verifier 显式传入，生产输入适配未实现，来源分类保持 `unknown`。

Version4 源码复核：`NPC.checkDead` 在 `NPC.cs:64603-64627` 对 type 398、517/422/507/493 写 `dontTakeDamage`，
对 type 548 写 `dontTakeDamageFromHostiles`；`GetHurtByOtherNPCs` (`:78546-78549`) 同时检查两者。type 548 分支没有
设置全伤害拒绝。`GetHurtByDebuff` (`:78077-78099`) 直接扣 life 并调用 forced Strike；tick 聚合中的
`UpdateNPC_BuffApplyDOTs` (`:77663-77666`) 先检查 `dontTakeDamage`。完整参考树 `D:\TRbackup\无任何删减通过编译\Terraria\NPC.cs`
的 `checkDead` (`:79209` 起) 保留 type 398/517/548 policy 分支，但其 GoodWorld 分支有额外 `netMode` gate；本轮
继续以 Version4 为权威行为。参考差异不复制到目标代码。

使用只读 CPG Query API，数据库 `D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite`，manifest SHA-256
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`。`dontTakeDamage`
查询为 `complete`、84 uses、0 gaps；`dontTakeDamageFromHostiles` 为 `complete`、8 uses、0 gaps。选定 shards 内
`checkDead`/`StrikeNPC`/`GetHurtByOtherNPCs`/`UpdateNPC` call-site 查询为 `complete`，分别 9/3/2/2 个、均无 query gap。
`life`、`lifeMax`、`ai` 使用点达到 200 项预算，各为 `partial` 并带 1 gap。以上是选定静态索引范围，不闭合动态调用、
被调函数效果或完整读写者集合。

核心 verifier 增加 type 398 phase consumer、恢复后的普通伤害拒绝，type 398 保留既有 hostile 限制，以及 type 548
hostile/非-hostile 策略和保留既有全伤害/trap 限制的断言。唯一
构建项目为 `Test/Terraria.NpcDamageCombatVerification/Terraria.NpcDamageCombatVerification.csproj`，命令经仓库
串行入口执行：

~~~powershell
$env:PATH = 'C:\Users\shan\.dotnet;' + $env:PATH
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'build', '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
~~~

build exit code 0，0 warnings、0 errors。验证器产物位于
`Build/bin/Terraria.NpcDamageCombatVerification/Debug/net10.0/Terraria.NpcDamageCombatVerification.dll`，SHA-256
`32398ECB6A87CA038BD01199F6EDD263A521A501CF8594A23279001B91F55573`；`Terraria.Npc.dll` 位于
`Build/bin/Terraria.Npc/Debug/net10.0/Terraria.Npc.dll`，SHA-256
`8ADE1F41EBCA2153779DD342296C6495BBF5B9CA7233886E722785D066AFF9E7`。

只运行该 10% 核心 verifier，使用已构建产物：

~~~powershell
$env:PATH = 'C:\Users\shan\.dotnet;' + $env:PATH
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'run', '--project', '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
~~~

run exit code 0，输出 `PASS: P12 combat, life, tracker and death core smoke`。该结果验证局部声明 API 与 consumer
组合，不证明生产 `UpdateNPC` caller、phase decision writer 闭包、hostile/trap 来源适配、packet 行为、完整 P12 或行为等价。
packet API 保持函数声明与空 DTO，不含编解码、授权、recipient 选择、transport 或 apply。整体继续为
`executionStatus: partial`、`designStatus: proposed`、`fullP12Verification: not-run`、`migrationStatus: not-claimed`；
权威 outputReport 与原已完成 session `64850edbdb464104ba04d413bfc362e1` 未改动、未重新 claim/结算。

### 2026-10-01 NPC buff flags 与过期清理分阶段候选

本轮继续实现局部 `NpcStatusSystem`，新增/修正目标切片：`NpcStatusFlagsComponent` 保存由活动 buff
重建的 flags；`ApplyBuffSlotPhase` 在本地副本上递减活动 timer、计算已映射 flags、保留 type 1
且 `ai[1] == 9` 的火焰/霜焰 60-tick 刷新，并处理 type 353 immunity 删除。修正了删除后仍写回旧
slot 的覆盖错误，保留 Version4 `DelBuff` 压缩数组后 for-loop 自然递增、跳过移入当前位置条目的行为。
对需要 type 353 immunity 但输入长度不足的情况，整个 flags/slots 阶段返回 `RequiredInputMissing`，不提交
部分更改。

`ClearExpiredBuffs` 是独立后续 API：timer-zero slot 在 flags 阶段仍可见，先留给 Version4 的
`UpdateNPC_SoulDrainDebuff` 阶段，再由此 API 压缩并在有清理时返回一个 buff-sync intent。状态阶段的
intent 顺序为 shimmer 删除同步、dripping 水可清理请求；过期删除同步由后续阶段返回。Version4 顺序见
`NPC.cs:76738-76742`，`DelBuff` 的压缩/packet 54 请求见 `NPC.cs:76457-76481`，type 353 分支见
`NPC.cs:78318-78343`。`UpdateNPC_SoulDrainDebuff` 的玩家/粒子实现尚未迁移；其调用顺序被保留为组合点。
`UpdateNPC_BuffApplyDOTs`、regen 聚合仍未由这两个 API 实现。Version4 `TryRemovingWaterPerishableEffects`
和 `ApplyEelWhipDoT` 为空方法体，其效果维持 `unknown`，没有从完整参考树补入实现。

核心 verifier 新增窄断言：flag 重建与 timer 递减、timer-zero flags 在独立清理前可见；type 1 且 AI slot 1 为 9
时火焰/霜焰 timer 刷新到 60；缺少 shimmer immunity 时 flags/slots 无部分提交；type 353 删除后的 slot 保留、
legacy loop 可见性，以及 status effect 与过期清理 intent 顺序。目标 packet API 未改，`INpcReplicationPacketApi` 仍只有接口函数声明与
空 DTO，`INpcSpawnSyncPacketPort` 仍只有函数声明；无解析、编码、授权、recipient 选择、transport 或 apply
实现。

只读 Version4 CPG Query API 使用数据库
`D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite`，manifest SHA-256
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`。
`UpdateNPC`、`UpdateNPC_BuffFlagsReset`、`UpdateNPC_BuffSetFlags`、
`UpdateNPC_BuffClearExpiredBuffs`、`UpdateNPC_BuffApplyDOTs`、`UpdateNPC_SoulDrainDebuff` 的符号查询均为
`complete`、无 query gap；选定 `Main.cs` 的 `UpdateNPC` 调用点 2 个，选定 `NPC.cs` 的每个 phase helper
调用点各 1 个，call-site 查询均为 `complete`、无 query gap。上述六个 callable facts 查询均为 `partial`、各有
1 gap，不证明 callee effects、动态 dispatch 或运行时闭包。选定 `NPC.cs` shard 中 `buffType` 47、
`buffTime` 15、`buffImmune` 21、`poisoned` 5、`lifeRegen` 69、`lifeRegenCount` 8 项 member uses
均为 `complete`、无 query gap；它们不是完整 writer 集或唯一 owner 证明。CPG 不绑定源码快照；独立源码
读取的 `D:\TRbackup\Version4\Terraria\NPC.cs` SHA-256 为
`29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`。

使用仓库选定 SDK `10.0.400`，构建前确认无活动 `dotnet`/`csc` 编译进程。仅构建受影响 verifier 项目及其项目引用，
通过仓库串行入口执行：

~~~powershell
$env:PATH = 'C:\Users\shan\.dotnet;' + $env:PATH
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'build', '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
~~~

build exit code 0，0 warnings、0 errors。目标产物位于 `Build/bin/`：
`Terraria.NpcDamageCombatVerification.dll` SHA-256
`0DDE5AAE6838FCE34D5D6F3B8458EC92C179AA52B9F2C3C0179E4AB52963E122`；
`Terraria.Npc.dll` SHA-256 `D997C4487DBE6B775EED17474DE5918932E0A22B597E40273C6650C43178E6E9`。

只运行该核心 verifier，使用构建产物：

~~~powershell
$env:PATH = 'C:\Users\shan\.dotnet;' + $env:PATH
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'run', '--project', '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
~~~

run exit code 0，输出 `PASS: P12 combat, life, tracker and death core smoke`。这只证明列明的核心 verifier
局部断言通过；不证明生产状态 tick 接线、SoulDrain/DoT 完整行为、network API 行为、完整 P12 或行为等价。
整体仍为 `executionStatus: partial`、设计 `proposed`、`fullP12Verification: not-run`、
`migrationStatus: not-claimed`。权威 outputReport 未改，原 session
`64850edbdb464104ba04d413bfc362e1` 已完成且未重新 claim/结算。

### 2026-10-02 DoT regeneration calculation correction

Version4 `NPC.UpdateNPC` 源码 `NPC.cs:76738-76743` 确认 flags reset、buff flags、SoulDrain、expired-buff cleanup、
DoT、VFX 的先后顺序。`UpdateNPC_BuffApplyDOTs` 在 `NPC.cs:77659-78074` 先受 `dontTakeDamage` gate，之后按
固定顺序调整 `lifeRegen` 和本地 `num`，最后使用 120 阈值累计回复或请求 `GetHurtByDebuff`。当前 CPG Query API
解析 `UpdateNPC_BuffApplyDOTs` 符号及选定 NPC.cs/Main.cs call-sites 为 `complete`、无 query gap；选定 NPC.cs
有 1 个 helper call-site。`GetHurtByDebuff` 为 2 个、`GetAttackDamage_ForTownNPC` 为 5 个所选范围静态 call-sites，
同样不闭合动态分派或完整运行时调用图。`UpdateNPC_BuffApplyDOTs` callable facts 是 `partial`，有
`CalleeEffectsNotExpanded` gap。CPG manifest SHA-256 为
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`。

Version4 关键源码 `D:\TRbackup\Version4\Terraria\NPC.cs` SHA-256 为
`29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`；完整参考树对应 NPC.cs SHA-256 为
`ED8AA2302730A9E046EF310E543204FBA391BD35B1C9C0B213C8065DFBBE39F0`，本次逐段阅读仅作对照，行为判定以 Version4 为准。
Version4 `GetAttackDamage_ForTownNPC` 在 `NPC.cs:6948-6954` 采样难度 multiplier 并转成 int；目标 API 将解析后的
multiplier 明确作为输入，不直接访问全局 difficulty 状态。

在 `NpcStatusSystem.ApplyDamageOverTimePhase` 中修正：Celled 在 `num < projectileCount * 20` 时覆盖为
`projectileCount * 10`；Dryad Bane 在 `num < townNpcDamage` 时覆盖为 `townNpcDamage / 3`。这两处是覆盖赋值而非
普通最小值。需要投射物计数时，现在要求完整、恰有 1000 项的 snapshot，对应 Version4 对索引 0..999 的扫描；缺失、
未标完整、数量不符或 legacy NPC slot 无效时拒绝整个阶段，不部分修改 status/count。Dryad Bane 的两倍 regen penalty
使用 64 位中间值，最终值无法表示时返回 `ArithmeticOverflow`。System 返回 regen/count、回血点数及待消费的伤害事件，
不直接改 health、不发起 forced strike，也没有生产 `UpdateNPC` consumer 接线；该生产组合和失败/重试语义仍为 `unknown`。

核心 smoke 源码加入 poison/120 余数、跨阈值回血、缺失/短 projectile snapshot 原子拒绝、Celled/Dryad Bane 覆盖阈值、
以及 Dryad Bane 溢出原子拒绝断言。对应的 affected verifier build 经仓库串行入口完成：

~~~powershell
$env:PATH = 'C:\Users\shan\.dotnet;' + $env:PATH
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'build', '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
~~~

build exit code 0，0 warnings、0 errors。验证器 DLL 位于
`Build/bin/Terraria.NpcDamageCombatVerification/Debug/net10.0/Terraria.NpcDamageCombatVerification.dll`，SHA-256
`A18BDE4EA3573F8FFBABB01D28322CE719E9983D9F89F14BE32CB000B862C502`；`Terraria.Npc.dll` 位于
`Build/bin/Terraria.Npc/Debug/net10.0/Terraria.Npc.dll`，SHA-256
`0F519CD14E07474B75A21BE12350CB7AFF291608AFF651BD45BD57D3A7A81EAA`。

下列 10% verifier run 在用户随后要求停止测试之前已执行；此后未启动测试或构建：

~~~powershell
$env:PATH = 'C:\Users\shan\.dotnet;' + $env:PATH
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'run', '--project', '.\Test\Terraria.NpcDamageCombatVerification\Terraria.NpcDamageCombatVerification.csproj',
  '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
~~~

exit code 0，输出 `PASS: P12 combat, life, tracker and death core smoke`。仅证明该 verifier 明确列出的局部断言，
不证明 DoT 生产接线、完整 GetHurtByDebuff 消费、network API 行为、完整 P12 或迁移等价。packet 23/28 和
spawn-sync API 仍仅有函数声明/空 DTO，没有解析、编码、授权、recipient 选择、transport 或 apply 实现。
当前 `executionStatus: partial`、`designStatus: proposed`、`fullP12Verification: not-run`、
`migrationStatus: not-claimed`；authority outputReport 与已结算 session `64850edbdb464104ba04d413bfc362e1` 未修改。

### 2026-10-02 DoT result consumer composition

增加 `NpcCombatSystem.CommitLifeRegeneration`，在同一 combat owner 边界按源顺序消费已计算结果：先对当前命中的
NPC health 提交回血，再按 `DamageEventCount` 逐次调用 `ApplyDamageOverTime`，并返回逐事件结果。新增的
`NpcHealthComponent.ApplyHealing` 按本地 health maximum 截断实际回血。DoT 对父体的直接伤害和 forced strike 之后，
将 life-owner health 镜像到被命中的 segment。该安排对应 Version4 `NPC.cs:78042-78072` 的回血先于伤害循环，以及
`NPC.cs:78077-78099` 的 `realLife` life owner 路由。

此 consumer 目前是候选 API：没有生产 `UpdateNPC`/tick caller。`ApplyDamageOverTimePhase` 已先提交 accumulator；
若单个事件拒绝、同步异常或 lethal event 改变 lifecycle 后仍有剩余事件，counter 回滚、补偿和重试语义仍为
`unknown`。它不会执行 packet 28、recipient 选择或 transport。Version4 `MessageBuffer.cs:3287-3292` 的 packet
153 入站调用不纳入本地 tick consumer；packet API 依用户要求仍只有函数声明和空 DTO。

修复 Dryad Bane 范围校验为 `(double)scaledDamage > int.MaxValue`，防止 `int.MaxValue` 转换到 `float` 时舍入至
`2147483648` 并漏过超界检测。

CPG Query API 对 `UpdateNPC_BuffApplyDOTs` 在选定 NPC.cs/Main.cs shards 的 call-site 查询为 `complete`、1 个调用点；
其 callable facts 为 `partial`，缺口 `CalleeEffectsNotExpanded`。`GetHurtByDebuff` 在选定 NPC.cs/MessageBuffer.cs
shards 的调用点查询为 `complete`、3 个调用点、无 query gap，其中一个来自 packet 153 handler。CPG manifest SHA-256
为 `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`；源码 SHA-256 与
CPG snapshot 的绑定仍为 `unknown`。静态查询不证明调用闭包或 callee effects。

本轮未运行 build、test 或 verifier。新改 C# 源码的编译状态未验证；`executionStatus: partial`、
`designStatus: proposed`、`fullP12Verification: not-run`、`migrationStatus: not-claimed`。权威 outputReport
未修改，原已完成 session `64850edbdb464104ba04d413bfc362e1` 未重新 claim 或结算。

### 2026-10-02 Soul Drain player eligibility query

新增 `NpcSoulDrainEligibilityQuery` 及显式输入/结果类型。输入要求 NPC center 与 255 项有序 player snapshot；匹配条件按
Version4 `NPC.cs:77258-77285` 的固定顺序实现：Soul Drain 激活、玩家 active 且未死亡、`(npcCenter - player.position).Length() < 1100`、
selected item type 为 3006、`itemAnimation > 0`。未激活时在读取 player snapshot 前返回空结果；活动状态下快照缺失或长度不符时整次
拒绝，不返回部分 legacy slots。

此 API 只返回资格 slots，不实现 Version4 后续 `Main.rand` gate、Dust 创建与字段设置，也没有生产 status tick caller。完整参考树
`NPC.cs:92564-92594` 多出的 local-player `soulDrain` 递增未移植，因为 Version4 对应方法没有该写入。网络 packet API 未改，仍仅有
函数声明与空 DTO。

只读 CPG Query API 查询 `UpdateNPC_SoulDrainDebuff`：symbol `complete`，选定 `NPC.cs`/`Main.cs` call-site 查询 `complete`，一个
静态调用点位于 `NPC.cs`，无 query gap；coverage 为 2 个选定 shard。manifest SHA-256 为
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`。此结果不证明运行时 caller 闭包。

按仓库串行构建入口只编译受影响的 NPC 项目：

~~~powershell
$env:PATH = 'C:\Users\shan\.dotnet;' + $env:PATH
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'build', '.\src\NSSLC\Component\Npc\Terraria.Npc.csproj', '--no-restore',
  '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
~~~

build exit code 0，0 warnings、0 errors；产物位于
`Build/bin/Terraria.Npc/Debug/net10.0/Terraria.Npc.dll`。本轮未运行 test 或 verifier，故
`verificationStatus: not-run`、`executionStatus: partial`、`designStatus: proposed`、
`migrationStatus: not-claimed`；权威 `outputReport` 未修改，原已完成 session `64850edbdb464104ba04d413bfc362e1` 未重新 claim 或结算。

### 2026-10-02 death 与 tracker 关系复核

death 与 tracker 切片由子代理分别审阅；两者未改共享代码。Version4 `NPC.checkDead` 的 CPG 符号查询
complete；在选定 `Terraria/NPC.cs`、`Terraria/Main.cs`、`Terraria/MessageBuffer.cs` 三个 shard 的 call-site
查询 complete，返回 9 个静态调用点且均位于 `NPC.cs`，无 query gap。`Get-CpgCallableFacts(checkDead)` 为
partial，gap 为 `CalleeEffectsNotExpanded`，因此不能由该结果证明完整调用闭包或死亡副作用。

tracker 的 CPG 查询与 Version4 源码交叉核对结果：`NPCDamageTracker.AddDamage` 两个 overload 的符号查询
complete；`CompareTo` 符号查询 complete，但选定 tracker shard 的调用点查询零命中并带
`NoMatchingFactInScannedScope` gap，callable facts 也因 `CalleeEffectsNotExpanded` 为 partial。Version4
`NPCDamageTracker.cs` 中 `CreditEntry.CompareTo` 返回默认 `int`；`_lastAttacker` 在 `AddDamage` 中赋值，整份
Version4 源树未发现读取或消费点。对应 `_lastAttacker` member-use 查询为零命中且 partial，不能单凭零命中推断
无读取。`AddDamageToLastAttack` 在所选 Version4 tracker shard 的符号查询 partial、零命中并有
`NoMatchingFactInScannedScope` gap；完整 Version4 源码搜索未找到其声明或引用。完整参考树的
`NPCDamageTracker.cs` 有该 helper，`NPC.cs` 有 3 个分段死亡调用点；这些仅为 reference-only，未迁移。

目标 `NpcDeathPhaseQuery` 仍只返回已有 guard、有限 phase decisions 和效果 intents；没有生产 consumer。tracker
清空方法和死亡效果的剩余 writer、调度关系仍为 partial/unknown。本轮没有新增迁移代码，不运行 build、test 或
verifier。CPG manifest SHA-256 为
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`；其查询结果
不绑定当前源码快照，也不证明运行时闭包。整体仍为 `executionStatus: partial`、`designStatus: proposed`、
`verificationStatus: not-run`、`migrationStatus: not-claimed`；权威 `outputReport` 和原 session
`64850edbdb464104ba04d413bfc362e1` 未修改、未重新 claim 或结算。

### 2026-10-02 Clothier death follow-up intent

在 `NpcDeathPhaseQuery` 增加 Version4 type 54 夜间死亡的 Skeletron 资格决策。输入沿现有 phase context 传入昼夜、
type 35 活动状态及有序玩家槽快照；只有夜间且无 type 35 时才要求恰好 255 项，并按 0..254 找首个
active、未死亡且 `KillClothier` 的玩家。匹配时输出 `NpcDeathSkeletronSpawnIntent(PlayerSlot)`；
缺少快照或长度不符时返回 `RequiredInputMissing` 并压住终态，避免在缺少判断数据时漏过待处理的 Skeletron 分支。
`NpcDeathLifecycleSystem` 将该 intent 标记为 `TerminalCommitPending`，供后续外部 owner 处理。

行为依据为 Version4 `NPC.cs:64700-64707` 与 `NPC.cs:66738-66780`。CPG `SpawnSkeletron` 符号及所选
`Terraria/NPC.cs` 调用点查询 complete、无 query gap，2 个静态调用点；callable facts partial，gap 为
`CalleeEffectsNotExpanded`。manifest SHA-256 `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，
`SourceSnapshotId=null`。完整参考树在该分支额外增加 `Main.netMode != 1`；实现遵从 Version4，未导入差异。

受影响 NPC 项目通过仓库串行入口编译：

~~~powershell
$env:PATH = 'C:\Users\shan\.dotnet;' + $env:PATH
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'build', '.\src\NSSLC\Component\Npc\Terraria.Npc.csproj', '--no-restore',
  '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
~~~

build exit code 0，0 warnings、0 errors；产物 `Build/bin/Terraria.Npc/Debug/net10.0/Terraria.Npc.dll`，
SHA-256 `A42A987C626D6D1C8629DC3AB082A8FC1E36479520FF61F8278C1A10DF116FEB`。按用户要求，本轮未运行测试或
verifier；因此 compile 不提升行为验证状态。该 intent 没有生产 caller、spawn adapter 或消费协调，helper effects
及失败/重试语义仍为 `unknown`。整体保持 `executionStatus: partial`、`designStatus: proposed`、
`verificationStatus: not-run`、`migrationStatus: not-claimed`；未修改权威 `outputReport`，未重新 claim/结算原 session。

### 2026-10-02 Soul Drain visual effect consumer

新增 `NpcSoulDrainVisualEffectSystem`、effect input/result 与 `INpcSoulDrainVisualEffectPort`。它复用 eligibility
query，按 Version4 顺序对每个合格 player slot 抽取 1/3 gate；通过后依次抽取两个位置扰动值、加入 NPC velocity、创建
type 235 Dust，再设置零速度、scale 与 `playerSlot + 1` fade-in。随机和 Dust 写入共享同一个显式端口，保留单一随机流
顺序。没有增加完整参考树中的本地玩家 `soulDrain` 计数写入。

该 consumer 仍没有生产 `UpdateNPC` caller；效果端口也没有 Terraria adapter。没有新的 build、test 或 verifier
结果支持这段实现，代码编译及行为状态未验证。整体为 `executionStatus: partial`、`designStatus: proposed`、
`verificationStatus: not-run`、`migrationStatus: not-claimed`；权威 `outputReport` 与已结算 session
`64850edbdb464104ba04d413bfc362e1` 未修改。

### 2026-10-02 GoodWorld death world-effect consumer and source identity

新增 `NpcDeathWorldEffectSystem`、`NpcDeathWorldEffectResult` 与 `INpcDeathWorldEffectPort`。fixed-position 分支将
type 13 的 type `-12` spawn 坐标交给 NPC-AI spawn port；surface-search 分支按 Version4 的 x/y random 顺序、每轮
至多 1000 次、半径 50、底边 200 tile 扫描，在第一个有效候选处调用 spawn，并停止该轮搜索。成功条件与 packet 23
同步请求保留源代码的 `legacySlot < maxNpcSlots` 判定。packet 行为只经过既有 `INpcSpawnSyncPacketPort` 声明，不实现
packet writer 或 transport。

复核 Version4 `NPC.cs:64633-64660` 与 `NPC.cs:77061-77065` 后，明确 `GetSpawnSourceForNPCFromNPCAI()` 返回
`EntitySource_Parent(this)`。`NpcDeathPhaseContext`、phase input 及 spawn intents 因而显式携带 `SourceNpcInstanceId`；
type 396/397、GoodWorld type 13/36 在缺少该身份时阻止产生 spawn decision 并标记 `RequiredInputMissing`。
来源稳定 ID 由调用方提供，adapter 必须据其建立 parent spawn source，不可依赖 ambient current-NPC 状态。

只读 CPG Query API 的 spawn-source symbol 与所选 `Terraria/NPC.cs` call-site 查询为 `complete`、无 query gap；所选
shard 共 58 个静态调用点，其中 `checkDead` method span 内 3 个。`checkDead` callable facts 为 `partial`，缺口
`CalleeEffectsNotExpanded`。manifest SHA-256 为
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`。该索引只提供静态关系，
不证明完整副作用闭包。

此前 death-phase 子代理报告 NPC 项目串行 build exit code 0、0 warnings、0 errors，输出位于
`Build/bin/Terraria.Npc/Debug/net10.0/Terraria.Npc.dll`；该次 build 不覆盖本段 GoodWorld consumer 与 source-identity
改动。本轮按最新指示没有再运行 build、test 或 verifier。没有生产 death coordinator、NPC-AI spawn adapter 或 effect
port 实现；terminal ordering、失败恢复及真实 packet 发送仍 `unknown`。当前仍为 `executionStatus: partial`、
`designStatus: proposed`、`verificationStatus: not-run`、`migrationStatus: not-claimed`；权威 `outputReport` 未修改，
原 session `64850edbdb464104ba04d413bfc362e1` 未重新 claim 或结算。

### 2026-10-02 NPC status tick phase composition

新增 `NpcStatusTickInput`、`NpcStatusTickResult` 与 `NpcStatusTickSystem`。`Advance` 的顺序与 Version4
`NPC.UpdateNPC` 一致：`ApplyBuffSlotPhase` -> Soul Drain visual effect -> `ClearExpiredBuffs` ->
`ApplyDamageOverTimePhase` -> `NpcCombatSystem.CommitLifeRegeneration`。首个 buff phase 因缺少必需输入被拒绝时立即返回；
Soul Drain visual query 失败仅保留在其独立结果中，后续状态 phase 仍按源代码顺序继续。regen input 的 AI state 1 和
当前/max health 由显式 `NpcAiStateComponent` 与当前 health 覆盖。buff slot sync 与 water-perishable cleanup intents
由结果保留，未加入 packet writer 或外部 effects 实现。

查询关系使用只读 `CpgEvidence.ps1` API，数据库为
`D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite`。在 `Terraria/NPC.cs`、`Terraria/Main.cs`、
`Terraria/MessageBuffer.cs` 选定 scope 中，`UpdateNPC` symbol/call-site 查询为 complete，调用点 2 个，均位于
`Main.cs`；`UpdateNPC_BuffFlagsReset`、`UpdateNPC_BuffSetFlags`、`UpdateNPC_SoulDrainDebuff`、
`UpdateNPC_BuffClearExpiredBuffs`、`UpdateNPC_BuffApplyDOTs` 的 symbol 与 call-site 查询均 complete、无 gap，
每项有 1 个 `NPC.cs` 调用点；`GetHurtByDebuff` 的查询 complete、无 gap，3 个调用点来自 `MessageBuffer.cs` 和
`NPC.cs`。manifest SHA-256 为
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`。原始 Version4
`NPC.cs:76738-76743` 与完整参考树 `NPC.cs:91990-91995` 的 phase 顺序一致；参考树 Soul Drain 方法体多出的
local-player counter 写入不属于当前 Version4 行为，没有迁移。

完整参考树 `NPC.cs:93720-93736` 的 expired-buff 清理另有 multiplayer-client gate，Version4
`NPC.cs:78101-78118` 没有该 gate；本次组合按 Version4 实现，不复制此 reference-only 差异。

`NpcStatusTickSystem` 目前是局部 phase composition，没有目标 Main/session scheduler、active NPC snapshot 或生产 caller。
`UpdateNPC_BuffApplyVFX`、Blood Moon transformation、gravity、life<=0 network cleanup、Soul Drain Terraria port、buff
sync/water cleanup consumer 均未接入；DoT counter 已提交后若 Combat 拒绝/抛错的回滚与重试仍为 `unknown`。本轮按用户
最新指示未运行 build、test 或 verifier；代码编译及行为未验证。整体保持 `executionStatus: partial`、
`designStatus: proposed`、`verificationStatus: not-run`、`fullP12Verification: not-run`、
`migrationStatus: not-claimed`。未修改权威 `outputReport`，未重新 claim 或结算 session
`64850edbdb464104ba04d413bfc362e1`。

对照 Version4 `NPC.UpdateNPC` 的入口 guard 后，`NpcStatusTickSystem.Advance` 增加 inactive early return 和
`SkippedInactive` result；该分支不验证 DoT text identity、不读 buff snapshots、不改变状态或调用 Soul Drain port。
早退顺序对应 `NPC.cs:76711` 的 active 检查早于 `NPC.cs:76738` 的首个 status phase。本轮仍未运行 build、test
或 verifier。

### 2026-10-02 Packet 54 buff-sync declaration and status evidence follow-up

扩展 `INpcReplicationPacketApi`，新增 `PublishPacket54BuffSlots(int npcLegacySlot, ReadOnlyMemory<StatusEffectSlot> buffSlots)`
函数声明。没有添加 packet encoder、recipient selection、authorization、transport 或 inbound apply 逻辑。该签名仅表达
Version4 `NPC.DelBuff`/expired-buff cleanup 的出站 buff-slot 同步边界；packet API 依用户要求保持 declaration-only。

只读 CPG Query API 对 `UpdateNPC_BuffApplyVFX:void()`、`UpdateNPC_BloodMoonTransformations:void()`、
`UpdateNPC_UpdateGravity:void(float)`、`TryRemovingWaterPerishableEffects:void(bool)` 的 symbol 查询均为 `complete`。
在选定 `Terraria/NPC.cs`、`Terraria/Main.cs`、`Terraria/MessageBuffer.cs` 范围内，前三项各有 1 个 `NPC.cs`
静态调用点，water-perishable helper 有 3 个 `NPC.cs` 静态调用点；查询无 query gap。manifest SHA-256 为
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`。这些结果只界定
所选静态关系，不证明调度、callee effects 或完整运行时调用闭包。

Version4 `NPC.cs:76738-76743` 在 status/regen 阶段后依次调用 buff VFX、Blood Moon transformation 和 gravity；
当前 `NpcStatusTickSystem` 尚未实现或接入这三段。`NPC.UpdateNPC_BuffClearExpiredBuffs` 在至少一个过期 slot 被删时
发送一次 packet 54；`NPC.DelBuff` 非 quiet 路径也发送 packet 54。`NetMessage.cs:1018-1035` 的 writer 写 NPC legacy
slot、occupied buff type/time pairs 和零 terminator；`MessageBuffer.cs:2016-2017` 的 packet 54 分支是无条件 `break`，
入站 apply 行为仍为 `unknown`。

Version4 `NPC.cs:79301` 的 `TryRemovingWaterPerishableEffects(bool)` 函数体为空。完整参考树
`NPC.cs:95164` 起有 on-fire/on-fire3 与 stinky buff 删除逻辑，并有 multiplayer-client gate；它没有被移植到
Version4 目标逻辑。对应 `WaterPerishableCleanupRequested` 保留为未消费 intent，行为标 `unknown`。

本轮按用户要求未运行测试或 verifier。曾先用系统默认 SDK 尝试完整参考树 build，因 PATH 解析到 10.0.100、而
`global.json` 要求 10.0.400，未进入编译；将 `C:\Users\shan\.dotnet` 前置到 PATH 后，旧
`project.assets.json` 又因指向不存在的 `D:\ruanjian\VS\Shared\NuGetPackages` 回退目录而失败。通过仓库串行入口
刷新 restore 元数据后，完整参考树与受影响的目标 NPC 项目均编译成功。

完整参考树 restore 命令：

~~~powershell
$env:PATH = 'C:\Users\shan\.dotnet;' + $env:PATH
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'restore', 'D:\TRbackup\无任何删减通过编译\TerrariaServer.csproj',
  '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
~~~

完整参考树 build 命令：

~~~powershell
$env:PATH = 'C:\Users\shan\.dotnet;' + $env:PATH
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'build', 'D:\TRbackup\无任何删减通过编译\TerrariaServer.csproj', '--no-restore',
  '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
~~~

exit code 0，85 warnings、0 errors；产物 `D:\TRbackup\无任何删减通过编译\bin\Debug\net40\TerrariaServer.exe`。
目标 NPC 项目 build 命令：

~~~powershell
$env:PATH = 'C:\Users\shan\.dotnet;' + $env:PATH
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'build', '.\src\NSSLC\Component\Npc\Terraria.Npc.csproj', '--no-restore',
  '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
~~~

目标项目 exit code 0，0 warnings、0 errors，产物为
`Build\bin\Terraria.Npc\Debug\net10.0\Terraria.Npc.dll`。两个产物均已检查存在。

编译成功不提升行为验证状态。保持 `executionStatus: partial`、`designStatus: proposed`、
`verificationStatus: not-run`、`fullP12Verification: not-run`、`migrationStatus: not-claimed`；权威
`outputReport` 未修改，原 session `64850edbdb464104ba04d413bfc362e1` 未重新 claim 或结算。

### 2026-10-02 Status tick 后续 phase 与当前编译

新增 `NpcBloodMoonTransformationSystem`/`NpcBloodMoonTransformationIntent`：依据 Version4
`NPC.cs:78120-78160` 将三组 NPC type 映射为 Crimson/Corruption 目标，并保留旧 `value == 0` 时转换后恢复为
零的 intent。该结果尚无 consumer，不执行 `Transform`，type/defaults 的提交和失败恢复均为 `unknown`。

新增 `NpcGravityEnvironmentSnapshot`、`NpcGravitySystem` 与 `NpcGravityResult`。System 依据 Version4
`NPC.cs:77176-77280` 计算 type/AI 特例、world-surface gravity scale、wet/shimmer/honey gravity 与最大下落速度，
并返回特定 type 的 Y 速度上限。`NpcStatusTickSystem.Advance` 现在按顺序输出 Blood Moon intent 和可选 gravity
结果。DoT/regen 未计算时不执行 Combat commit，但仍推进到后续 phase，避免拒绝分支提前跳过。
Version4 `NPC.Transform` 会重设 defaults/AI 参数并按新旧高度调整位置；有 Blood Moon intent 且提供 gravity 输入时，
结果设置 `GravityDeferredUntilPostTransformationSnapshot`，不使用转换前的快照计算 gravity。Transform consumer 提交后，
须以新 type/AI/位置快照单独调用 `NpcGravitySystem`。gravity 输入为显式快照；没有 Terraria 取样 adapter，也不提交
Movement velocity/gravity。`UpdateNPC_BuffApplyVFX`、tile-null、`noGravity` 应用、
后续 fall-speed clamp/velocity quantization、life<=0 network/dirty cleanup、生产 scheduler/caller 均未闭合。

只读 CPG Query API manifest SHA-256 为
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`。三个 helper
(`UpdateNPC_BuffApplyVFX`、`UpdateNPC_BloodMoonTransformations`、`UpdateNPC_UpdateGravity`) 的精确 method symbol
查询均 `complete` 且各命中一个 NPC.cs symbol。`UpdateNPC_BuffApplyVFX` 在选定 `NPC.cs`/`Main.cs` 范围的
call-site 查询为 `complete`、1 个 NPC.cs 调用点；callable facts 为 `partial`、`CalleeEffectsNotExpanded`。
Blood Moon 与 gravity helper 的选定范围 call-site 查询为 `partial`、`NoMatchingFactInScannedScope`，callable facts 为
`partial`、`CallableSymbolNotPresentInShard`。不以索引空结果推断无调用；已回读 Version4 `NPC.cs:76738-76745`、
Blood Moon helper 和 `NPC.Transform` (`NPC.cs:67349-67405`)，
源码确认入口 phase 顺序以及转换期间 defaults/AI/position 的变化。gravity helper 在
`NPC.cs:77176-77280`。完整参考树 `NPC.cs:91990-91997` 仅作交叉核对。

`UpdateNPC_BuffApplyVFX` 仍未迁移，且不能当成纯视觉 consumer：Version4 `NPC.cs:77287-77657` 除
Dust/Gore/particle/light effects 外，还推进 `shimmerTransparency`、在首尾加减 `netOffset`，并于 shimmer 条件满足时
调用 `GetShimmered()`。Version4 `NPC.cs:77658` 的该 helper 是空方法；完整参考树包含的实现不能作为 Version4 行为
事实。目标现已增加 `NpcShimmerStateComponent` 和 `NpcShimmerTransparencySystem`，并在本地 tick composition 中按
DoT/regen 后的顺序调用；这只迁移透明度数值转移，不包括 effect consumer、`netOffset` adapter、packet 23 hydrate、
AI reader 接线或生产 scheduler。

### 2026-10-02 Shimmer state owner 补充

只读 CPG Query API 查询 `shimmerTransparency:float` 的 field symbol 为 `complete`，命中
`Terraria/NPC.cs` 声明；在 manifest 的 967 个 source paths 上执行 member-use 查询为 `complete`，返回 19 项、无
query-level gap：`NPC.cs` 17 项（4 个 `ReadWrite`、4 个 `Write`、9 个 `Unknown`），`MessageBuffer.cs` 1 个
`Write`，`NetMessage.cs` 1 个 `Unknown`。其中方向未分类的 9 项仍保留 unknown；完整查询范围与 `SourceSnapshotId=null`
不证明索引与当前源码快照绑定。manifest SHA-256 为
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`。

Version4 源码确认 NPC 侧关系包括 reset (`NPC.cs:8120`)、Shimmer AI 的调用/透明度与速度转移
(`NPC.cs:33293`, `43355-43386`)、despawn AI 读取 (`NPC.cs:47351`)、IdleSounds 读取 (`NPC.cs:76508`) 和
BuffApplyVFX 转移 (`NPC.cs:77626-77653`)。MessageBuffer packet 23 的 bit 4 可令目标 NPC 的
`shimmerTransparency` 设为 `1f` (`MessageBuffer.cs:1281`)；NetMessage packet 23 仅发送
`spawnNeedsSyncing && shimmerTransparency > 0f` 标志 (`NetMessage.cs:709`)。因此状态转换 System 不能被视为该字段的
完整 owner；packet hydrate、NetMessage projection 和所有 AI reader 仍未接线。

同一 CPG 会话中 `UpdateNPC_BuffApplyVFX:void()` symbol 查询为 `complete`；在选定
`NPC.cs`/`Main.cs` 中 call-site 查询为 `complete`、1 个 NPC.cs 调用点；callable facts 为 `partial`，gap 为
`CalleeEffectsNotExpanded`。这只确认静态入口和选定 shard 的调用，不闭合 helper 的效果。完整参考树
`NPC.cs:93124` 的 `GetShimmered` 有实现而 Version4 `NPC.cs:77658` 是空方法，按权威 Version4 留空。

新增目标文件为 `NpcShimmerStateComponent.cs`、`NpcShimmerTransparencyInput.cs`、
`NpcShimmerTransparencyResult.cs`、`NpcShimmerTransparencySystem.cs`；`NpcStatusTickSystem.Advance` 接收状态 owner，
并在 DoT/regen commit 阶段后输出 shimmer transition result。缺少 buff 353 immunity 快照时不推进衰减并设置
`RequiredInputMissing`；`CanDisplayBuffs=false` 时不修改状态。没有实现 packet API 的 decode/encode 逻辑。此局部
System 无生产 scheduler/caller，也没有专门验证；整项仍保持 partial。

按仓库串行构建约束，先检查没有活动 `dotnet`/`csc` 编译进程，再以 SDK `10.0.400` 执行：

~~~powershell
$env:PATH = 'C:\Users\shan\.dotnet;' + $env:PATH
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'build', '.\src\NSSLC\Component\Npc\Terraria.Npc.csproj', '--no-restore',
  '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
~~~

exit code 0，0 warnings、0 errors；生成物存在于
`D:\TRbackup\NLTX\Build\bin\Terraria.Npc\Debug\net10.0\Terraria.Npc.dll`。本轮没有运行测试或 verifier；
编译结果不提升行为验证状态。整体仍为 `executionStatus: partial`、`designStatus: proposed`、
`verificationStatus: not-run`、`fullP12Verification: not-run`、`migrationStatus: not-claimed`。权威
`outputReport` 未修改，未使用新的 claim，原 session `64850edbdb464104ba04d413bfc362e1` 未重新结算。

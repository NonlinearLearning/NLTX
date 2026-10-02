# Version4 P12 NPC 战斗、网络与伤害追踪 System 设计

~~~yaml
partitionId: P12
taskId: AUTH-SYS-P12
originalSessionId: 64850edbdb464104ba04d413bfc362e1
sourceReport: D:\TRbackup\NLTX\docs\system-decomposition\reports\2026-09-18-system-decomposition-authoritative-P12-npc-combat-network-damage.md
authoritativeSource: D:\TRbackup\Version4
completeReferenceSource: D:\TRbackup\无任何删减通过编译
targetArea: D:\TRbackup\NLTX\src\NSSLC
designStatus: proposed
executionStatus: partial
verificationStatus: not-run
sourceModified: true
testsRun: true
verificationScope: 10-percent-core-smoke-after-packet-api-declarations-immortal-strike-knockback-velocity-commit-checkdead-phases-goodworld-13-36-intents-phase-health-owner-restore-segment-mirror-authority-ai-policy-sync-consumer-policy-preservation-and-npc-status-slot-phase-expiration-order-and-npc-dot-regen-poison-heal-celled-dryad-threshold-full-projectile-snapshot-arithmetic-atomicity
fullP12Verification: not-run
documentationPassSourceModified: true
documentationPassTestsRun: false
migrationStatus: not-claimed
~~~

## 1. 文档定位

本文件把已完成的 P12 System 拆分报告转成后续实现可执行的目标设计。它只描述
System、Component、Query、Command、Adapter 和 Projection 的候选边界、组合顺序和
证据门槛，不表示这些类型已经在目标项目中接通，也不表示 Version4 行为已经迁移或等价。

frontmatter 的 `sourceModified: true`/`testsRun: true` 记录 packet API 声明范围、普通 immortal
Strike 修正、knockback 计算及 velocity state 提交、death lifecycle guard、已确认 phase 子集、
GoodWorld type 13/36 spawn intents、NPC status slot 清理，以及 DoT regeneration 窄边界断言和
随后通过的 10% 核心 smoke。smoke 检查局部
combat/life/tracker/death、knockback 边界、strike 对 `MovementStateComponent.Velocity` 的提交、
inactive `PendingDespawn`、四类 checkDead phase、type 35 公告、type 604/605 瓢虫效果和
GoodWorld type 13/36 intents；DoT 子集检查 poison/120 计数、回血、完整 projectile 输入保护、Celled 与
Dryad Bane 阈值覆盖和算术溢出原子拒绝。不验证 packet API 协议行为、DoT 生产调用/结果消费、GoodWorld 外部
spawn/effect intent consumer 或完整 P12。
权威 outputReport 状态不变。

本设计只继承 P12 claim 的输入范围：19 个叶子组、173 个字段、16 个属性，共 189 个成员。
没有把其他分区的成员重新领取到 P12。原始 P12 报告是本设计的唯一成员范围来源；本文件
是派生设计文档，不重新打开或改写 runner 的 task table、ledger、lock 或 outputReport。

核心约束：

~~~text
legacy entry or external input
  -> validated Query or Command
  -> one authoritative System commit
  -> Component state and revision
  -> effect Adapter or read-only Projection
~~~

所有未由当前 Version4 源码和查询结果共同证明的关系保留为 partial 或 unknown。完整参考
树用于补充候选关系和缺失方法体的线索，不能把 reference-only 行为升级为当前 Version4
confirmed。任何空方法体都按 unknown 处理。

## 2. 证据基线

### 2.1 源码和版本矩阵

| 来源 | 用途 | 当前证据状态 |
|---|---|---|
| D:\TRbackup\Version4\Terraria\NPC.cs | NPC 生命周期、UpdateNPC、StrikeNPC、死亡与网络意图 | authoritative；主路径 confirmed，完整写入闭包 partial |
| D:\TRbackup\Version4\Terraria\Main.cs | CalculateDamageNPCsTake(int,int) 伤害基础公式 | authoritative；公式 confirmed，CPG 在选定调用范围返回 1 个静态调用点 |
| D:\TRbackup\Version4\Terraria\MessageBuffer.cs | packet 23/28 入站读取和应用顺序 | authoritative；packet 28 顺序 confirmed、权限/畸形输入 partial；packet 23 在字段读取前无条件 break，后续 reader 不可达，入站行为 unknown |
| D:\TRbackup\Version4\Terraria\NetMessage.cs | packet 23/28 出站字段、接收者和 skip/stream 分支 | authoritative；线协议片段 confirmed，传输重试/reconnect partial |
| D:\TRbackup\Version4\Terraria.GameContent\NPCDamageTracker.cs | tracker 注册、credit、active/recent 生命周期 | authoritative；主体 partial；`CompareTo` 返回默认值，`_lastAttacker` 在当前 Version4 只见写入；分段最后一击 credit 仍 unknown |
| D:\TRbackup\Version4\Terraria.GameContent\BossDamageTracker.cs | composite definition 的 tracker type 归一化、damage membership 与 active check | authoritative；构造时选 `NPCTypes[0]` confirmed，状态/调度输入仍 partial |
| D:\TRbackup\Version4\Terraria.GameContent\NPCInteractions.cs | Initialize、商店注册和交互注册 | authoritative；注册 confirmed，动作实现 unknown |
| D:\TRbackup\无任何删减通过编译 | 完整方法体和额外关系的 reference-only 对照 | supplementary；不能覆盖当前 Version4 |
| C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Damage\Systems\SharedDamageOtherOnHitSystem.cs | component-scoped damage event handlers delegate to damage services | structure-only；不复制公式、归因或权威行为 |
| D:\TRbackup\NLTX\src\NSSLC | 已有目标组件、候选 System 和验证项目形状 | target evidence；不是集成或等价证明 |

当前 Version4 关键哈希：

| 文件 | SHA-256 |
|---|---|
| Terraria/NPC.cs | 29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17 |
| Terraria/Main.cs | 66DBE1D1E6FB89A24A512309BB039DFEC7C06D4678D2A22D7ADECBAD3AFD6520 |
| Terraria/MessageBuffer.cs | 0F474225FA9D98C539F61249619273A44E73F69A4CF388B44258433A2D15D5EE |
| Terraria/NetMessage.cs | 87B596BD8467B9C470DD1F2AD6963EBADE257689F87395163AB136C149BEBC9A |
| Terraria.GameContent/NPCDamageTracker.cs | 1B6812046FE624470BF51B48EE542EB8836D270CA60EC77BCCAB5A9F9E3DD8B0 |
| Terraria.GameContent/BossDamageTracker.cs | D49762B01F12890433D85D8432B2AC8539003200BFA6A9A73EAA75FB416467E7 |
| Terraria.GameContent/NPCInteractions.cs | 4379CCE14D30A126D02E3C65EC103DE9B99572AF1F117D0B659330F6287F792C |

完整参考树的对应哈希和行数不同，必须分开取证：

| 文件 | SHA-256 | 行数 | 允许用途 |
|---|---|---:|---|
| Terraria/NPC.cs | ED8AA2302730A9E046EF310E543204FBA391BD35B1C9C0B213C8065DFBBE39F0 | 97097 | reference-only 分段死亡和条件分支 |
| Terraria/Main.cs | E24E61C9903BB7995F47E36C51EDBE43481B643226861B717020877E7E22B63F | 67447 | reference-only damage helper 对照 |
| Terraria/MessageBuffer.cs | 48AABBBF4E0967964D97598E0168E9252ADD3DD868C66C6DA46AA87670EACAAB | 4500 | reference-only 网络条件 |
| Terraria/NetMessage.cs | F3066C50715D7C49B8BF2CC852B015303AD7F4C12ADC191C72FC0FE46181E7E2 | 3038 | reference-only 线协议补充 |
| Terraria.GameContent/NPCDamageTracker.cs | A75A5AF322A33B3CAD8ECBC7BF4B3B44AC92B06B5CD46A1404EBFD6FA7A64B8A | 429 | reference-only credit/report 行为 |
| Terraria.GameContent/BossDamageTracker.cs | 386075D5AC32CCE823FF63ADC829209DACF01440DC2E0DE3C76E686334D0E274 | 86 | reference-only 对照；构造归一化与 Version4 相同 |
| Terraria.GameContent/NPCInteractions.cs | F8493FA3E345ECB63211060F9739167756ABA1AEE9F110B0CC181370CA21800A | 667 | reference-only interaction action |

### 2.2 CPG Query API 证据

只读数据库为 D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite，manifest
SHA-256 为 6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364；导入状态
complete，967 shards、8,166,789 nodes、71,038,907 edges、1,317 diagnostics，且
sourceSnapshotId 为 null。

本轮使用 CpgEvidence.ps1 初始化并启动查询服务，再以符号 ID 查询调用点。已复核的
代表性结果：

| 查询 | 结果 | 解释 |
|---|---|---|
| Find-CpgSymbols(NPCDamageTracker.AddDamage) | complete；解析到 `void(Terraria.NPC,int,int)` 与 `void(int,int)` 两个 overload | 方法身份已解析；overload 必须分开查询 |
| Find-CpgCallSites(AddDamage(NPC,int,int), 选定 NPC.cs/MessageBuffer.cs/Main.cs) | complete；2 个 call site，均在 NPC.cs，无 gap | StrikeNPC 与 GetHurtByDebuff 静态调用；不闭合运行时或动态关系 |
| Find-CpgCallSites(AddDamage(int,int), 同一选定范围) | partial；零命中并带 `NoMatchingFactInScannedScope` gap | 不能据此推断该 overload 没有调用点 |
| Find-CpgSymbols(MessageBuffer.GetData) | complete；`void(int,int,int)` 一个符号，无 gap | 只确认方法符号；CPG 不判断 packet switch 中 unconditional break 后代码是否可达 |
| Find-CpgCallSites(BossKilled) | complete；1 个 NPC.cs 调用点 | 死亡事件路径的静态入口 |
| Find-CpgCallSites(Update) | complete；1 个 Main.cs 调用点 | tracker tick 的选定调用点 |
| Find-CpgSymbols(checkDead) / Find-CpgCallSites(checkDead, 选定 NPC.cs/Main.cs/MessageBuffer.cs) | symbol 与 call-site 查询 complete；9 个静态调用点均在 NPC.cs，无 query gap | `Get-CpgCallableFacts` 为 partial，gap=`CalleeEffectsNotExpanded`；不闭合运行时调用或 callee effects |
| Find-CpgSymbols(NPCDamageTracker.AddDamageToLastAttack) | partial；选定 Version4 tracker shard 零命中，带 `NoMatchingFactInScannedScope` gap | Version4 全源码搜索未发现该方法或引用；完整参考树中的方法和 NPC 调用仅为 reference-only |
| Find-CpgSymbols(_lastAttacker) / Get-CpgMemberUses | field symbol complete；member-use partial，零命中并带 query gap | 当前 Version4 tracker 全文件仅声明和赋值 `_lastAttacker`；静态零命中本身不证明没有读取 |
| Find-CpgSymbols(CompareTo) / Find-CpgCallSites(CompareTo) | symbol complete；call-site partial，零命中并带 query gap | 源码实现返回默认 `int`；没有据此推断目标排序或导入完整参考树排序行为 |
| Find-CpgCallSites(GetHurtByDebuff) | complete；3 个调用点 | 入站范围内的 DoT 路径 |
| Find-CpgCallSites(UpdateNPC) | complete；2 个 Main.cs 调用点 | 选定范围内的主循环入口 |
| Find-CpgCallSites(StrikeNPC) | complete；5 个点，来自 MessageBuffer、NPC、Player、Projectile | 受击入口的选定调用点 |
| Find-CpgSymbols(CalculateDamageNPCsTake) | complete；Terraria/Main.cs 中 1 个 double(int,int) 符号，无 gap | helper 身份已解析 |
| Find-CpgCallSites(CalculateDamageNPCsTake, 选定 NPC/Player/Projectile/MessageBuffer) | complete；1 个调用点在 Terraria/NPC.cs，无 gap | 只证明所选 4 个 shard 的静态边；不证明范围外无调用 |
| Find-CpgSymbols(NetMessage.SendData) / Find-CpgCallSites(SendData, 选定 NPC.cs/MessageBuffer.cs) | complete；1 个签名 `void(int,int,int,NetworkText,int,float,float,float,int,int,int)`；89 个静态调用点，无 query gap | 调用点查询不展开 msgType 值或运行时广播；packet 28 字段布局来自 source switch case 28 |
| Find-CpgSymbols(NetMessage.SendPacket) / Find-CpgCallSites(SendPacket, 选定 Terraria/NetMessage.cs shard) | complete；8 个静态调用点，无 query gap | 不按 packet id 筛选，不证明发送运行时闭包；SourceSnapshotId 为 null |
| Find-CpgSymbols(SendData/TrySendData) / Find-CpgCallSites(选定 NPC.cs/NetMessage.cs/MessageBuffer.cs) | complete；122/127 个静态调用点，无 query gap；SourceSnapshotId 为 null | 调用点不按 msgType 过滤，也不证明调用闭包、运行时模式或广播接收者；本轮通过只读 API 复核 |

complete 只表示索引查询在预算内完成，不表示动态派发、反射、事件订阅、调度顺序、
持久化、别名或副作用闭包完成。Unknown access mode、预算耗尽、零命中和
sourceSnapshotId 缺失都继续保留为 evidence gap。当前数据库 SourceSnapshotId=null；查询范围和
manifest 必须与 Version4 源码 hash 分开记录，不能把 CPG 结果当作当前源码绑定证明。

Tracker 与死亡调用边界还受 Version4 当前源码中的 stub 限制：`CreditEntry.CompareTo` 返回默认 `int`，
`_lastAttacker` 只在 `AddDamage` 中赋值；Version4 源树没有 `AddDamageToLastAttack`，而完整参考树在
NPC 分段死亡路径中有对应调用。分段最后一击的 credit 与报告排序行为因此保持 unknown/reference-only。
`checkDead` 的 9 个静态调用点只给出所选 shard 范围，不证明所有生命周期入口或运行时调度闭包。

### 2.3 Packet 23/28 源码控制流复核

| 路径 | Version4 权威源码 | 完整参考树 `D:\TRbackup\无任何删减通过编译` | 证据结论 |
|---|---|---|---|
| packet 23 outbound writer | `NetMessage.cs:680` 起有字段 writer；UpdateNetworkCode 的发送/stream 分支仍需分别按源码路径解释 | `NetMessage.cs:684` 起有对应 writer | Version4 出站 writer 可用于 writer 侧布局；不推出 Version4 reader 可用 |
| packet 23 inbound reader | `MessageBuffer.cs:1182-1184` 的 case 23 立即无条件 `break`；之后读取字段、SetDefaults 和应用状态的代码不可达 | `MessageBuffer.cs:1577-1583` 先按 `Main.netMode != 1` 条件退出，client 路径随后读取字段 | Version4 当前 reader 行为 `unknown`；参考树 reader 仅为 reference-only 行为候选 |
| packet 28 inbound | `MessageBuffer.cs:1406-1437` 读取并应用 damage/death 分支，随后发送 packet 28/23；authority 和畸形输入处理仍需闭合 | `MessageBuffer.cs:1819-1856` 仅在 server mode 标记 interaction 并 rebroadcast/follow-up；Strike owner 随 mode 变化 | 当前 Version4 与参考树存在可观察 mode/owner 差异，不能把参考树分支并入 Version4 基线 |

已结算的 P12 outputReport 在网络 DAG 中把 packet 23 body 描述成有效 hydrate 路径。
本设计按当前 Version4 源码行级控制流将该 reader 结论降为 `unknown`；该更正只限定
packet 23 入站行为，不改变 P12 成员清单，也不把完整参考树行为提升为 Version4 confirmed。

## 3. 目标行为和不变量

### 3.1 观察契约

后续每个迁移切片必须比较以下 Observation，而不是只比较最后几个字段：

~~~text
Observation = (
  return_or_error,
  authoritative_state_delta,
  emitted_events_and_external_effects,
  order_and_visibility,
  lifecycle_and_scope,
  retry_and_idempotency
)
~~~

只有 Observation 全部满足已批准差异，才允许把切片从 proposed 提升到 verified。编译、
静态映射、旧 facade 可调用或局部 verifier 都不能写成 migration-success。

### 3.2 P12 不变量

1. 一个权威事实只有一个最终写者；Query、Projection 和 Adapter 不写回权威 Component。
2. 一个 accepted strike 至多产生一次生命提交和一次 tracker credit，并只进入一次死亡决策；
   普通 immortal strike 可被接受，但生命 delta 与 tracker credit 均为零。
3. 普通 Strike 路径存在 realLife 时，父 NPC 生命提交和子段镜像必须在同一明确的提交协议中完成；GetHurtByDebuff 是 Version4 明确的例外，直接扣 parent life 后不镜像 child。
4. packet 23/28 先完成输入验证，再交给 authority System；Adapter 不能自行创造权威状态。
5. tracker 只记录已经由 Combat System 接受的伤害，不重复执行 defense、crit 或 multiplier。
6. 网络、掉落、进度、UI、音效、持久化均通过显式 effect port 或 Projection 出站。
7. world/session、连接、slot generation 和 tracker 生命周期不能依靠进程静态集合隐式共享。

### 3.3 Damage 数值合同

Version4 的 Main.CalculateDamageNPCsTake(int Damage, int Defense) 返回 double，基础结果为
max(1, Damage - Defense * 0.5)。NPC.StrikeNPC 随后按固定次序处理：

1. 不满足 active/life 前置条件时返回 0；否则调用上述 helper。
2. crit 为真时乘 2。
3. RedHatSkeletronAdjustmentsEnabled() 为真时先乘 0.699999988079071，转换为 int 截断，
   再将结果下限设为 1。
4. 仅 takenDamageMultiplier > 1f 时应用该倍率；等于或小于 1 不乘入结果。
5. 后续 tracker credit 与父/本体生命写入使用 (int)num。realLife >= 0 时扣父 NPC 生命，
   再把父生命和 lifeMax 镜像到子段。

Version4 在 `NPC.cs:67625-67629` 以原始 `Damage >= 9999 && owner == 255` 判断 forced-world
分支：该分支跳过 `NPCDamageTracker.AddDamage`，但仍继续生命提交和后续死亡路径。目标代码通过
`NpcDamageRequest.FromLegacyOwner` 保留该显式输入合同；没有 `LegacyOwner` 的 world contributor
不会被隐式猜成 owner 255。

普通 `StrikeNPC` 对通过其他准入检查、life owner 仍存活的 immortal NPC 不应作为拒绝处理。
Version4 `NPC.cs:67623` 的 `if (!immortal)` 同时包住 tracker credit 与本体/parent life 写入；
后续 knockback 分支位于该条件之外。目标 `NpcCombatSystem` 因而返回 accepted 结果
(`Applied: true`、`RejectionReason: None`、`AppliedDamage: 0`)，跳过 tracker、life commit 和
parent-to-segment mirror，但继续既有 death reconciliation。该 System 不表示 knockback、justHit、
AI reaction 或 HitEffect 已迁移；这些效果仍属 partial/unknown。此 Strike 规则与下文 immortal
DoT 规则分开：DoT 仍记录 capped world credit 和 CombatText intent，只是不写生命。
另有顺序差异：目标 `ResolveAndCommit` 当前在返回前执行 death reconciliation，而 Version4 在
knockback、HitEffect 和音效之后才调用 `checkDead`；在有明确编排点前，不把 immortal Strike 的终态时序
记为等价。

`NPC.GetHurtByDebuff(amount)` 是独立的 DoT 语义：先以 owner 255 调 tracker（tracker 将 credit
限制为当前 parent life），再解析 realLife 并对非 immortal owner 直接减去原始 amount；此分支不做
defense、crit 或 takenDamageMultiplier 解析，也不把 parent 的 life/lifeMax 镜像回 child。随后
在 child 位置请求 DoT CombatText；若 owner 已致死，将 life 设为 1，再调用
`StrikeNPCNoInteraction(9999)` 并发送 packet 28。目标 `ApplyDamageOverTime` 已实现 raw direct
health 与 capped world credit；致死时用 owner=255、damage=9999 的 forced-world request 调用
`ResolveAndCommit`，回传 strike result、最终 life 和 packet 28 intent，且不镜像 child。它在
direct stage 之后、forced strike 之前同步调用必需注入的 `INpcDamageOverTimeTextPort`，并将
发布结果写入 DoT result；预期投递失败以 `false` 返回，不阻断权威 strike。当前只有该调用边界，
没有连接 Terraria CombatText sink。DoT packet 28 intent 仅作为 Combat 输出的本地意图保留；
packet 23/28 编解码、recipient 选择与 publish 均由 `INpcReplicationPacketApi` 声明，当前不提供
函数体或 DTO 字段。Version4 的 `ignoreClient`、broadcast、connection、section-active predicate
和 slot 顺序仍作为源码证据，不由目标代码执行。目标没有已证明的 connection/section snapshot
映射，`NpcReplicationDirtyState.ClientStates` 按 `ulong` key，不能据此推导 legacy slot 等价。
入站授权、slot generation、mode gate、transport/retry 与日志均保持 unknown。

Version4 `NetMessage.SendData` 的 packet 28 广播分支位于 `Terraria/NetMessage.cs:1737-1747`；
完整参考树同类分支位于 `:1760-1770`，两处 predicate 一致。完整参考树在该方法入口有
`Main.netMode == 0` 早退，当前 Version4 的 `SendData` 对应入口没有此判断。该 mode 差异只作
reference-only 记录，不移植到候选策略。Version4 `SendPacket` 在 `:1831-1859` 对每个 client
send 捕获异常、记录 `Netplay.LogHandshake` 并继续；完整参考 `:1861-1885` 也捕获后继续，但不作
同样日志。早期本地 packet transport 原型曾把失败 slot 返回给调用者，但该实现已随 API 收敛而移除；
当前目标没有 runtime Adapter 或 transport port，日志/诊断映射与调用者接线仍 unknown。调用者
模式门、packet 23、完整 death effects 和 parent/child network 同步仍未接通，不可视为完整 DoT 迁移。

完整参考树的 `NPC.GetHurtByDebuff` (`Terraria/NPC.cs:93683` 起) 仅在 `netMode != 1` 时记
tracker credit，并仅在 `netMode != 1` 时执行 lethal forced strike；packet 28 只在
`netMode == 2` 时发送。当前 Version4 对应方法 (`NPC.cs:78077` 起) 没有这些 mode gate，直接
记 credit、提交 lethal strike 并调用 `SendData(28)`。完整参考树的 packet 28 reader
(`MessageBuffer.cs:1819` 起) 仅在 server mode 条件内标记 interaction 并 rebroadcast/发送后续
packet 23；strike 分支本身仍执行，owner 在 server mode 下取 `whoAmI`，否则取 255。Version4
(`MessageBuffer.cs:1406` 起) 对应分支没有这些 mode gate。以上均是 reference-only 差异，
不改变本设计以当前 Version4 为权威的候选行为；目标的 server/client authority policy 仍
partial/unknown，需在 NetworkApply/Combat owner 集成时单独裁定。

Red Hat 判定读取 NPC type 与 ai[3] / localAI[3]：type 35、36、32、33 的不同 AI slot
条件会启用该调整。目标 NpcBehaviorComponent 当前只列出 aiStyle、Action、ai[4]，没有
localAI 和完整行为写入闭包；NpcDamageRequest 现在接收显式的
RedHatSkeletronAdjustmentEnabled 输入。因此 Combat 不在内部猜 AI 规则，但跨 System 的行为
快照 owner、revision 和调度时点仍为 unknown。

源码锚点：Version4 Main.cs:14254；NPC.cs:53188、67461、67485-67500、67620 附近。完整参考
Main.cs:67056；NPC.cs:67615、82485、82509-82524。两个源码树的 hash 分列于 2.1。

可复核的数值例：Damage=40, Defense=10, crit=true 的 Version4 基础结果为 70；若上述 Red Hat
调整为真，则在后续倍率前截断为 48；takenDamageMultiplier=0.5 不改变 Version4 结果。当前
NpcCombatSystem 已按该顺序实现局部纯计算，并由 request 显式提供 Red Hat 判定；调用方尚未
证明 AI 快照的解析与传递闭包，故该局部结果不升级为完整行为等价。

### 3.4 Knockback 数值与提交边界

Version4 `NPC.StrikeNPC` (`Terraria/NPC.cs:67641` 起) 仅在 `knockBack > 0` 且
`knockBackResist > 0` 时处理 knockback。初始幅度为 `knockBack * knockBackResist`，`onFire2`
时乘 `1.1`；随后依序对大于 8、10、12、14 的超额部分乘 `0.9`、`0.8`、`0.7`、`0.6`，大于
16 时截为 16，最后 crit 再乘 `1.4`。crit 的倍率发生在 16 截断之后，所以最终值可大于 16。

分支边界 smoke 使用 type 185、knockback 10、resistance 1、resolved damage 10、life maximum 100、
非 expert。初始幅度先经 `> 8` 阻尼变为 9.8，伤害阈值等于 life maximum；由于源码判断是严格 `>`，
走低伤害分支，结果速度为 `(-9.8, -7.35)`，不会应用 type 185 的高伤害垂直倍率。

分支阈值为 `(int)resolvedDamage * 10`，expert mode 时改用 `(int)resolvedDamage * 15`，严格大于
`lifeMax` 才进入高伤害分支。低伤害分支直接重设 X/Y 速度：X 是幅度乘 hit direction 和
`knockBackResist`；Y 是负幅度乘 `0.5`（`noGravity`）或 `0.75`，并再次乘 `knockBackResist`。
高伤害分支按 hit direction 修改并限幅水平速度；若 NPC type 为 185，再把幅度乘 `1.5`，并以
`noGravity ? -0.5 : -0.75` 生成垂直冲量，只在当前 Y 速度尚未达到该冲量时应用和限幅。type 185
倍率只作用于这个高伤害分支的垂直方向。

目标 `NpcMovementSystem.CalculateKnockback` 把 type、命中时速度、knockback 输入、抗性、`onFire2`、
crit、已解析伤害、life maximum、expert mode 和 `noGravity` 作为显式输入，返回 eligible 标记和速度
前后值。`ApplyKnockback` 将计算结果提交到 `MovementStateComponent.Velocity`；候选编排
`NpcCombatSystem.ResolveAndCommitStrike` 在伤害/追踪提交后调用 Movement，再进行 death reconciliation。
核心 smoke 覆盖该局部组合的 velocity delta 和阶段结果。该提交尚无已证明的生产调用方或完整共享
velocity writer 闭包，因此仍是候选 owner 接线；历史、碰撞、状态刷新、`justHit`、HitEffect、AI
reaction、音效和 network projection 仍 partial/unknown。

Version4 与完整参考树对应 knockback 片段分别位于 `NPC.cs:67641` 和 `NPC.cs:82668`；核对范围内
公式顺序和 type 185 分支一致。CPG `StrikeNPC(double(int,float,int,bool,bool,bool,int))` 符号及
所选 `NPC.cs`、`MessageBuffer.cs`、`Player.cs`、`Projectile.cs` 直接调用查询为 complete，得到 5 个
静态调用点且无 query gap；`Get-CpgCallableFacts` 为 partial，不能封闭 callee effects。索引
manifest 为 `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，
`SourceSnapshotId=null`，所以行为公式以 Version4 源码为准，查询结果只支持所选范围内的结构关系。
源码还确认相关可观察顺序为 `justHit` 预先置位 (`NPC.cs:67525`)、tracker/生命提交
(`:67623-67640`)、knockback (`:67641` 起)、HitEffect 和音效 (`:67749` 起)、最后对父体或本体调用
`checkDead` (`:67816-67824`)。目标当前 `NpcCombatSystem.ResolveAndCommit` 在返回前已经调用
`NpcDeathLifecycleSystem.Reconcile`；所以未来的 Movement 提交若在该 API 返回后才执行，会晚于死亡结算，
与 Version4 次序不同。必须先给 combat result、movement commit、效果 projection 和 death reconcile
确定一个协调提交序列，再接通这条路径。

## 4. System 边界决定

### 4.1 选定的候选 System

| 候选边界 | 主要权威状态 | 读集 | 写集/提交点 | 出站效果 | 状态 |
|---|---|---|---|---|---|
| NpcAuthoritySystem | identity、type/netID compatibility、target/AI commit token | definition、target、输入快照 | identity generation、AI/target transition | lifecycle/network intent | proposed；稳定 ID、AI 完整写闭包 unknown |
| NpcMovementSystem | position、velocity、old history、medium、teleport/collision | world/collision/status/authority snapshot；knockback 使用命中时速度和 combat snapshot | movement commit、history、teleport；`ApplyKnockback` 写入 MovementState velocity | movement/network intent | proposed；knockback 候选提交接入 `ResolveAndCommitStrike` 且核心 smoke 通过；生产调用、完整 velocity writer、物理/碰撞/传送仍 partial/unknown |
| NpcStatusSystem / NpcStatusTickSystem | buff slots、immunity、status flags、shimmer transparency、life-regeneration accumulator | buff slots、immunity、NPC type/AI、projectile/player/world snapshots、life owner | active guard -> rebuild flags/timers -> Soul Drain visual phase -> expired-slot cleanup -> DoT/regen calculation and optional Combat commit -> shimmer transparency transition -> Blood Moon transform intent -> gravity result | ordered Packet 54 buff-sync request, water-perishable cleanup intent, Soul Drain visual port, transform intent, gravity calculation | proposed；local phase composition now calculates shimmer transparency from explicit inputs; production scheduler/caller, remaining buff VFX, packet 23 hydrate, AI consumer wiring, transform/gravity state consumer, life<=0 cleanup, packet transport, and water-helper behavior remain partial/unknown |
| NpcCombatSystem | damage admission、accepted amount、parent/local life delta、hit flags | combat policy、status、owner attribution、health/parent link | 唯一生命提交和 combat result | tracker command、death intent、hit effect port | proposed；generic DamageResolution 不等价 |
| NpcDeathLifecycleSystem | terminal lifecycle、phase decision、cleanup token | health、parent/segment、combat result、progress rules | terminal lifecycle commit；有前置效果 intent 时标记 `TerminalCommitPending`，调用方交付效果后用 `CommitAfterPreTerminalEffects` 显式提交 | announcement、LadyBugKilled、loot/progression/network/death effect intents | proposed；目标只返回 guard、四组 phase 子集、type 35/604/605 前置效果及 GoodWorld type 13/36 spawn intents；type 400 只返回 spawn intent；消费顺序、AI 单写者、Combat 生产调用、其余 phase 和跨域效果 partial/unknown |
| NpcDamageTrackingSystem | active/recent registry、credit、tick/expiry、canonical tracker NPC type | accepted damage、definition strategy、clock、kill event | tracker credit、start/stop/advance/mark-killed | immutable tracker snapshots | proposed；composite type canonicalization implemented；parent解析、清空方法、world scope partial |
| NpcReplicationAdapter | packet API declarations、经核实后的 validated apply；packet 23/54 inbound 不定义行为 | authority snapshot、connection/section | 不写 authority；生成 NetworkApplyCommand 或 bytes | packet 23/28 declarations、Packet 54 buff-slot publish declaration、transport send/broadcast | proposed adapter；packet 23/54 reader unknown，packet implementation/authorization/malformed-input/reconnect partial |
| NpcNetworkSyncProjection（候选形状，无现存同名类型） | per-client skip/ack 与 stream cursor 的只读决策输入 | committed snapshot、connection | 只能产出 cursor/selection intent，不能写 NPC authority | packet selection metadata | proposed；当前目标仅有 `NpcReplicationDirtyState.StreamCursor` 与 `NpcClientReplicationState.SkippedSyncCount/LastAcknowledgedRevision`，连接绑定不等价仍 unknown |
| NpcInteractionSystem | interaction availability、registry、commerce intent | talk NPC/player/session、definition | registry init、explicit interaction result | UI/chat/shop/quest adapters | proposed；当前动作存根化 |
| NpcPresentationProjection | combat text、frame/trail/name/sound intent | committed snapshots、events | 无权威写入 | presentation sink | separate projection；效果顺序 partial |
| NpcDamageDefinitionCatalog | immutable boss-mob/composite type-group definition | content bootstrap | 单次 snapshot | definition query | static registration set implemented and matched to current Version4; reload/persistence and invasion strategy unknown |

不选择单一 NpcSystem：它会同时持有 189 个成员、AI、状态、生命、tracker、网络和
交互效果，无法形成独立提交点。也不按每个字段或每个 NPC type 建 System；realLife
父子关系、网络快照和 tracker grouping 要求按不变量分组。

当前目标实现覆盖 `NpcStatusSystem.ApplyBuffSlotPhase`、`ClearExpiredBuffs`、DoT/regen phase，
并由 `NpcStatusTickSystem` 按 Version4 顺序局部组合：inactive guard、buff flags、Soul Drain visual、
过期 slot 清理、DoT/regen 计算和 Combat life commit。buff flags 重建已映射状态、递减活动 timer、
处理 type 1 的火焰/霜焰刷新及 type 353 免疫删除；sync intent 对应 Packet 54 高层发布函数声明，
该 API 仍无编解码或传输实现。Soul Drain eligibility 与 visual consumer 已有独立边界。

这仍不是生产 `UpdateNPC` coordinator：没有 scheduler、active-NPC snapshot owner 或生产 caller；
`UpdateNPC_BuffApplyVFX`、Blood Moon Transform consumer、gravity state consumer、life<=0 network cleanup、buff sync transport
均未接通。Version4 `TryRemovingWaterPerishableEffects(bool)` 方法体为空，而完整参考树包含模式受限的
buff 删除逻辑，因此该 intent 的运行时效果为 `unknown`，不移植 reference-only 行为。DoT counter 已提交后
Combat 拒绝/异常的回滚与重试语义仍为 `unknown`。不以局部实现或历史 smoke 声称 P12 迁移成功。

### 4.2 Component、Query、Command、Adapter、Projection 角色

| 角色 | 建议内容 | 约束 |
|---|---|---|
| Component | identity/lifecycle、movement/history、status slots、combat/life、replication cursor、interaction state | 只存所属状态；数组和跨实体引用须有 generation/reset 契约 |
| Query | Definition、Eligibility、ParentLifeLink、Interaction、ReplicationSnapshot、TrackerCredit 和同 revision 行为事实 | 只读；damage 数值规则留在 Combat API，knockback 计算属于 Movement API，不额外拆出调度节点 |
| Command | Spawn、NetworkApply、Damage、DebuffDamage、MarkKilled、Reset/Despawn、Interaction、AdvanceTracker | 由唯一 owner commit；排队前定义可见时点、失败和幂等 |
| Adapter | PacketCodec、NetworkTransport、Player/Projectile attribution、Loot/Progression/Commerce、Persistence | 只转换协议和外部 effect；不得实现第二套 combat/death |
| Projection | packet payload、credit report、combat text、presentation snapshot、interaction menu | 只读取已提交 revision；失败不得回写 authority |
| Definition/Registry | damage grouping、NPC type/netID、interaction registration | bootstrap/reload 只有一个 writer，顺序和重复初始化显式定义 |

## 5. Legacy API 到新组合

下表是概念 API，不是已存在的 C# 签名。每个入口都必须经过同一 commit owner。

| Legacy 入口 | ConceptId | Proposed composition | 唯一提交者 | 证据 |
|---|---|---|---|---|
| NPC.SetDefaults/ResetForNewNPC/NewNPC | NPC.SpawnIdentity | DefinitionQuery -> SpawnCommand -> Authority/Lifecycle commit -> projection | Lifecycle/Authority owner | current source structure confirmed；defaults 全效果 partial |
| NPC.UpdateNPC(i) | NPC.Tick | `BuffFlagsReset -> BuffSetFlags -> SoulDrainDebuff -> BuffClearExpiredBuffs -> BuffApplyDOTs`（其余 tick stages 另行组合） | 各自 owner，遵循已确认的局部次序 | Version4 status 前缀源码顺序 confirmed；候选 status phase 冒烟覆盖，生产 scheduler、动态 AI 与 DoT 聚合 partial/unknown |
| NPC.StrikeNPCNoInteraction -> StrikeNPC | NPC.DamageAdmission | eligibility -> accepted damage/tracker/life commit -> knockback snapshot/calculate/velocity commit -> HitEffect/sound projection -> parent/local death reconcile | Combat writes life; Movement owns velocity commit; Death owns terminal transition | Version4 order confirmed；target `ResolveAndCommitStrike` implements and smoke-verifies damage/tracker -> velocity commit -> death reconcile, but production caller, `justHit`, HitEffect/sound and full parent/death composition remain partial/unknown |
| Player.ApplyDamageToNPC | NPC.PlayerDamage | Player attribution -> DamageCommand -> combat result -> player effect projection | NpcCombatSystem | Player caller confirmed；post-hit effects partial |
| Projectile NPC-hit path | NPC.ProjectileDamage | projectile owner -> DamageCommand -> combat result -> projectile projection | NpcCombatSystem | Projectile caller confirmed；owner variants partial |
| NPC.GetHurtByDebuff | NPC.DebuffDamage | status tick -> NpcCombatSystem.ApplyDamageOverTime (raw direct amount + capped world credit) -> synchronous text Port -> forced owner strike commit -> packet 28 intent -> declared packet API | Combat plus tracker; effect/network adapters remain separate | local effect order partial; packet API is declaration-only; real text sink/packet dispatch unknown |
| NPCDamageTracker.AddDamage/BossKilled/Update | NPC.DamageEncounter | accepted damage/kill/tick -> tracker System -> immutable snapshots | tracker System | CPG call sites confirmed；strategy gaps partial/unknown |
| NetMessage.SendData(23/28) | NPC.NetworkPublish | committed intent -> `INpcReplicationPacketApi` declarations | network adapter | packet 28 helper implementations are out of scope; packet 23 Version4 outbound writer exists, full send closure partial; API behavior unknown |
| MessageBuffer packet 23/28 | NPC.NetworkApply | decode declaration -> schema/authority Query declaration -> NetworkApplyCommand -> Authority/Combat | authority/combat | packet 23 Version4 reader body unreachable，入站行为 unknown；packet 28 当前 reader 顺序可见，malformed/auth partial |
| NPCInteractions.Initialize/Shop/Register | NPC.InteractionRegistry | bootstrap definition -> registry commit -> availability Query -> InteractionCommand -> adapter | Interaction/registry | registration confirmed；action behavior current unknown |

数据包边界后续只新增或调整函数声明，不实现 parser、writer、recipient 选择、transport 或 authority apply 的逻辑。
签名和结果 DTO 仍是 proposed；packet 23 inbound 的基线与 schema 未裁定前，相关声明不得暗示行为已确认。

### 5.1 数据包 API 声明范围

以下是供后续接口评审的候选函数签名，不定义 DTO 字段或函数体。本次范围只包含声明；
不实现字段解析、编码、recipient selection、transport、授权或权威 apply 逻辑。
packet 23 inbound schema 继续为 unknown，不能从完整参考树 reader 推定 Version4 行为。

| API | 候选签名 | 状态 |
|---|---|---|
| packet 23 decode | DecodePacket23(ReadOnlyMemory<byte> payload): Packet23DecodeResult | 声明候选；reader 与 DTO schema unknown |
| packet 23 encode | EncodePacket23(Packet23EncodeRequest request): Packet23EncodeResult | 声明候选；writer 对照可读，完整发送闭包 partial |
| packet 28 decode | DecodePacket28(ReadOnlyMemory<byte> payload): Packet28DecodeResult | 仅函数声明；目标不含本地解码 helper |
| packet 28 encode | EncodePacket28(Packet28EncodeRequest request): Packet28EncodeResult | 声明候选；transport 和调用模式 unknown |
| recipient selection | SelectRecipients(PacketRecipientRequest request): PacketRecipientPlan | 声明候选；slot/section snapshot binding unknown |
| publish | Publish(PacketPublishRequest request): PacketPublishResult | 声明候选；runtime transport/retry unknown |

关键 damage 组合：

~~~text
DamageEligibilityQuery
  -> accepted amount and attribution
  -> NpcCombatSystem.ResolveAndCommit
       -> parent or local life commit
       -> CombatResult(revision, acceptedAmount, lethal, parentId)
  -> NpcDamageTrackingSystem.RecordAcceptedDamage
  -> NpcDeathLifecycleSystem.Reconcile
  -> NpcReplicationAdapter / NpcPresentationProjection / effect ports
~~~

Combat 不把 tracker credit 当作生命提交，也不把 packet 28 当作客户端可直接写生命的权限。
Debuff lethal 路径必须保证直接 decrement、随后 StrikeNPCNoInteraction(9999) 和 packet 28
被新组合压成一个可观察协议，或者把差异列为批准的行为变更。

DoT 与普通 damage 不共用 `ResolveAndCommit` 的 damage formula：`ApplyDamageOverTime` 仍由
`NpcCombatSystem` 作为同一 health owner 提交，调用 `NpcDamageTrackingSystem` 作为唯一 tracker
owner；它以 World contributor 记录 `min(amount, lifeBefore)`，immortal 时保留 credit 并不写 health。
致死时 direct stage 将 owner health 暂置为 1，随后以 owner=255、damage=9999 的 forced-world
request 复用 `ResolveAndCommit`，并在结果中暴露 strike result、direct/final life 与 packet 28 intent。
DoT 的 child health 不同步。Request 携带 child identity 与 CombatText 矩形快照；result 的文本
intent 保留 child instance ID、矩形和 raw amount，packet 28 intent 保留 life-owner instance ID、legacy
slot 和 9999 damage。CombatText intent 现在交给同步注入的 `INpcDamageOverTimeTextPort`，调用点
位于 forced strike 前；返回的 `CombatTextPublished` 记录该次发布是否成功。10% smoke 可观察到
发布时 owner 仍 active，并覆盖 Port 返回 false 后仍完成 forced strike。`INpcReplicationPacketApi`
只声明 packet 23/28 decode/encode、recipient selection 和 publish 函数；嵌套 request/result 类型
不提供字段。Version4 packet 23 reader 不可达，packet 28 malformed/auth 和 mode 行为未闭合，
因此目标不提供 wire codec、recipient policy、transport 或 apply 函数体。DoT 的本地
`NpcDeathPacket28Intent` 只记录 Combat 产生的待发布意图，不证明已发送或协议等价。真实 Terraria
text sink、packet 28 transport/recipient/mode gate、完整 death effects 仍未接通。

## 6. 依赖图、phase 和 barrier

~~~text
Definition/bootstrap
  -> Spawn/Network validation Query
  -> Lifecycle/Authority commit
  -> Status reset/apply/DoT input
  -> Authority AI/target snapshot
  -> Movement/collision commit
  -> Combat damage/debuff commit
  -> Damage tracker credit/kill event
  -> Death terminal reconciliation
  -> Network, presentation, commerce projections
~~~

至少需要以下 barrier：

1. Status inputs 在 damage eligibility 读取前可见。
2. Parent realLife life commit 在 child mirror 和 death decision 前可见。
3. accepted amount 提交后才允许 tracker credit。
4. Authority snapshot 冻结后才编码 packet 23。
5. NetworkApply 验证完整后才一次性提交。
6. slot reuse、disconnect、world unload 先清理 generation/cursor/tracker 引用，再发布新实体。

Version4 只直接证明 UpdateNPC 的局部顺序和 Main 对 tracker/update 的选定调用，未证明
上述新 phase 在目标调度器中存在。调度、并发和多 world 隔离是 integration gate。

## 7. 生命周期和副作用

| 阶段 | 当前源码观察 | 目标设计合同 | 状态 |
|---|---|---|---|
| bootstrap | tracker 静态注册；NPCInteractions.Initialize 注册 shop/action | 一个 world/session definition snapshot；重复初始化幂等或拒绝 | partial |
| spawn/reset | NewNPC/ResetForNewNPC/SetDefaults 改 slot、defaults、active、AI、关系 | Lifecycle commit 原子发布 generation | partial |
| packet hydrate | Version4 packet 23 writer 可见；对应 MessageBuffer reader 在无条件 `break` 后有不可达 body | 先裁定 Version4 当前基线与参考树 reader 的差异；只有 baseline 明确后才能定义 apply 行为；packet API 仅声明 | packet 23 inbound unknown |
| tick | UpdateNPC 先 status/DoT，再 AI、movement、network、CheckActive | scheduler barrier 固定；clock/random/world 走端口 | partial |
| damage | StrikeNPC 计算 amount、改 justHit/AI、写 life、HitEffect、checkDead | Combat 唯一 life writer；effect 只读 result | partial |
| tracker | active/recent、3 上限、54000 过期、world/player credit | session-scoped registry；tick/expiry/credit 可重放 | partial |
| death/despawn | checkDead 和 UpdateNPC 都可能触发 active/cleanup/网络 | DeathLifecycle 单次 terminal transition | partial |
| network send | packet 23 有 spam、skip、stream、town 分支；packet 28 广播 predicate 与 per-recipient failure continuation 已映射 | runtime transport 失败日志、重连、mode caller gate、ack、cursor reset 和 recipient snapshot binding 可观察 | partial/unknown |
| interaction | 当前只有注册结构，动作方法被清空 | Query 纯读，Command 显式调用 adapter | unknown |
| persistence/session | focused closure 未闭合 save/load/disconnect/multi-world | 明确 scope、取消、恢复和 schema 版本 | unknown |

建议的出站端口（概念名称）：INpcWorldQuery、INpcCollisionPort、INpcDamageEffectPort、
INpcLootPort、INpcProgressionPort、INpcNetworkTransport、INpcPacket28TransportPort、INpcPersistencePort、
INpcInteractionAdapter、INpcPresentationSink、INpcPlayerProjectilePort。端口必须报告
错误或重试结果，不得静默修改 authority。

## 8. 当前目标树的对齐

| 目标文件/形状 | 观察 | 设计处理 |
|---|---|---|
| Component/Npc/NpcDamageTrackingSystem.cs | active/recent、3、54000、snapshot、首次受击 type 与 canonical tracker type | combat 使用 life-owner type 选择 tracker group，并把实际命中 type 单独传入 `InitialNpcType`；父子首次命中 smoke 已覆盖未登记 segment type；复合组仍以 `NPCTypes[0]` 为 canonical type；realLife 全闭包、当前存根和 session scope 尚未等价 |
| Component/Npc/NpcDamageDefinitionCatalog.cs | 当前 Version4 boss-mob 和 composite 分组 | 静态登记集已对照权威源与完整参考源；reload、持久化和 invasion strategy unknown |
| Component/Npc/NpcHealthComponent.cs + NpcCombatSystem.cs | health 由 combat API 内部提交，含普通 strike 的公式/parent mirror、DoT raw-direct phase 与 lethal forced strike commit | realLife slot 到当前实体解析、generation freshness、CombatText/packet adapters、phase/death effects、AI、网络仍 partial/unknown |
| Component/Npc/NpcMovementSystem.cs + NpcKnockbackRequest.cs + NpcKnockbackResult.cs | knockback eligibility、幅度阻尼/crit、damage threshold、水平速度和垂直冲量的计算；`ApplyKnockback` 写 `MovementStateComponent.Velocity` | 输入快照显式化；当前候选 writer 通过 `NpcCombatSystem.ResolveAndCommitStrike` 调用 | 局部 velocity commit 有核心 smoke；生产入口、history、collision、完整 velocity ownership 和网络投影仍 partial/unknown；type 185 分支只在高伤害计算中体现 |
| Component/Npc/NpcDeathLifecycleSystem.cs + NpcDeathPhaseQuery.cs | 对显式 life-owner/AI 输入评估已确认的 checkDead phase 子集、type 35/604/605 前置效果及 GoodWorld type 13/36 spawn 计划，并返回 decision；terminal transition 只提交 lifecycle 自身状态 | `NpcCombatSystem` 仅在调用方显式提供 `NpcDeathPhaseContext` 时调用 phase-aware lifecycle 并把结果返回；AI、health、damage-policy 和外部效果仍由对应 owner 消费，当前无生产 caller；type 400 与 GoodWorld spawns 只返回 intent；tile/random/spawn/sync consumers、parent/segment 清理和其余 death effects 仍 partial/unknown |
| Component/Npc/NpcIdentityComponent.cs | status proposed，稳定 ID evidence missing | 不把 instance ID 当 whoAmI/netID 等价 |
| Component/Npc/NpcBehaviorComponent.cs | aiStyle/aiAction/ai[4] partial | AI 写闭包和动态 dispatch 未闭合 |
| Component/Npc/NpcInteractionStateComponent.cs | playerInteraction/lastInteraction partial | 不决定 commerce command owner |
| Component/Npc/NpcReplicationDirtyState.cs + NpcClientReplicationState.cs | `StreamCursor`、`SkippedSyncCount`、`LastAcknowledgedRevision` 与 dirty intent | 没有 `NpcNetworkSyncProjection.cs` 或已证明的 client snapshot binding；legacy skipped/stream/ack/reset 语义仍 partial/unknown |
| Component/Npc/Network/INpcReplicationPacketApi.cs | 只声明 packet 23/28 decode/encode、recipient selection 和 publish；request/result DTO 为空壳 | 无实现、字段 schema、transport 或 authority apply；Version4 packet 23 inbound schema unknown，packet 28 authority/mode partial/unknown |

设计文档不替代生产代码接线或完整验收。当前已落地 tracker、catalog、普通 parent health、DoT lethal
forced-strike commit、带目标数据的文本/packet intent、局部 strike knockback velocity commit，以及显式
death-phase context 到 combat result 的候选组合。无 context 的 combat 调用仍走原通用 lifecycle 路径；
phase decision 尚无生产 caller/consumer。其他候选边界仍需在 src/NSSLC 的 Npc、Combat 和
Adapter/Projection 目录按实际职责逐项接入，并先完成写者盘点和目录约束评审。

## 9. 完整参考树的限制性结论

完整参考树含当前 Version4 缺少的关系：CreditEntry.CompareTo 按 Damage 降序；
AddDamageToLastAttack 用于分段 Boss 后续死亡 credit；RecentAttempts、GetReport、
百分比/本地化/当前玩家高亮；BossDamageTracker 和 InvasionDamageTracker 的策略；
NPCInteractions.OpenShop 和其他 UI、聊天、治疗、任务动作；以及更完整的 netMode gate。

网络对照必须单独处理：Version4 的 packet 23 reader 在 `MessageBuffer.cs:1184` 无条件退出；
完整参考树在 `MessageBuffer.cs:1579-1582` 仅于非客户端模式退出，并在 client 路径继续解析。
虽然两棵树都能看到 packet 23 writer，完整参考 reader 不能补成当前 Version4 的有效入站行为。
Version4 packet 28 reader 在 `MessageBuffer.cs:1417-1435` 无条件执行 interaction/strike/rebroadcast；
参考树在 `:1826-1855` 将 interaction/rebroadcast 限制于 server mode，并改变 owner 选择。
该差异需作为 baseline 决定，不能据“完整参考树”名称直接移植。

另外，完整参考树的 Main.CalculateDamageNPCsTake helper 与当前 Version4 公式相同；NPC.StrikeNPC
也保持“基础公式 -> crit -> Red Hat 截断/下限 -> takenDamageMultiplier > 1”的顺序。该对照来自
各自源码快照的独立读取（Version4 Main.cs SHA-256 为
66DBE1D1E6FB89A24A512309BB039DFEC7C06D4678D2A22D7ADECBAD3AFD6520；完整参考 Main.cs SHA-256 为
E24E61C9903BB7995F47E36C51EDBE43481B643226861B717020877E7E22B63F），只是对当前行为的交叉印证，
不会把 reference-only 的 tracker、AI 或网络行为提升为 Version4 confirmed。

这些都是 reference-only 或 candidate relationship。当前 Version4 的 CompareTo、
InvasionDamageTracker.IncludeDamageFor 和多个交互动作是清空/存根形态，不能写成
“无排序”“不包含入侵”或“交互无副作用”。实现前必须决定当前 Version4 还是完整参考
作为行为基线，并为差异建立批准记录。

## 10. 阻塞项与 integration handoff

| 阻塞项 | 状态 | 必须补的证据/决定 |
|---|---|---|
| stable identity、slot generation、netID、realLife parent/segment | partial/unknown | generation、父生命权威、child mirror、reuse cleanup |
| life/health/death 单 writer | partial | 所有 life/active/realLife 写者、lethal debuff、checkDead 重入 |
| damage eligibility | partial | defense、crit、multiplier、immortal、legacy owner normalization 已有局部合同；slot/name、mode gate 仍 unknown |
| status/DoT ordering | partial | buff reset/apply/expire、immunity、regen 到 combat 的唯一路径 |
| tracker credit semantics | partial/unknown | CompareTo、last attack、invasion/boss strategy、clear method 行为基线 |
| network packet 23/28 | partial/unknown | schema、authority、malformed、recipient、skip/stream、reconnect/version |
| interaction/commerce | unknown | current action bodies、talk NPC scope、UI/quest/progression failure |
| persistence/session/multi-world | unknown | save/load、disconnect、unload、cancel、scope isolation |
| scheduler/dynamic AI/mod/reflection | partial/unknown | registration、subscriber、virtual target、phase visibility |
| target duplicate writers | unknown | 逐成员读写盘点，确认 src/NSSLC 单一注册 owner |

交 integration-review 的输出必须包含 owner、输入/输出、失败、重试、生命周期范围和
删除旧 writer 的条件；只给类型名或文件路径不足以关闭阻塞。

## 11. 迁移行为和回滚合同

后续实现应先 shadow/read-only 比较，再在单一 barrier 切换唯一写者。旧 facade 可以作为
输入适配，但不得同时写新旧两套权威状态。回滚必须停止新路由、丢弃未提交状态、恢复旧
adapter 唯一写入、保留已经发布 effect 的补偿记录，并记录触发回滚的 Observation 差异。

没有持久化 schema、网络协议、effect retry 和 slot generation 证据时，不得把 shadow
输出发送给真实外部消费者。回滚路径本身也要通过后续行为验证。

## 12. 验证边界

完整 P12 的验证仍未运行，migrationStatus 仍为 not-claimed。此前核心 smoke 中的 packet 28
payload、recipient/broadcast-plan 和 fake port 断言已随 packet API 收敛为声明而移除。之后受影响
verifier 已构建并运行 10% combat/life/tracker/death 核心 smoke，包含普通 immortal parent/segment
strike 的零 life delta、无 tracker credit 和无 child mirror 断言，以及四组已确认的 checkDead phase、type 35 公告和
type 604/605 瓢虫效果 intent 输入与结果；
结果通过，但不覆盖 packet 行为，也不证明 Version4 行为等价、真实网络发送、全部 189 成员接入、存档或完整迁移。

未来验证顺序：189 成员单写者审计；spawn/reset/reuse/parent/death/disconnect；
各类 damage 和 tracker；interaction Query/Command；
persistence/session/multi-world；Observation 差分、失败/重试/回滚。

在真实目标项目的全部必需行为测试通过且命中新 owner/新组合以前，迁移结论保持
migrationStatus: not-claimed；本设计仍是 proposed 边界。当前 10% 核心 smoke 只验证所列局部闭环，
完整 P12 验证仍为 not-run。

## 13. Death lifecycle guard evidence

Version4 `D:\TRbackup\Version4\Terraria\NPC.cs:64571-64578` 的 `checkDead` 入口只有在 NPC active、
当前 NPC 是 `realLife` health owner（`realLife < 0` 或 `realLife == whoAmI`）且 `life <= 0` 时才继续。
完整参考树 `D:\TRbackup\无任何删减通过编译\Terraria\NPC.cs:79209-79216` 的入口 guard 相同；后续
phase、spawn、网络和 death effects 仍须以当前 Version4 为基线，不能由参考树补齐。

只读 CPG Query API 对 `checkDead:void()` 返回 `complete`，符号 ID 为
`node-83386e93d361e4ad3d4d8d2525a1c7228229cb0978db62c74fc688413b93b808`。在 `NPC.cs`、`Main.cs` 和
`MessageBuffer.cs` 选定 shard 范围中找到 9 个静态调用点，均位于 `NPC.cs`，无 query gap；可调用事实返回
`partial`，有 `CalleeEffectsNotExpanded` gap（103 个 operation nodes）。数据库 manifest SHA-256 为
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`，所以这些结果
不能证明运行时调用闭包或被调用效果。

目标 `NpcDeathLifecycleSystem.Reconcile` 将已 `Despawned` 状态作为幂等终态；其他情况仅在 health dead
且 lifecycle active 时尝试提交 despawn。inactive `PendingDespawn` 不会被误判为已处理或终态。该 guard
只约束 terminal commit，不单独代表 `checkDead` 的替代实现。

随后增加的 phase 子集显式接收 NPC type、active/life-owner 条件、life、AI 和 center 输入；覆盖源码已确认的
四组 phase 条件：type 396/397 的 `ai[0] != -2` 与 type 400 spawn intent、type 398 的 `ai[0] != 2`、
type 517/422/507/493 的 `ai[2] != 1`，以及 type 548 的 `ai[1] != 1`。此外，type 35 且 `ai[3] == 1`
返回红色 Skeletron 公告 intent，type 604/605 返回包含 center 与 gold variant 的 `LadyBugKilled` intent。
GoodWorld type 13/36 分别返回固定坐标 spawn intent 与地表搜索参数 intent；type 13 缺少 `BottomY` 时
decision 标记 `RequiredInputMissing` 并压住 terminal transition。AI after-state、恢复生命与伤害策略标记、
replication sync request、spawn 和早期效果 intents 都作为 decision 返回；Lifecycle 不直接写 AI、health、
damage policy、world 或 player state。存在公告、瓢虫或 world-effect intent 时，结果设置
`TerminalCommitPending` 并保持 lifecycle active，调用方必须先交付前置效果，再调用
`CommitAfterPreTerminalEffects` 显式提交 terminal transition。该方法拒绝非-pending 结果，但不执行或确认
外部效果；消费协调当前尚未接入生产调用。
没有 spawn adapter，也没有接入 `NpcCombatSystem` 的生产调用方。combat 仅在收到显式
`NpcDeathPhaseContext` 时运行 type-aware lifecycle 并向调用方暴露 phase decision；未提供 context 的路径仍使用
通用 lifecycle。其余 `checkDead` 特例、phase effects、AI 单写者、分段清理及外部 effects 未关闭。

packet 23/28 API 仍只有函数声明及空 request/result 类型；协议解析、编码、授权、recipient 选择、发送和
authority apply 均未实现。

此 decision 子集不表示 `checkDead` 或死亡流程迁移完成。`LadyBugKilled` 的世界加成与玩家 luck 写入由外部 owner
消费；`StingerExplosion` 的 Version4 函数体为空，效果保持 unknown。GoodWorld type 13/36 目前只有 spawn
intent；type 36 随机 tile search、spawn、packet sync 和两类 intent 的 owner consumer 均未实现。其余 phase、
spawn source/runtime adapter、对应 owner 消费 decision 的协调和 Combat 生产入口仍未接通。

## 14. GoodWorld type 13/36 Intent Slice

Version4 `NPC.checkDead` 的 GoodWorld 分支在 `NPC.cs:64633-64660`：type 13 请求在 `(int)Center.X`、
`(int)(position.Y + height)` 生成 type `-12`；type 36 最多做 3 轮地表 spawn 搜索，每轮最多抽取 1000 个
候选，中心 tile 的 x/y 偏移均为 `[-50, 50]`，向上扫描至 `maxTilesY - 200`，找到地表后生成 type `32`。
新实体有效时请求 packet 23 同步。Query 只表达固定位置或搜索参数 intent，不执行随机、tile lookup、spawn 或同步。

目标 `NpcDeathPhaseInput` 显式包含 `IsGoodWorld` 和可空 `BottomY`。type 13 无 `BottomY` 时返回
`RequiredInputMissing` 并保留 lifecycle；有输入时返回固定坐标 spawn intent。type 36 返回 3 轮、半径 50、
每轮 1000 候选、底部边距 200 的搜索 intent。存在该 intent 时 lifecycle 保持 `TerminalCommitPending`。
输入、decision、缺输入时 lifecycle 保持 active、以及 intent pending 行为由 10% 核心 smoke 覆盖；实际
tile/random/spawn/sync consumer 和重试语义
仍为 `unknown`，因此这只是局部决策实现。

完整参考树 `D:\TRbackup\无任何删减通过编译\Terraria\NPC.cs:79272-79305` 在该分支外加
`Main.netMode != 1` gate；当前 Version4 对应分支没有此 gate。本实现按 Version4 权威源码，不导入参考树
差异。完整参考树只提供对照，不作为 Version4 行为证据。

packet 23/28 与 spawn-sync API 仅保留函数签名及空声明 DTO。`RequestReplicationSyncAfterSpawn` 是本地
intent 字段，不是 packet writer 或 transport 实现。协议解析/编码、授权、recipient 选择、发送、入站 apply
和 packet 23 reader 均未实现；后者继续为 `unknown`。

## 15. Combat phase health-owner commit slice

当调用方显式提供 `NpcDeathPhaseContext`，且非 terminal phase decision 设置
`RestoreLifeToMaximum` 时，`NpcCombatSystem` 在 combat commit 边界恢复 life-owner health；若本次命中的是
parent 的 segment，则随后把 owner 的当前/最大生命镜像回命中 segment。Combat result 的 `LifeAfter` 反映恢复后的
owner health。缺少 phase context 时仍走通用 lifecycle；Lifecycle/Query 不直接写 AI、damage policy 或外部同步状态。

10% 核心 verifier 覆盖 type 398 lethal hit、lethal DoT forced strike，以及 type 398 parent 被 segment 命中时的
owner 恢复和 segment 镜像。该组合尚无生产 `UpdateNPC` caller；AI、damage policy 与 replication sync intents 的
外部消费者仍未接通，因此这只是局部候选实现，不表示完整死亡阶段行为或 P12 迁移完成。packet 相关 API 仍为
函数声明与空 DTO，没有编解码、授权、recipient、transport 或 apply 逻辑。

## 16. Death phase decision authority commit slice

`NpcAuthoritySystem.CommitDeathPhaseDecision` 仅消费非 terminal、没有必需输入缺口且 `StateChanged=true` 的
decision。它把 `AiAfter` 的四个 AI slot 写入 `NpcBehaviorComponent`，只将 decision 明确要求为 true 的全伤害/hostile
策略置位到 `DamageAcceptancePolicyComponent`；false 表示该 phase 未写此策略，不清除其他 owner 已有的限制。它在要求同步时
标记本地 `NpcNetworkSyncIntentComponent`。该方法不发送 packet，
也不执行 spawn、公告或其它外部 intent。当前没有生产 `UpdateNPC` 或死亡协调器调用它；目前消费关系只由核心 verifier
直接覆盖，不能推断为生产接线。

`NpcCombatSystem` 的普通伤害入口可读 life-owner policy；`NpcDamageRequest` 显式携带 hostile/trap 分类。type 548
验证器输入模拟 hostile 与非-hostile 两种普通伤害，验证已提交的 hostile 策略只拒绝前者。生产伤害入口尚未提供来源
分类适配，`IsHostileDamage`/`IsTrapDamage` 的 caller 映射仍为 `unknown`。DoT 入口不读取该策略：Version4
`GetHurtByDebuff` 单独扣血并触发强制 Strike；tick 聚合的 `UpdateNPC_BuffApplyDOTs` 另有 `dontTakeDamage` gate，
二者不是同一策略入口，故本切片不将普通 Strike 的策略检查扩到 DoT。

只读 CPG Query API 对当前 Version4 索引得到：`dontTakeDamage` 在 `NPC.cs` shard 有 84 个 member uses，
`dontTakeDamageFromHostiles` 有 8 个，均为 `complete` 且无 gap；`checkDead`、`StrikeNPC`、
`GetHurtByOtherNPCs` 和 `UpdateNPC` 在所选 `NPC.cs`/`Main.cs`/`MessageBuffer.cs` shards 的 call-site 查询分别为
9、3、2、2 个，均为 `complete` 且无 query gap。`life`、`lifeMax` 和 `ai` 使用点查询达到 200 项预算并各带一个
gap，保持 `partial`。CPG manifest SHA-256 为
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`；实际策略语义仍以
Version4 `NPC.cs:64603-64627`、`:77663-77666`、`:78077-78099`、`:78546-78549` 源码复核为准。

10% 核心 smoke 覆盖 type 398 策略提交及随后普通命中拒绝、type 398 保留既有 hostile 限制、type 548 hostile 命中拒绝/
非-hostile 命中接受，以及 type 548 保留既有全伤害和 trap 限制的断言。
仅证明这些显式输入下的局部组合；不证明 caller 分类、生产 phase consumer、DoT 拦截策略、网络发送或完整 P12 行为。
当前状态仍为 `executionStatus: partial`、`designStatus: proposed`、`fullP12Verification: not-run`、
`migrationStatus: not-claimed`。packet 23/28 及 spawn-sync API 仍仅是函数声明/不透明 DTO，没有 packet 实现。

## 17. NPC DoT regeneration calculation slice

Version4 `NPC.UpdateNPC` 在 `NPC.cs:76738-76743` 的顺序为 flags reset、buff flags、SoulDrain、过期 buff 清理、
DoT、VFX。`UpdateNPC_BuffApplyDOTs` (`NPC.cs:77659-78074`) 先以 `dontTakeDamage` 提前返回，再将
`lifeRegenExpectedLossPerSecond` 作为局部值，按 debuff、projectile、Dryad Bane、SoulDrain 和特定 AI/world
条件顺序调整 regen 与预期损失。它将 `lifeRegen` 加入 `lifeRegenCount`，每跨过 120 返回一个回血点；负数阈值
则按 `120 * num` 或 120 循环请求 `GetHurtByDebuff`。`GetHurtByDebuff` 在 Version4 `NPC.cs:78077-78099`
单独扣血并触发 forced strike。

目标 `NpcStatusSystem.ApplyDamageOverTimePhase` 显式接收 status flags、life/count state、NPC type、`ai[1]`、
GoodWorld、LavaWet、realLife parent、infected seed、Dryad Bane progression、已解析的 town-NPC damage multiplier
和 projectile snapshot。需要 projectile 的状态必须提供调用方标记完整且恰为 1000 项的 snapshot；缺失、未标完整、
长度不足/超出或 legacy slot 无效时整阶段拒绝，不提交 status/count 部分状态。Town-NPC multiplier 缺失或无效时
Dryad Bane 分支同样拒绝。该倍率必须由外层按 Version4 `GetAttackDamage_ForTownNPC` 的 difficulty curve 先行解析；
这里不访问游戏全局状态。

返回结果包含最终 regen rate/count、回血点数、每次伤害值与事件数。`NpcStatusSystem` 写
`NpcLifeRegenerationStateComponent` 的计数/revision 和 `NpcStatusFlagsComponent.LifeRegenerationRate`，不直接修改
NPC health。`NpcCombatSystem.CommitLifeRegeneration` 按源顺序先提交当前命中 NPC 的回血，再逐次调用既有 DoT combat
commit；对 segment DoT，扣血目标为 `realLife` health owner，并在提交后镜像回 segment。该 API 不发送网络包。
生产 `UpdateNPC` 接线、DoT 拒绝后的计数恢复/重试契约仍为 `unknown`，因此不能视为已闭合消费。`MarkedByEelWhip`
对应 Version4 `ApplyEelWhipDoT` 空方法体，效果不从完整参考树导入，仍为 `unknown`。

两处特殊阈值遵循 Version4 的条件覆盖语义：Celled 在 `num < projectileCount * 20` 时将 num 覆盖为
`projectileCount * 10`；Dryad Bane 在 `num < townNpcDamage` 时将 num 覆盖为 `townNpcDamage / 3`。
这两个分支不等同于 `max(num, candidate)`。Dryad Bane 乘以 2 的 regen penalty 使用 64-bit 中间值，无法由目标
`int` 状态表示时拒绝且不提交。

只读 CPG Query API：`UpdateNPC_BuffApplyDOTs` 符号解析和选定 `NPC.cs`/`Main.cs` 静态调用点查询为 `complete`、
无 query gap；选定 `NPC.cs` shard 中调用点为 1 个。`GetHurtByDebuff` 静态调用点查询为 2 个，
`GetAttackDamage_ForTownNPC` 为 5 个，均仅代表所选 shard 范围。`UpdateNPC_BuffApplyDOTs` callable facts 为
`partial`，gap 是 `CalleeEffectsNotExpanded`；CPG manifest SHA-256 为
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`，故不是运行时闭包或
完整读写者证明。Version4 `NPC.cs` SHA-256 为
`29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`；完整参考树对应源 SHA-256 为
`ED8AA2302730A9E046EF310E543204FBA391BD35B1C9C0B213C8065DFBBE39F0`，只作对照。

DoT 核心 smoke 检查 poison 与 120 计数余数、跨阈值回血、缺失/短 projectile snapshot 原子拒绝、Celled/Dryad Bane
覆盖阈值和 Dryad Bane 算术溢出拒绝。其结果不证明 DoT 生产调用、GetHurtByDebuff 消费、完整状态 writer 闭包或行为等价。
packet 23/28、recipient 和 spawn-sync API 均保持函数声明与空 DTO；编解码、授权、recipient 选择、transport、入站 apply
和 sync consumer 没有实现。

## 18. DoT result consumer composition

Version4 `NPC.cs:78042-78072` 先更新 `lifeRegenCount` 并逐点处理回血，再逐事件请求 `GetHurtByDebuff`。
`GetHurtByDebuff` (`NPC.cs:78077-78099`) 记录当前 NPC 的 world damage credit、对 `realLife` root 扣血、在当前
NPC hitbox 发布 DoT combat text，并在致死时对 root 执行 forced strike 和 packet 28 intent。回血与 DoT damage
因此不是同一个 health target。

目标侧新增 `NpcHealthComponent.ApplyHealing` 和 `NpcCombatSystem.CommitLifeRegeneration`。组合 API 对命中的本地
health 应用回血，然后以 `DamagePerEvent` 为输入逐个调用 `ApplyDamageOverTime`，返回每个事件的 combat result；DoT
路径在 segment 命中时同步 root health 到 segment。此组合只处理已传入的计算结果与显式 owner/context，不接入
`UpdateNPC` phase scheduler，不调用 packet writer，也不自行建立重试队列。结果计算已先提交 regeneration count，
因此拒绝、异常、死亡阶段改变生命周期后的事件恢复语义仍为 `unknown`。

Dryad Bane `float` 与 `int.MaxValue` 比较改用 double promotion，避免 `int.MaxValue` 被转成 `float` 后舍入至
`2147483648`，使不可表示值绕过 overflow 拒绝。

本轮只进行了 CPG 查询和源码静态审阅；新增实现没有执行 build 或 test。当前消费接口只证明代码层的候选组合，
不证明生产 tick 接线、完整行为等价或 P12 迁移完成。packet API 保持声明/空 DTO。

## 19. Soul Drain player eligibility query

Version4 `NPC.UpdateNPC_SoulDrainDebuff` (`NPC.cs:77258-77285`) 在 `soulDrain` 为真时按 legacy player slot
0..254 顺序筛选 active、未死亡、与 NPC center 距离严格小于 1100、当前选中 item type 为 3006 且
`itemAnimation > 0` 的玩家。该 Version4 方法只请求 Dust 效果，不递增玩家的 Soul Drain 计数。

目标 `NpcSoulDrainEligibilityQuery` 将上述谓词变为只读计算：输入显式提供 NPC center 和 255 项有序玩家快照，返回
符合条件的 legacy slots；未激活时沿用旧方法早退并返回空列表。此 query 不采样随机数、不生成/消费 Dust，不写玩家或
NPC 权威状态。`NpcSoulDrainVisualEffectSystem` 已通过显式随机/Dust port 实现随机 gate、Dust 参数与字段设置；对应
Terraria port adapter 和生产 `UpdateNPC` 接线仍为 `unknown`/未实现。

完整参考树 `NPC.cs:92564-92594` 在本地玩家匹配时额外递增 `player.soulDrain`；这不是当前 Version4 行为，目标查询
不引入该差异。Version4 CPG 查询将 `UpdateNPC_SoulDrainDebuff` 符号和选定 `NPC.cs`/`Main.cs` 调用点解析为
`complete`，得到一个 `NPC.cs` 静态调用点且无 query gap；调用点在 `NPC.UpdateNPC`。CPG manifest SHA-256 为
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`，因此不证明运行时
调度闭包或源码快照绑定。受影响 NPC 项目已编译通过；本轮未运行测试，P12 状态仍为 proposed/partial，
`verificationStatus` 仍为 `not-run`。

## 20. Clothier Skeletron follow-up intent

Version4 `NPC.checkDead` 在 `NPC.cs:64700-64707` 对夜间死亡的 type 54 NPC 先检查是否存在 type 35；若没有，
按 player slot `0..254` 查找首个 `active && !dead && killClothier` 玩家，并调用 `SpawnSkeletron(slot)` 后停止扫描。
`SpawnSkeletron` 在 `NPC.cs:66738-66780` 会再次检查活动 Skeletron、寻找 Clothier/Old Man 位置、生成 type 35，
设置同步状态并广播公告，故本拆分只产生 player slot intent，不在死亡 query 内操作 NPC/world/network owner。

`NpcDeathClothierSkeletronContext` 显式接收昼夜、活动 Skeletron 标记和有序的 255 槽玩家快照。
只有夜间且无活动 type 35 时才要求完整快照；缺失或槽数不符时 decision 标记
`ClothierSkeletronInputMissing` / `RequiredInputMissing` 并抑制 terminal commit。有效快照按槽位升序匹配首个
合格玩家，返回 `NpcDeathSkeletronSpawnIntent`，使 `NpcDeathLifecycleSystem` 将该死亡留在
`TerminalCommitPending`，待外部效果交付后再显式提交。没有合格玩家或白天/已有 type 35 时不生成该 intent。

只读 CPG Query API 的 `SpawnSkeletron` symbol 和选定 `Terraria/NPC.cs` call-site 查询均为 `complete`、无 query gap，
共 2 个静态调用点；callable facts 为 `partial`，包含 `CalleeEffectsNotExpanded` gap。数据库 manifest SHA-256 为
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`；静态事实不闭合 helper
副作用或生产调用链。Version4 `NPC.cs:64700-64707` 没有 client-mode gate；完整参考树在对应分支额外包含
`Main.netMode != 1`，该差异未引入目标代码。

该切片没有生产 `UpdateNPC` / `checkDead` caller，也没有 `SpawnSkeletron` adapter 或 intent consumer；
helper 的生成、NPC state、announcement 与网络 effects 的失败/重试语义保持 `unknown`。它仅补充局部
death-phase decision，不代表完整 checkDead 行为或迁移等价。

## 21. GoodWorld death world-effect consumer

Version4 `NPC.checkDead` 对 GoodWorld type 13/36 的生成使用
`GetSpawnSourceForNPCFromNPCAI()`，该方法返回 `EntitySource_Parent(this)`。因此 death decision 必须把
来源 NPC 的 `NpcInstanceId` 带入 intent；身份缺失时 phase 返回 `RequiredInputMissing`，不得让 spawn adapter
从隐式全局状态猜测 parent。type 396/397 的 type 400 spawn intent 同样携带来源 NPC instance ID。

目标新增 `NpcDeathWorldEffectSystem`、`INpcDeathWorldEffectPort` 与结果类型。固定位置 intent 按其像素坐标
调用 NPC-AI spawn port；surface-search intent 按 intent 中的次数、tile 半径、底部边距执行随机 x/y 抽取、向下
扫描至固体 tile 上方并尝试 spawn。两个分支仅在 legacy 返回 slot 小于最大 NPC 槽数时，通过
`INpcSpawnSyncPacketPort` 请求 packet 23 同步。随机、tile 读取、spawn 与同步请求均由显式端口执行；本层不实现
packet 编解码、授权、recipient、transport 或 inbound apply。

Version4 `NPC.cs:64633-64660` 提供坐标、搜索循环和同步条件；`NPC.cs:77061-77065` 确认 spawn source 为
parent entity。只读 CPG 的 `GetSpawnSourceForNPCFromNPCAI` 符号和选定 NPC shard 调用点查询为 `complete`，无
query gap；全 shard 有 58 个调用点，在 `checkDead` 的源码 span 内有 3 个。`checkDead` callable facts 仍为
`partial`，有 `CalleeEffectsNotExpanded` gap。CPG manifest SHA-256 为
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`。

该 consumer 尚无生产调用方或 Terraria adapter；death decision 的前置效果排序、NPC spawn 创建与 packet 发送
失败后的恢复仍为 `unknown`。它补足局部 intent consumer，不表示死亡流程、生产接线或 P12 迁移完成。

## 22. Status tick phase composition

Version4 `NPC.UpdateNPC` 先调用 `UpdateNPC_BuffFlagsReset`、`UpdateNPC_BuffSetFlags`、
`UpdateNPC_SoulDrainDebuff`、`UpdateNPC_BuffClearExpiredBuffs`、`UpdateNPC_BuffApplyDOTs`，再进入
`UpdateNPC_BuffApplyVFX`。目标新增 `NpcStatusTickSystem`，按相同前五个 phase 顺序组合现有
`NpcStatusSystem`、`NpcSoulDrainVisualEffectSystem` 与 `NpcCombatSystem`：应用当前 buff flags 与 duration、执行
Soul Drain Dust intent、清理过期槽、用当前 NPC health 与 AI snapshot 计算 regeneration/DoT，再将计算结果提交给
Combat。buff sync 和 water-perishable cleanup 仍作为 phase result intents 返回，不在该 coordinator 中发送网络或
执行未建模的组件删除。

tick input 显式携带 AI、immunity、玩家快照、当前 NPC 位置/速度、progression/projectile 等 regeneration facts、DoT
text identity/bounds 和 tick。协调器将 `NpcAiStateComponent.State1` 与传入的当前 health/max health 覆盖到 regen input，
避免在该组合内部继续使用旧 AI 或 health 值。不可缺少的 identity/immunity/health/lifecycle 在任何 phase 写入前检查。

此类型只表达可调用的局部 phase composition，CPG 与 Version4 源码没有证明目标侧 Main/session scheduler 或 active NPC
snapshot owner，因此目前没有生产 caller。`UpdateNPC_BuffApplyVFX`、Blood Moon transformation、gravity 及 life<=0
后的 network/dirty-state 清理不属于本 System；Soul Drain Dust port 和 buff sync / water cleanup effects 也没有
Terraria adapter。若 phase input 缺失、regeneration 拒绝或 Combat effect 失败，已发生状态写入/随机效果的补偿、重试和
跨阶段原子性仍为 `unknown`。它不构成完整 NPC tick 或 P12 行为闭合。

只读 CPG Query API 重新查询 Version4 的 `UpdateNPC`、buff reset/set、Soul Drain、过期清理、DoT 与
`GetHurtByDebuff`。选定 `NPC.cs`、`Main.cs`、`MessageBuffer.cs` scope 的 symbol/call-site 查询均为 `complete`、无
query gap：`UpdateNPC` 有 2 个 Main call sites；五个 phase helper 各有 1 个 NPC call site；
`GetHurtByDebuff` 有 3 个静态 call sites。manifest SHA-256 为
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`；静态结果不证明目标
调用闭包、运行时调度或 callee effect closure。完整参考树 `NPC.cs:91990-91995` 保持相同 phase 顺序，但 Soul Drain
方法体有额外的本地玩家计数写入；Version4 当前源码没有该写入，目标不导入 reference-only 行为。

完整参考树的 `UpdateNPC_BuffClearExpiredBuffs` 还在 multiplayer-client 模式提前返回，Version4 对应方法没有该
gate；此差异保持 reference-only，目标按 Version4 清理过期 slots。

本节记录的 `NpcStatusTickSystem` 是后续新增的局部 composition，更新并细化本设计前文的 NPC.Tick 候选组合状态；
旧段落中“未实现随机/Dust”仅记录当时切片状态，不覆盖该 effect system 后续落地事实。生产 scheduler、VFX 和外部
effect adapters 仍未接通。

`NpcStatusTickSystem.Advance` 先检查 `NpcLifecycleComponent.IsActive`；inactive 时返回 `SkippedInactive`，不校验后续
phase identity、不改 buff/status/regen state，也不调用 Soul Drain effect port。该 guard 对应 Version4
`NPC.UpdateNPC` 在任何 tick phase 前的 `if (!active) return`。

### 22.1 Status tick 的后续 phase

目标新增 `NpcBloodMoonTransformationSystem`，依 Version4 `NPC.cs:78120-78160` 对三组 NPC type
计算 Crimson/Corruption 目标，返回 `NpcBloodMoonTransformationIntent`，并保留旧 `value == 0` 时于转换后
恢复为零的要求。这是决策结果，尚未调用 `Transform`；转换失败、type/defaults 写入及其副作用仍为 `unknown`。

目标新增 `NpcGravityEnvironmentSnapshot`、`NpcGravitySystem` 与 `NpcGravityResult`，依
Version4 `NPC.cs:77176-77280` 对 NPC type/AI、Y 位置、world surface/宽度与介质输入计算 gravity、maximum
fall speed 及 NPC Y velocity 上限。它只计算结果，不提交 `MovementStateComponent`，也不包含后续 tile-null
归零、`noGravity` 应用 gravity、fall-speed clamp 与速度量化；这些步骤位于 `UpdateNPC` 调用端，不能当作已迁移。
Version4 `NPC.Transform` 会重设 defaults/AI 参数并按新旧高度调整位置。当 Blood Moon transform intent 存在时，
`NpcStatusTickSystem` 将 gravity 标为 deferred，不用转换前快照产出貌似有效的结果。Transform consumer 仍须提交变化、
取得 post-transform type/AI/位置快照，再单独调用 gravity System；该调用链当前未接通。

Status tick 現在在 DoT/regen 計算拒絕時保留拒絕結果並略過 Combat commit，但仍繼續到 Blood Moon 與可用的 gravity
计算阶段；DoT/regen 之后新增的 shimmer transparency phase 也会按可用输入处理。Buff flags phase 本身缺少必需输入时
仍会按其拒绝结果提前退出。已发生的 phase 写入与 Soul Drain 随机效果在后续失败后的补偿/重试、血月 transform
生命周期以及 gravity 提交顺序仍为 `unknown`。

只讀 CPG Query API 對 Version4 `UpdateNPC_BuffApplyVFX`、`UpdateNPC_BloodMoonTransformations` 與
`UpdateNPC_UpdateGravity` 的精確 method symbol 查詢均為 `complete`，每個找到一個 `Terraria/NPC.cs` symbol；
manifest SHA-256 為 `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，
`SourceSnapshotId=null`。對所選 `NPC.cs`/`Main.cs` scope 的 `UpdateNPC_BuffApplyVFX` call-site 查詢為
`complete`、1 个 NPC.cs 调用点；其 callable facts 为 `partial`、`CalleeEffectsNotExpanded`。
`UpdateNPC_BloodMoonTransformations` 与 `UpdateNPC_UpdateGravity` 的选定范围 call-site 查询仍为
`partial`、`NoMatchingFactInScannedScope`，callable facts 为 `partial`、`CallableSymbolNotPresentInShard`。
这些索引缺口不得解读为无 caller 或无副作用。回读 Version4 `NPC.cs:76738-76745` 确认 `UpdateNPC` 直接调用 expired-buff、
DoT、buff VFX、Blood Moon、gravity phase；读取 helper bodies 确认转换在 Blood Moon 内调用 `Transform`，gravity
规则在 `UpdateNPC_UpdateGravity` 内。完整参考树对应入口在 `NPC.cs:91990-91997` 并用于交叉核对，不覆盖
Version4 authority。

补充：当同一 tick 产生 Blood Moon transformation intent 时，gravity 阶段不是基于旧 snapshot 计算；
`GravityDeferredUntilPostTransformationSnapshot` 显式要求 Transform consumer 提交 type/defaults 后，取得新的
type/AI/位置输入，再调用 gravity System。该 consumer 和后续状态提交尚未实现。

Version4 `NPC.cs:77626-77653` 的透明度状态转换已独立为目标 `NpcShimmerStateComponent` 与
`NpcShimmerTransparencySystem`，由 `NpcStatusTickSystem.Advance` 在 DoT/regen phase 后调用。System 只依据显式
`CanDisplayBuffs`、`Shimmering`、`JustHit` 和 buff 353 immunity 输入推进/衰减透明度；immunity 快照缺少 buff 353
时保留原值并返回 `RequiredInputMissing`。Version4 `GetShimmered()` 在 `NPC.cs:77658` 是空方法，故不从完整参考树
导入其转换/奖励逻辑。该 state owner 尚未接入 packet 23 hydrate，也未完整接入读取该字段的 AI、NPC 视觉快照或
生命周期组装；Version4 `UpdateNPC_BuffApplyVFX` 的 Dust/Gore/particle/light、`netOffset` 和 effect phase 仍未迁移。

`UpdateNPC_BuffApplyVFX` 不是纯视觉投影：Version4 `NPC.cs:77287-77657` 同时发出 Dust/Gore/particle/light
效果、调整 `shimmerTransparency`、在首尾应用/撤销 `netOffset`，并在 shimmer 条件下调用 `GetShimmered`。
Version4 `NPC.cs:77658` 的 `GetShimmered()` 是空方法；完整参考树同名 helper 有实际逻辑只能作为差异线索，不能
推断或覆盖 Version4 行为。当前已实现 shimmer 数值状态的局部 owner/转移 System，但没有完整 VFX effect consumer、
`netOffset` adapter，也没有生产 `UpdateNPC_BuffApplyVFX` phase caller；AI 与 network hydrate 的状态接线仍为 partial/unknown。

現有 gap：沒有 `UpdateNPC_BuffApplyVFX` System 或 effect port，沒有 Transform consumer、Movement gravity commit、
生產 `UpdateNPC` scheduler/caller、`life <= 0` 的 network/dirty-state cleanup，也沒有完整 tick behavior comparison。
目標 NPC 專案 compile 成功只證明可編譯；本輪沒有跑測試或 verifier。故 `executionStatus: partial`、
`designStatus: proposed`、`verificationStatus: not-run`、`migrationStatus: not-claimed` 保持不變。

# P12/C17 NPC Damage Tracking System 实验优化报告

## 结论摘要

本实验选择 P12（189 项）中的 `NpcDamageRuntimeTracking`，针对 Version4
`NPCDamageTracker` 建立了一个可独立审查的 ECS system 边界。当前实现已由
受影响的 NPC 项目 serial build 检查，但没有新增测试代码，也没有运行测试或
focused verifier；状态应记为 `implemented/build-checked`，不能记为
`focused-verified`。

这不是 Version4 或 Dome 的完整迁移。实验没有接管 Dome 主循环，没有删除
旧 tracker，没有接入持久化/网络/loot/UI，也没有声称行为等价或达到旧实现
删除门禁。

## Scope / Evidence

### 权威范围

权威 P12 成员报告为：

`docs/组件文档/迁移参考表/Version4权威模拟系统字段属性逐成员源码声明-20分区/P12-NPC-Combat-Network-Damage.md`

本次范围锁定为其中 C17 `NpcDamageRuntimeTracking`。执行前 ledger 状态为：

| 项目 | 值 |
| --- | --- |
| P12 状态 | `completed` |
| 期望成员数 | `189` |
| 观察成员数 | `189` |
| session | `dec7d02830c14d7bbba328dc0317cef3` |
| C17 成员数 | 12（9 字段 + 3 属性） |

本次没有重新 Claim/Cleanup P12，也没有修改 P12 报告、ledger、lock 或
`.agent-workplace/state`。

### Version4 来源

只读来源：

`D:\TRbackup\Version4\Terraria.GameContent\NPCDamageTracker.cs`

SHA-256：

`1B6812046FE624470BF51B48EE542EB8836D270CA60EC77BCCAB5A9F9E3DD8B0`

源码确认的关键事实：

- `_activeTrackers` 和 `_recentFinishedTrackers` 是进程静态 registry；
- `MAX_RECENT_TRACKERS` 为 `3`；
- `EXTRA_RECENT_TRACKER_EXPIRY_TIME` 为 `54000`；
- `_list` 按 contributor 首次出现顺序保存条目；
- owner 小于 `0` 或大于等于 `255` 时进入 world credit；
- Version4 在调用方将伤害限制为 `Math.Min(damage, npc.life)` 后记录该值；
- active tracker 每次 `Update` 推进 `_ticks`，失活后移动到 recent；
- recent 过期条件为 `count > 1 && TimeSinceLastHit > 54000`，因此至少保留
  一个 recent tracker；
- Boss composite tracker 在任一成员仍活跃时保持 active，`OnBossKilled`
  记录 killed 状态。

`InvasionDamageTracker.IncludeDamageFor` 在当前 Version4 快照中仍是
`return new bool();`。本实验将其列为 `unknown/deferred`，没有猜测入侵伤害
归属。

### C17 成员映射

| Version4 成员 | 实验归属 | 实验状态 |
| --- | --- | --- |
| `_activeTrackers` | `NpcDamageTrackingSystem._activeTrackers` | 已实现、build-checked |
| `_recentFinishedTrackers` | `NpcDamageTrackingSystem._recentFinishedTrackers` | 已实现、build-checked |
| `MAX_RECENT_TRACKERS` | `NpcDamageTrackingSystem` 私有常量 `MaxRecentTrackers` | 已实现、build-checked |
| `EXTRA_RECENT_TRACKER_EXPIRY_TIME` | `NpcDamageTrackingSystem` 私有常量 `ExtraRecentTrackerExpiryTime` | 已实现、build-checked |
| `_list` | `EncounterDamageCreditComponent` 的 ordered credit array | 已实现、build-checked |
| `_worldCredit` | `World` contributor entry + `WorldDamage` | 已实现、build-checked |
| `_lastAttacker` | `LastContributor` | 已实现、build-checked |
| `_ticks` | `NpcDamageTrackingSystem` 的显式 current tick | 已实现、build-checked |
| `_lastHitTime` | `EncounterDamageCreditComponent.LastHitAtTick` | 已实现、build-checked |
| `IsEmpty` | encounter component/snapshot projection | 已实现、build-checked |
| `Duration` | snapshot `Duration` | 已实现、build-checked |
| `TimeSinceLastHit` | snapshot `TimeSinceLastHit` | 已实现、build-checked |

映射是语义边界映射，不是声明级一对一复制。Version4 的静态全局 registry
被替换成每个 system/world 实例的 registry；这属于本实验的明确隔离设计，
不是已证明的完整旧运行时等价。

## Ownership And Boundary

### Owner

`NpcDamageTrackingSystem` 是唯一的 registry 与生命周期 owner，负责：

- 为一个 world/session 实例维护 active/recent tracker 列表；
- 分配本实例内的 encounter ID；
- 接收已经由上游结算的正 `appliedAmount`；
- 创建、停止、推进、转移、限长、过期和 reset tracker；
- 调用策略的 activity/killed seam；
- 生成只读 snapshot。

`NpcDamageEncounterTracker` 是单 encounter owner，私有持有现有
`EncounterDamageCreditComponent`，负责 contributor 顺序、累计、world
damage、last contributor、start/last-hit tick、lifecycle 和 revision。

`INpcDamageTrackingStrategy` 只表达动态目标集合：

- `Includes(NpcTypeId)`；
- `IsStillActive(IReadOnlySet<NpcTypeId>)`；
- `OnNpcKilled(NpcTypeId)`。

`NpcDamageSingleTypeStrategy` 和 `NpcDamageCompositeStrategy` 是两个实际
实现，因此接口不是单实现的未来抽象。策略不读取 `Main`、静态 NPC 数组、
系统时钟、随机源或外部 I/O。

`EncounterDamageCreditComponent` 没有被复制成第二套 NPC 专属组件。新增的
`TryAddCredit`、`Close`、`Expire` 是受控 mutation；tracker 的写方法和
strategy 引用已收窄为 assembly-internal，外部调用者不能绕过 system 直接
写 encounter。

### Snapshot

`NpcDamageTrackerSnapshot` 复制 credit array 后才对外暴露
`ReadOnlyMemory<DamageCreditEntry>`，snapshot 本身还复制 registry 返回的
数组。snapshot 包含 encounter ID、初始 NPC type、credits、world damage、
last contributor、start/last-hit tick、duration、idle time、empty/lifecycle、
active/recent、killed 和 revision。

## Read / Write / Emit Set

| Boundary | Read set | Write set | Emit set |
| --- | --- | --- | --- |
| `NpcDamageTrackingSystem` | 显式 tick、active NPC type set、NPC type、contributor、已接受伤害、strategy factory | active/recent registry、encounter ID、tracker lifecycle | snapshot array；无网络/文件副作用 |
| `NpcDamageEncounterTracker` | contributor、positive applied amount、hit tick、strategy | 一个 `EncounterDamageCreditComponent` | 供 system 投影的 encounter 状态 |
| credit component | contributor、amount、hit tick、现有 lifecycle | ordered credit、world damage、last contributor、last-hit tick、revision/lifecycle | 防御性 credit memory |
| strategy | NPC type、显式 active type set | killed flag | inclusion/activity/killed 结果 |

上游 damage resolver 仍然拥有生命值和伤害结算。system 不读取 health、不重算
defense、不修改 NPC lifecycle、loot 或 death effect；调用者必须传入已经接受
且已裁剪的 `appliedAmount`。

## Schedule / Data Flow

实验契约是：

```text
NPC identity/health + damage resolution
  -> accepted appliedAmount
  -> NpcDamageTrackingSystem.TryRecordAppliedDamage
  -> lifecycle/death signal
  -> MarkKilled / AdvanceTo
  -> snapshot projection
```

具体约束：

- `AdvanceTo(tick, activeNpcTypes)` 必须先建立或推进当前 tick；
- `TryRecordAppliedDamage`、`MarkKilled`、`StopTracking` 必须使用当前 tick；
- health/damage resolution must-before credit commit；
- activity/lifecycle 集合 must-before 下一次 `AdvanceTo`；
- snapshot consumer must-after system commit；
- 当前实验没有新增 scheduler phase、engine command buffer 或 barrier；同一
  tick 内以调用方显式顺序作为 commit order；
- 真实 Dome 集成时，`DamageResolution` 必须先捕获实际 `appliedAmount`，再由
  单一 owner 提交 tracker；否则不能声称 credit 与生命结算一致。

Dome 工作区证据显示 `dome/dome1` 的 `NpcSystemPipeline` 已有
`ContactEffect -> DamageResolution -> Lifecycle -> Death -> Loot -> Replication`
阶段；但当前 `CommitNpcDamageCommands` 还没有正式接入本实验 system。本报告
因此只把它作为调用闭包证据，不把工作区副本改动计入本提交。

## Clock / Random / I/O

| 依赖 | 实验处理 | 结论 |
| --- | --- | --- |
| Clock | caller 传入 `long tick`；system 拒绝倒退和 future commit | 已通过编译；运行时契约未执行 |
| Random | 不使用 | 无随机等价结论 |
| Health | 不读取；只接受已结算 `appliedAmount` | 不改变 health owner |
| File / persistence | 不读写 | `not applicable` to prototype；生产恢复未闭合 |
| Network | 不发包、不读协议 | `not applicable` to prototype；网络 projection 未闭合 |
| UI/localization | 不产生显示文本 | 不复制 Version4 `NetworkText`/`LocalizedText` |
| Background/concurrency | 无后台任务、锁或共享 static registry | 仅源码确认；world isolation 与跨线程语义未执行 |

`Reset` 只清理 system 的内存 registry 和本地 encounter ID/cursor；调用者需要
再次用 `AdvanceTo` 建立当前 tick。它不修改外部 world clock，也不回滚已发生的
health、网络、存档或 loot 效果。

## Keep / Partial / Separate

| 方案 | 优点 | 风险/不足 | 决策 |
| --- | --- | --- | --- |
| keep | 旧 registry 与行为集中，迁移面小 | 静态状态跨 world 污染；动态策略和 snapshot 不易独立验证 | 拒绝 |
| partial | 改善文件导航，不新增运行时 owner | 不降低 ownership、生命周期或验证耦合 | 拒绝 |
| separate | registry、encounter state、strategy、snapshot 可分别审查；world instance 隔离；single writer 清晰 | 真实接入仍需 lifecycle、Dome caller、持久化和网络闭合 | 采用 |

Separate 不是“每个方法一个 system”。只有一个
`NpcDamageTrackingSystem` 负责跨 encounter registry 与生命周期；per-encounter
tracker 是其拥有的数据/行为对象，不是第二个调度节点。

## Failure / Rollback

已处理的失败边界：

- 非法 NPC type、空 strategy、strategy 不包含 initial type：拒绝；
- player contributor 缺少 account provenance：拒绝；
- world contributor 携带 player provenance：拒绝；
- `appliedAmount <= 0`：不创建、不更新 tracker；
- current tick 未建立、future commit、backwards tick：拒绝；
- closed/expired encounter 再写入：拒绝；
- credit、world damage、revision、encounter ID 溢出：在提交前由 checked
  运算抛出，内部状态不替换；
- 重复 Stop：返回 `false`，不会重复进入 recent；
- 空 encounter close：关闭但不进入 recent；
- recent cap：移除最旧项并标记 expired；expiry：在至少一个 recent 保留的
  条件下按严格 `>` 移除旧项。

本实验无外部不可逆副作用，回滚是删除/反转实验 commit，并让原 writer 继续
工作。真实迁移仍需说明 shadow state 版本、队列/订阅恢复、已发网络消息和
已生成 loot 的补偿边界；仅切换 feature flag 不足以证明可回滚。

## Evidence Gaps And Optimization Findings

### Contributor provenance

Version4 用 player index 再从 `Main.player[owner].name` 取得名称；当前 Dome
的 `SourceIdentity` 不是可靠账户 provenance。实验因此要求
`CombatContributorId(Player, accountUuid)`，并把缺少 UUID 判为错误，而没有
把外部伤害擅自归入玩家。正式接入前需要一个已确认的账户/实体身份 adapter，
并验证重连、改名、跨世界和回放语义。

### Dynamic strategy

Boss/composite 行为已经由显式 active type set 验证；但 Version4 的默认 boss
registry、realLife alias、完整 custom definition 注册和 Invasion tracker
仍未移入。特别是 invasion `IncludeDamageFor` 的源码是未闭合生成体，不能由
本实验补全。最小补证据是恢复一份可执行的完整源实现或取得明确的运行 trace，
然后为每个策略组建立 golden fixture。

### Dome caller closure

已定位的 Dome 调用闭包包括：

- `DomeSimulation.QueueNpcDamage`：伤害命令入口；
- `DomeSimulation.CommitNpcDamageCommands`：NPC damage commit；
- `DomeSimulation.NpcPipeline`：`DamageResolution` 阶段；
- `DomeSimulation.Tick`：tick owner。

当前正式路径仍丢弃或未统一消费 tracker 所需的 `appliedAmount`，且 NPC death、
lifecycle、persistence、network snapshot 尚未包含 C17 状态。因此建议的下一步
是先做 isolated shadow comparison，只允许一个 credit commit owner，再决定是否
接入，不应直接把本实验视为已迁移。

## Verification Record

本次按用户要求不新增测试代码、不运行测试或 focused verifier。所有实际
编译命令均通过 `Build/Tools/Invoke-SerialDotnet.ps1`，并在执行前检查了
活动 `dotnet.exe`/`csc.exe`；没有并行构建。

### Affected Project Build

```powershell
$npcBuildArgs = @(
  'build', '.\src\Npc\Terraria.Npc.csproj',
  '-m:1', '-nr:false', '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
& .\Build\Tools\Invoke-SerialDotnet.ps1 @npcBuildArgs
```

结果：exit code `0`；warning `0`；error `0`；

`Build/bin/Terraria.Npc/Debug/net10.0/Terraria.Npc.dll`

该结果只证明项目引用、类型和实现可以在当前工作区配置下编译。状态转换、
credit 顺序、recent expiry、strategy 动态分派、world/session 隔离和 snapshot
防御性复制均未通过运行时 verifier 执行；Version4 所有 dynamic subclass、
入口/反射/mod hook、Dome 主循环、网络协议、存档恢复、线程调度和旧/新
golden trace 的行为等价也未证明。

## Branch / Commit / Deletion Gate

实验分支为：

`codex/npc-damage-tracking-system-experiment-20260917`

它从当时的 `main` 提交创建。实验提交只应包含本报告、设计/计划、C17
system/tracker/strategy/snapshot、现有 credit component 的受控 mutation，以及 NPC 项目的单向 Combat
引用；不得包含用户已有 dirty changes、Build 输出、Dome 整体副本或 P12
ledger。最终 `main` reachability、实验分支删除和 dirty 状态保留以提交后
`git status`/`git branch`/`git merge-base` 输出为准。

Version4 deletion gate：`not applicable`。原因是旧实现未删除、Dome 未切换
唯一 writer、持久化/网络未验证、动态策略仍有 gap，且本实验只提供源码审查
与 build evidence。

## `ecs-system-domain-splitting` 使用性评估

### 有效点

- 强制先锁定 authority/source hash/member scope，避免从“NPC damage”泛化到
  P12 全部 189 项；
- 用 keep/partial/separate 对比阻止把 partial 文件拆分误当成运行时 system；
- Read/Write/Emit set 迫使 registry、encounter component、strategy 和
  projection 的 owner 可见；
- 生命周期、调用闭包、phase/barrier、clock/I/O 和 rollback 清单暴露了真实
  Dome 接入缺口；
- 删除门禁明确阻止“编译通过 = 可删除旧实现”；
- `source-inventory-confirmed`、`partial`、`unknown`、`proposed` 与
  `build-checked` 分开记录，降低过度宣称风险。

### 优化建议

1. 在 skill 的最小交付模板中增加“执行命令/exit code/warning/error/artifact”
   固定字段，避免报告最后才补验证细节。
2. 把“source/build evidence”和“runtime-integrated differential evidence”
   定义成两个明确等级，并要求报告同时填写 integration status。
3. 对 `static registry -> world/session instance` 这类所有权变化增加一个
   必测 multi-world isolation 验收项；当前 skill 的一般性生命周期清单容易让
   它被遗漏。
4. 对 strategy seam 增加“factory freshness、overlapping strategy、dynamic
   dispatch unresolved”检查；接口存在本身不能证明动态分派闭合。
5. 对已接受伤害类系统增加“upstream applied amount provenance”字段，明确禁止
   system 重新计算或使用请求金额替代裁剪后的结算金额。
6. 对 `Reset` 增加“是否清理本地 cursor、是否保留外部 clock、是否需要重新
   establish tick”的明确三项契约，减少文档和实现语义漂移。

本次评估结论：该 skill 对复杂 system 的边界分析和证据分级有效，但应把
source/build 与 runtime-integrated 的等级、world isolation、applied amount
来源和 reset clock contract 进一步结构化，减少依赖人工报告解释的部分。

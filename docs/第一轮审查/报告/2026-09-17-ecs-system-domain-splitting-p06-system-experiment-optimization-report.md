# P06 System 拆分实验与 ecs-system-domain-splitting 优化报告

date: 2026-09-17
experimentStatus: focused-verified
partitionStatus: P06 running; not settled by this experiment
branch: codex/ecs-system-split-p06-experiment-20260917
baseCommit: 3a90e999d4da756ae9e45a864a272df748ae47cd
designCommit: 80efab1a811468b63c3c9d16aa74225cd34a1372
partitionSession: ab9f33ba8db0405d97312bd5b73fd336

## 1. Scope And Evidence

本实验以最新现场权威清单
`docs/组件文档/迁移参考表/Version4权威模拟系统字段属性逐成员源码声明-20分区/P06-Player-Combat-Status.md`
为输入。runner `List` 在本轮返回 P06 `expectedMemberCount=254`、
`observedMemberCount=254`，当前手工会话仍为 `running`；本实验没有重复
claim、Cleanup 或手改 ledger。

P06 包含 22 个叶子子系统和 254 个字段。本次只实现其中
`PlayerCombatDamageProcState`（C02）的 15 个字段：
`lifeSteal`、`ghostDmg`、`eocDash`、`eocHit`、`infernoCounter`、
`starCloakCooldown`、`onHitDodge`、`onHitRegen`、`onHitPetal`、
`onHitTitaniumStorm`、`titaniumStormCooldown`、`hasTitaniumStormBuff`、
`petalTimer`、`boneGloveTimer`、`phantomPhoneixCounter`。

Version4 来源快照为 `D:\TRbackup\Version4\Terraria\Player.cs`，
SHA-256 为
`E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86`。
C02 声明证据位于 `Player.cs:642-644,688-690,736-739,798-816`；
tick、恢复、计数器和冷却路径位于 `Player.cs:14876-15029,15173-15183`；
EOC dash/target sentinel 路径位于 `Player.cs:12575-12646`。

这些 Version4 事实确认了源字段和原始语义范围，但不证明本实验已经
完成 Version4 行为等价。原始投射物、副作用、随机、网络、存档和真实
游戏调度都明确排除。

## 2. Baseline And Boundary Decision

实验前的当前 NLTX C02 owner 同时处理命中提交、tick 算术、EOC 命令、
生命周期重置和快照读取。边界选择如下：

| 选项 | 结论 | 理由 |
|---|---|---|
| 保持单文件、单 System | 拒绝作为实验交付 | 行为安全，但不能验证新的纯规则边界。 |
| 仅使用 `partial` 拆源文件 | 作为组织层保留 | 可以按命中、tick、生命周期审查，但不会增加调度节点或纯规则 seam。 |
| 新建第二个 tick System | 拒绝 | 会为同一 Component 引入第二个写者；没有证据证明存在独立 phase、生命周期或事务。 |
| `TickInput -> TickQuery -> TickResult`，由原 System 提交 | 采用 | 把确定性计算隔离为可直接验证的 Query，同时保留唯一写者和原调用面。 |

最终结构：

```text
committed hit / lifecycle command -> PlayerCombatProcSystem -> Component

Component values + ExpertMode -> PlayerCombatProcTickQuery -> TickResult
                                                        |
                                                        +-> PlayerCombatProcSystem commits

Component -> PlayerCombatProcQuery -> immutable Snapshot
```

`PlayerCombatProcStateComponent` 仍是 15 个 C02 字段的权威状态容器。
`PlayerCombatProcSystem` 仍是唯一提交者；partial 文件是同一个运行时类型，
不是四个 System。`PlayerCombatProcTickQuery` 不保存 Component、时钟、
随机源、事件订阅、可变集合或 I/O。

## 3. Ownership And Read/Write Contract

| 模块 | Read set | Write set | Emit/effect | Lifecycle/order |
|---|---|---|---|---|
| `PlayerCombatProcTickQuery` | 9 个 tick 字段值、`ExpertMode` | 无外部状态；返回值对象 | none | 无注册、无 Update；调用方决定时机。 |
| `PlayerCombatProcSystem.AdvanceTick` | Component 的 9 个 tick 字段 | 同一 Component 的 9 个 tick 字段 | none | 保持原 tick 调用点；纯计算结束后逐字段提交。 |
| `PlayerCombatProcSystem.AcceptCommittedHit` | hit event、replay set、Ghost/Life/flags | Ghost、Life、4 个 on-hit flags、replay set | none | committed event 的 at-most-once gate；不由 Tick Query 处理。 |
| `PlayerCombatProcSystem.ResetEffects` | 无外部读 | 4 个 on-hit flags、Titanium buff flag | none | 效果重建边界；不清 replay set。 |
| `PlayerCombatProcSystem.Reset` | replay set | 全部 15 个 C02 字段和 replay set | none | 创建/重生/移除时的生命周期 owner；打开新的事件历史。 |
| `PlayerCombatProcQuery.Snapshot` | Component 全部 15 个字段 | 无 | immutable snapshot | 单向读取；不是 authority writer。 |

关键重叠是 `GhostDmg` 和 `LifeSteal` 同时被 hit commit 与 tick 使用。
这不是两个 writer：HitCommit 只提交命中增量/消耗，Tick 只提交周期结果，
两者都由同一个 `PlayerCombatProcSystem` 顺序执行。若未来拆成独立运行时
System，必须先取得实际调度和 barrier 证据，并保留单一 commit owner。

## 4. Implementation

落盘的实验闭包为：

- `src/Player/PlayerCombatProcStateComponent.cs`
- `src/Player/PlayerCombatProcSystem.cs`
- `src/Player/PlayerCombatProcSystem.HitCommit.cs`
- `src/Player/PlayerCombatProcSystem.Tick.cs`
- `src/Player/PlayerCombatProcSystem.Lifecycle.cs`
- `src/Player/PlayerCombatProcTickInput.cs`
- `src/Player/PlayerCombatProcTickQuery.cs`
- `src/Player/PlayerCombatProcTickResult.cs`
- `src/Player/PlayerCombatProcQuery.cs`
- `src/Player/PlayerCombatProcSnapshot.cs`
- `src/Player/PlayerCommittedCombatHitEvent.cs`
- `src/Player/PlayerCombatProcHitEffects.cs`
- `Test/Terraria.PlayerCombatProcSplitVerification/Program.cs`
- `Test/Terraria.PlayerCombatProcSplitVerification/Terraria.PlayerCombatProcSplitVerification.csproj`

focused project 使用显式 `Compile` 项引用 C02 源闭包，不通过整个
`Terraria.Player.csproj` 间接扩大验证范围。生产项目仍单独构建，以区分
局部规则证据和项目接入证据。

## 5. Verification Evidence

所有 compile-capable 命令均从实验 worktree 根目录，经
`Build/Tools/Invoke-SerialDotnet.ps1 -DotnetArguments` 串行执行；每次执行前
检查 `dotnet.exe`/`csc.exe`，本轮未发现活动编译进程。

| 阶段 | 命令结果 | 证据 |
|---|---|---|
| RED | `run --project Test/Terraria.PlayerCombatProcSplitVerification/Terraria.PlayerCombatProcSplitVerification.csproj`，exit `1` | 4 个预期 `CS0246`：`PlayerCombatProcTickInput`、`PlayerCombatProcTickResult`、`PlayerCombatProcTickQuery` 缺失。 |
| focused build | 同一 wrapper 的 `build`，exit `0` | 0 warning、0 error；产物 `Build/bin/Terraria.PlayerCombatProcSplitVerification/Debug/net10.0/Terraria.PlayerCombatProcSplitVerification.dll`。 |
| focused run | `run --project ... --no-build --no-restore`，exit `0` | `PASS: player combat proc split behavior`。 |
| isolated production build | 实验 worktree 中的 `build src/Player/Terraria.Player.csproj`，exit `0` | 0 warning、0 error；产物 `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll`。 |
| shared worktree production recheck | 当前承载 worktree 中的 `build src/Player/Terraria.Player.csproj`，exit `1` | 0 warning、5 errors；错误全部来自既有/并行未跟踪的 `src/Player/Progression` 文件（缺少 `Terraria.Relationships`、`Terraria.Projectile`、`EntityReference`、`ProjectileIdentityComponent`）。本次没有把该项目级基线阻断归因于 C02 实验，也没有把失败产物作为证据。 |
| source hygiene | `git diff --check`，exit `0` | 无空白错误。 |

focused verifier 覆盖：C02 默认值和 sentinel、命中幂等、ghost/life 更新、
Expert/normal cap、dash 关闭、inferno wrap、cooldown 不下溢、transient
reset、生命周期 replay history，以及 snapshot 不写回。它验证的是当前
NLTX 实验闭包，不是完整 Version4 主循环。

## 6. Lifecycle, Rollback And Deletion Gate

本实验没有增加网络/存档字段、注册表、事件总线、外部 adapter 或不可逆
副作用。唯一可变的非 Component 状态是 owner 内的 `_acceptedEventIds`；
`Reset` 清理它，重复 committed event 为 at-most-once。Query 为纯计算，
失败只表现为返回/编译失败，不启动后台任务或外部操作。

回滚点是删除三类 tick 文件和 focused verifier，并把
`AdvanceTick` 恢复为原来的直接算术；partial 文件可合并回单文件。回滚
不涉及 schema、network、save、registry 或 runtime scheduler。

以下 P06 删除门禁仍未满足：

- 254 个字段的入站/出站调用闭包和动态入口未全部确认；
- P04 lifecycle、P09 Item、P12/P15 damage source、Network、Persistence、
  Presentation/Town 的跨分区 owner 未闭合；
- 未做真实主循环差分、网络/存档恢复、多世界和运行时调度验证；
- 本实验只覆盖 C02 的 15/254 个字段；
- 没有删除旧 Version4 facade，也没有资格把 P06 runner 会话标记为完成。

因此当前状态只能写作 `focused-verified`，不能写作 migrated、equivalent、
runtime-integrated 或 deletion-eligible。

## 7. ecs-system-domain-splitting Usability

### 有效部分

1. 技能把能力、状态所有权、写入权限和运行时调度分开，直接阻止了“每个
   字段/每个方法新建一个 System”的机械拆分。
2. `keep / partial / separate` 判定适合本案例：partial 用于源组织，纯
   Tick Query 用于可复用确定性计算，最终写入仍归一个 owner。
3. Read/write/emit/order/lifecycle 契约和删除门禁迫使报告区分局部 verifier、
   项目构建、真实运行时接入和可删除状态，避免“源码存在”等同于迁移完成。
4. 证据状态词汇 `confirmed`、`partial`、`proposed`、`unknown` 与实现状态
   分离，适合表达本实验的 15/254 focused 结果。

### 可优化部分

| 问题 | 本轮表现 | 建议 |
|---|---|---|
| 参考路径不稳定 | `ecs-system-domain-splitting` 引用的 `output-risk-profile.md` 和 tModLoader retrieval 文档在当前 checkout 缺失；`public-decomposition` 还引用不存在的旧 `约束/公共拆分约束.md`。 | 技能入口先做路径 preflight；缺失时返回 `missing-reference`，同时给出当前 `Context/约束` 的映射，不让执行者反复试错。 |
| 缺少窄实验模式 | 正文偏向完整迁移/删除门禁，本例只需一个叶子组和一个 System seam。 | 增加 `focused-system-experiment` profile，规定最小成员表、读写集、pure-query 检查、TDD、项目/closure 双验证和明确 deferred 列表。 |
| wrapper 调用契约未显式化 | 直接传 `-p:` 会被 PowerShell wrapper 误绑定；`dotnet run` 省略 `--project` 时也可能不编译目标项目。 | 在技能命令模板中固定 `-DotnetArguments @(...)`、`--project`、`--no-build --no-restore` 的完整 Windows 示例，并单独记录命令层失败。 |
| “唯一写者”缺少自动检查模板 | 本轮需要手工 `rg` 反查 `_component` 写点。 | 增加轻量 writer-scan 输出：按字段列出 writer 文件、Query 是否出现赋值、Projection 是否引用 commit API；动态/反射仍标 partial。 |
| 分区 runner 与 System 实验边界不够醒目 | P06 已有 running session；本实验正确地未重复 claim，但技能没有直接给出“只读权威输入、不结算会话”的模式。 | 增加 concurrent-session policy：System 实验可引用已 claim report，禁止重复 claim/cleanup/settle，报告必须带 session ID 和未结算声明。 |
| 结果状态容易被顶部元数据误读 | 历史 P06 文档同时出现 implemented、source-only、not-verified。 | 要求固定四层状态：source inventory、focused boundary、runtime integration、deletion gate，并禁止上层状态覆盖下层缺口。 |

综合判断：该技能对 System 领域拆分是可用的，尤其适合本次“纯规则提取、
唯一提交者保持不变”的实验；但实际使用应补上路径预检、focused profile、
Windows wrapper 模板和四层状态模型。本轮没有修改技能正文，这些是后续
技能维护建议，不是本实验已经实现的能力。

## 8. Final Disposition

实验代码和本报告已带回当前承载 worktree；该 worktree 当前 ref 为
`codex/npc-damage-tracking-system-experiment-20260917`，与 `main` 在实验前
同指 `3a90e99`，且保留了其它既有未提交改动。focused verifier 已在该
worktree 重新构建并运行通过；当前 worktree 的完整 Player 构建则被上述
Progression 基线错误阻断。确认代码、报告和 focused 证据落地到主线引用
后，再删除临时 worktree 和 `codex/ecs-system-split-p06-experiment-20260917`
分支；实验 worktree 的 `Build/bin` 产物不构成主工作树的验证证据。

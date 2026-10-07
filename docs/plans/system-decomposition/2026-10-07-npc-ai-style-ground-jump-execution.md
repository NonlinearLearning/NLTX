# NPC AI Style Ground/Jump 类别执行文档

状态：类别有限切片已落盘；指定 style 覆盖仍 partial/open
起点快照：`96d893ac09a6aced38df3e3ad0233eb09ed75621`
风格范围：`1, 3, 8, 10, 13, 19, 20, 22, 23, 25, 26, 39, 40, 41, 50, 56, 66, 67, 98, 100, 101, 102, 103, 107`

## 目标与范围

实现上述 NPC AI style 的地面移动、重力/落地、跳跃阈值、障碍/门/台阶、目标重选、碰撞历史、昼夜退出及类型专有效果。保留每个 NPC type 的条件，不把共享公式扩展为所有类型行为。重点检查 Slimes、Fighters、Teleport、Book、Unicorn、ImprovedWalkers、SandShark。

## Style 来源与证据

- 设计：[NPC AI 系统重新设计](../../system-decomposition/2026-10-05-npc-ai-system-redesign.md)，重点为 §2、§3、§4、§5、§6、§7.3、§7.4、§8、§9。设计要求按 type 与条件区分风格覆盖，以每实例状态作为唯一 owner，并保留同 tick 同步效果顺序；有限 profile 不代表来源等价。
- 执行：[NPC AI 系统执行文档](2026-10-06-npc-ai-system-execution-plan.md)，重点为 §1、§3、§4.2–4.3、§5、§9–§11。执行门禁要求逐 profile 读取来源/helper，登记实际身份与端别，有限验证不升级为来源对照。
- 风格来源索引：[128 风格索引](../../system-decomposition/2026-10-05-npc-ai-reference-index.md)，逐行入口与索引候选见第 39–145 行；来源参考 `D:\TRbackup\无任何删减通过编译\Terraria\NPC.cs`。下表中其余候选仍只代表索引记录，不据 NPCID 名称推断完整 type/netID/profile 映射。
- 状态来源：[profile coverage ledger](../../migration/ledgers/2026-10-06-npc-ai-profile-coverage-ledger.md) 中对应风格行。当前只有 style 1 的 Blue/Mother Slime 与 style 3 的 Zombie 有有限 profile 接线；都仍为 partial/open，未登记为 `implemented` 或 `verified`。

## Type / netId / aiStyle / profile 映射

| aiStyle | 来源入口与索引候选 | 已证明的 type / netID / profile | 未闭合映射与状态 |
| --- | --- | --- | --- |
| 1 | `AI_001_Slimes`，dispatch `NPC.cs:20121–20125`，实现 `61133–62551`；BlueSlime (1)、MotherSlime (16)、LavaSlime (59) | Blue `type=1/netID=1/aiStyle=1` → `NpcBlueSlimeProfile`；Mother `type=16/netID=16/aiStyle=1` → `NpcMotherSlimeProfile`，均为有限接线 | 三例身份已 mapped；Lava profile 未接线。其他 style-1 type 专有 `ai[1]` 分支 open |
| 3 | `AI_003_Fighters`，dispatch `NPC.cs:20131–20135`，实现 `56637–61118`；Zombie (3)、Skeleton (21)、GoblinPeon (26) | Zombie `type=3/netID=3/aiStyle=3` → `NpcFighterProfile` 有限接线 | Skeleton/GoblinPeon profile 未映射；其他 Fighter type、世界/难度条件 open |
| 8 | `AI_AttemptToFindTeleportSpotNearBooks` (19166)、`AI_AttemptToFindTeleportSpot` (19092)、`AI_FindNearbyBook` (63141)；FireImp (24)、GoblinSorcerer (29)、DarkCaster (32) | 未有具体 profile | `indexed / unmapped / open / not-run`；候选 ID 不作为 type/netID 配对证明 |
| 10 | 内联 `NPC.cs:21780–22130`；CursedSkull (34)、GiantCursedSkull (289)、WaterBoltMimic (694) | 未有具体 profile | `indexed / unmapped / open / not-run` |
| 13 | 内联 `NPC.cs:22831–23121`；ManEater (43)、Snatcher (56)、Clinger (101) | 未有具体 profile | `indexed / unmapped / open / not-run` |
| 19 | 内联 `NPC.cs:24692–24822`；Antlion (69) | 未有具体 profile | `indexed / unmapped / open / not-run` |
| 20 | 内联 `NPC.cs:24823–24900`；SpikeBall (70) | 未有具体 profile | `indexed / unmapped / open / not-run` |
| 22 | 内联 `NPC.cs:24953–25542`；Pixie (75)、Wraith (82)、Gastropod (122) | 未有具体 profile | `indexed / unmapped / open / not-run` |
| 23 | 内联 `NPC.cs:25543–25622`；CursedHammer (83)、EnchantedSword (84)、CrimsonAxe (179) | 未有具体 profile | `indexed / unmapped / open / not-run` |
| 25 | 内联 `NPC.cs:25848–25940`；Mimic (85)、PresentMimic (341)、IceMimic (629) | 未有具体 profile | `indexed / unmapped / open / not-run` |
| 26 | `AI_026_Unicorns` (63201)；索引候选 Unicorn (86)、Wolf (155)、HeadlessHorseman (315) | Wolf `type=155/netID=155/aiStyle=26` → `NpcWolfGroundJumpProfile`，仅覆盖来源近距反向地面跳跃 `NPC.cs:63398–63404`；identity 由 `NPCID.Wolf=155` (`NPCID.cs:11420`)、`SetDefaults` (`NPC.cs:11075–11083`) 和默认 `netID=type` (`NPC.cs:17934`) 交叉确认 | 仅类别局部 identity/profile slice 已映射；中心 ledger 未更新，仍为 `indexed / unmapped / open / not-run`。type=329 共用跳跃分支但未纳入此 profile；其他 style-26 identity、特殊行为与完整 helper 闭包 open |
| 39 | 内联 `NPC.cs:29484–30010`；GiantTortoise (153)、IceTortoise (154)、SolarSroller (417) | 未有具体 profile | `indexed / unmapped / open / not-run` |
| 40 | 内联 `NPC.cs:30011–30243`；WallCreeperWall (165)、JungleCreeperWall (237)、BlackRecluseWall (238) | 未有具体 profile | `indexed / unmapped / open / not-run` |
| 41 | 内联 `NPC.cs:30244–30507`；Herpling (174)、Derpling (177)、ChatteringTeethBomb (378) | 未有具体 profile | `indexed / unmapped / open / not-run` |
| 50 | 内联 `NPC.cs:32034–32099`；FungiSpore (261)、Spore (265) | 未有具体 profile | `indexed / unmapped / open / not-run` |
| 56 | 内联 `NPC.cs:33142–33163`；DungeonSpirit (288) | 未有具体 profile | `indexed / unmapped / open / not-run` |
| 66 | 内联 `NPC.cs:34717–34812`；Worm (357)、TruffleWorm (374)、GoldWorm (448) | 未有具体 profile | `indexed / unmapped / open / not-run` |
| 67 | 内联 `NPC.cs:34813–35087`；Snail (359)、GlowingSnail (360)、MagmaSnail (655) | 未有具体 profile | `indexed / unmapped / open / not-run` |
| 98 | 内联 `NPC.cs:41913–42223`；索引未得到示例 | 未有具体 profile | 保留 `indexed / unmapped / open / not-run`，不根据零示例删除入口或补造算法 |
| 100 | 内联 `NPC.cs:42291–42369`；AncientLight (522) | 未有具体 profile | `indexed / unmapped / open / not-run` |
| 101 | 内联 `NPC.cs:42370–42450`；AncientDoom (523) | 未有具体 profile | `indexed / unmapped / open / not-run` |
| 102 | 内联 `NPC.cs:42451–42847`；SandElemental (541) | 未有具体 profile | `indexed / unmapped / open / not-run` |
| 103 | 内联 `NPC.cs:42848–43033`；SandShark (542)、SandsharkCorrupt (543)、SandsharkCrimson (544) | 未有具体 profile | `indexed / unmapped / open / not-run` |
| 107 | `AI_107_ImprovedWalkers` (63766)；DD2GoblinT1 (552)、DD2GoblinT2 (553)、DD2GoblinT3 (554) | 未有具体 profile | `indexed / unmapped / open / not-run` |

除明确列出的 Blue Slime、Mother Slime、Zombie 和本续批证明的 Wolf 外，括号数字是参考索引中的候选 ID 文本，不默认等于 `type` 或 `netID`。其余身份须逐个核对 `SetDefaults`、netID 赋值、变体和变换后的 profile；本批不将其改为已映射。

## 状态所有者与任务生命周期

设计 owner 是每个 NPC 实例唯一的权威 `ai[4]`/动作状态：`NpcBehaviorStateComponent` 持有动作和 ai 槽，`NpcLocalBehaviorStateComponent` 持有同一实例的 `localAI[4]`；`NpcAiStateComponent` 是值快照，不得作为第二份运行态。profile 输入/结果描述一次确定性状态转移。Zombie grounded idle counter、door wait counter、方向/动作与 Slime grounded counter、jump phase、变体状态均按实例隔离，不放入共享 profile/static。

本类别目前没有单独的 `NpcTaskStateComponent` 持续任务。Slime/Fighter 跨 tick 进度留在该 NPC 的 ai 槽位中；进入、完成、中断、释放和变换时如何初始化/清理槽位，除已列有限分支外仍按具体来源核实。某个 type/netID 的变体状态不能由同 style 其他 profile 自动继承。

新增 `NpcFighterProfileAuthorityAdapter` 只封装 type=3 profile 的评估和效果提交：明确接收 `netMode`，仅允许单人 authority (`0`) 或 multiplayer server (`2`)；client (`1`) 和未知值都拒绝，且不执行 profile 计算或调用效果端口。新增 `NpcWolfGroundJumpAuthorityAdapter` 对 type=155 的有限 ground-jump 计算采用相同 allowlist。类别 adapter 不是共享 runtime gate，当前也尚未接入 `RuntimeNpcStore`。

## API、更新顺序与 authority

有限 Fighter API 为 `NpcFighterProfileInput → NpcFighterProfile.Evaluate → NpcFighterProfileResult`，效果经 `NpcFighterProfile.ApplyEffects`，保持目标重选、同步、开门的显式调用顺序。新增 `TryEvaluateAuthoritative(netMode, input, out result)` 与 `TryApplyAuthoritativeEffects(netMode, result, effectPort)` 为后续 host 集成提供 fail-closed 边界；当前宿主仍直接调用 profile，因此不能声称 runtime client AI 已关闭。

当前有限宿主顺序（来源等价尚未证明）：`RuntimeNpcStore.Update` (`RuntimeNpcStore.cs:1590`) 先推进伤害跟踪，再逐槽自然 despawn，然后调用 `UpdateMovement` (`:1769`)。该路径捕获碰撞历史、湿态和目标快照，在 AI 前应用 gravity；style=3 分支进入 `UpdateFighterMovement` (`:2733`)，捕获目标与 tile traversal，直接调用 `NpcFighterProfile.Evaluate` (`:2760`)，提交方向/ai 槽 (`:2794`)，执行 profile effects (`:2808`)，应用跳跃冲量 (`:2820`)，再 gravity 和 `TileCollision` (`:2826–2843`) 与有限碰撞后跳跃 (`:2844–2848`)。这是 finite host 路径，不是完整参考 `UpdateNPC` 顺序。

style=26 Wolf ground-jump slice：来源 `AI_026_Unicorns` 在 `NPC.cs:63398–63404` 对 `type == 155 || type == 329` 使用当前 target；仅 `velocity.Y == 0`、来源 `num9 < 100`、`abs(velocity.X) > 3` 且 NPC 正沿水平方向朝 target 移动时执行 `velocity.Y -= 4`。profile 只为已核对身份 type=155/netID=155/aiStyle=26 建模这段分支；使用来源 `Math.Sqrt` 距离和整除后的半宽方向比较。原分支没有设置 `netUpdate`。它位于 `TargetClosest` (`NPC.cs:63421–63430`) 之前，因此 host 必须传入当次 AI 开始时已选中的 player 快照。当前 `RuntimeNpcStore.UpdateMovement` 没有 style=26 handler；新增 profile/adapter 尚未接线，不能视为实际宿主实现。

**已发现的 client AI 路径，当前未接入修复：** `RuntimeNpcStore.Update` 没有 `netMode` gate，`netMode=1` 仍会进入 `UpdateMovement`、通用/精确 profile、目标选择和物理路径。`NpcBlueSlimeProfileInput.IsClient` 仅挡住体内物品首次选择，不阻止 Blue Slime profile 计算、状态推进、目标重选或移动。Fighter 当前直接调用 `NpcFighterProfile.Evaluate/ApplyEffects`，未使用本批 adapter。Eye 的 `RuntimeNpcStore` 效果端口只在 servant spawn 处检查 client。`TargetHostVerification` client 案例明确设置 `netMode=1` 并调用 `npcStore.Update(1, ...)` (`Program.cs:693–701`)，只断言不创建 Servant，不能证明其他 client AI 未运行。

主会话唯一集成点：在 `RuntimeNpcStore.Update` 的首个可观察效果前、`AdvanceDamageTrackingTo(tickNumber)` (`RuntimeNpcStore.cs:1598`) 之前接入一次统一 authoritative AI tick gate；client 走独立 replication/observe/presentation 路径，不执行该 store 的 AI/物理循环。主会话负责公共入口集成，本类别不改 `RuntimeNpcStore.cs`、`NpcAiSystem.cs`、Simulation `Program.cs`、项目文件或中央 ledger，也不建立第二个共享 gate。主会话可在接入时把 Fighter profile/effect 调用替换为本类别 adapter。

### 服务端权威硬约束

NPC AI 决策、目标选择、随机消费、物理/碰撞权威、生成、变换、奖励、任务推进、关系修改及 authority effects 只能在服务端/权威端执行。客户端仅消费服务端复制的状态、网络同步意图和呈现数据；禁止通过 client tick 运行或补算 AI、预测权威效果、创建权威实体。类别层的 Fighter 与 Wolf adapters 提供 type-specific fail-closed 评估入口；已发现的共享 client tick 路径登记为未接入，等待主会话在单一宿主入口完成统一 gate。

`Test/Terraria.NpcAi.TargetHostVerification/Program.cs` 原有断言未修改或放宽：约 671–701 行要求 `netMode=1` 客户端 active count 不增加且保留 `ServantSpawnSkipped:ClientAuthority` 证据；约 703–733 行要求 `netMode=2` 服务端 active count 增加并观察到已生成 Servant。该文件仍不足以证明客户端没有执行其他 AI；主会话 gate 集成后应保留这两组断言，并增加客户端 AI/物理状态不推进的断言。

## 改动文件与集成边界

- `src/NSSLC/Component/Npc/NpcFighterProfileAuthorityAdapter.cs`：新增 type=3 profile 的权威 mode API，负责纯评估和窄 effect 提交，不读取 host/global `netMode`。
- `src/NSSLC/Component/Npc/NpcWolfGroundJumpProfileInput.cs`、`NpcWolfGroundJumpProfileResult.cs`、`NpcWolfGroundJumpProfile.cs`：增加 type=155/netID=155/style=26 的纯 ground-jump slice；保留来源距离运算和方向比较使用整数半宽的规则。
- `src/NSSLC/Component/Npc/NpcWolfGroundJumpAuthorityAdapter.cs`：为 Wolf slice 提供 fail-closed 的 netMode 0/2 评估入口；尚未接入共享 host。
- 本 Markdown：唯一类别执行文档，记录来源映射、调用点、authority 缺口、集成步骤和精确回退范围。
- 未修改共享 `Test/Terraria.NpcAi.Verification/Program.cs`、`Test/Terraria.NpcAi.TargetHostVerification/Program.cs`、公共 `RuntimeNpcStore.cs`、`NpcAiSystem.cs`、Simulation `Program.cs`、项目文件和中央 coverage ledger。共享 client tick gate 与 client observe/replication/presentation 接线留给主会话。

## 验证记录

遵循 [构建与验证约束](../../../Context/约束/构建与验证约束.md) 和当前主会话边界：本续批只做源级断言与 `git diff --check`，不运行 build/restore/run 或任何测试。Focused ground/jump runtime verifier 记为 `not-run`；authority adapter 和 Wolf profile 不因旧 DLL 的运行结果而宣称 runtime verified。

本批实际验证：

| 检查 | 命令 | 结果 | 证据/边界 |
| --- | --- | --- | --- |
| profile authority 静态不变量 | PowerShell 读取 `NpcFighterProfileAuthorityAdapter.cs`，检查 allowlist 为 netMode 0/2、评估和效果 guard 均先于实际调用、无全局 `Main.netMode`/随机读取 | exit 0；四项均 PASS | 仅为源码结构检查，不是编译或 runtime 证明 |
| 类别范围/必需栏目静态检查 | PowerShell 读取本 Markdown，比较 24 个 style 行与指定列表并检查 mapping/owner/order/authority/integration/rollback 栏目 | exit 0；24/24，缺失栏目 0 | 本文件为交付报告路径 |
| TargetHost authority 断言保留 | `rg -n -C 2 'netMode = 1|A client Eye update must not allocate|ServantSpawnSkipped:ClientAuthority|netMode = 2|authoritative server Eye update must be able to allocate|ServantSpawned\(slot=' Test/Terraria.NpcAi.TargetHostVerification/Program.cs` | exit 0 | client netMode=1 不新增 active NPC 并保留 skip trace；server netMode=2 新增 Servant 且 trace 保留，源码约 693–733 行 |
| 共享 verifier diff 清理 | `git diff --exit-code -- Test/Terraria.NpcAi.Verification/Program.cs Test/Terraria.NpcAi.TargetHostVerification/Program.cs` | exit 0；无 diff | 只恢复了本轮触碰文件的混合换行；两个共享 verifier 内容与起点一致 |
| Wolf identity、authority、公式源码断言 | PowerShell 检查 `NpcWolfGroundJumpProfile*.cs` 与 `NpcWolfGroundJumpAuthorityAdapter.cs` 的精确 identity、0/2 allowlist、client guard 次序、target availability guard、距离/速度/方向阈值及 `-4f` 冲量；并核对参考源码行 | 待本续批检查完成后填写 | 不代替编译、运行或来源 golden |
| whitespace 检查 | `git diff --check`；PowerShell 对新增 adapter 和 Markdown 检查 trailing whitespace/tab | 两者 exit 0；新增文件干净 | `git diff --check` 检查 tracked diff，PowerShell 检查本轮新增文件 |
| ground/jump focused runtime verifier | 未运行 | not-run | 当前主会话边界只允许源级检查与 `git diff --check`；没有用旧 DLL 冒充本轮 adapter 验收 |

本轮 adapter 源级检查使用的 PowerShell 命令：

```powershell
$ErrorActionPreference='Stop'; $p='src/NSSLC/Component/Npc/NpcFighterProfileAuthorityAdapter.cs'; $s=Get-Content -Raw -LiteralPath $p; $e=$s.IndexOf('TryEvaluateAuthoritative'); $eg=$s.IndexOf('if (!IsAuthoritativeNetMode(netMode))',$e); $ec=$s.IndexOf('NpcFighterProfile.Evaluate(in input)',$e); $f=$s.IndexOf('TryApplyAuthoritativeEffects'); $fg=$s.IndexOf('if (!IsAuthoritativeNetMode(netMode))',$f); $fc=$s.IndexOf('NpcFighterProfile.ApplyEffects(in result, effectPort)',$f); $ok=$e -ge 0 -and $eg -gt $e -and $ec -gt $eg -and $fg -gt $f -and $fc -gt $fg -and $s.Contains('SinglePlayerNetMode = 0') -and $s.Contains('MultiplayerServerNetMode = 2') -and $s -notmatch 'Main\.netMode|Main\.rand'; if (-not $ok) { exit 1 }; Write-Output 'PASS: netMode allowlist 0/2; evaluation/effect guards precede calls; no global mode or random access.'
```

未执行 `dotnet build`、`dotnet restore`、solution build 或任何测试运行；不存在本轮生成的机器报告。权威交付/报告为本 Markdown。唯一允许的增量构建由主会话对 `Terraria.NpcAi.TargetHostVerification` 负责。

## 缺口与回退

- 24 个指定 style 中当前有 style 1、3 的有限既有 profile，以及 style 26 的 Wolf 近距地面跳跃纯计算 slice；三者均未完成来源行为闭包。style 26 尚未进入 central ledger；其余指定 style 仍保持 `indexed / unmapped / open / not-run`，style 98 保留无示例条目。
- style 1/3 的其余 type/netID、特殊世界/难度/液体分支、来源随机流、helper 闭包、来源 golden、真实 authority sink、完整碰撞 scratch 和退出/变换语义仍 open。
- 类别 adapters 尚未接入真实 host。已确认共享 `RuntimeNpcStore.Update` 在 client netMode=1 仍进入 AI/目标选择/物理路径；统一 host gate 与 client observe/replication/presentation 为主会话集成缺口。Wolf style=26 分支还必须在本轮 `TargetClosest` 之前消费当前 player target，并由宿主继续承担重力、碰撞和 movement commit。
- authority 证明边界：Fighter 与 Wolf adapter 源码仅在 netMode 0/2 继续到对应 profile，其他 mode 返回 false；TargetHostVerification 源码断言保留且未放宽，但本续批未构建/运行该 verifier，不能声称 runtime authority 修复完成。
- 精确回退范围：本续批只删除 `src/NSSLC/Component/Npc/NpcWolfGroundJumpAuthorityAdapter.cs`、`NpcWolfGroundJumpProfile.cs`、`NpcWolfGroundJumpProfileInput.cs` 和 `NpcWolfGroundJumpProfileResult.cs` 即可回退 Wolf slice；如整体回退前批 Fighter adapter，再单独删除 `NpcFighterProfileAuthorityAdapter.cs`。保留本执行文档作为来源/缺口历史，保留既有 `NpcFighterProfile` ground/jump/door 行为、共享 verifier 与其现有断言、ledger 和历史证据。回退不撤销已经提交的 runtime 状态或世界效果，也不回退任何用户既有修改。

## PUA 失败计数与七项自检

- 失败计数：3。第一次是 PowerShell 下 `rg ... src/NSSLC/Component/Npc/*Profile.cs` 的 glob 被当作无效路径，ripgrep 报 OS error 123（当时未捕获数值 exit code）；改用 `rg -g '*Profile.cs' src/NSSLC/Component/Npc` 后搜索通过。第二次是前一阶段合并式 `apply_patch` 因上下文不匹配失败；第三次是本续批综合 patch 引用了不存在的验证表行，未应用任何文件变更。两次都改为小块、逐段 patch 后通过，没有留下部分变更。
- 三铁律证据：没有在工具排查前询问；已读原始设计/执行/索引/owner 与 authority 调用点；除最小 Fighter adapter 外主动检查并登记共享 client AI 路径、上下游与主会话集成点。
- 七项自检：
  - [x] 读失败信号：逐字读取 rg 的无效路径错误与 patch 上下文不匹配输出。
  - [x] 主动搜索：检查指定 styles、client netMode 接线、profile 与 target host authority 断言。
  - [x] 读原始材料：完整读取 NPC AI 设计和执行文档，并读取当前 host/profile/verifier 源码上下文。
  - [x] 验证前置假设：确认 HEAD 等于指定快照、共享 Program 起点干净、当前 host 没有 central netMode gate、adapter 只接收显式 mode。
  - [x] 反转假设：反查“client 只跳过 Servant 就已足够”的假设；发现 client tick 仍进入通用 AI、目标和物理路径。
  - [x] 最小隔离：将本续批新增实现限制为 type=155 的单个 ground-jump 分支，并把 authority mode 检查留在独立 adapter。
  - [x] 换方向：从失败的大 patch/路径 glob 切换为分段补丁、目录级 ripgrep filter 与 PowerShell 源级不变量检查。

## 最终工作树快照

- 分支：detached `HEAD`（当前没有本地分支名）；起点/当前 HEAD：`96d893ac09a6aced38df3e3ad0233eb09ed75621`。
- 本续批新增四个 Wolf profile 源文件；类别 Markdown 和 Fighter adapter 是既有未跟踪工作。共享 `NpcAi.Verification/Program.cs`、`TargetHostVerification/Program.cs` 未修改。
- 本续批 source-only identity/authority/formula check 和 `git diff --check` 结果待下文实跑后补记；既有 TargetHost authority 断言仍由先前只读源码检查记录，本续批未运行 verifier。

## 交付前自检

- [ ] 检查同类 style/type 行为、上下游顺序、authority 与边界。
- [ ] 检查失败路径、目标重选、昼夜退出及状态生命周期。
- [ ] 确认只运行一个最小 focused/no-build 验证，未扩大为全量构建/测试。
- [ ] 记录 branch、文件、命令/exit code、报告、authority 断言证据、缺口和最小集成步骤。



# NPC AI 系统执行文档

文档 ID：DOC-2026-10-06-NPC-AI-SYSTEM-EXECUTION-PLAN  
逻辑域：plans  
产物类型：plan  
状态：active  
执行状态：in_progress（A–E 已有有限切片；完整参考 AI 迁移尚未执行完成）
证据状态：partial（已完成入口索引和有限规则接线）  
复核日期：2026-10-06（Asia/Shanghai）  
范围：NPC AI 风格 0–127，普通敌人、Boss、城镇、宠物、召唤与群体行为  
设计入口：[NPC AI 系统设计](../../system-decomposition/2026-10-05-npc-ai-system-redesign.md)  
canonical 路径：docs/plans/system-decomposition/2026-10-06-npc-ai-system-execution-plan.md

本文规定后续实施任务和验收，不记录尚未发生的代码与测试结果。执行人员以配套设计的
状态 owner、API 和更新顺序为契约，从 A 阶段开始，按具体 profile 完成小切片后接入。
完整行为以指定 Terraria 源码为依据；本地 ECS 项目提供组织方式参考。设计变更同步维护
设计文档，实际运行结果写入独立验证记录。

## 1. 输入、基线和改动边界

工作目录为 `D:/TRbackup/NLTX`，Shell 为 PowerShell。行为参考根目录为
`D:/TRbackup/无任何删减通过编译`；组织参考根目录为 `C:/Users/shan/Downloads/ECS`，
二者只读。开始源码实施前重新读取根 AGENTS.md 及其适用的 C#、命名、文件组织、
副作用、架构边界与构建约束。保留工作区已有的无关修改。

| 基线项目 | 已有材料 | 尚需完成 |
| --- | --- | --- |
| 参考入口 | [128 风格索引](../../system-decomposition/2026-10-05-npc-ai-reference-index.md) | 具体 type/netId、世界/难度分支及传递调用闭包 |
| 有限规则 | [NpcAiSystem](../../../src/NSSLC/Component/Npc/NpcAiSystem.cs) 的七个显式 netId | 完整执行入口、效果契约、来源 profile 注册 |
| 有限宿主 | [RuntimeNpcStore](../../../src/NSSLC.Tools.Simulation/RuntimeNpcStore.cs:430) | 原版完整更新顺序、动态物理门控和即时效果 |
| 既有验证 | [有限接入记录](../../system-decomposition/2026-10-05-npc-ai-redesign-verification.md) | 完整来源行为、多 tick 生命周期和联机权威验收 |
| 组织依据 | [ECS AI 源码对照](../../research/2026-10-06-ecs-ai-reference-study.md) | 把适用建议接入 NLTX 并验证 |

有限 netId 1、2、3、16、22、37、488 的已有结果只能用于有限规则回归。当前 catalog 中
“Green Slime 16”、Guide/Old Man 风格 0 和 Dummy 风格 0，与参考身份/风格存在差异。
A 阶段先登记映射，完整内容启用时显式选择来源 profile，不把有限夹具改名后算成迁移完成。

正式变更主要放在 `src/NSSLC/Component/Npc/` 及实际协作的 Content、SpatialSimulation、
Relationships、Combat、Town、WorldSession、Projectile 等领域；宿主接线放在现有宿主。
只有稳定职责和文件规模需要时才增加子目录。不在只读参考目录或
`src/NSSLC.Infrastructure/分类参考/` 实现代码，不预建全部 Boss/任务的空目录和项目。

## 2. 依赖、批次与优先顺序

以下编号对应设计第 8 节。每阶段可分多个 profile 小批次；具体能力依赖决定顺序，不要求
所有普通敌人或所有城镇 NPC 完成后才能开始一个依赖已齐的 Boss。

| 阶段 | 前置 | 实施重点 | 阶段状态 |
| --- | --- | --- | --- |
| A：来源与覆盖登记 | 现有设计/索引 | 来源绑定、身份映射、调用与效果闭包、对照执行条件 | in progress：三份来源指纹已复核，128 风格登记已建立；9 个具体 profile 已登记，5 个精确有限 handler 已接线；G3 已扩至 Eye/Blue/Mother 的 32 场景/182 tick，速度差异为 0，仍有 8 个状态与 11 个效果/可观察差异；完整传递效果闭包和各 profile 来源对照仍待完成 |
| B：完整执行骨架 | A 的相关契约已明确 | 唯一状态、目标策略、实时槽位、物理历史、即时生成、随机 | in progress：有限宿主由 NpcBehaviorStateComponent 持有动作和 ai[4]，NpcLocalBehaviorStateComponent 持有实例 localAI[4]；NpcAiSystem 与死亡阶段 authority 共用权威 ai[4] 提交入口。NpcInstanceId 已改为创建时分配的进程内单调唯一身份，槽位仅作兼容投影；ECS EntityUuid/RuntimeEntityHandle 也由每个运行时身份分配。探针覆盖槽位复用、reset、实际两次世界切换、实体根引用和旧对象/旧世界引用拒绝。状态收敛后专用 verifier 和 34 项宿主回归通过，见验证记录。旧 NpcBehaviorComponent 保留到 G 删除门禁。完整 TargetClosest/关系输入、来源效果、动态物理闭包和 authority/network 仍未接入 |
| C：普通敌人和生物 | B 与该 profile 所需能力 | 地面、飞行、水中、环境与特殊敌人 | in progress：Blue Slime C1 已接入有限运动和普通世界 `ai[1]` 体内物品选择；Demon Eye C2 与 Zombie C3 接入有限路径；Mother Slime C1.3 已按精确 `type=16/netID=16/aiStyle=1` 接入公共湿态/跳跃/计数切片并通过纯 verifier 与 120 tick 宿主 smoke；来源效果、catalog 身份、authority/Tile/network 闭包仍待完成 |
| D：城镇与救援 | B、住房/世界事实及效果契约 | 城镇 7、风格 0 特例、宠物、124–127 救援 | in progress：Guide D1 日间任务、夜间跨时钟宿主验证、D2 有限回家传送和 D3 住房重新验证/owner 同步切片已通过；完整路径、坐具、交谈、救援仍 open |
| E：关系、召唤与分节 | B、生成/关系/生命根契约 | 父子、相邻节、多部位、断链、随从 | in progress：E1 父子生命周期与 E2 三节点根归属、根死亡整链释放、中段释放断链、代际拒绝和部分链容量回滚已通过；具体虫链/多部位/召唤者来源 profile 仍 open |
| F：Boss 与遭遇 | B 及具体 Boss 所需的 C/E/世界能力 | 专有阶段、攻击、事件/波次/目标协作 | in progress：F1 Eye of Cthulhu `type=4/netID=4/aiStyle=4` 纯 verifier 和历史 37-evidence 宿主回归通过；最新夜间切片已验收 Classic 599/600/601 与 Expert 209/210/211 的悬停/首次冲刺边界，主会话补跑第二/第三次冲刺（Classic 752/903、Expert 312/413），与 1053/513 tick 返回悬停的报告合计十二条真实运行全部 exit 0。首次/后续冲刺速度分别为 6/7。绑定的正常 Simulation build exit 0、17 warnings / 0 errors；并行写入后的完整回归未重跑。白天 smoke、失败和历史构建仍保留。完整变身/仆从、网络 authority、完整目标输入、呈现、save/load/unload cleanup 与来源 golden 仍 open，F1 整体保持 partial |
| G：全量接线与验收 | C–F 的必需 profile 已实现 | 剩余入口、外围更新、联机、卸载/保存、删除门禁 | in progress：G1 库存校验通过；最新 128/128 style 行一致、124 indexed、4 mapped、9 个 mapped profile、5 个 exact finite handler、0 个 implemented/verified；新增 --require-complete 正确返回 exit 2，拒绝当前 128 style / 9 profile 未闭合状态；G2 三文件 6/6 哈希比较通过；G3 的 32 场景/182 tick 实际来源差分剩余 19 项；任务终止/引用推进已通过有限生产宿主验收，完整行为 golden、网络 authority、保存/卸载和删除门禁仍 open |

首批执行顺序：A 的身份及源调用核对 → B 的调度/状态最小骨架 → 一个实际 Slime 来源
profile → 目标/物理契约补齐 → 一个关系生成切片和一个城镇切片 → 逐 Boss 扩展。
首个 Slime 的 type/netId 在 A 阶段从参考解析，不直接采用有限清单里的名称。

## 3. A：固定来源和实施登记

1. 对 NPC.cs、Main.cs、NPCID.cs 重算 SHA-256，与索引指纹核对。变化时重新定位受影响
   分支；旧行号不能继续作为当前来源证明。
2. 从 SetDefaults、特殊世界覆盖和运行时变换解析 type/netId/aiStyle，建立具体 profile
   登记。无默认类型示例的 98/124 风格仍保留待解析条目；禁止因零命中删除入口。
3. 每个首批 profile 从 AI 分支展开其 helper、目标选择、随机、生成、战斗、空间和网络
   调用，记录读写对象、即时返回、效果顺序、权限及退出路径。
4. 查明参考现有可运行产物或建立隔离的对照运行方式。先复现一条多 tick 行为；没有
   来源输出的切片可实施，但验证状态保持 not-run，不使用新实现自己的输出作 golden。
5. 登记内容差异、容量失败保护等已知差异；来源存根或空函数保持 unknown，不补造算法。

执行时维护一个覆盖登记，逻辑位置归 `docs/migration/ledgers/`；下面是字段契约，登记文件
尚未在本次创建。结构化产物与其说明放在同一语义目录，诊断 trace 放在 Build。

| 登记项 | 必需内容 |
| --- | --- |
| 来源身份 | 文件指纹、风格分支、辅助调用和来源 profile |
| 内容范围 | type、netId、定义/变体、难度、世界条件、authority mode |
| 状态演进 | indexed → mapped → implemented → verified；另记验证 not-run/failed/passed |
| API 与状态 | 源行为到新 API 的组合、owner、初始化/失效、同步与跨 tick 效果 |
| 验证范围 | 场景、初态、输入与随机来源、预期来源、实际输出目录 |
| 差异/缺口 | 已知差异、未闭合调用、未验证条件和影响的结论 |

退出条件：128 个入口都在登记中；首批具体 profile 的身份、依赖和可观察结果明确；未知
内容显式拒绝或保留已声明的有限规则。静态索引完成不等于 A 的来源行为对照已经完成。

## 4. B：建立完整单实例执行路径

### B1：状态和内容选择

核对 [NpcBehaviorComponent](../../../src/NSSLC/Component/Npc/NpcBehaviorComponent.cs)、
[NpcBehaviorStateComponent](../../../src/NSSLC/Component/Npc/NpcBehaviorStateComponent.cs)
及宿主 RuntimeNpcEntity。为完整路径选择唯一 ai/localAI 权威表示，初始化与变换通过该
写入协议；有限路径的快照 API 继续按其声明范围使用。类型化阶段是权威状态的访问器或
明确转换后的替代表示，不每 tick 双向同步两套字段。

按实际解析的内容 profile 选择处理器。处理器保存共享算法，实例保存目标、阶段、计时和
执行进度。变换后重新解析身份及失效事实，是否继续当前分支按源行为决定，不从 TickOne
入口重跑已提交的生成或奖励。组合入口确认每世界注册、创建和卸载边界。

有限宿主的 NPC 实例身份由创建/恢复入口单调分配，槽位只用于兼容访问，不再构成身份根。
当前验证覆盖同槽复用和 store reset；这只证明该宿主的运行时身份不别名，尚未接入完整
NPC 关系、变换或参考来源的创建闭包。

### B2：目标、随机和效果契约

分别实现普通/WOF/Upgraded 的候选计算与目标提交，保留平局、aggro、noAggro、gross、
混乱方向、玩家/NPC 目标及坦克宠物几何。核对现有目标组件的表达能力后补齐，不能统一
替换成最近欧氏距离。

复用 Combat、Status、Spawn、Projectile、Item、Tile、Housing 的现有公开能力；不足的
能力在对应 owner 补契约。AI 的 SpawnNow/TransformNow 是目标能力语义，不能把现有
[NpcSpawnSystem](../../../src/NSSLC/Component/Npc/NpcSpawnSystem.cs) 的自然生成循环控制结果
误当成创建 handle。即时生成返回已创建实例或容量拒绝，然后才能附加关系/初始化子状态。

随机使用显式来源，保持 authority mode 下的消费次序和参数；诊断不抽样。含共享 scratch
的空间读取保持串行，并在调用点捕获 side channel。网络与呈现结果失败时不重跑领域生成。

2026-10-06 已落地目标策略的第一段契约：`NpcTargetSelectionSystem` 分开实现普通、WOF
和 Upgraded 的候选扫描与提交结果。普通/WOF 使用来源的曼哈顿距离、aggro、noAggro、
gross、严格小于平局及坦克宠物门控；Upgraded 使用欧氏距离、548 NPC 候选、自己的宠物
规则和无候选早退。`NpcTargetSelectionStateComponent` 只在结果声明 `ShouldCommit` 时
提交 legacy 目标、几何、方向和 netUpdate 意图；`NpcTargetSelectionInputs` 的宠物
`CanHit` 必须由调用边界先通过现有空间事实查询取得，选择查询不读取共享 scratch。
`RuntimeNpcEntity` 现在为每个实例挂载 `NpcTargetSelectionStateComponent`，
`RuntimeNpcStore` 的 Blue Slime/Demon Eye 目标重选端口通过
`SelectFinitePlayerTarget` 调用 `NpcTargetSelectionSystem.Select`，并提交目标组件、几何、
方向和即时 network intent。该宿主适配器显式使用有限默认值（Aggro=0、NoAggro=false、
Gross=true、无坦克宠物、无 NPC 候选），因此这是真实 tick 的 B2 接线证据，不是完整来源
输入闭包。aggro/noAggro/gross、坦克宠物、548 NPC、WOF/Upgraded 和 authority/network
仍是 open 项。

有限宿主 B2 验收记录（2026-10-06）：

| 验证 | 命令/产物 | 结果 |
| --- | --- | --- |
| Simulation 宿主构建 | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal` | exit 0；0 warning / 0 error；产物在 `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/` |
| NPC AI verifier | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal`；`dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore` | 构建 0 warning / 0 error；运行 PASS |
| C1 真实 tick | `... 120 --players 0 --spawn-npc 1` 与 `... 120 --players 1 --spawn-npc 1` | 两次 `Succeeded=true`；120 ticks；目标选择调用与移动路径进入宿主；报告观察位置/碰撞，分别为 `Build/diagnostics/NpcAiRedesign/runs/stage-b2-target-selection-runtime-20261006/c1-smoke-120.json` 和 `c1-smoke-120-player.json` |
| C2 真实 tick | `... 60 --players 0 --spawn-npc 2` | `Succeeded=true`；60 ticks；报告为 `Build/diagnostics/NpcAiRedesign/runs/stage-b2-target-selection-runtime-20261006/c2-smoke-60.json` |
| 完整宿主回归 | `Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath <world> -OutputDirectory Build/diagnostics/NpcAiRedesign/runs/stage-b2-target-selection-runtime-20261006/full-verifier-20261006` | exit 0；`summary.json` `Succeeded=true`；33 evidence；Simulation/clock build/run 均 0 warning / 0 error |

### B3：更新编排与物理

以 [ActiveNpcTickPhase](../../../src/NSSLC.Tools.Simulation/ActiveNpcTickPhase.cs) 和
[RuntimeNpcStore.UpdateMovement](../../../src/NSSLC.Tools.Simulation/RuntimeNpcStore.cs:534)
为实际接线位置，逐项核对设计第 6 节的源时序。当前自然销毁前置、有限 Eye 直接移动和
定义中的默认物理门控不能被认定为完整调度。

保留实时槽位循环，每实例完成状态/DOT/变换、重力准备、AI、AI 后物理与碰撞、同步及
活动检查后，再读下一槽位。补齐 oldVelocity、collideX/Y、湿态、动态 noGravity/
noTileCollide 等事实；碰撞后的合法 ai 写入不能被先前快照覆盖。

2026-10-06 已接入 B3 的有限宿主状态切片：`NpcMovementTickStateComponent` 在每个运行时
NPC tick 开始捕获 oldPosition/oldVelocity 和 Wet/Shimmer/Honey 事实，AI 返回后提交
`noGravity/noTileCollide`，碰撞完成后提交 collideX/collideY。现有槽位循环仍逐槽读取当时
的实体；默认有限行为的两个物理门控保持 false，Training Dummy 仍按既有静止分支处理。
该切片只建立状态归属与提交顺序，未实现来源完整状态/DOT/变换/碰撞闭包，也没有把动态
门控声称为来源等价。

本批宿主回归还暴露并修复了一个提交边界问题：致死投射物命中会先释放 NPC 运行时身份，
因此掉落效果必须使用伤害提交前捕获的位置，不能在释放后重新读取 `npc.Movement`。修复后
有限宿主和双世界切换回归通过；这项修复只保证当前投射物掉落路径的释放顺序，不替代完整
来源奖励/生成效果闭包验收。

B 的必需验收：

- 当前槽生成到后槽，本 tick 更新；生成到已遍历前槽，下 tick 更新；父可立即读取子状态。
- 释放/复用槽位、跨会话引用及变换后的 profile 校验正确，不把旧目标绑定到新实例。
- 重力准备前后速度、AI 修改物理门控、碰撞历史及碰撞后 ai 写入与参考一致。
- 目标评分/提交及诱饵几何、随机消费、满容量拒绝的已完成部分效果可观察。
- 单玩家/服务端/客户端的权威效果按源条件执行，连续推进不会重复一次性初始化。

退出条件：一个来源 profile 经完整更新路径接线并多 tick 对照通过；基础契约验证通过。
仅 Evaluate 的输入不变/可重复求值不能证明完整路径具有同样的纯计算契约。

## 5. C：普通敌人和环境行为

从风格 1/2/3 的实际来源类型开始，再按能力依赖扩展飞行、水中、跳跃、行走、环境生物和
特殊敌人。类型示例只用于选择批次，全部剩余条目以覆盖登记为准。

每批读取原分支和 helper；迁移计数器、目标重选、运动/碰撞、昼夜/水态、伤害门控、随机
和专有生成/变换。只提取已证明相同的公式，原先嵌入 AI 的生成/开门/放置等交给相应 owner
同步提交。Gnome 石像、体内物品、捕捉及特殊世界变化分别保留成功/失败效果顺序。

### C1：Blue Slime type=1 / netID=1

来源闭包固定在 `NPC.cs:20121-20125` → `AI_001_Slimes` (`NPC.cs:61133-62551`)；默认身份由
`SetDefaults` (`NPC.cs:8686-8699`) 和 `NPCID.cs:11112` 复核。首个切片只接入可证明的普通状态
转移：`ai[0..3]` 地面计数与三段跳跃阈值、方向初始化、湿态速度处理、目标重选边界、空中
加速、`-999` 冻结哨兵和 `netUpdate` 请求。精确 type=1 来源身份还固定了尺寸 24×18 和
value=25。普通非 Skyblock 世界的非客户端体内物品首次选择已接入 `ai[1]` owner：
`NpcBlueSlimeContainedItemGenerator.SelectForTypeOne` 按来源条件选择 item id 或待选哨兵，
宿主先写回该 NPC 的 `ai[1]` 再完成本 tick profile 求值，使新选中的 Dirt Slime 状态可立即
影响后续计数；随后按来源顺序提交 network sync 和目标重选。这一步不创建世界物品；
选中的 `ai[1]` 后续变体效果仍未全部接线。来源 `NPCLoot` (`NPC.cs:80031-81030`) 中未发现
`ai[1]` 专属的普通物品掉落分支；不要把 item id 选择解释成实际物品实体或掉落。

代码/API：`NpcBlueSlimeProfileInput` → `NpcBlueSlimeProfile.Evaluate` →
`NpcBlueSlimeProfileResult`；副作用由 `INpcBlueSlimeProfileEffectPort` 执行，按来源分支重建
顺序（初始路径为 item → network → target，湿态与跳跃路径按源调用点变化）。
`RuntimeNpcStore.UpdateMovement` 已对 type=1/netID=1/aiStyle=1 选择该 profile，提交 ai[4]、
方向、目标和 network intent，并沿用有限宿主 gravity/TileCollision。体内选择目前只接普通
非 Skyblock type=1 的首次 `ai[1]` 状态提交；有限宿主使用 `NetMode=0` 和本地 `Random`，没有
接入来源 `UnifiedRandom` 或服务端/客户端权威模式。当前还实现 Dirt Slime (`ai[1] == 2`) 的
落地计数顺序：在冻结哨兵检查之前，落地时先给 `ai[0]` 加 9，再执行同 tick 的通用落地计数和
跳跃阶段判断（`NPC.cs:61579-61585,61841-61845,62325-62360`）。其余 `ai[1]` 变体的统计、
碰撞、Buff、Tile、Projectile、NPC 生成和呈现效果、完整 Tile scratch、authority/random 流及
来源 golden 仍是 ledger 中的 open 项；目前未发现 `ai[1]` 专属的普通物品掉落代码。

验证：`Test/Terraria.NpcAi.Verification` 覆盖普通首次选择、地表 item、地下 helper、No Traps、
派对、Remix 首次尝试、已提交状态拒绝重抽、item → network → target 顺序、99 tick 计数到首次
跳跃、夜间加速、湿态/冻结哨兵、Dirt Slime 落地计数和它先于冻结哨兵/同 tick 跳跃的顺序，
以及 inner helper 随机消费。C1 宿主报告导出 ai/localAI 快照；最终 120 tick smoke 观察到
`ai[1]` 从 0 变为 -1 待选哨兵并保持到末 tick。退出条件仍要求接入真实 authority 随机流、
其余选中 `ai[1]` 变体效果、Tile/collision scratch、网络和多 tick reference 对照。

#### C1.1：Dirt Slime `ai[1]` 落地计数切片（2026-10-06）

来源在 `AI_001_Slimes`：当 `ai[1] == 2` 且 `velocity.Y == 0` 时先 `ai[0] += 9`；这一分支
位于 `ai[0] == -999` 冻结哨兵判断之前，后续通用落地计数和跳跃阶段判断仍在同一 tick 运行。
`NpcBlueSlimeProfile.Evaluate` 现按该顺序提交纯状态转移，结果标记
`DirtSlimeGroundCounter`。Verifier 覆盖普通落地从 `-100` 变为 `-90`，以及 `ai[0] == -999`
时 Dirt Slime 分支使其进入 `-500..-1000` 跳跃阈值、同 tick 执行该跳并将状态设为 `-2120`。
该切片不代表其他 `ai[1]` 变体或所有普通 type=1 分支已闭合。

验证结果（SDK 10.0.400）：

| 验证 | 命令/输出 | 结果 |
| --- | --- | --- |
| NPC AI verifier 构建 | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal` | exit 0；0 warning / 0 error；DLL 在 `Build/bin/Terraria.NpcAi.Verification/Debug/net10.0/` |
| NPC AI verifier 运行 | `dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore` | exit 0；PASS，含 C1 Dirt Slime selected-item 同 tick 回填和跳跃顺序 |
| Simulation 直接构建 | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal` | exit 0；17 warnings / 0 errors；warnings 全来自既有 `NSSLC.WorldGeneration` 文件；产物在 `Build/bin/` |
| C1 无玩家 smoke | `dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll 'Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld' 120 --players 0 --spawn-npc 1 --report 'Build/diagnostics/NpcAiRedesign/runs/stage-c1-dirt-slime-counter-20261006-final/c1-smoke-120-no-player.json'` | exit 0；`Succeeded=true`；120 ticks；最终 tick 已更新；`ai[1]=-1` |
| C1 有玩家 smoke | `dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll 'Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld' 120 --players 1 --spawn-npc 1 --report 'Build/diagnostics/NpcAiRedesign/runs/stage-c1-dirt-slime-counter-20261006-final/c1-smoke-120-player.json'` | exit 0；`Succeeded=true`；120 ticks；最终 tick 已更新；`ai[1]=-1`；2 次受击；该随机种子没有选择 Dirt item |
| 完整宿主回归 | `& Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld -OutputDirectory Build/diagnostics/NpcAiRedesign/runs/stage-c1-dirt-slime-counter-20261006-final/full-verifier` | exit 0；summary `Succeeded=true`；36 evidence；Simulation 与 clock build/run 均 0 warning / 0 error；summary 在 `.../full-verifier/summary.json` |
| 来源指纹 | `Get-FileHash -Algorithm SHA256 -LiteralPath 'D:\TRbackup\无任何删减通过编译\Terraria\NPC.cs','D:\TRbackup\无任何删减通过编译\Terraria\Main.cs','D:\TRbackup\无任何删减通过编译\Terraria.ID\NPCID.cs'` | 三个哈希与 reference index 一致；未修改只读参考文件 |

C1 回退：删除 `NpcBlueSlimeProfile*`、`NpcBlueSlimeContainedItem*`、
`NpcBlueSlimeTypeOneSelection*`、两个随机/效果端口、`RuntimeNpcStore` 的 Blue Slime 分支和
对应 verifier 场景；恢复有限 `NpcSlimeAiBehavior`，并在 `SimulationContentBootstrap` 恢复
Blue Slime 原有限尺寸/value。回退只影响后续 tick 的行为选择，不回滚已经提交的 ai[0..3]、
`ai[1]`、目标或 network intent，也不撤销其他已提交效果。保留 C2 仍使用的
`NpcImmediateEffectStateComponent` 和 ledger/验证记录中的来源与未闭合项，不把删除契约误记为
profile 完成。

2026-10-06 有限宿主接线验收：

| 验证 | 命令 | 验收结果 |
| --- | --- | --- |
| 宿主构建 | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal` | exit 0；0 warning / 0 error |
| C1 多 tick smoke | `dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll <world> 120 --players 0 --spawn-npc 1 --report <c1-smoke-120.json>` | `Succeeded=true`；120 ticks；type=1 位置/碰撞历史可观察；最终 tick 更新完成 |
| C1 有玩家 smoke | 同一宿主命令 `--players 1 --spawn-npc 1` | `Succeeded=true`；type=1 连续移动并完成有限目标/Tile 路径 |
| 完整宿主回归 | `Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath <world> -OutputDirectory <c1-full-verifier-20261006>` | exit 0；`summary.json` `Succeeded=true`；33 evidence；Simulation/clock build/run 0 warning / 0 error |

### C1 普通世界体内物品选择与 ai[1] 提交（2026-10-06）

`NpcBlueSlimeContainedItemGenerator.SelectForTypeOne` 将来源外层首次选择分支接到 NPC AI owner：
仅在 `ai[1] == 0`、非 client、`value > 0` 时运行；保留普通 world 的 Slime Rain 尝试次数、
helper/附加 item/special item/派对/Remix/Vampire 分支顺序及随机区间。WorldSession 的 Remix、
No Traps、GetGoodWorld、Vampire、HardMode、MoonPhase、GenuineParty 和 RockLayer 事实经显式输入
传入。Skyblock/LowTiles 仍拒绝进入宿主选择，避免缺少 `WorldGen.Skyblock` 事实时伪造路径。

Owner 读取当前 ai[1] 并将选中 item id 或 `-1` 待选哨兵写回同一 NPC 的 AI state；返回的同步意图
随后在 target reacquire 前提交。选中 id 是 Slime 的体内状态，不是 `RuntimeWorldItemStore` 中已
生成的物品；`ai[1] > 0` 时被击出、拾取/消费及其 item-specific stat/presentation 分支仍未接入。
Reference type=1 的 Blue Slime 宽高 24×18、value 25 已用于模拟定义，修正地表中心分支和 hitbox
输入；Green Slime 的 finite netId=16 映射未改。

| 验证 | 命令/场景 | 结果 | 输出 |
| --- | --- | --- | --- |
| Simulation 宿主构建 | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal` | exit 0；0 warning / 0 error | `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll` |
| NPC AI verifier build/run | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal`；`dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore` | 构建和运行 exit 0；0 warning / 0 error；PASS | `Build/bin/Terraria.NpcAi.Verification/Debug/net10.0/` |
| C1 无玩家 smoke | `dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld 120 --players 0 --spawn-npc 1 --report Build/diagnostics/NpcAiRedesign/runs/stage-c1-contained-item-selection-20261006-r2/c1-contained-item-smoke-120.json` | `Succeeded=true`；120 ticks；type=1 最终 `ai[0]=-1120, ai[1]=-1, ai[2]=1`；最终 tick 更新完成 | `Build/diagnostics/NpcAiRedesign/runs/stage-c1-contained-item-selection-20261006-r2/c1-contained-item-smoke-120.json` |
| C1 有玩家 smoke | 同一宿主命令 `--players 1` 并输出 `c1-contained-item-smoke-120-player.json` | `Succeeded=true`；120 ticks；type=1 受击 2 次、life=19、`ai[1]=-1`；最终 tick 更新完成 | `Build/diagnostics/NpcAiRedesign/runs/stage-c1-contained-item-selection-20261006-r2/c1-contained-item-smoke-120-player.json` |
| 完整宿主回归 | `& Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld -OutputDirectory Build/diagnostics/NpcAiRedesign/runs/stage-c1-contained-item-selection-20261006-r2/full-verifier-20261006` | exit 0；`Succeeded=true`；36 evidence；Simulation 与 clock build/run 均 0 warning / 0 error | `Build/diagnostics/NpcAiRedesign/runs/stage-c1-contained-item-selection-20261006-r2/full-verifier-20261006/summary.json` |

这些结果只闭合普通世界首次体内 item id 选择及 `ai[1]` 提交的有限路径；不证明参考随机输出，
也不覆盖选中后消费/掉出、特殊 stat、NewNPC/NewProjectile、Tile 放置、真实 network/presentation、
完整 authority、Skyblock/LowTiles 或 reference golden。

### C2：Demon Eye type=2 / netID=2

来源闭包固定为 `NPC.cs:20126-20130` → `AI_002_FloatingEye` (`NPC.cs:53027-53509`)；默认身份由
`SetDefaults` (`NPC.cs:8700-8711`) 和 `NPCID.cs:11114` 复核。type=2 的可达路径是：
碰撞反弹（`collideX/Y`、`oldVelocity`、`noTileCollide`）→日间地表 discouragement 或
`TargetClosest`→通用 style-2 缩放速度加速→type=2 的 `Next(40)` dust 分支→wet 垂直修正及
再次 `TargetClosest`。The Hungry II (116)、Wandering Eye (133) 和其它 style-2 类型不共享此
profile 的身份条件。

代码/API：`NpcFloatingEyeProfileInput` → `NpcFloatingEyeProfile.Evaluate` →
`NpcFloatingEyeProfileResult`；副作用由 `INpcFloatingEyeProfileEffectPort` 执行，随机由
`INpcFloatingEyeRandomPort` 提供。profile 返回 `noGravity`、方向、速度、discouraged/target/dust/
wet-target intents 和分支位；`ApplyEffects` 保留 target → dust → wet-target 顺序，discouraged
路径只提交 `EncourageDespawn(10)`。`RuntimeNpcStore.UpdateMovement` 已对 type=2/netID=2/aiStyle=2
选择该 profile，提交方向、目标、弃生存和 dust intent，执行动态 TileCollision 并记录碰撞状态；
`NpcImmediateEffectStateComponent` 和运行时报告提供可观察的效果意图。dust presentation、Tile
scratch、authority/network 和来源 golden 仍未闭合，有限接线不启用其它 style-2 类型。

验证夹具覆盖身份拒绝、碰撞反弹先于通用加速、日间地表 discouragement、wet 阻尼、`Next(40)`
一次消费及 target → dust → wet-target 顺序。退出条件仍要求 owner 接线后执行真实 TargetClosest
候选、碰撞 scratch、联机 authority 和多 tick reference 对照。

2026-10-06 有限宿主接线验收：

| 验证 | 命令 | 验收结果 |
| --- | --- | --- |
| 宿主构建 | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal` | exit 0；0 warning / 0 error |
| C2 多 tick smoke | `dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll <world> 60 --players 0 --spawn-npc 2 --report <c2-smoke-60.json>` | `Succeeded=true`；60 ticks；type=2 `NoGravity=true`，位置变化，弃生存 intent 可观察，最终 tick 更新完成 |
| 完整宿主回归 | `Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath <world> -OutputDirectory <full-verifier-final2-20261006>` | exit 0；`summary.json` `Succeeded=true`；33 evidence；Simulation/clock build/run 0 warning / 0 error |
| C1/C2 profile verifier | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal`；`dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore` | 构建 0 warning / 0 error；运行 PASS |

这组结果只把 C2 接入有限宿主的调用、状态提交和效果意图记录闭合；不把有限最近玩家选择
当作完整 `TargetClosest`，也不宣称 ZoneGraveyard、真实 dust/network presentation、Tile
scratch、source golden、联机 authority 或其它 style-2 类型已完成。

C2 回退：删除 `NpcFloatingEyeProfile*`、`INpcFloatingEyeProfileEffectPort`、
`INpcFloatingEyeRandomPort`、`NpcImmediateEffectStateComponent` 及对应 verifier/宿主接线，
恢复有限 `NpcFloatingEyeAiBehavior`，并同步移除 `RuntimeNpcEntity` 的即时效果快照入口。
回退前已提交的运行时实体状态或呈现请求不自动重放；回退只针对后续 tick 的行为选择。
ledger 与验证记录保留来源映射和未闭合项。

### C3：Zombie type=3 / netID=3 / aiStyle=3

来源闭包固定为 `NPC.cs:20131-20135` → `AI_003_Fighters` (`NPC.cs:56637-61118`)；本批只启用
`type=3 / netID=3 / aiStyle=3`，不会因共享 `aiStyle=3` 自动启用 Skeleton、GoblinPeon 或其它
Fighter 类型。来源中可证明且已接线的有限状态包括：方向初始化、目标重选意图、日间地表弃生存
意图、停滞两 tick 反向、缩放后的水平速度上限/0.07 加速和落地反向时的 0.8 衰减。

代码/API：`NpcFighterProfileInput` → `NpcFighterProfile.Evaluate` → `NpcFighterProfileResult`；
副作用由 `INpcFighterProfileEffectPort` 执行，宿主按弃生存 → 目标重选 → 同步顺序提交
`NpcImmediateEffectStateComponent`、目标组件和 network intent。`RuntimeNpcStore.UpdateMovement`
在 profile 前消费由真实命中提交的 `NpcHitStateComponent`，并把当前目标中心/高度和 NPC 高度
传入 profile，使来源 `justHit` 计数清零及目标底部对齐后的 `directionY=-1` 可观察；profile 后继续使用已有重力、TileCollision 和碰撞历史；Tile 台阶/开门、完整 jump 条件、
战斗专有动作、声音/呈现、authority/network sink 和其它 type=3 变体仍未闭合。

实现中的有限 host 差异已登记：来源日间分支与有限宿主的持续战斗目标选择在当前切片同时记录，
避免 verifier 的现有 Zombie 战斗路径因目标清除而漂离；这不是完整来源等价声明，后续需要
独立的 source golden 和 helper 条件对照。

| 验证 | 命令/产物 | 结果 |
| --- | --- | --- |
| C3 profile verifier | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal`；`dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore` | 构建 0 warning / 0 error；运行 PASS，覆盖身份拒绝、0.07 加速、两 tick 反向、缩放阻尼、日间弃生存与目标重选意图 |
| C3 120 tick smoke | `dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld 120 --players 1 --spawn-npc 3 --report Build/diagnostics/NpcAiRedesign/runs/stage-c3-fighter-profile-20261006/fighter-smoke-120.json` | `Succeeded=true`；120 ticks；type=3 移动/碰撞可观察；`DespawnEncouragementTicks=10`；最终 NPC 更新完成 |
| C3 跨日 smoke | 同一宿主 `1 --players 1 --world-time-rate 50000 --spawn-npc 3` | `Succeeded=true`；`InitialDayTime=true` → `FinalDayTime=false`；Zombie 保持可追踪移动，弃生存 intent 清零 |
| C3 完整宿主回归 | `& Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld -OutputDirectory Build/diagnostics/NpcAiRedesign/runs/stage-c3-fighter-profile-20261006/full-verifier-20261006-r2` | exit 0；`summary.json` `Succeeded=true`；Simulation/clock build/run 均 0 warning / 0 error；35 项 evidence；脚本 Zombie 45 次受击后掉落并拾取 1 Gel |

2026-10-07 延续切片补齐两个来源输入边界：真实非致死命中在 `RuntimeNpcStore.ApplyProjectileHit`
成功后写入 `NpcHitStateComponent`，下一个 Fighter AI tick 原子消费并传入 `JustHit`；宿主从当前
目标槽位捕获目标中心和固定玩家碰撞高度，并传入 NPC 高度，profile 在来源顶部几何条件成立时
提交 `DirectionY=-1`。纯 verifier 新增“命中先于两 tick 反向计数”和“目标底部与 NPC 底部对齐”断言。
C2 同时把 profile 输入的 `TargetSlot` 从硬编码 `-1` 改为实体当前目标槽位。此次延续只验证了
NPC 领域 DLL 与独立 verifier；Simulation 编译仍被共享工作区阻断使修改后的宿主二进制 smoke
未执行，ZoneGraveyard、完整目标候选、authority/network sink、source golden 仍为 open。

| 验证 | 命令/产物 | 结果 |
| --- | --- | --- |
| NPC 领域聚焦构建 | `dotnet build src/NSSLC/Component/Npc/Terraria.Npc.csproj --no-restore --no-dependencies --nologo -v:minimal` | exit 0；0 warning / 0 error；`Build/bin/Terraria.Npc/Debug/net10.0/Terraria.Npc.dll` |
| 独立 verifier 聚焦构建 | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --no-dependencies --nologo -v:minimal` | exit 0；0 warning / 0 error；`Build/bin/Terraria.NpcAi.Verification/Debug/net10.0/` |
| 独立 verifier 运行 | `dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore` | exit 0；PASS，包含 C2 Demon Eye、C3 Fighter、命中消费和目标底部方向边界 |
| 常规 verifier 构建首次尝试 | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal` | exit 1；当时 `WorldStorageRoot.cs` 报 3 个缺失 `Dispose`（`ProjectileIdentityIndex`、`TileEntityUpdateSchedule`、`WorldPressurePlateRegistryComponent`）及 6 个既有警告；未修改这些文件 |
| 常规 verifier 构建重试 | 同一命令 | exit 0；0 warning / 0 error；依赖输出刷新后 verifier 可构建 |
| Simulation 聚焦构建尝试 | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --no-dependencies --nologo -v:minimal` | exit 1；首次输出 15 个共享错误，依赖输出刷新后的最新输出收敛为 `RuntimeProjectileStore.cs:207,21` 和 `:311,21` 两个 `CS0305`（`TryEditComponents` 需要 6 个类型参数）；未执行修改后二进制宿主 smoke |

C3 回退：删除 `NpcFighterProfile*`、`INpcFighterProfileEffectPort`、`NpcFighterSourceBranch` 及
`RuntimeNpcStore` 的 exact type=3 profile 分支，恢复 `NpcFighterAiBehavior` 的有限行为选择。
不回滚已经提交的报告或历史证据；回退后仍必须把 style-3 行登记为有限旧路径，不能宣称来源
profile 已完成。当前未接入的 Tile/门/完整 authority/network 和其它 Fighter 类型仍保持 open。

### C4：Zombie Fighter 障碍跳跃与开门效果边界（2026-10-06）

C4 沿用 C3 的精确身份，不扩大到其它 `aiStyle=3` 类型。新增
`NpcFighterTraversalInput`，由宿主在 AI 调用边界捕获前方一至三格的 Tile 几何、闭门类型和
有限目标垂直关系；`NpcFighterProfile` 根据来源 type=3 的顺序输出一格/多格障碍跳跃冲量，
以及闭门等待 60 tick 后的 `RequestOpenDoor`。跳跃冲量在重力和 TileCollision 前提交，门请求
通过 `INpcFighterProfileEffectPort` 执行；有限宿主调用 `WorldGen.OpenDoor`，随后用
`LegacyWorldTileMapProjection.CommitLegacyTileMutations` 把变更提交回 Tile owner。即时效果
快照记录跳跃和门坐标，避免把门变更或物理冲量藏在 AI 状态里。

本批没有启用来源中的所有 Fighter 特殊类型、专家/墓地/血月例外、共享 Tile scratch、声音和
呈现，也没有建立 reference tick golden。完整战斗动作、authority/network sink、台阶高度与门
失败边界仍保持 open；实际 WorldFile smoke 只证明普通 Zombie 在该宿主能经过新的捕获/提交
顺序，不代表地图中自动命中门场景已覆盖。

| 验证 | 命令/产物 | 结果 |
| --- | --- | --- |
| C4 profile verifier | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal`；`dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore` | 构建与运行 exit 0；PASS，覆盖一格/多格跳跃、门等待阈值、效果顺序和身份边界 |
| C4 宿主 180 tick smoke | `dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld 180 --players 1 --spawn-npc 3 --report Build/diagnostics/NpcAiRedesign/runs/stage-c4-fighter-traversal-20261006/fighter-traversal-180.json` | `Succeeded=true`；180 ticks；Zombie 位置/速度/碰撞和新增效果快照可观察 |
| C4 完整宿主回归 | `& Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld -OutputDirectory Build/diagnostics/NpcAiRedesign/runs/stage-c4-fighter-traversal-20261006/full-verifier-20261006` | exit 0；`summary.json` `Succeeded=true`；35 项 evidence；Simulation 构建 exit 0（34 个既有依赖 warning、0 error），clock 构建/运行 0 warning / 0 error；战斗/drop/pickup/save 通过 |

C4 回退：删除 `NpcFighterTraversalInput`、新增 profile result/效果端口字段、即时效果快照字段和
`CaptureFighterTraversal`/门提交接线，恢复 C3 的有限运动与碰撞路径。已经提交的 Tile 变化、
网络意图和跳跃状态不由回退重放；保留 C4 verifier/诊断证据，并继续把门失败、source golden
和完整战斗动作登记为 open。

验收采用实际多 tick 场景：起始静止/运动、落地/碰壁/液体、目标死亡/消失、昼夜变化、
命中及变换。观察位置/速度之外的 ai/localAI、方向、战斗门控和效果序列。每个 profile
通过后再登记可用，不因一个 Slime/Fighter 示例通过启用同风格全部类型。

## 6. D：城镇、宠物和救援

核对风格 7 与风格 0 的具体身份，从有家/无家、日夜归家的一条真实路径起步，再接住房、
坐具、交谈、危险/战斗、天气、Shimmer、宠物和 124–127 特殊救援/解锁。

住房合法性与分配交给现有 Housing owner，世界解锁/进度交给世界 owner。回家传送保留
落点选择、碰撞检查、立即位置/速度/脏标记、坐下及失败回退的先后。住房失效与卸载清理
实例关联，不靠任务黑板作为住房权威状态。

首个跨 tick 任务验证进入、推进、完成、失败和中断，明确路径/坐具/移动控制归属。成功时
按源规则继续后续行为；清理只释放该任务拥有的资源。原版没有的自动规划和长期工作/
社交活动留在扩展规则，不算本次原版迁移内容。随后才提取多个 profile 共用的任务协议。

必需场景：日/夜/雨/血月等相关分支；有家/无家/住房变化；交谈与危险；传送成功/失败；
目标或坐具失效；救援/奖励/解锁只执行一次；死亡、变换和卸载后没有残留控制或占用。

### D1：Guide 有限持续任务生命周期（2026-10-06）

首个跨 tick 任务切片已接入 Guide 宿主，用于落实本节的进入、推进、完成、失败和中断契约，
但没有把有限任务切片扩展为完整城镇 AI。新增 `NpcTaskKind`、`NpcTaskPhase`、
`NpcTaskFailureReason`、`NpcTaskStateComponent`、`NpcTaskLifecycleResult` 和
`NpcTaskLifecycleSystem`，生命周期 API 为 `Enter`、`Advance`、`Complete`、`Fail`、
`Interrupt`、`Reset`。实例通过 `RuntimeNpcEntity` 持有任务状态快照；
`RuntimeNpcStore.SyncGuideTask` 在每个真实 tick 的 AI 前按世界事实选择任务：日间进入或继续
`GuideDayPatrol`，夜间有家进入或继续 `GuideReturnHome`，无可用任务时中断运行中的任务并记录
`TargetUnavailable`。选中的任务每 tick 推进一次 cursor，既有移动数值和动作分支保持不变。

本切片证明任务状态不再依赖隐式黑板，且任务替换、失败原因和跨 tick 进度可观察；住房分配、
回家传送、坐具控制、交谈、危险抢占和完整 town profile 仍由后续 D 批次闭合。验证证据见
[NPC AI 重设计验证记录](../../system-decomposition/2026-10-05-npc-ai-redesign-verification.md)
和 [profile 覆盖登记](../../migration/ledgers/2026-10-06-npc-ai-profile-coverage-ledger.md)。

| 验证 | 命令/产物 | 结果 |
| --- | --- | --- |
| NPC AI verifier | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal`；`dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore` | 构建 0 warning / 0 error；运行 exit 0；PASS，含幂等进入、推进、完成、任务替换、失败、中断和重置 |
| Guide 200 tick smoke | `dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll <world> 200 --players 1 --spawn-npc 22 --input-script <neutral-input.json> --report <guide-town-ai-200.json>` | `Succeeded=true`；`Action=-1`；`GuideDayPatrol/Running`；`TaskCursor=200`；位置发生移动；最终 tick 更新完成 |
| 完整宿主回归 | `& Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath <world> -OutputDirectory Build/diagnostics/NpcAiRedesign/runs/stage-d-guide-task-lifecycle-20261006-r4/full-verifier-20261006` | `summary.json` `Succeeded=true`；33 项 evidence；Simulation/clock 构建与运行 0 warning / 0 error；TileEntity 双实体清理探针通过 |

D1 回退：删除 `NpcTask*` 生命周期类型、`RuntimeNpcEntity` 任务快照/提交入口和
`RuntimeNpcStore.SyncGuideTask` 接线，恢复 Guide 仅由既有移动分支驱动；保留 D1 ledger 与验证
记录为已验证的有限契约，不把回退后的 Guide 行为登记为任务生命周期完成。

### D1 夜间宿主补验（2026-10-06）

宿主新增 `--npc-night-probe true`。探针给新建 Guide 提交一个有限住房事实，使用
`--world-time-rate 50000` 让真实 WorldClock 在首 tick 跨过日落，再由 NPC 阶段执行任务同步。
报告必须同时满足 `InitialDayTime=true`、`FinalDayTime=false`、`Task=GuideReturnHome`、
`TaskPhase=Running`、`TaskCursor=1`。这只闭合夜间任务选择与更新顺序，不把住房合法性、回家
传送、坐具或完整城镇 AI 登记为完成。

| 验证 | 命令 | 验收 |
| --- | --- | --- |
| 夜间 Guide 宿主切片 | `dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll <world> 1 --players 0 --world-time-rate 50000 --npc-night-probe true --report Build/diagnostics/NpcAiRedesign/runs/stage-d-guide-night-20261006/guide-night-1.json` | exit 0；报告满足上述五项断言 |

### D2：Guide 回家传送有限切片（2026-10-06）

本批在 D1 的 `GuideReturnHome` 任务上接入一个有限的回家效果协议。领域层新增
`INpcHomeReturnCollisionQuery`、`NpcHomeReturnDestination` 和
`NpcHomeReturnDestinationQuery`；查询按来源切片保留 home floor 上方三格空间检查和
`0、-1、+1` 候选偏移顺序，只返回已确认可用的落点。宿主在任务 cursor 达到 60 时，
先读取住房事实，再执行查询；成功路径提交位置、清零速度、记录候选偏移并请求 network
update，随后完成任务；全部候选阻塞或住房事实无效时记录 `NoPath`、标记 homeless 并保留
失败任务状态。即时效果快照只记录本 tick 的请求/成功/失败与目标，不把传送呈现或网络发送
伪装成已完成能力。

宿主新增 `--npc-home-return-probe <success|blocked|invalid>`。success 使用生成世界中
可验证的住房落点，blocked 和 invalid 分别覆盖全阻塞与住房失效；三种场景都运行 60 tick，
以真实 NPC 更新顺序观察任务和即时效果。

| 验证 | 命令/产物 | 结果 |
| --- | --- | --- |
| D2 领域/规则 verifier | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal`；`dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore` | 两命令 exit 0；PASS，含候选顺序、全阻塞和 D2 回家查询 |
| D2 success 宿主切片 | `dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll <world> 60 --players 0 --world-time-rate 0 --npc-home-return-probe success --report Build/diagnostics/NpcAiRedesign/runs/stage-d2-guide-home-return-20261006/home-return-success.json` | exit 0；`GuideReturnHome/Completed`；传送成功、速度清零、位置改变、network update 请求可观察 |
| D2 blocked 宿主切片 | 同上，将 probe 改为 `blocked`，输出 `home-return-blocked.json` | exit 0；`TaskPhase=Failed`、`TaskFailureReason=NoPath`、传送失败且住房标记 homeless |
| D2 invalid 宿主切片（D3 前语义） | 同上，将 probe 改为 `invalid`，输出 `home-return-invalid.json` | 历史 D2 证据：住房事实无效时回家效果记录 `NoPath`、无成功传送；D3 更新顺序后由 D3 专用 invalid probe 验证住房先失效、任务不再进入 `GuideReturnHome` |
| 完整宿主回归（D1 基线） | `& Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld -OutputDirectory Build/diagnostics/NpcAiRedesign/runs/stage-d2-guide-home-return-20261006/full-verifier` | exit 0；`summary.json` `Succeeded=true`；35 项 evidence；Simulation/clock 构建和运行 0 warning / 0 error；D2 三个专用 probe 另行执行 |

D2 回退：删除 `NpcHomeReturn*` 查询、即时效果中的回家字段、`RuntimeNpcEntity` 传送提交
入口、`RuntimeNpcStore` cursor=60 分支和三个宿主 probe，恢复 D1 的夜间任务仅推进/失败状态。
保留 D2 验证记录作为已验证的有限协议证据；不把回退后的 Guide 行为登记为传送完成。住房
分配、完整路径搜索、坐具、交谈、危险抢占、真实网络发送和完整 town profile 仍 open。

### D3：Guide 住房事实重新验证与 owner 同步（2026-10-06）

D3 将住房重新验证放到 `RuntimeNpcStore.SyncGuideTask` 之前。对 town NPC，宿主每个真实 tick
使用 `WorldGen.IsHousingRoomValidAt` 重新检查已记录的 home tile；有效时保持 ECS housing
事实，并把 `TownHousingRegistrySystem` 同步到相同房间；无效时清除 ECS home、提交 homeless、
将 registry 标记为 homeless、请求 network update，并记录 `HousingRevalidationFailed` 与
`HousingRegistrySynchronized` 即时效果。持久化快照对 homeless NPC 输出 `(-1,-1)`，避免旧房屋
坐标被重新载入为有效事实。任务选择因此读取的是本 tick 已重验证的事实：无效住房在日间仍可进入
`GuideDayPatrol`，但夜间不会再进入 `GuideReturnHome`。

宿主新增 `--npc-housing-revalidation-probe <valid|invalid>`。`valid` 安装临时合法房间并故意让
registry 处于 homeless，验证 owner 被重新分配；`invalid` 先提交房屋事实，再使其失效，验证
ECS/registry 同步、network intent 和任务不进入回家分支。住房 registry 仍按现有 NPC type key
管理；D3 不宣称完整住房分配、救援、坐具、对话或 Terraria reference parity。

| 验证 | 命令/产物 | 结果 |
| --- | --- | --- |
| D3 领域/规则 verifier | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal`；`dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore` | 两命令 exit 0；PASS，含住房失效与 owner 同步效果断言 |
| D3 valid 住房宿主切片 | `dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll <world> 1 --players 0 --world-time-rate 0 --npc-housing-revalidation-probe valid --report Build/diagnostics/NpcAiRedesign/runs/stage-d3-housing-revalidation-20261006/housing-valid.json` | exit 0；`HousingHasHome=true`、`RegistryHasRoom=true`、`RegistryIsHomeless=false`、`HousingRevalidationFailed=false`、`HousingRegistrySynchronized=true` |
| D3 invalid 住房宿主切片 | 同上，将 probe 改为 `invalid`，输出 `housing-invalid.json` | exit 0；`HousingHasHome=false`、`HousingIsHomeless=true`、`RegistryHasRoom=false`、`RegistryIsHomeless=true`、`HousingRevalidationFailed=true`、`HousingRegistrySynchronized=true`、`NetworkUpdateRequested=true`；`Task=None`、`TaskPhase=Idle` |
| D2 success/blocked 回归 | D3 目录复核报告 `Build/diagnostics/NpcAiRedesign/runs/stage-d3-housing-revalidation-20261006/home-return-success.json` 与 `home-return-blocked.json` | success 仍完成传送；blocked 仍以 `NoPath` 失败并标记 homeless |
| D3 homeless 保存/重载闭环 | invalid probe 加 `--save .../persistence/homeless-after-revalidation.wld`，再以该文件运行 1 tick | 保存报告 `SaveCommitted=true` 且 Guide `HomeX/HomeY=-1/-1`；重载报告仍为 `Homeless=true`、`HomeX/HomeY=-1/-1` |
| 完整宿主回归 | `& Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld -OutputDirectory Build/diagnostics/NpcAiRedesign/runs/stage-d3-housing-revalidation-20261006/full-verifier` | `summary.json` `Succeeded=true`；既有战斗、drop、save、TileEntity 和 housing 相关场景无回归 |

D3 回退：移除 `RevalidateHousing` 接线、住房即时效果字段、临时 probe 与 homeless 持久化投影，
恢复 D2 的住房事实直接进入任务选择。回退不撤销已经提交的 registry、network 或保存输出；回退后
必须把 D3 证据保留为历史记录，并重新标记住房 owner 同步为 open。

## 7. E：召唤、分节和多部位关系

先实现一个生成后立即建立关系的切片，再扩展 Worm、骷髅、肉墙、毁灭者、石巨人、
世纪之花、月总等具体家族。创建、父子/相邻节、共享生命根和召唤者分别建立契约。
现有 [NpcParentRelationComponent](../../../src/NSSLC/Component/Npc/NpcParentRelationComponent.cs)
只是可复用来源，不能据其名字把全部关系压成一个 parent 字段。

每个节点逐槽更新，保留独立目标、易损与退出状态。生成返回实际类型和实例后才 attach；
链创建的每一步保持可观察，容量不足时记录已创建节点，按具体来源/差异策略退出。
共享生命由 Combat owner 解析，AI 不复制父节点 life。Projectile 表示的玩家随从保持
Projectile/Player owner，NPC AI 只协作生成、仇恨、目标和伤害关系。

必需场景：后槽/前槽子节点；满槽和部分链；父/根死亡；身体中段消失；节点变换或槽位
复用；召唤者离开；共享伤害路由；奖励次数；世界卸载。容量 sentinel 的保护性变化单独
登记，不与正常路径一起宣称原版完全等价。

### E1：父子生成与解绑切片（2026-10-06）

首个 E 批次切片使用 Zombie 作为父节点、Blue Slime 作为子节点。`RuntimeNpcStore.TrySpawnParentChild`
先取得两个真实运行时实例，随后在子节点挂载 `NpcParentRelationComponent` 和
`EntityRelationState(EntityRelationKind.Parent)`，绑定结果显式携带父/子实例身份、稳定
`EntityReference`、兼容槽位与 `AttachedAtTick`。父节点释放前由 owner 扫描活动子节点并解绑
两种关系组件，再终止父实体；子节点不被错误地当作父节点生命周期的一部分，可独立释放。
子生成或 attach 失败则立即释放已创建实例，禁止发布半成品关系。

本切片同时接入 Combat owner 的生命根解析：对子节点的非致命命中，父节点生命减少且子节点
镜像父生命；父槽释放并被新实例复用后，旧子节点只保留本地生命；父节点致死时按父节点
生命根释放整组并由父节点承担掉落归属。容量边界填满其余槽位后，父已创建而子拒绝的
路径恢复原活动数，没有泄漏半成品关系。它仍不等价于 Worm/多部位完整来源规则。

| 验证 | 命令 | 验收 |
| --- | --- | --- |
| E1 关系宿主切片 | `dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll <world> 1 --players 0 --npc-relation-probe true --report Build/diagnostics/NpcAiRedesign/runs/stage-e1-npc-parent-relation-20261006/relation-probe-r3.json` | exit 0；attach、父生命根路由、父槽复用隔离、解绑后子本地命中、致死整组释放和掉落归属均为 true；活动计数恢复 |

E1 回退：删除 `TrySpawnParentChild`、父关系挂载/快照/解绑入口和两个宿主 probe，恢复普通
`TrySpawn` 路径；不回滚已经产生的外部世界效果。该切片不宣称虫链相邻节、多部位共享生命、
召唤者关系、部分链容量策略、奖励/变换、保存加载或网络 authority。

### E2：三节点关系链与根生命周期（2026-10-06）

E2 在 E1 的父子契约上增加一个显式三节点链：Zombie 作为根，两个 Blue Slime 依次挂到前一
节点。`RuntimeNpcStore.TrySpawnParentChildChain` 只在每个节点真实生成后建立相邻关系，并返回
按链顺序排列的 `EntityReference`、`NpcInstanceId`、兼容槽位和 `AttachedAtTick`。任一步生成、
引用解析或 attach 失败，都在提交 binding 前逆序释放已创建节点；因此不会发布半成品链。

Combat owner 解析尾节点的递归父链，将伤害和生命根归属到根节点；根节点致死时递归发现全部后代，
逆序释放整条链并由根承担掉落。根槽位复用后，旧尾节点 projectile target 通过实体引用和代际检查
被拒绝。单独释放中段时，owner 清理该节点并解绑尾节点；尾节点之后只影响本地生命，根生命保持
不变。这是当前通用关系 API 的退出策略证据，实际 Worm/多部位来源 profile 仍需逐族确定节点丢失
后的行为。容量探针保留 `MaximumNpcCapacity - 2` 个填充节点，使三节点链只能部分创建，验证回滚
后活动数恢复且无关系泄漏。该切片不宣称 Worm 相邻节、多部位来源规则、节点变换、召唤者关系、
奖励次数、保存加载或网络 authority。

| 验证 | 命令 | 验收 |
| --- | --- | --- |
| E2 三节点关系链 | `dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll <world> 1 --players 0 --npc-relation-chain-probe true --report Build/diagnostics/NpcAiRedesign/runs/stage-e2-npc-relation-chain-20261006/relation-chain-probe-r2.json` | exit 0；相邻引用、尾击归根、根死整链释放、根槽复用旧目标拒绝、中段释放后尾段解绑并改为本地生命、部分链容量回滚和活动数恢复全部为 true |
| E2 完整宿主回归 | `& Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld -OutputDirectory Build/diagnostics/NpcAiRedesign/runs/stage-e2-npc-relation-chain-20261006/full-verifier-20261006-r2` | exit 0；summary `Succeeded=true`；36 项 evidence；Simulation/clock build 和 run 均 0 warning / 0 error；E1/E2 relation probes 通过，包含中段释放断链 |

E2 回退：移除 `TrySpawnParentChildChain` 和 `--npc-relation-chain-probe` 的链场景及断言，将根解析
与死亡清理恢复到 E1 的单父子语义；保留 E1 的 `TrySpawnParentChild` 路径和已记录证据。回退不
撤销已经提交的实体释放或外部世界效果，并继续保留代际引用校验和普通 NPC 槽位生命周期约束。

## 8. F：Boss 阶段与遭遇

从依赖已齐的单体 Boss 开始，逐个建立来源攻击表；再迁移多部位 Boss 和事件单位。
各 Boss 保留自己的阶段/攻击规则，公用工具只覆盖相同能力。事件波次、塔/水晶/门户和
真正共享的遭遇目标由相应 owner 提供，不由某个 NPC 存一份世界状态副本。

每个攻击表至少记录入口/退出条件、比较符、计时器更新位置、目标锁定/重选、运动与
易损门控、攻击创建数量/顺序、随机参数、同步条件、异常/中断及子实体清理。

测试覆盖每个阶段边界前、边界值、边界后，以及生命阈值、目标死亡/离开、昼夜/退出、
难度和相关特殊世界、多玩家目标选择、容量不足及权威端差异。使用实际连续 tick 观察
攻击顺序和子实体生命周期，不用通用“追踪—冲刺—休息”示例替代来源 Boss。

退出条件：该 Boss 全部必需阶段/类型条件及协作者真实接线并通过；其他 Boss 的共用
风格或同一个 helper 不自动获得 verified。已有多 Boss 场景确认实例/遭遇状态互不污染。

### F1：Eye of Cthulhu 开场与首次冲刺切片（2026-10-07）

F1 固定 `type=4 / netID=4 / aiStyle=4`，覆盖开场悬停计数、普通 600 tick 与专家 210 tick
边界、首次冲刺意图、生命阈值（普通 `< 50%`、专家 `< 65%`）以及目标丢失/死亡退出。纯
profile、随机端口、效果端口和 NPC AI verifier 已通过；`RuntimeNpcStore` 的精确身份分支、
无重力路径、内容注册和有序效果 trace 已接线。

Simulation full verification root11 汇总位于
`Build/diagnostics/NpcAiRedesign/runs/current-simulation-verification-root-11/summary.json`：
`Succeeded=true`，Simulation build exit 0、0 warnings / 0 errors；clock verifier build/run 与
TileEntity fixture build 均 exit 0，汇总 37 项 evidence。成功产物为
`Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll`。这里以该 runs
目录下的 `build.log` 和 `summary.json` 为准；根目录的 `current-simulation-build-root-11.log`
是另一份较早失败日志，不代表此 full verification。

基于该 fresh Simulation artifact，root36 两个 Eye smoke 均 exit 0，报告位于
`Build/diagnostics/NpcAiRedesign/runs/stage-f1-eye-current-root-36/`：

- `eye-smoke-1-no-player.json`：`Succeeded=true`，1 tick、0 players、白天；Eye `NetId=4`、
  `NoGravity=true`、`NoTileCollide=true`。trace 为
  `TargetReacquire → Random.Next(5)=0 → ProfileSupported:true → SpawnDust → EncourageDespawn(10)`。
- `eye-smoke-220-one-player.json`：`Succeeded=true`，220 ticks、1 player、白天；Eye
  `NetId=4`、`NoGravity=true`、`NoTileCollide=true`，坐标由 `(33558, 3670)` 移至
  `(33988, 2840.6597)`。trace 为
  `Random.Next(5)=3 → ProfileSupported:true → EncourageDespawn(10)`。

两份报告均为 `SourceFileUnchanged=true`，输入 SHA-256 相同：
`022FB1C6F1C7DEDDE71BF31B1D81D5F5F5E818F7B90655A18EEB88C18CA5B241`。220 tick 结果表明 Eye
能在宿主中连续更新并发生明显位移；该场景全程白天，Eye 走退出效果路径，因此不证明
hover counter 推进或首次冲刺行为。F1 coverage 仍为 mapped/partial/open。

root17 旧 FixtureHost smoke 曾在 `TickNumber=0` 以
`Tile mutation tracking requires the active published session` 失败，清理另报 session 已释放；
root16 较早构建因重复 `ApplyTownNpcCount` 失败，后续已清理；root28/root30 的通过结果为
较早源码快照历史证据。保留这些诊断记录，但不覆盖 root11/root36 的当前证据。命令、报告和
日志见 [F1 Eye of Cthulhu checkpoint](../../migration/ledgers/2026-10-07-npc-ai-f1-eye-of-cthulhu-checkpoint.md)。

完整变身、仆从生成、network authority、save/load、unload cleanup、presentation、完整目标输入
和来源 golden 均未闭合。F1 整体保持 partial/open。F1 回退只删除 Eye profile、精确宿主分支/
内容注册/trace、verifier 用例和该检查点，不回退 C1–C4、D 或 E。
## 9. G：全量闭包、接线和退出

逐项处理 C–F 未覆盖的登记项，包括无默认类型示例、事件单位和 AI 外围 helper。核对
实际调用者、内容注册、初始化、变换、碰撞后状态、网络和 CheckActive 的完整路径。

完成客户端/服务器权限及字段复制接线；生成、奖励、变换和解锁只由对应 authority 提交。
复用现有保存/加载用例，按源存档规则捕获 NPC/世界状态，不默认持久化全部 Boss 阶段。
卸载后清理关系、任务、缓存和待效果；新世界不能解析旧会话引用。

删除旧正式实现时，先确认静态/动态调用、配置和注册已解除，且相应行为及失败边界通过。
只读参考和来源索引保留。当前已删除的有限 UpdateMovementLegacy 不作为完整 AI 删除
证据。当前工作区存在无关修改，禁止用广泛 reset/clean 代替回退。

完成标准：所有 0–127 入口及其可达具体 profile 都有覆盖结论；全部必需实现与实际接线
完成；目标、Boss、城镇、召唤、群体、权限及卸载/保存场景有来源依据并通过。已知差异
逐条说明，未验证或未知条件不能登记 verified；“原版等价”要求相关差异已被证实消除。

### G1：coverage/registration gate 基础设施（2026-10-07）

G1 新增 `NpcAiProfileCoverageRegistry`、显式 `(type, netID, aiStyle)` 注册、负 `netID` 变体保留、
未注册 profile 拒绝和 `RequireFiniteHandler` / `RequireVerified` 检查，并以独立 verifier 读取
参考索引与 coverage ledger 的 0–127 style 行。已通过的报告为
`Build/diagnostics/NpcAiCoverage/coverage-gate-report-root.json`：128 行一致、125 indexed、3
mapped、0 implemented/verified、8 个 mapped profile、3 个 exact finite handler、27 项断言。
该 gate 不把 fallback 当作完成，也不关闭 network authority、source golden、save/load、unload
cleanup 或 canonical ledger 的后续闭包。检查点见
[coverage gate](../../system-decomposition/2026-10-07-npc-ai-coverage-gate.md)。

### G2：固定来源身份前置门禁（2026-10-07）

新增独立项目
[`Terraria.NpcAi.SourceIdentityVerification`](../../../Test/Terraria.NpcAi.SourceIdentityVerification/Terraria.NpcAi.SourceIdentityVerification.csproj)，
不引用 Simulation 或 NPC runtime 项目。verifier 从参考索引和本 ledger 各读取唯一的三份源码
SHA-256 声明，并对 ledger 指定的只读参考树重算哈希。2026-10-07 三个文件的索引/ledger/实际
来源比较均通过（6/6）。报告写入
`Build/diagnostics/NpcAiSourceIdentity/source-identity-report.json`。

G2 只确认行为对照将使用的固定源码身份；它没有执行来源服务器或多 tick differential，不能
称为 source behavior golden，也不提升任何 profile 到 implemented/verified。实际来源输出、
network authority、save/load、unload cleanup 和旧实现 deletion gate 继续保持 open。证据与命令见
[G2 checkpoint](../../system-decomposition/2026-10-07-npc-ai-coverage-gate.md#G2-source-identity-preflight)。

```powershell
dotnet restore Test/Terraria.NpcAi.SourceIdentityVerification/Terraria.NpcAi.SourceIdentityVerification.csproj --nologo -v:minimal
dotnet build Test/Terraria.NpcAi.SourceIdentityVerification/Terraria.NpcAi.SourceIdentityVerification.csproj --no-restore --nologo -v:minimal
dotnet run --project Test/Terraria.NpcAi.SourceIdentityVerification/Terraria.NpcAi.SourceIdentityVerification.csproj --no-build --no-restore
```

验证结果：restore/build/run 均 exit 0；build 为 0 warnings / 0 errors，DLL 位于
`Build/bin/Terraria.NpcAi.SourceIdentityVerification/Debug/net10.0/`；运行报告 3 个文件、6/6
哈希比较通过，并生成上述 JSON 报告。该项目无需完整 Simulation 构建。

合并 F1 的 style 4 partial/mapped 登记后，原 coverage verifier 于 2026-10-07 重跑通过：128 行
一致，124 indexed、4 mapped、0 implemented/verified；8 个 profile mapped、3 个 exact finite
handler、27 项断言。新增的第四个 mapped style 是 Eye of Cthulhu 的 aiStyle 4；G1 最初报告的
125/3 是 F1 登记前快照，不再代表当前 ledger。运行命令为
`dotnet run --project Test/Terraria.NpcAi.CoverageVerification/Terraria.NpcAi.CoverageVerification.csproj --no-build --no-restore`；报告位于
`Build/diagnostics/NpcAiCoverage/coverage-gate-report.json`。

## 10. 构建、运行与证据

### 10.1 既有有限规则回归

以下命令在 `D:/TRbackup/NLTX` 执行，使用 global.json 选择的 .NET SDK（当前配置
10.0.400），目标 net10.0。已有 restore 结果时运行如下增量命令；缺少/失效时只为所需
项目执行必要还原，然后再构建。verifier 引用 Terraria.Npc，不需要先重复构建同一依赖。

```powershell
dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal
dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore
```

预期产物为 `Build/bin/Terraria.NpcAi.Verification/Debug/net10.0/` 和依赖的
`Build/bin/Terraria.Npc/Debug/net10.0/`。运行结果只证明已有有限规则及新增的明确场景，
不证明全量 128 风格。涉及其他 owner 时增量验证相应项目；不构建完整 solution。

### 10.2 既有宿主回归

正式场景来源为 [Simulation verifier](../../../Test/NSSLC.Tools.Simulation.Verification/verify.ps1)。
当前工作区已有 [增量执行副本](../../../Build/diagnostics/NpcAiRedesign/verify-final.ps1)，
固定本仓库根目录并给构建添加 --no-restore，未更改场景断言。确认它仍对应正式场景后
可执行以下命令；此副本是诊断产物，清除后按正式入口核对并重新生成，不把它当长期业务代码。

```powershell
$aiRunRoot = 'Build/diagnostics/NpcAiRedesign/runs/baseline-' + (Get-Date -Format 'yyyyMMdd-HHmmss')
& Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath 'Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld' -OutputDirectory $aiRunRoot
```

输入是当前已有的 WorldFile 319 生成世界；执行前确认文件存在且来源适用于本批。更换世界
须登记来源，不能借用旧 summary。命令会增量构建宿主和时钟 verifier，并写新运行目录的
build.log、clock 日志和 summary.json，不覆盖历史 ai-redesign-final。第二世界场景需要
实际提供 SecondWorldPath；未提供时保持未覆盖。

### 10.3 新增完整行为验收

完整来源 profile 的多 tick/效果/权限场景尚无已实现的统一 CLI。本阶段在真实调用路径
接入后扩展现有验证项目，按需要增加场景参数或专用验证项目，并记录实际可运行命令。
不要先写不存在的命令或只断言处理器自身返回值来替代宿主验收。

每次运行记录命令、项目、SDK、来源/输入、exit code、warning/error、输出路径和覆盖范围。
来源行为对照记录 ai/localAI、目标/几何/方向、位置/速度/flags、生命/伤害/免疫，以及生成、
关系、Tile、奖励、世界进度和同步的有序效果。数值差异先查运算顺序与精度，不直接扩大容差。

既有有限验证的历史结果见接入记录；本轮 B2/C1/C2 有限宿主命令已实际执行并写入新的
诊断目录。当前仍不存在完整 NPC AI 来源对照、全量 profile 构建或联机 authority 通过结果。

## 11. 每批交付、回退与最终交接

每个实施批次交付：

- 实际源码路径/API/注册变化、对应来源分支及 profile 覆盖更新。
- 真实宿主调用路径、状态写入协议和效果可见点；本批差异与未闭合依赖。
- 按第 10 节要求记录的实际构建/行为结果与独立输出目录。
- 回退范围及状态处理：尚未提交效果可停止；已生成/已改世界状态按领域生命周期清理。

优先在初始化/内容加载边界启用已通过的 profile；切换进行中实例时必须有明确状态转换，
否则先结束该实例。代码回退不能撤销已经提交的世界效果，不设计自动重放整个 tick 的重试。
保留无关源码及历史证据，按本批精确文件/API 回退。

最终交接包含设计、执行状态、覆盖登记和验证记录四个入口。设计已形成、执行文档已形成、
完整实现已完成和来源行为验收通过分别陈述；不得用其中一项代替其他三项。

## 2026-10-06 执行计划 C1.2 检查点：Stone / Cloud Slime gravity 分支

来源 `NPC.cs:61592-61603` 在 Blue Slime `AI_001_Slimes` 中按 `ai[1]` 处理 Stone 和 Cloud
Slime 的竖直速度：Stone（`ai[1] == 3`）在 `velocity.Y > 0` 时加上 `gravity * 2`；Cloud
（`ai[1] == 751`）在 `velocity.Y != 0` 时减去 `gravity * 0.6`。这两段位于方向默认值、
`ai[0] == -999` 早退及通用湿态/地面跳跃处理之前。`RuntimeNpcStore` 将本 tick 既有
`NpcGravitySystem.Evaluate` 的 gravity 值作为显式输入，profile 在方向初始化、冻结哨兵、
湿态修正和公共地面计数之前应用对应速度调整。

Verifier 固定 gravity=0.25，覆盖 Stone 下落的 +0.5、Stone 上升不变、Cloud 下落/上升均减
0.15、Cloud 静止跳过分支，以及 Stone / Cloud 在 `ai[0] == -999` 返回前先调整速度。
`NPC.cs` / `Main.cs` / `NPCID.cs` 指纹与 reference index 相同。
这个切片只覆盖速度调整；特殊名称、颜色、剩余 `ai[1]` variant stats/effects、来源随机和
authority、Tile/collision scratch 及 reference multi-tick golden 仍为 open。普通 120 tick
host smoke 没有强制选择 Stone/Cloud，不能当作这两个分支的运行证据。

| 验证 | 命令/场景 | 结果与输出 |
| --- | --- | --- |
| NPC AI verifier 构建 | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal` | exit 0；0 warning / 0 error；产物在 `Build/bin/Terraria.NpcAi.Verification/Debug/net10.0/` |
| NPC AI verifier 运行 | `dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore` | exit 0；PASS，含 Stone / Cloud gravity 条件 |
| Simulation 构建 | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal` | exit 0；0 warning / 0 error；日志 `Build/diagnostics/NpcAiRedesign/runs/stage-c1-stone-cloud-gravity-20261006/simulation-build.log`；产物在 `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/` |
| 0 / 1 玩家 smoke | 同一 WorldFile 319 各运行 120 ticks，`--spawn-npc 1`，players=0 / 1 | 两条 exit 0、`Succeeded=true`、末 tick NPC 更新完成；players=0 最终 `ai[0]=-1120, ai[1]=-1`；players=1 `ai[0]=-1106, ai[1]=-1` 且 2 次受击。普通 seed 未锁定 Stone / Cloud。报告位于该批 diagnostics 目录 |
| 完整宿主回归 | `& Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath 'Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld' -OutputDirectory 'Build/diagnostics/NpcAiRedesign/runs/stage-c1-stone-cloud-gravity-20261006/full-verifier'` | exit 0；`summary.json` `Succeeded=true`、36 项 evidence；Simulation 与 clock build/run 均 0 warning / 0 error |
| 来源指纹 | 对参考 `NPC.cs`、`Main.cs`、`NPCID.cs` 执行 `Get-FileHash -Algorithm SHA256` | 三个哈希均与 reference index 一致；只读来源未修改 |

C1.2 回退仅移除 profile 输入的 gravity、Stone / Cloud 两个分支和对应 verifier；保留 C1.1
Dirt Slime 状态接续及体内 item 选择。已提交速度效果不通过回退自动重放或撤销。


### 主会话当前源码验收快照（2026-10-07）

并行 owner 收口后，主会话对共享工作区的当前源码重新执行了增量构建、完整 Simulation
verification、NPC AI verifier、G1 coverage gate 和 G2 source-identity gate。当前证据目录为
`Build/diagnostics/NpcAiRedesign/current-acceptance-root/`，完整 Simulation 汇总为
`Build/diagnostics/NpcAiRedesign/runs/current-acceptance-root/simulation-verification/summary.json`。

| Check | Result |
| --- | --- |
| `src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj` build | exit 0; 0 warnings / 0 errors |
| NPC AI verifier build and run | both exit 0; 0 warnings / 0 errors; PASS for the current finite cases through C1/C1.3/C2/C3/C4 and D2/D3 |
| Full Simulation verifier | `Succeeded=true`; 37 evidence; Simulation build, clock build/run, and TileEntity fixture build all exit 0 with 0 warnings / 0 errors |
| G1 coverage | 128 style entries; 124 indexed, 4 mapped, 0 implemented, 0 verified; 8 mapped profiles; 3 finite handlers; 27 assertions; 128 open |
| G2 source identity | 3 reference files; 6/6 index/ledger/source SHA-256 comparisons passed |

这些结果只关闭当前宿主/构建回归检查，不把任何 profile 提升为 `implemented` 或 `verified`，
也不关闭 source behavior golden、network authority、presentation、完整 Boss 生命周期、保存/卸载、
删除门禁或剩余 124 个 indexed/open style。Eye 的精确一 tick 与 220 tick 白天 smoke 仍按 F1
checkpoint 的 partial/open 边界解释。

### G1 全量覆盖关闭门禁续批（2026-10-07）

现有 coverage verifier 新增 `--require-complete`，把入口库存校验与全量覆盖关闭分开：
正常库存检查 exit 0；当前 128 个 style 与 9 个 runtime profile 未闭合时，完成门禁写报告后
返回 exit 2。登记补上已存在的 Mother Slime 和 Eye 精确有限 handler，当前计数为 9 mapped
profile、5 finite handler、31 assertions；stage 仍全部 mapped/open，不代表来源等价。

`Build/diagnostics/NpcAiRedesign/coverage-completion-20261007/` 保留 build（exit 0、0 warnings /
0 errors）、inventory（exit 0）、completion（预期 exit 2）及“只将 ledger 全部提升但 runtime
registry 未提升”的反向验收（预期 exit 2，9 个 profile 仍 incomplete）。源行为、实际宿主、权限、
保存/卸载和删除门禁仍需各自证据。可运行命令与回退范围见
[coverage completion checkpoint](../../system-decomposition/2026-10-07-npc-ai-coverage-gate.md#g1-coverage-completion-gate-continuation-2026-10-07)。

### 并行长线续批（2026-10-07）

三个新的 Luna 6 Max 本地会话共享当前 checkout，各自维护 active goal 并读取 PUA skill；
主会话拥有 canonical 登记与集成验收。每批须交付实际实现、真实命令和独立诊断，而非检索摘要。

| 会话 | 长线交付边界 | 文件归属与验收 |
| --- | --- | --- |
| `01a11439-550f-7721-917c-0b2bd1637ad9`，NPC AI F1 夜间悬停与冲刺完整接线 | 夜间普通/expert hover counter、首次/循环 dash、来源浮点顺序；后续变身与 servants | Eye profile 和专属宿主 probe；真实多 tick 更新与来源结果；保持未闭合协作者 open |
| `01a11439-a9c9-72b0-8427-3ac886cb1fca`，NPC AI 来源行为对照长线门禁 | 复用真实 reference harness，mismatch 非零退出；扩到 Blue/Mother Slime | ReferenceVerification 项目与独立 checkpoint；来源 IL/provenance、逐 tick 字段与效果差分；不并行修改 Eye profile |
| `01a11439-fb3e-7b31-958f-78ad9fca2be3`，NPC AI 任务与实体结束生命周期闭包 | 死亡、变换、移除、卸载的任务终止协议、旧引用拒绝与清理顺序 | 领域 task 协议/独立 verifier；不写 RuntimeNpcStore，主会话随后做真实宿主集成与验收 |

公共 RuntimeNpcStore 生命周期区域同时有实体组织 owner 修改，所有批次写前重读、仅窄改自身
代码；禁止以广泛 reset/clean 收口。各会话完成自身声明切片不等同于主 goal 完成；主验收仍需
完整 0–127 coverage、全部具体 profile 行为/效果/条件闭包，以及权限、保存/卸载和删除门禁。

### B / D / G：任务结束的生产宿主续批（2026-10-07）

领域任务结束协议和独立 verifier 已通过，主会话将 `RuntimeEntityHandle + TaskGeneration`
校验接入 `RuntimeNpcEntity`，并在普通释放、真实致死父子清理、Hydrate 替换、Reset 和 Dispose
的 root 移除之前调用任务结束。实际 probe 同时修复了借用期间 `TryRelease` 先读取 identity
而抛错的次序问题。`--npc-task-lifecycle-probe true` 已接入 Simulation Program。

主会话新增 `Terraria.NpcAi.HostLifecycleVerification`，直接链接生产 NPC owner 与稳定宿主源码、
通过正式世界加载入口执行。独立源码构建 exit 0、0 warnings / 0 errors，真实 world load、任务
替换/槽位复用/借用拒绝、真实 lethal parent-child cleanup、Reset / repeat Dispose 均 PASS。
构建明确使用 `BuildProjectReferences=false` 复用现有依赖；完整 Simulation 当前源码仍受并行
rollback/network owner 源码写入影响，不能以这项局部结果登记整体回归通过。

证据与精确边界见
[任务结束宿主检查点](../../migration/ledgers/2026-10-07-npc-ai-task-host-checkpoint.md)。
Transform 用例只验收 task 结束边界；实际 profile/type 变换、target/effect cleanup 仍 open。
原生命周期会话已继续以 `/goal` 实现引用绑定的 Advance/Complete/Fail/Interrupt；主会话接入
和验收后再决定关闭相应任务缺口。全量 coverage、来源行为、authority、持久化和删除门禁
保持原完整目标。

### 任务引用推进的主会话验收续批（2026-10-07）

生命周期会话实现了引用绑定的 Advance/Complete/Fail/Interrupt，并收紧无效 kind/reason
枚举检查。主会话将四个入口接到 `RuntimeNpcEntity`，Guide 的实际 tick、回家完成、失败与
中断调用均传入当前捕获的 task reference；主会话 probe 覆盖同 kind 重启后四个迟到操作
全部拒绝、新引用正常推进及重复完成/失败不再提交。

`Build/diagnostics/NpcAiRedesign/runs/task-reference-host-20261007/acceptance-result.json`
记录 NPC 领域构建 exit 0、0 warnings / 0 errors，独立生产宿主构建 exit 0、0 / 0，以及
完整依赖 Simulation build exit 0、17 warnings / 0 errors。七条真实宿主运行全部 exit 0：
任务 probe、Guide success/blocked/invalid 各 120 tick、success 的 60/61 tick 和 blocked 的
60 tick 边界。success 第 60 tick 为 Completed 且传送成功，第 61/120 tick 不重复传送；
blocked 第 60 tick 为 Failed/NoPath 且失败效果可观察，第 120 tick 保留失败；invalid 住房
保持 None/Idle。输入世界文件均未修改。

上述关闭本批 task reference owner 接线与定向回归；完整 Simulation 大回归尚未在此批重跑，
任何覆盖 profile 都没有升级。最初整体构建失败和独立隔离证据仍保留为历史，最新构建/运行
结果在[宿主检查点续批](../../migration/ledgers/2026-10-07-npc-ai-task-host-checkpoint.md)中解释。

来源门禁会话的新实际报告
`Build/diagnostics/NpcAiReferenceVerification/20261007T033802164Z/differential-report.json`
有 32 场景/182 tick、0 velocity differences、8 state differences、11 effects/observables
differences。八个 ai[2] cooldown 差异与十一个最终 netUpdate 差异分别保留；后者须核对
TargetClosest helper 与独立 profile 的组合边界，不直接据此补造 profile network requests。
来源 CallTracker 只记录每 tick 每个方法的首次 entry，不证明完整调用次数/效果顺序。

新增 Luna 6 Max 会话 `01a11477-fcbb-7070-a228-28ad5b29743d`，名称“NPC AI Slime 来源差异与
行为闭包”，以 `/goal` 和 PUA 自检推进 Blue/Mother production 差异修复及后续行为闭包；
来源门禁会话继续独立掌握 ReferenceVerification。原生命周期会话已接续精确来源 Guide
type=22/netID=22/aiStyle=7 的 D 阶段 profile；当前有限 catalog style=0 的差异仍保留，不能
直接改名视为迁移完成。主会话继续拥有集成、canonical 登记和完整目标验收。

### F1 夜间开场主验收与 B2 目标输入续批（2026-10-07）

主会话已复核 Eye owner 的八条夜间边界/循环报告，并独立补跑 Classic 752/903、Expert
312/413 的第二/第三次冲刺边界。十二条运行全部 exit 0，普通/Expert 冲刺速度为 6/7，
三次冲刺后分别在 1053/513 tick 返回悬停；世界文件保持不变。运行绑定 task-reference
批次已构建程序集，不把共享工作区后续未构建修改算作通过。证据范围与回退见
[F1 夜间主验收](../../migration/ledgers/2026-10-07-npc-ai-f1-night-host-acceptance.md)。

新增 Luna 6 Max 会话 `01a11489-0274-7493-aaee-b507e828b927`，名称“NPC AI B2 完整目标输入与
宿主接线”，以 `/goal` 和完整 PUA 方法论持续实现普通/WOF/Upgraded 目标输入、即时提交和
目标几何。它复用现有 player/status/spatial/projectile/NPC owner，拥有窄 target API、独立
target host adapter/verifier/checkpoint；不覆盖 Eye/Slime/Guide 专属区域、身份/生命周期或
canonical 文档。现有 `SelectFinitePlayerTarget` 的固定 aggro/ghost/gross、历史方向、
Boss/碰撞、宠物和 NPC 候选只是有限输入，这个续批须以真实状态与宿主证据逐项替换。
未实现事实保持缺口，不用固定默认值冒充来源闭包。

### G3 门禁反向主验收发现（2026-10-07）

主会话在不包含已有 Slime 失败项的 Eye control 上复验：24 场景/36 tick、零差异时 exit 0；
只修改 source capture 的一个 rotationBits 后，来源比较检出一项差异却仍 exit 0。
空 request/captures 的 0 场景/0 tick 也返回成功。较早的 Slime mutation 本来已有 19 个
profile 差异，exit 2 因而不能单独证明来源矛盾的拒绝路径。门禁退出语义验收暂不通过，
来源会话已接续实现非空/编号校验、来源可信结果与 profile 结果区分，以及独立反向夹具。
复现与修复验收要求见
[来源门禁主验收](../../migration/ledgers/2026-10-07-npc-ai-source-gate-root-acceptance.md)。
所有真实 capture、历史失败和 coverage 阶段保持原记录，不因门禁本身的修复提升 profile。

### B2 来源评分主验收反例（2026-10-07）

主会话直接调用只读原 EXE 和既有隔离源码 build 的 TargetClosest/WOF，在两玩家反例中均
选玩家 0，而当前中心点距离式会偏向玩家 1。四个相关 helper IL 两侧一致，实际程序集
运行前后不变。B2 会话已接续修复源码字面距离式、整数半宽与几何/提交边界，并核对无候选
fallback 和 Upgraded NPC `slot+300` 编码；源码 probe 通过不代表生产修复已经通过。
来源运行证据、兼容 harness 编译和验收条件见
[B2 目标主验收](../../migration/ledgers/2026-10-07-npc-ai-b2-target-root-review.md)。

### B3 更新时序与物理长线续批（2026-10-07）

新增 Luna 6 Max 会话 `01a11498-d325-7d43-ac05-d1608b998569`，名称“NPC AI B3 更新时序与物理
闭包”，以 active goal 和完整 PUA 方法论实现实际 UpdateNPC 的共同准备/物理/活动检查
顺序，并验收实时槽位生成可见性。它拥有窄 tick/physics 领域 API、独立宿主 adapter、
verifier 和 `2026-10-07-npc-ai-b3-tick-physics-checkpoint.md`；保留 Eye/Slime/Guide 专属块、
B2 target API、身份/task 生命周期与主会话 canonical 归属。当前有限 oldVelocity/碰撞状态
记录不代表完整来源顺序；本续批须逐调用点证明 AI 前后物理门控与碰撞后 ai 提交。

### C1 / C1.3 cooldown 与条件效果主验收（2026-10-07）

Slime 会话已补齐两个 profile 的 `3→2→1→1` cooldown 与冻结哨兵边界，并启用 Blue
TargetClosest 端口对结果条件 network intent 的提交。主会话独立重跑 Blue/Mother 各
1/120 tick 与 NPC AI verifier，全部 exit 0、输入世界不变。绑定正常 Simulation build
exit 0、17 warnings / 0 errors；首 tick network intent 为 true，120 tick 清除。Blue
首 tick 的 item 初始化与 target 效果仍按组合证据解释。完整 source differential 和
37-evidence 回归尚未在此批重跑，coverage stage 不升级。证据与回退见
[Slime cooldown 主验收](../../migration/ledgers/2026-10-07-npc-ai-slime-cooldown-host-acceptance.md)。

### G3/A4 来源行为差分续验（2026-10-07）

主会话用 PowerShell 7 重跑 `Test/Terraria.NpcAi.ReferenceVerification/Invoke-ReferenceDifferential.ps1`，
报告目录为 `Build/diagnostics/NpcAiReferenceVerification/20261007T072353828Z/`。隔离参考源码
构建 exit 0、85 warnings / 0 errors；verifier 构建 exit 0、0 warnings / 0 errors。只读参考树
1609 个输入文件 manifest 指纹保持为 `ccf14390f8d1f40a90318727975e0092d544ef8be6a170d0e45b03e774d80d16`，
原 `TerrariaServer.exe` 运行前后 SHA-256 均为
`cf30ebda6839e9ff9556f3ba6cd84b18385cd3283e5462745baa57d1548b669a`。原 EXE 与隔离源码构建的
`NPC.AI()`、`AI_001_Slimes()` IL 一致，32 个场景共 182 ticks 的来源输出也逐字段相同。

本轮修正参考 harness 在直接调用 `NPC.AI()` 前未捕获真实更新循环 `oldTarget`、`oldDirection`、
`oldDirectionY` 的问题。修正后生产 Eye、Blue Slime、Mother Slime 已建模状态/速度字段均为零差异；
仍有 3 个可观察差异：Blue Slime 湿态首次 TargetClosest、Mother Slime 初始化首次 TargetClosest、
Mother Slime 湿态首次 TargetClosest 都因 NPC 上方目标令原版 `directionY` 从 1 改为 -1，并由
`SetTargetTrackingValues` 设置 `netUpdate=true`。Slime profile 当前不建模 `directionY`，故这 3 项仍是
B2 target owner 与 Slime profile 的未闭合组合行为；差分 gate 按 profile differences 返回 exit 2，
没有任何 profile 因此升级为 verified。11 项旧结果由过期历史字段造成，保留在旧报告；修正后的
权威当前差异与退出边界见
[G3/A4 差分检查点](../../system-decomposition/2026-10-07-npc-ai-g3-reference-differential.md)。

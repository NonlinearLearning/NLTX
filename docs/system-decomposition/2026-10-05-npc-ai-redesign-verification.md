# NPC AI 有限接入与验证记录

文档 ID：DOC-2026-10-05-NPC-AI-REDESIGN-VERIFICATION  
逻辑域：system-decomposition  
产物类型：evidence  
状态：active  
范围：当前七类型有限模拟规则的提取与宿主接线；完整参考 AI 尚未迁移  
证据入口：当前源码、专用 verifier 输出、宿主 summary.json  
canonical 路径：docs/system-decomposition/2026-10-05-npc-ai-redesign-verification.md

## 实际接入

完整架构见 [NPC AI 设计](2026-10-05-npc-ai-system-redesign.md)，全部来源入口见
[128 风格索引](2026-10-05-npc-ai-reference-index.md)。本记录只证明以下实现切片：

- 在 `src/NSSLC/Component/Npc/` 新增 NpcAiSystem、INpcAiBehavior、输入/环境/目标值快照、
  NpcAiDecision、Guide/Slime/Fighter/FloatingEye/静止行为及有限数值工具。
- `RuntimeNpcStore.UpdateMovement` 从已物化玩家和住房/时间事实构造输入；
  `NpcAiSystem.EvaluateAndCommitFinite` 提交动作和四个权威 AI 槽位，宿主再提交速度、重力和碰撞。
  2026-10-06 起，宿主先计算 AI 前重力参数并应用前置速度限制，AI 后才施加重力。
  Demon Eye type=2/netID=2/aiStyle=2 现在在同一宿主路径中使用 `NpcFloatingEyeProfile`，并将
  方向、目标、弃生存和 dust effect intent 写入 NPC 组件；运行时报告通过快照导出这些意图。
  Blue Slime type=1/netID=1/aiStyle=1 也已在同一宿主路径使用 `NpcBlueSlimeProfile`，提交
  ai[4]、方向、有限目标和 network intent，并沿用有限 gravity/TileCollision；体内 item owner
  仍显式关闭。
  Zombie type=3/netID=3/aiStyle=3 现在使用 `NpcFighterProfile`，提交来源切片的方向、目标重选、
  日间弃生存、停滞反向、缩放速度和 network intent；Skeleton/GoblinPeon 等共享 aiStyle 类型
  仍拒绝进入该 profile。有限宿主保留现有 gravity/TileCollision，Tile 台阶/开门和完整 Fighter
  helper 闭包仍 open。
  TargetSlot 是本次求值信息，没有建立长期
  裸槽目标关系。Dummy 仍在宿主 AI 前跳过移动。
- 默认注册只接受明确的 netId：1、2、3、16、22、37、488。其他类型即使共享风格也拒绝，
  不能通过通用 fallback 静默声明支持。
- Slime 的最近目标选择使用完整 NPC 中心，与提取前有限宿主的选择位置一致；其返回目标
  也用同一个中心。Eye 的速度逼近保持原有限规则的 delta/加法顺序。
- 删除本次提取后未调用的 UpdateMovementLegacy 及其私有行为/数学 helper，物理只保留
  一条执行路径；不保留重复的“兼容”实现。
- 新增 `Test/Terraria.NpcAi.Verification/`，检查目标中心/平局/死亡或不活动、输入不变与
  重复求值、hover/dive 边界、夜间回家、静止实体、相同风格的未知内容拒绝。

有限规则仍是简化跳跃、追踪、hover/dive、确定性 Guide 巡逻。当前 catalog 的名称/ID 和
aiStyle 与指定参考存在明确差异（见设计中的内容映射说明）；本次测试不证明这些内容已达到
参考行为等价。完整效果接口、目标策略、多部位关系、城镇与 Boss 阶段未被本次代码实现。

## 最终构建与运行

以下命令均在 `D:/TRbackup/NLTX` 执行，宿主回归发生在生产代码修改完成后。
专用 verifier 最后修正了一行日志的列宽，随后再次增量构建并运行通过。

```text
dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --nologo -v:minimal
dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal
dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore
& Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld -OutputDirectory Build/diagnostics/NonCommunicationSimulationAudit/ai-redesign-final
```

| 检查 | exit code | warning/error | 产物/证据 |
| --- | ---: | --- | --- |
| NpcAi.Verification 项目构建（含 Terraria.Npc 依赖） | 0 | 0 / 0 | Build/bin/Terraria.NpcAi.Verification/Debug/net10.0/Terraria.NpcAi.Verification.dll；Build/bin/Terraria.Npc/Debug/net10.0/Terraria.Npc.dll |
| 专用规则 verifier | 0 | PASS | center、ties、target gates、repeatability、phase boundary、home、coverage |
| NSSLC.Tools.Simulation 项目构建 | 0 | 0 / 0 | Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll；最终运行目录 build.log |
| NSSLC.Application.Simulation.Verification 项目构建 | 0 | 0 / 0 | Build/bin/NSSLC.Application.Simulation.Verification/Debug/net10.0/NSSLC.Application.Simulation.Verification.dll |
| 时钟 verifier | 0 | PASS | 最终运行目录 clock-verification.log |
| 既有宿主回归脚本 | 0 | PASS，33 条 evidence | 最终运行目录 summary.json，Succeeded=true |

专用 verifier 是新项目，因此首次构建进行了所需还原。宿主验证使用现有
`Test/NSSLC.Tools.Simulation.Verification/verify.ps1` 的生成执行副本：只固定 repoRoot、
给两次增量项目构建添加 `--no-restore`，其余断言和场景保持相同。原验证脚本未修改。
生成副本在 Build/diagnostics，不是生产源码。没有构建完整 solution 或参考历史项目。

## 宿主观察与限制

最终证据目录为 `Build/diagnostics/NonCommunicationSimulationAudit/ai-redesign-final/`。
summary 包括 600 tick 的 0/1/2 玩家，3600 tick 长跑，200 槽容量拒绝/代际复用，七类型自然
销毁策略，Demon Eye 周期、Guide 巡逻，战斗/drop/pickup、NPC 接触伤害/死亡/复活，以及
原有物品、TileEntity、压力板、保存/重载/取消/保存失败场景。

其中 Eye 场景移动约 72.54 px，Guide 约 110.35 px；3600 tick 战斗场景记录 1 次 NPC 死亡
和 1 次拾取，接触伤害场景记录 4 次玩家死亡。这些是有限规则的观察结果，不是原版 goldens。
本次没有传入 SecondWorldPath，因此不宣称覆盖该脚本可选的第二世界切换场景。

输入使用已有生成的 WorldFile 319 世界；不以本次运行证明其他格式的 owner 加载绑定可用。
没有执行完整参考的逐 tick 差分、Boss/城镇/召唤/群体或客户端/服务器联机等价测试。

### B：AI 状态 owner 收敛与复验

本次把 `NpcAiSystem`、`NpcAuthoritySystem` 与 Training Dummy 创建初始化的 ai[4] 提交统一到 `NpcBehaviorStateComponent.CommitAiStateSlots`；运行宿主仍将该组件作为动作/ai[4] 权威状态，`NpcLocalBehaviorStateComponent` 是同一实例的 localAI[4] 状态，`NpcAiStateComponent` 仅作为输入/结果快照。有限 verifier 用 localAI 11–14 检查读取后保留与逐槽提交。当前行为处理器没有写 localAI 的实例；初始化、变换、复制和来源要求的持久化仍未接线。仓库生产源码不再引用旧 `NpcBehaviorComponent`，但其类型保留到 G 阶段删除门禁。

首次运行 `Terraria.NpcDamageCombatVerification` 时，生产代码正确拒绝了缺少 source instance identity 的 396/397 分裂生成输入。检查同文件与查询要求后，为 396/397、GoodWorld type 13 和 type 36 的验证夹具补齐身份，随后 verifier 通过；这不改动生产规则。

| 验证 | 命令 | 结果 | 输出 |
| --- | --- | --- | --- |
| AI 状态与 combat authority 构建 | `dotnet build Test/Terraria.NpcDamageCombatVerification/Terraria.NpcDamageCombatVerification.csproj --no-restore --nologo -v:minimal` | exit 0；0 warning / 0 error | `Build/bin/Terraria.NpcDamageCombatVerification/Debug/net10.0/` |
| Combat/death verifier | `dotnet run --project Test/Terraria.NpcDamageCombatVerification/Terraria.NpcDamageCombatVerification.csproj --no-build --no-restore` | exit 0；PASS | `P12 combat, life, tracker and death core smoke` |
| AI owner verifier 构建与运行 | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal`；`dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore` | 两命令 exit 0；构建 0 warning / 0 error；PASS | `Build/bin/Terraria.NpcAi.Verification/Debug/net10.0/`；center、ties、target gates、repeatability、phase boundary、home、coverage 与 localAI 槽位 |
| 状态收敛后的宿主回归 | `Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath 'Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld' -OutputDirectory 'Build/diagnostics/NpcAiRedesign/runs/stage-b-single-ai-owner-commit-20261006'` | exit 0；summary `Succeeded=true`，33 项 evidence；simulation 增量 build 0 warnings / 0 errors；时钟 build/run 0 warnings / 0 errors | `Build/diagnostics/NpcAiRedesign/runs/stage-b-single-ai-owner-commit-20261006/summary.json`；产物在 `Build/bin/` |

最终回归脚本使用已生成的宿主产物，增量 build 为 0 warning / 0 error。此前针对本次源码的直接构建 `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal` exit 0，17 warnings / 0 errors；17 条均来自既有 WorldGeneration 源文件，NPC AI 和 `RuntimeNpcStore` 为 0 warning。运行未提供第二世界。该回归验证此次状态 API 变更与有限宿主兼容，不覆盖来源多 tick 差分、AI 即时生成、动态物理门控、碰撞历史、localAI 变换/恢复或网络权威性；B 阶段仍为 in progress。

#### NPC 实例身份与槽位复用补验

修正 `RuntimeNpcEntity.Hydrate` 先前以 `worldId + slot` 派生 `NpcInstanceId` 的实现。现在所有
创建与世界恢复路径共用进程内单调身份分配器；槽位复用、`RuntimeNpcStore.Reset` 和候选世界
重新 hydration 都会生成新身份。运行时 NPC 引用不跨进程保存，身份序号耗尽时明确失败。

正式 Simulation verifier 和本地执行副本的 NPC 槽位探针增加两类断言：同一 store 释放并复用
槽位时 ID 改变、generation 前进且旧对象拒绝释放；store reset 后重新占用同槽时 ID 改变且
旧对象仍被拒绝。后一场景当前 generation 数值会重新从 1 开始，因此旧对象拒绝还依赖 store
解析后的对象引用相等性校验；该引用不会被新实体替代。有限宿主结果不证明完整关系 owner 的
所有查询都已迁移到 `NpcInstanceId`。

| 验证 | 命令/场景 | 结果 | 输出 |
| --- | --- | --- | --- |
| 宿主增量构建 | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal` | exit 0；23 warnings / 0 errors；warning 来自既有 WorldStorage/WorldGeneration 文件 | `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll` |
| NPC AI verifier | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal`；`dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore` | 两命令 exit 0；0 warning / 0 error；PASS | `Build/bin/Terraria.NpcAi.Verification/Debug/net10.0/` |
| Combat/death verifier | `dotnet build Test/Terraria.NpcDamageCombatVerification/Terraria.NpcDamageCombatVerification.csproj --no-restore --nologo -v:minimal`；`dotnet run --project Test/Terraria.NpcDamageCombatVerification/Terraria.NpcDamageCombatVerification.csproj --no-build --no-restore` | 两命令 exit 0；构建 0 warning / 0 error；PASS | `Build/bin/Terraria.NpcDamageCombatVerification/Debug/net10.0/` |
| 33 项宿主回归 | `& Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath 'Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld' -OutputDirectory 'Build/diagnostics/NpcAiRedesign/runs/stage-b-instance-identity-20261006'` | exit 0；`Succeeded=true`；NPC 复用 ID 3→201、reset ID 202→203；两种旧句柄均拒绝；回归 build 与时钟 build/run 为 0 warning / 0 error | `Build/diagnostics/NpcAiRedesign/runs/stage-b-instance-identity-20261006/summary.json`；宿主产物位于 `Build/bin/` |

本批没有执行来源 NPC 输出差分。B 仍未完成：目标策略、效果返回、完整实时逐槽调度、AI 后物理门控及碰撞历史尚未迁移；同样未覆盖 C–G 的 profile 与删除门禁。
本次宿主命令没有传入 `SecondWorldPath`；reset 探针证明同一 store 重置后的运行时身份隔离，
不等价于完整 world-switch / candidate-hydration 场景通过。

#### NPC 实例身份跨世界切换补验

随后用两个现有 WorldFile 319 世界运行正式 34 项宿主回归，并启用脚本的 repeated world
switch 场景。每次切换创建新的 NPC `EntityRuntime`；进程内单调 `NpcInstanceId` 与新的
ECS `EntityUuid` 均不得与前一 store 重合，`RuntimeEntityHandle` 还受新的 `RuntimeId`
作用域保护。报告实际记录两次切换的 NPC ID：`[1, 2] -> [3, 4] -> [5, 6]`；每次集合
互不相交，旧世界的实体句柄/引用在新 store 解析失败。`NpcInstanceId` 仍是有限宿主身份
投影，完整父子/目标/召唤关系消费路径尚未迁移。

| 验证 | 命令/场景 | 结果 | 输出 |
| --- | --- | --- | --- |
| 宿主构建 | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal` | exit 0；0 warning / 0 error | `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll`；`Build/diagnostics/NpcAiRedesign/runs/stage-b-world-switch-identity-20261006/build.log` |
| 34 项宿主回归与两次世界切换 | `& Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath 'Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld' -SecondWorldPath 'Build/diagnostics/WorldGenerationRoundTrip/67890-medium-final/generated.wld' -OutputDirectory 'Build/diagnostics/NpcAiRedesign/runs/stage-b-world-switch-identity-20261006'` | exit 0；`Succeeded=true`；34 项 evidence；宿主/时钟构建和运行均 0 warning / 0 error；两次切换均 `NpcInstanceIdentitiesDidNotOverlap=true`、`PreviousNpcHandleRejected=true` | `Build/diagnostics/NpcAiRedesign/runs/stage-b-world-switch-identity-20261006/summary.json`；切换明细 `repeated-switch-initial.json.switch.json` |

世界切换测试只证明当前有限宿主创建、解析和拒绝边界；没有逐条验证完整来源 AI 保存策略、
完整行为关系清理或客户端网络身份复制。

### B2：目标策略候选与提交契约（2026-10-06）

新增 `src/NSSLC/Component/Npc/NpcTargetSelectionSystem.cs` 及其输入、候选快照、结果和
提交状态类型。普通、WOF、Upgraded 各自保留来源评分和控制流：普通/WOF 使用实体中心的
曼哈顿距离与严格小于，WOF 只接受 `gross` 玩家；三者都保留 aggro、noAggro 和坦克宠物
候选；Upgraded 额外接受活动的 548 NPC，使用欧氏距离，并在无候选时返回
`ShouldCommit=false` 以保留原目标。坦克宠物的 `CanHit` 是空间调用边界提供的快照事实，
不在选择查询中重复读取 Collision 或共享 scratch。

`SelectAndCommit` 只提交显式允许提交的结果，写入目标种类、owner/legacy 索引、宠物索引、
目标几何、方向和网络更新意图。Upgraded 的 NPC/宠物/玩家分支保留来源没有即时
`netUpdate` 的返回路径；普通/WOF 按方向或目标变化产生更新意图。2026-10-06 起，
`RuntimeNpcEntity` 为每个实例挂载 `NpcTargetSelectionStateComponent`，有限宿主的 Blue
Slime/Demon Eye effect port 通过 `SelectFinitePlayerTarget` 调用 `NpcTargetSelectionSystem.Select`
并提交目标状态。该适配器显式提供 Aggro=0、NoAggro=false、Gross=true、无 tank pet 和无
548 NPC 候选；这证明公式已进入真实 tick，不冒充完整来源输入闭包。

| 验证 | 命令 | 结果 | 产物 |
| --- | --- | --- | --- |
| 目标契约构建 | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal` | exit 0；0 warning / 0 error | `Build/bin/Terraria.NpcAi.Verification/Debug/net10.0/`；`Build/bin/Terraria.Npc/Debug/net10.0/` |
| 有限 verifier（含普通/WOF/Upgraded、宠物、548 和 no-candidate 提交门控） | `dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore` | exit 0；PASS | 控制台 `center、ties、target gates、repeatability、phase boundary、home、coverage` |
| Simulation 宿主增量构建 | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal` | exit 0；0 warning / 0 error | `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll` |
| 有限宿主 C1/C2 smoke | C1 120 tick（0/1 玩家）与 C2 60 tick，输出目录 `Build/diagnostics/NpcAiRedesign/runs/stage-b2-target-selection-runtime-20261006/` | 均 `Succeeded=true`；最终 tick 更新完成；报告直接观察移动/碰撞/效果，目标提交由宿主接线和 verifier 路径覆盖 |
| 完整宿主回归 | `Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath <world> -OutputDirectory Build/diagnostics/NpcAiRedesign/runs/stage-b2-target-selection-runtime-20261006/full-verifier-20261006` | exit 0；`summary.json` `Succeeded=true`；33 evidence；Simulation/clock 构建和运行 0 warning / 0 error |

这组证据证明候选/提交契约的公式、平局和门控，以及有限玩家目标适配器进入 C1/C2 真实
tick；没有证明完整来源输出差分、aggro/noAggro/gross、tank pet、548 NPC、网络 authority、
实时 NPC 槽位更新或 C–G profile 覆盖。宿主回归仍按有限规则记录执行。

### B3：逐实例物理事实与 AI 后门控（2026-10-06）

新增 `NpcMovementTickStateComponent`，并在 `RuntimeNpcStore.UpdateMovement` 接入：tick
开始记录旧位置、旧速度和 Wet/Shimmer/Honey；AI 返回后提交 `NoGravity`、`NoTileCollide`；
直接积分或 TileCollision 结束后记录 `CollideX/CollideY`。`NpcAiDecision` 的新增门控字段
默认均为 false，因此已有七类型有限行为的数值路径不被改变；后续 profile 可以在自己的
行为处理器中显式提交门控。该状态由每个 `RuntimeNpcEntity` 持有，不使用全局快照覆盖槽位
复用或碰撞后的 AI 写入。

| 验证 | 命令 | 结果 | 产物 |
| --- | --- | --- | --- |
| B3 状态与宿主构建 | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal` | exit 0；17 warning / 0 error；警告来自既有 WorldGeneration 文件 | `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll` |
| NPC AI verifier（含 tick 状态提交） | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal`；`dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore` | 构建 exit 0，0 warning / 0 error；运行 exit 0，PASS | `Build/bin/Terraria.NpcAi.Verification/Debug/net10.0/` |
| tick 状态报告投影 | `dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll 'Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld' 60 --players 0 --spawn-npc 1 --report 'Build/diagnostics/NpcAiRedesign/runs/stage-b3-movement-state-20261006-r2/observed-movement-state-60.json'` | exit 0；最终报告可观察 OldPosition/OldVelocity、Wet、CollideX/Y 和两个物理门控 | `Build/diagnostics/NpcAiRedesign/runs/stage-b3-movement-state-20261006-r2/observed-movement-state-60.json` |
| 34 项宿主回归与双世界切换 | `& Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath 'Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld' -SecondWorldPath 'Build/diagnostics/WorldGenerationRoundTrip/67890-medium-final/generated.wld' -OutputDirectory 'Build/diagnostics/NpcAiRedesign/runs/stage-b3-movement-state-20261006-r2'` | 首次复现发现致死投射物后的释放后位置读取，已改为提交伤害前捕获掉落位置；修复后 exit 0，`Succeeded=true`，34 项 evidence，双切换身份隔离通过 | `Build/diagnostics/NpcAiRedesign/runs/stage-b3-movement-state-20261006-r2/summary.json`；切换明细 `repeated-switch-initial.json.switch.json` |

这组证据证明有限宿主的状态记录和提交顺序，不证明来源完整更新顺序、动态 profile 门控、
碰撞历史、网络权限或同 tick 生成/关系闭包。

## 静态检查

来源索引断言 128 个连续且不重复的风格入口，记录三个参考文件 SHA-256。
任务新增文件的链接、元数据、manifest 唯一行和行尾空白单独检查；作用域内
`git diff --check` exit code 0。工作区已有的无关修改保留，未提交或重置。

## 2026-10-06 执行计划 A/B 检查点

### A：来源和覆盖登记

- 重新计算 `NPC.cs`、`Main.cs`、`NPCID.cs` 的 SHA-256，三者均与 128 风格索引一致。
- 新增 [NPC AI Profile Coverage Ledger](../migration/ledgers/2026-10-06-npc-ai-profile-coverage-ledger.md)，登记全部 128 个 style 分支，并把具体 profile 与 style 库存分开。
- 映射 Blue Slime 默认来源身份：type/netID/style 为 1/1/1；Mother Slime 16 与 Lava Slime 59 保留为不同待迁移 profile。Blue Slime 的 helper 和效果闭包仍为 open，来源运行输出仍 not-run。
- 复查确认 ledger 表格含 style 0–127 连续、无重复的 128 行，并仅有一条 manifest 文档注册。发现并修复过表头与 style 0 粘连的问题；修复后重新计数通过。参考目录中找到 `TerrariaServer.exe`，但没有可控的隔离状态记录器，因此未将其运行结果当作 golden。

### B：有限宿主的重力阶段顺序切片

`RuntimeNpcStore.UpdateMovement` 现在在 AI 求值前计算重力参数、湿态侧通道和来源 AI 前速度限制；`NpcAiSystem.EvaluateAndCommitFinite` 同步提交有限行为的动作与四槽状态，宿主提交返回速度后再施加重力并运行当前移动/碰撞分支。该改动只建立有限路径的单一 AI 状态提交入口并对齐一段顺序，不提供动态 `noGravity`/`noTileCollide`、碰撞历史或完整 NPC 状态/效果路径，也不证明参考行为等价。

| 验证 | 命令/项目 | 结果 | 输出 |
| --- | --- | --- | --- |
| 专用 NPC AI 构建 | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal` | exit 0；0 warning / 0 error | `Build/bin/Terraria.NpcAi.Verification/Debug/net10.0/` 与依赖的 `Build/bin/Terraria.Npc/Debug/net10.0/` |
| 专用有限规则 verifier | `dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore` | exit 0；PASS | 控制台输出；中心/平局/目标门控/重复求值/周期边界/回家/覆盖拒绝 |
| 宿主模拟回归 | `& Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath 'Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld' -OutputDirectory 'Build/diagnostics/NpcAiRedesign/runs/stage-b-ai-owner-20261006'` | exit 0；模拟构建 34 warning / 0 error，全部 warning 属于既有 WorldGeneration 项目且 NPC AI 触及文件为 0 warning；时钟构建 0 / 0；summary `Succeeded=true`，33 项 evidence | `Build/diagnostics/NpcAiRedesign/runs/stage-b-ai-owner-20261006/summary.json`；构建产物位于 `Build/bin/` |

宿主脚本与正式 verifier 的差异已核对：副本固定本地 repo 根路径，并在两个增量构建增加 `--no-restore`；断言未改。输入为已有 WorldFile 319 生成世界。summary 的 33 项 evidence 覆盖既有 smoke、长跑、NPC 自然销毁、容量/代际、战斗、掉落、接触伤害、保存/加载与取消场景；未传 `SecondWorldPath`。这些仍只验有限宿主，不覆盖风格 0–127 来源差分、AI 内即时生成顺序或权威联机。

## 2026-10-06 执行计划 C1 检查点：Blue Slime 来源状态切片

来源闭包复核固定为 `type=1 / netID=1 / aiStyle=1`：`NPC.cs:8686-8699`、`NPCID.cs:11112`
和 `NPC.cs:20121-20125`。`AI_001_Slimes` 的默认 Blue Slime 路径仍包含非客户端体内物品
随机、item/stat 变体、Tile/collision scratch、网络和呈现分支，因此没有把整段方法误报为
已迁移。

本批新增以下生产 API：

- `NpcBlueSlimeProfile.CanHandle`：只接受 type=1、netID=1、aiStyle=1，防止把 style=1 的
  Mother/Lava 等其他 profile 误接入。
- `NpcBlueSlimeProfileInput` / `NpcBlueSlimeProfileState`：显式承载位置、速度、`ai[0..3]`、
  方向、目标槽位、昼夜/受伤/地下/SlimeRain、水态、碰撞历史和 authority 事实。
- `NpcBlueSlimeProfile.Evaluate`：纯状态转移，覆盖方向初始化、`ai[2] == 0` 初始化、
  `ai[0]` 三段阈值（0、-500..-1000、-1500..-2000）、湿态 -4 上限、-999 冻结哨兵、
  -6/-8 跳跃冲量和空中加速。
- `NpcBlueSlimeProfileResult` / `NpcBlueSlimeSourceBranch`：返回下一状态、动作、分支位和
  `ContainedItemGenerationRequested`、`TargetClosestRequested`、`NetUpdateRequested`。
- `NpcBlueSlimeContainedItemGenerator` / `INpcBlueSlimeRandomPort`：按来源调用顺序复现
  `AI_001_Slimes_GenerateItemInsideBody` 的四类物品选择、Ballooned 分支和 LowTiles/RockLayer/
  HardMode/NetMode 条件。`SelectForTypeOne` 增加普通非 Skyblock type=1 的外层首次选择；owner
  将选中 item id 或 `-1` 哨兵提交到该 NPC 的 ai[1]，不在此阶段创建 WorldItem。
- `INpcBlueSlimeProfileEffectPort` 与 `NpcBlueSlimeProfile.ApplyEffects`：将 item 生成、目标
  重选和网络同步按来源调用点交给 owner（初始路径为 item → network → target，湿态与跳跃
  路径按分支位重建）；纯计算不读取共享随机或 Tile scratch。

验证夹具新增并通过：普通首次 item 选择、地表特殊 item、地下 helper、No Traps 特殊物品、
派对分支、Remix 首次尝试、已提交状态拒绝重抽、item/network/target 顺序、99 tick 计数到首次
跳跃、夜间加速并在跳跃边界重选目标、-999 冻结哨兵和 inner helper 随机消费。`RuntimeNpcStore`
对精确 type=1 identity 选择该 profile，提交 ai[4]/ai[1]、方向、有限目标和 network intent，沿用
gravity/TileCollision。随后 24×18 和 value=25 与参考 SetDefaults 对齐。C1 仍只是有限宿主切片，
消费/掉出体内物品及其 stat/presentation、完整 authority、Tile scratch 和来源 golden 均 open。

| 验证 | 命令 | 结果 | 输出 |
| --- | --- | --- | --- |
| NPC 领域生产项目构建 | `dotnet build src/NSSLC/Component/Npc/Terraria.Npc.csproj --no-restore --nologo -v:minimal` | exit 0；0 warning / 0 error | `Build/bin/Terraria.Npc/Debug/net10.0/Terraria.Npc.dll` |
| NPC AI verifier（含 C1 profile） | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal`；`dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore` | 构建 exit 0；0 warning / 0 error；运行 exit 0；PASS | `Build/bin/Terraria.NpcAi.Verification/Debug/net10.0/`；控制台 `PASS: NPC AI center, ties, target gates, repeatability, phase boundary, home, coverage.` |
| Simulation 宿主增量构建（含 C1 依赖） | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal` | exit 0；0 warning / 0 error（本次增量输出） | `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll` |
| C1 依赖加载 smoke | `dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll 'Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld' 60 --players 0 --spawn-npc 1 --report 'Build/diagnostics/NpcAiRedesign/runs/stage-c1-blue-slime-profile-20261006-r2/c1-smoke-60.json'` | exit 0；`Succeeded=true`、60 tick、3 个 runtime NPC、`RuntimeNpcsUpdatedAtFinalTick=true` | `Build/diagnostics/NpcAiRedesign/runs/stage-c1-blue-slime-profile-20261006-r2/c1-smoke-60.json` |
| 34 项宿主回归与双世界切换 | `& Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath 'Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld' -SecondWorldPath 'Build/diagnostics/WorldGenerationRoundTrip/67890-medium-final/generated.wld' -OutputDirectory 'Build/diagnostics/NpcAiRedesign/runs/stage-c1-blue-slime-profile-20261006-r2'` | exit 0；`Succeeded=true`；34 项 evidence；宿主与时钟构建/运行 exit 0，记录 0 warning / 0 error；双世界切换身份隔离通过 | `Build/diagnostics/NpcAiRedesign/runs/stage-c1-blue-slime-profile-20261006-r2/summary.json`；宿主 `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll` |

这组证据只证明声明的纯状态切片和 effect-owner 顺序，不证明参考逐 tick golden、体内物品
随机结果、特殊 item stat、NewNPC/NewProjectile、Tile 放置、网络端别或完整宿主更新等价。

### C1 有限宿主接线补验（2026-10-06）

最初的宿主接线检查中，`RuntimeNpcStore.UpdateMovement` 只对 `type=1 / netID=1 / aiStyle=1` 选择
`NpcBlueSlimeProfile`，在 AI 前捕获目标槽位、old velocity、湿态和碰撞历史；求值后提交
ai[4]、方向、目标选择和 network intent，再执行现有 gravity/TileCollision 并提交碰撞状态。
当时 `CanContainItems=false` 是显式边界，体内 item 生成仍等待 owner，不会在 NPC AI 中偷偷创建
物品。后续普通世界选择与 ai[1] owner 接线见下方新增 C1 检查点。运行时报告已导出
`NetworkUpdateRequested`，本轮又加入 AI/localAI 槽快照。

| 验证 | 命令/场景 | 结果 | 输出 |
| --- | --- | --- | --- |
| Simulation 宿主增量构建 | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal` | exit 0；0 warning / 0 error | `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll` |
| C1 无玩家 smoke | `dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll 'Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld' 120 --players 0 --spawn-npc 1 --report 'Build/diagnostics/NpcAiRedesign/runs/stage-c1-blue-slime-runtime-20261006/c1-smoke-120.json'` | exit 0；`Succeeded=true`、120 ticks、`RuntimeNpcsUpdatedAtFinalTick=true` | `Build/diagnostics/NpcAiRedesign/runs/stage-c1-blue-slime-runtime-20261006/c1-smoke-120.json` |
| C1 有玩家 smoke | 同上，`--players 1` | exit 0；`Succeeded=true`；type=1 连续移动并完成有限目标/Tile 路径 | `Build/diagnostics/NpcAiRedesign/runs/stage-c1-blue-slime-runtime-20261006/c1-smoke-120-player.json` |
| NPC AI verifier | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal`；`dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore` | 两命令 exit 0；构建 0 warning / 0 error；运行 PASS（含 C1/C2） | `Build/bin/Terraria.NpcAi.Verification/Debug/net10.0/` |
| 完整宿主 verifier | `& Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath 'Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld' -OutputDirectory 'Build/diagnostics/NpcAiRedesign/runs/stage-c1-blue-slime-runtime-20261006/full-verifier-20261006'` | exit 0；`Succeeded=true`；33 项 evidence；Simulation/clock 构建和运行均 0 warning / 0 error | `Build/diagnostics/NpcAiRedesign/runs/stage-c1-blue-slime-runtime-20261006/full-verifier-20261006/summary.json` |

本补验仍未覆盖体内 item/stat、NewNPC/NewProjectile、真实 network/presentation、完整 authority、
source golden 或 Mother/Lava 等其它 style-1 身份；C1 回退需移除宿主分支并恢复有限 Slime 行为，
不撤销已提交的运行时 intent。

### C1 普通世界体内 item 选择与 ai[1] owner 接线（2026-10-06）

按来源 `NPC.cs:61150-61488` 加入 `NpcBlueSlimeTypeOneSelectionInput/Result` 和
`NpcBlueSlimeContainedItemGenerator.SelectForTypeOne`。精确 Blue Slime type=1 的首次体内物品入口
现检查 ai[1]、netMode、value，按来源次序推进 Slime Rain 尝试、helper、追加 item、No Traps/
Good World 特殊物品、生日派对、Remix 和 Vampire 分支。宿主用当前世界 seed、surface/rock 层、
时间、Party 与 npc value 构造输入；只支持普通非 Skyblock world。reference SetDefaults 的
Blue Slime type=1 宽高 24×18、value=25 已同步到 Simulation catalog，其他 catalog identity 不动。

Owner 将本次所选 item id，或来源没有选择时的 `-1` 待选哨兵提交到同一实例 ai[1]，然后在目标
重选前提交 network intent。这里实现的是 Slime 内部 item id 状态；没有创建 RuntimeWorldItem，
也未接入 later hit/consume/drop、item stat、呈现或奖励逻辑。运行时报告增加 Ai0..Ai3 和
LocalAi0..LocalAi3 用于观察写入。随机使用宿主 `.NET Random` 且传 `NetMode=0`，不是来源
`UnifiedRandom` / 服务端消费顺序；Skyblock/LowTiles 暂由能力门控阻断。

| 验证 | 命令 | 结果 | 输出 |
| --- | --- | --- | --- |
| Simulation build | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal` | exit 0；0 warnings / 0 errors | `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll` |
| NPC AI verifier build/run | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal`；`dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore` | 两命令 exit 0；build 0 warnings / 0 errors；PASS，包含 C1 普通选择分支及已有 C1–C4 场景 | `Build/bin/Terraria.NpcAi.Verification/Debug/net10.0/` |
| C1 120 tick smoke（0/1 玩家） | `dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld 120 --players {0|1} --spawn-npc 1 --report <report.json>` | 两次 `Succeeded=true`；`ai[1]` 从 0 到 -1 并保留到 tick 120；均更新 final tick；玩家场景命中 2 次、life 19 | `Build/diagnostics/NpcAiRedesign/runs/stage-c1-contained-item-selection-20261006-r2/c1-contained-item-smoke-120{,-player}.json` |
| 完整宿主回归 | `& Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld -OutputDirectory Build/diagnostics/NpcAiRedesign/runs/stage-c1-contained-item-selection-20261006-r2/full-verifier-20261006` | exit 0；`Succeeded=true`；36 项 evidence；Simulation/clock build/run 均 0 warnings / 0 errors | `Build/diagnostics/NpcAiRedesign/runs/stage-c1-contained-item-selection-20261006-r2/full-verifier-20261006/summary.json` |

这组证据没有执行 reference output differential，也不证明抽到了与 Terraria 相同的随机 item；
`ai[1]=-1` 表示来源待选状态，不是 item 已生成或被玩家拾取。type=1 消费/掉出、LowTiles/
Skyblock、其它 style-1 profile 和完整 authority 仍未闭合。

## 2026-10-06 执行计划 C2 检查点：Demon Eye 来源状态切片

来源闭包复核固定为 `type=2 / netID=2 / aiStyle=2`：`NPC.cs:8700-8711`、`NPCID.cs:11114`
和 `NPC.cs:20126-20130`；`AI_002_FloatingEye` 位于 `NPC.cs:53027-53509`。本批只提取普通
Demon Eye（type=2）可达分支，明确排除 The Hungry II、Wandering Eye 等共享 style-2 的类型。

本批新增以下生产 API：

- `NpcFloatingEyeProfile.CanHandle`：只接受 type=2、netID=2、aiStyle=2。
- `NpcFloatingEyeProfileInput` / `NpcFloatingEyeProfileResult`：显式承载碰撞前事实、方向、
  缩放、昼夜/地表/墓地、水态和 dust 随机 roll，返回速度、方向、noGravity 与分支/effect intents。
- `NpcFloatingEyeProfile.Evaluate`：覆盖 `oldVelocity` 反弹、日间地表 discouragement、通用
  缩放水平/垂直加速、wet 垂直 `0.95/-0.5/-4` 修正及 type=2 `Next(40)` dust 判定。
- `INpcFloatingEyeProfileEffectPort` / `INpcFloatingEyeRandomPort`：将 `EncourageDespawn(10)`、
  TargetClosest、dust 呈现和随机读取放到 owner 边界；`ApplyEffects` 保留 target → dust →
  wet-target 顺序。

验证夹具新增并通过：身份拒绝、碰撞反弹先于通用加速、日间地表 discouragement、wet 阻尼、
`Next(40)` 一次消费和 effect 顺序。随后 `RuntimeNpcStore` 对 type=2 选择该 profile，接入目标、
弃生存、dust intent、动态 `NoGravity`/`NoTileCollide` 和 TileCollision/碰撞状态提交，因此本批
状态为“C2 有限宿主接线已实现/验证，完整来源闭包仍 open”。

| 验证 | 命令 | 结果 | 输出 |
| --- | --- | --- | --- |
| NPC 领域项目聚焦构建（C2 改动） | `dotnet build src/NSSLC/Component/Npc/Terraria.Npc.csproj --no-restore --no-dependencies --nologo -v:minimal` | exit 0；0 warning / 0 error | `Build/bin/Terraria.Npc/Debug/net10.0/Terraria.Npc.dll` |
| NPC AI verifier（含 C1/C2 profile） | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal`；`dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore` | 构建 exit 0；0 warning / 0 error；运行 exit 0；PASS | `Build/bin/Terraria.NpcAi.Verification/Debug/net10.0/`；控制台 `PASS: NPC AI center, ties, target gates, repeatability, phase boundary, home, coverage, C1 Blue Slime, C2 Demon Eye.` |

该证据证明声明的纯状态切片和 effect-owner 顺序；新增宿主 smoke 还证明 type=2 运行时实体
连续 60 tick 更新、位置变化、`NoGravity=true`、目标/弃生存 intent 和碰撞报告可观察。它仍不
证明参考逐 tick golden、完整 TargetClosest 候选输入、真实 dust presentation、Tile scratch、
网络 authority 或完整来源更新等价。C2 回退为删除 profile、两个 C2 端口、即时效果组件及宿主
接线，恢复既有限 `NpcFloatingEyeAiBehavior`；已提交的运行时意图不自动重放。

一次依赖完整的 `dotnet build src/NSSLC/Component/Npc/Terraria.Npc.csproj --no-restore` 尝试被
工作树既有的未跟踪 `src/NSSLC/Component/Share/Entity/EntityRuntime.cs:290` 阻塞（lambda 捕获
`in firstComponent`，`CS1628`）。该文件不属于 C2 改动；使用 `--no-dependencies` 的 NPC 聚焦
构建和 verifier 构建均通过。未修改该无关阻塞。

## 2026-10-06 执行计划 C2 宿主接线与回归

本批把 Demon Eye 的有限 profile 接入 `RuntimeNpcStore.UpdateMovement`。身份条件严格为
`type=2 / netID=2 / aiStyle=2`；宿主在 AI 前捕获 old velocity、碰撞历史和湿态，在 profile
求值后提交方向、`NoGravity`/`NoTileCollide`，通过 effect port 提交目标选择、弃生存和 dust
intent，随后执行 TileCollision 或边界积分并提交 `CollideX/CollideY`。`NpcImmediateEffectStateComponent`
只保存本 tick 的效果意图，未伪装成真实 dust presentation 或网络提交。

实现过程中完整回归首先暴露一个组件生命周期错误：`TryRelease` 在终止 ECS 实体后才读取
`InstanceId`，投射物致死路径因此无法移除索引；修复为终止前捕获 identity。NPC 槽位探针同时
改为在释放前保存旧 identity，再验证释放后旧引用拒绝，避免从已移除组件读取身份。

| 验证 | 命令/场景 | 结果 | 输出 |
| --- | --- | --- | --- |
| Simulation 宿主增量构建 | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal` | exit 0；0 warning / 0 error | `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll` |
| Demon Eye 60 tick smoke | `dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll 'Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld' 60 --players 0 --spawn-npc 2 --report 'Build/diagnostics/NpcAiRedesign/runs/stage-c2-demon-eye-runtime-20261006/c2-smoke-60.json'` | exit 0；`Succeeded=true`、60 tick、`RuntimeNpcsUpdatedAtFinalTick=true`；type=2 `NoGravity=true`、位置变化、`DespawnEncouragementTicks=10` | `Build/diagnostics/NpcAiRedesign/runs/stage-c2-demon-eye-runtime-20261006/c2-smoke-60.json` |
| NPC AI verifier | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal`；`dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore` | 两命令 exit 0；构建 0 warning / 0 error；运行 PASS（含 C1/C2） | `Build/bin/Terraria.NpcAi.Verification/Debug/net10.0/` |
| 完整宿主 verifier | `& Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath 'Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld' -OutputDirectory 'Build/diagnostics/NpcAiRedesign/runs/stage-c2-demon-eye-runtime-20261006/full-verifier-final2-20261006'` | exit 0；`Succeeded=true`；33 项 evidence；Simulation/clock 构建和运行均 0 warning / 0 error | `Build/diagnostics/NpcAiRedesign/runs/stage-c2-demon-eye-runtime-20261006/full-verifier-final2-20261006/summary.json` |

本批没有传入 `SecondWorldPath`，因此不宣称第二世界切换覆盖。C2 仍未闭合 ZoneGraveyard、完整
TargetClosest/aggro 输入、真实 dust/网络呈现、reference golden、其它 style-2 类型，以及
完整 authority/保存策略；C1 也已接入有限运行时宿主，但体内 item/stat/presentation 和来源
golden 仍保持 open。

## 2026-10-06 执行计划 C3 检查点：Zombie Fighter 来源状态切片

C3 固定来源身份为 `type=3 / netID=3 / aiStyle=3`，来源入口是 `NPC.cs:20131-20135` 的
`AI_003_Fighters` (`NPC.cs:56637-61118`)。`NpcFighterProfile` 只提取了可在当前宿主事实中
证明的状态计算：方向初始化、目标重选意图、日间地表弃生存意图、停滞两 tick 反向、按 scale
计算的 1.0 基准速度上限与 0.07 加速，以及落地反向时的 0.8 衰减。`INpcFighterProfileEffectPort`
按弃生存、目标重选、同步顺序提交即时效果/目标/network intent；宿主继续执行有限 gravity、
TileCollision 和碰撞历史。

首次完整回归暴露了一个可观察回归：如果日间弃生存分支清除目标重选，脚本化 Zombie 会漂离
玩家，只接受 6 次命中且不掉落 Gel。修复为在有限宿主同时记录弃生存和目标重选意图后，独立
3600 tick 场景恢复 45 次命中、1 个 Gel 掉落和 1 次拾取。该兼容点已登记为有限 host 差异，
不是来源 golden 或 helper 闭包完成。

| 验证 | 命令/场景 | 结果 | 输出 |
| --- | --- | --- | --- |
| Simulation 宿主构建 | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal` | exit 0；0 warning / 0 error | `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll` |
| NPC AI verifier | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal`；`dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore` | 构建 exit 0；0 warning / 0 error；运行 PASS，包含 C1/C2/C3 | `Build/bin/Terraria.NpcAi.Verification/Debug/net10.0/` |
| Zombie 120 tick smoke | `dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll 'Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld' 120 --players 1 --spawn-npc 3 --report 'Build/diagnostics/NpcAiRedesign/runs/stage-c3-fighter-profile-20261006/fighter-smoke-120.json'` | exit 0；`Succeeded=true`；120 tick；type=3 位置/碰撞可观察；`DespawnEncouragementTicks=10`；最终 tick 更新完成 | `Build/diagnostics/NpcAiRedesign/runs/stage-c3-fighter-profile-20261006/fighter-smoke-120.json` |
| Zombie 跨日 smoke | 同一宿主 `1 --players 1 --world-time-rate 50000 --spawn-npc 3` | exit 0；`Succeeded=true`；`InitialDayTime=true` → `FinalDayTime=false`；type=3 仍移动，弃生存 intent 清零 | `Build/diagnostics/NpcAiRedesign/runs/stage-c3-fighter-profile-20261006/fighter-night-1.json` |
| 完整宿主 verifier | `& Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath 'Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld' -OutputDirectory 'Build/diagnostics/NpcAiRedesign/runs/stage-c3-fighter-profile-20261006/full-verifier-20261006-r2'` | exit 0；`summary.json` `Succeeded=true`；35 项 evidence；Simulation/clock build/run 均 0 warning / 0 error；战斗/drop/pickup/save 通过 | `Build/diagnostics/NpcAiRedesign/runs/stage-c3-fighter-profile-20261006/full-verifier-20261006-r2/summary.json` |

C3 仍未闭合 Tile 台阶/开门、完整 jump 与战斗动作、真实呈现/声音、authority/network sink、
reference golden 以及其它 aiStyle=3 类型；C4 接下来只接入显式障碍/门事实和有限效果端口，
不改变上述覆盖结论。

## 2026-10-06 执行计划 C4 检查点：Zombie Fighter 障碍跳跃与开门

C4 继续固定 `type=3 / netID=3 / aiStyle=3`。`NpcFighterTraversalInput` 将宿主捕获的前方
一至三格 Tile 几何、闭门坐标和有限目标垂直关系作为显式输入；profile 在 AI 结果中返回
`JumpRequested`/冲量以及 60 tick 闭门等待后的 `DoorOpenRequested`。宿主在重力与 TileCollision
前提交跳跃，效果端口先记录门请求，再由 `WorldGen.OpenDoor` 执行成功的门变更并调用
`LegacyWorldTileMapProjection.CommitLegacyTileMutations`。即时效果快照新增跳跃和门坐标，
因此 AI 状态、物理提交和 Tile owner 的边界可观察。

这不是完整 Fighter 迁移：宿主没有为本批构造带门的专用 WorldFile，C4 的 smoke 证明真实
Zombie tick 进入新的 Tile 捕获/提交顺序，但不证明某个门已在场景中成功打开。专家/墓地/血月
条件、共享 scratch、完整战斗动作、声音/呈现、authority/network sink、来源 golden 和其它
style-3 身份仍保持 open。

| 验证 | 命令/场景 | 结果 | 输出 |
| --- | --- | --- | --- |
| C4 profile verifier | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal`；`dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore` | 两命令 exit 0；构建 0 warning / 0 error；运行 PASS，包含 C1/C2/C3/C4 跳跃、门阈值和效果顺序 | `Build/bin/Terraria.NpcAi.Verification/Debug/net10.0/` |
| Zombie 180 tick smoke | `dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll 'Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld' 180 --players 1 --spawn-npc 3 --report 'Build/diagnostics/NpcAiRedesign/runs/stage-c4-fighter-traversal-20261006/fighter-traversal-180.json'` | exit 0；`Succeeded=true`；180 tick；位置、速度、碰撞、方向及新增跳跃/门快照字段可观察 | `Build/diagnostics/NpcAiRedesign/runs/stage-c4-fighter-traversal-20261006/fighter-traversal-180.json` |
| 完整宿主 verifier | `& Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath 'Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld' -OutputDirectory 'Build/diagnostics/NpcAiRedesign/runs/stage-c4-fighter-traversal-20261006/full-verifier-20261006'` | exit 0；`summary.json` `Succeeded=true`；35 项 evidence；Simulation 构建 exit 0、0 error（34 个既有依赖 warning）；clock 构建/运行 0 warning / 0 error；战斗/drop/pickup/save 通过 | `Build/diagnostics/NpcAiRedesign/runs/stage-c4-fighter-traversal-20261006/full-verifier-20261006/summary.json` |

C4 回退范围为移除 `NpcFighterTraversalInput`、结果/效果端口字段、即时效果快照字段和宿主
Tile 捕获/门提交，恢复 C3 有限运动路径。已提交的 Tile 变化、网络意图和跳跃状态不自动
重放；本记录保留证据，未把门失败边界或来源 golden 标为 verified。

## 2026-10-07 C2/C3 来源输入边界延续

C2 宿主 profile 输入现在携带实体当前 `TargetSlot`，不再把目标槽位隐藏为 `-1`；完整
`TargetClosest` 候选、aggro/noAggro/gross 和 `ZoneGraveyard` 事实仍明确保持 open。

C3 新增 `NpcHitStateComponent` 作为命中状态端口：NPC 成功承受非致死 projectile strike 后提交
`JustHit`，下一个 Fighter tick 原子消费后再进入 profile。宿主从当前目标槽位捕获目标中心、
玩家碰撞高度和 NPC 高度，profile 在来源底部对齐条件成立时返回 `DirectionY=-1`，宿主提交
该方向。verifier 增加命中清零 idle counter 与目标底部对齐断言。

本轮实际验证：NPC 领域聚焦构建 exit 0（0 warning / 0 error）；NPC AI verifier 聚焦构建
exit 0，运行 exit 0 且 PASS。常规 verifier 的首次构建遇到共享 `WorldStorageRoot.cs` 的 3 个
缺失 `Dispose` 成员，依赖输出刷新后按同一命令重试已 exit 0；Simulation 聚焦构建的最新
尝试仍被 `RuntimeProjectileStore.cs:207,21` 和 `:311,21` 的 `CS0305` 阻断，因此未执行
修改后二进制的宿主 smoke，也未把 C2/C3 标记为 source golden、authority/network 或完整
profile verified。

## 2026-10-06 执行计划 D1 检查点：Guide 有限任务生命周期

本批按设计第 5.2 节建立了首个真实跨 tick 任务状态。`NpcTaskLifecycleSystem` 将任务进入、
推进、完成、失败和中断变成显式结果；`RuntimeNpcEntity` 保存任务实例状态；
`RuntimeNpcStore.SyncGuideTask` 在每个真实 tick 的 AI 前按日间、住房事实选择 Guide 日间巡逻、
夜间回家或中断，并推进任务 cursor。该切片不改既有移动数值，也没有把住房、传送、坐具或
交谈伪装成已完成能力。

| 验证 | 命令/场景 | 结果 | 输出 |
| --- | --- | --- | --- |
| Simulation 宿主增量构建 | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal` | exit 0；0 warning / 0 error | `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll` |
| NPC AI verifier | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal`；`dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore` | 构建 exit 0；0 warning / 0 error；运行 exit 0；PASS，含 task lifecycle 的幂等进入、推进、完成、任务替换、失败、中断和重置 | `Build/bin/Terraria.NpcAi.Verification/Debug/net10.0/` |
| Guide 200 tick smoke | `dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll 'Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld' 200 --players 1 --spawn-npc 22 --input-script 'Build/diagnostics/NpcAiRedesign/runs/stage-d-guide-task-lifecycle-20261006-r2/neutral-input.json' --report 'Build/diagnostics/NpcAiRedesign/runs/stage-d-guide-task-lifecycle-20261006-r2/guide-town-ai-200.json'` | exit 0；`Succeeded=true`；Guide `Action=-1`、`Task=GuideDayPatrol`、`TaskPhase=Running`、`TaskCursor=200`；位置发生移动 | `Build/diagnostics/NpcAiRedesign/runs/stage-d-guide-task-lifecycle-20261006-r2/guide-town-ai-200.json` |
| 完整宿主回归 | `& Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath 'Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld' -OutputDirectory 'Build/diagnostics/NpcAiRedesign/runs/stage-d-guide-task-lifecycle-20261006-r4/full-verifier-20261006'` | `summary.json` `Succeeded=true`；33 项 evidence；Simulation/clock 构建和运行 0 warning / 0 error；命令 exit 0 | `Build/diagnostics/NpcAiRedesign/runs/stage-d-guide-task-lifecycle-20261006-r4/full-verifier-20261006/summary.json` |

本批同时修正了验证脚本与当前 TileEntity 探针的契约漂移：探针现在覆盖 Logic Sensor 与
Training Dummy 两个实体，失效清理后的更新次数为 4，并验证 Training Dummy 绑定、实体移除、
NPC 释放和活动计数恢复；两个同源 verifier 已同步断言并在末尾显式 `exit 0`，避免预期失败保存
场景遗留 `$LASTEXITCODE` 造成“summary 成功、命令失败”。D1 仍是有限城镇任务切片，不关闭
完整住房/传送/坐具/救援或参考 golden。

## 2026-10-06 D1 夜间宿主与 E1 父子关系切片

宿主新增两个受控场景。`--npc-night-probe true` 创建带住房事实的 Guide，配合
`--world-time-rate 50000` 在真实时钟阶段跨过日落；NPC 阶段最终观察到
`GuideReturnHome/Running` 和 cursor 1。`--npc-relation-probe true` 创建 Zombie 父节点与
Blue Slime 子节点，先绑定 `NpcParentRelationComponent` 和 generic `EntityRelationState`，再
释放父节点，确认子节点关系解绑后独立释放，活动 NPC 数恢复。
同一 probe 还验证子节点非致命命中路由到父生命根、父槽复用不污染旧子节点、解绑后子节点
恢复本地生命，以及父节点致死时整组释放和掉落归属；容量填充后子生成拒绝仍会恢复活动数。

| 验证 | 命令/场景 | 结果 | 输出 |
| --- | --- | --- | --- |
| Simulation 宿主增量构建 | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal` | exit 0；0 warning / 0 error | `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll` |
| D1 night host | `dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll <world> 1 --players 0 --world-time-rate 50000 --npc-night-probe true --report Build/diagnostics/NpcAiRedesign/runs/stage-d-guide-night-20261006/guide-night-1.json` | exit 0；`Succeeded=true`；`InitialDayTime=true` → `FinalDayTime=false`；`GuideReturnHome/Running`; cursor 1 | `Build/diagnostics/NpcAiRedesign/runs/stage-d-guide-night-20261006/guide-night-1.json` |
| E1 parent relation host | `dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll <world> 1 --players 0 --npc-relation-probe true --report Build/diagnostics/NpcAiRedesign/runs/stage-e1-npc-parent-relation-20261006/relation-probe-r3.json` | exit 0；`Succeeded=true`; attach、父生命根路由、父槽复用隔离、解绑后子本地命中、致死整组释放和掉落归属均通过 | `Build/diagnostics/NpcAiRedesign/runs/stage-e1-npc-parent-relation-20261006/relation-probe-r3.json` |
| Full host verifier | `& Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath <world> -OutputDirectory Build/diagnostics/NpcAiRedesign/runs/stage-e1-parent-relation-20261006/full-verifier-20261006-r3` | exit 0；`summary.json` `Succeeded=true`；35 evidence；Simulation/clock builds and runs 0 warnings / 0 errors | `Build/diagnostics/NpcAiRedesign/runs/stage-e1-parent-relation-20261006/full-verifier-20261006-r3/summary.json` |

E1 remains a generation/attach/detach slice. It does not cover worm adjacency, multi-part shared
life, summon ownership, partial-chain capacity policy, rewards/transforms, persistence, or network
authority. D1 remains a task lifecycle slice and does not close housing validity, teleport, seating,
dialogue, rescue, or complete town AI.

## 2026-10-06 执行计划 D2 检查点：Guide 回家传送有限协议

D2 在 D1 的 `GuideReturnHome` 任务上接入有限的回家传送协议。领域查询保留候选落点
`0、-1、+1` 的顺序，并检查 home floor 上方三格空间；宿主在 cursor 60 处执行查询，成功时
提交位置、清零速度、候选偏移和 network update 意图后完成任务，失败时记录 `NoPath`、标记
住房关系为 homeless 并保留失败任务。`NpcImmediateEffectStateComponent` 和运行时报告只
导出本 tick 的效果快照，不把真实网络发送、完整住房分配或路径搜索登记为已完成。

| 验证 | 命令/场景 | 结果 | 输出 |
| --- | --- | --- | --- |
| Simulation 宿主增量构建 | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore -p:UseSharedCompilation=false -v:minimal` | exit 0；0 warning / 0 error；产物在 `Build/bin/` | `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll` |
| NPC AI verifier | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal`；`dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore` | 两命令 exit 0；PASS，含 D2 候选顺序和全阻塞 | `Build/bin/Terraria.NpcAi.Verification/Debug/net10.0/` |
| D2 success 宿主 | `dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll 'Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld' 60 --players 0 --world-time-rate 0 --npc-home-return-probe success --report 'Build/diagnostics/NpcAiRedesign/runs/stage-d2-guide-home-return-20261006/home-return-success.json'` | exit 0；`Succeeded=true`；`GuideReturnHome/Completed`；`HomeTeleportSucceeded=true`；速度清零；位置改变；network update 意图可观察 | `Build/diagnostics/NpcAiRedesign/runs/stage-d2-guide-home-return-20261006/home-return-success.json` |
| D2 blocked 宿主 | 同一宿主命令将 probe 改为 `blocked`，输出 `home-return-blocked.json` | exit 0；`TaskPhase=Failed`、`TaskFailureReason=NoPath`、`HomeTeleportFailed=true`、住房 homeless | `Build/diagnostics/NpcAiRedesign/runs/stage-d2-guide-home-return-20261006/home-return-blocked.json` |
| D2 invalid 宿主（历史顺序） | 同一宿主命令将 probe 改为 `invalid`，输出 `home-return-invalid.json` | 历史 D2 结果：住房事实无效，回家效果失败并记录 `NoPath`，无成功传送；当前 D3 顺序见下方 D3 invalid probe | `Build/diagnostics/NpcAiRedesign/runs/stage-d2-guide-home-return-20261006/home-return-invalid.json` |
| D1 夜间补验 | `dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll 'Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld' 1 --players 0 --world-time-rate 50000 --npc-night-probe true --report 'Build/diagnostics/NpcAiRedesign/runs/stage-d-guide-night-20261006/guide-night-1.json'` | exit 0；日间跨到夜间；`GuideReturnHome/Running`；cursor 1 | `Build/diagnostics/NpcAiRedesign/runs/stage-d-guide-night-20261006/guide-night-1.json` |
| 完整宿主回归 | `& Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath 'Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld' -OutputDirectory 'Build/diagnostics/NpcAiRedesign/runs/stage-d2-guide-home-return-20261006/full-verifier'` | exit 0；`summary.json` `Succeeded=true`；35 项 evidence；Simulation/clock 构建和运行 0 warning / 0 error | `Build/diagnostics/NpcAiRedesign/runs/stage-d2-guide-home-return-20261006/full-verifier/summary.json` |

本批还发现并以最小范围修复了 Simulation 的三个既有集成编译断点：`RuntimePlayerStore`
补齐 `IDisposable` 声明，Player inventory slots 提供清理 TrashItem 的公开操作，root probe
使用 `PlayerInventoryItemSnapshot` 别名避免 `TileCoordinate` 命名冲突。这些修复不改变 NPC AI
协议；未执行广泛 reset/clean，工作树其他修改保持原样。D2 仍是有限 Guide 传送切片，不
宣称完整 Terraria reference parity。

## 2026-10-06 执行计划 D3 检查点：Guide 住房事实重新验证与 owner 同步

D3 将 `RuntimeNpcStore.RevalidateHousing` 放在 `SyncGuideTask` 之前。每个 town NPC tick 都重新
使用 `WorldGen.IsHousingRoomValidAt` 检查已记录的房屋事实；合法房屋保留 ECS 关系并同步
`TownHousingRegistrySystem.AssignRoom`，registry 不一致时记录 `HousingRegistrySynchronized`。
房屋失效时清除 ECS home、提交 homeless、调用 registry `MarkHomeless`、请求 network update，并
记录 `HousingRevalidationFailed` 与 owner 同步效果。保存快照对 homeless NPC 使用 `(-1,-1)` home
坐标，避免失效房屋坐标在恢复后重新成为有效事实。

这一更新顺序改变了 D2 invalid 场景的观察语义：住房失效发生在任务选择前，因此夜间 Guide 不再
进入 `GuideReturnHome`，而是保持 `Task=None/Idle`。D2 invalid 的历史报告仍保留为“回家效果拒绝
无效住房”的前一顺序证据；D3 专用 invalid probe 验证新的先失效后选任务契约。D3 仍是有限住房
owner 切片，不覆盖完整分配、坐具、对话、救援、路径搜索或 reference golden。

| 验证 | 命令/场景 | 结果 | 输出 |
| --- | --- | --- | --- |
| D3 Simulation 构建 | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal` | exit 0；0 warning / 0 error | `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll` |
| D3 NPC AI verifier | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal`；`dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore` | 两命令 exit 0；PASS，含 D3 housing effect | `Build/bin/Terraria.NpcAi.Verification/Debug/net10.0/` |
| D3 valid housing probe | `dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll <world> 1 --players 0 --world-time-rate 0 --npc-housing-revalidation-probe valid --report Build/diagnostics/NpcAiRedesign/runs/stage-d3-housing-revalidation-20261006/housing-valid.json` | exit 0；有效住房和 registry room 均存在；registry homeless=false；revalidation failure=false；owner sync=true；`GuideDayPatrol` | `Build/diagnostics/NpcAiRedesign/runs/stage-d3-housing-revalidation-20261006/housing-valid.json` |
| D3 invalid housing probe | 同上，将 probe 改为 `invalid`，输出 `housing-invalid.json` | exit 0；ECS/registry 均 homeless；revalidation failure=true；owner sync=true；network update=true；`Task=None`、`TaskPhase=Idle` | `Build/diagnostics/NpcAiRedesign/runs/stage-d3-housing-revalidation-20261006/housing-invalid.json` |
| D2 success/blocked 复核 | D2 success/blocked 60 tick probes，输出 `Build/diagnostics/NpcAiRedesign/runs/stage-d3-housing-revalidation-20261006/home-return-success.json` 与 `home-return-blocked.json` | success `GuideReturnHome/Completed`；blocked `NoPath` + homeless；两者未受 D3 影响 | D3 目录中的复核报告 |
| D3 homeless 保存/重载 | invalid probe 保存至 `Build/diagnostics/NpcAiRedesign/runs/stage-d3-housing-revalidation-20261006/persistence/homeless-after-revalidation.wld`，再运行 1 tick 重载 | 保存 `SaveCommitted=true` 且 Guide `HomeX/HomeY=-1/-1`；重载后仍 `Homeless=true`、`HomeX/HomeY=-1/-1` | `.../persistence/save-invalid.json`、`.../persistence/reload.json` |
| 完整宿主回归 | `& Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld -OutputDirectory Build/diagnostics/NpcAiRedesign/runs/stage-d3-housing-revalidation-20261006/full-verifier` | exit 0；`summary.json` `Succeeded=true`；35 项 evidence；Simulation/clock build/run 均 0 warning / 0 error | `Build/diagnostics/NpcAiRedesign/runs/stage-d3-housing-revalidation-20261006/full-verifier/summary.json` |

D3 只证明有限 town housing owner 的重新验证和 registry 同步边界。registry 当前仍按 NPC type
key 维护，完整住房分配、世界卸载清理、坐具/对话、救援奖励、联机真实发送和参考逐 tick 对照仍
保持 open。

## 2026-10-06 执行计划 E2 检查点：三节点关系链与根生命周期

E2 使用 `RuntimeNpcStore.TrySpawnParentChildChain` 创建 Zombie 根、Blue Slime 中段和 Blue Slime
尾段。每个子节点的 `NpcParentRelationComponent` 与 `EntityRelationState` 都指向前一节点的稳定
`EntityReference`，binding 同时记录三个实例身份、槽位和 attach tick。Combat owner 从尾段递归
解析到根生命 owner；非致命命中先改变根生命，致命命中释放根、中段和尾段并选择根掉落。根槽位
随后复用时，旧尾段 projectile target 被代际/引用校验拒绝。容量保持两个空槽后尝试三节点链，
只创建的前缀按逆序回滚，活动数回到容量 sentinel，填充节点释放后回到探针初始活动数。

| 验证 | 命令/场景 | 结果 | 输出 |
| --- | --- | --- | --- |
| E2 Simulation 构建 | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal` | exit 0；0 warning / 0 error | `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll` |
| E2 chain probe | `dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll <world> 1 --players 0 --npc-relation-chain-probe true --report Build/diagnostics/NpcAiRedesign/runs/stage-e2-npc-relation-chain-20261006/relation-chain-probe-r2.json` | exit 0；`Succeeded=true`；三节点 attach/reference 稳定、尾节点命中归属根生命、根死亡整链释放、中段释放后尾节点解绑并只影响本地生命、同槽复用旧引用拒绝、容量部分链回滚和活动数恢复均为 true | `Build/diagnostics/NpcAiRedesign/runs/stage-e2-npc-relation-chain-20261006/relation-chain-probe-r2.json` |
| 完整宿主回归 | `& Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld -OutputDirectory Build/diagnostics/NpcAiRedesign/runs/stage-e2-npc-relation-chain-20261006/full-verifier-20261006-r2` | exit 0；`summary.json` `Succeeded=true`；36 项 evidence；Simulation 与 clock build/run 均 0 warning / 0 error；E1 与 E2 relation probes 全部通过，包含中段释放断链 | `Build/diagnostics/NpcAiRedesign/runs/stage-e2-npc-relation-chain-20261006/full-verifier-20261006-r2/summary.json` |

E2 仍是有限关系链运行时契约，不覆盖 Worm 或其他多部位来源规则、节点变换、召唤者关系、奖励
次数、保存加载和网络 authority。中段释放后的尾节点解绑是当前通用 owner 策略证据，不能替代
具体来源家族对断链的处理规则。回退恢复 E1 单父子根解析/死亡清理并移除 chain probe；E1 路径
及普通槽位代际校验保持不变。

## 2026-10-06 执行计划 C1.1 检查点：Dirt Slime 同 tick 状态接续

参考文件指纹与 `2026-10-05-npc-ai-reference-index.md` 一致：`NPC.cs` 为
`ED8AA2302730A9E046EF310E543204FBA391BD35B1C9C0B213C8065DFBBE39F0`，`Main.cs` 为
`E24E61C9903BB7995F47E36C51EDBE43481B643226861B717020877E7E22B63F`，`NPCID.cs` 为
`E040B9CFFFD57842C0099F11322EE5EC1DDDD605743A25BAE5415DB31B0C025D`。

`AI_001_Slimes` 先尝试为符合条件的 Slime 选择 `ai[1]`，接着在同一调用里运行变体分支。
因此 runtime owner 现在先执行选择并提交 `ai[1]`，再完成 profile 求值。共享纯函数
`NpcBlueSlimeProfile.WithContainedItemSelection` 把选中结果放进求值输入；Dirt Slime (`ai[1] == 2`)
落地时先将 `ai[0]` 加 9，再过 `-999` 冻结哨兵检查和通用地面计数/跳跃阶段逻辑。已选结果
传给 `ApplyEffects`，防止重复抽取随机数；同步意图仍在目标重选之前提交。

Verifier 用固定输入验证了普通 Dirt Slime 从 `ai[0] = -100` 到 `-90`，以及 `ai[0] = -999`
经选中 `ai[1] = 2` 后进入 `-500..-1000` 跳跃阈值并在同 tick 重置为 `-2120`。有限宿主的
120 tick smoke 使用正常随机种子，最终 `ai[1]` 均为 `-1`；它们没有随机选出 Dirt item，不能
算作宿主实际执行这条 Dirt 分支的证据。来源逐 tick golden、统一随机序列和 authority parity
仍未验证。

| 验证 | 命令/场景 | 结果 | 输出 |
| --- | --- | --- | --- |
| 环境 SDK | `dotnet --version` | `10.0.400` | 仓库 `global.json` 选择的 SDK |
| NPC AI verifier 构建 | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal` | exit 0；0 warnings / 0 errors | `Build/bin/Terraria.NpcAi.Verification/Debug/net10.0/Terraria.NpcAi.Verification.dll` |
| NPC AI verifier 运行 | `dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore` | exit 0；PASS，含同 tick item handoff、Dirt 计数和冻结/跳跃顺序 | same verifier project |
| Simulation 直接构建 | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal` | exit 0；17 warnings / 0 errors；全部 warnings 来自已有 `NSSLC.WorldGeneration` 文件 | `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll` |
| C1 0/1 玩家 smoke | 两条 120 tick 命令，`--spawn-npc 1`，players=0/1 | 均 exit 0、`Succeeded=true`、120 ticks、最终 NPC 更新完成、`ai[1]=-1`；players=1 记录 2 次受击 | `Build/diagnostics/NpcAiRedesign/runs/stage-c1-dirt-slime-counter-20261006-final/c1-smoke-120-no-player.json`、`c1-smoke-120-player.json` |
| 完整宿主回归 | `& Build/diagnostics/NpcAiRedesign/verify-final.ps1 -WorldPath Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld -OutputDirectory Build/diagnostics/NpcAiRedesign/runs/stage-c1-dirt-slime-counter-20261006-final/full-verifier` | exit 0；`Succeeded=true`；36 项 evidence；Simulation 与 clock build/run 均 0 warnings / 0 errors | `Build/diagnostics/NpcAiRedesign/runs/stage-c1-dirt-slime-counter-20261006-final/full-verifier/summary.json` |

这个检查点只证明 finite host 的选择顺序接线、Dirt Slime 纯状态转移和通用宿主回归。普通随机
smoke 没有选中 Dirt item；其他 `ai[1]` 分支仍有 stat、物理、Buff、Tile、Projectile、NPC spawn
和 presentation effects 未接入。`NPCLoot` 行为扫描未发现 `ai[1]` 专属普通物品掉落分支，完整
death/effect 图仍 open；LowTiles/Skyblock、`UnifiedRandom`、客户端/服务端 authority 和 reference
output parity 也保持 open。

## 2026-10-06 执行计划 C1.2 验证：Stone / Cloud Slime gravity

此次实现从原版 `NPC.cs:61592-61603` 接入 Stone Slime（`ai[1] == 3`）下落时
`velocity.Y += gravity * 2`，以及 Cloud Slime（`ai[1] == 751`）非零竖直速度时
`velocity.Y -= gravity * 0.6`。宿主将本 tick `NpcGravitySystem` 的 gravity 作为 profile 显式
输入；两变体调整发生在 profile 的冻结哨兵、湿态和通用地面/跳跃计算之前。纯 profile
verifier 用 gravity=0.25 固定检查 Stone / Cloud 上升与下落、Cloud 静止门控，以及两变体
在 `ai[0] == -999` 早退前调整速度的顺序。名称与颜色等 presentation 仍没有接入。

| 验证 | 命令/输出 | 结果 |
| --- | --- | --- |
| NPC AI verifier 构建 | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal` | exit 0；0 warning / 0 error |
| NPC AI verifier 运行 | `dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore` | exit 0；PASS，包含 C1.2 Stone / Cloud gravity 场景 |
| Simulation 构建 | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal` | exit 0；0 warning / 0 error；log `Build/diagnostics/NpcAiRedesign/runs/stage-c1-stone-cloud-gravity-20261006/simulation-build.log` |
| 120 tick smoke | 0 / 1 玩家，`--spawn-npc 1` | 均 `Succeeded=true`，末 tick 更新完成；无玩家 `ai[0]=-1120, ai[1]=-1`；有玩家 `ai[0]=-1106, ai[1]=-1`、2 次受击。随机 seed 未强制 Stone / Cloud 分支 |
| 完整宿主回归 | `Build/diagnostics/NpcAiRedesign/runs/stage-c1-stone-cloud-gravity-20261006/full-verifier/summary.json` | exit 0；`Succeeded=true`；36 项 evidence；Simulation / clock build 和 run 均 0 warning / 0 error |
| 参考文件指纹 | `NPC.cs`、`Main.cs`、`NPCID.cs` SHA-256 | 与 reference index 一致；来源只读 |

C1.2 不代表其他 `ai[1]` variant stats、Buff、Tile、Projectile、NPC 生成与呈现行为已经闭合，
也不代表 source output parity、随机流或 authority 已验证。host smoke 没有强制选择这两个变体；
精确条件由纯 verifier 覆盖。




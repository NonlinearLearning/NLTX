# NPC AI C1.3 检查点：Mother Slime style-1 有限状态切片

日期：2026-10-07（Asia/Shanghai）  
阶段：C 普通敌人和生物  
状态：有限 profile 已接入并通过 verifier / 宿主 smoke；来源差分未运行  
执行计划状态：仍为 `in_progress`。依照工作区并发协调要求，本检查点独立新增，没有改写
覆盖 ledger 或执行计划的公共段落。

## 来源与选择

本批选择 Mother Slime（type=16、netID=16、aiStyle=1），不选择 Lava Slime。参考
`NPC.cs` 的 `AI_001_Slimes` 对 Mother Slime 使用公共 wet、落地计数和跳跃路径；
`SlimeCanContainItems` 的显式集合只包含 1、59、147、184、537，因此 type 16 不进入
contained-item 选择。Lava Slime type 59 另有 Remix、hellstone 与跳跃速度分支，带有更多
本批没有接入的世界状态与效果依赖。

来源定位：

- `NPC.cs:20121-20125` 将 `aiStyle=1` 派发到 `AI_001_Slimes`。
- `NPC.cs:9077-9093` 设置 Mother Slime type 16 的默认属性；`NPC.cs:17934` 是默认
  `netID=type` 赋值；`NPCID.cs:11142` 命名 `MotherSlime=16`。
- `NPCID.cs:4820` 定义 `SlimeCanContainItems` 集合，type 16 不在其中。
- 本批使用的只读来源指纹与索引一致：`NPC.cs`
  `ED8AA2302730A9E046EF310E543204FBA391BD35B1C9C0B213C8065DFBBE39F0`；`Main.cs`
  `E24E61C9903BB7995F47E36C51EDBE43481B643226861B717020877E7E22B63F`；`NPCID.cs`
  `E040B9CFFFD57842C0099F11322EE5EC1DDDD605743A25BAE5415DB31B0C025D`。

## 已实现范围

`NpcMotherSlimeProfile.CanHandle` 只接受 `type=16 / netID=16 / aiStyle=1`；profile 输入身份
不匹配时 `Evaluate` 明确拒绝。它不作为其它 style-1 NPC 的 fallback，也不复用 Blue Slime
profile 的身份门控。

纯状态转移覆盖本来源公共路径的有限部分：

- `ai[2]==0` 初始化 `ai[0]=-100`、`ai[2]=1` 并请求目标重选；同 tick 继续进入落地计数。
- wet 状态读取 `collideY`、速度与 `ai[3]`，处理上浮转向、竖直阻尼/上限和目标重选门控。
- 落地处理 `SolidCollision` 位置修正、`ai[3]` 转向、水平摩擦和 `ai[0]` 计数。
- 依据来源阈值处理三段跳跃计数、水平/竖直冲量、第三段 X 记录和同步意图；空中路径
  保留 `collideX` 小位移修正与水平加速/阻尼。
- `ai[0]==-999` 在 wet 与普通运动前返回冻结结果。
- `localAI[0..3]` 不由 Mother Slime 来源路径写入；宿主提交 ai 状态时保留原 localAI 快照。
- 每个 profile 结果都包含 `ContainedItemSelectionRejected=true`；
  `SupportsContainedItemGeneration` 固定为 false，对应 `SlimeCanContainItems[16]` 的来源
  拒绝边界。

效果通过 `INpcMotherSlimeProfileEffectPort` 执行。跳跃分支的网络同步意图先于目标重选意图；
初始化只请求目标重选。Simulation 宿主把网络效果记入 NPC 即时效果状态，并将目标重选交给
当前有限 `NpcTargetSelectionSystem` 适配器。这个宿主适配器仍使用 `Aggro=0`、
`NoAggro=false`、`Gross=true`、无 Tank Pet 和无 NPC 候选的有限输入。

`RuntimeNpcStore.UpdateMovement` 在重力准备和湿态捕获之后，对精确身份选择
`UpdateMotherSlimeMovement`，提交 ai[0..3]、方向、效果意图和速度，再沿用当前宿主的重力与
`Collision.TileCollision`。`-999` 返回跳过后续 profile 移动/TileCollision。

## 身份限制

Simulation 当前 catalog 的 netID 16 定义仍是 `npc.green-slime`，typeId 也为 16，aiStyle 为
1；它不是 Terraria Mother Slime 内容定义。宿主 smoke 因此证明真实 tick 路径按数值身份进入
profile，并观察到状态推进，但不证明 Mother Slime 的来源属性或几何一致。该 catalog 定义
当前生命上限为 14、尺寸 32×24；参考 Mother Slime 为生命上限 90、尺寸 36×24、scale 1.25。
Tile 碰撞和目标几何受该映射差异影响，必须在 ledger 中保留为 open。

## 验证

| 验证 | 命令/结果 | 输出 |
| --- | --- | --- |
| SDK | `dotnet --version` → `10.0.400` | 仓库 `global.json` 所选 SDK |
| 纯 verifier 构建 | `dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-restore --nologo -v:minimal`；exit 0，0 warning / 0 error | `Build/bin/Terraria.NpcAi.Verification/Debug/net10.0/Terraria.NpcAi.Verification.dll` |
| 纯 verifier 运行 | `dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore`；exit 0，PASS；包含 C1.3 Mother Slime | verifier 控制台 |
| Simulation 增量构建 | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal`；exit 0，23 warnings / 0 errors | `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll` |
| 真实宿主 smoke | `dotnet Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll 'Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld' 120 --players 0 --world-time-rate 0 --spawn-npc 16 --report 'Build/diagnostics/NpcAiRedesign/runs/stage-c1-3-mother-slime-20261007/mother-slime-smoke-120.json'`；exit 0，`Succeeded=true`、120 tick、末 tick NPC 更新完成；netID 16 的 `ai[2]=1`、`ai[0]=-1120`，位置由 `33520,3672` 变为 `33568.71,3639.4001` | `Build/diagnostics/NpcAiRedesign/runs/stage-c1-3-mother-slime-20261007/mother-slime-smoke-120.json` |

verifier 首次构建发现既有 Blue Slime fixture 少传 `BaseDefense`，且一个
`NpcBlueSlimeProfileResult` fixture 未跟上新增的 `Defense` 参数。本批只补齐这两个 verifier
构造参数，之后 verifier 构建和运行通过；没有更改 Blue Slime profile 行为。

其后尝试重新构建 Simulation 时，工作区其他 NPC/WorldStorage 改动导致当前树无法再次构建：

- 同一增量构建命令 exit 1，0 warning / 2 error：`WorldStorageRoot.cs:27` 和
  `TileEntityRestoreSystem.cs:4` 找不到 `TileEntityStore`；当前工作区状态显示
  `TileEntityStore.cs` 被删除。
- `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --no-dependencies --nologo -v:minimal` exit 1，0 warning / 2 error：当前
  `Program.cs:427`、`:2841` 调用 `RuntimeItemRegistry` 时未传其新必需的 `EntityRuntime` 参数。

这两次失败在 profile 和宿主分支首次成功构建、Smoke 运行之后出现，属于当前共享工作树中的
其他未闭合改动。本批没有恢复 `TileEntityStore.cs`、修改 `RuntimeItemRegistry` 调用点或运行
clean/reset。成功的 Simulation 构建和 120 tick smoke 输出仍留在 `Build/`。

## 未闭合项

- **随机**：Mother Slime 当前接入的公共状态转移不消费随机数，且明确拒绝 contained-item
  随机入口；未执行来源 RNG trace 或 reference 随机流对照。
- **authority / 网络**：当前 Simulation 是本地有限宿主。profile 只记录同步意图；没有证明
  服务端/客户端执行门控，也没有接入或验证真实 packet/network sink、重传和去重。
- **Tile / 碰撞**：宿主在边界捕获 Wet、碰撞历史并执行一次 `SolidCollision` 输入查询，profile
  返回位置修正；其后使用现有重力与 `TileCollision`。共享 Tile scratch、来源调用时序/几何和
  Mother 专属尺寸尚未逐 tick 对照。
- **目标与外部效果**：目标候选仅为有限玩家适配器；没有完整 TargetClosest 参数、Aggro、
  noAggro、gross、Tank Pet 或 NPC 目标闭包。死亡分裂、变换、伤害/掉落、呈现与完整 AI helper
  图仍未接入。
- **来源 parity**：纯 verifier 证明输入边界与选定公式；120 tick smoke 运行的是有限宿主，未
  对照 Terraria source output，故 reference golden 状态仍为 `not-run`。
- **完整 host build**：profile 所在 Simulation 项目有一次成功增量构建；当前工作树后续改动
  阻断了再次构建，须待 `TileEntityStore` 和 `RuntimeItemRegistry` 上下文恢复一致后复验。

## 回退

只回退本 C1.3 切片时，删除 `NpcMotherSlimeProfile`、其 Input/State/Result/SourceBranch 与
effect-port 六个新增源码文件；从 `RuntimeNpcStore` 移除 `isSourceMotherSlime` 门控、即时效果
tick 条件、`UpdateMotherSlimeMovement` 调用/方法和 `RuntimeNpcMotherSlimeEffectPort`；从纯
verifier 移除 C1.3 断言与 `RecordingMotherSlimeEffectPort`；删除本 checkpoint。保留已有
Blue Slime fixture 的签名补齐（它修复的是既有 verifier 编译问题），不触碰 D/E/F 文件或
`Build/` 输出，也不运行广泛清理。回退后 netID 16 恢复到现有有限
`NpcSlimeAiBehavior` 路径；这只是模拟器行为，不得登记为 Mother Slime 来源行为。

## 建议合并项

建议主会话将 coverage ledger 中 Mother Slime 行从 `mapped / not-run` 更新为 `mapped / partial`
实现与验证：精确身份 handler、公共 ai/wet/jump 有限切片和 contained-item 拒绝已实现；保留
Green Slime netID 16 catalog mismatch、有限目标输入、Tile/authority/network、其他效果及
reference golden `not-run`。执行计划 C 仍保持 `in_progress`，只在 C 状态摘要中追加
“Mother Slime C1.3 有限状态切片已接入并完成纯 verifier/宿主 smoke，完整来源对照仍未完成”。

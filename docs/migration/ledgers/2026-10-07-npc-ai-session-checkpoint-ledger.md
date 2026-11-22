# NPC AI 会话检查点合并账本（2026-10-07）

文档 ID：DOC-2026-10-07-npc-ai-session-checkpoint-ledger
逻辑域：migration
产物类型：ledger、evidence
状态：merged
范围：2026-10-07 NPC AI 并行会话的 22 份切片检查点
证据入口：`Build/diagnostics/NpcAiRedesign/`、`Build/diagnostics/NpcAiReferenceVerification/20261007T035310365Z/`
canonical 路径：`docs/migration/ledgers/2026-10-07-npc-ai-session-checkpoint-ledger.md`

## 0. 合并说明

本文件合并了同目录下 22 份 `2026-10-07-npc-ai-*.md` 检查点，保留各切片的源码范围、实现结论、
验证状态和未闭合项；逐条构建命令、完整日志和逐场景断言已从正文移除，只保留证据目录入口。

被合并的文件已删除，可从前一提交 `6518acc6e3cc96a891f97d859847f31a384a961d` 恢复：

```powershell
git show 6518acc6e3cc96a891f97d859847f31a384a961d:docs/migration/ledgers/<原文件名> > <输出路径>
```

本文件**不提升任何 profile / coverage 状态**。原文各自的"未验收""partial/open""failed"
结论在合并后仍然成立。

## 1. 固定只读来源

全部切片对照同一份只读来源，各切片指纹一致：

| 来源文件 | SHA-256 |
| --- | --- |
| `Terraria/NPC.cs` | `ED8AA2302730A9E046EF310E543204FBA391BD35B1C9C0B213C8065DFBBE39F0` |
| `Terraria/Main.cs` | `E24E61C9903BB7995F47E36C51EDBE43481B643226861B717020877E7E22B63F` |
| `Terraria.ID/NPCID.cs` | `E040B9CFFFD57842C0099F11322EE5EC1DDDD605743A25BAE5415DB31B0C025D` |
| 只读来源树 manifest（1,609 文件） | `F8EB8115173B0BDA98577C5654542C3D63141A0CA8A8BF3E065E72EDA3F3D12E` |
| 原始 `TerrariaServer.exe` | `CF30EBDA6839E9FF9556F3BA6CD84B18385CD3283E5462745BAA57D1548B669A` |
| `NPC.AI` IL（原 EXE 与隔离源码构建一致） | `6269E82F828E8AD9F7C1F371AEB3CF8BB3F4BDE65864EE2F925A9FB4C2048BCC` |

来源根目录：`D:/TRbackup/无任何删减通过编译`。所有切片均只读引用，未修改来源。

## 2. Guide（`type=22 / netID=22 / aiStyle=7`，11 个切片）

纯 profile 计算，世界读写、门体变更、随机和网络由 caller/owner 端口承担；task 生命周期仍归
`NpcTaskLifecycleSystem` / `NpcTaskReference`。

| 切片 | 源码范围 | 落地的行为 | 验证 |
| --- | --- | --- | --- |
| 源 profile 首片 | `9163-9169`、`20996-20998`、`53759-53775`、`54204-54230`、`53525-53628`、`53630-53668`、`56443-56472` | 城镇回归压力 `Raining\|\|!DayTime\|\|Eclipse\|\|SlimeRain\|\|(IsStorming && Y/16<WorldSurface)`；服务端+非无家+压力+无好休息点的回家门；`0,-1,+1` 候选顺序与三格净空；成功 `home-teleport -> network-sync -> force-sitting`，失败 `mark-homeless -> quick-find-home` | Guide verifier PASS；source golden `not-run` |
| 日间移动 | `54406-54623` | `ai[0]==0/1` 多 tick 转移、方向 `0→1` 归一、落地阻尼、回家座标下的 ForceSitting 请求 | 同 verifier，含 `ai[0]==0` 阻尼与请求场景 |
| 行走预测与对话门 | `56475-56555`、`54096-54116` | `homeFloorX±35` 范围、拥挤/溺水 keep-walking、六格 `j=-1..4` 液/固采样、落水溺水风险；对话门对非专用状态复位 `ai[0]=0/ai[1]=300/localAI[3]=100` 并朝向玩家 | 场景覆盖落地、越界归家、拥挤、岩浆/满液、专用状态保留 |
| 好休息点与落座 | `53525-53545`、`53546-53628`、`53630-53668`、`56443-56464` | 夜间 `ai[0]==5` 双轴 ≤7、否则当前格精确相等；地板下探、座椅候选 `x±7`/`y+2..y-6` 最近曼哈顿优先；落座 owner 的 5 项实时校验后消费一次 `Next(10800)` 并按源序写 `ai[0]=5`、`ai[1]=900+r`、direction、bottom、零速度、`localAI[3]=0`、`netUpdate` | 3 项验收修正后有占用格/可用格/已在座三类 owner probe；profile SHA-256 `9837DBBF…3204AC` |
| 落座 tick | `55058-55087` | 横向阻尼 `0.8f`、`ai[1]` 递减、无效座即时 `ai[1]=0`、到期转 `ai[0]=0`/`60+Next(60)`/`ai[2]=0`/`localAI[3]=30+Next(60)` | verifier PASS |
| `ai[0]==8` 续接 | `55058-55087` | 阻挡后的速度阻尼与 `ai[1]` 危险计时刷新（`ai[1]<60` 边界） | verifier PASS |
| 对话 tick | `55089-55119` | `ai[0]==6/7/18/19`：有效目标阻尼、计时递减、方向修正、`ai[0]==18` 的 `localAI[3]` 钳入 `[1,2]` | verifier PASS |
| 危险扫描 / 攻击配置 / 空闲 | `54241-54405`、`55312-55582`、`55989-56149`、`96181-96193` | `NpcGuideDangerScan` 扩展与基础危险范围、stinky、最近追击 ID；`PrettySafe < 最近危险距离` 条件；攻击配置与空闲分支 | 修正 PrettySafe fixture 后 PASS |
| 攻击发射窗口 | `55312-55582` | `ai[0]==12`：普通模式射弹 `1`、基础伤害 `12`、冷却 `30+Next(20)`；困难模式分支 | verifier PASS |
| 通行（门/跳/溺水） | `54624-55030` | 关门/高门 pending、`Next(10)==0\|\|压力` 开门门禁（压力时仍消费随机）、三/二/一格跳 `-6f/-5f/-4.4f`、危险阻挡转 `ai[0]=8/ai[1]=240`、keep-walking `ai[1]=90`、避坠恢复 `ai[0]=0/50+Next(50)`、溺水逃逸 `26f` 上限 | verifier PASS |
| 编译复核 | — | `CS8156` 复核：当前 NPC 项目 compile passed | 仅证明编译，不证明 Guide 行为或服务端整体 |

Guide 仍然 open：通用 `ai[0]==2/11` 空闲 tick、普通动画分支、完整宿主组合、source differential、`ai[0]==10` 的 Guide 专用攻击配置。

## 3. Slime（C1 Blue / C1.3 Mother，4 份）

| 切片 | 结论 |
| --- | --- |
| Mother Slime 有限切片 | `type=16/netID=16/aiStyle=1` 精确身份 handler，公共 `ai`/wet/落地计数/跳跃切片，明确拒绝 contained-item 随机入口；来源范围 `9077-9093`、`20121-20125`。宿主 smoke 通过，**来源差分未运行**；coverage 行建议由 `mapped/not-run` 升为 `mapped/partial` |
| 来源差分 C1/C1.3 | 32 场景 / 182 tick；原 EXE 与隔离源码构建的采样状态与 IL 一致，source-to-source 差异为空。但 Blue/Mother profile 对照仍 **failed**：8 个状态差异 + 11 个 observable（含 `netUpdate`），profile gate exit 2，共 19 项差异。C2/C3 尚未加入 |
| cooldown 宿主验收 | `ai[2]` 递减修复（`3→2→1→1`，冻结哨兵保留 3），位置在 `ai[0]==-999` 早退之后、湿态之前；4 条宿主场景（Blue/Mother × 1/120 tick）全部 exit 0、末 tick NPC 更新完成、输入世界未改。绑定产物：Simulation `8D1994A4…BC50`、`Terraria.Npc.dll` `6A54C035…6192` |
| B2 目标选择主验收 | 固定来源 `NPC.cs:78861,78871,78878` 的普通/WOF 距离表达式被复现为**来源反例**：生产 `Select` 与来源字面评分不一致（整数半宽、目标矩形截断）。生产修复与宿主验收 pending |

## 4. Eye of Cthulhu（F1，`type=4/netID=4/aiStyle=4`，3 份）

| 切片 | 结论 |
| --- | --- |
| 首片 profile | 开场悬停、冲刺周期、变身、仆从 profile 均有 verifier 覆盖；含二次变身边界 12 个 Dust（1 通用 + 10 仆从 + 1）。产物见 `Build/diagnostics/NpcAiRedesign/stage-f1-host-recovery-20261007/` |
| 夜间悬停与冲刺 | 经典/专家夜间的开场悬停边界与首次冲刺在**新鲜 Simulation 产物**上跑通；范围外状态转移（`ai[0]=1` 变身）未实现也未宿主验收 |
| 夜间宿主验收 | 12 条真实宿主运行 exit 0、`Succeeded=true`、输入世界 SHA `022FB1C6…B241` 不变；断言覆盖 `EyeOfCthulhuVelocityX/Y`、`EyeOfCthulhuTargetSlot`、`BeginDash`、network intent 与夜间条件 |

F1 仍 open：变身阶段、仆从即时生成、完整目标输入、authority/network、呈现、save/load/unload、完整 source golden。

## 5. NPC 任务生命周期（3 份）

| 切片 | 结论 |
| --- | --- |
| 结束协议 | 四种结束原因 `Death/Transform/Removal/WorldUnload` 只写 `NpcTaskStateComponent`；终止是**取消**不是失败，`PreviousTask` 保留原 kind/phase/cursor/failure/generation，`CurrentTask` 清除并记录首次 `NpcTaskEndReason`。同原因重复终止返回 `AlreadyEnded`，不同后续原因保留首次 reason。`Reset` 与新 task 递增 generation 以拒绝旧引用 |
| 引用绑定推进 | `Advance/Complete/Fail/Interrupt` 增加 `(handle, NpcTaskReference, kind)` 重载；7 种拒绝原因 `InvalidReference`、`StaleEntityReference`、`StaleTaskReference`、`NotRunning`、`TaskKindMismatch`、`InvalidTaskKind`、`InvalidFailureReason`，检查顺序固定，失败路径不写状态。kind-only 旧入口保留但只适用于同一同步边界 |
| 宿主接线与验收 | `RuntimeNpcEntity.TryCaptureTaskReference/TryTerminateTask` 经 `EntityRuntime.TryCapture/TryEdit` 绑定当前附着组件；`RuntimeNpcStore.TryRelease/ReleaseParentAndChildren/RemoveOwnedRuntimeEntities` 在借用检查之后、实体终止之前结束 task。7 条真实 Program 场景（task-probe、Guide success/blocked/invalid 及三个边界）全部 exit 0、`Succeeded=true`、`SourceFileUnchanged=true`；fresh Simulation 构建通过，替代此前"最新 normal build 仍失败"的判断 |

未闭合：真实 `Transform` 提交点、跨世界生命周期闭包、目标/效果的变换清理、source golden、authority/network、保存恢复与删除门禁。`CoverageProfilesPromoted=0`。

## 6. 来源门禁反向验收（1 份）

门禁退出语义验收结论为 **rejected**：用三组独立夹具检验比较器时，

| 夹具 | 期望 | 实测 |
| --- | --- | --- |
| Eye control（零差异） | exit 0 | exit 0 ✓ |
| Eye source `rotationBits` 变异 | 非零 | exit 0 ✗（顶层仍标 no-difference） |
| 空 `cases=[]` | 非零 | exit 0 ✗ |

即"相同 IL、相同输入却出现来源输出矛盾"和"没有任何样本"都不触发失败。修复条件：至少一个有效
场景与实际 tick、来源可信性与 profile 差异分层、pinned-source fallback 的显式来源要求、直接
比较器与 PowerShell `-CompareOnly` 双路夹具、verifier 实际执行程序集与 build-source 指纹关联。
修复后须重跑真实 32/182 对比，不得删除、豁免或重命名差异项后算通过。

## 7. 证据目录

| 主题 | 目录 |
| --- | --- |
| Guide 全部切片 | `Build/diagnostics/NpcAiRedesign/guide-source-profile-20261007/` |
| 来源差分 32/182 | `Build/diagnostics/NpcAiReferenceVerification/20261007T035310365Z/`（负向夹具在 `negative-path/`） |
| 门禁反向验收 | `Build/diagnostics/NpcAiRedesign/runs/source-gate-negative-root-review-20261007/` |
| Slime cooldown 宿主 | `Build/diagnostics/NpcAiRedesign/runs/slime-cooldown-root-review-20261007/` |
| 任务生命周期宿主 | `Build/diagnostics/NpcAiRedesign/runs/task-lifecycle-host-20261007/`、`task-reference-host-20261007/` |
| 任务协议领域 | `Build/diagnostics/NpcAiRedesign/task-termination-20261007/`、`task-reference-operations-20261007/` |
| Eye F1 宿主 | `Build/diagnostics/NpcAiRedesign/stage-f1-host-recovery-20261007/` |

上述目录是可再生的本地诊断产物，不在源码控制内；本账本只记录其路径与对应切片。

## 8. 合并后的未闭合总表

| 项 | 状态 | 归属 |
| --- | --- | --- |
| Guide 通用空闲 tick、普通动画、完整宿主组合 | open | Guide profile 会话 |
| Guide `ai[0]==10` 专用攻击配置 | 无源码分支 | — |
| Slime 来源差分（8 状态 + 11 observable） | failed | production 工作流 |
| B2 普通/WOF 目标评分 | 来源反例已复现，修复 pending | B2 目标 owner |
| 来源门禁拒绝语义 | rejected，待修复重验 | ReferenceVerification 会话 |
| Eye 变身 / 仆从生成 / 完整目标输入 | open | F1 会话 |
| 真实 `Transform` 提交点与跨世界闭包 | open | 生命周期 owner |
| authority/network、save/load/unload、source golden | open | 各 owner |

**本账本不宣称**任何 profile 已完成 Terraria 等价、任何 coverage 行升级，或旧报告的差异已关闭。

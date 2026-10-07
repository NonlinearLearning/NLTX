# NPC AI `aiStyle 0–127` 第二阶段并行实施执行文档

日期：2026-10-07  
上游设计：[NPC AI 系统设计](../../system-decomposition/2026-10-05-npc-ai-system-redesign.md)  
上游计划：[NPC AI 系统执行计划](2026-10-06-npc-ai-system-execution-plan.md)  
第一阶段索引：[0–127 并行分类执行与验收索引](2026-10-07-npc-ai-style-parallel-dispatch.md)  
第一阶段验收：[并行执行与主会话验收报告](2026-10-07-npc-ai-style-parallel-acceptance.md)

状态：active / all-category parallel continuation  
执行主体：7 个既有类别会话，各自保留原类别分支；主会话统一验收和整合  
目标：把第一阶段的 `partial/open` 局部 profile 推进到至少一条真实 owner → profile → effect/lifecycle → host observation 的可追踪实施闭环；不把静态映射、文件存在或局部源检查写成完成。

## 1. 不可变硬约束

1. 七个类别同时推进，不采用“一个类别完成后再开始下一个类别”的串行流程。
2. 每个类别继续使用原类别会话和 `codex/npc-ai-style-` 分支前缀；不得为同一类别创建第二个并行分支来掩盖未完成工作。
3. 子会话开工前必须使用 `/goal`，读取并遵守 `pua` skill；失败两次后必须切换调查/实现路径，并在 Markdown 中记录失败计数、已排除假设和下一假设。
4. NPC AI 决策、目标选择、随机消费、物理/碰撞权威提交、生成、变换、奖励、任务推进、关系修改和权威效果只能在 `netMode=0/2` 执行。
5. `netMode=1` 只能观察服务端复制状态、同步意图和呈现数据。不得在客户端调用 `_aiSystem.Evaluate`、目标选择、随机、profile 决策、movement、physics、collision、natural despawn、task 或 authority spawn。
6. 不修改或削弱 `Test/Terraria.NpcAi.TargetHostVerification/Program.cs` 中的 client/server authority 断言；不恢复任何客户端 AI 路径。
7. 子会话不得执行 build、restore、run 或测试全集。主会话只允许执行约 10% 的核心验证，以及唯一增量构建：

   ```powershell
   dotnet build Test/Terraria.NpcAi.TargetHostVerification/Terraria.NpcAi.TargetHostVerification.csproj --no-restore --nologo -v:minimal
   ```

8. 不使用 `git reset --hard`、`git clean` 或广泛删除；保留工作树既有修改。类别回退必须是文件级、可恢复且写入 Markdown。

## 2. 共同实施闭环

每个类别必须围绕一个真实可执行窄切片完成以下闭环，不能只提交搜索报告：

```text
固定来源 identity
  -> profile input/state/result
  -> server-only owner adapter
  -> effect/lifecycle commit contract
  -> real host call site or explicit integration seam
  -> focused source/runtime evidence
  -> open-gap and rollback record
```

### 2.1 Identity 与来源

- 精确记录 `type`、`netId`（包括负变体）、`aiStyle`、激活条件和来源文件/行号。
- 共享 `aiStyle` 不得被当成共享 profile；每个 `(type, netId, aiStyle)` 都必须独立校验。
- 不确定的 helper、动态调用、事件订阅、被清空函数体或 catalog 缺项标记为 `unknown/open`。

### 2.2 状态归属与 API

- `Component` 只保存实体状态；profile 只做纯决定/规则计算；最终状态和副作用由领域 owner 提交。
- Query 必须先检查时钟、随机、缓存、事件和别名写入；不按方法名推断纯度。
- Command/effect port 必须说明最终写入者、幂等性、失败结果和提交顺序。
- Adapter 只能转换身份、输入和结果，不能偷偷实现第二套 AI 或代替 owner。

### 2.3 更新顺序与生命周期

至少写出一条可追踪顺序，例如：

```text
authority gate
  -> capture input snapshot
  -> target/query snapshot
  -> profile decision
  -> owner state commit
  -> effect/network intent commit
  -> movement/physics/collision visibility
  -> lifecycle cleanup or replication observation
```

若来源顺序未证实，必须写 `unknown`，不能用文件顺序代替运行时顺序。

### 2.4 交付与证据

类别 Markdown 必须更新并包含：

- 本阶段选定的精确 identity 和 style 子集；
- 状态 owner、行为模块、任务/生成/死亡生命周期、API 和更新顺序；
- server-only/client observation-only 证据；
- 真实改动文件和不改文件清单；
- focused/no-build 源检查或既有可复用运行证据，实际 exit code；
- 未运行 build/restore/run/test 的原因；
- `indexed → mapped → implemented → verified` 当前状态，不满足条件时保持 `partial/open`；
- 精确回退文件范围；
- PUA 失败计数、自检和下一阶段缺口。

## 3. 类别并行工作包

以下工作包在同一时间窗口启动，互不等待。每个子会话先阅读自己的第一阶段 Markdown，再执行本节对应的真实切片。

### 3.1 ground-jump

范围：`1,3,8,10,13,19,20,22,23,25,26,39,40,41,50,56,66,67,98,100,101,102,103,107`。

优先实施：从现有 Fighter/Slime/Wolf 调查中选择一个来源 identity 已能闭合的地面/跳跃 profile，优先完成 `type/netId/aiStyle` 精确 predicate、障碍/门/台阶输入、跳跃阈值和实际 owner commit。若 style 26 Wolf 仍未闭合，至少将一个真实跳跃分支接到类别 adapter 的 production seam，并保留 `RuntimeNpcStore` 公共入口由主会话整合。

必须证明：落地/跳跃状态变化、碰撞输入、目标输入、authority guard、跳跃后 movement 提交顺序；不得把通用 `aiStyle=3` fallback 作为 Wolf 或其它 identity 的完成证据。

### 3.2 air-water

范围：`2,5,9,14,16,17,18,21,24,44,49,63,70,72,74,80,85,86,91,95,96,99,108,113,118,119`。

优先实施：完成 Eater of Souls `type=6/netId=6/aiStyle=5` 的碰撞/水空环境切片，或 WaterStrider `type/netId=612,613/aiStyle=116` 的湿态、水线、跳跃和随机输入切片；只能选择来源和输入已经被源码证据支持的一条作为本阶段闭环。

必须证明：`oldVelocity`、`collideX/collideY` 或水线/湿态事实如何进入 profile，反弹/跳跃结果由谁提交，client 不重复计算；不把环境快照默认值当成来源事实。

### 3.3 town-rescue

范围：`0,7,42,124,125,127`。

优先实施：将 Lost Girl `type=195/netId=195/aiStyle=42` 的第 21 tick 变换意图，或 Guide `type=22/netId=22/aiStyle=7` 的一个真实任务/回家 owner 组合接入明确的 server-only effect/lifecycle seam。已有 Guide task 证据必须区分有限任务闭环与完整城镇 AI。

必须证明：变换/任务完成只提交一次、失败/阻塞可观察、旧引用失效或生命周期清理明确、client 只观察复制意图；不把 `aiStyle=0` catalog 分支写成 Guide/Old Man 的来源等价。

### 3.4 bio-environment

范围：`64,65,68,112,114,115,116`。

优先实施：完成 WaterStrider 的精确身份 profile（至少一个 type），闭合水线、湿态、跳跃和随机 port 的输入/结果；若 612/613 必须共用规则，仍需分别注册 identity 并记录差异。

必须证明：环境 Query 的时序、随机消费位置、movement commit owner、network intent 和 client observation 边界；不将“能在水上移动”的局部规则升级为完整环境 AI。

### 3.5 single-boss-special

范围：`4,15,43,45,51,54,57,58,60,61,69,93,97,110,117,120,121,122,123`。

优先实施：继续 Eye of Cthulhu `type=4/netId=4/aiStyle=4` 的一个尚未闭合阶段（目标输入、变身、Servant、网络/效果顺序），或者选择来源证据最完整的另一个单体 Boss identity。禁止复制第二套 Eye owner。

必须证明：阶段状态、阈值、目标/随机输入、效果 port、Servant/变身 authority、重入/重复提交和 cleanup 顺序；客户端不得预测 Boss AI 或 Servant 生成。

### 3.6 segmented-multipart

范围：`6,11,12,27,28,29,30,31,32,33,34,35,36,37,46,47,48,52,53,55,59,71,75,76,77,78,79,81,82,84,88,89,90`。

优先实施：完成 style 6 Wyvern 的一个真实链级生命周期切片：根生成、14 节创建顺序、`ai[0]/ai[1]/ai[3]/realLife` 投影、父子关系 owner、根死亡整链释放或容量失败回滚。单条 `TryPrepareAttachment` 不能作为链级完成证据。

必须证明：generation/slot/handle 代际、邻接合法性、shared life、detach/release、stale reference 拒绝、server-only 关系修改；client 只观察已复制链状态。

### 3.7 encounter-event

范围：`38,62,73,83,87,92,94,104,105,106,109,111,126`。

优先实施：完成 TargetDummy `type=488/netId=488/aiStyle=92` 的 owner 接线或一个来源证据更闭合的 encounter/event profile；若 TargetDummy catalog 仍以 style 0 存在，必须先记录 identity mismatch，再提供明确的映射修复或保持 open，不得强行别名。

必须证明：事件/波次/TileEntity/NPC owner 的写入边界、authority、重复触发/幂等、失败恢复和网络呈现；不得把 DD2 事件、奖励或波次状态放进通用 NPC profile。

## 4. 主会话验收顺序

主会话不按类别串行等待；在各会话独立推进期间，主会话只做以下并行可重复的验收准备：

1. 读取每个类别 Markdown 的最新状态和改动清单；
2. 检查七类 identity 不重叠且仍覆盖 `0–127`；
3. 检查每个新 profile 的 `CanHandle` 三元 identity predicate；
4. 检查所有 client 分支是否在统一 gate 之前或 profile 输入层被拒绝；
5. 检查共享 `RuntimeNpcStore`、TargetHost Program、central coverage ledger 是否被类别分支越权修改；
6. 只在主会话执行唯一 TargetHost 增量构建，并读取 client/server authority 断言；
7. 只有真实 owner 接线、行为证据和 open work 全部闭合后，才允许主会话把 profile 从 `mapped/open` 推进；否则保留 `partial/open`。

## 5. 回退与停止条件

- 类别失败只回退该类别本阶段新增文件和对应 Markdown 段落；不回退全局 authority gate、TargetHost 断言或其它类别。
- 发现客户端 AI、共享入口双写、identity 冲突、无来源默认值或无法证明的动态 helper 时，立即停止该切片的 promotion，记录 `open/unknown` 并保留可恢复代码。
- 如果代码无法在不修改共享入口的情况下形成真实闭环，必须交付最小 integration seam 和主会话接线补丁说明，不能以“只做文档”结束。
- 本阶段仍不能关闭完整 `0–127` goal；只有所有必要 profile 的真实 runtime、authority、生命周期、来源差分、网络/存档/卸载和删除门禁完成后，主会话才可进行最终 completion audit。

## 6. 并行会话恢复记录

本阶段仍保持一个类别一个分支的并行模型。原 `air-water` rollout 在 Codex 状态中已不可读取；主会话没有创建第二个 air-water 分支，而是恢复原分支 `codex/npc-ai-style-air-water`，在以下隔离 worktree 中启动替代 Luna 6 Max 会话：

```text
C:\Users\shan\.codex\worktrees\npc-ai-style-air-water-p2\NLTX
```

该替代会话只拥有原 air-water 分支的恢复路径，所有命令必须显式使用该 worktree；它不允许写入主工作树，也不改变 `aiStyle 0–127` 的类别归属。其它需要恢复的类别使用同样的原分支恢复 worktree：

```text
C:\Users\shan\.codex\worktrees\npc-ai-style-town-rescue-p2\NLTX
C:\Users\shan\.codex\worktrees\npc-ai-style-bio-environment-p2\NLTX
C:\Users\shan\.codex\worktrees\npc-ai-style-single-boss-special-p2\NLTX
```

恢复和替代只解决隔离执行环境，不提升任何 coverage 状态；最终仍以类别 Markdown、真实 owner 接线和主会话证据为准。

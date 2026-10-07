# NPC AI `aiStyle 0–127` Phase 3 并行执行文档

日期：2026-10-07  
上游设计：[NPC AI 系统设计](../../system-decomposition/2026-10-05-npc-ai-system-redesign.md)  
上游计划：[NPC AI 系统执行计划](2026-10-06-npc-ai-system-execution-plan.md)  
Phase 1 验收：[并行分类验收](2026-10-07-npc-ai-style-parallel-acceptance.md)  
Phase 2 约束：[Phase 2 并行执行](2026-10-07-npc-ai-style-phase2-parallel-execution.md)

状态：active / all-category parallel host-integration continuation  
执行主体：7 个既有类别会话，继续使用原类别分支；主会话统一审查、构建和验收  
目标：把 Phase 2 的 `partial/open` seam 推进到至少一条真实 host call site、owner commit 或可复用 lifecycle contract，并以来源可追踪的证据证明 client 不执行 NPC AI。未满足证据的条目继续保持 `partial/open`。

## 1. 不可变硬约束

1. 七类必须同时推进。不得等待一个类别完成后再启动另一个类别，也不得以新增第二个同类别分支掩盖未完成工作。
2. 每类继续使用原会话和统一前缀 `codex/npc-ai-style-` 的原分支；恢复 worktree 若为 detached，只能记录该事实并在安全时恢复对应分支，不能在错误分支冒充交付。
3. 子会话每次 continuation 都必须使用 `/goal`，读取并遵守 `pua` skill；连续失败时必须记录失败计数、失败命令、排除的假设和下一条调查路径。
4. 服务器/单机（`netMode=0/2`）才可执行目标选择、随机、AI profile、任务、生成/变换/奖励、movement、physics、collision、contact damage、network/save/load/unload/delete owner。`netMode=1` 只能消费复制状态、同步意图和呈现数据。
5. client 绝不能调用 `_aiSystem.Evaluate`、目标选择、随机端口、profile decision、movement/physics/collision、natural despawn、task commit 或 authority spawn。发现任何 client side effect 时，立即停止 promotion，记录 `open`。
6. 不修改或削弱 `Test/Terraria.NpcAi.TargetHostVerification/Program.cs` 的现有 client/server 断言；不得运行 TargetHost 程序来规避“只构建”限制。
7. 子会话不得执行 build、restore、run 或测试；主会话只允许约 10% 核心源级审计和以下唯一增量构建：

   ```powershell
   dotnet build Test/Terraria.NpcAi.TargetHostVerification/Terraria.NpcAi.TargetHostVerification.csproj --no-restore --nologo -v:minimal
   ```

8. 不使用 `git reset --hard`、`git clean` 或广泛删除；不覆盖主树既有用户修改。所有回退必须按文件列出并可恢复。

## 2. Phase 3 的强制实施闭环

每个类别必须选择一个可以在当前源码中闭合的窄切片，并依次证明：

```text
source identity + provenance
  -> input/query snapshot
  -> server-only profile/adapter
  -> owner state commit
  -> effect/network/lifecycle commit
  -> actual host call site or explicit one-line integration seam
  -> client negative-path source evidence
  -> stale/deletion/save/unload boundary
  -> open-gap + file-level rollback
```

“实际 host call site”可以是最小、可复用的生产入口；若共享入口不安全地属于主会话，类别必须提交窄 seam、精确接线点和参数合同，不能只提交静态 profile。任何 profile 若没有 host 调用或可审查 seam，仍为 `mapped/open`。

### 2.1 证据等级

- `indexed`：仅有 style 或来源索引。
- `mapped`：三元 identity 和 profile 输入/结果已明确，但没有真实 owner 接线。
- `implemented`：真实 server owner 已调用 profile 并提交状态/效果，client negative path 已被源级证明，生命周期和回退合同齐全。
- `verified`：在允许的核心验证范围内有编译/运行或可信来源差分证据；静态文件存在、hash、`git diff --check` 或子会话自报不能单独升级状态。

Phase 3 子会话不得直接修改 central coverage ledger 的 promotion 字段；主会话在所有证据齐全后才决定是否提升。

## 3. 七类并行工作包

以下工作包必须同一时间窗口启动，各类别不互相等待。

### 3.1 ground-jump（24 styles）

styles：`1,3,8,10,13,19,20,22,23,25,26,39,40,41,50,56,66,67,98,100,101,102,103,107`。

优先对象：Wolf `type=155/netId=155/aiStyle=26`，或来源更闭合的 Fighter/Slime ground branch。

必须完成：

- 解决 Wolf 是否存在于 AI registry 和 default catalog 的可达性；不允许在未注册行为上提交 movement；
- 在 `ActiveNpcTickPhase`、natural spawn、contact damage 和 `RuntimeNpcStore.Update` 之间确认同一 authority gate，不能只 gate store 内部；
- 显式捕获目标快照、grounded/障碍/距离/速度输入，在来源顺序点计算 jump intent；
- owner 负责提交 velocity，再由既有 gravity/tile-collision owner 继续处理；profile 不保存跨 tick 状态；
- client 源路径必须在自然生成、AI、target、movement、physics、collision 和 contact damage 前返回，只允许 observation trace。

交付：更新 Ground/Jump Markdown，记录真实 registry/catalog 文件、host call site、静态检查 exit code、未编译事实和精确回退。若 blocker 不能安全修复，保留 `partial/open`，不要用 generic `aiStyle=3` fallback 伪装 Wolf。

### 3.2 air-water（26 styles）

styles：`2,5,9,14,16,17,18,21,24,44,49,63,70,72,74,80,85,86,91,95,96,99,108,113,118,119`。

优先对象：Eater of Souls `6/6/5`。只有在水线事实已有来源和宿主 Query 时，才切换到 WaterStrider `612/613/116`。

必须完成：

- 将 `oldVelocity`、`collideX/collideY`、速度下限和 `0.4` 反弹结果接到真实碰撞 owner；
- 核实 type 6 在当前 catalog/host spawn 路径是否可达；不可达时交付明确 integration seam，不宣称运行时闭环；
- client 不执行 profile、随机、碰撞或 network commit，只观察复制速度和同步意图；
- 记录完整来源指纹、effect/network intent 顺序、失败和回退。

### 3.3 town-rescue（6 styles）

styles：`0,7,42,124,125,127`。

优先对象：Guide `22/22/7` 的真实回家 owner，或 Lost Girl `195/195/42` 第 21 tick transform。

必须完成：

- task reference、generation、一次性完成、阻塞/失败、过期引用和清理顺序；
- server-only 变换/回家 effect commit；client 只观察复制意图；
- 明确 shared host 仍走旧直接逻辑还是已调用新 owner；未接线不得写 `implemented`；
- 不把 local catalog `aiStyle=0` 直接当来源 `aiStyle=7/42`。

### 3.4 bio-environment（7 styles）

styles：`64,65,68,112,114,115,116`。

优先对象：WaterStrider 612 与 613 分别保留 identity。

必须完成：

- 接入真实水线/湿态 Query 或将缺失事实明确标为 `unknown/open`；
- 证明 Query → profile → velocity/ai state → network intent 的顺序和唯一 owner；
- 随机 port 只能在 server/单机调用，并记录 seed/消费位置；
- client 只呈现服务端结果，不预测水上移动或跳跃。

### 3.5 single-boss-special（19 styles）

styles：`4,15,43,45,51,54,57,58,60,61,69,93,97,110,117,120,121,122,123`。

优先对象：继续 Eye `4/4/4`，只接一个未闭合阶段，不创建第二套 owner。

必须完成：

- 目标、阶段、阈值、变身、Servant 和效果 network intent 的来源顺序；
- 同一 tick/re-entry 幂等、Servant/变身 cleanup、死亡和卸载；
- client 在目标/随机/AI/Servant 前返回，不能预测 boss 阶段；
- 若只能补 authority/lifecycle seam，保留 `partial/open`。

### 3.6 segmented-multipart（33 styles）

styles：`6,11,12,27,28,29,30,31,32,33,34,35,36,37,46,47,48,52,53,55,59,71,75,76,77,78,79,81,82,84,88,89,90`。

优先对象：Wyvern `aiStyle=6` 15 节链。

必须完成：

- 真实 host spawn/update seam，而非只在 probe 中构造链；
- 根/中段/尾部的 slot、handle、generation、`realLife`、`ai[0]/ai[1]/ai[3]` 和邻接 owner；
- 根死亡整链释放、容量失败逆序回滚、中段 detach、stale reference 拒绝和 slot 复用；
- client 只能观察已复制链，不修改关系或释放 owner。

### 3.7 encounter-event（13 styles）

styles：`38,62,73,83,87,92,94,104,105,106,109,111,126`。

优先对象：TargetDummy `488/488/92`。

必须完成：

- 保持本地 catalog `488/488/0` 与来源 `488/488/92` 的 mismatch/open，除非有来源证据支持 catalog 修复；
- 事件/TileEntity/NPC owner 的 admission、幂等、重复请求、部分失败和 cleanup；
- client 路由只能 observation，不构造 TileEntity/NPC，不提交 wave/reward/event state；
- 不把 event/wave/reward 逻辑塞进通用 NPC profile。

## 4. 子会话 Markdown 必须包含

每个类别执行文档追加 Phase 3 小节，并明确：

1. 精确 identity、来源文件、行号/指纹和当前 coverage stage；
2. 状态 owner、输入 Query、行为模块、任务/生成/变换/死亡/卸载生命周期；
3. server-only 顺序和 client negative-path 顺序；
4. 实际修改文件、明确不修改文件、host call site 或 seam 的签名；
5. 每条 focused source check 的实际 exit code；
6. 未运行 build/restore/run/test 的原因，不能写成编译或运行通过；
7. open/unknown、来源差分、network/save/load/unload/deletion 缺口；
8. 精确文件级 rollback；
9. PUA 失败计数、自检和下一批建议。

## 5. 主会话并行验收

主会话在七类仍运行时只做读操作：检查分类互斥、`CanHandle` 三元 predicate、client 禁止调用、TargetHost 文件未被类别修改、worktree 变更范围和 Markdown 完整性。所有类别终态后，主会话才执行一次唯一 TargetHost 增量构建，读取源码 authority 断言，并把结果追加到统一验收 Markdown。

任何一个类别发现 client side effect、identity 冲突、无来源默认值、shared-entry 双写、未经证明的运行时结论，均不得 promotion；只写 `partial/open` 和精确回退。

## 6. 完成门禁

Phase 3 不关闭完整 goal。只有以下证据全部齐全，主会话才可进入最终 completion audit：

- 128 style 均有唯一来源 identity 和完整 profile/handler 映射；
- 每一条被提升的 profile 都有真实 server owner、状态提交、效果/网络/生命周期闭包；
- client 只观察复制状态，且有 client negative-path 证据；
- 来源 differential、network、save/load/unload、自然 despawn、删除和关系/任务门禁完成；
- coverage ledger 的 `implemented/verified` 与实际 evidence 一致；
- 唯一 TargetHost 增量构建通过，且 client/server authority 断言仍在；
- 每类都有可执行回退，不影响其它类别和主树既有修改。


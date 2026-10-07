# NPC AI `aiStyle 0–127` 并行执行与主会话验收报告

日期：2026-10-07
范围：`docs/system-decomposition/2026-10-05-npc-ai-system-redesign.md` 与 `docs/plans/system-decomposition/2026-10-06-npc-ai-system-execution-plan.md`
验收主体：主会话
状态：并行批次已验收；完整 `0–127` 来源等价迁移仍未完成，继续保持 `partial/open`。

## 1. 验收结论

7 个互斥类别已同时派出 Luna 6 Max 子会话，统一使用 `codex/npc-ai-style-` 分支前缀；每个类别均产生了执行 Markdown 和一个实际窄切片，而不是只做检索。主会话已收齐类别范围、来源边界、状态 owner、生命周期、API、更新顺序、authority、验证和回退记录，并将 7 份类别 Markdown 保存到主树。

客户端运行 AI 的共享入口缺口已经在主会话关闭：

- `RuntimeNpcStore.Update` 在任何 NPC AI、目标、随机、自然 despawn、移动、物理和碰撞之前 fail-closed；
- `ActiveNpcTickPhase` 在自然生成和 NPC 接触伤害之前 fail-closed；
- `RuntimeNpcNaturalSpawnPass.Update` 和 `RuntimeNpcStore.TrySpawn` 拒绝 client/未知模式的权威生成；
- client 只保留观察性 authority trace，不构造 `NpcAiInput`，不选择目标、不消费随机、不推进 AI/movement/task/physics、不创建权威实体；
- `Test/Terraria.NpcAi.TargetHostVerification/Program.cs` 的 client/server authority 断言已增强并保持。

这只关闭了宿主 authority 边界，不等于 128 个 style 已完成来源等价迁移。类别窄 profile 大多尚未接入生产宿主，不能把“7 个并行会话完成”解释为“0–127 已完成”。

## 2. 完整互斥分类

| 类别 | style 数 | style 清单 | 分支 | 会话 | 类别执行文档 |
| --- | ---: | --- | --- | --- | --- |
| ground-jump | 24 | `1,3,8,10,13,19,20,22,23,25,26,39,40,41,50,56,66,67,98,100,101,102,103,107` | `codex/npc-ai-style-ground-jump` | `01a115c8-c732-7a42-91e2-9d7fdaa6cb60` | [ground-jump](2026-10-07-npc-ai-style-ground-jump-execution.md) |
| air-water | 26 | `2,5,9,14,16,17,18,21,24,44,49,63,70,72,74,80,85,86,91,95,96,99,108,113,118,119` | `codex/npc-ai-style-air-water` | `01a115c8-cb96-7912-8069-8bbfc811b736` | [air-water](2026-10-07-npc-ai-style-air-water-execution.md) |
| town-rescue | 6 | `0,7,42,124,125,127` | `codex/npc-ai-style-town-rescue` | `01a115c8-cbab-7bf2-8d0c-6c1a7e011cc1` | [town-rescue](2026-10-07-npc-ai-style-town-rescue-execution.md) |
| bio-environment | 7 | `64,65,68,112,114,115,116` | `codex/npc-ai-style-bio-environment` | `01a115c8-d703-7bb2-a6d6-2fc42580d5ec` | [bio-environment](2026-10-07-npc-ai-style-bio-environment-execution.md) |
| single-boss-special | 19 | `4,15,43,45,51,54,57,58,60,61,69,93,97,110,117,120,121,122,123` | `codex/npc-ai-style-single-boss-special` | `01a115c8-e60c-7612-9ea2-35b4d18df768` | [single-boss-special](2026-10-07-npc-ai-style-single-boss-special-execution.md) |
| segmented-multipart | 33 | `6,11,12,27,28,29,30,31,32,33,34,35,36,37,46,47,48,52,53,55,59,71,75,76,77,78,79,81,82,84,88,89,90` | `codex/npc-ai-style-segmented-multipart` | `01a115c8-f657-77f1-8dc9-f22c898efd6c` | [segmented-multipart](2026-10-07-npc-ai-style-segmented-multipart-execution.md) |
| encounter-event | 13 | `38,62,73,83,87,92,94,104,105,106,109,111,126` | `codex/npc-ai-style-encounter-event` | `01a115c8-f655-7060-88a3-8e5ced07f887` | [encounter-event](2026-10-07-npc-ai-style-encounter-event-execution.md) |

分类审计结果：128 个 style、128 个不同值、0 重复、0 遗漏。

## 3. 并行类别窄切片验收

| 类别 | 实际窄切片 | 主会话判定 |
| --- | --- | --- |
| ground-jump | type=3 Fighter authority adapter；并行文档还明确 Blue/Mother Slime 为有限既有 profile | `partial/open`；未接入共享宿主 |
| air-water | Eater of Souls collision：`type=6/netId=6/aiStyle=5`，保留 `oldVelocity`、`collideX/Y` 和 `0.4` 反弹 | `partial/open`；类别 verifier 未运行 |
| town-rescue | Lost Girl：`type=195/netId=195/aiStyle=42`，权威端第 21 tick 返回 `Transform(196)` 意图 | `partial/open`；未接入生产宿主 |
| bio-environment | WaterStrider：`type/netId=612`、`613`，`aiStyle=116`，水线/湿态/跳跃/随机 port | `partial/open`；未接入生产宿主 |
| single-boss-special | 20 个精确 identity 的 authority adapter；修正 style 97/110 等来源映射 | `partial/open`；未接入完整 boss profiles |
| segmented-multipart | Wyvern root/body/legs/tail identity、handle、legacy parent/realLife 和邻接关系校验 | `partial/open`；未接入 multipart runtime |
| encounter-event | TargetDummy：`type/netId=488/aiStyle=92`，返回 NPC/TileEntity 停用意图 | `partial/open`；本地 catalog style 差异未闭合 |

所有窄切片都坚持“profile 返回决定，owner 提交副作用”；没有新增客户端 AI。类别文档中标记为 `mapped/open`、`indexed/open` 的条目不得在下一批之前被登记为 `implemented/verified`。

## 4. 主会话 authority gate

新增：[NpcAiAuthorityGate.cs](/D:/TRbackup/NLTX/src/NSSLC/Component/Npc/NpcAiAuthorityGate.cs)

允许执行 AI 的模式仅为：

- `0`：single-player authority；
- `2`：multiplayer server authority；
- `1`：client observation-only；
- 其他值：fail-closed。

接线文件：

- [ActiveNpcTickPhase.cs](/D:/TRbackup/NLTX/src/NSSLC.Tools.Simulation/ActiveNpcTickPhase.cs)
- [RuntimeNpcNaturalSpawnPass.cs](/D:/TRbackup/NLTX/src/NSSLC.Tools.Simulation/RuntimeNpcNaturalSpawnPass.cs)
- [RuntimeNpcStore.cs](/D:/TRbackup/NLTX/src/NSSLC.Tools.Simulation/RuntimeNpcStore.cs)

client 分支的唯一允许行为是读取/观察现有复制实体和记录 authority observation trace。它不会调用 `_aiSystem.Evaluate`、不会进入 `UpdateMovement`、不会推进 `AdvanceDamageTrackingTo`、自然 despawn、target selection、随机、任务、movement/physics/collision、contact damage 或 authority spawn。

## 5. TargetHost authority 断言

验证源：[Program.cs](/D:/TRbackup/NLTX/Test/Terraria.NpcAi.TargetHostVerification/Program.cs)

client `netMode=1` 断言包括：

- active NPC 数量不增加，不生成 Servant；
- AI state 每个字段不变化；
- movement/collision snapshot 不变化；
- behavior action 和 last-updated tick 不变化；
- 保留 `ServantSpawnSkipped:ClientAuthority` 观察 trace。

server `netMode=2` 断言包括：

- active NPC 数量增加；
- `ServantSpawned(slot=...)` authority trace 存在。

TargetHost `Program.cs` 当前 SHA-256：

```text
E49627212B4F8B6661D71187DC3F294F399EC4791CA8CFA52A816658E28996A9
```

主会话源级顺序检查：`exit 0`。检查确认 `NpcAiAuthorityGate` 位于 `RuntimeNpcStore.Update` 的 `AdvanceDamageTrackingTo`、`UpdateMovement` 和 `_aiSystem.Evaluate` 之前，位于 `ActiveNpcTickPhase` 的自然生成和接触伤害之前，并确认 client/server authority 字符串全部存在。

## 6. 唯一允许的增量构建

只执行了用户允许的命令：

```text
dotnet build Test/Terraria.NpcAi.TargetHostVerification/Terraria.NpcAi.TargetHostVerification.csproj --no-restore --nologo -v:minimal
```

结果：`exit 0`，无 error；仅观察到仓库既有非阻塞 warning，未执行 solution build、restore、完整 Simulation build、测试全集或 TargetHost 程序运行。

产物：

```text
D:/TRbackup/NLTX/Build/bin/Terraria.NpcAi.TargetHostVerification/Debug/net10.0/Terraria.NpcAi.TargetHostVerification.dll
SHA-256: FBC957ED55EFB503172310CBB38C86BC4C66D7CB2FB26E488716FBFF58E02072
```

第一次重建在断言实现阶段暴露了 `NpcAiStateComponent` 无 `==` 运算符的测试编译错误；主会话改成逐字段断言后，使用同一条唯一允许命令重建通过。该错误已闭合并记录，不把失败隐藏为环境问题。

## 7. 回退要求

类别回退只能删除对应类别新增的窄 profile/adapter/verifier/Markdown，不得：

- `git reset --hard`；
- `git clean`；
- 删除用户既有工作树修改；
- 回退公共 TargetHost authority 断言；
- 让 client 重新进入 AI/物理/生成路径。

主会话 gate 的回退必须作为单独架构变更处理，因为它是跨类别的唯一 authority 边界，不能被任何类别分支局部回退。

## 8. 未关闭项目

- 128 个 style 的完整来源 golden、type/netId/profile identity 和 helper 闭包；
- profile → runtime handler 的生产接线和 coverage ledger `implemented/verified` 门禁；
- 网络复制、保存/卸载、自然 despawn、变换、奖励、关系和任务生命周期的完整端到端证明；
- server/client 实际联机运行验证；
- 各类别窄 profile 的 focused runtime verifier；
- 完整来源 differential、网络 golden、coverage 统计和删除门禁。

因此本报告可以验收“并行拆分、类别文档、窄切片和客户端 AI 宿主门禁”，但不能宣称完整 NPC AI `0–127` 迁移已完成。

## 9. 主树最终文档与 authority 源审计（2026-10-07）

主会话对最终主树重新执行了只读审计，没有扩大验证范围：

- 7 份 canonical 类别 Markdown 均存在；每份都包含 `partial`/`open` 状态、server/authority 边界、client 说明和精确回退范围；类别交付契约检查 35 项通过。
- 7 行分类表的声明数量与实际列表逐行一致；合计 128、distinct 128、遗漏 0、重复 0。此前 `segmented-multipart` 的表格数量笔误已修正为 33，实际 style 集合未发生变化。
- `NpcAiAuthorityGate`、`RuntimeNpcStore`、`ActiveNpcTickPhase`、`RuntimeNpcNaturalSpawnPass` 与 TargetHost verifier 的 authority 源检查 11 项通过。
- TargetHost verifier 保持 client `netMode=1` 与 server `netMode=2` 两条断言，当前 `Program.cs` SHA-256 仍为 `E49627212B4F8B6661D71187DC3F294F399EC4791CA8CFA52A816658E28996A9`。
- 本次最终审计没有执行 `dotnet build`、`restore`、程序运行或全量测试；唯一允许的 TargetHost 增量构建结果仍以第 6 节为准。

覆盖账本最新已记录基线仍为：128 个 style 行，`124 indexed / 4 mapped / 0 implemented / 0 verified`；10 个 `mapped/open` profile、6 个 finite handler、31 个断言，completion gate 未关闭。最新来源差分记录为 32 个场景 / 182 ticks，state 与 velocity 差分为 0，但仍有 11 个 effect/observable 差异，因此 source differential gate 仍失败。这些是账本中已有证据，本节没有重新运行它们，也没有将其提升为完成状态。

最终结论不变：本批完成了 7 类并行拆分、每类 Markdown 交付、局部窄切片记录和 server-only 宿主门禁验收；客户端不运行 NPC AI；完整 `aiStyle 0–127` 来源等价迁移仍为 `partial/open`。

## 10. Phase 2 并行实施最终验收（2026-10-07 当前工作树）

本节是本轮 Phase 2 子会话完成后的主会话独立验收记录，覆盖 7 个并行类别会话。它不覆盖旧的历史快照；若本节与前文的 hash、数量或“已接线”表述不同，以本节的当前工作树证据为准。所有类别改动仍保留在隔离 worktree，未直接复制或合并到 `D:/TRbackup/NLTX` 主树。

### 10.1 会话、分支和 worktree

| 类别 | 会话 | 统一分支 | worktree | 主会话状态 |
| --- | --- | --- | --- | --- |
| ground-jump | `01a115c8-c732-7a42-91e2-9d7fdaa6cb60` | `codex/npc-ai-style-ground-jump`（分支存在；恢复 worktree 当前为 detached） | `C:/Users/shan/.codex/worktrees/a797/NLTX` | `partial/open`；已有静态收束记录，后续 continuation 曾发现 host gate/catalog 缺口，未把其新一轮中间改动验收为完成 |
| air-water | `01a11674-22cc-76f2-b4f5-5a7fdf596941` | `codex/npc-ai-style-air-water`（分支存在；恢复 worktree 当前为 detached） | `C:/Users/shan/.codex/worktrees/npc-ai-style-air-water-p2/NLTX` | `partial/open`；Eater 碰撞切片交付，Water Strider 和宿主注册仍 open |
| town-rescue | `01a115c8-cbab-7bf2-8d0c-6c1a7e011cc1` | `codex/npc-ai-style-town-rescue` | `C:/Users/shan/.codex/worktrees/npc-ai-style-town-rescue-p2/NLTX` | `partial/open`；Guide owner seam 交付，公共宿主接线仍 open |
| bio-environment | `01a115c8-d703-7bb2-a6d6-2fc42580d5ec` | `codex/npc-ai-style-bio-environment` | `C:/Users/shan/.codex/worktrees/npc-ai-style-bio-environment-p2/NLTX` | `partial/open`；612/613 profile seam 交付，水线事实和宿主调用点仍 open |
| single-boss-special | `01a115c8-e60c-7612-9ea2-35b4d18df768` | `codex/npc-ai-style-single-boss-special` | `C:/Users/shan/.codex/worktrees/npc-ai-style-single-boss-special-p2/NLTX` | `partial/open`；Eye authority 早退与实例生命周期 seam 交付，完整来源行为仍 open |
| segmented-multipart | `01a115c8-f657-77f1-8dc9-f22c898efd6c` | `codex/npc-ai-style-segmented-multipart` | `C:/Users/shan/.codex/worktrees/9217/NLTX` | `partial/open`；Wyvern 链级 projection/lifecycle seam 交付，公共 dispatcher/catalog 接线仍 open |
| encounter-event | `01a115c8-f655-7060-88a3-8e5ced07f887` | `codex/npc-ai-style-encounter-event` | `C:/Users/shan/.codex/worktrees/32e1/NLTX` | `partial/open`；TargetDummy mismatch/admission seam 交付，TileEntity/NPC 生产 owner 接线仍 open |

7 个会话均以 Luna 6 Max 运行，并按下发的执行文档要求自行使用 `/goal` 与 `pua`；主会话只负责收束和验收。各类别没有被串行依赖阻塞，均在同一阶段并行推进。Ground/Air 的 detached 状态只表示恢复 checkout 没有检出分支；对应统一前缀分支对象仍存在，未在错误分支上把未提交改动宣称为已合并。

### 10.2 类别交付与不可夸大的证据

| 类别 | 当前交付 | 静态证据 | 明确未完成 |
| --- | --- | --- | --- |
| ground-jump | Wolf `type=155/netId=155/aiStyle=26` profile、input/result、authority adapter 和普通 verifier 源断言 | 8 组 Python source-contract 检查 `exit 0`；`git diff --check` `exit 0` | 当前恢复 continuation 发现 `ActiveNpcTickPhase` 自然生成/contact damage 位于 store guard 外，且 default catalog/AI registry 的 Wolf 可达性仍需修复；未编译/运行 |
| air-water | Eater `type=6/netId=6/aiStyle=5` 碰撞反弹 profile/owner seam，来源 `0.4` 反弹、速度下限、碰撞同步意图 | 3 份来源指纹匹配；`git diff --check` `exit 0`；空白扫描 `exit 0` | 宿主没有已证明的 type 6 注册/生成路径；完整目标/转向/随机/网络以及 Water Strider 未完成 |
| town-rescue | Guide `type=22/netId=22/aiStyle=7` 回家 owner seam，拒绝 client、过期和重复 task reference | 13 项 source-level assertions `exit 0`；空白和 `git diff --check` `exit 0` | Guide 宿主仍走直接逻辑，未调用新 owner；Lost Girl `195/195/42` 未找到生产 profile；未编译/运行 |
| bio-environment | WaterStrider 612 与 613 分别记录 `type/netId/aiStyle=612/612/116`、`613/613/116`，profile/authority/random/owner ports | identity、catalog、client short-circuit、顺序和未接线检查 `exit 0`；`git diff --check` `exit 0` | RuntimeNpcStore 没有 adapter 调用点，缺少水线事实 query；未使用默认值伪装水线；未编译/运行 |
| single-boss-special | Eye `type=4/netId=4/aiStyle=4` 的 authority 早退、同 tick claim、释放清理和 client trace | authority、调用顺序、重入、清理和 whitespace 检查 `exit 0` | 仅为实例/authority seam；完整 Eye 来源输出、网络复制和完整 boss 行为未验证；未编译/运行 |
| segmented-multipart | Wyvern `aiStyle=6` 链级 15 节 `realLife` projection、slot/handle generation、detach、根死亡清理、stale reference 和同槽复用 | source-level authority/identity/lifecycle/signature/format 检查 `exit 0` | 未接默认 catalog、公共 dispatcher、正常 host spawn/update；probe 未编译/运行 |
| encounter-event | TargetDummy 来源三元组 `488/488/92` admission；本地 `488/488/0` 明确 mismatch/open；client observe route | 11 项源码静态断言 `exit 0`；`git diff --check` `exit 0`；TargetHost 文件无 diff | 生产 NPC/TileEntity owner、重复请求和部分失败恢复未接线；未编译/运行 |

所有类别 Markdown 都保留 `partial/open`、实际文件、更新顺序、authority、开放缺口和精确回退范围；没有把静态映射、文件存在、源 hash 或 `git diff --check` 解释为运行时等价。所有类别都禁止 `netMode=1` 调用目标选择、随机、AI 求值、movement/physics/collision、task、natural despawn、contact damage 或 authority spawn；未接入的宿主路径按 open 处理。

### 10.3 主树唯一允许验证

本轮主会话只执行了用户指定的增量构建，命令为：

```text
dotnet build Test/Terraria.NpcAi.TargetHostVerification/Terraria.NpcAi.TargetHostVerification.csproj --no-restore --nologo -v:minimal
```

结果为 `exit 0`、`0 errors`、17 个既有非阻塞 warning；没有执行 restore、solution/full build、Simulation 全量构建、TargetHost 程序运行或测试全集。当前产物为：

```text
D:/TRbackup/NLTX/Build/bin/Terraria.NpcAi.TargetHostVerification/Debug/net10.0/Terraria.NpcAi.TargetHostVerification.dll
SHA-256: 59683F2A107422DBFF4CD536E71F5AFB2B4990E80EED0DEE6F489E049C3FD28E
```

当前主树 `Test/Terraria.NpcAi.TargetHostVerification/Program.cs` SHA-256 为：

```text
55FF9AC25E782C663392C008187690A69E3254246733B6823B4DCA0BD1A6D371
```

源级证据确认 client/server authority 断言仍在：

- `Program.cs:697` 设置 `RuntimeMain.netMode = 1`，随后断言 client 不增加 active NPC、不改变 AI state、movement/collision snapshot、behavior action/last-updated tick，并保留 `ServantSpawnSkipped:ClientAuthority`；
- `Program.cs:746` 设置 `RuntimeMain.netMode = 2`，随后断言 server 能增加 Servant，并存在 `ServantSpawned(slot=...)` trace；
- `NpcAiAuthorityGate.IsAuthoritative` 只接受 `netMode=0` 或 `2`；`ActiveNpcTickPhase`、`RuntimeNpcStore` 和 natural-spawn/authority 路径均以该 gate 为边界，client 只允许 observation trace。

本轮没有运行 TargetHost，因此“断言源码存在且 TargetHost project 编译通过”已验证，“运行时断言通过”仍为 not-run，不能写成完整 runtime acceptance。

### 10.4 当前主会话结论

已验收：

- `aiStyle 0–127` 的 7 类互斥分类仍为 128/128，0 遗漏、0 重复；
- 7 个类别均有独立详细 Markdown、统一前缀分支和并行会话记录；
- 类别 seam 均把状态计算与 owner 副作用提交分开，并保留 server-only/client observation-only 约束；
- 主树 authority gate 与 TargetHost client/server 断言源码仍存在，且唯一允许的 TargetHost 增量构建通过；
- 没有编写客户端运行的 NPC AI，也没有运行完整测试或扩大构建范围。

未验收、因此不能标记 goal complete：

- 128 个 style 的完整来源等价迁移、生产接线和 `implemented/verified` coverage gate；
- 各类别完整任务/变换/网络/save/load/unload/deletion 生命周期和来源 differential；
- Ground 的最新 host/catalog 修复 continuation 尚未产生新的终态报告；
- 各类别窄切片的编译/运行时验证；
- 主树尚未合并任何类别 worktree 改动，避免把未编译/未运行的局部 seam 伪装为全局完成。

因此当前目标继续保持 `active`，本 Markdown 是本轮可交付的主会话验收记录，而不是完整 NPC AI 迁移完成声明。

## 11. Phase 3 并行终态与主会话独立验收（2026-10-08）

本节覆盖七个类别会话的 Phase 3 最终 checkpoint，以及主会话在当前主树执行的独立审计。类别代码没有复制或合并到 `D:/TRbackup/NLTX` 主树；本节取代前文历史快照中的旧 hash、旧 warning 数和“尚未产生终态”描述，但保留历史内容用于追踪。

### 11.1 七类并行终态

| 类别 | 最终 worktree / 分支 | Phase 3 交付 | 主会话判定 |
| --- | --- | --- | --- |
| ground-jump | `C:/Users/shan/.codex/worktrees/a797/NLTX`；detached `96d893a`；分支对象 `codex/npc-ai-style-ground-jump` | Wolf `155/155/26` source-level host wiring、共享 authority helper、phase/store early return、manifest/catalog 注册、12/12 静态审计 | `partial/open`；未编译、未运行；其余 21 个 style 及来源闭包仍缺失 |
| air-water | `C:/Users/shan/.codex/worktrees/npc-ai-style-air-water-p2/NLTX`；`codex/npc-ai-style-air-water`，`96d893a` | Eater `6/6/5` profile/owner seam；保留 `oldVelocity`、碰撞轴和 `0.4` 反弹；type 6 catalog/spawn 不可达 | `partial/open`；WaterStrider、完整 Eater 行为、网络和生命周期未闭合 |
| town-rescue | `C:/Users/shan/.codex/worktrees/npc-ai-style-town-rescue-p2/NLTX`；`codex/npc-ai-style-town-rescue`，`96d893a` | Guide `22/22/7` 回家 task owner 接入真实 host update；过期引用、错误 kind、完成/NoPath 终态 | `partial/open`；Lost Girl、Guide source profile、复制和其他生命周期未闭合 |
| bio-environment | `C:/Users/shan/.codex/worktrees/npc-ai-style-bio-environment-p2/NLTX`；`codex/npc-ai-style-bio-environment`，`96d893a` | WaterStrider `612/612/116` 与 `613/613/116` 精确 identity；真实液体 Query；server-only random/profile/owner 顺序 | `partial/open`；保存/卸载/删除、复制和完整 ContentCatalog 绑定未闭合 |
| single-boss-special | `C:/Users/shan/.codex/worktrees/npc-ai-style-single-boss-special-p2/NLTX`；`codex/npc-ai-style-single-boss-special`，`96d893a` | Eye `4/4/4` 每实例每 tick Servant spawn claim；authority → claim → content → spawn → velocity；死亡/reset/dispose 清理 | `partial/open`；Servant 联动释放、网络、来源差分和完整 Boss 行为未闭合 |
| segmented-multipart | `C:/Users/shan/.codex/worktrees/9217/NLTX`；`codex/npc-ai-style-segmented-multipart`，`96d893a` | Wyvern style 6 的 15 节 identity、spawn/update seam、slot/generation/realLife projection、detach/stale/slot reuse | `partial/open`；默认 catalog/dispatcher/正常 host spawn-update 未接入，probe 未运行 |
| encounter-event | `C:/Users/shan/.codex/worktrees/32e1/NLTX`；`codex/npc-ai-style-encounter-event`，`9d711d3` | TargetDummy `488/488/92` admission/lifecycle seam；`Deactivate` 修正为清绑定并发同步，不删除 TileEntity；14/14 静态断言 | `partial/open`；本地 `488/488/0` mismatch、生产 coordinator/NPC owner、journal/outbox 和网络恢复未闭合 |

最终类别 Markdown 为：

- [ground-jump 执行文档](C:/Users/shan/.codex/worktrees/a797/NLTX/docs/plans/system-decomposition/2026-10-07-npc-ai-style-ground-jump-execution.md)
- [air-water checkpoint](C:/Users/shan/.codex/worktrees/npc-ai-style-air-water-p2/NLTX/docs/migration/ledgers/2026-10-07-npc-ai-air-water-checkpoint.md)
- [town-rescue 执行文档](C:/Users/shan/.codex/worktrees/npc-ai-style-town-rescue-p2/NLTX/docs/plans/system-decomposition/2026-10-07-npc-ai-style-town-rescue-execution.md)
- [bio-environment 执行文档](C:/Users/shan/.codex/worktrees/npc-ai-style-bio-environment-p2/NLTX/docs/plans/system-decomposition/2026-10-07-npc-ai-style-bio-environment-execution.md)
- [single-boss-special 执行文档](C:/Users/shan/.codex/worktrees/npc-ai-style-single-boss-special-p2/NLTX/docs/plans/system-decomposition/2026-10-07-npc-ai-style-single-boss-special-execution.md)
- [segmented-multipart 执行文档](C:/Users/shan/.codex/worktrees/9217/NLTX/docs/plans/system-decomposition/2026-10-07-npc-ai-style-segmented-multipart-execution.md)
- [encounter-event 执行文档](C:/Users/shan/.codex/worktrees/32e1/NLTX/docs/plans/system-decomposition/2026-10-07-npc-ai-style-encounter-event-execution.md)

这些链接指向隔离 worktree 的证据文件，不是主树合并结果。回退只能按各文档记录的文件级范围执行，不能使用 `git reset --hard`、`git clean`，也不能回退主树已有的 authority gate 或 TargetHost 断言。

### 11.2 128 style 分类独立重算

主会话用只读 PowerShell 数组重新计算分类：

```text
ground-jump: 24
air-water: 26
town-rescue: 6
bio-environment: 7
single-boss-special: 19
segmented-multipart: 33
encounter-event: 13
total: 128
distinct: 128
missing: (empty)
duplicates: (empty)
```

分类门禁为 `128 total / 128 distinct / 0 missing / 0 duplicate`。这只证明分类覆盖，不证明每个 style 已有来源等价实现。

### 11.3 主树 authority 与 TargetHost 验收

主树实际 authority gate 是 [NpcAiAuthorityGate.cs](D:/TRbackup/NLTX/src/NSSLC/Component/Npc/System/NpcAiAuthorityGate.cs)：`SinglePlayerNetMode = 0` 与 `ServerNetMode = 2` 才能通过 `IsAuthoritative`；`ClientNetMode = 1` 为 observation-only，未知模式 fail-closed；`RuntimeNpcStore` 与 `ActiveNpcTickPhase` 均调用该 gate。类别 worktree 没有覆盖或削弱主树 gate；类别文档指出的旧基线 client gap 仍是其分支中的 open evidence。

主会话唯一执行的构建命令：

```powershell
dotnet build Test/Terraria.NpcAi.TargetHostVerification/Terraria.NpcAi.TargetHostVerification.csproj --no-restore --nologo -v:minimal
```

结果：`exit 0`、`0 warnings`、`0 errors`。产物位于 `D:/TRbackup/NLTX/Build/bin/Terraria.NpcAi.TargetHostVerification/Debug/net10.0/`，DLL SHA-256 为 `FBD797F3DA034FA95E6CB844098D313334EA8C93AF15D940E1328B2CA9BF863D`；当前 `Program.cs` SHA-256 为 `438398F651604665ADFC054D732D9CF3AACFD38B4BD98B3E5C6C11FFC8D3D6E6`。

独立 authority 源级检查为 `10/10`：gate 的 0/1/2 常量、0/2 allowlist、Store/phase 调用，以及 [TargetHost Program.cs](D:/TRbackup/NLTX/Test/Terraria.NpcAi.TargetHostVerification/Program.cs:697) 的 client `netMode=1`/`ServantSpawnSkipped:ClientAuthority` 和 [Program.cs](D:/TRbackup/NLTX/Test/Terraria.NpcAi.TargetHostVerification/Program.cs:746) 的 server `netMode=2`/`ServantSpawned(slot=...)` 均存在。

没有运行 TargetHost 程序，没有运行完整测试集、Simulation verifier、restore 或其他项目构建；因此仅验证“断言源码存在 + 目标项目增量构建通过”，不宣称运行时断言通过。

### 11.4 client-side AI 禁止项

主会话没有编写客户端运行的 NPC AI。`netMode=1` 不得执行目标选择、随机消费、profile/AI 求值、movement、physics、collision、task、natural despawn、contact damage、authority spawn、变换、奖励、save/load/unload/delete owner 或状态提交；客户端只允许消费已复制状态、同步意图和呈现数据。主树统一 gate 是唯一共享入口边界，类别旧基线缺口不能被主树验收掩盖。

### 11.5 coverage 与完成门禁

coverage ledger 最新基线仍为 `124 indexed / 4 mapped / 0 implemented / 0 verified`；本批没有修改 promotion 字段。来源 differential、network replication/presentation、save/load、unload、natural despawn、删除、变换、奖励、关系和完整任务生命周期仍未全部闭合。七类静态 evidence 不能替代行为门禁。

主会话 goal 继续保持 `active`。本批验收完成的是七类并行长线会话、统一分支前缀、详细 Markdown、互斥分类复核、server-only 主树 gate 和 TargetHost 增量构建；完整 `aiStyle 0–127` 来源等价 NPC AI 迁移尚未完成，不能标记 goal complete。

### 11.6 主会话 PUA/自检记录

主会话源审计首轮有两次输入假设错误：一次把 authority gate 当作根目录文件，一次使用 PowerShell 保留变量 `$Host` 并按字面 `netMode == 0/2` 检查。两次均为只读命令，无文件副作用；随后按 `rg --files` 定位真实文件，改用非保留变量和实际常量实现，最终 `authority_source_checks=10/10`、exit 0。已完成 PUA 七项自检：读完整错误、重新搜索、读取原始源码、核对路径/常量前置条件、反转路径假设、最小化到 gate/TargetHost、切换到精确源级检查。

## 12. Phase 4 并行续接已派发（2026-10-08）

由于完整目标仍未闭合，主会话新建了详细执行包
[2026-10-08-npc-ai-style-phase4-parallel-execution.md](D:/TRbackup/NLTX/docs/plans/system-decomposition/2026-10-08-npc-ai-style-phase4-parallel-execution.md)，并将剩余 identity、handler、host、network、save/load/unload/deletion 和来源差分缺口同时下发到原有七个类别会话。没有创建重复类别分支，也没有向主树复制类别代码。

本轮续接会话：

- ground-jump：`01a115c8-c732-7a42-91e2-9d7fdaa6cb60`
- air-water：`01a11674-22cc-76f2-b4f5-5a7fdf596941`
- town-rescue：`01a115c8-cbab-7bf2-8d0c-6c1a7e011cc1`
- bio-environment：`01a115c8-d703-7bb2-a6d6-2fc42580d5ec`
- single-boss-special：`01a115c8-e60c-7612-9ea2-35b4d18df768`（已从 archived 恢复后续接）
- segmented-multipart：`01a115c8-f657-77f1-8dc9-f22c898efd6c`
- encounter-event：`01a115c8-f655-7060-88a3-8e5ced07f887`

七个会话均已收到对应的详细 Phase 4 工作包并进入 active continuation。子会话继续使用 `/goal` 和
`pua`，只在隔离 worktree 源码/Markdown 范围内工作；不运行 build、restore、run 或 test，不修改
TargetHost、central coverage ledger 或主树。主会话将在七个 Phase 4 checkpoint 收齐后，才重新进行
只读验收和唯一 TargetHost 增量构建。完整目标继续保持 `active`。

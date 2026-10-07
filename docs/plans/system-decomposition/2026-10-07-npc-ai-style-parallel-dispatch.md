# NPC AI style 0–127 并行分类执行与验收索引

文档 ID：DOC-2026-10-07-NPC-AI-STYLE-PARALLEL-DISPATCH
状态：active / parallel execution
基线快照：`96d893ac09a6aced38df3e3ad0233eb09ed75621`
分支前缀：`codex/npc-ai-style-`
主验收工作树：`D:/TRbackup/NLTX`

## 1. 执行规则

本轮把参考 NPC AI 的 `aiStyle 0–127` 划分为 7 个互斥类别。每个类别对应：

1. 一个 style 清单；
2. 一个独立执行 Markdown；
3. 一个独立 Git 分支和 worktree；
4. 一个 Luna 6 Max 新会话；
5. 一个独立实现/最小验证切片；
6. 一个由主会话进行的集成验收。

7 个类别同时派出，不采用“完成一个类别后才进入下一个类别”的串行调度。类别分支从同一
快照派生，避免把当前工作树的既有 NPC AI 迁移内容丢失；快照分支只用于并行协调，不代表
最终接受提交，也不推进 `main`。

## 2. 全局硬约束

- 不编写客户端运行的 AI。NPC AI 决策、目标选择、随机消费、权威物理/碰撞、生成、变换、
  奖励、任务推进、关系修改和权威效果只能在服务端/权威端执行。
- 客户端只能消费服务端复制的状态、同步意图和呈现数据；不得在 client tick 补算 AI、预测
  权威效果、推进任务/关系或创建权威实体。
- 保留并核对 `Test/Terraria.NpcAi.TargetHostVerification/Program.cs` 中的源码断言：
  `netMode=1` 的 client Eye 不生成 Servant，`netMode=2` 的 server Eye 可以生成 Servant，
  并保留对应效果 trace 断言。
- 每个子会话必须先读取根 `AGENTS.md`、`Context/progress.md`、变更/构建/副作用/C# 约束、
  两份 NPC AI 设计/执行文档和 `C:/Users/shan/.agents/skills/pua/SKILL.md`。
- 每个子会话必须维护类别执行 Markdown，记录来源映射、状态 owner、生命周期、API、更新顺序、
  authority、实际变更、验证命令和 exit code、缺口及精确回退范围。
- 只执行约 10% 的核心验证，不跑 solution 全量构建、全量回归或测试全集。主会话只增量构建
  `Terraria.NpcAi.TargetHostVerification`，用于确认当前源码仍包含 client/server authority 断言。
- 未经来源和运行证据证明的条目只能保持 `indexed`/`mapped`/`open`；fallback、固定默认值、
  纯 verifier 自身输出和客户端预测不能伪装成来源等价。
- 不使用 `git reset --hard`、`git clean` 或广泛删除来收口；回退只能针对本类别精确文件/API。

## 3. 互斥完整分类

| 类别 | style 清单 | 分支 | 会话 | 子会话执行文档 |
| --- | --- | --- | --- | --- |
| ground-jump | 1, 3, 8, 10, 13, 19, 20, 22, 23, 25, 26, 39, 40, 41, 50, 56, 66, 67, 98, 100, 101, 102, 103, 107 | `codex/npc-ai-style-ground-jump` | `01a115c8-c732-7a42-91e2-9d7fdaa6cb60` | `docs/plans/system-decomposition/2026-10-07-npc-ai-style-ground-jump-execution.md` |
| air-water | 2, 5, 9, 14, 16, 17, 18, 21, 24, 44, 49, 63, 70, 72, 74, 80, 85, 86, 91, 95, 96, 99, 108, 113, 118, 119 | `codex/npc-ai-style-air-water` | `01a115c8-cb96-7912-8069-8bbfc811b736` | `docs/plans/system-decomposition/2026-10-07-npc-ai-style-air-water-execution.md` |
| town-rescue | 0, 7, 42, 124, 125, 127 | `codex/npc-ai-style-town-rescue` | `01a115c8-cbab-7bf2-8d0c-6c1a7e011cc1` | `docs/plans/system-decomposition/2026-10-07-npc-ai-style-town-rescue-execution.md` |
| bio-environment | 64, 65, 68, 112, 114, 115, 116 | `codex/npc-ai-style-bio-environment` | `01a115c8-d703-7bb2-a6d6-2fc42580d5ec` | `docs/plans/system-decomposition/2026-10-07-npc-ai-style-bio-environment-execution.md` |
| single-boss-special | 4, 15, 43, 45, 51, 54, 57, 58, 60, 61, 69, 93, 97, 110, 117, 120, 121, 122, 123 | `codex/npc-ai-style-single-boss-special` | `01a115c8-e60c-7612-9ea2-35b4d18df768` | `docs/plans/system-decomposition/2026-10-07-npc-ai-style-single-boss-special-execution.md` |
| segmented-multipart | 6, 11, 12, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 46, 47, 48, 52, 53, 55, 59, 71, 75, 76, 77, 78, 79, 81, 82, 84, 88, 89, 90 | `codex/npc-ai-style-segmented-multipart` | `01a115c8-f657-77f1-8dc9-f22c898efd6c` | `docs/plans/system-decomposition/2026-10-07-npc-ai-style-segmented-multipart-execution.md` |
| encounter-event | 38, 62, 73, 83, 87, 92, 94, 104, 105, 106, 109, 111, 126 | `codex/npc-ai-style-encounter-event` | `01a115c8-f655-7060-88a3-8e5ced07f887` | `docs/plans/system-decomposition/2026-10-07-npc-ai-style-encounter-event-execution.md` |

分类审计：共 128 个 style，128 个不同值，0 重复，0 遗漏；style 0 和 127 归入城镇/救援，
分节、多部位与附属实体按关系生命周期单独归类。分类是并行 ownership 边界，不表示任何 style
已经完成来源等价迁移。

## 4. 子会话交付格式

每个类别执行 Markdown 至少包含：

- 本类别 style、来源 `NPC.cs` 分支/helper、具体 `type/netId/aiStyle` 和变体差异；
- 状态 owner、共享定义/实例状态分离、任务生命周期和失效/卸载清理；
- API 组合、目标/物理/生成/关系/网络边界和完整更新顺序；
- server/authority-only 的执行条件，以及客户端仅观察/复制/呈现的边界；
- 实际修改文件和不修改的公共文件；
- 一项最小真实实现或可执行窄适配器，不得只交检索摘要；
- 约 10% 核心验证命令、实际 exit code、warnings/errors、报告路径和输入未变更证明；
- 未闭合缺口、`indexed → mapped → implemented → verified` 状态、失败计数和 PUA 复盘；
- 精确回退文件/API、已提交效果不自动重放的说明，以及主会话最小集成步骤。

## 5. 主会话验收门槛

子会话完成不等于类别关闭，也不等于完整 0–127 目标完成。主会话按以下顺序验收所有并行
交付：

1. 检查每个类别 Markdown 是否存在、内容完整、style 归属无交叉；
2. 检查每个分支的实际 diff，确认没有客户端 AI、没有跨类公共文件争写或伪造 coverage；
3. 按子会话报告选择最小可合并切片，先做源码和 API review，再做主树集成；
4. 只在主会话增量构建 `Test/Terraria.NpcAi.TargetHostVerification/Terraria.NpcAi.TargetHostVerification.csproj`，
   并以当前源码 hash 绑定构建与运行结果；
5. 读取并核验 client/server authority 断言，至少覆盖 client 不生成 Servant、server 可生成
   Servant、effect trace 与 active-count 变化；
6. 任何一个类别存在客户端 AI、来源未闭合、权限不明、公共 owner 冲突或验证缺失，都保持
   `open/partial`，不把并行完成数折算为全量完成率。

## 6. 当前并行状态

7 个子会话均已创建并进入 active；工作树准备路径由 Codex 管理。主会话暂不合并分支，等待
各类别 Markdown 和真实证据后逐项验收。既有其他领域会话继续运行时，NPC 类别只在各自 worktree
写入，不对 `D:/TRbackup/NLTX` 主树做广泛清理。

## 7. 主会话 TargetHost authority 验收（2026-10-07）

### 源码断言

当前主工作树的 `Test/Terraria.NpcAi.TargetHostVerification/Program.cs` 保留以下源码证据：

- `RuntimeMain.netMode = 1` 的客户端场景；
- `ServantSpawnSkipped:ClientAuthority` effect trace；
- 客户端 active-count 不增加；
- `RuntimeMain.netMode = 2` 的服务端场景；
- 服务端 active-count 增加并生成 Servant 的断言；
- 服务端 authoritative Servant effect trace。

当前源码 SHA-256：`E49627212B4F8B6661D71187DC3F294F399EC4791CA8CFA52A816658E28996A9`。

### 主会话唯一 server-only gate

主会话新增 `src/NSSLC/Component/Npc/NpcAiAuthorityGate.cs`，并在以下公共边界使用同一个 gate：

- `RuntimeNpcStore.Update`：在 damage tracking、自然 despawn、目标选择、profile、AI、movement 和 collision 之前拒绝 client/未知模式；
- `ActiveNpcTickPhase`：在自然生成和 NPC 接触伤害之前拒绝 client/未知模式；
- `RuntimeNpcNaturalSpawnPass.Update`：拒绝 client/未知模式的自然生成；
- `RuntimeNpcStore.TrySpawn`：拒绝 client/未知模式的权威实体创建；
- client 分支只记录既有 authority observation trace，不构造 `NpcAiInput`，不消费随机、不推进 AI/任务/物理状态。

TargetHost client fixture 现在还断言：AI state、movement/collision snapshot、behavior action 和 last-updated tick 在 client update 后均不变；server fixture 仍断言 active-count 增长和 `ServantSpawned(slot=...)` trace。旧 `ServantSpawnSkipped:ClientAuthority` 断言保留为防御性 authority observation。

### 唯一增量构建

```text
dotnet build Test/Terraria.NpcAi.TargetHostVerification/Terraria.NpcAi.TargetHostVerification.csproj --no-restore --nologo -v:minimal
```

结果：exit `0`，`0` errors；仅观察到仓库既有非阻塞 warnings。未执行 solution build、全量回归或测试全集。

产物：

```text
D:/TRbackup/NLTX/Build/bin/Terraria.NpcAi.TargetHostVerification/Debug/net10.0/Terraria.NpcAi.TargetHostVerification.dll
SHA-256: CEDA834D66A77540FC39105CDDD5BF5CC3B4AB2264760CC52D6BD933E8C893BB
```

该构建只证明当前 TargetHostVerification 源码可增量编译并包含 authority 断言；它不证明
完整 0–127 style 行为、网络复制、保存/卸载、来源 golden 或客户端 AI gate 已全部闭合。

本轮 gate 接入后的 TargetHostVerification DLL SHA-256：
`FBC957ED55EFB503172310CBB38C86BC4C66D7CB2FB26E488716FBFF58E02072`。

7 个类别 Markdown 已收口到主树 `docs/plans/system-decomposition/`，分别记录并行会话的分支、会话、实际窄切片、authority 证据、未闭合项和精确回退。所有类别仍保持 `partial/open`，未将窄切片折算为 0–127 完成。

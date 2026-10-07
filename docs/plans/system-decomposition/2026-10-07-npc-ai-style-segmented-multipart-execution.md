# NPC AI segmented/multipart 风格类别执行记录

## 目标与边界

- 目标：为指定的 NPC AI 风格范围交付一个真实、可运行的 segmented/multipart 生命周期切片，并记录它接入其余风格的精确步骤。
- 起点快照：`96d893ac09a6aced38df3e3ad0233eb09ed75621`。
- 风格范围：`6, 11, 12, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 46, 47, 48, 52, 53, 55, 59, 71, 75, 76, 77, 78, 79, 81, 82, 84, 88, 89, 90`。
- 本类别重点：Worm / Skeletron / WallOfFlesh / Mechanical / Golem / Plantera / Brain / MoonLord / Cultist / Mothron 的 `SpawnNow → Attach → 立即读取 → 更新/共享生命 → Detach/Release` 生命周期。
- 限制：保留已有用户改动；不触碰公共 `RuntimeNpcStore`、`NpcAiSystem`、`Program.cs`、项目文件及中央 coverage ledger；不做 solution/full regression/full test sweep；不得削弱 client/server authority 断言；不引入 float slot 新身份、fallback 或未经证明的等价映射。
- 全局 authority 硬约束（2026-10-07 用户追加）：NPC AI 决策、目标选择、随机消费、物理/碰撞权威、生成、变换、奖励、任务推进、关系修改和 authority effects 只在服务端/权威端执行。客户端不得通过 client tick 运行/补算 AI、预测权威效果或创建权威实体；客户端只消费服务端复制状态、网络同步意图和呈现数据。现有 client-side 路径必须改为明确的 observe/replication/presentation gate，或记录为未接入。

## 需求变更记录（本类别内）

- 提出人/渠道/日期：用户；当前任务 IM；2026-10-07（Asia/Shanghai）。
- 原文（逐字）：

  > 用户新增全局硬约束，立即纳入当前 goal 和类别执行文档：
  > 1. 不要编写客户端运行的 AI。NPC AI 决策、目标选择、随机消费、物理/碰撞权威、生成、变换、奖励、任务推进、关系修改和 authority effects 只能在服务端/权威端执行；客户端只允许消费服务端复制的状态、网络同步意图和呈现数据，不能通过 client tick 运行 AI、补算 AI、预测权威效果或创建权威实体。
  > 2. 检查你负责范围及现有接线中是否已有 client-side AI 路径；若有，必须改成明确的 client observe/replication/presentation gate，或记录为未接入，不得新增客户端 AI。保留并加强 Test/Terraria.NpcAi.TargetHostVerification 中 client netMode=1 不生成 Servant、server netMode=2 可生成 Servant 的源码断言。
  > 3. 交付必须是 Markdown 文件：继续维护 docs/plans/system-decomposition/2026-10-07-npc-ai-style-<category>-execution.md，并在文件中记录这一条架构约束、实际代码变更、authority 证明、验证命令/exit code、报告路径、未闭合项和精确回退范围。最终回复主会话时优先给出该 md 文件的绝对路径与摘要，不要只发聊天总结。
  > 4. 不扩大测试或构建范围，继续遵守约 10% 核心验证上限。
- 建议分级：重大（把 NPC AI 的权威执行边界提升为全局硬门槛，会影响本类别入口 gate 和现有接线审查）；本轮决定：用户已明确要求立即纳入当前交付，按该范围执行，不另建类别文档或扩大覆盖面。
- 影响评估：不改数据格式、项目/构建策略或公共 NPC owner；需要审计 AI 调用入口与 authority 守卫，必要时只添加本类别窄 gate/适配器；验证范围仍限一个 multipart focused/no-build 切片。Client mode 只复制/观察/呈现，不运行本地决策。
- 主会话边界补充（2026-10-07）：共享入口的统一 client gate 由主会话集成；本类别不在 `RuntimeNpcStore` / 公共 `NpcAiSystem` / host entry 各自实现第二个 gate。原文：

  > 主会话补充边界：如果你发现 RuntimeNpcStore/公共 NpcAiSystem/宿主入口在 client netMode=1 仍会推进通用 AI，不要在各类别分支各自修改同一个公共入口，也不要重复实现多个 client gate。请在类别 Markdown 记录准确调用点、证据和影响；只在本类别文件中实现必要的 server-only profile/authority 适配或 verifier。统一宿主入口 gate 由主会话集成，最终只保留一个可审计的 server-only AI 入口。客户端不得执行 AI 决策、随机、物理、生成、任务推进或权威效果。

## 来源与映射

- 类别设计来源：[NPC AI 重设计](../../system-decomposition/2026-10-05-npc-ai-system-redesign.md) 的 Spawn/关系 owner 与生命周期边界（约第 130–145、279–282 行）；[NPC AI 执行计划](2026-10-06-npc-ai-system-execution-plan.md) §E 将具体虫链、分节及多部位 profile 保持为 open（第 54–55、533–554 行）。风格全集来自 [NPC AI reference index](../../system-decomposition/2026-10-05-npc-ai-reference-index.md)。
- 参考快照采用 `D:/TRbackup/无任何删减通过编译/Terraria/NPC.cs` 与 `D:/TRbackup/无任何删减通过编译/Terraria.ID/NPCID.cs`；不能用被裁剪的 Version4 NPC.cs 证明 AI_006 的实现，因为其方法体为空。

| 实体 | `type` | `netId` | `aiStyle` | 来源状态与证据 |
| --- | ---: | ---: | ---: | --- |
| Wyvern Head | 87 | 87 | 6 | `NPCID.cs:11284`; `NPC.cs:10053–10070` 设置 `aiStyle=6`; `NPC.cs:17934` 将 `netID=type`; `NPC.cs:20991–20995` 分派到 `AI_006_Worms` |
| Wyvern Legs | 88 | 88 | 6 | `NPCID.cs:11286`; `NPC.cs:10071–10088`; 共用 `netID=type` 与 style-6 分派 |
| Wyvern Body | 89 | 89 | 6 | `NPCID.cs:11288`; `NPC.cs:10089–10106`; 共用 `netID=type` 与 style-6 分派 |
| Wyvern Body2 | 90 | 90 | 6 | `NPCID.cs:11290`; `NPC.cs:10107–10124`; 共用 `netID=type` 与 style-6 分派 |
| Wyvern Body3 | 91 | 91 | 6 | `NPCID.cs:11292`; `NPC.cs:10125–10142`; 共用 `netID=type` 与 style-6 分派 |
| Wyvern Tail | 92 | 92 | 6 | `NPCID.cs:11294`; `NPC.cs:10143–10160`; 共用 `netID=type` 与 style-6 分派 |

上述是参考源码映射，不代表仓库已经注册或实现这些 profile。参考 `AI_006_Worms` 的根初始化只在 `Main.netMode != 1` 下运行（`NPC.cs:51903–51908`）；它创建 14 个节（`NPC.cs:51911–51930`），默认 type 89，并在索引 1/8 使用 88、索引 11/12/13 使用 90/91/92。每节记录根槽到 `ai[3]` / `realLife`，前驱槽到 `ai[1]`，并由前驱 `ai[0]` 指向新节（`NPC.cs:51931–51938`）。这条源码链给出完整序列证据；当前 adapter 只准备单条父子关系，没有实现这 14 节生成序列。

coverage 状态：reference index 的 style 6 行目前只列 Devourer Head/Body/Tail（type 7/8/9，索引第 44 行），未列 Wyvern 87–92。本类别不修改中央 coverage ledger/index；Wyvern 的 profile registration、handler 和 verified 状态仍为 `open`。本类别给出的 42 个指定 style 中除 style 6 外其余风格均未在此实现或映射。

## 状态 owner 与生命周期

- 当前状态 owner：本工作树没有 Wyvern SpawnNow、multipart relation store 或 Wyvern profile caller。`NpcParentRelationComponent` 仅保存 parent instance、legacy slot 和 attached tick（`src/NSSLC/Component/Npc/NpcParentRelationComponent.cs:5–30`）；它自身不是关系索引、attach/detach owner，也不执行生命同步。
- 身份与有效性：`NpcAiProfileIdentity` 的字段为三个独立整数 `TypeId / NetId / AiStyle`（`src/NSSLC/Component/Npc/NpcAiProfileIdentity.cs:3`）。adapter 检查捕获/当前 instance、reference、handle 相同，active、NPC scope、runtime 与 local slot 一致（`NpcWyvernMultipartProfile.cs:155–170`）；它没有直接持有 session/generation owner，generation 只能由调用方提供的当前快照身份间接校验。
- 生命周期状态：本轮没有实现或运行 `SpawnNow → Attach → immediate read → update / shared life → Detach / Release`。`TryPrepareAttachment` 返回待提交数据，不会生成实体、写组件、设置兼容槽、立即查询、推进 AI、同步共享生命或清理关系。因此真实生命周期切片目标尚未完成。
- 关系边界：单条关系准备会比较 root `realLife` 投影、子节点前驱槽和不同 child slot；没有 14 节整链原子容量预留/回滚，也没有 root death 全链释放、中段断链修复/释放、transform 旧引用失效或 unload 清理的接线与证据。不得把这些边界标成已覆盖。

## API、更新顺序与 authority

- 新 adapter API：`NpcWyvernMultipartProfile.CanHandle(NpcAiProfileIdentity)` 与 `TryPrepareAttachment(int netMode, EntityRuntimeId activeRuntimeId, EntitySnapshot root, EntitySnapshot parent, EntitySnapshot child, bool spawnSucceeded, long attachedAtTick)`（`src/NSSLC/Component/Npc/NpcWyvernMultipartProfile.cs:150–178, 65–148`）。只允许 `netMode=0/2` 产生准备结果；client `1` 返回 `ClientObserveOnly`，其他模式拒绝；失败 spawn 返回 `CapacityRefused`；无效 tick、过期/错位 handle、错 root/neighbor 和不支持身份均返回拒绝状态。准备结果带 root/parent/child 的 reference、handle、instance、slot、legacy projections 及 `NpcParentRelationComponent`。当前仓库没有调用方，因此这是类别边界 adapter，不是已接入的 AI gate。
- 更新顺序：唯一确定的来源顺序是参考链逐节 `NewNPC → 立即读取新节 → 写入 ai[3]/realLife/ai[1] → 写前驱 ai[0] → SendData`（`NPC.cs:51930–51938`）。仓库侧没有 Wyvern 调度和调用证据；生成后同 tick AI、shared life、Detach/Release 的顺序仍 `unknown`。
- authority 硬约束：NPC AI 决策、目标选择、随机消费、物理/碰撞权威、生成、变换、奖励、任务推进、关系修改及 authority effects 仅在服务端/权威端执行。client tick 只消费复制状态、同步意图与呈现数据。adapter 的 netMode 拒绝只约束其自身准备 API，不保证外部不调用 AI；共享入口的统一 gate 仍由主会话集成。
- 宿主 authority 证据未改动：`Test/Terraria.NpcAi.TargetHostVerification/Program.cs:671–701` 保留 client `netMode=1` 更新 Eye 后 ActiveCount 不增、trace 为 `ServantSpawnSkipped:ClientAuthority`；`:703–733` 保留 server `netMode=2` 可生成 Servant 且记录 `ServantSpawned(slot=...)`。这些现存 Eye/Servant 断言不能证明 Wyvern 已有 authority gate。

## 改动文件

- [本执行文档](2026-10-07-npc-ai-style-segmented-multipart-execution.md)：唯一类别文档，记录来源映射、边界、未闭合项、验证及回退。
- [NpcWyvernMultipartProfile.cs](../../../src/NSSLC/Component/Npc/NpcWyvernMultipartProfile.cs)：新增 Wyvern style 6 身份白名单及单边关系准备 adapter；没有 caller，不提交实体/组件变更，也不包含 AI、生命同步或清理逻辑。
- 未改 `RuntimeNpcStore.cs`、公共 `NpcAiSystem`、`Test/Terraria.NpcAi.TargetHostVerification/Program.cs`、项目文件或中央 coverage ledger。

## 验证与证据

- 计划验证上限：源码核对 + 一个父子或三节点 focused/no-build 生命周期切片；不得扩大为 solution/full regression/full test sweep。本轮按用户最新限制未运行 `dotnet restore/build/run/test`，也没有执行编译或运行型 verifier；故 adapter 编译和行为验证均为 `not-run`，不能声称端到端通过。
- 已完成的只读证据检查：NPCID 与 NPC 来源映射、style-6 worm chain 的权威分支、目标宿主 client/server Servant 断言和 RuntimeNpcStore client 调用点均用 `rg`/PowerShell 源读取检查；authority assertions 保持原文，`Program.cs` 未改。首次在 `src` 下查询 reference 源码无结果（源码快照位于 `D:/TRbackup`），纠正路径后成功。先前五项工具/路径失败累计仍记为 PUA 失败计数 5，详见下列逐项记录；上述纠正后的读取未执行任何编译或测试。
- 可复现只读命令与结果：`rg -n 'WyvernHead|WyvernLegs|WyvernBody|WyvernBody2|WyvernBody3|WyvernTail' 'D:/TRbackup/无任何删减通过编译/Terraria.ID/NPCID.cs'` → exit 0；`rg -n -e 'type == (87|88|89|90|91|92)' -e 'aiStyle = 6' -e 'netID = type' -e 'aiStyle == 6' -e 'AI_006_Worms\(\)' 'D:/TRbackup/无任何删减通过编译/Terraria/NPC.cs'` → exit 0；读取 `NPC.cs` 第 51891–52005 行的 PowerShell `Get-Content | Select-Object` → exit 0；`rg -n -C 1 'ServantSpawnSkipped:ClientAuthority|An authoritative server Eye update|ServantSpawned\(slot=' Test/Terraria.NpcAi.TargetHostVerification/Program.cs` → exit 0；`rg -n -C 1 'IsClient: Main.netMode == 1|Clients still run the|if \(Main.netMode == 1\)' src/NSSLC.Tools.Simulation/RuntimeNpcStore.cs` → exit 0。
- PowerShell `[IO.File]::ReadAllLines` 源文本扫描（本执行文档与新 adapter）：exit 0，未发现尾部空白；C# 文件没有 tab 缩进、超 100 字符行；没有运行 formatter 或编译器。`git diff --check`：exit 0，但当前两个交付文件均为 untracked，故该 Git 命令不覆盖它们，也不作为其格式证据。
- TargetHost authority 源码核对：exit 0；见 `Program.cs:671–733` 的 client/server 两条断言。共享 client AI 路径核对：exit 0；`RuntimeNpcStore.cs:2292` 将 `Main.netMode == 1` 传给 Slime profile，`:3470–3476` 明确允许客户端运行 Eye profile，只跳过 Servant allocation。当前统一 server-only AI gate 未接入。
- 单独诊断报告：未生成。源级检查证据、未闭合项和本次范围统一记录在本 canonical Markdown；没有额外报告路径。
- PUA 失败计数：5。① 首次读取 fst-change 技能误用 `C:/Users/shan/.agents/skills/fst-change/SKILL.md`（不存在），之后按技能根目录索引改用 `C:/Users/shan/.codex/skills/fst-change/SKILL.md`。② `Context/架构设计/公共拆分约束.md` 不存在，批量读取 exit 1；`rg --files` 未找到该约束正文（Google C# 约束中的链接悬空）。③ 检索误用不存在的 `src/NSSLC/Component/Entity` 路径，之后改查已确认的 Relationships / WorldStorage 目录。④ 尝试读取并不存在的 `EntityRelationState.cs`、`EntityRelationKind.cs`，之后从目录与声明确认类别 API 复用现存 `NpcParentRelationComponent`。⑤ 静态检查编排脚本因 PowerShell `` `n `` 与 JS 模板字符串冲突，在 JS 解析阶段失败且没有执行子命令；之后改用直接 `rg` / Git 命令。前四项为路径/读取问题，第五项为脚本解析问题；没有编译或运行失败。L3 七项检查已完成；L4 源级隔离检查已完成，构建/运行因用户限制明确保持 `not-run`。
- 工具参数更正：最终读取任务记录时曾传 `turnLimit=12`，Codex app 接口拒绝（上限 10）；改用 `turnLimit=10` 后读取成功。该次没有仓库、构建或测试副作用；按 self-improvement 记录于本类别文档，未另建诊断文件。

## 缺口、集成与回退

- 未覆盖风格：指定集合中除本轮 style 6 的 Wyvern 身份目录外，styles `11,12,27,28,29,30,31,32,33,34,35,36,37,46,47,48,52,53,55,59,71,75,76,77,78,79,81,82,84,88,89,90` 没有本类别实现或身份映射；style 6 也未完成 registration/handler/行为闭包。未推断邻近 type/netId 映射。
- 主会话接入步骤：① 在唯一共享 NPC AI authority gate 确保 client 不推进 AI、随机、物理或 authority effects；不在类别分支复制 gate。② 由 lifecycle owner 权威 SpawnNow 创建 Wyvern head 和 14 个子节，检查每次容量/定义结果并提供整链失败回滚。③ 每次生成后立即解析新实例，按参考顺序将父/子/根快照交给 adapter，并由关系 owner 原子提交 `NpcParentRelationComponent` 与兼容 `ai[0]/ai[1]/ai[3]/realLife` 投影；不能把 `TryPrepareAttachment` 当提交。④ 由同一 owner 提供生成后同 tick 读取、AI 调度与 shared-life 语义，再实现根死亡、断链、变换、卸载时 detach/release。⑤ 在不削弱现有 TargetHost assertions 的前提下增加 Wyvern server/client 与 stale-handle/capacity 的 focused no-build probe，并记录实际调用、失败和清理轨迹。以上均为集成要求，当前尚未执行。
- 开放风险/缺口：adapter 未编译；`TryPrepareAttachment` 是单边校验而非 14 节事务，`spawnSucceeded` 只表达调用方给出的布尔结果，没有容量预留或部分生成回滚；对任意 segment-parent / segment-child 组合的精确合法序列仍需由链级 owner 约束；没有 shared-life、death/unlink/transform/unload 生命周期实现；统一 client gate 未接入。故本类别交付为 adapter + 源证据记录，整体 goal 仍 partial。
- 精确回退范围仅两项本轮新建文件：删除 `src/NSSLC/Component/Npc/NpcWyvernMultipartProfile.cs` 和 `docs/plans/system-decomposition/2026-10-07-npc-ai-style-segmented-multipart-execution.md` 即可撤回本轮内容；不回退或覆盖其他文件，也不改用户既有内容。两项目前均为 untracked。

- client-side AI 审计结果：已发现共享宿主仍有 client tick AI 路径，主会话统一 gate 尚未接入；本类别不重复改共享入口。`src/NSSLC.Tools.Simulation/RuntimeNpcStore.cs:2292` 将 `Main.netMode == 1` 传为 Slime profile 的 `IsClient` 输入；`:3470-3473` 注释明确写着客户端仍运行 Eye profile 做移动/呈现，只在仆从生成时跳过，`:3473-3476` 的 gate 仅拒绝 Servant allocation，不能证明 client 不运行 AI。精确调用点与周围更新入口待后续复核/补行号。此路径登记 `open / not integrated with global authority gate`；主会话集成唯一 server-only gate 后，客户端只读复制状态/同步意图/呈现数据。
- authority verifier 现状：`Test/Terraria.NpcAi.TargetHostVerification/Program.cs:671-701` 以 `netMode=1` 更新 Eye 并断言 ActiveCount 不增、trace 为 `ServantSpawnSkipped:ClientAuthority`；`:703-733` 以 `netMode=2` 更新 Eye 并断言 ActiveCount 增长及 `ServantSpawned(...)` trace。不得删除/放宽。本类别不改该共享 `Program.cs`；补充的类别 verifier 只能验证本类别 profile/adapter 的 server-only 语义，不替代这两条宿主断言。

## 交付前自检

- [ ] 最小切片为真实运行路径，生命周期和 authority 可追踪（adapter 未接入，未达到）。
- [ ] 同类别类似问题、上下游关系、容量和断链边界已由生产 owner 闭合（当前只有来源审查；容量事务和清理未实现）。
- [x] 指定 authority 断言未删除、未放宽；共享 `Program.cs` 未改。
- [x] 已记录实际源检查、已知退出码、报告状态、缺口和精确回退范围；编译/运行验证明确为 `not-run`。
- [x] 未知/未实现映射明确保持 `open`，没有将本地缺席解释为已实现，也没有添加 fallback。



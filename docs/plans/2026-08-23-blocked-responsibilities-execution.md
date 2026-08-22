# Blocked Responsibility Families Execution Proposal

> 状态：执行中；本提案只推进有 source oracle、唯一 owner 和可重放边界的窄卡。
> 不把任何单卡结果升级为完整 ECS 迁移完成，也不扩大到 full regression/client suite。

## 目标与不变量

本批处理以下尚未解除的阻塞：

- `M-001`：不创建 `InitializeAlmostEverything` 或新的 aggregate initializer；逐个确认独立
  responsibility family。
- `M-014/M-024`：不引入 `Queue<Action>`、通用 `IEnumerator` scheduler 或不可序列化 callback。
  只有恢复出 typed、可取消、可重放 caller contract 才能实现。
- `B-007`：不把 domain RNG 宣称为 legacy global `Main.rand` ordering oracle；没有完整 source
  trace 就保留自动概率分支为 deferred。
- entity/event lifecycle、NPC/item/projectile tables、random starts、client/presentation
  分支继续逐卡处理，不通过默认值、静态表占位或客户端投影伪造完成度。

每张卡必须记录 source path、SHA-256、行范围、owner、RED、accepted predicate、rejected
predicate、persistence/replay 影响、明确 deferred 分支和验证输出。

## 约束与验证预算

执行前读取 `AGENTS.md`、`约束/Google-CSharp-Style-Guide-约束.md`、主执行提案以及精确
Version4 source member。保持用户已有 dirty changes，不执行 broad cleanup 或回滚。

每张卡只运行：

1. changed-card focused verifier；
2. 最多一个直接受影响 verifier/project build；
3. `Test/Terraria.Dome.MainBoundary.Verification`；
4. 本卡 scoped `git diff --check`。

不运行 full regression、完整 client suite、root Release、duplicate replay matrix，除非
用户另行扩大范围。验证失败必须保留原始失败和重跑结果，不能用成功重跑覆盖 timing flake。

## 执行顺序

### Card 1：NPC death/loot lifecycle qualification

**目的：**确认现有 `NpcDeathSystem -> PublishNpcDeaths -> CommitNpcLoot` 只覆盖一次性死亡
发布和 deterministic loot child，不宣称 `NPC.checkDead/NPCLoot` parity。

**Source oracle：**

- `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs`
- `checkDead`：`64571-64752`；`NPCLoot`：`65312-65416`
- 记录当前 source SHA 与实际行范围；source branch 中的 boss、town、invasion、random、
  network、sound、achievement、client 分支必须逐项列为 deferred，除非另有独立 owner。

**RED：** focused verifier 证明重复 inactive/dead observation 不得二次发布 loot；非
`Killed` despawn、缺失 loot table、无效 health 和不稳定 identity 必须拒绝或无产出。

**允许实现：**仅补齐现有 NPC lifecycle owner 的窄去重/命令证据；loot table 必须是显式
`NpcDefinition`/registry 输入，不能按 NPC type 推断完整表，不能消费 `Main.rand`。

**验收：**一次死亡产生至多一个 typed loot child；save/restart 仍不重复；NPC verifier、
最多一个 combat/loopback verifier、MainBoundary、scoped diff check 通过。未满足 source
branch 依据时，卡片状态为 `unknown/deferred`，不改生产逻辑。

### Card 2：M-001 单一静态定义 family

当前树中符合条件的 TileEntity/torch definition children 已经存在，本批先完成 coverage
matrix reconciliation：将 `TileEntity.InitializeAll` 与 `TorchID.Initialize` 记为
`covered`，并保留其 payload/update/persistence、color/light 和完整表语义为 deferred。下一
张 implementation card 必须选择另一个仍未决且重新满足“固定 identity、单一 server owner、
无 runtime callback/UI”的 family；排除 ArmorSetBonuses、ShopHelper、TeleportPylons、
ContentSamples repair 等已证明混合责任族。不得创建总初始化器。

### Card 3：M-014 typed caller contract

从 `main-thread-action-contracts` 选择一个有稳定 caller identity、phase、取消、重试、
restart 和 projection 证据的 caller。若 caller 仍只有 arbitrary `Action`，只写 qualification
card 并保持 deferred。实现必须是 caller-specific command/state machine；禁止 arbitrary
queue。

当前执行结果：`docs/research/2026-08-24-main-thread-action-caller-inventory.md` 重新记录
`SetAllSectionsLoaded` 与 background `mainThreadFollowup` 两个 caller，以及 `DoUpdate` 后
FIFO drain。两者都缺 typed/replayable contract，M-014 保持 `unknown/deferred`，没有新增
delegate queue。

### Card 4：M-024 delayed process contract

以 `delayed-process-contract-matrix` 为 RED 基线。当前 `DelayedProcesses` 和
`DelayedProcessesInGame` 没有 source-backed `.Add` caller，故默认 outcome 是 deferred。
只有恢复 typed owner、phase/pause、cancellation、restart、persistence 和 replay identity
后才能新增代码；不得用 coroutine wrapper 伪造语义。

当前执行结果：`docs/research/2026-08-24-delayed-process-caller-inventory.md` 重新扫描
Version4 `.cs` 树，仍只有声明与 `MoveNext/Remove` 消费，没有 `.Add` caller。M-024 保持
`unknown/deferred`，未新增 scheduler 或 coroutine wrapper。

### Card 5：B-007 legacy RNG oracle

按 `Main.cs:13739-13762` 建立完整 night-start source trace，包含所有先行 `Main.rand`
消费和 owner prerequisites。若 trace 依赖 client/UI/WorldGen 或不可恢复的全局顺序，记录
第一个不可恢复 consumer，保留显式 meteor command，自动概率继续 deferred。只有可执行 trace
能复现 qualified/unqualified branch 且 save/restart 不重复消费时才实现。

当前执行结果：trace 已记录至 `Build/diagnostics/main-migration/task-10-meteor-rng-oracle-audit/20260823-220000/`。
`WorldEventRandomState` 的 WorldRules 验证通过，但它不等价于 legacy `Main.rand`；B-007
继续为 `explicit-deferred`，未添加自动概率实现。

### Card 6：entity/event continuation matrix

对 Player/NPC/Projectile/Item 和 invasion/weather/event 各创建一张 coverage card。每张只
推进一个 lifecycle predicate，要求 stable identity、forged/duplicate rejection、必要的
persistence 和 replication evidence。NPC tables、random starts、client/presentation 只作
独立后续卡，不从窄 child 推导完成。

## 卡片产物模板

```text
Build/diagnostics/main-migration/task-10-<slug>/<run-id>/scenario.yaml
docs/research/2026-08-23-<slug>-boundary.md   # 需要 source qualification 时
progress.md
```

`scenario.yaml` 至少包含：`status`、`source.path`、`source.sha256`、`source.lines`、
`owner`、`red`、`accepted`、`rejected`、`deferred`、`focused_verifier`、`affected_gate`、
`main_boundary`、`diff_check`。

## 停止条件

- source member 有多个 plausible owner；
- 需要未建模的 NPC/projectile/item table、global random ordering 或 presentation state；
- caller 没有 typed/replayable contract；
- focused verifier 失败且连续重跑仍失败；
- dirty worktree 使 ownership 无法判定。

停止时只记录 `unknown/deferred/blocked` 和证据，不以默认值或空实现清除阻塞。

## 完成定义

本提案完成仅表示每个阻塞项都有独立的“可实现证据路径”或“可复现 deferred 边界”。
`M-001`、`M-014`、`M-024`、`B-007` 以及完整 entity/event lifecycle、NPC tables、random
starts、client/presentation 分支仍必须在各自 accepted cards 完成后，才能从主 coverage
matrix 移除未决状态；本提案不产生整体迁移完成声明。

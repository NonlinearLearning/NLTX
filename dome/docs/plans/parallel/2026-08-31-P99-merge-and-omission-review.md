# P99 Parallel Plan Merge and Omission Review Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 串行合并 P00-P11 的独立工作稿，核对遗漏、重复、Protocol 回流和状态口径，
并将经审查的结果写回总迁移提案。

**Architecture:** P99 是唯一允许修改 parent plan 的任务。它只合并已存在的字段卡和边界，
不扩展为 C# 实现、不替换未闭合行为、不将工作稿数量转成迁移率。合并后仍保持 partial、
deferred 和删除门状态。

**Tech Stack:** Documentation-only Markdown; no C# changes, no new functions, no tests/builds.

---

## 1. Task identity and write set

| 项目 | 内容 |
| --- | --- |
| Task | `P99` |
| Parent plan | `docs/plans/2026-08-31-server-required-field-property-migration-plan.md` |
| Dependency | P09-P11 完成；P10/P11 可并行后再启动 |
| Wave | `3`，串行唯一回写任务 |
| Read set | P00-P11 全部工作稿、parent plan 第 0-19 节、已有 migration/research/ledger references |
| Write set | 仅 parent plan；子任务工作稿只读 |
| Status | `[ ] not-run` |

## 2. Merge order

```text
1. Read P00 contract and freeze status/exclusion vocabulary.
2. Merge P01 World/Main and P02 Player domain cards.
3. Merge P03 NPC, P04 Projectile and P05 Item cards.
4. Merge P06 WorldObjects, P07 WorldGen and P08 Server host cards.
5. Apply P09 Protocol exclusion matrix as a negative pass over all merged rows.
6. Apply P10 persistence/identity/deletion-gate decisions.
7. Apply P11 component/read-write/command/commit seams.
8. Reconcile counts, duplicate IDs, source hashes, links and status words.
9. Write only the parent plan and record the final not-run boundary.
```

## 3. Required merge checks

### 3.1 Main/World

- [ ] `offLimitBorderTiles`、`dungeonX`、`dungeonY`、`tileTable`、`tileBlockLight`、
  `tileNoSunLight`、`tileSpelunker`、`tileLargeFrames`、`tileNoFail` 都有独立 declaration row。
- [ ] `invasionProgressIcon` 与 typed invasion authority 分开；display projection 不作为
  Simulation field。
- [ ] `GameMode`、`IsJourneyMode`、`NoFunctionalSurface`、`masterMode`、`expertMode`、
  `IsRainingForever` 各自独立核对，不合并成一个 mode 行。
- [ ] Main `members=696`、`migratedScope=695`、`identityExcluded=1` 和旧 `685` 口径差异
  继续显式保留，不混算为迁移率。

### 3.2 Player

- [ ] mount、wings、wingsLogic、wingTime、wingTimeMax、grappling、grapCount、fishingSkill、
  golferScoreAccumulated 逐项出现。
- [ ] luck/luck caps/luckPotion/oldLuckPotion、shadow dodge、Paladin/shield/parry 逐项出现。
- [ ] `respawnTimer`、`attackCD`、item animation/time/tool timers、fallStart/fallStart2、
  breath/lava/adjacent liquid 逐项出现。
- [ ] armor/dye/misc equipment/misc dyes/inventory/inventoryChestStack/trash/bank-bank4/
  Void Vault/Buff arrays 逐项出现。
- [ ] `59/40/990` 三套布局保留差异，没有 legacy 58-slot alias。
- [ ] `SpawnX/SpawnY`、death/drop marker、pending world-item intent 与 lifecycle transition
  有单独行；小写兼容命名不得伪造 source declaration。

### 3.3 NPC

- [ ] 18 个遗漏字段/属性逐项出现，含 `_givenName`、static capacities、classification、
  progression query、shield resource 和 targeting index split。
- [ ] 所有 `AI_###` markers 都归入 behavior family；每个 family 有 typed state、reader、
  writer、default、phase、random、transition、terminal、persistence 和 deferred reason。
- [ ] `NPC.netID`/business handle 保留；encoded target index 和 wire expression excluded。

### 3.4 Projectile

- [ ] 当前 source hash 和 `maxAI=3` 口径作为当前事实；slot 只到 `0..2`。
- [ ] `ai[0..2]`、`localAI[0..2]` 逐 slot 出现；`ai[3]`/`localAI[3]` 只保留 negative inventory。
- [ ] `netUpdate`、`netUpdate2`、`netSpam`、`netSyncSkippedForPlayer`、B248、network
  readiness 和 replication snapshot 全部 excluded。

### 3.5 Item

- [ ] `ITM-001..ITM-140` 和 `ITM-P-001..ITM-P-008` 每行都有 declaration-level metadata，
  不能只保留范围计数。
- [ ] Definition、instance、equipment、use/recovery、placement、economy、world item 和
  presentation boundary 清晰。
- [ ] `stack` 是 instance state，`maxStack` 是 definition cap；business persistence 与
  Protocol projection 分离。

### 3.6 WorldObjects/host/WorldGen

- [ ] Chest、Sign、TileEntity、Door、Wire、Actuator、TrainingDummy 都有 identity/anchor/
  lifecycle/content/link/permission/revision/restore/stale-link 行。
- [ ] TileEntity static manager/index/update list、Wiring scratch 和 TrainingDummy query
  workspace 不成为 generic singleton owner。
- [ ] WorldGen `field/property/constant/derived/scratch/host/client` 统计不相加；七类遗漏
  声明都有分类和 source hash。
- [ ] host path/handle/session/permission/timeout/save/recovery 不进入 Simulation；
  `maxNetPlayers` 与 `maxPlayers` 分开。

## 4. Protocol return-to-excluded pass

P99 必须对合并后的全文做负向检查：

- [ ] Owner、Component、Snapshot、Command、System 名称中没有 Protocol/Transport/Replication
  类型作为 server business owner。
- [ ] 没有 message ID、packet kind、payload length、cursor、bit mask、wire slot、sync
  cadence、network revision 或 replication snapshot 进入迁移完成条件。
- [ ] B248 仍是 reference-only；没有把其网络调度字段计入 Projectile/server completion。
- [ ] `WorldId`、Player handle、entity handle、Item instance handle、Chest identity、
  `Projectile.identity`、`NPC.netID` 和 business revision 没有被错误删除。
- [ ] request/response/message 87/open/update/delete layout 只在 excluded matrix 中出现。

## 5. Status and deletion-gate reconciliation

- [ ] 新增字段行全部保留 `[ ]`；未执行项使用 `not-run`，不伪造验证结果。
- [ ] `accepted-narrow` 只保留现有窄证据值域，不升级成 full parity。
- [ ] `partial`/`deferred` 必须保留未闭合 consumer、顺序、恢复或边界原因。
- [ ] `ServerRelevant deferred=44` 原样保留；4 条 ReplacedWithEvidence 不扩散到邻近字段。
- [ ] `canRemoveLegacyWorldGen=false` 原样保留；aggregate tile/extended-state/random/
  persistence mismatch 未解决前不得修改。
- [ ] 不把工作稿数量、registry 数量、声明数量、weighted score 或局部绿色 verifier 当作
  迁移完成率、Terraria parity 或 release-ready。

## 6. Parent-plan write procedure

- [ ] 只使用 `apply_patch` 修改 parent plan 中的并行拆分索引/经审查的字段卡。
- [ ] 保留 P00-P11 独立文件链接；不将其内容压扁为一个范围 bullet。
- [ ] 不修改 `progress.md`、Flowstate manifest、CSV、JSON、源码、`.csproj` 或 Build 输出。
- [ ] 不执行 test、build、verifier、regression、docs gate 或 WorldGen differential。
- [ ] 在 parent plan 中保留“本轮仅文档拆分、无实现、无验证”的边界声明。

## 7. Acceptance and handoff

- [ ] 所有 P00-P11 工作稿均可追溯，且没有重叠写集冲突。
- [ ] parent plan 可通过子任务 ID 导航到每个独立 Markdown 文件。
- [ ] 字段遗漏、组合行、WorldObjects inventory、AI family、AI/localAI slot 和 Protocol
  exclusion 都有明确闭环入口。
- [ ] 迁移总体仍为 `partial`；`ServerRelevant deferred=44` 与
  `canRemoveLegacyWorldGen=false` 不变。
- [ ] P99 完成后只交付文档合并结果；实现、测试和构建必须另开批次重新授权。

本任务是文档合并与反向审计，不执行任何代码、测试、构建、verifier、regression、
Protocol 迁移或物理删除。

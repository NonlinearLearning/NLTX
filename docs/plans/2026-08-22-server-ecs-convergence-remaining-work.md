# Server ECS Convergence：剩余任务与最终目的

> Flowstate execution context: `docs/flowstate/plan/2026-08-22-server-ecs-convergence.md` and
> `docs/flowstate/task/2026-08-22-server-ecs-convergence.md`. The graph is active at B6;
> this document remains the authoritative semantic completion boundary.

## 目的

在不复制 `Main.cs`、`Player.cs`、`WorldGen.cs` 或其他 legacy 文件的前提下，完成
server-authoritative ECS 收敛。每一项被接受的 legacy responsibility 必须形成并通过完整链路：

```text
完整 source/runtime oracle
-> immutable input 或 snapshot
-> Simulation query/system
-> typed command
-> deterministic commit
-> revisioned snapshot
-> persistence / V1456 projection
-> focused verifier 和 loopback verifier
```

最终目标不是“代码能编译”或“verifier 数量足够多”，而是：

1. 所有声明为已迁移的 server capability 都有 source-backed owner、mutation path、
   persistence/protocol consequence 和 clean-process evidence。
2. 支持的 WorldGen profile 与完整 oracle 在每个 stage 和最终 tile/extended state 上
   具有一致 fingerprint。
3. 所有 Version4 物理删除都有可审计的分类；任何未恢复、未替换或语义不确定的行为都
   保持 `deferred`，不能被当成完成迁移。
4. legacy WorldGen 和完整 source oracle 只有在 deletion gate 明确通过后才允许移除。

## 当前已确认完成

- Simulation 和 Server Release 构建已通过，当前受影响构建为 0 warning、0 error。
- 当前 clean-process sweep：49 个 verifier 通过，0 个 verifier 失败；需要 `--port` 的
  `RealClientFixtureHost` 单独跳过。
- completion manifest：100 分 capability manifest，核心 server evidence score 为 92。
- session identity、command ordering、disconnect lifecycle、player authority、physics、
  death/respawn、NPC/projectile combat、PVS/tombstone、item/inventory、WLD persistence、
  world clock/weather/progression、chest/sign 基础 authority 已有对应 verifier。
- TrainingDummy 已有 bounded server-owned placement/removal、typed persistence、NPC link、
  tick lifecycle、observer/V1456 projection，以及 source-backed message-87 bounded inbound
  contract；完整 TrainingDummy family parity 仍保持 `Partial`，见下文。
- WellFed 已完成 server-owned state、typed food command、clear transition、tick decay 和
  player persistence；证据位于
  `Build/diagnostics/server-ecs-convergence/P4-deletion/20260822-wellfed/evidence.json`。
- Version4 physical deletion ledger 已覆盖 535 行：

  ```text
  ClientOnly=425
  SharedDefinition=60
  ReplacedWithEvidence=4
  ServerRelevant=44
  Unknown=0
  serverRelevantWithoutEvidence=0
  ```

## 剩余任务

### 1. 完成 WorldGen source/runtime contract 和 differential parity

当前状态：`blocked`，但仍可继续推进；不是删除授权。

权威现状：

- `canRemoveLegacyWorldGen=false`
- oracle inventory：684 methods、233 fields、125 partial methods、559 unmapped methods、
  233 unmapped fields
- 最新完整 differential：比较 5,040,000 tiles，mismatch 3,190,404；extended-state
  mismatch 1,046,843
- fresh differential trace：`Build/diagnostics/server-ecs-convergence/P9-worldgen/current-full-differential/trace.txt`
  records the same negative result; ECS-only stage fingerprints are available at
  `Build/diagnostics/server-ecs-convergence/P9-worldgen/current-stage-trace/stage-fingerprints.json`
  with `oracleParity=not-compared`.
- 当前 pipeline 只支持 default seed、difficulty 0、non-hardmode profile

待办：

- 为 Terrain、Cave、Biome、Ore、Structure、Tree、Liquid、Frame、Final Commit 建立
  source-derived stage fingerprint，而不是只验证 ECS 自身 determinism。
- 恢复选定 profile 的完整 RNG consumption、options、pass order、runtime side effects，
  特别是 `Main.rand`、secret seed、structure scanning、liquid propagation、tile frame
  notification 和 destruction semantics。
- 每加入一个 predicate，先形成 source anchor -> RED differential -> immutable query ->
  typed command batch -> atomic commit -> fingerprint -> focused verifier 的闭环。
- 重新运行完整 oracle differential；只有 tile state、extended state、metadata、command
  sequence 和 random checkpoint 都一致时，才可修改 deletion gate。

验收证据：

- `docs/worldgen/legacy-worldgen-differential.json`
- `docs/worldgen/worldgen-source-inventory.json`
- `docs/worldgen/worldgen-deletion-gate.json`
- fresh differential artifact，结果必须明确记录 mismatch 为零

### 2. 逐项清理 44 个剩余 ServerRelevant physical deletions

当前状态：ledger 分类完整，但 44 行仍 `deferred`；物理删除 safety gate 必须继续非零。

待办：

- 逐行读取完整 oracle，而不是仅依据 Version3 空壳或文件名推断。
- 对每行补齐 legacy anchor、authoritative owner、component/snapshot、typed command/commit、
  persistence/protocol consequence 和 focused verifier。
- 可证明为离线工具、UI、graphics、social 或其他不能修改 authoritative server state 的
  条目，改为 `ClientOnly`，并保留 source/reference evidence。
- 有完整 replacement chain 的条目改为 `ReplacedWithEvidence`；仅有名称相似、声明存在或
  bounded fixture 的条目不得升级。
- 每次变更后运行 ledger verifier；要求 `serverRelevantWithoutEvidence=0`，但在全部语义
  完成前 `serverRelevantDeferred` 可以保持非零。

重点审计组：

- `Terraria.GameContent.Biomes.*`
- `Terraria.GameContent.Generation.*`
- `Terraria.WorldBuilding.*`
- `Terraria.GameContent.FlexibleTileWand.cs`
- `Terraria.GameContent.MinecartDiggerHelper.cs`
- `Terraria.GameContent.ShimmerHelper.cs`
- `Terraria.GameContent.ObjectInteractions.PotionOfReturnHelper.cs`
- 任何仍依赖 WorldGen 全局状态、随机流、tile scan 或 pass scheduling 的文件

验收证据：

- `docs/migrations/version4-physical-deletion-ledger.csv`
- `Build/diagnostics/server-ecs-convergence/P4-deletion/*/verify-ledger.ps1` 输出
- 每个 `ReplacedWithEvidence` 行对应的 source/evidence artifact

### 3. 完成 TrainingDummy 的剩余 authority 边界

当前状态：`Bounded contract accepted`，message 87 的 source-backed inbound placement
方向、typed decode、session/bounds/PVS guards、silent-drop rejection、valid/duplicate/invalid
loopback、disconnect 和 reconnect evidence 已通过；完整 TrainingDummy family parity 仍非
本批次目标。

待办：

- 已确认完整 oracle 的 message 87 是客户端发送、服务器接收的 placement frame；不能把它
  误标为仅 server-to-client projection。
- 已实现 packet decode、session validation、tile validity/ownership guard、typed placement
  command、deterministic owner commit 和 source-aligned silent-drop rejection。
- oracle 接收分支没有 interaction-distance predicate，因此不额外声称 range contract；
  完整 TrainingDummy family parity 仍在 capability matrix 中保持独立状态。
- 为 valid placement、duplicate placement、invalid tile、foreign session、disconnect、
  reconnect 和 observer PVS 写 loopback evidence。

验收证据：

- `Test/Terraria.Dome.WorldObjects.Verification`
- `Test/Terraria.Dome.WorldObjects.Loopback.Verification`
- `docs/research/2026-08-23-tile-entity-authority-qualification.md`
- protocol source anchor 和明确的 inbound/outbound direction evidence

### 4. 同步当前 gate metadata，淘汰过期过程结论

当前状态：部分完成。代码和最新 verifier 已更新，但部分旧 JSON/历史 checkpoint 仍保留
早期数字，不能作为当前状态的唯一依据。

待办：

- 将 WorldGen gate 中过期的 build warning、旧 regression 数字标为 historical，或更新为
  最新可复现 artifact；不删除历史证据。
- 保持 `completion-manifest.json` 指向最新 full sweep 和 focused evidence。
- 保持 proposal 的历史 checkpoint 不改写，只更新明确标注为 current 的 baseline/status。
- 继续使用 JSON/CSV evidence；只有最终收敛或必要的状态汇总才新增 Markdown。

验收证据：

- `docs/server-completion/completion-manifest.json`
- `Build/diagnostics/server-ecs-convergence/P-regression/20260823-0100/summary.md`
- `Build/diagnostics/server-ecs-convergence/P-regression/20260823-0100/results.json`
- `docs/worldgen/worldgen-deletion-gate.json`
- `git diff --check`

The former `summary-after-wellfed.json` path is retained only as a historical reference in
`docs/cr/CR-2026-08-23-build-evidence-refresh.md`; it is not a current acceptance artifact.

### 5. 最终 acceptance review

只有前四项全部满足后才执行：

- 从干净进程重新运行所有 claimed verifier 和 loopback verifier。
- 重新构建 Simulation、Server、Protocol、WorldFile、WorldCompatibility 直接消费者。
- 重新执行完整 WorldGen differential 和 physical deletion ledger gate。
- 逐条检查 design 与 execution proposal 的 success criteria，不以 weighted score 替代语义
  完成度。
- 确认没有 deferred/unmapped/unknown 项被错误标为 accepted。
- 最后才允许在一份最终 Markdown 中记录结论；在此之前不删除 legacy WorldGen 或完整 oracle。

## 当前不能声称的内容

- 不能声称 100% Terraria compatibility。
- 不能声称 WorldGen parity 或 legacy WorldGen 可删除。
- 不能声称 Version4 physical deletion 已安全完成。
- 不能把 92/100 core evidence score 当成完整迁移百分比。
- 不能把 49 个 verifier 通过解释成未覆盖行为已正确。

## 完成定义

本目标只有在以下条件同时成立时才算完成：

```text
all claimed server contracts accepted
and
WorldGen supported-profile differential mismatch = 0
and
WorldGen deletion gate = true
and
physical deletion ledger has no deferred ServerRelevant rows
and
fresh full verifier sweep passes
and
all design/execution proposal success criteria are evidence-backed
```

在上述条件成立之前，项目状态必须保持 `incomplete`，未解决行为必须保持
`deferred` 或 `blocked`，并继续保留完整 source/runtime oracle。

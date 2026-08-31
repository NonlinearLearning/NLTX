# P04 Projectile Declaration and Behavior-State Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 将 Projectile 的 `maxAI`、`ai` 和 `localAI` 声明拆为可审核的行为状态候选，同时
明确排除所有 Protocol/replication/network-scheduling 字段。

**Architecture:** Definition、transform、owner、collision、combat、lifetime、cooldown 和
behavior family state 分层。原始 `float[]` 不成为新的公共 owner；只有经 source consumer
证明会改变服务器行为的 slot 才能映射为 typed state。

**Tech Stack:** Documentation-only Markdown; no C# changes, no new functions, no tests/builds.

---

## 1. Task identity and write set

| 项目 | 内容 |
| --- | --- |
| Task | `P04` |
| Parent plan | `docs/plans/2026-08-31-server-required-field-property-migration-plan.md` |
| Dependency | `P00` |
| Wave | `1`，可与 P01-P03、P05-P09 并行 |
| Read set | Projectile oracle、Projectile migration research、AI-style docs、B248 reference、parent plan 第 10 节 |
| Write set | 仅本文件 |
| Status | `[ ] not-run` |

## 2. Source snapshot and slot-count reconciliation

```text
Build/worldgen-oracle/legacy-instrumented-source/Terraria/Projectile.cs
SHA256 8D6428FA1D098B26ADFDC1D6CC028251814B40CD3ACE576F0BFF462FB19D6038
```

当前 checkout 的声明为：

- `Projectile.cs:130`：`public static int maxAI = 3`；
- `Projectile.cs:132`：`public float[] ai = new float[maxAI]`；
- `Projectile.cs:134`：`public float[] localAI = new float[maxAI]`。

因此当前 source 的合法 slot 只有 `0..2`。旧笔记或旧计划若写成 `maxAI=4`，必须在字段卡
中作为 stale-source reconciliation 记录，不能添加 source 当前不存在的 slot `3`。

## 3. Declaration-level inventory

| ID | Legacy declaration | Source anchor | Exact type/default | Server owner candidate | Lifetime | Persistence | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `P04-F-001` | `maxAI` | `Projectile.cs:130` | static `int`; `3` | behavior capacity metadata | definition/process | definition/reference | `deferred` |
| `P04-F-002` | `ai[0]` | `Projectile.cs:132` | instance `float`; `0f` | typed behavior-family state | projectile lifetime/tick | family decision | `deferred` |
| `P04-F-003` | `ai[1]` | `Projectile.cs:132` | instance `float`; `0f` | typed behavior-family state | projectile lifetime/tick | family decision | `deferred` |
| `P04-F-004` | `ai[2]` | `Projectile.cs:132` | instance `float`; `0f` | typed behavior-family state | projectile lifetime/tick | family decision | `deferred` |
| `P04-F-005` | `localAI[0]` | `Projectile.cs:134` | instance `float`; `0f` | behavior state only if server consumer exists | projectile lifetime/tick | usually transient | `deferred/excluded` |
| `P04-F-006` | `localAI[1]` | `Projectile.cs:134` | instance `float`; `0f` | behavior state only if server consumer exists | projectile lifetime/tick | usually transient | `deferred/excluded` |
| `P04-F-007` | `localAI[2]` | `Projectile.cs:134` | instance `float`; `0f` | behavior state only if server consumer exists | projectile lifetime/tick | usually transient | `deferred/excluded` |
| `P04-F-008` | `ai[3]` | no declaration in current source | not applicable | no owner | not applicable | not applicable | `excluded/legacy-reference` |
| `P04-F-009` | `localAI[3]` | no declaration in current source | not applicable | no owner | not applicable | not applicable | `excluded/legacy-reference` |

`P04-F-008` 和 `P04-F-009` 是 negative inventory rows，不是待实现字段。只有新的、带
source hash 的 oracle 明确将 `maxAI` 改为 4 时，才允许重新开一条 reconciliation change。

## 4. Per-slot behavior matrix

P04 必须将每个 slot 与所有 source consumers 对照；不能只记录数组声明：

| Slot | Behavior family | Typed replacement | Default | Readers | Writers | Transition | Terminal condition | Persistence | Status |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `ai[0]` | 按 `aiStyle`/source family 分类 | family-specific scalar/enum/handle | `0f` | exact methods/lines | exact methods/lines | spawn/use/tick | despawn/phase end | family decision | `deferred` |
| `ai[1]` | 按 `aiStyle`/source family 分类 | family-specific scalar/enum/handle | `0f` | exact methods/lines | exact methods/lines | spawn/use/tick | despawn/phase end | family decision | `deferred` |
| `ai[2]` | 按 `aiStyle`/source family 分类 | family-specific scalar/enum/handle | `0f` | exact methods/lines | exact methods/lines | spawn/use/tick | despawn/phase end | family decision | `deferred` |
| `localAI[0]` | behavior or local scratch classification | typed counter/resource or excluded | `0f` | exact methods/lines | exact methods/lines | tick/effect | family end | usually transient | `deferred` |
| `localAI[1]` | behavior or local scratch classification | typed counter/resource or excluded | `0f` | exact methods/lines | exact methods/lines | tick/effect | family end | usually transient | `deferred` |
| `localAI[2]` | behavior or local scratch classification | typed counter/resource or excluded | `0f` | exact methods/lines | exact methods/lines | tick/effect | family end | usually transient | `deferred` |

同一 slot 被多个 projectile type 复用时，必须生成多条 family-specific subrow；不能把数值
语义粗暴统一为 `BehaviorValue0/1/2`。

## 5. Explicit network exclusion inventory

以下行只做负向核对，不能加入迁移分母、Component、Snapshot 或 owner：

| Legacy/current name | Classification | Required disposition |
| --- | --- | --- |
| `netUpdate` | replication scheduling | `excluded`；不得创建 network-update component |
| `netUpdate2` | replication scheduling | `excluded` |
| `netSpam` | network cadence/throttle | `excluded` |
| `netSyncSkippedForPlayer` | per-player replication bookkeeping | `excluded` |
| `NetworkUpdateReady` | replication readiness | `excluded` |
| `ProjectileReplicationSnapshot` | replication projection | `excluded`；不能作为 business snapshot |
| session cursor/replication cursor | transport/session state | `excluded` |
| packet identity/owner index | wire encoding | `excluded`；业务 entity handle 保留 |
| B248 scheduling state | Protocol/replication reference-only | 不计入本次 server field migration |

`Projectile.identity`、Projectile entity handle 和业务 revision 仍可保留为运行时/持久化身份，
但任何将其编码到 packet 的字段另行标 `excluded`。

## 6. Component boundaries

| Owner | Allowed state | Forbidden state |
| --- | --- | --- |
| `ProjectileDefinitionComponent` | type、aiStyle、base capability、default limits | network layout、raw per-instance array |
| `ProjectileBehaviorStateComponent` | 按 family 拆出的 typed counters/phase/handles | `float[] ai/localAI` 作为公共 owner |
| `ProjectileTransformComponent` | position、velocity、direction、bounds | render rotation/oldPos cache |
| `ProjectileLifecycleComponent` | active、timeLeft、spawn/despawn reason | replication cadence |
| `ProjectileOwnerComponent` | Player/NPC/world business handle | owner wire index |
| `ProjectileCollisionComponent` | tile/liquid collision rule/state | client collision visualization |
| `ProjectileCombatComponent` | damage、friendly/hostile、penetration、hit result | hit sound/particle |
| `ProjectileCooldownComponent` | restrike/local immunity business cooldown | `netSpam`/sync throttle |

## 7. Ordered actions

- [ ] 冻结当前 source hash 和 `maxAI=3`；标记旧 `maxAI=4` 口径冲突。
- [ ] 对 `ai[0..2]` 和 `localAI[0..2]` 分别列出所有 readers/writers 和 source family。
- [ ] 将 slot values 按 behavior family 转换候选；render/audio/local scratch 标 `excluded`。
- [ ] 为每个 family 记录 default、phase、random state、transition、terminal condition 和
  persistence；缺证据项保持 `deferred`。
- [ ] 单独回查 `netUpdate`、`netSpam`、B248 及 replication snapshot，确保没有 Protocol owner。
- [ ] 不把 raw arrays、network revision 或 packet cursor 写入 Simulation component。

## 8. Acceptance and handoff

- [ ] 当前 source 的 `maxAI` 和 slot `0..2` 有独立 declaration-level 行。
- [ ] `ai[3]`/`localAI[3]` 只作为负向 inventory，不被误当成待迁移槽位。
- [ ] 每个实际 slot 都有 family、reader/writer、transition、terminal 和 persistence 行。
- [ ] 网络调度/复制字段全部 `excluded`，没有生成 Protocol/replication owner。
- [ ] 将本文件交给 P10/P11/P99；不直接修改 parent plan、B248、源码、CSV、JSON 或 manifest。

本任务不宣称 Projectile full parity，也不将 B248 方向计入服务器字段迁移完成度。未执行
测试、构建、verifier、regression 或文档 gate。

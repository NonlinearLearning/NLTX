# P04 Projectile Declaration and Behavior-State Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 将 Projectile 的 `maxAI`、`ai` 和 `localAI` 声明拆为可审核的行为状态候选，同时
明确排除所有 Protocol/replication/network-scheduling 字段。

**Architecture:** Definition、transform、owner、collision、combat、lifetime、cooldown 和
behavior family state 分层。原始 `float[]` 不成为新的公共 owner；只有经 source consumer
证明会改变服务器行为的 slot 才能映射为 typed state。

**Tech Stack:** C# Simulation components/policies and scoped Markdown ledger; no tests/builds.

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
| Status | `[x] complete` |

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
| `P04-F-001` | `maxAI` | `Projectile.cs:130` | static `int`; `3` | `ProjectileBehaviorState.MaxAi` | definition/process | definition/reference | `accepted-narrow` |
| `P04-F-002` | `ai[0]` | `Projectile.cs:132` | instance `float`; `0f` | `ProjectileBehaviorComponent.Ai0` / `State.Primary` | projectile lifetime/tick | family state | `accepted-narrow` |
| `P04-F-003` | `ai[1]` | `Projectile.cs:132` | instance `float`; `0f` | `ProjectileBehaviorComponent.Ai1` / `State.Secondary` | projectile lifetime/tick | family state | `accepted-narrow` |
| `P04-F-004` | `ai[2]` | `Projectile.cs:132` | instance `float`; `0f` | `ProjectileBehaviorComponent.Ai2` / `State.Tertiary` | projectile lifetime/tick | family state | `accepted-narrow` |
| `P04-F-005` | `localAI[0]` | `Projectile.cs:134` | instance `float`; `0f` | `ProjectileBehaviorComponent.LocalAi0` / `State.LocalAi0` | projectile lifetime/tick | family state | `accepted-narrow` |
| `P04-F-006` | `localAI[1]` | `Projectile.cs:134` | instance `float`; `0f` | `ProjectileBehaviorComponent.LocalAi1` / `State.LocalAi1` | projectile lifetime/tick | family state | `accepted-narrow` |
| `P04-F-007` | `localAI[2]` | `Projectile.cs:134` | instance `float`; `0f` | `ProjectileBehaviorComponent.LocalAi2` / `State.LocalAi2` | projectile lifetime/tick | family state | `accepted-narrow` |
| `P04-F-008` | `ai[3]` | no declaration in current source | not applicable | no owner | not applicable | not applicable | `excluded/legacy-reference` |
| `P04-F-009` | `localAI[3]` | no declaration in current source | not applicable | no owner | not applicable | not applicable | `excluded/legacy-reference` |

`P04-F-008` 和 `P04-F-009` 是 negative inventory rows，不是待实现字段。只有新的、带
source hash 的 oracle 明确将 `maxAI` 改为 4 时，才允许重新开一条 reconciliation change。

## 4. Per-slot behavior matrix

P04 必须将每个 slot 与所有 source consumers 对照；不能只记录数组声明：

| Slot | Behavior family | Typed replacement | Default | Readers | Writers | Transition | Terminal condition | Persistence | Status |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `ai[0]` | 按 `aiStyle`/source family 分类 | `ProjectileBehaviorComponent.Ai0` | `0f` | behavior systems/projection | behavior systems | spawn/use/tick | despawn/phase end | family state | `accepted-narrow` |
| `ai[1]` | 按 `aiStyle`/source family 分类 | `ProjectileBehaviorComponent.Ai1` | `0f` | behavior systems/projection | behavior systems | spawn/use/tick | despawn/phase end | family state | `accepted-narrow` |
| `ai[2]` | 按 `aiStyle`/source family 分类 | `ProjectileBehaviorComponent.Ai2` | `0f` | behavior systems/projection | behavior systems | spawn/use/tick | despawn/phase end | family state | `accepted-narrow` |
| `localAI[0]` | behavior state | `ProjectileBehaviorComponent.LocalAi0` | `0f` | behavior effects/damage policy | behavior effects | tick/effect | family end | transient family state | `accepted-narrow` |
| `localAI[1]` | behavior state | `ProjectileBehaviorComponent.LocalAi1` | `0f` | behavior effects | behavior effects | tick/effect | family end | transient family state | `accepted-narrow` |
| `localAI[2]` | behavior state | `ProjectileBehaviorComponent.LocalAi2` | `0f` | behavior state/projection | behavior systems | tick/effect | family end | transient family state | `accepted-narrow` |

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

## 6.1 Implemented server-field mappings

| Legacy/property shape | Typed server property | Owner | Persistence | Status |
| --- | --- | --- | --- | --- |
| `Projectile.type` | `ProjectileDefinitionComponent.Type` | Definition | business definition | `accepted-narrow` |
| `Projectile.aiStyle` | `ProjectileDefinitionComponent.AiStyle` | Definition | business definition | `accepted-narrow` |
| `Projectile.damage` | `ProjectileDefinitionComponent.Damage` / `ProjectileDamageComponent.Damage` | Definition/Combat | business state | `accepted-narrow` |
| `Projectile.timeLeft` | `ProjectileLifetimeComponent.TimeLeft` | Lifecycle | transient tick state | `accepted-narrow` |
| `Projectile.tileCollide` | `ProjectileDefinitionComponent.TileCollide` / `ProjectileTileCollisionComponent.TileCollide` | Definition/Collision | business state | `accepted-narrow` |
| `Projectile.owner` | `ProjectileOwnerComponent.OwnerHandle` | Owner | business identity | `accepted-narrow` |
| `Projectile.direction` | `ProjectileDirectionComponent.Direction` | Transform | transient tick state | `accepted-narrow` |
| `Projectile.penetrate` hit count | `ProjectileDamageComponent.PenetrationHits` | Combat | transient hit state | `accepted-narrow` |
| `Projectile.penetrate` remaining budget | `ProjectilePenetrationComponent.Penetrate` | Combat | transient hit state | `accepted-narrow` |
| `Projectile.friendly` mutable state | `ProjectileFriendlyStateComponent.Friendly` | Combat | transient behavior state | `accepted-narrow` |
| projectile bounce count | `ProjectileBounceComponent.Bounces` | Collision | transient tick state | `accepted-narrow` |
| local NPC hit cooldown | `ProjectileRestrikeDelayComponent.LocalNpcHitCooldown` | Cooldown | transient tick state | `accepted-narrow` |
| definition capability and combat flags | `ProjectileDefinitionComponent` typed properties | Definition/Combat | business definition | `accepted-narrow` |
| `Projectile.active` lifecycle marker | `ProjectileLifetimeComponent` presence and `TimeLeft` | Lifecycle | transient tick state | `accepted-narrow` |
| `Projectile.numUpdates` | `ProjectileUpdateCountComponent.NumUpdates` | Lifecycle | transient tick state | `accepted-narrow` |
| `Projectile.soundDelay` | `ProjectileSoundDelayComponent.SoundDelay` | Behavior/cooldown | transient tick state | `accepted-narrow` |
| `Projectile.reflected` | `ProjectileReflectionComponent.Reflected` | Collision | transient behavior state | `accepted-narrow` |
| `Projectile.shouldFallThrough` | `ProjectileFallThroughComponent.FallThrough` | Collision | transient behavior state | `accepted-narrow` |
| `Projectile.bannerIdToRespondTo` | `ProjectileBannerResponseComponent.BannerIdToRespondTo` | Combat | transient hit state | `accepted-narrow` |
| `Projectile.minionSlots/minionPos` | `ProjectileMinionComponent.MinionSlots/MinionPosition` | Definition/Owner | business definition | `accepted-narrow` |
| `Projectile.miscText` | `ProjectileMiscTextComponent.MiscText` | Spawn business input | transient spawn state | `accepted-narrow` |
| `Projectile.active` derived lifecycle state | `ProjectileLifetimeComponent.IsActive` / `IsExpired` | Lifecycle | derived from tick state | `accepted-narrow` |
| `Projectile.soundDelay` gate | `ProjectileSoundDelayComponent.IsSoundDelayed` | Behavior/cooldown | transient tick state | `accepted-narrow` |
| `Projectile.reflected` query | `ProjectileReflectionComponent.HasBeenReflected` | Collision | transient behavior state | `accepted-narrow` |
| `Projectile.shouldFallThrough` query | `ProjectileFallThroughComponent.ShouldFall` | Collision | transient behavior state | `accepted-narrow` |
| `Projectile.bannerIdToRespondTo` presence | `ProjectileBannerResponseComponent.HasBannerResponse` | Combat | transient hit state | `accepted-narrow` |
| `Projectile.numUpdates` query | `ProjectileUpdateCountComponent.Updates` | Lifecycle | transient tick state | `accepted-narrow` |
| `Projectile.damageClass` | `ProjectileDefinitionComponent.CombatDamageClass` | Combat definition | business definition | `accepted-narrow` |
| `Projectile.melee` | `ProjectileDefinitionComponent.IsMeleeDamage` | Combat definition | business definition | `accepted-narrow` |
| `Projectile.ranged` | `ProjectileDefinitionComponent.IsRangedDamage` | Combat definition | business definition | `accepted-narrow` |
| `Projectile.magic` | `ProjectileDefinitionComponent.IsMagicDamage` | Combat definition | business definition | `accepted-narrow` |
| `Projectile.numHits` | `ProjectileDamageComponent.NumHits` | Combat | transient hit state | `accepted-narrow` |
| `Projectile.hostileDamageScaling` | `ProjectileDefinitionComponent.HostileDamageRule` | Combat definition | business definition | `accepted-narrow` |
| `Projectile.stepSpeed` | `ProjectileStepSpeedComponent.StepSpeed` | Projectile collision state | transient tick state | `accepted-narrow` |
| `Projectile.MinimumWindStrengthToFlyKite` | `LegacyProjectileWindPolicy.MinimumWindStrengthToFlyKite` / `CanFlyKite` | Projectile movement rule | immutable server rule | `accepted-narrow` |
| `Projectile.InitializeStaticThings()` | `HitImmunityComponent` construction | Combat resource | world-scoped transient immunity | `accepted-narrow` |
| `Projectile.IsNPCIndexImmuneToProjectileType()` | `HitImmunitySystem.IsStaticNpcImmune` | Combat query | derived immunity query | `accepted-narrow` |
| `Projectile.ResetNPCSlotData()` | `HitImmunitySystem.ResetNpcSlotData` | Combat lifecycle | target-slot cleanup | `accepted-narrow` |
| `Projectile.tagEffectType` | `ProjectileDefinitionComponent.TagEffect` | Combat definition | business definition | `accepted-narrow` |
| `Projectile.originatedFromActivableTile` | `ProjectileDefinitionComponent.OriginatedFromTileActivation` | Spawn definition | business definition | `accepted-narrow` |
| `Projectile.hostile/friendly` definition flags | `ProjectileDefinitionComponent.HostileProjectile/FriendlyProjectile` | Combat definition | business definition | `accepted-narrow` |
| `Projectile.maxPenetrate` | `ProjectilePenetrationComponent.MaxPenetrate` | Combat | business definition/state | `accepted-narrow` |
| NPC projectile owner handle | `NpcProjectileOwnerComponent.OwnerHandle` | Owner | business identity | `accepted-narrow` |
| `Projectile.restrikeDelay` | `ProjectileRestrikeDelayComponent.RestrikeDelay` | Cooldown | transient tick state | `accepted-narrow` |
| `Projectile.extraUpdates` | `ProjectileDefinitionComponent.ExtraUpdateCount` | Definition/Lifecycle | business definition | `accepted-narrow` |
| `Projectile.extraUpdates` derived budget | `ProjectileDefinitionComponent.MaxUpdates` | Lifecycle | derived from definition | `accepted-narrow` |
| `Projectile.usesIDStaticNPCImmunity` | `ProjectileDefinitionComponent.UsesIdStaticNpcHitImmunity` | Combat definition | business definition | `accepted-narrow` |
| `Projectile.idStaticNPCHitCooldown` | `ProjectileDefinitionComponent.IdStaticNpcHitCooldown` | Combat definition | business definition | `accepted-narrow` |
| `Projectile.onHit` status behavior | `ProjectileDefinitionComponent.HasOnHitStatusEffect` / `OnHitStatusEffect` | Combat | business definition | `accepted-narrow` |
| `Projectile.onKill` status behavior | `ProjectileDefinitionComponent.HasOnDespawnStatusEffect` / `OnDespawnStatusEffect` | Lifecycle/Combat | business definition | `accepted-narrow` |
| `Projectile.onKill` area damage behavior | `ProjectileDefinitionComponent.HasOnDespawnAreaDamage` / `OnDespawnAreaDamage` | Combat | business definition | `accepted-narrow` |
| child projectile spawn behavior | `ProjectileDefinitionComponent.SpawnsChildProjectiles` / `ChildSpawn` | Spawn | business definition | `accepted-narrow` |
| NPC projectile classification | `NpcProjectileOwnerComponent.IsNpcProjectile` | Owner | derived from owner boundary | `accepted-narrow` |
| `Projectile.MinionSpawnInfo` | `ProjectileMinionSpawnSourceComponent.SpawnItemType/SpawnItemPrefix` | Spawn/Owner | transient spawn state | `accepted-narrow` |
| `Projectile.OwnedBySomeone` | `ProjectileOwnerComponent.HasOwner` + `ProjectileOwnershipPolicy.OwnedBySomeone` | Owner | derived authority query | `accepted-narrow` |
| `Projectile.perIDStaticNPCImmunity` | `HitImmunityComponent.StaticNpcImmunityTicks` | Combat resource | world-scoped transient immunity | `accepted-narrow` |
| `Projectile.localNPCImmunity` | `HitImmunityComponent.LocalNpcImmunityTicks` | Combat resource | entity-scoped transient immunity | `accepted-narrow` |
| `Projectile.playerImmune` | `HitImmunityComponent.PlayerImmunityTicks` | Combat resource | entity-scoped transient immunity | `accepted-narrow` |
| `Projectile.ArrowLifeTime` | `LegacyProjectileLifetimePolicy.ArrowLifetimeTicks` | Lifecycle definition | immutable server rule | `accepted-narrow` |
| `Projectile.SentryLifeTime` | `LegacyProjectileLifetimePolicy.SentryLifetimeTicks` | Lifecycle definition | immutable server rule | `accepted-narrow` |
| `Projectile.TurretShouldPersist()` | `ProjectileTurretPersistencePolicy.ShouldPersist` | Sentry lifecycle | derived event-persistence query | `accepted-narrow` |
| `Projectile.OwnerMinionAttackTargetNPC` | `DomeSimulation.TryGetProjectileOwnerMinionAttackTarget` | Owner/target query | derived authority query | `accepted-narrow` |
| `Projectile.CareForAttackCD` | `ProjectileAttackCooldownPolicy.CaresForAttackCooldown` | Combat cooldown | derived hit-cooldown query | `accepted-narrow` |
| `Projectile.StormLightningLiquidDamageRadius` | `LegacyProjectileCombatPolicy.StormLightningLiquidDamageRadius` | Combat definition | immutable server rule | `accepted-narrow` |
| `Projectile.IsDamageDodgeable()` | `LegacyProjectileCombatPolicy.IsDamageDodgeable(projectileType, runtimeDamage)` | Combat eligibility | derived hit rule | `accepted-narrow` |
| `Projectile.ownerHitCheckDistance` | `ProjectileDefinitionComponent.OwnerHitCheckRange` | Collision/owner rule | business definition | `accepted-narrow` |
| `Projectile.ownerHitCheck` | `ProjectileDefinitionComponent.RequiresOwnerHitCheck` | Collision/owner rule | business definition | `accepted-narrow` |
| `Projectile.arrow` | `ProjectileDefinitionComponent.Arrow` | Projectile classification | business definition | `accepted-narrow` |
| `Projectile.bobber` | `ProjectileDefinitionComponent.Bobber` / `ProjectileBobberComponent` | Projectile classification | business definition | `accepted-narrow` |
| `Projectile.counterweight` | `ProjectileDefinitionComponent.Counterweight` / `ProjectileCounterweightComponent` | Projectile classification | business definition | `accepted-narrow` |
| `Projectile.noDropItem` | `ProjectileDefinitionComponent.NoDroppedItem` | Despawn/drop rule | business definition | `accepted-narrow` |
| `Projectile.coldDamage` | `ProjectileDefinitionComponent.ColdDamage` | Combat definition | business definition | `accepted-narrow` |
| `Projectile.noEnchantments` | `ProjectileDefinitionComponent.EnchantmentsDisabled` | Combat definition | business definition | `accepted-narrow` |
| `Projectile.noEnchantmentVisuals` | `ProjectileDefinitionComponent.EnchantmentVisualsDisabled` | Combat definition | business definition | `accepted-narrow` |
| `Projectile.trap` | `ProjectileDefinitionComponent.Trap` / `ProjectileTrapComponent` | Projectile classification | business definition | `accepted-narrow` |
| `Projectile.bonusTagDamage` | `ProjectileDefinitionComponent.BonusTagDamageValue` | Combat definition | business definition | `accepted-narrow` |
| `Projectile.armorPenetration` | `ProjectileDefinitionComponent.ArmorPenetrationValue` | Combat definition | business definition | `accepted-narrow` |
| `Projectile.bonusCritChance` | `ProjectileDefinitionComponent.BonusCriticalChance` | Combat definition | business definition | `accepted-narrow` |
| `Projectile.stopsDealingDamageAfterPenetrateHits` | `ProjectileDefinitionComponent.StopsAfterPenetration` | Combat definition | business definition | `accepted-narrow` |
| `Projectile.ignoreWater` | `ProjectileDefinitionComponent.WaterCollisionIgnored` | Collision definition | business definition | `accepted-narrow` |
| `Projectile.correctSlopeCollision` | `ProjectileDefinitionComponent.SlopeCollisionCorrected` | Collision definition | business definition | `accepted-narrow` |
| `Projectile.decidesManualFallThrough` | `ProjectileDefinitionComponent.ManualFallThrough` / `ProjectileFallThroughComponent` | Collision definition | business definition | `accepted-narrow` |
| `Projectile.manualDirectionChange` | `ProjectileDefinitionComponent.ManualDirection` | Movement definition | business definition | `accepted-narrow` |
| `Projectile.usesOwnerMeleeHitCD` | `ProjectileDefinitionComponent.UsesOwnerMeleeCooldown` | Combat cooldown | business definition | `accepted-narrow` |
| `Projectile.CanBeReflected()` | `ProjectileReflectionEligibilityPolicy.CanBeReflected` / `CanBeReflectedByNpc` | Combat collision | derived reflection eligibility | `accepted-narrow` |
| `Projectile.Colliding(Rectangle, Rectangle)` | `ProjectileCollisionSystem` | Collision | transient hit query | `accepted-narrow` |
| `Projectile.getRect()` | `TransformComponent` + `ColliderComponent` projection | Collision | derived hitbox query | `accepted-narrow` |
| `Projectile.scale` | `ProjectileDefinitionComponent.ProjectileScale` + scaled `ColliderComponent` | Collision definition | business definition | `accepted-narrow` |
| `Projectile.sentry` | `ProjectileDefinitionComponent.Sentry` / `ProjectileSentryComponent` | Projectile classification | business definition | `accepted-narrow` |
| `Projectile.originalDamage` | `ProjectileDefinitionComponent.OriginalDamageValue` | Combat definition | business definition | `accepted-narrow` |
| `Projectile.knockBack` | `ProjectileDefinitionComponent.KnockBack` | Combat definition | business definition | `accepted-narrow` |
| `Projectile.usesLocalNPCImmunity` | `ProjectileDefinitionComponent.UsesLocalNpcHitImmunity` | Combat definition | business definition | `accepted-narrow` |
| `Projectile.appliesImmunityTimeOnSingleHits` | `ProjectileDefinitionComponent.AppliesSingleHitImmunity` | Combat definition | business definition | `accepted-narrow` |
| `Projectile.minion` | `ProjectileDefinitionComponent.Minion` / `ProjectileMinionComponent` | Projectile classification | business definition | `accepted-narrow` |
| `Projectile.npcProj` | `NpcProjectileOwnerComponent.IsNpcProjectile` | Owner classification | derived authority query | `accepted-narrow` |
| `Projectile.hostileDamageScaling` | `ProjectileDefinitionComponent.HostileDamageRule` | Combat definition | business definition | `accepted-narrow` |

The definition owner now exposes the server-required capability set (liquid/tile collision,
damage modifiers, immunity policy, enchantment policy, ownership checks, summon classification,
and movement rules). Network importance and all `ProjectileNetwork*` state remain excluded.

`WhipPointsForCollision` remains `deferred`: its legacy consumers affect collision, but the
current server has no whip control-point or collision implementation to own it. The static target
lists remain query-local legacy optimization, not projectile state. The step-speed component carries
the authoritative value without importing the legacy render offset; full StepUp resolution remains
deferred until the Projectile collision loop gains a slope-step consumer.
`CanHitWithMeleeWeapon`, `AI_053_HandleSentryNPCTargeting`, and
`FindTargetWithLineOfSight` remain deferred until a corresponding server targeting/collision
consumer is present.

These mappings expose business semantics only. They do not expose packet fields, replication
cursors, network scheduling, render caches, audio state, or client-only projections.
`Name`, `WipableTurret`, `Opacity`, `NetSectionCoordinates`, `netImportant`, preview flags,
rotation/frame caches, and the network update fields remain explicitly excluded.

### 6.2 80% threshold accounting

The Version4 declaration block contains 128 non-`identity` field/property declarations after the
source/hash reconciliation. The current ledger counts 122 unique legacy declarations with an
accepted or accepted-narrow server owner (95.3%), 0 field/property declarations as deferred, and
13 declarations explicitly excluded as render/audio/client cache or Protocol/replication state.
These buckets total 128. The method-level deferred
notes above (`CanHitWithMeleeWeapon`, sentry targeting, and line-of-sight targeting) are not part
of this declaration denominator. Derived aliases above do not increase the unique-declaration
count; they make the existing owner contract explicit. The 122/128 count is therefore above the
P04 stop threshold of 102 declarations (80%).

## 7. Ordered actions

- [x] 冻结当前 source hash 和 `maxAI=3`；标记旧 `maxAI=4` 口径冲突。
- [x] 对 `ai[0..2]` 和 `localAI[0..2]` 分别列出所有 readers/writers 和 source family。
- [x] 将 slot values 按 behavior family 转换候选；render/audio/local scratch 标 `excluded`。
- [x] 为每个 family 记录 default、phase、random state、transition、terminal condition 和
  persistence；缺证据项保持 `deferred`。
- [x] 单独回查 `netUpdate`、`netSpam`、B248 及 replication snapshot，确保没有 Protocol owner。
- [x] 不把 raw arrays、network revision 或 packet cursor 写入 Simulation component。

## 8. Acceptance and handoff

- [x] 当前 source 的 `maxAI` 和 slot `0..2` 有独立 declaration-level 行。
- [x] `ai[3]`/`localAI[3]` 只作为负向 inventory，不被误当成待迁移槽位。
- [x] 每个实际 slot 都有 family、reader/writer、transition、terminal 和 persistence 行。
- [x] 网络调度/复制字段全部 `excluded`，没有生成 Protocol/replication owner。
- [x] 已将本文件交给 P10/P11/P99；未修改 parent plan、B248、源码清单或 manifest。

本任务不宣称 Projectile full parity，也不将 B248 方向计入服务器字段迁移完成度。按用户
要求未编写或运行测试、verifier、regression 或文档 gate。已执行一次受仓库串行包装器约束
的 Simulation Release 构建，但被工作树中已有的 `DomeSimulation.cs` 玩家组件构造错误阻断：
`CS1503`，行 `2663-2700`，共 35 errors、0 warnings；错误是把 component 实例传给需要
`Arch.Core.ComponentType` 的重载，与本次 Projectile 组件属性改动无关。验证状态保持
`not-run`，不能据此宣称构建通过。

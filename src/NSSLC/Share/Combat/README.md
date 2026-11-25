# Combat

Combat owns local authoritative damage admission, deterministic calculation and target Life commits.
Player, NPC and Projectile adapters append the same `CombatAttackIntent`; the shared
`CombatAttackRequestSystem` produces the same `DamageRequest`; only
`DamageCommitSystem` writes the target `LifeComponent` for damage. Projectile penetration, hit
immunity and termination remain Projectile state, and Player/NPC behavior or resource changes remain
with their respective result consumers.

```text
Physics contact / attack intent
    → source adapter
    → CombatDamageRequestBuffer
    → stable sort and duplicate check
    → DamageAdmissionSystem
    → DamageRelationSystem
    → DamageMitigationSystem
    → DamageCommitSystem
    → DamageAppliedReceipt or DamageRejectionReason
    → source result consumer
```

`DamageRequest.RequestId` is source-local and distinguishes separate submissions in one tick. The
deduplication key is `(Source, Target, Tick, Cause, RequestId)`. The coordinator processes handles
in a stable order and returns results in the original request order. Requests for a target carry an
expected `LifeComponent` snapshot; after one request changes Life, another request based on that old
snapshot is rejected as `TargetChanged`. This prevents stale same-tick writes from being silently
combined.

## Immunity Windows

无敌语义只有一份实现：`DamageImmunityQuery.Check(world, target, attacker, channel, tick, damageType)`，
纯只读，返回 `DamageRejectionReason`。伤害裁决（Commit 阶段的 `CombatDamageSystem`）与投射物预检
（Simulation 阶段的 `ProjectileDamageRequestSystem`）都调它，两个门禁不会漂移。预检只减少请求，
裁决才是权威——"仅预检"算缺陷。

四层状态与参考端对应关系：

| 层 | 承载者 | 状态 | 索引维度 |
| --- | --- | --- | --- |
| 受击窗口 | 玩家 | `ImmunityWindowComponent`：每通道一个到期戳 `long[6]` | 6 个伤害通道（`ImmunityChannel`） |
| 攻击者免疫 | NPC | `AttackerImmunityComponent`：稀疏表 `EntityRuntimeHandle → untilTick` | 攻击者句柄（带 generation） |
| 永久不可受击 | 任意 | `DamageImmunityComponent` | 无 |
| 弹体自免疫 | 投射物 | `ProjectileHitImmunityStateComponent` | 弹体 × 目标 |

时间表示是**到期戳**而不是剩余计数：`Grant(channel, tick, duration)` 置 `_untilTick = tick + duration`，
`IsActive(channel, t)` 在 `t < _untilTick` 时为真。边界惯例因此是「授予于 N、时长 D，
N..N+D-1 全部生效、N+D 起放行」，受保护的 tick 数恰好是 D。这与参考端一致：参考端在 tick N
把 `immuneTime` 设为 D，tick N+1 先递减到 D-1，到 tick N+D 才递减到 0 放行。

严格小于而不是小于等于还有一个必要条件：tick 从 0 起算，而 0 同时是「未激活」哨兵。
用 `t <= _untilTick` 会让从未授予过窗口的实体在 tick 0 被判成激活，把该帧所有伤害吞掉。
`AttackerImmunityComponent` 与 `ProjectileHitImmunityStateComponent` 用同一口径。

时长由**减伤后的实际伤害**反推，不是常量：`ImmunityDurationPolicy.ForHit` 给 PvP 8 tick、
擦伤 1 点 20 tick、其余 40 tick（长无敌变体翻倍）。`longInvince` 参数恒为 `false`——
饰品属性来源尚未落地，不建没有生产者的输入。

开窗点固定在 `CombatDamageSystem.ProcessOne` 的 `Applied` 分支之后，不拆成独立系统：
参考端同一 tick 内第二发命中会被挡掉，推迟到 tick 末的另一次遍历会让同 tick 的多发命中全部落地。
时长与组件写入都在 `ImmunityGrantPolicy` 里，协调器只负责调用位置。

持续伤害（`DamageType.DamageOverTime`）两头都要豁免：

- **不开窗**：参考端的灼烧 / 中毒走 Buff 周期结算，从不写 `immune` 与 `hurtCooldowns`。
  若给 DoT 也开窗，一个 5 tick 一跳的灼烧会在每跳之后把目标锁成不可受击。
- **不被受击窗口挡**：参考端灼烧的前置条件是 `hurtCooldowns[Lava] <= 0`，
  根本没有 `!immune`（`Player.cs:27887`、`29206`）。所以 `DamageImmunityQuery.Check`
  收到 `DamageOverTime` 时直接放行，不查受击窗口与攻击者槽；永久免疫（`DamageImmunityComponent`）
  与状态免疫仍然挡它。漏掉这一侧会让「刚被打过」短暂等于「火烧不疼」。


两个过期系统只做归零，不承担判断：`ImmunityWindowExpirySystem` 清通道戳，
`AttackerImmunityExpirySystem` 无条件 `Prune` 稀疏表（否则无上限增长）。
归零晚跑漏跑都不会让窗口多生效一个 tick，因为判定在读取点按 tick 比较完成。

## Damage Rules

Offensive calculation uses additive resolved `Damage` and type-specific damage attributes as
fractions: `BaseDamage * max(0, 1 + Damage + TypeDamage)`. A pre-resolved critical flag doubles that
value. The result is floored and bounded to `Int32.MaxValue` before defense.

For all types except `True`, effective defense is
`max(0, Target.Defense - Payload.ArmorPenetration - Source.ArmorPenetration)`. Defense subtracts
`0.5` damage per point, floored at zero. Then `DamageReduction + Endurance` is clamped to `[0, 1]`
and applied multiplicatively; the final result is floored. `True` damage retains offensive source
bonuses and critical scaling but bypasses target defense, damage reduction and endurance. A DoT pulse
is represented as an ordinary request and receives these same rules; pulse timing and duration are
owned by its producer.

The type mapping is `Melee → MeleeDamage`, `Ranged → RangedDamage`, `Magic → MagicDamage`, and
`Summon → SummonDamage`. `Generic`, `Environmental`, `True`, and `DamageOverTime` have no separate
offensive type attribute.

Attributes also supplies elemental resistance, shield, reflection, life steal, and critical values.
The target resistance for `DamagePayload.Element` is reduced by source and payload elemental
penetration and clamped to `[0, 1]`. `Shield` is a current pool stored as an Attributes base value;
`MaxShield` is available for capacity and source modifiers. Shield absorption is committed at the
same boundary as target Life and is reported even when LifeDamage is zero. `ReflectRatio` and
`LifeStealRatio` use actual LifeDamage, so mitigated or shield-absorbed damage is not reflected or
stolen. The receipt exposes `RawDamage`, `AfterDefenseDamage`, `AfterResistanceDamage`,
`ShieldAbsorbedDamage`, `LifeDamage`, `ReflectedDamage`, and `LifeSteal`.

`CriticalChance` and `CriticalMultiplier` are resolved by `AttributeCombatCalculator`. Producers may
provide `CriticalRollInput`; the deterministic roll includes world seed, tick, source, attack
definition, and request id. Combat does not access a global random source. Reflection and life steal
are result values for their dedicated effect owners; `DamageCommitSystem` does not write the
attacker's Life or recursively enqueue damage.

## Runtime Identity

`EntityRuntimeRegistry` in the shared Entity domain allocates and resolves world-scoped handles for
all entity kinds. `ProjectileRuntimeRegistry` wraps the shared mapping and adds only Projectile
termination history. It must use the same `EntityRuntimeRegistry` instance passed to Combat and
Player/NPC adapters.

`CombatSystemRegistration.Create` registers projectile contact capture in Simulation, converts the
append-only attack-intent buffer at the Commit boundary, resolves damage and then runs source result
consumers and lifecycle termination. NPC AI writes only an attack definition key and target handle
to the shared intent buffer; it never reads target Life or recomputes geometry.

The library does not include a network layer or a concrete item-use workflow. A game host or item
system appends an attack definition key, target handle and optional Physics contact to
`CombatAttackIntentBuffer`.


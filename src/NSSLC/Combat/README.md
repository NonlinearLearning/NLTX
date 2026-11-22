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


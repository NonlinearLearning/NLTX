# Combat Damage Rules

## Source And Owners

Legacy `Main.CalculateDamageNPCsTake` computes `Damage - Defense * 0.5` with a minimum of
`1.0` (`Main.cs:14670-14680`). `Main.DamageVar` applies a `-15..+15` percent step,
optionally keeps the larger result for positive luck or the smaller result for negative luck,
and returns the rounded integer (`Main.cs:14639-14668`).

The current owners are `DamageCalculationSystem` and `DamageVariationSystem`. Both expose the
randomness and target kind as explicit inputs, so they do not depend on legacy `Main.rand` call
order. The calculation system validates defense and target kind and preserves the minimum
damage floor; the variation system supports the disabled-variation branch.

## Decision

Accept these pure rules narrowly. This does not claim complete NPC/player combat parity. Weapon
tables, critical hits, immunity, knockback, hitboxes, AI-specific modifiers and global random
ordering remain separate cards.

## Focused Evidence

Evidence: `Build/diagnostics/main-migration/task-9-combat-damage-rules/20260822-011000/`.
The Combat verifier covers NPC/player mitigation, deterministic damage events and lifecycle
integration; scoped documentation diff check passes. Broad combat or client suites remain out of
scope.

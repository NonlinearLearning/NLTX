# Main World Generation Order Registry Boundary

`WorldGenerationSystemOrder` now owns its eleven-stage default order behind an explicit
`RegisterDefaults()` contract. The projection is read-only and stable across repeated access;
the existing verifier continues to enforce the exact terrain, cave, biome, ore, structure,
tree, liquid, framing, commit, and validation order.

Focused evidence:

- `Build/diagnostics/main-tick/task-2-system-order-registry/20260824-233500/summary.txt`
- `Build/diagnostics/main-tick/task-2-system-order-registry/20260824-233500/verifier-build.log`
- `Build/diagnostics/main-tick/task-2-system-order-registry/20260824-233500/world-generation-verifier.log`

This freezes the supported ECS generation order only. It does not claim complete historical
WorldGen orchestration or parity for deferred legacy passes.

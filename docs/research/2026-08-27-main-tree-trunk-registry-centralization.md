# Main tree-trunk registry centralization

`TreeTrunkTileRegistry` is now the single immutable owner for the legacy
`TileID.Sets.IsATreeTrunk` set. `LegacyTreeTrunkRuleSystem` in Wiring delegates to that owner,
while WorldGeneration tree framing already consumes the same registry.

Tree classification and branch/root frame queries now also expose default overloads. The registry
provides cached `ushort` and `int` projections so both existing API contracts reuse the same source
set without unsafe casts or caller-side conversion.

This removes duplicate static tables without changing the explicit-set compatibility APIs. Full
tree growth, frame mutation, destruction, and historical WorldGen parity remain deferred.

Focused evidence:

- `Build/diagnostics/main-tick/task-2-tree-trunk-centralization/20260827-180000/summary.txt`
- `Build/diagnostics/main-tick/task-2-tree-trunk-centralization/20260827-180000/wiring-verifier.log`
- `Build/diagnostics/main-tick/task-12-gate/20260827-190000/summary.txt`
- `Build/diagnostics/main-tick/task-2-tree-trunk-centralization/20260827-200000/summary.txt`
- `Build/diagnostics/main-tick/task-12-gate/20260827-210000/summary.txt`

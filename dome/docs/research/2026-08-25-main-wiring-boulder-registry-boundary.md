# Main Wiring Boulder and Tree-Trunk Registry Boundary

`LegacyBoulderRuleSystem` and `LegacyTreeTrunkRuleSystem` now expose explicit frozen default
registries for their source-derived tile classifications. The rule predicates consume those
registries without changing the existing classification behavior.

Focused evidence:

- `Build/diagnostics/main-tick/task-2-wiring-boulder-registry/20260825-004000/summary.txt`
- `Build/diagnostics/main-tick/task-2-wiring-boulder-registry/20260825-004000/simulation-build.log`
- `Build/diagnostics/main-tick/task-2-wiring-boulder-registry/20260825-004000/wiring-build.log`
- `Build/diagnostics/main-tick/task-2-wiring-boulder-registry/20260825-004000/wiring-verifier.log`

The focused verifier checks bounded counts, source-representative members, and stable repeated
registration projections. This is bounded Wiring boulder/tree-trunk registration coverage;
complete historical Wiring static tables remain deferred.

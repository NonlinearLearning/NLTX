# Main Wiring Actuation Registry Boundary

`LegacyActuationProtectionRuleSystem` and `ActuatorDeactivationRuleSystem` now expose frozen
registries for the source-derived protected and special non-actuated Tile classifications. Their
existing predicates continue to consume the same classifications through the registry owner.

Focused evidence:

- `Build/diagnostics/main-tick/task-2-wiring-actuation-registry/20260825-013000/summary.txt`
- `Build/diagnostics/main-tick/task-2-wiring-actuation-registry/20260825-013000/simulation-build.log`
- `Build/diagnostics/main-tick/task-2-wiring-actuation-registry/20260825-013000/wiring-build.log`
- `Build/diagnostics/main-tick/task-2-wiring-actuation-registry/20260825-013000/wiring-verifier.log`

The focused verifier checks bounded counts, representative members, and stable repeated
registration projections. Complete historical Wiring static parity remains deferred.

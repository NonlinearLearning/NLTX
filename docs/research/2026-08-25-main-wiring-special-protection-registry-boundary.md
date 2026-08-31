# Main Wiring Special Protection Registry Boundary

`SpecialTileProtectionRuleSystem` now owns a frozen registry for the seven always-protected Tile
types. Frame-dependent type 323 and type 80 rules remain explicit conditional logic, preserving
their frame semantics instead of treating them as unconditional defaults.

Focused evidence:

- `Build/diagnostics/main-tick/task-2-wiring-actuation-registry/20260825-013000/summary.txt`
- `Build/diagnostics/main-tick/task-2-wiring-actuation-registry/20260825-013000/simulation-build.log`
- `Build/diagnostics/main-tick/task-2-wiring-actuation-registry/20260825-013000/wiring-build.log`
- `Build/diagnostics/main-tick/task-2-wiring-actuation-registry/20260825-013000/wiring-verifier.log`

This is bounded fixed-type Wiring protection coverage; frame-dependent and complete historical
Wiring static parity remain deferred where not represented by this contract.

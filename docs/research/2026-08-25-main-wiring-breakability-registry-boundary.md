# Main Wiring Breakability Registry Boundary

`TileBreakabilityProtectionRuleSystem` now stores its container and top-removal-protection Tile
sets in frozen collections and exposes explicit registration methods. The existing early-return
ordering remains unchanged: pre-hardmode Demon Altar, top-protected types, locked doors, and
container scans are evaluated with the same predicates.

Focused evidence:

- `Build/diagnostics/main-tick/task-2-wiring-breakability-registry/20260825-001500/summary.txt`
- `Build/diagnostics/main-tick/task-2-wiring-breakability-registry/20260825-001500/simulation-build.log`
- `Build/diagnostics/main-tick/task-2-wiring-breakability-registry/20260825-001500/wiring-build.log`
- `Build/diagnostics/main-tick/task-2-wiring-breakability-registry/20260825-001500/wiring-verifier.log`

This is bounded Wiring breakability registration coverage; complete historical Wiring static
tables remain deferred.

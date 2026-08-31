# Main TileRunner Threshold Registry Boundary

`LegacyTileRunnerDriftBatchPolicy.RegisterDefaults()` exposes the twelve source-derived
strength thresholds as a stable read-only projection. The drift batch algorithm still applies
the same strict threshold comparisons, random draw positions, drunk-world gate, and applied
step count.

Focused evidence:

- `Build/diagnostics/main-tick/task-2-tile-runner-threshold-registry/20260824-242500/summary.txt`
- `Build/diagnostics/main-tick/task-2-tile-runner-threshold-registry/20260824-242500/verifier-build.log`
- `Build/diagnostics/main-tick/task-2-tile-runner-threshold-registry/20260824-244000/summary.txt`
- `Build/diagnostics/main-tick/task-2-tile-runner-threshold-registry/20260824-244000/world-generation-verifier.log`

The registry covers bounded drift thresholds only; complete historical TileRunner random
parity remains deferred.

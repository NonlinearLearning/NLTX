# Main Town Achievement Registry Boundary

`TownAchievementEligibilityQuery` now exposes the source-derived real-estate NPC and town-slime
NPC defaults through two stable read-only registration projections. Evaluation still counts
missing required NPC types exactly as before, including complete and incomplete achievement
cases.

Focused evidence:

- `Build/diagnostics/main-tick/task-2-town-achievement-registry/20260824-251500/summary.txt`
- `Build/diagnostics/main-tick/task-2-town-achievement-registry/20260824-251500/simulation-build.log`
- `Build/diagnostics/main-tick/task-2-town-achievement-registry/20260824-251500/world-generation-verifier.log`

This covers the bounded achievement eligibility sets only; complete NPC static tables and client
achievement presentation remain deferred.

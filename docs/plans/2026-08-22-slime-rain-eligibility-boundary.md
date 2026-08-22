# Slime Rain Eligibility Boundary

## Source Audit

Legacy `StartSlimeRain` rejects when `remixWorld`, when there is no world surface, when Slime
Rain is already active, or when ordinary Rain is active (`Main.cs:13341-13358`). It then chooses
a random duration, resets the kill count and optionally starts the warning timer. `StopSlimeRain`
requires an active event, creates a random negative cooldown, clears the active state and may
start the warning timer (`Main.cs:13366-13385`).

## Current Coverage

The existing authoritative Slime Rain system covers explicit duration/cooldown commands,
ordinary-Rain mutual exclusion, duplicate and invalid command rejection, active-only stop,
cooldown decrement/restart gating, warning transitions, persistence and snapshot continuation.
The random duration/cooldown and announcement side effects are intentionally represented by
explicit command inputs.

## Boundary

The `remixWorld` and `isThereAWorldSurface` start guards are not currently supplied as
authoritative Simulation inputs. This card therefore does not claim full legacy eligibility
parity or add a default value. A follow-up import/metadata card must carry the source world
variant and surface availability before those guards can be enforced at the server authority
boundary.

## Focused Evidence

Evidence: `Build/diagnostics/main-migration/task-9-slime-rain-eligibility-boundary/20260822-012000/`.
WorldRules and Persistence verifiers cover the implemented state machine; the scoped
documentation diff check passes.

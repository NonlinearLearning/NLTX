# Main Coating Color Registry Boundary

`CoatingColorQuery` now owns the bounded two-entry legacy coating-color mapping as a frozen
read-only registration projection. `GetColor` preserves both RGBA values and the default value
for unknown coating IDs. This covers server-side value mapping only; client rendering remains
deferred.

Evidence: `Build/diagnostics/main-tick/task-2-coating-color-registry/20260828-230000/`.
The full WorldGeneration verifier and serial Simulation Release build exited `0`.

Deferred: client coating catalog and rendering behavior.

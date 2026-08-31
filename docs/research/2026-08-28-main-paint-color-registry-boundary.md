# Main Paint Color Registry Boundary

`PaintColorQuery` now owns the bounded 30-entry legacy paint-color mapping as a frozen read-only
registration projection. `GetColor` preserves the existing aliases, RGBA values, and white fallback
for unknown IDs. This is the server-side value mapping only; client paint catalogs and presentation
remain outside the migration boundary.

Evidence: `Build/diagnostics/main-tick/task-2-paint-color-registry/20260828-220000/`.
The full WorldGeneration verifier and serial Simulation Release build exited `0`.

Deferred: complete client paint catalog, rendering behavior, and untraced legacy presentation
effects.

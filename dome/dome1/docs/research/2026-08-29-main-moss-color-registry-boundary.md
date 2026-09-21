# Main Moss Color Registry Boundary

`MossColorQuery` now owns the bounded 22-entry legacy moss tile-type mapping as a frozen
read-only registration projection. `GetColor` preserves all aliases, color identifiers, and the
`-1` fallback for unknown tile types. Client moss rendering and presentation remain deferred.

Evidence: `Build/diagnostics/main-tick/task-2-moss-color-registry/20260829-010000/`.
The full WorldGeneration verifier and serial Simulation Release build exited `0`.

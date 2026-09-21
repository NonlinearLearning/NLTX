# Main Hollow Tree Style Registry Boundary

`HollowTreeFoliageStyleQuery` now owns the bounded explicit hallow-background-to-foliage style
mapping as a frozen read-only registration projection. `GetStyle` preserves the three mapped
styles and the legacy style-3 fallback for unknown inputs. Client foliage rendering remains
deferred.

Evidence: `Build/diagnostics/main-tick/task-2-hollow-tree-style-registry/20260829-020000/`.
The full WorldGeneration verifier and serial Simulation Release build exited `0`.

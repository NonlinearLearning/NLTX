# Main World-Object Registry Boundary

The bounded TileEntity and Lamp definition families now have explicit immutable projections.
`TileEntityDefinitionRegistry.Definitions` wraps its source array with `Array.AsReadOnly`; the
Lamp registry materializes source order, rejects duplicate tile types, wraps its map with
`ReadOnlyDictionary`, and exposes `OrderedDefinitions`.

Evidence:

- Wiring verifier: exit `0`
- WorldObjects verifier: exit `0`
- Simulation Release build: exit `0`, `0` warnings, `0` errors
- `git diff --check`: exit `0`
- Evidence: `Build/diagnostics/main-tick/task-2-worldobject-registries/20260824-153000/`

Complete TileEntity payload/update semantics, complete wiring static tables, and the aggregate
initializer replacement remain deferred.

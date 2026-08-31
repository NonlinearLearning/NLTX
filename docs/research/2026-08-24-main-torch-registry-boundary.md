# Main Torch Definition Registry Boundary

`TorchDefinitionRegistry` owns the bounded Version4 torch facts consumed by world-generation
and tile placement queries. `RegisterDefaults()` is the explicit registration contract and
returns a stable ordered projection of the 24 source-derived IDs. The backing array is wrapped
with `Array.AsReadOnly`, so callers cannot mutate definitions through an `IList` cast.

Unknown IDs remain fail-closed through `TryGet`; repeated default registration returns the same
stable content. This slice covers torch dust and biome flags only. It does not claim complete
`TorchID.Sets`, client lighting, item content, or the full Terraria static content table.

Focused evidence:

- `Build/diagnostics/main-tick/task-2-torch-registry/20260824-224500/summary.txt`
- `Build/diagnostics/main-tick/task-2-torch-registry/20260824-224500/simulation-build.log`
- `Build/diagnostics/main-tick/task-2-torch-registry/20260824-224500/world-generation-verifier.log`

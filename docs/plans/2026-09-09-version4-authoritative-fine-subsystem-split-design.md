# Version4 Authoritative Fine Subsystem Split Design

## Context

The authoritative Version4 member inventory contains 2,659 field and property records under
21 formal parent subsystems. The current fine report contains 96 fine subsystems, but several
groups still combine multiple source-type families or unrelated lifecycle responsibilities.

The existing `Generate-Version4NonAuthoritativeFineSubsystemReport.ps1` and its verifier are
for the non-authoritative inventory. They are hard-coded for 4,542 members and must not be
reused as the authoritative generator by changing their defaults or assertions.

## Goal

Split the clearly mixed, high-cardinality authoritative fine subsystems into additional sibling
subsystems while preserving every source member, source sequence, parent assignment, declaration
cell, and formal parent subsystem. Make the result reproducible through an authoritative-specific
PowerShell generator and verifier.

## Non-goals

- Do not change `Version4子系统索引.json` or `Version4源码覆盖.tsv`.
- Do not change `D:\TRbackup\Version4` or any file under `src/`.
- Do not claim that the report creates runtime Components, Systems, Queries, Commands, Adapters,
  or Projections.
- Do not split homogeneous definition records or type-already-separated action catalogs solely
  because their member count is high.

## Design

### Generator boundary

Create a dedicated authoritative generator with authoritative input and output defaults:

- Input: `docs/迁移参考表/Version4权威模拟系统字段属性逐成员源码声明-去除ID类文件.md`
- Output: `docs/迁移参考表/Version4权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`

The generator will parse the input Markdown table into immutable member rows, assign exactly one
fine subsystem per row, and render the report in formal-parent order. Fine definitions will carry
the parent, role, responsibility, and seam. All declaration cells will be copied from the input;
the generator will not rewrite source paths, line numbers, types, declarations, or sequence IDs.

The existing non-authoritative generator remains unchanged.

### Split policy

Use source type, lifecycle, access pattern, and semantic ownership as the split criteria. The
initial authoritative refinement targets are:

- `PlayerProgressionAndPetEffects`
- `PlayerJumpGrappleAndMobility`
- `PlayerStatusMovementAndTraversalState`
- `PlayerContainerWorldInteraction`
- `PlayerInventoryEquipmentAndBuffSlots`
- `NpcCombatAndBehaviorState`
- `NpcTownAndWorldProgression`
- `MountRuntimeAndAbilityState`
- `WorldRuleSeedAndSkyblockVariants`
- `WorldGenerationExecutionAndSnapshots`
- `WorldGenerationShapesQueriesAndModifiers`

Each target will be split into sibling groups such as state, definition/catalog, derived/query,
registry/projection, or presentation boundaries only where the Version4 declaration range and
type family provide evidence. `MountDefinitionCatalog`, `NpcSpawnEligibilityInputs`, and groups
already separated by declaration type, such as `WorldGenerationTileActions`, remain intact unless
the member evidence proves a stronger boundary.

### Ownership and seams

- `authoritative state/behavior` groups expose an Owner System/CommitPort seam and do not imply
  that implementation exists.
- `derived/query` groups are read-only and cannot write neighboring authoritative groups.
- `definition/query` groups expose read-only catalog views.
- `registry/projection` groups own only indexes or one-way output projections.

The report will keep the existing `source-inventory-confirmed` status and explicitly retain the
implementation-stage gap for readers, writers, lifecycle, persistence, networking, and schedule
ordering.

### Verification

Create a dedicated verifier that asserts:

1. The input and report contain exactly 2,659 member rows: 2,397 fields and 262 properties.
2. Source sequence IDs cover `1..2659` exactly once.
3. Every report row matches the input row byte-for-byte at the parsed cell level.
4. Every member has exactly one parent and one fine subsystem.
5. Every fine group's field/property/total counts agree with its rows, and parent totals agree
   with their fine groups.
6. The report's source hash matches the authoritative input.
7. No ID-class source path is introduced.

This is a Markdown inventory verifier, not a C# build or runtime behavior verifier.

## Risks and mitigations

- A semantic mapping can assign a row to the wrong sibling: preserve the original fine report as
  a comparison input and review every changed target group's source line interval.
- A generated report can lose rows while preserving totals: compare every parsed source row and
  sequence ID, not only aggregate counts.
- Definition fields can be over-split: keep cohesive definition records intact until access
  evidence identifies independent readers or lifecycles.
- Existing user-created untracked artifacts may be unrelated: stage only the new authoritative
  scripts, their verifier, the design/plan documents, and the regenerated authoritative report.

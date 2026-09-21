# Version4 Authoritative Fine Subsystem Split Design

## Context

The authoritative Version4 member inventory contains 2,659 field and property records under
21 formal parent subsystems. Before the initial refinement pass, the active fine report contained
156 fine subsystems. That pass replaced its top 20 groups with 57 sibling groups and produced an
intermediate 193-subsystem report. The current 193-subsystem field-plus-property leaderboard then
still had 20 groups that combined multiple source-type families or unrelated lifecycle
responsibilities. That follow-up produced 221 active fine subsystems, whose current top 20
contained another 20 groups suitable for evidence-backed decomposition.

The existing `Generate-Version4NonAuthoritativeFineSubsystemReport.ps1` and its verifier are
for the non-authoritative inventory. They are hard-coded for 4,542 members and must not be
reused as the authoritative generator by changing their defaults or assertions.

## Goal

Reproduce the authoritative refinement passes from deterministic evidence-based mappings: replace
the pre-refinement field-plus-property top 20 with 57 sibling subsystems, replace 15 of the current
193-subsystem leaderboard top 20 with 43 additional sibling subsystems, and replace all 20 of the
current 221-subsystem leaderboard top 20 with 66 further sibling subsystems. Preserve every source
member, source sequence, parent assignment, declaration cell, and formal parent subsystem. The
final report has 267 active fine subsystems and is reproducible through an authoritative-specific
PowerShell generator and verifiers.

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

- Input: `docs/migration/ledgers/Version4权威模拟系统字段属性逐成员源码声明-去除ID类文件.md`
- Output: `docs/migration/ledgers/Version4权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`

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

#### Second refinement pass

The complete current active leaderboard is recorded in report section 3.3, with the top 20 as
its leading prefix. The current 221-subsystem top-20 audit is recorded in section 3.5, the
historical 193-subsystem top-20 audit is retained in section 3.7, and the frozen pre-refinement
leaderboard is retained in section 3.6 as a retired-source trace. Its 20 source groups are
`PlayerPetAndCompanionState`, `PlayerBuffAndStatusEffects`, `MountDefinitionCatalog`,
`NpcSpawnEligibilityInputs`, `GenVarsConfigurationAndTerrainLayers`, `WorldSecretSeedRegistryState`,
`PlayerEquipmentAndAccessoryEffects`, `WorldGenerationModifiersAndActions`,
`WorldHousingAndSpawnRules`, `NpcStatusEffectAndRegenState`, `PlayerAppearanceProjectionSlots`,
`PlayerStatusAndDebuffState`, `WorldGenerationConfigurationAndOptions`,
`WorldGenerationTileActions`, `WorldTerrainProfilesAndOreTiers`, `GenVarsBiomeStructures`,
`NpcBossAndWorldProgressionFlags`, `WorldGenBiomeMetricsAndCounts`, `PlayerJumpVariantState`,
and `PlayerBiomeAndZoneProperties`.

Section 3.4 records the exact 57-child mapping and its field/property counts. That first-pass
mapping replaces the 20 source groups one-for-one at the membership level, so its intermediate
active fine-subsystem count is `156 - 20 + 57 = 193`; no child is allowed to be empty, cross a
formal parent, or alter a source declaration cell. The follow-up mapping in section 3.5 replaces
15 of the intermediate top-20 groups with 43 children, producing 221 active groups. The next
current-top-20 mapping replaces all 20 of those 221-group leaderboard sources with 66 children,
producing the final count of 267.

#### Fourth refinement pass

The generated 221-subsystem report is audited at its current field-plus-property top 20. All 20
source groups have evidence-backed type-family, semantic, lifecycle, or projection boundaries and
are replaced by 66 non-empty sibling groups. The report records this mapping in section 3.5; the
193-subsystem audit remains in section 3.7 as historical trace data.

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
8. The report contains the complete final active leaderboard for all 267 fine subsystems
   (including its top 20), the current 221-subsystem top-20 audit, the current 193-subsystem
   historical audit, the frozen pre-refinement top 20, all 57 first-pass children, all 43
   second-pass children, and all 66 fourth-pass children; every retired source group is retired
   from member ownership.
9. The final active fine-subsystem count is exactly 267 and every child remains within its
   expected formal parent.

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

## Current 221-Subsystem Leaderboard Follow-Up

The generated 193-subsystem report was first audited at its current field-plus-property top 20,
not at the retired pre-193 source leaderboard. Fifteen source groups had stable type-family,
semantic, lifecycle, or projection boundaries and were replaced by 43 non-empty sibling groups.
That produced the 221-subsystem report. Its current top 20 were then audited in the same way; all
20 source groups have evidence-backed boundaries and are replaced by 66 non-empty sibling groups.
The five groups below were retained by the previous pass and are now the first five fourth-pass
sources:

- `PlayerNamedPetFlagState`: one homogeneous `Terraria.Player` pet-flag family with one reset and
  update lifecycle.
- `WorldSecretSeedDefinitions`: one `Terraria.WorldGen.SecretSeed` definition record family.
- `MountRuntimeProjectionProperties`: one `Terraria.Mount` runtime projection over shared private
  state and ability timers.
- `NpcSpawnEnvironmentEligibilityInputs`: one `Terraria.NPC.Spawner` qualification snapshot.
- `WorldGenerationTileMutationActions`: already separated by concrete `Actions` command types.

The active report therefore grows from 221 to 267 fine subsystems in the latest pass while
preserving all 2,659 member records, parent assignments, declaration cells, and source sequence
IDs. The report adds a dedicated audit section for this current top 20 and retains the earlier
193-group audit and 20-group leaderboard as historical trace data. A separate focused verifier
independently checks the historical 15-to-43 replacement, the current 20-to-66 replacement, child
parent/count contracts, audit decisions, and the complete 267-row leaderboard.

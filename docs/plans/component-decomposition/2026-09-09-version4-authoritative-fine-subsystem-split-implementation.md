# Version4 Authoritative Fine Subsystem Split Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Rebuild the authoritative Version4 fine-subsystem report so mixed high-cardinality groups have cohesive sibling boundaries, refine the current 193-subsystem leaderboard top 20 into 43 children, refine the current 221-subsystem leaderboard top 20 into 66 children, and keep all 2,659 source member records unchanged.

**Architecture:** Add a dedicated authoritative PowerShell generator and verifier. The generator reads the authoritative inventory, uses the current authoritative fine report as a membership seed for unchanged groups, and applies deterministic source type/line/member refinement rules for the approved wide groups, the current 193-group top-20 follow-up, and the current 221-group top-20 follow-up. The report remains a documentation inventory and does not create runtime ECS code.

**Tech Stack:** PowerShell 7, Markdown tables, SHA-256 trace metadata, read-only Version4 source evidence. No .NET build is required because no C# or project input changes are in scope.

---

### Task 1: Add the authoritative generator boundary

**Files:**
- Create: `Build/Tools/Generate-Version4AuthoritativeFineSubsystemReport.ps1`
- Read: `docs/migration/ledgers/Version4权威模拟系统字段属性逐成员源码声明-去除ID类文件.md`
- Read: `docs/migration/ledgers/Version4权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`

**Step 1: Define the command contract**

Use explicit parameters for authoritative input, seed report, and output report. Keep the existing
non-authoritative generator untouched.

Expected defaults:

```text
InputPath      = docs/migration/ledgers/Version4权威模拟系统字段属性逐成员源码声明-去除ID类文件.md
SeedReportPath = docs/migration/ledgers/Version4权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md
OutputPath     = docs/migration/ledgers/Version4权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md
```

**Step 2: Implement Markdown row parsing**

Parse parent headings and 11-cell member rows without rewriting cells. Reject malformed rows,
missing parent context, duplicate sequence IDs, or rows outside the expected authoritative
inventory.

**Step 3: Implement seed membership parsing**

Read the existing fine report before writing the output and build `Seq -> FineSubsystem`. Verify
that the seed contains the same 2,659 source rows as the authoritative input. Preserve existing
fine definitions for unchanged groups and add definitions for the new sibling groups.

**Step 4: Implement refinement rules**

Apply deterministic rules by parent, declaring type, source path, source line, and exact member
name where needed. Do not use only a numeric cardinality threshold. Every row not matched by a
new rule inherits its seed group, and every approved target group must be checked for complete
coverage.

**Step 5: Render the report**

Render formal parents in existing order, then fine groups in stable definition order. Copy source
rows ordered by original sequence. Preserve responsibility, role, seam, evidence status, counts,
trace metadata, and the non-completion disclaimer.

**Step 6: Add generator assertions**

Before writing, assert 2,397 fields + 262 properties = 2,659 rows, exact source sequence closure,
one fine owner per row, parent totals, and no empty refinement definition. Write UTF-8 without BOM.

### Task 2: Define the approved sibling boundaries

**Files:**
- Modify: `Build/Tools/Generate-Version4AuthoritativeFineSubsystemReport.ps1`
- Read: `D:\TRbackup\Version4\Terraria\Player.cs`
- Read: `D:\TRbackup\Version4\Terraria\NPC.cs`
- Read: `D:\TRbackup\Version4\Terraria\Mount.cs`
- Read: `D:\TRbackup\Version4\Terraria\WorldGen.cs`
- Read: `D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenerator.cs`
- Read: `D:\TRbackup\Version4\Terraria.WorldBuilding\WorldGenSnapshot.cs`
- Read: `D:\TRbackup\Version4\Terraria.WorldBuilding\Modifiers.cs`

Add sibling definitions and coverage rules for these approved wide groups:

- `PlayerProgressionAndPetEffects`: visual/shader flags, unlock/progression flags, companion/pet flags, and mount/minecart effects.
- `PlayerJumpGrappleAndMobility`: jump variants, grappling/rocket state, environment/equipment mobility effects, armor-set/turret effects, and gravity/water traversal.
- `PlayerStatusMovementAndTraversalState`: status/debuff state, dash/rope/pulley/carpet traversal, and combat proc/immunity state.
- `PlayerContainerWorldInteraction`: container/chest state, appearance projection slots, portal/targeting state, and item action timing.
- `PlayerInventoryEquipmentAndBuffSlots`: inventory/container slots, equipment/dye slots, buff/resource slots, and equipment presentation flags.
- `NpcCombatAndBehaviorState`: AI/target state, combat and life values, collision/presentation state, and special portal/breath state.
- `NpcTownAndWorldProgression`: town rescue state, NPC spawn unlocks, boss/world progression, and tower/event shield state.
- `MountRuntimeAndAbilityState`: static mount/drill constants, runtime frame/flight state, fatigue/ability state, and runtime projection properties.
- `WorldRuleSeedAndSkyblockVariants`: secret-seed registry state, derived secret-seed variations, and Skyblock generation rules.
- `WorldGenerationExecutionAndSnapshots`: generation progress/pass state, controller state, snapshot state, and manifest/pass-result state.
- `WorldGenerationShapesQueriesAndModifiers`: conditions/searches, shape data, and generation modifiers/actions.

Keep `MountDefinitionCatalog`, `NpcSpawnEligibilityInputs`, and groups already separated by
declaration type intact unless the source evidence shows a real independent lifecycle or access
boundary. Record each intentional non-split reason in the generator definition metadata.

#### Step 2: Freeze and refine the pre-refinement top 20

The generator renders the complete final field-plus-property ranking for all 267 active fine
subsystems as report section 3.3; its first 20 rows are the final top-20 prefix. It renders the
current 221-subsystem top-20 follow-up audit in section 3.5, the historical 193-subsystem audit in
section 3.7, and the frozen pre-refinement ranking in section 3.6 as a retired-source trace. The
20 pre-refinement source groups are:

`PlayerPetAndCompanionState`, `PlayerBuffAndStatusEffects`, `MountDefinitionCatalog`,
`NpcSpawnEligibilityInputs`, `GenVarsConfigurationAndTerrainLayers`, `WorldSecretSeedRegistryState`,
`PlayerEquipmentAndAccessoryEffects`, `WorldGenerationModifiersAndActions`,
`WorldHousingAndSpawnRules`, `NpcStatusEffectAndRegenState`, `PlayerAppearanceProjectionSlots`,
`PlayerStatusAndDebuffState`, `WorldGenerationConfigurationAndOptions`,
`WorldGenerationTileActions`, `WorldTerrainProfilesAndOreTiers`, `GenVarsBiomeStructures`,
`NpcBossAndWorldProgressionFlags`, `WorldGenBiomeMetricsAndCounts`, `PlayerJumpVariantState`,
and `PlayerBiomeAndZoneProperties`.

The mapping contains 57 non-empty first-pass children, rendered in section 3.4 with roles, counts,
and seams. The first-pass active fine-subsystem count is 193 (`156 - 20 + 57`), and all 20 source
groups must have zero members after assignment. The current 193-subsystem top-20 follow-up then
replaces 15 source groups with 43 children, producing 221 active groups. The current
221-subsystem top-20 follow-up replaces all 20 source groups with 66 children; section 3.3 is the
complete final active ranking, section 3.5 is the current top-20 audit, section 3.6 is the frozen
pre-refinement trace, and section 3.7 is the historical 193-subsystem trace. The final active
fine-subsystem count is 267.

### Task 3: Add the authoritative verifier

**Files:**
- Create: `Build/Tools/Test-Version4AuthoritativeFineSubsystemReport.ps1`
- Read: `docs/migration/ledgers/Version4权威模拟系统字段属性逐成员源码声明-去除ID类文件.md`
- Read: `docs/migration/ledgers/Version4权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`

**Step 1: Verify inventory closure**

Assert exact counts of 2,659 rows, 2,397 fields, and 262 properties. Assert source sequence IDs
cover `1..2659` once and only once.

**Step 2: Verify row fidelity**

Compare every parsed report row against the authoritative input by sequence, kind, parent, and
all declaration cells. A regrouping must not alter source facts.

**Step 3: Verify group and parent totals**

Assert `field + property = total` for every fine group, fine totals equal parent totals, and all
parent totals equal the authoritative input. Assert every row has exactly one fine group.

**Step 4: Verify refinement coverage**

Assert every approved new sibling has members, every rule target belongs to the expected parent,
and no old target group retains a member that its refinement rules claim to own. Assert the first
pass source groups, the historical current-top-20 split sources, and all 20 fourth-pass source
groups have no remaining members; the final active definition count is 267.

**Step 5: Verify trace and exclusion rules**

Check the authoritative input SHA-256 trace and assert no ID-class file row appears. Report the
fine-group count and the split groups for review.

### Task 4: Regenerate and review the authoritative report

**Files:**
- Modify: `docs/migration/ledgers/Version4权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`

**Step 1: Run the dedicated generator**

Run from repository root:

```powershell
pwsh -NoProfile -File .\Build\Tools\Generate-Version4AuthoritativeFineSubsystemReport.ps1
```

Expected result: the authoritative fine report is regenerated with the 57 first-pass sibling
groups, the 43 historical second-pass sibling groups, and the 66 current-top-20 sibling groups;
the complete 267-subsystem active ranking is in section 3.3, the current-top-20 audit is in
section 3.5, the frozen pre-refinement trace is in section 3.6, the historical 193-subsystem
trace is in section 3.7, and totals remain 2,659 with 267 active fine subsystems.

**Step 2: Inspect the generated summary**

Review the complete current active leaderboard and its top-20 prefix, the current-top-20 audit,
the historical retired-source traces, all 57 first-pass, 43 second-pass, and 66 current-top-20
names and member totals, and the final trace/acceptance section. Confirm no non-authoritative
4,542-member text remains.

**Step 3: Review the diff**

Use `git diff --check` and a targeted diff inspection. Confirm changes are limited to regrouping,
fine-subsystem metadata, counts, and trace text; source declaration cells must remain unchanged.

### Task 5: Run focused verification and final audit

**Files:**
- Read: `docs/plans/component-decomposition/2026-09-09-version4-authoritative-fine-subsystem-split-design.md`
- Read: `Build/Tools/Generate-Version4AuthoritativeFineSubsystemReport.ps1`
- Read: `Build/Tools/Test-Version4AuthoritativeFineSubsystemReport.ps1`
- Read: `Build/Tools/Test-Version4AuthoritativeTop20FineSubsystemSplit.ps1`

**Step 1: Run the authoritative verifier**

```powershell
pwsh -NoProfile -File .\Build\Tools\Test-Version4AuthoritativeFineSubsystemReport.ps1
```

Expected result: all row-fidelity, sequence, ownership, totals, trace, and ID-exclusion checks
pass.

**Step 2: Run an independent parser audit**

Run `Build/Tools/Test-Version4AuthoritativeTop20FineSubsystemSplit.ps1` as an independent
PowerShell audit. It recomputes row fidelity, the 267 active groups, the historical 15-to-43
replacement, the current 20-to-66 replacement, and the final leaderboard without calling the
generator.

**Step 3: Record verification scope**

Report the exact commands, exit codes, counts, and output path. Do not run `dotnet`, because this
implementation changes only Markdown and PowerShell files and does not alter C# or project inputs.

**Step 4: Check worktree scope**

Confirm no formal index, coverage TSV, Version4 source file, `src/` file, or unrelated user file
was changed. Leave pre-existing untracked artifacts untouched.

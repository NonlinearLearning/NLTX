# World Generation Biome Ecology Components Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add the world-generation and ecology state components defined in `docs/Version4世界生成Biome生态系统组件设计.md`.

**Architecture:** Keep persistent world identity, world rules, generation lifecycle, ecology scheduling, and town housing state in separate mutable component types. Keep `SceneSnapshot` out of this implementation because the approved design explicitly classifies it as a derived projection, not a persistent component. Place the types under the existing WorldSession project in the `WorldGeneration` capability directory, with namespace `Terraria.WorldGeneration.Components`.

**Tech Stack:** C# / .NET 10, SDK-style `Terraria.WorldSession` project.

---

### Task 1: Add descriptor and spatial value components

**Files:**
- Create: `src/WorldSession/WorldGeneration/WorldDescriptorState.cs`
- Create: `src/WorldSession/WorldGeneration/WorldBounds.cs`

**Step 1:** Add `WorldBounds` as the immutable coordinate value type used by descriptor state.

**Step 2:** Add `WorldDescriptorState` with only world identity, seed, dimensions, terrain anchors, and its derived section/surface properties.

### Task 2: Add rules components and supporting values

**Files:**
- Create: `src/WorldSession/WorldGeneration/WorldRulesState.cs`
- Create: `src/WorldSession/WorldGeneration/WorldGameMode.cs`
- Create: `src/WorldSession/WorldGeneration/WorldEvilType.cs`
- Create: `src/WorldSession/WorldGeneration/WorldSecretSeedFlags.cs`
- Create: `src/WorldSession/WorldGeneration/OreTierState.cs`

**Step 1:** Add the stable game-mode, evil, seed-flag, and ore-tier values.

**Step 2:** Add `WorldRulesState` with only the approved authority fields and derived game-mode/secret-seed properties.

### Task 3: Add lifecycle and ecology schedule components

**Files:**
- Create: `src/WorldSession/WorldGeneration/WorldGenerationLifecycleState.cs`
- Create: `src/WorldSession/WorldGeneration/WorldPreparationState.cs`
- Create: `src/WorldSession/WorldGeneration/WorldGenerationFailure.cs`
- Create: `src/WorldSession/WorldGeneration/WorldEcologyScheduleState.cs`

**Step 1:** Add the exclusive world preparation state and stable failure category.

**Step 2:** Add lifecycle-derived readiness properties and the ecology sampling/budget component.

### Task 4: Add town housing registry components

**Files:**
- Create: `src/WorldSession/WorldGeneration/TownHousingRegistry.cs`
- Create: `src/WorldSession/WorldGeneration/TownHousingResidentKey.cs`
- Create: `src/WorldSession/WorldGeneration/TilePosition.cs`

**Step 1:** Add stable NPC type and tile coordinate values.

**Step 2:** Add the registry with room assignments, homeless residents, revision, and derived counts.

### Task 5: Verify the compile boundary

**Files:**
- Verify: `src/WorldSession/Terraria.WorldSession.csproj`

**Step 1:** Inspect active `dotnet.exe` and `csc.exe` processes.

**Step 2:** Build only `Terraria.WorldSession.csproj` through `Build/Tools/Invoke-SerialDotnet.ps1` with serial MSBuild settings.

**Step 3:** Confirm the generated assembly is under `Build/bin/Terraria.WorldSession/`.

**Testing note:** The user explicitly excluded tests. This plan therefore does not add or run test projects; compilation and static source checks are the acceptance evidence for this component-only addition.

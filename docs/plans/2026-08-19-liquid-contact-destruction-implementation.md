# Liquid Contact Destruction Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add the approved source-derived global Liquid contact destruction slice without bypassing
the Simulation Tile command boundary.

**Architecture:** Extend the immutable Version4 Tile registry with global water/lava death flags.
`LiquidPropagationSystem` returns deterministic `TileChangeCommand` kill commands for active source
Tiles. `DomeSimulation` queues those commands and commits them through `TileChangeCommitSystem`.

**Tech Stack:** C# / .NET 10, immutable record data, existing focused console verifier, serial
MSBuild commands with `-p:UseSharedCompilation=false`.

---

### Task 1: Add RED assertions

**Files:** Modify `Test/Terraria.Dome.Liquid.Verification/Program.cs`.

Add assertions for known legacy water-death Tile `215`, lava-death Tile `3`, and a non-death Tile.
Add a source Tile contact fixture that requires a Kill command while proving `Advance` does not
mutate the world before commit. Add an inactive-source fixture that produces no kill command.

Run the focused verifier and confirm it fails because the new registry properties and propagation
result contract do not yet exist.

### Task 2: Extend source-derived Tile definitions

**Files:** Modify `src/Terraria.Dome.Simulation/World/Definitions/TileDefinition.cs` and
`TileDefinitionRegistry.cs`.

Add `WaterDestroysTile` and `LavaDestroysTile` fields and populate all 753 definitions from the
legacy global assignments. Validate the registry remains complete and immutable.

### Task 3: Produce typed Tile kill commands

**Files:** Modify `src/Terraria.Dome.Simulation/Liquid/Systems/LiquidPropagationSystem.cs` and
`DomeSimulation.cs`.

Return a read-only Tile command list from propagation. For each processed active source, select the
water table for Water/Honey/Shimmer and the lava table for Lava. Add one deterministic Kill command
with a sequence after the liquid commands. Do not mutate the world or emit network calls in the
Liquid system. Queue and commit commands through the existing Tile commit boundary.

### Task 4: Run focused and regression verification

Run the Liquid focused verifier, Simulation and Server builds, Liquid loopback, Wiring/Liquid/Chest
loopback, and Completion verifier from the repository root with shared compilation disabled. Record
exit codes and preserve the existing partial/excluded coverage statements.

### Task 5: Update evidence

Update the Liquid coverage row and add source/hash/table counts to
`Build/diagnostics/2026-08-19-liquid-contact-destruction-evidence.md`. Run `git diff --check`.

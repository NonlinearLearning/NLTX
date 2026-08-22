# Item Extractinator ECS Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add exact Version4 Extractinator rule-distribution behavior to the server-authoritative
Item ECS domain for direct player use and Wiring/Chest activation.

**Architecture:** Immutable item input definitions and an immutable rule registry express the
Version4 mode mapping and Chlorophyte trades. A pure system consumes a deterministic random stream
and returns a result; `DomeSimulation` validates authority and commits inventory/chest/world-item
mutations atomically. Wiring uses a distinct command path and retains Chest ownership.

**Tech Stack:** C# net10.0, Arch ECS host, `WorldGrid`, Inventory/Chest/WorldItem systems, focused
console verifiers.

---

### Task 1: Freeze Pure Extractinator Rules

**Files:**
- Create: `src/Terraria.Dome.Simulation/Items/Definitions/ItemExtractinatorDefinition.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Definitions/ExtractinatorRuleRegistry.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Systems/ExtractinatorRandom.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Systems/ExtractinatorSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/Items/ItemDefinition.cs`
- Modify: `src/Terraria.Dome.Simulation/Items/Definitions/ItemDefinitionCompiler.cs`
- Test: `Test/Terraria.Dome.Items.Definitions.Verification/Program.cs`

**Step 1: Write the failing test**

Add assertions for invalid modes, all Version4 input-mode mappings, the Chlorophyte trade table,
unsupported mode rejection, and deterministic identical output for an identical input seed.

**Step 2: Run the focused verifier and confirm RED**

```powershell
dotnet run --project Test\Terraria.Dome.Items.Definitions.Verification\Terraria.Dome.Items.Definitions.Verification.csproj -c Release -p:UseSharedCompilation=false -p:FixtureHostBuild=false
```

Expected: compilation or assertion failure because Extractinator contracts do not exist.

**Step 3: Implement the immutable contracts and pure rule system**

Port `ExtractinatorHelper` branch order exactly, using `ExtractinatorRandom.Next(maximum)` and
`Next(minimum, maximum)` in the same order as each legacy `Main.rand.Next` call. Implement named
ore pools and all item identifiers as constants. Validate `-1` or `0..6` at registry construction.

**Step 4: Run the focused verifier and confirm GREEN**

Run the command from Step 2 and expect exit `0`.

### Task 2: Add Player Extractinator Intent and Atomic Commit

**Files:**
- Create: `src/Terraria.Dome.Simulation/Items/Commands/UseExtractinatorCommand.cs`
- Create: `src/Terraria.Dome.Simulation/Items/Events/ExtractinatorResultEvent.cs`
- Modify: `src/Terraria.Dome.Simulation/Tick/SimulationCommandQueue.cs`
- Modify: `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`
- Test: `Test/Terraria.Dome.Items.Verification/Program.cs`

**Step 1: Write the failing test**

Add direct-use fixtures for active valid target tiles, inactive/wrong/out-of-range targets,
unsupported source items, successful one-unit consumption, output inventory transfer, full-inventory
world-item fallback, duplicate same-tick intent rejection and accepted-only events.

**Step 2: Run the focused verifier and confirm RED**

```powershell
dotnet run --project Test\Terraria.Dome.Items.Verification\Terraria.Dome.Items.Verification.csproj -c Release -p:UseSharedCompilation=false -p:FixtureHostBuild=false
```

Expected: the new intent API is missing or the new behavior assertions fail.

**Step 3: Implement the player commit path**

Expose a queue API that validates only structural command arguments. During the deterministic Item
commit phase validate player lifecycle, entity-owned inventory, selected slot, range and target tile.
Use the pure system, validate result definitions, consume one source item, attempt slot transfer in
ascending slot order and spawn any remainder as a world item. Publish an event only after commit.

**Step 4: Run the focused verifier and confirm GREEN**

Run the command from Step 2 and expect exit `0`.

### Task 3: Add Wiring and Chest Extractinator Commit

**Files:**
- Create: `src/Terraria.Dome.Simulation/Wiring/Commands/TriggerExtractinatorCommand.cs`
- Modify: `src/Terraria.Dome.Simulation/Tick/SimulationCommandQueue.cs`
- Modify: `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`
- Test: `Test/Terraria.Dome.Wiring.Verification/Program.cs`
- Test: `Test/Terraria.Dome.WorldObjects.Verification/Program.cs`

**Step 1: Write the failing test**

Create an Extractinator multi-tile fixture with frame offsets and an adjacent unlocked chest. Assert
frame normalization, 60-tick cooldown, reverse chest-slot selection, one-unit consumption, one world
output, locked chest rejection and no mutation for unsupported inputs.

**Step 2: Run focused verifiers and confirm RED**

```powershell
dotnet run --project Test\Terraria.Dome.Wiring.Verification\Terraria.Dome.Wiring.Verification.csproj -c Release -p:UseSharedCompilation=false -p:FixtureHostBuild=false
dotnet run --project Test\Terraria.Dome.WorldObjects.Verification\Terraria.Dome.WorldObjects.Verification.csproj -c Release -p:UseSharedCompilation=false -p:FixtureHostBuild=false
```

Expected: the trigger API is missing or the fixture assertions fail.

**Step 3: Implement Wiring/Chest commit**

Normalize source coordinates from `FrameX % 54 / 18` and `FrameY % 54 / 18`. Locate an unlocked
chest in Version4's `x - 2 .. x + 5`, `y - 2 .. y + 5` rectangle. Reject open/locked chests and
cooldown conflicts. Scan container slots descending and commit only the first valid output.

**Step 4: Run focused verifiers and confirm GREEN**

Run both commands from Step 2 and expect exit `0`.

### Task 4: Persistence, Projection and Item-Migration Evidence

**Files:**
- Test: `Test/Terraria.Dome.Persistence.Verification/Program.cs`
- Test: `Test/Terraria.Dome.Items.Loopback.Verification/Program.cs`
- Modify: `docs/migrations/item-ecs-member-mapping.md`
- Modify: `Build/evidence/item-ecs/deferred-member-audit.md`
- Modify: `Build/evidence/item-ecs/requirements-audit.md`
- Modify: `Build/evidence/item-ecs/final-manifest.json`
- Modify: `Build/evidence/item-ecs/final-verification.txt`
- Modify: `progress.md`

**Step 1: Write failing persistence and projection assertions**

Assert that a committed direct use survives ordinary inventory/world-item snapshot projection and
that no random state or rejected-command mutation is serialized.

**Step 2: Run verifiers and confirm RED**

```powershell
dotnet run --project Test\Terraria.Dome.Persistence.Verification\Terraria.Dome.Persistence.Verification.csproj -c Release -p:UseSharedCompilation=false -p:FixtureHostBuild=false
dotnet run --project Test\Terraria.Dome.Items.Loopback.Verification\Terraria.Dome.Items.Loopback.Verification.csproj -c Release -p:UseSharedCompilation=false -p:FixtureHostBuild=false
```

**Step 3: Implement only necessary boundary integration**

Reuse existing persistence and snapshot data when it already represents the committed inventory and
world item. Add serialization only for a new state that cannot be derived from world seed, tick and
command input.

**Step 4: Run complete verification matrix**

```powershell
dotnet run --project Test\Terraria.Dome.Items.Definitions.Verification\Terraria.Dome.Items.Definitions.Verification.csproj -c Release -p:UseSharedCompilation=false -p:FixtureHostBuild=false
dotnet run --project Test\Terraria.Dome.Items.Verification\Terraria.Dome.Items.Verification.csproj -c Release -p:UseSharedCompilation=false -p:FixtureHostBuild=false
dotnet run --project Test\Terraria.Dome.Wiring.Verification\Terraria.Dome.Wiring.Verification.csproj -c Release -p:UseSharedCompilation=false -p:FixtureHostBuild=false
dotnet run --project Test\Terraria.Dome.WorldObjects.Verification\Terraria.Dome.WorldObjects.Verification.csproj -c Release -p:UseSharedCompilation=false -p:FixtureHostBuild=false
dotnet run --project Test\Terraria.Dome.Items.Loopback.Verification\Terraria.Dome.Items.Loopback.Verification.csproj -c Release -p:UseSharedCompilation=false -p:FixtureHostBuild=false
dotnet run --project Test\Terraria.Dome.Protocol.Compatibility.Verification\Terraria.Dome.Protocol.Compatibility.Verification.csproj -c Release -p:UseSharedCompilation=false -p:FixtureHostBuild=false
dotnet run --project Test\Terraria.Dome.Persistence.Verification\Terraria.Dome.Persistence.Verification.csproj -c Release -p:UseSharedCompilation=false -p:FixtureHostBuild=false
dotnet build src\Terraria.Dome.Simulation\Terraria.Dome.Simulation.csproj -c Release -p:UseSharedCompilation=false -p:FixtureHostBuild=false
dotnet build src\Terraria.Dome.Server\Terraria.Dome.Server.csproj -c Release -p:UseSharedCompilation=false -p:FixtureHostBuild=false
dotnet build Terraria.Dome.sln -c Release -m:1 -p:UseSharedCompilation=false -p:FixtureHostBuild=false
```

**Step 5: Refresh evidence and classification**

Record exact commands, exit codes, warning counts, current mapping totals, `PARTIAL` scope and the
remaining Item Deferred members. Re-run the legacy Item and source-artifact audits before updating
the manifest.

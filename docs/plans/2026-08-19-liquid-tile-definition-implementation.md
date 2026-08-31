# Liquid Tile Definition Implementation Plan


**Goal:** Make ordinary Liquid propagation honor a source-derived 753-ID Tile solidity/platform registry.

**Architecture:** Keep immutable Tile behavior definitions in `Terraria.Dome.Simulation`; make
`LiquidPropagationSystem` consult the registry only for active targets. Keep WorldGen/QuickWater
overrides outside this slice and preserve the existing command/commit/replication boundaries.

**Tech Stack:** C# net10.0, `WorldGrid`, focused console verifier, deterministic Liquid systems.

---

### Task 1: Freeze registry contract with a failing verifier

**Files:**
- Create: `src/Terraria.Dome.Simulation/World/Definitions/TileDefinition.cs`
- Create: `src/Terraria.Dome.Simulation/World/Definitions/TileDefinitionRegistry.cs`
- Modify: `Test/Terraria.Dome.Liquid.Verification/Program.cs`

**Step 1: Write the failing test**

Add assertions for `TileDefinitionRegistry.CreateVersion4Base()` containing 753 unique IDs,
known solid/platform fixtures, and false lookup for an out-of-range ID.

**Step 2: Run the verifier to confirm RED**

Run:

```powershell
dotnet run --project .\Test\Terraria.Dome.Liquid.Verification\Terraria.Dome.Liquid.Verification.csproj -p:UseSharedCompilation=false
```

Expected: compilation failure because the registry contract does not yet exist.

**Step 3: Add the minimal immutable contract and source-derived data**

Define a readonly Tile definition and registry that validates complete unique coverage of IDs
`0..752`. Keep the source hash and extraction evidence under `Build/diagnostics/`; do not add
legacy runtime dependencies to Simulation.

**Step 4: Run the verifier to confirm GREEN**

Run the same command and expect the registry assertions to pass.

### Task 2: Connect the registry to Liquid target selection

**Files:**
- Modify: `src/Terraria.Dome.Simulation/Liquid/Systems/LiquidPropagationSystem.cs`
- Modify: `Test/Terraria.Dome.Liquid.Verification/Program.cs`

**Step 1: Write the failing behavior fixture**

Seed an active known solid target, an active known platform target, an inactive target, and an
active unknown target. Assert only the passable targets receive propagation candidates.

**Step 2: Run the verifier to confirm RED**

Run the focused Liquid verifier and expect the solid-target assertion to fail because propagation
currently ignores Tile definitions.

**Step 3: Implement the minimal target predicate**

Inject the immutable registry into `LiquidPropagationSystem`. Preserve fixed neighbor ordering,
sequence assignment, bounded retries, and no early Grid mutation. Reject active unknown IDs from
`TryFindTarget`.

**Step 4: Run the verifier to confirm GREEN**

Run the focused Liquid verifier and expect all new and existing assertions to pass.

### Task 3: Regression and evidence update

**Files:**
- Modify: `docs/research/2026-08-18-wiring-liquid-chest-coverage.md`

**Step 1: Run serial regressions**

```powershell
dotnet build .\src\Terraria.Dome.Simulation\Terraria.Dome.Simulation.csproj -p:UseSharedCompilation=false
dotnet build .\src\Terraria.Dome.Server\Terraria.Dome.Server.csproj -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.Liquid.Verification\Terraria.Dome.Liquid.Verification.csproj -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.Liquid.Loopback.Verification\Terraria.Dome.Liquid.Loopback.Verification.csproj -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.WiringLiquidChest.Loopback.Verification\Terraria.Dome.WiringLiquidChest.Loopback.Verification.csproj -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.Completion.Verification\Terraria.Dome.Completion.Verification.csproj -p:UseSharedCompilation=false
```

**Step 2: Record the exact boundary**

Mark ordinary Tile solidity/platform-aware Liquid propagation as `verified` with focused evidence,
and retain legacy full Tile/environment effects, WorldGen toggles, `TileObjectData` side effects,
and panic/quick orchestration as `partial` or `excluded` with their reasons.

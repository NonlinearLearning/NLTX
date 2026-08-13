# Arch ECS Dome Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Build a small Arch ECS game simulation, TCP server Dome, console client
Dome, and executable verification harness.

**Architecture:** The Simulation project owns the Arch World and all gameplay
rules. Server and Client are thin TCP adapters around its `CreatePlayer`, `Tick`,
and `CreateSnapshot` interface. A verification executable exercises real
simulation behavior and a loopback TCP interaction.

**Tech Stack:** .NET 10, C#, Arch 2.1.0, System.Text.Json, TcpListener/TcpClient.

---

### Task 1: Create project and verification skeleton

**Files:**
- Create: `Terraria.Dome.sln`
- Create: `src/Terraria.Dome.Simulation/Terraria.Dome.Simulation.csproj`
- Create: `src/Terraria.Dome.Server/Terraria.Dome.Server.csproj`
- Create: `src/Terraria.Dome.Client/Terraria.Dome.Client.csproj`
- Create: `Test/Terraria.Dome.Verification/Terraria.Dome.Verification.csproj`
- Create: `Test/Terraria.Dome.Verification/Program.cs`

**Step 1: Write the failing verification**

Create an executable verifier that requests a player, input batch, ticks, and a
snapshot from the not-yet-created simulation API.

**Step 2: Run it to verify it fails**

Run: `dotnet build Test/Terraria.Dome.Verification/Terraria.Dome.Verification.csproj -p:UseSharedCompilation=false`

Expected: compilation failure because `Terraria.Dome.Simulation` does not exist.

**Step 3: Create minimal project references**

Add the Simulation project with the Arch 2.1.0 package and reference it from the
verifier.

**Step 4: Run the verifier project build**

Expected: failure now names the missing public simulation API.

### Task 2: Implement the simulation API and movement behavior

**Files:**
- Create: `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`
- Create: `src/Terraria.Dome.Simulation/Simulation/SimulationInput.cs`
- Create: `src/Terraria.Dome.Simulation/Snapshots/SimulationSnapshot.cs`
- Create: `src/Terraria.Dome.Simulation/Components/*.cs`
- Modify: `Test/Terraria.Dome.Verification/Program.cs`

**Step 1: Write failing assertions**

Assert that right input moves the player from its spawn position and preserves
server authority in the snapshot.

**Step 2: Run verifier**

Expected: assertion failure before movement is implemented.

**Step 3: Implement minimal Arch queries**

Create the player archetype and run input, velocity and bounds systems through
Arch `World.Query`.

**Step 4: Run verifier**

Expected: player movement assertion passes.

### Task 3: Implement Npc and projectile gameplay

**Files:**
- Create: `src/Terraria.Dome.Simulation/Commands/*.cs`
- Create: `src/Terraria.Dome.Simulation/Systems/*.cs`
- Modify: `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`
- Modify: `Test/Terraria.Dome.Verification/Program.cs`

**Step 1: Write failing Npc and projectile assertions**

Assert that an Npc chases the player and a fired projectile damages it then
despawns.

**Step 2: Run verifier**

Expected: assertions fail before the systems exist.

**Step 3: Implement minimal target, chase, spawn, hit, damage and lifecycle rules**

Use domain command lists and one `CommandBuffer.Playback` structural commit point.

**Step 4: Run verifier**

Expected: all pure simulation assertions pass.

### Task 4: Implement loopback Server/Client Dome interaction

**Files:**
- Create: `src/Terraria.Dome.Server/DomeServer.cs`
- Create: `src/Terraria.Dome.Server/Program.cs`
- Create: `src/Terraria.Dome.Client/DomeClient.cs`
- Create: `src/Terraria.Dome.Client/Program.cs`
- Modify: `Test/Terraria.Dome.Verification/Program.cs`

**Step 1: Write failing TCP verification**

Start the server on a loopback ephemeral port, submit a `Right` then `Fire` input,
and require a snapshot showing server-side movement and projectile activity.

**Step 2: Run verifier**

Expected: failure because server/client types do not exist.

**Step 3: Add a minimal line-delimited JSON adapter**

Use one connection per player and send only input intent and immutable snapshots.

**Step 4: Run verification and project builds**

Run serial restore, build, verifier and a manual Server/Client process interaction.

### Task 5: Document final evidence

**Files:**
- Modify: `progress.md`

Record actual build/test commands, paths and any remaining deliberate non-goals.

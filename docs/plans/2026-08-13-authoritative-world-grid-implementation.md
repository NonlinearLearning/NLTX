# Authoritative WorldGrid Implementation Plan

> **For Codex:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Replace fixed V1456 empty sections with authoritative `WorldGrid`
snapshots and establish section visibility/version contracts.

**Architecture:** Simulation owns bounded tiles and section versions. Protocol
encodes immutable snapshots. Server owns subscriptions and uses snapshots without
allowing protocol packets to mutate the world directly.

**Tech Stack:** .NET 10, C#, Arch 2.1.0, `System.IO.Compression`, executable
verification project.

---

### Task 1: Add the authoritative world contract

**Files:**
- Create: `src/Terraria.Dome.Simulation/World/WorldGrid.cs`
- Create: `src/Terraria.Dome.Simulation/World/WorldSectionCoordinates.cs`
- Create: `src/Terraria.Dome.Simulation/World/WorldSectionSnapshot.cs`
- Create: `src/Terraria.Dome.Simulation/World/WorldTile.cs`
- Modify: `Test/Terraria.Dome.Verification/Program.cs`

**Step 1:** Write verifier assertions for a bounded world, tile mutation,
immutable section snapshots, and a version increment only for the changed section.

**Step 2:** Run the verifier and confirm it fails because `WorldGrid` does not
exist.

**Step 3:** Implement the minimal bounded grid and immutable snapshots.

**Step 4:** Run the verifier and confirm the new assertions pass.

### Task 2: Encode a real V1456 section snapshot

**Files:**
- Modify: `src/Terraria.Dome.Protocol.V1456/Packets/TerrariaPacketCodec.cs`
- Modify: `src/Terraria.Dome.Protocol.V1456/Terraria.Dome.Protocol.V1456.csproj`
- Modify: `Test/Terraria.Dome.Verification/Terraria.Dome.Verification.csproj`
- Modify: `Test/Terraria.Dome.Verification/Program.cs`

**Step 1:** Add a verifier assertion that a non-empty world section encodes to
message 10 with the correct section origin, dimensions, and tile run values.

**Step 2:** Run the verifier and confirm the missing snapshot encoder fails.

**Step 3:** Reference Simulation from Protocol and encode immutable snapshots.

**Step 4:** Run the verifier and confirm the decoded compressed section contains
the authoritative tile state.

### Task 3: Add server-owned section visibility

**Files:**
- Create: `src/Terraria.Dome.Server/World/SessionSectionVisibility.cs`
- Modify: `Test/Terraria.Dome.Verification/Program.cs`

**Step 1:** Assert a session sees newly subscribed sections once, notices an
updated version, and releases all subscriptions on disconnect.

**Step 2:** Run the verifier and confirm the missing visibility type fails.

**Step 3:** Implement the smallest version-aware subscription tracker.

**Step 4:** Run the verifier and confirm all section visibility assertions pass.

### Task 4: Replace the fixed initial section stream

**Files:**
- Modify: `src/Terraria.Dome.Server/Protocol/TerrariaProtocolSessionHost.cs`
- Modify: `src/Terraria.Dome.Server/DomeServer.cs`
- Modify: `src/Terraria.Dome.Protocol.V1456/Packets/TerrariaPacketCodec.cs`
- Modify: `Test/Terraria.Dome.Verification/Program.cs`

**Step 1:** Assert initial world entry streams snapshots selected around the
requested spawn and preserves the V1456 frame order.

**Step 2:** Run the verifier and confirm the fixed stream assertion fails.

**Step 3:** Feed Server-owned world snapshots through the protocol encoder.

**Step 4:** Run the loopback verifier and inspect frame sequence.

### Task 5: Verify and document

**Files:**
- Modify: `progress.md`

**Step 1:** Run serial restore/build and the verifier after user-owned running
processes are stopped or outputs are isolated.

**Step 2:** Record actual pass/fail evidence and remaining next-slice work.

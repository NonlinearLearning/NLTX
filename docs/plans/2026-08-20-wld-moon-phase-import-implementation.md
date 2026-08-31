# WLD Moon Phase Import Implementation Plan


**Goal:** Carry validated legacy WLD moon phase into the authoritative Dome world clock.

**Architecture:** The parser validates the positional `Int32` at source ingress. Immutable
metadata models carry the resulting byte. The compatibility projector creates the existing
authoritative clock snapshot; no Server or protocol-side mutation is introduced.

**Tech Stack:** .NET 10, C#, V319 WLD reader, executable verification projects.

---

### Task 1: Establish parser RED

**Files:**
- Modify: `Test/Terraria.WorldFile.V319.Verification/Program.cs`
- Modify: `Test/Terraria.WorldFile.V319.Verification/Fixtures/WldVersionMatrixFixtureWriter.cs`

1. Require the matrix reader to expose a non-zero Header moon phase.
2. Run the V319 verifier and capture the expected missing model-member RED.

### Task 2: Preserve and validate the WLD value

**Files:**
- Modify: `src/Terraria.WorldFile.V319/Format/WldHeaderReader.cs`
- Modify: `src/Terraria.WorldFile.V319/Format/WldLegacyV1ToV87Reader.cs`
- Modify: `src/Terraria.WorldFile.V319/Model/LegacyWorldMetadata.cs`

1. Read the source `Int32` in its exact position.
2. Reject values outside `0..7` before creating the metadata record.
3. Run the V319 verifier to prove the reader is GREEN.

### Task 3: Project to authoritative simulation state

**Files:**
- Modify: `src/Terraria.WorldCompatibility/Model/CompatibilityWorldMetadata.cs`
- Modify: `src/Terraria.WorldCompatibility/Projection/CompatibilityToDomeProjection.cs`
- Modify: `Test/Terraria.Dome.WorldImport.Verification/Program.cs`

1. Carry the validated phase through compatibility metadata.
2. Construct `WorldClockSnapshot` with the imported phase.
3. Assert a valid phase reaches the snapshot and invalid phase is rejected.

### Task 4: Verify boundaries

1. Run focused WLD and WorldImport verifiers.
2. Run persistence, protocol compatibility, deterministic replay, MainBoundary, and serial root Release.
3. Save fresh artifacts, then update the migration control record without claiming unrelated Task 9 work complete.

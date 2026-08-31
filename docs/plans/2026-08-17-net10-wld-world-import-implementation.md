# Net10 Wld World Import Implementation Plan


**Goal:** Load valid historical Terraria `.wld` files from versions `v1` through
`v319` with an independent `net10.0` parser, project the imported state through
an explicit compatibility model, and start Dome only after a strict import succeeds.

**Architecture:** Keep the original legacy `WorldFile` implementation as an
excluded, hash-recorded format oracle.  A new `Terraria.WorldFile.V319` project
parses `v1..v87` and `v88..v319` through separate paths into immutable legacy
documents.  `Terraria.WorldCompatibility` maps those documents into Dome-owned
snapshots; the server never receives a legacy `Terraria.*` runtime object.

**Tech Stack:** .NET 10, BCL `BinaryReader`/seekable `Stream`, current Dome
simulation/server projects, executable verification projects, PowerShell.

---

## Non-Negotiable Boundaries

- Runtime code must not reference the old Terraria executable, `Terraria.Main`,
  `WorldGen`, old `Tile`, or old `WorldFile` assemblies.
- `v1..v87` uses the pre-pointer-table reader; `v88..v319` uses the pointer-table
  reader.  `v320+` fails closed with a version-specific `InvalidDataException`.
- Every parser stream is seekable and bounded.  Validate file length, pointer
  count/order/ranges, section bounds, dimensions, count caps, string byte lengths,
  tile-RLE runs, coordinates, and footer identity before producing a document.
- All parser and compatibility outputs are immutable.  Unsupported material is
  retained in a `CompatibilityLoadReport`; strict import refuses startup when a
  required Dome representation is missing.
- The personal file `C:\Users\shan\Documents\My Games\Terraria\Worlds\test.wld`
  is read-only acceptance input, never a repository fixture or a tracked file.
- No listener begins before parsing, projection, and strict import all succeed.
  `7778` is protected.  `7777` may be used manually only after proving it is free
  or after explicit approval to stop its current external owner.
- Run every .NET command from `D:\TRbackup\NLTX` with
  `-p:UseSharedCompilation=false`.  Do not commit while Git author identity is
  absent; each conditional commit step below is documentation only.

## Reference Baseline

The behavior oracle is
`D:\TRbackup\无任何删减通过编译\Terraria.IO\WorldFile.cs`, SHA-256
`92DF79BB7AE89393327AE9EF6B7B78762909E5B2E197C0F3FCCFE709E152F289`.
Its dispatch is legacy `<=87`, pointer-table `88..319`, and later-version
rejection above `319`.  Relevant reader responsibilities are `LoadWorld`,
`LoadWorld_Version2`, `LoadFileFormatHeader`, `LoadHeader`, `LoadWorldTiles`,
`LoadChests`, `LoadSigns`, `LoadNPCs`, `LoadFooter`, `LoadTileEntities`, and
`LoadWorld_Version1_Old_BeforeRelease88`.

### Task 1: Isolate Legacy Format Evidence

**Files:**
- Create: `src/Terraria.WorldFile.V319/LegacyReference/SourceManifest.md`
- Create: `src/Terraria.WorldFile.V319/LegacyReference/WorldFile.cs`
- Create: `src/Terraria.WorldFile.V319/LegacyReference/Tile.cs`
- Create: `src/Terraria.WorldFile.V319/LegacyReference/Sign.cs`
- Create: `src/Terraria.WorldFile.V319/Terraria.WorldFile.V319.csproj`
- Create: `Test/Terraria.WorldFile.V319.Verification/Terraria.WorldFile.V319.Verification.csproj`
- Create: `Test/Terraria.WorldFile.V319.Verification/Program.cs`
- Modify: `Terraria.Dome.sln`

**Step 1: Write the failing reference-isolation verifier.**

The verifier reads the new project XML and manifest, requires the source hash and
original source paths, and fails if the legacy copies appear in a `Compile`,
`ProjectReference`, or `Reference` item.  It also asserts that the parser project
targets `net10.0` and contains no package reference to old Terraria binaries.

**Step 2: Run the red verifier.**

Run:

```powershell
dotnet run --project .\Test\Terraria.WorldFile.V319.Verification\Terraria.WorldFile.V319.Verification.csproj -p:UseSharedCompilation=false
```

Expected: failure because the parser project, manifest, and reference copies do
not yet exist.

**Step 3: Add the isolated reference area.**

Copy the three named source files byte-for-byte from the verified old source
checkout.  Set their build action to `None` in the parser project.  In
`SourceManifest.md`, record each absolute origin, SHA-256, original namespace and
type, selected format methods, and why the file is format evidence rather than a
runtime dependency.  Add only the new parser and verification projects to the
solution.

**Step 4: Run the verifier green.**

Run the command from Step 2.

Expected: `PASS: legacy source evidence is excluded from the net10 runtime graph`.

**Step 5: Conditional commit.**

When a Git author is configured, commit only the manifest, excluded source copies,
and project/solution additions with `docs: isolate wld reference sources`.

### Task 2: Define the Immutable Parser Contract and Safety Guards

**Files:**
- Create: `src/Terraria.WorldFile.V319/Model/LegacyWorldDocument.cs`
- Create: `src/Terraria.WorldFile.V319/Model/LegacyWorldMetadata.cs`
- Create: `src/Terraria.WorldFile.V319/Model/LegacyTile.cs`
- Create: `src/Terraria.WorldFile.V319/Model/LegacyChest.cs`
- Create: `src/Terraria.WorldFile.V319/Model/LegacyChestItem.cs`
- Create: `src/Terraria.WorldFile.V319/Model/LegacySign.cs`
- Create: `src/Terraria.WorldFile.V319/Model/LegacyNpc.cs`
- Create: `src/Terraria.WorldFile.V319/Model/LegacyTileEntity.cs`
- Create: `src/Terraria.WorldFile.V319/Model/LegacySectionDiagnostic.cs`
- Create: `src/Terraria.WorldFile.V319/Format/WldReadLimits.cs`
- Create: `src/Terraria.WorldFile.V319/Format/WldBinaryReader.cs`
- Create: `src/Terraria.WorldFile.V319/Format/WldFormatVersion.cs`
- Modify: `Test/Terraria.WorldFile.V319.Verification/Program.cs`

**Step 1: Write failing safety tests.**

Add independent tests for a non-seekable stream, a truncated primitive, a string
whose encoded length exceeds the configured cap, an impossible width/height/tile
count, and immutable collection copying.  The desired public entry point is
`WldWorldReader.Read(Stream input, WldReadLimits? limits = null)`.

**Step 2: Run red.**

Run the Task 1 verifier command.

Expected: compilation failure for missing `WldWorldReader` and models.

**Step 3: Implement minimal models and bounded reads.**

Use only `System.*` and parser-local models.  A bounded reader must reject
positions outside its assigned section and must never expose mutable arrays or
lists from a completed document.

**Step 4: Run green.**

Run the Task 1 command.

Expected: model immutability and malformed-stream cases pass.

### Task 3: Add Version Dispatch and Future-Version Rejection

**Files:**
- Create: `src/Terraria.WorldFile.V319/WldWorldReader.cs`
- Create: `src/Terraria.WorldFile.V319/Format/WldSectionKind.cs`
- Modify: `Test/Terraria.WorldFile.V319.Verification/Program.cs`

**Step 1: Write failing dispatch tests.**

Build minimal header-only synthetic streams for versions `1`, `87`, `88`, `319`,
and `320`.  Assert that the first two select `LegacyV1ToV87`, the next two select
`PointerTableV88ToV319`, and `320` throws an error containing both `320` and
`319`.

**Step 2: Run red.**

Run the Task 1 command.  Expected: missing dispatch implementation.

**Step 3: Implement minimal dispatch.**

Read and validate the signed version before any allocation.  Do not use a default
or best-effort path for a future version.

**Step 4: Run green.**

Run the Task 1 command.  Expected: all five dispatch assertions pass.

### Task 4: Parse the Pre-Release-88 (`v1..v87`) Layout

**Files:**
- Create: `src/Terraria.WorldFile.V319/Format/WldLegacyV1ToV87Reader.cs`
- Create: `Test/Terraria.WorldFile.V319.Verification/Fixtures/LegacyV1ToV87FixtureWriter.cs`
- Modify: `Test/Terraria.WorldFile.V319.Verification/Program.cs`

**Step 1: Write failing v1/v87 tests.**

Generate the smallest valid synthetic records for v1 and v87.  Assert their
header fields, dimensions, tile count, chest/sign/NPC boundaries, footer identity,
and rejection of an RLE run that crosses a row boundary.

**Step 2: Run red.**

Run the Task 1 command.  Expected: parser reports that legacy layout is missing.

**Step 3: Implement the legacy reader.**

Translate only format operations from the isolated oracle into parser-local code.
Gate each historically introduced field by its version.  Return the same common
`LegacyWorldDocument`; no legacy game objects or global state may be constructed.

**Step 4: Run green.**

Run the Task 1 command.  Expected: both synthetic versions parse and malformed
RLE is rejected before an oversized write.

### Task 5: Parse the v88+ Pointer Table and Header Section

**Files:**
- Create: `src/Terraria.WorldFile.V319/Format/WldSectionPointerTable.cs`
- Create: `src/Terraria.WorldFile.V319/Format/WldV88ToV319Reader.cs`
- Create: `src/Terraria.WorldFile.V319/Format/WldHeaderReader.cs`
- Modify: `Test/Terraria.WorldFile.V319.Verification/Program.cs`

**Step 1: Write failing pointer-table tests.**

Cover a valid v88 and v319 table plus short count, negative offset, out-of-file
offset, non-monotonic offsets, section overlap, and a header that ends beyond its
next pointer.

**Step 2: Run red.**

Run the Task 1 command.  Expected: pointer table types are absent.

**Step 3: Implement the pointer-table reader.**

Require exact section count for every supported version, record all offsets, turn
each pair into a bounded section reader, and decode the version-specific header
fields into `LegacyWorldMetadata`.

**Step 4: Run green.**

Run the Task 1 command.  Expected: valid fixtures parse and every invalid pointer
case fails deterministically.

### Task 6: Parse Tiles and RLE Without Allocating Beyond World Bounds

**Files:**
- Create: `src/Terraria.WorldFile.V319/Format/WldTileRleReader.cs`
- Modify: `src/Terraria.WorldFile.V319/Model/LegacyTile.cs`
- Modify: `Test/Terraria.WorldFile.V319.Verification/Program.cs`

**Step 1: Write failing tile tests.**

Use synthetic v88/v319 tile sections to cover inactive and active tiles, type
width changes, wall, liquid, wire flags, slope/actuator flags, frame coordinates,
paint, and one-byte/two-byte RLE.  Assert that RLE cannot exceed the remaining
column/row tile count or its section bound.

**Step 2: Run red.**

Run the Task 1 command.  Expected: tile fields and RLE parser are missing.

**Step 3: Implement tile decoding.**

Map every on-disk tile bit to `LegacyTile`, preserving values Dome cannot yet
represent.  Allocate only after validated dimensions and use checked arithmetic
for each linear index and run length.

**Step 4: Run green.**

Run the Task 1 command.  Expected: full tile field and RLE cases pass.

### Task 7: Parse Chests, Signs, NPCs, and Footer Records

**Files:**
- Create: `src/Terraria.WorldFile.V319/Format/WldChestReader.cs`
- Create: `src/Terraria.WorldFile.V319/Format/WldSignReader.cs`
- Create: `src/Terraria.WorldFile.V319/Format/WldNpcReader.cs`
- Create: `src/Terraria.WorldFile.V319/Format/WldFooterReader.cs`
- Modify: `Test/Terraria.WorldFile.V319.Verification/Program.cs`

**Step 1: Write failing record-section tests.**

Generate valid and invalid bounded sections for chest item slots, sign text,
active NPC records, and footer name/id.  Include oversized count, duplicate or
out-of-range coordinates, overlong string, negative stack, and mismatched footer
identity cases.

**Step 2: Run red.**

Run the Task 1 command.  Expected: each reader is missing.

**Step 3: Implement the section readers.**

Version-gate the exact wire fields.  Capture records in immutable models and
include context in every failure: version, section, record index, and stream
offset.  The footer must match the header's identity before document construction.

**Step 4: Run green.**

Run the Task 1 command.  Expected: valid records round-trip; invalid records
cannot produce a partial document.

### Task 8: Parse Tile Entities and Remaining v88+ Sections

**Files:**
- Create: `src/Terraria.WorldFile.V319/Format/WldTileEntityReader.cs`
- Create: `src/Terraria.WorldFile.V319/Format/WldPressurePlateReader.cs`
- Create: `src/Terraria.WorldFile.V319/Format/WldTownManagerReader.cs`
- Create: `src/Terraria.WorldFile.V319/Format/WldBestiaryReader.cs`
- Create: `src/Terraria.WorldFile.V319/Format/WldCreativePowerReader.cs`
- Modify: `src/Terraria.WorldFile.V319/Model/LegacyTileEntity.cs`
- Modify: `Test/Terraria.WorldFile.V319.Verification/Program.cs`

**Step 1: Write failing exhaustive v319-section tests.**

Cover each known tile-entity kind, an unknown kind, pressure-plate coordinates,
town/bestiary/creative records, count caps, and no-trailing-bytes enforcement.
Assert unknown but structurally readable records are retained as opaque data with
their section/type/coordinates instead of being silently discarded.

**Step 2: Run red.**

Run the Task 1 command.  Expected: tail-section parser is absent.

**Step 3: Implement version-gated tail readers.**

Keep raw compatibility information for data Dome cannot execute.  Require each
bounded section to be consumed exactly, except for explicitly retained opaque
payloads whose lengths are validated.

**Step 4: Run green.**

Run the Task 1 command.  Expected: all v319 section cases pass and unknown data
is reported rather than dropped.

### Task 9: Build the Parser-to-Compatibility Projection

**Files:**
- Create: `src/Terraria.WorldCompatibility/Terraria.WorldCompatibility.csproj`
- Create: `src/Terraria.WorldCompatibility/Model/CompatibilityWorldSnapshot.cs`
- Create: `src/Terraria.WorldCompatibility/Model/CompatibilityWorldMetadata.cs`
- Create: `src/Terraria.WorldCompatibility/Model/CompatibilityTile.cs`
- Create: `src/Terraria.WorldCompatibility/Model/CompatibilityChestSnapshot.cs`
- Create: `src/Terraria.WorldCompatibility/Model/CompatibilitySignSnapshot.cs`
- Create: `src/Terraria.WorldCompatibility/Model/CompatibilityNpcSnapshot.cs`
- Create: `src/Terraria.WorldCompatibility/Model/CompatibilityTileEntitySnapshot.cs`
- Create: `src/Terraria.WorldCompatibility/Model/CompatibilityUnsupportedRecord.cs`
- Create: `src/Terraria.WorldCompatibility/Model/CompatibilityLoadReport.cs`
- Create: `src/Terraria.WorldCompatibility/Projection/WldToCompatibilityProjection.cs`
- Create: `Test/Terraria.WorldCompatibility.Verification/Terraria.WorldCompatibility.Verification.csproj`
- Create: `Test/Terraria.WorldCompatibility.Verification/Program.cs`
- Modify: `Terraria.Dome.sln`

**Step 1: Write failing projection tests.**

Construct a parser document with a tile containing fields Dome cannot currently
represent, a chest, sign, NPC, known tile entity, and an unknown entity.  Assert
the projection is immutable, preserves all known data, and reports the unknown
record with source section/type/coordinates/reason.

**Step 2: Run red.**

Run:

```powershell
dotnet run --project .\Test\Terraria.WorldCompatibility.Verification\Terraria.WorldCompatibility.Verification.csproj -p:UseSharedCompilation=false
```

Expected: project/reference and projection types do not exist.

**Step 3: Implement the compatibility boundary.**

Reference only `Terraria.WorldFile.V319`; never reference Dome Server.  Keep
projection loss explicit and attach it to the load report.

**Step 4: Run green.**

Run the command from Step 2.

Expected: loss reporting and immutable projection assertions pass.

### Task 10: Project Compatibility State Into a Persistent Dome Snapshot

**Files:**
- Create: `src/Terraria.WorldCompatibility/Projection/CompatibilityToDomeProjection.cs`
- Create: `src/Terraria.Dome.Simulation/Snapshots/ChestPersistentState.cs`
- Create: `src/Terraria.Dome.Simulation/Snapshots/SignPersistentState.cs`
- Create: `src/Terraria.Dome.Simulation/Snapshots/TileEntityPersistentState.cs`
- Modify: `src/Terraria.Dome.Simulation/Snapshots/DomeSimulationSnapshot.cs`
- Modify: `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`
- Modify: `src/Terraria.Dome.Server/Persistence/DomeStatePersistenceFormat.cs`
- Modify: `Test/Terraria.Dome.Persistence.Verification/Program.cs`
- Modify: `Test/Terraria.WorldCompatibility.Verification/Terraria.WorldCompatibility.Verification.csproj`

**Step 1: Write failing Dome round-trip tests.**

Add a snapshot containing two chests with item slots, a sign, a representable tile
entity, and opaque compatibility material.  Persist it, restore it, construct a
new `DomeSimulation`, and assert the recreated chests/signs/entities and load
report are preserved.  A strict projection with unrepresentable required state
must fail before the snapshot is returned.

**Step 2: Run red.**

Run:

```powershell
dotnet run --project .\Test\Terraria.Dome.Persistence.Verification\Terraria.Dome.Persistence.Verification.csproj -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.WorldCompatibility.Verification\Terraria.WorldCompatibility.Verification.csproj -p:UseSharedCompilation=false
```

Expected: snapshot does not expose imported object state and persistence format
cannot round-trip it.

**Step 3: Implement snapshot, runtime restoration, and a versioned format bump.**

Maintain reads for all prior Dome-state format versions.  Give the new version a
bounded trailing payload, write deterministic order, and reconstruct runtime
chests/signs/entities through existing simulation APIs or dedicated restoration
methods that validate IDs and coordinates.

**Step 4: Run green.**

Run both commands from Step 2.

Expected: old state reads remain valid and imported object state survives a full
save/restore cycle.

### Task 11: Add Strict CLI Import Before Server Startup

**Files:**
- Create: `src/Terraria.Dome.Server/Startup/ServerLaunchOptions.cs`
- Create: `src/Terraria.Dome.Server/Import/DomeWorldImportApplier.cs`
- Create: `src/Terraria.Dome.Server/Import/DomeWorldImportResult.cs`
- Modify: `src/Terraria.Dome.Server/Terraria.Dome.Server.csproj`
- Modify: `src/Terraria.Dome.Server/Program.cs`
- Create: `Test/Terraria.Dome.WorldImport.Verification/Terraria.Dome.WorldImport.Verification.csproj`
- Create: `Test/Terraria.Dome.WorldImport.Verification/Program.cs`
- Modify: `Terraria.Dome.sln`

**Step 1: Write failing CLI/import tests.**

Assert `--world <absolute .wld path> --port <1..65535>` accepts only one of each
argument, rejects unknown/duplicate/missing values, rejects relative paths and
`7778`, opens the world read-only with `FileShare.Read`, and does not bind a port
when parsing/projection/strict import fails.  The lifecycle test must bind a fresh
temporary port, never `7777` or `7778`.

**Step 2: Run red.**

Run:

```powershell
dotnet run --project .\Test\Terraria.Dome.WorldImport.Verification\Terraria.Dome.WorldImport.Verification.csproj -p:UseSharedCompilation=false
```

Expected: startup options and import service are absent.

**Step 3: Implement startup orchestration.**

Parse options before constructing `DomeServer`.  Open the chosen world as
`FileAccess.Read`/`FileShare.Read`, parse to a document, project it under strict
mode, then construct `DomeServer(DomeSimulationSnapshot)`.  Only then call
`Start(port)`.  Report import version, dimensions, entity counts, and unsupported
records without exposing mutable parser objects.

**Step 4: Run green.**

Run the command from Step 2.

Expected: valid fixture starts on a temporary port; every invalid path exits
without a listener.

### Task 12: Cover the Full Historical Version Matrix and Differential Oracle

**Files:**
- Create: `Test/Terraria.WorldFile.V319.Verification/Fixtures/WldVersionMatrixFixtureWriter.cs`
- Create: `Test/Terraria.WorldFile.V319.Verification/Fixtures/ExpectedVersionLayouts.cs`
- Create: `Test/Terraria.WorldFile.V319.Verification/Oracle/LegacyOracleContract.md`
- Modify: `Test/Terraria.WorldFile.V319.Verification/Program.cs`
- Modify: `src/Terraria.WorldFile.V319/LegacyReference/SourceManifest.md`

**Step 1: Write failing matrix/oracle tests.**

Require an explicit row for every version `1..319`, identifying legacy or
pointer-table path and each section/layout transition.  Test a generated valid
minimum fixture per transition version plus representative versions within every
stable range.  Differential cases compare normalized metadata, tile fields,
chests, signs, NPCs, entities, and footer identity against an externally produced
oracle artifact; the net10 parser project must never compile the oracle.

**Step 2: Run red.**

Run the Task 1 command.  Expected: missing rows/fixtures or unimplemented format
gates are reported by exact version.

**Step 3: Fill every historical version gate.**

Use the isolated reference source and documented recorded oracle artifacts to
implement the narrow format changes.  Keep generated fixture writers in `Test`,
not `Build`, and ensure they produce no user-world copies.

**Step 4: Run green.**

Run the Task 1 command, then:

```powershell
dotnet build .\Terraria.Dome.sln -m:1 -p:UseSharedCompilation=false
```

Expected: all `v1..v319` matrix assertions pass; the solution build has exit code
`0`.  Record warnings verbatim rather than treating them as success noise.

### Task 13: Accept the User's v319 World and Document Manual Launch

**Files:**
- Create: `Test/Terraria.Dome.WorldImport.Verification/LocalWorldAcceptance.cs`
- Create: `docs/server-completion/v319-test-world-import-runbook.md`
- Modify: `Test/Terraria.Dome.WorldImport.Verification/Program.cs`

**Step 1: Write a skipped-by-default local acceptance test.**

The test runs only when `TERRARIA_WLD_ACCEPTANCE_PATH` explicitly equals an
absolute path.  It asserts the file opens read-only, parser reports version `319`,
metadata/dimensions are valid, a set of documented representative tile coordinates
projects exactly, and a temporary-port server starts and stops cleanly.  The test
prints neither world contents nor personal paths in source control logs.

**Step 2: Run red against the specified real file.**

Run:

```powershell
$env:TERRARIA_WLD_ACCEPTANCE_PATH = 'C:\Users\shan\Documents\My Games\Terraria\Worlds\test.wld'
dotnet run --project .\Test\Terraria.Dome.WorldImport.Verification\Terraria.Dome.WorldImport.Verification.csproj -p:UseSharedCompilation=false
```

Expected: failure until v319 parse/projection/startup support is complete.

**Step 3: Complete acceptance diagnostics and runbook.**

The runbook must include the verified parser report, no-listener-on-failure proof,
temporary-port lifecycle command, and the manual production command:

```powershell
dotnet run --project .\src\Terraria.Dome.Server\Terraria.Dome.Server.csproj -- --world 'C:\Users\shan\Documents\My Games\Terraria\Worlds\test.wld' --port 7777
```

It must also require a fresh `Get-NetTCPConnection -LocalPort 7777` check and
explicit authorization before changing any external process using that port.  It
must never stop an existing listener automatically.

**Step 4: Run green acceptance and final regression.**

Run the command from Step 2, followed by:

```powershell
dotnet build .\Terraria.Dome.sln -m:1 -p:UseSharedCompilation=false
```

Expected: local v319 acceptance passes, no service is bound to `7777` or `7778`
by the verifier, and the solution build exits `0`.

**Step 5: Conditional commit.**

When Git identity is configured, commit the implementation/tests/runbook in
small reviewable commits corresponding to Tasks 2-13.  Do not add the personal
world file or generated `Build` output.

## Completion Evidence

Completion requires all of the following, not merely a green compile:

1. The isolated source manifest matches hashes and has no runtime reference path.
2. Synthetic matrix coverage proves every `v1..v319` dispatch/layout path and
   proves `v320+` rejection.
3. Fuzzed malformed inputs cannot allocate past configured limits or return a
   partial document.
4. Compatibility reports every unsupported record, and strict import prevents a
   listener when required state cannot be represented.
5. Dome persistence round-trips imported chests, signs, tile entities, opaque
   compatibility records, tiles, NPCs, and world items.
6. The real read-only v319 world parses/projects/starts on a temporary port.
7. A manual `7777` launch is performed only after its existing owner is handled
   by explicit user authorization.

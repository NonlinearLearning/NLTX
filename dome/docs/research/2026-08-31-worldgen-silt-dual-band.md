# WorldGen Silt dual-band boundary

The instrumented legacy source defines two Silt loops. The first runs
`floor(width * height * 0.0001)` attempts with TileRunner strength `[5,12)` and
steps `[15,50)`. The second runs `floor(width * height * 0.0005)` attempts with
strength `[2,5)` and steps `[2,5)`. Both use tile type 123 and skip wall types
187 and 216.

`LegacySiltPassDefinition.CreateDefault()` represents the first loop and the
new `CreateSecondary()` represents the second. `LegacySiltPass.CreateInvocations`
now schedules both bands with source-order X/Y, wall-veto, strength, and steps
draws, including the Remix surface-to-rock-layer Y range. The owner also
projects requests into primary/secondary `LegacyTileRunnerPassInvocation`
records with source recipe names and four random draws. A focused probe confirms
total invocation count, provenance, command parameters, random draw count, and
the Skyblock no-op. The profile-backed pipeline now traverses these records and
commits typed tile commands through `LegacyTileRunnerCommandCommitBoundary`.
The Simulation Release build is warning-free. Exact traversal/liquid behavior,
aggregate, and WLD parity remain deferred.

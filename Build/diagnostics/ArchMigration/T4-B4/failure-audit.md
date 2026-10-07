# T4-B4 compile failure record

## Probe compile correction

The first build of the newly added probe failed in the probe source only:

```text
Program.cs(292,26): error CS1628: Cannot use ref, out or in parameter 'increment' inside an anonymous method, lambda expression, query expression, or local function.
Exit code: 1; warnings: 0; errors: 1.
```

The query callback captured the method's `in int increment` parameter. The probe now copies that
value to a local (`amount`) before creating the callback. The next incremental build completed
with exit 0, 0 warnings, and 0 errors. This correction changes no production file.

## Source inventory path correction

An initial hash-list command used the nonexistent path `src/NSSLC.Component/WorldStorage/...`.
The repository path is `src/NSSLC/Component/WorldStorage/System/TileEntityUpdateSchedule.cs`; the
manifest was regenerated with that path. No source or build command was affected.

## Warning scope

The local `NSSLC.Tools.Simulation` closure build succeeded with 17 warnings and 0 errors. The
warnings were emitted by `NSSLC.WorldGeneration` dependency sources. The main acceptance owner
reported a separate T4 closure build with 6 warnings and 0 errors. The logs have different
reported scopes; this handoff does not claim they are the same invocation or reconcile the
counts.

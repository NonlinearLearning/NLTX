# T1 selected risk sample report

**Sample status: partial.** The selected sample contains 5 of 44 registered probe cases (11.36%, rounded up from the requested approximate 10% to include every required risk class). Four selected cases completed successfully in the final run; the CommandBuffer case terminated the process before it could complete.

## Selection

| Case | Risk represented | Final explicit-project run |
|---|---|---|
| `world.id-reuse` | World release and ID reuse | PASS |
| `world.isalive-worldid-boundary` | Cross-World local ID/version collision | PASS |
| `component.struct-class-access-critical` | Struct/class access and ref reacquisition across structural change | PASS |
| `query.composition-critical` | All/Any/None/Exclusive composition | PASS |
| `command-buffer.create-and-groups-critical` | Staged Create plus Add/Set grouping | ABORTED by process-level `AccessViolationException`; incomplete |

The risk requirement defines five required categories. A five-case selection is 11.36% of the 44-case probe matrix. The remaining 39 registered cases were intentionally not executed.

## Execution history

1. The first wrapper invocation omitted `--project` and exited 1 before launching the probe. It ran zero cases. Raw output: `core-risk-sample.log`.
2. The first explicit-project sample ran the first four cases successfully, then failed its CommandBuffer assertion (`Grouped Add then Set uses the Set value`). This was an incorrect expected value: fixed source shows `Add<T>` also writes the shared Set value slot. The failure and review are preserved in `pua-playback-order-review.md`.
3. The probe rebuilt after correcting that expectation. `arch-probe-build-corrected.log` and `arch-probe-build-final.log` report exit 0, zero warnings, zero errors.
4. The final explicit-project sample again passed the first four cases. During `command-buffer.create-and-groups-critical`, playback reached `Chunk.GetArray(ComponentType)` and the process terminated with `System.AccessViolationException`; the fifth case did not complete. Exact output: `core-risk-sample-explicit-project.log` and `commandbuffer-access-violation.raw.txt`.

The final run exited `-1073741819` (`0xC0000005`), which is a process-level memory access violation, not a managed exception that the probe's `CaptureException` can safely catch. No test was rerun after this final evidence. The main sample command is preserved in `command-ledger.jsonl` with inputs, output path, source, DLL, PDB, and assets hashes.

## Not run

- The other 39 registered probe cases, including standalone World Clear, stale entity version, token guard, individual component accessor, query-per-operator, and ordinary CommandBuffer cleanup/replay cases.
- Full probe matrix, all solution tests, full solution build, and production integration.
- The post-crash component/world state. The process exited before that state could be inspected.
- `Chunk.cs` internals. Only the captured call site and runtime stack are available.

## Build/package evidence (not test results)

- `dotnet restore Test/Terraria.Arch.Verification/Terraria.Arch.Verification.csproj --verbosity normal`: exit 0, 0 warnings, 0 errors.
- `dotnet nuget verify ... --all`: exit 0, 0 warnings, 0 errors.
- Final probe build: exit 0, 0 warnings, 0 errors. See `arch-probe-build-final.log` and the exact hashes in the command ledger.
- No full solution build or full test suite was run.


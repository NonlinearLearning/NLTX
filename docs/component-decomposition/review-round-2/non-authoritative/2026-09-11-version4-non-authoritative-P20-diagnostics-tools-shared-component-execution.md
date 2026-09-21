# Version4 non-authoritative P20 diagnostics/tools/shared execution plan

partitionId: P20
sessionId: 85646c1d982f4900a64d1e2b10265204
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\20-diagnostics-tools-shared.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P20-diagnostics-tools-shared-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P20-diagnostics-tools-shared-component-execution.md
designStatus: proposed
executionStatus: completed (Component-only slice; non-Component boundaries remain deferred)
implementationStatus: completed (C01,C02,C04,C05,C13 Component state slice saved under src2)
verificationStatus: failed (Diagnostics library build passed; existing behavior verifier build failed)
completedComponents: C01,C02,C04,C05,C13 (Component state files present under src2; C04 ring-state invariant and C05 series-capacity invariant are saved)
currentComponent: none (all independently implementable Component slices saved; serial verification recorded)
pendingComponents: C03,C06,C07,C08,C09,C10,C11,C12,C14,C15,C16,C17,C18,C19,C20,C21,C22 (deferred non-Component boundaries or dependency-dependent work)
lastCheckpointUtc: 2026-09-12T10:52:11.1776848Z
evidence-gap: missing first-round P20 public-decomposition report; per-member reader/writer, lifecycle, persistence, network, and scheduler evidence is partial; cross-partition owners are unresolved; 约束/公共拆分约束.md is referenced but missing; Diagnostics library build passed with 0 warnings and 0 errors, but the existing DiagnosticsVerification build fails with CS0246/CS8422 because the non-Component BuildStatusAdapter file is excluded by the repository DefaultItemExcludes and verifier code cannot be modified under the Component-only boundary; Component behavior beyond compilation remains unverified
blocking-decision: only C01, C02, C04, C05, and C13 state types are in the current Component-only implementation slice; all Adapter, Query, Command, Projection, Port, Value-only, System, and verifier work is deferred; the existing verifier cannot be repaired without modifying excluded non-Component code; broader migration, production-src integration, shared-owner assignment, and external-effect policy require integration-review

## Execution status and guardrails

This is a Component-only implementation checkpoint. C01, C02, C04, C05, and C13 state files are present under src2; C04 has the rotated-window fix and C05 has explicit positive series-capacity validation. Non-Component source, verifier, and dependency work is excluded or deferred and is not counted as completed.

## Component implementation checkpoint

Saved Component state files under D:\TRbackup\NLTX\src2\Diagnostics:

- C01: Runtime/RuntimeDiagnosticsOptionsComponent.cs
- C02: Runtime/RuntimeTickContextComponent.cs
- C04: TimeSeries/TimeSeriesWindowState.cs
- C05: TimeSeries/TimeLogEntryState.cs
- C13: Debug/DebugRuntimeOptionsComponent.cs

C04 was saved with the rotated-window occupancy fix, and C05 was saved with the series-capacity invariant. Existing Adapter, Query, Command, Projection, Port, Value-only, System, and verifier files are excluded from this Component-only checkpoint.

## Session handoff

- Handoff command: `pwsh -NoProfile -File .\Build\Tools\Invoke-Version4NonAuthoritativePartitionSession.ps1 -Action Handoff -PartitionId P20 -SessionId d0495ba0efc441268951704d9acf9591 -HandoffId P20-diagnostics-tools-shared-implementation-20260912 -LockWaitSeconds 60`
- Handoff result: `status=handed-off`, `previousStatus=running`, `oldSessionId=d0495ba0efc441268951704d9acf9591`, `sessionId=85646c1d982f4900a64d1e2b10265204`, `claimMode=manual`, `lockReleased=true`.

## Verification record

- Diagnostics library build command: `& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', '.\src2\Diagnostics\Terraria.NonAuthoritative.Diagnostics.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')`; result exit code 0, 0 warnings, 0 errors; artifact `Build/bin/Terraria.NonAuthoritative.Diagnostics/Debug/net10.0/Terraria.NonAuthoritative.Diagnostics.dll` confirmed after the C05 checkpoint.
- DiagnosticsVerification build command: `& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', '.\src2\DiagnosticsVerification\Terraria.NonAuthoritative.DiagnosticsVerification.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')`; result exit code 1, 0 warnings, 2 errors: `Program.cs(258,3) CS0246 BuildStatusAdapter` and cascading `Program.cs(259,3) CS8422`. The missing type is under the excluded `Diagnostics/Build` directory; repairing this requires non-Component verifier or project-boundary changes and was not performed.
- No Component-only focused verifier exists that can validate C01/C02/C04/C05/C13 without the excluded verifier dependencies.

The paired design document is the source of boundary rationale and invariants. This document supplies an implementation sequence, explicit ownership, source-to-target mapping, compatibility controls, rollback conditions, and future verification commands. The five saved Component checkpoints are synchronized in both documents; the remaining boundary rows are explicitly deferred and are not Component completion claims.

This session modified one Component file under src2 and the two paired checkpoint documents. It did not modify src, tests, or verifier code, and it did not manually modify the ledger or lock; the runner updated the non-authoritative ledger through Handoff. `git diff --check` is intentionally not run on either second-round document per the implementation prompt.

## Proposed target manifest

| ID | Proposed target path(s) | Proposed namespace/types | Kind | Single write owner |
|---|---|---|---|---|
| C01 | src2/Diagnostics/Runtime/RuntimeDiagnosticsOptionsComponent.cs; src2/Diagnostics/Runtime/NetDiagnosticsUiAdapter.cs; src2/Diagnostics/Runtime/FrameTimingAdapter.cs; src2/Diagnostics/Runtime/SimulationRateConfigurationCandidate.cs | Terraria.Diagnostics.Runtime | Component plus Adapter/Projection | Runtime diagnostics command for options; integration-review for rates |
| C02 | src2/Diagnostics/Runtime/RuntimeTickContextComponent.cs; src2/Diagnostics/Runtime/WorldUpdateTimingAdapter.cs; src2/Diagnostics/Runtime/SpelunkerProjectileServicePort.cs | Terraria.Diagnostics.Runtime | transient Component plus Adapter/Port | projectile-loop coordinator for cursor |
| C03 | src2/Diagnostics/CallTracking/CallTrackingSinkAdapter.cs | Terraria.Diagnostics.CallTracking | Adapter/Projection | CallTrackingSinkAdapter |
| C04 | src2/Diagnostics/TimeSeries/TimeSeriesWindowState.cs; src2/Diagnostics/TimeSeries/TimeSeriesAggregationQuery.cs | Terraria.Diagnostics.TimeSeries | Component plus Query | TimeSeries sample writer |
| C05 | src2/Diagnostics/TimeSeries/TimeLogEntryState.cs; src2/Diagnostics/TimeSeries/TimeLogEntryCommand.cs | Terraria.Diagnostics.TimeSeries | Component/record plus Command | TimeLogger entry coordinator |
| C06 | src2/Diagnostics/Formatting/DiagnosticFormatPool.cs | Terraria.Diagnostics.Formatting | Value/Query cache | format-pool factory |
| C07 | src2/Diagnostics/Random/FastRandomValue.cs; src2/Diagnostics/Random/Lcg32RandomState.cs; src2/Diagnostics/Random/UnifiedRandomState.cs; src2/Diagnostics/Random/RandomStreamFactory.cs | Terraria.Diagnostics.Random | Value/Component/Factory | owning random-stream caller |
| C08 | src2/Diagnostics/Memory/BufferPoolAdapter.cs; src2/Diagnostics/Memory/CachedBufferLease.cs; src2/Diagnostics/Memory/SegmentedCollectionState.cs; src2/Diagnostics/Memory/EntrySortPlan.cs | Terraria.Diagnostics.Memory | Adapter/Value/Collection | lease or collection owner |
| C09 | src2/Diagnostics/Values/FloatRangeValue.cs; src2/Diagnostics/Values/IntRangeValue.cs; src2/Diagnostics/Values/Bits64Value.cs; src2/Diagnostics/Values/BitSet2DScratch.cs; src2/Diagnostics/Values/Vertical64BitStripsValue.cs; src2/Diagnostics/Values/BitsByteValue.cs | Terraria.Diagnostics.Values | Value/Scratch/Query | operation caller for scratch |
| C10 | src2/Diagnostics/Crash/CrashObservationAdapter.cs | Terraria.Diagnostics.Crash | Adapter/Projection | CrashObservationAdapter |
| C11 | src2/Diagnostics/Legacy/LegacyAttributeMetadata.cs; src2/Diagnostics/Legacy/SecretMaterialAdapter.cs; src2/Diagnostics/Legacy/MinecartRenderOperationContext.cs; src2/Diagnostics/Legacy/LegacyDelegateOperationContext.cs | Terraria.Diagnostics.Legacy | Value/Adapter/operation context | explicit operation caller; integration-review |
| C12 | src2/Diagnostics/Debug/DebugCommandMetadata.cs; src2/Diagnostics/Debug/DebugCommandCatalogAdapter.cs; src2/Diagnostics/Debug/DebugCommandRequestProjection.cs; src2/Diagnostics/Debug/DebugCommandDispatcher.cs; src2/Diagnostics/Debug/MemoCommandFileAdapter.cs | Terraria.Diagnostics.Debug | Command/Adapter/Projection | DebugCommandDispatcher for dispatch |
| C13 | src2/Diagnostics/Debug/DebugRuntimeOptionsComponent.cs; src2/Diagnostics/Debug/DebugOptionsReplicationAdapter.cs | Terraria.Diagnostics.Debug | Component plus Adapter | authorized options command/server owner |
| C14 | src2/Diagnostics/Telemetry/FrameEventRecord.cs; src2/Diagnostics/Telemetry/FrameTelemetrySlot.cs; src2/Diagnostics/Telemetry/FrameTelemetryRing.cs; src2/Diagnostics/Telemetry/GcAllocationSnapshot.cs; src2/Diagnostics/Telemetry/FrameTelemetryProjection.cs | Terraria.Diagnostics.Telemetry | Snapshot/Projection | frame collector |
| C15 | src2/Diagnostics/Build/BuildStatusAdapter.cs; src2/Diagnostics/Build/BuildStatusProjection.cs | Terraria.Diagnostics.Build | Adapter/Projection | BuildStatusAdapter |
| C16 | src2/Diagnostics/TimeLogger/TimeLoggerFrameCoordinatorAdapter.cs; src2/Diagnostics/TimeLogger/TimeLoggerFrameState.cs | Terraria.Diagnostics.TimeLogger | Adapter/System state | TimeLoggerFrameCoordinatorAdapter |
| C17 | src2/Diagnostics/Utilities/UtilityCacheAdapter.cs; src2/Diagnostics/Utilities/FloodFillScratchQuery.cs | Terraria.Diagnostics.Utilities | Adapter/Query scratch | utility operation owner |
| C18 | src2/Diagnostics/TimeLogger/TimeLoggerDisplayFormatSet.cs | Terraria.Diagnostics.TimeLogger | Value/Query cache | display-format factory |
| C19 | src2/Diagnostics/TimeLogger/Metrics/EntityAndInterfaceMetricsProjection.cs | Terraria.Diagnostics.TimeLogger.Metrics | Projection | entity/interface metric port |
| C20 | src2/Diagnostics/TimeLogger/Metrics/TileAndLiquidMetricsProjection.cs | Terraria.Diagnostics.TimeLogger.Metrics | Projection | tile/liquid metric port |
| C21 | src2/Diagnostics/TimeLogger/Metrics/LightingMapBackgroundMetricsProjection.cs | Terraria.Diagnostics.TimeLogger.Metrics | Projection | lighting/map/background metric port |
| C22 | src2/Diagnostics/Issues/IssueReportCatalogAdapter.cs; src2/Diagnostics/Issues/IssueReportRecord.cs; src2/Diagnostics/Issues/IssueReportProjection.cs | Terraria.Diagnostics.Issues | Adapter/record/Projection | IssueReportCatalogAdapter |

## Proposed implementation order

The order below is a dependency proposal. It is not permission to edit code and must be approved by integration-review.

1. Freeze the source inventory and record the missing first-round evidence without creating a replacement report.
2. Establish proposed ports for clock, stopwatch, filesystem, compression, network/chat, reflection, random seed, exception, and build metadata effects.
3. Implement C09 deterministic range/bit value contracts and C07 explicit random stream contracts as isolated value tests.
4. Implement C08 buffer/collection lease contracts and C17 utility/flood-fill scratch leases.
5. Implement C04 time-series aggregation, then C05 entry state and C06 formatting.
6. Implement C18 display format set and the C19-C21 metric projections. Register metric handles through C05 only.
7. Implement C16 frame coordination after the metric write ports, file output port, and failure policy are approved.
8. Implement C14 frame telemetry snapshots and projections, wiring the frame counter through an injected clock/counter port.
9. Implement C13 debug options with authorization/replication ports, then C12 command metadata, registry, parsing, dispatch, and reply projection.
10. Implement C01/C02 runtime diagnostic and tick seams after world/entity/projectile integration owners accept the handoff.
11. Implement C03, C10, C15, C11, and C22 external adapters with fake ports and explicit teardown.
12. Run focused verifiers, affected-project serial build, and no-build/no-restore tests only after code authorization.

## Synchronized checkpoint record

Each row represents a proposed planning checkpoint recorded in both this document and the paired design document. No implementation work occurred at any checkpoint.

| Checkpoint | Boundary | Paired metadata action | Implementation status |
|---|---|---|---|
| C01 | Runtime diagnostics and simulation-rate view | both documents updated together | completed Component state |
| C02 | Runtime tick and projectile-loop context | both documents updated together | completed Component state |
| C03 | Call-tracking sink | both documents updated together | deferred Adapter/Projection |
| C04 | Time-series window aggregation | both documents updated together | completed Component state; Query deferred |
| C05 | Time-log entry state | both documents updated together | completed Component state; Command deferred |
| C06 | Diagnostic format pool | both documents updated together | deferred Value/Query |
| C07 | Random stream state | both documents updated together | deferred Value/stream boundary |
| C08 | Buffer and collection ownership | both documents updated together | deferred Adapter/Value/Collection |
| C09 | Range and bit values | both documents updated together | deferred Value/Scratch |
| C10 | Crash and exception observation | both documents updated together | deferred Adapter/Projection |
| C11 | Legacy metadata and delegate operation context | both documents updated together | deferred Value/operation context |
| C12 | Debug command protocol | both documents updated together | deferred Command/Adapter/Projection |
| C13 | Debug runtime options | both documents updated together | completed Component state; Adapter deferred |
| C14 | Detailed frame telemetry | both documents updated together | deferred Snapshot/Projection |
| C15 | Build status | both documents updated together | deferred Adapter/Projection |
| C16 | TimeLogger frame coordination | both documents updated together | deferred Adapter/coordinator state |
| C17 | General utility caches and flood-fill scratch | both documents updated together | deferred Adapter/Query scratch |
| C18 | TimeLogger display formats | both documents updated together | deferred Value/Query cache |
| C19 | Entity/interface phase metrics | both documents updated together | deferred Projection |
| C20 | Tile/liquid render phase metrics | both documents updated together | deferred Projection |
| C21 | Lighting/map/background metrics | both documents updated together | deferred Projection |
| C22 | Issue-report catalog | both documents updated together | deferred Adapter/record/Projection |

## Source member to proposed target mapping

This mapping is complete for the 250 input members. Each sequence number is mapped once to a proposed role and a proposed target boundary/file.

### 4.1 MainDiagnosticsAndSimulationRates -> C01

| Seq | Source member | Proposed target role/file |
|---:|---|---|
| 116 | Terraria.Main._activeNetDiagnosticsUI | NetDiagnosticsUiAdapter.cs external UI handle |
| 117 | Terraria.Main.fpsTimer | FrameTimingAdapter.cs stopwatch handle |
| 118 | Terraria.Main.showSplash | RuntimeDiagnosticsOptionsComponent.cs option |
| 119 | Terraria.Main.ignoreErrors | RuntimeDiagnosticsOptionsComponent.cs option |
| 120 | Terraria.Main.defaultIP | RuntimeDiagnosticsOptionsComponent.cs endpoint option; integration-review |
| 121 | Terraria.Main.dayRate | SimulationRateConfigurationCandidate.cs shared rate; integration-review |
| 122 | Terraria.Main.desiredWorldTilesUpdateRate | SimulationRateConfigurationCandidate.cs shared rate; integration-review |

### 4.2 MainTickAndDiagnosticState -> C02

| Seq | Source member | Proposed target role/file |
|---:|---|---|
| 493 | Terraria.Main.ProjectileUpdateLoopIndex | RuntimeTickContextComponent.cs transient cursor |
| 494 | Terraria.Main.TARGET_FRAME_TIME | RuntimeTickContextComponent.cs immutable target duration |
| 495 | Terraria.Main._worldUpdateTimeTester | WorldUpdateTimingAdapter.cs stopwatch port |
| 496 | Terraria.Main.SpelunkerProjectileHelper | SpelunkerProjectileServicePort.cs injected service |

### 4.3 SharedCallTrackingDiagnostics -> C03

| Seq | Source member | Proposed target role/file |
|---:|---|---|
| 1010 | Terraria.CallTracker.LogQueue | CallTrackingSinkAdapter.cs event queue |
| 1011 | Terraria.CallTracker.LoggedMethods | CallTrackingSinkAdapter.cs dedupe state |
| 1012 | Terraria.CallTracker.FlushTimer | CallTrackingSinkAdapter.cs flush scheduler |

### 4.4 SharedTimeSeriesDataSeriesState -> C04

| Seq | Source member | Proposed target role/file |
|---:|---|---|
| 3517 | Terraria.TimeLogger.DataSeries.values | TimeSeriesWindowState.cs ring values |
| 3518 | Terraria.TimeLogger.DataSeries.used | TimeSeriesWindowState.cs occupancy |
| 3519 | Terraria.TimeLogger.DataSeries.next | TimeSeriesWindowState.cs write cursor |
| 3520 | Terraria.TimeLogger.DataSeries.count | TimeSeriesWindowState.cs window count |
| 3521 | Terraria.TimeLogger.DataSeries.usedCount | TimeSeriesWindowState.cs populated count |
| 3522 | Terraria.TimeLogger.DataSeries.previous | TimeSeriesAggregationQuery.cs previous value |
| 3523 | Terraria.TimeLogger.DataSeries.median | TimeSeriesAggregationQuery.cs median |
| 3524 | Terraria.TimeLogger.DataSeries.p90 | TimeSeriesAggregationQuery.cs p90 |
| 3525 | Terraria.TimeLogger.DataSeries.max | TimeSeriesAggregationQuery.cs maximum |
| 3526 | Terraria.TimeLogger.DataSeries._sort | TimeSeriesWindowState.cs sort scratch |

### 4.5 SharedTimeSeriesEntryState -> C05

| Seq | Source member | Proposed target role/file |
|---:|---|---|
| 3527 | Terraria.TimeLogger.TimeLogData.name | TimeLogEntryState.cs label |
| 3528 | Terraria.TimeLogger.TimeLogData.format | TimeLogEntryState.cs formatter policy |
| 3529 | Terraria.TimeLogger.TimeLogData.budget | TimeLogEntryState.cs budget |
| 3530 | Terraria.TimeLogger.TimeLogData.pendingDisplay | TimeLogEntryState.cs display intent |
| 3531 | Terraria.TimeLogger.TimeLogData.data | TimeLogEntryState.cs series slots |

### 4.6 SharedTimeSeriesFormattingState -> C06

| Seq | Source member | Proposed target role/file |
|---:|---|---|
| 3532 | Terraria.TimeLogger.FormatPool._format | DiagnosticFormatPool.cs template |
| 3533 | Terraria.TimeLogger.FormatPool._minValue | DiagnosticFormatPool.cs lower bound |
| 3534 | Terraria.TimeLogger.FormatPool._rounding | DiagnosticFormatPool.cs rounding policy |
| 3535 | Terraria.TimeLogger.FormatPool._strings | DiagnosticFormatPool.cs string cache |
| 3536 | Terraria.TimeLogger.FormatPool._nullString | DiagnosticFormatPool.cs null display |

### 4.7 SharedRandomSources -> C07

| Seq | Source member | Proposed target role/file |
|---:|---|---|
| 2957 | Terraria.Utilities.FastRandom.RANDOM_MULTIPLIER | FastRandomValue.cs algorithm constant |
| 2958 | Terraria.Utilities.FastRandom.RANDOM_ADD | FastRandomValue.cs algorithm constant |
| 2959 | Terraria.Utilities.FastRandom.RANDOM_MASK | FastRandomValue.cs algorithm constant |
| 2963 | Terraria.Utilities.LCG32Random.state | Lcg32RandomState.cs stream state |
| 2985 | Terraria.Utilities.UnifiedRandom.MBIG | UnifiedRandomState.cs algorithm constant |
| 2986 | Terraria.Utilities.UnifiedRandom.MSEED | UnifiedRandomState.cs algorithm constant |
| 2987 | Terraria.Utilities.UnifiedRandom.MZ | UnifiedRandomState.cs algorithm constant |
| 2988 | Terraria.Utilities.UnifiedRandom.inext | UnifiedRandomState.cs sequence cursor |
| 2989 | Terraria.Utilities.UnifiedRandom.SeedArray | UnifiedRandomState.cs sequence storage |
| 3939 | Terraria.Utilities.FastRandom.Seed | FastRandomValue.cs seed property |

### 4.8 SharedBufferAndCollectionPools -> C08

| Seq | Source member | Proposed target role/file |
|---:|---|---|
| 1069 | Terraria.DataStructures.BufferPool.SMALL_BUFFER_SIZE | BufferPoolAdapter.cs bucket policy |
| 1070 | Terraria.DataStructures.BufferPool.MEDIUM_BUFFER_SIZE | BufferPoolAdapter.cs bucket policy |
| 1071 | Terraria.DataStructures.BufferPool.LARGE_BUFFER_SIZE | BufferPoolAdapter.cs bucket policy |
| 1072 | Terraria.DataStructures.BufferPool.HUGE_BUFFER_SIZE | BufferPoolAdapter.cs bucket policy |
| 1073 | Terraria.DataStructures.BufferPool.bufferLock | BufferPoolAdapter.cs synchronization |
| 1074 | Terraria.DataStructures.BufferPool.SmallBufferQueue | BufferPoolAdapter.cs lease queue |
| 1075 | Terraria.DataStructures.BufferPool.MediumBufferQueue | BufferPoolAdapter.cs lease queue |
| 1076 | Terraria.DataStructures.BufferPool.LargeBufferQueue | BufferPoolAdapter.cs lease queue |
| 1077 | Terraria.DataStructures.BufferPool.HugeBufferQueue | BufferPoolAdapter.cs lease queue |
| 1078 | Terraria.DataStructures.CachedBuffer.Data | CachedBufferLease.cs byte storage |
| 1079 | Terraria.DataStructures.CachedBuffer.Writer | CachedBufferLease.cs writer |
| 1080 | Terraria.DataStructures.CachedBuffer.Reader | CachedBufferLease.cs reader |
| 1081 | Terraria.DataStructures.CachedBuffer._memoryStream | CachedBufferLease.cs stream |
| 1082 | Terraria.DataStructures.CachedBuffer._isActive | CachedBufferLease.cs lease flag |
| 1088 | Terraria.DataStructures.DoubleStack<T1>._segmentList | SegmentedCollectionState.cs storage |
| 1089 | Terraria.DataStructures.DoubleStack<T1>._segmentSize | SegmentedCollectionState.cs segment policy |
| 1090 | Terraria.DataStructures.DoubleStack<T1>._segmentCount | SegmentedCollectionState.cs segment count |
| 1091 | Terraria.DataStructures.DoubleStack<T1>._segmentShiftPosition | SegmentedCollectionState.cs index shift |
| 1092 | Terraria.DataStructures.DoubleStack<T1>._start | SegmentedCollectionState.cs start cursor |
| 1093 | Terraria.DataStructures.DoubleStack<T1>._end | SegmentedCollectionState.cs end cursor |
| 1094 | Terraria.DataStructures.DoubleStack<T1>._size | SegmentedCollectionState.cs size |
| 1095 | Terraria.DataStructures.DoubleStack<T1>._last | SegmentedCollectionState.cs last cursor |
| 1140 | Terraria.DataStructures.EntrySorter<TEntryType, TStepType>.Steps | EntrySortPlan.cs sort steps |
| 3686 | Terraria.DataStructures.CachedBuffer.Length | CachedBufferLease.cs length query |
| 3687 | Terraria.DataStructures.DoubleStack<T1>.Count | SegmentedCollectionState.cs count query |

### 4.9 SharedRangeAndBitUtilities -> C09

| Seq | Source member | Proposed target role/file |
|---:|---|---|
| 2951 | Terraria.Utilities.Terraria.Utilities.FloatRange.Minimum | FloatRangeValue.cs lower bound |
| 2952 | Terraria.Utilities.Terraria.Utilities.FloatRange.Maximum | FloatRangeValue.cs upper bound |
| 2953 | Terraria.Utilities.Bits64.v | Bits64Value.cs storage |
| 2954 | Terraria.Utilities.BitSet2D.offset | BitSet2DScratch.cs origin |
| 2955 | Terraria.Utilities.BitSet2D.size | BitSet2DScratch.cs extent |
| 2956 | Terraria.Utilities.BitSet2D.bits | BitSet2DScratch.cs words |
| 2961 | Terraria.Utilities.IntRange.Minimum | IntRangeValue.cs lower bound |
| 2962 | Terraria.Utilities.IntRange.Maximum | IntRangeValue.cs upper bound |
| 2990 | Terraria.Utilities.Vertical64BitStrips.arr | Vertical64BitStripsValue.cs storage |
| 2997 | Terraria.BitsByte.value | BitsByteValue.cs storage |
| 3931 | Terraria.Utilities.Bits64.this[] | Bits64Value.cs bit access |
| 3932 | Terraria.Utilities.Bits64.IsEmpty | Bits64Value.cs emptiness query |
| 3933 | Terraria.Utilities.BitSet2D.this[] | BitSet2DScratch.cs bit access |
| 3942 | Terraria.Utilities.Vertical64BitStrips.this[] | Vertical64BitStripsValue.cs strip access |
| 3943 | Terraria.BitsByte.this[] | BitsByteValue.cs bit access |

### 4.10 SharedGeneralDiagnosticsUtilities -> C10

| Seq | Source member | Proposed target role/file |
|---:|---|---|
| 3934 | Terraria.Utilities.CrashWatcher.LogAllExceptions | CrashObservationAdapter.cs configuration |
| 3935 | Terraria.Utilities.CrashWatcher.DumpOnException | CrashObservationAdapter.cs configuration |
| 3936 | Terraria.Utilities.CrashWatcher.DumpOnCrash | CrashObservationAdapter.cs configuration |
| 3937 | Terraria.Utilities.CrashWatcher.CrashDumpOptions | CrashObservationAdapter.cs dump policy |
| 3938 | Terraria.Utilities.CrashWatcher.DumpPath | CrashObservationAdapter.cs output path |

### 4.11 SharedGeneralDelegateAndMetadataUtilities -> C11

| Seq | Source member | Proposed target role/file |
|---:|---|---|
| 2965 | Terraria.Utilities.OldAttribute.message | LegacyAttributeMetadata.cs message |
| 2966 | Terraria.Utilities.Secrets._salt | SecretMaterialAdapter.cs protected material |
| 3034 | Terraria.DelegateMethods.Minecart.rotationOrigin | MinecartRenderOperationContext.cs origin |
| 3035 | Terraria.DelegateMethods.Minecart.rotation | MinecartRenderOperationContext.cs rotation |
| 3036 | Terraria.DelegateMethods.v3_1 | LegacyDelegateOperationContext.cs vector result |
| 3037 | Terraria.DelegateMethods.v2_1 | LegacyDelegateOperationContext.cs vector input |
| 3038 | Terraria.DelegateMethods.f_1 | LegacyDelegateOperationContext.cs scalar |
| 3039 | Terraria.DelegateMethods.CheckResultOut | LegacyDelegateOperationContext.cs result flag |
| 3040 | Terraria.DelegateMethods.tilecut_0 | LegacyDelegateOperationContext.cs tile-cut context |
| 3041 | Terraria.DelegateMethods.tileCutIgnore | LegacyDelegateOperationContext.cs ignore mask |
| 3940 | Terraria.Utilities.OldAttribute.Message | LegacyAttributeMetadata.cs message projection |

### 4.12 DebugCommandProtocol -> C12

| Seq | Source member | Proposed target role/file |
|---:|---|---|
| 2891 | Terraria.Testing.ChatCommands.DebugCommandAttribute.InternalDebugCommand._processMethod | DebugCommandDispatcher.cs process port |
| 2892 | Terraria.Testing.ChatCommands.DebugCommandAttribute.Name | DebugCommandMetadata.cs command name |
| 2893 | Terraria.Testing.ChatCommands.DebugCommandAttribute.Description | DebugCommandMetadata.cs description |
| 2894 | Terraria.Testing.ChatCommands.DebugCommandAttribute.Requirements | DebugCommandMetadata.cs requirement policy |
| 2895 | Terraria.Testing.ChatCommands.DebugCommandAttribute.HelpText | DebugCommandMetadata.cs help text |
| 2896 | Terraria.Testing.ChatCommands.DebugCommandProcessor._commands | DebugCommandCatalogAdapter.cs registry |
| 2897 | Terraria.Testing.ChatCommands.DebugCommandProcessor.MemoCommandsPath | MemoCommandFileAdapter.cs file path |
| 2898 | Terraria.Testing.ChatCommands.DebugMessage.COMMAND_PREFIX | DebugCommandRequestProjection.cs protocol prefix |
| 2899 | Terraria.Testing.ChatCommands.DebugMessage.Author | DebugCommandRequestProjection.cs player-slot projection |
| 2900 | Terraria.Testing.ChatCommands.DebugMessage.CommandName | DebugCommandRequestProjection.cs command name |
| 2901 | Terraria.Testing.ChatCommands.DebugMessage.Arguments | DebugCommandRequestProjection.cs arguments |
| 2902 | Terraria.Testing.ChatCommands.DebugMessage.MousePosition | DebugCommandRequestProjection.cs pointer |
| 3922 | Terraria.Testing.ChatCommands.DebugCommandAttribute.InternalDebugCommand.Name | DebugCommandMetadata.cs name contract |
| 3923 | Terraria.Testing.ChatCommands.DebugCommandAttribute.InternalDebugCommand.Description | DebugCommandMetadata.cs description contract |
| 3924 | Terraria.Testing.ChatCommands.DebugCommandAttribute.InternalDebugCommand.HelpText | DebugCommandMetadata.cs help contract |
| 3925 | Terraria.Testing.ChatCommands.DebugCommandAttribute.InternalDebugCommand.Requirements | DebugCommandMetadata.cs requirements contract |
| 3926 | Terraria.Testing.ChatCommands.IDebugCommand.Name | DebugCommandMetadata.cs port |
| 3927 | Terraria.Testing.ChatCommands.IDebugCommand.Description | DebugCommandMetadata.cs port |
| 3928 | Terraria.Testing.ChatCommands.IDebugCommand.HelpText | DebugCommandMetadata.cs port |
| 3929 | Terraria.Testing.ChatCommands.IDebugCommand.Requirements | DebugCommandMetadata.cs port |

### 4.13 DebugRuntimeOptions -> C13

| Seq | Source member | Proposed target role/file |
|---:|---|---|
| 2903 | Terraria.Testing.DebugOptions.enableDebugCommands | DebugRuntimeOptionsComponent.cs option |
| 2904 | Terraria.Testing.DebugOptions.Shared_ReportCommandUsage | DebugRuntimeOptionsComponent.cs option |
| 2905 | Terraria.Testing.DebugOptions.Shared_ServerPing | DebugRuntimeOptionsComponent.cs diagnostic value |
| 2906 | Terraria.Testing.DebugOptions.UpdateWaitInMs | DebugRuntimeOptionsComponent.cs timing option |
| 2907 | Terraria.Testing.DebugOptions.noLimits | DebugRuntimeOptionsComponent.cs option |
| 2908 | Terraria.Testing.DebugOptions.ShowNetOffsetDust | DebugRuntimeOptionsComponent.cs option |
| 2909 | Terraria.Testing.DebugOptions.FakeNetOffset | DebugRuntimeOptionsComponent.cs test vector |
| 2910 | Terraria.Testing.DebugOptions.NoDamageVar | DebugRuntimeOptionsComponent.cs option |
| 2911 | Terraria.Testing.DebugOptions.LetProjectilesAimAtTargetDummies | DebugRuntimeOptionsComponent.cs option |
| 2912 | Terraria.Testing.DebugOptions.PracticeMode | DebugRuntimeOptionsComponent.cs option |

### 4.14 DebugFrameTelemetry -> C14

| Seq | Source member | Proposed target role/file |
|---:|---|---|
| 2913 | Terraria.Testing.DetailedFPS.Frame.Event.category | FrameEventRecord.cs category |
| 2914 | Terraria.Testing.DetailedFPS.Frame.Event.timestamp | FrameEventRecord.cs timestamp |
| 2915 | Terraria.Testing.DetailedFPS.Frame.events | FrameTelemetrySlot.cs event list |
| 2916 | Terraria.Testing.DetailedFPS.Frame.CollectionCount | FrameTelemetrySlot.cs GC counts |
| 2917 | Terraria.Testing.DetailedFPS.Frame.Allocated | FrameTelemetrySlot.cs allocation count |
| 2918 | Terraria.Testing.DetailedFPS.FrameCount | FrameTelemetryRing.cs capacity |
| 2919 | Terraria.Testing.DetailedFPS.Frames | FrameTelemetryRing.cs slots |
| 2920 | Terraria.Testing.DetailedFPS.oldest | FrameTelemetryRing.cs oldest cursor |
| 2921 | Terraria.Testing.DetailedFPS.newest | FrameTelemetryRing.cs newest cursor |
| 2922 | Terraria.Testing.DetailedFPS.LastGCPauseTime | GcAllocationSnapshot.cs pause |
| 2923 | Terraria.Testing.DetailedFPS.LastCollectionCount | GcAllocationSnapshot.cs counts |
| 2924 | Terraria.Testing.DetailedFPS.LastAllocatedBytes | GcAllocationSnapshot.cs bytes |
| 2925 | Terraria.Testing.DetailedFPS.PixelsPerMs | FrameTelemetryProjection.cs scale |
| 2926 | Terraria.Testing.DetailedFPS.FrameWidth | FrameTelemetryProjection.cs width |
| 2927 | Terraria.Testing.DetailedFPS.BoxHeight | FrameTelemetryProjection.cs height |
| 2928 | Terraria.Testing.DetailedFPS._gcGenText | FrameTelemetryProjection.cs text cache |

### 4.15 DebugBuildStatus -> C15

| Seq | Source member | Proposed target role/file |
|---:|---|---|
| 2929 | Terraria.Testing.GitStatus._gitSHA | BuildStatusAdapter.cs lazy value |
| 3930 | Terraria.Testing.GitStatus.GitSHA | BuildStatusProjection.cs read-only value |

### 4.16 SharedTimeLoggerFrameCoordinationState -> C16

| Seq | Source member | Proposed target role/file |
|---:|---|---|
| 3537 | Terraria.TimeLogger.FrameCount | TimeLoggerFrameState.cs capacity |
| 3538 | Terraria.TimeLogger.logWriter | TimeLoggerFrameCoordinatorAdapter.cs file writer |
| 3539 | Terraria.TimeLogger.logBuilder | TimeLoggerFrameCoordinatorAdapter.cs builder |
| 3540 | Terraria.TimeLogger.framesToLog | TimeLoggerFrameState.cs quota |
| 3541 | Terraria.TimeLogger.currentFrame | TimeLoggerFrameState.cs cursor |
| 3542 | Terraria.TimeLogger.startLoggingNextFrame | TimeLoggerFrameState.cs start command |
| 3543 | Terraria.TimeLogger.endLoggingThisFrame | TimeLoggerFrameState.cs end command |
| 3544 | Terraria.TimeLogger.currentlyLogging | TimeLoggerFrameState.cs active flag |
| 3545 | Terraria.TimeLogger.DataSeriesHeaders | TimeLoggerFrameState.cs headers |
| 3546 | Terraria.TimeLogger.activeDataSeries | TimeLoggerFrameState.cs active-series cursor |
| 3547 | Terraria.TimeLogger.entries | TimeLoggerFrameState.cs entry registry |
| 3614 | Terraria.TimeLogger._onNextFrame | TimeLoggerFrameCoordinatorAdapter.cs callback queue |
| 3615 | Terraria.TimeLogger.ABTestMode | TimeLoggerFrameState.cs experiment mode |
| 3616 | Terraria.TimeLogger.ABTestName | TimeLoggerFrameState.cs experiment name |
| 3617 | Terraria.TimeLogger._entriesToDraw | TimeLoggerFrameCoordinatorAdapter.cs draw queue |
| 4024 | Terraria.TimeLogger.ABTestFlag | TimeLoggerFrameState.cs experiment projection |

### 4.17 SharedGeneralRandomAndBufferUtilities -> C17

| Seq | Source member | Proposed target role/file |
|---:|---|---|
| 3647 | Terraria.Utils.charLengths | UtilityCacheAdapter.cs font cache |
| 3648 | Terraria.Utils._substitutionRegex | UtilityCacheAdapter.cs regex cache |
| 3649 | Terraria.Utils.RANDOM_MULTIPLIER | UtilityCacheAdapter.cs algorithm constant |
| 3650 | Terraria.Utils.RANDOM_ADD | UtilityCacheAdapter.cs algorithm constant |
| 3651 | Terraria.Utils.RANDOM_MASK | UtilityCacheAdapter.cs algorithm constant |
| 3652 | Terraria.Utils._floodFillQueue1 | FloodFillScratchQuery.cs queue lease |
| 3653 | Terraria.Utils._floodFillQueue2 | FloodFillScratchQuery.cs queue lease |
| 3654 | Terraria.Utils._floodFillBitset | FloodFillScratchQuery.cs bitset lease |

### 4.18 SharedTimeLoggerDisplayFormattingState -> C18

| Seq | Source member | Proposed target role/file |
|---:|---|---|
| 3618 | Terraria.TimeLogger._PinnedCPUFormat | TimeLoggerDisplayFormatSet.cs named format |
| 3619 | Terraria.TimeLogger._AssignedCPUFormat | TimeLoggerDisplayFormatSet.cs named format |
| 3620 | Terraria.TimeLogger._procThrottleFormat | TimeLoggerDisplayFormatSet.cs named format |
| 3621 | Terraria.TimeLogger._expectedCPUFormat | TimeLoggerDisplayFormatSet.cs named format |
| 3622 | Terraria.TimeLogger._terrariaCPUFormat | TimeLoggerDisplayFormatSet.cs named format |
| 3623 | Terraria.TimeLogger._pendingCPUFormat | TimeLoggerDisplayFormatSet.cs named format |
| 3624 | Terraria.TimeLogger._percentFormat | TimeLoggerDisplayFormatSet.cs named format |
| 3625 | Terraria.TimeLogger._msFormat | TimeLoggerDisplayFormatSet.cs named format |
| 3626 | Terraria.TimeLogger._intFormat | TimeLoggerDisplayFormatSet.cs named format |

### 4.19 SharedTimeLoggerEntityAndInterfacePhaseMetricsState -> C19

| Seq | Source member | Proposed target role/file |
|---:|---|---|
| 3593 | Terraria.TimeLogger.PlayerChat | EntityAndInterfaceMetricsProjection.cs metric handle |
| 3595 | Terraria.TimeLogger.NPCs | EntityAndInterfaceMetricsProjection.cs metric handle |
| 3596 | Terraria.TimeLogger.Projectiles | EntityAndInterfaceMetricsProjection.cs metric handle |
| 3597 | Terraria.TimeLogger.Players | EntityAndInterfaceMetricsProjection.cs metric handle |
| 3598 | Terraria.TimeLogger.Items | EntityAndInterfaceMetricsProjection.cs metric handle |
| 3599 | Terraria.TimeLogger.Rain | EntityAndInterfaceMetricsProjection.cs metric handle |
| 3600 | Terraria.TimeLogger.Gore | EntityAndInterfaceMetricsProjection.cs metric handle |
| 3601 | Terraria.TimeLogger.Dust | EntityAndInterfaceMetricsProjection.cs metric handle |
| 3602 | Terraria.TimeLogger.Particles | EntityAndInterfaceMetricsProjection.cs metric handle |
| 3603 | Terraria.TimeLogger.LeashedEntities | EntityAndInterfaceMetricsProjection.cs metric handle |
| 3604 | Terraria.TimeLogger.Interface | EntityAndInterfaceMetricsProjection.cs metric handle |
| 3605 | Terraria.TimeLogger.DrawFPSGraph | EntityAndInterfaceMetricsProjection.cs metric handle |
| 3606 | Terraria.TimeLogger.DrawTimeLogger | EntityAndInterfaceMetricsProjection.cs metric handle |
| 3607 | Terraria.TimeLogger.Overlays | EntityAndInterfaceMetricsProjection.cs metric handle |
| 3608 | Terraria.TimeLogger.Filters | EntityAndInterfaceMetricsProjection.cs metric handle |
| 3609 | Terraria.TimeLogger.SunVisibility | EntityAndInterfaceMetricsProjection.cs metric handle |
| 3610 | Terraria.TimeLogger.MenuDrawTime | EntityAndInterfaceMetricsProjection.cs metric handle |
| 3611 | Terraria.TimeLogger.SplashDrawTime | EntityAndInterfaceMetricsProjection.cs metric handle |
| 3612 | Terraria.TimeLogger.DrawFullscreenMap | EntityAndInterfaceMetricsProjection.cs metric handle |
| 3613 | Terraria.TimeLogger.GCPause | EntityAndInterfaceMetricsProjection.cs metric handle |

### 4.20 SharedTimeLoggerTileAndLiquidRenderMetricsState -> C20

| Seq | Source member | Proposed target role/file |
|---:|---|---|
| 3548 | Terraria.TimeLogger.TotalDrawAndUpdate | TileAndLiquidMetricsProjection.cs metric handle |
| 3549 | Terraria.TimeLogger.DrawSolidTiles | TileAndLiquidMetricsProjection.cs metric handle |
| 3550 | Terraria.TimeLogger.FlushSolidTiles | TileAndLiquidMetricsProjection.cs metric handle |
| 3551 | Terraria.TimeLogger.SolidDrawCalls | TileAndLiquidMetricsProjection.cs metric handle |
| 3552 | Terraria.TimeLogger.DrawNonSolidTiles | TileAndLiquidMetricsProjection.cs metric handle |
| 3553 | Terraria.TimeLogger.FlushNonSolidTiles | TileAndLiquidMetricsProjection.cs metric handle |
| 3554 | Terraria.TimeLogger.NonSolidDrawCalls | TileAndLiquidMetricsProjection.cs metric handle |
| 3555 | Terraria.TimeLogger.DrawBlackTiles | TileAndLiquidMetricsProjection.cs metric handle |
| 3556 | Terraria.TimeLogger.DrawWallTiles | TileAndLiquidMetricsProjection.cs metric handle |
| 3557 | Terraria.TimeLogger.FlushWallTiles | TileAndLiquidMetricsProjection.cs metric handle |
| 3558 | Terraria.TimeLogger.WallDrawCalls | TileAndLiquidMetricsProjection.cs metric handle |
| 3559 | Terraria.TimeLogger.DrawWaterTiles | TileAndLiquidMetricsProjection.cs metric handle |
| 3560 | Terraria.TimeLogger.LiquidDrawCalls | TileAndLiquidMetricsProjection.cs metric handle |
| 3561 | Terraria.TimeLogger.DrawBackgroundWaterTiles | TileAndLiquidMetricsProjection.cs metric handle |
| 3562 | Terraria.TimeLogger.LiquidBackgroundDrawCalls | TileAndLiquidMetricsProjection.cs metric handle |
| 3565 | Terraria.TimeLogger.DrawWireTiles | TileAndLiquidMetricsProjection.cs metric handle |
| 3566 | Terraria.TimeLogger.ClothingRacks | TileAndLiquidMetricsProjection.cs metric handle |
| 3567 | Terraria.TimeLogger.TileExtras | TileAndLiquidMetricsProjection.cs metric handle |
| 3568 | Terraria.TimeLogger.Nature | TileAndLiquidMetricsProjection.cs metric handle |
| 3569 | Terraria.TimeLogger.RenderSolidTiles | TileAndLiquidMetricsProjection.cs metric handle |
| 3570 | Terraria.TimeLogger.RenderNonSolidTiles | TileAndLiquidMetricsProjection.cs metric handle |
| 3571 | Terraria.TimeLogger.RenderBlacksAndWalls | TileAndLiquidMetricsProjection.cs metric handle |
| 3573 | Terraria.TimeLogger.RenderBackgroundLiquid | TileAndLiquidMetricsProjection.cs metric handle |
| 3574 | Terraria.TimeLogger.RenderLiquid | TileAndLiquidMetricsProjection.cs metric handle |

### 4.21 SharedTimeLoggerLightingMapAndBackgroundMetricsState -> C21

| Seq | Source member | Proposed target role/file |
|---:|---|---|
| 3563 | Terraria.TimeLogger.DrawUndergroundBackground | LightingMapBackgroundMetricsProjection.cs metric handle |
| 3564 | Terraria.TimeLogger.DrawOldUndergroundBackground | LightingMapBackgroundMetricsProjection.cs metric handle |
| 3572 | Terraria.TimeLogger.RenderUndergroundBackground | LightingMapBackgroundMetricsProjection.cs metric handle |
| 3575 | Terraria.TimeLogger.TotalDrawByRenderCount | LightingMapBackgroundMetricsProjection.cs metric array |
| 3576 | Terraria.TimeLogger.TotalDrawRenderNow | LightingMapBackgroundMetricsProjection.cs metric handle |
| 3577 | Terraria.TimeLogger.TotalDraw | LightingMapBackgroundMetricsProjection.cs metric handle |
| 3578 | Terraria.TimeLogger.Lighting | LightingMapBackgroundMetricsProjection.cs metric handle |
| 3579 | Terraria.TimeLogger.LightingInit | LightingMapBackgroundMetricsProjection.cs metric handle |
| 3580 | Terraria.TimeLogger.LightingByPass | LightingMapBackgroundMetricsProjection.cs metric array |
| 3581 | Terraria.TimeLogger.FindPaintedTiles | LightingMapBackgroundMetricsProjection.cs metric handle |
| 3582 | Terraria.TimeLogger.PrepareRequests | LightingMapBackgroundMetricsProjection.cs metric handle |
| 3583 | Terraria.TimeLogger.FindingWaterfalls | LightingMapBackgroundMetricsProjection.cs metric handle |
| 3584 | Terraria.TimeLogger.MapChanges | LightingMapBackgroundMetricsProjection.cs metric handle |
| 3585 | Terraria.TimeLogger.MapSectionUpdate | LightingMapBackgroundMetricsProjection.cs metric handle |
| 3586 | Terraria.TimeLogger.MapUpdate | LightingMapBackgroundMetricsProjection.cs metric handle |
| 3587 | Terraria.TimeLogger.SectionFraming | LightingMapBackgroundMetricsProjection.cs metric handle |
| 3588 | Terraria.TimeLogger.SectionRefresh | LightingMapBackgroundMetricsProjection.cs metric handle |
| 3589 | Terraria.TimeLogger.SkyBackground | LightingMapBackgroundMetricsProjection.cs metric handle |
| 3590 | Terraria.TimeLogger.SunMoonStars | LightingMapBackgroundMetricsProjection.cs metric handle |
| 3591 | Terraria.TimeLogger.SurfaceBackground | LightingMapBackgroundMetricsProjection.cs metric handle |
| 3592 | Terraria.TimeLogger.Map | LightingMapBackgroundMetricsProjection.cs metric handle |
| 3594 | Terraria.TimeLogger.Waterfalls | LightingMapBackgroundMetricsProjection.cs metric handle |

### 4.22 SharedIssueReportCatalogState -> C22

| Seq | Source member | Proposed target role/file |
|---:|---|---|
| 1185 | Terraria.DataStructures.GeneralIssueReporter._reports | IssueReportCatalogAdapter.cs catalog |
| 1186 | Terraria.DataStructures.IssueReport.timeReported | IssueReportRecord.cs timestamp |
| 1187 | Terraria.DataStructures.IssueReport.reportText | IssueReportRecord.cs text |

## Ownership, compatibility, and migration controls

### Single-writer rules

- C01 options are written by an authorized diagnostics command; rate fields remain integration-review until world/tile owners accept them.
- C02 cursor is written by the projectile-loop coordinator and reset only by that coordinator. Stopwatches are written by their adapter.
- C03, C10, C15, and C22 are adapter-owned effects with no component-side writes.
- C04 and C05 have one time-series/entry coordinator. C06 and C18 are constructed and invalidated by format factories only.
- C07 streams are written by the caller that owns the stream; factories own seed creation.
- C08 leases are written by Request/Recycle ownership scopes; collection state is written by its collection adapter.
- C09 and C17 scratch buffers are written only inside an operation lease; range/bit value objects are immutable after construction where possible.
- C11 operation context is created and returned by each explicit caller; no new static scratch writer is allowed.
- C12 dispatch is written by DebugCommandDispatcher; C13 options by the authorized options command/server owner; C14 by the frame collector.
- C16 frame state and log resources are written by TimeLoggerFrameCoordinatorAdapter.
- C19-C21 metric samples are written by their domain metric ports; projections only read snapshots.

### Compatibility and dual-write strategy

1. Before any code edit, capture characterization tests against the Version4 mirror for each behavior with deterministic fake ports.
2. Introduce proposed ports and pure value types behind an NLTX-only feature flag or explicit composition root. Do not change the Version4 tree.
3. During a controlled migration, a compatibility adapter may read the legacy value and publish one proposed snapshot. It must not let both old and new paths independently write authoritative state.
4. If dual-write is unavoidable for telemetry, the legacy path remains the sole writer and the proposed path receives a read-only copy until comparison proves equal windows, formatting, and phase ordering.
5. Debug commands and options use shadow parse/validation first. Network/file projections are disabled until authorization and error policies are approved.
6. Remove the compatibility adapter only after focused tests, call-site inventory, and integration-review sign-off. This plan does not authorize that removal.

### Network, snapshot, and persistence strategy

- DebugMessage.Author, command name, arguments, mouse position, and DebugOptions replication use an explicit network projection. No network packet type is stored in a component.
- Runtime options and telemetry snapshots may be serialized only through a proposed versioned snapshot adapter after ownership is assigned. No P20 persistence schema is currently proposed as final.
- TimeLogger output is a diagnostic file projection, not world persistence. File path, compression, truncation, and retry are injected ports.
- Random stream persistence/replay, issue-report retention, and crash dump output require explicit external policies. Until then, adapters return a typed unavailable/error result and do not invent defaults.
- EntityUuid, world persistence IDs, network player slots, external file paths, and GitSHA remain distinct identity domains.

## Per-boundary implementation notes

| ID | Planned implementation note | Rollback condition |
|---|---|---|
| C01 | Add immutable options snapshots and rate query ports; keep UI/stopwatch behind adapters | Any world/entity caller still mutates rates through an implicit global |
| C02 | Add frame-local cursor scope and explicit timing/helper ports | Cursor survives frame end or helper ownership cannot be assigned |
| C03 | Add bounded queue/dedupe and cancellable flush port | Flush loss, duplicate logs, or shutdown timer leak |
| C04 | Port ring operations and derive summaries from immutable snapshot | Window indexes or percentile outputs differ from characterization |
| C05 | Register entries once and route samples by entry handle | Duplicate registration or formatter mutation from a reader |
| C06 | Make formatting construction pure and cache invalidation explicit | Formatting changes for identical input/configuration |
| C07 | Isolate seed source and test replay per stream | Any implicit wall-clock or process-global random dependency remains |
| C08 | Convert buffer use to leases and test recycle ownership | Double recycle, stale stream, or cross-owner buffer access |
| C09 | Port value semantics and bounded bit access | Out-of-range access or changed inclusive/exclusive range semantics |
| C10 | Subscribe/unsubscribe through exception port and route dumps through filesystem port | Crash observer causes gameplay mutation or unbounded recursive failure |
| C11 | Replace static scratch reads with explicit operation contexts | Caller order or reentrancy still depends on global mutable state |
| C12 | Parse, validate, dispatch, and project replies as separate stages | Unauthorized command, malformed input, or network/file side effect bypass |
| C13 | Snapshot options and replicate through authorized adapter | Client can change server authority or joining-player sync diverges |
| C14 | Collect fixed ring snapshots with injected counters | Frame rotation, GC deltas, or timestamps drift from characterization |
| C15 | Read build metadata once and expose unavailable status on failure | World manifest receives a fabricated or mutable SHA |
| C16 | Give frame/log lifecycle one coordinator and explicit start/end effects | File remains open, callback order changes, or compression failure is hidden |
| C17 | Lease font/regex/flood scratch and remove static operation coupling | Concurrent flood fill shares queues or cache invalidation races |
| C18 | Construct named format set from C06 and expose read-only queries | Display formats are mutated by metric writers |
| C19 | Route entity/interface samples through metric ports | Projection begins owning entity/UI state or ordering is implicit |
| C20 | Route tile/liquid samples through metric ports | Render metric writes alter tile/liquid behavior |
| C21 | Route lighting/map/background samples through metric ports | Map/lighting phase order or array bounds change |
| C22 | Append immutable report records through catalog adapter | Timestamp, redaction, retention, or failure policy is unspecified |

## Rollback procedure

Rollback is planned and has not been executed:

1. Disable the proposed composition root/feature flag.
2. Stop all proposed adapters and release buffer, scratch, timer, exception, network, and file resources.
3. Restore the legacy read path as the sole writer; do not delete user changes or broad generated directories.
4. Preserve comparison logs and failure evidence outside source directories, then rerun the focused characterization verifier.
5. Roll back the affected boundary only when its invariant, ownership, or external-effect contract fails; do not roll back unrelated partitions.
6. Reopen integration-review for any cross-partition owner conflict. A rollback does not resolve ownership.

## Focused verifier and serial build plan (not run)

The future verifier must be read-only with respect to the Version4 source and must assert:

- exactly 250 source sequence numbers are mapped once in the two planning documents;
- C04 handles empty, partial, full, and rotated windows with stable summaries;
- C07 replay, explicit seed, and no-clock behavior;
- C08 buffer activation/recycle, Length, collection count, and sort-plan ownership;
- C09 range inclusivity, bit bounds, vertical strips, and scratch isolation;
- C10/C15/C22 fake external ports, error results, teardown, and redaction/retention decisions;
- C11 reentrant operation contexts and no static scratch dependency;
- C12 parse/requirement/dispatch/reply behavior and Author slot versus EntityUuid;
- C13 authorized replication and option snapshot semantics;
- C14 frame ring, GC/allocation snapshots, and display projection;
- C16 frame start/end, callback order, file/compression failure, and metric snapshot order;
- C17 font/regex/flood-fill cache and scratch lifetime;
- C18-C21 format and metric handle registration without phase-state ownership.

When implementation is authorized, the repository command shape is:

pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 <restore|build|test> .\path\AffectedProject.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false

Before every compile-capable command, inspect active dotnet.exe and csc.exe processes, run only one command from the repository root, verify Build/bin output, and run the verifier with --no-build --no-restore. The command was not run for this session because this is documentation-only planning.

Expected future record fields are command, affected project, exit code, warning/error counts, and artifact path under Build/bin/. No such record exists for this session.

## Risks and evidence gaps

- The first-round P20 report is missing; this plan cannot claim that first-round call-site or ownership conclusions were reviewed.
- The source inventory has declarations but not a complete writer/read graph. All unresolved ownership is deferred to integration-review.
- Main rates, projectile loop cursor, DelegateMethods scratch, debug options, command Author, and TimeLogger metric order cross domain boundaries.
- External behavior depends on clock, random seed, file, compression, reflection, network, AppDomain, console, and build metadata APIs that require fake ports and failure semantics.
- Namespace and project placement are proposed from current NLTX organization and are not proof of an existing module.
- No persistence, network packet, or behavior-equivalence acceptance criterion is closed.

Blocking decision: do not claim full production migration until the missing evidence is recovered or explicitly superseded, cross-partition owners are assigned, and persistence/network/external-effect policies are approved. The src2 implementation is complete only for the five saved Component state boundaries; executionStatus is complete for this Component-only slice, while production integration and all deferred non-Component boundaries remain unauthorized.

## Handoff

The implementation owner should use the paired design document for invariants and the complete role rationale, then obtain integration-review approval for:

- simulation-rate and projectile-loop ownership;
- DelegateMethods operation-context seams;
- DebugMessage Author/network projection and DebugOptions authorization;
- TimeLogger phase registration/order and output policy;
- persistence, replay, crash dump, issue retention, and build-status failure behavior.

The P20 Component state implementation was written only under src2 and the Diagnostics library compiled successfully. The existing DiagnosticsVerification build remains failed because of the excluded non-Component BuildStatusAdapter dependency; no production-src migration, full behavior-equivalence validation, persistence/network closure, or cross-partition owner decision was performed.

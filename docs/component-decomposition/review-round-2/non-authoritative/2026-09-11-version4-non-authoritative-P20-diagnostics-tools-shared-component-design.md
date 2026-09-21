# Version4 non-authoritative P20 diagnostics/tools/shared component design

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

## Status and interpretation

Every type, interface, adapter, projection, query, command, system, directory, and file name in this document is proposed. Nothing below claims that NLTX already contains these types or that Version4 behavior has been migrated. The source report is an inventory of declarations, not an implementation or ownership decision.

The paired documents now record only the Component state slice permitted by this implementation session. The current state files are RuntimeDiagnosticsOptionsComponent, RuntimeTickContextComponent, TimeSeriesWindowState, TimeLogEntryState, and DebugRuntimeOptionsComponent under src2. C04 was changed so rotated windows scan all occupied slots before deriving summaries; C05 now validates its series capacity before allocating state. Adapter, Query, Command, Projection, Port, Value-only, System, and verifier files are excluded or deferred and are not counted as completed Components.

## Component implementation checkpoint

The following Component state files are present under D:\TRbackup\NLTX\src2\Diagnostics:

- C01: Runtime/RuntimeDiagnosticsOptionsComponent.cs
- C02: Runtime/RuntimeTickContextComponent.cs
- C04: TimeSeries/TimeSeriesWindowState.cs
- C05: TimeSeries/TimeLogEntryState.cs
- C13: Debug/DebugRuntimeOptionsComponent.cs

C04 was saved with the ring-window rotation fix, and C05 was saved with explicit positive series-capacity validation. The paired documents do not count the existing non-Component source files as Component completion.

## Session handoff

- Handoff command: `pwsh -NoProfile -File .\Build\Tools\Invoke-Version4NonAuthoritativePartitionSession.ps1 -Action Handoff -PartitionId P20 -SessionId d0495ba0efc441268951704d9acf9591 -HandoffId P20-diagnostics-tools-shared-implementation-20260912 -LockWaitSeconds 60`
- Handoff result: `status=handed-off`, `previousStatus=running`, `oldSessionId=d0495ba0efc441268951704d9acf9591`, `sessionId=85646c1d982f4900a64d1e2b10265204`, `claimMode=manual`, `lockReleased=true`.

## Scope

In scope are the 250 source members in the P20 report:

- Runtime composition diagnostics and tick fields from Terraria.Main.
- Call tracking, TimeLogger data-series/entry/formatting/frame state, and phase metric handles.
- Debug command metadata and protocol projections, debug runtime options, detailed frame telemetry, and build status.
- Random streams, buffer leases, segmented collections, ranges, bit values, flood-fill scratch, crash observation, legacy delegate scratch, issue reports, and related utility caches.

Explicitly excluded are gameplay authority, entity identity, world/tile authority, network protocol ownership outside the listed debug projection, Version4 source edits, first-round report creation, other P partitions, current NLTX production implementation, and final cross-partition scheduling decisions.

## Evidence

The source inventory and member sequence numbers come from:

- D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\20-diagnostics-tools-shared.md
- Source report SHA-256: b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196

Direct Version4 evidence inspected:

- D:\TRbackup\Version4\CallTracker.cs, especially the static queue/deduplication/timer and first-occurrence enqueue path.
- D:\TRbackup\Version4\Terraria\Main.cs, including diagnostics/rate declarations, ProjectileUpdateLoopIndex reset/use, update telemetry, and DetailedFPS frame hooks.
- D:\TRbackup\Version4\Terraria\TimeLogger.cs, including DataSeries, TimeLogData, FormatPool, frame start/end, file logging, display formats, and phase metric handles.
- D:\TRbackup\Version4\Terraria\Utils.cs, BitsByte.cs, DelegateMethods.cs, and Terraria.Utilities types.
- D:\TRbackup\Version4\Terraria.DataStructures\BufferPool.cs, CachedBuffer.cs, DoubleStack.cs, EntrySorter.cs, GeneralIssueReporter.cs, and IssueReport.cs.
- D:\TRbackup\Version4\Terraria.Testing.ChatCommands\DebugCommandAttribute.cs, DebugCommandProcessor.cs, DebugMessage.cs, and IDebugCommand.cs.
- D:\TRbackup\Version4\Terraria.Testing\DebugOptions.cs, DetailedFPS.cs, and GitStatus.cs.
- Local tModLoader mirror D:\TRbackup\tmodloader-api-docs-stable, version v2026.07, for CachedBuffer, BufferPool, CrashWatcher, FastRandom, UnifiedRandom, BitsByte, Bits64, IssueReport, GeneralIssueReporter, Main, and Utils API shape.
- Minimal external ECS structure evidence from C:\Users\shan\Downloads\ECS\space-station-14-master for the distinction between shared buffering state, server effects, command adapters, and debug monitor projections.

Missing evidence:

- The expected first-round input D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P20-diagnostics-tools-shared-public-decomposition.md is absent. It was not recreated.
- Complete call-site ownership for every inventoried member, exact persistence/network contracts, and final scheduler order are not established.
- 约束/公共拆分约束.md is referenced by the decomposition material but is absent from this checkout.

## Current NLTX state

No P20 diagnostics/time-logger/debug implementation was found in the inspected NLTX source. Existing code contains unrelated domain-first ECS work, including an EntityProvenanceKind.DebugCommand value; it is not evidence that P20 has been migrated and is not expanded by this plan. The target paths below are proposals only.

## Proposed boundary inventory

| ID | Proposed boundary | Kind | Primary ownership decision |
|---|---|---|---|
| C01 | Runtime diagnostics and simulation-rate view | proposed Component plus Adapter/Projection | Options may be component state; UI, clocks, and simulation rates require integration-review |
| C02 | Runtime tick and projectile-loop context | proposed transient Component plus Adapter/Value | Cursor is frame-local; helper and stopwatch remain ports |
| C03 | Call-tracking sink | proposed Adapter/Projection | Queue, dedupe, timer, and file/log side effects have one sink owner |
| C04 | Time-series window aggregation | proposed Component plus Query | Ring storage is state; summary statistics are derived |
| C05 | Time-log entry state | proposed Component/record | One entry owns label, formatter, budget, display intent, and series references |
| C06 | Diagnostic format pool | proposed Value Object/Query cache | Formatting cache is not gameplay state |
| C07 | Random stream state | proposed Value Objects/Components | Each stream owns its own mutable sequence; no global RNG component |
| C08 | Buffer and collection ownership | proposed Adapter/Value/Collection | Lease and collection ownership stay explicit |
| C09 | Range and bit values | proposed Value Objects plus Scratch Query | Deterministic values and flood-fill scratch remain utility boundaries |
| C10 | Crash and exception observation | proposed Adapter/Projection | AppDomain, console, dump, and filesystem effects stay outside ECS |
| C11 | Legacy metadata and delegate operation context | proposed Value/Adapter/Command context | Legacy scratch is call-scoped and cross-subsystem |
| C12 | Debug command protocol | proposed Command/Adapter/Projection | Reflection, command registry, chat/network, and memo files stay at the edge |
| C13 | Debug runtime options | proposed Component plus network Adapter | Options are diagnostic/test configuration, not entity authority |
| C14 | Detailed frame telemetry | proposed Snapshot/Projection | Ring data and GC/allocation values are observational |
| C15 | Build status | proposed Adapter/Projection | Git SHA is external build metadata |
| C16 | TimeLogger frame coordination | proposed Adapter/System state | Frame/log file lifecycle has a single coordinator |
| C17 | General utility caches and flood-fill scratch | proposed Adapter/Query scratch | Font, regex, random constants, and flood-fill buffers are not shared ECS state |
| C18 | TimeLogger display formats | proposed Value/Query cache | Named format pools are formatting dependencies |
| C19 | Entity/interface phase metrics | proposed Diagnostic Projection | Handles measure phases; they do not own entity state |
| C20 | Tile/liquid render phase metrics | proposed Diagnostic Projection | Handles measure tile/liquid render phases |
| C21 | Lighting/map/background metrics | proposed Diagnostic Projection | Handles measure lighting/map/background phases |
| C22 | Issue-report catalog | proposed Adapter/Projection | Report collection and timestamps are diagnostic output |

## Proposed component and boundary contracts

### C01 - Runtime diagnostics and simulation-rate view (proposed)

Proposed target names are RuntimeDiagnosticsOptionsComponent, NetDiagnosticsUiAdapter, FrameTimingAdapter, and SimulationRateConfigurationCandidate under proposed src2/Diagnostics/Runtime/. showSplash, ignoreErrors, and defaultIP are configuration state with explicit mutation commands. _activeNetDiagnosticsUI and fpsTimer are external UI/clock handles. dayRate and desiredWorldTilesUpdateRate are candidate shared configuration views only; their final owner is integration-review because world/entity systems read them. The adapter reads platform/UI and Stopwatch APIs; the component has no file, network, or clock side effect. Initialization is explicit startup, reset is session teardown, and no persistence claim is made.

### C02 - Runtime tick and projectile-loop context (proposed)

Proposed target names are RuntimeTickContextComponent, WorldUpdateTimingAdapter, and SpelunkerProjectileServicePort under proposed src2/Diagnostics/Runtime/. ProjectileUpdateLoopIndex is a transient cursor written by the projectile loop and reset to -1 at its frame boundary. TARGET_FRAME_TIME is an immutable timing value, _worldUpdateTimeTester is a stopwatch port, and SpelunkerProjectileHelper is an injected helper/service reference. No cursor survives a frame and no helper is serialized. The projectile/world systems are candidate writers; final owner is integration-review.

### C03 - Call-tracking sink (proposed)

Proposed target CallTrackingSinkAdapter under proposed src2/Diagnostics/CallTracking/. LogQueue accepts diagnostic events, LoggedMethods provides process-local first-occurrence deduplication, and FlushTimer schedules flushing. Only the adapter owns enqueue/deduplication/flush and the output port owns logging. The timer must be cancellable at shutdown; failure and retry policy belongs to the output adapter. These fields are not an ECS component.

### C04 - Time-series window aggregation (proposed)

Proposed target TimeSeriesWindowState and TimeSeriesAggregationQuery under proposed src2/Diagnostics/TimeSeries/. values, used, next, count, and usedCount form one bounded ring invariant. previous, median, p90, and max are derived summaries exposed through a read-only query after a sample commit. _sort is scratch storage and must not be shared across concurrent series without synchronization. The time-log coordinator is the sole writer; initialization allocates a fixed window and teardown releases it. No entity, persistence, or network identity is present.

### C05 - Time-log entry state (proposed)

Proposed target TimeLogEntryState and TimeLogEntryCommand under proposed src2/Diagnostics/TimeSeries/. name, format, budget, pendingDisplay, and the two DataSeries slots form one diagnostic entry. A sample writer commits values; a display command toggles pendingDisplay; a query formats a snapshot. Formatter delegates are injected policy, not arbitrary component behavior. Entries are created during TimeLogger initialization, registered once, and retired with the logger; persistence is owned by the frame logging adapter.

### C06 - Diagnostic format pool (proposed)

Proposed target DiagnosticFormatPool under proposed src2/Diagnostics/Formatting/. _format, _minValue, _rounding, _strings, and _nullString form an immutable-after-construction formatting cache. A pure query maps a numeric value to a string; cache construction and invalidation are owned by the formatting adapter. It has no entity scope and must not read or mutate simulation state.

### C07 - Random stream state (proposed)

Proposed target names are FastRandomValue, Lcg32RandomState, UnifiedRandomState, and RandomStreamFactory under proposed src2/Diagnostics/Random/. FastRandom constants and Seed property describe one deterministic stream; LCG32 state is a mutable local stream; UnifiedRandom constants, inext, and SeedArray describe its sequence. A stream owns only its own state, has explicit seed creation, and never reads a process clock except through an injected seed source. Constants are value metadata, not global components. Callers own persistence and replay semantics; final shared owner is integration-review.

### C08 - Buffer and collection ownership (proposed)

Proposed target names are BufferPoolAdapter, CachedBufferLease, SegmentedCollectionState, and EntrySortPlan under proposed src2/Diagnostics/Memory/. Buffer bucket sizes, lock, and queues belong to BufferPoolAdapter. CachedBuffer Data/Reader/Writer/MemoryStream/_isActive/Length belong to one lease with Request/Recycle ownership and duplicate-recycle protection. DoubleStack segmented storage and Count belong to a collection adapter; EntrySorter Steps belongs to an explicit sort plan. Leases are acquired, activated, used, and recycled in one ownership scope; no buffer crosses an owner without a lease contract. These are not entity components.

### C09 - Range and bit values (proposed)

Proposed target names are FloatRangeValue, IntRangeValue, Bits64Value, BitSet2DScratch, Vertical64BitStripsValue, and BitsByteValue under proposed src2/Diagnostics/Values/. Range min/max values are deterministic value objects with explicit inclusivity semantics to be confirmed. Bits64, BitSet2D, Vertical64BitStrips, and BitsByte expose bit operations while preserving bounds and index contracts. BitSet2D scratch used by flood fill is an operation-owned buffer, not a world component. Queries are pure; callers own allocation and lifetime.

### C10 - Crash and exception observation (proposed)

Proposed target CrashObservationAdapter under proposed src2/Diagnostics/Crash/. LogAllExceptions, DumpOnException, DumpOnCrash, CrashDumpOptions, and DumpPath are adapter configuration. The adapter subscribes/unsubscribes to AppDomain exception events, emits a crash projection, and writes console/dump/file output behind explicit ports. Configuration failure is reported once and does not mutate gameplay state. Exact dump ownership and security policy are evidence gaps.

### C11 - Legacy metadata and delegate operation context (proposed)

Proposed target names are LegacyAttributeMetadata, SecretMaterialAdapter, MinecartRenderOperationContext, and LegacyDelegateOperationContext under proposed src2/Diagnostics/Legacy/. OldAttribute message/Message is metadata; Secrets._salt remains secret material behind a secure adapter. Minecart rotationOrigin/rotation and DelegateMethods v3_1, v2_1, f_1, CheckResultOut, tilecut_0, and tileCutIgnore are call-scoped scratch context. Each operation passes an explicit context and returns results; no static mutable scratch is made a shared component. Callers in projectile, tile, wiring, and lighting domains require integration-review for the final seams and ordering.

### C12 - Debug command protocol (proposed)

Proposed target names are DebugCommandCatalogAdapter, DebugCommandRequestProjection, DebugCommandDispatcher, and DebugCommandMetadata under proposed src2/Diagnostics/Debug/. Attribute metadata and IDebugCommand properties describe command identity and requirements. The registry owns command lookup; the dispatcher owns requirement checks and execution; DebugMessage parsing produces a protocol request. Author remains a network/player-slot projection and is never an EntityUuid. MemoCommandsPath is an explicit file adapter. Reply and broadcast effects are projections through a network/chat port. Reflection and third-party protocol types do not enter ECS components.

### C13 - Debug runtime options (proposed)

Proposed target DebugRuntimeOptionsComponent and DebugOptionsReplicationAdapter under proposed src2/Diagnostics/Debug/. The ten DebugOptions fields are diagnostic/test configuration. A command or server authority changes options; systems read a snapshot and never mutate options implicitly. SyncToJoiningPlayer is an adapter action through the network port, not a component method. Ownership, authorization, persistence, and whether each option is server- or client-scoped require integration-review.

### C14 - Detailed frame telemetry (proposed)

Proposed target names are FrameEventRecord, FrameTelemetrySlot, FrameTelemetryRing, GcAllocationSnapshot, and FrameTelemetryProjection under proposed src2/Diagnostics/Telemetry/. Event category/timestamp, per-frame event lists, collection counts, allocated bytes, ring cursors, GC pause, allocation deltas, and display geometry form observational snapshots. The frame collector is the sole writer; a query or projection reads immutable snapshots for display. Ring rotation and GC counters must be deterministic under a supplied clock/counter port. No telemetry value is gameplay authority.

### C15 - Build status (proposed)

Proposed target BuildStatusAdapter and BuildStatusProjection under proposed src2/Diagnostics/Build/. _gitSHA is lazy external build metadata and GitSHA is its read-only projection. The adapter reads the build source once, caches it, and returns an unavailable value on failure without inventing a SHA. World manifest consumers receive a projection; the value is not persisted as entity state.

### C16 - TimeLogger frame coordination (proposed)

Proposed target TimeLoggerFrameCoordinatorAdapter and TimeLoggerFrameState under proposed src2/Diagnostics/TimeLogger/. FrameCount, frame counters, logging flags, DataSeriesHeaders, activeDataSeries, entries, callback queue, ABTest fields, draw queue, and ABTestFlag are coordinated by one proposed frame owner. logWriter and logBuilder are file/string output resources owned by the adapter. StartNextFrame drains callbacks, rotates/commits series, and may open the log; EndDrawFrame flushes/closes it. File path, clock, compression, failure, and retry behavior are explicit ports. The coordinator must call metric writers in a declared order, but the final order is integration-review.

### C17 - General utility caches and flood-fill scratch (proposed)

Proposed target UtilityCacheAdapter and FloodFillScratchQuery under proposed src2/Diagnostics/Utilities/. charLengths and _substitutionRegex are cache dependencies. RANDOM_MULTIPLIER/RANDOM_ADD/RANDOM_MASK are utility constants. _floodFillQueue1, _floodFillQueue2, and _floodFillBitset are one operation's reusable scratch buffers. FloodFillTile receives an explicit scratch lease and returns a result; it does not expose static queues as shared state. Font and regex lifetimes are adapter-owned.

### C18 - TimeLogger display formats (proposed)

Proposed target TimeLoggerDisplayFormatSet under proposed src2/Diagnostics/TimeLogger/. The nine FormatPool handles are named formatting dependencies. Construction is logger initialization; reads occur during display projection; invalidation occurs on display configuration change or logger teardown. No phase metric may mutate a format pool. This boundary depends on C06 and is output-only.

### C19 - Entity/interface phase metrics (proposed)

Proposed target EntityAndInterfaceMetricsProjection under proposed src2/Diagnostics/TimeLogger/Metrics/. The 20 TimeLogData handles for chat, NPCs, projectiles, players, items, rain, gore, dust, particles, leashed entities, interface, graph/logger drawing, overlays, filters, sun visibility, menu, splash, fullscreen map, and GC pause are measurement handles only. Entity and UI systems write samples through a metric port; the projection reads C05/C04 snapshots. It must not become owner of the measured entity state. Cross-subsystem owner and phase boundaries are integration-review.

### C20 - Tile/liquid render phase metrics (proposed)

Proposed target TileAndLiquidMetricsProjection under proposed src2/Diagnostics/TimeLogger/Metrics/. The 24 handles for total draw/update, solid/non-solid/wall/black/water/background-water draws and flushes, wire/clothing/tile extras/nature, and render phases are metric handles. Tile/liquid render systems submit samples; the projection reads time-log entries. It has no tile authority and no implicit render scheduling.

### C21 - Lighting/map/background metrics (proposed)

Proposed target LightingMapBackgroundMetricsProjection under proposed src2/Diagnostics/TimeLogger/Metrics/. The 22 handles for underground/background, render counts, total draw, lighting, painted tiles, requests, waterfalls, map changes/updates/sections, sky/sun/surface background, map, and waterfalls are diagnostic handles. Lighting, map, background, and liquid callers submit samples through explicit ports. Arrays such as TotalDrawByRenderCount and LightingByPass remain bounded metric collections. Final phase order and ownership are integration-review.

### C22 - Issue-report catalog (proposed)

Proposed target IssueReportCatalogAdapter, IssueReportRecord, and IssueReportProjection under proposed src2/Diagnostics/Issues/. GeneralIssueReporter._reports owns a bounded or policy-controlled catalog; IssueReport.timeReported and reportText are immutable record data after creation. A reporter command appends records, a query reads a snapshot, and a projection emits UI/log output. Time is injected; no report becomes an entity or world component. Retention, redaction, and persistence are evidence gaps.

## Complete source-member to proposed-role mapping

The following mapping covers every row in the input report. Sequence numbers are the source-report identifiers; names are copied from the source inventory. A role is a proposed role, not an implemented target.

### 4.1 MainDiagnosticsAndSimulationRates -> C01

| Seq | Source member | Proposed role |
|---:|---|---|
| 116 | Terraria.Main._activeNetDiagnosticsUI | NetDiagnosticsUiAdapter external UI handle |
| 117 | Terraria.Main.fpsTimer | FrameTimingAdapter stopwatch handle |
| 118 | Terraria.Main.showSplash | RuntimeDiagnosticsOptionsComponent option |
| 119 | Terraria.Main.ignoreErrors | RuntimeDiagnosticsOptionsComponent option |
| 120 | Terraria.Main.defaultIP | RuntimeDiagnosticsOptionsComponent external endpoint option; integration-review |
| 121 | Terraria.Main.dayRate | SimulationRateConfigurationCandidate shared rate; integration-review |
| 122 | Terraria.Main.desiredWorldTilesUpdateRate | SimulationRateConfigurationCandidate shared rate; integration-review |

### 4.2 MainTickAndDiagnosticState -> C02

| Seq | Source member | Proposed role |
|---:|---|---|
| 493 | Terraria.Main.ProjectileUpdateLoopIndex | RuntimeTickContextComponent transient projectile cursor |
| 494 | Terraria.Main.TARGET_FRAME_TIME | TargetFrameDuration proposed immutable value |
| 495 | Terraria.Main._worldUpdateTimeTester | WorldUpdateTimingAdapter stopwatch handle |
| 496 | Terraria.Main.SpelunkerProjectileHelper | SpelunkerProjectileServicePort proposed injected helper |

### 4.3 SharedCallTrackingDiagnostics -> C03

| Seq | Source member | Proposed role |
|---:|---|---|
| 1010 | Terraria.CallTracker.LogQueue | CallTrackingSinkAdapter event queue |
| 1011 | Terraria.CallTracker.LoggedMethods | CallTrackingSinkAdapter first-occurrence dedupe |
| 1012 | Terraria.CallTracker.FlushTimer | CallTrackingSinkAdapter flush scheduler |

### 4.4 SharedTimeSeriesDataSeriesState -> C04

| Seq | Source member | Proposed role |
|---:|---|---|
| 3517 | Terraria.TimeLogger.DataSeries.values | TimeSeriesWindowState ring values |
| 3518 | Terraria.TimeLogger.DataSeries.used | TimeSeriesWindowState occupancy |
| 3519 | Terraria.TimeLogger.DataSeries.next | TimeSeriesWindowState write cursor |
| 3520 | Terraria.TimeLogger.DataSeries.count | TimeSeriesWindowState window count |
| 3521 | Terraria.TimeLogger.DataSeries.usedCount | TimeSeriesWindowState populated count |
| 3522 | Terraria.TimeLogger.DataSeries.previous | TimeSeriesAggregationQuery previous value |
| 3523 | Terraria.TimeLogger.DataSeries.median | TimeSeriesAggregationQuery median |
| 3524 | Terraria.TimeLogger.DataSeries.p90 | TimeSeriesAggregationQuery p90 |
| 3525 | Terraria.TimeLogger.DataSeries.max | TimeSeriesAggregationQuery maximum |
| 3526 | Terraria.TimeLogger.DataSeries._sort | TimeSeriesWindowState sort scratch |

### 4.5 SharedTimeSeriesEntryState -> C05

| Seq | Source member | Proposed role |
|---:|---|---|
| 3527 | Terraria.TimeLogger.TimeLogData.name | TimeLogEntryState label |
| 3528 | Terraria.TimeLogger.TimeLogData.format | TimeLogEntryState formatter policy |
| 3529 | Terraria.TimeLogger.TimeLogData.budget | TimeLogEntryState budget |
| 3530 | Terraria.TimeLogger.TimeLogData.pendingDisplay | TimeLogEntryState display intent |
| 3531 | Terraria.TimeLogger.TimeLogData.data | TimeLogEntryState two-series slots |

### 4.6 SharedTimeSeriesFormattingState -> C06

| Seq | Source member | Proposed role |
|---:|---|---|
| 3532 | Terraria.TimeLogger.FormatPool._format | DiagnosticFormatPool format template |
| 3533 | Terraria.TimeLogger.FormatPool._minValue | DiagnosticFormatPool lower bound |
| 3534 | Terraria.TimeLogger.FormatPool._rounding | DiagnosticFormatPool rounding policy |
| 3535 | Terraria.TimeLogger.FormatPool._strings | DiagnosticFormatPool string cache |
| 3536 | Terraria.TimeLogger.FormatPool._nullString | DiagnosticFormatPool null display |

### 4.7 SharedRandomSources -> C07

| Seq | Source member | Proposed role |
|---:|---|---|
| 2957 | Terraria.Utilities.FastRandom.RANDOM_MULTIPLIER | FastRandomValue algorithm constant |
| 2958 | Terraria.Utilities.FastRandom.RANDOM_ADD | FastRandomValue algorithm constant |
| 2959 | Terraria.Utilities.FastRandom.RANDOM_MASK | FastRandomValue algorithm constant |
| 2963 | Terraria.Utilities.LCG32Random.state | Lcg32RandomState mutable stream state |
| 2985 | Terraria.Utilities.UnifiedRandom.MBIG | UnifiedRandomState algorithm constant |
| 2986 | Terraria.Utilities.UnifiedRandom.MSEED | UnifiedRandomState algorithm constant |
| 2987 | Terraria.Utilities.UnifiedRandom.MZ | UnifiedRandomState algorithm constant |
| 2988 | Terraria.Utilities.UnifiedRandom.inext | UnifiedRandomState sequence cursor |
| 2989 | Terraria.Utilities.UnifiedRandom.SeedArray | UnifiedRandomState sequence storage |
| 3939 | Terraria.Utilities.FastRandom.Seed | FastRandomValue stream seed property |

### 4.8 SharedBufferAndCollectionPools -> C08

| Seq | Source member | Proposed role |
|---:|---|---|
| 1069 | Terraria.DataStructures.BufferPool.SMALL_BUFFER_SIZE | BufferPoolAdapter bucket policy |
| 1070 | Terraria.DataStructures.BufferPool.MEDIUM_BUFFER_SIZE | BufferPoolAdapter bucket policy |
| 1071 | Terraria.DataStructures.BufferPool.LARGE_BUFFER_SIZE | BufferPoolAdapter bucket policy |
| 1072 | Terraria.DataStructures.BufferPool.HUGE_BUFFER_SIZE | BufferPoolAdapter bucket policy |
| 1073 | Terraria.DataStructures.BufferPool.bufferLock | BufferPoolAdapter synchronization |
| 1074 | Terraria.DataStructures.BufferPool.SmallBufferQueue | BufferPoolAdapter lease queue |
| 1075 | Terraria.DataStructures.BufferPool.MediumBufferQueue | BufferPoolAdapter lease queue |
| 1076 | Terraria.DataStructures.BufferPool.LargeBufferQueue | BufferPoolAdapter lease queue |
| 1077 | Terraria.DataStructures.BufferPool.HugeBufferQueue | BufferPoolAdapter lease queue |
| 1078 | Terraria.DataStructures.CachedBuffer.Data | CachedBufferLease byte storage |
| 1079 | Terraria.DataStructures.CachedBuffer.Writer | CachedBufferLease writer |
| 1080 | Terraria.DataStructures.CachedBuffer.Reader | CachedBufferLease reader |
| 1081 | Terraria.DataStructures.CachedBuffer._memoryStream | CachedBufferLease stream |
| 1082 | Terraria.DataStructures.CachedBuffer._isActive | CachedBufferLease ownership flag |
| 1088 | Terraria.DataStructures.DoubleStack<T1>._segmentList | SegmentedCollectionState storage |
| 1089 | Terraria.DataStructures.DoubleStack<T1>._segmentSize | SegmentedCollectionState segment policy |
| 1090 | Terraria.DataStructures.DoubleStack<T1>._segmentCount | SegmentedCollectionState segment count |
| 1091 | Terraria.DataStructures.DoubleStack<T1>._segmentShiftPosition | SegmentedCollectionState index shift |
| 1092 | Terraria.DataStructures.DoubleStack<T1>._start | SegmentedCollectionState start cursor |
| 1093 | Terraria.DataStructures.DoubleStack<T1>._end | SegmentedCollectionState end cursor |
| 1094 | Terraria.DataStructures.DoubleStack<T1>._size | SegmentedCollectionState size |
| 1095 | Terraria.DataStructures.DoubleStack<T1>._last | SegmentedCollectionState last cursor |
| 1140 | Terraria.DataStructures.EntrySorter<TEntryType, TStepType>.Steps | EntrySortPlan steps |
| 3686 | Terraria.DataStructures.CachedBuffer.Length | CachedBufferLease length query |
| 3687 | Terraria.DataStructures.DoubleStack<T1>.Count | SegmentedCollectionState count query |

### 4.9 SharedRangeAndBitUtilities -> C09

| Seq | Source member | Proposed role |
|---:|---|---|
| 2951 | Terraria.Utilities.Terraria.Utilities.FloatRange.Minimum | FloatRangeValue lower bound |
| 2952 | Terraria.Utilities.Terraria.Utilities.FloatRange.Maximum | FloatRangeValue upper bound |
| 2953 | Terraria.Utilities.Bits64.v | Bits64Value storage |
| 2954 | Terraria.Utilities.BitSet2D.offset | BitSet2DScratch origin |
| 2955 | Terraria.Utilities.BitSet2D.size | BitSet2DScratch extent |
| 2956 | Terraria.Utilities.BitSet2D.bits | BitSet2DScratch words |
| 2961 | Terraria.Utilities.IntRange.Minimum | IntRangeValue lower bound |
| 2962 | Terraria.Utilities.IntRange.Maximum | IntRangeValue upper bound |
| 2990 | Terraria.Utilities.Vertical64BitStrips.arr | Vertical64BitStripsValue storage |
| 2997 | Terraria.BitsByte.value | BitsByteValue storage |
| 3931 | Terraria.Utilities.Bits64.this[] | Bits64Value bit query/set |
| 3932 | Terraria.Utilities.Bits64.IsEmpty | Bits64Value emptiness query |
| 3933 | Terraria.Utilities.BitSet2D.this[] | BitSet2DScratch bit query/set |
| 3942 | Terraria.Utilities.Vertical64BitStrips.this[] | Vertical64BitStripsValue strip query |
| 3943 | Terraria.BitsByte.this[] | BitsByteValue bit query/set |

### 4.10 SharedGeneralDiagnosticsUtilities -> C10

| Seq | Source member | Proposed role |
|---:|---|---|
| 3934 | Terraria.Utilities.CrashWatcher.LogAllExceptions | CrashObservationAdapter configuration |
| 3935 | Terraria.Utilities.CrashWatcher.DumpOnException | CrashObservationAdapter configuration |
| 3936 | Terraria.Utilities.CrashWatcher.DumpOnCrash | CrashObservationAdapter configuration |
| 3937 | Terraria.Utilities.CrashWatcher.CrashDumpOptions | CrashObservationAdapter dump policy |
| 3938 | Terraria.Utilities.CrashWatcher.DumpPath | CrashObservationAdapter output path |

### 4.11 SharedGeneralDelegateAndMetadataUtilities -> C11

| Seq | Source member | Proposed role |
|---:|---|---|
| 2965 | Terraria.Utilities.OldAttribute.message | LegacyAttributeMetadata message |
| 2966 | Terraria.Utilities.Secrets._salt | SecretMaterialAdapter protected material |
| 3034 | Terraria.DelegateMethods.Minecart.rotationOrigin | MinecartRenderOperationContext origin |
| 3035 | Terraria.DelegateMethods.Minecart.rotation | MinecartRenderOperationContext rotation |
| 3036 | Terraria.DelegateMethods.v3_1 | LegacyDelegateOperationContext vector result |
| 3037 | Terraria.DelegateMethods.v2_1 | LegacyDelegateOperationContext vector input |
| 3038 | Terraria.DelegateMethods.f_1 | LegacyDelegateOperationContext scalar |
| 3039 | Terraria.DelegateMethods.CheckResultOut | LegacyDelegateOperationContext result flag |
| 3040 | Terraria.DelegateMethods.tilecut_0 | LegacyDelegateOperationContext tile-cut context |
| 3041 | Terraria.DelegateMethods.tileCutIgnore | LegacyDelegateOperationContext ignore mask |
| 3940 | Terraria.Utilities.OldAttribute.Message | LegacyAttributeMetadata message projection |

### 4.12 DebugCommandProtocol -> C12

| Seq | Source member | Proposed role |
|---:|---|---|
| 2891 | Terraria.Testing.ChatCommands.DebugCommandAttribute.InternalDebugCommand._processMethod | DebugCommandDispatcher reflected process port |
| 2892 | Terraria.Testing.ChatCommands.DebugCommandAttribute.Name | DebugCommandMetadata command name |
| 2893 | Terraria.Testing.ChatCommands.DebugCommandAttribute.Description | DebugCommandMetadata description |
| 2894 | Terraria.Testing.ChatCommands.DebugCommandAttribute.Requirements | DebugCommandMetadata requirement policy |
| 2895 | Terraria.Testing.ChatCommands.DebugCommandAttribute.HelpText | DebugCommandMetadata help text |
| 2896 | Terraria.Testing.ChatCommands.DebugCommandProcessor._commands | DebugCommandCatalogAdapter registry |
| 2897 | Terraria.Testing.ChatCommands.DebugCommandProcessor.MemoCommandsPath | MemoCommandFileAdapter path |
| 2898 | Terraria.Testing.ChatCommands.DebugMessage.COMMAND_PREFIX | DebugMessage protocol prefix |
| 2899 | Terraria.Testing.ChatCommands.DebugMessage.Author | DebugMessage network/player-slot projection |
| 2900 | Terraria.Testing.ChatCommands.DebugMessage.CommandName | DebugCommandRequestProjection name |
| 2901 | Terraria.Testing.ChatCommands.DebugMessage.Arguments | DebugCommandRequestProjection arguments |
| 2902 | Terraria.Testing.ChatCommands.DebugMessage.MousePosition | DebugCommandRequestProjection pointer |
| 3922 | Terraria.Testing.ChatCommands.DebugCommandAttribute.InternalDebugCommand.Name | DebugCommandMetadata name contract |
| 3923 | Terraria.Testing.ChatCommands.DebugCommandAttribute.InternalDebugCommand.Description | DebugCommandMetadata description contract |
| 3924 | Terraria.Testing.ChatCommands.DebugCommandAttribute.InternalDebugCommand.HelpText | DebugCommandMetadata help contract |
| 3925 | Terraria.Testing.ChatCommands.DebugCommandAttribute.InternalDebugCommand.Requirements | DebugCommandMetadata requirements contract |
| 3926 | Terraria.Testing.ChatCommands.IDebugCommand.Name | IDebugCommand proposed port |
| 3927 | Terraria.Testing.ChatCommands.IDebugCommand.Description | IDebugCommand proposed port |
| 3928 | Terraria.Testing.ChatCommands.IDebugCommand.HelpText | IDebugCommand proposed port |
| 3929 | Terraria.Testing.ChatCommands.IDebugCommand.Requirements | IDebugCommand proposed port |

### 4.13 DebugRuntimeOptions -> C13

| Seq | Source member | Proposed role |
|---:|---|---|
| 2903 | Terraria.Testing.DebugOptions.enableDebugCommands | DebugRuntimeOptionsComponent option |
| 2904 | Terraria.Testing.DebugOptions.Shared_ReportCommandUsage | DebugRuntimeOptionsComponent option |
| 2905 | Terraria.Testing.DebugOptions.Shared_ServerPing | DebugRuntimeOptionsComponent diagnostic value |
| 2906 | Terraria.Testing.DebugOptions.UpdateWaitInMs | DebugRuntimeOptionsComponent timing option |
| 2907 | Terraria.Testing.DebugOptions.noLimits | DebugRuntimeOptionsComponent option |
| 2908 | Terraria.Testing.DebugOptions.ShowNetOffsetDust | DebugRuntimeOptionsComponent option |
| 2909 | Terraria.Testing.DebugOptions.FakeNetOffset | DebugRuntimeOptionsComponent test vector |
| 2910 | Terraria.Testing.DebugOptions.NoDamageVar | DebugRuntimeOptionsComponent option |
| 2911 | Terraria.Testing.DebugOptions.LetProjectilesAimAtTargetDummies | DebugRuntimeOptionsComponent option |
| 2912 | Terraria.Testing.DebugOptions.PracticeMode | DebugRuntimeOptionsComponent option |

### 4.14 DebugFrameTelemetry -> C14

| Seq | Source member | Proposed role |
|---:|---|---|
| 2913 | Terraria.Testing.DetailedFPS.Frame.Event.category | FrameEventRecord category |
| 2914 | Terraria.Testing.DetailedFPS.Frame.Event.timestamp | FrameEventRecord timestamp |
| 2915 | Terraria.Testing.DetailedFPS.Frame.events | FrameTelemetrySlot event list |
| 2916 | Terraria.Testing.DetailedFPS.Frame.CollectionCount | FrameTelemetrySlot GC counts |
| 2917 | Terraria.Testing.DetailedFPS.Frame.Allocated | FrameTelemetrySlot allocation count |
| 2918 | Terraria.Testing.DetailedFPS.FrameCount | FrameTelemetryRing capacity |
| 2919 | Terraria.Testing.DetailedFPS.Frames | FrameTelemetryRing slots |
| 2920 | Terraria.Testing.DetailedFPS.oldest | FrameTelemetryRing oldest cursor |
| 2921 | Terraria.Testing.DetailedFPS.newest | FrameTelemetryRing newest cursor |
| 2922 | Terraria.Testing.DetailedFPS.LastGCPauseTime | GcAllocationSnapshot pause |
| 2923 | Terraria.Testing.DetailedFPS.LastCollectionCount | GcAllocationSnapshot counts |
| 2924 | Terraria.Testing.DetailedFPS.LastAllocatedBytes | GcAllocationSnapshot bytes |
| 2925 | Terraria.Testing.DetailedFPS.PixelsPerMs | FrameTelemetryProjection scale |
| 2926 | Terraria.Testing.DetailedFPS.FrameWidth | FrameTelemetryProjection width |
| 2927 | Terraria.Testing.DetailedFPS.BoxHeight | FrameTelemetryProjection height |
| 2928 | Terraria.Testing.DetailedFPS._gcGenText | FrameTelemetryProjection text cache |

### 4.15 DebugBuildStatus -> C15

| Seq | Source member | Proposed role |
|---:|---|---|
| 2929 | Terraria.Testing.GitStatus._gitSHA | BuildStatusAdapter lazy external value |
| 3930 | Terraria.Testing.GitStatus.GitSHA | BuildStatusProjection read-only value |

### 4.16 SharedTimeLoggerFrameCoordinationState -> C16

| Seq | Source member | Proposed role |
|---:|---|---|
| 3537 | Terraria.TimeLogger.FrameCount | TimeLoggerFrameState frame capacity |
| 3538 | Terraria.TimeLogger.logWriter | TimeLoggerFrameCoordinatorAdapter file writer |
| 3539 | Terraria.TimeLogger.logBuilder | TimeLoggerFrameCoordinatorAdapter output builder |
| 3540 | Terraria.TimeLogger.framesToLog | TimeLoggerFrameState logging quota |
| 3541 | Terraria.TimeLogger.currentFrame | TimeLoggerFrameState current cursor |
| 3542 | Terraria.TimeLogger.startLoggingNextFrame | TimeLoggerFrameState start command |
| 3543 | Terraria.TimeLogger.endLoggingThisFrame | TimeLoggerFrameState end command |
| 3544 | Terraria.TimeLogger.currentlyLogging | TimeLoggerFrameState active flag |
| 3545 | Terraria.TimeLogger.DataSeriesHeaders | TimeLoggerFrameState header projection |
| 3546 | Terraria.TimeLogger.activeDataSeries | TimeLoggerFrameState active-series cursor |
| 3547 | Terraria.TimeLogger.entries | TimeLoggerFrameState entry registry |
| 3614 | Terraria.TimeLogger._onNextFrame | TimeLoggerFrameCoordinatorAdapter callback queue |
| 3615 | Terraria.TimeLogger.ABTestMode | TimeLoggerFrameState experiment mode |
| 3616 | Terraria.TimeLogger.ABTestName | TimeLoggerFrameState experiment name |
| 3617 | Terraria.TimeLogger._entriesToDraw | TimeLoggerFrameCoordinatorAdapter draw queue |
| 4024 | Terraria.TimeLogger.ABTestFlag | TimeLoggerFrameState experiment projection |

### 4.17 SharedGeneralRandomAndBufferUtilities -> C17

| Seq | Source member | Proposed role |
|---:|---|---|
| 3647 | Terraria.Utils.charLengths | UtilityCacheAdapter font-width cache |
| 3648 | Terraria.Utils._substitutionRegex | UtilityCacheAdapter regex cache |
| 3649 | Terraria.Utils.RANDOM_MULTIPLIER | UtilityCacheAdapter algorithm constant |
| 3650 | Terraria.Utils.RANDOM_ADD | UtilityCacheAdapter algorithm constant |
| 3651 | Terraria.Utils.RANDOM_MASK | UtilityCacheAdapter algorithm constant |
| 3652 | Terraria.Utils._floodFillQueue1 | FloodFillScratchQuery queue lease |
| 3653 | Terraria.Utils._floodFillQueue2 | FloodFillScratchQuery queue lease |
| 3654 | Terraria.Utils._floodFillBitset | FloodFillScratchQuery bitset lease |

### 4.18 SharedTimeLoggerDisplayFormattingState -> C18

| Seq | Source member | Proposed role |
|---:|---|---|
| 3618 | Terraria.TimeLogger._PinnedCPUFormat | TimeLoggerDisplayFormatSet named format |
| 3619 | Terraria.TimeLogger._AssignedCPUFormat | TimeLoggerDisplayFormatSet named format |
| 3620 | Terraria.TimeLogger._procThrottleFormat | TimeLoggerDisplayFormatSet named format |
| 3621 | Terraria.TimeLogger._expectedCPUFormat | TimeLoggerDisplayFormatSet named format |
| 3622 | Terraria.TimeLogger._terrariaCPUFormat | TimeLoggerDisplayFormatSet named format |
| 3623 | Terraria.TimeLogger._pendingCPUFormat | TimeLoggerDisplayFormatSet named format |
| 3624 | Terraria.TimeLogger._percentFormat | TimeLoggerDisplayFormatSet named format |
| 3625 | Terraria.TimeLogger._msFormat | TimeLoggerDisplayFormatSet named format |
| 3626 | Terraria.TimeLogger._intFormat | TimeLoggerDisplayFormatSet named format |

### 4.19 SharedTimeLoggerEntityAndInterfacePhaseMetricsState -> C19

| Seq | Source member | Proposed role |
|---:|---|---|
| 3593 | Terraria.TimeLogger.PlayerChat | EntityAndInterfaceMetricsProjection metric handle |
| 3595 | Terraria.TimeLogger.NPCs | EntityAndInterfaceMetricsProjection metric handle |
| 3596 | Terraria.TimeLogger.Projectiles | EntityAndInterfaceMetricsProjection metric handle |
| 3597 | Terraria.TimeLogger.Players | EntityAndInterfaceMetricsProjection metric handle |
| 3598 | Terraria.TimeLogger.Items | EntityAndInterfaceMetricsProjection metric handle |
| 3599 | Terraria.TimeLogger.Rain | EntityAndInterfaceMetricsProjection metric handle |
| 3600 | Terraria.TimeLogger.Gore | EntityAndInterfaceMetricsProjection metric handle |
| 3601 | Terraria.TimeLogger.Dust | EntityAndInterfaceMetricsProjection metric handle |
| 3602 | Terraria.TimeLogger.Particles | EntityAndInterfaceMetricsProjection metric handle |
| 3603 | Terraria.TimeLogger.LeashedEntities | EntityAndInterfaceMetricsProjection metric handle |
| 3604 | Terraria.TimeLogger.Interface | EntityAndInterfaceMetricsProjection metric handle |
| 3605 | Terraria.TimeLogger.DrawFPSGraph | EntityAndInterfaceMetricsProjection metric handle |
| 3606 | Terraria.TimeLogger.DrawTimeLogger | EntityAndInterfaceMetricsProjection metric handle |
| 3607 | Terraria.TimeLogger.Overlays | EntityAndInterfaceMetricsProjection metric handle |
| 3608 | Terraria.TimeLogger.Filters | EntityAndInterfaceMetricsProjection metric handle |
| 3609 | Terraria.TimeLogger.SunVisibility | EntityAndInterfaceMetricsProjection metric handle |
| 3610 | Terraria.TimeLogger.MenuDrawTime | EntityAndInterfaceMetricsProjection metric handle |
| 3611 | Terraria.TimeLogger.SplashDrawTime | EntityAndInterfaceMetricsProjection metric handle |
| 3612 | Terraria.TimeLogger.DrawFullscreenMap | EntityAndInterfaceMetricsProjection metric handle |
| 3613 | Terraria.TimeLogger.GCPause | EntityAndInterfaceMetricsProjection metric handle |

### 4.20 SharedTimeLoggerTileAndLiquidRenderMetricsState -> C20

| Seq | Source member | Proposed role |
|---:|---|---|
| 3548 | Terraria.TimeLogger.TotalDrawAndUpdate | TileAndLiquidMetricsProjection metric handle |
| 3549 | Terraria.TimeLogger.DrawSolidTiles | TileAndLiquidMetricsProjection metric handle |
| 3550 | Terraria.TimeLogger.FlushSolidTiles | TileAndLiquidMetricsProjection metric handle |
| 3551 | Terraria.TimeLogger.SolidDrawCalls | TileAndLiquidMetricsProjection metric handle |
| 3552 | Terraria.TimeLogger.DrawNonSolidTiles | TileAndLiquidMetricsProjection metric handle |
| 3553 | Terraria.TimeLogger.FlushNonSolidTiles | TileAndLiquidMetricsProjection metric handle |
| 3554 | Terraria.TimeLogger.NonSolidDrawCalls | TileAndLiquidMetricsProjection metric handle |
| 3555 | Terraria.TimeLogger.DrawBlackTiles | TileAndLiquidMetricsProjection metric handle |
| 3556 | Terraria.TimeLogger.DrawWallTiles | TileAndLiquidMetricsProjection metric handle |
| 3557 | Terraria.TimeLogger.FlushWallTiles | TileAndLiquidMetricsProjection metric handle |
| 3558 | Terraria.TimeLogger.WallDrawCalls | TileAndLiquidMetricsProjection metric handle |
| 3559 | Terraria.TimeLogger.DrawWaterTiles | TileAndLiquidMetricsProjection metric handle |
| 3560 | Terraria.TimeLogger.LiquidDrawCalls | TileAndLiquidMetricsProjection metric handle |
| 3561 | Terraria.TimeLogger.DrawBackgroundWaterTiles | TileAndLiquidMetricsProjection metric handle |
| 3562 | Terraria.TimeLogger.LiquidBackgroundDrawCalls | TileAndLiquidMetricsProjection metric handle |
| 3565 | Terraria.TimeLogger.DrawWireTiles | TileAndLiquidMetricsProjection metric handle |
| 3566 | Terraria.TimeLogger.ClothingRacks | TileAndLiquidMetricsProjection metric handle |
| 3567 | Terraria.TimeLogger.TileExtras | TileAndLiquidMetricsProjection metric handle |
| 3568 | Terraria.TimeLogger.Nature | TileAndLiquidMetricsProjection metric handle |
| 3569 | Terraria.TimeLogger.RenderSolidTiles | TileAndLiquidMetricsProjection metric handle |
| 3570 | Terraria.TimeLogger.RenderNonSolidTiles | TileAndLiquidMetricsProjection metric handle |
| 3571 | Terraria.TimeLogger.RenderBlacksAndWalls | TileAndLiquidMetricsProjection metric handle |
| 3573 | Terraria.TimeLogger.RenderBackgroundLiquid | TileAndLiquidMetricsProjection metric handle |
| 3574 | Terraria.TimeLogger.RenderLiquid | TileAndLiquidMetricsProjection metric handle |

### 4.21 SharedTimeLoggerLightingMapAndBackgroundMetricsState -> C21

| Seq | Source member | Proposed role |
|---:|---|---|
| 3563 | Terraria.TimeLogger.DrawUndergroundBackground | LightingMapBackgroundMetricsProjection metric handle |
| 3564 | Terraria.TimeLogger.DrawOldUndergroundBackground | LightingMapBackgroundMetricsProjection metric handle |
| 3572 | Terraria.TimeLogger.RenderUndergroundBackground | LightingMapBackgroundMetricsProjection metric handle |
| 3575 | Terraria.TimeLogger.TotalDrawByRenderCount | LightingMapBackgroundMetricsProjection bounded metric array |
| 3576 | Terraria.TimeLogger.TotalDrawRenderNow | LightingMapBackgroundMetricsProjection metric handle |
| 3577 | Terraria.TimeLogger.TotalDraw | LightingMapBackgroundMetricsProjection metric handle |
| 3578 | Terraria.TimeLogger.Lighting | LightingMapBackgroundMetricsProjection metric handle |
| 3579 | Terraria.TimeLogger.LightingInit | LightingMapBackgroundMetricsProjection metric handle |
| 3580 | Terraria.TimeLogger.LightingByPass | LightingMapBackgroundMetricsProjection bounded metric array |
| 3581 | Terraria.TimeLogger.FindPaintedTiles | LightingMapBackgroundMetricsProjection metric handle |
| 3582 | Terraria.TimeLogger.PrepareRequests | LightingMapBackgroundMetricsProjection metric handle |
| 3583 | Terraria.TimeLogger.FindingWaterfalls | LightingMapBackgroundMetricsProjection metric handle |
| 3584 | Terraria.TimeLogger.MapChanges | LightingMapBackgroundMetricsProjection metric handle |
| 3585 | Terraria.TimeLogger.MapSectionUpdate | LightingMapBackgroundMetricsProjection metric handle |
| 3586 | Terraria.TimeLogger.MapUpdate | LightingMapBackgroundMetricsProjection metric handle |
| 3587 | Terraria.TimeLogger.SectionFraming | LightingMapBackgroundMetricsProjection metric handle |
| 3588 | Terraria.TimeLogger.SectionRefresh | LightingMapBackgroundMetricsProjection metric handle |
| 3589 | Terraria.TimeLogger.SkyBackground | LightingMapBackgroundMetricsProjection metric handle |
| 3590 | Terraria.TimeLogger.SunMoonStars | LightingMapBackgroundMetricsProjection metric handle |
| 3591 | Terraria.TimeLogger.SurfaceBackground | LightingMapBackgroundMetricsProjection metric handle |
| 3592 | Terraria.TimeLogger.Map | LightingMapBackgroundMetricsProjection metric handle |
| 3594 | Terraria.TimeLogger.Waterfalls | LightingMapBackgroundMetricsProjection metric handle |

### 4.22 SharedIssueReportCatalogState -> C22

| Seq | Source member | Proposed role |
|---:|---|---|
| 1185 | Terraria.DataStructures.GeneralIssueReporter._reports | IssueReportCatalogAdapter catalog |
| 1186 | Terraria.DataStructures.IssueReport.timeReported | IssueReportRecord injected timestamp |
| 1187 | Terraria.DataStructures.IssueReport.reportText | IssueReportRecord text |

## Dependencies and proposed scheduling contract

The following is a candidate dependency graph, not a final runtime order:

1. C01 and C02 expose immutable configuration and frame-local context through explicit read ports.
2. C07 and C09 provide deterministic value objects; C08 and C17 provide owned scratch/lease resources.
3. C04 commits a time-series sample; C05 owns entry registration and C06 provides pure formatting.
4. C19, C20, and C21 receive phase samples through metric ports and do not call one another.
5. C14 collects observational frame data. C16 starts/ends a frame, drains callbacks, rotates C04 entries, and asks the metric boundaries for snapshots.
6. C13 options are changed through a command boundary; C12 validates and dispatches debug requests; projections publish replies.
7. C03, C10, C15, and C22 publish diagnostics through independent adapters. Their failures cannot mutate gameplay authority.

Any cross-partition ordering, including Main rates versus world/entity systems, DelegateMethods context versus tile/projectile/lighting callers, and TimeLogger phase metric order, is marked crossSubsystemOwner: integration-review. No file or directory order may define execution order.

## Identity and effect boundaries

- EntityUuid is reserved for entity identity and is not present in this inventory.
- DebugMessage.Author is a protocol/player-slot projection, not an EntityUuid.
- defaultIP is an external endpoint configuration and is not a persistent world identity.
- GitSHA is external build metadata, not an entity or world persistence ID.
- File paths, AppDomain events, Stopwatch/clock reads, random seeds, reflection methods, network messages, console output, crash dumps, and compressed TimeLogger output cross only through proposed adapters or projections.
- No P20 member establishes a persistence schema. Persistence and snapshot formats are deferred until integration-review identifies the owning world/session boundary.

## Evidence gaps and blocking decisions

Evidence gaps are intentionally explicit:

- First-round P20 decomposition report is missing.
- Member names and declarations are confirmed, but complete readers, writers, initialization, cleanup, concurrency, retention, persistence, network, and retry semantics are not uniformly confirmed.
- Main.dayRate, Main.desiredWorldTilesUpdateRate, Main.ProjectileUpdateLoopIndex, DelegateMethods scratch, DebugOptions.Shared_ServerPing, DebugMessage.Author, and TimeLogger phase ownership cross subsystem boundaries.
- Crash dump security, issue-report retention/redaction, memo-command file policy, debug-option authorization, and build-SHA failure behavior need owner decisions.
- Existing NLTX has no confirmed P20 target modules, so namespace, registration, and project ownership remain proposed.

Blocking decision: the five saved state Components are the complete independently implementable Component-only slice currently supported by the paired plan. C03, C06-C12, and C14-C22 remain deferred because their proposed boundaries are adapters, queries, commands, projections, values, operation contexts, or dependency-dependent coordinator work; integration-review must assign shared owners and external-effect policies before those boundaries can be implemented.

## Focused verifier plan (not run)

When implementation is authorized, the focused verifier should:

- Parse the source inventory and assert all 250 source sequence numbers map exactly once.
- Test DataSeries ring rotation, empty/partial windows, median/p90/max derivation, and concurrent access policy.
- Test BufferPool request/recycle ownership, CachedBuffer activation, Length, and duplicate recycle rejection.
- Test random stream seed/replay behavior without implicit wall-clock access.
- Test range/bit bounds, flood-fill scratch isolation, and pure query behavior.
- Test debug command parsing, requirement rejection, author-slot projection, memo path policy, and reply/broadcast ports.
- Test option synchronization, frame telemetry ring rotation, TimeLogger start/end failure handling, and metric snapshot ordering.
- Test crash/issue/build adapters with fake clock, filesystem, network, and exception ports.
- Run only the affected project through Build/Tools/Invoke-SerialDotnet.ps1 with UseSharedCompilation=false and then run verifiers with --no-build --no-restore, after checking active dotnet.exe/csc.exe ownership. These commands were not run in this session.

## Integration handoff

The next owner must receive:

- the exact source report SHA and 250-member mapping above;
- the C01-C22 proposed boundary list and candidate target paths in the paired execution plan;
- the explicit integration-review items for rates, delegate scratch, debug protocol IDs, TimeLogger order, persistence, network, crash dumps, and issue retention;
- the fact that first-round P20 evidence is missing;
- the fact that implementationStatus is completed only for the saved C01/C02/C04/C05/C13 Component state slice; the Diagnostics library build is independently verified, while the existing verifier build remains failed and behavior, network, persistence, and cross-partition closure are unverified.

No production `src`, Version4 source, first-round report, other partition output, or test code was modified. The runner updated the non-authoritative ledger through the Handoff action; the lock was not manually changed. The Diagnostics build generated its artifact under `Build/bin`.

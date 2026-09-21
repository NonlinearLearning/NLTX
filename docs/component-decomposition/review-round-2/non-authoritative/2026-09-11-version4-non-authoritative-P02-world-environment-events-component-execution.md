# Version4 非权威 P02 世界环境与事件：proposed Execution Plan

partitionId: P02
sessionId: e8b587c9624249b99352262ee7d52f49
previousSessionId: e6d8f732513b4ac6947b01343c0dad30
handoffId: P02-world-environment-events-component-implementation-20260912-1859
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\02-world-environment-events.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P02-world-environment-events-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P02-world-environment-events-component-execution.md
designStatus: proposed
executionStatus: completed (src2-only Component implementation checkpoint)
implementationStatus: completed
verificationStatus: partial (serial build passed; focused verifier and behavior-equivalence not run)
src2CodeModified: yes
completedComponents:
- SlimeRainStateComponent
- SlimeRainPolicyComponent
- SlimeRainProgressComponent
- CalendarClockComponent
- CalendarVisualPresentationStateComponent
- WorldCalendarOverrideStateComponent
- WeatherPresentationStateComponent
- CoinRainEventStateComponent
- CloudFieldStateComponent
- AmbientWindStateComponent
- WindPhysicsPolicyComponent
- SkyPresentationStateComponent
- InvasionProgressPresentationStateComponent
- SeasonalWorldStateComponent
- SeasonalOverridePolicyComponent
- TitleRefreshRequestStateComponent
- CreditsRollPresentationStateComponent
- ScreenObstructionPresentationStateComponent
- BannerKillProgressStateComponent
- BannerClaimableCountStateComponent
- BannerClaimNotificationStateComponent
- BossEncounterOutcomeStateComponent
- TreeTopVariationStateComponent
- BackgroundChangeFlashStateComponent
- AmbientSpawnScheduleStateComponent
- BirthdayPartyStateComponent
- LanternNightStateComponent
- MysticFairyEventStateComponent
- CultistRitualStateComponent
- SandstormStateComponent
- Dd2InvasionProgressionStateComponent
- Dd2InvasionRunStateComponent
- Dd2InvasionWaveStateComponent
- Dd2InvasionCrystalDropStateComponent
currentComponent: none
pendingComponents: []
deferredComponents:
- WorldInvasionStateComponent: requires unresolved InvasionType from prohibited production source.
- Dd2InvasionArenaStateComponent: requires an unresolved arena rectangle or coordinate value type.
lastCheckpointUtc: 2026-09-12T11:10:07.0292606Z
evidence-gap: 已实现组件仅包含可独立表达的原始值状态；WorldInvasionStateComponent 依赖未解析的 InvasionType，Dd2InvasionArenaStateComponent 依赖未闭合的竞技场几何值类型，二者 deferred；credits/ScreenObstruction caller/reset/render/packet receive、Banner catalog ID mapping/claim consumer/full-state receive/save/network writer、claimable count consumption/notification consumer、Boss tracker session/lifecycle owner、localization/message projection、TreeTop area catalog/variation writer/packet receive/save fallback、BackgroundChangeFlash trigger/consumer/resource lifecycle、ambient schedule reset/consumer and forced-request buffer、BirthdayParty/LanternNight roster/transition/natural-attempt/packet owner、MysticFairy tile-scan/spawn/coordinate/Version4-empty-method owner、CultistRitual recheck persistence/reset and spawn transaction、Sandstorm duration/start-stop/random/packet/reset owner、DD2 run/wave/crystal writer and event/session identity remain open.
implementationCheckpoint: Component-only implementation completed for all independently expressible P02 Components under src2/WorldSession; this checkpoint was continued after runner Handoff. System, Query, Command, Adapter, Projection, registration, network, persistence, scheduler, test and behavior-equivalence work remains excluded and unimplemented.
blocking-decision: crossSubsystemOwner: integration-review；不得让组件补齐未实现的 System、Query、Command、Adapter、Projection、注册、网络、持久化或测试边界；需另行裁决 InvasionType、DD2 竞技场几何、跨组件 writer、协议 DTO 和快照 owner。

## 1. Plan status and non-execution declaration

This file began as a follow-up implementation plan for the P02 non-authoritative second round.
The Component-only subset has now been saved under `src2/WorldSession` and continued after the
runner Handoff. Every non-Component target path, namespace, interface, command, adapter, projection
and scheduler edge below remains `proposed`; no registration, network migration, persistence
migration, test, verifier or behavior-equivalence work has been executed.

The plan remains staged by evidence checkpoints. The 34 independently expressible Component
checkpoints are saved, while `WorldInvasionStateComponent` and
`Dd2InvasionArenaStateComponent` remain deferred. `executionStatus` is completed for the
src2-only Component checkpoint; `verificationStatus` remains `not-run` until the required serial
build is executed.

## 2. Proposed target layout

The non-Component paths below remain target locations only. The Component paths listed in
`completedComponents` are actual files under `src2/WorldSession`; no non-Component file is claimed
by this session.

```text
status: proposed
src2/WorldSession/Calendar/SlimeRainStateComponent.cs
src2/WorldSession/Calendar/SlimeRainPolicyComponent.cs
src2/WorldSession/Calendar/SlimeRainProgressComponent.cs
src2/WorldSession/Calendar/SlimeRainSystem.cs
src2/WorldSession/Calendar/SlimeRainWarningSystem.cs
src2/WorldSession/Calendar/SlimeRainEligibilityQuery.cs
src2/WorldSession/Calendar/SlimeRainNetworkProjection.cs
src2/WorldSession/Events/Dd2/Dd2InvasionDamageTrackingSession.cs
src2/WorldSession/Events/Dd2/Dd2InvasionDamageTrackingSystem.cs
src2/WorldSession/Events/Dd2/Dd2InvasionDamageTrackingNameQuery.cs
src2/WorldSession/Events/Dd2/Dd2InvasionDamageOutcomeProjection.cs
src2/WorldSession/Events/Dd2/IDd2InvasionDamageTrackerPort.cs
src2/WorldSession/Events/Dd2/Dd2InvasionDefinition.cs
src2/WorldSession/Events/Dd2/Dd2InvasionPresentationPolicy.cs
src2/WorldSession/Events/Dd2/Dd2InvasionProgressionStateComponent.cs
src2/WorldSession/Events/Dd2/Dd2InvasionRunStateComponent.cs
src2/WorldSession/Events/Dd2/Dd2InvasionWaveStateComponent.cs
src2/WorldSession/Events/Dd2/Dd2InvasionWavePolicy.cs
src2/WorldSession/Events/Dd2/Dd2InvasionArenaStateComponent.cs
src2/WorldSession/Events/Dd2/Dd2InvasionCrystalDropStateComponent.cs
src2/WorldSession/Events/Dd2/Dd2InvasionDeathPositionBuffer.cs
src2/WorldSession/Events/Dd2/Dd2InvasionProgressionSystem.cs
src2/WorldSession/Events/Dd2/Dd2InvasionWaveSystem.cs
src2/WorldSession/Events/Dd2/Dd2InvasionArenaSystem.cs
src2/WorldSession/Events/Dd2/Dd2InvasionCrystalDropSystem.cs
src2/WorldSession/Events/Dd2/Dd2InvasionWaveStatusQuery.cs
src2/WorldSession/Events/Dd2/Dd2InvasionArenaQuery.cs
src2/WorldSession/Events/Dd2/Dd2InvasionBuildingBlockQuery.cs
src2/WorldSession/Events/Dd2/Dd2InvasionBartenderReadinessQuery.cs
src2/WorldSession/Events/Dd2/Dd2InvasionNetworkProjection.cs
src2/WorldSession/Events/Dd2/Dd2InvasionPersistenceAdapter.cs
```

All seventeen component groups now have proposed target locations in their checkpoints. Domain-first
organization suggests `WorldSession/Calendar` for calendar/world-event state and
`WorldSession/Events/Dd2` for DD2-specific progression, wave, arena and tracker boundaries, with a
separate capability directory for environment scans or rendering-only projections. No generic
`Shared/Components/` directory is proposed. Proposed type names follow the repository component
naming constraints but are not approved registration keys.

## 3. First execution checkpoint: `MainSlimeRainState`

### 3.1 Source-to-role mapping

| Version4 member | proposed target role | proposed write owner | migration note |
| --- | --- | --- | --- |
| `maxRain` | weather-capacity configuration input | proposed weather capacity owner | Keep outside slime-rain state; verify existing rain-array ownership before any move. |
| `slimeWarningTime` | `SlimeRainStateComponent.WarningRemaining` | proposed `SlimeRainWarningSystem` | Preserve zero-crossing broadcast semantics; no direct NPC writer. |
| `slimeWarningDelay` | `SlimeRainPolicyComponent.WarningDelay` | proposed calendar/event policy command | Confirm whether it is runtime configuration or persisted world data. |
| `slimeRainNPCSlots` | `SlimeRainPolicyComponent.NpcSlotMultiplier` | proposed policy initialization | Read-only for NPC spawn calculations. |
| `slimeRainNPC` | proposed eligibility cache/query input | proposed content-registration adapter | Do not dual-write from Main and NPC systems; clear/rebuild at world lifecycle boundaries. |
| `slimeRainTime` | `SlimeRainStateComponent.TimeState` | proposed `SlimeRainSystem` | Preserve positive-active and negative-cooldown semantics until integration decides persistence. |
| `slimeRain` | `SlimeRainStateComponent.Active` | proposed `SlimeRainSystem` | Start/stop commands go through one state writer. |
| `slimeRainKillCount` | `SlimeRainProgressComponent.KillCount` | proposed NPC-kill event command handler | Verify threshold reset and stop behavior before changing representation. |

### 3.2 Proposed implementation sequence

1. Freeze an evidence-backed behavior table for start, stop, warning, cooldown, NPC eligibility and
   kill progress. Record all Version4 branches before adding any target type.
2. Introduce proposed state shape behind a compatibility read adapter. Do not remove or rename a
   legacy field until network, save, NPC and calendar readers are inventoried.
3. Add one proposed `SlimeRainSystem` as the sole writer for active/time transitions and one
   proposed warning system for warning countdown/message intent.
4. Route NPC qualification through a pure proposed query. The query returns eligibility/slot data;
   it does not modify event state. NPC kills submit an explicit progress command to the event writer.
5. Add network and persistence projections only after the authoritative state is committed. Packet
   and save DTO types remain outside core components behind adapters.
6. Run the focused verifier plan and serial affected-project checks only in a later implementation
   session under the repository `Invoke-SerialDotnet.ps1` contract.

### 3.3 Single-write and compatibility strategy

The proposed single write owner is `SlimeRainSystem` for `Active` and `TimeState`,
`SlimeRainWarningSystem` for `WarningRemaining`, and an explicitly coordinated event-progress
writer for `KillCount`. During migration, a compatibility adapter may read legacy Main fields and
produce the proposed component, but it must not write both representations independently.

The proposed dual-read window ends only after every direct Version4 reader (NPC spawn, calendar
tick, message/network path, and reset path) is routed through the proposed ports and the focused
verifier proves one state transition per command. If a command result is unknown after an external
send, do not retry blindly; the adapter must expose the actual delivery semantics.

### 3.4 Network, snapshot and persistence strategy

- Core state contains only domain values and stable candidate IDs; no packet writer, localization
  object, file stream, UI object or third-party texture type.
- `proposed SlimeRainNetworkProjection` observes committed state and produces a versioned network
  view. Its final `NetworkId`/packet owner is `crossSubsystemOwner: integration-review`.
- A proposed persistence adapter may save active/time/progress only after a decision on whether the
  negative cooldown state is authoritative recovery data or a derived schedule. No save format is
  changed in this plan.
- Client presentation observes warning/event facts through a projection. It cannot write
  `Active`, `TimeState` or `KillCount`.

## 4. Second execution checkpoint: `MainCalendarWeatherState`

### 4.1 Source-member-to-role mapping

| Version4 member | proposed target role/file | proposed single writer | migration note |
| --- | --- | --- | --- |
| `dayTime` | `CalendarClockComponent.IsDayTime` in `src2/WorldSession/Calendar/CalendarClockComponent.cs` | proposed `CalendarTransitionSystem` | Preserve day/night boundary and network projection; do not let gameplay readers write it. |
| `time` | `CalendarClockComponent.TimeOfDay` | proposed `CalendarClockSystem` | Preserve `dayRate`, day/night reset points and 0..cycle-length interpretation. |
| `timeForVisualEffects` | `CalendarVisualPresentationStateComponent.VisualTime` | proposed visual-clock adapter/system | Direct Version4 writer is unresolved; keep outside authority until client update path is known. |
| `moonPhase` | `CalendarClockComponent.MoonPhase` | proposed `CalendarTransitionSystem` | Preserve 0..7 increment/wrap and blood-moon eligibility dependency. |
| `sunModY` | `CalendarVisualPresentationStateComponent.SunOffsetY` | proposed `CalendarVisualProjection` adapter | Network packet 18 currently carries it; sender/receiver and persistence are not closed. |
| `moonModY` | `CalendarVisualPresentationStateComponent.MoonOffsetY` | proposed `CalendarVisualProjection` adapter | Same boundary as `sunModY`; no core clock ownership. |
| `bloodMoon` | `WorldCalendarOverrideStateComponent.BloodMoon` | proposed `SeasonalMoonTransitionSystem` | Preserve night-only roll, day cleanup and mutual exclusion. |
| `pumpkinMoon` | `WorldCalendarOverrideStateComponent.PumpkinMoon` | proposed moon-event command/system | Preserve start/stop commands and network bit projection. |
| `snowMoon` | `WorldCalendarOverrideStateComponent.SnowMoon` | proposed moon-event command/system | Preserve start/stop commands and network bit projection. |
| `cloudAlpha` | `WeatherPresentationStateComponent.CloudAlpha` | proposed `RainPresentationSystem` | Keep as derived/client-facing value; do not dual-write with `maxRaining`. |
| `maxRaining` | `WorldWeatherState.MaximumRainStrength` in `src/WorldSession/WorldWeatherState.cs` | proposed `RainWeatherSystem` | Preserve `ChangeRain` random strength and non-raining packet normalization. |
| `oldMaxRaining` | `WeatherNetworkProjectionState.LastPublishedRainStrength` | proposed `WeatherNetworkProjection` | Projection cache only; exclude from world persistence. |
| `rainTime` | `WorldWeatherState.RainTime` | proposed `RainWeatherSystem` | Preserve infinite-rain threshold, freeze-rain behavior and stop-at-zero. |
| `raining` | `WorldWeatherState.IsRaining` | proposed `RainWeatherSystem` | One write root for start/stop; gameplay systems receive read-only weather view. |
| `coinRain` | `CoinRainEventStateComponent.RemainingValue` | proposed `CoinRainSystem` | Preserve initialization, storm/stop/reset cleanup and non-negative consumption. |
| `eclipse` | `WorldCalendarOverrideStateComponent.Eclipse` | proposed `SeasonalMoonTransitionSystem` | Preserve day-start roll, network command path and night/world reset. |

### 4.2 Proposed implementation sequence

1. Freeze a behavior table for `UpdateTime`, `UpdateTime_StartDay`, `UpdateTime_StartNight`,
   `StartRain`, `StopRain`, `ChangeRain`, moon-event commands, `NetMessage` packet 7/18 and
   `WorldGen` reset/coin-rain consumption.
2. Introduce compatibility read ports for clock, weather and calendar overrides. Keep the legacy
   write roots active until visual, network and persistence consumers are inventoried; do not dual
   write component and legacy state independently.
3. Add the proposed clock transition writer and weather writer, then route all gameplay readers
   through read-only views. Add seasonal moon commands only after their mutual-exclusion tests pass.
4. Add `CoinRainSystem` as the sole progress consumer and a separate visual/network projection for
   cloud alpha, rain strength change notifications and sun/moon offsets.
5. Add versioned network/persistence adapters after the committed state shape is stable. The final
   `NetworkId`, persistence section, snapshot shape and cross-system schedule remain
   `crossSubsystemOwner: integration-review`.
6. Run the focused verifier and affected-project serial checks in a later implementation session.

### 4.3 Compatibility, snapshot and rollback strategy

During migration, compatibility adapters may read legacy `Main` fields into proposed state at a
single boundary. There must be one writer for each authority: calendar transition for time/day/moon
phase, rain system for rain facts, moon-event system for override flags, coin-rain system for its
remaining value. `cloudAlpha`, offsets and `oldMaxRaining` remain projections or caches until direct
writer/receiver evidence is complete.

Network packet 7 and packet 18 must be treated as external protocol adapters. A projection can only
serialize a committed snapshot and must retain the existing bit/width semantics during a compatibility
window. Persistence must decide whether rain timers, seasonal flags and coin-rain are authoritative
save values or recomputable state before changing a save schema. Roll back by disabling the proposed
writer and restoring the last compatibility read/write boundary if a verifier finds an incorrect
day/night edge, event mutual exclusion, rain stop, packet normalization, coin-rain cleanup or stale
projection cache. No source rollback is performed by this planning session.

### 4.4 Focused verifier additions

- Clock: day/night threshold, time reset, day-rate zero, moon phase wrap and one transition per tick.
- Calendar overrides: blood-moon/eclipses and pumpkin/snow moon mutual exclusion, network command
  rejection/duplication and world reset cleanup.
- Weather: rain start/stop, infinite rain, freeze power, strength changes, packet-7 normalization,
  and `oldMaxRaining` deduplication.
- Coin rain: guaranteed start, storm/stop cancellation, world reset and consumption clamping.
- Isolation: visual/network projections observe committed state only and no Query mutates any component.

## 5. Third execution checkpoint: `MainWeatherAndAmbientState`

### 5.1 Source-member-to-role mapping

| Version4 member | proposed target role/file | proposed single writer | migration note |
| --- | --- | --- | --- |
| `numStars` | `SkyPresentationStateComponent.StarCount` | proposed `SkyPresentationSystem` | Client-only count from `Star.SpawnStars`; keep Star objects outside world authority. |
| `weatherCounter` | `CloudFieldStateComponent.WeatherAdjustmentTimer` | proposed `CloudFieldSystem` | Preserve the 3600..10800 random reset and no implicit Query writes. |
| `numClouds` | `CloudFieldStateComponent.ActiveCount` | proposed `CloudFieldSystem` | Preserve 0..200 commit, rain-strength replenishment and packet-7 projection. |
| `numCloudsTemp` | `CloudFieldStateComponent.TargetCountWorkset` | proposed `CloudFieldSystem` | Transient target; clamp/commit at the same boundary as legacy UpdateWeather. |
| `windSpeedCurrent` | `AmbientWindStateComponent.CurrentSpeed` | proposed `AmbientWindSystem` | Preserve interpolation toward rain-adjusted target and read-only consumers. |
| `windSpeedTarget` | `AmbientWindStateComponent.TargetSpeed` | proposed `AmbientWindSystem` | Preserve random changes, player-based cap and packet-7 width. |
| `windCounter` | `AmbientWindStateComponent.ChangeTimer` | proposed `AmbientWindSystem` | Preserve reset range and Lantern Night/freeze conditions. |
| `extremeWindCounter` | `AmbientWindStateComponent.ExtremeChangeTimer` | proposed `AmbientWindSystem` | Preserve extreme-wind extensions and separate timer semantics. |
| `windPhysics` | `WindPhysicsPolicyComponent.Enabled` | proposed policy/configuration owner | Keep Projectile compatibility query pure; final configuration owner is unresolved. |
| `windPhysicsStrength` | `WindPhysicsPolicyComponent.Strength` | proposed policy/configuration owner | Do not infer this from current wind speed; preserve legacy multiplier behavior. |
| `cloud` | `CloudPresentationStateComponent.Slots` in proposed client presentation module | proposed `CloudPresentationAdapter` | `Cloud` contains presentation/platform data and fixed pool lifecycle; no core component field. |

### 5.2 Proposed implementation sequence

1. Capture the `ResetWindCounter`, `UpdateWeather`, `RandomizeWeather`, `Star.SpawnStars`, packet-7
   serialization and all wind/cloud consumers as behavior fixtures before moving any state.
2. Introduce a compatibility weather adapter that exposes read-only wind/cloud views. Preserve the
   legacy write root while confirming whether `FastRandom`, `UnifiedRandom`, freeze powers and Lantern
   Night pause semantics are part of the authoritative weather contract.
3. Add the proposed `AmbientWindSystem` and `CloudFieldSystem` with one writer per timer/value. Keep
   `Cloud[]` and star objects behind client presentation adapters and do not serialize third-party
   objects through the core component.
4. Route NPC, Player, Projectile, Rain and WorldGen consumers through pure weather/wind queries, then
   add packet projections after committed values are normalized. Add policy configuration only after
   the final wind-physics owner is agreed.
5. Add versioned persistence/network adapters only after timer, random stream and client lifecycle
   semantics are closed. The target files and scheduler remain proposed.
6. Run focused weather tests and affected-project serial checks in a later implementation session.

### 5.3 Single-write, compatibility and rollback strategy

The proposed single writer is `AmbientWindSystem` for wind speeds and wind timers, `CloudFieldSystem`
for cloud counts/timer, and `SkyPresentationSystem`/`CloudPresentationAdapter` for client-only
objects. During compatibility, legacy fields may be read at one adapter boundary but must not be
independently mutated by both representations. `windPhysics` and `windPhysicsStrength` remain a
policy boundary until integration review identifies their configuration/network owner.

Packet-7 `windSpeedTarget` and `numClouds` must retain their existing wire widths during a compatibility
window. No claim is made that the unsent counters/current wind/cloud object state is networked. Save
schema changes wait for evidence of whether weather timers and random state are persisted. Roll back
if wind interpolation, player cap, cloud clamp, freeze/pause semantics, packet normalization or
client object lifecycle diverges, or if a second writer can mutate the same state without a command.

### 5.4 Focused verifier additions

- Wind convergence, random timer ranges, extreme-wind extensions, player/no-player cap, and pause/freeze behavior.
- Cloud target/active clamping, rain-driven replenishment, fixed slot count and packet-7 cloud count.
- Star count generation and client-only object ownership; no presentation object enters a world snapshot.
- Wind-physics eligibility for projectile types and isolation of policy reads from wind-state writes.

## 6. Fourth execution checkpoint: `MainInvasionState`

### 6.1 Proposed target files and interfaces

`WorldInvasionStateComponent.cs` remains deferred because its `InvasionType` value is unresolved.
`InvasionProgressPresentationStateComponent.cs` is saved under `src2/WorldSession`; all other
entries in this proposed split remain non-Component design only.

```text
status: proposed
src2/WorldSession/Calendar/WorldInvasionStateComponent.cs
src2/WorldSession/Calendar/InvasionProgressPresentationStateComponent.cs
src2/WorldSession/Calendar/InvasionLifecycleSystem.cs
src2/WorldSession/Calendar/InvasionProgressSystem.cs
src2/WorldSession/Calendar/InvasionSpawnQuery.cs
src2/WorldSession/Calendar/InvasionWarningProjection.cs
src2/WorldSession/Calendar/InvasionProgressProjection.cs
src2/WorldSession/Calendar/InvasionNetworkAdapter.cs
src2/WorldSession/Calendar/InvasionPersistenceAdapter.cs
```

The proposed command seam is an explicit `StartInvasionCommand`, `InvasionProgressCommand`,
`AdvanceInvasionCommand` and `CompleteInvasionCommand` boundary. `InvasionLifecycleSystem` is the
proposed single authority writer; `InvasionProgressSystem` validates NPC group/delta input and emits
the progress command without mutating world state. Network packet numbers, persistent IDs, NPC entity
IDs, localization keys and any shared event snapshot stay behind proposed adapters and
`crossSubsystemOwner: integration-review`.

### 6.2 Source-member-to-role mapping

| Version4 member | proposed target role/file | proposed write owner | migration note |
| --- | --- | --- | --- |
| `invasionType` | `WorldInvasionStateComponent.Type` | proposed `InvasionLifecycleSystem` | Preserve type values 1..4 and the `None` cleanup; verify the `int`/enum conversion and packet-7 `sbyte` boundary. |
| `invasionX` | `WorldInvasionStateComponent.PositionX` | proposed `InvasionLifecycleSystem` | Preserve edge/Martian initialization, minimum one-tile movement and `spawnTileX` arrival clamp. |
| `invasionSize` | `WorldInvasionStateComponent.Size` | proposed `InvasionLifecycleSystem` after `InvasionProgressCommand` | NPC death becomes a command input; do not let NPC code and the component writer independently decrement the value. |
| `invasionDelay` | `WorldInvasionStateComponent.Delay` | proposed `InvasionLifecycleSystem` | Consume the day-boundary decrement event and external start commands; preserve the spawn gate at exactly zero. |
| `invasionWarn` | `WorldInvasionStateComponent.WarningTimer` | proposed `InvasionLifecycleSystem` | Preserve initial values, arrival warning, zero-crossing warning and 3600 reset. |
| `invasionSizeStart` | `WorldInvasionStateComponent.SizeStart` | proposed `InvasionLifecycleSystem` | Preserve player-count/type formulas and `FakeLoadInvasionStart` reconstruction; persistence is not yet closed. |
| `invasionProgressIcon` | `InvasionProgressPresentationStateComponent.ProgressIcon` | proposed `InvasionProgressPresentationSystem` | Presentation cache only; ordinary invasion icon is `invasionType + 3`, while moon/DD2 paths use other icons. |
| `invasionProgress` | `InvasionProgressPresentationStateComponent.Progress` or proposed compatibility sentinel view | proposed `InvasionProgressPresentationSystem` | Derive ordinary progress from committed `SizeStart - Size`; preserve moon-event `-1` only at an adapter boundary until the component invariant decision is made. |
| `invasionProgressMax` | `InvasionProgressPresentationStateComponent.ProgressMax` | proposed `InvasionProgressPresentationSystem` | Preserve ordinary start-size and moon/DD2 wave maxima; never use this field as the remaining-size authority. |
| `invasionProgressWave` | `InvasionProgressPresentationStateComponent.ProgressWave` | proposed `InvasionProgressPresentationSystem` | Preserve initialization and wave reports; no presentation write may change invasion lifecycle state. |
| `invasionProgressDisplayLeft` | `InvasionProgressPresentationStateComponent.DisplayFramesRemaining` | proposed `InvasionProgressPresentationSystem` / client adapter | Preserve the 160-frame report pulse and moon-event reset; full decrement consumer remains an evidence gap. |
| `invasionProgressAlpha` | `InvasionProgressPresentationStateComponent.Alpha` | proposed client presentation adapter | Keep client-only until the direct writer and update loop are found; do not infer it from display frames. |

### 6.3 Proposed implementation sequence

1. Freeze behavior fixtures for `StartInvasion`, `UpdateInvasion`, `InvasionWarning`,
   `FakeLoadInvasionStart`, `SyncAnInvasion`, `ReportInvasionProgress`, day-boundary delay
   decrement, the NPC spawn gate, NPC kill-group mapping, packet 7/78 writing, external negative
   invasion commands and the moon-event `-1` progress reset.
2. Define a compatibility read adapter that exposes the existing Version4 fields as a read-only
   invasion fact view. Keep the legacy write root until the NPC progress path, warning side effects,
   tracker lifecycle, network receive behavior and persistence boundary are inventoried.
3. Introduce the proposed six-field `WorldInvasionStateComponent` authority boundary and make
   `InvasionLifecycleSystem` the only writer. Route day-boundary ticks, start commands, position
   movement, completion cleanup and validated progress commands through it; do not dual-write
   Version4 `Main` and proposed state.
4. Add `InvasionProgressSystem` as a command translator for NPC deaths. Validate the active type,
   invasion group, positive point value and clamp behavior before the authority writer commits the
   remaining size. Coordinate `InvasionDamageTracker` start/stop with its separate proposed owner;
   this is `crossSubsystemOwner: integration-review`.
5. Add pure `InvasionSpawnQuery`, then route NPC spawn readers through it. Add warning and progress
   projections only after committed state is available; keep localization, packet 7/78 serialization,
   UI timing and alpha behind adapters/projections.
6. Add network and persistence adapters after packet 78 receive, packet 7 world-info decode, type
   widths, save fields and moon-event sentinel representation are confirmed. Run the focused verifier
   and affected-project serial checks only in a later implementation session.

### 6.4 Single-write, compatibility and rollback strategy

The proposed single write root for the six authority fields is `InvasionLifecycleSystem`.
`InvasionProgressSystem` produces an explicit command and cannot directly change `Size`;
`InvasionSpawnQuery`, `InvasionWarningProjection` and `InvasionProgressProjection` are read-only
consumers. During compatibility, one adapter may read legacy `Main` fields into the proposed view,
but a field must not be independently mutated in both representations. Completion must commit the
type/size/delay cleanup before warning, network or UI projections observe the terminal state.

The existing NLTX component's non-negative `Progress` invariant remains unchanged in this plan. An
ordinary invasion progress value is derived from committed authority; the Version4 moon-event `-1`
value is held as a proposed compatibility marker until integration review selects a representation.
Rollback by disabling the proposed writer and returning the compatibility adapter to the last legacy
read boundary if start qualification, size formulas, movement, delay gating, warning cadence, NPC kill
clamping, completion flags or sentinel handling diverge. No source rollback is performed here.

### 6.5 Network, snapshot and persistence strategy

- Treat packet 7 world-info and packet 78 progress as external protocol adapters. Packet 7 carries
  `invasionType` as `sbyte`; packet 78 writes progress, maximum and icon/wave values using
  `int,int,sbyte,sbyte`. Preserve those widths during any compatibility window.
- `MessageBuffer` currently has a no-op packet-78 receive branch in the inspected source, so no
  client receive or duplicate-command equivalence is claimed. Do not invent a decode path from the
  packet writer alone.
- A proposed invasion snapshot contains committed authority values only, with a separate optional
  presentation view. `EntityId` for NPC instances, `NetworkId` for protocol endpoints,
  `PersistentEntityId` for save records and external localization/content IDs must remain distinct;
  their final owners are `crossSubsystemOwner: integration-review`.
- No save/load mapping for the 12 fields was confirmed. Do not change a save schema until
  `invasionSizeStart`, delay/warning recovery, progress presentation and `InvasionDamageTracker`
  lifecycle are explicitly classified as authoritative or derived.

### 6.6 Focused verifier additions

- Start/duplicate-start rejection, qualifying-player counts, type-specific size formulas, initial
  side/Martian position, fake-load baseline reconstruction and no-player behavior.
- Per-tick position movement with `dayRate < 1`, arrival warning, warning timer reset, delay
  decrement, size-zero completion cleanup, event flag notification and world-info emission.
- NPC spawn eligibility for type/delay/size/position, invasion-group matching, point values,
  clamp-to-zero and exactly-once progress command submission.
- Ordinary progress derivation, packet-7/78 widths, packet-78 receive/duplicate behavior once
  evidence exists, and moon-event `-1` compatibility without violating the ordinary component
  invariant.
- Projection isolation: warning/progress/UI/network/persistence adapters observe committed state only;
  no Query or projection mutates the authority component. Save round trip, alpha lifecycle and
  `InvasionDamageTracker` ownership remain not-run/evidence-gap.

## 7. Fifth execution checkpoint: `MainSeasonalAndTitleState`

### 7.1 Proposed target files and interfaces

`PendingWorldEventStateComponent` and `WorldSecretSeedSeasonalRules*` remain current NLTX reference
shapes. `SeasonalWorldStateComponent`, `SeasonalOverridePolicyComponent` and
`TitleRefreshRequestStateComponent` are saved under `src2/WorldSession`; all other entries in this
proposed split remain non-Component design only.

```text
status: proposed
src2/WorldSession/Calendar/SeasonalWorldStateComponent.cs
src2/WorldSession/Calendar/SeasonalOverridePolicyComponent.cs
src2/WorldSession/Calendar/TitleRefreshRequestStateComponent.cs
src2/WorldSession/Calendar/SeasonalCalendarSystem.cs
src2/WorldSession/Calendar/SeasonalOverrideCommand.cs
src2/WorldSession/Calendar/SecretSeedSeasonalPolicyAdapter.cs
src2/WorldSession/Calendar/SeasonalWorldProjection.cs
src2/WorldSession/Calendar/TitleRefreshProjection.cs
```

The proposed command seam is `SeasonalOverrideCommand` for explicit daily/forever policy changes and
`TitleRefreshCommand` for a one-shot client title effect. `SeasonalCalendarSystem` is the proposed
single writer for the two active seasonal facts and the daily consume/reset transition;
`SecretSeedSeasonalPolicyAdapter` supplies forever policy inputs from secret-seed configuration and
save recovery. `TitleRefreshProjection` is an effect adapter, not a world-state writer. Network IDs,
persistent IDs, secret-seed IDs and platform title API IDs remain `crossSubsystemOwner: integration-review`.

### 7.2 Source-member-to-role mapping

| Version4 member | proposed target role/file | proposed write owner | migration note |
| --- | --- | --- | --- |
| `xMas` | `SeasonalWorldStateComponent.IsChristmasActive` | proposed `SeasonalCalendarSystem` | Recompute from the injected calendar/date view plus Christmas today/forever policy; preserve the December 15 boundary and read-only NPC/Item/WorldGen consumers. |
| `halloween` | `SeasonalWorldStateComponent.IsHalloweenActive` | proposed `SeasonalCalendarSystem` | Recompute from the injected calendar/date view plus Halloween today/forever policy; preserve the October/November 1 window and permit coexistence with Christmas where Version4 does. |
| `forceXMasForToday` | `SeasonalOverridePolicyComponent.ForceChristmasToday` | proposed `SeasonalOverrideCommand` / day-start reducer | Preserve moon-wave and saved daily override semantics, version-212 save field and world-info bit; consume/reset exactly once at day start. |
| `forceHalloweenForToday` | `SeasonalOverridePolicyComponent.ForceHalloweenToday` | proposed `SeasonalOverrideCommand` / day-start reducer | Preserve the version-212 save field, world-info bit and temporary moon-event transitions; coordinate with current `PendingWorldEventStateComponent` without dual write. |
| `forceXMasForever` | `SeasonalOverridePolicyComponent.ForceChristmasForever` | proposed `SecretSeedSeasonalPolicyAdapter` / recovery adapter | Preserve endless-Christmas initialization, version-287 save field and world-reset clear. |
| `forceHalloweenForever` | `SeasonalOverridePolicyComponent.ForceHalloweenForever` | proposed `SecretSeedSeasonalPolicyAdapter` / recovery adapter | Preserve endless-Halloween initialization, version-287 save field and world-reset clear. |
| `changeTheTitle` | `TitleRefreshRequestStateComponent.Pending` or proposed `TitleRefreshCommand` | proposed `TitleRefreshProjection` consumer | Main-loop consumption clears the request and calls the platform title API; direct writer is missing, so do not infer a world component owner. |

### 7.3 Proposed implementation sequence

1. Freeze behavior fixtures for `checkXMas`, `checkHalloween`, `isHalloweenDateNow`,
   `CheckForMoonEventsStartingTemporarySeasons`, `UpdateTime_StartDay`, secret-seed initialization,
   world reset, `WorldFile` version 212/287 save/load, world-info bit 11 and main-loop title refresh.
2. Define a calendar/date port and a read-only seasonal fact view. Preserve the legacy write root while
   confirming local-date/time-zone semantics and all NPC, Item and WorldGen readers.
3. Introduce the proposed policy component and explicit override command. Make `SeasonalCalendarSystem`
   the only writer for `IsChristmasActive`/`IsHalloweenActive`; clear and recompute daily flags at the
   same day-start boundary as the legacy method. Keep `PendingWorldEventStateComponent` as a compatibility
   read boundary until its overlapping daily fields have one owner.
4. Add the secret-seed policy adapter and persistence projection after the version gates and reset
   semantics are verified. Forever policy must not be inferred from active seasonal facts.
5. Add world-info projection/adapter for today flags and a separate title effect adapter. Do not make
   packet or platform side effects part of the pure seasonal Query/reducer.
6. Run the focused verifier and affected-project serial checks only in a later implementation session;
   this planning session does not modify C# or execute builds/tests.

### 7.4 Single-write, compatibility and rollback strategy

The proposed single writer for active Christmas/Halloween facts is `SeasonalCalendarSystem`.
`SeasonalOverrideCommand` and the secret-seed adapter write policy inputs only; NPC, Item and WorldGen
queries remain read-only. During compatibility, one adapter may read legacy Main fields into the proposed
view, but `PendingWorldEventStateComponent` and the proposed policy component must not independently
mutate the same daily flags. The title request is consumed once by `TitleRefreshProjection` and cannot
change seasonal facts.

Rollback by disabling the proposed seasonal writer and restoring the compatibility read boundary if the
date window, today/forever precedence, wave-15 temporary season transition, world reset, save version
default, world-info bit or title-effect cadence diverges. No source rollback is performed here.

### 7.5 Network, snapshot and persistence strategy

- Treat world-file version 212/287 fields and world-info bit 11 as external adapters. The two active
  facts are derived and need not be persisted merely because their source flags are persisted.
- `MessageBuffer` packet 7 was a no-op branch in the inspected source, so no client receive or
  duplicate-update equivalence is claimed. Preserve wire-bit positions until the receive path is closed.
- A proposed world snapshot may contain committed active facts and policy inputs only after integration
  review decides whether those inputs are world authority or configuration. `EntityId`, `NetworkId`,
  `PersistentEntityId`, secret-seed ID and platform title API ID stay distinct.
- Save/load must preserve the Version4 version gates, default false behavior for older files, and world
  reset clearing. Do not add a new save section in this planning session.

### 7.6 Focused verifier additions

- Date windows and precedence: normal date, today override, forever override, both seasonal facts and
  the exact Halloween November 1 boundary.
- Day-start consume/reset, pumpkin/snow wave-15 temporary flags, forever suppression, broadcast intent,
  world reset and secret-seed initialization.
- World-file version 212/287 round-trip/defaults, world-info bit encoding/receive once evidence exists,
  and no double write between `PendingWorldEventStateComponent` and seasonal policy.
- Seasonal readers observe committed facts only; `TitleRefreshProjection` consumes at most once, clears
  the pending request and isolates the platform title effect. Direct title writer, time-zone behavior,
  packet receive and behavior equivalence remain not-run/evidence-gap.

## 8. Sixth execution checkpoint: `SharedWorldEventPresentationState`

### 8.1 Proposed target files and interfaces

`CreditsRollPresentationStateComponent.cs` and `ScreenObstructionPresentationStateComponent.cs`
are saved under `src2/WorldSession`. The remaining entries are non-Component targets only, and XNA
types remain behind client presentation adapters.

```text
status: proposed
src2/WorldSession/Presentation/CreditsRollPresentationPolicy.cs
src2/WorldSession/Presentation/CreditsRollPresentationStateComponent.cs
src2/WorldSession/Presentation/CreditsRollSystem.cs
src2/WorldSession/Presentation/CreditsRollNetworkProjection.cs
src2/WorldSession/Presentation/CreditsRollNetworkAdapter.cs
src2/WorldSession/Presentation/CreditsRollSkyAdapter.cs
src2/WorldSession/Presentation/MoonlordPiecePresentation.cs
src2/WorldSession/Presentation/MoonlordExplosionPresentation.cs
src2/WorldSession/Presentation/MoonlordDeathDramaPresentationAdapter.cs
src2/WorldSession/Presentation/MoonlordLightRequestBuffer.cs
src2/WorldSession/Presentation/MoonlordDramaSystem.cs
src2/WorldSession/Presentation/MoonlordPieceLifetimeQuery.cs
src2/WorldSession/Presentation/MoonlordExplosionLifetimeQuery.cs
src2/WorldSession/Presentation/MoonlordDeathDramaProjection.cs
src2/WorldSession/Presentation/ScreenObstructionPresentationStateComponent.cs
src2/WorldSession/Presentation/ScreenObstructionTargetQuery.cs
src2/WorldSession/Presentation/ScreenObstructionSystem.cs
src2/WorldSession/Presentation/ScreenObstructionProjection.cs
```

The proposed interfaces are `ICreditsRollSkyPort`, `ICreditsRollNetworkPort`,
`IMoonlordPresentationAssetPort`, `IMoonlordLightRequestPort` and `IScreenObstructionProjectionPort`.
They are boundary candidates only; external IDs, XNA assets, `SceneState`, `SceneMetrics`, platform
render handles and final namespaces remain `crossSubsystemOwner: integration-review`.

### 8.2 Source-member-to-role mapping

| Version4 member | proposed target role | proposed write owner | migration note |
| --- | --- | --- | --- |
| `MAX_TIME_FOR_CREDITS_ROLL_IN_FRAMES` | `CreditsRollPresentationPolicy.MaxFrames` | proposed policy construction | Preserve the `28800` upper clamp; keep it out of mutable component state. |
| `_creditsRollRemainingTime` | `CreditsRollPresentationStateComponent.RemainingFrames` | proposed `CreditsRollSystem` | Start from the NPC command boundary, decrement once per world tick, reset on world clear; packet 140 receive remains unassigned. |
| `MoonlordPiece._texture` | external texture handle in `MoonlordPiecePresentation` | proposed `MoonlordDeathDramaPresentationAdapter` | Keep `Texture2D` outside a core component and release through the client asset port. |
| `MoonlordPiece._position` | `MoonlordPiecePresentation.Position` | proposed `MoonlordDramaSystem` | Preserve construction and per-tick movement; world-size input is an integration dependency. |
| `MoonlordPiece._velocity` | `MoonlordPiecePresentation.Velocity` | proposed `MoonlordDramaSystem` | Preserve gravity, position integration and client-only lifecycle. |
| `MoonlordPiece._origin` | `MoonlordPiecePresentation.Origin` | proposed piece adapter | Preserve draw origin as presentation metadata; do not serialize `Vector2`. |
| `MoonlordPiece._rotation` | `MoonlordPiecePresentation.Rotation` | proposed `MoonlordDramaSystem` | Preserve angular update without creating authority state. |
| `MoonlordPiece._rotationVelocity` | `MoonlordPiecePresentation.RotationVelocity` | proposed `MoonlordDramaSystem` | Preserve `0.99` damping and object ownership. |
| `MoonlordPiece.Dead` | `MoonlordPieceLifetimeQuery.IsDead` | proposed `MoonlordDramaSystem` consumes result | Keep as a pure predicate over position and world bounds; no mutable `Dead` field. |
| `MoonlordExplosion._texture` | external texture handle in `MoonlordExplosionPresentation` | proposed drama presentation adapter | Keep XNA asset ownership and release outside ECS state. |
| `MoonlordExplosion._position` | `MoonlordExplosionPresentation.Position` | proposed `MoonlordDramaSystem` | Preserve construction position and boundary expiry. |
| `MoonlordExplosion._origin` | `MoonlordExplosionPresentation.Origin` | proposed explosion adapter | Recompute from the first frame; keep `Rectangle` at the client boundary. |
| `MoonlordExplosion._frame` | `MoonlordExplosionPresentation.Frame` | proposed `MoonlordDramaSystem` | Preserve seven-frame texture slicing and projection-only use. |
| `MoonlordExplosion._frameCounter` | `MoonlordExplosionPresentation.FrameCounter` | proposed `MoonlordDramaSystem` | Initialize to zero and increment once per client update. |
| `MoonlordExplosion._frameSpeed` | `MoonlordExplosionPresentation.FrameSpeed` | proposed explosion creation command/adapter | Validate positive input before division; creator is currently an evidence gap. |
| `MoonlordExplosion.Dead` | `MoonlordExplosionLifetimeQuery.IsDead` | proposed `MoonlordDramaSystem` consumes result | Keep the boundary or seven-frame predicate pure; remove only after predicate is true. |
| `MoonlordDeathDrama._pieces` | `MoonlordDeathDramaPresentationAdapter.PiecePool` | proposed drama adapter/system | Preserve temporary collection semantics; creation, draw and reset call sites must be found before implementation. |
| `MoonlordDeathDrama._explosions` | `MoonlordDeathDramaPresentationAdapter.ExplosionPool` | proposed drama adapter/system | Preserve temporary collection semantics; never put the pool in world persistence. |
| `MoonlordDeathDrama._lightSources` | `MoonlordLightRequestBuffer.Sources` | proposed `IMoonlordLightRequestPort` / drama system | Collect NPC requests, evaluate 2000-pixel proximity, then clear once per update. |
| `MoonlordDeathDrama.whitening` | client-only drama presentation value | proposed `MoonlordDramaSystem` | Preserve `MoveTowards` step `0.02`; final scene consumer and reset are evidence gaps. |
| `MoonlordDeathDrama.requestedLight` | `MoonlordLightRequestBuffer.MaxRequestedLight` | proposed light request port | Clamp to 1, aggregate by maximum, consume and reset every frame. |
| `ScreenObstruction.lastSpeed` | `ScreenObstructionPresentationStateComponent.LastTransitionSpeed` | proposed `ScreenObstructionSystem` | Preserve `0.01`/`0.3` selection and recovery reuse; no save/network mapping. |
| `ScreenObstruction.screenObstruction` | `ScreenObstructionPresentationStateComponent.CurrentAmount` | proposed `ScreenObstructionSystem` | Preserve target values, `MoveTowards` and local projection; caller/reset remain evidence gaps. |

### 8.3 Proposed implementation sequence

1. Freeze focused fixtures for credits start, Sky override, tick decrement, reset, join synchronization and
   packet 140 encoding. Confirm whether packet subtype 0 has a client receive path before introducing a
   receive adapter.
2. Add the proposed credits policy/state seam and make `CreditsRollSystem` the only writer. Keep the NPC
   trigger, `Main` tick and world-clear boundaries as compatibility inputs until their callers are migrated.
3. Confirm Moon Lord piece/explosion construction, list insertion, draw and update ownership. Only then add
   the client adapter and object pools, with explicit asset acquisition/release and a positive frame-speed
   validation.
4. Add `MoonlordLightRequestBuffer` and route NPC requests through an explicit port. Run drama update after
   requests are collected, clear the buffer after proximity evaluation, then project whitening to the client
   scene. Do not expose the buffer through a world snapshot.
5. Add the proposed ScreenObstruction state, target query and system after locating its direct caller and
   reset boundary. Keep `SceneMetrics`/player inputs at the adapter boundary and do not share state with
   Moon Lord drama.
6. Add network or persistence mappings only after integration review classifies the credits timer as a
   recoverable shared event; the Moon Lord and obstruction values remain client-only unless direct evidence
   proves otherwise.

### 8.4 Single-write, compatibility and rollback strategy

`CreditsRollSystem` is the proposed single writer for remaining credits frames. NPC trigger, world tick and
world-clear code submit explicit commands or lifecycle inputs; network projection observes committed state,
and a future packet-140 receive adapter may submit through the same writer only after the missing receive
semantics are confirmed.

`MoonlordDeathDramaPresentationAdapter` owns the piece/explosion pools and asset handles. `MoonlordDramaSystem`
owns per-update movement, lifetime removal, light-buffer consumption and whitening smoothing. NPC code may
submit light requests but may not mutate the pools or whitening. `ScreenObstructionSystem` exclusively writes
the local obstruction state; its query is pure over supplied metrics and its projection is read-only.

During compatibility, adapters may read the legacy static values, but legacy and proposed writers must not
run concurrently for the same value. Roll back by disabling the proposed writer/adapter and restoring the
legacy presentation boundary if packet 140 timing, credits clamp, object cleanup, light aggregation,
whitening, obstruction smoothing or resource release diverges. No source rollback is performed in this plan.

### 8.5 Network, snapshot and persistence strategy

- Preserve packet 140's `byte` subtype plus `int` payload shape for credits. The inspected subtype-0 receive
  branch does not commit a credits value, so no receive-equivalence or duplicate-delivery claim is made.
- The credits timer may be a presentation/network snapshot candidate, but final authority, revision and
  stale-message handling are `crossSubsystemOwner: integration-review`.
- Do not serialize `Texture2D`, `Rectangle`, `Vector2`, object pools, light-source buffers, whitening or
  screen-obstruction values as world persistence or shared network components without new direct evidence.
- If client reconnection requires Moon Lord or obstruction reconstruction, derive it from explicit client
  inputs or an approved event snapshot rather than persisting the legacy private collections.
- Keep EntityId, NetworkId, PersistentEntityId, asset IDs and platform/render handles distinct.

### 8.6 Focused verifier additions

- Credits: default `28800`, Sky duration override, per-tick clamp, NPC trigger, world clear, non-zero join
  synchronization, packet 140 width, subtype handling and no duplicate writer.
- Moon Lord pieces: gravity `0.3`, rotation and `0.99` damping, 480-pixel bounds and pure `Dead` removal.
- Moon Lord explosions: frame counter, seven-frame selection, positive frame speed, boundary expiry and
  removal exactly once.
- Light/whitening: input clamp to 1, maximum aggregation, 2000-pixel proximity, buffer clear, `0.02`
  smoothing and scene projection after committed client state.
- Screen obstruction: wall-progress clamp, head-covered precedence, `0.01`/`0.3` speed memory,
  `MoveTowards` result, reset behavior and read-only projection.
- Asset lifecycle, direct callers, packet subtype-0 receive, persistence and behavior equivalence remain
  not-run/evidence-gap until their source boundaries are closed.

## 9. Seventh execution checkpoint: `SharedInvasionAndBossTracking`

### 9.1 Proposed target files and interfaces

`BannerKillProgressStateComponent.cs`, `BannerClaimableCountStateComponent.cs` and
`BannerClaimNotificationStateComponent.cs` are saved under `src2/WorldSession`. The remaining
entries are proposed non-Component files, types, registration or adapters and have not been created.
The layout keeps persistent Banner progress, tracker-session state, immutable definitions and
localization/network boundaries separate.

```text
status: proposed
src2/WorldSession/Events/Banners/BannerCatalogPolicy.cs
src2/WorldSession/Events/Banners/BannerKillProgressStateComponent.cs
src2/WorldSession/Events/Banners/BannerClaimableCountStateComponent.cs
src2/WorldSession/Events/Banners/BannerClaimNotificationStateComponent.cs
src2/WorldSession/Events/Banners/BannerProgressSystem.cs
src2/WorldSession/Events/Banners/BannerPersistenceAdapter.cs
src2/WorldSession/Events/Banners/BannerNetworkProjection.cs
src2/WorldSession/Events/Banners/BannerNetworkAdapter.cs
src2/WorldSession/Events/BossTracking/BossTrackingDefinition.cs
src2/WorldSession/Events/BossTracking/BossEncounterOutcomeStateComponent.cs
src2/WorldSession/Events/BossTracking/BossTrackingNameQuery.cs
src2/WorldSession/Events/BossTracking/BossTrackingMessageProjection.cs
src2/WorldSession/Events/BossTracking/BossTrackingLifecycleAdapter.cs
src2/WorldSession/Events/InvasionTracking/InvasionDefinitionCatalog.cs
src2/WorldSession/Events/InvasionTracking/InvasionTrackingSessionScope.cs
src2/WorldSession/Events/InvasionTracking/InvasionTrackingNameProjection.cs
src2/WorldSession/Events/InvasionTracking/InvasionTrackingMessageProjection.cs
src2/WorldSession/Events/InvasionTracking/InvasionDamageEligibilityQuery.cs
src2/WorldSession/Events/InvasionTracking/InvasionTrackingLifetimeSystem.cs
```

Proposed seams are `IBannerPersistencePort`, `IBannerNetworkPort`, `ILocalizationTextPort`,
`INpcDamageTrackingPort` and `IInvasionGroupLookupPort`. `BannerId`, NPC type, InvasionGroup,
EntityId, NetworkId, PersistentEntityId, player identity and item ID remain distinct values;
their shared value-object and DTO ownership is `crossSubsystemOwner: integration-review`.

### 9.2 Source-member-to-role mapping

| Version4 member | proposed target role | proposed write owner | migration note |
| --- | --- | --- | --- |
| `BannerSystem.MaxBannerTypes` | `BannerCatalogPolicy.MaxBannerTypes` | proposed catalog construction | Preserve fixed capacity `293`; expose it as read-only policy, not mutable component state. |
| `BannerSystem.killCount` | `BannerKillProgressStateComponent.KillCounts` | proposed `BannerProgressSystem` | Preserve `int` counts indexed by BannerId; NPC death mapping is the only increment path. |
| `BannerSystem.claimableBanners` | `BannerClaimableCountStateComponent.Counts` | proposed `BannerProgressSystem` | Preserve `ushort` counts and the independent world-file v289+ payload. |
| `BannerSystem.AnyNewClaimableBanners` | `BannerClaimNotificationStateComponent.HasNewClaimableBanners` | proposed `BannerProgressSystem` and an explicit notification-consume command | Keep it transient; do not add it to world save/full-state until its consumer and receive semantics are confirmed. |
| `BossDamageTracker._type` | `BossTrackingDefinition.BossType` | proposed tracker-session construction | Immutable root type; composite definitions use their first configured type as the Version4 display/root identity. |
| `BossDamageTracker._overrides` | `BossTrackingDefinition.Override` | proposed tracker-session construction | Keep custom NPC type membership and optional name override immutable and outside player/world progress. |
| `BossDamageTracker._killed` | `BossEncounterOutcomeStateComponent.WasKilled` | proposed boss-kill command handled by `BossTrackingLifecycleAdapter` | Preserve one-way false-to-true outcome and message selection; no persistence claim. |
| `BossDamageTracker.Name` | `BossTrackingNameQuery` and `BossNameProjection` | no state writer | Preserve override-name then `Lang.GetNPCName(_type)` fallback; localization remains an adapter boundary. |
| `BossDamageTracker.KillTimeMessage` | `BossTrackingMessageProjection.KillTimeMessage` | no state writer | Preserve `KillTime` versus `KillTimeEscaped` based on the committed outcome. |
| `InvasionDamageTracker.VanillaInvasionNameKeys` | `InvasionDefinitionCatalog.NameKeys` | proposed catalog construction | Preserve groups `1/2/3/4/-2/-1` and their localization keys; unknown-group behavior is gated. |
| `InvasionDamageTracker._invasionGroup` | `InvasionTrackingSessionScope.InvasionGroup` | proposed tracker-session construction | Keep group identity separate from NPC type and network/persistence IDs. |
| `InvasionDamageTracker._name` | `InvasionTrackingNameProjection.ResolvedName` | proposed tracker-session construction | Preserve explicit name override or catalog resolution; do not serialize `LocalizedText`. |
| `InvasionDamageTracker.Name` | `InvasionTrackingNameProjection` | no state writer | Read-only UI/log projection of the session name. |
| `InvasionDamageTracker.KillTimeMessage` | `InvasionTrackingMessageProjection.KillTimeMessage` | no state writer | Preserve Version4 `null`; do not reuse Boss kill/escape text. |

### 9.3 Proposed implementation sequence

1. Freeze fixtures for Banner IDs, the 293-entry capacity, `KillsToBanner` threshold behavior,
   world clear, save versions 288/289 and the packet subtype 0/1/2 write widths. Record the
   current Version4 `Deserialize` no-op before designing a receive adapter.
2. Add the proposed Banner catalog and the two persistent progress components. Route the NPC death
   command through `BannerProgressSystem`; commit kill count first, then claimable count and the
   notification latch in one transaction. Keep `AddNPCKillBy` as a compatibility input until all
   callers are migrated.
3. Add `BannerPersistenceAdapter` with the Version4 length-prefixed payload and v289 claimable
   boundary. Add outbound network projection only after the committed state is updated. Do not
   infer client receive or claim-consumption behavior from the writer methods.
4. Locate the claim/notification consumers and the full-state receive owner. Only then add
   `BannerNetworkAdapter`; stale revisions, duplicate packets and client-side claim consumption
   must submit through the same proposed writer or remain explicitly deferred.
5. Add the Boss definition/outcome seam and connect damage, kill, update and reset lifecycle
   inputs through `BossTrackingLifecycleAdapter`. Preserve active versus recent-finished tracker
   behavior and keep credit entries outside this checkpoint until integration review assigns them.
6. Add Invasion catalog/session projections. Keep the Version4 `IncludeDamageFor` and
   `CheckActive` stubs as the compatibility baseline. Add group matching and active-stop behavior
   only if integration review explicitly accepts the complete-reference supplement.

### 9.4 Single-write, compatibility and rollback strategy

`BannerProgressSystem` is the sole proposed writer for kill counts, claimable counts and the
notification latch. World clear and load are lifecycle inputs to that writer; save and outbound
network are read-only projections. A notification-consume command may clear the latch only after
its consumer is identified, and must not decrement claimable inventory implicitly.

`BossTrackingLifecycleAdapter` is the proposed owner of tracker-session creation, damage routing,
kill outcome commit, active-stop and recent retention. Definition and localized name projections
are read-only. `InvasionTrackingLifetimeSystem` must not add group matching or stop behavior while
the Version4 stub compatibility decision is open; its proposed ports may observe only committed
invasion facts.

During compatibility, adapters can read legacy values, but the legacy root and proposed writer
must never mutate the same value in the same tick. Roll back by disabling the proposed Banner
writer, persistence/network adapter or tracker adapter and restoring the legacy root if any count
duplicates, v289 payload is misread, packet ordering changes, Boss recent retention changes, or
the Invasion stub is silently replaced by complete-reference behavior. No source rollback is
performed by this planning session.

### 9.5 Network, snapshot and persistence strategy

- Preserve Banner full-state subtype 0 as the existing save-shaped payload, kill update subtype 1
  as `short BannerId + int`, and claim update subtype 2 as `short BannerId + ushort`. These are
  outbound write facts only; the current Version4 deserializer does not prove a receive commit.
- Preserve world-file behavior: always read the kill-count length and values; read claimable length
  and values only for version 289 or later. Clamp copied entries to the proposed catalog capacity
  without changing the serialized lengths consumed from the stream.
- Do not put `AnyNewClaimableBanners` in persistence or shared snapshots until its transient
  consumer, clear point and reconnection behavior are evidenced. Banner progress is world-scoped,
  not an Entity component keyed by NPC entity.
- Boss and Invasion members have no direct Version4 save/network writer evidence in this review.
  Keep definitions and tracker outcomes local to the tracking session; only expose approved
  projections or event messages. Do not serialize `LocalizedText`, player display names or raw
  external IDs.
- If integration review approves a shared event snapshot, include an explicit revision and typed
  `InvasionGroup`/`BannerId`; never infer either from NetworkId or PersistentEntityId.

### 9.6 Focused verifier additions

- Banner: v288 ignores absent claimable data, v289 round-trips it, array lengths remain bounded,
  kill count increments once, threshold creates one claimable count, notification is latched once,
  world clear resets all three mutable values, and join full-state is read-only observation.
- Network: packet subtype and `short/int/ushort` widths, invalid BannerId handling, duplicate and
  stale packet behavior, and the explicitly missing current receive commit.
- Boss: ordinary and composite registration, override-name fallback, active-stop transition,
  recent-finished retention, one-way `WasKilled`, message selection and world reset.
- Invasion: six default catalog mappings, explicit names, unknown-group policy and null
  `KillTimeMessage`; fixtures must run separately for Version4 stub behavior and complete-reference
  supplement behavior, with no automatic substitution.
- Credit and player identity boundaries: world credit, player display name and NPC entity identity
  must not become a persistent Banner/Boss/Invasion ID. All current results remain not-run.

## 10. Eighth execution checkpoint: `SharedLightningGenerationState`

### 10.1 Proposed target files and interfaces

These are proposed target locations only. A Bolt is a per-generation payload, not a persistent
entity component, and XNA/Tile types remain behind ports.

```text
status: proposed
src2/WorldSession/Environment/Lightning/LightningGenerationPolicy.cs
src2/WorldSession/Environment/Lightning/LightningBoltPresentationPayload.cs
src2/WorldSession/Environment/Lightning/LightningBoltSegmentPayload.cs
src2/WorldSession/Environment/Lightning/LightningBoltCollisionResult.cs
src2/WorldSession/Environment/Lightning/LightningGenerationPort.cs
src2/WorldSession/Environment/Lightning/LightningBoltGenerationSystem.cs
src2/WorldSession/Environment/Lightning/LightningProjectionAdapter.cs
```

Proposed seams are `ILightningRandomPort`, `ILightningTileCollisionPort`,
`ILightningProjectionPort` and `ILightningCoordinatePort`. No proposed path is an existing file.

### 10.2 Source-member-to-role mapping

| Version4 member | proposed target role | proposed write owner | migration note |
| --- | --- | --- | --- |
| `Bolt.positions` | `LightningBoltPresentationPayload.Positions` | proposed generation system | Keep as one-call geometry payload; do not serialize or place in a world Component. |
| `Bolt.rotations` | `LightningBoltPresentationPayload.Rotations` | proposed rotation calculation stage | Preserve optional calculation flags; current calculation method is a stub. |
| `Bolt.progressRange` | `LightningBoltSegmentPayload.ProgressRange` | proposed recursive generator | Preserve root `0..1` and configured fork range. |
| `Bolt.forkDepth` | `LightningBoltSegmentPayload.ForkDepth` | proposed recursive generator | Bound by policy `MaxForkDepth`; reset for the root bolt. |
| `Bolt.collidedWithTile` | `LightningBoltCollisionResult.CollidedWithTile` | proposed collision adapter/system | Treat as generated output from a read-only Tile query; direct writer is not closed. |
| `StormLightning.Generator` | `LightningGenerationPolicy.Default` | proposed content/configuration initialization | Preserve the Version4 default object values and avoid mutable global access from projections. |
| `StormLightning.SourceRotationLimit` | `LightningGenerationPolicy.SourceRotationLimit` | proposed policy construction | Preserve `PI / 9`. |
| `StormLightning.Length` | `LightningGenerationPolicy.SourceLength` | proposed policy construction | Preserve `1000f`; coordinate units remain an integration boundary. |
| `LightningGenerator.SolidTileCollision` | `LightningGenerationPolicy.SolidTileCollision` | proposed policy construction | Preserve collision enablement as policy, not as a Tile state field. |
| `LightningGenerator.RotationStrength` | `LightningGenerationPolicy.RotationStrength` | proposed policy construction | Root path rotation parameter. |
| `LightningGenerator.StepSize` | `LightningGenerationPolicy.StepSize` | proposed policy construction | Validate positive value before generation. |
| `LightningGenerator.Layers` | `LightningGenerationPolicy.Layers` | proposed policy construction | Preserve layer count and test zero/negative input behavior. |
| `LightningGenerator.LayerStrengthFactor` | `LightningGenerationPolicy.LayerStrengthFactor` | proposed policy construction | Preserve layer scaling. |
| `LightningGenerator.PerpendicularDeviationFactor` | `LightningGenerationPolicy.PerpendicularDeviationFactor` | proposed policy construction | Consume through explicit random/geometry port. |
| `LightningGenerator.ReduceRandomnessAfter` | `LightningGenerationPolicy.ReduceRandomnessAfter` | proposed policy construction | Preserve progress threshold. |
| `LightningGenerator.ForkGenerationThresholdAngleFraction` | `LightningGenerationPolicy.ForkAngleThresholdFraction` | proposed policy construction | Preserve fork eligibility threshold. |
| `LightningGenerator.ForkReflectAngleMultiplier` | `LightningGenerationPolicy.ForkReflectAngleMultiplier` | proposed policy construction | Preserve reflected angle scale. |
| `LightningGenerator.ForkRotationStrengthMultiplier` | `LightningGenerationPolicy.ForkRotationStrengthMultiplier` | proposed policy construction | Preserve fork rotation scale. |
| `LightningGenerator.ForkStepSizeMultiplier` | `LightningGenerationPolicy.ForkStepSizeMultiplier` | proposed policy construction | Preserve fork step scale. |
| `LightningGenerator.ForkLengthMultiplier` | `LightningGenerationPolicy.ForkLengthMultiplier` | proposed policy construction | Preserve fork length scale. |
| `LightningGenerator.MaxForksPerBolt` | `LightningGenerationPolicy.MaxForksPerBolt` | proposed policy construction | Preserve maximum branch count. |
| `LightningGenerator.MaxForkDepth` | `LightningGenerationPolicy.MaxForkDepth` | proposed policy construction | Preserve recursion depth limit and verify against payload depth. |
| `LightningGenerator.ForkProgressRange` | `LightningGenerationPolicy.ForkProgressRange` | proposed policy construction | Preserve the `0.3..0.8` default interval. |

### 10.3 Proposed implementation sequence

1. Freeze deterministic fixtures for the default policy, source rotation/length, root and fork
   payloads, calculation flags and invalid parameter handling. Record the current Version4 stubs.
2. Introduce the proposed policy as immutable input and route seed generation through an explicit
   random port. Do not expose the legacy static generator as a mutable ECS component.
3. Implement the generation seam as a pure calculation over policy, seed, coordinates and a Tile
   collision input. Keep recursive Bolt lists and XNA arrays inside the operation scope.
4. Connect Projectile callers through the projection adapter only after output geometry, rotation
   and collision semantics are verified. Dispose the payload at the end of the presentation/use
   scope; no save/network migration is planned.
5. Resolve the Version4 algorithm-stub decision before any behavior-equivalence claim. A complete
   reference implementation may be used only as separately labeled supplementary evidence.

### 10.4 Single-write, compatibility and rollback strategy

The proposed generation system is the only writer for Bolt payloads. The policy is immutable after
content/configuration initialization. Tile collision is queried through a port and never mutated
by the lightning Query or projection. During compatibility, the legacy `StormLightning.Generator`
may be read as an input, but legacy and proposed generators must not both emit the same lightning
effect in one update.

Roll back by disabling the proposed projection/generator adapter and restoring the legacy caller if
seed determinism, source angle, length, fork limits, Tile collision or rotation output differs.
Discard only operation-scoped payloads created by the proposed plan; this planning session performs
no source rollback or cleanup.

### 10.5 Network, snapshot and persistence strategy

No Version4 save or network writer for these 23 fields was found. Keep seed, coordinate, Tile query
result, Projectile identity, NetworkId, PersistentEntityId and render asset handles distinct. If a
future network effect needs synchronization, send an approved event/seed input or regenerate on the
receiver; do not serialize `Vector2[]`, rotation arrays, recursive lists or XNA values as world state.

### 10.6 Focused verifier additions

- Default policy values, `PI/9` source rotation limit, `1000f` source length and deterministic seed
  reproducibility.
- Root/fork progress ranges, depth/count limits, step/layer values, all fork multipliers and
  `collidedWithTile` behavior for solid/non-solid Tile inputs.
- `calcPositions`/`calcRotations` combinations, empty output lists, invalid policy values and
  payload disposal after projection.
- Separate tests for current Version4 stub behavior and any complete-reference supplement; no
  algorithm-equivalence claim is allowed before that decision. All results are currently not-run.

## 11. Ninth execution checkpoint: `SharedWaterfallState`

### 11.1 Proposed target files and interfaces

These are proposed target locations only. No target file, component, registration, configuration key,
resource adapter or render port is claimed to exist. The legacy `WaterfallData` values are temporary
slot payloads; they are not world persistence or ECS entity state.

```text
status: proposed
src2/WorldSession/Environment/Waterfalls/WaterfallGenerationPolicy.cs
src2/WorldSession/Environment/Waterfalls/WaterfallCapacityPolicy.cs
src2/WorldSession/Environment/Waterfalls/WaterfallSlotPayload.cs
src2/WorldSession/Environment/Waterfalls/WaterfallSlotBuffer.cs
src2/WorldSession/Environment/Waterfalls/WaterfallTextureCatalog.cs
src2/WorldSession/Environment/Waterfalls/WaterfallGenerationQuery.cs
src2/WorldSession/Environment/Waterfalls/WaterfallGenerationSystem.cs
src2/WorldSession/Environment/Waterfalls/WaterfallConfigurationAdapter.cs
src2/WorldSession/Environment/Waterfalls/WaterfallPresentationAdapter.cs
```

Proposed ports are `IWaterfallTileInputPort`, `IWaterfallTextureResourcePort`,
`IWaterfallConfigurationPort` and `IWaterfallPresentationPort`. Their namespaces, coordinate value
objects, liquid/wetness snapshot and final client asset owner remain
`crossSubsystemOwner: integration-review`.

### 11.2 Source-member-to-role mapping

| Version4 member | proposed target role | proposed write owner | migration note |
| --- | --- | --- | --- |
| `WaterfallData.x` | `WaterfallSlotPayload.TileX` | proposed `WaterfallGenerationSystem` | Keep as a transient Tile coordinate; do not persist it as an entity or world ID. |
| `WaterfallData.y` | `WaterfallSlotPayload.TileY` | proposed `WaterfallGenerationSystem` | Keep paired with `TileX` for one generation/presentation pass. |
| `WaterfallData.type` | `WaterfallSlotPayload.WaterfallTypeId` | proposed `WaterfallGenerationSystem` | Treat as a typed content index until the legacy catalog and unknown-type fallback are evidenced. |
| `WaterfallData.stopAtStep` | `WaterfallSlotPayload.StopAtStep` | proposed `WaterfallGenerationSystem` | Preserve as a traversal/draw result; close its relation to `maxLength` with caller evidence. |
| `minWet` | `WaterfallGenerationPolicy.MinWetness` | proposed policy construction | Preserve `160` as a policy default; do not infer weather ownership from the constant alone. |
| `maxWaterfallCountDefault` | `WaterfallCapacityPolicy.DefaultCapacity` | proposed policy construction | Preserve `1000` as the default resource capacity. |
| `maxLength` | `WaterfallGenerationPolicy.MaxLength` | proposed policy construction | Preserve `100` pending confirmation of its unit and stop-step semantics. |
| `maxTypes` | `WaterfallGenerationPolicy.MaxWaterfallTypes` | proposed policy construction | Preserve `28` as a content/texture slot limit, not a network ID range. |
| `maxWaterfallCount` | `WaterfallCapacityPolicy.ActiveCapacity` | proposed `WaterfallConfigurationAdapter` | Validate non-negative configured capacity and clamp it to the actual buffer capacity. |
| `waterfalls` | `WaterfallSlotBuffer.Slots` | proposed `WaterfallGenerationSystem` plus explicit reset/clear lifecycle | Keep the `WaterfallData[1000]`-shaped storage transient and reusable; never register it as a saved component. |
| `waterfallTexture` | `WaterfallTextureCatalog` external handles | proposed `IWaterfallTextureResourcePort` adapter | Keep `Asset<Texture2D>[28]` and release/invalidation outside core state. |

### 11.3 Proposed implementation sequence

1. Close the missing Version4 writer, clear/update, draw and texture acquisition/release call graph.
   If those call sites remain absent, record the legacy behavior as an evidence gap and do not invent
   a generation algorithm from the field names.
2. Freeze pure fixtures for the four slot fields, default constants, configured capacity, unknown type,
   over-capacity input, empty buffer and `StopAtStep`/`MaxLength` boundaries.
3. Add the proposed policy, capacity and payload seams. Route Tile/liquid/wetness input through an
   explicit port and keep `WaterfallGenerationQuery` side-effect free.
4. Add `WaterfallSlotBuffer` and make `WaterfallGenerationSystem` the only proposed slot writer.
   Reset or reuse the buffer at an explicit scene/world lifecycle boundary before the next generation
   pass; do not let the presentation adapter mutate it.
5. Add `WaterfallTextureCatalog` behind the resource port. Define load, replacement, invalidation and
   release behavior only after the texture caller and content type catalog are found.
6. Add the read-only presentation adapter and any network/persistence projection only after integration
   review decides whether approved inputs or a deterministic seed are synchronized. Do not send slot
   arrays, XNA assets or texture handles as world state.

### 11.4 Single-write, compatibility and rollback strategy

`WaterfallGenerationSystem` is the proposed sole writer for slot payloads and the slot buffer.
`WaterfallConfigurationAdapter` is the only proposed writer for effective capacity and policy inputs;
`Preferences.OnLoad` is an external input, not an authority to mutate world event state.
`WaterfallPresentationAdapter` is read-only, and `IWaterfallTextureResourcePort` owns external asset
acquisition and release. A world/scene reset command must clear the buffer through the same explicit
lifecycle owner.

During compatibility, adapters may read the legacy manager, but legacy and proposed slot writers must
not run in the same scene pass. Roll back by disabling the proposed generation/presentation adapter and
restoring the legacy boundary if slot count, coordinate/type mapping, stop-step behavior, configured
capacity, texture lifetime or reset timing differs. No source rollback is performed by this plan.

### 11.5 Network, snapshot and persistence strategy

No Version4 save or network writer for these 11 members was found. Keep `WaterfallData[]`,
`Asset<Texture2D>[]`, XNA handles, Tile coordinates, `WaterfallTypeId`, `EntityId`, `NetworkId` and
`PersistentEntityId` distinct. Do not serialize the temporary slot buffer or resource catalog as world
state. If a future client needs reconstruction, synchronize an approved Tile/liquid input snapshot,
deterministic seed or event revision and regenerate slots on the receiving side; capacity configuration
ownership remains `crossSubsystemOwner: integration-review`.

### 11.6 Focused verifier additions

- Slot payload round-trip for `x`, `y`, `type` and `stopAtStep`, including negative/overflow boundary
  policy and unknown type handling.
- Policy defaults `160`, `1000`, `100` and `28`; configured capacity clamping, zero capacity and buffer
  length mismatch behavior.
- Query purity, single-writer enforcement, over-capacity truncation/rejection, empty-buffer behavior and
  clear/reuse at scene/world reset.
- Texture load, replacement, invalidation and release through the resource port without leaking XNA
  handles or exposing them through snapshots.
- Preferences callback changes only configuration inputs and does not implicitly mutate world state.
- Direct Version4 writer/draw call graph, network/save DTOs and behavior equivalence remain not-run and
  evidence-gap until the missing boundaries are found.

## 12. Tenth execution checkpoint: `WorldEnvironmentScanHelpers`

### 12.1 Proposed target files and interfaces

These are proposed target locations only. No scan component, query, system, port, registration or
presentation adapter is claimed to exist. The Spelunker collections are transient work state, the
UnbreakableWall values are query policy/input, and the Void Lens values are one-shot presentation
payloads rather than saved world state.

```text
status: proposed
src2/WorldSession/Environment/Scans/SpelunkerScanPolicy.cs
src2/WorldSession/Environment/Scans/SpelunkerScanWorkBuffer.cs
src2/WorldSession/Environment/Scans/SpelunkerScanFrameBoundarySystem.cs
src2/WorldSession/Environment/Scans/SpelunkerScanSystem.cs
src2/WorldSession/Environment/Scans/SpelunkerSpotQuery.cs
src2/WorldSession/Environment/Scans/UnbreakableWallScanPolicy.cs
src2/WorldSession/Environment/Scans/UnbreakableWallScanQuery.cs
src2/WorldSession/Environment/Scans/UnbreakableWallTileReadPort.cs
src2/WorldSession/Environment/Scans/VoidLensPresentationPayload.cs
src2/WorldSession/Environment/Scans/VoidLensPresentationSystem.cs
src2/WorldSession/Environment/Scans/VoidLensProjectionAdapter.cs
```

Proposed ports are `IUnbreakableWallTileReadPort`, `IVoidLensLightingPort`,
`IVoidLensRandomPort`, `IVoidLensDustPort` and `IVoidLensPresentationPort`. Their namespaces,
Tile/World coordinate value objects, Projectile identity boundary and final client presentation owner
remain `crossSubsystemOwner: integration-review`.

### 12.2 Source-member-to-role mapping

| Version4 member | proposed target role | proposed write owner | migration note |
| --- | --- | --- | --- |
| `SpelunkerProjectileHelper._positionsChecked` | `SpelunkerScanWorkBuffer.CheckedPositions` | proposed `SpelunkerScanSystem` for additions; proposed `SpelunkerScanFrameBoundarySystem` for the explicit reset command | Preserve position de-duplication for one scan window; do not persist or network the collection. |
| `SpelunkerProjectileHelper._tilesChecked` | `SpelunkerScanWorkBuffer.CheckedTiles` | proposed `SpelunkerScanSystem` for additions; proposed `SpelunkerScanFrameBoundarySystem` for the explicit reset command | Keep Tile de-duplication separate from position de-duplication; `CheckSpot` is currently empty, so no result writer is invented. |
| `SpelunkerProjectileHelper._clampBox` | `SpelunkerScanWorkBuffer.ClampBounds` | proposed `SpelunkerScanFrameBoundarySystem` | Rebuild from `Main.maxTilesX`/`Main.maxTilesY` at the observed frame boundary; keep Rectangle and world dimensions outside persisted state. |
| `SpelunkerProjectileHelper._frameCounter` | `SpelunkerScanWorkBuffer.FrameCounter` plus `SpelunkerScanPolicy.ResetInterval` | proposed `SpelunkerScanFrameBoundarySystem` | Preserve the observed reset interval of `10`; do not reinterpret it as world time or a save tick. |
| `UnbreakableWallScan.ScanDistance` | `UnbreakableWallScanPolicy.MaxDistance` | proposed policy construction | Preserve `250` as a query step limit, not as a player cooldown, wall entity distance or network field. |
| `UnbreakableWallScan.Directions` | `UnbreakableWallScanPolicy.Directions` | proposed policy construction | Preserve the eight directions and source order as immutable query input. |
| `VoidLensHelper._position` | `VoidLensPresentationPayload.Position` | proposed `VoidLensPresentationSystem`/presentation adapter at payload creation | Preserve the constructor-specific `Y -= 2f` adjustment and keep coordinates separate from Projectile, Entity, Network and Persistent IDs. |
| `VoidLensHelper._opacity` | `VoidLensPresentationPayload.Opacity` | proposed `VoidLensPresentationSystem`/presentation adapter at payload creation | Carry the input without making it authoritative; the final draw consumer remains an evidence gap. |
| `VoidLensHelper._frameNumber` | `VoidLensPresentationPayload.FrameNumber` | proposed `VoidLensPresentationSystem`/presentation adapter at payload creation | Preserve Projectile-frame input and the world-position frame calculation; the resource frame catalog remains unresolved. |

### 12.3 Proposed implementation sequence

1. Freeze focused fixtures from the observed Version4 contracts: position/Tile de-duplication,
   clamp reconstruction, frame counter reset at `10`, the empty `CheckSpot` behavior, the `250`
   wall-scan step limit, eight-direction ordering, wall type `350` with `wallColor() >= 16`, Void
   Lens constructor offsets, opacity and frame selection. Resolve only the missing Tile reader and
   presentation call sites needed to make each seam explicit.
2. Add the proposed `SpelunkerScanPolicy` and `SpelunkerScanWorkBuffer`. Make
   `SpelunkerScanFrameBoundarySystem` the only writer of `ClampBounds` and `FrameCounter`; it
   submits the explicit reset operation for both collections at the observed interval. Make
   `SpelunkerScanSystem` the only writer that adds checked positions and Tiles. Neither
   `SpelunkerSpotQuery` nor a projection may mutate the buffer.
3. Add `SpelunkerSpotQuery` only as a read-only query over an explicit Tile/world port. Keep the
   current empty `CheckSpot` as a compatibility fact until a direct result writer is evidenced;
   do not add mineral highlighting, Tile writes or output entities based on the helper name.
4. Add `UnbreakableWallScanPolicy` and `UnbreakableWallScanQuery` behind
   `IUnbreakableWallTileReadPort`. Keep `LineScan` and `InsideUnbreakableWalls` pure: invalid or
   empty Tile reads return the observed false result, wall `350` checks `wallColor() >= 16`, and
   the bit-mask rotation follows the source direction order. Player cooldown, last position,
   `insideUnbreakableWalls` commit and change notification remain the P08/player owner boundary.
5. Add `VoidLensPresentationPayload` and route it through `VoidLensPresentationSystem` to the
   explicit lighting, random, Dust and presentation ports. Keep random selection, lighting,
   Dust velocity/scale and `customData` in adapters; dispose the payload after presentation and do
   not make the one-shot values an ECS component or snapshot.
6. During compatibility, compare read-only query results and payload inputs before switching the
   legacy callers. Switch one writer boundary at a time, and preserve the observed order from
   Projectile/world-position input through lighting and Dust effects. No migration step is
   allowed to introduce a second world-state writer.

### 12.4 Single-write, compatibility and rollback strategy

The proposed work-buffer boundary has explicit field-level ownership: `SpelunkerScanFrameBoundarySystem`
owns clamp/frame updates and the reset command, while `SpelunkerScanSystem` owns additions to both
de-duplication sets. `SpelunkerSpotQuery` is read-only. `UnbreakableWallScanQuery` has no state
writer; the player-owned caller commits only its own cooldown/position/result fields. Void Lens
payload creation is a presentation boundary and the effect ports consume it without writing world
authority. No NPC, Item, Banner, invasion, lightning, waterfall or projection adapter may write
these scan values.

Compatibility adapters may read the legacy helpers and shadow-compare pure query inputs/results, but
legacy and proposed writers must not run for the same scan window. Keep the empty Version4
`CheckSpot` behavior until a real result owner is found. Keep `UnbreakableWallScan.NetModule` receive
and broadcast unresolved; do not silently add network authority. Roll back by disabling the proposed
scan/presentation route and restoring the legacy caller boundary if cache reset timing, de-duplication,
wall color/bit-mask behavior, Player result timing, Void Lens frame/opacity input, effect-port order
or payload cleanup differs. No source rollback is performed by this plan.

### 12.5 Network, snapshot and persistence strategy

Do not serialize or network `CheckedPositions`, `CheckedTiles`, `ClampBounds`, `FrameCounter`,
`ScanDistance`, `Directions`, XNA `Rectangle`/`Vector2`, random streams, lighting requests, Dust
instances or `customData`. These are transient work data, immutable query policy or client
presentation inputs. `ProjectileId`, `EntityId`, `NetworkId` and `PersistentEntityId` remain distinct
from Tile/world coordinates. If wall-scan results later cross the player/network boundary, the
P08/integration owner must define a versioned result or input snapshot with stale/duplicate handling;
P02 does not make `insideUnbreakableWalls` a world component. Void Lens has no proposed save or
network DTO and no resource handle crosses the projection boundary.

### 12.6 Focused verifier additions

- Work-buffer tests for position/Tile de-duplication, clamp reconstruction from world dimensions,
  the `10`-frame clear-and-zero boundary, reset idempotence and empty `CheckSpot` compatibility.
- Pure wall-query tests for eight-direction ordering, `250` steps, out-of-range/empty Tile results,
  wall `350`, `wallColor() >= 16`, bit-mask rotation, and proof that the query does not write Player
  or Tile state.
- Void Lens tests for both constructors, `Y -= 2f`, frame calculation, opacity propagation and
  payload disposal; deterministic random, lighting and Dust port tests must assert call order and
  `customData` lifetime.
- Compatibility tests for Player cooldown/last-position gating, optional network result revision,
  duplicate/stale handling and the absence of a second writer. Direct Version4 consumer closure,
  behavior equivalence and actual verifier execution remain `not-run` and are still evidence gaps.

## 13. Eleventh execution checkpoint: `SharedAmbientSkyCatalogState`

### 13.1 Proposed target files and interfaces

`TreeTopVariationStateComponent.cs` and `BackgroundChangeFlashStateComponent.cs` are saved under
`src2/WorldSession`. The remaining catalog, persistence adapter, network projection, presentation
system and registration entries are proposed only and are not claimed to exist. The tree-top variation array
has a world-save boundary; the area constants are immutable definitions; the background-flash arrays
are transient presentation state.

```text
status: proposed
src2/WorldSession/Environment/AmbientSky/TreeTopAreaCatalog.cs
src2/WorldSession/Environment/AmbientSky/TreeTopVariationStateComponent.cs
src2/WorldSession/Environment/AmbientSky/TreeTopVariationSystem.cs
src2/WorldSession/Environment/AmbientSky/TreeTopVariationQuery.cs
src2/WorldSession/Environment/AmbientSky/TreeTopPersistenceAdapter.cs
src2/WorldSession/Environment/AmbientSky/TreeTopNetworkProjection.cs
src2/WorldSession/Environment/AmbientSky/BackgroundChangeFlashStateComponent.cs
src2/WorldSession/Environment/AmbientSky/BackgroundChangeFlashSystem.cs
src2/WorldSession/Environment/AmbientSky/BackgroundPresentationPort.cs
```

Proposed ports are `ITreeTopTileReadPort`, `ITreeTopRandomPort`, `IWorldInfoTreeTopPort`,
`ITreeTopNetworkPort` and `IBackgroundPresentationPort`. Their namespaces, area-index value type,
world-coordinate type, packet DTO and final background/sky owner remain
`crossSubsystemOwner: integration-review`.

### 13.2 Source-member-to-role mapping

| Version4 member | proposed target role | proposed write owner | migration note |
| --- | --- | --- | --- |
| `BackgroundChangeFlashInfo._variations` | `BackgroundChangeFlashStateComponent.VariationsByArea` | proposed `BackgroundChangeFlashSystem` for cache refresh | Preserve the 13-area mapping supplied by `UpdateCache`; `UpdateVariation` is empty in Version4, so no flash event writer is invented. |
| `BackgroundChangeFlashInfo._flashPower` | `BackgroundChangeFlashStateComponent.FlashPowerByArea` | proposed `BackgroundChangeFlashSystem` for decay; trigger owner unresolved | Preserve `Clamp(value - 0.05f, 0f, 1f)` per update; keep trigger and presentation consumer open. |
| `TreeTopsInfo.AreaId.Forest1` | `TreeTopAreaCatalog.Forest1 = 0` | proposed immutable catalog construction | Stable area index; not an ECS state field, network ID or persistence ID. |
| `TreeTopsInfo.AreaId.Forest2` | `TreeTopAreaCatalog.Forest2 = 1` | proposed immutable catalog construction | Preserve array and save-slot ordering. |
| `TreeTopsInfo.AreaId.Forest3` | `TreeTopAreaCatalog.Forest3 = 2` | proposed immutable catalog construction | Do not merge with `Main.treeStyle` or its world-X boundaries. |
| `TreeTopsInfo.AreaId.Forest4` | `TreeTopAreaCatalog.Forest4 = 3` | proposed immutable catalog construction | Stable area index; world boundary input remains external. |
| `TreeTopsInfo.AreaId.Corruption` | `TreeTopAreaCatalog.Corruption = 4` | proposed immutable catalog construction | Keep background type and tree-top variation separate. |
| `TreeTopsInfo.AreaId.Jungle` | `TreeTopAreaCatalog.Jungle = 5` | proposed immutable catalog construction | Tile/biome input is external; the catalog does not own Jungle facts. |
| `TreeTopsInfo.AreaId.Snow` | `TreeTopAreaCatalog.Snow = 6` | proposed immutable catalog construction | Area index is not a weather state or network field. |
| `TreeTopsInfo.AreaId.Hallow` | `TreeTopAreaCatalog.Hallow = 7` | proposed immutable catalog construction | Preserve the index used by the `GetHollowTreeFoliageStyle` read path. |
| `TreeTopsInfo.AreaId.Crimson` | `TreeTopAreaCatalog.Crimson = 8` | proposed immutable catalog construction | Stable area index; does not own Crimson world state. |
| `TreeTopsInfo.AreaId.Desert` | `TreeTopAreaCatalog.Desert = 9` | proposed immutable catalog construction | Background randomization remains an external command/input boundary. |
| `TreeTopsInfo.AreaId.Ocean` | `TreeTopAreaCatalog.Ocean = 10` | proposed immutable catalog construction | Do not treat the area index as an ocean entity or Tile coordinate. |
| `TreeTopsInfo.AreaId.GlowingMushroom` | `TreeTopAreaCatalog.GlowingMushroom = 11` | proposed immutable catalog construction | Defines only the index; resource and lighting ownership remain external. |
| `TreeTopsInfo.AreaId.Underworld` | `TreeTopAreaCatalog.Underworld = 12` | proposed immutable catalog construction | Stable area index; does not own Underworld assets. |
| `TreeTopsInfo.AreaId.Count` | `TreeTopAreaCatalog.Count = 13` | proposed immutable catalog construction | Fixes the two array capacities; no weather or presentation system may mutate it. |
| `TreeTopsInfo._variations` | `TreeTopVariationStateComponent.Variations` | proposed `TreeTopVariationSystem` | Preserve `Save`/`Load` and 13-byte `SyncSend` boundaries; old-version fallback and packet-7 receive remain unresolved. |

### 13.3 Proposed implementation sequence

1. Freeze fixtures for the 13 area indices, array capacity, `GetTreeStyle`, world-coordinate Tile
   classification, random non-repeat rules, save version `211`, length truncation and the 13-byte
   network projection. Record `CopyExistingWorldInfo` and packet-7 receive as unresolved Version4
   boundaries rather than filling them from names or the complete reference.
2. Add the proposed immutable `TreeTopAreaCatalog` and the proposed
   `TreeTopVariationStateComponent`. Make `TreeTopVariationSystem` the only proposed writer of the
   variation array, including world generation and the explicit world-position randomization
   command. The query only reads committed variation values.
3. Add `TreeTopPersistenceAdapter` with the observed format: write the array length and integer
   values; on load versions below `211`, call the compatibility fallback; on newer versions,
   consume only the local capacity. Do not silently make the empty Version4 fallback equivalent to
   any complete-reference implementation.
4. Add the read-only `TreeTopNetworkProjection` for the 13 byte values. Keep packet-7 receive,
   authority and stale/duplicate handling behind integration review because the inspected
   `MessageBuffer` packet-7 case is empty.
5. Add `BackgroundChangeFlashStateComponent` and `BackgroundChangeFlashSystem` as a presentation
   work set. Preserve the 13 `UpdateCache` input slots and 0.05 decay, but leave `UpdateVariation`
   as a compatibility gap until its trigger and consumer are evidenced. The system must not write
   tree-top authority or world-save data.
6. During compatibility, compare committed tree-top values and serialized bytes before switching
   the legacy call sites. Legacy and proposed tree-top writers must not run for the same world
   transition; background flash refresh may observe committed values but cannot mutate them.

### 13.4 Single-write, compatibility and rollback strategy

`TreeTopVariationSystem` is the proposed sole writer for the saved 13-slot variation state.
`TreeTopPersistenceAdapter` and `TreeTopNetworkProjection` only serialize or project committed
values. `TreeTopVariationQuery` is read-only. `BackgroundChangeFlashSystem` owns its transient cache
refresh and decay only; no background presentation adapter may write tree-top variations.

Compatibility adapters may shadow-read `WorldGen.TreeTops` and compare persistence/network output,
but they must not execute a second variation writer. Preserve the Version4 `loadVersion < 211`
fallback contract and the empty packet-7 receive fact until integration review closes them. Roll back
by disabling the proposed variation/presentation route and restoring the legacy boundary if area-slot
ordering, randomization, save bytes, truncation, packet handling, flash decay or presentation timing
differs. No source rollback is performed by this plan.

### 13.5 Network, snapshot and persistence strategy

`TreeTopVariationStateComponent.Variations` is the only proposed value in this checkpoint with a
world-save boundary. Its save format must remain versioned and its packet projection must remain
distinct from background style arrays, `TreeTopAreaCatalog`, `NetworkId`, `PersistentEntityId`,
Tile coordinates and entity IDs. Do not serialize `BackgroundChangeFlashStateComponent`, its
`FlashPowerByArea`, resource handles, random streams or presentation payloads. Packet 7 contains
multiple world-info domains; the proposed adapter must project tree-top variations explicitly rather
than creating a broad ambient-sky snapshot. Receive, permissions and stale/duplicate policy remain
`crossSubsystemOwner: integration-review`.

### 13.6 Focused verifier additions

- Catalog and component tests for all 13 area values, `Count`, capacity invariants and read-only
  `GetTreeStyle` behavior.
- World-generation and world-position tests for Tile classification, `Main.treeX` partition input,
  random non-repeat rules, unsupported area IDs and explicit writer ownership.
- Save/load byte tests around version `211`, array length truncation, old-version fallback and
  13-byte network projection; packet-7 receive must prove that an empty receive handler does not
  fabricate client authority.
- Background cache tests for all 13 `UpdateCache` inputs, empty `UpdateVariation` compatibility,
  per-update 0.05 clamp, no world-state writes and presentation-port lifetime.
- Cross-boundary tests for duplicate/stale network packets, world reset/reload and simultaneous
  background refresh plus tree-top variation commit. Direct Version4 consumer closure, behavior
  equivalence and actual verifier execution remain `not-run`.

## 14. Twelfth execution checkpoint: `SharedAmbientSpawnAndWindState`

### 14.1 Proposed target files and 11-member mapping

`AmbientSpawnScheduleStateComponent.cs` is saved under `src2/WorldSession`. The following
non-Component paths and types remain `status: proposed` and are not claimed to exist:

```text
src2/WorldSession/Environment/Ambience/AmbientSpawnRequest.cs
src2/WorldSession/Environment/Ambience/AmbientSpawnIntervalPolicy.cs
src2/WorldSession/Environment/Ambience/AmbientSpawnPolicyCatalog.cs
src2/WorldSession/Environment/Ambience/AmbientSpawnScheduleStateComponent.cs
src2/WorldSession/Environment/Ambience/AmbientSpawnEligibilityQuery.cs
src2/WorldSession/Environment/Ambience/AmbientSpawnSystem.cs
src2/WorldSession/Environment/Ambience/AmbientSpawnRequestBuffer.cs
src2/WorldSession/Environment/Ambience/AmbientSpawnNetworkProjection.cs
src2/WorldSession/Environment/AmbientWind/AmbientWindWorkBuffer.cs
src2/WorldSession/Environment/AmbientWind/AmbientWindScanQuery.cs
src2/WorldSession/Environment/AmbientWind/AmbientWindSystem.cs
src2/WorldSession/Environment/AmbientWind/IAmbientWindRandomPort.cs
src2/WorldSession/Environment/AmbientWind/IAmbientWindTileReadPort.cs
src2/WorldSession/Environment/AmbientWind/IAmbientWindPresentationPort.cs
src2/WorldSession/Environment/AmbientWind/AmbientWindPresentationAdapter.cs
```

| Version4 member | proposed target role/file | proposed single writer or reader | execution boundary |
| --- | --- | --- | --- |
| `AmbienceServer.AmbienceSpawnInfo.skyEntityType` | `AmbientSpawnRequest.SkyEntityType` in `AmbientSpawnRequest.cs` | `AmbientSpawnSystem` creates; `AmbientSpawnNetworkProjection` reads committed requests | `SkyEntityType` is a definition ID, not an entity, Player, network or persistence ID. |
| `AmbienceServer.AmbienceSpawnInfo.targetPlayer` | `AmbientSpawnRequest.TargetPlayerSlot` in `AmbientSpawnRequest.cs` | `AmbientSpawnSystem` resolves the target; query and projection are read-only | Preserve `-1` as reselect-visible-player input; an explicit value is a Player slot/index. |
| `AmbienceServer.MINIMUM_SECONDS_BETWEEN_SPAWNS` | `AmbientSpawnIntervalPolicy.MinimumSeconds` in `AmbientSpawnIntervalPolicy.cs` | immutable proposed policy | Preserve the Version4 value `10`; verify its conversion against the tick schedule before migration. |
| `AmbienceServer.MAXIMUM_SECONDS_BETWEEN_SPAWNS` | `AmbientSpawnIntervalPolicy.MaximumSeconds` in `AmbientSpawnIntervalPolicy.cs` | immutable proposed policy | Preserve the Version4 value `120`; do not store it in the mutable schedule component. |
| `AmbienceServer._spawnConditions` | `AmbientSpawnPolicyCatalog.GlobalConditions` plus `AmbientSpawnEligibilityQuery` | catalog initialization; query only calculates eligibility | Main/NPC facts are explicit inputs; the query does not mutate world or Player state. |
| `AmbienceServer._secondarySpawnConditionsPerPlayer` | `AmbientSpawnPolicyCatalog.PerPlayerConditions` plus `AmbientSpawnEligibilityQuery` | catalog initialization; query only calculates eligibility | Player zone snapshots and cross-system ownership remain `crossSubsystemOwner: integration-review`. |
| `AmbienceServer._updatesUntilNextAttempt` | `AmbientSpawnScheduleStateComponent.UpdatesUntilNextAttempt` | `AmbientSpawnSystem` | Preserve `dayRate` decrement and the `600..7200` reset range, including the tenth-anniversary adjustment; no save claim. |
| `AmbienceServer._forcedSpawns` | `AmbientSpawnRequestBuffer.Pending` | `AmbientSpawnSystem` is the sole consumer; command adapters only submit | Match Version4's reverse removal and at-most-once consumption, including consumption when no eligible player is found. |
| `AmbientWindSystem._random` | `IAmbientWindRandomPort` in `IAmbientWindRandomPort.cs` | `AmbientWindSystem` receives the port | Keep seed/stream ownership explicit; Version4 wind methods are empty, so complete-reference random behavior is not confirmed. |
| `AmbientWindSystem._spotsForAirboneWind` | `AmbientWindWorkBuffer.CandidateSpots` in `AmbientWindWorkBuffer.cs` | `AmbientWindSystem` owns clear/commit; `AmbientWindScanQuery` returns candidates | Tile coordinates are transient work data and must not become a persistent component or network field. |
| `AmbientWindSystem._updatesCounter` | `AmbientWindWorkBuffer.UpdateCounter` in `AmbientWindWorkBuffer.cs` | `AmbientWindSystem` | Preserve the local `ZoneGraveyard` gate and cadence boundary; reset and world-switch semantics require integration review. |

### 14.2 Implementation order and single-write ownership

1. Freeze the Version4 evidence table for normal spawn, forced spawn, target selection, schedule
   decrement and packet serialization. The complete-reference Tile scan, probability values and Gore
   creation remain a separately labeled compatibility hypothesis because Version4's
   `SpawnAirborneWind`, `GetTileWorkSpace` and `TrySpawningWind` bodies are empty.
2. Add read-only ports for clock, weather, event facts, visible Player slots and Tile access. Add the
   immutable `AmbientSpawnPolicyCatalog` and `AmbientSpawnIntervalPolicy` only after their external
   inputs are explicit.
3. Make `AmbientSpawnSystem` the sole writer of `UpdatesUntilNextAttempt` and the sole owner of the
   forced-request consume boundary. `AmbientSpawnEligibilityQuery` may return ordinary and per-player
   candidates, but it cannot write `Main`, Player, the request buffer or the network projection.
4. Commit an `AmbientSpawnRequest` through `AmbientSpawnRequestBuffer`, resolving `targetPlayer=-1`
   to a visible Player slot when required and preserving an explicit slot as an index. Only after the
   request is committed may `AmbientSpawnNetworkProjection` serialize it.
5. Preserve the outbound `NetAmbienceModule` packet shape as an adapter boundary: Player `whoAmI`
   byte, random seed `int`, and `SkyEntityType` byte. Keep Version4 `Deserialize` empty until receive,
   permission, duplicate and stale semantics are evidenced; an empty receive path must not fabricate
   client Sky authority.
6. Run `AmbientWindSystem` after its local-player/graveyard gate and after a read-only Tile/random
   query. It owns `UpdateCounter` and candidate-buffer clearing; `IAmbientWindPresentationPort` owns
   Gore/client effects. Candidate coordinates, random handles and effect resources are cleared after
   their presentation lifetime and at world reset.
7. Keep all proposed writers behind the existing compatibility boundary until the focused verifier
   proves one state transition per request and the integration review resolves the Version4 stub
   policy. No source migration is performed in this plan.

The single-write rule is explicit: `AmbientSpawnSystem` writes ambient schedule and request-buffer
state; `AmbientWindSystem` writes wind work-buffer state; the eligibility/scan queries are pure; network,
Gore and client adapters observe committed values only. No NPC, weather presentation, Sky projection or
Tile adapter may write these authorities directly.

### 14.3 Forced-request rollback, network and persistence boundary

Forced requests are transient commands. The proposed buffer records an attempt and removes each entry
at most once, matching Version4's reverse-removal behavior even when no player satisfies the ordinary
eligibility policy. An unknown external delivery result must not silently requeue the request: retain an
explicit failure/diagnostic outcome for integration review, or disable the proposed writer and restore
the legacy compatibility boundary before any legacy write root is removed. This avoids duplicate Sky
spawns while leaving retry policy visible rather than implicit.

`targetPlayer` is not a network entity identity. The implementation plan keeps these values distinct:
the `targetPlayer=-1` sentinel, a Player slot/index, `SkyEntityType`, `NetworkId`,
`PersistentEntityId`, Tile coordinates and any spawned entity ID. The outbound packet projection may
resolve the selected Player slot to `whoAmI`, but the core request must not replace the slot with a
network or persistence ID.

`AmbientSpawnRequest`, `AmbientSpawnScheduleStateComponent`, `AmbientSpawnRequestBuffer`,
`AmbientWindWorkBuffer`, `IAmbientWindRandomPort` handles, candidate Tile coordinates and Gore/client
resources are transient. They do not enter world save data or network snapshots. Only a later
integration-reviewed projection may carry committed packet inputs. The empty Version4 receive method is
preserved as an explicit compatibility constraint until a receiver owner is confirmed.

### 14.4 Focused verifier and checkpoint status

The future focused verifier must cover:

- the `10..120` policy values, `600..7200` tick reset range, `dayRate` decrement and
  tenth-anniversary shortening;
- ordinary global/per-player eligibility, visible-player absence, secondary/fallback selection and
  the distinction between policy rejection and request-buffer consumption;
- `targetPlayer=-1`, explicit Player slots, invalid/offline targets, reverse removal and no duplicate
  consumption after a failed or unknown external delivery;
- packet field widths and ordering (`whoAmI` byte, seed `int`, `SkyEntityType` byte), plus proof that
  Version4's empty `Deserialize` path does not fabricate a client request;
- graveyard gating, work-buffer cadence, Tile bounds, deterministic random-port behavior, candidate
  clearing, Gore-port lifetime and world-reset cleanup;
- an explicit compatibility test showing that Version4's empty wind methods are not silently replaced
  by complete-reference behavior.

implementationStatus: completed (Component-only subset)
verificationStatus: partial (serial build passed; focused verifier and behavior-equivalence not run)
productionCodeModified: no; src2CodeModified: yes

The Component source for this checkpoint is saved under `src2/WorldSession`. Registration, network,
persistence, compile, test and verifier work for the proposed non-Component boundary remains deferred.

## 15. Thirteenth execution checkpoint: `SharedCelebrationAndLanternEvents`

### 15.1 Proposed target files and 16-member mapping

`BirthdayPartyStateComponent.cs`, `LanternNightStateComponent.cs` and
`MysticFairyEventStateComponent.cs` are saved under `src2/WorldSession`. Every remaining path and
type below is `status: proposed` and is not claimed to exist:

```text
src2/WorldSession/Environment/Celebration/BirthdayPartyStateComponent.cs
src2/WorldSession/Environment/Celebration/BirthdayPartyRosterBuffer.cs
src2/WorldSession/Environment/Celebration/BirthdayPartyPolicyCatalog.cs
src2/WorldSession/Environment/Celebration/BirthdayPartyActiveQuery.cs
src2/WorldSession/Environment/Celebration/BirthdayPartySystem.cs
src2/WorldSession/Environment/Celebration/BirthdayPartyProjection.cs
src2/WorldSession/Environment/Celebration/BirthdayPartyTransitionCache.cs
src2/WorldSession/Environment/Celebration/LanternNightStateComponent.cs
src2/WorldSession/Environment/Celebration/LanternNightPolicyCatalog.cs
src2/WorldSession/Environment/Celebration/LanternNightActiveQuery.cs
src2/WorldSession/Environment/Celebration/LanternNightSystem.cs
src2/WorldSession/Environment/Celebration/LanternNightProjection.cs
src2/WorldSession/Environment/Celebration/LanternNightTransitionCache.cs
src2/WorldSession/Environment/Celebration/MysticFairyEventStateComponent.cs
src2/WorldSession/Environment/Celebration/MysticFairySpawnPolicy.cs
src2/WorldSession/Environment/Celebration/MysticFairyLogScanWorkBuffer.cs
src2/WorldSession/Environment/Celebration/MysticFairyEligibilityQuery.cs
src2/WorldSession/Environment/Celebration/MysticFairySpawnSystem.cs
src2/WorldSession/Environment/Celebration/ICelebrationTileReadPort.cs
src2/WorldSession/Environment/Celebration/ICelebrationNpcSpawnPort.cs
src2/WorldSession/Environment/Celebration/ICelebrationPresentationPort.cs
```

| Version4 member | proposed target role/file | proposed single writer or reader | execution boundary |
| --- | --- | --- | --- |
| `BirthdayParty.ManualParty` | `BirthdayPartyStateComponent.ManualOverride` | proposed `BirthdayPartySystem` writes; command adapter submits | Manual authority is distinct from the genuine event fact; command permission and receive ownership remain `crossSubsystemOwner: integration-review`. |
| `BirthdayParty.GenuineParty` | `BirthdayPartyStateComponent.GenuineActive` | proposed `BirthdayPartySystem` writes from natural-attempt and day/night inputs | Do not write it from the derived active query or from presentation. |
| `BirthdayParty.PartyDaysOnCooldown` | `BirthdayPartyStateComponent.DaysOnCooldown` | proposed `BirthdayPartySystem` decrements and resets | Preserve the observed `5..10` success range and world-clear reset; persistence is not claimed. |
| `BirthdayParty.CelebratingNPCs` | proposed `BirthdayPartyRosterBuffer.LegacyNpcSlots` plus an `NpcEntityId` resolution port | proposed birthday system commits the roster; identity adapter resolves slots | `whoAmI` is a Version4 NPC slot, not a stable entity, network or persistence ID. Validate slot generation and activity before resolving. |
| `BirthdayParty._wasCelebrating` | proposed `BirthdayPartyTransitionCache.PreviousActive` | proposed birthday system updates the edge cache | Transition cache is presentation/input history only and is not saved or network authority. |
| `BirthdayParty.PartyIsUp` | proposed `BirthdayPartyActiveQuery` | pure query reads `GenuineActive` and `ManualOverride` | Preserve `GenuineParty || ManualParty`; the query cannot write either fact. |
| `LanternNight.ManualLanterns` | `LanternNightStateComponent.ManualOverride` | proposed `LanternNightSystem` writes from the manual command path | Command permission, final network owner and duplicate handling remain integration-review boundaries. |
| `LanternNight.GenuineLanterns` | `LanternNightStateComponent.GenuineActive` | proposed lantern system writes from natural-attempt and morning/night transitions | Complete-reference natural-attempt behavior is supplementary because the Version4 boundary must remain explicit. |
| `LanternNight.NextNightIsLanternNight` | `LanternNightStateComponent.NextNightRequested` | proposed lantern system consumes the request once during the night reducer | Preserve the NPC event-cleared writer and one-shot consumption; do not let a projection re-arm it. |
| `LanternNight.LanternNightsOnCooldown` | `LanternNightStateComponent.NightsOnCooldown` | proposed `LanternNightSystem` decrements and resets | Preserve the observed cooldown lifecycle and world-clear reset; persistence is unresolved. |
| `LanternNight._wasLanternNight` | proposed `LanternNightTransitionCache.PreviousActive` | proposed lantern system updates the edge cache | Edge history is not a durable event fact and must not enter packet 7. |
| `LanternNight.LanternsUp` | proposed `LanternNightActiveQuery` | pure query reads `GenuineActive` and `ManualOverride` | Preserve `GenuineLanterns || ManualLanterns`; no weather, wind or UI writer may mutate it. |
| `MysticLogFairiesEvent._canSpawnFairies` | `MysticFairyEventStateComponent.CanAttemptSpawn` | proposed `MysticFairySpawnSystem` writes at night/reset and after an evidenced spawn result | Version4 `TrySpawningFairies` is empty; a successful-spawn transition cannot be claimed until integration review resolves the stub. |
| `MysticLogFairiesEvent._delayUntilNextAttempt` | `MysticFairyEventStateComponent.DelayUntilNextAttempt` | proposed fairy system decrements by the explicit day-rate input and commits the retry delay | Preserve the observed `60` reset path without assuming complete-reference units beyond the Version4 evidence. |
| `MysticLogFairiesEvent.DELAY_BETWEEN_ATTEMPTS` | proposed `MysticFairySpawnPolicy.DelayBetweenAttempts` | immutable policy value | Policy is not mutable ECS state; calendar/day-rate adaptation is an explicit input. |
| `MysticLogFairiesEvent._stumpCoords` | `MysticFairyLogScanWorkBuffer.CandidateStumps` | proposed scan system clears/rebuilds; spawn system reads | Tile coordinates are transient work data. `GetStumpTopLeft` is empty in Version4, so coordinate reconstruction remains an evidence gap. |

### 15.2 Implementation order and single-write ownership

1. Freeze the Version4 evidence table for manual toggles, genuine event facts, cooldowns, the
   next-night request, NPC roster slots, packet 7/111 boundaries and the three Mystic Fairy methods.
   Complete-reference natural-attempt, qualification, spawn-probability, line-of-sight and coordinate
   code remains a labeled supplementary hypothesis.
2. Add proposed state components, transition caches, the roster/work buffers and immutable policy
   catalogs. Add ports for world clock/rules, stable NPC identity resolution, read-only Tile access,
   NPC spawn submission and presentation; none of these ports may write event authority implicitly.
3. Make proposed `BirthdayPartySystem` the sole writer of birthday manual/genuine/cooldown facts,
   roster commits and the previous-active cache. Make proposed `LanternNightSystem` the sole writer
   of lantern manual/genuine/next-night/cooldown facts and its previous-active cache. The active
   queries remain pure.
4. Preserve the observed scheduler edges: normal ticks update BirthdayParty, LanternNight and the
   Mystic Log event; entering night runs birthday check, lantern check and fairy start; entering
   morning runs the two event cleanup checks. World initialization starts the fairy event and world
   clear resets all three event boundaries. These are proposed scheduler constraints, never file-order
   behavior.
5. Project only committed `PartyIsUp` and `LanternsUp` facts to packet 7 at the observed bit positions
   (`bitsByte9[7]` and `bitsByte11[1]`). Keep Birthday manual command traffic at the packet 111
   boundary. Preserve the Version4 packet 7 receive no-op until an owner, permission model and stale/
   duplicate policy are evidenced; an adapter must not fabricate client authority.
6. Rebuild `MysticFairyLogScanWorkBuffer` from a read-only Tile port, evaluate eligibility through a
   pure query, and submit NPC creation only through `ICelebrationNpcSpawnPort`. Do not fill
   `IsAGoodTime`, `TrySpawningFairies` or `GetStumpTopLeft` from the complete reference in this plan.
7. Clear roster, transition caches, retry state and scan worksets at the documented morning/world-reset
   boundaries. Keep stable NPC identity resolution separate from the legacy slot and never let packet,
   UI, Sky or NPC queries write the proposed world facts.

The single-write rule is explicit: the birthday system owns birthday state, the lantern system owns
lantern state, and the fairy system owns fairy retry/eligibility state. Active queries, Tile readers,
network projections, presentation ports and NPC spawn adapters observe submitted values only. The
`NPC.Spawner.fairyLog` writer is shared with NPC spawn infrastructure and therefore remains
`crossSubsystemOwner: integration-review`.

### 15.3 Compatibility, network, persistence and rollback boundary

`CelebratingNPCs` remains a legacy slot buffer. A proposed adapter may resolve each slot to a stable
`NpcEntityId` only after checking the slot generation and active entity; it must not substitute a
`NetworkId` or `PersistentEntityId`. Roster entries, `_wasCelebrating`, `_wasLanternNight`, fairy stump
coordinates and all fairy scan buffers are transient and are not proposed save or network fields.

The packet 7 projection contains only the committed active facts observed in Version4. It does not
serialize cooldowns, transition caches, roster slots or Tile worksets. Birthday manual control keeps
the packet 111 command boundary, while packet 7 receive remains an explicit empty/unknown boundary.
Any future persistence DTO must be assigned by integration review; no save compatibility claim is made
for the event cooldowns or manual overrides here.

Rollback is required before removing a legacy writer if a verifier observes a different manual/genuine
precedence, cooldown or next-night consumption; if a packet projection can mutate authority; if an NPC
slot resolves to a stale entity; if a fairy request is duplicated or committed after a failed spawn;
or if complete-reference behavior is introduced despite a Version4 empty method. Disable the proposed
writer and restore the last compatibility adapter boundary. No source rollback is performed by this plan.

### 15.4 Focused verifier and checkpoint status

The future focused verifier must cover:

- manual override precedence, genuine activation, night/morning transitions, cooldown `5..10`, roster
  cleanup and stale/reused NPC-slot rejection;
- Lantern Night manual/genuine precedence, forbidden-event conditions supported by Version4 evidence,
  one-shot `NextNightIsLanternNight` consumption, cooldown and morning cleanup;
- packet 7 field positions and widths, packet 111 manual command routing, empty packet 7 receive,
  duplicate/stale command handling and proof that projections cannot write authority;
- fairy log scan clear/rebuild, Tile type `488`, remix/world bounds, day-rate delay, visible-player/LOS
  inputs, NPC spawn command ownership and the explicit Version4 empty-method compatibility policy;
- reset behavior for all transition caches, cooldowns, roster buffers and Tile scan worksets.

implementationStatus: completed (Component-only subset)
verificationStatus: partial (serial build passed; focused verifier and behavior-equivalence not run)
productionCodeModified: no; src2CodeModified: yes

The Component source for this checkpoint is saved under `src2/WorldSession`. Registration, network,
persistence, compile, test and verifier work for the proposed non-Component boundary remains deferred.

## 16. Fourteenth execution checkpoint: `SharedRitualAndStormEvents`

### 16.1 Proposed target files and 12-member mapping

`CultistRitualStateComponent.cs` and `SandstormStateComponent.cs` are saved under
`src2/WorldSession`. Every remaining path and type below is `status: proposed` and is not claimed
to exist:

```text
src2/WorldSession/Environment/Ritual/CultistRitualStateComponent.cs
src2/WorldSession/Environment/Ritual/CultistRitualPolicy.cs
src2/WorldSession/Environment/Ritual/CultistRitualEligibilityQuery.cs
src2/WorldSession/Environment/Ritual/CultistRitualSystem.cs
src2/WorldSession/Environment/Ritual/ICultistRitualTileReadPort.cs
src2/WorldSession/Environment/Ritual/ICultistRitualNpcSpawnPort.cs
src2/WorldSession/Environment/Storm/SandstormStateComponent.cs
src2/WorldSession/Environment/Storm/SandstormPolicy.cs
src2/WorldSession/Environment/Storm/SandstormSystem.cs
src2/WorldSession/Environment/Storm/SandstormActiveQuery.cs
src2/WorldSession/Environment/Storm/SandstormNetworkProjection.cs
src2/WorldSession/Environment/Storm/SandstormPersistenceAdapter.cs
```

| Version4 member | proposed target role/file | proposed single writer or reader | execution boundary |
| --- | --- | --- | --- |
| `CultistRitual.delayStart` | `CultistRitualPolicy.DelayStartTicks` | immutable policy value | Keep as policy evidence; current Version4 call graph does not prove a runtime read. |
| `CultistRitual.respawnDelay` | `CultistRitualPolicy.RespawnDelayTicks` | immutable policy value | `TabletDestroyed` resets the delay to `43200`; do not mix the policy with persisted state. |
| `CultistRitual.timePerCultist` | `CultistRitualPolicy.TimePerCultistTicks` | immutable policy value | Current Version4 usage is not closed; no new behavior may be inferred from the name. |
| `CultistRitual.recheckStart` | `CultistRitualPolicy.RecheckStartTicks` | immutable policy value | The observed retry gate uses `600`, and multiplies it by `6` when `NPC.AnyDanger()` is true. |
| `CultistRitual.delay` | `CultistRitualStateComponent.DelayTicks` | proposed `CultistRitualSystem` writes | Day-rate decrement, zero clamp, tablet reset and WorldFile save/load must remain in one explicit state boundary. |
| `CultistRitual.recheck` | `CultistRitualStateComponent.RecheckTicks` | proposed `CultistRitualSystem` writes | Treat as transient retry gate until a persistence decision exists; do not serialize it by analogy. |
| `Sandstorm.SANDSTORM_DURATION_MINIMUM` | `SandstormPolicy.MinimumDurationTicks` | immutable policy value | Version4 declares `28800`; the empty start method does not prove duration assignment. |
| `Sandstorm.SANDSTORM_DURATION_MAXIMUM` | `SandstormPolicy.MaximumDurationTicks` | immutable policy value | Version4 guards an overlarge `TimeLeft` with `86400`; complete start-duration behavior remains open. |
| `Sandstorm.Happening` | `SandstormStateComponent.Active` | proposed `SandstormSystem` writes | Persisted activity fact and packet 7 output bit; WorldGen clear sets it false. |
| `Sandstorm.TimeLeft` | `SandstormStateComponent.RemainingTicks` | proposed `SandstormSystem` writes | Activity and wind rules reduce it; zero wind forces zero and calls the Version4 empty stop method. |
| `Sandstorm.Severity` | `SandstormStateComponent.Severity` | proposed `SandstormSystem` writes | NaN is corrected, then the value approaches the target and is clamped to `0..1`; finite invariant is required. |
| `Sandstorm.IntendedSeverity` | `SandstormStateComponent.TargetSeverity` | proposed `SandstormSystem` writes | Random intention changes are projected to packet 7/world-info; receive must not become authority. |

### 16.2 Implementation order and single-write ownership

1. Freeze the direct evidence from `CultistRitual.cs`, `Sandstorm.cs`, `Main.UpdateTime`, `WorldFile`,
   `NetMessage`, `MessageBuffer`, `WorldGen` and the NPC ritual path. Keep `delayStart` and
   `timePerCultist` marked partial where their Version4 call graph is not demonstrated.
2. Add the proposed ritual state and policy types, then the read-only Tile/LOS input seam and the
   NPC spawn command port. `CultistRitualEligibilityQuery` must accept a snapshot of hardmode,
   defeated-boss facts, delay, existing NPC type `437`, bounds, LOS and floor/collision facts; it
   returns a decision and transient spawn points but never creates an NPC or mutates state.
3. Make proposed `CultistRitualSystem` the sole writer of `DelayTicks` and `RecheckTicks`. Preserve
   the day-rate decrement, zero clamp, dangerous-NPC six-times recheck delay, tablet reset and the
   `force`/LOS/bounds gates. Submit NPC type `437` only through `ICultistRitualNpcSpawnPort` after
   the read-only eligibility result is accepted.
4. Add the proposed Sandstorm state and policy types, then make `SandstormSystem` the sole writer of
   `Active`, `RemainingTicks`, `Severity` and `TargetSeverity`. Inject day rate, wind, hardmode and
   random input through explicit ports or snapshots; do not hide clocks or randomness in the state.
5. Preserve the Version4 stub boundary for `StartSandstorm` and `StopSandstorm`. Until integration
   review decides otherwise, an implementation may record the attempted transition at the seam but
   must not silently assign duration or add start/stop effects from the complete reference.
6. Add network and persistence adapters after the state reducers are independently specified. Map
   packet 7 `bitsByte10[3]` to a read-only Sandstorm active projection, map world-info
   `IntendedSeverity` to a float projection, restore `<174` Sandstorm defaults, and persist the
   observed ritual delay plus Sandstorm fields without inventing recheck persistence.
7. Add focused tests for pure eligibility/severity calculations and effect-port failure/duplicate
   behavior before removing any legacy writer. World reset must preserve the observed Sandstorm
   clear call; a CultistRitual reset must remain an integration decision because no matching
   Version4 WorldGen call was found.

The single-write rule is explicit: `CultistRitualSystem` owns ritual countdown state, and
`SandstormSystem` owns all four Sandstorm state fields. Queries, Tile/LOS ports, NPC spawn ports,
network projections, persistence adapters and presentation consumers observe or submit explicit
requests only; they cannot mutate authority through a read or projection path.

### 16.3 Compatibility, network, persistence and rollback boundary

`CultistRitual.delay` is a durable countdown: `WorldFile` stages, saves and restores it. The
proposed persistence adapter must keep `RecheckTicks` transient until its recovery semantics are
decided. `CheckFloor`'s four `Point` values are a short-lived work buffer and the legacy NPC slot/type
must not be promoted to a persistent or network identity. Tile reads, LOS, collision and NPC type
`437` creation remain adapter/command boundaries owned across the ritual, Tile and NPC subsystems.

The proposed Sandstorm persistence adapter maps `Active`, `RemainingTicks`, `Severity` and
`TargetSeverity` to the observed WorldFile fields. For save versions below `174`, it restores false
and zero values exactly as Version4 does. The network projection writes only the observed packet 7
activity bit (`bitsByte10[3]`) and world-info target-severity float. `MessageBuffer` packet 7 receive
is empty in Version4, so no client-side receive or projection may be treated as a Sandstorm authority
write without an integration decision.

The rollback trigger is any verifier result showing a second writer, a persisted recheck that changes
retry timing, a stale Tile/LOS snapshot used for NPC creation, duplicated or partially committed NPC
spawn, a packet projection that mutates authority, severity outside finite `0..1`, an incorrect
`<174` default, or any duration/start/stop effect added despite the Version4 empty methods. Disable
the proposed writer and restore the compatibility adapter at the last legacy boundary. No source
rollback is performed by this plan.

### 16.4 Focused verifier and checkpoint status

The future focused verifier must cover:

- ritual day-rate decrement, zero clamp, `TabletDestroyed` reset, `600` recheck and six-times danger
  delay;
- ritual force/LOS/bounds/hardmode/Boss/type `437` gates, four-point transient work-buffer lifetime,
  Tile read isolation and exactly one NPC spawn command;
- ritual save/load of `delay`, absence of accidental `recheck` persistence and the unresolved world
  reset boundary;
- Sandstorm active and inactive transitions, duration upper guard, wind-dependent time reduction,
  zero-wind stop request, random input and the Version4 empty start/stop compatibility seam;
- finite severity, NaN correction, target movement, `0..1` clamp and explicit `Severity`/
  `TargetSeverity` authority;
- packet 7 `bitsByte10[3]`, world-info float width, empty packet 7 receive and stale/duplicate
  policy, plus WorldFile `<174` defaults and four-field round trip.

implementationStatus: completed (Component-only subset)
verificationStatus: partial (serial build passed; focused verifier and behavior-equivalence not run)
productionCodeModified: no; src2CodeModified: yes

The Component source for this checkpoint is saved under `src2/WorldSession`. Registration, network,
persistence, compile, test and verifier work for the proposed non-Component boundary remains deferred.

## 17. Fifteenth execution checkpoint: `SharedSceneWeatherAndEventZoneState`

### 17.1 Proposed target files and 9-member mapping

This checkpoint has no independently expressible Component file in `completedComponents`. Every
path and type below is `status: proposed` and is not claimed to exist:

```text
src2/WorldSession/Environment/Scene/SceneWeatherEventZoneSnapshot.cs
src2/WorldSession/Environment/Scene/SceneWeatherEventZoneScanPolicy.cs
src2/WorldSession/Environment/Scene/SceneWeatherEventZoneScanSystem.cs
src2/WorldSession/Environment/Scene/SceneWeatherEventZoneQuery.cs
src2/WorldSession/Environment/Scene/ISceneMetricsTileReadPort.cs
src2/WorldSession/Environment/Scene/ISceneMetricsPlayerEffectPort.cs
src2/WorldSession/Environment/Scene/SceneWeatherEventZoneProjection.cs
```

| Version4 member | proposed target role/file | proposed single writer or reader | execution boundary |
| --- | --- | --- | --- |
| `SceneMetrics.ZoneRain` | `SceneWeatherEventZoneSnapshot.Raining` | proposed `SceneWeatherEventZoneScanSystem` writes; weather/Rain readers read | Scene scan result only; it is not the persistent weather authority. |
| `SceneMetrics.ZoneSandstorm` | `SceneWeatherEventZoneSnapshot.Sandstorm` | proposed `SceneWeatherEventZoneScanSystem` writes; Sandstorm/NPC/visual readers read | Local zone result remains separate from `SandstormStateComponent.Active`. |
| `SceneMetrics.SurfaceAtmospherics` | `SceneWeatherEventZoneSnapshot.SurfaceAtmospherics` | proposed scan system writes; scene presentation reads | Surface/depth calculation remains an explicit input boundary because Version4 calculation is empty. |
| `SceneMetrics.UndergroundForShimmering` | `SceneWeatherEventZoneSnapshot.UndergroundForShimmering` | proposed scan system writes; shimmer readers read | Depth result is frame-scoped and must not become world persistence. |
| `SceneMetrics.ZoneShimmer` | `SceneWeatherEventZoneSnapshot.Shimmer` | proposed scan system writes; Player/Faeling/presentation readers read | Player accessors remain read-only projections. |
| `SceneMetrics.ZoneWaterCandle` | `SceneWeatherEventZoneSnapshot.WaterCandle` | proposed scan system writes from Tile/player-effect inputs | Candle source and precedence require verifier coverage; no reader may write it. |
| `SceneMetrics.ZonePeaceCandle` | `SceneWeatherEventZoneSnapshot.PeaceCandle` | proposed scan system writes from Tile/player-effect inputs | Peace/Water candle interaction remains a pure calculation boundary. |
| `SceneMetrics.ZoneShadowCandle` | `SceneWeatherEventZoneSnapshot.ShadowCandle` | proposed scan system writes; NPC spawn readers read | NPC spawn adjustments cannot feed back into the snapshot. |
| `SceneMetrics.InTorchGodMinigame` | `SceneWeatherEventZoneSnapshot.InTorchGodMinigame` | proposed scan system writes from player-effect input; gameplay/presentation reads | Torch God input is perspective-player scoped; event authority remains integration-review. |

### 17.2 Implementation order and single-write ownership

1. Freeze the direct evidence from `SceneMetrics`, `Main.UpdateSceneMetrics`,
   `Player.UpdateSceneMetrics`, Player zone accessors, `SceneState.Update`, NPC zone readers and
   Sandstorm visual readers. Record the Version4 `Scan` cache gate, scan order and `Reset` clearing
   boundary before proposing any replacement writer.
2. Add the proposed immutable `SceneWeatherEventZoneSnapshot` and `SceneWeatherEventZoneScanPolicy`
   shape. The snapshot must carry frame version, scan center, TileCenter, camera/player identity and
   all nine results without claiming persistence or network authority. TileCenter, coordinate value
   objects, frame identity and snapshot ownership remain `crossSubsystemOwner: integration-review`.
3. Add `ISceneMetricsTileReadPort` and `ISceneMetricsPlayerEffectPort`. Tile, wall, liquid, candle,
   visual-area and perspective-player inputs must be explicit and read-only; clocks, random sources,
   logging, persistence and network effects cannot be hidden in the snapshot.
4. Make proposed `SceneWeatherEventZoneScanSystem` the sole writer. It must implement the proposed
   cache contract: same frame and same center may reuse the committed snapshot; a new frame, changed
   center, changed perspective player, camera mode change or world reset must invalidate before a
   new scan. `SceneMetrics.Reset` remains the compatibility reset boundary and must clear all nine
   fields before a new snapshot is committed.
5. Add `SceneWeatherEventZoneQuery` as a pure read-only query and
   `SceneWeatherEventZoneProjection` as a one-way output boundary for Player, NPC, Rain, SceneState,
   UI and presentation. Neither may mutate weather/event authority or trigger a second scan writer.
6. Preserve the two observed metrics lifecycles: Main camera metrics and Player-owned metrics are
   separate instances. When a tracked camera/perspective player is used, local-player metrics and
   selected perspective-player inputs must not overwrite one another through a shared mutable
   snapshot.
7. Add focused tests for cache hit/miss, reset, source isolation and projection direction before
   changing any legacy call site. Complete-reference zone formulas may be compared as supplementary
   cases only; they must not silently replace Version4's empty `ScanTiles`, `AggregateTileCounts`,
   `CalculateZones`, `ScanNPCPositions` or `AddPlayerEffects` methods.

The single-write rule is explicit: `SceneWeatherEventZoneScanSystem` owns the committed nine-field
snapshot and its invalidation. Tile/player-effect ports provide inputs only. Query and projections are
read-only. Player, NPC, Rain and SceneState consumers may receive projections, but cannot write the
snapshot, Sandstorm authority, weather authority or Tile state through this boundary.

### 17.3 Compatibility, snapshot, network, persistence and rollback boundary

The proposed execution must retain Version4's same-frame/same-center cache behavior and its order of
reset, center assignment, Tile scan, visual scan, NPC scan, aggregation, zone calculation and player
effects. `Reset` must clear all nine scene fields plus related scan state; no stale snapshot may survive
a frame, center, perspective-player or world-reset invalidation. Camera and Player metrics are separate
compatibility boundaries, not two views over one mutable global component.

No independent WorldFile or packet 7/78 DTO was found for these nine fields. The plan therefore adds
no persistence or network migration. Any future client/UI output must be an explicit projection with
freshness checks and must not be treated as world authority. Complete-reference formulas and player
effect behavior remain supplementary evidence until integration review closes the empty Version4
methods.

Rollback is required if a focused verifier finds a same-center cache miss, stale values after `Reset`,
cross-player or camera snapshot leakage, a second writer, a projection that mutates authority, candle
or Torch God state sourced from the wrong player, rain/sandstorm zone confusion, or any complete-reference
algorithm silently replacing a Version4 empty method. Disable the proposed scan writer and restore the
last legacy read boundary; no source rollback is performed by this planning session.

### 17.4 Focused verifier and checkpoint status

The future focused verifier must cover:

- same frame plus same scan center cache hit, center change rescan, new-frame invalidation and world-reset invalidation;
- visual scan-area and perspective-player isolation, including separate Main camera and Player metrics instances;
- `Reset` clearing `ZoneRain`, `ZoneSandstorm`, `SurfaceAtmospherics`, `UndergroundForShimmering`,
  `ZoneShimmer`, `ZoneWaterCandle`, `ZonePeaceCandle`, `ZoneShadowCandle` and `InTorchGodMinigame`;
- rain/sandstorm mutual-exclusion boundaries, surface atmosphere and underground shimmer depth conditions;
- Tile/liquid/candle input isolation, Water/Peace/Shadow candle source boundaries and Torch God player-effect input;
- read-only Player/NPC/Rain/SceneState/UI projections and no authority mutation through queries or projections;
- no stale snapshot leakage across frame, center or perspective-player changes;
- explicit preservation of Version4 empty scan/calculation methods rather than silent complete-reference replacement.

implementationStatus: completed (Component-only subset)
verificationStatus: partial (serial build passed; focused verifier and behavior-equivalence not run)
productionCodeModified: no; src2CodeModified: yes

The Component source for this checkpoint is saved under `src2/WorldSession`. Registration, network,
persistence, compile, test and verifier work for the proposed non-Component boundary remains deferred.

## 18. Sixteenth execution checkpoint: `SharedInvasionDamageTrackingState`

### 18.1 Proposed target paths and 4-member mapping

This checkpoint has no independently expressible Component file in `completedComponents`. Every
target below is `status: proposed`; no file is claimed to exist and no inherited `NPCDamageTracker`
state is duplicated:

```text
src2/WorldSession/Events/Dd2/Dd2InvasionDamageTrackingSession.cs
src2/WorldSession/Events/Dd2/Dd2InvasionDamageTrackingSystem.cs
src2/WorldSession/Events/Dd2/Dd2InvasionDamageTrackingNameQuery.cs
src2/WorldSession/Events/Dd2/Dd2InvasionDamageOutcomeProjection.cs
src2/WorldSession/Events/Dd2/IDd2InvasionDamageTrackerPort.cs
```

| Version4 member | proposed target role | proposed single writer or reader | execution boundary |
| --- | --- | --- | --- |
| `DamageTracker._won` | `Dd2InvasionDamageTrackingSession.Won` | proposed damage-tracking system writes | The result is written exactly once by an explicit win/loss stop command; the message projection only reads it. |
| `DD2Event._damageTracker` | active `Dd2InvasionDamageTrackingSession` handle | proposed DD2 damage-tracking system owns creation/cleanup | Start/register and world-reset boundaries are explicit; inherited base tracker fields stay behind the proposed port. |
| `DamageTracker.Name` | `Dd2InvasionDamageTrackingNameQuery.Name` | proposed pure query reads localization | Fixed Old Ones Army localization lookup; no lifecycle or state mutation. |
| `DamageTracker.KillTimeMessage` | `Dd2InvasionDamageOutcomeProjection.KillTimeMessage` | proposed projection reads committed outcome | Selects defeated/lost text from `Won`; UI, log and network outputs cannot write the session. |

### 18.2 Implementation order and single-write ownership

1. Freeze the Version4 start/register, NPC damage/update/kill, failure stop and world-reset call graph.
2. Add the proposed event-scoped session and explicit `IDd2InvasionDamageTrackerPort`; do not copy
   the base `NPCDamageTracker` fields into an entity component.
3. Make the proposed damage-tracking system the only writer for session creation, `Won`, stop and
   reset. Route NPC damage and kill facts through commands or the port, then commit one outcome.
4. Expose `Name` through a read-only query and `KillTimeMessage` through a one-way projection only
   after the outcome is committed.
5. Keep Version4 `IncludeDamageFor` returning `false`; any complete-reference filter behavior needs
   a separate integration decision and verifier case.

The required order is: DD2 start -> session registration -> damage/kill submissions -> one outcome
commit -> message projection -> tracker cleanup. `StopInvasion(win: true)` and the empty
`WinInvasionInternal` remain unresolved evidence boundaries; this plan does not infer successful
tracker cleanup from the projection.

### 18.3 Compatibility, network, persistence and rollback

No independent WorldFile, packet 7/78 or event-snapshot DTO was found for these four members. This
checkpoint adds no save or network migration. Event session identity, `NpcEntityId`, `NetworkId` and
`PersistentEntityId` remain separate and are `crossSubsystemOwner: integration-review`.

Rollback is required if a second writer can mutate `Won`, a failed stop emits a success message, a
successful stop is inferred without evidence, reset leaves an active tracker, or a projection/query
mutates the session. Disable the proposed adapter/projection and restore the legacy read boundary;
this planning session performs no source rollback.

### 18.4 Focused verifier and checkpoint status

Future verification must cover one tracker per start, damage/update/kill ordering, reset cleanup,
`Stop(false)` message selection, the unresolved successful stop path, pure `Name` lookup, empty
`IncludeDamageFor` compatibility and projection isolation. All results are currently unrun.

implementationStatus: completed (Component-only subset)
verificationStatus: partial (serial build passed; focused verifier and behavior-equivalence not run)
productionCodeModified: no; src2CodeModified: yes

The Component source for this checkpoint is saved under `src2/WorldSession`. Registration, network,
persistence, compile, test and verifier work for the proposed non-Component boundary remains deferred.

## 19. Seventeenth execution checkpoint: `SharedInvasionWaveAndArenaState`

### 19.1 Proposed target paths and 22-member mapping

`Dd2InvasionProgressionStateComponent.cs`, `Dd2InvasionRunStateComponent.cs`,
`Dd2InvasionWaveStateComponent.cs` and `Dd2InvasionCrystalDropStateComponent.cs` are saved under
`src2/WorldSession`. `Dd2InvasionArenaStateComponent.cs` remains deferred. Every remaining target
below is `status: proposed` and is not claimed to exist:

```text
src2/WorldSession/Events/Dd2/Dd2InvasionDefinition.cs
src2/WorldSession/Events/Dd2/Dd2InvasionPresentationPolicy.cs
src2/WorldSession/Events/Dd2/Dd2InvasionProgressionStateComponent.cs
src2/WorldSession/Events/Dd2/Dd2InvasionRunStateComponent.cs
src2/WorldSession/Events/Dd2/Dd2InvasionWaveStateComponent.cs
src2/WorldSession/Events/Dd2/Dd2InvasionWavePolicy.cs
src2/WorldSession/Events/Dd2/Dd2InvasionArenaStateComponent.cs
src2/WorldSession/Events/Dd2/Dd2InvasionCrystalDropStateComponent.cs
src2/WorldSession/Events/Dd2/Dd2InvasionDeathPositionBuffer.cs
src2/WorldSession/Events/Dd2/Dd2InvasionProgressionSystem.cs
src2/WorldSession/Events/Dd2/Dd2InvasionWaveSystem.cs
src2/WorldSession/Events/Dd2/Dd2InvasionArenaSystem.cs
src2/WorldSession/Events/Dd2/Dd2InvasionCrystalDropSystem.cs
src2/WorldSession/Events/Dd2/Dd2InvasionWaveStatusQuery.cs
src2/WorldSession/Events/Dd2/Dd2InvasionArenaQuery.cs
src2/WorldSession/Events/Dd2/Dd2InvasionBuildingBlockQuery.cs
src2/WorldSession/Events/Dd2/Dd2InvasionBartenderReadinessQuery.cs
src2/WorldSession/Events/Dd2/Dd2InvasionNetworkProjection.cs
src2/WorldSession/Events/Dd2/Dd2InvasionPersistenceAdapter.cs
```

| Version4 member | proposed target role | proposed single writer or reader | execution boundary |
| --- | --- | --- | --- |
| `INFO_NEW_WAVE_COLOR` | `Dd2InvasionPresentationPolicy.NewWaveColor` | immutable policy read by message projection | New-wave color is presentation input, not run authority. |
| `INFO_START_INVASION_COLOR` | `Dd2InvasionPresentationPolicy.StartInvasionColor` | immutable policy read by message projection | Start color is presentation input; final ChatColors.World adapter remains integration-review. |
| `INFO_FAILURE_INVASION_COLOR` | `Dd2InvasionPresentationPolicy.FailureInvasionColor` | immutable policy read by message projection | Failure color is presentation input and cannot write `LostThisRun`. |
| `INVASION_ID` | `Dd2InvasionDefinition.InvasionId` | immutable definition read by event/packet adapter | Value `3` is an event/packet kind, never an EntityId, NetworkId or PersistentEntityId. |
| `DownedInvasionT1` | `Dd2InvasionProgressionStateComponent.DownedTier1` | proposed progression system writes; persistence adapter restores | Preserve this independent save/load field and world-reset behavior. |
| `DownedInvasionT2` | `Dd2InvasionProgressionStateComponent.DownedTier2` | proposed progression system writes; persistence adapter restores | Preserve this independent save/load field and world-reset behavior. |
| `DownedInvasionT3` | `Dd2InvasionProgressionStateComponent.DownedTier3` | proposed progression system writes; persistence adapter restores | Preserve this independent save/load field and world-reset behavior. |
| `LostThisRun` | `Dd2InvasionRunStateComponent.LostThisRun` | proposed run/lifecycle system writes | Failure stop writes it; presentation cannot mutate it. |
| `WonThisRun` | `Dd2InvasionRunStateComponent.WonThisRun` | proposed run/lifecycle system writes | Successful write remains partial because `WinInvasionInternal` is empty in Version4. |
| `Ongoing` | `Dd2InvasionRunStateComponent.Ongoing` | proposed run/lifecycle system writes | Start/stop is the sole write boundary; spawn and arena queries read it. |
| `OngoingDifficulty` | `Dd2InvasionRunStateComponent.Difficulty` | proposed run/lifecycle system writes | Start path sets it; wave/spawn queries read it. |
| `LaneSpawnRate` | `Dd2InvasionWavePolicy.LaneSpawnRate` | proposed wave system consumes; NPC spawn query reads | Preserve the default `60`; configuration persistence remains unresolved. |
| `ArenaHitbox` | `Dd2InvasionArenaStateComponent.ArenaHitbox` | proposed arena system writes; building query reads | Hitbox is a transient local snapshot, not a persistent entity position. |
| `_arenaHitboxingCooldown` | `Dd2InvasionArenaStateComponent.RefreshCooldown` | proposed arena system writes; building query reads | Cooldown is a refresh throttle; `ShouldBlockBuilding` cannot refresh it. |
| `_deadGoblinSpots` | `Dd2InvasionDeathPositionBuffer.Positions` | proposed death-position owner appends/consumes/clears | Temporary work buffer only; consumption boundary remains evidence-gap. |
| `_crystalsDropping_lastWave` | `Dd2InvasionCrystalDropStateComponent.LastWave` | proposed crystal-drop system writes | Last-wave marker participates in one atomic drop transaction. |
| `_crystalsDropping_toDrop` | `Dd2InvasionCrystalDropStateComponent.ToDrop` | proposed crystal-drop system writes | Pending quantity is read by NPC drop paths, never by presentation. |
| `_crystalsDropping_alreadyDropped` | `Dd2InvasionCrystalDropStateComponent.AlreadyDropped` | proposed crystal-drop system writes | Already-dropped count prevents duplicate NPC/波次 submission. |
| `_timeLeftUntilSpawningBegins` | `Dd2InvasionWaveStateComponent.TimeLeftUntilSpawningBegins` | proposed wave system writes and decrements | `TimeLeftBetweenWaves` is a command/query boundary over this field. |
| `ReadyToFindBartender` | `Dd2InvasionBartenderReadinessQuery.IsReady` | pure query reads `NPC.downedBoss2` | Query cannot mutate run or progression state. |
| `TimeLeftBetweenWaves` | `Dd2InvasionWaveStatusQuery.TimeLeftBetweenWaves` over wave state | query reads; setter maps to an explicit wave command | Do not expose arbitrary external field writes. |
| `EnemySpawningIsOnHold` | `Dd2InvasionWaveStatusQuery.EnemySpawningIsOnHold` | pure query reads wave state | Derived from nonzero remaining time; no second writable bool. |

### 19.2 Implementation order and single-write ownership

1. Restore the three persistent tier fields through a versioned adapter and make the progression
   system their sole writer; reset them only through the documented world reset command.
2. Commit run facts (`LostThisRun`, `WonThisRun`, `Ongoing`, `OngoingDifficulty`) through one DD2
   lifecycle writer. Keep colors and `INVASION_ID` in definition/presentation policy boundaries.
3. Commit wave pause time and lane policy through the wave system. Derive hold status in a pure query;
   preserve `AttemptToSkipWaitTime`, zero/nonzero hold and day-rate-zero cases.
4. Commit arena hitbox and refresh cooldown through the arena system. `ShouldBlockBuilding` and
   arena queries are read-only and do not refresh the cooldown.
5. Commit the three crystal counters atomically through the crystal-drop system. Preserve last-wave,
   to-drop and already-dropped idempotence across NPC drop calls.
6. Treat dead goblin positions as a transient buffer with explicit append, consume and clear owners;
   do not persist or register it as an entity component until its consumer is identified.
7. Add network projection only after the relevant authority is committed. The event definition must
   not be used as an entity identity.

The proposed schedule is: persistent progression/run commit -> wave pause reduction -> arena refresh
and read query -> NPC spawn/arena building query -> crystal-drop transaction -> presentation/network
projection. Directory order is not a runtime order.

### 19.3 Compatibility, snapshot, network, persistence and rollback

Only the three `DownedInvasionT*` members have direct Save/Load and world-reset evidence. No
independent persistence DTO is claimed for run flags, lane policy, arena hitbox/cooldown, dead spots,
crystal counters or wave pause. No network DTO is claimed for these fields; future output must be a
one-way projection of committed state.

`SetEnemySpawningOnHold` is empty in Version4. The proposed plan preserves that fact and does not
silently substitute complete-reference behavior. Event snapshots, NPC identity, NetworkId,
PersistentEntityId, Rectangle/coordinate values and scheduler ownership remain
`crossSubsystemOwner: integration-review`.

Rollback is required if tier round-trip/reset diverges, run facts can be written by a presentation
reader, a same-tick pause or skip changes behavior, arena cooldown is refreshed by a query, dead
positions leak across runs, crystal counters double-submit a drop, event ID is confused with an entity
identity, or complete-reference behavior replaces the Version4 empty hold method. Disable the
proposed writer/projection and restore the legacy read boundary; no source rollback is performed here.

### 19.4 Focused verifier and checkpoint status

Future verification must cover tier Save/Load and world reset; run start/failure/success flags; lane
rate and NPC consumption; wave pause decrement, zero/nonzero hold, skip-wait and day-rate-zero;
arena hitbox/cooldown and building-block read isolation; dead-position buffer lifecycle; crystal
counter atomicity and idempotence; bartender readiness and event-ID identity separation; projection
isolation; and Version4 empty `SetEnemySpawningOnHold` compatibility. All results are currently unrun.

implementationStatus: completed (Component-only subset)
verificationStatus: partial (serial build passed; focused verifier and behavior-equivalence not run)
productionCodeModified: no; src2CodeModified: yes

The Component source for this checkpoint is saved under `src2/WorldSession`. Registration, network,
persistence, compile, test and verifier work for the proposed non-Component boundary remains deferred.

## 20. Proposed schedule edge

This is a candidate contract, not a committed runtime schedule:

```text
resolve world clock/rules and external commands
  -> proposed SeasonalCalendarSystem policy/fact transition
  -> proposed CultistRitualSystem countdown/recheck reducer
  -> proposed CultistRitualEligibilityQuery (read-only Tile/LOS and NPC facts)
  -> proposed ICultistRitualNpcSpawnPort command for NPC type 437
  -> proposed SandstormSystem active/time/severity reducer
  -> proposed SandstormActiveQuery and committed weather/event snapshot
  -> world weather/sandstorm/player-camera inputs
  -> SceneMetrics reset/cache check
  -> Tile/liquid and player-effect scan ports
  -> proposed SceneWeatherEventZoneSnapshot commit
  -> proposed SceneWeatherEventZoneQuery
  -> proposed Player/NPC/Rain/SceneState/UI projections
  -> proposed BirthdayPartySystem and proposed LanternNightSystem event transition/reducer
  -> proposed MysticFairyLogScanWorkBuffer and proposed MysticFairyEligibilityQuery
  -> proposed MysticFairySpawnSystem and proposed ICelebrationNpcSpawnPort
  -> proposed InvasionLifecycleSystem transition/delay/movement
  -> proposed InvasionProgressSystem command validation and authority commit
  -> proposed InvasionSpawnQuery for spawn readers
  -> proposed InvasionWarningProjection and InvasionProgressProjection
  -> proposed Dd2InvasionProgressionSystem and Dd2InvasionRunStateComponent commit
  -> proposed Dd2InvasionWaveSystem pause reduction and wave status query
  -> proposed Dd2InvasionArenaSystem and building-block query
  -> proposed Dd2InvasionCrystalDropSystem and NPC drop transaction
  -> proposed Dd2InvasionDamageTrackingSystem session/outcome commit
  -> proposed Dd2InvasionDamageOutcomeProjection and DD2 network/persistence projections
  -> proposed CreditsRollSystem and proposed MoonlordDramaSystem client presentation update
  -> proposed ScreenObstructionSystem local smoothing
  -> proposed SpelunkerScanFrameBoundarySystem and proposed UnbreakableWallScanQuery
  -> proposed VoidLensPresentationSystem and read-only presentation ports
  -> proposed AmbientSpawnEligibilityQuery and proposed AmbientSpawnSystem
  -> proposed AmbientSpawnRequestBuffer consumption and AmbientSpawnNetworkProjection
  -> proposed AmbientWindScanQuery and AmbientWindSystem cadence/clear
  -> proposed AmbientWindPresentationAdapter and effect ports
  -> network/persistence/UI projections
```

The final schedule must explicitly handle same-tick start/stop, warning expiration, invasion movement,
NPC kill threshold, completion cleanup, world reset, pause/day-rate zero, save/load and duplicate
network commands. Directory or file order must never determine the order.

## 21. Rollback and failure conditions

Rollback is allowed only before deleting or changing the legacy write root. Disable the proposed
writer, discard its uncommitted projection, and restore the compatibility adapter to the last
known legacy read/write boundary. Roll back if any of the following occurs:

- a focused verifier finds different start/stop/cooldown or warning behavior;
- a second writer can mutate active/time/progress without a command;
- network or persistence payloads cannot distinguish stale state from a new event revision;
- NPC spawn eligibility observes an uncommitted or partially cleared cache;
- a failure after an external message leaves delivery/retry semantics unknown;
- current NLTX validation shows a component invariant that cannot represent Version4 negative-time
  cooldown without an integration decision.
- invasion progress or packet/UI projection can write authority state, or the moon-event `-1` sentinel
  is silently coerced into an ordinary non-negative invasion progress value.
- a celebration verifier finds different manual/genuine precedence, cooldown, next-night consumption,
  roster identity or morning/world-reset behavior;
- packet 7/111 handling mutates event authority from a projection, or a fairy spawn is duplicated,
  committed after a failed port call, or derived from a complete-reference method that is empty in
  Version4.
- a ritual recheck is persisted or reset without evidence, Tile/LOS eligibility bypasses the read-only
  boundary, NPC type `437` spawn is duplicated, or the ritual delay writer is split;
- Sandstorm duration is assigned, Start/Stop gains effects, severity leaves finite `0..1`, packet 7
  receive writes authority, or `<174` persistence defaults diverge from Version4.
- SceneMetrics cache misses on the same frame/center, stale values survive `Reset`, camera/player
  snapshots leak across instances, a projection writes authority, candle/Torch God input uses the
  wrong perspective player, or a complete-reference formula silently replaces a Version4 empty scan method.
- DD2 damage tracking creates a second session writer, duplicates inherited `NPCDamageTracker` state,
  emits the wrong `KillTimeMessage`, infers successful cleanup without evidence, or lets a projection
  mutate `Won`.
- DD2 wave/arena persistence, run flags, pause timer, arena cooldown, dead-position buffer or crystal
  counters diverge from the observed boundaries; a query refreshes state; the event ID is treated as an
  entity ID; or complete-reference behavior replaces Version4's empty `SetEnemySpawningOnHold`.

No destructive cleanup or source rollback is performed by this planning session.

## 22. Focused verifier and serial build plan

Proposed verifier coverage:

- pure state transition cases for start/stop, active positive time, negative cooldown and warning
  zero-crossing;
- NPC eligibility and kill progress command cases, including duplicate and stale commands;
- world reset/cache rebuild cases for `slimeRainNPC`;
- network/persistence projection cases that verify committed-state-only observation and version
  handling;
- ordering cases for same-tick start, kill, stop and projection;
- deterministic random input tests using an explicit random stream port.
- invasion start/movement/delay/completion, NPC progress command, packet 7/78 adapter and sentinel
  compatibility cases described in checkpoint 4.
- seasonal date/policy/day-start/reset, save version, world-info bit and title-effect cases described
  in checkpoint 5.
- credits start/tick/reset, packet 140 projection and subtype-0 receive boundary; Moonlord piece/explosion
  lifetime, light aggregation, whitening, asset cleanup; ScreenObstruction target/smoothing/reset and
  read-only projection cases described in checkpoint 6.
- BirthdayParty/LanternNight manual and natural transitions, cooldown/next-night/roster reset, packet
  7/111 projection and empty receive, plus Mystic Fairy Tile scan, delay, spawn-port and Version4-stub
  compatibility cases described in checkpoint 13.
- CultistRitual delay/recheck, Tile/LOS/type-437 eligibility, transient four-point workset, delay
  persistence and reset decision; Sandstorm active/time/severity reduction, finite severity invariant,
  empty Start/Stop compatibility, packet 7/world-info projections and `<174` persistence defaults as
  described in checkpoint 14.
- SceneWeatherEventZone same-frame/same-center cache, center/frame/perspective-player invalidation,
  separate camera/player metrics, Reset clearing of all nine fields, rain/sandstorm/depth/candle/Torch
  God boundaries, read-only projections and Version4 empty-method compatibility as described in checkpoint 15.
- DD2 damage tracker session creation/reset, failure and unresolved success stop, name/message projection,
  empty `IncludeDamageFor` compatibility, inherited-state isolation, and DD2 wave progression, tier
  persistence/reset, run flags, pause/skip timing, arena hitbox/cooldown, dead-position buffer,
  bartender readiness, crystal-drop idempotence and empty `SetEnemySpawningOnHold` compatibility as
  described in checkpoints 18 and 19.

Later implementation commands must be run from the repository root, serially, through:

```powershell
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\path\AffectedProject.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 test .\path\FocusedVerifier.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false
```

The focused verifier commands remain a future plan. The Component project build was executed after
the Handoff; its command and result are recorded below. No test output or independent behavior
verifier result exists for this Component-only task.

### 23.1 Actual Component verification

Command:

```powershell
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'build',
  '.\src2\WorldSession\Terraria.WorldSession.csproj',
  '-m:1',
  '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false'
)
```

Result: exit code `0`, `0` warnings, `0` errors. Artifact:
`D:\TRbackup\NLTX\Build\bin\Terraria.WorldSession\Debug\net10.0\Terraria.WorldSession.dll`.
The 34 names in `completedComponents` were independently matched to same-named source files under
`src2/WorldSession` with zero missing files and zero duplicate names. No focused verifier or test
code was created or run; network, persistence, registration, integration and behavior-equivalence
verification remain outside this Component-only task.

## 23. Checkpoint ledger for this pair of documents

| checkpoint | design status | execution status | implementation | verification | current/pending |
| --- | --- | --- | --- | --- | --- |
| `MainSlimeRainState` | proposed mapping written | completed (Component-only) | completed | partial (serial build) | completed; covered `8/225`; pendingGroups `16` |
| `MainCalendarWeatherState` | complete 16-member proposed mapping written | completed (Component-only) | completed | partial (serial build) | completed; covered `24/225`; pendingGroups `15` |
| `MainWeatherAndAmbientState` | complete 11-member proposed mapping written | completed (Component-only) | completed | partial (serial build) | completed; covered `35/225`; pendingGroups `14` |
| `MainInvasionState` | complete 12-member proposed mapping written | completed (Component-only) | completed | partial (serial build) | completed; covered `47/225`; pendingGroups `13` |
| `MainSeasonalAndTitleState` | complete 7-member proposed mapping written | completed (Component-only) | completed | partial (serial build) | completed; covered `54/225`; pendingGroups `12` |
| `SharedWorldEventPresentationState` | complete 23-member proposed mapping written | completed (Component-only) | completed | partial (serial build) | completed; covered `77/225`; pendingGroups `11` |
| `SharedInvasionAndBossTracking` | complete 14-member proposed mapping written | completed (Component-only) | completed | partial (serial build) | completed; covered `91/225`; pendingGroups `10` |
| `SharedLightningGenerationState` | complete 23-member proposed mapping written | completed (Component-only) | completed | partial (serial build) | completed; covered `114/225`; pendingGroups `9` |
| `SharedWaterfallState` | complete 11-member proposed mapping written | completed (Component-only) | completed | partial (serial build) | completed; covered `125/225`; pendingGroups `8` |
| `WorldEnvironmentScanHelpers` | complete 9-member proposed mapping written | completed (Component-only) | completed | partial (serial build) | completed; covered `134/225`; pendingGroups `7` |
| `SharedAmbientSkyCatalogState` | complete 17-member proposed mapping written | completed (Component-only) | completed | partial (serial build) | completed; covered `151/225`; pendingGroups `6` |
| `SharedAmbientSpawnAndWindState` | complete 11-member proposed mapping written | completed (Component-only) | completed | partial (serial build) | completed; covered `162/225`; pendingGroups `5` |
| `SharedCelebrationAndLanternEvents` | complete 16-member proposed mapping written | completed (Component-only) | completed | partial (serial build) | completed; covered `178/225`; pendingGroups `4` |
| `SharedRitualAndStormEvents` | complete 12-member proposed mapping written | completed (Component-only) | completed | partial (serial build) | completed; covered `190/225`; pendingGroups `3` |
| `SharedSceneWeatherAndEventZoneState` | complete 9-member proposed mapping written | completed (Component-only) | completed | partial (serial build) | completed; covered `199/225`; pendingGroups `2` |
| `SharedInvasionDamageTrackingState` | complete 4-member proposed mapping written | completed (Component-only) | completed | partial (serial build) | completed; covered `203/225`; pendingGroups `1` |
| `SharedInvasionWaveAndArenaState` | complete 22-member proposed mapping written | completed (Component-only) | completed | partial (serial build) | completed; covered `225/225`; pendingGroups `0` |

All seventeen checkpoints are complete: the final checkpoint is
`SharedInvasionWaveAndArenaState`. Both documents keep
`completedComponents`, `currentComponent`, `pendingComponents`, `lastCheckpointUtc`,
`evidence-gap` and `blocking-decision` synchronized. The 34 independently expressible Components
are saved under `src2/WorldSession`; the two deferred Components and all non-Component roles remain
unimplemented. The 225-member input report has full proposed coverage; this does not claim
production integration or behavior equivalence.

## 24. Integration Handoff

subsystemId: proposed WorldEnvironmentAndEventsNonAuthoritative
taskNumber: second-round-component-plan-P02
reportPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P02-world-environment-events-component-execution.md

evidenceStatus: partial; all seventeen P02 checkpoints are written through SharedInvasionWaveAndArenaState and all 225 source members have proposed mappings; unresolved evidence gaps remain
nltxStatus: partial
verificationStatus: partial (serial build passed; focused verifier and behavior-equivalence not run)

confirmedOwners:
- Version4 `Main` is a legacy storage host, not by itself proof of one ECS owner.
- For the first checkpoint, `StartSlimeRain`/`StopSlimeRain` and `UpdateSlimeRainWarning` are the direct transition/countdown roots.
- `StartInvasion` initializes the direct Version4 invasion state; `UpdateInvasion` moves and completes it, while NPC death handling directly decrements remaining size and reports progress.
- `SyncAnInvasion`/`ReportInvasionProgress` are direct progress presentation/projection roots; packet 78 receive, persistence and final progress authority remain open.
- `checkXMas`/`checkHalloween` reduce date and policy inputs into active seasonal facts; their source flags are not evidence of an ECS owner.
- `changeTheTitle` is consumed once in the main loop, but its direct writer and platform owner remain open.
- `CreditsRollEvent` owns the direct credits countdown start/tick/reset operations and packet 140 emission; subtype-0 receive ownership remains open.
- `MoonlordDeathDrama.RequestLight` owns per-frame light-request accumulation and its update owns clearing/whitening smoothing; object creation and draw ownership remain open.
- `ScreenObstruction.Update` owns direct writes to the obstruction smoothing fields; external caller, reset and render ownership remain open.
- `BannerSystem.AddNPCKillBy` owns the direct Banner kill/threshold transition; `Clear`, `Save`/`Load` and outbound NetBannersModule writers define the observed reset, persistence and packet boundaries. Receive and claim consumption remain open.
- `NPCDamageTracker` owns tracker creation/update/reset calls; `BossDamageTracker` owns its subclass definition fields, `_killed` transition and localized name/message derivation. Base credit entry ownership remains open.
- `Main` creates InvasionDamageTracker for ordinary, Pumpkin Moon and Frost Moon groups. Version4's IncludeDamageFor stub and empty CheckActive are evidence mismatches; the complete reference is supplementary only.
- `StormLightning.GenerateMainBoltPath` is the direct lightning generation entry and `Projectile` consumes its Bolt result; recursive geometry/rotation methods are Version4 stubs, so the algorithm remains an evidence gap.
- `Main` owns the observed `WaterfallManager` reference and constructs it during initialization; `WaterfallManager.BindTo` only registers the Preferences load callback. No direct Version4 waterfall slot writer, clear/update loop, draw caller or texture-load/release owner was found in the inspected evidence.
- `SpelunkerProjectileHelper` owns the observed transient position/Tile de-duplication sets, clamp reconstruction and ten-frame reset boundary; its `CheckSpot` body is empty, so the scan result writer remains open.
- `UnbreakableWallScan` owns the observed 250-step and eight-direction query policy; `LineScan`/`InsideUnbreakableWalls` read Tile facts, while Player cooldown/result commit and empty network methods remain outside this checkpoint.
- `VoidLensHelper` owns one-shot position, opacity and frame inputs used by its update path; lighting, random, Dust and final draw/resource consumers remain explicit effect-port boundaries.
- `TreeTopsInfo` owns the observed 13 stable area IDs, variation array, save/load version-211 boundary and outbound 13-byte sync payload; `RandomizeTreeStyle` and its world-position variant method are direct variation-change roots, while packet 7 receive remains open.
- `BackgroundChangeFlashInfo.UpdateFlashValues` owns the observed per-update `0.05` clamp decay; `UpdateVariation` is empty in Version4, and no save/network owner or confirmed background-flash consumer was found.
- `AmbienceServer` owns the observed ambient spawn policy registries, retry timer and forced-request queue; `AmbienceSpawnInfo` separates `SkyEntityType` from the Player-slot target, while final visibility and request ownership remain open.
- `AmbientWindSystem.Update` owns the observed local/graveyard-gated cadence; Version4's scan/workspace/spawn methods are empty, so complete-reference Tile/random/Gore behavior remains supplementary only.
- `BirthdayParty` owns the observed manual/genuine party facts, cooldown, celebrating-NPC slot roster and active-edge cache; `PartyIsUp` is a derived `GenuineParty || ManualParty` query.
- `LanternNight` owns the observed manual/genuine lantern facts, next-night request, cooldown and active-edge cache; `LanternsUp` is a derived `GenuineLanterns || ManualLanterns` query.
- `MysticLogFairiesEvent` owns the observed night eligibility flag, retry delay and stump-coordinate scan workset; `NPC.Spawner.fairyLog` and Version4-empty fairy qualification/spawn/coordinate methods remain shared or incomplete boundaries.
- `CultistRitual` owns the observed delay/recheck countdown fields and ritual eligibility entry points; `delay` has WorldFile save/restore evidence, `recheck` has no persistence evidence, and Tile/LOS/type `437` NPC creation remain integration-review transaction boundaries.
- `Sandstorm` owns the observed active/time/severity/target-severity fields; WorldFile saves/restores all four, packet 7 writes `bitsByte10[3]`, world-info writes the target-severity float, packet 7 receive is empty, and Version4 `StartSandstorm`/`StopSandstorm` are empty stubs.
- `SceneMetrics.Scan` owns the observed frame/center cache gate and scan ordering; `SceneMetrics.Reset` owns invalidation and clearing of the nine scene fields. The Version4 Tile scan, aggregation, zone calculation, NPC scan and player-effect methods are empty, so the direct field writer remains an evidence gap.
- `DD2Event.DamageTracker` exposes the DD2 name and win-dependent kill-time message; `Stop(bool won)` writes the local outcome before delegating to the base tracker, while Version4 `IncludeDamageFor` returns `false` and successful tracker cleanup remains incomplete.
- `DD2Event.StartInvasion` creates/registers the DD2 damage tracker, failure calls `Stop(false)`, and world reset invokes the tracker reset path; the proposed session does not duplicate inherited `NPCDamageTracker` state.
- `DD2Event` owns the observed DD2 progression save/load/reset, wave pause timer, arena hitbox/cooldown, crystal-drop counters and derived readiness/hold properties; `SetEnemySpawningOnHold` is empty in Version4 and remains a compatibility boundary.

proposedTypes:
- proposed `SlimeRainStateComponent`, `SlimeRainPolicyComponent`, `SlimeRainProgressComponent`
- proposed `SlimeRainSystem`, `SlimeRainWarningSystem`, `SlimeRainEligibilityQuery`
- proposed network/persistence/UI projections and external adapters
- proposed calendar/weather components and transition/projection systems described in checkpoint 2
- proposed ambient wind/cloud/sky components, policy and presentation adapters described in checkpoint 3
- proposed `WorldInvasionStateComponent`, `InvasionProgressPresentationStateComponent`, lifecycle/progress/spawn query and warning/progress/network/persistence adapters described in checkpoint 4
- proposed seasonal fact/policy/title components, reducer, override command, secret-seed/save/world-info adapters and title projection described in checkpoint 5
- proposed credits policy/state/system, packet adapters/projections and Sky adapter described in checkpoint 6
- proposed Moonlord piece/explosion presentation adapters, lifetime queries, light buffer, drama system and projection described in checkpoint 6
- proposed ScreenObstruction presentation state, target query, system and projection described in checkpoint 6
- proposed Banner catalog/progress/notification components, progress system, persistence adapter and network projection/adapter described in checkpoint 7
- proposed Boss tracking definition/outcome, name/message projections and lifecycle adapter described in checkpoint 7
- proposed Invasion catalog/session, name/message projections, eligibility query and lifetime system described in checkpoint 7
- proposed Lightning generation policy, Bolt payload/result, generation port, generation system and projection adapter described in checkpoint 8
- proposed `WaterfallGenerationPolicy`, `WaterfallCapacityPolicy`, `WaterfallSlotPayload`, `WaterfallSlotBuffer`, `WaterfallTextureCatalog`, `WaterfallGenerationQuery`, `WaterfallGenerationSystem`, `WaterfallConfigurationAdapter` and `WaterfallPresentationAdapter` described in checkpoint 9
- proposed `SpelunkerScanPolicy`, `SpelunkerScanWorkBuffer`, `SpelunkerScanFrameBoundarySystem`, `SpelunkerScanSystem`, `SpelunkerSpotQuery`, `UnbreakableWallScanPolicy`, `UnbreakableWallScanQuery`, `IUnbreakableWallTileReadPort`, `VoidLensPresentationPayload`, `VoidLensPresentationSystem`, `VoidLensProjectionAdapter` and effect ports described in checkpoint 10
- proposed `TreeTopAreaCatalog`, `TreeTopVariationStateComponent`, `TreeTopVariationSystem`, `TreeTopVariationQuery`, `TreeTopPersistenceAdapter` and `TreeTopNetworkProjection`
- proposed `BackgroundChangeFlashStateComponent`, `BackgroundChangeFlashSystem` and `BackgroundPresentationPort`
- proposed `AmbientSpawnRequest`, `AmbientSpawnIntervalPolicy`, `AmbientSpawnPolicyCatalog`, `AmbientSpawnScheduleStateComponent`, `AmbientSpawnEligibilityQuery`, `AmbientSpawnSystem`, `AmbientSpawnRequestBuffer` and `AmbientSpawnNetworkProjection`
- proposed `AmbientWindWorkBuffer`, `AmbientWindScanQuery`, `AmbientWindSystem`, `IAmbientWindRandomPort`, `IAmbientWindTileReadPort`, `IAmbientWindPresentationPort` and `AmbientWindPresentationAdapter`
- proposed `BirthdayPartyStateComponent`, `BirthdayPartyRosterBuffer`, `BirthdayPartyPolicyCatalog`, `BirthdayPartyActiveQuery`, `BirthdayPartySystem`, `BirthdayPartyProjection`, transition cache and celebration ports described in checkpoint 13
- proposed `LanternNightStateComponent`, `LanternNightPolicyCatalog`, `LanternNightActiveQuery`, `LanternNightSystem`, `LanternNightProjection` and transition cache described in checkpoint 13
- proposed `MysticFairyEventStateComponent`, `MysticFairySpawnPolicy`, `MysticFairyLogScanWorkBuffer`, `MysticFairyEligibilityQuery`, `MysticFairySpawnSystem`, `ICelebrationTileReadPort`, `ICelebrationNpcSpawnPort` and `ICelebrationPresentationPort` described in checkpoint 13
- proposed `CultistRitualStateComponent`, `CultistRitualPolicy`, `CultistRitualEligibilityQuery`, `CultistRitualSystem`, `ICultistRitualTileReadPort` and `ICultistRitualNpcSpawnPort` described in checkpoint 14
- proposed `SandstormStateComponent`, `SandstormPolicy`, `SandstormSystem`, `SandstormActiveQuery`, `SandstormNetworkProjection` and `SandstormPersistenceAdapter` described in checkpoint 14
- proposed `SceneWeatherEventZoneSnapshot`, `SceneWeatherEventZoneScanPolicy`, `SceneWeatherEventZoneScanSystem`, `SceneWeatherEventZoneQuery`, `ISceneMetricsTileReadPort`, `ISceneMetricsPlayerEffectPort` and `SceneWeatherEventZoneProjection` described in checkpoint 15
- proposed `Dd2InvasionDamageTrackingSession`, `Dd2InvasionDamageTrackingSystem`, `Dd2InvasionDamageTrackingNameQuery`, `Dd2InvasionDamageOutcomeProjection` and `IDd2InvasionDamageTrackerPort` described in checkpoint 18
- proposed `Dd2InvasionDefinition`, `Dd2InvasionPresentationPolicy`, `Dd2InvasionProgressionStateComponent`, `Dd2InvasionRunStateComponent`, `Dd2InvasionWaveStateComponent`, `Dd2InvasionWavePolicy`, `Dd2InvasionArenaStateComponent`, `Dd2InvasionCrystalDropStateComponent`, `Dd2InvasionDeathPositionBuffer`, DD2 progression/wave/arena/crystal systems, read-only status queries, network projection and persistence adapter described in checkpoint 19

sharedTypesForIntegrationReview:
- world clock/rules, random stream, event snapshot, NPC identity, network ID and persistence ID

crossSubsystemReaders:
- NPC/spawn, calendar/weather, player readiness, network/session and client presentation
- WorldGen coin-rain/weather consumers and visual effects
- NPC/Player/Projectile wind consumers and Cloud/Star client object lifecycle
- invasion NPC spawn/progress, warning and packet 78 consumers
- NPC/Item/WorldGen seasonal readers, secret-seed initialization, world-file/world-info adapters and title effect consumer
- NPC credits trigger, packet 140/session join path, CreditsRollSky and client death-drama presentation
- client scene obstruction consumers and `DangerousDungeonCurse`/player environment inputs
- NPC death/banner mapping, SceneMetrics banner capacity, Banner UI/claim consumers and joining-player network session
- NPC damage/kill/update/reset paths, Boss registration and localization/message consumers
- ordinary/seasonal invasion start, NPC invasion-group lookup, invasion progress/UI readers and localized tracking output
- storm lightning seed/target callers, Projectile presentation and Tile collision/query boundary
- Tile/liquid/settled-world inputs, waterfall slot generation and client waterfall texture/presentation boundary
- Projectile/world-position scan inputs, Player wall-scan result boundary, Tile reads and client lighting/Dust/presentation consumers
- tree-top area/variation readers, world-generation Tile region mapping, world-file loading and packet 7 projection/receive consumers
- background-flash presentation consumers and any client resource lifecycle that is confirmed during integration
- visible-player/weather/event eligibility readers, NPC/Projectile ambient consumers, NetAmbience session projection, and AmbientWind Tile/Gore effect consumers
- NPC party eligibility and roster consumers, packet 7/world-info consumers, Lantern Night event consumers, Mystic Log Tile scans and fairy/NPC spawn consumers
- ritual countdown/eligibility readers, Tile/LOS collision inputs, NPC type `437` spawn consumers, WorldFile delay adapter and world-reset integration
- weather/wind queries, Sandstorm visual consumers, packet 7/world-info consumers, WorldFile persistence and the empty packet 7 receive boundary
- Main camera and Player `SceneMetrics.Scan` callers, Player zone accessors, NPC spawn readers, Rain/Sandstorm visual readers, SceneState visual consumers, Tile/liquid/candle inputs and Torch God player-effect inputs
- DD2 NPC damage/kill/update/reset callers, damage-tracker session consumers, DD2 progression/NPC spawn readers, arena building checks, wave timing, bartender readiness, crystal-drop NPC paths and WorldFile progress adapters

crossSubsystemWriters:
- calendar command path, NPC kill/progress path and external network command adapters
- rain/coin-rain lifecycle and world-reset path
- weather randomization, wind timer and client presentation lifecycle
- invasion start/complete command path, NPC kill progress path and warning/progress projection path
- seasonal policy command, day-start reduction, secret-seed/recovery path, world reset and title refresh effect path
- credits NPC trigger, world tick/reset, packet 140 projection/receive and Sky lifecycle
- Moonlord light-request callers, object creation/update/cleanup/draw and asset lifecycle; ScreenObstruction smoothing lifecycle
- NPC death, Banner threshold/notification commit, world reset, save/load and outbound packet projection
- NPC damage/kill/update/reset lifecycle, Boss definition registration and localization/message projection
- invasion start/stop facts, NPC group filter/lifetime boundary and Invasion tracking projection
- lightning generation command/port, explicit random source, Tile collision input and Projectile/presentation adapter
- waterfall generation/configuration path, scene/world reset path and client texture/presentation adapter; direct Version4 writer remains unconfirmed
- Spelunker scan requests and frame reset, explicit UnbreakableWall Tile reader and Player result command, and Void Lens lighting/random/Dust/presentation ports
- tree-top variation commands/world-generation coordinate queries, save/load adapters and packet 7 projection/receive path; background variation refresh and flash presentation tick
- AmbienceServer schedule/forced-request paths, meteor-triggered request submission, NetAmbience outbound projection, AmbientWind local update/graveyard gate, Tile/random/Gore adapters and world-reset buffer clearing
- BirthdayParty and LanternNight manual commands, day/night transitions, NPC event-cleared input, packet 7 projection/empty receive boundary and world-clear reset; MysticLogFairiesEvent Tile scan, `NPC.Spawner.fairyLog` integration and NPC spawn/presentation ports
- ritual tablet/NPC event paths, day-rate clock, Tile/LOS readers and NPC spawn command path; `CultistRitual.delay` persistence writer and unresolved recheck/reset boundary
- Sandstorm wind/random/start-stop path, world clear, packet 7/world-info projection, WorldFile save/load and unresolved duration assignment
- Main camera and Player metrics callers, proposed scene scan/reset system, Tile/liquid/player-effect ports and read-only Player/NPC/Rain/SceneState/UI projections; no Version4 direct zone writer is closed
- DD2 damage-session system, NPC tracker port, progression/run/wave/arena/crystal-drop writers, death-position buffer, NPC drop and building queries, event/packet projection and persistent tier adapter

orderingConstraints:
- calendar transition before day/night-gated event rolls; weather commit before weather queries; coin-rain commit before projection
- wind commit before wind-dependent queries; cloud commit before packet projection; presentation objects after committed state
- authoritative transition before eligibility and projections; progress commit before threshold/stop effects; cache clear at world reset
- invasion authority transition before spawn query and projections; validated NPC progress before completion cleanup; cache/snapshot reset at world reset
- seasonal policy commit before active-fact reduction; daily consume/reset before seasonal readers; title request commit before one-shot platform effect
- credits start/reset before packet projection and Sky consumption; per-tick countdown before presentation projection
- Moonlord request collection before drama update, proximity check before light-buffer clear, drama update before draw, lifetime query before removal
- scene metrics/player input before obstruction target query, target before smoothing commit, projection after local state commit
- Banner catalog/load before NPC death commit; Banner state commit before save or packet projection; notification consume only through an explicit command
- Boss definition registration before tracker creation; damage/kill outcome before active/recent cleanup; localized projection after outcome commit
- Invasion facts and NPC group lookup before damage filtering; Version4 stub compatibility decision before adding complete-reference filter/stop behavior; reset tracker sessions with world reset
- DD2 persistent tier and run facts before wave/spawn queries; wave pause before hold/readiness queries; arena commit before building checks; crystal-drop counters before NPC drop projection; damage session registration before NPC damage and one outcome commit before KillTimeMessage projection. Preserve Version4 empty `IncludeDamageFor`, `WinInvasionInternal` and `SetEnemySpawningOnHold` boundaries.
- Resolve lightning policy and seed/random input before generation; complete Bolt payload before Projectile projection; keep Tile collision read-only and discard payloads after presentation
- Resolve waterfall policy and Tile/liquid inputs before slot generation; commit the complete temporary slot buffer before presentation; draw only from read-only payloads and clear/reuse the buffer before the next scene pass
- Rebuild Spelunker bounds and apply the ten-frame reset before scan requests; run wall queries against read-only Tile input before Player result commit; consume Void Lens payloads through effect ports after Projectile/world-position input and before payload disposal
- Resolve the immutable tree-top catalog before variation queries; commit all 13 variation values before save/network projection; decay background flash only after an explicit presentation refresh and never write the tree-top authority from the flash cache
- Commit weather/calendar/event and visible-player facts before ambient eligibility; consume the ambient schedule and forced-request buffer through one request owner before NetAmbience projection; run the local AmbientWind gate and read-only Tile/random query before presentation, then clear transient wind work at its cadence and reset boundaries
- Commit BirthdayParty/LanternNight manual and natural facts before active queries and packet 7 projection; consume next-night requests and cooldowns in the single night reducer; clear transition caches and fairy scan worksets at their documented morning/world-reset boundaries. Run Mystic Fairy qualification only after read-only Tile scan and visible-player/event facts, and submit NPC creation through the spawn port after the proposed event state commit
- Run the ritual countdown/recheck reducer after world clock/rules, then the read-only Tile/LOS eligibility query and NPC type `437` spawn command; run the Sandstorm active/time/severity reducer after committed weather/wind input and before packet 7/world-info projection. Keep Sandstorm world clear explicit and leave CultistRitual reset unresolved until integration review.
- Run world weather/sandstorm/player-camera inputs through the SceneMetrics reset/cache check, then Tile/liquid and player-effect scan ports, commit the SceneWeather snapshot, expose the read-only query, and only then project to Player/NPC/Rain/SceneState/UI. Preserve same-frame/same-center cache hits and separate camera/player metrics.

boundaryChallenges:
- direct writer/receiver for visual time and sun/moon offsets; packet/save closure; coin-rain owner
- Cloud/Star lifecycle, weather random stream and wind-physics policy owner
- final owner of NPC eligibility cache, negative cooldown representation and event progress commit root
- final owner of invasion remaining-size writer, progress `-1` sentinel, NPCDamageTracker lifecycle and packet/persistence adapters
- final owner of seasonal active/policy state, PendingWorldEventStateComponent overlap, secret-seed policy, packet/save DTOs, title effect and date/time-zone port
- final owner of credits packet subtype-0 receive and Sky activation, Moonlord object/asset/reset/draw lifecycle, SceneState whitening consumer, and ScreenObstruction render/reset boundary
- Banner claim consumption, notification clear, packet full-state receive and stale/duplicate policy; BannerId/NPC type/item ID separation
- NPCDamageTracker credit/session owner, Boss recent-retention integration and localization adapter boundary
- Version4 InvasionDamageTracker stub compatibility versus complete-reference group-filter/active-stop behavior
- Lightning seed/random ownership, coordinate values, Tile collision, Bolt payload lifetime and Version4 algorithm-stub policy
- Waterfall tile coordinate/type IDs, liquid or wetness input, slot-buffer clear/reuse owner, effective capacity configuration, texture resource lifecycle, and the missing Version4 writer/draw call graph
- Spelunker result ownership and Tile reader, scan-buffer field writers and reset timing, UnbreakableWall Player/network result boundary, and Void Lens coordinate/random/lighting/Dust/presentation ownership
- Tree-top packet 7 receive and stale/duplicate policy, save version 211 adapter boundary, old-version fallback, area-specific random input and background-flash trigger/consumer/resource lifetime remain integration-review boundaries
- Define AmbientSpawn `targetPlayer=-1` handling, Player-slot versus `SkyEntityType`/`NetworkId`/`PersistentEntityId`, forced-request at-most-once or retry policy, AmbienceServer random/eligibility ownership, NetAmbience empty receive behavior, AmbientWind stub compatibility, Tile/random/Gore ports and work-buffer reset ownership.
- Define BirthdayParty/LanternNight manual-versus-genuine authority, cooldown and next-night persistence, `CelebratingNPCs` NPC-slot to stable `NpcEntityId` resolution, packet 7/111 permission and duplicate semantics, and Mystic Fairy `NPC.Spawner.fairyLog`, Tile scan and NPC spawn transaction ownership; all remain `crossSubsystemOwner: integration-review`.
- Define CultistRitual `delay` versus transient `recheck` recovery/reset, `delayStart`/`timePerCultist` usage, Tile/LOS snapshot and NPC type `437` spawn transaction ownership; all remain `crossSubsystemOwner: integration-review`.
- Define Sandstorm duration assignment, wind/random owner, empty Start/Stop compatibility, four-field authority, packet 7/world-info receive semantics and world-reset scope; all remain `crossSubsystemOwner: integration-review`.
- Define the sole writer and freshness contract for the nine SceneMetrics fields, separate camera/player snapshot identity, TileCenter/world-coordinate value objects, frame invalidation, and the boundary between Version4 empty scan methods and supplementary complete-reference formulas; all remain `crossSubsystemOwner: integration-review`.
- Define DD2 damage-session identity and inherited tracker ownership, successful `WinInvasionInternal` cleanup, `IncludeDamageFor` compatibility, persistent tier DTOs, run/wave/arena/crystal authority, dead-position buffer consumption, event-kind versus entity identity and the empty `SetEnemySpawningOnHold` policy; all remain `crossSubsystemOwner: integration-review`.

evidenceGaps:
- persistence/network semantics for all checkpoints; visual offset and Cloud/Star lifecycle; full content registration and NPC lifecycle
- MainInvasionState packet 7/78 decode, save/load recovery, `invasionProgressAlpha` writer, NPC progress commit, NPCDamageTracker lifecycle and moon-event `-1` compatibility remain open
- MainSeasonalAndTitleState `changeTheTitle` writer, title platform adapter, packet-7 receive, date/time-zone semantics, version compatibility and PendingWorldEventStateComponent overlap remain open
- SharedWorldEventPresentationState packet 140 subtype-0 receive, CreditsRollSky activation, Moonlord object creation/add/draw/update callers, asset lifetime, reset paths, ScreenObstruction caller/consumer and client DTO semantics remain open
- SharedInvasionAndBossTracking Banner receive/claim semantics, Boss credit/session lifecycle, Invasion group filter/active-stop lifecycle and Version4 stub-versus-complete-reference behavior remain open; no Boss/Invasion save/network DTO is claimed
- SharedLightningGenerationState recursive geometry, rotation smoothing, Tile collision writer, random seed source, payload cleanup and Projectile/client consumption remain open; no lightning save/network DTO is claimed
- SharedWaterfallState slot writer, clear/reuse/update/draw caller, Preferences key, waterfall type catalog, `stopAtStep` semantics, texture load/release lifecycle and world-reset boundary remain open; no waterfall save/network DTO is claimed
- WorldEnvironmentScanHelpers Spelunker `CheckSpot` result writer and Tile reader, buffer reset/clear ownership, UnbreakableWall Tile adapter and Player/network result semantics, and Void Lens draw/Dust `customData` lifetime remain open; no scan cache, policy or Void Lens save/network DTO is claimed
- SharedAmbientSkyCatalogState `BackgroundChangeFlashInfo.UpdateVariation`, flash-power trigger/consumer, `TreeTopsInfo` old-version `CopyExistingWorldInfo`, packet 7 receive and region-random-input boundaries remain open; no background-flash save/network DTO is claimed
- SharedAmbientSpawnAndWindState `AmbienceServer` forced/ordinary request eligibility, target-player policy, NetAmbience receive, AmbientWind Tile/random/Gore effects, Version4 empty-method compatibility and transient reset boundaries remain open; no ambient spawn/wind save DTO is claimed
- SharedCelebrationAndLanternEvents Version4 `BirthdayParty`/`LanternNight` packet receive and command permission semantics, roster slot lifetime, natural-attempt conditions, `MysticLogFairiesEvent` qualification/spawn/coordinate stubs, `NPC.Spawner.fairyLog` shared writer, Tile scan workset and transient reset boundaries remain open; no celebration/fairy save DTO is claimed
- SharedRitualAndStormEvents `recheck` persistence/reset, delayStart/timePerCultist call graph, Tile/LOS/type `437` spawn transaction, Sandstorm duration/start-stop stubs, wind/random owner, packet 7 receive and world-reset boundaries remain open; no behavior-equivalence claim is made
- SharedSceneWeatherAndEventZoneState direct writers for Tile aggregation, zone formulas, NPC-position scan and player effects are missing in Version4; no independent save/network DTO or complete camera/player projection owner is claimed
- SharedInvasionDamageTrackingState has no independent save/network DTO evidence; successful tracker stop, inherited tracker ownership, empty `IncludeDamageFor` behavior and UI/message projection remain open; current NLTX has no evidence of completed Version4 behavior equivalence
- SharedInvasionWaveAndArenaState has persistence evidence only for the three downed-tier fields; run flags, lane policy, arena/cooldown, death-position buffer, crystal-drop counters, pause timer and empty hold behavior remain open; current NLTX has no evidence of completed Version4 behavior equivalence

blockingDecisions:
- crossSubsystemOwner: integration-review for shared calendar/weather snapshots, wind/cloud/sky presentation, packet/save DTOs, coin-rain, invasion authority/progress ownership, NPCDamageTracker and Boss/Invasion tracker sessions, BannerId/NPC type/item ID boundaries, Banner receive/claim semantics, seasonal fact/policy/title ownership, event IDs/snapshots and NPC eligibility/progress ownership; Lightning seed/random, coordinate values, Tile collision, Bolt payload lifetime and Projectile presentation; credits packet receive, Moonlord client object/asset lifecycle, SceneState whitening and ScreenObstruction render/reset boundaries; Waterfall tile/liquid inputs, slot-buffer lifetime, capacity configuration, texture resource port and the missing writer/draw call graph; Spelunker result/Tile-reader ownership, scan-buffer reset boundary, UnbreakableWall Player/network result and wall directory boundary, Void Lens coordinate values and random/lighting/Dust/presentation ports. The Version4 InvasionDamageTracker and LightningGenerator stub policies must be explicit before behavior-changing implementation.
- Tree-top area/variation authority, packet 7 receive and stale/duplicate policy, save version 211 adapter semantics, old-version fallback, region-specific random input, and BackgroundChangeFlash trigger/consumer/flashPower ownership remain `crossSubsystemOwner: integration-review`.
- AmbientSpawn `targetPlayer=-1`, Player slot and SkyEntityType/NetworkId/PersistentEntityId distinctions, forced-request consumption, random/eligibility ownership, NetAmbience empty receive, AmbientWind Version4 stub compatibility, Tile/random/Gore ports and transient work-buffer reset remain `crossSubsystemOwner: integration-review`.
- BirthdayParty/LanternNight manual/genuine/next-night/cooldown ownership, `CelebratingNPCs` slot-to-entity resolution, packet 7/111 receive and duplicate policy, Mystic Fairy qualification and coordinate behavior, `NPC.Spawner.fairyLog` shared ownership, Tile scan workset and NPC spawn command boundary remain `crossSubsystemOwner: integration-review`; complete-reference behavior cannot silently replace Version4 empty methods.
- CultistRitual `delay`/`recheck` authority and persistence, `delayStart`/`timePerCultist` usage, Tile/LOS/type `437` spawn transaction, and reset ordering remain `crossSubsystemOwner: integration-review`.
- Sandstorm four-field authority, duration assignment, wind/random ports, empty Start/Stop compatibility, packet 7/world-info receive and world-reset scope remain `crossSubsystemOwner: integration-review`; complete-reference behavior cannot silently replace Version4 empty methods.
- `SceneWeatherEventZoneSnapshot` freshness/version, TileCenter and world-coordinate value objects, camera/player identity, SceneMetrics reset/cache ownership, the nine-field direct writer, Tile/player-effect ports and read-only projections remain `crossSubsystemOwner: integration-review`; complete-reference formulas cannot silently replace Version4 empty scan methods.
- DD2 damage-session identity, inherited `NPCDamageTracker` fields, successful `WinInvasionInternal` cleanup, `IncludeDamageFor` stub compatibility, wave/run authority, arena rectangle and cooldown, death-position consumption, crystal-drop transaction, persistent tier DTOs, event-kind versus entity identity and empty `SetEnemySpawningOnHold` behavior remain `crossSubsystemOwner: integration-review`.

notImplemented:
- no non-Component C# migration, tests, registration, network changes, persistence changes or behavior-equivalence verification; the Component-only source build was executed and passed

## 25. Explicit execution declaration

This handoff continuation records a completed src2-only Component checkpoint. No production `src`
source, test code, registration, network, persistence, non-Component role or behavior-equivalence
code was modified. Compile verification is recorded below after the serial project build; focused
tests and cross-partition integration remain outside this Component-only task.

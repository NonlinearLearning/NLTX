# Version4 P15 Projectile Component Execution Plan

```yaml
partitionId: P15
sessionId: d27d18b95dab4abd9b181453fe1f4916
inputReport: D:\\TRbackup\\NLTX\\docs\\组件文档\\迁移参考表\\Version4权威模拟系统字段属性逐成员源码声明-20分区\\P15-Projectile.md
componentDesignPath: D:\\TRbackup\\NLTX\\docs\\组件文档\\第二轮审查\\2026-09-11-version4-P15-projectile-component-design.md
componentExecutionPath: D:\\TRbackup\\NLTX\\docs\\组件文档\\第二轮审查\\2026-09-11-version4-P15-projectile-component-execution.md
executionStatus: in-progress
implementationStatus: partial
designStatus: proposed
evidenceStatus: partial
currentNltxStatus: partial
verificationStatus: partial
designCompletedComponents: [ProjectileIdentityAndClassificationState, ProjectileAiState, ProjectileLifetimeAndRuntimeState, ProjectileCombatAndImmunity, ProjectileMovementAndCollisionState, ProjectileNetworkReplicationState, ProjectileMinionAndPresentationState, ProjectileDamageAndElementState, ProjectileAnimationAndDirectionState, ProjectileCollisionAndTargetingState, ProjectileCombatScalingState, ProjectileCollisionGeometryCache, ProjectileTargetSelectionCache, ProjectileFishingAndMiningQueryState, ProjectileKiteAndLightningRules, ProjectileDerivedProperties, ProjectileStormDefinition]
completedComponents: [ProjectileStormDefinition, ProjectileAiState, ProjectileLifetimeAndRuntimeState, ProjectileDispositionStateComponent, ProjectileTrailCacheComponent, ProjectileCollisionPolicyComponent, ProjectileNetworkStateComponent, ProjectileAnimationStateComponent, ProjectilePresentationStateComponent, ProjectileMinionCapabilityComponent, ProjectileBobberCapabilityComponent, ProjectileCounterweightCapabilityComponent, ProjectileSentryCapabilityComponent, ProjectileTrapCapabilityComponent, ProjectileDamagePolicyComponent, ProjectileDefinitionReferenceComponent, ProjectileBehaviorStateComponent, ProjectileLifetimeStateComponent, ProjectileDamagePayloadComponent, ProjectileGeometryStateComponent, ProjectileSourceMetadataComponent, ProjectileTrajectoryStateComponent]
completedSourceUnits: [ProjectileStormDefinition, ProjectileStormDefinitionFactory, ProjectileIdentityComponent, ProjectileAiState, ProjectileBehaviorStateComponent, ProjectileLifetimeAndRuntimeState, ProjectileLifetimeStateComponent, ProjectileEffectCooldownStateComponent, ProjectileDispositionStateComponent, ProjectilePenetrationStateComponent, ProjectileHitImmunityPolicyComponent, ProjectileHitImmunityStateComponent, ProjectileReflectionStateComponent, ProjectileTrailCacheComponent, ProjectileCollisionPolicyComponent, ProjectileNetworkStateComponent, ProjectileAnimationStateComponent, ProjectileDamagePayloadComponent, ProjectileDamagePolicyComponent, ProjectileDirectionPolicyComponent, ProjectileUpdateCadenceComponent, ProjectilePresentationStateComponent, ProjectileMinionCapabilityComponent, ProjectileBobberCapabilityComponent, ProjectileCounterweightCapabilityComponent, ProjectileSentryCapabilityComponent, ProjectileTrapCapabilityComponent, ProjectileGeometryStateComponent, ProjectileTrajectoryStateComponent, ProjectileSourceMetadataComponent]
currentComponent: ProjectileDerivedProperties
pendingComponents: [ProjectileIdentityAndClassificationState, ProjectileCombatAndImmunity, ProjectileMovementAndCollisionState, ProjectileNetworkReplicationState, ProjectileMinionAndPresentationState, ProjectileDamageAndElementState, ProjectileAnimationAndDirectionState, ProjectileCollisionAndTargetingState, ProjectileCombatScalingState, ProjectileCollisionGeometryCache, ProjectileTargetSelectionCache, ProjectileFishingAndMiningQueryState, ProjectileKiteAndLightningRules, ProjectileDerivedProperties]
lastCheckpointUtc: 2026-09-12T17:14:10Z
evidence-gap: All 17 leaf groups and 126 members remain mapped. Narrow source boundaries with current build evidence include ProjectileStormDefinition, ProjectileIdentityComponent, ProjectileDefinitionReferenceComponent, ProjectileAiState, ProjectileBehaviorStateComponent, ProjectileLifetimeAndRuntimeState, ProjectileLifetimeStateComponent, ProjectileEffectCooldownStateComponent, ProjectileDispositionStateComponent, ProjectilePenetrationStateComponent, ProjectileHitImmunityPolicyComponent, ProjectileHitImmunityStateComponent, ProjectileReflectionStateComponent, ProjectileTrailCacheComponent, ProjectileCollisionPolicyComponent, ProjectileNetworkStateComponent, ProjectileAnimationStateComponent, ProjectileDamagePayloadComponent, ProjectileDamagePolicyComponent, ProjectileDirectionPolicyComponent, ProjectileUpdateCadenceComponent, ProjectilePresentationStateComponent, ProjectileMinionCapabilityComponent, ProjectileBobberCapabilityComponent, ProjectileCounterweightCapabilityComponent, ProjectileSentryCapabilityComponent, ProjectileTrapCapabilityComponent, ProjectileGeometryStateComponent, ProjectileTrajectoryStateComponent, and ProjectileSourceMetadataComponent. The three capability marker components have been compiled and reflection-checked with value-type layout, bool marker fields, default `false`, and explicit `true` construction; no runtime attachment, Player ownership, world interaction, or cleanup behavior is claimed. The existing focused verifier covers the earlier state boundaries; no new test was added for the four newly hardened payload/geometry/trajectory/source constructors, the new presentation/capability-marker fields, DirectionPolicy, UpdateCadence, or the three capability marker constructors. The read-only reflection check also exercises valid constructor values and rejection of invalid scale, step speed, knockback, and null source-text inputs. Player/fishing capability ownership, enchantment evaluation, alpha/glowMask writers, client projection, identity lifecycle, static NPC immunity, packet transport, world collision, cadence scheduling, and behavior-equivalence remain unresolved. Narrow systems remain storage/writer boundaries only and do not decide target eligibility, write NPC/Player health, own static registries, publish packet bytes, access textures, or terminate projectiles. The cadence component has no negative-value validation because MaxUpdates writer ordering and zero/negative compatibility behavior remain unresolved. The current component-only scope is exhausted for independently determinable state; remaining P15 units require non-component systems, queries, commands, adapters, projections, or cross-domain owners and remain pending.
blocking-decision: Treat each listed source component as complete only for its stored fields, constructor defaults, and compile evidence. This includes the eight presentation fields, `IsMinion`, `IsBobber`, `NoEnchantments`, and `NoEnchantmentVisuals`; it does not close their runtime writers or external consumers. Keep runtime integration, the unresolved identity members, Player/fishing ownership, enchantment evaluation, client projection, static NPC immunity, packet transport, world/owner checks, and AI dispatch pending. Do not add registry cleanup, protocol writes, world/owner checks, or presentation effects until their owners and evidence are closed; do not fill missing Version4 behavior by inference.
  capability-marker-evidence-gap: The three capability marker source files are compiled and reflection-checked in `Build/bin/Terraria.Projectile/Debug/net10.0/Terraria.Projectile.dll`; each is a value type with a bool marker, default `false`, and explicit `true` construction. No runtime attachment, Player ownership, world interaction, or cleanup behavior is claimed.
  evidence-gap-update: ProjectileDamagePayloadComponent now exposes read-only IsMelee, IsRanged, and IsMagic compatibility views derived from the single DamageClass authority; no second writable boolean authorities were added. The fresh build, existing focused verifier, and read-only reflection smoke check now pass. The duplicate Reflected and Ai/LocalAi storage surfaces remain preserved for public compatibility pending caller and writer audit.
  blocking-decision-update: Keep the DamageClass-derived legacy views read-only and retain the existing duplicate Reflected and Ai/LocalAi fields until their consumers are migrated and a focused compatibility contract is verified. Do not remove or redirect ProjectileCollisionPolicyComponent.ManualDirectionChange because the existing ProjectileCombatVerification uses it.
  derived-properties-evidence-gap-update: ProjectilePresentationStateComponent now exposes read-only Opacity as the legacy Alpha-derived view `1.0f - Alpha / 255.0f`; ProjectileUpdateCadenceComponent now exposes read-only MaxUpdates as the legacy ExtraUpdates-derived view `ExtraUpdates + 1`. The focused component smoke evidence passes for the documented boundary values. These properties add no second writable authority. Setter ownership, runtime writer ordering, substep/lifetime/cooldown integration, network or persistence projection, and client presentation behavior remain unresolved.
  derived-properties-blocking-decision-update: Treat Opacity and MaxUpdates as compatibility views only. Do not add setters, duplicate opacity/cadence storage, or claim the full ProjectileDerivedProperties group until the Alpha and ExtraUpdates writers, legacy conversion behavior, and cross-domain consumers are recovered and verified.
  component-boundary-audit: The remaining report members were reconciled against the current `src/Projectile` component files. No independently determinable component file remains safe to add without duplicating an existing authority: owner-hit policy is already stored by ProjectileCollisionPolicyComponent, bonus crit is already stored by ProjectileDamagePayloadComponent, and the remaining scaling curve, static immunity registry, geometry/target/fishing workspaces, kite/lightning rules, and derived properties belong to definitions, queries, adapters, or projections. A second TargetingPolicyComponent would create duplicate owner-hit state until its callers are migrated. This audit added no source code and does not advance implementation or verification status.
```

## 1. Plan Contract

This document records the implementation sequence and the source-only checkpoints completed in this
session. The affected-project build and focused verifiers have run, but no full test, network replay,
save/load replay, or behavior-equivalence check has run. `executionStatus: in-progress`,
`implementationStatus: partial`, and `verificationStatus: partial` remain because only narrow storm,
identity-value, definition-reference, AI state, lifecycle, combat-state, movement-state, network-
scheduling, presentation storage, capability-marker storage, damage-payload, geometry, trajectory,
source-metadata, and damage-policy storage units are implemented and verified at their narrow source
boundary.

The implementation must process only P15 members and must preserve the public API and behavior of
the legacy `Terraria.Projectile` boundary until a replacement writer has passed the focused verifier.
No double-write compatibility window is allowed for authoritative fields.

### Current Source Checkpoint

The following component source units are part of the current source checkpoint:

- `src/Projectile/ProjectilePresentationStateComponent.cs`
- `src/Projectile/ProjectileMinionCapabilityComponent.cs`
- `src/Projectile/ProjectileBobberCapabilityComponent.cs`
- `src/Projectile/ProjectileDamagePolicyComponent.cs`
- `src/Projectile/ProjectileGeometryStateComponent.cs`
- `src/Projectile/ProjectileTrajectoryStateComponent.cs`
- `src/Projectile/ProjectileDamagePayloadComponent.cs`
- `src/Projectile/ProjectileSourceMetadataComponent.cs`
- `src/Projectile/ProjectileDirectionPolicyComponent.cs`
- `src/Projectile/ProjectileUpdateCadenceComponent.cs`
- `src/Projectile/ProjectileCounterweightCapabilityComponent.cs`
- `src/Projectile/ProjectileSentryCapabilityComponent.cs`
- `src/Projectile/ProjectileTrapCapabilityComponent.cs`

`ProjectilePresentationStateComponent` stores `Alpha`, `GlowMask`, `Light`, `IsPreviewDummy`,
`IsPreviewDisplayDoll`, `DrawLayer`, `UsesOwnerLight`, and `Hide` without rendering, lighting,
network, or effect side effects. The minion and bobber components now store explicit `IsMinion` and
`IsBobber` markers while preserving their existing accounting/type constructor arguments. The
damage policy stores both `NoEnchantments` and `NoEnchantmentVisuals`. The geometry, trajectory,
damage-payload, and source-metadata components enforce their documented constructor invariants
without adding external behavior. `ProjectileDirectionPolicyComponent` and
`ProjectileUpdateCadenceComponent` remain storage-only boundaries; the cadence component does not
claim negative-value validation. These source units have no Player/fishing ownership, enchantment
evaluation, alpha/glowMask writer, or client projection. The affected Projectile build completed
with exit code `0`, `0` warnings, and `0` errors. The existing
`Terraria.ProjectileCombatVerification` run passed with exit code `0`; a reflection check confirmed
all listed fields/properties in the resulting DLL.

Verification evidence for this checkpoint:

```powershell
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'build',
  '.\src\Projectile\Terraria.Projectile.csproj',
  '--no-restore',
  '-m:1',
  '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false')
```

Exit code `0`; 0 warnings; 0 errors. The affected artifacts are
`Build/bin/Terraria.Projectile/Debug/net10.0/Terraria.Projectile.dll` and its dependency outputs
under `Build/bin`.

```powershell
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'build',
  '.\src\ProjectileCombatVerification\Terraria.ProjectileCombatVerification.csproj',
  '--no-restore',
  '-m:1',
  '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false')

& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @(
  'run',
  '--project',
  '.\src\ProjectileCombatVerification\Terraria.ProjectileCombatVerification.csproj',
  '--no-build',
  '--no-restore',
  '-m:1',
  '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false')
```

Verifier build exit code `0`; 0 warnings; 0 errors; artifact
`Build/bin/Terraria.ProjectileCombatVerification/Debug/net10.0/Terraria.ProjectileCombatVerification.dll`.
Verifier run exit code `0` with output `PASS: projectile disposition, penetration, immunity, trail,
reflection, collision policy, network, and animation state boundaries`.

An additional read-only reflection check against the Projectile DLL confirmed the public fields and
properties for the current checkpoint components, including `ProjectileDirectionPolicyComponent`
and `ProjectileUpdateCadenceComponent`, and exercised valid constructor values plus rejection of
invalid scale, step speed, knockback, and null source-text inputs. It exited with code `0`. This
does not verify external runtime writers, cadence scheduling, or behavior equivalence.

The source checkpoint also includes the storage-only `ProjectileCounterweightCapabilityComponent`,
`ProjectileSentryCapabilityComponent`, and `ProjectileTrapCapabilityComponent`, with explicit
`IsCounterweight`, `IsSentry`, and `IsTrap` markers. Their post-edit build and reflection evidence
are verified: the Projectile build exited `0` with `0` warnings and `0` errors, and the read-only
reflection check confirmed value-type layout, bool fields, default `false`, and explicit `true`
construction for all three markers.

### Counterweight capability marker checkpoint

Saved source file:

- `src/Projectile/ProjectileCounterweightCapabilityComponent.cs`

The component now stores the Version4 `counterweight` marker as `IsCounterweight`, with a default
value of `false`. It remains a storage-only boundary and does not claim counterweight attachment,
Player ownership, spawn/despawn behavior, presentation, or network integration. The subsequent
serial Projectile build and read-only reflection check passed; no runtime status is advanced by
this source-only checkpoint.

### Sentry capability marker checkpoint

Saved source file:

- `src/Projectile/ProjectileSentryCapabilityComponent.cs`

The component stores the Version4 `sentry` marker as `IsSentry`, with a default value of `false`.
It remains a storage-only boundary and does not claim DD2 placement, Player ownership, lifetime
persistence, presentation, or network integration. The subsequent serial Projectile build and
read-only reflection check passed; no runtime status is advanced by this source-only checkpoint.

### Trap capability marker checkpoint

Saved source file:

- `src/Projectile/ProjectileTrapCapabilityComponent.cs`

The component stores the Version4 `trap` marker as `IsTrap`, with a default value of `false`. It
remains a storage-only boundary and does not claim world interaction, NPC damage, presentation,
network integration, or cleanup. The subsequent serial Projectile build and read-only reflection
check passed; no runtime status is advanced by this source-only checkpoint.

## 2. Initial Checkpoint

`ProjectileIdentityAndClassificationState` is checked against the 16 rows in the authoritative
report. The first implementation unit must separate owner reference, owner-scoped identity, UUID,
catalog type, capability markers, network importance, source/drop metadata, geometry scale and
presentation compatibility instead of copying the legacy field cluster into one component.

Required precondition before code work:

- define and verify the mapping between legacy owner `int`, `EntityReference`, slot, identity,
  `projUUID`, runtime entity, replication ID and any persistent ID;
- make one identity/terminal writer responsible for clearing `Main.projectileIdentity`-equivalent
  state;
- keep `alpha` and `glowMask` in a projection/compatibility seam until their writer contract is
  proven.

### Identity value boundary checkpoint

Saved source files:

- `src/Projectile/ProjectileOwnerReference.cs`
- `src/Projectile/ProjectileIdentityComponent.cs`

The value boundary keeps the ECS owner reference, legacy owner slot, projectile slot, owner-scoped
identity, and optional projectile UUID separate. It validates the known sentinel/range contracts
and provides pure owner/identity and network-identity matching methods. It has no global registry,
allocation, terminal cleanup, network transport, or presentation side effect.

The identity/classification implementation unit remains in progress. The next source unit is not
authorized until this saved checkpoint is built and its focused identity verifier passes.

### Definition reference checkpoint

Saved source file:

- `src/Projectile/ProjectileDefinitionReferenceComponent.cs`

The new immutable component owns only `ProjectileType` and `CatalogRevision`, validates non-negative
values, and exposes `HasProjectileType`. It does not replace the broad compatibility
`ProjectileDefinitionComponent` yet and does not own mutable disposition, AI, damage, lifetime,
network scheduling, or optional capabilities.

The identity/value checkpoint was built and focused-verified with exit code `0`. The focused verifier
covered owner/slot/identity/UUID separation, default sentinels, range validation, owner/identity
matching, optional UUID matching, and immutable type/catalog reference validation. The identity
group remains in progress because lifecycle writers, reverse-index cleanup, duplicate network
rejection, and unresolved classification/projection writers are not implemented.

### AI state implementation checkpoint

Saved source files:

- `src/Projectile/ProjectileBehaviorStateComponent.cs`
- `src/Projectile/ProjectileBehaviorStateResetSystem.cs`

The component preserves three synchronized `ai` values and three local `localAI` values, rejects
non-finite values, and exposes deterministic slot reads. `aiStyle` is not stored in this mutable
component; `ProjectileDefinitionReferenceComponent.BehaviorKey` is the definition-side behavior key.
The reset system is the explicit writer for zeroing both state sets. Behavior dispatch, unknown-style
fail-closed handling, network ingress, lifecycle scheduling, and raw/local synchronization remain
unverified and are not claimed here.

## 3. Ordered Implementation Units

Each unit is a small, independently revertible change. The source path, target path and dependency
impact are recorded before moving a type. Do not depend on file order for runtime order.

| Unit | Scope | Proposed source/target boundary | Single writer | Focused evidence before next unit |
|---:|---|---|---|---|
| 0 | verifier harness | add/extend Projectile-focused verifier project under `dome/Test`, output under `Build/` | verifier owns assertions only | identity, slot reuse and terminal cleanup cases are runnable |
| 1 | identity/classification | legacy spawn/slot code -> `Projectile/Identity` and `Projectile/Definition` seams | `ProjectileSpawnSystem` then terminal commit | owner/identity/UUID uniqueness and duplicate network rejection |
| 2 | AI state | `ai`, `localAI`, `aiStyle` -> `Projectile/Behavior` | `ProjectileBehaviorSystem` | raw AI reset, local-only state, unknown behavior fail-closed |
| 3 | lifetime/update cadence | `active`, `timeLeft`, `extraUpdates`, `numUpdates`, `stepSpeed` -> `Projectile/Lifecycle` | `ProjectileLifetimeSystem` | substep count, expiry, owner loss, out-of-world and kill reasons |
| 4 | combat payload | damage, original damage, knockback, disposition, element/tag/scaling -> `Projectile/Combat` | combat calculation plus hit commit port | damage class, current/original damage, modifiers and accepted-hit event |
| 5 | penetration/immunity | counters and local/static/player immunity -> `Projectile/Combat` and registry adapter | `ProjectileCombatSystem` plus immunity tick system | reset/reuse, cooldown isolation, penetration terminal transition |
| 6 | movement/collision | trajectory, trail history and collision policy -> `Projectile/Movement` and `Projectile/Collision` | movement/collision systems | sweep collision, slope/fall-through, liquid policy, cache invalidation |
| 7 | network projection | dirty/section/skip state -> `Projectile/Network` projection | network scheduler/adapter | owner identity matching, section resend, no packet type in core |
| 8 | minion/capability/source | optional markers and source metadata -> `Projectile/Capability` and `Projectile/Spawn` | capability-specific system/command | sentry/minion/trap/bobber/counterweight ownership and source provenance |
| 9 | query extraction | static geometry/target/fishing/mining lists -> pure query workspaces | query caller owns workspace lifetime | deterministic candidate ordering and no global mutable query state |
| 10 | definitions and derived views | constants, storm value, derived properties -> `Definition`, `Queries`, `Presentation` | catalog/adapter/projection | immutable catalog revision and read-only derived views |
| 11 | compatibility removal | legacy field readers/writers -> ports/adapters, then remove duplicate fields | one replacement owner per field | full regression matrix and rollback checkpoint |

## 4. Proposed System Schedule

The runtime schedule must be explicit in a schedule type or equivalent orchestrator:

```text
1. Resolve projectile spawn/despawn/replication commands.
2. Validate identity, definition revision and source metadata.
3. Advance behavior and substep budget.
4. Apply environment and movement inputs through spatial/liquid ports.
5. Run collision and target eligibility queries.
6. Resolve hit commands, immunity, penetration and combat result events.
7. Apply terminal destroy, child spawn and item-drop commands at one commit root.
8. Decrement timers and publish committed network/persistence/presentation projections.
```

The exact position of environment updates, damage and terminal cleanup must be confirmed against
the Version4 `Update`, `Damage` and `Kill` paths. The schedule must be tested for duplicate commands,
paused ticks, invalid targets, owner disconnect and network replay. File or directory order is not a
valid scheduling mechanism.

## 5. Compatibility and Dependency Rules

- Preserve `Terraria.Projectile` public names and legacy adapters until the new writer is verified.
- Treat `ProjectileDefinitionComponent` in the current dome as a broad partial type; split catalog
  data from mutable instance state before adding new consumers.
- Keep `LocationComponent`, `VelocityComponent`, `ColliderComponent`, `Player`, `NPC`, `Tile`,
  liquid, inventory, network transport and persistence envelopes outside projectile components.
- Use commands for structural changes (`SpawnProjectileCommand`, `ProjectileHitCommand`,
  `ProjectileDestroyCommand`, child-spawn and item-drop commands). Events express committed facts.
- Network and persistence snapshots read committed state and never become a second authority.
- During migration, a legacy adapter may read the new component state, but two writers for one field
  are forbidden. Rollback means restoring the last single-writer boundary, not re-enabling dual writes.

## 6. Verification Plan

The following commands describe the required verification shape for a future full integration
pass. The source-only commands actually run for the current checkpoint are recorded above and
below. Any compile-capable command must run serially from the repository root through the required
wrapper, after inspecting active `dotnet.exe`/`csc.exe` processes, and must target only the affected
project:

```powershell
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 `
  build .\dome\src\Terraria.Dome.Simulation\Terraria.Dome.Simulation.csproj `
  --no-restore -m:1 -nr:false `
  -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false

pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 `
  test .\dome\Test\Terraria.Dome.Combat.Verification\Terraria.Dome.Combat.Verification.csproj `
  --no-build --no-restore -m:1 -nr:false `
  -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false
```

The verifier matrix must cover: slot reuse and UUID mapping; spawn reset; raw AI synchronization;
substep/lifetime expiry; owner loss; tile/liquid collision; target invalidation; local/static/player
immunity; penetration and reflection; friendly/hostile transitions; minion/sentry/bobber/trap
capabilities; source propagation; network duplicate/kill/section resend; presentation projection;
pure geometry/target/fishing/mining queries; storm progress/hitbox math; and snapshot restore.

Existing verifier source is evidence of planned or existing scenarios only. It is not current-session
verification. Every actual run must record command, project, exit code, warning/error counts and
`Build/bin` artifact path in the implementation record.

## 7. Rollback and Completion Criteria

Rollback at any unit if a focused verifier detects duplicate ownership, changed hit order, changed
identity mapping, changed serialized shape, changed terminal effect ordering, or a query writes state.
Restore the prior single-writer adapter and retain the failing fixture; do not silently broaden the
component or bypass the verifier.

The implementation is complete only when every P15 member has one confirmed owner, all cross-domain
ports are explicit, proposed compatibility projections are tested, focused verifier and affected
project checks pass, and the build artifact is under `Build/bin`. This planning session makes none of
those claims.

## 8. Checkpoint Log

| Checkpoint | Status | Note |
|---|---|---|
| `ProjectileIdentityAndClassificationState` | complete for design ledger | 16 report members mapped; identity/disposition/presentation ownership gaps recorded |
| `ProjectileAiState` | complete for narrow source unit | raw `ai`/`localAI` split from `aiStyle`; behavior key is definition-side, reset is explicit, dispatch/network remain pending |
| `ProjectileLifetimeAndRuntimeState` | complete for narrow source unit | active/timeLeft, terminal transition, effect delay, definition constants, and trajectory GfxOffY source boundaries are implemented and focused-verified; runtime scheduler and terminal identity cleanup remain pending |
| `ProjectileCombatAndImmunity` | complete for design ledger | hit, penetration and immunity are separate state/policy seams; cross-owner combat commit remains integration-review |
| `ProjectileMovementAndCollisionState` | complete for design ledger | history/cache, collision policy and environment inputs are separate from shared spatial authority |
| `ProjectileNetworkReplicationState` | complete for design ledger | dirty/throttle/section retry state is one-way projection scheduling; identity and packet types remain external |
| `ProjectileMinionAndPresentationState` | complete for design ledger | minion capability/source, presentation fields and player immunity are separated; Player and client owners remain integration-review |
| `ProjectileDamageAndElementState` | complete for design ledger | damage class, element, tags and source flags use separate payload/policy/provenance seams |
| `ProjectileAnimationAndDirectionState` | complete for design ledger | frame and direction policy are separated from raw AI, damage and texture lookup |
| `ProjectileCollisionAndTargetingState` | complete for design ledger | UUID, collision policy, cooldown and Banner association use explicit query/command seams |
| `ProjectileCombatScalingState` | complete for design ledger | bonus crit is an instance/source snapshot; hostile scaling is an immutable definition/query with the Version4 consumer still partial |
| `ProjectileCollisionGeometryCache` | complete for design ledger | static conditions, javelin buffers, lance bounds, whip points and lightning bounds have explicit query/workspace ownership |
| `ProjectileTargetSelectionCache` | complete for design ledger | seven static candidate/blacklist workspaces mapped to behavior-scoped deterministic queries; source call paths remain partial |
| `ProjectileFishingAndMiningQueryState` | complete for design ledger | fishing context/candidates and mining skip points are external attempt/tile-query workspaces; fish/item effects remain commit-port responsibilities |
| `ProjectileKiteAndLightningRules` | complete for design ledger | two constants remain immutable definition/query policy; kite and lightning consumers are explicit seams with missing specialized bodies recorded |
| `ProjectileDerivedProperties` | complete for design ledger | eight derived properties mapped to explicit views; Opacity/MaxUpdates setters become compatibility commands and cross-domain references become snapshots/handles |
| `ProjectileStormDefinition` | complete for implementation checkpoint | immutable pellet-storm value object and pure progress/position/hitbox queries implemented and focused-verified; storm AI and collision integration remain pending |

## 12. Lifetime and Runtime State Execution Checkpoint

`ProjectileLifetimeAndRuntimeState` is complete for the narrow source implementation unit and its
focused verifier. The
following files were saved before this checkpoint was written:

- `src/Projectile/ProjectileLifetimeDefinition.cs`
- `src/Projectile/ProjectileLifetimeStateComponent.cs`
- `src/Projectile/ProjectileLifetimeSystem.cs`
- `src/Projectile/ProjectileEffectCooldownStateComponent.cs`
- `src/Projectile/ProjectileEffectCooldownSystem.cs`
- `src/Projectile/ProjectileTrajectoryStateComponent.cs`

The implementation preserves the confirmed `SentryLifeTime=36000` and `ArrowLifeTime=1200` constants,
keeps `Active` separate from `TimeLeft`, clamps the normal advance transition to inactive/zero with
`LifetimeExpired`, and makes explicit termination idempotent. It accepts the confirmed `soundDelay`
`-1` sentinel and decrements positive delays only. `GfxOffY` is stored with trajectory runtime state
and is not merged into lifetime or position authority.

This source unit has no world, owner, entity-store, network, identity-map, audio, child-spawn, item-drop,
channel, or presentation side effect. It is not yet wired to the legacy/root `ProjectileLifetimeComponent`
or a runtime scheduler; those integration and behavior-ordering checks remain pending.

The affected Projectile project was built serially through the repository wrapper:

```powershell
& .\Build\Tools\Invoke-SerialDotnet.ps1 `
  -DotnetArguments @(
    'build',
    '.\src\Projectile\Terraria.Projectile.csproj',
    '--no-restore',
    '-m:1',
    '-nr:false',
    '-p:UseSharedCompilation=false',
    '-p:MSBuildNodeReuse=false',
    '-p:BuildInParallel=false'
  )
```

Result: exit code `0`, `0` warnings, `0` errors; artifact
`Build/bin/Terraria.Projectile/Debug/net10.0/Terraria.Projectile.dll`.

A focused reflection verifier loaded that artifact and passed with exit code `0`. It covered the two
definition constants, active/timeLeft separation, expiry transition, idempotent terminal commit,
invalid lifetime and cooldown rejection, positive cooldown decrement, the `-1` cooldown sentinel,
and trajectory `GfxOffY`/`StepSpeed` retention. Runtime scheduling, identity cleanup, network
tombstones, owner/environment checks, and full behavior equivalence remain unverified.

## 13. Identity and Classification Source Checkpoint

The following source files were saved before this checkpoint was written:

- `src/Projectile/ProjectileDispositionStateComponent.cs`
- `src/Projectile/ProjectileDispositionSystem.cs`

`ProjectileDispositionStateComponent` is the instance-owned source boundary for `friendly` and
`hostile`. `ProjectileDispositionSystem.InitializeFromDefinition` copies the compatibility
definition defaults once at spawn composition, while `SetFriendly` and `SetHostile` are explicit
writers for later runtime decisions. The component does not merge with
`ProjectileIdentityComponent`, `ProjectileDefinitionReferenceComponent`,
`ProjectileNetworkStateComponent`, or `ProjectileSourceMetadataComponent`.

This checkpoint intentionally leaves `active`, `perIDStaticNPCImmunity`, `ownerHitCheckDistance`,
`arrow`, `numHits`, `bobber`, `netImportant`, `noDropItem`, `counterweight`, `scale`, `rotation`,
`type`, `alpha`, `sentry`, `glowMask`, owner allocation/cleanup, duplicate network rejection, and
global/static registry behavior pending. No Player, NPC, world, network, presentation, or terminal
side effect is performed by this source unit.

## 14. Combat Source Checkpoint: Penetration State

The following source files were saved before this checkpoint was written:

- `src/Projectile/ProjectilePenetrationStateComponent.cs`
- `src/Projectile/ProjectilePenetrationSystem.cs`

The component validates the Version4-compatible `-1` unlimited sentinel, rejects values below
`-1`, and separates remaining hits, maximum hits, accepted-hit count, and the
`StopsDealingDamageWhenDepleted` policy. `ProjectilePenetrationSystem.Initialize` is the explicit
spawn/setup writer. `CanDealDamage` is a pure read over committed penetration state, and
`CommitAcceptedHit` is the only writer for accepted-hit counting and finite decrement. A depleted
state returns `false` without changing the state; unlimited penetration increments the hit count
without decrementing `-1`.

Dependency impact is limited to the existing `Terraria.Projectile` project and the already isolated
combat namespace. No NPC, Player, target-health, local/static immunity, network, persistence, item,
or terminal writer was added. The pending combat commit must still combine eligibility, immunity,
penetration, damage, reflection, and terminal transition as one auditable result.

## 15. Combat Source Checkpoint: Reflection Result

The following source files were saved before this checkpoint was written:

- `src/Projectile/ProjectileReflectionStateComponent.cs`
- `src/Projectile/ProjectileReflectionSystem.cs`

The component stores the committed instance-level `Reflected` result. `MarkReflected` and
`SetReflected` are the only writers in this source unit. This does not reconstruct Version4's NPC
reflection rules, velocity/rotation transform, target state, damage, network projection, or terminal
cleanup; those boundaries remain pending evidence and integration ownership.

## 16. Combat Source Checkpoint: Per-Projectile Immunity State

The following source files were saved before this checkpoint was written:

- `src/Projectile/ProjectileHitImmunityPolicyComponent.cs`
- `src/Projectile/ProjectileHitImmunityStateComponent.cs`
- `src/Projectile/ProjectileHitImmunitySystem.cs`

The policy component preserves the observed local cooldown sentinels (`-2` for no local-array
write and `-1` for a persistent local immunity value) and rejects values below those supported
sentinels. The state component stores local NPC cooldowns, player cooldowns, and restrike delay in
per-projectile state. `ProjectileHitImmunitySystem.AdvanceTick` decrements player cooldowns on
every tick, decrements local NPC cooldowns only when the policy enables local immunity, and
decrements restrike delay when positive. Set/reset methods validate indices and values and do not
touch external NPC/Player or static registry state.

Dependency impact is limited to the existing `Terraria.Projectile` project. The static NPC
immunity registry, target eligibility, normal NPC immunity, owner melee cooldown, accepted-hit
commit, network projection, and terminal lifecycle remain unimplemented and are recorded as
evidence gaps rather than inferred here.

## 17. Movement Source Checkpoint: Trail Cache

The following source files were saved before this checkpoint was written:

- `src/Projectile/ProjectileTrailCacheComponent.cs`
- `src/Projectile/ProjectileTrailCacheSystem.cs`

`ProjectileTrailCacheComponent` owns the caller-sized position, rotation, and sprite-direction
history arrays plus the caller-owned `WhipPoints` workspace. Its constructor rejects negative
history lengths. `ProjectileTrailCacheSystem.Record` shifts all three histories together and
writes the newest sample at index zero; `Reset` clears all histories and whip points. No world
query, specialized whip-control-point generation, player displacement compensation, presentation
effect, or collision commit is included. The Projectile project build completed with exit code `0`,
`0` warnings, and `0` errors, producing
`Build/bin/Terraria.Projectile/Debug/net10.0/Terraria.Projectile.dll`. The focused verifier, invoked
through the serial wrapper with explicit `--project`, completed with exit code `0` and covered
history record/reset and array-boundary behavior. The evidence is limited to this cache boundary.

## 18. Movement Source Checkpoint: Collision Policy

The following source files were saved before this checkpoint was written:

- `src/Projectile/ProjectileCollisionPolicyComponent.cs`
- `src/Projectile/ProjectileCollisionPolicySystem.cs`

`ProjectileCollisionPolicyComponent` preserves the twelve policy/instance fields for tile
collision, liquid handling, slope handling, manual fall-through, tile reflection/bounce inputs,
owner self-hit qualification, and behavior-controlled direction changes. Its constructor rejects
negative bounce capacity, non-finite or negative bounce values, and non-finite or negative owner-hit
distance. `ProjectileCollisionPolicySystem` exposes explicit writers for each policy family,
validates numeric updates, and computes effective fall-through only when both
`DecidesManualFallThrough` and `ShouldFallThrough` are true. It does not own Tile, liquid,
WorldGrid, broadphase, target colliders, velocity, rotation, or reflection/bounce physics. The
Projectile project build completed with exit code `0`, `0` warnings, and `0` errors, producing
`Build/bin/Terraria.Projectile/Debug/net10.0/Terraria.Projectile.dll`. The focused verifier, invoked
through the serial wrapper with explicit `--project`, completed with exit code `0` and covered
writer validation, fall-through qualification, bounce boundaries, and owner-hit distance boundaries.
The evidence is limited to this policy boundary.

## 19. Network Source Checkpoint: Scheduling State

The following source files were saved before this checkpoint was written:

- `src/Projectile/ProjectileNetworkStateComponent.cs`
- `src/Projectile/ProjectileNetworkStateSystem.cs`

The component validates non-negative player capacity and `NetSpam`. The system exposes explicit
primary/secondary update requests, applies the observed `60` send-budget threshold and `+5`
increment, decrements positive spam once per tick, and preserves deferred requests when the budget
is saturated. Per-player section skip flags are bounds-checked and are cleared only through the
explicit clear writer intended for a successful eligible send. Reset clears all state. This unit
does not parse or serialize packets, match owner/identity, inspect active sections or connections,
publish transport effects, or create network tombstones. The Projectile project build completed with
exit code `0`, `0` warnings, and `0` errors, producing
`Build/bin/Terraria.Projectile/Debug/net10.0/Terraria.Projectile.dll`. The focused verifier, invoked
through the serial wrapper with explicit `--project`, completed with exit code `0` and covered
primary/secondary request deferral, the `60` threshold and `+5` budget increment, positive spam
decay, per-player section flags, and reset. The evidence is limited to scheduling state.

## 20. Animation Source Checkpoint: Frame State

The following source files were saved before this checkpoint was written:

- `src/Projectile/ProjectileAnimationStateComponent.cs`
- `src/Projectile/ProjectileAnimationStateSystem.cs`

`ProjectileAnimationStateComponent` owns only the non-negative `Frame` and `FrameCounter` values.
The reset writer returns both values to zero. The system provides explicit frame/counter setters
and a deterministic counter rollover for a caller-supplied positive limit. It does not select an
AI-specific cadence, read raw AI, change `manualDirectionChange`, change `spriteDirection`, inspect
texture layout, or mutate geometry, damage, or presentation state. The Projectile project build
completed with exit code `0`, `0` warnings, and `0` errors, producing
`Build/bin/Terraria.Projectile/Debug/net10.0/Terraria.Projectile.dll`. The focused verifier, invoked
through the serial wrapper with explicit `--project`, completed with exit code `0` and covered frame
and counter initialization, deterministic rollover, reset, and invalid limits. The evidence is
limited to frame/counter state.

## 9. Kite and Lightning Execution Checkpoint

`ProjectileKiteAndLightningRules` is complete for the execution ledger. This checkpoint changes no
source code and authorizes no implementation. The planned implementation unit must introduce or
reuse immutable definition/query types only after the missing specialized behavior is recovered.

| Member | Planned target | Writer/consumer | Required evidence before implementation | Verification focus |
|---|---|---|---|---|
| `MinimumWindStrengthToFlyKite` | `ProjectileKiteWindPolicy` (`status: proposed`) | pure kite wind query; movement system applies returned intent | Version4 wind speed/direction, authority, lift/drag and threshold transition fixture | finite/non-finite input, below/equal/above threshold, direction reversal, deterministic intent, no query side effect |
| `StormLightningLiquidDamageRadius` | `ProjectileStormLightningDefinition` plus liquid query (`status: proposed`) | liquid/world snapshot query; combat/world commit port | Version4 liquid candidate selection, collision ordering, damage timing, duplicate-event handling and client/server ownership | radius boundary, empty liquid, multiple regions, immunity/disposition, exactly-once commit and projection-only client path |

`ProjectileDerivedProperties` is complete for the execution ledger. The final unit is
`ProjectileStormDefinition`, including its seven value fields and the pure methods
`GetBulletProgress`, `IsValid`, `GetBulletPosition`, and `GetBulletHitbox`.

## 10. Derived Properties Execution Checkpoint

`ProjectileDerivedProperties` is complete for the execution ledger. This checkpoint authorizes no
implementation and does not convert any existing partial NLTX helper into Version4-equivalent
behavior. The implementation unit must preserve the two legacy setter aliases through single-writer
commands while keeping all other properties as read-only projections or queries.

| Member | Planned target | Planned writer/consumer | Required evidence before implementation | Verification focus |
|---|---|---|---|---|
| `Name` | `ProjectileNameQuery` (`status: proposed`) | localization/catalog adapter | catalog and language revision lifecycle, missing-entry behavior | type bounds, empty name fallback, no localized string in authoritative/persistence state |
| `WipableTurret` | `SentryCleanupEligibilityQuery` (`status: proposed`) | lifecycle cleanup command | authoritative owner, sentry authority, DD2 event persistence and actual cleanup caller | local-owner gate, listed DD2 types while event is active/inactive, no query-side destruction |
| `Opacity` | `ProjectileOpacityView` plus `SetProjectileOpacityCommand` (`status: proposed`) | presentation alpha writer; rendering/effect readers | alpha protocol/reset writer and setter conversion contract | `alpha=0/255`, below/above-range opacity, truncation, repeated read/write round-trip |
| `MaxUpdates` | `ProjectileUpdateCadenceView` plus `SetProjectileMaxUpdatesCommand` (`status: proposed`) | cadence/lifecycle writer; substep and definition setup readers | live `extraUpdates` writer ordering and negative/zero compatibility behavior | getter/setter alias, substep count, lifetime/cooldown expressions and no duplicate cadence state |
| `OwnerMinionAttackTargetNPC` | `OwnerMinionTargetQuery` (`status: proposed`) | Player snapshot adapter and behavior systems | invalid owner/target slot handling and NPC snapshot version | negative sentinel, stable handle, chaseability/line-of-sight filtering outside query, no raw NPC reference |
| `OwnedBySomeone` | `ProjectileOwnershipModeQuery` (`status: proposed`) | combat eligibility and source classification readers | exact `npcProj`/`trap` source flags and distinction from owner validity | all four flag combinations, no implicit owner merge, compatibility with PVE eligibility |
| `CareForAttackCD` | `ProjectileOwnerMeleeCooldownQuery` (`status: proposed`) | accepted-hit commit port to Player | player cooldown ownership and owner slot authority | exact predicate, rejected hit does not write cooldown, one accepted hit produces one command |
| `NetSectionCoordinates` | `ProjectileNetworkSectionQuery` (`status: proposed`) | network scheduler and skipped-section retry | section dimensions, integer arithmetic and position authority | tile/section boundaries, negative coordinates, skipped resend, no persistence authority |

Required implementation order:

1. Define immutable input snapshots and adapter interfaces for localization, Player/NPC, DD2 event
   state, and network section mapping.
2. Add pure derived queries with no global reads or writes. Keep `OwnerMinionAttackTargetNPC` as a
   handle/snapshot result and retain the exact legacy flag predicates.
3. Add compatibility commands for `Opacity` and `MaxUpdates`; route their writes to the existing
   alpha/cadence authorities and remove any second writer only after focused fixtures pass.
4. Connect cleanup, accepted-hit cooldown, rendering, behavior, and network projection systems in
   the explicit runtime schedule. Do not let a property getter call a terminal command.
5. Record catalog/revision, snapshot, network, and persistence behavior before the final storm unit.

The preceding units remain design-only. Their implementation status is not changed by the storm
checkpoint; no build, test, or focused verifier has been run yet.

## 11. Storm Definition Execution Checkpoint

`ProjectileStormDefinition` is complete for the execution ledger and has one source-only pure
implementation checkpoint. This does not claim that the placeholder storm AI has been reconstructed.

| Member / behavior | Planned target | Planned writer/consumer | Required evidence before implementation | Verification focus |
|---|---|---|---|---|
| `StartAngle`, `AnglePerBullet` | immutable `ProjectileStormDefinition` (`status: proposed`) | pure position/hitbox query | catalog revision and exact factory angle formulas | angle zero, rotation spacing, deterministic position |
| `BulletsInStorm` | immutable storm cardinality | collision system loop | type-871 loop ownership and index contract | empty/negative compatibility policy, normal count, no out-of-range mutation |
| `BulletsProgressInStormStartNormalized`, `BulletsProgressInStormBonusByIndexNormalized` | immutable progress inputs | pure progress/validity methods; factory reads behavior snapshot | localAI writer and `Utils.GetLerpValue` compatibility | progress at 0/1, negative/greater-than-one, slope and index behavior |
| `StormTotalRange`, `BulletSize` | immutable geometry definition | pure position/hitbox methods | exact vector/rectangle arithmetic and rounding | radial position, centered rectangle dimensions, rotated directions |
| `GetBulletProgress`, `IsValid`, `GetBulletPosition`, `GetBulletHitbox` | pure methods on the value type | collision query only | method body and no hidden side effects | repeated calls are stable; no NPC/world/projectile writes |
| `AI_172_GetPelletStormInfo` / six-storm factory | `ProjectileStormDefinitionFactory` (`status: proposed`) | collision system obtains caller-owned value snapshots | recover enclosing AI lifetime/authority and localAI progression | six definitions, per-index formulas, cache invalidation and catalog revision |

Required implementation order:

1. Define the immutable value type and preserve the legacy formulas and the fact that `IsValid` does
   not itself check `BulletsInStorm` bounds. Completed in the source-only checkpoint below. Catalog
   revision capture remains an integration gap rather than an invented field.
2. Add a pure factory that accepts a copied `localAI[0]` snapshot and returns caller-owned storm
   values. Do not read static `Main` state or retain a static list. Completed in the source-only
   checkpoint below.
3. Connect the type-871 collision query to the factory and emit a collision intent; keep combat,
   penetration, target health, liquid, network, and presentation writes outside the value/query.
4. Add focused fixtures for progress, position, hitbox intersection, six-storm enumeration,
   snapshot restore, catalog revision, and repeated-query determinism.
5. Recover or fixture-test the placeholder `AI_172_HallowBossRainbowPelletStorm` lifecycle before
   changing any writer or claiming behavior equivalence.

## 12. Final Integration Handoff

The P15 plan now covers all 17 leaf groups and all 126 report members:

| Category | Groups | Members | Final boundary |
|---|---:|---:|---|
| authoritative instance/capability state | 10 | 87 | narrow projectile components with one writer per field and explicit commands/events |
| registry/projection state | 1 | 4 | one-way replication scheduling and adapter boundary; transport is external |
| derived/query workspaces | 3 | 18 | caller-owned snapshots/workspaces; no static mutable global authority |
| immutable definition/query rules | 1 | 2 | catalog policies and specialized queries; no mutable projectile instance state |
| derived properties | 1 | 8 | read-only projections plus two explicit legacy setter commands |
| storm definition | 1 | 7 | immutable value/query geometry boundary |
| **Total** | **17** | **126** | **proposed; narrow implementation partial** |

The execution plan for a later implementation session is:

1. Re-read `AGENTS.md`, `Context/progress.md`, the ECS file-organization constraint, side-effect isolation
   constraint, and the authoritative report; confirm the partition/session identity before edits.
2. Run the repository's focused verifier harness only through the serial dotnet wrapper, after
   inspecting active `dotnet.exe` and `csc.exe` processes. Do not parallelize compile-capable work.
3. Implement in capability order: identity/reset, AI, lifecycle, combat/immunity, movement/collision,
   network projection, capabilities/source, query extraction, definitions/derived views, then
   compatibility removal. Record every source/target move and dependency impact.
4. Preserve one writer per authoritative field. Keep Player, NPC, Item, Tile/liquid, localization,
   transport, persistence, audio and rendering behind explicit ports/adapters/projections.
5. Use focused fixtures before moving to the next unit: spawn reset and slot reuse; raw/local AI and
   unknown behavior; update budget and lifetime; collision/target/immunity/penetration; capability
   ownership; network section/identity/kill; derived properties; storm math and snapshot restore.
6. Run affected-project build and verifier commands serially from the repository root through
   `Build/Tools/Invoke-SerialDotnet.ps1`, with `UseSharedCompilation=false`, `MSBuildNodeReuse=false`,
   and `BuildInParallel=false`. Record command, project, exit code, warning/error counts, and the
   artifact path under `Build/bin/`.

Required integration questions before implementation are: which system owns each legacy setter;
whether owner slot `255`, NPC projectiles, traps, and invalid handles have explicit semantics; which
catalog/language/replay revision is captured; how negative coordinates are handled by network mapping;
where sentry cleanup and Player cooldown commands commit; how query cache versions invalidate; and how
placeholder specialized AI bodies are supplied without inferring behavior.

No full test, verifier integration, network replay, save/load replay, or behavior-equivalence claim
was made before the source-only checkpoint. The remaining P15 work is still unverified and the
top-level statuses are `executionStatus: in-progress`, `implementationStatus: partial`, and
`verificationStatus: partial`.

## 13. Source-Only Implementation Checkpoint: ProjectileStormDefinition

### Saved source files

- `src/Projectile/ProjectileStormDefinition.cs`
- `src/Projectile/ProjectileStormDefinitionFactory.cs`
- `src/Projectile/Terraria.Projectile.csproj`

### Core behavior

- The immutable value type contains the seven Version4 storm fields.
- The four pure geometry methods preserve the confirmed progress, inclusive validity, rotated
  position, and centered rectangle truncation semantics.
- `ProjectileStormDefinitionFactory.Create` preserves the confirmed per-index formulas from
  `AI_172_GetPelletStormInfo`; `CreateAll` returns six caller-owned definitions.
- No query method reads global state, writes projectile/NPC/combat/world state, emits effects, or
  retains a static mutable workspace.

### Dependency impact

`Terraria.Projectile.csproj` now references the existing `Terraria.EntityEcs` project for its
immutable `EntityHitbox` return value. No legacy API, registration key, network/persistence field,
or runtime writer was replaced. The returned hitbox is a value result; target intersection and
combat commit remain outside this unit.

### Verification evidence

The affected Projectile project was built serially through the repository wrapper:

```powershell
& .\Build\Tools\Invoke-SerialDotnet.ps1 `
  -DotnetArguments @(
    'build',
    '.\src\Projectile\Terraria.Projectile.csproj',
    '--no-restore',
    '-m:1',
    '-nr:false',
    '-p:UseSharedCompilation=false',
    '-p:MSBuildNodeReuse=false',
    '-p:BuildInParallel=false'
  )
```

Result: exit code `0`; `Terraria.Relationships`, `Terraria.EntityEcs`, and
`Terraria.Projectile` built without warnings or errors. The artifact is
`Build/bin/Terraria.Projectile/Debug/net10.0/Terraria.Projectile.dll`.

The focused pure-value verifier loaded `Terraria.Relationships.dll`, `Terraria.EntityEcs.dll`, and
`Terraria.Projectile.dll` from the same output directory and passed with exit code `0`. It covered
six-storm enumeration, per-index angle and progress formulas, progress `0` and `1`, negative-index
compatibility, inclusive validity, centered hitbox `(92,192,16,16)`, rotated position math, and
deterministic repeated queries. This evidence does not verify storm AI, type-871 collision
integration, lifecycle, authority, network, persistence, or full behavior equivalence.

### Unfinished and unverified

The other 15 implementation groups remain pending. `AI_172_HallowBossRainbowPelletStorm`, type-871
collision integration, catalog revision capture, lifecycle/authority, network/persistence behavior,
and behavior-equivalence fixtures remain unresolved. The current `verificationStatus: partial`
records the successful affected-project build and focused pure-value verifier only; it does not
authorize claiming the remaining groups as implemented.

### DamageClass compatibility-view checkpoint

`src/Projectile/ProjectileDamagePayloadComponent.cs` keeps `DamageClass` as the single writable
damage-classification authority and exposes the read-only compatibility views `IsMelee`,
`IsRanged`, and `IsMagic`. No second set of writable `melee`, `ranged`, or `magic` flags was
introduced. The existing duplicate `Reflected` and `Ai`/`LocalAi` fields remain pending a caller
and writer audit; `ManualDirectionChange` remains because the existing
`ProjectileCombatVerification` consumes it.

This checkpoint changed no System, Query, Command, Adapter, Projection, test, verifier, or
project-file code. The serial Projectile build exited `0` with `0` warnings and `0` errors, and
produced `Build/bin/Terraria.Projectile/Debug/net10.0/Terraria.Projectile.dll`. The existing
verifier was run through the repository wrapper with `run --project
.\src\ProjectileCombatVerification\Terraria.ProjectileCombatVerification.csproj --no-build
--no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false
-p:BuildInParallel=false`; it exited `0` and printed:

```text
PASS: projectile disposition, penetration, immunity, trail, reflection, collision policy, network, and animation state boundaries
```

A read-only reflection smoke check also exited `0`: `Generic` reported all three views as
`false`; `Melee` reported only `IsMelee=true`; `Ranged` reported only `IsRanged=true`; and
`Magic` reported only `IsMagic=true`.

The implementation status remains `partial`. Runtime classification writers, the remaining
implementation groups, and cross-domain behavior are still unimplemented.

## 14. Derived Properties Compatibility-View Checkpoint

This checkpoint adds two read-only projections that are independently determined by already
existing component authorities:

- `src/Projectile/ProjectilePresentationStateComponent.cs`: `Opacity` is calculated from the
  stored `Alpha` value with the legacy formula `1.0f - Alpha / 255.0f`. It does not store a second
  opacity value and does not perform rendering, lighting, or client projection.
- `src/Projectile/ProjectileUpdateCadenceComponent.cs`: `MaxUpdates` is calculated from the stored
  `ExtraUpdates` value with the legacy formula `ExtraUpdates + 1`. It does not add a setter, validate
  negative values, or schedule substeps, lifetime, or cooldown updates.

The affected files are limited to these two component definitions. No System, Query, Command,
Adapter, Projection, test, verifier, or project-file code was added. The full
`ProjectileDerivedProperties` group remains partial: setter ownership, runtime writer ordering,
network/persistence/client projection, and cross-domain consumers are not closed by these getters.

### Verification evidence

The affected Projectile project was built serially through the repository wrapper with
`.\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\Projectile\Terraria.Projectile.csproj --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`; it exited `0` with `0` warnings and `0` errors. The verified artifact is
`Build/bin/Terraria.Projectile/Debug/net10.0/Terraria.Projectile.dll`.

The existing `ProjectileCombatVerification` was then built through the same serial wrapper and
exited `0` with `0` warnings and `0` errors. Its `--no-build --no-restore` run exited `0` and
printed `PASS: projectile disposition, penetration, immunity, trail, reflection, collision policy,
network, and animation state boundaries`.

A read-only PowerShell reflection smoke check loaded the verified Projectile artifact and passed
`Opacity(alpha=0) = 1`, `Opacity(alpha=255) = 0`, `Opacity(alpha=127) = 0.5019607543945312`,
`MaxUpdates(extra=0) = 1`, `MaxUpdates(extra=3) = 4`, and the preserved compatibility result
`MaxUpdates(extra=-1) = 0`. This is focused component evidence only; it does not verify setters,
runtime writer ordering, substep scheduling, network/persistence projection, rendering, or full
Version4 behavior equivalence. The P15 partition remains incomplete and the top-level statuses
remain `implementationStatus: partial` and `verificationStatus: partial`.

## 15. Component-Only Continuation Audit

The remaining P15 report groups were reconciled against the actual component files under
`src/Projectile`. The component-owned members are already represented by the existing narrow
boundaries: identity and owner values, disposition, definition reference, raw/local AI, lifetime
and cooldown state, damage payload/policy, penetration and immunity state, trajectory and trail
state, collision policy, network scheduling state, presentation state, capability markers, and
source metadata.

No additional component was added in this continuation because each remaining candidate crosses a
boundary that the execution design explicitly keeps outside component storage:

- `perIDStaticNPCImmunity` is a shared registry rather than per-projectile state.
- The owner-hit fields already have one storage/writer boundary in
  `ProjectileCollisionPolicyComponent`; adding a separate targeting component before caller
  migration would create duplicate authority.
- `hostileDamageScaling` is an immutable difficulty-curve definition/query, while
  `bonusCritChance` already belongs to `ProjectileDamagePayloadComponent`.
- Geometry, target-selection, fishing/mining, and derived-property gaps are caller-owned queries or
  projections; kite/lightning constants are definition/query values.

This audit changed no `src` file and introduced no System, Query, Command, Adapter, Projection,
test, verifier, or project-file code. Per the current user instruction, no build, test, verifier,
or reflection command was run during this continuation. The P15 implementation and verification
statuses remain partial; the next source change requires an explicit cross-boundary ownership
decision rather than another storage component.

## 16. Current Manual Session Checkpoint

`currentComponent: ProjectileDerivedProperties` is `blocked` for this component-only session.
The eight remaining members are derived views or cross-domain seams and do not define an
independently determinable component. Existing component files already own the determinable P15
state; adding another component would duplicate authority for owner-hit policy, bonus critical
chance, alpha, or update cadence.

Missing owners are the static NPC immunity registry, target-policy integration, localization and
catalog lookup, Player/NPC target snapshots, DD2 cleanup, alpha/`extraUpdates` compatibility
writers, network-section mapping, and geometry/fishing/mining/kite/lightning queries. These require
non-component code or cross-domain ownership and cannot be implemented under the current request.
No `src` file, test, verifier, System, Query, Command, Adapter, Projection, or project file was
changed in this session. `completedComponents` and `completedSourceUnits` remain unchanged;
`pendingComponents` remains unchanged; `verificationStatus: not-run` for this session.

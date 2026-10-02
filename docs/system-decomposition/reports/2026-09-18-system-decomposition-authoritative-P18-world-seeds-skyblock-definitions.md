# System Decomposition Report: authoritative P18

taskId: AUTH-SYS-P18
partitionId: P18
sessionId: e4147654f9464b5193bb35f895cdc63f
inputReport: D:/TRbackup/NLTX/docs/migration/ledgers/authoritative-20-partitions/P18-World-Seeds-Skyblock-Definitions.md
prompt: D:/TRbackup/NLTX/docs/component-decomposition/review-round-1/design/2026-09-11-version4-authoritative-20-partition-prompts/2026-09-11-version4-P18-world-seeds-skyblock-definitions-public-decomposition.md
outputReport: D:/TRbackup/NLTX/docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P18-world-seeds-skyblock-definitions.md
designStatus: proposed
verificationStatus: not-run
sourceModified: false
evidenceStatus: partial

## Scope and Evidence

本报告只审查 claim 的 P18，覆盖输入台账中 12 组、86 个字段、50 个属性，共 136 个声明成员。System 会话的 outputReport 以 runner 返回的任务行及 task-state 为准；P18 历史 public-decomposition prompt 指向的是旧 Component 报告位置，不是本次写入授权。

| 输入组 | 字段 | 属性 | System 视角 |
|---|---:|---:|---|
| WorldGenerationSecretSeedFlags | 10 | 0 | 世界生成模式/旗标状态 |
| WorldSecretSeedRegistryDefinitions | 6 | 0 | 静态秘密种子定义登记 |
| WorldSecretSeedVisualAndSurfaceRules | 9 | 0 | 规则定义 |
| WorldSecretSeedTerrainAndStructureRules | 11 | 0 | 规则定义 |
| WorldSecretSeedProgressionAndInfectionRules | 12 | 0 | 规则定义 |
| WorldSecretSeedSeasonalRules | 3 | 0 | 规则定义 |
| WorldSecretSeedRuntimeRegistry | 2 | 1 | 启用集合及派生计数 |
| WorldSecretSeedDerivedOptions | 0 | 2 | 条件/派生读取 |
| WorldSecretSeedDerivedVariations | 0 | 22 | 条件/派生读取 |
| WorldSkyblockGenerationRules | 11 | 3 | 扫描结果、派生规则与可观察效果 |
| WorldSeedOptionCatalog | 1 | 21 | 选项定义与全局选择 API |
| WorldLandmassAndTreeProfiles | 21 | 1 | 地貌/树木 profile 定义 |

Source facts 与设计状态分开记录。P18 的方法调用只作为解释这 136 个字段/属性实际行为的上下文，不因此把 P18 扩展成 WorldGen 全量方法清单。Version4 的关键源码路径为 WorldGen.cs、Main.cs、Terraria.IO/WorldFile.cs、Terraria.IO/WorldFileData.cs、Terraria.WorldBuilding/WorldGenerationOptions.cs、AWorldGenerationOption.cs、WorldSeedOption_Everything.cs 和 LandmassData.cs。Version4 目录没有 Git HEAD；上述文件的 SHA-256 用于标识本次读取的本地样本：

| Source file | SHA-256 |
|---|---|
| D:/TRbackup/Version4/Terraria/WorldGen.cs | A06A8463E39EA065441FF1EF702A787D3450ADAE5CD3065CF30BF76784D3EA1D |
| D:/TRbackup/Version4/Terraria/Main.cs | 66DBE1D1E6FB89A24A512309BB039DFEC7C06D4678D2A22D7ADECBAD3AFD6520 |
| D:/TRbackup/Version4/Terraria.IO/WorldFile.cs | 92DF79BB7AE89393327AE9EF6B7B78762909E5B2E197C0F3FCCFE709E152F289 |
| D:/TRbackup/Version4/Terraria.IO/WorldFileData.cs | 454E025775195D3C93943050F3B3AA022AF2782D9A59F94BDAEA256A368F89C5 |
| D:/TRbackup/Version4/Terraria.WorldBuilding/WorldGenerationOptions.cs | 0AC2FF3D0870D4A827F5B7A6240F0FA25370407105F1CC1C69B23F31CBB5C2A8 |
| D:/TRbackup/Version4/Terraria.WorldBuilding/AWorldGenerationOption.cs | 800309DB63733EFC5D22976B10184F1580284C890F1A0C6C7577EC19C01213C1 |
| D:/TRbackup/Version4/Terraria.WorldBuilding/WorldSeedOption_Everything.cs | 58023D2C7C9CD9C99586E2A47F53364EA8E246E3B54D43725A56DC1864DD0957 |
| D:/TRbackup/Version4/Terraria.WorldBuilding/LandmassData.cs | D856C99EDF524C8BD5585ED392027D3F7DDFD17EDC53B3F6A4C0BCD28A065FD8 |

The read-only CPG reader was initialized from D:/TRbackup/Version4-cpg-export/out-dop8-interproc.sqlite. Health reported import complete, 967 shards, 8,166,789 nodes, 71,038,907 edges, 1,317 diagnostics, manifest SHA-256 6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364 and project fingerprint 521CBD4C11E7EB731256C232350A54099F431D0E59EBDF3C38880281F169C11B. SourceSnapshotId and per-file source hashes are absent from the manifest. The on-disk source hashes above confirm which Version4 files were read but do not bind those files to the CPG graph snapshot.

CPG queries used exact declaration paths and explicit SourcePath scopes. Type-surface queries completed for Terraria.WorldGen.SecretSeed (94 direct members), Terraria.WorldGen.Skyblock (18), AWorldGenerationOption (26), WorldSeedOption_Everything (9), LandmassData (7) and WorldGen.GrowTreeSettings (7). Find-CpgCallSites returned one confirmed Skyblock.ScanTiles caller in Terraria.IO/WorldFile.cs. Scoped member-use queries returned activeSecretSeedCount: 17 uses, all AccessMode Unknown; lowTiles: 4 uses, 2 Write and 2 Unknown; currentActiveTiles: 5 uses, 2 Write and 3 Unknown. Unknown access mode and selected-shard coverage do not establish complete read/write ownership. A broad Reset method-name search also found confirmed WorldGen.Reset call sites in WorldGenerator.cs and WorldGenSnapshot.cs; other same-name symbols had partial/no-hit results. No no-caller claim is drawn from those results.

The target tree was inspected under src/NSSLC/Component/WorldSession/WorldGeneration. Candidate flag, secret-seed registry, Query, scan and commit source files are present. Scoped text search found declarations for the reviewed candidate APIs but no explicit production call sites for the flag component, catalog/derived Queries or the cited registry/Skyblock commit APIs. This is partial static evidence only: dynamic, reflection, generated, serializer, external assembly and runtime edges are not closed.

The selected SS14 reference is C:/Users/shan/Downloads/ECS/space-station-14-master. SharedHandsSystem.cs and SharedHandsSystem.Drop.cs support one System type owning related API/event behavior across partial files; PullingSystem.CanPull illustrates that an eligibility-shaped API may emit a cancellable event. These are organization examples only, not Terraria behavior evidence. The local tModLoader v2026.07 page struct_world_gen_1_1_grow_tree_settings.html confirms the public GrowTreeSettings shape and documented fields; it does not establish Version4's private registration, option or execution semantics. Context/约束/公共拆分约束.md, referenced by the public-decomposition skill, was not present in this checkout; applicable file-organization, API and side-effect rules were read from their available sources.

## Prior Component Decomposition Reconciliation

The same-partition Component design and execution records are docs/component-decomposition/review-round-2/2026-09-11-version4-p18-world-seeds-skyblock-definitions-component-design.md and the matching component-execution.md. They organize the 136 members into definition/query surfaces plus proposed runtime flag, secret-seed registry and Skyblock scan data. Their historical implementation and verification entries were not rerun for this report.

At System level, the 12 inventory groups do not imply 12 runtime Systems. The four rule-definition groups, registry definitions, seed-option definitions and landmass/tree profiles are catalog data. They need no per-world mutable Component and do not each justify a scheduler node. Derived option/variation properties may become pure Query inputs only after their reads of Main, random state, shared state and lazy/mutable collections are explicit.

The prior design leaves canonical selected-flag ownership and persistence/network policy deferred. The current target source includes WorldGenerationSecretSeedFlagsComponent, WorldSecretSeedRuntimeRegistryComponent, WorldSecretSeedRuntimeRegistryCommitSystem, WorldSkyblockGenerationScanSystem and WorldSkyblockGenerationRulesCommitSystem. Presence of those files is current-source evidence, not evidence that WorldFile, WorldGenerator or the scheduler invokes the new composition. The current report therefore retains the previous owner gap and refines the proposal around observable entry paths and commit order; it does not carry forward prior partially-verified status.

## Conceptual Behaviors

| ConceptId | Behavior slice | Observable result and invariant | Evidence |
|---|---|---|---|
| P18.SeedOptionSelection | Resolve seed text/config to an option and expose/change selection | Selected option, enabled flags, registration/display order, option-state notification and later world-generation flag snapshot | WorldGenerationOptions.cs:29-108; AWorldGenerationOption.cs:16-30; WorldFile.cs:691-700 |
| P18.SecretSeedActivation | Register definitions, resolve secret-seed text, enable/disable/clear active entries | Registered definitions remain available; enabled state and active count change once per transition; optional sound only on a new enable | WorldGen.cs:489-552; WorldFileData.cs:212-268; Main.cs:2471 |
| P18.SecretSeedRuleEvaluation | Read seed flags/count to derive option and variation properties | Return the same rule result at the same evaluation point; preserve short-circuiting and random draw timing | WorldGen.cs:40-488, including Main.skyblockWorld at line 238, genRand.Next(3) at line 436 and Main.tenthAnniversaryWorld at line 450 |
| P18.SkyblockWorldState | Scan loaded/generated tile and wall presence, derive no-content/low-tile rules, commit results | Tile/wall sets and active count feed rule values; noDungeon may clear dungeon coordinates; lowTiles transition may send world data | WorldGen.cs:3141-3318; WorldFile.cs:783; WorldGen.cs:59149 |
| P18.WorldGenerationCatalogs | Supply option metadata, secret-seed rule definitions, landmass data and tree profiles | Stable lookup keys, metadata, profile membership/order and predicate behavior remain consumable by UI, persistence and generation | WorldGenerationOptions.cs:29-108; WorldSeedOption_Everything.cs:16-44; WorldGen.cs:3834-4006; LandmassData.cs:5-24 |

These are behavior slices, not new public API commitments. In particular, the inventory's DerivedOptions and DerivedVariations properties include global/random dependencies and cannot be bulk-evaluated or cached as a pure Query without changing when state is observed or how random values are consumed.

## State Ownership and Write Closure

| State/effect | Version4 readers and writers | Candidate ownership | Status |
|---|---|---|---|
| Option definitions/order and selected Enabled state | WorldGenerationOptions static registration populates a process-level list; AWorldGenerationOption.Enabled writes _enabled and invokes OnEnabledStateChanged plus a static event only when the value changes. WorldGenerationOptions.Reset enables Normal; SelectOption calls Reset then enables its argument. TryEnablingFlagFrom writes AutoGenEnabled, a distinct field. | Immutable definition/catalog view plus one selection commit owner. Do not expose live option objects or mutable dependency lists as Query results. | Source writes confirmed; event subscribers and lifetime/selection scope partial. |
| SecretSeed definitions and runtime enablement | SecretSeed.Register appends to AllSecretSeeds. CheckInputForSecretSeed may write _plaintext and TextThatWasUsedToUnlock. Enable/Disable update seed._enabled and activeSecretSeedCount; Enable can call SoundEngine.PlaySound. ClearAllSeeds iterates and disables entries; it does not remove registered definitions. | Separate process-level definitions from active runtime state. A single runtime registry owner may commit enable/disable/clear and derive the count from the set. Per-world Component ownership is conditional on resolving the static source's world/session scope. | Direct source-local mutation confirmed; world/session ownership partial. |
| World-generation selection flags | WorldGen.Reset reads the registered Enabled options and assigns the paired Main and WorldGen flags at lines 10156-10165. SecretSeed.InitializeSecretSeeds then writes additional Main secret-seed flags. Many generation methods read these values. WorldFileData also serializes seed/mode information. | One selection/reset composition, with one authoritative write route and compatibility projections only. Final owner crosses P16/P17/P19/P20 and is integration-review. | Local path confirmed; canonical cross-partition owner and restore semantics unresolved. |
| Secret-seed derived values | SecretSeed.Variations reads Enabled state, activeSecretSeedCount, Main flags and, for at least one getter, genRand. WorldGen generation code consumes these values throughout generation. | Pure Query for deterministic rules over an immutable input snapshot; random/global reads must be supplied explicitly at the same call point. Do not eagerly precompute random getters. | Partial: selected getters are direct evidence; full dynamic/caller and repeated-evaluation closure absent. |
| Skyblock scan and derived flags | ScanTiles resets presence arrays and active count, visits the interior tile region, counts active tiles, observes walls even on inactive tiles, then calls Calculate. Calculate derives no-content and lowTiles values, resets scratch arrays/count, can clear Main.dungeonX/Y and conditionally calls NetMessage.SendData(7). A separate WorldGen path also increments currentActiveTiles and writes hasWall/hasTile at line 59149. | One Skyblock state/commit owner must include load-time full scan and generation-time incremental updates, or delegate both through an explicit contract. Query only derives values from one committed snapshot. Coordinate/network effects use explicit ports. | Local paths confirmed; cross-partition incremental writers and effect owner integration-review. |
| Catalog/profile definitions | Options are initialized and registered in static order; duplicate registration throws. WorldSeedOption_Everything.Dependencies lazily caches and returns a mutable List. LandmassData.Top writes Position from RadiusOrHalfSize; GrowTreeSettings includes GroundTest/WallTest delegates and numeric profile fields. | Catalog/Definition values, not mutable per-world Components. Snapshot/copy at adapters; avoid returning aliased mutable collections. Profile predicate execution belongs at the generation boundary with explicit world inputs. | Declaration behavior confirmed; all external consumers and callback effects partial. |

The source-local writers do not resolve cross-partition authority. In particular, assigning a new per-world component while retaining writable Main/WorldGen fields would create two authorities unless a single commit/projection contract and retirement point are defined.

## Boundary Role and Decision

1. Decision: keep the secret-seed rule groups, option metadata, landmass values and tree-profile values as definitions/catalog data. They are not independently scheduled Systems. A shared immutable lookup Query is justified only where multiple decisions reuse the same rule; no Query should return the mutable Everything dependency list or live AWorldGenerationOption instances.
2. Decision: separate secret-seed activation from option catalog/selection. Registration definitions and active enablement have different lifetimes and write behavior. One WorldSecretSeedRuntimeRegistryCommitSystem capability should own enable, disable, clear and active-count consistency; input normalization/translation and sound are explicit adapters/effects. Its per-generation IDs, runtime versions and idempotency map are extra semantics not found in the legacy source; keep them only if the actual caller protocol requires them.
3. Decision: keep Skyblock scan, rule evaluation and commit as one synchronous behavior composition at the current load/generation call sites. A read adapter may observe tiles and walls; a deterministic Query derives the values; the owning System commits scratch/output state, dungeon coordinates and network effects in their established order. Do not create separate scheduled scan and commit nodes solely because current target files have separate class names. A persistent scan Component is justified only if the real scheduler or retry contract requires state to survive the call.
4. Decision: integration-review for selected flag ownership and generation reset/finalization, not a P18-only System decision. Candidate WorldSeedSelectionSystem or WorldGenerationResetSystem work must compose with P19/P20's actual execution owner and P16/P17's shared progression/biome state. Do not write the same flags from both this boundary and a world-generation coordinator.

The strongest alternative is one WorldGenerationSystem owning option selection, secret seeds, rule data and Skyblock. That would centralize many fields but combines UI/config selection, an activation registry, static definitions and a tile scan with different lifetimes and effects. Conversely, creating one System per input group would add forwarding/scheduling structure without distinct ownership. The recommended boundary is the two behavior owners above plus an integration-reviewed selection/reset seam, with catalogs remaining data and no fixed System count tied to inventory groups.

## System API and Legacy Behavior Mapping

| ConceptId / legacy entry points | Proposed API composition | Behavior to preserve |
|---|---|---|
| P18.SeedOptionSelection: WorldGenerationOptions.GetOptionFromSeedText, SelectOption, Reset, AWorldGenerationOption.Enabled, TryEnablingFlagFrom | SeedTextAdapter -> WorldSeedOptionCatalogQuery.Resolve -> WorldSeedSelectionSystem.Select/SetAutoGeneration -> WorldGenerationResetCoordinator.ApplySelection. Selection state and flag commit are distinct calls; the coordinator is integration-review. | Preserve option registration order and matching rules, null/no-match behavior, Reset-before-select order, Enabled transition notification, UI/config metadata, and the distinction between Enabled and AutoGenEnabled. Query returns data; only the selection owner invokes state-changing hooks. |
| P18.SecretSeedActivation: SecretSeed.Register, CheckInputForSecretSeed, Enable, Disable, ClearAllSeeds, InitializeSecretSeeds, FinalizeSecretSeeds; WorldFileData.TryApplyingCopiedSeed/GetSecretSeedCodes | SecretSeedInputAdapter normalizes the external string -> SecretSeedDefinitionQuery resolves plaintext/code -> EnableSecretSeedCommand/DisableSecretSeedCommand/ClearSecretSeedsCommand -> one runtime registry owner -> optional sound port and read-only projection. World-load parsing remains an adapter; initialize/finalize calls are composed at the same generation phases by integration-review. | Preserve regex normalization and existing culture behavior, plaintext-first then transformed-code lookup, success-only unlock text writes, false on blank/unmatched input, transition-only active-count delta, optional playSound behavior, clear-without-unregister, serialized seed text handling and order. Sound is not part of a pure Query. |
| P18.SecretSeedRuleEvaluation: the 2 derived-option and 22 derived-variation properties | WorldSecretSeedDerivedOptionsQuery / WorldSecretSeedDerivedVariationsQuery over one immutable seed/world snapshot plus explicit random input where required. Consumers call the Query at the old evaluation point. | Preserve results, lazy/short-circuit evaluation and exact RNG draw count/order. A query must not read ambient Main/genRand, mutate caches/shared state, or expose live aliases. The current target Query shape does not by itself establish those compatibility properties. |
| P18.SkyblockWorldState: Skyblock.ScanTiles -> Calculate | WorldGridReaderAdapter -> Skyblock scan capability -> WorldSkyblockGenerationRulesQuery.Evaluate -> one Skyblock commit owner -> dungeon/network effect ports. Preserve this as a direct synchronous composition unless execution evidence requires a scheduled boundary. | Preserve bounds [40, maxTiles-40), active-tile counting, walls observed for every scanned cell, tile/wall classification, strict low-tile threshold, scratch reset, noDungeon coordinate clearing and transition-only message send. Preserve load timing after tile load/liquid settling and include generation-time incremental updates. |
| P18.WorldGenerationCatalogs: static WorldGenerationOptions registry; GrowTreeSettings.Profiles; LandmassData | WorldSeedOptionCatalogQuery and WorldTreeProfileCatalogQuery return immutable/catalog values. LandmassDataAdapter copies its value semantics. Tree evaluation remains in the generation owner, receiving tile/wall/random inputs and returning or committing the same result as the legacy delegate path. | Preserve static option/profile keys and order, lazy/effectful boundary behavior, delegate selection and random tree height draw. LandmassData.Top setter's Position round trip remains value-local; do not convert it into a hidden global write. |

All proposed public APIs and composition points are design-only. The current target code has matching candidate definitions, Queries and commit/scan classes, but no reviewed production call path connecting them to the legacy WorldFile/WorldGen entry points. Its WorldSkyblockGenerationRulesQuery also guards WorldTileCount > 0 before computing the low-tile ratio, whereas Version4 divides currentActiveTiles by Main.maxTilesX * Main.maxTilesY directly. The guard may be harmless for valid world dimensions, but it is a behavior difference for an empty/invalid dimension input until the accepted input domain is established. Compatibility is therefore not established.

## Call and Dependency DAG

The confirmed source-level path is:

World creation/config input -> WorldGenerationOptions.GetOptionFromSeedText / copied-seed parsing -> SelectOption or secret-seed enablement -> WorldGen.Reset reads selected options and ActiveWorldFileData.Seed -> Main/WorldGen flag writes -> SecretSeed.InitializeSecretSeeds writes Main secret flags -> Main.rand is reset from that seed -> generation passes consume flags/rules -> SecretSeed.FinalizeSecretSeeds near generation completion.

The load path is:

WorldFile reads world metadata and seed text -> TryApplyingCopiedSeed may reset options, ClearAllSeeds and enable parsed entries -> world tiles load and liquid settling completes -> WorldGen.Skyblock.ScanTiles -> Skyblock.Calculate -> derived Skyblock flags/scratch reset -> optional dungeon coordinate clear -> conditional world-data message.

Relevant inbound/outbound evidence:

- Main.cs:2471 calls CheckInputForSecretSeed from a seed-input flow.
- WorldFile.cs:691 calls TryApplyingCopiedSeed; :699-700 resolves and selects a seed option; :783 invokes Skyblock.ScanTiles after the loading/settling sequence.
- WorldFileData.cs:200-204 exposes secret seed codes from _seedText; :212 onward parses copied seed input; :260-265 resets options, clears enabled seeds and enables parsed seeds.
- WorldGenerator.TryReset and WorldGenSnapshot.Restore call WorldGen.Reset; CPG returned those two scoped call sites with confirmed CallTargets edges. WorldGen.Reset copies option flags at :10148-10165.
- WorldGen.SecretSeed.FinalizeSecretSeeds is called at the generation end path at WorldGen.cs:21648.
- WorldGen.cs:59149 increments Skyblock.currentActiveTiles during another generation path, so the load-time full scan is not the only writer.

The CPG callsite result for ScanTiles was complete for the selected WorldFile.cs shard and identified one caller. Member-use direction for activeSecretSeedCount remained Unknown; the three Skyblock field queries covered only selected files and did not close all readers/writers. CPG call-site APIs omit dynamic dispatch, callbacks/event subscribers and runtime scheduling. Source searches and excerpts therefore supplement, rather than upgrade, those query results.

## Lifecycle and Side Effects

| Phase | Source behavior | Required composition constraint |
|---|---|---|
| Process/type initialization | WorldGenerationOptions static initialization registers options in fixed order. SecretSeed registration populates a static definition list. Duplicate option registration throws. | Keep catalog initialization once and stable; do not recreate it per world or reorder it by file enumeration. Registration/unregistration lifecycle outside the observed paths is unknown. |
| Selection/input | Enabled changes its backing field before invoking OnEnabledStateChanged and the static option-state event. Secret-seed lookup may write unlock text; enabling may play sound. WorldFileData applies parsed options/seeds. | Maintain event count and visible state on success/failure. Event subscriber enumeration, error/rollback semantics and UI projection are incomplete. |
| Generation reset | WorldGen.Reset copies option values into Main and WorldGen flags, then initializes secret-seed Main flags before later world generation state and random use. Reset is also reached from generation reset/snapshot restoration paths. | Keep commit order and snapshot/restore semantics. Never infer scheduler order from file order; bind the flag projection to the actual generation coordinator. |
| Generation execution/finalization | Numerous WorldGen paths read seed Enabled/variation values. FinalizeSecretSeeds runs near the end of a generation path. Skyblock fields can be incrementally written while generation places/observes tiles. | Query inputs and random draws must be evaluated at the old decision point. P19/P20 writers and pass ordering require integration review. |
| World load | WorldFileData can clear/re-enable seed entries; WorldFile invokes a full Skyblock scan after load and liquid settling. | Keep scan after tile data is present and preserve load failure behavior. Unload/reload and simultaneous-world lifetime rules are not established. |
| Save/network | WorldFileData contains serialized seed/mode values and secret-seed text. Skyblock Calculate can send message 7 when lowTiles changes; Enable can play sound. | Save keys/bitfields, network payload and client/server authority stay behind existing adapters. Do not make derived flags independently writable by a Query or projection. |

No evidence here proves multi-world concurrency, thread safety, unload cleanup, exception recovery, retry semantics or persistence/replication for every enabled seed and scan result. The source uses process-wide static registries and flags, while the proposed target uses world/generation-scoped components; changing that scope requires an explicit decision and tests.

## Integration Handoff

subsystemId: WorldGenerationAndEcology.P18.WorldSeedsSkyblockDefinitions
taskNumber: P18
taskId: AUTH-SYS-P18
reportPath: D:/TRbackup/NLTX/docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P18-world-seeds-skyblock-definitions.md

evidenceStatus: partial
nltxStatus: partial
designStatus: proposed
verificationStatus: not-run

confirmedOwners:
- Version4 source-local SecretSeed methods mutate the registered seed enable flags/count and may emit sound.
- Version4 Skyblock.ScanTiles/Calculate mutate scan/derived values and have the local dungeon-coordinate/network effects described above.
- WorldGenerationOptions owns static option registration and Enabled setter/event behavior in the observed source.

proposedTypes:
- WorldSecretSeedRuntimeRegistryComponent and one WorldSecretSeedRuntimeSystem commit owner.
- WorldSeedOptionCatalogDefinition/Query with a separate selection/reset integration seam.
- One SkyblockWorldStateSystem composition with tile-reader and effect ports, plus a deterministic rules Query.
- Immutable secret-seed rule/catalog and tree/landmass profile values; no per-rule System.

sharedTypesForIntegrationReview:
- Main/WorldGen selected flags and their WorldFileData persistence representation.
- World/session/generation identity for the enabled secret-seed set and scan snapshot.
- Main.dungeonX/Main.dungeonY and Skyblock lowTiles/network snapshot.
- Tile/wall IDs, seed option metadata and any persisted/replicated projections.

crossSubsystemReaders:
- WorldGen generation passes and other world-rule/biome consumers reading secret-seed flags or derived variations.
- WorldFile/UI/server-config paths reading option catalog and selection state.
- World loading and consumers reading Skyblock-derived no-content/lowTiles state.

crossSubsystemOwner: integration-review

crossSubsystemWriters:
- WorldFileData/WorldFile seed parsing and option/secret-seed activation.
- WorldGen.Reset and SecretSeed.InitializeSecretSeeds flag projection.
- WorldGen generation paths that increment Skyblock scan counters/presence fields.
- Skyblock.Calculate writes to dungeon coordinates and may emit world-data network state.

orderingConstraints:
- Option selection occurs before WorldGen.Reset consumes Enabled values.
- WorldGen.Reset writes option-derived flags before SecretSeed.InitializeSecretSeeds and generation execution.
- FinalizeSecretSeeds remains at its generation-end point.
- WorldFile's full Skyblock scan remains after world tile load/liquid settling; Calculate derives, resets scan scratch, then applies coordinate/network effects.

boundaryChallenges:
- Do not equate the 12 inventory groups with 12 Systems.
- Resolve process-static versus per-world ownership before treating current components as canonical.
- Keep synchronous Skyblock scan/evaluation/commit in one ordered behavior path unless real scheduling evidence requires multiple nodes.
- Keep definition/catalog Query results immutable and separate from option selection commits and external effects.

evidenceGaps:
- CPG manifest has no source snapshot ID; field AccessMode is Unknown in material uses, and query scopes do not close dynamic/event/runtime edges.
- Current NSSLC candidate APIs have no confirmed production call path in the inspected source tree.
- Event subscribers, full derived-property callers, generator pass scheduling, exception/retry paths and unload behavior are incomplete.
- Persistence, network/client projection and multi-world/session scope are unresolved.
- Context/约束/公共拆分约束.md is missing from this checkout.

blockingDecisions:
- integration-review must select the canonical owner/scope for the option-derived flags and enabled secret-seed runtime set. Options are (a) retain a process-static compatibility facade during staged replacement, minimizing immediate semantic change but preserving singleton limitations; (b) promote state to a world-session Component, requiring identity, persistence and network migration; or (c) snapshot process selection into a generation-scoped immutable input, separating UI selection from execution but requiring reset/restore coordination.
- integration-review must assign Skyblock incremental generation writes, post-load scan, dungeon coordinates and lowTiles replication to one commit contract. Options are (a) a P18-owned world-state System with P19/P20 callers, (b) ownership inside the generation execution coordinator, or (c) WorldFile-owned post-load orchestration delegating state/effects. The choice changes lifecycle and ordering and cannot be finalized from P18 alone.

notImplemented:
- Production wiring from WorldFile/WorldFileData/WorldGenerator to the current NSSLC candidate APIs is not confirmed.
- This report session did not modify production code or tests.

verifierPlan:
- Seed option parsing/default/no-match, registration order, Enabled event transitions and AutoGenEnabled separation.
- Secret-seed empty/plaintext/transformed-code inputs, duplicate enable/disable, clear-without-unregister, count invariants, unlock-text writes and optional sound count.
- Derived properties against fixed world/seed snapshots, including exact random input/draw order and repeat-call stability for non-random results.
- Skyblock scan border bounds, active tile versus wall observation, no-content classifications, below/equal/above 10 percent cases, incremental generation writes, scratch reset, dungeon coordinate clearing and transition-only network effect.
- World save/load round trips for seed text and serialized flags, including reset/restore paths and repeated world loads.

Integration Handoff is a proposal; it does not decide cross-partition owner, persisted schema or runtime order.

## Migration Behavior Contract

Design comparison must use the same seed input and world context and cover: result or rejection; exception timing; authoritative state delta; option/secret-seed event and sound count; network/save effects; evaluation/commit order; world/session scope; and duplicate/retry behavior. Equal final flags alone are insufficient if the new path changes intermediate visibility, event order, active count, random draws, saved seed text, tile scan timing or the number of network messages.

Proposed sequence:

1. Freeze the observed Version4 behavior through narrow source-to-concept mappings and identify the actual current NLTX callers/writers before changing any legacy entry point.
2. Represent static option/seed/tree/landmass definitions as immutable catalog values. Keep legacy registration keys/order and adapter behavior during the transition.
3. Route secret-seed input to a resolver and one active-state owner. Preserve source transition guards and sound timing; do not add idempotency keys unless the actual request transport requires them.
4. Bind selection and flag projection to the world-generation reset owner after integration assigns shared flags and restore paths. Avoid dual-writing Main/WorldGen fields and Components.
5. Replace Skyblock scan/derive/commit as one synchronous call composition at the load point, while accounting for generation-time incremental writes and using explicit ports for tile reads, coordinates and network.
6. Retain compatibility adapters until focused behavior scenarios exercise the new owners and all relevant observations. Only then decide whether the legacy static facade can be removed.

Every step remains a plan. No implementation or behavior-equivalence claim follows from this report.

## Evidence Gaps and Blocking Decisions

1. Canonical ownership of selected world-generation flags and secret-seed activation is unresolved across P16/P17/P19/P20. The current source is static/process-wide; current target candidate components add generation/world identity. A world/session owner cannot be inferred from the field grouping.
2. Skyblock presence/counter fields have a load-time full scan and generation-time incremental writers. The cross-partition ordering and single write owner for dungeon coordinates, lowTiles replication and derived snapshots are not closed.
3. CPG provides direct surfaces and scoped relationships, but not a source-bound snapshot, complete heap aliases, event subscribers, reflection/configuration edges or runtime schedule. Unknown AccessMode values must remain unknown.
4. AWorldGenerationOption.Enabled invokes a static event after changing its state. The full subscriber set and failure behavior are unresolved. WorldSeedOption_Everything.Dependencies is lazily initialized and exposes a mutable list; whether callers mutate it is not established.
5. Derived variation and tree-profile callbacks may read Main, random state or tile data. A pure Query boundary requires explicit inputs and preservation of evaluation timing. The tree profile consumer/delegate closure is incomplete.
6. Current target candidate sources exist but scoped searches did not establish production call-site wiring or behavior parity. Historical P18 component reports include earlier compile/verifier statements; they were not rerun and do not change this report's verificationStatus.

These gaps do not prevent a proposed boundary for immutable definitions, one secret-seed activation capability and one Skyblock ordered composition. The two owner decisions in Integration Handoff block final canonical ownership and any migration-success conclusion.

## Verification Plan

verificationStatus: not-run
designStatus: proposed

This session used read-only source inspection, the read-only CPG Query API, and bounded static reference searches. It did not run a build, test, behavior verifier or application runtime. It did not modify src/, Test/, project files, input ledgers, prompts or runner state by hand. Runner settlement only records completion of this report task and is not evidence of implementation or equivalence.

The next authorized implementation review should first settle the two Integration Handoff decisions, then run the focused verifier scenarios listed there through the actual new call paths. Required acceptance must compare persisted seed representation, authoritative state, random draw sequence, event/effect count, order/visibility and world/session lifecycle. No migration-success status is justified until those real behavior checks pass.

本报告仅基于 Version4 本地源码样本、tModLoader v2026.07 公开 API 页面和有限 SS14 组织参考形成 P18 的 proposed System 边界与组合设计。它不是迁移完成报告、行为等价证明、API 兼容证明，也不是当前 NLTX 已实现能力的声明。

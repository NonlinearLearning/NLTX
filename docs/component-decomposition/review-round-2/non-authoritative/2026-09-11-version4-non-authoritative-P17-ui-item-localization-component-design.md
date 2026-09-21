# P17 UI 物品、排序与本地化 proposed component design

partitionId: P17
sessionId: 4aa4d033449e43359ca5e2ca42b5ea3c
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\17-ui-item-localization.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P17-ui-item-localization-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P17-ui-item-localization-component-execution.md
designStatus: proposed
executionStatus: planned
implementationStatus: in-progress
verificationStatus: not-run
completedComponents:
- C01 AchievementProgressAndPresentation (implemented; verifier pending)
currentComponent: C02 LocalizationCultureAndLanguage
pendingComponents:
- C02 LocalizationCultureAndLanguage
- C03 LocalizedTextNetworkValue
- C04 LegacyLanguageCatalog
- C05 ItemAppearanceAndTooltip
- C06 CreativePowerUiCatalog
- C07 ItemSlotContextDisplayAndPulse
- C08 ItemSlotTransferCommand
- C09 ItemSortingRegistryAndCatalog
- C10 ItemSortingExecutionWorkset
lastCheckpointUtc: 2026-09-12T02:03:58Z
evidence-gap:
- The repository guidance references `约束/公共拆分约束.md`, but that file is absent from the checkout; the available guidance and public-decomposition references were used and the missing shared constraint remains an evidence gap.
- The P17 first-round output report named by the P17 prompt does not exist in the checkout; the input partition report is the only P17 inventory input available.
- Version4 ItemSlot contains declarations and static initialization but omits the operational slot interaction methods, so complete UI readers, writers, and event timing are partial or missing.
- Version4 ItemTooltip.ValidateTooltip, Achievement persistence bodies, several tracker methods, VariableText condition evaluation, and some value-object methods are stubbed; the proposed design does not infer omitted behavior.
- Current NLTX src/Test contains no matching P17 implementation names or localization/item-slot/sorting implementation; this is an absence-of-match observation, not a capability claim.
- P17 inventory member source rows are declaration evidence; cross-callers and persistence/network ownership still require implementation-stage closure.
- `LanguageManager.GetText` creates a missing-key `LocalizedText` in the Version4 slice, so a future pure lookup query must not silently replace this compatibility side effect.
blocking-decision:
- The final owner of achievement progress and item slot transfer must be integrated with the player, inventory, content-catalog, network, and persistence partitions; this session proposes candidates only.
- The final contract for language-change invalidation and tooltip rebuild timing must be selected by integration review.
- The final representation of ItemInstanceId, PlayerEntityId, item slot reference, network id, and persistence id is crossSubsystemOwner: integration-review.

## 1. Scope and exclusions

This document is a non-authoritative, proposed design for the 22 leaf groups and 296 member
records in input report P17. It distinguishes Version4 facts, the observed current NLTX state,
and a later implementation plan. Nothing in this document means that a component, system, query,
command, adapter, projection, path, namespace, migration, or behavior-equivalence test exists.

In scope:

- achievement definitions, progress trackers, transient achievement event context, icon lookup,
  and achievement save projection;
- culture registration, language loading, localized values, variable substitutions, legacy Lang
  catalogs, and network-localized text;
- item appearance fields that are actually part of an item instance or content definition,
  tooltip cache/projection, and the Creative Power UI catalog/layout;
- ItemSlot context constants, display/highlight worksets, transfer payloads, and UI command
  boundaries;
- ItemSorting layer definitions, ranking/whitelist registries, and one-sort execution worksets.

Excluded from this partition:

- the authoritative player inventory/equipment/container owner;
- item creation, item identity, item network serialization, player input, content definitions,
  achievements outside the listed Version4 members, and final client rendering;
- any final cross-partition owner, network protocol, persistence schema, or scheduler order.

All excluded shared boundaries use `crossSubsystemOwner: integration-review`.

## 2. Evidence and status rules

### 2.1 Version4 facts

The validated inventory is `docs/migration/ledgers/non-authoritative-component-partitions/17-ui-item-localization.md`,
source SHA-256 `b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`, with 22 leaf
groups and 296 records. The following direct Version4 evidence was read:

| Evidence | Confirmed fact | Status |
|---|---|---|
| `D:\TRbackup\Version4\Terraria.Achievements\Achievement.cs:9-46` | Achievement definitions hold names, localized name/description references, a construction-time local Id, and a condition map. | confirmed |
| `D:\TRbackup\Version4\Terraria.Achievements\AchievementCondition.cs:6-30` | Conditions expose completion and tracker seams, with JSON opt-in on completion; lifecycle methods are present but empty in this Version4 slice. | partial |
| `D:\TRbackup\Version4\Terraria.Achievements\AchievementManager.cs:16-84` | Achievement registry, icon indexes, save path/cloud flag, crypto key, serializer settings, and an I/O lock are manager state; `Save(path, cloud)` is empty in this slice. | partial |
| `D:\TRbackup\Version4\Terraria.Achievements\AchievementTracker.cs:5-51` | Tracker value/max/name/type are behavior/progress state; `SetValue` reports updates and invokes completion at max. Several interface methods are empty. | partial |
| `D:\TRbackup\Version4\Terraria.GameContent.Achievements\AchievementsHelper.cs:15-150` | mining and Mecha Mayhem flags are mutable transient context; event methods call progression/network effects and mutate those flags. | confirmed for shown paths; broader readers partial |
| `D:\TRbackup\Version4\Terraria.GameContent.Achievements\CustomFloatCondition.cs:7-47` | float value is clamped, forwarded to a tracker, and completes at max; the value has a JSON property. | partial |
| `D:\TRbackup\Version4\Terraria\Player.cs:17619`, `Terraria\Projectile.cs:13520-13584`, `Terraria\NPC.cs:66205-66223` | player movement, projectile mining, and NPC events feed achievement helper paths. | confirmed |
| `D:\TRbackup\Version4\Terraria\Main.cs:26400`, `Terraria\Program.cs:178-207` | achievement save and startup localization/legacy initialization are invoked from lifecycle code. | confirmed |
| `D:\TRbackup\Version4\Terraria.Localization\GameCulture.cs:7-146` | culture lookup has named and legacy registries, a default culture, `CultureInfo`, and legacy ids. | confirmed |
| `D:\TRbackup\Version4\Terraria.Localization\LanguageManager.cs:20-118,152-217,220-417` | language changes reset/reload values, update active culture, invoke `OnLanguageChanged`, load embedded/content-source files, and expose pure-looking lookup methods over mutable registries. | confirmed for shown implementation; failure behavior is partial |
| `D:\TRbackup\Version4\Terraria.Localization\LocalizedText.cs:8-164` | a localized key has mutable current value, English baseline, variable formatting, conditions, and conversion to a NetworkText key. | confirmed; helper lookup implementation partial |
| `D:\TRbackup\Version4\Terraria.Localization\NetworkText.cs:6-140` | network text separates literal/formattable/localization-key modes and serializes substitutions. | confirmed |
| `D:\TRbackup\Version4\Terraria.Localization\VariableText.cs:8-107` | variable text parses placeholders and conditions and formats through a supplied lookup; condition evaluation body is stubbed. | partial |
| `D:\TRbackup\Version4\Terraria\Lang.cs:17-357` | legacy arrays and item/name/tooltip caches are populated from language keys; `GetTooltip` and item-name lookups return cached projections. | confirmed for shown paths |
| `D:\TRbackup\Version4\Terraria\Item.cs:98-220,317-337,1020-1027,48380-48468` | item appearance/tool/tooltip fields are public item state; `Name`, `PaintOrCoating`, and `RebuildTooltip` cross into localization/tooltip state; reset/default paths write several fields. | confirmed for shown paths |
| `D:\TRbackup\Version4\Terraria.UI\ItemSlot.cs:23-355` | contexts, transfer/pulse value types, UI arrays, glow state, and static initialization are declared; operational slot methods are absent from this slice. | partial |
| `D:\TRbackup\Version4\Terraria.UI\ItemTooltip.cs:5-42` | tooltip can be built from a language key and carries raw/processed text and a validator key; validation body is empty. | partial |
| `D:\TRbackup\Version4\Terraria.UI\ItemSorting.cs:8-1307` | sorting layers classify `Item[]` candidates, stable-sort indices, build whitelists and item-type layer indexes; setup is called by `Terraria\Main.cs:3416`. | confirmed for shown implementation |
| `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowersHelper.cs:12-81`, `CreativePowerUIElementRequestInfo.cs:3-8` | Creative Power icon coordinates, texture dimensions, selected color, and preferred UI dimensions are presentation/catalog state. | confirmed |

### 2.2 Cross-check sources

The local tModLoader mirror is `D:\TRbackup\tmodloader-api-docs-stable`, page header
`tModLoader v2026.07`. Actual pages read were:

- `class_game_culture.html`, which exposes `FromLegacyId`, `DefaultCulture`, and `KnownCultures`;
- `class_localized_text.html`, including `FormatWith` and `ToNetworkText`;
- `class_network_text.html`, whose detailed description distinguishes `FromKey` for localized
  network text from `FromLiteral` for non-localizable text, and documents `Serialize`;
- `class_item_tooltip.html`, which exposes `FromLanguageKey` and `FromLocalization`.

These pages cross-check public boundary semantics only. Version4 remains the behavior and
coverage authority.

The SS14 reference was read only for structure: `Content.Shared\Localizations\ContentLocalizationManager.cs`
shows an injected localization port and explicit culture loading; `Content.Shared\Inventory\InventorySystem.Slots.cs`
shows inventory slots as component-owned relations with pure lookup methods and explicit update
events; `Content.Client\Inventory\ClientInventorySystem.cs` shows client slot state as a
projection/event consumer and UI actions as explicit network requests. SS14 does not establish
Terraria behavior, names, paths, or ownership.

### 2.3 Current NLTX state

The current `src/` tree contains domain projects such as `Items`, `Player`, and `Share`, but an
exact search for the P17 Version4 type names and the P17 localization, ItemSlot, tooltip, sorting,
Creative UI, and achievement implementation names did not find a matching implementation in
`src`, `Test`, or `dome/src`. Existing item verification projects do not establish that this
partition is implemented. Therefore the current NLTX status for every proposed P17 design is
`not-found-in-current-sources`, not `implemented`.

## 3. Proposed decomposition and ordering

The proposed design uses domain-first paths and keeps UI projections separate from item,
inventory, achievement, and network authority. Proposed paths are names only; no such files are
claimed to exist.

```text
Achievement definitions/progress
  -> achievement event commands
  -> deterministic progress system
  -> presentation projection and persistence adapter

Culture/language catalog
  -> localized value query
  -> tooltip/legacy catalog projection
  -> client UI projection

Item authority + inventory relation
  -> ItemSlot transfer command
  -> inventory/equipment owner (integration-review)
  -> slot display/highlight projection

Item definition catalog + item snapshot
  -> sorting registry query
  -> one-sort execution workset
  -> UI order projection
```

Proposed scheduler contract, subject to integration review:

1. `LanguageCatalogLoadSystem` establishes culture and text values before any client text or
   tooltip projection is rebuilt.
2. `AchievementEventIngestionSystem` accepts domain events, then
   `AchievementProgressSystem` mutates only achievement progress ownership.
3. `ItemTooltipProjectionSystem` reads item state and localized keys after language change and
   item revision changes; it never writes item inventory or equipment authority.
4. `ItemSlotTransferCommandSystem` validates and submits slot intents to the inventory owner;
   the UI projection is refreshed only after the owner returns an accepted result/event.
5. `ItemSortingRegistryBuildSystem` runs after item/content definitions are available and before
   `ItemSortingQuery`; `ItemSortingExecutionSystem` reads a snapshot and writes only its
   transient workset/result.
6. Client draw/projection systems consume snapshots after all preceding invalidations. File or
   directory order is not a scheduler contract.

## 4. Checkpoint C01: proposed achievement design

### 4.1 Proposed state roles

| Proposed role | Scope | Proposed contents | State kind | Single writer candidate | Status |
|---|---|---|---|---|---|
| `AchievementDefinitionCatalogComponent` | world/session singleton or catalog entity | stable achievement name, local definition id, localization keys, condition descriptors, icon index | definition/catalog; localized fields are references, not rendered strings | `AchievementDefinitionRegistrationSystem` | proposed |
| `AchievementProgressComponent` | player-achievement-condition relation; exact entity/relationship owner is integration-review | completion bit, tracker value, max value, tracker type/name, version | authoritative progress plus derived completion projection | `AchievementProgressSystem` | proposed |
| `AchievementEncounterContextComponent` | world/session or event scope | mining flag and Mecha Mayhem eligibility/down flags | transient behavior state; not a save snapshot by default | `AchievementEventIngestionSystem` | proposed |
| `AchievementPresentationSnapshot` | client/player projection | display name/description key, completion, current/max, icon index | snapshot/projection; not authoritative | `AchievementPresentationProjection` | proposed |
| `AchievementSaveSnapshot` | persistence boundary | condition JSON-compatible progress only, schema/version, player persistence id | persistence snapshot | `AchievementPersistenceAdapter` | proposed |

`LocalizedText`, `AchievementProgressId`, player/entity ids, icon ids, and achievement network
messages are shared candidates. Their final owner is `crossSubsystemOwner: integration-review`.

### 4.2 Proposed behavior boundaries

| Proposed boundary | Reads | Writes/effects | Failure/retry rule | Status |
|---|---|---|---|---|
| `AchievementEventIngestionSystem` | explicit mining, movement, NPC-killed, progression, and special-event inputs | emits progress commands; updates only transient encounter context | duplicate event ids must be deduplicated by integration owner; current Version4 does not prove delivery semantics | proposed |
| `AchievementProgressSystem` | `AchievementProgressComponent`, definition descriptors, accepted commands | clamps numeric progress, emits completion event once, updates completion state | rejected unknown condition is a result, not an exception; completion idempotency requires a versioned owner | proposed |
| `AchievementProgressQuery` | immutable definition/progress snapshot | returns completion/current/max/icon lookup; no writes | deterministic and side-effect free | proposed |
| `AchievementPresentationProjection` | progress query and localized text query | emits client-only display snapshot/UI invalidation | stale language revision causes rebuild; no reverse write | proposed |
| `AchievementPersistenceAdapter` | save snapshot and selected storage port | file/cloud I/O, encryption, load/save diagnostics | serialize once per accepted revision; retry only at adapter boundary; unknown timeout is not success | proposed |

### 4.3 C01 member coverage

Every C01 member from the input report is assigned below. The source line is the Version4
declaration location where it was rechecked; lifecycle or reader claims marked `partial` remain
open because the Version4 slice is stubbed or the direct reader is outside the listed file.

| Input group | Version4 member coverage | Proposed role and evidence status |
|---|---|---|
| `SharedAchievementProgressSupport` | `Terraria.GameContent.Achievements.AchievementsHelper._isMining`, `mayhemOK`, `mayhem1down`, `mayhem2down`, `mayhem3down` (`AchievementsHelper.cs:15-23`); `CurrentlyMining` (`AchievementsHelper.cs:25-35`); `Terraria.GameContent.Achievements.CustomFloatCondition._value`, `_maxValue` (`CustomFloatCondition.cs:9-12`); `Value` (`CustomFloatCondition.cs:14-33`) | `_isMining`/`CurrentlyMining` and four mayhem flags map to proposed `AchievementEncounterContextComponent` as transient behavior state; `_value` maps to `AchievementProgressComponent` authoritative progress and `_maxValue` to definition/progress constraint. Writes are confirmed in shown helper/condition methods; broader readers, save timing, and event delivery are partial. |
| `AchievementPresentationState` | `Achievement._totalAchievements`, `Name`, `FriendlyName`, `Description`, `Id`, `_conditions` (`Achievement.cs:14-29`); `AchievementCondition.Name`, `_tracker`, `_isCompleted`, `IsCompleted` (`AchievementCondition.cs:9-18`); `AchievementManager.StoredAchievement.Conditions`, `_savePath`, `_isCloudSave`, `_achievements`, `_serializerSettings`, `_cryptoKey`, `_achievementIconIndexes`, `_ioLock` (`AchievementManager.cs:18-36`); `AchievementTracker<T>._value`, `_maxValue`, `_name`, `_type` (`AchievementTracker.cs:7-13`); `AchievementCondition.IsCompleted` (`AchievementCondition.cs:18`) | definition fields map to `AchievementDefinitionCatalogComponent`; condition/tracker fields map to `AchievementProgressComponent`; `StoredAchievement.Conditions` maps only to `AchievementSaveSnapshot`; path/cloud/serializer/key/lock map to `AchievementPersistenceAdapter` state, never a simulation component; icon indexes map to presentation catalog/projection. `Id` is a local definition id, not a network or persistence id. Completion is persisted candidate but exact transaction boundary is integration-review. |

### 4.4 C01 invariants and lifecycle

- A progress update must target one player-achievement-condition relation and must not mutate the
  item, inventory, player movement, or network state directly.
- Numeric progress is in `[0, maxValue]`; completion is true only after the accepted value reaches
  the definition maximum. The Version4 clamp and completion path is at
  `CustomFloatCondition.cs:20-31`.
- `AchievementEncounterContextComponent` is created with a session/world lifecycle, reset on
  encounter start/clear, and removed at session teardown. It is not included in persistence unless
  integration review proves that a partially completed encounter must survive restart.
- Progress is loaded before presentation projection and saved after an accepted progress revision.
  Save completion and cloud/file selection are adapter effects; the Version4 `Save` body is not
  sufficient evidence for atomicity.
- `AchievementPresentationSnapshot` may be rebuilt on language revision or progress revision and
  is cleared on client/session teardown.

### 4.5 C01 implementation checkpoint

The proposed C01 boundary is implemented under `src2/UiItemLocalization` with the following
source files:

- `Achievements/AchievementDefinition.cs`, `AchievementConditionDefinition.cs`,
  `AchievementDefinitionCatalog.cs`, and `AchievementDefinitionRegistrationSystem.cs`;
- `Achievements/AchievementProgressState.cs`, `AchievementProgressCommand.cs`,
  `AchievementProgressSystem.cs`, `AchievementProgressUpdateResult.cs`,
  `AchievementProgressQuery.cs`, and `AchievementProgressSnapshot.cs`;
- `Achievements/AchievementEncounterContextComponent.cs`;
- `Achievements/AchievementPresentationSnapshot.cs`,
  `AchievementPresentationProjection.cs`, and `IAchievementTextResolver.cs`;
- `Achievements/AchievementConditionSaveSnapshot.cs`, `AchievementSaveSnapshot.cs`,
  `LocalAchievementId.cs`, and `PlayerEntityId.cs`;
- `Infrastructure/Persistence/AchievementPersistenceAdapter.cs` and
  `Infrastructure/Persistence/IAchievementPersistenceStore.cs`.

The implementation preserves the proposed boundaries: progress is clamped and written only by
`AchievementProgressSystem`, presentation is a one-way snapshot, and persistence is delegated to
an explicit store port. The focused verifier has not run yet. Version4 persistence bodies and the
final player/persistence owner remain evidence gaps; no behavior-equivalence claim is made.

## 5. Checkpoint C02: proposed culture and language design

### 5.1 Proposed roles

| Proposed role | Scope | Proposed contents | State kind | Single writer candidate | Status |
|---|---|---|---|---|---|
| `CultureRegistryCatalog` | process/runtime catalog | named cultures, legacy id map, `CultureInfo`, default culture, culture name/active derivations | definition/catalog; `CultureInfo` is an external platform value behind an adapter | `CultureRegistryInitializationSystem` | proposed |
| `LanguageCatalogState` | process/client language service | localized key map, category index, variation map, fallback culture, content sources, active culture, revision | mutable runtime cache/catalog; not ECS entity authority | `LanguageReloadSystem` | proposed |
| `SetLanguageCommand` | application/UI intent | requested legacy id, culture name, or proposed culture key plus expected language revision | transient command | `LanguageCommandSystem` submits to reload system | proposed |
| `LanguageChangeEvent` | runtime event | old/new culture identity, language revision, reload result | event/snapshot invalidation | `LanguageReloadSystem` | proposed |
| `LocalizationContentAdapter` | infrastructure | embedded/content-source file enumeration, CSV/JSON parsing, asset errors | external adapter; no third-party/content-source object in core state | adapter boundary | proposed |

The proposed catalog is process/client scoped, not a world ECS component. If the final ECS
runtime requires an entity, it must be a singleton entity explicitly owned by the localization
domain; no generic `Shared/Components` container is proposed.

### 5.2 C02 member coverage

| Input group | Version4 members and declaration evidence | Proposed role and lifecycle |
|---|---|---|
| `LocalizationCultureAndLanguageState` | `GameCulture._NamedCultures`, `_legacyCultures`, `CultureInfo`, `LegacyId`, `DefaultCulture`, `IsActive`, `Name` (`Terraria.Localization.GameCulture.cs:26-38`); `LanguageManager.Instance`, `_localizedTexts`, `_categoryGroupedKeys`, `_textVariations`, `_fallbackCulture`, `_contentSources`, `VariationSeparatorSign`, `ActiveCulture` (`Terraria.Localization.LanguageManager.cs:22-38`); `Language.ActiveCulture` (`Terraria.Localization.Language.cs:6-8`) | `_NamedCultures`/`_legacyCultures` and `CultureInfo`/`LegacyId` map to `CultureRegistryCatalog`; `DefaultCulture`, `IsActive`, `Name`, and `Language.ActiveCulture` are derived/read-only views except the default setter, so they map to a culture query/projection. `Instance`, text/category/variation maps, fallback, content sources, separator, and active culture map to `LanguageCatalogState`/adapter configuration. `ActiveCulture` is written by `LoadLanguage` (`LanguageManager.cs:109-118`), while maps are mutated by file/content loading and text update (`LanguageManager.cs:220-305`). Status: confirmed for shown paths, content failure/registration timing partial. |

### 5.3 C02 system and query contract

| Proposed boundary | Reads | State transition/effect | Purity/status |
|---|---|---|---|
| `CultureRegistryInitializationSystem` | fixed culture definitions and platform culture adapter | registers named/legacy lookup, selects default culture | initialization effect; proposed |
| `LanguageCommandSystem` | `SetLanguageCommand`, current language revision | resolves requested culture through registry; rejects unknown input or normalizes to Version4 fallback behavior | command boundary; proposed |
| `LanguageReloadSystem` | resolved culture, fallback culture, content adapter | sets thread current culture/UI culture, resets/reloads text values, clears variations, loads embedded/content-source files, processes copy commands, publishes `LanguageChangeEvent` | side-effecting system; proposed |
| `LocalizedTextLookupQuery` | immutable language snapshot | returns existing text/value or explicit missing-key result | pure only if it does not create keys; proposed |
| `LanguageCatalogProjection` | language snapshot and revision | publishes client invalidation and diagnostic counts | output only; proposed |

Version4 `SetLanguage(GameCulture)` only reloads when the requested culture differs from the active
culture (`LanguageManager.cs:78-88`). `ReloadLanguage` resets to keys/fallback, loads target text,
then invokes `OnLanguageChanged` (`LanguageManager.cs:90-107`). This ordering is a proposed hard
contract for compatibility, but the event payload/revision must be decided by integration review.

### 5.4 Invariants, side effects, and failure ownership

- The culture registry must have one default culture and a deterministic legacy-id fallback. The
  Version4 `FromLegacyId` path normalizes ids below one and falls back when a key is absent
  (`GameCulture.cs:51-64`).
- A language revision increments only after the reload outcome is known. A failed content file
  must not publish a successful revision; Version4 currently logs and breaks for embedded file
  errors (`LanguageManager.cs:120-149`) and forwards content-source errors to an asset handler
  (`LanguageManager.cs:178-218`). The exact proposed result type remains integration-review.
- Thread culture changes are external process effects and belong to the reload adapter/system,
  never a query or component constructor.
- Existing `LocalizedText` identity may be observable because the manager mutates objects in place.
  The compatibility adapter must either preserve object identity or prove that all readers use a
  revisioned lookup.
- The content-source list is an adapter-owned dependency. It must not expose `IContentSource` to
  proposed core components.
- A missing text key has two possible contracts: preserve Version4's create-on-read behavior, or
  add a side-effect-free `TryGet` and keep creation behind an explicit `RegisterMissingKey`
  command. This is a blocking compatibility decision for integration review.

### 5.5 C02 checkpoint handoff

`C02 LocalizationCultureAndLanguage` is complete as a proposed design checkpoint. Culture lookup,
language reload, external content loading, and language-change invalidation are now separated;
localized value formatting was subsequently separated into C03. No implementation or verifier ran.

## 6. Checkpoint C03: proposed localized value and network text design

### 6.1 Proposed roles and boundaries

| Proposed role | Scope | Responsibility | State kind | Single writer candidate | Status |
|---|---|---|---|---|---|
| `LocalizedTextValue` | shared runtime value object | localized key, current value, English baseline, variable formatting entry point | value object/cache; not item or UI authority | `LanguageReloadSystem` through a value update adapter | proposed |
| `VariableTextTemplate` | shared formatting value | parsed format, substitution names, condition descriptors, reusable buffers | transient formatting state | `VariableTextFactory` and `VariableTextQuery` | proposed |
| `NetworkTextPayload` | protocol boundary | literal, formattable, or localization-key mode and nested substitutions | wire value object | `NetworkTextSerializationAdapter` | proposed |
| `LocalizedTextLookupQuery` | read-only | resolves key/value and condition result against a language snapshot | pure query | none | proposed |
| `LocalizedTextRenderProjection` | client/UI boundary | turns a resolved value into display text for one revision | projection | client projection system | proposed |
| `NetworkTextAdapter` | transport boundary | serializes/deserializes protocol values without rendering them | external adapter | transport owner | proposed |

`LocalizedTextKey`, language revision, network id, entity id, and persistence id remain separate
cross-partition values with `crossSubsystemOwner: integration-review`. A rendered string is never a
replacement for a localization key in shared or network state.

### 6.2 C03 member coverage

| Source members and evidence | Proposed affiliation | Boundary note |
|---|---|---|
| `LocalizedText.Empty`, `_substitutionRegex`, `Key`, `_value`, `_propertyLookupCache` (`Terraria.Localization/LocalizedText.cs:8-27`) | `LocalizedTextValue`; regex and reflection cache are internal formatting caches | `Empty` is a sentinel value object; `_value` is updated from the language catalog, while `_propertyLookupCache` is never persisted or sent on the wire |
| `LocalizedText.Value`, `UnformattedValue`, `EnglishValue`, `ConditionsMet` (`LocalizedText.cs:29-164`) | `LocalizedTextLookupQuery` and `LocalizedTextRenderProjection` | these are derived reads; the condition result is partial because the underlying variable-condition path is stubbed in the Version4 slice |
| `NetworkText.Empty`, `_substitutions`, `_text`, `_mode` (`Terraria.Localization/NetworkText.cs:6-20`) | `NetworkTextPayload` and `NetworkTextAdapter` | preserve the mode and substitution tree; the receiver resolves a key or formats a payload in its own culture |
| `VariableText.Condition.RequiredValue`, `Name` (`Terraria.Localization/VariableText.cs:10-15`) | `VariableTextTemplate` condition descriptors | condition evaluation is a pure query candidate, but the Version4 body is incomplete and must remain an evidence gap |
| `VariableText._original`, `_format`, `_conditions`, `_variables`, `_formatArgBuffer`, `_formatBuffer`, `_substitutionRegex` (`VariableText.cs:8-107`) | `VariableTextTemplate` plus a transient formatter workset | parsed format and reusable buffers share a formatting lifecycle; buffers are cleared after one projection and are not component persistence |

All 22 `LocalizedTextValueState` records are represented above: source rows 2774-2791 and
properties 3898-3901. The table intentionally keeps protocol payload, localized value identity,
formatting buffers, and rendered output as separate roles.

### 6.3 Invariants, lifecycle, and ordering

- `LocalizedText.Key` is an identity key, not the current culture's rendered value. The value
  object may retain identity while `LanguageReloadSystem` updates its current and English values.
- `UnformattedValue` and `Value` must not mutate the language catalog. A query may report an
  explicit missing-key result; compatibility creation of a missing key remains behind the C02
  adapter until integration review selects the final contract.
- `VariableTextQuery` accepts a stable lookup snapshot and returns a formatting result. It does not
  write substitutions into the source text or leak its mutable argument buffer.
- `NetworkTextPayload` preserves `Mode`, key/literal text, and recursively serializable
  substitutions. `Serialize` must never send a client-rendered `Value` as if it were a key.
- A language revision invalidates localized and tooltip projections after the catalog commit. A
  failed formatting or serialization attempt returns a failure result without changing the source
  value object.
- Proposed order is language catalog commit -> localized value refresh -> variable formatting
  query or network decode -> client render projection. Transport serialization is an adapter
  boundary and does not depend on client drawing.

Evidence gaps are the stubbed variable-condition evaluation, some value-object helper bodies, and
the exact Version4 network reader/writer call graph outside `NetworkText.Serialize`. No behavior is
inferred from the missing bodies.

## 7. Checkpoint C04: proposed legacy language catalog design

### 7.1 Proposed roles

| Proposed role | Scope | Responsibility | State kind | Single writer candidate | Status |
|---|---|---|---|---|---|
| `LegacyLanguageTextCatalog` | compatibility projection | category arrays `menu`, `gen`, `misc`, `inter`, `tip`, `mp`, chest/dresser arrays, and `prefix` | legacy projection/cache | `LegacyLanguageCatalogProjectionSystem` | proposed |
| `LegacyNameCacheProjection` | compatibility projection | item, projectile, NPC, negative-NPC, buff, and emoji name caches | derived cache | `LegacyLanguageCatalogProjectionSystem` | proposed |
| `LegacyTooltipCacheProjection` | compatibility projection | item tooltip objects derived from current localized values | UI compatibility cache | `LegacyLanguageCatalogProjectionSystem` | proposed |
| `LegacySubstitutionRegistry` | compatibility adapter state | global substitution callbacks and prefix format text | adapter/runtime registry | localization compatibility owner | proposed |
| `PrefixNameProjection` | read-only value | `ItemPrefixCombiner.ItemName` and `PrefixName` display values | derived presentation value | `LegacyLanguageCatalogProjectionSystem` | proposed |

These are compatibility projections over C02/C03 localization state. They are not authoritative
item, NPC, buff, or tooltip components and are not serialized as simulation state.

### 7.2 C04 member coverage

| Source members and evidence | Proposed affiliation | Lifecycle and ownership |
|---|---|---|
| `Lang.menu`, `gen`, `misc`, `inter`, `tip`, `mp`, `chestType`, `dresserType`, `chestType2`, `prefix` (`Terraria/Lang.cs:17-32`) | `LegacyLanguageTextCatalog` | arrays are allocated/filled by legacy initialization and rebuilt after a language revision; callers receive compatibility values, not write access |
| `Lang._itemNameCache`, `_projectileNameCache`, `_npcNameCache`, `_negativeNpcNameCache`, `_buffNameCache`, `_buffDescriptionCache` (`Lang.cs:34-60`) | `LegacyNameCacheProjection` | cache entries are derived by key/id and invalidated together; unknown id behavior must remain explicit rather than returning a stale entry |
| `Lang._itemTooltipCache`, `_emojiNameCache` (`Lang.cs:62-65`) | `LegacyTooltipCacheProjection` and `LegacyNameCacheProjection` | tooltip values are rebuilt from C03 and `ItemTooltip` projection state; no cache entry writes an Item authority field |
| `Lang._globalSubstitutions`, `_prefixFormatText` (`Lang.cs:67-73`) | `LegacySubstitutionRegistry` | callbacks remain an adapter-owned side effect; format text is a localized value reference and is invalidated by language revision |
| `ItemPrefixCombiner.ItemName`, `PrefixName` (`Lang.cs:256-357`) | `PrefixNameProjection` | pure display reads over item/prefix snapshots; item identity and prefix authority remain cross-partition |

All 22 `LegacyLanguageCatalogState` records are covered: source rows 3286-3305 and properties
3970-3971. `Lang.InitializeLegacyLocalization` is evidence for initialization and cache filling;
the complete invalidation and failure behavior remains partial.

### 7.3 Invariants and ordering

- Legacy arrays preserve their category/index compatibility while their values are rebuilt from
  the active language revision. An array index is not an Item, NPC, or persistence id.
- A tooltip cache validator must include the relevant item revision and language revision. It cannot
  become a source of truth for item appearance or inventory contents.
- Global substitutions execute only through an explicit compatibility adapter. Query code must not
  invoke arbitrary callbacks while calculating a pure key lookup.
- Proposed order is C02 language reload -> C03 localized values -> legacy catalog projection -> C05
  tooltip projection. A failed catalog rebuild leaves the last accepted cache generation in place.

## 8. Checkpoint C05: proposed item appearance and tooltip design

### 8.1 Proposed roles and integration review boundaries

| Proposed role | Scope | Candidate contents | State kind | Status |
|---|---|---|---|---|
| `ItemAppearanceSnapshot` | item instance/content boundary | `dye`, `hairDye`, `paint`, `paintCoating`, `color`, `alpha`, `glowMask`, `scale` | appearance state; authority is integration-review | proposed |
| `ItemAudioAndCombatDefinition` | item/content/combat boundary | `UseSound`, `useSoundPitch`, `defense` | content/gameplay state; not a UI component | proposed, `crossSubsystemOwner: integration-review` |
| `ItemTooltipRequestProjection` | client read boundary | `tooltipContext`, `tooltipSlot`, `stringColor`, and item/bestiary display inputs | request/projection state | proposed |
| `ItemTooltipCache` | client-only cache | `Item.ToolTip` compatibility reference and `ItemTooltip` fields | derived cache | proposed |
| `ItemTooltipProjection` | client UI output | localized tooltip lines and processed text | projection | proposed |

`BestiaryNotes` may be content or a localized presentation note; it remains
`crossSubsystemOwner: integration-review` until its save/network and content readers are closed.
`PaintOrCoating` is a derived query over `paint` and `paintCoating`, not a second authority field.

### 8.2 C05 member coverage

| Source members and evidence | Proposed affiliation | Boundary note |
|---|---|---|
| `Item.tooltipContext`, `tooltipSlot` (`Terraria/Item.cs:98-107`) | `ItemTooltipRequestProjection` | UI context/slot inputs; they must not identify or mutate an authoritative inventory slot |
| `Item.dye`, `hairDye`, `paint`, `paintCoating` (`Item.cs:108-116`) | `ItemAppearanceSnapshot` | preserve byte/short compatibility and derive paint state through a query; owner is item/content integration |
| `Item.color`, `alpha`, `glowMask`, `scale` (`Item.cs:156-166`) | `ItemAppearanceSnapshot` | rendering inputs are snapshot data; renderer owns external color/texture effects |
| `Item.UseSound`, `useSoundPitch`, `defense` (`Item.cs:168-178`) | `ItemAudioAndCombatDefinition` | these are not moved into a UI cache; audio/combat owner is unresolved across partitions |
| `Item.stringColor`, `ToolTip`, `BestiaryNotes` (`Item.cs:208-220`) | `ItemTooltipRequestProjection`, `ItemTooltipCache`, and integration-review content note | string color and tooltip are derived presentation; bestiary notes require content/persistence closure |
| `Item.PaintOrCoating` (`Item.cs:1020-1027`) | `PaintOrCoatingQuery` over `ItemAppearanceSnapshot` | derived boolean/value read; no independent writer |
| `ItemTooltip.None`, `_neverUpdateHack`, `_tooltipLines`, `_validatorKey`, `_text`, `_processedText` (`Terraria.UI/ItemTooltip.cs:5-42`) | `ItemTooltipCache` and `ItemTooltipProjection` | cache sentinel, validator, lines, localized source, and processed output are client-only; `ValidateTooltip` is partial/stubbed evidence |

The 17 `SharedItemAppearanceAndTooltipState` records (3177-3239, property 3963) and six
`UiItemTooltipState` records (4428-4433) are all assigned above. `Item.cs:48380-48468` shows the
tooltip rebuild/name boundary, but it does not prove the final persistence or network owner.

### 8.3 Invariants and ordering

- Item authority owns item identity, stack, content, and any gameplay values selected by integration
  review. Tooltip, processed text, cache validators, and UI colors never write that authority.
- A tooltip cache key includes item identity/revision, tooltip context/slot, language revision, and
  any content revision used by the projection. `ulong.MaxValue` compatibility sentinels must not
  bypass an explicit invalidation decision.
- Appearance values that affect actual game behavior or network/save formats are serialized by the
  item/content owner, not by client UI. The proposed snapshot is a boundary value, not a persistence
  schema.
- Proposed order is item/content snapshot -> C03 localized lookup -> C04 legacy compatibility when
  required -> tooltip rebuild -> client draw. Item revision or language revision invalidates the
  cache; a rejected item snapshot leaves the prior display only until the next valid projection.

## 9. Checkpoint C06: proposed Creative Power UI catalog design

### 9.1 Proposed roles

| Proposed role | Scope | Responsibility | State kind | Single writer candidate | Status |
|---|---|---|---|---|---|
| `CreativePowerUiLayoutCatalog` | client catalog | preferred button width/height and icon sheet dimensions | immutable UI definition | `CreativePowerUiCatalogInitializationSystem` | proposed |
| `CreativePowerIconLocationCatalog` | client catalog | named icon `Point` locations for Creative Power controls | immutable UI definition | `CreativePowerUiCatalogInitializationSystem` | proposed |
| `CreativePowerUiStyleProjection` | client projection | selected color and request-info projection | derived presentation | Creative UI projection system | proposed |

The catalog describes controls; it does not own Creative Power activation, research, duplication,
world time, weather, or enemy state. Creative authority and request handling are
`crossSubsystemOwner: integration-review`.

### 9.2 C06 member coverage

| Source members and evidence | Proposed affiliation | Status |
|---|---|---|
| `CreativePowerUIElementRequestInfo.PreferredButtonWidth`, `PreferredButtonHeight` (`Terraria.GameContent.Creative/CreativePowerUIElementRequestInfo.cs:3-8`) | `CreativePowerUiLayoutCatalog` and request projection | proposed |
| `CreativePowerIconLocations.Unassigned`, `Deprecated`, `ItemDuplication`, `ItemResearch`, `TimeCategory`, `WeatherCategory`, `EnemyStrengthSlider`, `GameEvents`, `Godmode`, `BlockPlacementRange`, `StopBiomeSpread`, `EnemySpawnRate`, `FreezeTime`, `TimeDawn`, `TimeNoon`, `TimeDusk`, `TimeMidnight`, `WindDirection`, `WindFreeze`, `RainStrength`, `RainFreeze`, `ModifyTime`, `PersonalCategory` (`Terraria.GameContent.Creative/CreativePowersHelper.cs:12-57`) | `CreativePowerIconLocationCatalog` | proposed |
| `CreativePowersHelper.TextureIconColumns`, `TextureIconRows`, `CommonSelectedColor` (`CreativePowersHelper.cs:59-81`) | `CreativePowerUiLayoutCatalog` and `CreativePowerUiStyleProjection` | proposed |

All 28 records in the three Creative Power groups are covered. `Point` and `Color` are boundary
values copied into a client catalog; platform/rendering types must not leak into shared simulation
authority.

### 9.3 Invariants and ordering

- Icon names and coordinates are stable catalog keys. Initialization must reject duplicate keys or
  invalid sheet coordinates before publishing the catalog.
- Button dimensions and selected color are UI configuration; changing them invalidates client layout
  only and never changes Creative Power authority.
- Proposed order is content/UI asset availability -> catalog initialization -> request projection ->
  Creative Power command projection. A catalog failure leaves the previous accepted catalog and
  reports a client configuration error.

## 10. Checkpoint C07: proposed ItemSlot context, display, and pulse design

### 10.1 Proposed roles

| Proposed role | Scope | Responsibility | State kind | Single writer candidate | Status |
|---|---|---|---|---|---|
| `ItemSlotContextCatalog` | shared/client compatibility catalog | integer context values and the `Count` sentinel | definition/catalog | `ItemSlotContextInitializationSystem` | proposed |
| `ItemSlotDisplayState` | client slot projection | display key, draw availability, option arrays, loadout colors, scratch display slots | transient projection/workset | `ItemSlotDisplayProjectionSystem` | proposed |
| `ItemSlotPulseState` | client animation | new-item highlight, pulse effects, glow hue/time, overdraw styling | transient presentation state | `ItemSlotPulseSystem` | proposed |
| `ItemSlotDisplayQuery` | read-only | maps an external slot snapshot and context to a display snapshot | pure query | none | proposed |
| `ItemSlotReferenceAdapter` | cross-partition boundary | translates `PlayerItemSlotID.SlotReference` and item identity into a display-safe token | external identity adapter | integration-review owner | proposed |

Inventory, equipment, chest, shop, crafting, and item authority do not belong to these client
components. The final slot reference, `ItemInstanceId`, `PlayerEntityId`, and slot revision are
`crossSubsystemOwner: integration-review`.

### 10.2 C07 member coverage

| Input group | All source members | Proposed affiliation |
|---|---|---|
| `UiItemSlotStorageAndCraftingContexts` (`ItemSlot.cs:33-113`) | `Context.InventoryItem`, `InventoryCoin`, `InventoryAmmo`, `ChestItem`, `BankItem`, `TrashItem`, `GuideItem`, `ShopItem`, `MouseItem`, `CraftingMaterial`, `InWorld`, `VoidItem`, `InWorldDisplay` | `ItemSlotContextCatalog`; preserve exact integer values and route authority through integration-review |
| `UiItemSlotEquipmentAndDisplayContexts` (`ItemSlot.cs:43,101`) | `Context.PrefixItem`, `Context.GoldDebug` | `ItemSlotContextCatalog`; `GoldDebug` remains compatibility/debug policy, not simulation authority |
| `UiItemSlotCreativeAndCraftingContextState` (`ItemSlot.cs:87-105`) | `Context.CreativeInfinite`, `CreativeSacrifice`, `CreativeInfiniteLocked`, `NewCraftingUIRecipe`, `NewCraftingUICraftSlot`, `NewCraftingUIMaterial` | `ItemSlotContextCatalog`; Creative/crafting owner remains external |
| `UiItemSlotHotbarDisplayAndUtilityContextState` (`ItemSlot.cs:51-113`) | `Context.EquipArmor`, `EquipArmorVanity`, `EquipAccessory`, `EquipAccessoryVanity`, `EquipDye`, `HotbarItem`, `ChatItem`, `EquipGrapple`, `EquipMount`, `EquipMinecart`, `EquipPet`, `EquipLight`, `DisplayDollArmor`, `DisplayDollAccessory`, `DisplayDollDye`, `HatRackHat`, `HatRackDye`, `EquipMiscDye`, `BannerClaiming`, `HotbarItemSmartSelected`, `OverdrawGlow`, `DisplayDollWeapon`, `DisplayDollMount`, `Count` | `ItemSlotContextCatalog`; `Count` is a count/sentinel and cannot be a routable slot |
| `UiItemSlotDisplayAndInteractionState` (`ItemSlot.cs:126-267`) | `ItemDisplayKey.Context`, `Slot`; `_nextTickDrawAvailable`, `DrawGoldBGForCraftingMaterial`, `singleSlotArray`, `canFavoriteAt`, `canShareAt`, `canQuickDropAt`, `LoadoutSlotColors`, `_dirtyHack` | `ItemSlotDisplayState`; scratch arrays and dirty compatibility data are client-only and must not retain authoritative `Item` references |
| `UiItemSlotPulseAndHighlightState` (`ItemSlot.cs:28,210-265`) | `Options.HighlightNewItems`; `PulseEffect.EffectDuration`, `NumPulses`, `color`, `slotRef`, `itemInSlot`, `time`, `PulseEffect.IsActive`; `DrawSelectionHighlightForGridSlot`, `inventoryGlowHue`, `inventoryGlowTime`, `inventoryGlowHueChest`, `inventoryGlowTimeChest`, `playerSlotPulseEffects`, `forceClearGlowsOnChest`, `OverdrawGlowSize`, `OverdrawGlowColorMultiplier` | `ItemSlotPulseState`; `slotRef` and `itemInSlot` are copied through the identity adapter, while pulse time/glow arrays are transient |

The six listed input groups contain all 72 C07 records. The Version4 file provides declarations and
static initialization but omits operational transfer/draw/pulse methods; readers, writers, and event
timing are therefore partial or missing.

### 10.3 Invariants and lifecycle

- Context integers are compatibility data. Their values must be preserved behind an explicit map;
  unknown contexts are rejected rather than silently treated as inventory.
- Display queries read an immutable item/slot snapshot and do not mutate inventory, equipment, or
  crafting state. `singleSlotArray` and `_dirtyHack` are migration buffers only.
- Pulse state is created on an accepted new-item/display event, advances through an injected clock
  port, and is cleared on expiry, slot revision change, UI teardown, or explicit chest-glow reset.
- Proposed order is accepted inventory/equipment snapshot -> context/display query -> display
  projection -> pulse update -> draw. No pulse system may run before the slot identity is resolved.

## 11. Checkpoint C08: proposed ItemSlot transfer command design

### 11.1 Proposed roles and command boundary

| Proposed role | Scope | Responsibility | State kind | Single writer candidate | Status |
|---|---|---|---|---|---|
| `AlternateClickActionCatalog` | client command catalog | cursor overrides, localized gamepad hints, and named alternate actions | immutable command definition | `ItemSlotActionCatalogInitializationSystem` | proposed |
| `ItemSlotTransferCommand` | client-to-authority boundary | source/target slot context, item type compatibility field, amount, expected revision, and action | transient command | `ItemSlotTransferCommandSystem` submits only | proposed |
| `ItemSlotTransferResult` | authority-to-client boundary | accepted/rejected status, reason, new slot snapshot/revision | result/event projection | inventory/equipment owner | proposed; `crossSubsystemOwner: integration-review` |

`ItemTransferInfo` is treated as a compatibility payload, not as a mutable inventory component.
The source spelling `FromContenxt` is retained in the mapping and may only be renamed behind an
explicit wire/API compatibility adapter.

### 11.2 C08 member coverage

| Source members and evidence | Proposed affiliation | Boundary note |
|---|---|---|
| `AlternateClickAction.cursorOverride`, `gamepadHintText` (`ItemSlot.cs:158-162`) | `AlternateClickActionCatalog` | cursor and localized hint are client command metadata; the hint is a C03 value reference |
| `AlternateClickAction.Trash`, `TransferToBackpack`, `Unequip`, `TransferFromChest`, `TransferToChest`, `Sell` (`ItemSlot.cs:164-174`) | `AlternateClickActionCatalog` | static definitions create command intents; they do not perform transfer or sell side effects |
| `ItemTransferInfo.ItemType`, `TransferAmount`, `FromContenxt`, `ToContext` (`ItemSlot.cs:183-197`) and its constructor | `ItemSlotTransferCommand` compatibility payload | submit a context/amount intent with slot identity and expected revision added at the integration boundary; no mutable `Item` reference crosses the boundary |

All 12 `UiItemSlotTransferState` records are covered. The transfer operation and acceptance timing
are not present in the read Version4 file, so transaction semantics are evidence-gap and remain
integration-review.

### 11.3 Invariants, failure, and order

- The command is created by UI input, validated against a display snapshot, and submitted once to
  the authoritative inventory/equipment owner. It cannot directly swap arrays or mutate an Item.
- The owner rejects stale slot revision, invalid context route, insufficient quantity, unsupported
  action, and item-type mismatch with a typed result. Retry is allowed only with a new snapshot and
  command id; duplicate command ids must be deduplicated by the owner.
- On acceptance, the owner publishes a new slot snapshot. C07 display/pulse systems refresh only
  from that result. On rejection, the UI keeps authority unchanged and clears the pending command.
- Proposed order is alternate-click projection -> command validation -> inventory/equipment commit ->
  transfer result -> slot display and pulse refresh. Network and persistence are owner adapters.

## 12. Checkpoint C09: proposed ItemSorting registry and catalog design

### 12.1 Proposed roles

| Proposed role | Scope | Responsibility | State kind | Single writer candidate | Status |
|---|---|---|---|---|---|
| `ItemSortingLayerDefinition` | content/client catalog | stable layer name and deterministic sorting method descriptor | catalog definition | `ItemSortingCatalogInitializationSystem` | proposed |
| `ItemSortingLayerCatalog` | client catalog | weapon/tool, armor/accessory, and consumable/misc layer definitions | immutable catalog | `ItemSortingCatalogInitializationSystem` | proposed |
| `ItemSortingRegistryState` | client runtime | ordered layer list, whitelist maps, item-type indexes, count, and damage ranking entries | registry/index state | `ItemSortingRegistryBuildSystem` | proposed |
| `ItemSortingRegistryQuery` | read-only | resolve a layer, whitelist, or type index against an immutable registry | pure query | none | proposed |
| `DamageTypeSortingLayerEntry` | registry value object | multiplier, layer, and index for damage ordering | value object | registry build system | proposed |

The sorting query consumes item snapshots and content set snapshots. It cannot write item authority,
inventory contents, or item-definition sets. The final content catalog owner is
`crossSubsystemOwner: integration-review`.

### 12.2 C09 member coverage

| Input group | All source members | Proposed affiliation |
|---|---|---|
| `UiItemSortingRegistryAndRankingState` (`ItemSorting.cs:8-34`, `Main.cs:3416`) | `DamageTypeSortingLayerEntry.Multiplier`, `Layer`, `Index`; `ItemSorting._layerList`, `_layerWhiteLists`, `_layerIndexForItemType`, `_layerCount`, `_damageRankings`; `ItemSorting.LayerCount` | `ItemSortingRegistryState` and `DamageTypeSortingLayerEntry`; `SetupWhiteLists` is the registry build boundary and `LayerCount` is a derived query |
| `UiItemSortingWeaponAndToolCatalogState` (`ItemSorting.cs:38-405`) | `ItemSortingLayer.Name`, `SortingMethod`; `ItemSortingLayers.WeaponsMelee`, `WeaponsRanged`, `WeaponsMagic`, `WeaponsMinions`, `WeaponsAssorted`, `WeaponsAmmo`, `ToolsPicksaws`, `ToolsHamaxes`, `ToolsPickaxes`, `ToolsAxes`, `ToolsHammers`, `ToolsTerraforming`, `ToolsFishing`, `ToolsGolf`, `ToolsInstruments`, `ToolsKeys`, `ToolsKites`, `ToolsAmmoLeftovers`, `ToolsMisc` | `ItemSortingLayerDefinition` and `ItemSortingLayerCatalog`; closures are adapted to read-only item snapshots and preserve stable tie breaks |
| `UiItemSortingArmorAndAccessoryCatalogState` (`ItemSorting.cs:429-601`) | `ItemSortingLayers.ArmorCombat`, `ArmorVanity`, `ArmorAccessories`, `EquipGrapple`, `EquipMount`, `EquipCart`, `EquipLightPet`, `EquipVanityPet` | `ItemSortingLayerCatalog`; source predicates and comparator order are compatibility data, not item writes |
| `UiItemSortingConsumableAndMiscCatalogState` (`ItemSorting.cs:218-405`) | `ItemSortingLayers.PotionsLife`, `PotionsJustTheMushroom`, `PotionsMana`, `PotionsElixirs`, `PotionsBuffs`, `PotionsFood`, `PotionsDyes`, `PotionsHairDyes`, `MiscValuables`, `MiscWiring`, `MiscMaterials`, `MiscJustTheGlowingMushroom`, `MiscExtractinator`, `MiscPainting`, `MiscRopes`, `MiscHerbsAndSeeds`, `MiscGems`, `MiscAcorns`, `MiscBossBags`, `MiscCritters`, `LastMaterials`, `LastTilesImportant`, `LastTilesCommon`, `LastNotTrash`, `LastTrash` | `ItemSortingLayerCatalog`; the source layer order and whitelist behavior must be preserved exactly until a compatibility test approves change |

The four groups contain all 63 C09 records. The source `SortingMethod` closures read item fields and
`ItemID`/`MountID` sets and remove indexes from a work list; the proposed query makes that mutation
local to C10 rather than allowing the catalog to mutate an Item or inventory.

### 12.3 Invariants and ordering

- Layer registration order, whitelist contents, comparator tie breaks, and item-type indexes are
  observable sorting behavior. No layer may be silently reordered by file or directory order.
- `SetupWhiteLists` or its proposed equivalent runs after content definitions and before any sort
  query. A failed or duplicate layer registration leaves the prior accepted registry.
- Registry rebuild invalidates C10 worksets. Sorting reads a stable registry and item snapshot; it
  does not update content sets or item fields.
- Proposed order is content definitions -> catalog initialization -> whitelist/index build -> sorting
  execution workset -> output order projection. Registry and rankings are not saved with inventory.

## 13. Checkpoint C10: proposed ItemSorting execution workset design

### 13.1 Proposed role

`ItemSortingExecutionWorkset` is a client/runtime transient owned by one proposed
`ItemSortingExecutionSystem`. It contains index lists and item snapshots for one sort or fill-ammo
operation, never persistent inventory state. `ItemSortingQuery` computes an output order from an
immutable registry, item snapshot, and slot availability input; the UI receives an order projection
and an inventory owner command if rearrangement is accepted.

### 13.2 C10 member coverage

| Source members and evidence | Proposed affiliation | Lifecycle |
|---|---|---|
| `_sort_itemsToSort`, `_sort_sortedItemIndexes`, `_sort_counts` (`Terraria.UI/ItemSorting.cs:1206-1210`) | `ItemSortingExecutionWorkset` | allocate/clear per sort request; index lists are never shared with item authority |
| `_sort_itemsCache` (`ItemSorting.cs:1212`) | `ItemSnapshotWorkset` owned by the sorting system | copy only the fields required by catalog predicates/comparators; do not retain mutable `Terraria.Item` references |
| `_sort_availableSortingSlots` (`ItemSorting.cs:1214`) | `ItemSortingExecutionWorkset` | derived from the accepted inventory/slot snapshot and discarded after output or failure |
| `_fillAmmoFromInventory_acceptedAmmoTypes`, `_fillAmmoFromInventory_emptyAmmoSlots` (`ItemSorting.cs:1216-1218`) | `AmmoFillWorkset` within the same execution owner | build from current item/slot snapshots; clear on completion, cancellation, stale revision, or exception |

All seven `UiItemSortingExecutionState` records are covered. The actual sort/fill entry points are
partial in the Version4 slice; the design does not infer missing call order or mutation semantics.

### 13.3 Invariants, failure, and ordering

- A workset has one request id, registry revision, item snapshot revision, and owner. Reentrant
  requests are rejected or queued by an explicit policy; they never share mutable lists.
- Workset lists contain slot indexes or immutable snapshot values, not authoritative Item instances.
  The execution system emits a proposed order or transfer command; the inventory owner decides
  whether to apply it.
- Every success, rejection, cancellation, stale-revision result, and exception clears the workset.
  A failed fill-ammo attempt must not leave accepted ammo types or empty slots available to the next
  operation.
- Proposed order is C09 registry build -> C10 snapshot/workset build -> pure layer/query evaluation
  -> result/order projection -> optional C08/inventory command -> C07 display refresh.

## 14. Checkpoint index

The following proposed component units were completed as synchronized checkpoints in this document
and its paired execution plan:

| Checkpoint | Proposed unit | Input groups | Planned boundary |
|---|---|---|---|
| C01 | `AchievementProgressAndPresentation` | `SharedAchievementProgressSupport`, `AchievementPresentationState` | progress state, event commands, presentation and persistence adapter |
| C02 | `LocalizationCultureAndLanguage` | `LocalizationCultureAndLanguageState` | culture registry, language load/reload system, language-change event/adapter |
| C03 | `LocalizedTextNetworkValue` | `LocalizedTextValueState` | value object, variable-text query, network-text adapter/projection |
| C04 | `LegacyLanguageCatalog` | `LegacyLanguageCatalogState` | legacy arrays and item/name/tooltip catalog projection |
| C05 | `ItemAppearanceAndTooltip` | `SharedItemAppearanceAndTooltipState`, `UiItemTooltipState` | item instance appearance state plus read-only tooltip projection |
| C06 | `CreativePowerUiCatalog` | three Creative Power groups | icon/layout catalog and UI request projection |
| C07 | `ItemSlotContextDisplayAndPulse` | six slot context/display/pulse groups | slot context definitions and client-only display worksets |
| C08 | `ItemSlotTransferCommand` | `UiItemSlotTransferState` | explicit transfer intent and inventory-owner result |
| C09 | `ItemSortingRegistryAndCatalog` | registry/ranking and three catalog groups | stable sorting definitions, registry, rankings, whitelists |
| C10 | `ItemSortingExecutionWorkset` | `UiItemSortingExecutionState` | one-shot sort/fill-ammo query and transient execution state |

## 15. Focused verifier plan for the complete P17 design

The following are plans only and remain `verificationStatus: not-run`:

- member coverage verifier: parse the P17 input report and assert all 296 records appear exactly
  once in the final coverage ledger;
- language verifier: culture fallback, language reload ordering, `OnLanguageChanged` invalidation,
  missing-key behavior, variable condition behavior, and repeated language switch idempotency;
- network text verifier: `FromKey`, literal/formattable modes, nested substitutions, serialization
  round-trip, and rejection of arbitrary UI rendered text as a network payload;
- achievement verifier: clamping, completion-once, save/load schema, cloud/file adapter failure,
  event duplication, and presentation rebuild after language change;
- item/tooltip verifier: item revision invalidates tooltip, tooltip never writes inventory, missing
  localization falls back deterministically, and appearance fields are not mistaken for UI cache;
- ItemSlot verifier: context-to-owner mapping, accepted/rejected transfer commands, stale slot
  revision, pulse cleanup, and no mutation from read-only display queries;
- sorting verifier: stable order tie-breaks, whitelist setup before query, every catalog layer,
  empty/invalid slots, and workset cleanup after a failed fill-ammo operation;
- static checks: proposed file placement under capability-first directories, one core public type
  per same-named file, explicit scheduler edges, and no external third-party type in core state.

No compile-capable command is authorized or required for this second-round planning session.

## 16. Integration Handoff

partitionId: P17
sessionId: 4aa4d033449e43359ca5e2ca42b5ea3c
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\17-ui-item-localization.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P17-ui-item-localization-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P17-ui-item-localization-component-execution.md
evidenceStatus: C01-C10 proposed evidence assembled; no implementation evidence
nltxStatus: matching P17 implementation not found in current src/Test/dome/src search
verificationStatus: not-run
confirmedOwners:
- Version4 `CustomFloatCondition.Value` owns the shown clamp/tracker/completion transition.
- Version4 `LanguageManager` owns the shown active-culture reload and text registry mutation.
- Version4 `NetworkText.Serialize` owns the shown serialization operation.
- Version4 `ItemSorting.SetupWhiteLists` owns the shown sorting whitelist/index build in the reference slice.
proposedTypes:
- proposed `AchievementDefinitionCatalogComponent`
- proposed `AchievementProgressComponent`
- proposed `AchievementEncounterContextComponent`
- proposed `AchievementPresentationSnapshot`
- proposed `AchievementSaveSnapshot`
- proposed `LocalizedTextValue`, `VariableTextTemplate`, `NetworkTextPayload`
- proposed `LegacyLanguageTextCatalog`, `LegacyNameCacheProjection`, `LegacyTooltipCacheProjection`
- proposed `ItemAppearanceSnapshot`, `ItemTooltipRequestProjection`, `ItemTooltipCache`
- proposed Creative Power UI catalogs and projections
- proposed ItemSlot context/display/pulse state and queries
- proposed ItemSlot transfer commands and result boundary
- proposed ItemSorting catalogs, registry, and execution workset
- proposed systems, queries, commands, adapters, and projections listed above
sharedTypesForIntegrationReview:
- `PlayerEntityId`, `AchievementProgressId`, `ItemInstanceId`, `ItemSlotReference`, `NetworkId`, `PersistentId`, `LocalizedTextKey`, and language revision
crossSubsystemReaders:
- player movement/combat/world event producers read or trigger achievement progress through explicit events;
- item, inventory, player, network, and client UI consumers read localized/item-slot/sort projections.
crossSubsystemWriters:
- player/NPC/projectile/world event systems can submit achievement events;
- inventory/equipment/content systems own item and slot authority; client UI only submits commands.
orderingConstraints:
- language load before localized projections;
- item/content definitions before sorting registry build;
- accepted inventory result before slot display refresh;
- progress update before achievement save/presentation snapshot.
boundaryChallenges:
- Version4 mixes achievement definitions, runtime progress, save metadata, and UI icon indexes in manager-adjacent types;
- Item fields mix simulation/content values with tooltip and presentation caches;
- ItemSlot declarations mix static context catalog, transient glow worksets, and transfer payloads.
evidenceGaps:
- The repository guidance references `约束/公共拆分约束.md`, but that file is absent from the checkout; the available guidance and public-decomposition references were used and the missing shared constraint remains an evidence gap.
- missing P17 first-round report;
- missing operational ItemSlot methods and tooltip validator body in Version4 slice;
- incomplete persistence/network evidence for item appearance and achievement save transaction;
- no current NLTX P17 implementation to map as an existing owner.
blockingDecisions:
- final player/inventory/achievement owner, transfer transaction boundary, language invalidation contract, and shared ID/value-object owner remain integration-review.
notImplemented:
- No production code, C#, test, csproj, source generator, or migration was created or modified.
verifierPlan:
- Execute only after implementation planning is accepted; use the focused verifiers in section 6 and the repository serial build/test wrapper.

This document is a proposed non-authoritative design. It is not an ECS implementation, migration
completion report, behavior-equivalence proof, API compatibility proof, or claim about current NLTX
capability.

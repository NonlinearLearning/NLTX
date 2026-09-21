# P17 UI 物品、排序与本地化 proposed component execution plan

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

## 1. Execution boundary

This is a later implementation plan only. It does not create, move, split, or rename C# files. All
target paths, namespaces, types, and commands below are `status: proposed`. The source of truth for
the plan is the paired proposed design document. The implementation must preserve public API and
behavior only after focused verification demonstrates that preservation.

The proposed domain-first layout is:

```text
status: proposed
src2/Share/Achievements/
src2/Share/Localization/
src2/Share/Items/
src2/Client/ItemUi/
src2/Client/CreativeUi/
src2/Client/ItemSorting/
src2/Infrastructure/Localization/
src2/Infrastructure/Persistence/
```

These directories are not claimed to exist. A final owner session must reconcile them with the
actual NLTX project boundaries and the ECS file organization constraint before implementation.

## 2. C01 checkpoint: achievement execution plan

### 2.1 Proposed target files and namespaces

| Proposed file | Proposed namespace/type | Role | Status |
|---|---|---|---|
| `src2/Share/Achievements/AchievementDefinitionCatalogComponent.cs` | `NLTX.Share.Achievements.AchievementDefinitionCatalogComponent` | definition names, localization keys, condition descriptors, icon indexes | proposed |
| `src2/Share/Achievements/AchievementProgressComponent.cs` | `NLTX.Share.Achievements.AchievementProgressComponent` | player-condition progress and completion state | proposed |
| `src2/Share/Achievements/AchievementEncounterContextComponent.cs` | `NLTX.Share.Achievements.AchievementEncounterContextComponent` | mining/Mecha Mayhem transient flags | proposed |
| `src2/Share/Achievements/AchievementCommands.cs` | `NLTX.Share.Achievements.AdvanceAchievementProgressCommand` and related proposed command records | explicit progress/complete intent | proposed; one core public type per final file must be decided before implementation |
| `src2/Share/Achievements/AchievementQueries.cs` | `NLTX.Share.Achievements.AchievementProgressQuery` and `AchievementIconQuery` | pure read-only qualifications/projections | proposed; split files if multiple public types violate local rule |
| `src2/Client/Achievements/AchievementPresentationSnapshot.cs` | `NLTX.Client.Achievements.AchievementPresentationSnapshot` | client display snapshot | proposed |
| `src2/Infrastructure/Persistence/AchievementPersistenceAdapter.cs` | `NLTX.Infrastructure.Persistence.IAchievementPersistenceAdapter` and adapter implementation | file/cloud storage seam | proposed; interface/implementation file split required at implementation time |

The final namespaces and project ownership are integration-review decisions. No target file is
currently created.

### 2.2 Source member to role mapping

| Source members | Target role | Migration note |
|---|---|---|
| `AchievementsHelper._isMining`, `CurrentlyMining` | `AchievementEncounterContextComponent.MiningActive` | preserve writes from projectile/event adapters; no direct UI write |
| `AchievementsHelper.mayhemOK`, `mayhem1down`, `mayhem2down`, `mayhem3down` | `AchievementEncounterContextComponent.MechaMayhemState` | preserve start/kill/clear transition; reset on encounter/session lifecycle |
| `CustomFloatCondition._value`, `Value` | `AchievementProgressComponent.CurrentValue` | preserve clamp, tracker update, and completion-on-max ordering |
| `CustomFloatCondition._maxValue` | definition/progress constraint `MaximumValue` | do not make max mutable through UI; load compatibility field explicitly |
| `Achievement._totalAchievements`, `Name`, `FriendlyName`, `Description`, `Id`, `_conditions` | definition catalog | `Id` remains local definition identity until integration approves persistent/network IDs |
| `AchievementCondition.Name`, `_tracker`, `_isCompleted`, `IsCompleted` | progress component plus tracker strategy | tracker behavior is a system/strategy seam, not a behavior-bearing component |
| `StoredAchievement.Conditions` | `AchievementSaveSnapshot` | serialize only accepted progress revisions; preserve JSON property names in compatibility adapter |
| `AchievementManager._savePath`, `_isCloudSave`, `_serializerSettings`, `_cryptoKey`, `_ioLock` | persistence adapter configuration/runtime resources | keep file/cloud/crypto dependencies outside ECS state |
| `AchievementManager._achievements`, `_achievementIconIndexes` | definition/projection registry | `_achievements` is a lookup catalog; icon indexes are client projection metadata |
| `AchievementTracker<T>._value`, `_maxValue`, `_name`, `_type` | progress component plus deterministic tracker query | preserve `SetValue` update-before-complete ordering |
| `AchievementProgression` helper callers in `Player`, `Projectile`, `NPC`, `Main` | event adapters/commands | replace hidden static calls only after an event delivery and duplicate policy is verified |

### 2.3 Implementation order

1. Freeze proposed compatibility records for local definition id, achievement condition key,
   player/entity reference, progress revision, and persistence schema version. Mark shared ids
   `crossSubsystemOwner: integration-review`.
2. Add focused pure tests for clamping, completion-once, event-to-command conversion, and stable
   snapshot projection before moving any caller.
3. Add the proposed definition catalog and load the existing definitions without changing the
   legacy registry.
4. Add the proposed progress component/system behind an adapter. During the compatibility window,
   the adapter is the sole bridge from legacy conditions to proposed progress.
5. Add the encounter context system and migrate one event family at a time: mining, movement,
   NPC-killed, progression, then Mecha Mayhem.
6. Add the read-only presentation snapshot and icon query.
7. Add persistence adapter read/write with schema version and failure result; compare snapshots
   before enabling any write cutover.
8. Remove compatibility reads only after integration review accepts parity evidence. This plan
   does not authorize that removal.

### 2.4 Single write owner and side effects

`AchievementProgressSystem` is the sole proposed writer of progress/completion. Event producers
submit commands; queries and presentation projections never write. `AchievementPersistenceAdapter`
is the sole proposed owner of file/cloud I/O, encryption, lock lifetime, retries, and error
classification. The adapter must distinguish accepted, rejected, failed, cancelled, and unknown
timeout results. A log or UI callback cannot advance progress.

### 2.5 Compatibility, snapshot, and rollback

- Keep legacy achievement lookup and the proposed catalog in read-only comparison mode first.
- During dual-read, compare condition key, current value, max value, completion, and icon index;
  do not dual-write until the owner and revision protocol are approved.
- The persistence adapter must read the old `Conditions` JSON shape and write a versioned proposed
  snapshot only after a successful conversion. A failed conversion leaves the old snapshot intact.
- Roll back by disabling proposed command consumption, restoring legacy event routing, and ignoring
  proposed snapshots newer than the last accepted legacy revision. Rollback is required when a
  completion is duplicated, progress decreases unexpectedly, a save is partially committed, or a
  player/achievement relation cannot be resolved.

### 2.6 C01 focused verifier plan

Planned, not run:

- unit verifier for `[0, max]` clamping, `reportUpdate` ordering, completion at max, and duplicate
  completion;
- contract verifier for each source event family and exact command payload;
- persistence verifier for file/cloud adapter, crypto failure, lock release, old JSON migration,
  retry/unknown result, and atomic replacement;
- projection verifier for icon lookup, language revision invalidation, and no reverse mutation;
- serial repository build/test through `Build/Tools/Invoke-SerialDotnet.ps1` only after C# exists.

### 2.7 C01 implementation checkpoint

Implemented source is under `src2/UiItemLocalization/Achievements` and
`src2/UiItemLocalization/Infrastructure/Persistence`. The implementation includes definition
registration, clamped progress updates with duplicate event rejection, transient encounter state,
read-only presentation snapshots, and a JSON persistence adapter behind
`IAchievementPersistenceStore`. The C01 verifier and serial build are pending; implementation
status is therefore `in-progress` and verification status remains `not-run`.

## 3. C02 checkpoint: localization execution plan

### 3.1 Proposed target files and namespaces

| Proposed file | Proposed namespace/type | Role | Status |
|---|---|---|---|
| `src2/Share/Localization/CultureRegistryCatalog.cs` | `NLTX.Share.Localization.CultureRegistryCatalog` | named and legacy culture definitions | proposed |
| `src2/Share/Localization/LanguageCatalogState.cs` | `NLTX.Share.Localization.LanguageCatalogState` | localized key/category/variation catalog and active revision | proposed |
| `src2/Share/Localization/LanguageChangeEvent.cs` | `NLTX.Share.Localization.LanguageChangeEvent` | old/new culture and revision invalidation payload | proposed |
| `src2/Share/Localization/SetLanguageCommand.cs` | `NLTX.Share.Localization.SetLanguageCommand` | explicit language change intent | proposed |
| `src2/Share/Localization/LanguageReloadSystem.cs` | `NLTX.Share.Localization.LanguageReloadSystem` | the single state-transition owner for reload | proposed |
| `src2/Share/Localization/LanguageQueries.cs` | proposed lookup/query types | side-effect-free read projections | proposed; split by public type before implementation |
| `src2/Infrastructure/Localization/ILocalizationContentAdapter.cs` | `NLTX.Infrastructure.Localization.ILocalizationContentAdapter` | embedded/content file boundary | proposed |
| `src2/Infrastructure/Localization/LocalizationContentAdapter.cs` | `NLTX.Infrastructure.Localization.LocalizationContentAdapter` | content enumeration and parse adapter | proposed |

The names and directories are candidates only. The implementation owner must reconcile them with
the existing project graph and the capability-first ECS file constraint before creating files.

### 3.2 Source member mapping and execution order

| Input members | Proposed target | Implementation action |
|---|---|---|
| `GameCulture._NamedCultures`, `_legacyCultures`, `CultureInfo`, `LegacyId`, `DefaultCulture`, `IsActive`, `Name`; `Language.ActiveCulture` | `CultureRegistryCatalog` plus `CultureQuery` | register named/legacy aliases once; keep `CultureInfo` behind the platform adapter and expose immutable culture identity |
| `LanguageManager.Instance`, `_localizedTexts`, `_categoryGroupedKeys`, `_textVariations`, `_fallbackCulture`, `_contentSources`, `VariationSeparatorSign`, `ActiveCulture` | `LanguageCatalogState` and adapter configuration | make the catalog the only reload-owned mutable registry; preserve lookup identity where compatibility readers require it |
| `LanguageManager` loading and reload members | `LanguageReloadSystem` and `ILocalizationContentAdapter` | load fallback/current values, apply files and copy commands, then publish one revisioned event |

The sequence is: resolve `SetLanguageCommand` -> validate culture/fallback -> enumerate and parse
content through the adapter -> update registry values and variation tables -> apply thread culture
as an external effect -> increment revision only on the accepted result -> publish
`LanguageChangeEvent` -> invalidate localized/tooltip/legacy projections. No UI query may perform
reload or mutate a missing-key entry implicitly.

### 3.3 Compatibility, rollback, and focused verifier

- Preserve Version4's reload only when the requested culture differs from the active culture; an
  equal request returns an idempotent no-op with the current revision.
- During migration, dual-read the old language lookup and proposed catalog. Preserve
  `LocalizedText` object identity where existing callers may retain references. Keep a compatibility
  path for Version4 `GetText` create-on-read until integration review chooses an explicit
  `TryGet`/registration contract.
- Content parse or thread-culture failure returns a failed reload result and does not publish a
  successful revision. The adapter owns file/asset diagnostics and retry; the reload system owns
  state rollback to the last accepted catalog snapshot.
- Rollback disables proposed `SetLanguageCommand` consumption, restores the legacy reload route,
  discards uncommitted catalog/revision state, and rebuilds projections from the last accepted
  snapshot. Trigger rollback on partial file application, stale object identity, inconsistent
  fallback, or an event published before the catalog commit.
- Planned verifier: named/legacy culture lookup, default fallback, no-op repeated reload,
  content-source failure, thread culture isolation, event-after-commit ordering, missing-key
  compatibility, and projection invalidation by language revision. No verifier is run here.

## 4. C03 checkpoint: localized value and network text execution plan

### 4.1 Proposed targets and source mapping

| Proposed target | Proposed role | Source members | Status |
|---|---|---|---|
| `src2/Share/Localization/LocalizedTextValue.cs` | localized key/value identity and English baseline | `LocalizedText.Empty`, `_substitutionRegex`, `Key`, `_value`, `_propertyLookupCache`, `Value`, `UnformattedValue`, `EnglishValue`, `ConditionsMet` | proposed |
| `src2/Share/Localization/VariableTextTemplate.cs` | parsed variables, conditions, and format descriptors | `VariableText.Condition.RequiredValue`, `Name`; `_original`, `_format`, `_conditions`, `_variables` | proposed |
| `src2/Share/Localization/VariableTextFormatter.cs` | transient formatting buffers and pure query | `_formatArgBuffer`, `_formatBuffer`, `_substitutionRegex` | proposed |
| `src2/Share/Network/NetworkTextPayload.cs` | protocol value | `NetworkText.Empty`, `_substitutions`, `_text`, `_mode` | proposed |
| `src2/Infrastructure/Network/NetworkTextSerializationAdapter.cs` | wire serialization boundary | Version4 `NetworkText.Serialize` and tModLoader `FromKey`/`FromLiteral` semantics | proposed |
| `src2/Client/Localization/LocalizedTextProjectionSystem.cs` | rendered UI string projection | `LocalizedText` derived properties | proposed |

The public types should be split into same-named files before implementation. No target exists and
no raw rendered string is allowed to replace the key/mode/substitution payload.

### 4.2 Execution, compatibility, and rollback

1. Register or preserve localized text identity from the C02 catalog, then construct a
   `LocalizedTextValue` view without mutating the catalog during lookup.
2. Parse `VariableTextTemplate` once per source format and execute a pure query against a stable
   lookup snapshot. Reusable buffers are cleared after each result.
3. Map Version4 network modes to `NetworkTextPayload`; serialize keys and substitutions through the
   adapter. A receiving client resolves keys in its active culture.
4. Dual-read legacy `NetworkText` and proposed payloads during migration. Reject a payload with an
   unknown mode, invalid substitution depth, or ambiguous key/literal representation.
5. Roll back by disabling the proposed adapter, routing reads through the legacy serializer, and
   discarding only uncommitted value/projection revisions. Trigger rollback on a wire round-trip
   mismatch, rendered text sent in a key field, or stale localized object identity.

### 4.3 Focused verifier

Planned, not run: `Empty` sentinel behavior; key/literal/formattable construction; nested
substitution serialization round-trip; variable condition truth table; English/current fallback;
object identity across language reload; malformed mode and substitution rejection; and proof that
rendered client text is never the network payload.

## 5. C04 checkpoint: legacy language catalog execution plan

### 5.1 Proposed targets and source mapping

| Proposed target | Proposed role | Source members | Status |
|---|---|---|---|
| `src2/Share/Localization/Legacy/LegacyLanguageTextCatalog.cs` | compatibility category arrays | `Lang.menu`, `gen`, `misc`, `inter`, `tip`, `mp`, `chestType`, `dresserType`, `chestType2`, `prefix` | proposed |
| `src2/Share/Localization/Legacy/LegacyNameCacheProjection.cs` | name caches | `_itemNameCache`, `_projectileNameCache`, `_npcNameCache`, `_negativeNpcNameCache`, `_buffNameCache`, `_buffDescriptionCache`, `_emojiNameCache` | proposed |
| `src2/Client/Localization/LegacyTooltipCacheProjection.cs` | tooltip cache | `_itemTooltipCache` | proposed |
| `src2/Share/Localization/Legacy/LegacySubstitutionRegistry.cs` | callback compatibility boundary | `_globalSubstitutions`, `_prefixFormatText` | proposed |
| `src2/Client/Localization/Legacy/PrefixNameProjection.cs` | prefix/item display values | `ItemPrefixCombiner.ItemName`, `PrefixName` | proposed |
| `src2/Client/Localization/Legacy/LegacyLanguageCatalogProjectionSystem.cs` | rebuild and invalidation owner | Version4 `InitializeLegacyLocalization` and language-change call path | proposed |

The 22 source records are mapped above. The output is a projection of C02/C03 and is not an item,
NPC, buff, or tooltip authority component.

### 5.2 Execution, compatibility, and rollback

- Build arrays and caches from a stable language revision. Preserve array indices and legacy fallback
  behavior while adding explicit unknown-id diagnostics.
- During dual-read, compare category keys, name cache keys, tooltip validator keys, and prefix
  display values. Do not dual-write any item or NPC state.
- Keep global substitution callbacks behind an adapter and do not invoke them from pure lookup
  queries. Rebuild `_prefixFormatText` only after C03 values are accepted.
- On a failed rebuild, retain the last accepted cache generation and mark projections stale; do not
  publish a half-filled set of arrays. Roll back by disabling the proposed projection and using the
  legacy catalog until a complete generation is available.

### 5.3 Focused verifier

Planned, not run: all ten category arrays preserve indexes; all seven name/tooltip caches invalidate
on language revision; missing IDs use the selected compatibility result; prefix values use current
localized text; callback isolation; and failed rebuilds cannot expose mixed revisions.

## 6. C05 checkpoint: item appearance and tooltip execution plan

### 6.1 Proposed targets and source mapping

| Proposed target | Proposed role | Source members | Status |
|---|---|---|---|
| `src2/Share/Items/ItemAppearanceSnapshot.cs` | appearance boundary value | `dye`, `hairDye`, `paint`, `paintCoating`, `color`, `alpha`, `glowMask`, `scale`, `PaintOrCoating` query | proposed |
| `src2/Share/Items/ItemAudioAndCombatDefinition.cs` | non-UI item definition boundary | `UseSound`, `useSoundPitch`, `defense` | proposed, owner `crossSubsystemOwner: integration-review` |
| `src2/Client/ItemUi/ItemTooltipRequestProjection.cs` | context and display inputs | `tooltipContext`, `tooltipSlot`, `stringColor`, `BestiaryNotes` candidate | proposed, owner review for content fields |
| `src2/Client/ItemUi/ItemTooltipCache.cs` | client cache | `Item.ToolTip`, `ItemTooltip.None`, `_neverUpdateHack`, `_tooltipLines`, `_validatorKey`, `_text`, `_processedText` | proposed |
| `src2/Client/ItemUi/ItemTooltipProjectionSystem.cs` | cache rebuild and draw projection | `Item.RebuildTooltip`/name boundary and `ValidateTooltip` compatibility seam | proposed |

The 17 shared appearance/tooltip records plus six UI tooltip records are explicitly mapped. The
implementation must not copy mutable `Terraria.Item` or external audio/render types into a shared
component without an integration decision.

### 6.2 Execution, compatibility, and rollback

1. Identify the item/content owner and expose an immutable appearance snapshot with item revision.
   Keep `UseSound`, `defense`, and bestiary content in their owning domain until approved.
2. Construct tooltip requests from a slot/display snapshot, then resolve C03 text and C04 legacy
   compatibility values. Cache only the projection keyed by item revision, context/slot, language
   revision, and content revision.
3. During dual-read, compare old and proposed tooltip lines and validator keys. Do not write tooltip
   output back into the item or inventory owner.
4. Roll back by disabling the proposed tooltip projection and rebuilding from the legacy path. Trigger
   rollback on stale lines after language change, a tooltip write to item authority, missing required
   appearance fields, or a save/network path that incorrectly includes client cache state.

### 6.3 Focused verifier

Planned, not run: appearance snapshot round-trip at the selected owner; derived `PaintOrCoating`;
tooltip invalidation on item/language revision; context/slot key separation; `None` sentinel;
validator behavior; missing localization fallback; and static proof that tooltip code cannot mutate
inventory/equipment authority.

## 7. C06 checkpoint: Creative Power UI catalog execution plan

### 7.1 Proposed targets and source mapping

| Proposed target | Proposed role | Source members | Status |
|---|---|---|---|
| `src2/Client/CreativeUi/CreativePowerUiLayoutCatalog.cs` | button and sheet dimensions | `PreferredButtonWidth`, `PreferredButtonHeight`, `TextureIconColumns`, `TextureIconRows` | proposed |
| `src2/Client/CreativeUi/CreativePowerIconLocationCatalog.cs` | named icon coordinates | all 23 `CreativePowerIconLocations` fields | proposed |
| `src2/Client/CreativeUi/CreativePowerUiStyleProjection.cs` | selected color and request projection | `CommonSelectedColor`, request-info projection | proposed |
| `src2/Client/CreativeUi/CreativePowerUiCatalogInitializationSystem.cs` | single initializer | `CreativePowersHelper` declarations and static initialization | proposed |

The proposed catalog preserves all 28 records and contains no Creative Power authority or world
mutation.

### 7.2 Execution, compatibility, and rollback

- Initialize only after the required UI texture/asset metadata is available. Validate unique names,
  coordinate bounds, positive dimensions, and expected row/column counts.
- Keep a versioned immutable catalog snapshot. A request projection reads the snapshot and submits a
  separate Creative Power command to the integration owner.
- Roll back to the last accepted catalog if an icon is out of bounds, style data is incomplete, or a
  request projection resolves the wrong control. No rollback may undo Creative Power authority.

### 7.3 Focused verifier

Planned, not run: all 23 coordinates, dimensions and sheet bounds, selected-color projection,
duplicate-key rejection, catalog revision invalidation, and proof that catalog updates do not mutate
world/Creative Power state.

## 8. C07 checkpoint: ItemSlot context, display, and pulse execution plan

### 8.1 Proposed targets and source mapping

| Proposed target | Proposed role | Source members | Status |
|---|---|---|---|
| `src2/Client/ItemUi/ItemSlotContextCatalog.cs` | integer context compatibility catalog | all 45 `ItemSlot.Context` constants including `Count` | proposed |
| `src2/Client/ItemUi/ItemSlotDisplayState.cs` | display projection/workset | `ItemDisplayKey.Context`, `Slot`; draw map, option arrays, scratch arrays, `LoadoutSlotColors`, `_dirtyHack` | proposed |
| `src2/Client/ItemUi/ItemSlotPulseState.cs` | transient highlight/pulse state | `HighlightNewItems`, `PulseEffect` fields and `IsActive`, glow arrays, draw/overdraw settings | proposed |
| `src2/Client/ItemUi/ItemSlotDisplayQuery.cs` | pure slot display query | context/display mapping and immutable slot/item snapshot | proposed |
| `src2/Client/ItemUi/ItemSlotDisplayProjectionSystem.cs` | client display owner | display state and accepted owner result | proposed |
| `src2/Client/ItemUi/ItemSlotPulseSystem.cs` | time-based pulse owner | pulse state and injected clock | proposed |
| `src2/Infrastructure/Items/ItemSlotReferenceAdapter.cs` | identity adapter | `PlayerItemSlotID.SlotReference`, item identity, slot revision | proposed, owner `crossSubsystemOwner: integration-review` |

The 72 records from the six C07 input groups are mapped. `Item[]` scratch arrays and
`slotRef`/`itemInSlot` references are compatibility inputs, not proposed authority storage.

### 8.2 Execution, compatibility, and rollback

1. Register context constants with exact Version4 integer values. Reject unknown or `Count` as a
   routable context.
2. Convert an accepted inventory/equipment snapshot through the reference adapter, then calculate a
   display snapshot. The query is read-only and never swaps item arrays.
3. Create pulse state only from an accepted new-item/display event. Advance it through a clock port,
   clear it on expiry/revision change/teardown, and keep glow arrays client-local.
4. Operational ItemSlot methods are absent from the source slice, so legacy interaction is retained
   behind an adapter until the implementation owner closes the missing call graph.
5. Roll back by disabling the proposed projection/pulse systems, draining transient worksets, and
   restoring legacy display routing. Trigger rollback on context mismatch, stale slot display,
   pulse leakage across slot revisions, or any authority mutation from a query.

### 8.3 Focused verifier

Planned, not run: exact context integer table; `Count` rejection; display key uniqueness; no mutable
Item reference retention; accepted/rejected snapshot refresh; pulse duration/count/cleanup with a
fake clock; chest-glow reset; stale slot revision; and static side-effect checks.

## 9. C08 checkpoint: ItemSlot transfer command execution plan

### 9.1 Proposed targets and source mapping

| Proposed target | Proposed role | Source members | Status |
|---|---|---|---|
| `src2/Client/ItemUi/Commands/AlternateClickActionDefinition.cs` | alternate action definition | `cursorOverride`, `gamepadHintText` | proposed |
| `src2/Client/ItemUi/Commands/AlternateClickActionCatalog.cs` | named action definitions | `Trash`, `TransferToBackpack`, `Unequip`, `TransferFromChest`, `TransferToChest`, `Sell` | proposed |
| `src2/Client/ItemUi/Commands/ItemSlotTransferCommand.cs` | explicit transfer intent | `ItemType`, `TransferAmount`, `FromContenxt`, `ToContext`, plus slot identity/revision/action | proposed |
| `src2/Client/ItemUi/Commands/ItemSlotTransferResult.cs` | accepted/rejected result | proposed result boundary for inventory/equipment owner | proposed, owner `crossSubsystemOwner: integration-review` |
| `src2/Client/ItemUi/Systems/ItemSlotTransferCommandSystem.cs` | submit-only command system | transfer command and owner port | proposed |

Keep the source typo `FromContenxt` in a compatibility mapper until a wire/API migration is
approved. Do not silently change its serialized name.

### 9.2 Execution, compatibility, and rollback

- UI creates a command from a display snapshot and expected revision. It submits through a port; it
  never invokes an inventory mutation method directly.
- The owner validates route, quantity, item type, permissions, and revision, then returns one typed
  result. Duplicate command ids are deduplicated at the owner boundary.
- During dual-read, compare legacy transfer payload fields and proposed slot identity/revision. Keep
  the legacy route active until accepted/rejected outcomes match focused tests.
- Roll back by stopping command consumption, clearing pending commands, and returning to legacy input
  routing. Trigger rollback on duplicate transfer, item loss/duplication, wrong context route, or a
  result published without an authority revision.

### 9.3 Focused verifier

Planned, not run: action catalog localization; source spelling compatibility; command serialization;
stale revision rejection; accepted/rejected result mapping; duplicate id handling; no direct Item or
inventory mutation from UI; and refresh only after an accepted owner result.

## 10. C09 checkpoint: ItemSorting registry and catalog execution plan

### 10.1 Proposed targets and source mapping

| Proposed target | Proposed role | Source members | Status |
|---|---|---|---|
| `src2/Client/ItemSorting/ItemSortingLayerDefinition.cs` | layer name and method descriptor | `ItemSortingLayer.Name`, `SortingMethod` | proposed |
| `src2/Client/ItemSorting/ItemSortingLayerCatalog.cs` | weapon/tool, armor/accessory, consumable/misc layer set | all 19 weapon/tool, eight armor/accessory, and 25 consumable/misc layer fields | proposed |
| `src2/Client/ItemSorting/ItemSortingRegistryState.cs` | list, whitelist, type index, count, rankings | `_layerList`, `_layerWhiteLists`, `_layerIndexForItemType`, `_layerCount`, `LayerCount` | proposed |
| `src2/Client/ItemSorting/DamageTypeSortingLayerEntry.cs` | ranking value object | `Multiplier`, `Layer`, `Index` | proposed |
| `src2/Client/ItemSorting/ItemSortingRegistryBuildSystem.cs` | registration and `SetupWhiteLists` equivalent | registry setup and content dependency | proposed |
| `src2/Client/ItemSorting/ItemSortingRegistryQuery.cs` | read-only layer/type/whitelist lookup | registry snapshot | proposed |

All 63 C09 records are mapped. The source closures and `ItemID`/`MountID` set reads must be
adapted behind a snapshot boundary so a query cannot mutate an authoritative Item.

### 10.2 Execution, compatibility, and rollback

1. Register layer definitions in explicit source order after content sets are available.
2. Build whitelist and item-type indexes once, publish a registry revision, and reject duplicate
   names or invalid indexes before publication.
3. Preserve comparator and tie-break order during dual-read. Keep registry generation separate from
   C10 execution lists.
4. Roll back to the last accepted registry when setup fails or an output comparison detects layer
   order/tie-break drift. Invalidate any workset that references the rejected registry revision.

### 10.3 Focused verifier

Planned, not run: every named layer is registered once; source order and comparator ties are stable;
whitelist/index build precedes query; damage rankings and `LayerCount` agree; invalid definitions
are rejected atomically; and catalog reads have no Item/inventory writes.

## 11. C10 checkpoint: ItemSorting execution workset execution plan

### 11.1 Proposed targets and source mapping

| Proposed target | Proposed role | Source members | Status |
|---|---|---|---|
| `src2/Client/ItemSorting/ItemSortingExecutionWorkset.cs` | sort index lists and available slots | `_sort_itemsToSort`, `_sort_sortedItemIndexes`, `_sort_counts`, `_sort_availableSortingSlots` | proposed |
| `src2/Client/ItemSorting/ItemSortingItemSnapshotWorkset.cs` | immutable item snapshot cache | `_sort_itemsCache` | proposed |
| `src2/Client/ItemSorting/AmmoFillWorkset.cs` | fill-ammo transient lists | `_fillAmmoFromInventory_acceptedAmmoTypes`, `_fillAmmoFromInventory_emptyAmmoSlots` | proposed |
| `src2/Client/ItemSorting/ItemSortingExecutionSystem.cs` | single owner of build/evaluate/clear lifecycle | all seven workset members | proposed |
| `src2/Client/ItemSorting/ItemSortingResultProjection.cs` | output order and optional owner command | sorted indexes and accepted owner result | proposed |

The seven C10 records are all transient. No proposed target persists them or exposes mutable
`Terraria.Item` references.

### 11.2 Execution, compatibility, and rollback

- Allocate a request-scoped workset tagged with registry and item snapshot revisions. Copy only the
  fields needed by C09 predicates and comparators.
- Evaluate layers into local lists, produce a stable order, and clear all lists in a `finally`-style
  lifecycle path for success, rejection, cancellation, stale revision, or exception.
- Fill-ammo accepted types and empty slots are rebuilt per operation and cannot leak to a later sort.
- The execution system emits a display order or C08/inventory command; it never applies the order to
  inventory directly.
- Roll back by cancelling the current workset and invalidating its result. Trigger rollback on
  registry revision mismatch, reentrant shared-list use, partial fill-ammo state, or a comparator
  that mutates the snapshot.

### 11.3 Focused verifier

Planned, not run: empty/invalid slots, stable ties, available-slot calculation, ammo accepted-type
filtering, failed-operation cleanup, cancellation, stale registry/item revision, reentrancy, and
proof that result projection is the only output side effect.

## 12. Implementation sequence

| Order | Checkpoint | Proposed target boundary | Single write owner |
|---:|---|---|---|
| 1 | C01 AchievementProgressAndPresentation | achievement definition/progress and persistence adapter | achievement progress system |
| 2 | C02 LocalizationCultureAndLanguage | culture registry and localization content adapter | language reload system |
| 3 | C03 LocalizedTextNetworkValue | localized value objects and network text adapter | value update/serialization adapter |
| 4 | C04 LegacyLanguageCatalog | legacy arrays and cache projections | legacy catalog projection system |
| 5 | C05 ItemAppearanceAndTooltip | item appearance boundary and client tooltip cache | item/content owner for authority; tooltip projection for cache |
| 6 | C06 CreativePowerUiCatalog | Creative Power UI catalog and request projection | Creative UI catalog initializer |
| 7 | C07 ItemSlotContextDisplayAndPulse | context catalog and client display/pulse worksets | display/pulse systems; inventory remains external owner |
| 8 | C08 ItemSlotTransferCommand | transfer intent/result boundary | inventory/equipment owner selected by integration-review |
| 9 | C09 ItemSortingRegistryAndCatalog | sorting layer definitions, registry, whitelist/ranking catalog | sorting registry build system |
| 10 | C10 ItemSortingExecutionWorkset | one-sort and fill-ammo transient workset | sorting execution system |

Every row remains `executionStatus: planned` until its proposed implementation is separately
approved. The order is a scheduler dependency contract, not a file or directory ordering rule.

## 13. Network, persistence, and client boundary plan

- `NetworkText` is a protocol value containing a localization key, literal/formattable mode, and
  nested substitutions. It must be serialized by a network adapter, never by a UI component. The
  recipient resolves the key in its active culture. Network IDs and entity IDs remain separate
  from localization keys.
- `LocalizedText.Value`, formatted text, tooltip lines, sort results, pulse time, glow arrays,
  display-key maps, and Creative UI positions are client projections or transient worksets unless
  Version4 evidence proves otherwise. They are not persistence fields for item/inventory state.
- Item appearance fields that affect actual item behavior or player rendering remain under the
  item/content/inventory integration owner. Tooltip and name caches are invalidated by item
  revision and language revision, not copied into item authority.
- Achievement progress persistence is a player-scoped snapshot; achievement definition catalogs,
  icon locations, and rendered text are not saved as authority. File/cloud transport, crypto, and
  lock objects remain infrastructure-only.
- Slot transfer commands carry a slot/context reference and expected revision, not a mutable item
  object or UI text. The authoritative inventory owner returns an accepted/rejected result and a
  new snapshot; the client then rebuilds display state.

## 14. Risks, evidence gaps, and blocking decisions

Risks:

- Version4 is a partial/decompiled source slice. Empty bodies must not be replaced with guessed
  behavior during implementation.
- The same `Item` object contains definition, instance, appearance, tooltip, and compatibility
  fields. Splitting by field name alone could move authoritative item state into a client cache.
- `ItemSlot.Context` values are integer protocol-like constants. Changing values or reusing a
  context without an explicit compatibility map can corrupt inventory/equipment behavior.
- Sorting definitions are static closures over item fields and ItemID sets. Reordering layers or
  changing tie breaks changes observable inventory order even when item authority is unchanged.
- Language reload mutates existing `LocalizedText` objects and invokes an event. Replacing objects
  without preserving reference identity may leave stale callers.

Evidence gaps:

- P17 first-round report is missing.
- Operational ItemSlot transfer/draw/pulse methods are absent in the read Version4 file.
- Achievement persistence implementation and tracker load/report callbacks are empty in the slice.
- Item appearance network/save paths were not closed by the P17 declaration inventory.
- No current NLTX implementation exists to prove a safe source-to-target migration mapping.

Blocking decisions:

- Select the player/achievement relation owner and revision protocol.
- Select the inventory/equipment owner for transfer commands and context values.
- Select whether culture reload is a shared runtime service or client-only projection and define
  invalidation delivery semantics.
- Select the compatibility format for old achievement JSON, NetworkText wire data, and item slot
  context numbers.

## 15. Required verification and build policy

The later implementation session must record exact commands, project, exit code, warning/error
counts, and artifact paths. Compile-capable commands must run serially from the repository root
through `Build/Tools/Invoke-SerialDotnet.ps1`, with `UseSharedCompilation=false`, and affected
project scope only. Focused verifiers must use `--no-build --no-restore` after the affected build,
and artifacts must be checked under `Build/bin/`.

This planning session runs no compile-capable command, creates no verifier, and performs no C#,
compile, test, or behavior-equivalence validation.

## 16. Checkpoint handoff

partitionId: P17
sessionId: 4aa4d033449e43359ca5e2ca42b5ea3c
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\17-ui-item-localization.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P17-ui-item-localization-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P17-ui-item-localization-component-execution.md
evidenceStatus: C01-C10 proposed evidence assembled; no implementation evidence
verificationStatus: not-run
completedComponents:
- C01 AchievementProgressAndPresentation (proposed)
- C02 LocalizationCultureAndLanguage (proposed)
- C03 LocalizedTextNetworkValue (proposed)
- C04 LegacyLanguageCatalog (proposed)
- C05 ItemAppearanceAndTooltip (proposed)
- C06 CreativePowerUiCatalog (proposed)
- C07 ItemSlotContextDisplayAndPulse (proposed)
- C08 ItemSlotTransferCommand (proposed)
- C09 ItemSortingRegistryAndCatalog (proposed)
- C10 ItemSortingExecutionWorkset (proposed)
currentComponent: none (all proposed design checkpoints complete)
pendingComponents: []
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
productionCodeModified: no

The execution plan is proposed and planned only. No C# migration, compilation, test, or behavior
equivalence verification was executed.

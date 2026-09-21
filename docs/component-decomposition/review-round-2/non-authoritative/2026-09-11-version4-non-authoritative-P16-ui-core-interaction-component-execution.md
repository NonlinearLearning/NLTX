# P16 UI 核心与交互：proposed Component Execution Plan

partitionId: P16
sessionId: dc14bbe8fce0452785c893506c01068c
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\16-ui-core-interaction.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P16-ui-core-interaction-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P16-ui-core-interaction-component-execution.md
designStatus: proposed
executionStatus: planned
implementationStatus: completed
verificationStatus: independently-verified (all ten P16 src2 focused components)
completedComponents: UiLayoutGeometryValues; UiElementLayoutComponent; UiElementInteractionComponent; UiPointerInputComponent; UiCollectionProgressComponent; UiTextPanelComponent; UiOptionSelectionComponent; UiScreenPresentationComponent; UiCurrencyVisualsComponent; WorldInteractionVisualsComponent (implementation checkpoint 10/10)
currentComponent: none
pendingComponents: none
lastCheckpointUtc: 2026-09-12T01:58:17.2883869Z
evidence-gap: P16 first-round public-decomposition report and the referenced public split constraint file are absent; the input partition file currently hashes to 26c652b671371af4de21510f634947719a1b76908abf348e74e693ac67ce05ab while its header declares upstream source-report hash b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196; Version4 UI methods for event dispatch, history, and lifecycle are reduced; NLTX has no production client UI project; cross-subsystem owners for world/entity/network integration remain provisional; exact legacy callback, sorting, font metrics, renderer lifetimes, and world/file/favorites integration remain partial; Commerce balance ownership, emote transport, anchors, rarity, and wiring are unresolved. The focused src2 build/runtime verifier passed, but these integration evidence gaps remain.
blocking-decision: integration-review must decide whether the missing first-round evidence is a release gate, approve the client UI project/tree owner, and close renderer, input-clock, world-anchor, and network integration ownership before production integration. The P16 src2 implementation and focused verifier are complete; no cross-subsystem owner is claimed as resolved.

## 1. Execution boundary

This is a follow-up non-authoritative implementation record. The ten planned source units and the focused verifier project were created under `src2`; no production ECS component registration, project integration, Version4 migration, network migration, persistence migration, or behavior-equivalence claim is made. The listed production roles remain `status: proposed` until an integration owner accepts the design and adds an implementation-specific change record.

The proposed implementation target is a client-presentation capability boundary under `src2/ClientPresentation/Ui/`, subject to project-boundary review because the current NLTX checkout has no client UI project. It must not depend on server simulation internals or expose XNA/ReLogic/third-party types in shared authority components.

## 2. Checkpoint 1: UiLayoutGeometryValues

### Proposed files

| status | path | responsibility |
|---|---|---|
| proposed | `src2/ClientPresentation/Ui/Layout/UiStyleDimension.cs` | pixel/percentage layout input value |
| proposed | `src2/ClientPresentation/Ui/Layout/UiCalculatedStyle.cs` | derived geometry snapshot |
| proposed | `src2/ClientPresentation/Ui/Layout/UiSnapPoint.cs` | transient navigation/focus point |
| proposed | `src2/ClientPresentation/Ui/Layout/UiLayoutValueQuery.cs` | pure geometry calculation/query |

### Source-to-role mapping

`StyleDimension.Fill`, `Empty`, `Pixels`, `Precent`, `CalculatedStyle.X/Y/Width/Height`, and `SnapPoint.Name/_anchor/_offset/Id` map to the four proposed files above. The source members are presentation values, not world Entity components.

### Single writer and effects

The proposed `UiLayoutRecalculationSystem` is the only writer of derived calculated geometry. A proposed UI tree command may replace style inputs, but it cannot directly write the calculated cache. The query is pure; viewport size and UI scale enter through an explicit `IUiViewportAdapter` port. No clock, random, network, persistence or log effect is allowed in this component.

### Migration order and rollback

1. Add a focused pure verifier for pixel/percent/clamp/alignment and snap-point invalidation.
2. Introduce proposed value objects behind an adapter preserving existing UI-facing values.
3. Run an old/new geometry comparison in a client-only harness.
4. Cut the layout read path only after comparison passes.
5. Roll back by restoring the old layout projection and deleting only the newly introduced proposed implementation files; do not mutate world or network state during rollback.

Checkpoint 1 source files are implemented and the focused verifier passed. No network, persistence, renderer integration, or behavior-equivalence verification has been executed.

## 3. Checkpoint 2: UiElementLayoutComponent

### Proposed files and namespace

| status | path | proposed namespace | responsibility |
|---|---|---|---|
| proposed | `src2/ClientPresentation/Ui/Tree/UiElementLayoutComponent.cs` | `Terraria.ClientPresentation.Ui.Tree` | constraints, tree links, and calculated geometry for one UI node |
| proposed | `src2/ClientPresentation/Ui/Tree/UiElementTreeStore.cs` | `Terraria.ClientPresentation.Ui.Tree` | validates and owns parent/child adjacency |
| proposed | `src2/ClientPresentation/Ui/Tree/UiElementLayoutQuery.cs` | `Terraria.ClientPresentation.Ui.Tree` | immutable node/children query |
| proposed | `src2/ClientPresentation/Ui/Systems/UiElementLayoutSystem.cs` | `Terraria.ClientPresentation.Ui.Systems` | sole calculated-geometry writer |
| proposed | `src2/ClientPresentation/Ui/Commands/UiElementTreeCommands.cs` | `Terraria.ClientPresentation.Ui.Commands` | attach, detach, reparent, and constraint invalidation inputs |

The paths are implemented under `src2/ClientPresentation/Ui/` and included by `Terraria.ClientPresentation.Ui.csproj`. The exact runtime integration boundary still requires review because the existing checkout has no client UI project.

### Source-to-role mapping

Source sequences `4441`, `4442-4449`, `4455-4464`, `4465-4467`, `4539`, and `4541` map to the component, tree store, query, and layout system above. The old `UIElement` object reference in `Parent`/`Children` must become a presentation-only `UiElementId` link and immutable child projection. No `UIElement` or `CalculatedStyle` reference may cross into simulation components.

### Implementation order, single writers, and compatibility

1. Add pure tree/geometry verifiers and a cycle detector without changing runtime behavior.
2. Introduce the proposed node store behind an adapter that can read the existing UI tree.
3. Route append/remove/reparent operations through commands and keep the legacy tree as a read-only compatibility projection.
4. Run old/new geometry comparison fixtures for nested padding, margins, alignment, min/max constraints, and child order.
5. Switch hit-test and draw readers to immutable proposed snapshots only after comparison and lifecycle checks pass.

`UiElementTreeStore` is the only parent/child writer. `UiElementLayoutSystem` is the only calculated-style writer. During compatibility, legacy values may be projected from proposed snapshots, but there is no bidirectional dual write: the adapter owns the temporary legacy projection and rejects writes that bypass commands.

### Migration, rollback, and verification

No network or persistence migration is expected for this presentation-only unit. The proposed node ID is transient and must not be serialized as a world ID. Roll back by routing readers to the legacy UI projection, disabling proposed command intake, and discarding only transient presentation nodes; do not restore gameplay or network state from a UI snapshot. Rollback is required on cycle-detection divergence, geometry mismatch, stale-parent acceptance, or any observed simulation mutation.

Focused verifier passes cover cycle rejection, parent/child cleanup, deterministic recalculation, min/max clamping, padding/margin, alignment, zero-sized parents, viewport changes, and no external effects. Static checks must enforce one public type per file, no shared dependency on graphics types, and no direct component writes outside the two proposed owners. Checkpoint 1 and checkpoint 2 focused cases pass in the cumulative verifier.

## 4. Checkpoint 3: UiElementInteractionComponent

### Proposed files and namespace

| status | path | proposed namespace | responsibility |
|---|---|---|---|
| proposed | `src2/ClientPresentation/Ui/Tree/UiElementInteractionComponent.cs` | `Terraria.ClientPresentation.Ui.Tree` | lifecycle, interaction flags, hover result, and transient UI identity |
| proposed | `src2/ClientPresentation/Ui/Systems/UiElementLifecycleSystem.cs` | `Terraria.ClientPresentation.Ui.Systems` | initialize, activate, deactivate, and dispose transitions |
| proposed | `src2/ClientPresentation/Ui/Systems/UiElementInteractionSystem.cs` | `Terraria.ClientPresentation.Ui.Systems` | sole hover writer and explicit UI interaction command emission |
| proposed | `src2/ClientPresentation/Ui/Tree/UiElementInteractionQuery.cs` | `Terraria.ClientPresentation.Ui.Tree` | read-only hit-test/navigation capability queries |
| proposed | `src2/ClientPresentation/Ui/Rendering/UiElementGraphicsPolicyAdapter.cs` | `Terraria.ClientPresentation.Ui.Rendering` | isolates sampler/rasterizer and immediate-mode renderer state |

### Source-to-role mapping and single writers

Sequences `4450-4453`, `4469-4470`, `4475`, `4540`, and `4542` map to `UiElementInteractionComponent` and its lifecycle/interaction systems. Sequence `4471` maps to the proposed process-local identity allocator inside `UiElementLifecycleSystem`. Sequences `4454` and `4468` map only to `UiElementGraphicsPolicyAdapter`; `SamplerState` and `RasterizerState` must not appear in a shared ECS assembly. `UiElementInteractionSystem` alone writes hover state, while the lifecycle system alone writes initialization and identity state.

### Migration order, compatibility, and rollback

1. Build pure transition and identity tests, including duplicate lifecycle commands, stale node IDs, hover clearing, and snap-point cleanup.
2. Add the proposed interaction component behind a read-only adapter from the legacy UI element.
3. Route initialize/activate/deactivate through lifecycle commands while retaining a legacy callback projection.
4. Route pointer hit-test results into the proposed interaction system and compare hover/navigation decisions with the legacy UI harness.
5. Enable the graphics policy adapter only after the client project owner confirms the renderer dependency boundary.

The compatibility adapter may mirror legacy flags into a proposed snapshot, but it cannot dual-write hover or lifecycle state. Roll back by disabling proposed command intake and restoring the legacy callback projection; clear transient IDs and snap references, and never roll back by writing simulation, network, or persistence state. Trigger rollback on lifecycle divergence, cross-namespace ID use, stale hover after deactivation, or any graphics type leaking into shared state.

### Focused verifier and migration status

The cumulative verifier covers initialize/activate/deactivate recursion, idempotence, hover enter/out, pass-through versus ignored interaction boundaries, overflow clipping policy, snap-point cleanup, and adapter-only ownership of graphics objects. The interaction focused cases pass from `src2`; no renderer integration or behavior-equivalence verification has been executed.

## 5. Checkpoint 4: UiPointerInputComponent

### Proposed files and namespace

| status | path | proposed namespace | responsibility |
|---|---|---|---|
| proposed | `src2/ClientPresentation/Ui/Input/UiPointerInputComponent.cs` | `Terraria.ClientPresentation.Ui.Input` | host-scoped pointer position, button caches, hover, visibility, and current state link |
| proposed | `src2/ClientPresentation/Ui/Input/UiPointerEvent.cs` | `Terraria.ClientPresentation.Ui.Input` | immutable target/position event payload |
| proposed | `src2/ClientPresentation/Ui/Input/UiMouseEvent.cs` | `Terraria.ClientPresentation.Ui.Input` | pointer-position event payload |
| proposed | `src2/ClientPresentation/Ui/Input/UiScrollWheelEvent.cs` | `Terraria.ClientPresentation.Ui.Input` | wheel delta event payload |
| proposed | `src2/ClientPresentation/Ui/Systems/UiPointerDispatchSystem.cs` | `Terraria.ClientPresentation.Ui.Systems` | button cache, click/double-click classification, and command emission |
| proposed | `src2/ClientPresentation/Ui/Systems/UiStateTransitionSystem.cs` | `Terraria.ClientPresentation.Ui.Systems` | state transition, visibility, cleanup, and bounded history |
| proposed | `src2/ClientPresentation/Ui/Input/UiPointerInputAdapter.cs` | `Terraria.ClientPresentation.Ui.Input` | platform sample and monotonic-clock ports |
| proposed | `src2/ClientPresentation/Ui/Input/UiPointerQuery.cs` | `Terraria.ClientPresentation.Ui.Input` | read-only pointer/state query |

The pointer paths are implemented under `src2/ClientPresentation/Ui/`. `UiElementId`, `UiStateId`, the explicit clock port, and platform sample boundary remain proposed integration APIs; none is an existing NLTX production API.

### Source-to-role mapping and single writers

Sequences `4472-4474` map to immutable event payload files. Sequences `4476-4487` and `4490-4493` map to the host-scoped component and dispatch system. Sequence `4488` maps to an active-host registry adapter, `4489` to the bounded history store, and `4494-4495` to visibility/current-state fields managed by the transition system. `UiPointerDispatchSystem` is the only pointer-cache/hover writer; `UiStateTransitionSystem` is the only current-state/history writer; platform adapters never write component storage directly.

### Implementation order, compatibility, and rollback

1. Add pure click/double-click classification, state-change suppression, bounded-history, and event-target identity verifiers.
2. Introduce the proposed component and immutable payloads behind a read-only adapter from the legacy `UserInterface`.
3. Feed recorded platform samples into the proposed dispatch system without changing destination callbacks.
4. Compare event order, hover transitions, state activation/deactivation, and history pruning against a client behavior harness.
5. Route UI-only readers to proposed queries; only then expose destination commands for integration review.

Compatibility is one-way from legacy observations into proposed state until the cutover. There is no dual write of legacy pointer caches. Roll back by stopping proposed sample intake, clearing transient event/cache state, and restoring the legacy input projection. Trigger rollback on click classification divergence, state transition ordering changes, history overflow errors, ID namespace collisions, or any attempted simulation mutation.

### Migration, network, persistence, and focused verifier

Pointer caches, event payloads, state history, timestamps, and visibility are client-transient; they must not be serialized, persisted, or included in authoritative network snapshots. A destination command may be networked by the owning subsystem only after validation; this plan does not define that protocol. The cumulative verifier covers left/right button independence, click/double-click thresholds, state-change suppression, wheel payload preservation, history cap/prune, pointer clearing, and explicit clock samples. Pointer focused cases pass from `src2`; no behavior-equivalence verification has been executed.

## 6. Checkpoint 5: UiCollectionProgressComponent

### Proposed files and namespace

| status | path | proposed namespace | responsibility |
|---|---|---|---|
| proposed | `src2/ClientPresentation/Ui/Collections/UiCollectionProgressComponent.cs` | `Terraria.ClientPresentation.Ui.Collections` | list membership, scroll/progress state, and panel visual tokens |
| proposed | `src2/ClientPresentation/Ui/Systems/UiCollectionLayoutSystem.cs` | `Terraria.ClientPresentation.Ui.Systems` | content extent, clamping, and visibility derivation |
| proposed | `src2/ClientPresentation/Ui/Systems/UiProgressPresentationSystem.cs` | `Terraria.ClientPresentation.Ui.Systems` | one-way progress sample projection |
| proposed | `src2/ClientPresentation/Ui/Collections/UiCollectionCommand.cs` | `Terraria.ClientPresentation.Ui.Collections` | add/remove/clear/reorder/scroll/view commands |
| proposed | `src2/ClientPresentation/Ui/Collections/UiCollectionQuery.cs` | `Terraria.ClientPresentation.Ui.Collections` | count, visible IDs, scroll and can-scroll query |
| proposed | `src2/ClientPresentation/Ui/Rendering/UiCollectionGraphicsAdapter.cs` | `Terraria.ClientPresentation.Ui.Rendering` | texture, panel, scrollbar theme, and draw-resource boundary |

The collection/progress paths are implemented under `src2/ClientPresentation/Ui/`; the project boundary and graphics assembly remain an integration decision.

### Source-to-role mapping and single writers

Sequences `2264-2265` map to progress targets; `2267-2271` map to collection membership and layout policy; `2272-2277` map to panel visual tokens or the graphics adapter; `2278-2281` and `3838-3839` map to scroll state and derived scroll queries; `2282-2284` map to the graphics/theme adapter; `3837` maps to the read-only count query. The collection command store alone writes membership/order, the collection layout system alone writes derived scroll state, and the progress system alone writes progress targets.

### Implementation order, compatibility, and rollback

1. Add pure verifiers for ordered membership, duplicate/stale IDs, scroll clamping, empty/equal views, progress bounds, and deterministic sort commands.
2. Introduce the proposed collection state behind an adapter that observes legacy UI controls.
3. Migrate list/panel/scrollbar commands and compare child order, culling, scrollbar attachment, and visible-range decisions.
4. Feed progress through an explicit presentation sample adapter; do not dual-write world-generation state.
5. Switch draw/resource reads only after the graphics project owner confirms the adapter boundary.

Compatibility is one-way legacy observation -> proposed state until cutover. The renderer adapter may project proposed theme tokens to legacy draw calls, but external asset handles are never dual-owned. Roll back by stopping proposed collection commands and routing readers to the legacy controls; discard transient child/scroll/progress state only. Roll back on ordering divergence, invalid clamp behavior, asset lifetime leaks, or progress values crossing into authority.

### Network, persistence, focused verification, and status

List membership, scroll state, panel colors, texture handles, and progress targets are client presentation data and are excluded from authoritative network and persistence snapshots. Domain collections and world-generation progress remain owned by their source subsystems. The cumulative verifier covers add/remove/clear, deterministic sorting, scrollbar attachment, clamping, auto-hide, visible-range queries, progress sample rejection, and renderer adapter isolation. Collection/progress focused cases pass from `src2`; no behavior-equivalence verification has been executed.

## 7. Checkpoint 6: UiTextPanelComponent

### Proposed files and namespace

| status | path | proposed namespace | responsibility |
|---|---|---|---|
| proposed | `src2/ClientPresentation/Ui/Text/UiTextPanelComponent.cs` | `Terraria.ClientPresentation.Ui.Text` | typed content, text style, wrapping, origin, and fit policy |
| proposed | `src2/ClientPresentation/Ui/Text/UiTextContentCommand.cs` | `Terraria.ClientPresentation.Ui.Text` | content/style mutation boundary |
| proposed | `src2/ClientPresentation/Ui/Systems/UiTextMeasurementSystem.cs` | `Terraria.ClientPresentation.Ui.Systems` | localization resolution and sole measured-size writer |
| proposed | `src2/ClientPresentation/Ui/Text/UiTextQuery.cs` | `Terraria.ClientPresentation.Ui.Text` | resolved content/style/measurement query |
| proposed | `src2/ClientPresentation/Ui/Rendering/UiTextRenderProjection.cs` | `Terraria.ClientPresentation.Ui.Rendering` | immutable draw command projection |
| proposed | `src2/ClientPresentation/Ui/Text/UiTextLocalizationAdapter.cs` | `Terraria.ClientPresentation.Ui.Text` | localization and font measurement ports |

The text paths are implemented under `src2/ClientPresentation/Ui/`; `LocalizedText`, font, and graphics types remain outside shared component state and inside adapters or projections.

### Source-to-role mapping and single writers

Sequences `2266` and `3836` map to header content command/query; `2285-2292` map to the text component and measurement system; `3840-3846` map to text query/command style fields. `UiTextContentCommand` is the only content/style writer, `UiTextMeasurementSystem` is the only measured-size writer, and `UiTextRenderProjection` is read-only.

### Implementation order, compatibility, and rollback

1. Add pure source-kind, wrap/fit, origin, and deterministic measurement-result verifiers.
2. Introduce the typed source component behind a legacy `string`/`LocalizedText` observation adapter.
3. Compare resolved text, measured size, wrapping, dynamic scale, and layout invalidation against a client fixture.
4. Route draw calls through immutable render projections only after localization/font owners accept the adapter contract.

There is no bidirectional dual write. During compatibility, legacy controls may be populated from proposed resolved snapshots, while all proposed content changes flow through commands. Roll back by disabling proposed content commands and restoring the legacy text projection; discard transient measurement caches only. Trigger rollback on resolution mismatch, font-metric divergence, source-kind leakage, or an external graphics/localization object entering component state.

### Network, persistence, focused verifier, and status

Text content, measured sizes, colors, and wrapping are client presentation data. They are excluded from authoritative network and persistence snapshots unless a destination subsystem separately defines a typed domain message. The cumulative verifier covers localized source resolution, missing keys, dynamic scale-down, origin, color/shadow projection, and adapter isolation. Text focused cases pass from `src2`; revision invalidation, exact header resize, font metrics, renderer integration, and behavior-equivalence remain unverified.

## 8. Checkpoint 7: UiOptionSelectionComponent

### Proposed files and namespace

| status | path | proposed namespace | responsibility |
|---|---|---|---|
| proposed | `src2/ClientPresentation/Ui/Options/UiOptionSelectionComponent.cs` | `Terraria.ClientPresentation.Ui.Options` | option token, accepted selection token, visual policy, and icon/title projection data |
| proposed | `src2/ClientPresentation/Ui/Options/UiOptionSelectionCommand.cs` | `Terraria.ClientPresentation.Ui.Options` | validated selection intent and acceptance boundary |
| proposed | `src2/ClientPresentation/Ui/Options/UiOptionSelectionQuery.cs` | `Terraria.ClientPresentation.Ui.Options` | option/is-selected/effective-style query |
| proposed | `src2/ClientPresentation/Ui/Systems/UiOptionSelectionSystem.cs` | `Terraria.ClientPresentation.Ui.Systems` | sole accepted-selection writer |
| proposed | `src2/ClientPresentation/Ui/Rendering/UiOptionGraphicsAdapter.cs` | `Terraria.ClientPresentation.Ui.Rendering` | texture/icon/panel resource and highlight projection |

The option paths are implemented under `src2/ClientPresentation/Ui/`; the generic option token shape and destination command contract still require the owning Creative/player/configuration subsystem's review.

### Source-to-role mapping and single writers

Sequence `2245` maps to the accepted selection token; `2250` to the immutable button token; `2251-2257` and `2260-2263` to component visual policy; `2258` to a localized description key; `2259` to a child text-node link; `2246-2249` to the graphics adapter; `3830-3835` to immutable/derived option and icon queries. `UiOptionSelectionSystem` alone writes accepted selection; the graphics adapter alone owns texture handles.

### Implementation order, compatibility, and rollback

1. Add pure option-group membership/equality and effective-color verifiers.
2. Add the proposed component behind a read-only adapter from legacy option buttons.
3. Route pointer selections through typed commands and compare selected/hovered/highlighted projections.
4. Keep destination domain state unchanged until integration review accepts the cross-subsystem command.
5. Enable asset/icon projections after the graphics adapter validates resource ownership.

Legacy observations may seed proposed snapshots, but there is no bidirectional dual write of selection. Roll back by disabling proposed selection commands, clearing transient accepted tokens, and restoring the legacy button projection. Roll back on group mismatch, selection divergence, cross-domain mutation, asset lifetime errors, or use of a UI token as a persistence/network identity.

### Network, persistence, focused verifier, and status

Button selection, hover/highlight, icon frame, colors, and title/description projections are client-transient and excluded from authoritative network/persistence snapshots. Only a separately owned destination command may cross those boundaries. The cumulative verifier covers group membership, equality, initial selection, accepted/rejected intents, selected/unselected colors, icon frame/scale/offset, missing icons, title link projection, and graphics isolation. Option focused cases pass from `src2`; destination integration, exact legacy highlight/audio behavior, and behavior-equivalence remain unverified.

## 9. Checkpoint 8: UiScreenPresentationComponent

### Proposed files and namespace

| status | path | proposed namespace | responsibility |
|---|---|---|---|
| proposed | `src2/ClientPresentation/Ui/Screens/UiScreenPresentationComponent.cs` | `Terraria.ClientPresentation.Ui.Screens` | screen kind, child links, activation, attachment state, and transient screen cache |
| proposed | `src2/ClientPresentation/Ui/Screens/UiScreenLifecycleSystem.cs` | `Terraria.ClientPresentation.Ui.Screens` | child graph construction/teardown and activation boundary |
| proposed | `src2/ClientPresentation/Ui/Screens/UiWorldLoadProjection.cs` | `Terraria.ClientPresentation.Ui.Screens` | loading/world-generation query to progress/text commands |
| proposed | `src2/ClientPresentation/Ui/Screens/UiWorldSelectProjection.cs` | `Terraria.ClientPresentation.Ui.Screens` | world summary/favorites query to list and selection commands |
| proposed | `src2/ClientPresentation/Ui/Screens/UiScreenQuery.cs` | `Terraria.ClientPresentation.Ui.Screens` | read-only screen state query |
| proposed | `src2/ClientPresentation/Ui/Screens/UiWorldSummarySnapshot.cs` | `Terraria.ClientPresentation.Ui.Screens` | immutable screen-safe world summary projection |

The screen paths are implemented under `src2/ClientPresentation/Ui/`; `WorldFileData`, file I/O, world generation, favorite persistence, and network messages stay outside these files.

### Source-to-role mapping and single writers

Sequences `2293-2294` map to progress/header child links, `2295` to the read-only world summary adapter/snapshot, `2296-2298` to collection/panel/scroll child links, `2299` to lifecycle-owned attachment state, and `2300` to a transient cache fed by a favorites query. `UiScreenLifecycleSystem` alone writes child links and attachment state; projections only emit commands.

### Implementation order, compatibility, and rollback

1. Add pure screen lifecycle and scrollbar attach/detach verifiers, including no-scroll/scrolling transitions.
2. Introduce screen snapshots behind adapters from legacy `UIWorldLoad` and `UIWorldSelect`.
3. Compare child creation, progress/header commands, world-list item ordering, favorites display, and layout width changes.
4. Expose world-selection commands only after the world/file owner validates their destination contract.

Compatibility is one-way legacy observations -> proposed screen snapshot until cutover. The proposed screen may project commands to legacy controls, but it may not dual-own world data or favorites. Roll back by deactivating proposed screen lifecycle, clearing transient child/cache state, and restoring legacy screen readers. Roll back on child graph divergence, stale world snapshot use, favorite ownership violation, or any screen command mutating world/file state.

### Network, persistence, focused verifier, and status

Screen child links, active state, scrollbar attachment, progress/header projections, and favorite display cache are client-transient. World summaries may contain persistence/network identifiers only as typed read-only fields and must not be written by the screen. The cumulative verifier covers activation/deactivation, child cleanup, progress projection, scrollbar attachment, stale snapshot rejection, and no file/network/world-generation side effects. Screen source is implemented under `src2`; the cumulative focused build/runtime verifier passed after the final world-interaction change.

## 10. Checkpoint 9: UiCurrencyVisualsComponent

### Proposed files and namespace

| status | path | proposed namespace | responsibility |
|---|---|---|---|
| proposed | `src2/ClientPresentation/Ui/Currency/UiCurrencyVisualsComponent.cs` | `Terraria.ClientPresentation.Ui.Currency` | currency draw scale, text key, color, and visual formatting policy |
| proposed | `src2/ClientPresentation/Ui/Currency/UiCurrencyRegistryAdapter.cs` | `Terraria.ClientPresentation.Ui.Currency` | transient UI registration key and definition lookup adapter |
| proposed | `src2/ClientPresentation/Ui/Currency/UiCurrencyDefinitionQuery.cs` | `Terraria.ClientPresentation.Ui.Currency` | read-only denomination/cap query from Commerce owner |
| proposed | `src2/ClientPresentation/Ui/Systems/UiCurrencyProjection.cs` | `Terraria.ClientPresentation.Ui.Systems` | balance/price snapshot to text/icon draw projection |
| proposed | `src2/ClientPresentation/Ui/Currency/UiCurrencyPresentationAdapter.cs` | `Terraria.ClientPresentation.Ui.Currency` | localization/color/renderer formatting boundary |

The currency paths are implemented under `src2/ClientPresentation/Ui/`. `CurrencyBalanceComponent` remains outside P16 and no purchase/transaction implementation is included here.

### Source-to-role mapping and single writers

Sequence `2301` maps to the transient registry allocator adapter; `2302` to the registry/definition adapter; `2303-2305` to `UiCurrencyVisualsComponent`; and `2306-2307` to read-only Commerce definition snapshots. The registry adapter alone allocates presentation registration keys, the proposed component command owns visual metadata, and Commerce alone writes denominations, caps, balances, and transactions.

### Implementation order, compatibility, and rollback

1. Add pure formatting and typed-key namespace verifiers, including cap/overflow display decisions as read-only cases.
2. Introduce visual metadata behind an adapter from legacy currency definitions.
3. Route registry lookups and balance/price snapshots through explicit queries without dual-writing Commerce state.
4. Compare text/color/scale and missing-definition behavior against a client fixture.

Compatibility is one-way legacy definition/balance observations -> proposed visual snapshot. Roll back by disabling the proposed registry/projection and restoring the legacy display path; discard transient presentation keys and caches only. Roll back on a registration collision, balance mutation, purchase side effect, or any persistence/network identity being reused as a UI key.

### Network, persistence, focused verifier, and status

Visual metadata, registration keys, and formatted output are client-transient. Currency definitions, balances, caps, revisions, and transactions remain owned by Commerce and follow its own network/persistence contracts; this plan does not serialize or migrate them. The cumulative verifier covers registration/reset, duplicate keys, missing definitions, denomination/cap queries, balance/price projection, overflow/zero display, and proof by dependency isolation that projection cannot mutate `CurrencyBalanceComponent`. Currency source is implemented under `src2`; the cumulative focused build/runtime verifier passed after the final world-interaction change.

## 11. Checkpoint 10: WorldInteractionVisualsComponent

### Proposed files and namespace

| status | path | proposed namespace | responsibility |
|---|---|---|---|
| proposed | `src2/ClientPresentation/Ui/World/WorldInteractionVisualsComponent.cs` | `Terraria.ClientPresentation.Ui.World` | bubble, rarity, wire-radial, and typed anchor projection state |
| proposed | `src2/ClientPresentation/Ui/World/UiWorldAnchor.cs` | `Terraria.ClientPresentation.Ui.World` | entity/tile/position/none anchor value object |
| proposed | `src2/ClientPresentation/Ui/Systems/UiEmoteProjectionSystem.cs` | `Terraria.ClientPresentation.Ui.Systems` | bubble lifetime/frame projection and cleanup |
| proposed | `src2/ClientPresentation/Ui/World/UiEmoteTransportAdapter.cs` | `Terraria.ClientPresentation.Ui.World` | validated network payload/anchor translation boundary |
| proposed | `src2/ClientPresentation/Ui/World/UiRarityQuery.cs` | `Terraria.ClientPresentation.Ui.World` | read-only rarity-color query |
| proposed | `src2/ClientPresentation/Ui/Systems/UiWiresInteractionSystem.cs` | `Terraria.ClientPresentation.Ui.Systems` | radial/tool-mode presentation and explicit wiring intent |
| proposed | `src2/ClientPresentation/Ui/World/UiWorldAnchorQuery.cs` | `Terraria.ClientPresentation.Ui.World` | typed anchor to screen-coordinate query |

The world-interaction paths are implemented under `src2/ClientPresentation/Ui/`; entity/network/wiring owners must approve the typed adapter contracts before production integration.

### Source-to-role mapping and single writers

Sequences `2308-2322` map to bubble projection state, transport adapter, and animation constants; `2323` maps to the rarity query; `2324-2328` to the wires interaction system/radial state; and `2329-2332` to the typed anchor value/query. Bubble frame/lifetime state, radial state, rarity definitions, and anchor resolution each have one proposed owner. No adapter writes a raw entity or tile object into component storage.

### Implementation order, compatibility, and rollback

1. Add pure anchor validation, bubble lifetime/frame, rarity fallback, and radial state-machine verifiers.
2. Introduce typed world-anchor and visual state behind read-only legacy observations.
3. Validate inbound emote transport payloads and compare bubble lifecycle/anchor placement against a client fixture.
4. Project rarity and wire radial state without mutating item/tile authority.
5. Expose outbound emote/wiring intents only after integration owners accept their command/network contracts.

Compatibility is one-way legacy/network observations -> proposed visual snapshots. Roll back by disabling proposed projections and clearing transient bubbles/radial state; restore legacy draw/input paths and leave entity, tile, player, NPC, network, and persistence state untouched. Roll back on invalid anchor acceptance, ID namespace collision, bubble cleanup leak, network identity confusion, or any direct wiring/world mutation.

### Network, persistence, focused verifier, and status

Bubble network payloads are handled by the transport adapter and are not persisted by this component. Entity references, tile coordinates, item rarity values, and wiring intents retain their source subsystem's network/persistence contracts; P16 only consumes or emits typed boundaries. The cumulative verifier covers anchor type/slot/size validation, bubble cleanup/frame advance, unknown rarity fallback, radial open/close/tool-mode transitions, outbound intent isolation, and no direct world/entity mutation. World-interaction source is implemented under `src2`; the final cumulative build/runtime verifier passed through the serial repository wrapper.

## 12. Integration Handoff

- All 176 source members are mapped across the ten proposed units: 12 layout values, 24 element-layout members, 12 element-interaction members, 23 pointer-input members, 23 collection/progress members, 17 text/header members, 25 option-selection members, 8 screen members, 7 currency members, and 25 world-interaction members.
- The source paths are implemented under `src2/ClientPresentation/Ui/` and the focused project is `src2/ClientPresentation/UiVerification/`; this checkout still has no production client UI project. Production integration must establish project, assembly-reference, and namespace ownership before cutover.
- Single-writer contract: tree store owns parent/child links; layout system owns calculated geometry; lifecycle system owns identity/activation; pointer dispatch owns pointer cache/hover; state transition owns current state/history; collection store/layout/progress systems own their respective state; text command/measurement own content/measurement; option selection system owns accepted selection; screen lifecycle owns child links; currency registry/Commerce separation owns registration/definitions; world interaction systems own only visual state and typed adapter boundaries.
- Proposed implementation sequence and rollback order follow the ten checkpoints above. Compatibility is observation-first and one-way; no bidirectional dual write is planned for UI state, world state, currency balance, network objects, or persistence records.
- Focused verification in this session included source-path/type checks, no third-party graphics types in the UI boundary, typed identity separation, command-only cross-domain writes, lifecycle cleanup, adapter isolation, and the cumulative runtime cases. The build used the repository serial wrapper from the root and produced the recorded `Build/bin/` artifacts; production integration and behavior-equivalence verification remain future work.
- `crossSubsystemOwner: integration-review` remains on the missing first-round evidence decision, client UI project/tree store, renderer/input/clock/font/localization adapters, world/file/favorites snapshots, Creative/player option destinations, Commerce queries and purchase commands, emote transport/entity anchors, rarity definitions, and wiring authority.
- Input provenance must be reconciled before production integration: the current partition file hash is `26c652b671371af4de21510f634947719a1b76908abf348e74e693ac67ce05ab`, while the header's upstream source-report hash is `b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`. This implementation does not modify the input report.

The execution plan remains a proposed integration contract, while all ten planned P16 source units are implemented under `src2`. The final cumulative build/runtime verification passed after the world-interaction and anchor-size changes. No component registration in production, network migration, persistence migration, behavior-equivalence proof, or cross-subsystem owner resolution was executed.

## 14. Exact Source-to-Target Manifest

Every source sequence is listed individually below. The checkpoint mappings bind each sequence to the proposed target role/file; no source member is intentionally deferred.

| proposed unit | exact source sequences | proposed target files/roles |
|---|---|---|
| `UiLayoutGeometryValues` | 4272, 4273, 4274, 4275, 4434, 4435, 4436, 4437, 4438, 4439, 4440, 4538 | `UiStyleDimension`, `UiCalculatedStyle`, `UiSnapPoint`, `UiLayoutValueQuery` |
| `UiElementLayoutComponent` | 4441, 4442, 4443, 4444, 4445, 4446, 4447, 4448, 4449, 4455, 4456, 4457, 4458, 4459, 4460, 4461, 4462, 4463, 4464, 4465, 4466, 4467, 4539, 4541 | `UiElementLayoutComponent`, `UiElementTreeStore`, `UiElementLayoutQuery`, `UiElementLayoutSystem`, tree commands |
| `UiElementInteractionComponent` | 4450, 4451, 4452, 4453, 4454, 4468, 4469, 4470, 4471, 4475, 4540, 4542 | `UiElementInteractionComponent`, lifecycle/interaction systems, `UiElementGraphicsPolicyAdapter` |
| `UiPointerInputComponent` | 4472, 4473, 4474, 4476, 4477, 4478, 4479, 4480, 4481, 4482, 4483, 4484, 4485, 4486, 4487, 4488, 4489, 4490, 4491, 4492, 4493, 4494, 4495 | immutable event payloads, `UiPointerInputComponent`, dispatch/state systems, adapter/query |
| `UiCollectionProgressComponent` | 2264, 2265, 2267, 2268, 2269, 2270, 2271, 2272, 2273, 2274, 2275, 2276, 2277, 2278, 2279, 2280, 2281, 2282, 2283, 2284, 3837, 3838, 3839 | `UiCollectionProgressComponent`, collection/progress systems, collection command/query, graphics adapter |
| `UiTextPanelComponent` | 2266, 2285, 2286, 2287, 2288, 2289, 2290, 2291, 2292, 3836, 3840, 3841, 3842, 3843, 3844, 3845, 3846 | `UiTextPanelComponent`, content command, measurement system, text query/projection, localization adapter |
| `UiOptionSelectionComponent` | 2245, 2246, 2247, 2248, 2249, 2250, 2251, 2252, 2253, 2254, 2255, 2256, 2257, 2258, 2259, 2260, 2261, 2262, 2263, 3830, 3831, 3832, 3833, 3834, 3835 | `UiOptionSelectionComponent`, command/query/system, `UiOptionGraphicsAdapter` |
| `UiScreenPresentationComponent` | 2293, 2294, 2295, 2296, 2297, 2298, 2299, 2300 | screen component/lifecycle, world-load/world-select projections, summary snapshot/query |
| `UiCurrencyVisualsComponent` | 2301, 2302, 2303, 2304, 2305, 2306, 2307 | currency visuals, registry adapter, definition query, currency projection |
| `WorldInteractionVisualsComponent` | 2308, 2309, 2310, 2311, 2312, 2313, 2314, 2315, 2316, 2317, 2318, 2319, 2320, 2321, 2322, 2323, 2324, 2325, 2326, 2327, 2328, 2329, 2330, 2331, 2332 | world visual component, anchor value/query, emote transport/projection, rarity query, wires system |

## 13. Initial verifier plan

- Pure cases: `Fill`, `Empty`, mixed pixels/percent, negative pixel offsets, min/max clamp, parent padding/margin, alignment, zero-size parent and deterministic repeat calculation.
- Lifecycle cases: append/remove invalidates geometry; state deactivation clears snap points; recalculation never sends a network message or mutates a simulation component.
- Static checks: one public type per file, `Component` suffix only for attachable components, no proposed UI path treated as existing, and no external graphics type in shared state.
- Required command shape was followed: the affected project was built and run serially through `Build/Tools/Invoke-SerialDotnet.ps1`; the exact command and result are recorded below and in the session handoff.

## 4. Planned component sequence

1. `UiLayoutGeometryValues`
2. `UiElementLayoutComponent`
3. `UiElementInteractionComponent`
4. `UiPointerInputComponent`
5. `UiCollectionProgressComponent`
6. `UiTextPanelComponent`
7. `UiOptionSelectionComponent`
8. `UiScreenPresentationComponent`
9. `UiCurrencyVisualsComponent`
10. `WorldInteractionVisualsComponent`

The sequence is a proposed scheduler contract, not a runtime fact. Cross-domain commands, IDs, snapshots and network ordering remain `crossSubsystemOwner: integration-review`.

## 5. Explicit non-execution statement

C# source and project files for all ten planned units have been created under `src2`; the focused build and runtime verifier passed. No network migration, persistence migration, renderer production integration, or behavior-equivalence verification has been executed.

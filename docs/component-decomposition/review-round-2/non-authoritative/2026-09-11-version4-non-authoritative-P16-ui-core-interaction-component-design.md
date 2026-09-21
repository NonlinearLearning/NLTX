# P16 UI 核心与交互：proposed Component Design

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

## 1. 结论边界

本文件是 P16 的非权威第二轮 proposed 设计。它只把当前分区的 176 条 Version4 成员库存映射为候选的 UI value object、presentation Component、System、Query、Command、Adapter 和 Projection。它不表示 NLTX 已经拥有这些类型，不表示 Terraria 行为等价，也不裁决跨分区 owner、网络协议、持久化格式或最终调度顺序。

本分区范围是世界加载/世界选择屏幕、自定义货币 UI、表情/线路/世界锚点/稀有度呈现、列表/面板/滚动条/进度控件、选项按钮、文本/标题、布局值对象、UI 事件、指针缓存、元素树布局和元素生命周期。排除真实模拟状态、玩家/物品/世界/地图的权威写入、网络传输实现、存档格式、平台窗口实现、第三方图形资源和其他 P 分区成员；这些边界只通过 proposed port 或 `crossSubsystemOwner: integration-review` 交接。

## 2. Version4 事实与证据

### 2.1 直接源码

- `D:\TRbackup\Version4\Terraria.UI\StyleDimension.cs:3-54` 保存像素/百分比布局输入，并由 `Set`、`GetValue` 和静态构造方法修改或计算。
- `D:\TRbackup\Version4\Terraria.UI\CalculatedStyle.cs:3-37` 保存计算后的 X/Y/Width/Height，并提供矩形和位置派生值。
- `D:\TRbackup\Version4\Terraria.UI\SnapPoint.cs:7-24` 保存名称、私有 anchor/offset 和只读 Id；Version4 文件没有完整计算方法。
- `D:\TRbackup\Version4\Terraria.UI\UIElement.cs:23-95` 声明元素树、布局输入、边距/填充、派生尺寸、父子关系、唯一 ID 和 hover 状态；`:120-254` 记录 append/remove/recalculate/copy-style；`:268-319` 记录输入回调和 initialize/activate/deactivate 生命周期。
- `D:\TRbackup\Version4\Terraria.UI\UserInterface.cs:12-154` 声明左右指针缓存、当前 UIState、历史、可见性和屏幕尺寸；`:115-143` 记录 state 切换时的 MouseOut、Deactivate、Activate、Recalculate 和清理。
- `D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\UIList.cs:24-70`、`UIScrollbar.cs:17-73`、`UIPanel.cs:10-49`、`UIGenProgressBar.cs:11-30`、`UIHeader.cs:10-42`、`UIText.cs:15-111` 是内容控件的库存状态和写入口。
- `D:\TRbackup\Version4\Terraria.GameContent.UI.Elements\GroupOptionButton.cs:14-155` 保存选项、颜色、纹理、标题、图标和选择派生状态；其 `SetCurrentOption`、`SetIconFrame` 是已观测写入口。
- `D:\TRbackup\Version4\Terraria.GameContent.UI.States\UIWorldLoad.cs:16-39` 构造进度条/标题并 append；`UIWorldSelect.cs:18-58` 根据 scrollbar 是否可滚动增删子元素并调整列表宽度。
- `D:\TRbackup\Version4\Terraria.GameContent.UI\CustomCurrencyManager.cs:8-46` 注册货币并进行 `Item` 接受判断；`CustomCurrencySystem.cs:8-51` 保存货币单位和 cap 的候选定义及购买/价格接口；`CustomCurrencySingleCoin.cs:12-34` 保存 Defender Medal 的 UI 元数据。
- `D:\TRbackup\Version4\Terraria.GameContent.UI\EmoteBubble.cs:10-242` 保存短时气泡、anchor、动画帧、随机 NPC 表情和网络序列化入口；`MessageBuffer.cs:2660-2694` 接收网络气泡并写入 `byID`，`:2877-2880` 临时覆盖线路工具模式，`:2980-2981` 触发表情。
- `D:\TRbackup\Version4\Terraria.GameContent.UI\WiresUI.cs:8-36` 保存线路工具模式和 radial 交互缓存；`WorldUIAnchor.cs:6-47` 区分 Entity/Tile/Pos/None 锚点。
- `D:\TRbackup\Version4\Terraria.Main.cs:190-192,3336-3343,11231-11235` 声明 MenuUI/InGameUI、启动注册货币/稀有度并清理 MenuUI；`Terraria.WorldGen.cs:6296-6309` 把新世界状态交给 UIWorldLoad。
- `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowers.cs:84-150,211-289,431-729` 将 `GroupOptionButton<int>`、`UIElement` 和 slider 作为 Creative UI 工厂边界；这些属于本分区的直接读者，Creative 权威状态不在本分区重新设计。

输入分区文件当前 SHA-256 为 `26c652b671371af4de21510f634947719a1b76908abf348e74e693ac67ce05ab`；其文件头声明的上游来源报告 SHA-256 为 `b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`。本会话重新检查了输入报告的 176 个成员行，Version4 路径/行/成员文本 `mismatches=0`；两者的 provenance 差异保留为 evidence-gap，不修改输入报告。

### 2.2 补证角色

- 完整参考只用于补足 Version4 已存在文件的调用时序：`D:\TRbackup\无任何删减通过编译\Terraria.UI\UIElement.cs:97-700` 和 `UserInterface.cs:98-392` 显示事件向父级传播、Update/HandleClick、500ms double-click、200ms state-change 禁用、历史裁剪和 lifecycle 递归；对应 Version4 仍以实际声明和调用为主。
- tModLoader stable API mirror 的首页 `D:\TRbackup\tmodloader-api-docs-stable\index.html` 显示 `tModLoader v2026.07`。交叉证据为 `class_u_i_element.html:99,212-218,489-490,612,648,678,707,736`、`class_u_i_list.html:99,146,568`、`class_u_i_scroll_wheel_event.html:100-118`、`struct_style_dimension.html:98,151,181`、`class_user_interface.html:8,128,143,155`。它只确认公开 UI 的嵌套、列表/滚轮、布局和 state 概念，不替代 Version4 私有实现。
- SS14 只作为粒度参考，实际读取 `Content.Shared/UserInterface/IntrinsicUIComponent.cs:6-25`、`IntrinsicUISystem.cs:5-65`、`Content.Client/Wires/UI/WiresBoundUserInterface.cs:7-41` 和 `Content.Tests/Client/UserInterface/Controls/ListContainerTest.cs:23-289`。它支持“客户端 UI 状态、打开/关闭事件、服务器消息和 focused layout verifier 分离”的组织模式，不支持 Terraria 语义推断。
- P16 第一轮报告 `D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P16-ui-core-interaction-public-decomposition.md` 未找到；因此所有第一轮读写者/生命周期结论均由本次直接源码复核重新建立，并标记为 provisional。

## 3. 当前 NLTX 状态

当前 `D:\TRbackup\NLTX\src` 没有 `UIElement`、`UserInterface`、`UIState`、`UIMouseEvent`、`UIScrollWheelEvent`、`CustomCurrencySystem`、`EmoteBubble` 或 `WorldUIAnchor` 的实现或对应客户端项目。已存在的相邻状态包括：

- `src\Content\ContentPresentationIndex.cs:5-58`：只读的内容展示索引，包含物品/NPC/投射物的展示派生值。
- `src\Items\Commerce\CurrencyBalanceComponent.cs:5-54`：带 `ExternalAccountId`、余额、cap、revision 和 transaction 的领域权威状态，不是 UI 状态。
- `src\Player\InputIntentComponent.cs:3-64`：玩家模拟输入意图，不是鼠标指针、UI hit-test 或点击 payload。

因此当前 NLTX 映射为 `not-implemented`；本轮不把任何 proposed 类型写成已存在能力，也不把相邻领域组件改成 UI owner。

## 4. Checkpoint 1：UiLayoutGeometryValues（proposed）

### 4.1 角色与成员覆盖

这是布局输入和计算几何的 value-object 边界，不是可附加到模拟实体的权威 Component。它覆盖全部 12 条库存成员：

| source sequence | proposed role | member |
|---:|---|---|
| 4272-4275 | proposed `UiCalculatedStyle` value object | `CalculatedStyle.X`, `Y`, `Width`, `Height` |
| 4434-4436,4538 | proposed `UiSnapPoint` value object | `SnapPoint.Name`, `_anchor`, `_offset`, `Id` |
| 4437-4440 | proposed `UiStyleDimension` value object | `StyleDimension.Fill`, `Empty`, `Pixels`, `Precent` |

注：库存序号 4437-4439 的交叉行必须以输入表的声明类型为准；实际覆盖集合是 `StyleDimension` 的四个成员（4437-4440），而不是将 `Fill`/`Empty` 误归到 SnapPoint。

### 4.2 Proposed shape

- proposed `UiStyleDimension`：输入 `Pixels`、百分比值和 container size，纯计算 `GetValue`；`Fill`/`Empty` 是定义常量，不注册为 Component。
- proposed `UiCalculatedStyle`：由 layout system 生成的派生快照，只读输出 X/Y/Width/Height；禁止作为下一帧权威输入回写布局约束。
- proposed `UiSnapPoint`：短生命周期导航/焦点投影，引用 proposed `UiElementId`；不持有持久化 ID、网络 ID 或模拟实体对象。
- proposed `UiLayoutValueQuery`：纯函数查询 `StyleDimension + parent geometry -> CalculatedStyle`，无缓存写入、无时间/随机/日志/IO。

### 4.3 Proposed target paths and contract

| proposed path | proposed type | status |
|---|---|---|
| `src2/ClientPresentation/Ui/Layout/UiStyleDimension.cs` | `UiStyleDimension` | proposed |
| `src2/ClientPresentation/Ui/Layout/UiCalculatedStyle.cs` | `UiCalculatedStyle` | proposed |
| `src2/ClientPresentation/Ui/Layout/UiSnapPoint.cs` | `UiSnapPoint` | proposed |
| `src2/ClientPresentation/Ui/Layout/UiLayoutValueQuery.cs` | `UiLayoutValueQuery` | proposed |

上述路径已在 `src2/ClientPresentation/Ui/Layout/` 实际创建并纳入 `Terraria.ClientPresentation.Ui.csproj`；命名遵循领域/能力优先和一文件一个核心公开类型；没有创建 `Data`、`Manager` 或泛化 `Shared/Components` 目录。

### 4.4 Ownership and lifecycle

- Writer：仅 proposed `UiLayoutRecalculationSystem` 写入 `UiCalculatedStyle` 派生快照；`UiStyleDimension` 由元素构建/布局命令写入；`UiSnapPoint` 由元素焦点/导航适配器创建和清理。
- Reader：元素布局、hit-test、draw 和 gamepad projection 只读这些值；跨分区读者必须通过只读 snapshot/Query。
- Lifecycle：元素创建/Append 后计算；父尺寸、viewport、scale 或 style 变化后失效并重算；元素移除、state deactivation 或 UI dispose 时清理 snap point。
- Invariant：百分比和像素计算确定性；派生 geometry 不得驱动世界/玩家状态；anchor/offset 不得混入 entity persistence 或 network identity。

### 4.5 Current checkpoint decision

该组件已在 `src2` 实现并由 focused verifier 独立验证像素/百分比、`Fill`、`Empty`、派生几何、snap point 和布局查询；仍需整合会话确认 `UiElementId` 与现有实体 ID 的隔离、布局单位/缩放来源和 gamepad snap point owner。其生产集成证据仍受 integration-review 边界约束。

## 6. Checkpoint 2：UiElementLayoutComponent（proposed）

### 6.1 角色与成员覆盖

`UiElementLayoutComponent` 是 UI 元素树节点的布局输入、树关系和派生几何快照边界。它不是世界实体组件，也不把 `UIElement` 对象或第三方图形对象暴露给共享模拟层。它覆盖 24 条库存成员：

| source sequence | source member | proposed role |
|---:|---|---|
| 4441 | `UIElement.Elements` | child collection owned by the proposed tree store |
| 4442-4449 | `Top`, `Left`, `Width`, `Height`, `MaxWidth`, `MaxHeight`, `MinWidth`, `MinHeight` | mutable layout constraints |
| 4455-4464 | `PaddingTop`, `PaddingLeft`, `PaddingRight`, `PaddingBottom`, `MarginTop`, `MarginLeft`, `MarginRight`, `MarginBottom`, `HAlign`, `VAlign` | spacing and alignment inputs |
| 4465-4467 | `_innerDimensions`, `_dimensions`, `_outerDimensions` | derived `UiCalculatedStyle` snapshots |
| 4539 | `Parent` | parent `UiElementId` link, not an object reference |
| 4541 | `Children` | read-only child projection over the tree store |

The source report records the declarations as `StyleDimension`, `float`, `CalculatedStyle`, `UIElement`, and `IEnumerable<UIElement>` respectively. The proposed mapping replaces object references with an internal `UiElementId` and keeps the public child view query-only.

### 6.2 Proposed shape and contracts

- Proposed `UiElementLayoutComponent` stores one element's constraints, spacing, alignment, parent link, and the last calculated inner/content/outer geometry. It is presentation state scoped to a UI tree node, not persistent gameplay state.
- Proposed `UiElementTreeStore` owns the node index and child adjacency. It accepts `AttachUiElementCommand`, `DetachUiElementCommand`, and `ReparentUiElementCommand`; no arbitrary caller mutates `Children` or `Parent` directly.
- Proposed `UiElementLayoutQuery` returns immutable node and child snapshots for hit testing, drawing, accessibility, and navigation. It has no cache mutation and does not allocate an external object graph as a side effect.
- Proposed `UiElementLayoutSystem` consumes tree commands and viewport/style invalidations, calculates outer geometry, applies min/max constraints, derives inner geometry after padding, and publishes the three calculated snapshots. It is the only writer of the calculated fields.

### 6.3 Readers, writers, lifecycle, and invariants

| concern | proposed contract |
|---|---|
| writer | `UiElementTreeStore` is the single writer for parent/child links; `UiElementLayoutSystem` is the single writer for calculated geometry; UI construction commands are the only input writer for constraints and spacing |
| readers | pointer hit-test, clipping, draw projection, snap-point projection, list/panel layout, and accessibility queries read immutable snapshots |
| initialize | create a node ID, insert it into the tree, copy explicit constraints, attach to a parent, then calculate from the parent's geometry and the current viewport |
| update | a constraint, parent, viewport, UI scale, padding, margin, or child-order change marks the node and descendants dirty; recalculation occurs in explicit tree order, never by file order |
| cleanup | detach children through a command, clear parent links and calculated snapshots, invalidate related snap points, then release the node ID from the presentation store |
| failure | invalid parent, cycle, duplicate child, or stale node ID rejects the command without partially publishing a tree; the adapter owns diagnostics, not the component |

The parent/child graph must remain acyclic. A calculated geometry snapshot is never accepted as a future layout constraint without an explicit command. `HAlign` and `VAlign` affect element geometry only; they cannot alter a player, world, item, or currency balance component.

### 6.4 Dependency direction and proposed scheduling

`IUiViewportAdapter` and style/resource adapters provide read-only inputs to `UiElementTreeCommandAdapter`. The order is: tree command intake -> cycle/ownership validation -> layout invalidation propagation -> `UiElementLayoutSystem` -> calculated-style publication -> pointer hit-test and draw/navigation projections. The proposed layout system does not call network, persistence, clock, random, logging, or simulation mutation ports.

### 6.5 Evidence and decision

Direct Version4 evidence is `Terraria.UI/UIElement.cs:23-75,89-95,120-254` for the fields, append/remove, style copying, and parent/children projection. The exact invalidation and recursion behavior is partial because the Version4 checkout contains reduced method bodies; the complete-reference file was used only to identify a candidate lifecycle and must not be treated as implementation evidence. The proposed node ID and tree-store boundary therefore remain `evidence-gap: partial` until a client runtime owner supplies a focused behavior fixture.

`blocking-decision: integration-review` must approve whether the tree store is a standalone client presentation store or an ECS-backed UI world, and must define the viewport/scale source before production integration. The implementation files are saved under `src2`, and the cumulative focused build/runtime verifier is recorded in the execution document.

## 7. Checkpoint 3：UiElementInteractionComponent（proposed）

### 7.1 角色与成员覆盖

该单元保存 UI 节点的交互策略、生命周期标志、hover 派生值和导航引用；图形 API 状态通过 adapter/projection 隔离。它覆盖 12 条库存成员：

| source sequence | source member | proposed role |
|---:|---|---|
| 4450 | `UIElement._isInitialized` | lifecycle state in `UiElementInteractionComponent` |
| 4451-4453 | `IgnoresMouseInteraction`, `PassThroughMouseInteraction`, `OverflowHidden` | hit-test and clipping policy in the component |
| 4454 | `OverrideSamplerState` | graphics sampler policy adapter input; not shared component data |
| 4468 | `OverflowHiddenRasterizerState` | graphics rasterizer policy adapter cache; not an ECS component |
| 4469 | `UseImmediateMode` | presentation rendering mode in the component |
| 4470 | `UIElement._snapPoint` | transient `UiSnapPoint` reference in the component |
| 4471 | `UIElement._idCounter` | process-local UI identity allocator owned by a lifecycle system |
| 4475 | `UIState.NoGamepadSupport` | UI-state navigation capability in the component/state projection |
| 4540 | `UIElement.UniqueId` | immutable transient `UiElementId` projection |
| 4542 | `UIElement.IsMouseHovering` | hover result written by pointer dispatch, read by draw/navigation |

### 7.2 Proposed shape, writers, and lifecycle

- Proposed `UiElementInteractionComponent` stores interaction flags, immediate-mode preference, transient snap-point reference, UI hover result, initialization phase, and a process-local element identity. It does not store an `Entity`, world object, network handle, persistence key, or external graphics object.
- Proposed `UiElementLifecycleSystem` owns initialize/activate/deactivate/uninitialize transitions. It allocates a `UiElementId`, recursively enters or leaves the UI tree, clears hover and snap references on deactivation, and publishes lifecycle commands.
- Proposed `UiElementInteractionSystem` consumes pointer hit-test results and explicit hover/click commands. It is the only writer of `IsMouseHovering`; no renderer or caller sets hover directly.
- Proposed `UiElementInteractionQuery` exposes read-only `CanReceivePointer`, `IsHovered`, `HasSnapPoint`, and `SupportsGamepad` decisions.
- Proposed `UiElementGraphicsPolicyAdapter` translates `OverrideSamplerState`, `OverflowHidden`, and immediate-mode flags into the platform renderer. `OverflowHiddenRasterizerState` is an adapter-owned cache; it is not serialized or registered as an ECS component.

The lifecycle state is a finite transition set: created -> initialized -> active -> deactivated -> disposed. Repeated initialize/deactivate calls are idempotent at the command boundary, and a disposed node cannot receive pointer or navigation commands. Initialization and deactivation must recurse through the accepted tree snapshot so that child cleanup cannot leave stale parent links or snap points.

### 7.3 Identity and ownership boundaries

`UniqueId` and the `_idCounter` allocator are process-local presentation identities. `SnapPoint.Id` is a navigation/focus identity and is not the same namespace. Neither may be used as an ECS Entity ID, a persisted world record ID, a network object ID, or an external platform control ID. If an element displays or targets a world entity, the proposed adapter carries a separately typed `EntityReference` supplied by the owning world/presentation integration; this unit only stores the transient anchor reference.

`NoGamepadSupport` is a state-level capability flag. It must not alter player input authority or disable simulation input by side effect. `IsMouseHovering` is derived from the most recent pointer dispatch and is cleared on mouse-out, state change, deactivation, and tree removal.

### 7.4 Scheduling, effects, and evidence

The proposed order is lifecycle command intake -> tree/lifecycle validation -> pointer hit-test result -> interaction system -> hover/snap projection -> graphics policy adapter. The interaction system may emit explicit UI commands, but it cannot call persistence, network, clock, random, logging, world, player, or item mutation ports. Failure belongs to the command validator; stale nodes are ignored or rejected deterministically.

Direct evidence is `Terraria.UI/UIElement.cs:41-49,77-95,268-319` and `Terraria.UI/UIState.cs:5`. `OverrideSamplerState` and the static rasterizer are platform/rendering concerns confirmed by their types, but their adapter lifecycle is not closed in Version4; mark `evidence-gap: partial`. The exact `UniqueId` allocation and snap-point cleanup ordering also require a client fixture before migration.

`blocking-decision: integration-review` must choose whether gamepad support is a UI capability query or part of a broader input service, and must approve the renderer adapter boundary. The interaction component, lifecycle/interaction systems, query, and graphics policy adapter are saved under `src2`; cumulative focused build/runtime verification is complete for the isolated project, while renderer production integration remains unresolved.

## 8. Checkpoint 4：UiPointerInputComponent（proposed）

### 8.1 角色与成员覆盖

该单元把 UI 指针输入的不可变 payload、左右指针的时序缓存、当前 UI state 和有限历史放在同一 presentation interaction boundary。它覆盖 23 条库存成员；事件对象不会成为模拟输入或网络消息本身：

| source sequence | source member | proposed role |
|---:|---|---|
| 4472 | `UIEvent.Target` | transient `UiElementId` in a proposed event payload |
| 4473 | `UIMouseEvent.MousePosition` | viewport-space pointer position value |
| 4474 | `UIScrollWheelEvent.ScrollWheelValue` | scroll delta payload |
| 4476-4483 | `InputPointerCache.LastTimeDown`, `WasDown`, `LastDown`, `LastClicked`, `MouseDownEvent`, `MouseUpEvent`, `ClickEvent`, `DoubleClickEvent` | per-button pointer cache and dispatch mapping |
| 4484-4487 | `DOUBLE_CLICK_TIME`, `STATE_CHANGE_CLICK_DISABLE_TIME`, `MAX_HISTORY_SIZE`, `HISTORY_PRUNE_SIZE` | explicit pointer policy constants/configuration |
| 4488 | `ActiveInstance` | presentation host registry/active-instance adapter, not an entity component |
| 4489 | `_history` | bounded UI-state history store |
| 4490-4491 | `LeftMouse`, `RightMouse` | two button-specific pointer cache records |
| 4492 | `UserInterface.MousePosition` | current viewport-space pointer position |
| 4493 | `_lastElementHover` | transient last-hover `UiElementId` |
| 4494 | `IsVisible` | current UI host visibility projection |
| 4495 | `_currentState` | current proposed `UiStateId`/state snapshot link |

The source report's `UIElement` and `UIState` references become typed presentation IDs or immutable snapshots. They must not be copied into a server simulation component. Event delegates become explicit command/event kinds so the receiver and effect ordering are inspectable.

### 8.2 Proposed shape and contracts

- Proposed `UiPointerInputComponent` stores current pointer coordinates, left/right button caches, last-hover ID, visibility, current UI state ID, and bounded history metadata. It is scoped to one client UI host.
- Proposed `UiPointerEvent`, `UiMouseEvent`, and `UiScrollWheelEvent` are immutable value records carrying typed target ID, pointer position, and wheel delta. They are not durable snapshots and do not own callbacks.
- Proposed `UiPointerDispatchSystem` consumes platform pointer samples and hit-test snapshots, updates button cache state, recognizes click/double-click using an injected monotonic clock, and emits explicit `UiPointerCommand` values. It is the only writer of pointer cache and hover history.
- Proposed `UiStateTransitionSystem` accepts `SetUiStateCommand`, sends mouse-out/deactivate cleanup to the old state, activates/recalculates the new state, updates visibility, and bounds history. It is the only writer of current state and state history.
- Proposed `UiPointerInputAdapter` translates platform input into presentation samples. The adapter owns platform coordinates and device-specific button events; the component does not call `PlayerInput`, network, persistence, or simulation ports.
- Proposed `UiPointerQuery` exposes read-only current state, visibility, hover target, and button phase to UI projections. It never appends to history or clears caches.

### 8.3 Timing, ownership, identity, and lifecycle

The 500 ms double-click threshold and 200 ms state-change click-disable window are policy inputs, not calls to a global clock. A proposed `IUiMonotonicClock` adapter supplies the timestamp to the dispatch system, which records only the values needed for deterministic click classification. History is bounded at 32 entries and pruned by the proposed history owner; the exact legacy prune behavior is `evidence-gap` because `AddToHistory` is reduced in the Version4 checkout.

`UiElementId` targets, `UiStateId` links, and pointer cache references are process-local presentation identities. They are distinct from ECS Entity IDs, persistence IDs, network IDs, and external device/control IDs. A click that affects a world, player, item, or commerce domain becomes a typed command at the integration boundary; this component only records and dispatches the UI interaction.

Lifecycle is: host created -> state absent/hidden -> state active/visible -> state transition with pointer suppression -> new state active -> host disposed. `ClearPointers` clears both button caches and hover references. State changes must emit mouse-out before old-state deactivation, then activate/recalculate the new state before exposing it as visible; the final order remains proposed until a behavior fixture closes the reduced method gap.

### 8.4 Dependency direction, effects, and evidence

The proposed schedule is platform sample adapter -> layout snapshot query -> pointer hit-test -> `UiPointerDispatchSystem` -> explicit UI command queue -> state transition/lifecycle systems -> input and draw projections. The pointer systems may publish UI events but cannot write world/player/item state, serialize a snapshot, send a network packet, or log implicitly. Adapter failure rejects the sample for that frame and does not mutate the cache.

Direct evidence is `Terraria.UI/UIEvent.cs:3-13`, `UIMouseEvent.cs:3-15`, `UIScrollWheelEvent.cs:3-15`, and `UserInterface.cs:12-154`. The reduced `ResetState` and `AddToHistory` bodies make state reset and history pruning partial; the complete-reference file only supplies a candidate comparison target. `evidence-gap: partial` therefore remains on exact callback propagation, event ordering, and history semantics.

`blocking-decision: integration-review` must approve the monotonic clock port, platform input owner, state-history capacity semantics, and the rule that UI commands crossing into simulation are owned by their destination subsystem. Pointer source and verifier files are saved under `src2`; cumulative focused build/runtime verification is complete for the isolated project, while platform/input production ownership remains unresolved.

## 9. Checkpoint 5：UiCollectionProgressComponent（proposed）

### 9.1 角色与成员覆盖

该单元覆盖内容容器与进度呈现的 23 条库存成员。列表集合、滚动窗口、面板视觉定义和世界生成进度共同服务于客户端展示，但不拥有世界生成、资源加载或领域集合的权威状态：

| source sequence | source member | proposed role |
|---:|---|---|
| 2264-2265 | `UIGenProgressBar._targetOverallProgress`, `_targetCurrentProgress` | progress presentation targets |
| 2267-2269 | `UIList._items`, `_scrollbar`, `_innerList` | ordered child IDs, scrollbar link, and inner viewport node |
| 2270-2271 | `ListPadding`, `ManualSortMethod` | list layout policy and explicit sort-command adapter |
| 2272-2273 | `UIPanel._cornerSize`, `_barSize` | panel geometry/theme definition |
| 2274-2275 | `_borderTexture`, `_backgroundTexture` | renderer asset handles owned by a graphics adapter |
| 2276-2277 | `BorderColor`, `BackgroundColor` | panel visual palette |
| 2278-2280 | `UIScrollbar._viewPosition`, `_viewSize`, `_maxViewSize` | scroll viewport state and bounds |
| 2281 | `UIScrollbar.AutoHide` | scrollbar visibility policy |
| 2282-2284 | `_texture`, `_innerTexture`, `_theme` | renderer asset/theme handles and theme selection |
| 3837 | `UIList.Count` | read-only item-count query |
| 3838-3839 | `UIScrollbar.ViewPosition`, `CanScroll` | clamped position mutation/query and derived scrollability |

The source report records 20 fields and 3 properties. `Count` and `CanScroll` remain derived queries, while the proposed component stores only the minimum collection/scroll/progress state needed by the presentation systems.

### 9.2 Proposed shape and contracts

- Proposed `UiCollectionProgressComponent` stores ordered child presentation IDs, list padding, optional scrollbar link, progress targets, scroll position/window/max bounds, auto-hide policy, and panel visual tokens. It is scoped to a UI node or screen projection.
- Proposed `UiCollectionLayoutSystem` calculates list content extent, inner viewport geometry, and scrollbar visibility from immutable child/layout snapshots. It alone publishes derived item count, clamped scroll state, and `CanScroll`.
- Proposed `UiProgressPresentationSystem` consumes progress samples from a world-generation or loading adapter and writes only the target progress projection. It does not start, cancel, or mutate world generation.
- Proposed `UiCollectionCommand` covers add/remove/clear/reorder/set-scroll/set-view operations. The command validator rejects duplicate IDs, stale child IDs, invalid bounds, or a sort callback that escapes the adapter boundary.
- Proposed `UiCollectionGraphicsAdapter` owns `Asset<Texture2D>`, texture requests, panel nine-slice details, scrollbar theme assets, and renderer calls. Third-party graphics objects do not enter the component assembly.
- Proposed `UiCollectionQuery` returns count, visible child IDs, view position, view size, max view size, and `CanScroll` without mutation.

### 9.3 Writers, readers, lifecycle, and invariants

The collection command store is the only writer of child membership and list order. `UiCollectionLayoutSystem` is the only writer of derived content extent and scrollability. `UiProgressPresentationSystem` is the only writer of progress targets. The graphics adapter owns asset handles; UI construction may submit theme tokens but cannot hold or dispose external assets directly.

Initialize a container by creating the inner viewport node, attaching it to the UI tree, applying padding/theme defaults, and optionally binding a scrollbar. Add/remove/clear commands invalidate layout and keep the scrollbar link consistent. Removing a container detaches child presentation nodes, clears progress and scroll state, releases only transient UI IDs, and does not delete the source collection or cancel domain work.

The scroll position is clamped to `[0, maxViewSize - viewSize]`; `SetView` clamps view size to the available maximum before publishing. Empty and equal-size views produce `CanScroll = false`. Progress values are presentation samples and must define a policy for out-of-range inputs at the adapter boundary; they do not become authoritative generation progress in this component. `ManualSortMethod` is not an arbitrary component delegate: sorting is an explicit command handled by a proposed adapter with a deterministic comparer.

### 9.4 Identity, effects, and evidence

Child IDs and scrollbar links are transient `UiElementId` values. They are distinct from collection domain IDs, world object IDs, persistence IDs, network IDs, and asset/resource IDs. A list may display domain records via immutable projections, but it may not write those records. World-generation progress enters through a one-way query/adapter and remains distinct from the UI target interpolation state.

The proposed order is collection command intake -> membership validation -> list/layout recalculation -> scroll clamp/visibility calculation -> progress projection -> draw/resource adapter. No network, persistence, clock, random, logging, or simulation effect is implicit in the component systems.

Direct evidence is `Terraria.GameContent.UI.Elements/UIGenProgressBar.cs:11-30`, `UIList.cs:24-70`, `UIPanel.cs:10-49`, and `UIScrollbar.cs:17-73`. The Version4 draw and list calculation bodies are reduced, so exact sorting, culling, interpolation, and texture lifetime remain `evidence-gap: partial`. `blocking-decision: integration-review` must approve the asset/renderer owner, the progress source adapter, and whether UI collection nodes are stored in a dedicated client store or an ECS-backed presentation world. Collection/progress source and verifier files are saved under `src2`; cumulative focused build/runtime verification is complete for the isolated project, while those integration decisions remain open.

## 10. Checkpoint 6：UiTextPanelComponent（proposed）

### 10.1 角色与成员覆盖

该单元覆盖 `UIHeader` 和 `UIText` 的 17 条成员，负责客户端文本源、排版策略、颜色和标题/正文投影。文本内容可以来自本地化或其他只读查询，但文本组件不拥有本地化目录、字体资源或领域记录：

| source sequence | source member | proposed role |
|---:|---|---|
| 2266, 3836 | `UIHeader._text`, `UIHeader.Text` | header text source and command/query boundary |
| 2285 | `UIText._text` | typed text source, replacing unbounded `object` at the adapter boundary |
| 2286 | `_textScale` | text scale input |
| 2287 | `_textSize` | derived measured-size snapshot |
| 2288 | `_isLarge` | font-style/size policy input |
| 2289-2290 | `_color`, `_shadowColor` | text and shadow presentation colors |
| 2291-2292 | `_isWrapped`, `DynamicallyScaleDownToWidth` | wrapping and fit policy |
| 3840 | `UIText.Text` | resolved string query |
| 3841-3843 | `TextOriginX`, `TextOriginY`, `WrappedTextBottomPadding` | origin and wrapping layout inputs |
| 3844-3846 | `IsWrapped`, `TextColor`, `ShadowColor` | read/write presentation policy queries |

### 10.2 Proposed shape and contracts

- Proposed `UiTextPanelComponent` stores a typed text source (`Literal`, `LocalizedKey`, or an integration-owned formatted snapshot), scale/style flags, wrap/fit policy, origin/padding, and colors. It never stores a font object, localization service, renderer object, or arbitrary mutable `object` value.
- Proposed `UiTextContentCommand` is the only mutation entry for header/body content, scale, style, wrap, fit, origin, padding, and colors. It validates null/unsupported source kinds and records a content revision for deterministic measurement invalidation.
- Proposed `UiTextMeasurementSystem` resolves the source through `IUiLocalizationAdapter`, asks `IUiTextMeasurementAdapter` for size, and writes `_textSize`-equivalent derived snapshots. It is the only measured-size writer.
- Proposed `UiTextQuery` returns resolved text, style, measured size, and fit/wrap decisions without resolving or mutating anything implicitly.
- Proposed `UiTextRenderProjection` sends immutable text draw commands to the graphics adapter. It owns no text state and cannot change content or dimensions.

### 10.3 Writers, readers, lifecycle, and invariants

Content commands write the source and style inputs. The measurement system writes only the measured-size snapshot. Localization and font adapters own external services and assets. Header text changes invalidate width/height percentage assumptions and enqueue layout recalculation; text measurement must complete before the next draw projection that claims a new size.

Text source identity is not a domain/entity identity. A `LocalizedKey` is an external localization key, a formatted snapshot is transient presentation data, and any domain record ID remains owned by its source subsystem. Text, measured size, colors, and wrapping flags are client presentation values and are not persisted or placed in authoritative snapshots by this component.

The invariant is that `Text` is a deterministic resolved view of the typed source at a known localization/content revision. Dynamic scale-down may choose a smaller render scale to fit the calculated width; it may not mutate the source text. Wrapping and origin affect layout/draw projections only. Unsupported formatting or missing localization returns an explicit adapter result and does not call a global fallback with hidden state.

### 10.4 Scheduling, effects, and evidence

The proposed order is content command intake -> source validation/revision -> localization resolution -> text measurement -> layout invalidation/recalculation -> draw projection. The measurement and render adapters are the only external-effect edges; no clock, random, network, persistence, logging, or simulation write is implicit.

Direct evidence is `Terraria.GameContent.UI.Elements/UIHeader.cs:10-42` and `UIText.cs:15-111`. Version4 has reduced `Recalculate`, `InternalSetText`, and drawing bodies, so exact font metrics, localization timing, and dynamic fit semantics are `evidence-gap: partial`. `blocking-decision: integration-review` must choose the typed localization/text-measurement ports and determine whether text revisions are frame-local or cached by content revision. The typed text component, command, measurement system, query, projection, and adapter are implemented under `src2`; production integration remains unresolved.

## 11. Checkpoint 7：UiOptionSelectionComponent（proposed）

### 11.1 角色与成员覆盖

该单元覆盖 `GroupOptionButton<T>` 的 25 条库存成员：选项 token、当前选项、选择派生值、标题/说明文本、颜色、图标布局以及渲染资源策略。按钮的选择状态是客户端交互投影，不是 Creative、玩家、世界或其他领域状态的第二份 owner：

| source sequence | source member | proposed role |
|---:|---|---|
| 2245 | `_currentOption` | current selection token written by the selection command owner |
| 2246-2249 | `_BasePanelTexture`, `_selectedBorderTexture`, `_hoveredBorderTexture`, `_iconTexture` | renderer asset requests owned by graphics adapter |
| 2250 | `_myOption` | immutable option token for this button |
| 2251-2252 | `_color`, `_borderColor` | normal/border visual colors |
| 2253-2255 | `FadeFromBlack`, `InnerHighlightRim`, `ShowHighlightWhenSelected` | selection rendering policy |
| 2256-2257 | `_overrideUnpickedColor`, `_overridePickedColor` | selected/unselected color overrides |
| 2258 | `Description` | localized description source |
| 2259 | `_title` | child text-node link to the text presentation unit |
| 2260-2263 | `_iconScale`, `_iconOffset`, `_iconFrame`, `_iconColor` | icon presentation inputs |
| 3830 | `OptionValue` | immutable button option query |
| 3831 | `IsSelected` | derived equality query |
| 3832-3835 | `Icon`, `IconScale`, `IconOffset`, `IconColor` | read-only icon projection/query and style fields |

### 11.2 Proposed shape and contracts

- Proposed `UiOptionSelectionComponent<T>` stores an immutable button option token, selected token, visual policy, localized description key, title-node link, and icon layout/color tokens. It stores no `Texture2D`, `Asset<Texture2D>`, `LocalizedText` service object, or domain component reference.
- Proposed `UiOptionSelectionSystem` applies `SetUiOptionSelectionCommand` for a button group and writes `_currentOption` only after validating the group and token. `IsSelected` is derived by a pure equality query; it is not independently mutable.
- Proposed `UiOptionSelectionQuery` returns option value, selected state, title/description keys, effective selected/unselected color, and icon projection data. Querying does not change selection or load assets.
- Proposed `UiOptionSelectionCommand` emits a typed selection intent to the owning destination subsystem after UI validation. The button component cannot directly mutate Creative, player, world, item, or configuration authority.
- Proposed `UiOptionGraphicsAdapter` resolves panel/icon textures and applies border/highlight/fade policies. The title is a child `UiElementId`/text projection and is not duplicated as an embedded mutable `UIText` object.

### 11.3 Writers, readers, identity, and lifecycle

The option-selection system is the single writer of the current selected token. Construction assigns the immutable button token and initial selected token; the command validator enforces that both tokens belong to the same typed option group. Pointer interaction produces a selection intent, but only the group owner accepts and applies it. Hover state remains owned by `UiElementInteractionComponent`; `IsSelected` is computed from the selected token and button token.

Option tokens are presentation/group identities. If a token encodes a Creative power, item, player setting, or world choice, it is a typed reference supplied by that subsystem and must not be treated as a persisted UI ID or network object ID. The selection command crossing that boundary is `crossSubsystemOwner: integration-review`; this component only reports the intent and projects the accepted selection.

On initialization, create the button node, attach the title child if present, validate the option group, and resolve render resources lazily through the graphics adapter. On removal, detach the title link, invalidate pending selection intents, and release transient asset references through the adapter. A missing icon is a valid no-icon projection; it must not cause a domain mutation or hidden asset load.

### 11.4 Effects, scheduling, and evidence

The proposed order is option construction -> group/token validation -> pointer selection intent -> selection command acceptance -> derived selected query -> text/icon/panel projections. Asset loads, localization resolution, audio feedback, and any destination mutation are explicit adapter or integration edges. The component has no network, persistence, clock, random, logging, or simulation side effect.

Direct evidence is `Terraria.GameContent.UI.Elements/GroupOptionButton.cs:14-155`. `SetCurrentOption` and `SetIconFrame` are observed writes; draw, mouse, and color helper bodies are reduced, so exact highlight/audio behavior and selection-group ownership are `evidence-gap: partial`. `blocking-decision: integration-review` must identify the owner of typed option tokens and approve whether accepted selection is a local UI state or a command to Creative/player/configuration subsystems. The option component, command, system, query, and graphics adapter are implemented under `src2`; destination integration remains unresolved.

## 12. Checkpoint 8：UiScreenPresentationComponent（proposed）

### 12.1 角色与成员覆盖

该单元覆盖世界加载与世界选择屏幕的 8 条库存成员。它拥有 screen-local 的子节点链接、屏幕布局切换和短期展示缓存，不拥有世界文件、世界生成、收藏持久化或网络状态：

| source sequence | source member | proposed role |
|---:|---|---|
| 2293 | `UIWorldLoad._progressBar` | progress-bar child `UiElementId`/screen projection link |
| 2294 | `UIWorldLoad._progressMessage` | header child `UiElementId`/screen projection link |
| 2295 | `UIWorldSelect.NewlyGeneratedWorld` | read-only `WorldSummarySnapshot` input from world/file integration |
| 2296-2298 | `_worldList`, `_containerPanel`, `_scrollbar` | child node links to collection/panel/scroll presentation units |
| 2299 | `_isScrollbarAttached` | screen-local attachment state derived from scrollbar query |
| 2300 | `favoritesCache` | transient world-list favorite projection cache pending owner review |

### 12.2 Proposed shape and contracts

- Proposed `UiScreenPresentationComponent` stores screen kind (`WorldLoad` or `WorldSelect`), child node IDs, active/deactivated phase, scrollbar attachment result, and immutable screen inputs. It does not hold a `WorldFileData` object or UI control object.
- Proposed `UiScreenLifecycleSystem` constructs and tears down the screen child graph, applies activation/deactivation, and routes layout invalidations. It is the only writer of screen child links and attachment state.
- Proposed `UiWorldLoadProjection` consumes a world-generation/loading progress query and emits progress/header commands to the collection/text presentation units. It cannot start or mutate generation.
- Proposed `UiWorldSelectProjection` consumes world summary snapshots and a separately owned favorites query, builds list item commands, and emits typed world-selection commands. It cannot write world files or favorite persistence directly.
- Proposed `UiScreenQuery` returns active screen kind, child IDs, attachment state, and screen-local snapshots without exposing external objects.

### 12.3 Writers, readers, lifecycle, and invariants

The screen lifecycle system is the single writer of `_progressBar`/`_progressMessage`/list/panel/scrollbar links and `_isScrollbarAttached`. The world/file adapter is the only writer of the immutable `WorldSummarySnapshot` input; the screen projection only reads it. The favorites source is unresolved and remains `crossSubsystemOwner: integration-review`; the proposed UI cache may only mirror it and may not persist changes.

On construction, create the child graph and apply screen-specific styles. On activation, bind current read-only loading/world-list inputs, recalculate layout, and expose the screen. On deactivation, clear child links, pending selection commands, and transient favorites cache, then release UI IDs. If `CanScroll` changes, the screen issues an explicit attach/detach command and recalculates width; no direct child mutation occurs outside the lifecycle owner.

The screen must not retain a mutable `WorldFileData`, execute world generation, load files, write favorites, or send network messages. A generated-world snapshot has an external world/file identity distinct from the UI screen ID, ECS entity ID, persistence ID, network ID, and external file path. The progress bar target remains a presentation projection, distinct from the authoritative generation phase/progress owner.

### 12.4 Scheduling, effects, and evidence

The proposed order is screen command intake -> world/file/loading query adapters -> child graph creation -> collection/text progress commands -> layout recalculation -> scrollbar attachment query -> screen projection/draw. External file, world-generation, persistence, network, and renderer effects belong to explicit adapters or destination subsystems.

Direct evidence is `Terraria.GameContent.UI.States/UIWorldLoad.cs:16-39` and `UIWorldSelect.cs:18-58`. `OnActivate`, `OnDeactivate`, and draw bodies are reduced; favorites ownership and exact screen command flow are `evidence-gap: partial`. `blocking-decision: integration-review` must assign world-summary/favorites owners and confirm whether screen state is a dedicated client store or an ECS-backed UI entity set. The screen component, lifecycle, snapshots, queries, and projections are implemented under `src2`; world/file/favorites integration remains unresolved.

## 13. Checkpoint 9：UiCurrencyVisualsComponent（proposed）

### 13.1 角色与成员覆盖

该单元覆盖自定义货币管理/定义/单币种视觉元数据的 7 条库存成员，但不接管货币余额或购买提交。成员按视觉状态、注册表适配器和 Commerce 定义快照分开：

| source sequence | source member | proposed role |
|---:|---|---|
| 2301 | `CustomCurrencyManager._nextCurrencyIndex` | transient UI currency-registration allocator in an adapter |
| 2302 | `CustomCurrencyManager._currencies` | registry adapter from UI registration key to currency-definition port |
| 2303-2305 | `CurrencyDrawScale`, `CurrencyTextKey`, `CurrencyTextColor` | `UiCurrencyVisualsComponent` draw/text metadata |
| 2306 | `CustomCurrencySystem._valuePerUnit` | read-only currency-denomination definition snapshot; owner `crossSubsystemOwner: integration-review` |
| 2307 | `CustomCurrencySystem._currencyCap` | read-only currency-cap policy snapshot; balance/policy owner `crossSubsystemOwner: integration-review` |

### 13.2 Proposed shape and ownership

- Proposed `UiCurrencyVisualsComponent` stores a typed currency presentation key, draw scale, localization key, text color, and renderer formatting policy. It does not store a balance, account, inventory, item array, purchase result, or external graphics object.
- Proposed `UiCurrencyRegistryAdapter` translates transient registration calls to typed presentation keys and read-only currency-definition queries. Its allocator is process-local and must not be treated as an external-content or persistence identity.
- Proposed `UiCurrencyDefinitionQuery` reads denomination and cap snapshots from the Commerce owner. It is a query only; it does not change `_valuePerUnit`, cap, or balance.
- Proposed `UiCurrencyProjection` consumes a validated balance/price query plus visual metadata and emits text/icon draw commands. It cannot debit, combine, purchase, or persist currency.
- The existing NLTX `Items.Commerce.CurrencyBalanceComponent` remains the candidate balance owner. P16 must not move it, wrap it with a second authoritative balance, or infer its transaction semantics from Version4 UI fields.

### 13.3 Identity, lifecycle, and invariants

The UI registration key, external content/currency key, account ID, transaction ID, persistence ID, network ID, and UI element ID are separate namespaces. A transient registration index may identify a renderer lookup during one process only. A currency definition may expose a typed external content key and cap snapshot, but the UI component may only read it.

Registration occurs during client presentation initialization; duplicate or stale registrations are rejected by the adapter. Visual metadata is created with the currency projection and cleared on client teardown. The projection may display zero, capped, overflowing, or unavailable states supplied by Commerce, but it may not normalize or mutate the authoritative balance. Price formatting and purchase commands leave this component through explicit integration messages.

### 13.4 Scheduling, effects, and evidence

The proposed order is currency-registration adapter -> definition query -> balance/price query -> visual projection -> text/icon render adapter. The registry may report registration failure, but no UI currency system performs file, network, persistence, clock, random, logging, inventory, or balance side effects. Commerce owns accepted purchase commands, transactions, revisions, and caps.

Direct evidence is `Terraria.GameContent.UI/CustomCurrencyManager.cs:8-46`, `CustomCurrencySingleCoin.cs:12-34`, and `CustomCurrencySystem.cs:8-51`; the Version4 calculation/purchase/draw bodies are reduced. NLTX evidence is `src/Items/Commerce/CurrencyBalanceComponent.cs:5-54`, which confirms an existing balance boundary but does not establish UI integration. `evidence-gap: partial` remains for registry lifetime, price formatting, and cap ownership. `blocking-decision: integration-review` must assign the currency-definition/registration owner and define the read-only Commerce query before production integration. The currency visual component, registry/query, projection, and formatting adapter are implemented under `src2`; the Commerce balance remains outside P16.

## 14. Checkpoint 10：WorldInteractionVisualsComponent（proposed）

### 14.1 角色与成员覆盖

该单元覆盖表情气泡、稀有度颜色、线路工具 radial 状态和世界 UI 锚点的 25 条库存成员。它是客户端 world-interaction projection 边界；实体、线路、NPC/player 反应、网络和物品领域 owner 都保留在 integration-review：

| source sequence | source member | proposed role |
|---:|---|---|
| 2308-2309 | `EmoteBubble.byID`, `toClean` | transient bubble projection index and cleanup queue |
| 2310-2311 | `NextID`, `ID` | transient local/network display key; transport owner unresolved |
| 2312 | `anchor` | typed `UiWorldAnchor` value with external entity/tile reference |
| 2313-2316 | `lifeTime`, `lifeTimeStart`, `emote`, `metadata` | bubble lifetime/content projection state |
| 2317-2319 | `frameSpeed`, `frameCounter`, `frame` | animation policy and transient frame state |
| 2320-2321 | `EMOTE_SHEET_HORIZONTAL_FRAMES`, `EMOTE_SHEET_EMOTES_PER_ROW` | immutable sprite-sheet presentation constants |
| 2322 | `_temporaryBubble` | transient scratch/projection record, never shared authority |
| 2323 | `ItemRarity._rarities` | read-only item-rarity-to-color presentation definition |
| 2324 | `WiresUI.Settings.ToolMode` | client wire-tool mode projection; wiring command owner unresolved |
| 2325-2327 | `WiresRadial.position`, `active`, `OnWiresMenu` | transient radial UI state |
| 2328 | `WiresUI.radial` | radial state store/link |
| 2329-2332 | `WorldUIAnchor.type`, `entity`, `pos`, `size` | typed world anchor value object; raw entity reference is prohibited |

### 14.2 Proposed shape and contracts

- Proposed `WorldInteractionVisualsComponent` stores transient bubble content/lifetime/frame state, rarity color definitions, wire-tool/radial presentation state, and typed world-anchor projections. It does not store mutable `NPC`, `Player`, `Projectile`, `Entity`, tile map, wiring authority, or item state.
- Proposed `UiWorldAnchor` is a value object with `None`, `EntityReference`, `TileRect`, or `WorldPosition` cases. `EntityReference` is a typed external reference supplied by the owning entity/network integration; it is not a direct object pointer and its identity mapping is `crossSubsystemOwner: integration-review`.
- Proposed `UiEmoteProjectionSystem` consumes accepted emote snapshots, advances lifetime/frame counters from an explicit frame/tick input, and emits draw projections. It does not choose NPC reactions, mutate NPC/player state, or send packets.
- Proposed `UiEmoteTransportAdapter` translates an accepted network payload into a validated bubble snapshot and translates outbound emote commands only when the network owner authorizes them. `EmoteBubble.ID`/`NextID` cannot be assumed to be a globally unique network ID.
- Proposed `UiRarityQuery` reads a static rarity-color table and returns a fallback color without mutating item state. Proposed `UiRarityProjection` is read-only.
- Proposed `UiWiresInteractionSystem` owns radial open/close/tool-mode presentation transitions and emits explicit wiring commands. The wiring simulation/authority owner accepts or rejects those commands; P16 does not perform tile mutation.
- Proposed `UiWorldAnchorQuery` resolves a typed anchor through an entity/tile/position adapter to screen coordinates. It does not retain an external entity object or write the world.

### 14.3 Writers, readers, lifecycle, and effects

The emote projection system is the single writer of client lifetime/frame state; the transport adapter is the only ingress for network bubble snapshots; the rarity definition initializer is the only writer of the read-only color table; the wires interaction system owns radial/tool-mode presentation state; and the anchor adapter owns resolution from typed references to screen positions. Draw, tooltip, and accessibility projections read immutable snapshots.

Bubble lifecycle is create/accept -> visible/animate -> mark-for-cleanup -> remove. Cleanup must remove the bubble from the projection index and release its transient anchor reference. A state/world teardown clears all bubbles and radial state without deleting NPC/player/projectile/tile entities. `frameSpeed` and sprite-sheet dimensions are presentation constants. Random NPC emote selection, player/NPC reactions, and network serialization are explicit ports or destination systems, not hidden component behavior.

Rarity colors are initialized from a definition source and queried by item rarity value; unknown values use an explicit fallback. Wire radial state is client-transient: opening/closing or changing `ToolMode` may emit a wiring intent but cannot directly mutate tiles or inventory. Tile/entity/world anchors are screen-placement data, not persistence records.

### 14.4 Identity, network, persistence, scheduling, and evidence

The bubble display key, network message identity, ECS Entity ID, NPC/player/projectile slot, tile coordinate, persistence ID, external resource ID, and UI element ID are distinct. The old `whoAmI` anchor metadata is a transport-specific slot and must be validated by the owning network/entity adapter before it becomes an `EntityReference`. Bubble IDs and cleanup lists are not persisted. Rarity definitions may be loaded from content definitions, but the UI projection does not own item persistence. Wire commands may be networked or authoritative only by the wiring owner.

The proposed order is validated transport/world-anchor input -> bubble/radial/rarity state update -> lifetime/frame advancement -> anchor screen-coordinate query -> draw/tooltip projection -> explicit emote/wiring destination commands. No UI system calls network, persistence, randomness, logging, or world/entity mutation directly.

Direct evidence is `Terraria.GameContent.UI/EmoteBubble.cs:10-242`, `ItemRarity.cs:9-36`, `WiresUI.cs:8-36`, and `WorldUIAnchor.cs:6-47`; `Main.cs`, `MessageBuffer.cs`, `NPC.cs`, and `Player.cs` are direct callers/owners at the integration boundary. Emote selection, draw, cleanup, and wiring behavior are reduced or cross-subsystem, so `evidence-gap: partial` is mandatory. `blocking-decision: integration-review` must assign emote transport/entity-anchor ownership, wiring command ownership, rarity definition loading, and the client projection lifecycle before production integration. The world visual component, typed anchor/query, emote adapter/system, rarity query, and wires system are implemented under `src2`; owner decisions remain unresolved.

## 15. Integration Handoff

- All 176 source members are mapped across the ten proposed units: 12 layout values, 24 element-layout members, 12 element-interaction members, 23 pointer-input members, 23 collection/progress members, 17 text/header members, 25 option-selection members, 8 screen members, 7 currency members, and 25 world-interaction members.
- Dependencies point from platform/file/network/domain read adapters into presentation commands and snapshots, then into layout, pointer dispatch, interaction/selection, and client draw projections. The implemented P16 presentation components are not authorities for player, world, entity, item, inventory, wiring, generation, currency balance, network, or persistence state.
- Proposed system sequence is: input/window and external snapshot adapters -> UI tree/state commands -> layout recalculation -> hit-test/pointer dispatch -> explicit interaction/selection commands -> content/screen/currency/world visual projections -> renderer/audio/network/destination adapters. This is a candidate contract, not an accepted runtime order.
- `crossSubsystemOwner: integration-review` applies to the client UI project boundary, UI tree store versus ECS presentation world, platform renderer/input/clock adapters, world/file/favorites queries, Creative/player option destinations, Commerce currency definitions and commands, emote transport/entity anchors, rarity definitions, and wiring authority.
- Production integration remains blocked until the missing first-round report and public split constraint decision is resolved, a production client UI project/tree owner exists or is explicitly assigned, every cross-subsystem owner accepts typed IDs/commands, and focused behavior fixtures close the reduced Version4 method gaps.

The design remains a proposed integration artifact. The ten P16 source units are implemented under `src2` and focused verifier evidence is recorded in the execution document; this does not claim behavior equivalence, network/persistence closure, production registration, or cross-subsystem owner resolution.

## 16. Exact Member Coverage Manifest

The following manifest lists every source sequence individually. The checkpoint tables above provide the source type/member names and role details; this manifest makes the one-to-one inventory coverage mechanically auditable. No sequence is intentionally deferred.

| proposed unit | exact source sequences | target role |
|---|---|---|
| `UiLayoutGeometryValues` | 4272, 4273, 4274, 4275, 4434, 4435, 4436, 4437, 4438, 4439, 4440, 4538 | value objects and pure geometry query |
| `UiElementLayoutComponent` | 4441, 4442, 4443, 4444, 4445, 4446, 4447, 4448, 4449, 4455, 4456, 4457, 4458, 4459, 4460, 4461, 4462, 4463, 4464, 4465, 4466, 4467, 4539, 4541 | tree/constraint component, tree store, layout system, and read-only query |
| `UiElementInteractionComponent` | 4450, 4451, 4452, 4453, 4454, 4468, 4469, 4470, 4471, 4475, 4540, 4542 | interaction/lifecycle component plus graphics policy adapter |
| `UiPointerInputComponent` | 4472, 4473, 4474, 4476, 4477, 4478, 4479, 4480, 4481, 4482, 4483, 4484, 4485, 4486, 4487, 4488, 4489, 4490, 4491, 4492, 4493, 4494, 4495 | immutable event payloads, pointer cache, state transition system, and adapters |
| `UiCollectionProgressComponent` | 2264, 2265, 2267, 2268, 2269, 2270, 2271, 2272, 2273, 2274, 2275, 2276, 2277, 2278, 2279, 2280, 2281, 2282, 2283, 2284, 3837, 3838, 3839 | collection/progress component, layout/progress systems, and graphics adapter |
| `UiTextPanelComponent` | 2266, 2285, 2286, 2287, 2288, 2289, 2290, 2291, 2292, 3836, 3840, 3841, 3842, 3843, 3844, 3845, 3846 | typed text component, measurement system, and text projection |
| `UiOptionSelectionComponent` | 2245, 2246, 2247, 2248, 2249, 2250, 2251, 2252, 2253, 2254, 2255, 2256, 2257, 2258, 2259, 2260, 2261, 2262, 2263, 3830, 3831, 3832, 3833, 3834, 3835 | option component, selection system/query, and graphics adapter |
| `UiScreenPresentationComponent` | 2293, 2294, 2295, 2296, 2297, 2298, 2299, 2300 | screen lifecycle and world-load/world-select projections |
| `UiCurrencyVisualsComponent` | 2301, 2302, 2303, 2304, 2305, 2306, 2307 | currency visual component, registry adapter, and read-only Commerce query |
| `WorldInteractionVisualsComponent` | 2308, 2309, 2310, 2311, 2312, 2313, 2314, 2315, 2316, 2317, 2318, 2319, 2320, 2321, 2322, 2323, 2324, 2325, 2326, 2327, 2328, 2329, 2330, 2331, 2332 | bubble/rarity/wiring/anchor visual component and integration adapters |

## 5. 初步顺序与风险

Proposed 调度顺序：平台输入/窗口适配器 -> UI state/tree commands -> `UiLayoutRecalculationSystem` -> pointer hit-test/dispatch -> explicit interaction commands -> projection/draw。文件或目录顺序不表达调度顺序。

第一轮报告缺失、NLTX 没有生产客户端 UI 项目、`UIElement`/`UserInterface` 在 Version4 中存在空实现/删减方法、以及 `WorldUIAnchor`/`EmoteBubble.ID` 的网络身份语义未闭合，均是后续 integration gate。当前十个已落地单元由 `src2` focused verifier 运行通过；这不表示行为等价、网络闭合、持久化闭合或跨分区 owner 已裁决。

using System.Reflection;
using Terraria.ClientPresentation.Ui.Collections;
using Terraria.ClientPresentation.Ui.Commands;
using Terraria.ClientPresentation.Ui.Currency;
using Terraria.ClientPresentation.Ui.Input;
using Terraria.ClientPresentation.Ui.Layout;
using Terraria.ClientPresentation.Ui.Rendering;
using Terraria.ClientPresentation.Ui.Screens;
using Terraria.ClientPresentation.Ui.Systems;
using Terraria.ClientPresentation.Ui.Text;
using Terraria.ClientPresentation.Ui.Tree;
using Terraria.ClientPresentation.Ui.Options;
using Terraria.ClientPresentation.Ui.World;

string assemblyPath = Path.Combine(
  AppContext.BaseDirectory,
  "Terraria.ClientPresentation.Ui.dll");
Assembly uiAssembly = Assembly.LoadFrom(assemblyPath);
Type styleDimensionType = RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Layout.UiStyleDimension");
object styleDimension = Activator.CreateInstance(
  styleDimensionType,
  10f,
  0.25f) ?? throw new InvalidOperationException(
    "UiStyleDimension could not be constructed.");
object? value = styleDimensionType
  .GetMethod("GetValue")?
  .Invoke(styleDimension, [200f]);
Require(
  value is float calculatedValue && Math.Abs(calculatedValue - 60f) < 0.001f,
  "UiStyleDimension did not calculate pixels plus percentage.");

object fill = styleDimensionType.GetProperty("Fill")?.GetValue(null)
  ?? throw new InvalidOperationException("UiStyleDimension.Fill is missing.");
object empty = styleDimensionType.GetProperty("Empty")?.GetValue(null)
  ?? throw new InvalidOperationException("UiStyleDimension.Empty is missing.");
Require(
  Convert.ToSingle(styleDimensionType.GetMethod("GetValue")?.Invoke(fill, [200f])) == 200f,
  "UiStyleDimension.Fill did not fill the container.");
Require(
  Convert.ToSingle(styleDimensionType.GetMethod("GetValue")?.Invoke(empty, [200f])) == 0f,
  "UiStyleDimension.Empty was not empty.");

Type calculatedStyleType = RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Layout.UiCalculatedStyle");
object calculatedStyle = Activator.CreateInstance(
  calculatedStyleType,
  10f,
  20f,
  30f,
  40f) ?? throw new InvalidOperationException(
    "UiCalculatedStyle could not be constructed.");
Require(
  Convert.ToSingle(calculatedStyleType.GetProperty("Right")?.GetValue(calculatedStyle)) == 40f,
  "UiCalculatedStyle.Right is incorrect.");
Require(
  Convert.ToSingle(calculatedStyleType.GetProperty("Bottom")?.GetValue(calculatedStyle)) == 60f,
  "UiCalculatedStyle.Bottom is incorrect.");

Type snapPointType = RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Layout.UiSnapPoint");
object snapPoint = Activator.CreateInstance(
  snapPointType,
  "center",
  Activator.CreateInstance(
    uiAssembly.GetType("Terraria.ClientPresentation.Ui.Layout.UiVector2")!,
    0.5f,
    0.5f),
  Activator.CreateInstance(
    uiAssembly.GetType("Terraria.ClientPresentation.Ui.Layout.UiVector2")!,
    1f,
    2f),
  7) ?? throw new InvalidOperationException("UiSnapPoint could not be constructed.");
object? snapPosition = snapPointType
  .GetMethod("Calculate")?
  .Invoke(snapPoint, [calculatedStyle]);
Require(
  snapPosition is not null
    && Convert.ToSingle(snapPosition.GetType().GetProperty("X")?.GetValue(snapPosition)) == 26f
    && Convert.ToSingle(snapPosition.GetType().GetProperty("Y")?.GetValue(snapPosition)) == 42f,
  "UiSnapPoint did not calculate its anchored position.");

Type layoutQueryType = RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Layout.UiLayoutValueQuery");
object? layout = layoutQueryType
  .GetMethod("Calculate")?
  .Invoke(
    null,
    [
      Activator.CreateInstance(styleDimensionType, 10f, 0f)!,
      Activator.CreateInstance(styleDimensionType, 5f, 0f)!,
      Activator.CreateInstance(styleDimensionType, 50f, 0f)!,
      Activator.CreateInstance(styleDimensionType, 20f, 0f)!,
      calculatedStyle,
      0.5f,
      0.5f,
      0f,
      0f,
      0f,
      0f
    ]);
Require(layout is not null, "UiLayoutValueQuery did not return geometry.");

RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Tree.UiElementLayoutComponent");

UiElementTreeStore tree = new UiElementTreeStore();
UiElementId root = tree.CreateNode();
UiElementId child = tree.CreateNode();
Require(
  tree.Apply(UiElementTreeCommand.Attach(root, null)),
  "The root UI node could not be attached.");
Require(
  tree.Apply(UiElementTreeCommand.Attach(child, root)),
  "The child UI node could not be attached.");
Require(
  !tree.Apply(UiElementTreeCommand.Reparent(root, child)),
  "The tree accepted a cycle.");
Require(
  tree.Apply(UiElementTreeCommand.SetLayout(
    root,
    new UiElementTreeCommand.LayoutSettings(
      UiStyleDimension.Empty,
      UiStyleDimension.Empty,
      UiStyleDimension.FromPixels(100f),
      UiStyleDimension.FromPixels(80f),
      UiStyleDimension.Fill,
      UiStyleDimension.Fill,
      UiStyleDimension.Empty,
      UiStyleDimension.Empty,
      5f,
      5f,
      5f,
      5f,
      0f,
      0f,
      0f,
      0f,
      0f,
      0f))),
  "The root layout command was rejected.");
Require(
  tree.Apply(UiElementTreeCommand.SetLayout(
    child,
    new UiElementTreeCommand.LayoutSettings(
      UiStyleDimension.FromPixels(10f),
      UiStyleDimension.FromPixels(12f),
      UiStyleDimension.FromPixels(20f),
      UiStyleDimension.FromPixels(15f),
      UiStyleDimension.Fill,
      UiStyleDimension.Fill,
      UiStyleDimension.Empty,
      UiStyleDimension.Empty,
      0f,
      0f,
      0f,
      0f,
      0f,
      0f,
      0f,
      0f,
      0f,
      0f))),
  "The child layout command was rejected.");
new UiElementLayoutSystem(tree).Recalculate(
  new UiCalculatedStyle(0f, 0f, 200f, 100f));
Require(
  UiElementLayoutQuery.TryGet(tree, child, out UiElementLayoutQuery.Snapshot childSnapshot),
  "The child layout snapshot was not available.");
Require(
  childSnapshot.Dimensions.X == 17f
    && childSnapshot.Dimensions.Y == 15f
    && childSnapshot.Dimensions.Width == 20f
    && childSnapshot.Dimensions.Height == 15f,
  "The child layout snapshot was calculated incorrectly.");
Require(
  UiElementLayoutQuery.Contains(tree, child, new UiVector2(20f, 20f)),
  "The child hit-test query rejected an interior point.");
Require(
  tree.Apply(UiElementTreeCommand.Detach(root))
    && !tree.TryGet(child, out UiElementLayoutComponent? _),
  "Detaching a root did not clean up its descendants.");

RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Tree.UiElementInteractionComponent");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Systems.UiElementLifecycleSystem");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Systems.UiElementInteractionSystem");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Tree.UiElementInteractionQuery");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Rendering.UiElementGraphicsPolicyAdapter");

UiElementTreeStore interactionTree = new UiElementTreeStore();
UiElementId interactionRoot = interactionTree.CreateNode();
UiElementId interactionChild = interactionTree.CreateNode();
Require(
  interactionTree.Apply(UiElementTreeCommand.Attach(interactionRoot, null))
    && interactionTree.Apply(UiElementTreeCommand.Attach(interactionChild, interactionRoot)),
  "The interaction tree could not be attached.");
UiElementLifecycleSystem lifecycle = new UiElementLifecycleSystem(interactionTree);
Require(
  lifecycle.Initialize(interactionRoot, overflowHidden: true),
  "The interaction subtree could not be initialized.");
Require(
  lifecycle.Initialize(interactionRoot),
  "Initializing an already initialized UI subtree was not idempotent.");
Require(
  lifecycle.Activate(interactionRoot),
  "The interaction subtree could not be activated.");
Require(
  lifecycle.SetSnapPoint(
    interactionRoot,
    new UiSnapPoint(
      "root",
      new UiVector2(0.5f, 0.5f),
      UiVector2.Zero,
      1)),
  "The interaction snap point could not be set.");
UiElementInteractionSystem interaction = new UiElementInteractionSystem(lifecycle);
Require(
  interaction.SetHover(interactionChild, true),
  "The child hover state could not be written.");
Require(
  UiElementInteractionQuery.TryGet(
    lifecycle,
    interactionChild,
    out UiElementInteractionQuery.Snapshot interactionSnapshot)
    && interactionSnapshot.IsMouseHovering,
  "The interaction query did not observe hover state.");
Require(
  UiElementInteractionQuery.CanReceiveMouseInteraction(lifecycle, interactionChild),
  "An active interaction node was not hit-testable.");
Require(
  UiElementInteractionQuery.BlocksMouseInteraction(lifecycle, interactionRoot),
  "An ordinary interaction node did not block pointer propagation.");
Require(
  lifecycle.Deactivate(interactionRoot),
  "The interaction subtree could not be deactivated.");
Require(
  UiElementInteractionQuery.TryGet(
    lifecycle,
    interactionChild,
    out interactionSnapshot)
    && !interactionSnapshot.IsMouseHovering
    && interactionSnapshot.SnapPoint is null,
  "Deactivation did not clear transient interaction state recursively.");
Require(
  !UiElementInteractionQuery.CanReceiveMouseInteraction(lifecycle, interactionChild),
  "A deactivated node remained hit-testable.");
Require(
  lifecycle.Dispose(interactionRoot)
    && !lifecycle.TryGet(interactionRoot, out UiElementInteractionComponent? _)
    && !lifecycle.TryGet(interactionChild, out UiElementInteractionComponent? _),
  "Disposal did not clean up the interaction subtree.");

UiElementGraphicsPolicyAdapter graphicsPolicy = new UiElementGraphicsPolicyAdapter();
UiElementGraphicsPolicyAdapter.Policy policy = graphicsPolicy.GetPolicy(
  new UiElementInteractionQuery.Snapshot(
    interactionRoot,
    true,
    true,
    false,
    false,
    true,
    true,
    null,
    false,
    false));
Require(
  policy.ClipOverflow
    && policy.Rasterizer == UiElementGraphicsPolicyAdapter.RasterizerMode.Scissor
    && policy.Sampler == UiElementGraphicsPolicyAdapter.SamplerMode.PointClamp,
  "The graphics policy adapter did not isolate the presentation policy.");

RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Input.UiPointerInputComponent");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Input.UiPointerEvent");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Input.UiMouseEvent");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Input.UiScrollWheelEvent");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Systems.UiPointerDispatchSystem");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Systems.UiStateTransitionSystem");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Input.UiPointerInputAdapter");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Input.UiPointerQuery");

UiPointerInputComponent pointerInput = new UiPointerInputComponent(
  new UiElementId(100));
UiPointerDispatchSystem pointerDispatch = new UiPointerDispatchSystem(pointerInput);
UiPointerEvent leftDown = new UiPointerEvent(
  new UiElementId(101),
  new UiVector2(4f, 5f),
  UiPointerEvent.ButtonKind.Left,
  10,
  true);
UiPointerEvent leftUp = leftDown with { TimestampMilliseconds = 20, IsDown = false };
Require(
  pointerDispatch.Dispatch(leftDown).Accepted
    && pointerDispatch.Dispatch(leftUp).Clicked,
  "The left pointer click was not classified.");
UiPointerEvent rightDown = leftDown with
{
  Button = UiPointerEvent.ButtonKind.Right,
  TimestampMilliseconds = 30
};
UiPointerEvent rightUp = rightDown with { TimestampMilliseconds = 40, IsDown = false };
Require(
  pointerDispatch.Dispatch(rightDown).Accepted
    && pointerDispatch.Dispatch(rightUp).Clicked
    && pointerInput.LeftMouse.LastClickedTarget == new UiElementId(101)
    && pointerInput.RightMouse.LastClickedTarget == new UiElementId(101),
  "The pointer button caches were not independent.");
Require(
  pointerDispatch.Dispatch(leftDown with { TimestampMilliseconds = 50 }).Accepted
    && pointerDispatch.Dispatch(leftUp with { TimestampMilliseconds = 60 }).DoubleClicked,
  "The double-click threshold was not applied.");
UiStateTransitionSystem stateTransitions = new UiStateTransitionSystem(pointerInput);
Require(
  stateTransitions.TransitionTo(new UiStateId(1), 100),
  "The UI state transition was rejected.");
Require(
  !pointerDispatch.Dispatch(leftDown with { TimestampMilliseconds = 110 }).Accepted
    || !pointerDispatch.Dispatch(leftUp with { TimestampMilliseconds = 111 }).Clicked,
  "A state-change click was not suppressed.");
Require(
  pointerDispatch.DispatchScroll(
    new UiScrollWheelEvent(new UiElementId(101), 120),
    out UiScrollWheelEvent acceptedScroll)
    && acceptedScroll.ScrollWheelValue == 120,
  "The scroll-wheel payload was not preserved.");
for (int stateNumber = 2; stateNumber <= 40; stateNumber++)
{
  Require(
    stateTransitions.TransitionTo(new UiStateId(stateNumber), stateNumber * 10),
    "A valid UI state transition was rejected.");
}
Require(
  pointerInput.History.Count == 32
    && pointerInput.History[0] == new UiStateId(9),
  "The UI state history was not bounded and pruned.");
stateTransitions.SetVisible(false);
Require(
  !pointerInput.IsVisible
    && !pointerInput.LeftMouse.IsDown
    && !pointerInput.RightMouse.IsDown,
  "Pointer caches were not cleared when the host became invisible.");

UiPointerInputAdapter pointerAdapter = new UiPointerInputAdapter();
Require(
  pointerAdapter.TryRead(
    new FixedClock(500),
    new UiVector2(7f, 8f),
    true,
    out UiPointerInputAdapter.Sample pointerSample)
    && pointerSample.TimestampMilliseconds == 500
    && pointerSample.PointerPosition == new UiVector2(7f, 8f),
  "The pointer adapter did not expose an explicit clock sample.");

RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Collections.UiCollectionProgressComponent");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Collections.UiCollectionCommand");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Systems.UiCollectionLayoutSystem");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Systems.UiProgressPresentationSystem");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Collections.UiCollectionQuery");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Rendering.UiCollectionGraphicsAdapter");

UiCollectionProgressComponent collection = new UiCollectionProgressComponent(
  new UiElementId(200),
  listPadding: 2f);
UiElementId itemOne = new UiElementId(203);
UiElementId itemTwo = new UiElementId(201);
UiElementId itemThree = new UiElementId(202);
Require(
  collection.Apply(UiCollectionCommand.Add(itemOne))
    && collection.Apply(UiCollectionCommand.Add(itemTwo))
    && collection.Apply(UiCollectionCommand.Add(itemThree))
    && !collection.Apply(UiCollectionCommand.Add(itemOne)),
  "Collection membership validation did not reject a duplicate.");
Require(
  collection.Apply(UiCollectionCommand.SortAscending())
    && collection.Items[0] == itemTwo
    && collection.Items[2] == itemOne,
  "Collection sorting was not deterministic.");
Require(
  collection.Apply(UiCollectionCommand.AttachScrollbar(new UiElementId(204)))
    && collection.Apply(UiCollectionCommand.SetView(20f))
    && collection.Apply(UiCollectionCommand.SetScroll(100f)),
  "Collection view commands were rejected.");
new UiCollectionLayoutSystem(collection).Recalculate(10f, 5f);
Require(
  collection.CanScroll
    && collection.ViewPosition == 14f
    && collection.IsScrollbarVisible
    && UiCollectionQuery.GetVisibleItems(collection).Count == 2,
  "Collection layout did not clamp or derive visible items correctly.");
UiProgressPresentationSystem progress = new UiProgressPresentationSystem(collection);
Require(
  progress.ApplySample(0.75f, 0.5f)
    && !progress.ApplySample(1.1f, 0.5f)
    && collection.OverallProgressTarget == 0.75f,
  "Progress presentation did not reject an out-of-range sample.");
Require(
  collection.Apply(UiCollectionCommand.Remove(itemTwo))
    && collection.Apply(UiCollectionCommand.Clear())
    && collection.Items.Count == 0,
  "Collection removal and cleanup did not work.");
UiCollectionGraphicsAdapter collectionGraphics = new UiCollectionGraphicsAdapter();
UiCollectionGraphicsAdapter.Theme collectionTheme = collectionGraphics.GetTheme(collection);
Require(
  collectionTheme.HasScrollbarLink && collectionTheme.AutoHideScrollbar,
  "The collection graphics adapter did not isolate theme metadata.");

RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Text.UiTextPanelComponent");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Text.UiTextContentCommand");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Systems.UiTextMeasurementSystem");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Text.UiTextQuery");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Rendering.UiTextRenderProjection");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Text.UiTextLocalizationAdapter");

UiTextPanelComponent textPanel = new UiTextPanelComponent(new UiElementId(300));
Require(
  textPanel.Apply(UiTextContentCommand.SetSource(
    UiTextPanelComponent.TextSource.FromLocalizedKey("ui.title")))
    && textPanel.Apply(UiTextContentCommand.SetStyle(
      isLarge: false,
      isWrapped: true,
      dynamicallyScaleDownToWidth: true,
      wrappedTextBottomPadding: 1f,
      originX: 2f,
      originY: 3f))
    && textPanel.Apply(UiTextContentCommand.SetColors(0xffaabbcc, 0xff112233)),
  "Text content or style commands were rejected.");
UiTextLocalizationAdapter textAdapter = new UiTextLocalizationAdapter(
  new FixedTextResolver(),
  new FixedTextMeasurer());
UiTextMeasurementSystem textMeasurement = new UiTextMeasurementSystem(
  textPanel,
  textAdapter);
Require(
  textMeasurement.Recalculate(10f),
  "The text measurement system could not resolve the localized source.");
Require(
  textPanel.ResolvedText == "Localized title"
    && textPanel.HasResolvedText
    && textPanel.MeasuredSize.X == 10f
    && textPanel.EffectiveScale < textPanel.TextScale,
  "Text resolution or dynamic scale-down was incorrect.");
UiTextRenderProjection textProjection = new UiTextRenderProjection();
UiTextRenderProjection.DrawCommand drawCommand = textProjection.Create(
  UiTextQuery.Get(textPanel));
Require(
  drawCommand.Text == "Localized title"
    && drawCommand.Origin == new UiVector2(2f, 3f)
    && drawCommand.TextColor == 0xffaabbcc
    && drawCommand.ShadowColor == 0xff112233,
  "The text draw projection did not preserve immutable presentation values.");
UiTextPanelComponent missingText = new UiTextPanelComponent(new UiElementId(301));
Require(
  missingText.Apply(UiTextContentCommand.SetSource(
    UiTextPanelComponent.TextSource.FromLocalizedKey("ui.missing")))
    && !new UiTextMeasurementSystem(missingText, textAdapter).Recalculate()
    && !missingText.HasResolvedText
    && missingText.MeasuredSize == UiVector2.Zero,
  "Missing localized text did not produce an explicit empty result.");

RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Options.UiOptionSelectionComponent");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Options.UiOptionSelectionCommand");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Systems.UiOptionSelectionSystem");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Options.UiOptionSelectionQuery");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Rendering.UiOptionGraphicsAdapter");

UiOptionSelectionComponent.OptionToken optionOne = new(7, 1);
UiOptionSelectionComponent.OptionToken optionTwo = new(7, 2);
UiOptionSelectionComponent option = new UiOptionSelectionComponent(
  new UiElementId(400),
  optionOne,
  optionOne,
  color: 0xff101010,
  overrideUnpickedColor: 0xff202020,
  overridePickedColor: 0xfff0f0f0,
  titleId: new UiElementId(401),
  icon: new UiOptionSelectionComponent.IconProjection(
    true,
    3,
    0.75f,
    new UiVector2(1f, 2f),
    0xffabcdef));
UiOptionSelectionSystem optionSystem = new UiOptionSelectionSystem();
Require(
  UiOptionSelectionQuery.Get(option).IsSelected
    && UiOptionSelectionQuery.Get(option).EffectiveColor == 0xfff0f0f0,
  "The initial option selection query was incorrect.");
Require(
  optionSystem.Apply(option, UiOptionSelectionCommand.Select(optionTwo))
    && !UiOptionSelectionQuery.Get(option).IsSelected
    && UiOptionSelectionQuery.Get(option).EffectiveColor == 0xff202020,
  "The option selection system did not accept a same-group selection.");
Require(
  !optionSystem.Apply(
    option,
    UiOptionSelectionCommand.Select(new UiOptionSelectionComponent.OptionToken(8, 1)))
    && !optionSystem.Apply(
      option,
      UiOptionSelectionCommand.Select(UiOptionSelectionComponent.OptionToken.Invalid)),
  "The option selection system accepted an invalid group or token.");
UiOptionGraphicsAdapter optionGraphics = new UiOptionGraphicsAdapter();
UiOptionGraphicsAdapter.Policy optionPolicy = optionGraphics.GetPolicy(
  UiOptionSelectionQuery.Get(option));
Require(
  optionPolicy.HasIcon
    && optionPolicy.IconFrame == 3
    && optionPolicy.IconOffset == new UiVector2(1f, 2f),
  "The option graphics adapter did not preserve icon presentation metadata.");
UiOptionSelectionComponent noIconOption = new UiOptionSelectionComponent(
  new UiElementId(402),
  optionOne,
  optionOne,
  icon: UiOptionSelectionComponent.IconProjection.None);
Require(
  !UiOptionSelectionQuery.Get(noIconOption).Icon.HasIcon,
  "A missing icon was not represented as a valid no-icon projection.");

RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Screens.UiScreenPresentationComponent");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Screens.UiScreenLifecycleSystem");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Screens.UiScreenQuery");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Screens.UiWorldLoadProjection");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Screens.UiWorldSelectProjection");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Screens.UiWorldSummarySnapshot");

UiScreenPresentationComponent screen = new UiScreenPresentationComponent(
  new UiElementId(500),
  UiScreenPresentationComponent.ScreenKind.WorldSelect);
UiScreenLifecycleSystem screenLifecycle = new UiScreenLifecycleSystem(screen);
Require(
  screenLifecycle.AttachChildren(
    new UiElementId(501),
    new UiElementId(502),
    new UiElementId(503),
    new UiElementId(504),
    new UiElementId(505))
    && screenLifecycle.Activate()
    && screenLifecycle.SetScrollbarAttachment(true),
  "The screen child graph or activation boundary was rejected.");
UiWorldSummarySnapshot worldOne = new UiWorldSummarySnapshot(
  "world-1",
  "World One",
  true,
  9);
UiWorldSummarySnapshot worldTwo = new UiWorldSummarySnapshot(
  "world-2",
  "World Two",
  false,
  9);
Require(
  screenLifecycle.AcceptWorldSummary(worldOne)
    && screenLifecycle.AcceptFavoritesSnapshot([worldOne, worldTwo])
    && UiScreenQuery.Get(screen).FavoritesCache.Count == 1
    && UiScreenQuery.Get(screen).IsScrollbarAttached,
  "The screen summary or favorites snapshot was not projected read-only.");
UiWorldLoadProjection worldLoadProjection = new UiWorldLoadProjection();
Require(
  worldLoadProjection.TryProject(
    0.5f,
    0.25f,
    "Loading",
    out UiWorldLoadProjection.Projection loadProjection)
    && loadProjection.Message.Value == "Loading"
    && !worldLoadProjection.TryProject(
      1.5f,
      0.25f,
      "Invalid",
      out _),
  "The world-load projection did not validate progress samples.");
UiWorldSelectProjection worldSelectProjection = new UiWorldSelectProjection();
Require(
  worldSelectProjection.TryProject(
    [worldOne, worldTwo],
    9,
    out UiWorldSelectProjection.Projection selectProjection)
    && worldSelectProjection.TryCreateSelection(
      selectProjection,
      "world-1",
      9,
      out UiWorldSelectProjection.SelectionIntent selectionIntent)
    && selectionIntent.ExternalWorldKey == "world-1"
    && !worldSelectProjection.TryCreateSelection(
      selectProjection,
      "world-1",
      10,
      out _),
  "The world-select projection did not reject a stale snapshot.");
Require(
  screenLifecycle.Deactivate()
    && !UiScreenQuery.Get(screen).IsActive
    && UiScreenQuery.Get(screen).WorldListId is null
    && UiScreenQuery.Get(screen).FavoritesCache.Count == 0,
  "Screen deactivation did not clean up transient child state.");

RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Currency.UiCurrencyVisualsComponent");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Currency.UiCurrencyRegistryAdapter");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Currency.UiCurrencyDefinitionQuery");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Systems.UiCurrencyProjection");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Currency.UiCurrencyPresentationAdapter");

UiCurrencyRegistryAdapter currencyRegistry = new UiCurrencyRegistryAdapter();
UiCurrencyRegistryAdapter.Definition currencyDefinition =
  new UiCurrencyRegistryAdapter.Definition(
    "tokens",
    "currency.tokens",
    10,
    100,
    0xffffcc00,
    "icon.tokens");
Require(
  currencyRegistry.TryRegister(
    currencyDefinition,
    out UiCurrencyRegistryAdapter.RegistrationKey currencyKey)
    && !currencyRegistry.TryRegister(
      currencyDefinition,
      out UiCurrencyRegistryAdapter.RegistrationKey _),
  "The currency registry did not reject a duplicate definition.");
UiCurrencyVisualsComponent currencyVisuals = new UiCurrencyVisualsComponent(
  currencyKey,
  "currency.tokens",
  "icon.tokens",
  drawScale: 0.8f,
  color: 0xffffcc00);
UiCurrencyDefinitionQuery currencyDefinitions = new UiCurrencyDefinitionQuery(
  currencyRegistry);
UiCurrencyProjection currencyProjection = new UiCurrencyProjection(
  currencyDefinitions,
  new UiCurrencyPresentationAdapter());
Require(
  currencyProjection.TryProject(currencyVisuals, 0, 100, out UiCurrencyProjection.Projection currencyResult)
    && currencyResult.BalanceText == "0"
    && currencyResult.PriceText == "MAX"
    && currencyResult.Denomination == 10
    && !currencyProjection.TryProject(
      new UiCurrencyVisualsComponent(
        new UiCurrencyRegistryAdapter.RegistrationKey(999),
        "missing",
        "missing"),
      1,
      1,
      out _),
  "The currency projection did not isolate definition lookup or cap formatting.");
currencyRegistry.Reset();
Require(
  !currencyDefinitions.TryGet(currencyKey, out _),
  "Currency registry reset did not clear transient definitions.");

RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.World.WorldInteractionVisualsComponent");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.World.UiWorldAnchor");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Systems.UiEmoteProjectionSystem");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.World.UiEmoteTransportAdapter");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.World.UiRarityQuery");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.Systems.UiWiresInteractionSystem");
RequireType(
  uiAssembly,
  "Terraria.ClientPresentation.Ui.World.UiWorldAnchorQuery");

bool validEntityAnchor = UiWorldAnchor.TryForEntity(
  42,
  out UiWorldAnchor entityAnchor);
bool validTileAnchor = UiWorldAnchor.TryForTile(
  2,
  3,
  out UiWorldAnchor tileAnchor);
bool validPositionAnchor = UiWorldAnchor.TryForPosition(
  new UiVector2(100f, 50f),
  out UiWorldAnchor positionAnchor);
UiWorldAnchor invalidSizeAnchor = new UiWorldAnchor(
  UiWorldAnchor.AnchorKind.Entity,
  42,
  0,
  0,
  UiVector2.Zero,
  new UiVector2(-1f, 1f));
Require(
  validEntityAnchor
    && validTileAnchor
    && validPositionAnchor
    && tileAnchor.Size == new UiVector2(16f, 16f)
    && !invalidSizeAnchor.IsValid
    && !UiWorldAnchor.TryForEntity(0, out _),
  "World anchor validation did not preserve typed anchor boundaries or size constraints.");
Require(
  UiWorldAnchorQuery.TryGetScreenPosition(
    tileAnchor,
    new UiVector2(10f, 20f),
    new FixedEntityResolver(),
    out UiVector2 tileScreenPosition)
    && tileScreenPosition == new UiVector2(22f, 28f)
    && UiWorldAnchorQuery.TryGetScreenPosition(
      entityAnchor,
      new UiVector2(10f, 20f),
      new FixedEntityResolver(),
      out UiVector2 entityScreenPosition)
    && entityScreenPosition == new UiVector2(90f, 30f)
    && UiWorldAnchorQuery.TryGetScreenPosition(
      positionAnchor,
      new UiVector2(10f, 20f),
      new FixedEntityResolver(),
      out UiVector2 positionScreenPosition)
    && positionScreenPosition == new UiVector2(90f, 30f),
  "World anchor screen projection was incorrect.");
WorldInteractionVisualsComponent worldVisuals =
  new WorldInteractionVisualsComponent(new UiElementId(600), rarity: 2);
UiEmoteProjectionSystem emoteProjection = new UiEmoteProjectionSystem(
  worldVisuals,
  new UiEmoteTransportAdapter());
Require(
  emoteProjection.ApplyInbound(
    new UiEmoteTransportAdapter.Payload(
      11,
      4,
      100,
      25,
      entityAnchor))
    && worldVisuals.HasBubble
    && worldVisuals.BubbleAnchor == entityAnchor,
  "A valid emote payload was not projected.");
emoteProjection.Advance(50);
Require(
  worldVisuals.BubbleFrame == 2
    && worldVisuals.BubbleRemainingMilliseconds == 50,
  "Emote frame lifetime did not advance deterministically.");
Require(
  !emoteProjection.ApplyInbound(
    new UiEmoteTransportAdapter.Payload(
      11,
      4,
      100,
      25,
      new UiWorldAnchor(
        UiWorldAnchor.AnchorKind.Entity,
        0,
        0,
        0,
        UiVector2.Zero))),
  "Invalid emote anchor data was accepted.");
emoteProjection.Advance(50);
Require(
  !worldVisuals.HasBubble && worldVisuals.BubbleAnchor == UiWorldAnchor.None,
  "Expired emote state was not cleaned up.");
Require(
  UiRarityQuery.TryGet(2, out UiRarityQuery.Snapshot knownRarity)
    && knownRarity.Color == 0xff0080ff
    && !UiRarityQuery.TryGet(99, out UiRarityQuery.Snapshot unknownRarity)
    && unknownRarity.Color == 0xffffffff
    && worldVisuals.RarityColor == knownRarity.Color,
  "Rarity lookup did not provide a deterministic fallback.");
UiWiresInteractionSystem wires = new UiWiresInteractionSystem(worldVisuals);
Require(
  wires.OpenRadial()
    && wires.SelectTool(WorldInteractionVisualsComponent.ToolMode.Wire)
    && wires.TryCreateIntent(out UiWiresInteractionSystem.WiringIntent wiringIntent)
    && wiringIntent.Tool == WorldInteractionVisualsComponent.ToolMode.Wire,
  "The wiring radial state machine did not emit an explicit intent.");
Require(
  wires.CloseRadial()
    && !wires.TryCreateIntent(out _)
    && worldVisuals.SelectedTool == WorldInteractionVisualsComponent.ToolMode.None,
  "The wiring radial close transition did not clear transient state.");

Console.WriteLine("P16 UI verification passed.");

static Type RequireType(Assembly assembly, string fullName)
{
  return assembly.GetType(fullName)
    ?? throw new InvalidOperationException(fullName + " has not been implemented.");
}

static void Require(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

sealed class FixedClock(long timestampMilliseconds)
  : UiPointerInputAdapter.IMonotonicClock
{
  public long NowMilliseconds => timestampMilliseconds;
}

sealed class FixedTextResolver : UiTextLocalizationAdapter.IResolver
{
  public UiTextLocalizationAdapter.Resolution Resolve(
    UiTextPanelComponent.TextSource source)
  {
    return source.Value == "ui.title"
      ? new UiTextLocalizationAdapter.Resolution(true, "Localized title", 4)
      : new UiTextLocalizationAdapter.Resolution(false, string.Empty, 5);
  }
}

sealed class FixedTextMeasurer : UiTextLocalizationAdapter.IMeasurer
{
  public UiTextLocalizationAdapter.Measurement Measure(
    string text,
    float scale,
    bool isLarge,
    bool isWrapped,
    float maxWidth)
  {
    float width = text.Length * 2f * scale;
    return new UiTextLocalizationAdapter.Measurement(
      new UiVector2(width, isLarge ? 4f * scale : 2f * scale),
      scale);
  }
}

sealed class FixedEntityResolver : UiWorldAnchorQuery.IEntityPositionResolver
{
  public bool TryGetPosition(int entitySlot, out UiVector2 position)
  {
    if (entitySlot == 42)
    {
      position = new UiVector2(100f, 50f);
      return true;
    }

    position = default;
    return false;
  }
}

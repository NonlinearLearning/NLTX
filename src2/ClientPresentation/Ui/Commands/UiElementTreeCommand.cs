using Terraria.ClientPresentation.Ui.Layout;
using Terraria.ClientPresentation.Ui.Tree;

namespace Terraria.ClientPresentation.Ui.Commands;

public sealed record UiElementTreeCommand
{
  private UiElementTreeCommand(
    OperationKind operation,
    UiElementId nodeId,
    UiElementId? parentId,
    LayoutSettings? layout)
  {
    Operation = operation;
    NodeId = nodeId;
    ParentId = parentId;
    Layout = layout;
  }

  public enum OperationKind
  {
    Attach,
    Detach,
    Reparent,
    SetLayout
  }

  public OperationKind Operation { get; }

  public UiElementId NodeId { get; }

  public UiElementId? ParentId { get; }

  public LayoutSettings? Layout { get; }

  public static UiElementTreeCommand Attach(UiElementId nodeId, UiElementId? parentId)
  {
    return new UiElementTreeCommand(OperationKind.Attach, nodeId, parentId, null);
  }

  public static UiElementTreeCommand Detach(UiElementId nodeId)
  {
    return new UiElementTreeCommand(OperationKind.Detach, nodeId, null, null);
  }

  public static UiElementTreeCommand Reparent(
    UiElementId nodeId,
    UiElementId? parentId)
  {
    return new UiElementTreeCommand(OperationKind.Reparent, nodeId, parentId, null);
  }

  public static UiElementTreeCommand SetLayout(
    UiElementId nodeId,
    LayoutSettings layout)
  {
    return new UiElementTreeCommand(OperationKind.SetLayout, nodeId, null, layout);
  }

  public sealed record LayoutSettings(
    UiStyleDimension Top,
    UiStyleDimension Left,
    UiStyleDimension Width,
    UiStyleDimension Height,
    UiStyleDimension MaxWidth,
    UiStyleDimension MaxHeight,
    UiStyleDimension MinWidth,
    UiStyleDimension MinHeight,
    float PaddingTop,
    float PaddingLeft,
    float PaddingRight,
    float PaddingBottom,
    float MarginTop,
    float MarginLeft,
    float MarginRight,
    float MarginBottom,
    float HAlign,
    float VAlign);
}

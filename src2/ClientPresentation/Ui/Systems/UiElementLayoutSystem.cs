using Terraria.ClientPresentation.Ui.Layout;
using Terraria.ClientPresentation.Ui.Tree;

namespace Terraria.ClientPresentation.Ui.Systems;

public sealed class UiElementLayoutSystem
{
  private readonly UiElementTreeStore _store;

  public UiElementLayoutSystem(UiElementTreeStore store)
  {
    _store = store ?? throw new ArgumentNullException(nameof(store));
  }

  public void Recalculate(UiCalculatedStyle viewport)
  {
    foreach (UiElementId rootId in _store.RootNodes)
    {
      if (_store.TryGet(rootId, out UiElementLayoutComponent? root)
        && root is not null)
      {
        RecalculateNode(root, viewport);
      }
    }
  }

  private void RecalculateNode(
    UiElementLayoutComponent node,
    UiCalculatedStyle parentDimensions)
  {
    float width = UiLayoutValueQuery.Clamp(
      node.Width.GetValue(parentDimensions.Width),
      node.MinWidth,
      node.MaxWidth,
      parentDimensions.Width);
    float height = UiLayoutValueQuery.Clamp(
      node.Height.GetValue(parentDimensions.Height),
      node.MinHeight,
      node.MaxHeight,
      parentDimensions.Height);
    float availableWidth = parentDimensions.Width - width - node.MarginLeft - node.MarginRight;
    float availableHeight = parentDimensions.Height - height - node.MarginTop - node.MarginBottom;
    float x = parentDimensions.X
      + node.Left.GetValue(parentDimensions.Width)
      + node.MarginLeft
      + availableWidth * node.HAlign;
    float y = parentDimensions.Y
      + node.Top.GetValue(parentDimensions.Height)
      + node.MarginTop
      + availableHeight * node.VAlign;
    UiCalculatedStyle dimensions = new UiCalculatedStyle(x, y, width, height);
    UiCalculatedStyle outerDimensions = new UiCalculatedStyle(
      x - node.MarginLeft,
      y - node.MarginTop,
      width + node.MarginLeft + node.MarginRight,
      height + node.MarginTop + node.MarginBottom);
    UiCalculatedStyle innerDimensions = new UiCalculatedStyle(
      x + node.PaddingLeft,
      y + node.PaddingTop,
      Math.Max(0f, width - node.PaddingLeft - node.PaddingRight),
      Math.Max(0f, height - node.PaddingTop - node.PaddingBottom));
    node.SetCalculated(innerDimensions, dimensions, outerDimensions);

    foreach (UiElementId childId in node.Children)
    {
      if (_store.TryGet(childId, out UiElementLayoutComponent? child)
        && child is not null)
      {
        RecalculateNode(child, innerDimensions);
      }
    }
  }
}

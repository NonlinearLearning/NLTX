using Terraria.ClientPresentation.Ui.Layout;

namespace Terraria.ClientPresentation.Ui.Tree;

public static class UiElementLayoutQuery
{
  public static bool TryGet(
    UiElementTreeStore store,
    UiElementId id,
    out Snapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(store);
    if (!store.TryGet(id, out UiElementLayoutComponent? component)
      || component is null)
    {
      snapshot = default;
      return false;
    }

    snapshot = new Snapshot(
      component.Id,
      component.Parent,
      component.Children,
      component.InnerDimensions,
      component.Dimensions,
      component.OuterDimensions);
    return true;
  }

  public static IReadOnlyList<UiElementId> GetChildren(
    UiElementTreeStore store,
    UiElementId id)
  {
    return TryGet(store, id, out Snapshot snapshot)
      ? snapshot.Children
      : Array.Empty<UiElementId>();
  }

  public static bool Contains(
    UiElementTreeStore store,
    UiElementId id,
    UiVector2 point)
  {
    return TryGet(store, id, out Snapshot snapshot)
      && snapshot.Dimensions.Contains(point);
  }

  public readonly record struct Snapshot(
    UiElementId Id,
    UiElementId? Parent,
    IReadOnlyList<UiElementId> Children,
    UiCalculatedStyle InnerDimensions,
    UiCalculatedStyle Dimensions,
    UiCalculatedStyle OuterDimensions);
}

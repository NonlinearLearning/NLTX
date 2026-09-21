namespace Terraria.ClientPresentation.Ui.Collections;

public static class UiCollectionQuery
{
  public static Snapshot Get(UiCollectionProgressComponent collection)
  {
    ArgumentNullException.ThrowIfNull(collection);
    return new Snapshot(
      collection.HostId,
      collection.Items,
      collection.ScrollbarId,
      collection.OverallProgressTarget,
      collection.CurrentProgressTarget,
      collection.ViewPosition,
      collection.ViewSize,
      collection.MaxViewSize,
      collection.CanScroll,
      collection.IsScrollbarVisible,
      collection.FirstVisibleIndex,
      collection.LastVisibleIndex);
  }

  public static IReadOnlyList<Terraria.ClientPresentation.Ui.Tree.UiElementId>
    GetVisibleItems(UiCollectionProgressComponent collection)
  {
    ArgumentNullException.ThrowIfNull(collection);
    IReadOnlyList<Terraria.ClientPresentation.Ui.Tree.UiElementId> items = collection.Items;
    if (collection.LastVisibleIndex < collection.FirstVisibleIndex)
    {
      return Array.Empty<Terraria.ClientPresentation.Ui.Tree.UiElementId>();
    }

    return items
      .Skip(collection.FirstVisibleIndex)
      .Take(collection.LastVisibleIndex - collection.FirstVisibleIndex + 1)
      .ToArray();
  }

  public readonly record struct Snapshot(
    Terraria.ClientPresentation.Ui.Tree.UiElementId HostId,
    IReadOnlyList<Terraria.ClientPresentation.Ui.Tree.UiElementId> Items,
    Terraria.ClientPresentation.Ui.Tree.UiElementId? ScrollbarId,
    float OverallProgressTarget,
    float CurrentProgressTarget,
    float ViewPosition,
    float ViewSize,
    float MaxViewSize,
    bool CanScroll,
    bool IsScrollbarVisible,
    int FirstVisibleIndex,
    int LastVisibleIndex);
}

using Terraria.ClientPresentation.Ui.Tree;

namespace Terraria.ClientPresentation.Ui.Collections;

public sealed record UiCollectionCommand
{
  private UiCollectionCommand(
    OperationKind operation,
    UiElementId itemId,
    UiElementId? scrollbarId,
    float value)
  {
    Operation = operation;
    ItemId = itemId;
    ScrollbarId = scrollbarId;
    Value = value;
  }

  public enum OperationKind
  {
    Add,
    Remove,
    Clear,
    SortAscending,
    SortDescending,
    SetScroll,
    SetView,
    AttachScrollbar
  }

  public OperationKind Operation { get; }

  public UiElementId ItemId { get; }

  public UiElementId? ScrollbarId { get; }

  public float Value { get; }

  public static UiCollectionCommand Add(UiElementId itemId)
  {
    return new UiCollectionCommand(OperationKind.Add, itemId, null, 0f);
  }

  public static UiCollectionCommand Remove(UiElementId itemId)
  {
    return new UiCollectionCommand(OperationKind.Remove, itemId, null, 0f);
  }

  public static UiCollectionCommand Clear()
  {
    return new UiCollectionCommand(
      OperationKind.Clear,
      UiElementId.Invalid,
      null,
      0f);
  }

  public static UiCollectionCommand SortAscending()
  {
    return new UiCollectionCommand(
      OperationKind.SortAscending,
      UiElementId.Invalid,
      null,
      0f);
  }

  public static UiCollectionCommand SortDescending()
  {
    return new UiCollectionCommand(
      OperationKind.SortDescending,
      UiElementId.Invalid,
      null,
      0f);
  }

  public static UiCollectionCommand SetScroll(float position)
  {
    return new UiCollectionCommand(
      OperationKind.SetScroll,
      UiElementId.Invalid,
      null,
      position);
  }

  public static UiCollectionCommand SetView(float viewSize)
  {
    return new UiCollectionCommand(
      OperationKind.SetView,
      UiElementId.Invalid,
      null,
      viewSize);
  }

  public static UiCollectionCommand AttachScrollbar(UiElementId? scrollbarId)
  {
    return new UiCollectionCommand(
      OperationKind.AttachScrollbar,
      UiElementId.Invalid,
      scrollbarId,
      0f);
  }
}

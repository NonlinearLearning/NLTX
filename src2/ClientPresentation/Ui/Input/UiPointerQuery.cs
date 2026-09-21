namespace Terraria.ClientPresentation.Ui.Input;

public static class UiPointerQuery
{
  public static Snapshot Get(UiPointerInputComponent input)
  {
    ArgumentNullException.ThrowIfNull(input);
    return new Snapshot(
      input.HostId,
      input.PointerPosition,
      input.IsVisible,
      input.CurrentState,
      input.LeftMouse,
      input.RightMouse,
      input.History);
  }

  public readonly record struct Snapshot(
    Terraria.ClientPresentation.Ui.Tree.UiElementId HostId,
    Terraria.ClientPresentation.Ui.Layout.UiVector2 PointerPosition,
    bool IsVisible,
    UiStateId? CurrentState,
    UiPointerInputComponent.ButtonSnapshot LeftMouse,
    UiPointerInputComponent.ButtonSnapshot RightMouse,
    IReadOnlyList<UiStateId> History);
}

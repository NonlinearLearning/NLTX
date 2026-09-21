using Terraria.ClientPresentation.Ui.Input;
using Terraria.ClientPresentation.Ui.Tree;

namespace Terraria.ClientPresentation.Ui.Systems;

public sealed class UiPointerDispatchSystem
{
  private readonly UiPointerInputComponent _input;

  public UiPointerDispatchSystem(UiPointerInputComponent input)
  {
    _input = input ?? throw new ArgumentNullException(nameof(input));
  }

  public DispatchResult Dispatch(UiPointerEvent pointerEvent)
  {
    if (!pointerEvent.Target.IsValid || pointerEvent.TimestampMilliseconds < 0)
    {
      return DispatchResult.Rejected;
    }

    UiPointerInputComponent.ButtonSnapshot previous = _input.GetButton(
      pointerEvent.Button);
    if (pointerEvent.TimestampMilliseconds < previous.LastTimeDown)
    {
      return DispatchResult.Rejected;
    }

    _input.SetPointerPosition(pointerEvent.Position);
    if (pointerEvent.IsDown)
    {
      _input.SetButton(
        pointerEvent.Button,
        new UiPointerInputComponent.ButtonSnapshot(
          true,
          pointerEvent.Target,
          pointerEvent.TimestampMilliseconds,
          previous.LastClickedTarget,
          previous.LastClickTime,
          pointerEvent,
          null,
          null,
          null));
      return new DispatchResult(true, false, false, pointerEvent);
    }

    if (!previous.IsDown)
    {
      return DispatchResult.Rejected;
    }

    bool clicked = previous.LastDownTarget == pointerEvent.Target
      && !_input.AreClicksSuppressed(pointerEvent.TimestampMilliseconds);
    bool doubleClicked = clicked
      && previous.LastClickedTarget == pointerEvent.Target
      && pointerEvent.TimestampMilliseconds - previous.LastClickTime
        <= UiPointerInputComponent.DoubleClickTimeMilliseconds;
    UiPointerEvent? clickEvent = clicked ? pointerEvent : null;
    UiPointerEvent? doubleClickEvent = doubleClicked ? pointerEvent : null;
    _input.SetButton(
      pointerEvent.Button,
      new UiPointerInputComponent.ButtonSnapshot(
        false,
        previous.LastDownTarget,
        previous.LastTimeDown,
        clicked ? pointerEvent.Target : previous.LastClickedTarget,
        clicked ? pointerEvent.TimestampMilliseconds : previous.LastClickTime,
        previous.MouseDownEvent,
        pointerEvent,
        clickEvent,
        doubleClickEvent));
    return new DispatchResult(true, clicked, doubleClicked, pointerEvent);
  }

  public bool DispatchMouseMove(UiMouseEvent mouseEvent)
  {
    if (!mouseEvent.Target.IsValid)
    {
      return false;
    }

    _input.SetPointerPosition(mouseEvent.MousePosition);
    return true;
  }

  public bool DispatchScroll(
    UiScrollWheelEvent scrollEvent,
    out UiScrollWheelEvent acceptedEvent)
  {
    if (!scrollEvent.Target.IsValid)
    {
      acceptedEvent = default;
      return false;
    }

    acceptedEvent = scrollEvent;
    return true;
  }

  public void ClearFrameEvents()
  {
    ClearFrameEvents(UiPointerEvent.ButtonKind.Left);
    ClearFrameEvents(UiPointerEvent.ButtonKind.Right);
  }

  private void ClearFrameEvents(UiPointerEvent.ButtonKind button)
  {
    UiPointerInputComponent.ButtonSnapshot snapshot = _input.GetButton(button);
    _input.SetButton(
      button,
      snapshot with
      {
        MouseDownEvent = null,
        MouseUpEvent = null,
        ClickEvent = null,
        DoubleClickEvent = null
      });
  }

  public readonly record struct DispatchResult(
    bool Accepted,
    bool Clicked,
    bool DoubleClicked,
    UiPointerEvent? Event)
  {
    public static DispatchResult Rejected => new(false, false, false, null);
  }
}

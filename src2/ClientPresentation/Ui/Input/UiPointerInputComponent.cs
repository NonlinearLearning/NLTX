using Terraria.ClientPresentation.Ui.Layout;
using Terraria.ClientPresentation.Ui.Tree;

namespace Terraria.ClientPresentation.Ui.Input;

public sealed class UiPointerInputComponent
{
  public UiPointerInputComponent(UiElementId hostId)
  {
    if (!hostId.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(hostId));
    }

    HostId = hostId;
  }

  public UiElementId HostId { get; }

  public UiVector2 PointerPosition { get; private set; }

  public bool IsVisible { get; private set; } = true;

  public UiStateId? CurrentState { get; private set; }

  public ButtonSnapshot LeftMouse { get; private set; }

  public ButtonSnapshot RightMouse { get; private set; }

  public IReadOnlyList<UiStateId> History => _history.ToArray();

  internal const long DoubleClickTimeMilliseconds = 500;

  internal const long StateChangeClickDisableTimeMilliseconds = 100;

  private readonly List<UiStateId> _history = [];
  private long _suppressClicksUntilMilliseconds;

  internal void SetPointerPosition(UiVector2 pointerPosition)
  {
    PointerPosition = pointerPosition;
  }

  internal void SetVisible(bool isVisible)
  {
    IsVisible = isVisible;
    if (!isVisible)
    {
      ClearPointerCaches();
    }
  }

  internal void SetButton(
    UiPointerEvent.ButtonKind button,
    ButtonSnapshot snapshot)
  {
    if (button == UiPointerEvent.ButtonKind.Left)
    {
      LeftMouse = snapshot;
    }
    else
    {
      RightMouse = snapshot;
    }
  }

  internal ButtonSnapshot GetButton(UiPointerEvent.ButtonKind button)
  {
    return button == UiPointerEvent.ButtonKind.Left
      ? LeftMouse
      : RightMouse;
  }

  internal void SetCurrentState(UiStateId? state, long timestampMilliseconds)
  {
    CurrentState = state;
    if (state.HasValue)
    {
      _history.Remove(state.Value);
      _history.Add(state.Value);
      while (_history.Count > 32)
      {
        _history.RemoveRange(0, Math.Min(8, _history.Count - 32));
      }
    }

    _suppressClicksUntilMilliseconds = timestampMilliseconds
      + StateChangeClickDisableTimeMilliseconds;
    ClearPointerCaches();
  }

  internal bool AreClicksSuppressed(long timestampMilliseconds)
  {
    return timestampMilliseconds <= _suppressClicksUntilMilliseconds;
  }

  internal void ClearPointerCaches()
  {
    LeftMouse = default;
    RightMouse = default;
  }

  internal void ClearHistory()
  {
    _history.Clear();
  }

  public readonly record struct ButtonSnapshot(
    bool IsDown,
    UiElementId? LastDownTarget,
    long LastTimeDown,
    UiElementId? LastClickedTarget,
    long LastClickTime,
    UiPointerEvent? MouseDownEvent,
    UiPointerEvent? MouseUpEvent,
    UiPointerEvent? ClickEvent,
    UiPointerEvent? DoubleClickEvent);
}

using Terraria.ClientPresentation.Ui.Layout;
using Terraria.ClientPresentation.Ui.Tree;

namespace Terraria.ClientPresentation.Ui.Input;

public readonly record struct UiPointerEvent(
  UiElementId Target,
  UiVector2 Position,
  UiPointerEvent.ButtonKind Button,
  long TimestampMilliseconds,
  bool IsDown)
{
  public enum ButtonKind
  {
    Left,
    Right
  }
}

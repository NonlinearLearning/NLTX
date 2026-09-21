using Terraria.ClientPresentation.Ui.Tree;

namespace Terraria.ClientPresentation.Ui.Input;

public readonly record struct UiScrollWheelEvent(
  UiElementId Target,
  int ScrollWheelValue);

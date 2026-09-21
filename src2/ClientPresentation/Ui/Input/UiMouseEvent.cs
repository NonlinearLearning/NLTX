using Terraria.ClientPresentation.Ui.Layout;
using Terraria.ClientPresentation.Ui.Tree;

namespace Terraria.ClientPresentation.Ui.Input;

public readonly record struct UiMouseEvent(
  UiElementId Target,
  UiVector2 MousePosition);

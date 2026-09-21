using Terraria.ClientPresentation.Ui.Layout;
using Terraria.ClientPresentation.Ui.Text;

namespace Terraria.ClientPresentation.Ui.Rendering;

public sealed class UiTextRenderProjection
{
  public DrawCommand Create(UiTextQuery.Snapshot snapshot)
  {
    return new DrawCommand(
      snapshot.HostId,
      snapshot.ResolvedText,
      new UiVector2(snapshot.TextOriginX, snapshot.TextOriginY),
      snapshot.EffectiveScale,
      snapshot.TextColor,
      snapshot.ShadowColor,
      snapshot.IsWrapped,
      snapshot.IsLarge);
  }

  public readonly record struct DrawCommand(
    Terraria.ClientPresentation.Ui.Tree.UiElementId HostId,
    string Text,
    UiVector2 Origin,
    float Scale,
    uint TextColor,
    uint ShadowColor,
    bool IsWrapped,
    bool IsLarge);
}

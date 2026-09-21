using Terraria.ClientPresentation.Ui.Layout;

namespace Terraria.ClientPresentation.Ui.Text;

public static class UiTextQuery
{
  public static Snapshot Get(UiTextPanelComponent text)
  {
    ArgumentNullException.ThrowIfNull(text);
    return new Snapshot(
      text.HostId,
      text.Source,
      text.ResolvedText,
      text.HasResolvedText,
      text.TextScale,
      text.EffectiveScale,
      text.MeasuredSize,
      text.IsLarge,
      text.TextColor,
      text.ShadowColor,
      text.IsWrapped,
      text.DynamicallyScaleDownToWidth,
      text.TextOriginX,
      text.TextOriginY,
      text.WrappedTextBottomPadding,
      text.ContentRevision);
  }

  public readonly record struct Snapshot(
    Terraria.ClientPresentation.Ui.Tree.UiElementId HostId,
    UiTextPanelComponent.TextSource Source,
    string ResolvedText,
    bool HasResolvedText,
    float TextScale,
    float EffectiveScale,
    UiVector2 MeasuredSize,
    bool IsLarge,
    uint TextColor,
    uint ShadowColor,
    bool IsWrapped,
    bool DynamicallyScaleDownToWidth,
    float TextOriginX,
    float TextOriginY,
    float WrappedTextBottomPadding,
    int ContentRevision);
}

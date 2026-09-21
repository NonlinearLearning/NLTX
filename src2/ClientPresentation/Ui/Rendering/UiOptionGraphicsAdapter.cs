using Terraria.ClientPresentation.Ui.Options;

namespace Terraria.ClientPresentation.Ui.Rendering;

public sealed class UiOptionGraphicsAdapter
{
  public Policy GetPolicy(UiOptionSelectionQuery.Snapshot snapshot)
  {
    return new Policy(
      snapshot.IsSelected,
      snapshot.ShowHighlightWhenSelected,
      snapshot.FadeFromBlack,
      snapshot.InnerHighlightRim,
      snapshot.Icon.HasIcon,
      snapshot.Icon.Frame,
      snapshot.Icon.Scale,
      snapshot.Icon.Offset,
      snapshot.Icon.Color);
  }

  public readonly record struct Policy(
    bool IsSelected,
    bool ShowHighlight,
    bool FadeFromBlack,
    bool InnerHighlightRim,
    bool HasIcon,
    int IconFrame,
    float IconScale,
    Terraria.ClientPresentation.Ui.Layout.UiVector2 IconOffset,
    uint IconColor);
}

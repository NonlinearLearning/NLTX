using Terraria.ClientPresentation.Ui.Layout;
using Terraria.ClientPresentation.Ui.Text;
using Terraria.ClientPresentation.Ui.Tree;

namespace Terraria.ClientPresentation.Ui.Options;

public static class UiOptionSelectionQuery
{
  public static Snapshot Get(UiOptionSelectionComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    uint effectiveColor = component.IsSelected
      ? component.OverridePickedColor ?? component.Color
      : component.OverrideUnpickedColor ?? component.Color;
    return new Snapshot(
      component.HostId,
      component.OptionValue,
      component.CurrentSelection,
      component.IsSelected,
      effectiveColor,
      component.BorderColor,
      component.FadeFromBlack,
      component.InnerHighlightRim,
      component.ShowHighlightWhenSelected,
      component.Description,
      component.TitleId,
      component.Icon);
  }

  public readonly record struct Snapshot(
    UiElementId HostId,
    UiOptionSelectionComponent.OptionToken OptionValue,
    UiOptionSelectionComponent.OptionToken CurrentSelection,
    bool IsSelected,
    uint EffectiveColor,
    uint BorderColor,
    bool FadeFromBlack,
    bool InnerHighlightRim,
    bool ShowHighlightWhenSelected,
    UiTextPanelComponent.TextSource Description,
    UiElementId? TitleId,
    UiOptionSelectionComponent.IconProjection Icon);
}

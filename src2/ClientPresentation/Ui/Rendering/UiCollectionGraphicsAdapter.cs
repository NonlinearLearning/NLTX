using Terraria.ClientPresentation.Ui.Collections;

namespace Terraria.ClientPresentation.Ui.Rendering;

public sealed class UiCollectionGraphicsAdapter
{
  public Theme GetTheme(UiCollectionProgressComponent collection)
  {
    ArgumentNullException.ThrowIfNull(collection);
    return new Theme(
      collection.BorderColor,
      collection.BackgroundColor,
      collection.AutoHide,
      collection.ScrollbarId.HasValue);
  }

  public readonly record struct Theme(
    uint BorderColor,
    uint BackgroundColor,
    bool AutoHideScrollbar,
    bool HasScrollbarLink);
}

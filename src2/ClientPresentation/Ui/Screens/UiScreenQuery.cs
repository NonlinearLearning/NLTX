namespace Terraria.ClientPresentation.Ui.Screens;

public static class UiScreenQuery
{
  public static Snapshot Get(UiScreenPresentationComponent screen)
  {
    ArgumentNullException.ThrowIfNull(screen);
    return new Snapshot(
      screen.HostId,
      screen.Kind,
      screen.IsActive,
      screen.ProgressBarId,
      screen.ProgressMessageId,
      screen.WorldListId,
      screen.ContainerPanelId,
      screen.ScrollbarId,
      screen.IsScrollbarAttached,
      screen.NewlyGeneratedWorld,
      screen.FavoritesCache);
  }

  public readonly record struct Snapshot(
    Terraria.ClientPresentation.Ui.Tree.UiElementId HostId,
    UiScreenPresentationComponent.ScreenKind Kind,
    bool IsActive,
    Terraria.ClientPresentation.Ui.Tree.UiElementId? ProgressBarId,
    Terraria.ClientPresentation.Ui.Tree.UiElementId? ProgressMessageId,
    Terraria.ClientPresentation.Ui.Tree.UiElementId? WorldListId,
    Terraria.ClientPresentation.Ui.Tree.UiElementId? ContainerPanelId,
    Terraria.ClientPresentation.Ui.Tree.UiElementId? ScrollbarId,
    bool IsScrollbarAttached,
    UiWorldSummarySnapshot? NewlyGeneratedWorld,
    IReadOnlyList<UiWorldSummarySnapshot> FavoritesCache);
}

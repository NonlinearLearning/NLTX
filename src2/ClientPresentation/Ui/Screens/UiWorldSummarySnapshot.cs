namespace Terraria.ClientPresentation.Ui.Screens;

public readonly record struct UiWorldSummarySnapshot(
  string ExternalWorldKey,
  string Name,
  bool IsFavorite,
  int Revision)
{
  public bool IsValid => !string.IsNullOrWhiteSpace(ExternalWorldKey)
    && !string.IsNullOrWhiteSpace(Name)
    && Revision >= 0;
}

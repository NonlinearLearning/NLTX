namespace Terraria.ClientPresentation.Ui.Options;

public sealed record UiOptionSelectionCommand
{
  private UiOptionSelectionCommand(
    UiOptionSelectionComponent.OptionToken selection)
  {
    Selection = selection;
  }

  public UiOptionSelectionComponent.OptionToken Selection { get; }

  public static UiOptionSelectionCommand Select(
    UiOptionSelectionComponent.OptionToken selection)
  {
    return new UiOptionSelectionCommand(selection);
  }
}

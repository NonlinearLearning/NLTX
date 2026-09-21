using Terraria.ClientPresentation.Ui.Options;

namespace Terraria.ClientPresentation.Ui.Systems;

public sealed class UiOptionSelectionSystem
{
  public bool Apply(
    UiOptionSelectionComponent component,
    UiOptionSelectionCommand command)
  {
    ArgumentNullException.ThrowIfNull(component);
    ArgumentNullException.ThrowIfNull(command);
    UiOptionSelectionComponent.OptionToken selection = command.Selection;
    if (!selection.IsValid
      || selection.GroupId != component.OptionValue.GroupId)
    {
      return false;
    }

    component.SetCurrentSelection(selection);
    return true;
  }
}

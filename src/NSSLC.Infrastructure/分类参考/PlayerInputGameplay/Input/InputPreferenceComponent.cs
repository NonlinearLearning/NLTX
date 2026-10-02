namespace NLTX.PlayerInputGameplay.Input;

public sealed class InputPreferenceComponent
{
  public string InventoryCancelAction { get; private set; } = "Escape";

  public void SetInventoryCancelAction(string actionName)
  {
    if (string.IsNullOrWhiteSpace(actionName))
    {
      throw new ArgumentException("An input action is required.", nameof(actionName));
    }

    InventoryCancelAction = actionName;
  }
}

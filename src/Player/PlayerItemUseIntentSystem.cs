namespace Terraria.Player;

public static class PlayerItemUseIntentSystem
{
  public static PlayerItemUseIntentComponent CreateResetState()
  {
    return new PlayerItemUseIntentComponent
    {
      ControlUseItem = false,
      ControlUseTile = false,
    };
  }

  public static void Advance(
    in PlayerItemUseIntentInput input,
    bool reset,
    ref PlayerItemUseIntentComponent component)
  {
    if (reset)
    {
      component = CreateResetState();
      return;
    }

    component.ControlUseItem = input.ControlUseItem;
    component.ControlUseTile = input.ControlUseTile;
  }
}

namespace Terraria.Player;

public static class PlayerInteractionUiStateSystem
{
  public static PlayerInteractionUiStateComponent CreateResetState()
  {
    return new PlayerInteractionUiStateComponent
    {
      CreativeInterface = false,
      MouseInterface = false,
      LastMouseInterface = false,
      NoThrow = 0,
    };
  }

  public static void Advance(
    in PlayerInteractionUiStateInput input,
    bool reset,
    ref PlayerInteractionUiStateComponent state)
  {
    if (reset)
    {
      state = CreateResetState();
      return;
    }

    state.CreativeInterface = input.CreativeInterface;
    state.MouseInterface = input.MouseInterface;
    state.LastMouseInterface = input.LastMouseInterface;
    state.NoThrow = input.NoThrow;
  }
}

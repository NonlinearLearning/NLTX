using Terraria.ClientPresentation.Ui.Input;

namespace Terraria.ClientPresentation.Ui.Systems;

public sealed class UiStateTransitionSystem
{
  private readonly UiPointerInputComponent _input;

  public UiStateTransitionSystem(UiPointerInputComponent input)
  {
    _input = input ?? throw new ArgumentNullException(nameof(input));
  }

  public bool TransitionTo(UiStateId state, long timestampMilliseconds)
  {
    if (!state.IsValid || timestampMilliseconds < 0)
    {
      return false;
    }

    _input.SetCurrentState(state, timestampMilliseconds);
    return true;
  }

  public void SetVisible(bool isVisible)
  {
    _input.SetVisible(isVisible);
  }

  public void ClearPointers()
  {
    _input.ClearPointerCaches();
  }

  public void ClearHistory()
  {
    _input.ClearHistory();
  }
}

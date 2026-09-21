namespace Terraria.Player.Animation;

public sealed class PlayerEyeAnimationComponent
{
  public PlayerEyeAnimationComponent()
  {
    State = PlayerEyeAnimationState.NormalBlinking;
  }

  public PlayerEyeAnimationState State { get; private set; }

  public int TimeInState { get; private set; }

  public int EyeFrameToShow { get; private set; }

  internal void AdvanceTime()
  {
    TimeInState++;
  }

  internal void SetEyeFrame(int eyeFrameToShow)
  {
    EyeFrameToShow = eyeFrameToShow;
  }

  internal void SetState(PlayerEyeAnimationState state, bool resetStateTimerEvenIfAlreadyInState = false)
  {
    if (State == state && !resetStateTimerEvenIfAlreadyInState)
    {
      return;
    }

    State = state;
    TimeInState = 0;
  }

  internal void SetTimeInState(int timeInState)
  {
    TimeInState = timeInState;
  }

  internal void Reset()
  {
    State = PlayerEyeAnimationState.NormalBlinking;
    TimeInState = 0;
    EyeFrameToShow = 0;
  }
}

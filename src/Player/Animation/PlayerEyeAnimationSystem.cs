namespace Terraria.Player.Animation;

public sealed class PlayerEyeAnimationSystem
{
  private const int TimeToActDamaged = 20;

  private const int NormalBlinkPeriod = 240;
  private const int NormalBlinkStart = 234;
  private const int EnvironmentalBlinkPeriod = 120;
  private const int StormBlinkStart = 114;
  private const int ConditionBlinkStart = 100;

  public void BlinkBecausePlayerGotHurt(
    PlayerEyeAnimationComponent component,
    PlayerEyeHurtPresentationEvent hurtEvent)
  {
    ArgumentNullException.ThrowIfNull(component);

    _ = hurtEvent;
    component.SetState(
      PlayerEyeAnimationState.JustTookDamage,
      resetStateTimerEvenIfAlreadyInState: true);
  }

  public PlayerEyeAnimationSnapshot Update(
    PlayerEyeAnimationComponent component,
    in PlayerEyeAnimationInput input)
  {
    ArgumentNullException.ThrowIfNull(component);

    SetStateByInput(component, input);
    UpdateEyeFrame(component, input);

    // Preserve the legacy order: the frame uses the current state time, then the timer advances.
    component.AdvanceTime();

    return new PlayerEyeAnimationSnapshot(
      component.State,
      component.TimeInState,
      component.EyeFrameToShow);
  }

  public void ResetForSpawn(PlayerEyeAnimationComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Reset();
  }

  public void ResetForRemoval(PlayerEyeAnimationComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Reset();
  }

  private static bool IsModeratelyDamaged(PlayerEyeAnimationInput input)
  {
    return input.CountsAsModeratelyDamaged;
  }

  private static void SetStateByInput(
    PlayerEyeAnimationComponent component,
    PlayerEyeAnimationInput input)
  {
    if (input.IsBlackout || input.IsBlind)
    {
      component.SetState(PlayerEyeAnimationState.IsBlind);
      return;
    }

    if (component.State == PlayerEyeAnimationState.JustTookDamage &&
      component.TimeInState < TimeToActDamaged)
    {
      return;
    }

    if (input.IsSleeping)
    {
      component.SetState(
        PlayerEyeAnimationState.InBed,
        resetStateTimerEvenIfAlreadyInState: input.IsItemAnimating);
      return;
    }

    if (IsModeratelyDamaged(input))
    {
      component.SetState(PlayerEyeAnimationState.IsModeratelyDamaged);
      return;
    }

    if (input.IsTipsy)
    {
      component.SetState(PlayerEyeAnimationState.IsTipsy);
      return;
    }

    if (input.HasPoisonCondition)
    {
      component.SetState(PlayerEyeAnimationState.IsPoisoned);
      return;
    }

    component.SetState(
      input.HasStormExposure && !input.IsBehindBackWall
        ? PlayerEyeAnimationState.InStorm
        : PlayerEyeAnimationState.NormalBlinking);
  }

  private static void UpdateEyeFrame(
    PlayerEyeAnimationComponent component,
    PlayerEyeAnimationInput input)
  {
    int eyeFrameToShow = component.State switch
    {
      PlayerEyeAnimationState.NormalBlinking => GetNormalBlinkFrame(component.TimeInState),
      PlayerEyeAnimationState.InStorm => GetPeriodicClosedFrame(
        component.TimeInState,
        EnvironmentalBlinkPeriod,
        StormBlinkStart),
      PlayerEyeAnimationState.IsModeratelyDamaged =>
        GetPeriodicClosedFrame(
          component.TimeInState,
          EnvironmentalBlinkPeriod,
          ConditionBlinkStart),
      PlayerEyeAnimationState.IsTipsy => GetPeriodicClosedFrame(
        component.TimeInState,
        EnvironmentalBlinkPeriod,
        ConditionBlinkStart),
      PlayerEyeAnimationState.IsPoisoned => GetPeriodicClosedFrame(
        component.TimeInState,
        EnvironmentalBlinkPeriod,
        ConditionBlinkStart),
      PlayerEyeAnimationState.InBed => GetBedFrame(component, input),
      PlayerEyeAnimationState.IsBlind => 2,
      PlayerEyeAnimationState.JustTookDamage => 2,
      _ => 0,
    };

    component.SetEyeFrame(eyeFrameToShow);
  }

  private static int GetBedFrame(
    PlayerEyeAnimationComponent component,
    PlayerEyeAnimationInput input)
  {
    int eyeFrameToShow = IsModeratelyDamaged(input) ? 1 : 0;
    component.SetTimeInState(input.TimeSleeping);

    if (component.TimeInState >= 60)
    {
      eyeFrameToShow = component.TimeInState < 120 ? 1 : 2;
    }

    return eyeFrameToShow;
  }

  private static int GetNormalBlinkFrame(int timeInState)
  {
    int blinkPhase = timeInState % NormalBlinkPeriod - NormalBlinkStart;
    if (blinkPhase >= 4)
    {
      return 1;
    }

    if (blinkPhase < 2)
    {
      return blinkPhase >= 0 ? 1 : 0;
    }

    return 2;
  }

  private static int GetPeriodicClosedFrame(
    int timeInState,
    int period,
    int closedFrameStart)
  {
    return timeInState % period - closedFrameStart < 0 ? 1 : 2;
  }
}

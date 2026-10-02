using Terraria.Player.Animation;

PlayerEyeAnimationInput neutralInput = new(
  CurrentLife: 100,
  MaximumLife: 100,
  IsBlackout: false,
  IsBlind: false,
  IsSleeping: false,
  IsItemAnimating: false,
  TimeSleeping: 0,
  IsTipsy: false,
  IsPoisoned: false,
  IsVenomous: false,
  IsStarving: false,
  IsZoneSandstorm: false,
  IsZoneSnow: false,
  IsRaining: false,
  IsBehindBackWall: false);

VerifyNormalBlinking(neutralInput);
VerifyHurtHold(neutralInput);
VerifyStatePrecedence(neutralInput);
VerifySleepFrames(neutralInput);
VerifyStormSuppression(neutralInput);
VerifyLifecycleReset(neutralInput);

Console.WriteLine("PASS: player eye animation state, precedence, hurt hold, frame projection, and lifecycle reset");

static void VerifyNormalBlinking(PlayerEyeAnimationInput neutralInput)
{
  PlayerEyeAnimationComponent component = new();
  PlayerEyeAnimationSystem system = new();

  PlayerEyeAnimationSnapshot initial = system.Update(component, neutralInput);
  Require(initial.State == PlayerEyeAnimationState.NormalBlinking,
    "C17 must start in the normal blinking state.");
  Require(initial.EyeFrameToShow == 0 && initial.TimeInState == 1,
    "C17 must use the current timer for the frame before advancing it.");

  for (int index = 0; index < 233; index++)
  {
    system.Update(component, neutralInput);
  }

  PlayerEyeAnimationSnapshot blinkStart = system.Update(component, neutralInput);
  Require(blinkStart.EyeFrameToShow == 1,
    "C17 must begin the normal blink at timer phase 234.");

  system.Update(component, neutralInput);
  PlayerEyeAnimationSnapshot closed = system.Update(component, neutralInput);
  Require(closed.EyeFrameToShow == 2,
    "C17 must close the eye during the normal blink closed phase.");
}

static void VerifyHurtHold(PlayerEyeAnimationInput neutralInput)
{
  PlayerEyeAnimationComponent component = new();
  PlayerEyeAnimationSystem system = new();
  system.BlinkBecausePlayerGotHurt(component, new PlayerEyeHurtPresentationEvent());

  PlayerEyeAnimationSnapshot first = system.Update(component, neutralInput);
  Require(first.State == PlayerEyeAnimationState.JustTookDamage &&
      first.EyeFrameToShow == 2 && first.TimeInState == 1,
    "C17 hurt input must close the eyes and start the hurt timer.");

  for (int index = 0; index < 19; index++)
  {
    PlayerEyeAnimationSnapshot held = system.Update(component, neutralInput);
    Require(held.State == PlayerEyeAnimationState.JustTookDamage && held.EyeFrameToShow == 2,
      "C17 must hold the hurt state for twenty update ticks.");
  }

  PlayerEyeAnimationSnapshot released = system.Update(component, neutralInput);
  Require(released.State == PlayerEyeAnimationState.NormalBlinking,
    "C17 must leave the hurt state after the twenty-tick hold.");
}

static void VerifyStatePrecedence(PlayerEyeAnimationInput neutralInput)
{
  PlayerEyeAnimationSystem system = new();
  PlayerEyeAnimationComponent component = new();
  PlayerEyeAnimationSnapshot blind = system.Update(
    component,
    neutralInput with
    {
      IsBlackout = true,
      IsSleeping = true,
      IsTipsy = true,
      IsPoisoned = true,
    });
  Require(blind.State == PlayerEyeAnimationState.IsBlind && blind.EyeFrameToShow == 2,
    "C17 blindness must take precedence over sleep and status conditions.");

  PlayerEyeAnimationSnapshot damaged = system.Update(
    component,
    neutralInput with
    {
      CurrentLife = 20,
      MaximumLife = 100,
      IsTipsy = true,
      IsPoisoned = true,
    });
  Require(damaged.State == PlayerEyeAnimationState.IsModeratelyDamaged,
    "C17 moderate damage must take precedence over tipsy and poison.");

  PlayerEyeAnimationSnapshot tipsy = system.Update(
    component,
    neutralInput with { IsTipsy = true, IsPoisoned = true });
  Require(tipsy.State == PlayerEyeAnimationState.IsTipsy,
    "C17 tipsy must take precedence over the poison condition.");

  PlayerEyeAnimationSnapshot poisoned = system.Update(
    component,
    neutralInput with { IsPoisoned = true, IsZoneSandstorm = true });
  Require(poisoned.State == PlayerEyeAnimationState.IsPoisoned,
    "C17 poison must take precedence over storm exposure.");
}

static void VerifySleepFrames(PlayerEyeAnimationInput neutralInput)
{
  PlayerEyeAnimationComponent component = new();
  PlayerEyeAnimationSystem system = new();

  PlayerEyeAnimationSnapshot earlySleep = system.Update(
    component,
    neutralInput with { IsSleeping = true, TimeSleeping = 30 });
  Require(earlySleep.State == PlayerEyeAnimationState.InBed &&
      earlySleep.EyeFrameToShow == 0 && earlySleep.TimeInState == 31,
    "C17 early sleep must use an open frame and preserve the sleep timer assignment.");

  PlayerEyeAnimationSnapshot halfClosed = system.Update(
    component,
    neutralInput with { IsSleeping = true, TimeSleeping = 75 });
  Require(halfClosed.EyeFrameToShow == 1 && halfClosed.TimeInState == 76,
    "C17 sleep must use the half-closed frame from sixty through one hundred nineteen.");

  PlayerEyeAnimationSnapshot closed = system.Update(
    component,
    neutralInput with { IsSleeping = true, TimeSleeping = 120 });
  Require(closed.EyeFrameToShow == 2 && closed.TimeInState == 121,
    "C17 sleep must use the closed frame from one hundred twenty ticks.");
}

static void VerifyStormSuppression(PlayerEyeAnimationInput neutralInput)
{
  PlayerEyeAnimationSystem system = new();
  PlayerEyeAnimationSnapshot storm = system.Update(
    new PlayerEyeAnimationComponent(),
    neutralInput with { IsZoneSnow = true, IsRaining = true });
  Require(storm.State == PlayerEyeAnimationState.InStorm,
    "C17 snow and rain must enter the storm state.");

  PlayerEyeAnimationSnapshot behindWall = system.Update(
    new PlayerEyeAnimationComponent(),
    neutralInput with
    {
      IsZoneSandstorm = true,
      IsBehindBackWall = true,
    });
  Require(behindWall.State == PlayerEyeAnimationState.NormalBlinking,
    "C17 back-wall exposure must suppress the storm state.");
}

static void VerifyLifecycleReset(PlayerEyeAnimationInput neutralInput)
{
  PlayerEyeAnimationSystem system = new();
  PlayerEyeAnimationComponent component = new();

  system.BlinkBecausePlayerGotHurt(component, new PlayerEyeHurtPresentationEvent());
  _ = system.Update(component, neutralInput);
  Require(component.State == PlayerEyeAnimationState.JustTookDamage &&
      component.TimeInState == 1 && component.EyeFrameToShow == 2,
    "C17 lifecycle fixture must establish a short-lived hurt state before reset.");

  system.ResetForSpawn(component);
  Require(component.State == PlayerEyeAnimationState.NormalBlinking &&
      component.TimeInState == 0 && component.EyeFrameToShow == 0,
    "C17 spawn reset must clear hurt state, timer, and eye frame.");

  system.BlinkBecausePlayerGotHurt(component, new PlayerEyeHurtPresentationEvent());
  _ = system.Update(component, neutralInput);
  system.ResetForRemoval(component);
  Require(component.State == PlayerEyeAnimationState.NormalBlinking &&
      component.TimeInState == 0 && component.EyeFrameToShow == 0,
    "C17 removal reset must clear hurt state, timer, and eye frame.");
}

static void Require(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

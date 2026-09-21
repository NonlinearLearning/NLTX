namespace Terraria.SpatialMotionPhysics;

public sealed class DarknessHazardSystem
{
  private readonly DarknessHazardDefinition _definition;

  public DarknessHazardSystem(DarknessHazardDefinition definition)
  {
    _definition = definition ?? throw new ArgumentNullException(nameof(definition));
  }

  public DarknessHazardTickResult Tick(
    DarknessHazardStateComponent state,
    bool tooBright)
  {
    ArgumentNullException.ThrowIfNull(state);
    bool becameBright = tooBright && !state.LastFrameWasTooBright;
    bool becameDark = !tooBright && state.LastFrameWasTooBright;
    bool damageDue = false;

    if (tooBright)
    {
      state.DarknessHitTimer = 0;
      state.DarknessTimer = -1;
    }
    else
    {
      state.DarknessTimer = Math.Max(0, state.DarknessTimer) + 1;
      state.DarknessHitTimer++;
      if (state.DarknessHitTimer >= _definition.HitTimerMaxBeforeHit)
      {
        damageDue = true;
        state.DarknessHitTimer = 0;
      }
    }

    state.LastFrameWasTooBright = tooBright;
    return new DarknessHazardTickResult(damageDue, becameDark, becameBright);
  }
}

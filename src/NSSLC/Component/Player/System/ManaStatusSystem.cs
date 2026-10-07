namespace Terraria.Player;

public sealed class ManaStatusSystem
{
  private readonly PlayerManaActivityComponent _manaActivity;
  private readonly PlayerManaRegenModifierComponent _manaRegenModifiers;
  private readonly ManaActivityPolicyDefinition _policy;
  private readonly PlayerActivityQuery _activityQuery;

  public ManaStatusSystem(
    PlayerManaActivityComponent manaActivity,
    PlayerManaRegenModifierComponent manaRegenModifiers,
    ManaActivityPolicyDefinition policy,
    PlayerActivityQuery activityQuery)
  {
    ArgumentNullException.ThrowIfNull(manaActivity);
    ArgumentNullException.ThrowIfNull(manaRegenModifiers);
    ArgumentNullException.ThrowIfNull(policy);
    ArgumentNullException.ThrowIfNull(activityQuery);

    _manaActivity = manaActivity;
    _manaRegenModifiers = manaRegenModifiers;
    _policy = policy;
    _activityQuery = activityQuery;
  }

  public void RebuildManaStatus(in PlayerManaStatusInput input)
  {
    _manaActivity.ManaSick = input.ManaSickBuffActive;
    _manaActivity.ManaSickReduction = _policy.CalculateManaSickReduction(
      input.ManaSickBuffActive,
      input.ManaSickBuffTime);
    _manaRegenModifiers.ManaRegenBonus = input.ManaRegenBonus;
    _manaRegenModifiers.ManaRegenDelayBonus = input.ManaRegenDelayBonus;
  }

  public void AdvanceActivity(in PlayerActivityInput input)
  {
    PlayerActivityDecision decision = _activityQuery.Evaluate(input);
    if (decision.CountAfk)
    {
      _manaActivity.AfkCounter = IncrementCounter(
        _manaActivity.AfkCounter);
      _manaActivity.AfkCounterForKiting = IncrementCounter(
        _manaActivity.AfkCounterForKiting);
    }
    else if (decision.ResetAfkCounter)
    {
      _manaActivity.AfkCounter = 0;
      _manaActivity.AfkCounterForKiting = 0;
    }

    if (decision.ResetKitingCounter)
    {
      _manaActivity.AfkCounterForKiting = 0;
    }
  }

  public void ResetEffects()
  {
    _manaActivity.ResetEffects();
    _manaRegenModifiers.Reset();
  }

  public void ResetForLifecycle()
  {
    _manaActivity.ResetForLifecycle();
    _manaRegenModifiers.Reset();
  }

  private static int IncrementCounter(int value)
  {
    return value == int.MaxValue ? value : value + 1;
  }
}

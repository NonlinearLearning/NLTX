using Terraria.Relationships;

namespace Terraria.Combat;

public sealed class HitCooldownComponent
{
  private readonly Dictionary<EntityReference, int> _remainingTicksByTarget = new();

  public int GetRemaining(EntityReference target)
  {
    return _remainingTicksByTarget.GetValueOrDefault(target);
  }

  public void Arm(EntityReference target, int remainingTicks)
  {
    if (remainingTicks <= 0)
    {
      return;
    }

    _remainingTicksByTarget[target] = remainingTicks;
  }

  public void Clear(EntityReference target)
  {
    _remainingTicksByTarget.Remove(target);
  }

  public void Tick()
  {
    foreach (EntityReference target in _remainingTicksByTarget.Keys.ToArray())
    {
      int remainingTicks = _remainingTicksByTarget[target] - 1;
      if (remainingTicks <= 0)
      {
        _remainingTicksByTarget.Remove(target);
      }
      else
      {
        _remainingTicksByTarget[target] = remainingTicks;
      }
    }
  }
}

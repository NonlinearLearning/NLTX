using System;
using Terraria.Dome.Simulation.Items.Definitions;

namespace Terraria.Dome.Simulation.Player.Components;

public struct PlayerCooldownStateComponent
{
  public int ShadowDodgeTimer;
  public int AttackTicks;
  public int ItemAnimation;
  public int ItemAnimationMax;
  public int ItemTime;
  public int ItemTimeMax;
  public int ToolTime;
  public int RestorationDelay;
  public int EggnogDelay;
  public int MushroomDelay;

  public void Tick()
  {
    ShadowDodgeTimer = Decrement(ShadowDodgeTimer);
    AttackTicks = Decrement(AttackTicks);
    ItemAnimation = Decrement(ItemAnimation);
    ItemTime = Decrement(ItemTime);
    ToolTime = Decrement(ToolTime);
    RestorationDelay = Decrement(RestorationDelay);
    EggnogDelay = Decrement(EggnogDelay);
    MushroomDelay = Decrement(MushroomDelay);
  }

  public void BeginItemUse(int animationTicks, int cooldownTicks, int toolTicks)
  {
    if (animationTicks < 0 || cooldownTicks < 0 || toolTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(animationTicks));
    }

    ItemAnimation = animationTicks;
    ItemAnimationMax = animationTicks;
    ItemTime = cooldownTicks;
    ItemTimeMax = cooldownTicks;
    ToolTime = toolTicks;
  }

  public void ClearTransient()
  {
    ShadowDodgeTimer = 0;
    AttackTicks = 0;
    ItemAnimation = 0;
    ItemAnimationMax = 0;
    ItemTime = 0;
    ItemTimeMax = 0;
    ToolTime = 0;
    RestorationDelay = 0;
    EggnogDelay = 0;
    MushroomDelay = 0;
  }

  public void ApplyRecoveryDelay(ItemRecoveryDelayKind kind, int delayTicks)
  {
    if (!Enum.IsDefined(kind) || delayTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(delayTicks));
    }

    switch (kind)
    {
      case ItemRecoveryDelayKind.Restoration:
        RestorationDelay = delayTicks;
        break;
      case ItemRecoveryDelayKind.Eggnog:
        EggnogDelay = delayTicks;
        break;
      case ItemRecoveryDelayKind.Mushroom:
        MushroomDelay = delayTicks;
        break;
    }
  }

  private static int Decrement(int value)
  {
    return value > 0 ? value - 1 : 0;
  }
}

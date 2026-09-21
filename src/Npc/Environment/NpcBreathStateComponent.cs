using System;

namespace Terraria.Npc.Environment;

public sealed class NpcBreathStateComponent
{
  public NpcBreathStateComponent(
    int breath = NpcBreathRuleDefinition.BreathMax,
    int breathCounter = 0)
  {
    ValidateBreath(breath);
    ValidateBreathCounter(breathCounter);
    Breath = breath;
    BreathCounter = breathCounter;
  }

  public int Breath { get; private set; }

  public int BreathMax => NpcBreathRuleDefinition.BreathMax;

  public int BreathCounter { get; private set; }

  public bool ApplyEnvironmentStep(bool isSubmerged)
  {
    if (!isSubmerged)
    {
      Breath = Math.Min(
        BreathMax,
        Breath + NpcBreathRuleDefinition.BreathRecoveryPerEnvironmentStep);
      BreathCounter = 0;
      return false;
    }

    BreathCounter++;
    if (BreathCounter < NpcBreathRuleDefinition.DrowningCadence)
    {
      return false;
    }

    BreathCounter = 0;
    Breath = Math.Max(0, Breath - 1);
    return Breath == 0;
  }

  public void Reset()
  {
    Breath = BreathMax;
    BreathCounter = 0;
  }

  private static void ValidateBreath(int breath)
  {
    if (breath < 0 || breath > NpcBreathRuleDefinition.BreathMax)
    {
      throw new ArgumentOutOfRangeException(
        nameof(breath),
        "NPC breath must remain within the configured breath range.");
    }
  }

  private static void ValidateBreathCounter(int breathCounter)
  {
    if (breathCounter < 0 || breathCounter >= NpcBreathRuleDefinition.DrowningCadence)
    {
      throw new ArgumentOutOfRangeException(
        nameof(breathCounter),
        "NPC breath counter must remain before the next drowning decrement.");
    }
  }
}

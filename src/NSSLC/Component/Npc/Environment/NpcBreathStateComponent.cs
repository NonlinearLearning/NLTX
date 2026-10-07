using System;

namespace Terraria.Npc.Environment;

/// <summary>
/// 保存 NPC 呼吸资源和消耗计数。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：breath（第 6433 行）； breathMax（第 6435 行）； breathCounter（第 6437 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-p13-npc-town-progression-component-design.md。</para>
/// <para>依据位置：第 644 行。</para>
/// </remarks>
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

using System;

namespace Terraria.WorldSession.Calendar;

/// <summary>
/// 保存沙尘暴是否发生、剩余时间及当前与目标强度。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.GameContent.Events.Sandstorm。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent.Events/Sandstorm.cs。</para>
/// <para>
/// 主要源成员：Happening（第 14 行）； TimeLeft（第 16 行）； Severity（第 18 行）； IntendedSeverity（第 20 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/non-authoritative/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-non-authoritative-P02-world-environment-events-component-design.md。
/// </para>
/// <para>依据位置：第 1251 行。</para>
/// </remarks>
public sealed class SandstormStateComponent
{
  public const int MaximumDurationTicks = 86400;

  public SandstormStateComponent(
    bool happening = false,
    int timeLeft = 0,
    float severity = 0.0f,
    float intendedSeverity = 0.0f)
  {
    Happening = happening;
    TimeLeft = timeLeft;
    Severity = severity;
    IntendedSeverity = intendedSeverity;
    Validate();
  }

  public bool Happening;
  public int TimeLeft;
  public float Severity;
  public float IntendedSeverity;

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(TimeLeft);
    if (TimeLeft > MaximumDurationTicks)
    {
      throw new ArgumentOutOfRangeException(nameof(TimeLeft));
    }

    ValidateSeverity(Severity, nameof(Severity));
    ValidateSeverity(IntendedSeverity, nameof(IntendedSeverity));
  }

  private static void ValidateSeverity(float value, string parameterName)
  {
    if (!float.IsFinite(value) || value < 0.0f || value > 1.0f)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}

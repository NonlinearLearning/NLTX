using System;

namespace Terraria.WorldSession.Calendar;

/// <summary>
/// 保存史莱姆雨活动、击杀计数和警告计时。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Main。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Main.cs。</para>
/// <para>
/// 主要源成员：slimeRainTime（第 546 行）； slimeRain（第 548 行）； slimeRainKillCount（第 550 行）。
/// </para>
/// <para>重组说明：警告时刻与延迟在本模型中单独保存，未对应到上述原始成员。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/non-authoritative/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-non-authoritative-P02-world-environment-events-component-design.md。
/// </para>
/// <para>依据位置：第 154 行。</para>
/// </remarks>
public sealed class SlimeRainStateComponent
{
  public const int DefaultWarningDelay = 420;

  public SlimeRainStateComponent(
    bool active = false,
    double timeState = 0.0d,
    int killCount = 0,
    int warningTime = 0,
    int warningDelay = DefaultWarningDelay)
  {
    Active = active;
    TimeState = timeState;
    KillCount = killCount;
    WarningTime = warningTime;
    WarningDelay = warningDelay;
    Validate();
  }

  public bool Active;
  public double TimeState;
  public int KillCount;
  public int WarningTime;
  public int WarningDelay;

  public void Validate()
  {
    if (!double.IsFinite(TimeState))
    {
      throw new ArgumentOutOfRangeException(nameof(TimeState));
    }

    ArgumentOutOfRangeException.ThrowIfNegative(KillCount);
    ArgumentOutOfRangeException.ThrowIfNegative(WarningTime);
    ArgumentOutOfRangeException.ThrowIfNegative(WarningDelay);

    if (Active && TimeState <= 0.0d)
    {
      throw new ArgumentException(
        "An active Slime Rain must have a positive active time state.",
        nameof(TimeState));
    }
  }
}

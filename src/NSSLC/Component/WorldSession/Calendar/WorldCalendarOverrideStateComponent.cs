using System;

namespace Terraria.WorldSession.Calendar;

/// <summary>
/// 保存特殊月夜、日食、快进时间和日晷月晷冷却。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Main。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Main.cs。</para>
/// <para>
/// 主要源成员：bloodMoon（第 606 行）； pumpkinMoon（第 608 行）； snowMoon（第 610 行）； eclipse（第 624 行）；
/// fastForwardTimeToDawn（第 1209 行）； sundialCooldown（第 1211 行）； fastForwardTimeToDusk（第 1213 行）；
/// moondialCooldown（第 1215 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/non-authoritative/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-non-authoritative-P02-world-environment-events-component-design.md。
/// </para>
/// <para>依据位置：第 21 行。</para>
/// </remarks>
public sealed class WorldCalendarOverrideStateComponent
{
  public WorldCalendarOverrideStateComponent(
    bool bloodMoon = false,
    bool eclipse = false,
    bool pumpkinMoon = false,
    bool snowMoon = false,
    bool fastForwardTimeToDawn = false,
    bool fastForwardTimeToDusk = false,
    int sundialCooldown = 0,
    int moondialCooldown = 0)
  {
    BloodMoon = bloodMoon;
    Eclipse = eclipse;
    PumpkinMoon = pumpkinMoon;
    SnowMoon = snowMoon;
    FastForwardTimeToDawn = fastForwardTimeToDawn;
    FastForwardTimeToDusk = fastForwardTimeToDusk;
    SundialCooldown = sundialCooldown;
    MoondialCooldown = moondialCooldown;
    Validate();
  }

  public bool BloodMoon;
  public bool Eclipse;
  public bool PumpkinMoon;
  public bool SnowMoon;
  public bool FastForwardTimeToDawn;
  public bool FastForwardTimeToDusk;
  public int SundialCooldown;
  public int MoondialCooldown;

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(SundialCooldown);
    ArgumentOutOfRangeException.ThrowIfNegative(MoondialCooldown);

    if (PumpkinMoon && SnowMoon)
    {
      throw new ArgumentException(
        "Pumpkin Moon and Snow Moon cannot be active together.",
        nameof(SnowMoon));
    }

    if (BloodMoon && (PumpkinMoon || SnowMoon))
    {
      throw new ArgumentException(
        "Blood Moon cannot be active with a seasonal moon.",
        nameof(BloodMoon));
    }
  }
}

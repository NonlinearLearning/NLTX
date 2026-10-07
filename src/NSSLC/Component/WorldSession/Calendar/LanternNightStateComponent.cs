using System;

namespace Terraria.WorldSession.Calendar;

/// <summary>
/// 保存世界灯笼夜的触发、冷却和上一轮活动状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.GameContent.Events.LanternNight。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent.Events/LanternNight.cs。</para>
/// <para>
/// 主要源成员：ManualLanterns（第 8 行）； GenuineLanterns（第 10 行）； NextNightIsLanternNight（第 12 行）；
/// LanternNightsOnCooldown（第 14 行）； _wasLanternNight（第 16 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/non-authoritative/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-non-authoritative-P02-world-environment-events-component-design.md。
/// </para>
/// <para>依据位置：第 1115 行。</para>
/// </remarks>
public sealed class LanternNightStateComponent
{
  public LanternNightStateComponent(
    bool manualLanterns = false,
    bool genuineLanterns = false,
    bool nextNightIsLanternNight = false,
    int lanternNightsOnCooldown = 0,
    bool wasLanternNight = false)
  {
    ManualLanterns = manualLanterns;
    GenuineLanterns = genuineLanterns;
    NextNightIsLanternNight = nextNightIsLanternNight;
    LanternNightsOnCooldown = lanternNightsOnCooldown;
    WasLanternNight = wasLanternNight;
    Validate();
  }

  public bool ManualLanterns;
  public bool GenuineLanterns;
  public bool NextNightIsLanternNight;
  public int LanternNightsOnCooldown;
  public bool WasLanternNight;

  public bool IsUp => ManualLanterns || GenuineLanterns;

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(LanternNightsOnCooldown);
  }
}

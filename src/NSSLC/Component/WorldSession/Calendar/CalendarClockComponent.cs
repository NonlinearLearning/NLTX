using System;

namespace Terraria.WorldSession.Calendar;

/// <summary>
/// 保存世界时间、昼夜、月相和时间推进速率。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Main。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Main.cs。</para>
/// <para>主要源成员：dayRate（第 402 行）； dayTime（第 594 行）； time（第 596 行）； moonPhase（第 600 行）。</para>
/// <para>重组说明：TickNumber、暂停状态及昼夜时长是时钟模型新增或显式化的状态。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/non-authoritative/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-non-authoritative-P02-world-environment-events-component-design.md。
/// </para>
/// <para>依据位置：第 19 行。</para>
/// </remarks>
public sealed class CalendarClockComponent
{
  public const int DefaultDayLengthTicks = 54000;
  public const int DefaultNightLengthTicks = 32400;

  public CalendarClockComponent(
    long tickNumber,
    double timeOfDay,
    bool isDayTime = true,
    int dayRate = 1,
    int dayLengthTicks = DefaultDayLengthTicks,
    int nightLengthTicks = DefaultNightLengthTicks,
    byte moonPhase = 0,
    bool isPaused = false)
  {
    TickNumber = tickNumber;
    TimeOfDay = timeOfDay;
    IsDayTime = isDayTime;
    DayRate = dayRate;
    DayLengthTicks = dayLengthTicks;
    NightLengthTicks = nightLengthTicks;
    MoonPhase = moonPhase;
    IsPaused = isPaused;
    Validate();
  }

  public long TickNumber;
  public double TimeOfDay;
  public bool IsDayTime;
  public int DayRate;
  public int DayLengthTicks;
  public int NightLengthTicks;
  public byte MoonPhase;
  public bool IsPaused;

  public int CurrentCycleLength => IsDayTime ? DayLengthTicks : NightLengthTicks;

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(TickNumber);
    ArgumentOutOfRangeException.ThrowIfNegative(DayRate);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(DayLengthTicks);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(NightLengthTicks);

    if (!double.IsFinite(TimeOfDay) ||
        TimeOfDay < 0.0d ||
        TimeOfDay >= CurrentCycleLength)
    {
      throw new ArgumentOutOfRangeException(nameof(TimeOfDay));
    }

    if (MoonPhase > 7)
    {
      throw new ArgumentOutOfRangeException(nameof(MoonPhase));
    }
  }
}

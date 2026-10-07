namespace Terraria.Player;

/// <summary>
/// 保存玩家指南针、手表、深度计和环境仪表能力。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：accCompass（第 1974 行）； accWatch（第 1976 行）； accDepthMeter（第 1980 行）； accWeatherRadio（第
/// 1984 行）； accCalendar（第 1988 行）； accStopwatch（第 1998 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P10-player-input-control-component-design.md。</para>
/// <para>依据位置：第 398 行。</para>
/// </remarks>
public struct PlayerNavigationInstrumentComponent
{
  public int CompassLevel;
  public int WatchLevel;
  public int DepthMeterLevel;
  public bool WeatherRadioEnabled;
  public bool CalendarEnabled;
  public bool StopwatchEnabled;
}

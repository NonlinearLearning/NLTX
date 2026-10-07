namespace Terraria.Player;

/// <summary>
/// 保存玩家侦测仪表的能力和计数。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：accFishFinder（第 1982 行）； accJarOfSouls（第 1986 行）； accThirdEye（第 1992 行）；
/// accThirdEyeCounter（第 1994 行）； accOreFinder（第 2000 行）； accCritterGuide（第 2002 行）；
/// accDreamCatcher（第 2006 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P10-player-input-control-component-design.md。</para>
/// <para>依据位置：第 430 行。</para>
/// </remarks>
public struct PlayerDetectionInstrumentComponent
{
  public bool FishFinderEnabled;
  public bool JarOfSoulsEnabled;
  public bool ThirdEyeEnabled;
  public byte ThirdEyeCounter;
  public bool OreFinderEnabled;
  public bool CritterGuideEnabled;
  public bool DreamCatcherEnabled;
}

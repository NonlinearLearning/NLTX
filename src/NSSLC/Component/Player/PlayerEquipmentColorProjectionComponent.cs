namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-1326..P09-1342, P09-1346..P09-1348
// crossSubsystemOwner: color calculation, item metadata, and renderer consumption remain integration-review
/// <summary>
/// 保存玩家可见装备的染色投影编号。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：cHead（第 2298 行）； cBody（第 2300 行）； cLegs（第 2302 行）； cHandOn（第 2304 行）； cHandOff（第 2306
/// 行）； cBack（第 2308 行）； cFront（第 2310 行）； cShoe（第 2312 行）； cWaist（第 2314 行）； cShield（第 2316 行）；
/// cNeck（第 2318 行）； cFace（第 2320 行）； cFaceHead（第 2322 行）； cFaceFlower（第 2324 行）； cFaceMask（第 2326
/// 行）； cBalloon（第 2328 行）； cBalloonFront（第 2330 行）； cBackpack（第 2338 行）； cTail（第 2340 行）；
/// cShieldFallback（第 2342 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P09-player-inventory-equipment-component-design.md。
/// </para>
/// <para>依据位置：第 14 行。</para>
/// </remarks>
public sealed class PlayerEquipmentColorProjectionComponent
{
  public int CHead { get; internal set; }

  public int CBody { get; internal set; }

  public int CLegs { get; internal set; }

  public int CHandOn { get; internal set; }

  public int CHandOff { get; internal set; }

  public int CBack { get; internal set; }

  public int CFront { get; internal set; }

  public int CShoe { get; internal set; }

  public int CWaist { get; internal set; }

  public int CShield { get; internal set; }

  public int CNeck { get; internal set; }

  public int CFace { get; internal set; }

  public int CFaceHead { get; internal set; }

  public int CFaceFlower { get; internal set; }

  public int CFaceMask { get; internal set; }

  public int CBalloon { get; internal set; }

  public int CBalloonFront { get; internal set; }

  public int CBackpack { get; internal set; }

  public int CTail { get; internal set; }

  public int CShieldFallback { get; internal set; } = -1;
}

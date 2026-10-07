namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-783..P09-803
// crossSubsystemOwner: visible-slot calculation and renderer consumption remain integration-review
/// <summary>
/// 保存玩家各身体部位当前可见装备的编号。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：head（第 1164 行）； body（第 1166 行）； legs（第 1168 行）； coat（第 1170 行）； handon（第 1172 行）；
/// handoff（第 1174 行）； back（第 1176 行）； front（第 1178 行）； shoe（第 1180 行）； waist（第 1182 行）； shield（第
/// 1184 行）； neck（第 1186 行）； face（第 1188 行）； balloon（第 1190 行）； backpack（第 1192 行）； tail（第 1194
/// 行）； faceHead（第 1196 行）； faceFlower（第 1198 行）； faceMask（第 1200 行）； balloonFront（第 1202 行）；
/// beard（第 1204 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P09-player-inventory-equipment-component-design.md。
/// </para>
/// <para>依据位置：第 14 行。</para>
/// </remarks>
public sealed class PlayerVisibleEquipmentSelectionComponent
{
  public int Head { get; internal set; } = -1;

  public int Body { get; internal set; } = -1;

  public int Legs { get; internal set; } = -1;

  public int Coat { get; internal set; } = -1;

  public sbyte HandOn { get; internal set; } = -1;

  public sbyte HandOff { get; internal set; } = -1;

  public sbyte Back { get; internal set; } = -1;

  public sbyte Front { get; internal set; } = -1;

  public sbyte Shoe { get; internal set; } = -1;

  public sbyte Waist { get; internal set; } = -1;

  public sbyte Shield { get; internal set; } = -1;

  public sbyte Neck { get; internal set; } = -1;

  public sbyte Face { get; internal set; } = -1;

  public sbyte Balloon { get; internal set; } = -1;

  public sbyte Backpack { get; internal set; } = -1;

  public sbyte Tail { get; internal set; } = -1;

  public sbyte FaceHead { get; internal set; } = -1;

  public sbyte FaceFlower { get; internal set; } = -1;

  public sbyte FaceMask { get; internal set; } = -1;

  public sbyte BalloonFront { get; internal set; } = -1;

  public sbyte Beard { get; internal set; } = -1;
}

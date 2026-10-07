namespace Terraria.WorldInteraction.TileEntities;

/// <summary>
/// 保存逻辑感应器的检查种类、开关状态和计数数据。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.GameContent.Tile_Entities.TELogicSensor。</para>
/// <para>
/// 原始文件：D:/TRbackup/Version4/Terraria.GameContent.Tile_Entities/TELogicSensor.cs。
/// </para>
/// <para>主要源成员：logicCheck（第 33 行）； On（第 35 行）； CountedData（第 37 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/non-authoritative/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-non-authoritative-P04-world-tiles-storage-component-design.md。
/// </para>
/// <para>依据位置：第 558 行。</para>
/// </remarks>
public sealed class LogicSensorComponent
{
  public LogicCheckType CheckType { get; internal set; }
  public bool IsOn { get; internal set; }
  public int CountedData { get; internal set; }
}

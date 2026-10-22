using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存城镇住房扫描的游标、房间检测结果和候选位置。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldGen。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/WorldGen.cs。</para>
/// <para>
/// 主要源成员：prioritizedTownNPCType（第 4193 行）； maxRoomTiles（第 4213 行）； maxRoomSize（第 4215 行）；
/// roomTiles（第 4217 行）； roomX1（第 4221 行）； roomX2（第 4223 行）； roomY1（第 4225 行）； roomY2（第 4227 行）；
/// canSpawn（第 4229 行）； houseTile（第 4231 行）； bestX（第 4233 行）； bestY（第 4235 行）； roomTorch（第 4239
/// 行）； roomDoor（第 4241 行）； roomChair（第 4243 行）； roomTable（第 4245 行）； roomHasStinkbug（第 4247 行）；
/// roomHasEchoStinkbug（第 4249 行）； LastFoundHouse（第 4261 行）； sharedRoomX（第 4265 行）。
/// </para>
/// <para>重组说明：GenerationId、ScanRevision 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P16-world-lifecycle-housing-metrics-component-design.md。
/// </para>
/// <para>依据位置：第 244 行。</para>
/// <para>
/// 用途：住房扫描流程记录房间范围、家具条件、候选评分和失败原因，后续分配流程读取扫描结果。
/// </para>
/// <para>
/// 当前实现：GenerationId 构造时要求非负，其余数据在初始化时保存。可空坐标与布尔值允许表达尚未提供的检测结果；房间扫描、规则判定和分配由外部流程完成。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C183，L2（通用可选玩法）。分级理由：住房扫描、分配和注册是城镇居民玩法，不是世界加载本身。
/// </para>
/// </remarks>
public sealed class HousingScanStateComponent
{
  public HousingScanStateComponent(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
  }

  /// <summary>
  /// 所属世界生成会话编号。
  /// </summary>
  public long GenerationId { get; }

  /// <summary>
  /// 住房扫描结果版本。
  /// </summary>
  public ulong ScanRevision { get; init; }

  /// <summary>
  /// 住房扫描的进度游标。
  /// </summary>
  public long Cursor { get; init; }

  /// <summary>
  /// 优先安排住房的城镇 NPC 类型，可为空。
  /// </summary>
  public int? PrioritizedTownNpcType { get; init; }

  /// <summary>
  /// 当前记录的房间方块数量。
  /// </summary>
  public int RoomTiles { get; init; }

  /// <summary>
  /// 房间方块数量限制输入。
  /// </summary>
  public int MaxRoomTiles { get; init; }

  /// <summary>
  /// 房间尺寸限制输入。
  /// </summary>
  public int MaxRoomSize { get; init; }

  /// <summary>
  /// 房间横向第一边界，可为空。
  /// </summary>
  public int? RoomX1 { get; init; }

  /// <summary>
  /// 房间横向第二边界，可为空。
  /// </summary>
  public int? RoomX2 { get; init; }

  /// <summary>
  /// 房间纵向第一边界，可为空。
  /// </summary>
  public int? RoomY1 { get; init; }

  /// <summary>
  /// 房间纵向第二边界，可为空。
  /// </summary>
  public int? RoomY2 { get; init; }

  /// <summary>
  /// 最佳候选位置横坐标，可为空。
  /// </summary>
  public int? BestX { get; init; }

  /// <summary>
  /// 最佳候选位置纵坐标，可为空。
  /// </summary>
  public int? BestY { get; init; }

  /// <summary>
  /// 最佳候选评分，可为空。
  /// </summary>
  public int? HighScore { get; init; }

  /// <summary>
  /// 此扫描结果是否允许生成或入住，可为空。
  /// </summary>
  public bool? CanSpawn { get; init; }

  /// <summary>
  /// 住房方块条件的检测结果，可为空。
  /// </summary>
  public bool? HouseTile { get; init; }

  /// <summary>
  /// 照明条件的检测结果，可为空。
  /// </summary>
  public bool? RoomTorch { get; init; }

  /// <summary>
  /// 入口条件的检测结果，可为空。
  /// </summary>
  public bool? RoomDoor { get; init; }

  /// <summary>
  /// 椅子类家具条件的检测结果，可为空。
  /// </summary>
  public bool? RoomChair { get; init; }

  /// <summary>
  /// 桌子类家具条件的检测结果，可为空。
  /// </summary>
  public bool? RoomTable { get; init; }

  /// <summary>
  /// 房间是否检测到臭虫，可为空。
  /// </summary>
  public bool? RoomHasStinkbug { get; init; }

  /// <summary>
  /// 房间是否检测到回声臭虫，可为空。
  /// </summary>
  public bool? RoomHasEchoStinkbug { get; init; }

  /// <summary>
  /// 是否正在尝试另一个候选位置。
  /// </summary>
  public bool CurrentlyTryingAlternateSpot { get; init; }

  /// <summary>
  /// 共享房间相关横坐标，可为空。
  /// </summary>
  public int? SharedRoomX { get; init; }

  /// <summary>
  /// 最近找到的住房方块位置，可为空。
  /// </summary>
  public TilePosition? LastFoundHouse { get; init; }

  /// <summary>
  /// 住房扫描失败原因，可为空。
  /// </summary>
  public string? FailureReason { get; init; }
}

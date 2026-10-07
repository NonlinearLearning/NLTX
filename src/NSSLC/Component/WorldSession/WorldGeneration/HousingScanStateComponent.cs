using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存住房扫描的边界、材料条件、候选位置和结果。
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

  public long GenerationId { get; }

  public ulong ScanRevision { get; init; }

  public long Cursor { get; init; }

  public int? PrioritizedTownNpcType { get; init; }

  public int RoomTiles { get; init; }

  public int MaxRoomTiles { get; init; }

  public int MaxRoomSize { get; init; }

  public int? RoomX1 { get; init; }

  public int? RoomX2 { get; init; }

  public int? RoomY1 { get; init; }

  public int? RoomY2 { get; init; }

  public int? BestX { get; init; }

  public int? BestY { get; init; }

  public int? HighScore { get; init; }

  public bool? CanSpawn { get; init; }

  public bool? HouseTile { get; init; }

  public bool? RoomTorch { get; init; }

  public bool? RoomDoor { get; init; }

  public bool? RoomChair { get; init; }

  public bool? RoomTable { get; init; }

  public bool? RoomHasStinkbug { get; init; }

  public bool? RoomHasEchoStinkbug { get; init; }

  public bool CurrentlyTryingAlternateSpot { get; init; }

  public int? SharedRoomX { get; init; }

  public TilePosition? LastFoundHouse { get; init; }

  public string? FailureReason { get; init; }
}

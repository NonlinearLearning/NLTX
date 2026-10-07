using System.Collections.Generic;
using Terraria.WorldInteraction.Tiles;

namespace Terraria.Physics;

/// <summary>
/// 保存自动开关门追踪和按速度开门的有效窗口。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.GameContent.DoorOpeningHelper。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent/DoorOpeningHelper.cs。</para>
/// <para>
/// 主要源成员：_ongoingOpenDoors（第 89 行）； _timeWeCanOpenDoorsUsingVelocityAlone（第 91 行）。
/// </para>
/// <para>重组说明：门记录按方块锚点重组，最近方向和时刻用于显式追踪。</para>
/// <para>拆分依据目录：docs/component-decomposition/baseline/。</para>
/// <para>拆分依据文件：空间移动碰撞与液体组件字段设计.md。</para>
/// <para>依据位置：第 359 行。</para>
/// </remarks>
public sealed class DoorTraversalComponent
{
  public List<TileCoordinate> TrackedDoorAnchors = new();
  public int LastTraversalDirection = 1;
  public long? LastTraversalAtTick;
  public int VelocityOnlyOpenTicksRemaining;
  public bool HasVelocityOnlyOpeningWindow => VelocityOnlyOpenTicksRemaining > 0;
  public bool IsTrackingDoor => TrackedDoorAnchors.Count != 0;
}

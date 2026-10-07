using System.Numerics;

namespace Terraria.Player;

/// <summary>
/// 保存玩家本帧累计的瞬时位移。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：instantMovementAccumulatedThisFrame（第 1972 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P10-player-input-control-component-design.md。</para>
/// <para>依据位置：第 356 行。</para>
/// </remarks>
public struct PlayerInstantMovementAccumulatorComponent
{
  public Vector2 AccumulatedMovementThisFrame;
}

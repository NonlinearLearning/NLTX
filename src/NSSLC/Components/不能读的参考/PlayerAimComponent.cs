using System.Numerics;

namespace Terraria.Player;

/// <summary>
/// 保存玩家二维瞄准方向，供武器使用和射弹发射读取。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 Player.ItemCheck 中的武器瞄准与发射方向流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>重组说明：二维瞄准向量是 NLTX 新增的显式输入，不对应 Entity.direction 的水平整数朝向。</para>
/// <para>拆分依据目录：docs/plans/component-decomposition/。</para>
/// <para>拆分依据文件：2026-09-04-root-src-component-split-design.md。</para>
/// <para>依据位置：第 60 行。</para>
/// <para>
/// 用途：把玩家瞄准输入显式保存为二维向量，例如指向鼠标所在方向后由武器流程决定射弹速度。
/// </para>
/// <para>
/// 当前实现：构造时直接保存 Direction，未做归一化或零向量处理；方向长度和有效性由输入或发射流程约定。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C107，L2（通用可选玩法）。分级理由：这些类型当前未被消费。现模拟路径直接计算瞄准方向，丢弃也不要求独立瞬时标记。
/// </para>
/// </remarks>
public struct PlayerAimComponent
{
  public PlayerAimComponent(Vector2 direction)
  {
    Direction = direction;
  }

  /// <summary>
  /// 玩家当前二维瞄准向量；本组件不保证它是单位向量。
  /// </summary>
  public Vector2 Direction;
}

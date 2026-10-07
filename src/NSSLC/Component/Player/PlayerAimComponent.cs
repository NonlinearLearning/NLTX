using System.Numerics;

namespace Terraria.Player;

/// <summary>
/// 保存玩家二维瞄准方向。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 Player.ItemCheck 中的武器瞄准与发射方向流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>重组说明：二维瞄准向量是 NLTX 新增的显式输入，不对应 Entity.direction 的水平整数朝向。</para>
/// <para>拆分依据目录：docs/plans/component-decomposition/。</para>
/// <para>拆分依据文件：2026-09-04-root-src-component-split-design.md。</para>
/// <para>依据位置：第 60 行。</para>
/// </remarks>
public struct PlayerAimComponent
{
  public PlayerAimComponent(Vector2 direction)
  {
    Direction = direction;
  }

  public Vector2 Direction;
}

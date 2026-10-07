namespace EntityEcs.Components;

/// <summary>
/// 保存实体水平方向。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Entity。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Entity.cs。</para>
/// <para>主要源成员：direction（第 20 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/non-authoritative/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-non-authoritative-P06-entity-lifecycle-attribution-component-design.md。
/// </para>
/// <para>依据位置：第 241 行。</para>
/// </remarks>
public struct DirectionComponent
{
  public DirectionComponent(int horizontal)
  {
    Horizontal = horizontal;
  }

  public int Horizontal;
}

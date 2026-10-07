namespace EntityEcs.Components;

/// <summary>
/// 保存实体的二维速度。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Entity。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Entity.cs。</para>
/// <para>主要源成员：velocity（第 12 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/non-authoritative/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-non-authoritative-P06-entity-lifecycle-attribution-component-design.md。
/// </para>
/// <para>依据位置：第 237 行。</para>
/// </remarks>
public struct VelocityComponent
{
  public VelocityComponent(float x, float y)
  {
    X = x;
    Y = y;
  }

  public float X;
  public float Y;
}

namespace Terraria.Projectile;

/// <summary>
/// 保存射弹对友方和敌方的伤害阵营标记。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Projectile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>主要源成员：hostile（第 148 行）； friendly（第 154 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P15-projectile-component-design.md。</para>
/// <para>依据位置：第 16 行。</para>
/// </remarks>
public struct ProjectileDispositionStateComponent
{
  public ProjectileDispositionStateComponent(
    bool friendly = false,
    bool hostile = false)
  {
    Friendly = friendly;
    Hostile = hostile;
  }

  public bool Friendly;

  public bool Hostile;
}

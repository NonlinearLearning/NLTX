namespace Terraria.Projectile;

/// <summary>
/// 保存射弹效果或音效的延迟计时。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Projectile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>主要源成员：soundDelay（第 140 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P15-projectile-component-design.md。</para>
/// <para>依据位置：第 17 行。</para>
/// </remarks>
public struct ProjectileEffectCooldownStateComponent
{
  public ProjectileEffectCooldownStateComponent(int soundDelay = 0)
  {
    SoundDelay = soundDelay;
  }

  /// <summary>
  /// Version4 AI uses negative values as sentinels and countdown states.
  /// </summary>
  public int SoundDelay;

  public bool IsDelayed => SoundDelay > 0;
}

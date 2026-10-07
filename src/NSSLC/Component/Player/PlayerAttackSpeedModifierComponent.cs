namespace Terraria.Player;

/// <summary>
/// 保存玩家近战和召唤武器速度修正。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：meleeSpeed（第 1879 行）； summonerWeaponSpeedBonus（第 1881 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 1048 行。</para>
/// </remarks>
public sealed class PlayerAttackSpeedModifierComponent
{
  public float MeleeSpeed { get; internal set; } = 1f;

  public float SummonerWeaponSpeedBonus { get; internal set; }

  internal void ResetEffects()
  {
    MeleeSpeed = 1f;
    SummonerWeaponSpeedBonus = 0f;
  }
}

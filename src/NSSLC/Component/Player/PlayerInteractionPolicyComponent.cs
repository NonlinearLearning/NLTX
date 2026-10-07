namespace Terraria.Player;

/// <summary>
/// 保存玩家拾取、小动物和自然物交互策略。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：preventAllItemPickups（第 947 行）； dontHurtCritters（第 949 行）； hasLucyTheAxe（第 951 行）；
/// dontHurtNature（第 953 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 479 行。</para>
/// </remarks>
public sealed class PlayerInteractionPolicyComponent
{
  public bool PreventAllItemPickups { get; internal set; }

  public bool DontHurtCritters { get; internal set; }

  public bool HasLucyTheAxe { get; internal set; }

  public bool DontHurtNature { get; internal set; }

  internal void ResetEffects()
  {
    PreventAllItemPickups = false;
    DontHurtCritters = false;
    HasLucyTheAxe = false;
    DontHurtNature = false;
  }
}

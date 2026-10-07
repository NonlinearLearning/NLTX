namespace Terraria.Player;

/// <summary>
/// 保存玩家箭袋、熔岩饰品和远程武器效果。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：magicQuiver（第 1401 行）； magmaStone（第 1403 行）； lavaRose（第 1405 行）； hasMoltenQuiver（第 1407
/// 行）； phantasmTime（第 1409 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 655 行。</para>
/// </remarks>
public sealed class PlayerRangedAccessoryCapabilityComponent
{
  public PlayerRangedAccessoryCapabilityComponent(bool magicQuiver = false)
  {
    MagicQuiver = magicQuiver;
  }

  public bool MagicQuiver { get; internal set; }

  public bool MagmaStone { get; internal set; }

  public bool LavaRose { get; internal set; }

  public bool HasMoltenQuiver { get; internal set; }

  public int PhantasmTime { get; internal set; }

  internal void ResetEffects()
  {
    MagicQuiver = false;
    MagmaStone = false;
    LavaRose = false;
    HasMoltenQuiver = false;
  }

  internal void ResetForLifecycle()
  {
    MagicQuiver = false;
    MagmaStone = false;
    LavaRose = false;
    HasMoltenQuiver = false;
    PhantasmTime = 0;
  }
}

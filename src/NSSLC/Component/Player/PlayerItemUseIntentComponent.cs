namespace Terraria.Player;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for ItemCheck, Tile and protocol integration
/// <summary>
/// 保存玩家使用物品和使用方块的输入意图。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：controlUseItem（第 1229 行）； controlUseTile（第 1231 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P10-player-input-control-component-design.md。</para>
/// <para>依据位置：第 280 行。</para>
/// </remarks>
public struct PlayerItemUseIntentComponent
{
  public bool ControlUseItem;
  public bool ControlUseTile;
}

namespace Terraria.Player;

// Owns short-lived player interaction facts that are local to the Player entity.
// Creative, chat, mount, grapple and combat objects remain external boundaries.
/// <summary>
/// 保存玩家表情、洞穴探险和建筑工具开关状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：emoteTime（第 484 行）； spelunkerTimer（第 493 行）； builderAccStatus（第 495 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P04-player-lifecycle-interaction-component-design.md。
/// </para>
/// <para>依据位置：第 249 行。</para>
/// </remarks>
public sealed class PlayerRuntimeInteractionComponent
{
  public int EmoteRemainingTicks { get; set; }

  public byte SpelunkerRemainingTicks { get; set; }

  // The array is component-owned state; read adapters must return a defensive copy.
  public int[] BuilderToggleStatuses { get; } =
    new int[PlayerBuilderInteractionCatalog.Count];
}

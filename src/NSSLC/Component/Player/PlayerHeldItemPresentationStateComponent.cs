using System.Numerics;

namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-713, P09-714
// crossSubsystemOwner: held projectile link remains PlayerUse/Projectile integration-review
/// <summary>
/// 保存玩家手持物品的旋转和显示位置。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：itemRotation（第 1023 行）； itemLocation（第 1025 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P09-player-inventory-equipment-component-design.md。
/// </para>
/// <para>依据位置：第 14 行。</para>
/// </remarks>
public sealed class PlayerHeldItemPresentationStateComponent
{
  public float ItemRotation { get; internal set; }

  public Vector2 ItemLocation { get; internal set; }
}

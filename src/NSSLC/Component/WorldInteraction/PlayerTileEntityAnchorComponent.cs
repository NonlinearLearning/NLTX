using Terraria.Relationships;
using Terraria.WorldInteraction.Tiles;

namespace Terraria.WorldInteraction;

// Stores the player's relation to an external TileEntity by identity and tile.
// Registry lookup, validity checks and network projection remain adapters.
/// <summary>
/// 保存玩家正在关联的方块实体锚点。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：tileEntityAnchor（第 2292 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P04-player-lifecycle-interaction-component-design.md。
/// </para>
/// <para>依据位置：第 492 行。</para>
/// </remarks>
public sealed class PlayerTileEntityAnchorComponent
{
  public EntityReference? AnchorEntity { get; set; }

  public TileCoordinate? AnchorCoordinate { get; set; }

  public bool HasAnchor =>
    AnchorEntity.HasValue &&
    !AnchorEntity.Value.IsEmpty &&
    AnchorCoordinate.HasValue;
}

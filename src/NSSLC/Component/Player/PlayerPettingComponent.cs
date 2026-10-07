using System.Numerics;

namespace Terraria.Player;

// Stores the player's petting phase and target compatibility facts.
// NPC, Projectile and Mount registry validation remains outside the component.
/// <summary>
/// 保存玩家抚摸目标、位置偏移和目标种类信息。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.GameContent.PlayerPettingInfo。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent/PlayerPettingInfo.cs。</para>
/// <para>主要源成员：offsetFromPet（第 17 行）； isPetSmall（第 19 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P04-player-lifecycle-interaction-component-design.md。
/// </para>
/// <para>依据位置：第 488 行。</para>
/// </remarks>
public sealed class PlayerPettingComponent
{
  public int NpcSlot { get; set; } = -1;

  public int ExpectedNpcType { get; set; } = -1;

  public int ProjectileSlot { get; set; } = -1;

  public int ExpectedProjectileType { get; set; } = -1;

  public int MountId { get; set; } = -1;

  public bool IsMountTarget { get; set; }

  public Vector2 OffsetFromPet { get; set; }

  public bool IsPetSmall { get; set; }
}

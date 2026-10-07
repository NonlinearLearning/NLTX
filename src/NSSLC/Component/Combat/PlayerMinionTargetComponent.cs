using System.Numerics;

namespace Terraria.Combat;

// Stores minion rest and attack target compatibility facts.
// NPC registry validation and type 99/115 network projection remain external.
/// <summary>
/// 保存玩家指定的召唤物停驻点和攻击目标。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：MinionRestTargetPoint（第 2386 行）； MinionAttackTargetNPC（第 2388 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P04-player-lifecycle-interaction-component-design.md。
/// </para>
/// <para>依据位置：第 561 行。</para>
/// </remarks>
public sealed class PlayerMinionTargetComponent
{
  public Vector2 RestTargetPoint { get; set; }

  // This is a legacy NPC slot, not a stable ECS entity identity.
  public int AttackTarget { get; set; } = -1;

  public bool HasRestTarget => RestTargetPoint != Vector2.Zero;
}

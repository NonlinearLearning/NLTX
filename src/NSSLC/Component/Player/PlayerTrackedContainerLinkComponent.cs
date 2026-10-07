namespace Terraria.Player;

// Stores compatibility identity facts for the two external container projectiles.
// The component never holds a Projectile instance or performs registry recovery.
/// <summary>
/// 保存玩家追踪的飞猪存钱罐和虚空容器射弹关系。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：piggyBankProjTracker（第 2266 行）； voidLensChest（第 2268 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P04-player-lifecycle-interaction-component-design.md。
/// </para>
/// <para>依据位置：第 485 行。</para>
/// </remarks>
public sealed class PlayerTrackedContainerLinkComponent
{
  public bool IsTrackingPiggyBankProjectile { get; set; }

  public LegacyProjectileSlot? PiggyBankProjectileLocalSlot { get; set; }

  public LegacyPlayerSlot? PiggyBankProjectileOwnerSlot { get; set; }

  public int? PiggyBankProjectileIdentity { get; set; }

  public int? PiggyBankProjectileType { get; set; }

  public bool IsTrackingVoidLensProjectile { get; set; }

  public LegacyProjectileSlot? VoidLensProjectileLocalSlot { get; set; }

  public LegacyPlayerSlot? VoidLensProjectileOwnerSlot { get; set; }

  public int? VoidLensProjectileIdentity { get; set; }

  public int? VoidLensProjectileType { get; set; }
}

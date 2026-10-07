namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-746, P09-747, P09-748, P09-749, P09-750
// crossSubsystemOwner: container contents, capacity, and transfer ordering remain Items-owned
/// <summary>
/// 保存玩家四种随身容器及虚空背包状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：bank（第 1089 行）； bank2（第 1091 行）； bank3（第 1093 行）； bank4（第 1095 行）； voidVaultInfo（第 1097
/// 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P09-player-inventory-equipment-component-design.md。
/// </para>
/// <para>依据位置：第 114 行。</para>
/// </remarks>
public sealed class PlayerContainerRelationComponent
{
  public PlayerContainerRef Bank { get; internal set; } = PlayerContainerRef.None;

  public PlayerContainerRef Bank2 { get; internal set; } = PlayerContainerRef.None;

  public PlayerContainerRef Bank3 { get; internal set; } = PlayerContainerRef.None;

  public PlayerContainerRef Bank4 { get; internal set; } = PlayerContainerRef.None;

  public VoidVaultState VoidVaultState { get; internal set; }
}

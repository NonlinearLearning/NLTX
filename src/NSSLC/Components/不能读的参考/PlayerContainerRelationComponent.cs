namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-746, P09-747, P09-748, P09-749, P09-750
// crossSubsystemOwner: container contents, capacity, and transfer ordering remain Items-owned
/// <summary>
/// 保存玩家与四种个人存储容器的引用及虚空背包状态。
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
/// <para>
/// 用途：物品转移流程根据 Bank 到 Bank4 找到玩家的个人存储空间，再读取 VoidVaultState 决定虚空背包相关行为。
/// </para>
/// <para>
/// 当前实现：四个引用默认 None，只保存容器关联。容器容量、物品内容和转移顺序由 Items 领域维护。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C115，L2（通用可选玩法）。分级理由：多银行与虚空背包属于主背包之外的存储。
/// </para>
/// </remarks>
public sealed class PlayerContainerRelationComponent
{
  /// <summary>
  /// 第一种个人容器引用，对应原版猪猪存钱罐。
  /// </summary>
  public PlayerContainerRef Bank { get; internal set; } = PlayerContainerRef.None;

  /// <summary>
  /// 第二种个人容器引用，对应原版保险箱。
  /// </summary>
  public PlayerContainerRef Bank2 { get; internal set; } = PlayerContainerRef.None;

  /// <summary>
  /// 第三种个人容器引用，对应原版护卫熔炉。
  /// </summary>
  public PlayerContainerRef Bank3 { get; internal set; } = PlayerContainerRef.None;

  /// <summary>
  /// 第四种个人容器引用，对应原版虚空保险库。
  /// </summary>
  public PlayerContainerRef Bank4 { get; internal set; } = PlayerContainerRef.None;

  /// <summary>
  /// 虚空背包相关状态，具体标记由 VoidVaultState 定义。
  /// </summary>
  public VoidVaultState VoidVaultState { get; internal set; }
}

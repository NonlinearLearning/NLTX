namespace Terraria.Player;

// Holds the current-tick tile interaction capabilities derived from equipment.
/// <summary>
/// 保存玩家方块交互范围和建造速度饰品能力。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：equippedAnyTileRangeAcc（第 2410 行）； equippedAnyTileSpeedAcc（第 2412 行）；
/// equippedAnyWallSpeedAcc（第 2414 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P04-player-lifecycle-interaction-component-design.md。
/// </para>
/// <para>依据位置：第 496 行。</para>
/// </remarks>
public sealed class PlayerTileInteractionCapabilityComponent
{
  public bool HasTileRangeAccessory { get; set; }

  public bool HasTileSpeedAccessory { get; set; }

  public bool HasWallSpeedAccessory { get; set; }
}

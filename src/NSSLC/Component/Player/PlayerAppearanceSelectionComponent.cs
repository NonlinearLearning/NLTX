namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-804, P09-805, P09-806
// crossSubsystemOwner: network bitmask, loadout exchange, and persistence remain integration-review
/// <summary>
/// 保存玩家隐藏饰品和其他外观选择。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：voiceOverride（第 1206 行）； hideVisibleAccessory（第 1208 行）； hideMisc（第 1210 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P09-player-inventory-equipment-component-design.md。
/// </para>
/// <para>依据位置：第 201 行。</para>
/// </remarks>
public sealed class PlayerAppearanceSelectionComponent
{
  public const int HiddenVisibleAccessoryCount = 10;

  public sbyte VoiceOverride { get; internal set; }

  public bool[] HiddenVisibleAccessories { get; } =
    new bool[HiddenVisibleAccessoryCount];

  // Version4 stores hideMisc as BitsByte; byte preserves its packed representation
  // until the protocol type is available in NLTX.
  public byte HideMiscBits { get; internal set; }

}

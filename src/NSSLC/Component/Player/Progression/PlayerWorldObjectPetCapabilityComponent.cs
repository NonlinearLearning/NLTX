namespace Terraria.Player.Progression;

// status: local-rebuild-verified; production-integration: unknown
// source-members: petFlagDirtiestBlock, petFlagBoulderPet, petFlagRainbowBoulderPet,
// petFlagAxeFairyPet
/// <summary>
/// 保存玩家世界物体来源宠物的能力标记。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：petFlagDirtiestBlock（第 1654 行）； petFlagBoulderPet（第 1656 行）； petFlagRainbowBoulderPet（第
/// 1658 行）； petFlagAxeFairyPet（第 1664 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P05-player-progression-pets-minions-component-design.md。
/// </para>
/// <para>依据位置：第 297 行。</para>
/// </remarks>
public sealed class PlayerWorldObjectPetCapabilityComponent
{
  public bool PetFlagDirtiestBlock { get; internal set; }

  public bool PetFlagBoulderPet { get; internal set; }

  public bool PetFlagRainbowBoulderPet { get; internal set; }

  public bool PetFlagAxeFairyPet { get; internal set; }
}

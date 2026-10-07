namespace Terraria.Player.Progression;

// status: local-rebuild-verified; production-integration: unknown
// source-members: petFlagBerniePet, petFlagGlommerPet, petFlagDeerclopsPet, petFlagPigPet,
// petFlagChesterPet, petFlagJunimoPet, petFlagBlueChickenPet, petFlagSpiffo, petFlagCaveling,
// petFlagDeadCellsSwarmBiter, petFlagPufferfish, petFlagChillet, petFlagChilletIgnis
/// <summary>
/// 保存玩家联动宠物的能力标记。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：petFlagBerniePet（第 1636 行）； petFlagGlommerPet（第 1638 行）； petFlagDeerclopsPet（第 1640 行）；
/// petFlagPigPet（第 1642 行）； petFlagChesterPet（第 1644 行）； petFlagJunimoPet（第 1646 行）；
/// petFlagBlueChickenPet（第 1648 行）； petFlagSpiffo（第 1650 行）； petFlagCaveling（第 1652 行）；
/// petFlagDeadCellsSwarmBiter（第 1660 行）； petFlagPufferfish（第 1662 行）； petFlagChillet（第 1666 行）；
/// petFlagChilletIgnis（第 1668 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P05-player-progression-pets-minions-component-design.md。
/// </para>
/// <para>依据位置：第 284 行。</para>
/// </remarks>
public sealed class PlayerCrossoverPetCapabilityComponent
{
  public bool PetFlagBerniePet { get; internal set; }

  public bool PetFlagGlommerPet { get; internal set; }

  public bool PetFlagDeerclopsPet { get; internal set; }

  public bool PetFlagPigPet { get; internal set; }

  public bool PetFlagChesterPet { get; internal set; }

  public bool PetFlagJunimoPet { get; internal set; }

  public bool PetFlagBlueChickenPet { get; internal set; }

  public bool PetFlagSpiffo { get; internal set; }

  public bool PetFlagCaveling { get; internal set; }

  public bool PetFlagDeadCellsSwarmBiter { get; internal set; }

  public bool PetFlagPufferfish { get; internal set; }

  public bool PetFlagChillet { get; internal set; }

  public bool PetFlagChilletIgnis { get; internal set; }
}

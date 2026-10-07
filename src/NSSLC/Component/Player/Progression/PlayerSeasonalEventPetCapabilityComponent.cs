namespace Terraria.Player.Progression;

// status: local-rebuild-verified; production-integration: unknown
// source-members: petFlagDD2Gato, petFlagDD2Ghost, petFlagDD2Dragon, petFlagPumpkingPet,
// petFlagEverscreamPet, petFlagIceQueenPet, petFlagMartianPet, petFlagDD2OgrePet,
// petFlagDD2BetsyPet
/// <summary>
/// 保存玩家季节事件宠物的能力标记。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：petFlagDD2Gato（第 1560 行）； petFlagDD2Ghost（第 1562 行）； petFlagDD2Dragon（第 1564 行）；
/// petFlagPumpkingPet（第 1622 行）； petFlagEverscreamPet（第 1624 行）； petFlagIceQueenPet（第 1626 行）；
/// petFlagMartianPet（第 1628 行）； petFlagDD2OgrePet（第 1630 行）； petFlagDD2BetsyPet（第 1632 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P05-player-progression-pets-minions-component-design.md。
/// </para>
/// <para>依据位置：第 262 行。</para>
/// </remarks>
public sealed class PlayerSeasonalEventPetCapabilityComponent
{
  public bool PetFlagDD2Gato { get; internal set; }

  public bool PetFlagDD2Ghost { get; internal set; }

  public bool PetFlagDD2Dragon { get; internal set; }

  public bool PetFlagPumpkingPet { get; internal set; }

  public bool PetFlagEverscreamPet { get; internal set; }

  public bool PetFlagIceQueenPet { get; internal set; }

  public bool PetFlagMartianPet { get; internal set; }

  public bool PetFlagDD2OgrePet { get; internal set; }

  public bool PetFlagDD2BetsyPet { get; internal set; }
}

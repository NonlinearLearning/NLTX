namespace Terraria.Npc;

/// <summary>
/// 保存 NPC 当前状态效果标记及持续伤害输入。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>
/// 主要源成员：dripping（第 5953 行）； drippingSlime（第 5955 行）； drippingSparkleSlime（第 5957 行）； midas（第
/// 6075 行）； ichor（第 6077 行）； brokenArmor（第 6079 行）； onFire（第 6081 行）； onFire2（第 6083 行）；
/// onFire3（第 6085 行）； onFrostBurn（第 6087 行）； onFrostBurn2（第 6089 行）； poisoned（第 6091 行）； venom（第
/// 6093 行）； tipsy（第 6095 行）； bleeding（第 6097 行）； hemorrhage（第 6099 行）； markedByScytheWhip（第 6101
/// 行）； markedByEelWhip（第 6103 行）； shadowFlame（第 6105 行）； soulDrain（第 6107 行）； shimmering（第 6109
/// 行）； lifeRegen（第 6111 行）； lifeRegenExpectedLossPerSecond（第 6115 行）； confused（第 6117 行）；
/// loveStruck（第 6119 行）； stinky（第 6121 行）； dryadWard（第 6123 行）； javelined（第 6131 行）；
/// tentacleSpiked（第 6133 行）； bloodButchered（第 6135 行）； celled（第 6137 行）； dryadBane（第 6139 行）；
/// daybreak（第 6141 行）； betsysCurse（第 6145 行）； oiled（第 6147 行）。
/// </para>
/// </remarks>
public struct NpcStatusFlagsComponent
{
  public int LifeRegenerationRate;
  public int LifeRegenerationExpectedLossPerSecond;
  public bool SoulDrain;
  public bool Poisoned;
  public bool Venom;
  public bool Tipsy;
  public bool Bleeding;
  public bool Hemorrhage;
  public bool ShadowFlame;
  public bool OnFire;
  public bool Midas;
  public bool Ichor;
  public bool BrokenArmor;
  public bool OnFrostBurn;
  public bool OnFrostBurn2;
  public bool OnFire2;
  public bool OnFire3;
  public bool Confused;
  public bool LoveStruck;
  public bool DryadWard;
  public bool Stinky;
  public bool Dripping;
  public bool DrippingSlime;
  public bool DrippingSparkleSlime;
  public bool Daybreak;
  public bool Javelined;
  public bool TentacleSpiked;
  public bool BloodButchered;
  public bool Celled;
  public bool DryadBane;
  public bool BetsysCurse;
  public bool Oiled;
  public bool MarkedByScytheWhip;
  public bool MarkedByEelWhip;
  public bool Shimmering;
}

namespace Terraria.Npc;

// status: partial
// sourceMembers: IsABestiaryIconDummy, IsAPortraitDummy, ForcePartyHatOn,
// nameOverIncrement, nameOverDistance, nameOver, altTexture, townNpcVariationIndex
// crossSubsystemOwner: presentation snapshot integration-review
/// <summary>
/// 保存 NPC 图鉴、头像、名字显示、材质和派对外观状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>
/// 主要源成员：ForcePartyHatOn（第 5905 行）； nameOverIncrement（第 5943 行）； nameOverDistance（第 5945 行）；
/// nameOver（第 5947 行）； altTexture（第 5961 行）； townNpcVariationIndex（第 5963 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P12-npc-combat-network-damage-component-design.md。</para>
/// <para>依据位置：第 13 行。</para>
/// </remarks>
public sealed class NpcPresentationStateComponent
{
  public NpcPresentationStateComponent(
    bool isBestiaryIconDummy = false,
    bool isPortraitDummy = false,
    bool forcePartyHatOn = false,
    float nameOverIncrement = 0.0f,
    float nameOverDistance = 0.0f,
    float nameOver = 0.0f,
    int altTexture = 0,
    int townNpcVariationIndex = 0)
  {
    IsBestiaryIconDummy = isBestiaryIconDummy;
    IsPortraitDummy = isPortraitDummy;
    ForcePartyHatOn = forcePartyHatOn;
    NameOverIncrement = nameOverIncrement;
    NameOverDistance = nameOverDistance;
    NameOver = nameOver;
    AltTexture = altTexture;
    TownNpcVariationIndex = townNpcVariationIndex;
  }

  public bool IsBestiaryIconDummy { get; }

  public bool IsPortraitDummy { get; }

  public bool ForcePartyHatOn { get; }

  public float NameOverIncrement { get; }

  public float NameOverDistance { get; }

  public float NameOver { get; }

  public int AltTexture { get; }

  public int TownNpcVariationIndex { get; }
}

using System;

namespace Terraria.Town.Progression.Pets;

/// <summary>
/// 保存城镇猫、狗和兔子是否已被收养。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：boughtCat（第 6167 行）； boughtDog（第 6169 行）； boughtBunny（第 6171 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-p13-npc-town-progression-component-design.md。</para>
/// <para>依据位置：第 432 行。</para>
/// </remarks>
public sealed class TownPetAdoptionProgressStateComponent
{
  public bool BoughtCat { get; private set; }

  public bool BoughtDog { get; private set; }

  public bool BoughtBunny { get; private set; }

  public bool Adopt(TownPetKind petKind)
  {
    if (IsAdopted(petKind))
    {
      return false;
    }

    switch (petKind)
    {
      case TownPetKind.Cat:
        BoughtCat = true;
        break;
      case TownPetKind.Dog:
        BoughtDog = true;
        break;
      case TownPetKind.Bunny:
        BoughtBunny = true;
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(petKind), petKind, "Unknown town pet kind.");
    }

    return true;
  }

  public bool IsAdopted(TownPetKind petKind)
  {
    return petKind switch
    {
      TownPetKind.Cat => BoughtCat,
      TownPetKind.Dog => BoughtDog,
      TownPetKind.Bunny => BoughtBunny,
      _ => throw new ArgumentOutOfRangeException(nameof(petKind), petKind, "Unknown town pet kind."),
    };
  }

  public void Reset()
  {
    BoughtCat = false;
    BoughtDog = false;
    BoughtBunny = false;
  }
}

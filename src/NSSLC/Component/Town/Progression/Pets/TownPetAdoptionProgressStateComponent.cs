using System;

namespace Terraria.Town.Progression.Pets;

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

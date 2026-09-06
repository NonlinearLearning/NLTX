namespace Terraria.Town;

public sealed class TownResidentComponent
{
  public TownResidentComponent(
    bool isFriendly,
    int housingCategory,
    TownResidentCapabilities capabilities)
  {
    IsTownResident = true;
    IsFriendly = isFriendly;
    HousingCategory = housingCategory;
    Capabilities = capabilities;
  }

  public bool IsTownResident { get; }

  public bool IsFriendly { get; }

  public int HousingCategory { get; }

  public TownResidentCapabilities Capabilities { get; }

  public bool CanUseHousing =>
    (Capabilities & TownResidentCapabilities.CanUseHousing) != 0;

  public bool CanOpenDialogue =>
    (Capabilities & TownResidentCapabilities.CanOpenDialogue) != 0;
}

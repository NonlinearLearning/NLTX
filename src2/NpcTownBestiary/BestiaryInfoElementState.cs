namespace Terraria.NpcTownBestiary;

public sealed class BestiaryInfoElementState
{
  public BestiaryInfoElementState(
    string? flavorTextLocalizationKey = null,
    BestiaryDropRateView? dropRate = null,
    string? namePlateLocalizationKey = null,
    NpcNetId? namePlateNpcNetId = null,
    int? filledRarityStars = null,
    BestiaryNpcStatsView? stats = null,
    NpcNetId? identityNpcNetId = null,
    int? rarityLevel = null)
  {
    FlavorTextLocalizationKey = flavorTextLocalizationKey;
    DropRate = dropRate;
    NamePlateLocalizationKey = namePlateLocalizationKey;
    NamePlateNpcNetId = namePlateNpcNetId;
    FilledRarityStars = filledRarityStars;
    Stats = stats;
    IdentityNpcNetId = identityNpcNetId;
    RarityLevel = rarityLevel;
  }

  public string? FlavorTextLocalizationKey { get; }

  public BestiaryDropRateView? DropRate { get; }

  public string? NamePlateLocalizationKey { get; }

  public NpcNetId? NamePlateNpcNetId { get; }

  public int? FilledRarityStars { get; }

  public BestiaryNpcStatsView? Stats { get; }

  public NpcNetId? IdentityNpcNetId { get; }

  public int? RarityLevel { get; }
}

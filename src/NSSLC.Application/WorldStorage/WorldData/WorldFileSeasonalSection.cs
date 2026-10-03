namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// Persisted seasonal flags and hardmode ore tier choices following TreeTops data.
/// </summary>
public sealed class WorldFileSeasonalSection
{
  public const string SectionId = "world.seasonal";

  public WorldFileSeasonalSection(
    bool forceHalloweenForToday,
    bool forceChristmasForToday,
    int copperOreTier,
    int ironOreTier,
    int silverOreTier,
    int goldOreTier)
  {
    ForceHalloweenForToday = forceHalloweenForToday;
    ForceChristmasForToday = forceChristmasForToday;
    CopperOreTier = copperOreTier;
    IronOreTier = ironOreTier;
    SilverOreTier = silverOreTier;
    GoldOreTier = goldOreTier;
  }

  public bool ForceHalloweenForToday { get; }

  public bool ForceChristmasForToday { get; }

  public int CopperOreTier { get; }

  public int IronOreTier { get; }

  public int SilverOreTier { get; }

  public int GoldOreTier { get; }

  public static WorldFileSeasonalSection Empty => new(
    forceHalloweenForToday: false,
    forceChristmasForToday: false,
    copperOreTier: -1,
    ironOreTier: -1,
    silverOreTier: -1,
    goldOreTier: -1);
}

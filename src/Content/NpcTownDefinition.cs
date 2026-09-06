namespace Terraria.Content;

public sealed record NpcTownDefinition(bool IsTownNpc, bool IsLikeTownNpc = false)
{
  public bool CountsAsCritter { get; init; }

  public int TownNpcVariationCount { get; init; }

  public int? HousingPriority { get; init; }

  public bool CanBeReplacedByOtherNpcs { get; init; }
}

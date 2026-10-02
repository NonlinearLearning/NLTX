namespace Terraria.WorldGeneration.Definitions;

public readonly record struct WorldSeedOptionSeedMatchResult
{
  private WorldSeedOptionSeedMatchResult(
    bool isMatch,
    WorldSeedOptionMatch match)
  {
    IsMatch = isMatch;
    Match = match;
  }

  public bool IsMatch { get; }

  public WorldSeedOptionMatch Match { get; }

  public static WorldSeedOptionSeedMatchResult Matched(WorldSeedOptionMatch match)
  {
    return new WorldSeedOptionSeedMatchResult(true, match);
  }

  public static WorldSeedOptionSeedMatchResult NoMatch =>
    new(false, default);
}

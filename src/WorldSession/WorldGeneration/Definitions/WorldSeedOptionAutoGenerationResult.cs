namespace Terraria.WorldGeneration.Definitions;

public readonly record struct WorldSeedOptionAutoGenerationResult
{
  private WorldSeedOptionAutoGenerationResult(
    bool isMatch,
    WorldSeedOptionId optionId,
    bool autoGenerationEnabled)
  {
    IsMatch = isMatch;
    OptionId = optionId;
    AutoGenerationEnabled = autoGenerationEnabled;
  }

  public bool IsMatch { get; }

  public WorldSeedOptionId OptionId { get; }

  public bool AutoGenerationEnabled { get; }

  public static WorldSeedOptionAutoGenerationResult Matched(
    WorldSeedOptionId optionId,
    bool autoGenerationEnabled)
  {
    return new WorldSeedOptionAutoGenerationResult(
      true,
      optionId,
      autoGenerationEnabled);
  }

  public static WorldSeedOptionAutoGenerationResult NoMatch =>
    new(false, default, false);
}

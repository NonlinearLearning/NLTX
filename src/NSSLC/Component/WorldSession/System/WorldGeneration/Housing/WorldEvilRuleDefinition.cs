using System;

namespace Terraria.WorldGeneration.Housing;

public readonly record struct WorldEvilRuleDefinition
{
  public const int RandomRequestValue = -1;
  public const int CorruptionRequestValue = 0;
  public const int CrimsonRequestValue = 1;

  private WorldEvilRuleDefinition(int worldGenParameter)
  {
    WorldGenParameter = worldGenParameter;
  }

  public static WorldEvilRuleDefinition Random { get; } =
    new(RandomRequestValue);

  public static WorldEvilRuleDefinition Corruption { get; } =
    new(CorruptionRequestValue);

  public static WorldEvilRuleDefinition Crimson { get; } =
    new(CrimsonRequestValue);

  public int WorldGenParameter { get; }

  public bool IsRandom => WorldGenParameter == RandomRequestValue;

  public bool RequestsCorruption =>
    WorldGenParameter == CorruptionRequestValue;

  public bool RequestsCrimson => WorldGenParameter == CrimsonRequestValue;

  public static WorldEvilRuleDefinition Create(int worldGenParameter)
  {
    return worldGenParameter switch
    {
      RandomRequestValue => Random,
      CorruptionRequestValue => Corruption,
      CrimsonRequestValue => Crimson,
      _ => throw new ArgumentOutOfRangeException(nameof(worldGenParameter)),
    };
  }
}

using System;

namespace Terraria.WorldGeneration.Components;

public sealed class SpecialSeedGenerationRuleFlagsComponent
{
  public SpecialSeedGenerationRuleFlagsComponent(
    long generationId,
    bool notTheBeesAndForTheWorthyNoCelebration,
    bool noTrapsAndForTheWorthyNoCelebration)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
    NotTheBeesAndForTheWorthyNoCelebration =
      notTheBeesAndForTheWorthyNoCelebration;
    NoTrapsAndForTheWorthyNoCelebration =
      noTrapsAndForTheWorthyNoCelebration;
  }

  public long GenerationId { get; }

  public bool NotTheBeesAndForTheWorthyNoCelebration { get; }

  public bool NoTrapsAndForTheWorthyNoCelebration { get; }

  public SpecialSeedGenerationRuleFlagsSnapshot CreateSnapshot()
  {
    return new SpecialSeedGenerationRuleFlagsSnapshot(
      GenerationId,
      NotTheBeesAndForTheWorthyNoCelebration,
      NoTrapsAndForTheWorthyNoCelebration);
  }
}

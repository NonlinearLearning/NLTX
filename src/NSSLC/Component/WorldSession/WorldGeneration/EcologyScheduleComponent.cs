using System;

namespace Terraria.WorldGeneration.Components;

public sealed class EcologyScheduleComponent
{
  public EcologyScheduleComponent(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
  }

  public long GenerationId { get; }

  public bool IsInfectionSpreadAllowed { get; init; }

  public int? OvergroundSampleX { get; init; }

  public int? OvergroundSampleY { get; init; }

  public int? UndergroundSampleX { get; init; }

  public int? UndergroundSampleY { get; init; }

  public int? WorldUpdateRate { get; init; }

  public int? EcologyMutationBudget { get; init; }

  public ulong ScheduleRevision { get; init; }

  public bool IsEcologyPropagationEnabled =>
    IsInfectionSpreadAllowed && WorldUpdateRate is > 0;

  public bool HasConfiguredBudget => EcologyMutationBudget is >= 0;
}

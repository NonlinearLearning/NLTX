using System;

namespace Terraria.WorldGeneration.Adapters;

public readonly record struct StructureReservationIntent(
  long GenerationId,
  string ReservationId,
  WorldGenerationRectangle Bounds)
{
  public bool IsValid => GenerationId >= 0 &&
    !string.IsNullOrWhiteSpace(ReservationId);

  public void Validate()
  {
    if (!IsValid)
    {
      throw new ArgumentException(
        "A structure reservation requires a generation and stable identity.",
        nameof(ReservationId));
    }
  }
}

using System;
using Terraria.WorldGeneration.Adapters;

namespace Terraria.WorldGeneration.Structures;

public readonly record struct WorldStructureReservationRequest(
  long GenerationId,
  string ReservationId,
  WorldGenerationRectangle Bounds,
  int Padding = 0,
  bool IsProtected = true)
{
  public bool IsWellFormed =>
    GenerationId >= 0 &&
    !string.IsNullOrWhiteSpace(ReservationId) &&
    Padding >= 0;

  public void Validate()
  {
    if (GenerationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(GenerationId));
    }

    ArgumentException.ThrowIfNullOrWhiteSpace(ReservationId);
    if (Padding < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(Padding));
    }
  }

  public WorldGenerationRectangle GetReservedBounds()
  {
    Validate();
    return new WorldGenerationRectangle(
      checked(Bounds.X - Padding),
      checked(Bounds.Y - Padding),
      checked(Bounds.Width + (Padding * 2)),
      checked(Bounds.Height + (Padding * 2)));
  }
}

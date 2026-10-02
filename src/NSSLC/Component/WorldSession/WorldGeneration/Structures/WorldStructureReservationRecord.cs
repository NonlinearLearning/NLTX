using Terraria.WorldGeneration.Adapters;

namespace Terraria.WorldGeneration.Structures;

public readonly record struct WorldStructureReservationRecord(
  WorldStructureReservationRequest Request,
  WorldGenerationRectangle ReservedBounds);

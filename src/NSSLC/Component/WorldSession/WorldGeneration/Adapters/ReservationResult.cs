namespace Terraria.WorldGeneration.Adapters;

public readonly record struct ReservationResult(
  bool Accepted,
  string ReservationId,
  string? RejectionReason = null);

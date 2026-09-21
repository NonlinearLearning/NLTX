namespace Terraria.ExternalPlatformBoundaries.Nat;

public readonly record struct NatPortMappingResult(
  NatPortMappingStatus Status,
  NatPortMappingKey Key,
  string? Diagnostic);

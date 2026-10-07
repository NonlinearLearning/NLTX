namespace Terraria.WorldGeneration.Components;

// status: proposed supporting type
// evidenceStatus: unresolved until the housing key owner is decided
// crossSubsystemOwner: integration-review
public enum TownHousingKeyMode : byte
{
  Unresolved,
  Type,
  Instance,
  Dual,
}

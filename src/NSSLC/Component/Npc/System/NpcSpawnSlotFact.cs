namespace Terraria.Npc;

public readonly record struct NpcSpawnSlotFact(
  bool IsActive,
  int SpawnSlotProtection,
  bool CanBeReplacedByOtherNPCs,
  uint? Generation = null)
{
  public bool IsSpawnSlotInUse => IsActive || SpawnSlotProtection > 0;

  public uint? ExpectedGeneration =>
    CanBeReplacedByOtherNPCs && Generation is > 0 ? Generation : null;
}

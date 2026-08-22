namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct RoomNeedsResult(
  bool HasChair,
  bool HasTable,
  bool HasDoor,
  bool HasTorch)
{
  public bool CanSpawn => HasChair && HasTable && HasDoor && HasTorch;
}

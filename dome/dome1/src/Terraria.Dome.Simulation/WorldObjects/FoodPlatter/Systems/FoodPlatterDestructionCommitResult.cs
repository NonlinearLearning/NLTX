namespace Terraria.Dome.Simulation.WorldObjects.FoodPlatter.Systems;

public readonly record struct FoodPlatterDestructionCommitResult(
  bool Succeeded,
  bool TileEntityRemoved,
  bool ItemDropped,
  string? FailureReason)
{
  public static FoodPlatterDestructionCommitResult Failed(string reason)
  {
    return new FoodPlatterDestructionCommitResult(false, false, false, reason);
  }
}

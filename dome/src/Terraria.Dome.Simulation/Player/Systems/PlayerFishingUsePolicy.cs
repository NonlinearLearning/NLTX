namespace Terraria.Dome.Simulation.Player.Systems;

public readonly record struct PlayerFishingUseInput(
  int FishingPolePower,
  bool HasActiveOwnedBobber);

public static class PlayerFishingUsePolicy
{
  public static bool CanUseFishingPole(PlayerFishingUseInput input)
  {
    return input.FishingPolePower <= 0 || !input.HasActiveOwnedBobber;
  }
}

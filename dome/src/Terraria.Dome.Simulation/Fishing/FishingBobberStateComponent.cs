using Terraria.Dome.Simulation;

namespace Terraria.Dome.Simulation.Fishing;

public struct FishingBobberStateComponent
{
  public FishingBobberStateComponent(PlayerHandle owner, int bobberProjectileType)
  {
    Owner = owner;
    BobberProjectileType = bobberProjectileType;
    Phase = FishingBobberPhase.Waiting;
  }

  public PlayerHandle Owner;
  public int BobberProjectileType;
  public FishingBobberPhase Phase;
  public int? PendingItemType;
  public int WaterTilesCount;
  public int WaterNeededToFish;
  public int FishingLevel;
  public bool IsInLava;
  public bool IsInHoney;
  public int SwingCount;

  public bool IsBobber => true;
}

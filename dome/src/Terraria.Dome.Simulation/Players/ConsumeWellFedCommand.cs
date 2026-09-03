using System;

namespace Terraria.Dome.Simulation.Players;

public readonly record struct ConsumeWellFedCommand
{
  public ConsumeWellFedCommand(PlayerHandle player, int foodRank, int foodBuffTime)
  {
    if (!player.IsValid || foodRank < 0 || foodBuffTime < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(player));
    }

    Player = player;
    FoodRank = foodRank;
    FoodBuffTime = foodBuffTime;
  }

  public int FoodBuffTime { get; }

  public int FoodRank { get; }

  public PlayerHandle Player { get; }
}

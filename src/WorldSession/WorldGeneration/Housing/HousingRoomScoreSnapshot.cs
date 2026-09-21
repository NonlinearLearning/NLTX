using System;

namespace Terraria.WorldGeneration.Housing;

public readonly record struct HousingRoomScoreSnapshot
{
  public HousingRoomScoreSnapshot(
    int numRoomTiles,
    int hiScore,
    int? bestX,
    int? bestY,
    int? sharedRoomX)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(numRoomTiles);
    if (bestX.HasValue != bestY.HasValue)
    {
      throw new ArgumentException(
        "A best-room candidate must contain both coordinates or neither coordinate.",
        nameof(bestY));
    }

    NumRoomTiles = numRoomTiles;
    HighScore = hiScore;
    BestX = bestX;
    BestY = bestY;
    SharedRoomX = sharedRoomX;
  }

  public int NumRoomTiles { get; }

  public int HighScore { get; }

  public int? BestX { get; }

  public int? BestY { get; }

  public int? SharedRoomX { get; }

  public bool HasBestCandidate => BestX.HasValue;
}

using System;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Housing;

public readonly record struct HousingRoomScoreSnapshot
{
  private readonly IReadOnlyList<HousingRoomScoreCandidate>? _candidates;

  public HousingRoomScoreSnapshot(
    int numRoomTiles,
    int hiScore,
    int? bestX,
    int? bestY,
    int? sharedRoomX,
    HousingRoomScoreCandidate[]? candidates = null,
    int? baseScore = null,
    bool hasStandingSpace = false)
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
    BaseScore = baseScore ?? hiScore;
    BestX = bestX;
    BestY = bestY;
    SharedRoomX = sharedRoomX;
    HasStandingSpace = hasStandingSpace;
    _candidates = Array.AsReadOnly(
      candidates is null
        ? Array.Empty<HousingRoomScoreCandidate>()
        : (HousingRoomScoreCandidate[]) candidates.Clone());
  }

  public int NumRoomTiles { get; }

  public int HighScore { get; }

  public int BaseScore { get; }

  public int? BestX { get; }

  public int? BestY { get; }

  public int? SharedRoomX { get; }

  public bool HasStandingSpace { get; }

  public IReadOnlyList<HousingRoomScoreCandidate> Candidates
  {
    get
    {
      if (_candidates is not null)
      {
        return _candidates;
      }

      return Array.Empty<HousingRoomScoreCandidate>();
    }
  }

  public bool HasBestCandidate => BestX.HasValue;
}

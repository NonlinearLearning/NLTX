using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public enum DungeonProgressionStageCheck
{
  Equals,
  LessThanOrEqualTo,
  GreaterThanOrEqualTo
}

public readonly record struct DungeonRoomSearchSettingsSnapshot
{
  public DungeonRoomSearchSettingsSnapshot(
    int? progressionStage,
    DungeonProgressionStageCheck progressionStageCheck,
    int? maximumDistance,
    int fluff = 0)
  {
    if (progressionStage is < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(progressionStage));
    }

    if (maximumDistance is < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumDistance));
    }

    ArgumentOutOfRangeException.ThrowIfNegative(fluff);

    ProgressionStage = progressionStage;
    ProgressionStageCheck = progressionStageCheck;
    MaximumDistance = maximumDistance;
    Fluff = fluff;
  }

  public int? ProgressionStage { get; }

  public DungeonProgressionStageCheck ProgressionStageCheck { get; }

  public int? MaximumDistance { get; }

  public int Fluff { get; }
}

public readonly record struct DungeonRoomCandidate
{
  public DungeonRoomCandidate(string id, int centerX, int centerY, int progressionStage)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(id);
    ArgumentOutOfRangeException.ThrowIfNegative(progressionStage);
    Id = id;
    CenterX = centerX;
    CenterY = centerY;
    ProgressionStage = progressionStage;
  }

  public string Id { get; }

  public int CenterX { get; }

  public int CenterY { get; }

  public int ProgressionStage { get; }
}

public static class DungeonRoomSearchQuery
{
  public static bool CanChoose(
    DungeonRoomCandidate candidate,
    DungeonRoomCandidate? excludedCandidate,
    DungeonRoomSearchSettingsSnapshot settings)
  {
    if (excludedCandidate.HasValue && candidate.Id == excludedCandidate.Value.Id)
    {
      return false;
    }

    if (settings.ProgressionStage.HasValue &&
        !MatchesProgressionStage(
          candidate.ProgressionStage,
          settings.ProgressionStage.Value,
          settings.ProgressionStageCheck))
    {
      return false;
    }

    if (excludedCandidate.HasValue && settings.MaximumDistance.HasValue)
    {
      long deltaX = candidate.CenterX - (long)excludedCandidate.Value.CenterX;
      long deltaY = candidate.CenterY - (long)excludedCandidate.Value.CenterY;
      double distance = Math.Sqrt((double)(deltaX * deltaX + deltaY * deltaY));
      if (distance >= settings.MaximumDistance.Value)
      {
        return false;
      }
    }

    return true;
  }

  private static bool MatchesProgressionStage(
    int candidateStage,
    int requestedStage,
    DungeonProgressionStageCheck check)
  {
    return check switch
    {
      DungeonProgressionStageCheck.Equals => candidateStage == requestedStage,
      DungeonProgressionStageCheck.LessThanOrEqualTo => candidateStage <= requestedStage,
      DungeonProgressionStageCheck.GreaterThanOrEqualTo => candidateStage >= requestedStage,
      _ => throw new ArgumentOutOfRangeException(nameof(check))
    };
  }
}

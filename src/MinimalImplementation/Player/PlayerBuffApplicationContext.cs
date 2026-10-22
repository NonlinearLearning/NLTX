using System;

namespace MinimalImplementation.Server.Player;

/// <summary>Equipment and difficulty facts for applying one authoritative buff.</summary>
public sealed class PlayerBuffApplicationContext
{
  public PlayerBuffApplicationContext(
    bool isImmune,
    bool fromNetPvp,
    bool hasPotionStation,
    bool isExpertMode,
    float difficultyDebuffTimeMultiplier)
  {
    if (!float.IsFinite(difficultyDebuffTimeMultiplier)
      || difficultyDebuffTimeMultiplier <= 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(difficultyDebuffTimeMultiplier));
    }

    IsImmune = isImmune;
    FromNetPvp = fromNetPvp;
    HasPotionStation = hasPotionStation;
    IsExpertMode = isExpertMode;
    DifficultyDebuffTimeMultiplier = difficultyDebuffTimeMultiplier;
  }

  public bool IsImmune { get; }

  public bool FromNetPvp { get; }

  public bool HasPotionStation { get; }

  public bool IsExpertMode { get; }

  public float DifficultyDebuffTimeMultiplier { get; }
}

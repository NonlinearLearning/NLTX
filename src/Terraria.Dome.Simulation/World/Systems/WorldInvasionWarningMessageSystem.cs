using System;

namespace Terraria.Dome.Simulation.WorldModel.Systems;

public enum WorldInvasionWarningKind
{
  None,
  ApproachingGoblins,
  ApproachingFrost,
  ApproachingPirates,
  RecedingGoblins,
  RecedingFrost,
  RecedingPirates,
  ArrivedGoblins,
  ArrivedFrost,
  ArrivedPirates,
  ArrivedMartians,
  CompletedGoblins,
  CompletedFrost,
  CompletedPirates,
  CompletedMartians
}

public sealed class WorldInvasionWarningMessageSystem
{
  public WorldInvasionWarningKind Resolve(
    int invasionType,
    int invasionSize,
    double invasionX,
    double spawnTileX)
  {
    if (invasionType is < 1 or > 4 ||
        invasionSize < 0 ||
        !double.IsFinite(invasionX) ||
        !double.IsFinite(spawnTileX))
    {
      return WorldInvasionWarningKind.None;
    }

    if (invasionSize == 0)
    {
      return GetCompletedKind(invasionType);
    }

    if (invasionX == spawnTileX)
    {
      return GetArrivedKind(invasionType);
    }

    if (invasionType == 4)
    {
      return WorldInvasionWarningKind.None;
    }

    return invasionX < spawnTileX
      ? GetApproachingKind(invasionType)
      : GetRecedingKind(invasionType);
  }

  private static WorldInvasionWarningKind GetApproachingKind(int invasionType)
  {
    return invasionType switch
    {
      1 => WorldInvasionWarningKind.ApproachingGoblins,
      2 => WorldInvasionWarningKind.ApproachingFrost,
      3 => WorldInvasionWarningKind.ApproachingPirates,
      _ => WorldInvasionWarningKind.None
    };
  }

  private static WorldInvasionWarningKind GetRecedingKind(int invasionType)
  {
    return invasionType switch
    {
      1 => WorldInvasionWarningKind.RecedingGoblins,
      2 => WorldInvasionWarningKind.RecedingFrost,
      3 => WorldInvasionWarningKind.RecedingPirates,
      _ => WorldInvasionWarningKind.None
    };
  }

  private static WorldInvasionWarningKind GetArrivedKind(int invasionType)
  {
    return invasionType switch
    {
      1 => WorldInvasionWarningKind.ArrivedGoblins,
      2 => WorldInvasionWarningKind.ArrivedFrost,
      3 => WorldInvasionWarningKind.ArrivedPirates,
      4 => WorldInvasionWarningKind.ArrivedMartians,
      _ => throw new ArgumentOutOfRangeException(nameof(invasionType))
    };
  }

  private static WorldInvasionWarningKind GetCompletedKind(int invasionType)
  {
    return invasionType switch
    {
      1 => WorldInvasionWarningKind.CompletedGoblins,
      2 => WorldInvasionWarningKind.CompletedFrost,
      3 => WorldInvasionWarningKind.CompletedPirates,
      4 => WorldInvasionWarningKind.CompletedMartians,
      _ => throw new ArgumentOutOfRangeException(nameof(invasionType))
    };
  }
}

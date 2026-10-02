using System;

namespace Terraria.WorldSession.NpcProgression.MoonLord;

public sealed class MoonLordEncounterDefinition
{
  private readonly int[,,,] _moonLordAttacksArray;
  private readonly int[,] _moonLordAttacksArray2;

  public MoonLordEncounterDefinition(
    int[,,,] moonLordAttacksArray,
    int[,] moonLordAttacksArray2,
    int moonLordFightingDistance,
    int maxMoonLordCountdown,
    int naturalMoonlordCountdownTime,
    int itemMoonlordCountdownTime)
  {
    ArgumentNullException.ThrowIfNull(moonLordAttacksArray);
    ArgumentNullException.ThrowIfNull(moonLordAttacksArray2);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(moonLordFightingDistance);
    ArgumentOutOfRangeException.ThrowIfNegative(maxMoonLordCountdown);
    ArgumentOutOfRangeException.ThrowIfNegative(naturalMoonlordCountdownTime);
    ArgumentOutOfRangeException.ThrowIfNegative(itemMoonlordCountdownTime);

    _moonLordAttacksArray = (int[,,,])moonLordAttacksArray.Clone();
    _moonLordAttacksArray2 = (int[,])moonLordAttacksArray2.Clone();
    MoonLordFightingDistance = moonLordFightingDistance;
    MaxMoonLordCountdown = maxMoonLordCountdown;
    NaturalMoonlordCountdownTime = naturalMoonlordCountdownTime;
    ItemMoonlordCountdownTime = itemMoonlordCountdownTime;
  }

  public int MoonLordFightingDistance { get; }

  public int MaxMoonLordCountdown { get; }

  public int NaturalMoonlordCountdownTime { get; }

  public int ItemMoonlordCountdownTime { get; }

  public int[,,,] GetMoonLordAttacksArray()
  {
    return (int[,,,])_moonLordAttacksArray.Clone();
  }

  public int[,] GetMoonLordAttacksArray2()
  {
    return (int[,])_moonLordAttacksArray2.Clone();
  }
}

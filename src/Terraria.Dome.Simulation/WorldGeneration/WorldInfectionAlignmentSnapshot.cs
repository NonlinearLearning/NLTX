using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct WorldInfectionAlignmentSnapshot
{
  public WorldInfectionAlignmentSnapshot(
    int totalEvil,
    int totalBlood,
    int totalGood,
    int totalSolid)
  {
    if (totalEvil < 0 || totalBlood < 0 || totalGood < 0 || totalSolid < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(totalSolid));
    }

    TotalEvil = totalEvil;
    TotalBlood = totalBlood;
    TotalGood = totalGood;
    TotalSolid = totalSolid;
    EvilPercent = CalculatePercent(totalEvil, totalSolid);
    BloodPercent = CalculatePercent(totalBlood, totalSolid);
    GoodPercent = CalculatePercent(totalGood, totalSolid);
  }

  public int TotalEvil { get; }

  public int TotalBlood { get; }

  public int TotalGood { get; }

  public int TotalSolid { get; }

  public byte EvilPercent { get; }

  public byte BloodPercent { get; }

  public byte GoodPercent { get; }

  private static byte CalculatePercent(int total, int denominator)
  {
    if (denominator == 0)
    {
      return 0;
    }

    int percent = (int)Math.Round((double)total / denominator * 100.0);
    return (byte)Math.Clamp(percent == 0 && total > 0 ? 1 : percent, 0, byte.MaxValue);
  }
}

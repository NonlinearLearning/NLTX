using System;

namespace Terraria.WorldGeneration.Passes;

public sealed class TrapGenerationDiagnosticScratch
{
  public const int TrapTypeCount = 4;

  private readonly int[] _failureCounts = new int[TrapTypeCount];
  private readonly int[] _successCounts = new int[TrapTypeCount];

  public void RecordFailure(int trapType)
  {
    _failureCounts[ValidateTrapType(trapType)]++;
  }

  public void RecordSuccess(int trapType)
  {
    _successCounts[ValidateTrapType(trapType)]++;
  }

  public int GetFailureCount(int trapType)
  {
    return _failureCounts[ValidateTrapType(trapType)];
  }

  public int GetSuccessCount(int trapType)
  {
    return _successCounts[ValidateTrapType(trapType)];
  }

  public void Clear()
  {
    Array.Clear(_failureCounts);
    Array.Clear(_successCounts);
  }

  private static int ValidateTrapType(int trapType)
  {
    if ((uint)trapType >= TrapTypeCount)
    {
      throw new ArgumentOutOfRangeException(nameof(trapType));
    }

    return trapType;
  }
}

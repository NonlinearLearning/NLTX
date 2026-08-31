using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyWavyCavesPassDefinition(
  int ReferenceWorldWidth,
  int ReferenceInvocationCount,
  int RemixInvocationDivisor,
  int MinimumYInset,
  int UnderworldYInset,
  int MinimumStartXInset,
  int MinimumSpacing)
{
  public int CalculateInvocationCount(int width, bool isRemixWorld)
  {
    if (width < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    int count = checked((int)(ReferenceInvocationCount * Math.Pow(
      width / (double)ReferenceWorldWidth,
      2)));
    return isRemixWorld ? count / RemixInvocationDivisor : count;
  }

  public void Validate()
  {
    if (ReferenceWorldWidth <= 0 || ReferenceInvocationCount < 0 || RemixInvocationDivisor <= 0 ||
        MinimumYInset < 0 || UnderworldYInset < 0 || MinimumStartXInset < 0 ||
        MinimumSpacing < 0)
    {
      throw new InvalidOperationException("WavyCaves pass definition contains an invalid source contract.");
    }
  }

  public static LegacyWavyCavesPassDefinition CreateDefault()
  {
    return new LegacyWavyCavesPassDefinition(
      ReferenceWorldWidth: 4200,
      ReferenceInvocationCount: 35,
      RemixInvocationDivisor: 3,
      MinimumYInset: 100,
      UnderworldYInset: 100,
      MinimumStartXInset: 80,
      MinimumSpacing: 80);
  }
}

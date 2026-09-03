namespace Terraria.Dome.Simulation.WorldGeneration;

public static class RainbowPaintQuery
{
  private const int WigglyPeriod = 50;
  private const int CoordinatePeriod = 44;
  private const int PaintPeriod = 11;
  private const int FirstPaintId = 13;
  private const int WigglyOffset = 5;
  private const int WigglyAmplitude = 10;

  public static byte ToPaintId(int x, int y, bool wiggly = false)
  {
    int paintX = x;
    int paintY = y;

    if (wiggly)
    {
      paintX += (int)(System.Math.Sin(
        (float)(y % WigglyPeriod) / WigglyPeriod * ((float)System.Math.PI * 2f)) *
        WigglyAmplitude) - WigglyOffset;
      paintY = 0;
    }

    int colorOffset = paintX % CoordinatePeriod + paintY % CoordinatePeriod;
    colorOffset %= PaintPeriod;
    return (byte)(FirstPaintId + colorOffset);
  }
}

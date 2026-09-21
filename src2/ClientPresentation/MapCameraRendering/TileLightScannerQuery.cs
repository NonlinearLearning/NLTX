namespace NLTX.ClientPresentation.MapCameraRendering;

public static class TileLightScannerQuery
{
  public static bool ShouldScan(
    int x,
    int y,
    int scanStride,
    IRandomSampleSource random)
  {
    _ = x;
    _ = y;
    ArgumentNullException.ThrowIfNull(random);
    if (scanStride <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(scanStride));
    }

    return scanStride == 1 || random.Next(scanStride) == 0;
  }
}

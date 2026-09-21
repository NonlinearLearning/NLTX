namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class LightMapScanSystem
{
  public void Scan(
    LightMapCacheComponent component,
    IReadOnlyList<LightMapCellInput> cells,
    IRandomSampleSource random,
    int scanStride = 1)
  {
    ArgumentNullException.ThrowIfNull(component);
    ArgumentNullException.ThrowIfNull(cells);
    ArgumentNullException.ThrowIfNull(random);
    if (scanStride <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(scanStride));
    }

    component.Clear();
    foreach (LightMapCellInput cell in cells)
    {
      if (!component.Contains(cell.X, cell.Y) ||
        !TileLightScannerQuery.ShouldScan(cell.X, cell.Y, scanStride, random))
      {
        continue;
      }

      component.SetCell(cell);
    }
  }

  public void Clear(LightMapCacheComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Clear();
  }
}

namespace NLTX.ClientPresentation.MapCameraRendering;

public static class MapEncodingCatalogQuery
{
  public static bool IsValid(MapEncodingCatalogComponent catalog)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    return catalog.DrawLoopMilliseconds > 0 &&
      catalog.MaxTileOptions > 0 &&
      catalog.MaxWallOptions > 0 &&
      catalog.MaxLiquidTypes > 0 &&
      catalog.MaxSkyGradients > 0 &&
      catalog.MaxDirtGradients > 0 &&
      catalog.MaxRockGradients > 0 &&
      catalog.MapChunkSize > 0;
  }
}

namespace NLTX.ClientPresentation.MapCameraRendering;

public readonly record struct SceneScanThresholds(
  int SnowTileMax,
  int MushroomTileThreshold,
  int AssumedScreenWidth,
  int AssumedScreenHeight,
  int ZoneScanPadding,
  int ZoneScanWidth,
  int ZoneScanHeight,
  int TownNpcWidth,
  int TownNpcHeight)
{
  public static SceneScanThresholds Default => new(
    SnowTileMax: 300,
    MushroomTileThreshold: 100,
    AssumedScreenWidth: 1920,
    AssumedScreenHeight: 1080,
    ZoneScanPadding: 10,
    ZoneScanWidth: 169,
    ZoneScanHeight: 124,
    TownNpcWidth: 630,
    TownNpcHeight: 480);
}

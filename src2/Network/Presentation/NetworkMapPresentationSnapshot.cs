namespace Terraria.Network.Presentation;

public sealed record NetworkMapPresentationSnapshot(
  int InstantTransitionCounter,
  int BackgroundDelay,
  int BackgroundStyle,
  float FrontLayerAlpha,
  float FarBackLayerAlpha,
  int WallOfFleshNpcIndex,
  int DrawAreaTop,
  int DrawAreaBottom,
  bool RefreshMap,
  bool MapReady,
  bool UpdateMap,
  int MapTimeMax,
  int MapTime,
  bool ClearMap);

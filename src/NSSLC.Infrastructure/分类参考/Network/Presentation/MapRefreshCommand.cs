namespace Terraria.Network.Presentation;

public readonly record struct MapRefreshCommand(
  int MapTimeMax,
  bool UpdateMap,
  bool ClearMap);

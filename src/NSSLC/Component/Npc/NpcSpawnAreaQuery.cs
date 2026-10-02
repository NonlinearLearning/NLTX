using System;

namespace Terraria.Npc;

public static class NpcSpawnAreaQuery
{
  public static NpcSpawnAreaResult Calculate(in NpcSpawnAreaInputs input)
  {
    int screenWidthTiles = input.ScreenWidthPixels / 16;
    int screenHeightTiles = input.ScreenHeightPixels / 16;
    int spawnRangeX = (int)((double)screenWidthTiles * 0.7);
    int spawnRangeY = (int)((double)screenHeightTiles * 0.7);
    int safeRangeX = (int)((double)screenWidthTiles * 0.52);
    int safeRangeY = (int)((double)screenHeightTiles * 0.52);

    if (input.SelectedItemType == 1254 ||
      input.SelectedItemType == 1299 ||
      input.PlayerScope)
    {
      float zoomFactor = 1.5f;
      if (input.SelectedItemType == 1254 && input.PlayerScope)
      {
        zoomFactor = 1.25f;
      }
      else if (input.SelectedItemType == 1254)
      {
        zoomFactor = 1.5f;
      }
      else if (input.SelectedItemType == 1299)
      {
        zoomFactor = 1.5f;
      }
      else if (input.PlayerScope)
      {
        zoomFactor = 2f;
      }

      int extraSpawnRangeX =
        (int)((double)screenWidthTiles * 0.5 / (double)zoomFactor);
      int extraSpawnRangeY =
        (int)((double)screenHeightTiles * 0.5 / (double)zoomFactor);
      spawnRangeX += extraSpawnRangeX;
      spawnRangeY += extraSpawnRangeY;
      safeRangeX += extraSpawnRangeX;
      safeRangeY += extraSpawnRangeY;
    }

    NpcSpawnTileRectangle spawnArea = ClampToWorld(
      CenteredRectangle(
        input.PlayerTileX,
        input.PlayerTileY,
        spawnRangeX * 2,
        spawnRangeY * 2),
      input.MaxTilesX,
      input.MaxTilesY);

    int safeAreaWidth = input.DualDungeonsSeed &&
      !input.ZoneOverworldHeight &&
      !input.ZoneSkyHeight
        ? safeRangeX
        : safeRangeX * 2;
    int safeAreaHeight = input.DualDungeonsSeed &&
      !input.ZoneOverworldHeight &&
      !input.ZoneSkyHeight
        ? safeRangeY
        : safeRangeY * 2;
    NpcSpawnTileRectangle safeArea = CenteredRectangle(
      input.PlayerTileX,
      input.PlayerTileY,
      safeAreaWidth,
      safeAreaHeight);

    return new NpcSpawnAreaResult(
      spawnArea,
      safeArea,
      safeRangeX,
      safeRangeY);
  }

  private static NpcSpawnTileRectangle CenteredRectangle(
    int centerX,
    int centerY,
    int width,
    int height)
  {
    return new NpcSpawnTileRectangle(
      centerX - width / 2,
      centerY - height / 2,
      width,
      height);
  }

  private static NpcSpawnTileRectangle ClampToWorld(
    NpcSpawnTileRectangle rectangle,
    int maxTilesX,
    int maxTilesY)
  {
    int left = Math.Max(0, Math.Min(rectangle.Left, maxTilesX));
    int top = Math.Max(0, Math.Min(rectangle.Top, maxTilesY));
    int right = Math.Max(0, Math.Min(rectangle.Right, maxTilesX));
    int bottom = Math.Max(0, Math.Min(rectangle.Bottom, maxTilesY));
    return new NpcSpawnTileRectangle(
      left,
      top,
      right - left,
      bottom - top);
  }
}

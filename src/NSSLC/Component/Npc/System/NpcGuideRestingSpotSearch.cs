namespace Terraria.Npc;

public readonly record struct NpcGuideRestingSpotTile(
  bool Active,
  bool SolidOrSlopedOrPlatform,
  bool CanBeSatOnForNpc,
  int Type,
  int FrameY);

public interface INpcGuideRestingSpotTileQuery
{
  NpcGuideRestingSpotTile ReadTile(int tileX, int tileY);
}

public interface INpcGuideRestingSpotOccupancyQuery
{
  bool IsOccupiedBySittingTownNpc(int tileX, int tileY);
}

public readonly record struct NpcGuideRestingSpotSearchInput(
  int TypeId,
  int NetId,
  int AiStyle,
  int HomeTileX,
  int HomeTileY,
  int MyTileX,
  int MyTileY,
  bool DayTime,
  float Ai0,
  bool IsTownSlime,
  int MaxTilesY,
  INpcGuideRestingSpotTileQuery? TileQuery,
  INpcGuideRestingSpotOccupancyQuery? OccupancyQuery);

public readonly record struct NpcGuideRestingSpotSearchResult(
  int FloorX,
  int FloorY,
  bool UsedAlternateRestingSpot,
  bool AlternateRestingSpotOccupied,
  NpcGuideSourceBranch Branches);

/// <summary>
/// Source-shaped floor and seat search from AI_007_FindGoodRestingSpot for the
/// Guide identity. World tile reads and current NPC occupancy stay behind
/// explicit owner queries.
/// </summary>
public static class NpcGuideRestingSpotSearch
{
  public static NpcGuideRestingSpotSearchResult Find(
    in NpcGuideRestingSpotSearchInput input)
  {
    if (!NpcGuideSourceProfile.CanHandle(input.TypeId, input.NetId, input.AiStyle))
    {
      throw new InvalidOperationException(
        "Guide resting-spot search requires type=22, netID=22, and aiStyle=7.");
    }

    if (input.HomeTileX == -1 || input.HomeTileY == -1)
    {
      return new NpcGuideRestingSpotSearchResult(
        input.HomeTileX,
        input.HomeTileY,
        UsedAlternateRestingSpot: false,
        AlternateRestingSpotOccupied: false,
        Branches: NpcGuideSourceBranch.None);
    }

    ArgumentNullException.ThrowIfNull(input.TileQuery);
    if (input.MaxTilesY <= 20)
    {
      throw new ArgumentOutOfRangeException(
        nameof(input.MaxTilesY),
        "The source floor search requires a world taller than its lower safety margin.");
    }

    int floorX = input.HomeTileX;
    int floorY = input.HomeTileY;
    while (!input.TileQuery.ReadTile(floorX, floorY).SolidOrSlopedOrPlatform &&
           floorY < input.MaxTilesY - 20)
    {
      floorY++;
    }

    if (input.DayTime ||
        (!input.DayTime && input.Ai0 == 5f &&
         Math.Abs(input.MyTileX - floorX) < 7 &&
         Math.Abs(input.MyTileY - floorY) < 7))
    {
      return new NpcGuideRestingSpotSearchResult(
        floorX,
        floorY,
        UsedAlternateRestingSpot: false,
        AlternateRestingSpotOccupied: false,
        Branches: NpcGuideSourceBranch.None);
    }

    if (input.IsTownSlime || input.Ai0 == 5f)
    {
      return new NpcGuideRestingSpotSearchResult(
        floorX,
        floorY,
        UsedAlternateRestingSpot: false,
        AlternateRestingSpotOccupied: false,
        Branches: NpcGuideSourceBranch.None);
    }

    int bestDistance = -1;
    int bestX = -1;
    int bestY = -1;
    for (int tileX = floorX - 7; tileX <= floorX + 7; tileX++)
    {
      for (int tileY = floorY + 2; tileY >= floorY - 6; tileY -= 2)
      {
        NpcGuideRestingSpotTile tile = input.TileQuery.ReadTile(tileX, tileY);
        if (!tile.Active ||
            !tile.CanBeSatOnForNpc ||
            (tile.FrameY % 40 == 0 && tileY + 1 > floorY + 2))
        {
          continue;
        }

        int distance = Math.Abs(tileX - floorX) + Math.Abs(tileY - floorY);
        if (bestDistance == -1 || distance < bestDistance)
        {
          bestDistance = distance;
          bestX = tileX;
          bestY = tileY;
        }
      }
    }

    if (bestDistance == -1)
    {
      return new NpcGuideRestingSpotSearchResult(
        floorX,
        floorY,
        UsedAlternateRestingSpot: false,
        AlternateRestingSpotOccupied: false,
        Branches: NpcGuideSourceBranch.None);
    }

    NpcGuideRestingSpotTile bestTile = input.TileQuery.ReadTile(bestX, bestY);
    if (bestTile.Type == 497 || bestTile.Type == 15)
    {
      if (bestTile.FrameY % 40 != 0)
      {
        bestY--;
      }

      bestY += 2;
    }

    ArgumentNullException.ThrowIfNull(input.OccupancyQuery);
    if (input.OccupancyQuery.IsOccupiedBySittingTownNpc(bestX, bestY))
    {
      return new NpcGuideRestingSpotSearchResult(
        floorX,
        floorY,
        UsedAlternateRestingSpot: false,
        AlternateRestingSpotOccupied: true,
        Branches: NpcGuideSourceBranch.None);
    }

    return new NpcGuideRestingSpotSearchResult(
      bestX,
      bestY,
      UsedAlternateRestingSpot: true,
      AlternateRestingSpotOccupied: false,
      Branches: NpcGuideSourceBranch.None);
  }
}

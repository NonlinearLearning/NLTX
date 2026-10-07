using System;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Housing;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Computes Version4's best home spot from classified Tile facts.
/// Occupancy and NPC assignment remain external effects.
/// </summary>
public static class HousingRoomScoreSystem
{
  private const int InitialScore = 50;
  private const int GoodEvilPenaltyThreshold = 50;
  private const int HorizontalRoomBoundsPadding = 46;
  private const int VerticalRoomBoundsPadding = 44;

  public static HousingRoomScoreSnapshot ScoreRoom(
    HousingRoomEvaluationResult room,
    int worldWidth,
    int worldHeight,
    IHousingRoomScoreTileSource tileSource,
    Func<TilePosition, bool> isInsideRoom,
    int? sharedRoomX = null)
  {
    ArgumentNullException.ThrowIfNull(tileSource);
    ArgumentNullException.ThrowIfNull(isInsideRoom);
    if (worldWidth <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldWidth));
    }

    if (worldHeight <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldHeight));
    }

    if (!room.CanSpawn || !room.HasRoomBounds)
    {
      return new HousingRoomScoreSnapshot(0, -1, null, null, sharedRoomX);
    }

    (int startX, int endX, int startY, int endY) = GetTestedBounds(
      room,
      worldWidth,
      worldHeight);
    int goodEvilBalance = 0;
    for (int x = startX + 1; x <= endX - 1; x++)
    {
      for (int y = startY + 2; y <= endY + 1; y++)
      {
        HousingRoomScoreTileSample tile = tileSource.ReadTile(new TilePosition(x, y));
        if (tile.IsActive)
        {
          goodEvilBalance = checked(
            goodEvilBalance + tile.GoodEvilBalanceContribution);
        }
      }
    }

    int goodEvilPenalty = -goodEvilBalance;
    if (goodEvilPenalty < GoodEvilPenaltyThreshold)
    {
      goodEvilPenalty = 0;
    }

    int baseScore = InitialScore - goodEvilPenalty;
    if (baseScore <= -250)
    {
      return new HousingRoomScoreSnapshot(
        room.RoomTileCount,
        baseScore,
        null,
        null,
        sharedRoomX,
        Array.Empty<HousingRoomScoreCandidate>(),
        baseScore);
    }

    int highScore = 0;
    bool hasStandingSpace = false;
    int? bestX = null;
    int? bestY = null;
    List<HousingRoomScoreCandidate> candidates = new();
    for (int x = room.RoomMinimum.X + 1; x < room.RoomMaximum.X; x++)
    {
      for (int y = room.RoomMinimum.Y + 2; y < room.RoomMaximum.Y + 2; y++)
      {
        HousingRoomScoreTileSample candidate = tileSource.ReadTile(new TilePosition(x, y));
        if (!candidate.IsSolidActive || candidate.IsHomeSpotForbidden ||
            tileSource.HasSolidTiles(
              new TilePosition(x - 1, y - 3),
              new TilePosition(x + 1, y - 1)) ||
            !tileSource.ReadTile(new TilePosition(x - 1, y)).IsSolidActive ||
            !tileSource.ReadTile(new TilePosition(x + 1, y)).IsSolidActive)
        {
          continue;
        }

        int score = baseScore;
        int furnitureCount = 0;
        int chestCount = 0;
        for (int scanX = x - 2; scanX <= x + 2; scanX++)
        {
          for (int scanY = y - 4; scanY < y; scanY++)
          {
            HousingRoomScoreTileSample tile = tileSource.ReadTile(
              new TilePosition(scanX, scanY));
            if (!tile.IsActive || tile.IsIgnoredInHouseScore)
            {
              continue;
            }

            if (tile.IsDoorTile && !tile.IsOpenDoorAnchorFrame)
            {
              continue;
            }

            if (scanX == x)
            {
              furnitureCount++;
            }
            else if (tile.IsBasicChest)
            {
              chestCount++;
            }
            else
            {
              score = checked(score + tile.ScoreContribution);
            }
          }
        }

        hasStandingSpace |= score > 0;
        if (sharedRoomX.HasValue && score >= 1 &&
            Math.Abs(sharedRoomX.Value - x) < 3)
        {
          score = 1;
        }

        if (score > 0 && chestCount > 0)
        {
          score = Math.Max(1, score - 30 * chestCount);
        }

        if (score > 0 && furnitureCount > 0)
        {
          score = Math.Max(1, score - 15 * furnitureCount);
        }

        bool isNewHighScore = score > highScore && HasValidHeadroom(
          x,
          y,
          worldWidth,
          worldHeight,
          tileSource,
          isInsideRoom);
        candidates.Add(new HousingRoomScoreCandidate(x, y, score, isNewHighScore));
        if (!isNewHighScore)
        {
          continue;
        }

        highScore = score;
        bestX = x;
        bestY = y;
      }
    }

    return new HousingRoomScoreSnapshot(
      room.RoomTileCount,
      highScore,
      bestX,
      bestY,
      sharedRoomX,
      candidates.ToArray(),
      baseScore,
      hasStandingSpace);
  }

  private static (int StartX, int EndX, int StartY, int EndY) GetTestedBounds(
    HousingRoomEvaluationResult room,
    int worldWidth,
    int worldHeight)
  {
    int startX = Math.Max(5, room.RoomMinimum.X - HorizontalRoomBoundsPadding);
    int endX = Math.Min(worldWidth - 6, room.RoomMaximum.X + HorizontalRoomBoundsPadding);
    int startY = Math.Max(5, room.RoomMinimum.Y - VerticalRoomBoundsPadding);
    int endY = Math.Min(worldHeight - 6, room.RoomMaximum.Y + VerticalRoomBoundsPadding);
    return (startX, endX, startY, endY);
  }

  private static bool HasValidHeadroom(
    int x,
    int y,
    int worldWidth,
    int worldHeight,
    IHousingRoomScoreTileSource tileSource,
    Func<TilePosition, bool> isInsideRoom)
  {
    if (!isInsideRoom(new TilePosition(x, y)))
    {
      return false;
    }

    for (int offset = 1; offset <= 3; offset++)
    {
      TilePosition position = new(x, y - offset);
      if (!isInsideRoom(position) || position.X < 0 || position.X >= worldWidth ||
          position.Y < 0 || position.Y >= worldHeight ||
          tileSource.ReadTile(position).IsSolidActive)
      {
        return false;
      }
    }

    return true;
  }
}

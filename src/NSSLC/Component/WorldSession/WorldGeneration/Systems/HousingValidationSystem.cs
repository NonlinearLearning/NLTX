using System;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Housing;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Evaluates one room through an explicit tile source. It does not mutate NPC or registry state.
/// </summary>
public static class HousingValidationSystem
{
  public static bool TryEvaluateRoom(
    TilePosition startPosition,
    int worldWidth,
    int worldHeight,
    HousingValidationDefinition definition,
    IHousingTileSource tileSource,
    out HousingRoomEvaluationResult result)
  {
    ArgumentNullException.ThrowIfNull(tileSource);
    if (worldWidth <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldWidth));
    }

    if (worldHeight <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldHeight));
    }

    HousingRoomSearchContext context = new(
      startPosition,
      definition.MaxRoomTiles,
      definition.MaxRoomSize,
      definition.TileTypeCount);

    if (IsOutsideRoomMargin(
          startPosition,
          worldWidth,
          worldHeight,
          definition.WorldEdgeMargin))
    {
      result = Failure(
        HousingRoomValidationFailure.TooCloseToWorldEdge,
        context,
        default,
        false,
        false);
      return false;
    }

    HousingTileSample startTile = tileSource.ReadTile(startPosition);
    if (startTile.IsActive && startTile.IsSolid)
    {
      result = Failure(
        HousingRoomValidationFailure.StartedInSolidTile,
        context,
        default,
        false,
        false);
      return false;
    }

    context.PushRoomCheck(startPosition);
    bool hasTorch = false;
    bool hasDoor = false;
    bool hasChair = false;
    bool hasTable = false;
    bool hasStinkbug = false;
    bool hasEchoStinkbug = false;

    while (context.TryPopRoomCheck(out TilePosition position))
    {
      if (IsOutsideRoomMargin(position, worldWidth, worldHeight, definition.WorldEdgeMargin))
      {
        result = Failure(
          HousingRoomValidationFailure.TooCloseToWorldEdge,
          context,
          new HousingRoomRequirementResult(hasTorch, hasDoor, hasChair, hasTable, false),
          hasStinkbug,
          hasEchoStinkbug);
        return false;
      }

      if (context.VisitedTiles.Contains(position))
      {
        continue;
      }

      if (context.VisitedTiles.IsAtTileLimit ||
          context.VisitedTiles.WouldExceedRoomSize(position))
      {
        result = Failure(
          HousingRoomValidationFailure.RoomTooBig,
          context,
          new HousingRoomRequirementResult(hasTorch, hasDoor, hasChair, hasTable, false),
          hasStinkbug,
          hasEchoStinkbug);
        return false;
      }

      context.VisitedTiles.TryAdd(position);
      HousingTileSample tile = tileSource.ReadTile(position);
      if (tile.IsActive && (tile.IsSolid || tile.IsOpenGate))
      {
        // Version4 treats a solid tile or an open gate reached by the flood fill
        // as a boundary. The start tile is checked separately above.
        continue;
      }

      hasTorch |= tile.HasTorch;
      hasDoor |= tile.HasDoor;
      hasChair |= tile.HasChair;
      hasTable |= tile.HasTable;
      hasStinkbug |= tile.IsStinkbug;
      hasEchoStinkbug |= tile.IsEchoStinkbug;

      if (!HasHorizontalHouseWall(position, worldWidth, tileSource) ||
          !HasVerticalHouseWall(position, worldHeight, tileSource))
      {
        HousingRoomValidationFailure failure = tile.HasAnyWall
          ? HousingRoomValidationFailure.UnsafeWall
          : HousingRoomValidationFailure.MissingWall;
        result = Failure(
          failure,
          context,
          new HousingRoomRequirementResult(hasTorch, hasDoor, hasChair, hasTable, false),
          hasStinkbug,
          hasEchoStinkbug);
        return false;
      }

      for (int offset = -1; offset <= 1; offset++)
      {
        for (int otherOffset = -1; otherOffset <= 1; otherOffset++)
        {
          if (offset != 0 || otherOffset != 0)
          {
            context.PushRoomCheck(new TilePosition(
              position.X + offset,
              position.Y + otherOffset));
          }
        }
      }
    }

    HousingRoomRequirementResult requirements = new(
      hasTorch,
      hasDoor,
      hasChair,
      hasTable,
      hasTorch && hasDoor && hasChair && hasTable);
    if (context.VisitedTiles.NumRoomTiles < definition.MinimumRoomTiles)
    {
      result = Failure(
        HousingRoomValidationFailure.RoomTooSmall,
        context,
        requirements,
        hasStinkbug,
        hasEchoStinkbug);
      return false;
    }

    if (!requirements.CanSpawn)
    {
      result = Failure(
        HousingRoomValidationFailure.MissingRequirement,
        context,
        requirements,
        hasStinkbug,
        hasEchoStinkbug);
      return false;
    }

    result = new HousingRoomEvaluationResult(
      true,
      HousingRoomValidationFailure.None,
      context.VisitedTiles.NumRoomTiles,
      new TilePosition(context.VisitedTiles.RoomX1, context.VisitedTiles.RoomY1),
      new TilePosition(context.VisitedTiles.RoomX2, context.VisitedTiles.RoomY2),
      requirements,
      hasStinkbug,
      hasEchoStinkbug);
    return true;
  }

  private static HousingRoomEvaluationResult Failure(
    HousingRoomValidationFailure failure,
    HousingRoomSearchContext context,
    HousingRoomRequirementResult requirements,
    bool hasStinkbug,
    bool hasEchoStinkbug)
  {
    return new HousingRoomEvaluationResult(
      false,
      failure,
      context.VisitedTiles.NumRoomTiles,
      new TilePosition(context.VisitedTiles.RoomX1, context.VisitedTiles.RoomY1),
      new TilePosition(context.VisitedTiles.RoomX2, context.VisitedTiles.RoomY2),
      requirements,
      hasStinkbug,
      hasEchoStinkbug);
  }

  private static bool HasHorizontalHouseWall(
    TilePosition position,
    int worldWidth,
    IHousingTileSource tileSource)
  {
    for (int offset = -2; offset <= 2; offset++)
    {
      if (position.X + offset < 0 || position.X + offset >= worldWidth)
      {
        continue;
      }

      HousingTileSample sample = tileSource.ReadTile(
        new TilePosition(position.X + offset, position.Y));
      if (sample.IsHouseWall || (sample.IsActive && sample.IsSolid))
      {
        return true;
      }
    }

    return false;
  }

  private static bool HasVerticalHouseWall(
    TilePosition position,
    int worldHeight,
    IHousingTileSource tileSource)
  {
    for (int offset = -2; offset <= 2; offset++)
    {
      if (position.Y + offset < 0 || position.Y + offset >= worldHeight)
      {
        continue;
      }

      HousingTileSample sample = tileSource.ReadTile(
        new TilePosition(position.X, position.Y + offset));
      if (sample.IsHouseWall || (sample.IsActive && sample.IsSolid))
      {
        return true;
      }
    }

    return false;
  }

  private static bool IsOutsideRoomMargin(
    TilePosition position,
    int worldWidth,
    int worldHeight,
    int worldEdgeMargin)
  {
    return position.X < worldEdgeMargin ||
      position.Y < worldEdgeMargin ||
      position.X >= worldWidth - worldEdgeMargin ||
      position.Y >= worldHeight - worldEdgeMargin;
  }
}

namespace Terraria.Player.Interaction;

public static class PlayerTargetingSystem
{
  private const int AxeNeighborCorrectionTileType = 323;
  private const int DefaultTileRangeX = 5;
  private const int DefaultTileRangeY = 3;
  private const int JourneyRangeMultiplier = 2;
  private const int JourneyRangeBonus = 8;

  public static PlayerTileTargetRange ResolveRangeBaseline(
    in PlayerTileTargetRangeInput input)
  {
    if (!input.IsLocalPlayer || input.IsDisplayDollOrInanimate)
    {
      return new PlayerTileTargetRange(input.CurrentTileRangeX, input.CurrentTileRangeY);
    }

    int tileRangeX = DefaultTileRangeX;
    int tileRangeY = DefaultTileRangeY;
    if (input.IsJourneyMode &&
      input.FarPlacementRangePowerUnlocked &&
      input.FarPlacementRangePowerEnabledForPlayer)
    {
      tileRangeX = tileRangeX * JourneyRangeMultiplier + JourneyRangeBonus;
      tileRangeY = tileRangeY * JourneyRangeMultiplier + JourneyRangeBonus;
    }

    return new PlayerTileTargetRange(tileRangeX, tileRangeY);
  }

  public static void CaptureEffectiveRange(
    PlayerTileTargetingAndRangeStateComponent state,
    in PlayerTileTargetRange range)
  {
    ArgumentNullException.ThrowIfNull(state);

    state.LastTileRangeX = range.TileRangeX;
    state.LastTileRangeY = range.TileRangeY;
  }

  public static PlayerTileTargetCoordinate ResolveTarget(
    in PlayerTileTargetCoordinateInput coordinateInput,
    in PlayerTileTargetToolInput toolInput,
    IPlayerTileTargetWorldPort worldPort)
  {
    ArgumentNullException.ThrowIfNull(worldPort);

    PlayerTileTargetCoordinate coordinate = ResolveCoordinate(coordinateInput);
    worldPort.EnsureTileExists(coordinate.TileTargetX - 1, coordinate.TileTargetY);
    worldPort.EnsureTileExists(coordinate.TileTargetX + 1, coordinate.TileTargetY);
    worldPort.EnsureTileExists(coordinate.TileTargetX, coordinate.TileTargetY);

    if (toolInput.AxePower <= 0)
    {
      return coordinate;
    }

    PlayerTileTargetTileFacts centerTile = worldPort.ReadTile(
      coordinate.TileTargetX,
      coordinate.TileTargetY);
    if (centerTile.IsActive ||
      toolInput.CreateWall > 0 ||
      !(toolInput.HammerPower <= 0 || toolInput.AxePower != 0))
    {
      return coordinate;
    }

    PlayerTileTargetTileFacts leftTile = worldPort.ReadTile(
      coordinate.TileTargetX - 1,
      coordinate.TileTargetY);
    if (IsAxeCorrectionTile(leftTile))
    {
      return ResolveAxeNeighborTarget(
        coordinate,
        CreateAxeCorrectionInput(toolInput, centerTile, leftTile, default));
    }

    PlayerTileTargetTileFacts rightTile = worldPort.ReadTile(
      coordinate.TileTargetX + 1,
      coordinate.TileTargetY);
    return ResolveAxeNeighborTarget(
      coordinate,
      CreateAxeCorrectionInput(toolInput, centerTile, leftTile, rightTile));
  }

  public static PlayerTileTargetCoordinate ResolveCoordinate(
    in PlayerTileTargetCoordinateInput input)
  {
    int tileTargetX = (int)(((float)input.MouseX + input.ScreenPositionX) / 16f);
    int tileTargetY = (int)(((float)input.MouseY + input.ScreenPositionY) / 16f);
    if (input.GravityDirection == -1f)
    {
      tileTargetY = (int)(
        (input.ScreenPositionY + (float)input.ScreenHeight - (float)input.MouseY) / 16f);
    }

    if (tileTargetX >= input.MaxTilesX - 5)
    {
      tileTargetX = input.MaxTilesX - 5;
    }
    if (tileTargetY >= input.MaxTilesY - 5)
    {
      tileTargetY = input.MaxTilesY - 5;
    }
    if (tileTargetX < 5)
    {
      tileTargetX = 5;
    }
    if (tileTargetY < 5)
    {
      tileTargetY = 5;
    }

    return new PlayerTileTargetCoordinate(tileTargetX, tileTargetY);
  }

  public static PlayerTileTargetCoordinate ResolveAxeNeighborTarget(
    in PlayerTileTargetCoordinate coordinate,
    in PlayerTileTargetAxeCorrectionInput input)
  {
    int tileTargetX = coordinate.TileTargetX;
    if (input.AxePower > 0 &&
      !input.CenterTileActive &&
      input.CreateWall <= 0 &&
      (input.HammerPower <= 0 || input.AxePower != 0))
    {
      if (IsAxeCorrectionTile(input.LeftTileActive, input.LeftTileType))
      {
        if (input.LeftTileFrameY > 4)
        {
          tileTargetX--;
        }
      }
      else if (IsAxeCorrectionTile(input.RightTileActive, input.RightTileType) &&
        input.RightTileFrameY < -4)
      {
        tileTargetX++;
      }
    }

    return new PlayerTileTargetCoordinate(tileTargetX, coordinate.TileTargetY);
  }

  private static bool IsAxeCorrectionTile(in PlayerTileTargetTileFacts tile)
  {
    return IsAxeCorrectionTile(tile.IsActive, tile.Type);
  }

  private static bool IsAxeCorrectionTile(bool isActive, int tileType)
  {
    return isActive && tileType == AxeNeighborCorrectionTileType;
  }

  private static PlayerTileTargetAxeCorrectionInput CreateAxeCorrectionInput(
    in PlayerTileTargetToolInput toolInput,
    in PlayerTileTargetTileFacts centerTile,
    in PlayerTileTargetTileFacts leftTile,
    in PlayerTileTargetTileFacts rightTile)
  {
    return new PlayerTileTargetAxeCorrectionInput(
      toolInput.AxePower,
      centerTile.IsActive,
      toolInput.CreateWall,
      toolInput.HammerPower,
      leftTile.IsActive,
      leftTile.Type,
      leftTile.FrameY,
      rightTile.IsActive,
      rightTile.Type,
      rightTile.FrameY);
  }
}

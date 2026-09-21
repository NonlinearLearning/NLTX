using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldObjects.Placement;

public sealed class WorldObjectPlacementDefinition
{
  private readonly HashSet<int> _allowedDirections;

  public WorldObjectPlacementDefinition(
    ushort objectType,
    int width,
    int height,
    int originOffsetX,
    int originOffsetY,
    int coordinateWidth,
    int coordinatePadding,
    IReadOnlyList<int> coordinateHeights,
    bool styleHorizontal,
    int styleMultiplier,
    int styleWrapLimit,
    int randomStyleRange,
    WorldObjectPlacementAnchorKind anchorKind,
    IReadOnlyList<int> allowedDirections,
    bool supportsAlternate,
    bool supportsRandom,
    bool usesCustomCanPlace = false,
    int drawYOffset = 0,
    bool lavaDeath = true,
    int defaultDirection = 0)
  {
    ArgumentNullException.ThrowIfNull(coordinateHeights);
    ArgumentNullException.ThrowIfNull(allowedDirections);
    if (objectType == 0 || width <= 0 || height <= 0 || originOffsetX < 0 ||
        originOffsetY < 0 || originOffsetX >= width || originOffsetY >= height ||
        coordinateWidth <= 0 || coordinatePadding < 0 ||
        coordinateHeights.Count != height || styleMultiplier <= 0 || styleWrapLimit < 0 ||
        randomStyleRange < 0 || allowedDirections.Count == 0 || !Enum.IsDefined(anchorKind) ||
        drawYOffset < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(objectType));
    }

    int[] heights = new int[coordinateHeights.Count];
    for (int index = 0; index < coordinateHeights.Count; index++)
    {
      if (coordinateHeights[index] <= 0)
      {
        throw new ArgumentOutOfRangeException(nameof(coordinateHeights));
      }

      heights[index] = coordinateHeights[index];
    }

    int[] directions = new int[allowedDirections.Count];
    HashSet<int> directionSet = new();
    for (int index = 0; index < allowedDirections.Count; index++)
    {
      int direction = allowedDirections[index];
      if (!directionSet.Add(direction))
      {
        throw new ArgumentException(
          "World-object placement directions must be unique.",
          nameof(allowedDirections));
      }

      directions[index] = direction;
    }

    ObjectType = objectType;
    Width = width;
    Height = height;
    OriginOffsetX = originOffsetX;
    OriginOffsetY = originOffsetY;
    CoordinateWidth = coordinateWidth;
    CoordinatePadding = coordinatePadding;
    CoordinateHeights = Array.AsReadOnly(heights);
    StyleHorizontal = styleHorizontal;
    StyleMultiplier = styleMultiplier;
    StyleWrapLimit = styleWrapLimit;
    RandomStyleRange = randomStyleRange;
    AnchorKind = anchorKind;
    AllowedDirections = Array.AsReadOnly(directions);
    SupportsAlternate = supportsAlternate;
    SupportsRandom = supportsRandom;
    UsesCustomCanPlace = usesCustomCanPlace;
    DrawYOffset = drawYOffset;
    LavaDeath = lavaDeath;
    DefaultDirection = defaultDirection;
    _allowedDirections = directionSet;
  }

  public ushort ObjectType { get; }
  public int Width { get; }
  public int Height { get; }
  public int OriginOffsetX { get; }
  public int OriginOffsetY { get; }
  public (int X, int Y) OriginOffset => (OriginOffsetX, OriginOffsetY);
  public int CoordinateWidth { get; }
  public int CoordinatePadding { get; }
  public IReadOnlyList<int> CoordinateHeights { get; }
  public bool StyleHorizontal { get; }
  public int StyleMultiplier { get; }
  public int StyleWrapLimit { get; }
  public int RandomStyleRange { get; }
  public WorldObjectPlacementAnchorKind AnchorKind { get; }
  public IReadOnlyList<int> AllowedDirections { get; }
  public bool SupportsAlternate { get; }
  public bool SupportsRandom { get; }
  public bool UsesCustomCanPlace { get; }
  public int DrawYOffset { get; }
  public bool LavaDeath { get; }
  public int DefaultDirection { get; }

  public int CoordinateStep => checked(CoordinateWidth + CoordinatePadding);

  public int StyleFullWidth => checked(Width * CoordinateStep * StyleMultiplier);

  public bool IsValidStyle(int style)
  {
    return style >= 0 && (StyleWrapLimit == 0 || style < StyleWrapLimit) &&
      TryGetFrame(style, 0, 0, out _, out _);
  }

  public bool IsValidDirection(int direction)
  {
    return _allowedDirections.Contains(direction);
  }

  public bool TryGetFrame(
    int style,
    int column,
    int row,
    out short frameX,
    out short frameY)
  {
    frameX = 0;
    frameY = 0;
    if (style < 0 || column < 0 || column >= Width || row < 0 || row >= Height ||
        StyleWrapLimit != 0 && style >= StyleWrapLimit)
    {
      return false;
    }

    try
    {
      long styleBand = checked((long)style * StyleMultiplier);
      long horizontalOffset = checked((long)column * CoordinateStep);
      long calculatedFrameX = StyleHorizontal
        ? checked(styleBand * StyleFullWidth + horizontalOffset)
        : horizontalOffset;
      long calculatedFrameY = 0;
      for (int index = 0; index < row; index++)
      {
        calculatedFrameY = checked(calculatedFrameY + CoordinateHeights[index] + CoordinatePadding);
      }

      if (calculatedFrameX < short.MinValue || calculatedFrameX > short.MaxValue ||
          calculatedFrameY < short.MinValue || calculatedFrameY > short.MaxValue)
      {
        return false;
      }

      frameX = (short)calculatedFrameX;
      frameY = (short)calculatedFrameY;
      return true;
    }
    catch (OverflowException)
    {
      return false;
    }
  }

  public bool HasValidAnchor(
    WorldGrid world,
    int originX,
    int originY,
    TileDefinitionRegistry tileDefinitions)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    if (AnchorKind != WorldObjectPlacementAnchorKind.Bottom)
    {
      return false;
    }

    int supportY;
    try
    {
      supportY = checked(originY + Height);
    }
    catch (OverflowException)
    {
      return false;
    }

    for (int column = 0; column < Width; column++)
    {
      int supportX;
      try
      {
        supportX = checked(originX + column);
      }
      catch (OverflowException)
      {
        return false;
      }
      if (!world.Contains(supportX, supportY) ||
          !TileStateQuery.IsSolidAllowingBottomSlope(
            world.GetTile(supportX, supportY),
            tileDefinitions))
      {
        return false;
      }
    }

    return true;
  }
}

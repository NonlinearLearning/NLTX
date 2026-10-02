using System.Numerics;

namespace Terraria.WorldGeneration.Definitions;

public struct WorldLandmassDefinition
{
  public WorldLandmassDefinition(
    WorldLandmassDataType dataType,
    Vector2 position,
    int radiusOrHalfSize,
    int style)
  {
    DataType = dataType;
    Position = position;
    RadiusOrHalfSize = radiusOrHalfSize;
    Style = style;
  }

  public WorldLandmassDataType DataType { get; set; }

  public Vector2 Position { get; set; }

  public int RadiusOrHalfSize { get; set; }

  public int Style { get; set; }

  public Vector2 Top
  {
    get => Position - new Vector2(0f, RadiusOrHalfSize);
    set => Position = value + new Vector2(0f, RadiusOrHalfSize);
  }
}

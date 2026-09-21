namespace Terraria.WorldGeneration.Support;

public readonly record struct PaintingDefinition
{
  public PaintingDefinition(int tileType, int style)
  {
    TileType = tileType;
    Style = style;
  }

  public int TileType { get; }

  public int Style { get; }
}

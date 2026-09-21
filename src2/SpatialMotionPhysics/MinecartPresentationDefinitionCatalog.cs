using System.Numerics;

namespace Terraria.SpatialMotionPhysics;

public sealed class MinecartPresentationDefinitionCatalog
{
  private readonly Vector2[] _texturePositions;
  private readonly int[] _tileHeights;

  private MinecartPresentationDefinitionCatalog(
    Vector2[] texturePositions,
    int[] tileHeights)
  {
    _texturePositions = texturePositions;
    _tileHeights = tileHeights;
  }

  public int TotalFrames => 36;

  public static MinecartPresentationDefinitionCatalog CreateDefault()
  {
    return new MinecartPresentationDefinitionCatalog(
      new[] { Vector2.Zero },
      new[] { 1 });
  }

  internal Vector2 GetTexturePosition(int frame)
  {
    return _texturePositions[frame];
  }

  internal int GetTileHeight(int frame)
  {
    return _tileHeights[frame];
  }
}

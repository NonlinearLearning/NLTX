using System;

namespace Terraria.World;

/// <summary>Half-open integer rectangle in tile coordinates.</summary>
/// <remarks>
/// Version4 pass the equivalent <c>Rectangle</c> around for spawn areas, safe areas and tile
/// scans. Keeping it in the World domain lets every solver share one inclusive/exclusive
/// convention: <see cref="Left"/> and <see cref="Top"/> are inclusive, <see cref="Right"/> and
/// <see cref="Bottom"/> are exclusive.
/// </remarks>
public readonly record struct WorldTileArea(int X, int Y, int Width, int Height)
{
  public int Left => X;

  public int Top => Y;

  public int Right => X + Width;

  public int Bottom => Y + Height;

  public bool IsValid => Width > 0 && Height > 0;

  public bool Contains(int tileX, int tileY)
    => tileX >= Left && tileX < Right && tileY >= Top && tileY < Bottom;

  public bool Intersects(WorldTileArea other)
    => IsValid && other.IsValid &&
        Left < other.Right && other.Left < Right &&
        Top < other.Bottom && other.Top < Bottom;

  /// <summary>Builds a rectangle from a centre tile and a full extent, like <c>CenteredRectangle</c>.</summary>
  public static WorldTileArea Centered(int centerTileX, int centerTileY, int width, int height)
    => new(centerTileX - width / 2, centerTileY - height / 2, width, height);

  /// <summary>Clips the rectangle to the world, as <c>WorldUtils.ClampToWorld</c> does.</summary>
  public WorldTileArea ClampToWorld(int worldWidth, int worldHeight)
  {
    if (worldWidth <= 0 || worldHeight <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldWidth));
    }

    int left = Math.Clamp(Left, 0, worldWidth);
    int top = Math.Clamp(Top, 0, worldHeight);
    int right = Math.Clamp(Right, 0, worldWidth);
    int bottom = Math.Clamp(Bottom, 0, worldHeight);
    return right > left && bottom > top
        ? new WorldTileArea(left, top, right - left, bottom - top)
        : default;
  }
}

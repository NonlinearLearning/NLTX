namespace Terraria.SpatialMotionPhysics;

public sealed class EntityBoundsComponent
{
  public EntityBoundsComponent(int width, int height)
  {
    if (width < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    if (height < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(height));
    }

    Width = width;
    Height = height;
  }

  public int Width { get; private set; }

  public int Height { get; private set; }

  internal void SetSize(int width, int height)
  {
    if (width < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    if (height < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(height));
    }

    Width = width;
    Height = height;
  }
}

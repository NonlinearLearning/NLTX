namespace Terraria.EntityLifecycleAttribution;

public sealed class DirectionComponent
{
  public DirectionComponent(int direction)
  {
    Direction = Validate(direction);
  }

  public int Direction { get; private set; }

  public void SetDirection(int direction)
  {
    Direction = Validate(direction);
  }

  private static int Validate(int direction)
  {
    if (direction is not (-1 or 1))
    {
      throw new ArgumentOutOfRangeException(nameof(direction), "Direction must be -1 or 1.");
    }

    return direction;
  }
}

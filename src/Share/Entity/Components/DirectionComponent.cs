namespace EntityEcs.Components;

public struct DirectionComponent
{
  public DirectionComponent(int horizontal)
  {
    Horizontal = horizontal;
  }

  public int Horizontal;
}

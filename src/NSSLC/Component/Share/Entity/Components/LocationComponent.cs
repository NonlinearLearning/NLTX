namespace EntityEcs.Components;

public struct LocationComponent
{
  public LocationComponent(float x, float y)
  {
    X = x;
    Y = y;
  }

  public float X;
  public float Y;
}

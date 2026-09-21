namespace Terraria.ClientPresentation.Ui.Layout;

public readonly record struct UiCalculatedStyle(float X, float Y, float Width, float Height)
{
  public float Right => X + Width;

  public float Bottom => Y + Height;

  public UiVector2 Position => new UiVector2(X, Y);

  public UiRectangle ToRectangle()
  {
    return new UiRectangle(X, Y, Width, Height);
  }

  public bool Contains(UiVector2 point)
  {
    return ToRectangle().Contains(point);
  }
}

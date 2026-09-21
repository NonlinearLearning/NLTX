namespace Terraria.ClientPresentation.Ui.Layout;

public readonly record struct UiRectangle(float X, float Y, float Width, float Height)
{
  public float Right => X + Width;

  public float Bottom => Y + Height;

  public bool Contains(UiVector2 point)
  {
    return point.X >= X
      && point.X <= Right
      && point.Y >= Y
      && point.Y <= Bottom;
  }
}

namespace Terraria.ClientPresentation.Ui.Layout;

public readonly record struct UiVector2(float X, float Y)
{
  public static UiVector2 Zero => new(0f, 0f);

  public static UiVector2 operator +(UiVector2 left, UiVector2 right)
  {
    return new UiVector2(left.X + right.X, left.Y + right.Y);
  }

  public static UiVector2 operator -(UiVector2 left, UiVector2 right)
  {
    return new UiVector2(left.X - right.X, left.Y - right.Y);
  }
}

namespace Terraria.ClientPresentation.Ui.Layout;

public readonly record struct UiSnapPoint(
  string Name,
  UiVector2 Anchor,
  UiVector2 Offset,
  int Id)
{
  public UiVector2 Calculate(UiCalculatedStyle dimensions)
  {
    return new UiVector2(
      dimensions.X + dimensions.Width * Anchor.X + Offset.X,
      dimensions.Y + dimensions.Height * Anchor.Y + Offset.Y);
  }
}

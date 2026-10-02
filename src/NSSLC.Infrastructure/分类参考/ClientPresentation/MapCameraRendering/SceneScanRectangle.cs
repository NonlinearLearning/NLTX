namespace NLTX.ClientPresentation.MapCameraRendering;

public readonly record struct SceneScanRectangle(int X, int Y, int Width, int Height)
{
  public SceneScanRectangle Normalize()
  {
    int normalizedX = Width < 0 ? X + Width : X;
    int normalizedY = Height < 0 ? Y + Height : Y;
    return new SceneScanRectangle(
      normalizedX,
      normalizedY,
      Math.Abs(Width),
      Math.Abs(Height));
  }

  public bool Contains(int x, int y)
  {
    SceneScanRectangle normalized = Normalize();
    return x >= normalized.X &&
      y >= normalized.Y &&
      x < normalized.X + normalized.Width &&
      y < normalized.Y + normalized.Height;
  }
}

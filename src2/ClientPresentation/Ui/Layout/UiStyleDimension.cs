namespace Terraria.ClientPresentation.Ui.Layout;

public readonly record struct UiStyleDimension
{
  public UiStyleDimension(float pixels, float percent)
  {
    if (!float.IsFinite(pixels))
    {
      throw new ArgumentOutOfRangeException(nameof(pixels));
    }

    if (!float.IsFinite(percent))
    {
      throw new ArgumentOutOfRangeException(nameof(percent));
    }

    Pixels = pixels;
    Percent = percent;
  }

  public float Pixels { get; }

  public float Percent { get; }

  public float Precent => Percent;

  public static UiStyleDimension Empty => new UiStyleDimension(0f, 0f);

  public static UiStyleDimension Fill => new UiStyleDimension(0f, 1f);

  public static UiStyleDimension FromPixels(float pixels)
  {
    return new UiStyleDimension(pixels, 0f);
  }

  public static UiStyleDimension FromPercent(float percent)
  {
    return new UiStyleDimension(0f, percent);
  }

  public static UiStyleDimension FromPixelsAndPercent(float pixels, float percent)
  {
    return new UiStyleDimension(pixels, percent);
  }

  public float GetValue(float containerSize)
  {
    if (!float.IsFinite(containerSize))
    {
      throw new ArgumentOutOfRangeException(nameof(containerSize));
    }

    return Pixels + Percent * containerSize;
  }
}

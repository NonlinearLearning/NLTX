namespace Terraria.ClientPresentation.Ui.Layout;

public static class UiLayoutValueQuery
{
  public static UiCalculatedStyle Calculate(
    UiStyleDimension left,
    UiStyleDimension top,
    UiStyleDimension width,
    UiStyleDimension height,
    UiCalculatedStyle parent,
    float horizontalAlignment = 0f,
    float verticalAlignment = 0f,
    float marginLeft = 0f,
    float marginTop = 0f,
    float marginRight = 0f,
    float marginBottom = 0f)
  {
    float calculatedWidth = width.GetValue(parent.Width);
    float calculatedHeight = height.GetValue(parent.Height);
    float availableWidth = parent.Width - calculatedWidth - marginLeft - marginRight;
    float availableHeight = parent.Height - calculatedHeight - marginTop - marginBottom;
    float x = parent.X
      + left.GetValue(parent.Width)
      + marginLeft
      + availableWidth * ClampUnit(horizontalAlignment);
    float y = parent.Y
      + top.GetValue(parent.Height)
      + marginTop
      + availableHeight * ClampUnit(verticalAlignment);

    return new UiCalculatedStyle(x, y, calculatedWidth, calculatedHeight);
  }

  public static float Clamp(
    float value,
    UiStyleDimension minimum,
    UiStyleDimension maximum,
    float containerSize)
  {
    float minimumValue = minimum.GetValue(containerSize);
    float maximumValue = maximum.GetValue(containerSize);
    if (maximumValue < minimumValue)
    {
      maximumValue = minimumValue;
    }

    return Math.Clamp(value, minimumValue, maximumValue);
  }

  private static float ClampUnit(float value)
  {
    if (!float.IsFinite(value))
    {
      throw new ArgumentOutOfRangeException(nameof(value));
    }

    return Math.Clamp(value, 0f, 1f);
  }
}

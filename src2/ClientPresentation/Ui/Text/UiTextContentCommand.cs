namespace Terraria.ClientPresentation.Ui.Text;

public sealed record UiTextContentCommand
{
  private UiTextContentCommand(
    OperationKind operation,
    UiTextPanelComponent.TextSource source,
    float value,
    float secondaryValue,
    float tertiaryValue,
    bool isLarge,
    bool isWrapped,
    bool dynamicallyScaleDownToWidth,
    uint color,
    uint shadowColor)
  {
    Operation = operation;
    Source = source;
    Value = value;
    SecondaryValue = secondaryValue;
    TertiaryValue = tertiaryValue;
    IsLarge = isLarge;
    IsWrapped = isWrapped;
    DynamicallyScaleDownToWidth = dynamicallyScaleDownToWidth;
    Color = color;
    ShadowColor = shadowColor;
  }

  public enum OperationKind
  {
    SetSource,
    SetScale,
    SetStyle,
    SetColors
  }

  public OperationKind Operation { get; }

  public UiTextPanelComponent.TextSource Source { get; }

  public float Value { get; }

  public float SecondaryValue { get; }

  public float TertiaryValue { get; }

  public bool IsLarge { get; }

  public bool IsWrapped { get; }

  public bool DynamicallyScaleDownToWidth { get; }

  public uint Color { get; }

  public uint ShadowColor { get; }

  public static UiTextContentCommand SetSource(
    UiTextPanelComponent.TextSource source)
  {
    return new UiTextContentCommand(
      OperationKind.SetSource,
      source,
      0f,
      0f,
      0f,
      false,
      false,
      false,
      0,
      0);
  }

  public static UiTextContentCommand SetScale(float scale)
  {
    return new UiTextContentCommand(
      OperationKind.SetScale,
      UiTextPanelComponent.TextSource.Empty,
      scale,
      0f,
      0f,
      false,
      false,
      false,
      0,
      0);
  }

  public static UiTextContentCommand SetStyle(
    bool isLarge,
    bool isWrapped,
    bool dynamicallyScaleDownToWidth,
    float wrappedTextBottomPadding,
    float originX,
    float originY)
  {
    return new UiTextContentCommand(
      OperationKind.SetStyle,
      UiTextPanelComponent.TextSource.Empty,
      wrappedTextBottomPadding,
      originX,
      originY,
      isLarge,
      isWrapped,
      dynamicallyScaleDownToWidth,
      0,
      0);
  }

  public static UiTextContentCommand SetColors(uint color, uint shadowColor)
  {
    return new UiTextContentCommand(
      OperationKind.SetColors,
      UiTextPanelComponent.TextSource.Empty,
      0f,
      0f,
      0f,
      false,
      false,
      false,
      color,
      shadowColor);
  }
}

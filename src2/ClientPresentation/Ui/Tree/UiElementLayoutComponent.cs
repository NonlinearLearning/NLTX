using Terraria.ClientPresentation.Ui.Layout;

namespace Terraria.ClientPresentation.Ui.Tree;

public sealed class UiElementLayoutComponent
{
  private readonly List<UiElementId> _children = [];

  public UiElementLayoutComponent(UiElementId id)
  {
    if (!id.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(id));
    }

    Id = id;
  }

  public UiElementId Id { get; }

  public UiElementId? Parent { get; private set; }

  public UiStyleDimension Top { get; private set; } = UiStyleDimension.Empty;

  public UiStyleDimension Left { get; private set; } = UiStyleDimension.Empty;

  public UiStyleDimension Width { get; private set; } = UiStyleDimension.Empty;

  public UiStyleDimension Height { get; private set; } = UiStyleDimension.Empty;

  public UiStyleDimension MaxWidth { get; private set; } = UiStyleDimension.Fill;

  public UiStyleDimension MaxHeight { get; private set; } = UiStyleDimension.Fill;

  public UiStyleDimension MinWidth { get; private set; } = UiStyleDimension.Empty;

  public UiStyleDimension MinHeight { get; private set; } = UiStyleDimension.Empty;

  public float PaddingTop { get; private set; }

  public float PaddingLeft { get; private set; }

  public float PaddingRight { get; private set; }

  public float PaddingBottom { get; private set; }

  public float MarginTop { get; private set; }

  public float MarginLeft { get; private set; }

  public float MarginRight { get; private set; }

  public float MarginBottom { get; private set; }

  public float HAlign { get; private set; }

  public float VAlign { get; private set; }

  public UiCalculatedStyle InnerDimensions { get; private set; }

  public UiCalculatedStyle Dimensions { get; private set; }

  public UiCalculatedStyle OuterDimensions { get; private set; }

  public IReadOnlyList<UiElementId> Children => _children.ToArray();

  internal bool IsAttached => Parent.HasValue;

  internal bool IsDirty { get; private set; } = true;

  internal void SetConstraints(
    UiStyleDimension top,
    UiStyleDimension left,
    UiStyleDimension width,
    UiStyleDimension height,
    UiStyleDimension maxWidth,
    UiStyleDimension maxHeight,
    UiStyleDimension minWidth,
    UiStyleDimension minHeight,
    float paddingTop,
    float paddingLeft,
    float paddingRight,
    float paddingBottom,
    float marginTop,
    float marginLeft,
    float marginRight,
    float marginBottom,
    float hAlign,
    float vAlign)
  {
    Top = top;
    Left = left;
    Width = width;
    Height = height;
    MaxWidth = maxWidth;
    MaxHeight = maxHeight;
    MinWidth = minWidth;
    MinHeight = minHeight;
    PaddingTop = ValidateNonNegative(paddingTop, nameof(paddingTop));
    PaddingLeft = ValidateNonNegative(paddingLeft, nameof(paddingLeft));
    PaddingRight = ValidateNonNegative(paddingRight, nameof(paddingRight));
    PaddingBottom = ValidateNonNegative(paddingBottom, nameof(paddingBottom));
    MarginTop = marginTop;
    MarginLeft = marginLeft;
    MarginRight = marginRight;
    MarginBottom = marginBottom;
    HAlign = ValidateAlignment(hAlign, nameof(hAlign));
    VAlign = ValidateAlignment(vAlign, nameof(vAlign));
    MarkDirty();
  }

  internal void SetParent(UiElementId? parent)
  {
    Parent = parent;
    MarkDirty();
  }

  internal void AddChild(UiElementId child)
  {
    if (!_children.Contains(child))
    {
      _children.Add(child);
    }

    MarkDirty();
  }

  internal void RemoveChild(UiElementId child)
  {
    _children.Remove(child);
    MarkDirty();
  }

  internal void SetCalculated(
    UiCalculatedStyle innerDimensions,
    UiCalculatedStyle dimensions,
    UiCalculatedStyle outerDimensions)
  {
    InnerDimensions = innerDimensions;
    Dimensions = dimensions;
    OuterDimensions = outerDimensions;
    IsDirty = false;
  }

  internal void MarkDirty()
  {
    IsDirty = true;
  }

  private static float ValidateAlignment(float value, string parameterName)
  {
    if (!float.IsFinite(value))
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }

    return value;
  }

  private static float ValidateNonNegative(float value, string parameterName)
  {
    if (!float.IsFinite(value) || value < 0f)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }

    return value;
  }
}

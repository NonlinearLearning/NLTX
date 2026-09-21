using Terraria.ClientPresentation.Ui.Layout;
using Terraria.ClientPresentation.Ui.Tree;

namespace Terraria.ClientPresentation.Ui.Text;

public sealed class UiTextPanelComponent
{
  public UiTextPanelComponent(UiElementId hostId)
  {
    if (!hostId.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(hostId));
    }

    HostId = hostId;
  }

  public UiElementId HostId { get; }

  public TextSource Source { get; private set; } = TextSource.Empty;

  public float TextScale { get; private set; } = 1f;

  public float EffectiveScale { get; private set; } = 1f;

  public UiVector2 MeasuredSize { get; private set; }

  public bool IsLarge { get; private set; }

  public uint TextColor { get; private set; } = 0xffffffff;

  public uint ShadowColor { get; private set; } = 0xff000000;

  public bool IsWrapped { get; private set; }

  public bool DynamicallyScaleDownToWidth { get; private set; }

  public float TextOriginX { get; private set; }

  public float TextOriginY { get; private set; }

  public float WrappedTextBottomPadding { get; private set; }

  public string ResolvedText { get; private set; } = string.Empty;

  public bool HasResolvedText { get; private set; }

  public int ContentRevision { get; private set; }

  public bool Apply(UiTextContentCommand command)
  {
    ArgumentNullException.ThrowIfNull(command);
    switch (command.Operation)
    {
      case UiTextContentCommand.OperationKind.SetSource:
        if (!command.Source.IsValid)
        {
          return false;
        }

        Source = command.Source;
        ContentRevision++;
        return true;
      case UiTextContentCommand.OperationKind.SetScale:
        if (!float.IsFinite(command.Value) || command.Value <= 0f)
        {
          return false;
        }

        TextScale = command.Value;
        ContentRevision++;
        return true;
      case UiTextContentCommand.OperationKind.SetStyle:
        if (!float.IsFinite(command.Value)
          || command.Value < 0f
          || !float.IsFinite(command.SecondaryValue)
          || command.SecondaryValue < 0f)
        {
          return false;
        }

        IsLarge = command.IsLarge;
        IsWrapped = command.IsWrapped;
        DynamicallyScaleDownToWidth = command.DynamicallyScaleDownToWidth;
        WrappedTextBottomPadding = command.Value;
        TextOriginX = command.SecondaryValue;
        TextOriginY = command.TertiaryValue;
        ContentRevision++;
        return true;
      case UiTextContentCommand.OperationKind.SetColors:
        TextColor = command.Color;
        ShadowColor = command.ShadowColor;
        ContentRevision++;
        return true;
      default:
        return false;
    }
  }

  internal void SetMeasurement(
    string resolvedText,
    bool hasResolvedText,
    int sourceRevision,
    UiVector2 measuredSize,
    float effectiveScale)
  {
    ResolvedText = resolvedText;
    HasResolvedText = hasResolvedText;
    ContentRevision = Math.Max(ContentRevision, sourceRevision);
    MeasuredSize = measuredSize;
    EffectiveScale = effectiveScale;
  }

  public readonly record struct TextSource(SourceKind Kind, string Value)
  {
    public static TextSource Empty => new(SourceKind.Empty, string.Empty);

    public bool IsValid => Kind != SourceKind.Empty && !string.IsNullOrWhiteSpace(Value);

    public static TextSource FromLiteral(string value)
    {
      return Create(SourceKind.Literal, value);
    }

    public static TextSource FromLocalizedKey(string value)
    {
      return Create(SourceKind.LocalizedKey, value);
    }

    public static TextSource FromFormattedSnapshot(string value)
    {
      return Create(SourceKind.FormattedSnapshot, value);
    }

    private static TextSource Create(SourceKind kind, string value)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(value);
      return new TextSource(kind, value);
    }
  }

  public enum SourceKind
  {
    Empty,
    Literal,
    LocalizedKey,
    FormattedSnapshot
  }
}

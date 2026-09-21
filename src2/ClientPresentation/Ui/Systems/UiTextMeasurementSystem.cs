using Terraria.ClientPresentation.Ui.Layout;
using Terraria.ClientPresentation.Ui.Text;

namespace Terraria.ClientPresentation.Ui.Systems;

public sealed class UiTextMeasurementSystem
{
  private readonly UiTextPanelComponent _text;
  private readonly UiTextLocalizationAdapter _adapter;

  public UiTextMeasurementSystem(
    UiTextPanelComponent text,
    UiTextLocalizationAdapter adapter)
  {
    _text = text ?? throw new ArgumentNullException(nameof(text));
    _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
  }

  public bool Recalculate(float maxWidth = 0f)
  {
    if (!float.IsFinite(maxWidth) || maxWidth < 0f)
    {
      return false;
    }

    UiTextLocalizationAdapter.Resolution resolution = _adapter.Resolve(_text.Source);
    if (!resolution.IsAvailable)
    {
      _text.SetMeasurement(
        string.Empty,
        false,
        resolution.Revision,
        UiVector2.Zero,
        _text.TextScale);
      return false;
    }

    UiTextLocalizationAdapter.Measurement measurement = _adapter.Measure(
      resolution.Text,
      _text.TextScale,
      _text.IsLarge,
      _text.IsWrapped,
      maxWidth);
    float effectiveScale = measurement.EffectiveScale;
    if (_text.DynamicallyScaleDownToWidth
      && maxWidth > 0f
      && measurement.Size.X > maxWidth
      && measurement.Size.X > 0f)
    {
      effectiveScale *= maxWidth / measurement.Size.X;
      measurement = _adapter.Measure(
        resolution.Text,
        effectiveScale,
        _text.IsLarge,
        _text.IsWrapped,
        maxWidth);
    }

    _text.SetMeasurement(
      resolution.Text,
      true,
      resolution.Revision,
      measurement.Size,
      effectiveScale);
    return true;
  }
}

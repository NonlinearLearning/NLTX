using Terraria.ClientPresentation.Ui.Layout;

namespace Terraria.ClientPresentation.Ui.Text;

public sealed class UiTextLocalizationAdapter
{
  private readonly IResolver _resolver;
  private readonly IMeasurer _measurer;

  public UiTextLocalizationAdapter(IResolver resolver, IMeasurer measurer)
  {
    _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
    _measurer = measurer ?? throw new ArgumentNullException(nameof(measurer));
  }

  public Resolution Resolve(UiTextPanelComponent.TextSource source)
  {
    return _resolver.Resolve(source);
  }

  public Measurement Measure(
    string text,
    float scale,
    bool isLarge,
    bool isWrapped,
    float maxWidth)
  {
    return _measurer.Measure(text, scale, isLarge, isWrapped, maxWidth);
  }

  public readonly record struct Resolution(
    bool IsAvailable,
    string Text,
    int Revision);

  public readonly record struct Measurement(
    UiVector2 Size,
    float EffectiveScale);

  public interface IResolver
  {
    Resolution Resolve(UiTextPanelComponent.TextSource source);
  }

  public interface IMeasurer
  {
    Measurement Measure(
      string text,
      float scale,
      bool isLarge,
      bool isWrapped,
      float maxWidth);
  }
}

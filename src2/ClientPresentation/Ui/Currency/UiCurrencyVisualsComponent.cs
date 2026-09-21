namespace Terraria.ClientPresentation.Ui.Currency;

public sealed class UiCurrencyVisualsComponent
{
  public UiCurrencyVisualsComponent(
    UiCurrencyRegistryAdapter.RegistrationKey registrationKey,
    string textKey,
    string iconResourceKey,
    float drawScale = 1f,
    uint color = 0xffffffff)
  {
    if (!registrationKey.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(registrationKey));
    }

    ArgumentException.ThrowIfNullOrWhiteSpace(textKey);
    ArgumentException.ThrowIfNullOrWhiteSpace(iconResourceKey);
    if (!float.IsFinite(drawScale) || drawScale <= 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(drawScale));
    }

    RegistrationKey = registrationKey;
    TextKey = textKey;
    IconResourceKey = iconResourceKey;
    DrawScale = drawScale;
    Color = color;
  }

  public UiCurrencyRegistryAdapter.RegistrationKey RegistrationKey { get; }

  public string TextKey { get; }

  public string IconResourceKey { get; }

  public float DrawScale { get; }

  public uint Color { get; }
}

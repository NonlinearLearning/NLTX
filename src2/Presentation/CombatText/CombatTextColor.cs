namespace Terraria.Presentation.CombatText;

public readonly record struct CombatTextColor(
  byte Red,
  byte Green,
  byte Blue,
  byte Alpha = 255)
{
  public CombatTextColor Scale(float factor)
  {
    return new CombatTextColor(
      ScaleChannel(Red, factor),
      ScaleChannel(Green, factor),
      ScaleChannel(Blue, factor),
      Alpha);
  }

  private static byte ScaleChannel(byte channel, float factor)
  {
    return (byte)Math.Clamp((int)MathF.Round(channel * factor), 0, 255);
  }
}

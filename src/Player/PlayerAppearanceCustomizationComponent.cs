namespace Terraria.Player;

public sealed class PlayerAppearanceCustomizationComponent
{
  public byte HairDye { get; private set; }

  public int SkinDyePacked { get; private set; }

  public PlayerAppearanceColor HairColor { get; private set; } =
    new(215, 90, 55);

  public PlayerAppearanceColor SkinColor { get; private set; } =
    new(255, 125, 90);

  public PlayerAppearanceColor EyeColor { get; private set; } =
    new(105, 90, 75);

  public PlayerAppearanceColor ShirtColor { get; private set; } =
    new(175, 165, 140);

  public PlayerAppearanceColor UnderShirtColor { get; private set; } =
    new(160, 180, 215);

  public PlayerAppearanceColor PantsColor { get; private set; } =
    new(255, 230, 175);

  public PlayerAppearanceColor ShoeColor { get; private set; } =
    new(160, 105, 60);

  public int Hair { get; private set; }

  internal void Apply(in PlayerAppearanceCustomizationInput input)
  {
    HairDye = input.HairDye;
    SkinDyePacked = input.SkinDyePacked;
    HairColor = input.HairColor;
    SkinColor = input.SkinColor;
    EyeColor = input.EyeColor;
    ShirtColor = input.ShirtColor;
    UnderShirtColor = input.UnderShirtColor;
    PantsColor = input.PantsColor;
    ShoeColor = input.ShoeColor;
    Hair = input.Hair;
  }

  public PlayerAppearanceCustomizationSnapshot ToSnapshot()
  {
    return new PlayerAppearanceCustomizationSnapshot(
      HairDye,
      SkinDyePacked,
      HairColor,
      SkinColor,
      EyeColor,
      ShirtColor,
      UnderShirtColor,
      PantsColor,
      ShoeColor,
      Hair);
  }
}

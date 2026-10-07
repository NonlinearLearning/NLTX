namespace Terraria.Player;

/// <summary>
/// 保存玩家发型、染色和身体服装颜色。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：hairDye（第 1947 行）； skinDyePacked（第 1950 行）； hairColor（第 1952 行）； skinColor（第 1954 行）；
/// eyeColor（第 1956 行）； shirtColor（第 1958 行）； underShirtColor（第 1960 行）； pantsColor（第 1962 行）；
/// shoeColor（第 1964 行）； hair（第 1966 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p11-player-presentation-derived-component-design.md。
/// </para>
/// <para>依据位置：第 90 行。</para>
/// </remarks>
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

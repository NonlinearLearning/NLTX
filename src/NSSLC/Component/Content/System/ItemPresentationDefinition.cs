namespace Terraria.Content;

public sealed record ItemPresentationDefinition(
  int Rare = 0,
  ColorRgba? Color = null,
  int Alpha = 0,
  short? GlowMaskId = null,
  float Scale = 1f,
  int? StringColor = null,
  string? BestiaryNotes = null,
  CreativeItemSortValue? CreativeSorting = null,
  int? DyeShaderId = null,
  AnimationDefinition? Animation = null);
